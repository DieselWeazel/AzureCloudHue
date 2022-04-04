using System.Reflection;
using HueClient.Bindings;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(HueClientWebJobsStartup))]
namespace HueClient.Bindings;

public class HueClientWebJobsStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddExtension<HueClientConfigProvider>();
    }
    
}