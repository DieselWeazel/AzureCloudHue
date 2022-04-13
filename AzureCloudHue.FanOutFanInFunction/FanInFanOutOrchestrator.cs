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

        string accessToken = "";
        try
        {
            // TODO overloaded method igen..
            accessToken = await context.CallActivityAsync<string>("TokenRetriever", "null");
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
                var tokenWithHueLight = new TokenWithHueLight(hueLights[i], accessToken);
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
        var addedToCosmosDB = await context.CallActivityAsync<string>("AddHueResultToCosmosDB", concatenatedResponses);

        // var refreshToken = tokenKeyValuePair.Value;
        // refreshToken.access_token_expires_in = dateTimeExpiration.ToString();
        // refreshToken.refresh_token_expires_in = dateTimeRefreshTokenExpiresIn.ToString();
        // var tokenWrittenToCosmosDB = await context.CallActivityAsync<string>("WriteTokenToCosmosDB", refreshToken);
        
        // outputs.Add(tokenWrittenToCosmosDB) ?
        outputs.Add(addedToCosmosDB);
        
        return outputs;
    }
}