using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace HueClient.Bindings;

[Extension("BridgeAddress")]
internal class HueClientConfigProvider : IExtensionConfigProvider
{
    private AzureCloudHue.Service.HueClient _hueClient;
    
    public HueClientConfigProvider(AzureCloudHue.Service.HueClient hueClient)
    {
        _hueClient = hueClient;
    }
    
    public void Initialize(ExtensionConfigContext context)
    {
        var bindingRule = context.AddBindingRule<HueBridgeAttribute>();
        bindingRule.BindToCollector(attribute => new HueDispatcherAsyncCollector(attribute, _hueClient));
    }
}