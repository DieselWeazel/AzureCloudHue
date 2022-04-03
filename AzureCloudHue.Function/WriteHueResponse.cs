using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.Function;

public static class WriteHueResponse
{
    [FunctionName("AddHueResultToCosmosDB")]
    public static async Task<string> Run(
        [ActivityTrigger]string okObjectJson,
        [CosmosDB(
            databaseName: "ToDoList",
            collectionName: "new-container",
            CreateIfNotExists = true,
            ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
    {
        await documentsOut.AddAsync(new
        {
            id = Guid.NewGuid().ToString(), okObjectJson
        });

        return JsonConvert.SerializeObject(new OkObjectResult("The Hue Result was stored succesfully!"));
    }
}