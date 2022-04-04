using System.Reflection;
using ClassLibrary1;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(HueClientWebJobsStartup))]
namespace ClassLibrary1;

public class HueClientWebJobsStartup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.AddExtension<HueClientConfigProvider>();
    }
    
}