using System.Net.Http.Headers;
using System.Text;
using AzureCloudHue.FanOutFanInFunction;
using Newtonsoft.Json;

namespace HueClient.Bindings.HueOAuth2Binding;

public class HueRemoteOAuth2FluentBinding
{
    private static string REFRESH_TOKEN_URL = "https://api.meethue.com/oauth2/refresh?grant_type=refresh_token";

    private string _clientId;
    private string _clientSecret;
    
    HttpClient _client;

    public HueRemoteOAuth2FluentBinding(HueRemoteOAuth2Attribute attribute)
    {
        _clientId = attribute.ClientId;
        _clientSecret = attribute.ClientSecret;
        
        _client = new HttpClient();
    }
    
    public async Task<Token> FetchTokenFromPhilipsHue(string refreshToken)
    {
        var refreshTokenForm = new Dictionary<string,
            string>
        {
            {
                "refresh_token",
                refreshToken
            }
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, REFRESH_TOKEN_URL)
            {Content = new FormUrlEncodedContent(refreshTokenForm)};
        requestMessage.Content = new FormUrlEncodedContent(refreshTokenForm);

        var byteArray = Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var tokenResponse = await _client.SendAsync(requestMessage);

        var streamReader = new StreamReader(tokenResponse.Content.ReadAsStream());
        string content = await streamReader.ReadToEndAsync();
        var token = JsonConvert.DeserializeObject<Token>(content);

        if (token == null)
        {
            throw new InvalidOperationException("Could not retrieve token.");
        }

        return token;
    }
}