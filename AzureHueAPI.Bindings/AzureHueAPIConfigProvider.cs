using HueClient.Bindings.HueAPIInputBinding;
using Microsoft.Azure.WebJobs.Host.Config;

namespace HueClient.Bindings.HueAPIOutputBinding;

internal class AzureHueAPIConfigProvider : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        var azureHueApiSetLightStateBindingRule = context.AddBindingRule<AzureHueAPIAttribute>();
        azureHueApiSetLightStateBindingRule.BindToCollector(attribute => new HueAPIDispatcherAsyncCollector(attribute));

        var azureHueApiFetchLightStateBindingRule = context.AddBindingRule<CurrentLightStateBindingAttribute>();
        azureHueApiFetchLightStateBindingRule.BindToInput(attribute => new HueAPIFetcherFluentBinder(attribute).FetchLightStateFromAttribute());
    }
}