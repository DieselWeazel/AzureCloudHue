using System.Threading.Tasks;
using AzureCloudHue.Model;
using HueClient.Bindings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.Function;

public class SetLightStateFunction
{
    [FunctionName("HueLamp_SetLightState")]
    public async Task<string> SetLightState([ActivityTrigger] HueLight hueLight, 
        [AzureHueAPI(Address = "%HueAPIAddress%")]IAsyncCollector<HueLight> bridge,
        ILogger log)
    {
        log.LogInformation($"Setting light with id {hueLight.LightId}");
        log.LogInformation($"State is {hueLight.LightState}");

        await bridge.AddAsync(hueLight);
        
        return JsonConvert.SerializeObject(new OkObjectResult(hueLight));
    }
}