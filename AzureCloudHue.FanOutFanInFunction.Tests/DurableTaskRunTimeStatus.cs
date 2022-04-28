using Newtonsoft.Json;

namespace AzureFunctionTests;

public class DurableTaskRunTimeStatus
{
    [JsonProperty("instanceId")]
    public string InstanceId { get; set; }
    
    [JsonProperty("runtimeStatus")]
    public string RuntimeStatus { get; set; }
}