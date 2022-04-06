using Microsoft.Azure.WebJobs.Description;

namespace HueClient.Bindings.HueAPIInputBinding;

[Binding]
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class CurrentLightStateBindingAttribute : Attribute
{
    [AutoResolve]
    public string Address { get; set; }
    
    [AutoResolve]
    public string LightId { get; set; }
}