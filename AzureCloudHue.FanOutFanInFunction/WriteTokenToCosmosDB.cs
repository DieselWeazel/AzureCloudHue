using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.FanOutFanInFunction;

public class WriteTokenToCosmosDB
{
    private static CosmosClient cosmosclient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDbConnectionString"));

    [FunctionName("WriteTokenToCosmosDB")]
    public static async Task<string> WriteTokenToCosmosDBFunction(
        [ActivityTrigger] RefreshToken refreshToken,
        // [CosmosDB(
        //     databaseName: "Hue",
        //     collectionName: "RefreshToken",
        //     ConnectionStringSetting = "CosmosDbConnectionString")]
        // IAsyncCollector<RefreshToken> refreshTokenOut,
        ILogger log)
    {
        
        // CosmosClient cosmosclient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDbConnectionString"));
        // TODO, borde ersätta DB och Container med fler miljövariabler.
        // För simplicitet borde inte miljövariabeln heta samma som databasen egentligen,
        // men kan strunta i det nu eftersom de är rätt liten detalj för mig. Men framtida grejer bör nog inte förtälja det.
        Container container = cosmosclient.GetContainer("Hue", "RefreshToken");

        log.LogInformation($"ID: {refreshToken.id}");


        // testingRefreshToken.Id = Guid.NewGuid().ToString();
        // i exemplet skickades även new PartitionKey(..) in, i mitt fall finns ju ingen sån
        // var testing = await container.CreateItemAsync(testingRefreshToken);
        PartitionKey partitionKey = new PartitionKey(refreshToken.id);
        var refreshTokenResponse = await container.ReplaceItemAsync(refreshToken, refreshToken.id, new PartitionKey(refreshToken.id));

        // log.LogInformation($"RefreshTokenResponse from Cosmos: ({refreshTokenResponse})");
        // TODO nu följer jag inte mitt mönster från övriga functions
        // Men frågan är, vad vill man bekräfta med att vi lyckats med det här?
        // Möjligen bara ett OK? Eller tid skapad kanske? Hmm
        
        // Det kanske inte är bra att lagra refreshTokenResponse, det kanske gör så att man kan se den enkrypterade nyckeln?
        return JsonConvert.SerializeObject(new OkObjectResult(refreshTokenResponse.Headers));
    }
}