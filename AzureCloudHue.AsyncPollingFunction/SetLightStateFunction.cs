using System;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using HueClient.Bindings;
using HueClient.Bindings.HueAPIOutputBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.AsyncPollingFunction;

/*
 * TODO:
 * This file exists in AzureCloudHue.FanOutFanInFunction
 *
 * It's rather inconvenient copying it to every repo that uses it.
 *
 * It is possible to build it from project A to project B. If it's done via pipelines or some other way. Not sure.
 *
 * It's not all too relevant right now, but leaving this here. (Ideally best would be to have a base function library)
 *
 * https://stackoverflow.com/questions/41985925/how-to-add-a-reference-to-an-azure-function-c-sharp-project
 */
public class SetLightStateFunction
{
    [FunctionName("HueLamp_SetLightState")]
    public async Task<string> SetLightState([ActivityTrigger] HueLight hueLight, 
        [AzureHueAPI(Address = "%HueAPIAddress%")]IAsyncCollector<HueLight> bridge,
        ILogger log)
    {
        log.LogInformation($"Setting light with id {hueLight.LightId}");
        log.LogInformation($"State is {hueLight.LightState}");

        string instanceId = "";
        try
        {
            await bridge.AddAsync(hueLight);
        }
        catch (Exception e)
        {
            string errorMessage = e.Message;
            log.LogError(errorMessage);
            throw (e);
            // return JsonConvert.SerializeObject(new NotFoundResult());
        }
        
        
        return JsonConvert.SerializeObject(new OkObjectResult(hueLight));
    }
}