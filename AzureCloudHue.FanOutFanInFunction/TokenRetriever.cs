using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.FanOutFanInFunction;

/*
 * Secure OAuth 2.0 On-Behalf-Of refresh tokens for web services
 * https://docs.microsoft.com/en-us/azure/architecture/example-scenario/secrets/secure-refresh-tokens
 * "Azure Pipelines is a convenient place to add your key rotation strategy, if you're already using Pipelines for infrastructure-as-code (IaC)"
 *
 * (Men, om man redan har en pipeline så kunde de vara lika bra att använda den)
 */
public class TokenRetriever
{
    private static string REFRESH_TOKEN_URL = "https://api.meethue.com/oauth2/refresh?grant_type=refresh_token";
    
    // refresh_token
    // access_token_expires
    
    [FunctionName("TokenRetriever")]
    public static async Task<KeyValuePair<Token, RefreshToken>> TokenRetrieverFunction([ActivityTrigger] string dummyString,
        [CosmosDB(databaseName: "Hue",
            collectionName: "RefreshToken",
            SqlQuery = "SELECT * FROM RefreshToken",
            ConnectionStringSetting = "CosmosDbConnectionString")] IEnumerable<RefreshToken> refreshTokens,
        ILogger log)
    {
        HttpClient client = new HttpClient();
        
        if (refreshTokens is null)  
        {
            // return new NotFoundResult();
            throw new ArgumentException("No Tokens found!");
        }

        RefreshToken refreshTokenFromDB = null;
        foreach (RefreshToken refToken in refreshTokens)
        {
            refreshTokenFromDB = refToken;
        }

        if (refreshTokenFromDB is null)
        {
            throw new ArgumentException("Token was null.");
        }
        // var refreshTokenFromDB = refreshTokens.GetEnumerator().T; 
        
        // TODO om expiration date har passerat
        // så måste den här kasta ett exception
        
        // Det kan vara möjligt att förnya token i någon pipeline också förvisso kanske
        // som i sin tur kör något Selenium trams. Men det känns overkill.
        var decryptedRefreshToken = DecryptRefreshToken(refreshTokenFromDB.refresh_token);
        
        var refreshTokenForm = new Dictionary < string,
            string > {
            {
                "refresh_token",
                decryptedRefreshToken
                // "n6W54qj6Gl7FGtQHCWQ1czQs1VRLgDNf"
            }
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, REFRESH_TOKEN_URL) { Content = new FormUrlEncodedContent(refreshTokenForm) };
        requestMessage.Content = new FormUrlEncodedContent(refreshTokenForm);
        
        var byteArray = Encoding.ASCII.GetBytes("L0aAagc4uACK71LBexoYr5AuGVkTeGHR:L34YTv3nU8KZMNh1");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        
        var tokenResponse = await client.SendAsync(requestMessage);

        var streamReader = new StreamReader(tokenResponse.Content.ReadAsStream());
        string content = await streamReader.ReadToEndAsync();
        log.LogInformation($"Token Content {content}");
        var token = JsonConvert.DeserializeObject<Token>(content);

        // Encryption
        var refreshToken = EncryptRefreshToken(token.RefreshToken);
        log.LogInformation($"RefreshToken =({refreshToken})");

        string publicKey = "12345678";
        string secretKey = "87654321";
        
        // TODO den här mappningen höll ej, förmodligen för att ExpiresIn inte matchar refresh_token_expires_in, 
        // eller access_token_expires_in
        
        // TODO, den kanske funkar nu?
        log.LogInformation($"Token retirved, expires in {token.ExpiresIn}");

        // Sätt om refreshToken, detta borde rent tekniskt göras separat men whatever for now
        refreshTokenFromDB.refresh_token = refreshToken;
        
        return new KeyValuePair<Token, RefreshToken>(token, refreshTokenFromDB);
    }
    
    
    // TODO
    // Refresh_token_Expires_in
    // är nog max tiden jag kan använda access-token
    // Så den bör sparas eller sparas som en "giltig till" grej!
    static string DecryptRefreshToken(string refreshTokenToDecrypt)
    {
        try
        {
            string ToReturn = "";
            string publickey = Environment.GetEnvironmentVariable("public_key");
            string secretkey = Environment.GetEnvironmentVariable("secret_key");

            byte[] privatekeyByte = { };
            privatekeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
            byte[] publickeybyte = { };
            publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
            MemoryStream ms = null;
            CryptoStream cs = null;
            byte[] inputbyteArray = new byte[refreshTokenToDecrypt.Replace(" ", "+").Length];
            inputbyteArray = Convert.FromBase64String(refreshTokenToDecrypt.Replace(" ", "+"));
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                ms = new MemoryStream();
                cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                cs.FlushFinalBlock();
                Encoding encoding = Encoding.UTF8;
                ToReturn = encoding.GetString(ms.ToArray());
            }
            return ToReturn;
        }
        catch (Exception ae)
        {
            throw new Exception(ae.Message, ae.InnerException);
        }
    }
    
    public static string EncryptRefreshToken(string refreshToken)
    {
        try
        {
            string ToReturn = "";
            string publickey = Environment.GetEnvironmentVariable("public_key");
            string secretkey = Environment.GetEnvironmentVariable("secret_key");
            
            byte[] secretkeyByte = { };
            secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
            byte[] publickeybyte = { };
            publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
            MemoryStream ms = null;
            CryptoStream cs = null;
            byte[] inputbyteArray = Encoding.UTF8.GetBytes(refreshToken);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                ms = new MemoryStream();
                cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                cs.FlushFinalBlock();
                ToReturn = Convert.ToBase64String(ms.ToArray());
            }
            return ToReturn;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex.InnerException);
        }
    }
}