using Newtonsoft.Json;

namespace AzureFunctionTests;

public class StatusQueryURI
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("statusQueryGetUri")]
    public string StatusQueryGetUri { get; set; }
}