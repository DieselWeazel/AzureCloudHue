using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace HueClient.Bindings;

[Extension("BridgeAddress")]
internal class AzureHueAPIConfigProvider : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        var bindingRule = context.AddBindingRule<AzureHueAPIAttribute>();
        bindingRule.BindToCollector(attribute => new HueAPIDispatcherAsyncCollector(attribute));
    }
}