using System;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using HueClient.Bindings.HueAPIOutputBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.MonitorFunction;

public class SetLightToColor
{
    [FunctionName
        ("SetLightToColor")]
    public async Task<string> SetLightToColorFunction([ActivityTrigger]
        HueLight hueLight,
        // TODO uses the same Binding as the other function,
        // But it should perhaps have a more simpler binding?
        // Or is it possible to do it some other way?
        // Reason is, this one only changes color
        // The other one changes color + brightness + on status.
        
        // That way it could have an output binding with %COLOR% for instance!
        [AzureHueAPI(Address = "%HueAPIAddress%")]IAsyncCollector<HueLight> bridge,
        ILogger log)
    {
        string lightId = Environment.GetEnvironmentVariable("LampId");
        log.LogInformation($"Setting light with id {lightId}");

        await bridge.AddAsync(hueLight);

        log.LogInformation($"Light is now set!");

        return JsonConvert.SerializeObject(new OkObjectResult(hueLight));
    }
}