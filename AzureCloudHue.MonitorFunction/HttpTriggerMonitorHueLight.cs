using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureCloudHue.MonitorFunction;

public class HttpTriggerMonitorHueLight
{
    [FunctionName("MonitorHueLight_HttpStart")]
    public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        ILogger log)
    {
        HttpContent requestContent = req.Content;
        string hexColor = requestContent.ReadAsStringAsync().Result;

        string instanceId = await starter.StartNewAsync("HueLightPollingOrchestrator", input: hexColor);
        
        log.LogInformation($"Started orchestration with ID = '{instanceId}");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}