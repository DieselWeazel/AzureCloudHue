using Microsoft.Azure.WebJobs.Description;

namespace HueClient.Bindings;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Binding]
public class HueAPIAttribute : Attribute
{
    [AutoResolve]
    public string Address { get; set; }
}