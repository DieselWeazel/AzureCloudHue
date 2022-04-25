using HueClient.Bindings.HueAPIInputBinding;
using HueClient.Bindings.HueAPIOutputBinding;
using HueClient.Bindings.HueOAuth2Binding;
using HueClient.Bindings.OAuth2DecryptorBinding;
using Microsoft.Azure.WebJobs.Host.Config;

namespace HueClient.Bindings;

internal class AzureHueAPIConfigProvider : IExtensionConfigProvider
{

    public void Initialize(ExtensionConfigContext context)
    {
        var azureHueApiSetLightStateBindingRule = context.AddBindingRule<AzureHueAPIAttribute>();
        azureHueApiSetLightStateBindingRule.BindToCollector(attribute => new HueAPIDispatcherAsyncCollector(attribute));

        var azureHueApiFetchLightStateBindingRule = context.AddBindingRule<CurrentLightStateBindingAttribute>();
        azureHueApiFetchLightStateBindingRule.BindToInput(attribute => new HueAPIFetcherFluentBinder(attribute).FetchLightStateFromAttribute());
        // context.
        var decryptorBindingRule = context.AddBindingRule<CryptographerAttribute>();
        decryptorBindingRule.BindToInput(attribute => new CryptographerFluentBinding(attribute));

        var hueOauth2BindingRule = context.AddBindingRule<HueRemoteOAuth2Attribute>();
        hueOauth2BindingRule.BindToInput(attribute => new HueRemoteOAuth2FluentBinding(attribute));
        // TODO Validator?
        // https://www.tomfaltesek.com/azure-functions-input-validation/
    }
}
