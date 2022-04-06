using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureCloudHue.AsyncPollingFunction;

public class HttpTriggerCheckStatus
{
            [FunctionName("GetStatus")]
        public static async Task<IActionResult> Run(
                                                    [HttpTrigger(AuthorizationLevel.Anonymous, methods: "get", Route = "status/{instanceId}")] HttpRequest req,
                                                    [DurableClient] IDurableOrchestrationClient orchestrationClient,
                                                    string instanceId,
                                                    ILogger logger)
        {

            var status = await orchestrationClient.GetStatusAsync(instanceId);
            
            if (status != null)
            {
                var customStatus = status.CustomStatus;

                if (status.RuntimeStatus is OrchestrationRuntimeStatus.Running)
                {
                    StatusMessageWithTimeLeft statusMessageWithTimeLeft = customStatus.ToObject<StatusMessageWithTimeLeft>();
                    string checkStatusLocacion = string.Format("{0}://{1}/api/status/{2}", req.Scheme, req.Host, instanceId);
                    string message = $"Rotation is currently still running. {statusMessageWithTimeLeft.Message}\r\n" +
                                     $"Remaining time is at least: {statusMessageWithTimeLeft.RemainingTime}\r\n" +
                                     $"To check the status later, go to: GET {checkStatusLocacion}"; // To inform the client where to check the status

                    ActionResult response = new AcceptedResult(checkStatusLocacion, message);
                    req.HttpContext.Response.Headers.Add("retry-after", statusMessageWithTimeLeft.RemainingTime.ToString()); 
                    return response;
                }

                if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    string finishedMessage = customStatus.ToObject<string>();
                    if (finishedMessage == "Finished")
                        return new OkObjectResult($"Hue light rotation with instanceid '{instanceId}' is finished!");
                    
                    // TODO, Error recieved, perhaps light not found?
                    // Inkluderat error message vore bra.
                    return new ConflictObjectResult($"Sometime went wrong with '{instanceId}'. See error message: \r\n{finishedMessage}");
                }
            }
            
            // TODO, är man lite för snabb hinner man komma hit fastän orkestreringen går igenom. 
            // Finns det kanske något bättre sätt att kontrollera det här på?
            return new NotFoundObjectResult($"No running hue light rotation with instanceid '{instanceId}' found.");
        }
}