using System;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureCloudHue.MonitorFunction;

public static class HueLightPollingOrchestrator
{
    [FunctionName("HueLightPollingOrchestrator")]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        DateTime expireInTime = DateTime.UtcNow.AddMinutes(5);

        while (context.CurrentUtcDateTime < expireInTime)
        {
            var desiredHueLightStatus = await context.CallActivityAsync<MonitoredHueRequest>("GetHueLightStatus", null);

            if (desiredHueLightStatus.On)
            {
                await context.CallActivityAsync("SetToColor", null);
                break;
            }
        }

        log.LogInformation($"Monitor expiring.");
        
    }
    //
    // [Deterministic]
    // private static void VerifyRequest(MonitorRequest request)
    // {
    //     if (request == null)
    //     {
    //         throw new ArgumentNullException(nameof(request), "An input object is required.");
    //     }
    //
    //     if (request.Location == null)
    //     {
    //         throw new ArgumentNullException(nameof(request.Location), "A location input is required.");
    //     }
    //
    //     if (string.IsNullOrEmpty(request.Phone))
    //     {
    //         throw new ArgumentNullException(nameof(request.Phone), "A phone number input is required.");
    //     }
    // }

    
    // TODO, behövs verkligen en HttpTrigger? Kan jag inte bara köra min DurableFunction som ett litet troll i bakgrunden?
    [FunctionName("MonitorHueLight_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        string instanceId = await starter.StartNewAsync("HueLightPollingOrchestrator", null);
        
        log.LogInformation($"Started orchestration with ID = '{instanceId}");

        // TODO vad gör den här egentligen? Använder jag den även i mina andra functions?
        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}