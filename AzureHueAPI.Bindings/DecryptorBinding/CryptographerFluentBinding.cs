using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureCloudHue.FanOutFanInFunction;
using AzureCloudHue.Service;
using Newtonsoft.Json;

namespace HueClient.Bindings.OAuth2DecryptorBinding;

// TODO de här är snarare en VaultKeyDecryptor, eller VaultKeyCryptographer?
public class CryptographerFluentBinding
{
    private readonly string _secretKeyVaultKey;
    private readonly string _publicKeyVaultKey;
    private readonly string _vaultName;

    private string _secretEncryptionKey;
    private string _publicEncryptionKey;


    public CryptographerFluentBinding(CryptographerAttribute attribute)
    {
        _secretKeyVaultKey = attribute.SecretKeyVaultKey;
        _publicKeyVaultKey = attribute.PublicKeyVaultKey;
        _vaultName = attribute.VaultName;
    }

    public async Task<string> GetReEncryptedRefreshToken(string refreshToken)
    {
        await SetNewVaultKeySecrets();

        return Encrypt(refreshToken);
    }
    
    public string GetEncryptedToken(string token)
    {
        return Encrypt(token);
    }

    public async Task<string> GetDecryptedToken(string token)
    {
        var kvUri = "https://" + _vaultName + ".vault.azure.net";
        
        var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        var secretKeyResponse = await client.GetSecretAsync(_secretKeyVaultKey);
        var publicKeyResponse = await client.GetSecretAsync(_publicKeyVaultKey);

        _secretEncryptionKey = secretKeyResponse.Value.Value;
        _publicEncryptionKey = publicKeyResponse.Value.Value;
        
        var decryptedToken = Decrypt(token);

        return decryptedToken;
    }

    private async Task SetNewVaultKeySecrets()
    {
        var kvUri = "https://" + _vaultName + ".vault.azure.net";
        
        var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        var publicKeyVaultSecret = GetUpdatedKeyVaultSecret(_publicKeyVaultKey);
        var secretKeyVaultSecret = GetUpdatedKeyVaultSecret(_secretKeyVaultKey);
        
        _publicEncryptionKey = publicKeyVaultSecret.Value;
        _secretEncryptionKey = secretKeyVaultSecret.Value;
        
        await client.SetSecretAsync(publicKeyVaultSecret);
        await client.SetSecretAsync(secretKeyVaultSecret);
    }
    
    private KeyVaultSecret GetUpdatedKeyVaultSecret(string vaultKey)
    {
        Random rand = new Random();
        int randomPublicKeyValue = rand.Next(10000000, 99999999);
        var keyVaultSecret = new KeyVaultSecret(vaultKey, randomPublicKeyValue.ToString());

        return keyVaultSecret;
    }

    private string Decrypt(string refreshTokenToDecrypt)
    {
        try
        {
            string ToReturn = "";

            byte[] privatekeyByte = { };
            privatekeyByte = System.Text.Encoding.UTF8.GetBytes(_secretEncryptionKey);
            byte[] publickeybyte = { };
            publickeybyte = System.Text.Encoding.UTF8.GetBytes(_publicEncryptionKey);
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
    
    private string Encrypt(string token)
    {
        try
        {
            string ToReturn = "";

            byte[] privatekeyByte = { };
            privatekeyByte = System.Text.Encoding.UTF8.GetBytes(_secretEncryptionKey);
            byte[] publickeybyte = { };
            publickeybyte = System.Text.Encoding.UTF8.GetBytes(_publicEncryptionKey);
            MemoryStream ms = null;
            CryptoStream cs = null;
            byte[] inputbyteArray = Encoding.UTF8.GetBytes(token);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                ms = new MemoryStream();
                cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
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