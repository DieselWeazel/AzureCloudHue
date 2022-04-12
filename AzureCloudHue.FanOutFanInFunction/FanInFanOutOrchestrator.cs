using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.AspNetCore.Mvc;
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
        
        var hueLights = await context.CallActivityAsync<List<HueLight>>("DeserializeHueLights", jArrayOfLightsToChange);

        // TODO overloaded method igen..
        var oAuth2Token = await context.CallActivityAsync<OAuth2Token>("TokenRetriever", "null");
        
        // var dateTimeExpiration = context.CurrentUtcDateTime.AddSeconds(token.AccessTokenExpiresIn);
        // var dateTimeRefreshTokenExpiresIn = context.CurrentUtcDateTime.AddSeconds(token.RefreshTokenExpiresIn);
        
        // catch (FunctionFailedException) (kika på kanske)
        
        // log.LogInformation($"Date time when expire: {dateTimeExpiration}");
        var parallelTasks = new List<Task<string>>();
        for (int i = 0; i < hueLights.Count; i++)
        {
            var tokenWithHueLight = new TokenWithHueLight(hueLights[i], oAuth2Token);
            Task<string> task = context.CallActivityAsync<string>("HueLamp_SetLightState", tokenWithHueLight);
            parallelTasks.Add(task);
        }
        
        var results = await Task.WhenAll(parallelTasks);

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