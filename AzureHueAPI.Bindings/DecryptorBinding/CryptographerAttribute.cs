using Microsoft.Azure.WebJobs.Description;

namespace HueClient.Bindings.OAuth2DecryptorBinding;

[Binding]
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class CryptographerAttribute : Attribute
{
    [AutoResolve] public string PublicKeyVaultKey { get; set; }
    [AutoResolve] public string SecretKeyVaultKey { get; set; }
    [AutoResolve] public string VaultName { get; set; }
}