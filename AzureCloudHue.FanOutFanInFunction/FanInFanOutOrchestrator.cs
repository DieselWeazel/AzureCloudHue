using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json.Linq;

namespace AzureCloudHue.Function;

public static class FanInFanOutOrchestrator
{
    [FunctionName("DurableHueFunctionOrchestrator")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var outputs = new List<string>();
        var bsObj = context.GetInput<JArray>();
        
        var hueLights = await context.CallActivityAsync<List<HueLight>>("GetHueLightStates", bsObj);

        var parallelTasks = new List<Task<string>>();
        for (int i = 0; i < hueLights.Count; i++)
        {
            Task<string> task = context.CallActivityAsync<string>("HueLamp_SetLightState", hueLights[i]);
            parallelTasks.Add(task);
        }
        
        var results = await Task.WhenAll(parallelTasks);

        string concatenatedResponses = String.Join(String.Empty, results);
        var addedToCosmosDB = await context.CallActivityAsync<string>("AddHueResultToCosmosDB", concatenatedResponses);

        outputs.Add(addedToCosmosDB);
        
        return outputs;
    }
}