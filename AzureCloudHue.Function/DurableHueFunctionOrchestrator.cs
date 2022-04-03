using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Gamut;

namespace AzureCloudHue.Function;

public static class DurableHueFunctionOrchestrator
{
    [FunctionName("DurableHueFunctionOrchestrator")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var outputs = new List<string>();
        var bsObj = context.GetInput<JArray>();

        // var hueLight = new StreamReader(req.Content).ReadToEndAsync();
        var hueLights = JsonConvert.DeserializeObject<List<HueLight>>(bsObj.ToString());
        // hueLight.LightId = 19;
        // LightState lightState = new LightState();
        // lightState.On = true;
        // lightState.Brightness = 200;
        // lightState.HexColor = "0033ff";
        // lightState.TransitionTimeInMs = 1000.00;
        // hueLight.LightState = lightState;
        
        /*
         *
    var parallelTasks = new List<Task<int>>();

    // Get a list of N work items to process in parallel.
    
    // Den här hämtar ifrån något helt annat, men vrf inte lagra allt i en annan function? Är det så de är tänkt?
    https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp#application-patterns
    
    object[] workBatch = await context.CallActivityAsync<object[]>("F1", null);
    for (int i = 0; i < workBatch.Length; i++)
    {
        Task<int> task = context.CallActivityAsync<int>("F2", workBatch[i]);
        parallelTasks.Add(task);
    }
         */
        var parallelTasks = new List<Task<string>>();

        for (int i = 0; i < hueLights.Count; i++)
        {
            Task<string> task = context.CallActivityAsync<string>("HueLamp_SetLightState", hueLights[i]);
            parallelTasks.Add(task);
        }
        
        var results = await Task.WhenAll(parallelTasks);
        // SetParallellTasks(parallelTasks);
        // var deserializedObject =
        //     await context.CallActivityAsync<string>("HueLamp_SetLightState", hueLight);
        // outputs.Add(deserializedObject);
        
        // Console.WriteLine(deseriali

        string concatenatedResponses = String.Join(String.Empty, results);
        var addedToCosmosDB = await context.CallActivityAsync<string>("AddHueResultToCosmosDB", concatenatedResponses);

        outputs.Add(addedToCosmosDB);
        
        // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
        return outputs;
    }

    // [FunctionName("CollectHueResultsOutput")]
    // private static async Task<string> SetParallellTasks([ActivityTrigger]List<Task<int>> parallelTasks)
    // {
    //     throw new NotImplementedException();
    // }

    //
    // [FunctionName("HueLamp_SetLightState")]
    // public static async Task<string> SetLightState([ActivityTrigger] HueLight hueLight, ILogger log)
    // {
    //     log.LogInformation($"Setting light with id {hueLight.LightId}");
    //     log.LogInformation($"State is {hueLight.LightState}");
    //     
    //     LightCommand command = new LightCommand();
    //     ILocalHueClient _client = new LocalHueClient("192.168.1.5");
    //     _client.Initialize("3ioWgarB3Z6YFdK3aBsewSxPsSSI0DXtxu7loYto");
    //
    //     // TODO en Command Mapper! (Command GetCommandFromLightState(LightState lightState);"
    //     LightState lightState = hueLight.LightState;
    //
    //     command.On = lightState.On;
    //     CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(lightState.HexColor), null);
    //     command.SetColor(point.x, point.y);
    //         
    //     // TODO Behöver Transition Time verifieras? Måhända inte.. Men prova med random shit!
    //     command.TransitionTime = TimeSpan.FromMilliseconds(lightState.TransitionTimeInMs);
    //         
    //     // TODO något som verifierar att brightness är 0-255
    //     command.Brightness = Convert.ToByte(lightState.Brightness);
    //         
    //     var sentCommand = await _client.SendCommandAsync(command, new List<string>() {hueLight.LightId.ToString()});
    //
    //     return JsonConvert.SerializeObject(new OkObjectResult(sentCommand));
    // }

    [FunctionName("DurableHueFunctionOrchestrator_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        // TODO if GET request, return, don't start orchestrator?
        // Or do random shit?

        // Function input comes from the request content.
        HttpContent requestContent = req.Content;
        string jsonContent = requestContent.ReadAsStringAsync().Result;
        JArray bsObj = JsonConvert.DeserializeObject<JArray>(jsonContent);
        
        string instanceId = await starter.StartNewAsync("DurableHueFunctionOrchestrator", bsObj);

        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}