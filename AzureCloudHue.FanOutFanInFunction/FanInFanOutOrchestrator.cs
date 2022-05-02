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
        var hueLightCommands = await MapHueLightCommands(context, log, jArrayOfLightsToChange, outputs);

        var tokenDbEntityWithNewToken = await FetchUpdatedRefreshToken(context, outputs);

        var results = await SendCommandsToHueBridge(context, hueLightCommands, tokenDbEntityWithNewToken);

        string concatenatedResponses = String.Join(String.Empty, results);

        var insertNewRefreshToken = await RefreshAndInsertRefreshToken(context, log, tokenDbEntityWithNewToken, outputs);

        string addedToCosmosDB = null;

        addedToCosmosDB = await AddHueCommandResultsToCosmosDB(context, log, concatenatedResponses, outputs);

        outputs.Add(addedToCosmosDB);
        outputs.Add(insertNewRefreshToken);
        
        return outputs;
    }

    private static async Task<TokenDbEntityWithNewToken> FetchUpdatedRefreshToken(IDurableOrchestrationContext context, List<string> outputs)
    {
        TokenDbEntityWithNewToken tokenDbEntityWithNewToken = null;
        try
        {
            tokenDbEntityWithNewToken =
                await context.CallActivityAsync<TokenDbEntityWithNewToken>("TokenRetriever", context.InstanceId);
        }
        catch (FunctionFailedException e)
        {
            // log.LogError($"Error when retrieving token: {e.Message}");
            outputs.Add(e.Message);
            throw;
        }

        return tokenDbEntityWithNewToken;
    }

    private static async Task<List<HueLight>> MapHueLightCommands(IDurableOrchestrationContext context, ILogger log,
        JArray jArrayOfLightsToChange, List<string> outputs)
    {
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

        return hueLights;
    }

    private static async Task<string> AddHueCommandResultsToCosmosDB(IDurableOrchestrationContext context, ILogger log,
        string concatenatedResponses, List<string> outputs)
    {
        string addedToCosmosDB;
        try
        {
            addedToCosmosDB = await context.CallActivityAsync<string>("AddHueResultToCosmosDB", concatenatedResponses);
        }
        catch (FunctionFailedException fe)
        {
            log.LogError($"Error occured {fe.Message}");
            outputs.Add(fe.Message);
            throw;
        }

        return addedToCosmosDB;
    }

    private static async Task<string> RefreshAndInsertRefreshToken(IDurableOrchestrationContext context, ILogger log,
        TokenDbEntityWithNewToken tokenDbEntityWithNewToken, List<string> outputs)
    {
        string insertNewRefreshToken = null;
        try
        {
            insertNewRefreshToken =
                await context.CallActivityAsync<string>("InsertRefreshToken", tokenDbEntityWithNewToken);
        }
        catch (FunctionFailedException fe)
        {
            log.LogError($"Error occured {fe.Message}");
            outputs.Add(fe.Message);
            throw;
        }

        return insertNewRefreshToken;
    }

    private static async Task<string[]> SendCommandsToHueBridge(IDurableOrchestrationContext context, List<HueLight> hueLights,
        TokenDbEntityWithNewToken tokenDbEntityWithNewToken)
    {
        string[] results = { };
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
            results = new[] { fe.Message };
        }

        return results;
    }
}