using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HueClient.Bindings.HueOAuth2Binding;
using HueClient.Bindings.OAuth2DecryptorBinding;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.FanOutFanInFunction;

public class TokenRetriever
{
    /*
     *De här kanske inte är jätterelevant för min del, beror på slutlösning
     *
     * https://github.com/Azure/azure-functions-host/wiki/Managing-Connections
     *
     *
     * DO NOT create a new client with every function invocation.
        DO create a single, static client that can be used by every function invocation.
        CONSIDER creating a single, static client in a shared helper class if different functions will be using the same service.
     */

    private static string REFRESH_TOKEN_URL = "https://api.meethue.com/oauth2/refresh?grant_type=refresh_token";


    [FunctionName("TokenRetriever")]
    public static async Task<TokenDbEntityWithNewToken> TokenRetrieverFunction([ActivityTrigger] string orchestrationId,
        [CosmosDB("Hue",
            "Token",
            SqlQuery = "SELECT * FROM Token",
            ConnectionStringSetting = "CosmosDbConnectionString")]
        IEnumerable<RefreshToken> refreshTokens,
        [HueRemoteOAuth2
            (ClientId = "%CLIENT_ID%", ClientSecret = "%CLIENT_SECRET%")]
        HueRemoteOAuth2FluentBinding hueRemoteOAuth2,
        [Cryptographer
            (VaultName = "%VAULT_NAME%", PublicKeyVaultKey = "%PUBLIC_KEY%", SecretKeyVaultKey = "%SECRET_KEY%")]
        CryptographerFluentBinding cryptographer,
        ILogger log)
    {
        log.LogInformation($"Token retriever initialized with orchestrationId {orchestrationId}");
        log.LogInformation("Looking for token inside storage.");
        
        if (refreshTokens is null) throw new ArgumentException("No Tokens found!");

        var refreshTokenFromDB = FetchPreviousRefreshTokenFromDB(refreshTokens);

        log.LogInformation($"TokenId from DB: {refreshTokenFromDB.TokenId}");

        var decryptedRefreshToken = await cryptographer.GetDecryptedToken(refreshTokenFromDB.Refresh_token);
        var token = await hueRemoteOAuth2.FetchTokenFromPhilipsHue(decryptedRefreshToken);

        EncryptTokenToHideItsValuesInsideAzureStorage(cryptographer, token);

        var tokenDbEntityWithToken =
            new TokenDbEntityWithNewToken(refreshTokenFromDB.Id, refreshTokenFromDB.TokenId, token);
        return tokenDbEntityWithToken;
    }

    private static void EncryptTokenToHideItsValuesInsideAzureStorage(CryptographerFluentBinding cryptographer, Token token)
    {
        token.AccessToken = cryptographer.GetEncryptedToken(token.AccessToken);
        token.RefreshToken = cryptographer.GetEncryptedToken(token.RefreshToken);
    }

    private static RefreshToken FetchPreviousRefreshTokenFromDB(IEnumerable<RefreshToken> refreshTokens)
    {
        RefreshToken refreshTokenFromDB = null;
        var i = 0;
        foreach (var refToken in refreshTokens)
        {
            refreshTokenFromDB = refToken;
            i++;
        }

        if (refreshTokenFromDB is null) throw new ArgumentException("Token was null.");
        return refreshTokenFromDB;
    }
}