using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using AzureCloudHue.Util;
using HueClient.Bindings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Q42.HueApi.Interfaces;

namespace AzureCloudHue.Function;

public class SetLightStateFunction
{
    // private readonly ILocalHueClient _client;
    //
    // public SetLightStateFunction(ILocalHueClient localHueClient)
    // {
    //     _client = localHueClient;
    // }
    //
    // [FunctionName("HueLamp_SetLightState")]
    // public async Task<string> SetLightState([ActivityTrigger] HueLight hueLight, ILogger log)
    // {
    //     log.LogInformation($"Setting light with id {hueLight.LightId}");
    //     log.LogInformation($"State is {hueLight.LightState}");
    //
    //     var light = await _client.GetLightAsync(hueLight.LightId.ToString());
    //     
    //     if (light == null)
    //     {
    //         return JsonConvert.SerializeObject(
    //             new BadRequestObjectResult($"No light with id {hueLight.LightId} found in bridge"));
    //     }
    //
    //     var command = new LightCommandMapper(light).MapLightStateToCommand(hueLight.LightState);
    //
    //     if (command == null)
    //     {
    //         return JsonConvert.SerializeObject(
    //             new OkObjectResult($"Light with id {hueLight.LightId} already has the requested state"));
    //     }
    //     
    //     var sentCommand = await _client.SendCommandAsync(command, new List<string>() {hueLight.LightId.ToString()});
    //
    //     return JsonConvert.SerializeObject(new OkObjectResult(sentCommand));
    // }
    
    [FunctionName("HueLamp_SetLightState")]
    public async Task<string> SetLightState([ActivityTrigger] HueLight hueLight, 
        // TODO HueBridgeAddress borde vara LocalAddress bara.
        [HueBridge(Address = "%HueBridgeAddress%",
            AppKey="%HueAppKey%",
            AccessToken = "%AccessToken%",
            AccessTokenExpiresIn = "%AccessTokenExpiresIn%",
            RefreshToken = "%RefreshToken%")]IAsyncCollector<HueLight> bridge,
        ILogger log)
    {
        log.LogInformation($"Setting light with id {hueLight.LightId}");
        log.LogInformation($"State is {hueLight.LightState}");

        await bridge.AddAsync(hueLight);
        
        // var light = await _client.GetLightAsync(hueLight.LightId.ToString());
        //
        // if (light == null)
        // {
        //     return JsonConvert.SerializeObject(
        //         new BadRequestObjectResult($"No light with id {hueLight.LightId} found in bridge"));
        // }
        //
        // var command = new LightCommandMapper(light).MapLightStateToCommand(hueLight.LightState);
        //
        // if (command == null)
        // {
        //     return JsonConvert.SerializeObject(
        //         new OkObjectResult($"Light with id {hueLight.LightId} already has the requested state"));
        // }
        //
        // var sentCommand = await _client.SendCommandAsync(command, new List<string>() {hueLight.LightId.ToString()});

        return JsonConvert.SerializeObject(new OkObjectResult(hueLight));
    }
}