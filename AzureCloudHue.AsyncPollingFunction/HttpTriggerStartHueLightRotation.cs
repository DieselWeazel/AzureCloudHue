using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.AsyncPollingFunction;

public static class HttpTriggerStartHueLightRotation
{
    [FunctionName("HttpTriggerStartHueLightRotation_HttpStart")]
    public static async Task<AcceptedResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "submit")]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        string requestBody = new StreamReader(req.Body).ReadToEnd();
        var hueLightRotation = JsonConvert.DeserializeObject<HueLightRotation>(requestBody);
        
        string instanceId = await starter.StartNewAsync("ProcessLightRotationOrchestrator", hueLightRotation);

        log.LogInformation($"Started Async Http API with Polling orchestration with ID = '{instanceId}'.");
        string checkStatusLocacion = string.Format("{0}://{1}/api/status/{2}", req.Scheme, req.Host, instanceId);
        string message = $"Rotation of Hue Light with id  {hueLightRotation.LightId} started, to see status go to {checkStatusLocacion}";

        var totalTimeToExecuteRotation = 0.0;
        hueLightRotation.LightStates.ForEach(lightState => totalTimeToExecuteRotation += lightState.TransitionTimeInMs);
        
        req.HttpContext.Response.Headers.Add("retry-after", totalTimeToExecuteRotation.ToString());
        return new AcceptedResult(checkStatusLocacion, message);
    }
}