using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureCloudHue.Function;

public static class DurableHueFunctionOrchestrator
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

    [FunctionName("DurableHueFunctionOrchestrator_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        HttpContent requestContent = req.Content;
        string jsonContent = requestContent.ReadAsStringAsync().Result;
        JArray bsObj = JsonConvert.DeserializeObject<JArray>(jsonContent);

        string instanceId = await starter.StartNewAsync("DurableHueFunctionOrchestrator", bsObj);

        log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}