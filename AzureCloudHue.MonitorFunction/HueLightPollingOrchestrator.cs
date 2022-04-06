using System;
using System.Threading;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureCloudHue.MonitorFunction;

public static class HueLightPollingOrchestrator
{
    [FunctionName("HueLightPollingOrchestrator")]
    public static async Task<ResultMonitoringDTO> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        string hexColor = context.GetInput<string>();
        log.LogInformation($"Hex color to set is {hexColor}");
        DateTime expireInTime = context.CurrentUtcDateTime.AddSeconds(60);
        
        if (!context.IsReplaying)
        {
            log.LogInformation($"Monitor started, expiring at: {expireInTime}");
        }

        ResultMonitoringDTO result = new ResultMonitoringDTO();
        
        while (context.CurrentUtcDateTime < expireInTime)
        {
            log.LogInformation($"Expiration time is still: {expireInTime}");
            
            var hueLight = await context.CallActivityAsync<HueLight>("GetHueLightState", hexColor);

            if (hueLight.LightState.On)
            {
                hueLight.LightState.HexColor = hexColor;
                log.LogCritical("Light state was on, monitor proceeding.");
                result.OkObjectHueResult = await context.CallActivityAsync<string>("SetLightToColor", hueLight);
                result.DateTimeWhenMonitorExecuted = $"{context.CurrentUtcDateTime.ToLongDateString()} {context.CurrentUtcDateTime.ToLongTimeString()}";
                break;
            }
            
            var idleTime = context.CurrentUtcDateTime.AddSeconds(5);
            await context.CreateTimer(idleTime, CancellationToken.None);
        }
        
        log.LogInformation($"Monitor reached expiration time. Current time is {context.CurrentUtcDateTime}");
        return result;
    }
}