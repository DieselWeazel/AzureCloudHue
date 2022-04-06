using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureCloudHue.AsyncPollingFunction;

public static class ProcessLightRotationOrchestrator
{
    [FunctionName("ProcessLightRotationOrchestrator")]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger logger)
    {
        context.SetCustomStatus("Durable Orchestrator starting..");
        
        logger.LogInformation("Durable Function orchestrating Hue API calls in the background");
        HueLightRotation hueLightRotation = context.GetInput<HueLightRotation>();
        var outputs = new List<string>();

        var totalTimeToExecuteRotation = 0.0;
        hueLightRotation.LightStates.ForEach(lightState => totalTimeToExecuteRotation += lightState.TransitionTimeInMs);
        
        logger.LogInformation($"Orchestrating light rotation of length {hueLightRotation.LightStates.Count}");

        
        foreach (var lightState in hueLightRotation.LightStates)
        {
            totalTimeToExecuteRotation -= lightState.TransitionTimeInMs;
            var message = $"Light with id {hueLightRotation.LightId} setting to lightstate: {lightState}.";
            StatusMessageWithTimeLeft statusMessage =
                new StatusMessageWithTimeLeft(totalTimeToExecuteRotation, message);
            
            context.SetCustomStatus(statusMessage);
            HueLight hueLight = new HueLight();
            hueLight.LightId = hueLightRotation.LightId;
            hueLight.LightState = lightState;

            try
            {
                var output = await context.CallActivityAsync<string>("HueLamp_SetLightState", hueLight);
                outputs.Add(output);
            }
            catch (Exception e)
            {
                context.SetCustomStatus(e.Message);
                return outputs;
            }
        }
        
        context.SetCustomStatus("Finished");
        logger.LogInformation($"Light rotation with id {hueLightRotation.LightId} is finished");
        
        return outputs;
    }
}