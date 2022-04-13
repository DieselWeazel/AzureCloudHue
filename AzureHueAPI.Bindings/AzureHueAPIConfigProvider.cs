using System.Security.Cryptography;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureCloudHue.Model;
using AzureCloudHue.Service;
using HueClient.Bindings.HueAPIInputBinding;
using HueClient.Bindings.OAuth2DecryptorBinding;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HueClient.Bindings.HueAPIOutputBinding;

internal class AzureHueAPIConfigProvider : IExtensionConfigProvider
{

    public void Initialize(ExtensionConfigContext context)
    {
        var azureHueApiSetLightStateBindingRule = context.AddBindingRule<AzureHueAPIAttribute>();
        azureHueApiSetLightStateBindingRule.BindToCollector(attribute => new HueAPIDispatcherAsyncCollector(attribute));

        var azureHueApiFetchLightStateBindingRule = context.AddBindingRule<CurrentLightStateBindingAttribute>();
        azureHueApiFetchLightStateBindingRule.BindToInput(attribute => new HueAPIFetcherFluentBinder(attribute).FetchLightStateFromAttribute());
        // context.
        var hueRemoteOAuth2DecryptorBindingRule = context.AddBindingRule<HueRemoteOAuth2DecryptorAttribute>();
        hueRemoteOAuth2DecryptorBindingRule.BindToInput(attribute => new HueRemoteOAuth2DecryptorFluentBinding(attribute));
        // TODO Validator?
        // https://www.tomfaltesek.com/azure-functions-input-validation/
    }
}
