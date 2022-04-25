using Newtonsoft.Json;

namespace AzureCloudHue.FanOutFanInFunction;

public class RefreshToken
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("tokenId")]
    public string TokenId { get; set; }
    [JsonProperty("refresh_token")]
    public string Refresh_token { get; set; }
    
    // TODO något av de här behöver vi hålla koll på i refresh token..
    // Minns inte hur de höll ihop riktigt med när man inte längre kan refresha,
    // verkar som att siffrorna förlängs varje gång ändå?
    // public string access_token { get; set; }
    //
    // public string refresh_token_expires_in { get; set; }
    // public string access_token_expires_in { get; set; }

    // public RefreshToken(string id, string refreshToken, string accessTokenExpiresIn)
    // {
    //     this.id = id;
    //     refresh_token = refreshToken;
    //     access_token_expires_in = accessTokenExpiresIn;
    // }

    public RefreshToken(string id, string tokenId, string refreshToken)
    {
        this.Id = id;
        this.TokenId = tokenId;
        Refresh_token = refreshToken;
    }
}