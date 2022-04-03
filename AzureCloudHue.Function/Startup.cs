
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Q42.HueApi;
using Q42.HueApi.Interfaces;

[assembly: FunctionsStartup(typeof(AzureCloudHue.Function.Startup))]

namespace AzureCloudHue.Function;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton(
            _ =>
            {
                ILocalHueClient client = new LocalHueClient("192.168.1.5");
                client.Initialize("3ioWgarB3Z6YFdK3aBsewSxPsSSI0DXtxu7loYto");
                return client;
            });
    }
}