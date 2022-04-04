using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace HueClient.Bindings;

[Extension("BridgeAddress")]
internal class HueClientConfigProvider : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        var bindingRule = context.AddBindingRule<HueAPIAttribute>();
        bindingRule.BindToCollector(attribute => new HueDispatcherAsyncCollector(attribute));
    }
}