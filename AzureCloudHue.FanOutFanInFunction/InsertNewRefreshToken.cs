using System.Threading.Tasks;
using HueClient.Bindings.HueOAuth2Binding;
using HueClient.Bindings.OAuth2DecryptorBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.FanOutFanInFunction;

public static class InsertNewRefreshToken
{
    [FunctionName("InsertRefreshToken")]
    public static async Task<string> InsertNewRefreshTokenFunction(
        [ActivityTrigger] TokenDbEntityWithNewToken tokenDbEntityWithToken,
        [HueRemoteOAuth2
            (ClientId = "%CLIENT_ID%", ClientSecret = "%CLIENT_SECRET%")]
        HueRemoteOAuth2FluentBinding hueRemoteOAuth2,
        [Cryptographer
            (VaultName = "%VAULT_NAME%", PublicKeyVaultKey = "%PUBLIC_KEY%", SecretKeyVaultKey = "%SECRET_KEY%")]
        CryptographerFluentBinding cryptographer,
        [CosmosDB("Hue", "Token",
            ConnectionStringSetting = "CosmosDbConnectionString")]
        IAsyncCollector<dynamic> documentsOut,
        ILogger log)
    {
        log.LogInformation("Beginning authorization & insertion of new token.");

        var decryptedRefreshToken = await cryptographer.GetDecryptedToken(tokenDbEntityWithToken.Token.RefreshToken);
        var updatedToken = await hueRemoteOAuth2.FetchTokenFromPhilipsHue(decryptedRefreshToken);
        log.LogInformation("New token retrieved.");

        var encryptedRefreshToken = await cryptographer.GetReEncryptedRefreshToken(updatedToken.RefreshToken);
        log.LogInformation("Refresh token encrypted.");

        var newRefreshToken = new RefreshToken(tokenDbEntityWithToken.Id,
            tokenDbEntityWithToken.TokenId, encryptedRefreshToken);
        
        await documentsOut.AddAsync(newRefreshToken);

        log.LogInformation("Function to Store Token is now complete");
        return JsonConvert.SerializeObject(new OkObjectResult("New refresh token inserted to Cosmos DB"));
    }
}