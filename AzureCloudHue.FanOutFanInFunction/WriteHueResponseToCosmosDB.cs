using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.Function;

public static class WriteHueResponseToCosmosDB
{
    
    // TODO borde använda Hue Databasen!
    [FunctionName("AddHueResultToCosmosDB")]
    public static async Task<string> WriteHueResponseFunction(
        [ActivityTrigger]string okObjectJson,
        [CosmosDB(
            "Hue",
            "hue-lightstates",
            CreateIfNotExists = true,
            ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
    {
        await documentsOut.AddAsync(new
        {
            id = Guid.NewGuid().ToString(), okObjectJson
        });

        // TODO, egentligen borde väl den här ha ett OkObject som representerar
        // att vi lyckats lagra i Cosmos, 
        // som i sin tur håller vad den lagrat?
        
        // Dessutom formateras den lite uselt
        /*
         * 1. Massor av Escapes,
         *
         * 2. Inget radbryt mellan diverse ändringar,
         * success: lights/10/state/transitiontime, sen en till med state/on
         *
         * 3. Inget radbryt per lampa.
         * String.Join borde med andra ord göra ny rad åtminstone för det.
         */
        return okObjectJson;
    }
}