using AzureCloudHue.Service;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(HueClient.Bindings.Startup))]

namespace HueClient.Bindings;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton(
            _ =>
            {
                AzureCloudHue.Service.HueClient hueClient = new AzureCloudHue.Service.HueClient();
                Console.WriteLine("SINGLETON ADDED!");
                // hueClient.InitRemoteClient("")
                return hueClient;
            });
    }
}