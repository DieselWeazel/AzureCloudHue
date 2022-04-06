using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureCloudHue.FanOutFanInFunction;

public static class HttpTriggerChangeHueLights
{
    [FunctionName("HttpTriggerChangeHueLights_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        HttpContent requestContent = req.Content;
        string jsonContent = requestContent.ReadAsStringAsync().Result;
        JArray hueLightsJArray = JsonConvert.DeserializeObject<JArray>(jsonContent);

        string instanceId = await starter.StartNewAsync("DurableHueFunctionOrchestrator", hueLightsJArray);

        log.LogInformation($"Started fan in/out orchestration with ID = '{instanceId}'.");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}