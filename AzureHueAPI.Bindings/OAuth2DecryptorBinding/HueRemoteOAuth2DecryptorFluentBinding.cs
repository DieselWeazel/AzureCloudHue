using System.Security.Cryptography;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureCloudHue.Service;

namespace HueClient.Bindings.OAuth2DecryptorBinding;

public class HueRemoteOAuth2DecryptorFluentBinding
{
    private readonly string _secretKeyVaultKey;
    private readonly string _publicKeyVaultKey;
    private readonly string _vaultName;

    private string _secretKey;
    private string _publicKey;

    public HueRemoteOAuth2DecryptorFluentBinding(HueRemoteOAuth2DecryptorAttribute attribute)
    {
        _secretKeyVaultKey = attribute.SecretKeyVaultKey;
        _publicKeyVaultKey = attribute.PublicKeyVaultKey;
        _vaultName = attribute.VaultName;
    }

    public async Task<string> DecryptAccessToken(OAuth2Token oAuth2Token)
    {
        var kvUri = "https://" + _vaultName + ".vault.azure.net";
        
        var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        var secretKeyResponse = await client.GetSecretAsync(_secretKeyVaultKey);
        var publicKeyResponse = await client.GetSecretAsync(_publicKeyVaultKey);

        _secretKey = secretKeyResponse.Value.Value;
        _publicKey = publicKeyResponse.Value.Value;
        
        var acessToken = DecryptRefreshToken(oAuth2Token.AccessToken);

        return acessToken;
    }
    
    private string DecryptRefreshToken(string refreshTokenToDecrypt)
    {
        try
        {
            string ToReturn = "";

            byte[] privatekeyByte = { };
            privatekeyByte = System.Text.Encoding.UTF8.GetBytes(_secretKey);
            byte[] publickeybyte = { };
            publickeybyte = System.Text.Encoding.UTF8.GetBytes(_publicKey);
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
}