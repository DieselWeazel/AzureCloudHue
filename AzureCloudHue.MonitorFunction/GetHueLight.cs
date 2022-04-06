using AzureCloudHue.Model;
using HueClient.Bindings.HueAPIInputBinding;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureCloudHue.MonitorFunction;

public static class GetHueLight
{
    [FunctionName("GetHueLightState")]
    public static HueLight GetHueLightFunction([ActivityTrigger]
        string someDummyString,
        [CurrentLightStateBinding
            (Address = "%HueAPIAddress%", LightId = "%LightId%")] HueLight currentHueLight,
        ILogger log)
    {
        log.LogInformation($"Fetched Lamp with id {currentHueLight.LightId}");
        log.LogInformation($"State of lamp is: {currentHueLight.LightState}");
        return currentHueLight;
    }
}