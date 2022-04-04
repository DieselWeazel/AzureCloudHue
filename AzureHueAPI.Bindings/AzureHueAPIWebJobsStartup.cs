using System.Reflection;
using HueClient.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(AzureHueAPIWebJobsStartup))]
namespace HueClient.Bindings;

public class AzureHueAPIWebJobsStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddExtension<AzureHueAPIConfigProvider>();
    }
    
}