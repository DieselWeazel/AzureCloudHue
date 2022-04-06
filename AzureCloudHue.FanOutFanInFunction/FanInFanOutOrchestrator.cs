using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using AzureCloudHue.Util;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Function;

public static class FanInFanOutOrchestrator
{
    [FunctionName("DurableHueFunctionOrchestrator")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        var outputs = new List<string>();
        var bsObj = context.GetInput<JArray>();
        
        var hueLights = await context.CallActivityAsync<List<HueLight>>("GetHueLightStates", bsObj);

        RemoteHueClient remoteClient = null;
        remoteClient = await context.CallActivityAsync<RemoteHueClient>("GetRemoteHueClient", remoteClient);

        // string appId = "testingapp";
        // string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
        // string clientSecret = "L34YTv3nU8KZMNh1";
        //
        // IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
        //     
        // AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade("ZyI1HXEYD0P7XUxskYUiCn1snmWo",
        //     604799, "chjNXITp7sRTQvSm8QOaKLi2qAhZQICk", "Bearer_token");
        // authClient.Initialize(storedAccessToken);
        // var remoteClient = new RemoteHueClient(authClient.GetValidToken);
        //     
        // List<RemoteBridge> bridges = await remoteClient.GetBridgesAsync();
        //     
        // await remoteClient.RegisterAsync(bridges[0].Id, "Sample App");
        //
        //
        // log.LogInformation($"BRIDGE: {bridges[0].Id}");
        //
        
        // var parallelTasks = new List<Task<string>>();
        var parallelTasks = new List<Task<HueResults>>();

        for (int i = 0; i < hueLights.Count; i++)
        {
            // await remoteClient.RegisterAsync("001788fffe756f2e", "Sample App");
            // Task<string> task = context.CallActivityAsync<string>("HueLamp_SetLightState", hueLights[i]);
            var light = await remoteClient.GetLightAsync(hueLights[i].LightId.ToString());
            
            var command = new LightCommandMapper(light).MapLightStateToCommand(hueLights[i].LightState);

            Task<HueResults> task = remoteClient.SendCommandAsync(command,
                new List<string>() {hueLights[i].LightId.ToString()}); 
            
            parallelTasks.Add(task);
        }
        
        var results = await Task.WhenAll(parallelTasks);

        string concatenatedResponses = "";
        foreach (var result in results)
        {
            string resultString = JsonConvert.SerializeObject(result);
            concatenatedResponses += resultString;
        }
        
        // string concatenatedResponses = String.Join(String.Empty, results);
        var addedToCosmosDB = await context.CallActivityAsync<string>("AddHueResultToCosmosDB", concatenatedResponses);

        outputs.Add(addedToCosmosDB);
        
        return outputs;
    }
    
    // [FunctionName("HueLamp_SetLightState")]
    // public async Task<string> SetLightState([ActivityTrigger] HueLight hueLight, 
    //     [AzureHueAPI(Address = "%HueAPIAddress%")]IAsyncCollector<HueLight> bridge,
    //     ILogger log)
    // {
    //     log.LogInformation($"Setting light with id {hueLight.LightId}");
    //     log.LogInformation($"State is {hueLight.LightState}");
    //
    //     await bridge.AddAsync(hueLight);
    //     
    //     return JsonConvert.SerializeObject(new OkObjectResult(hueLight));
    // }
}