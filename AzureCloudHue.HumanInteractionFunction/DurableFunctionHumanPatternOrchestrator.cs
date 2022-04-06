using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureCloudHue.HumanInteractionFunction;

public static class DurableFunctionHumanPatternOrchestrator
{
    [FunctionName("DurableHueFunctionHumanPatternOrchestrator")]
    public static async Task RunHumanPatternOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        log.LogInformation("Durable Function orchestrator for human pattern has started..");
        
        // var outputs
        var jObjectNumberAndHueGroupRotation = context.GetInput<JObject>();
        var numberAndHueGroupRotation =
            JsonConvert.DeserializeObject<NumberAndHueGroupRotation>(jObjectNumberAndHueGroupRotation.ToString());
        
        if (string.IsNullOrEmpty(numberAndHueGroupRotation.Number))
        {
            throw new ArgumentNullException(
                nameof(numberAndHueGroupRotation.Number),
                "A phone number input is required.");
        }

        int challengeCode = await context.CallActivityAsync<int>(
            "E4_SendSmsChallenge",
            numberAndHueGroupRotation.Number);
        
        log.LogInformation("DONE! Sent sms");
    }
}