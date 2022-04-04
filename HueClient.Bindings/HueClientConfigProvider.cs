using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace ClassLibrary1;

[Extension("BridgeAddress")]
internal class HueClientConfigProvider : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        var bindingRule = context.AddBindingRule<HueBridgeAttribute>();
        bindingRule.BindToCollector(attribute => new HueDispatcherAsyncCollector(attribute));
    }
}