using Microsoft.Azure.WebJobs.Description;

namespace HueClient.Bindings.HueOAuth2Binding;

[Binding]
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class HueRemoteOAuth2Attribute : Attribute
{
    [AutoResolve] public string ClientId { get; set; }
    [AutoResolve] public string ClientSecret { get; set; }
}