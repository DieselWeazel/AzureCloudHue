using Microsoft.Azure.WebJobs.Description;

namespace HueClient.Bindings.HueAPIOutputBinding;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Binding]
public class AzureHueAPIAttribute : Attribute
{
    [AutoResolve]
    public string Address { get; set; }
}