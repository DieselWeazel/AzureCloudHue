using Newtonsoft.Json;

namespace AzureCloudHue.FanOutFanInFunction;

public class OAuth2Token
{
    [JsonProperty("access_token")]
    public string AccessToken {
        get;
        set;
    }
    [JsonProperty("refresh_token")]
    public string RefreshToken {
        get;
        set;
    }
    [JsonProperty("refresh_token_expires_in")]
    public int RefreshTokenExpiresIn {
        get;
        set;
    }

    [JsonProperty("access_token_expires_in")]
    public int AccessTokenExpiresIn
    {
        get;
        set;
    }
}