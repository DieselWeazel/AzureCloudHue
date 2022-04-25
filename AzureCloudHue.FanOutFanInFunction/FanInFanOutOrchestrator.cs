using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using AzureCloudHue.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AzureCloudHue.FanOutFanInFunction;

public static class FanInFanOutOrchestrator
{
    [FunctionName("DurableHueFunctionOrchestrator")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        var outputs = new List<string>();
        var jArrayOfLightsToChange = context.GetInput<JArray>();
        List<HueLight> hueLights;
        try
        {
            hueLights = await context.CallActivityAsync<List<HueLight>>("DeserializeHueLights", jArrayOfLightsToChange);

        }
        catch (FunctionFailedException e)
        {
            log.LogError($"Error when parsing: {e.Message}");
            outputs.Add(e.Message);
            throw;
        }

        TokenDbEntityWithNewToken tokenDbEntityWithNewToken = null;
        try
        {
            tokenDbEntityWithNewToken = await context.CallActivityAsync<TokenDbEntityWithNewToken>("TokenRetriever", context.InstanceId);
        }
        catch (FunctionFailedException e)
        {
            // log.LogError($"Error when retrieving token: {e.Message}");
            outputs.Add(e.Message);
            throw;
        }
        
        string[] results;
        try
        {
            var parallelTasks = new List<Task<string>>();
            for (int i = 0; i < hueLights.Count; i++)
            {
                var tokenWithHueLight = new TokenWithHueLight(hueLights[i], tokenDbEntityWithNewToken.Token);
                Task<string> task = context.CallActivityAsync<string>("HueLamp_SetLightState", tokenWithHueLight);
                parallelTasks.Add(task);
            }

            results = await Task.WhenAll(parallelTasks);
        }
        catch (FunctionFailedException fe)
        {
            outputs.Add(fe.Message);
            throw;
        }
        
        string concatenatedResponses = String.Join(String.Empty, results);

        string insertNewRefreshToken = null;
        string addedToCosmosDB = null;
        try
        {
            insertNewRefreshToken =
                await context.CallActivityAsync<string>("InsertRefreshToken", tokenDbEntityWithNewToken);
            addedToCosmosDB = await context.CallActivityAsync<string>("AddHueResultToCosmosDB", concatenatedResponses);
        }
        catch (FunctionFailedException fe)
        {
            log.LogError($"Error occured {fe.Message}");
            outputs.Add(fe.Message);
            throw;
        }

        outputs.Add(addedToCosmosDB);
        outputs.Add(insertNewRefreshToken);
        
        return outputs;
    }
}