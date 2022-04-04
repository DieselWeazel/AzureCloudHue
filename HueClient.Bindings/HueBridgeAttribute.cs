using Microsoft.Azure.WebJobs.Description;

namespace ClassLibrary1;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Binding]
public class HueBridgeAttribute : Attribute
{
    [AutoResolve]
    public string Address { get; set; }
    
    [AutoResolve]
    public string AppKey { get; set; }
    
    [AutoResolve]
    public string AccessToken { get; set; }
    
    [AutoResolve]
    public string AccessTokenExpiresIn { get; set; }
    
    [AutoResolve]
    public string RefreshToken { get; set; }
}