using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureCloudHue.Service;
using HueClient.Bindings.HueAPIOutputBinding;
using HueClient.Bindings.OAuth2DecryptorBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCloudHue.FanOutFanInFunction;

public class TokenRetriever
{

    [FunctionName("TokenRetriever")]
    public static async Task<string> TokenRetrieverFunction([ActivityTrigger] string dummyString,
        [CosmosDB("Hue",
            "OAuth2Token",
            SqlQuery = "SELECT * FROM OAuth2Token",
            ConnectionStringSetting = "CosmosDbConnectionString")] IEnumerable<OAuth2Token> refreshTokens,
        [HueRemoteOAuth2Decryptor
            (PublicKeyVaultKey = "%public_key%", SecretKeyVaultKey = "%secret_key%",
                VaultName = "%KEY_VAULT_NAME%")] HueRemoteOAuth2DecryptorFluentBinding hueRemoteOAuth2Decryptor,
        ILogger log)
    {
        OAuth2Token oAuth2Token = null;
        foreach (OAuth2Token refToken in refreshTokens)
        {
            oAuth2Token = refToken;
        }
        
        if (oAuth2Token is null)
        {
            throw new ArgumentException("No token has been found.");
        }
        
        var accessToken = await hueRemoteOAuth2Decryptor.DecryptAccessToken(oAuth2Token);
        log.LogInformation($"Access token = {accessToken}");
        
        return accessToken;
    }
    
    
    // TODO
    // Refresh_token_Expires_in
    // är nog max tiden jag kan använda access-token
    // Så den bör sparas eller sparas som en "giltig till" grej!
    static string DecryptRefreshToken(string refreshTokenToDecrypt, string secretKey, string publicKey)
    {
        try
        {
            string ToReturn = "";
            // string publickey = Environment.GetEnvironmentVariable("public_key");
            // string secretkey = Environment.GetEnvironmentVariable("secret_key");

            byte[] privatekeyByte = { };
            privatekeyByte = System.Text.Encoding.UTF8.GetBytes(secretKey);
            byte[] publickeybyte = { };
            publickeybyte = System.Text.Encoding.UTF8.GetBytes(publicKey);
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