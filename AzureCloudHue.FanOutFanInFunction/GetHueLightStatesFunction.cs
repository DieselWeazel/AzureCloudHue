using System.Collections.Generic;
using AzureCloudHue.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureCloudHue.Function;

public class GetHueLightStatesFunction
{

    [FunctionName("GetHueLightStates")]
    public static List<HueLight> GetHueLightStates([ActivityTrigger] JArray bsObj,
        ILogger log)
    {
        var hueLights = JsonConvert.DeserializeObject<List<HueLight>>(bsObj.ToString());
        log.LogInformation($"Deserializing HueLights of length {hueLights.Count}");
        return hueLights;
    }
}