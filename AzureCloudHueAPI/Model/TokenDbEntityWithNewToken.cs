using Newtonsoft.Json;

namespace AzureCloudHue.FanOutFanInFunction;

public class TokenDbEntityWithNewToken
{
    public string Id { get; set; }
    public string TokenId { get; set; }
    public Token Token { get; set; }

    public TokenDbEntityWithNewToken(string id, string tokenId, Token token)
    {
        this.Id = id;
        this.TokenId = tokenId;
        Token = token;
    }
}