using AzureCloudHue;
using AzureCloudHue.Model;
using AzureCloudHue.Util;
using Microsoft.Azure.WebJobs;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;

namespace ClassLibrary1;

public class HueDispatcherAsyncCollector : IAsyncCollector<HueLight>
{
    // private readonly BridgeAddressAttribute _bridgeAddressAttribute;

    private IHueClient _client;
    
    public HueDispatcherAsyncCollector(HueBridgeAttribute hueBridgeAttribute)
    {
        // _bridgeAddressAttribute = bridgeAddressAttribute;
        if (!string.IsNullOrEmpty(hueBridgeAttribute.Address))
        {
            // 192.168.1.5
            _client = new LocalHueClient(hueBridgeAttribute.Address);
        }
        else
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
            AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade(hueBridgeAttribute.AccessToken,
                Convert.ToInt32(hueBridgeAttribute.AccessTokenExpiresIn), hueBridgeAttribute.RefreshToken, "Bearer_token");
            authClient.Initialize(storedAccessToken);

            _client = new RemoteHueClient(authClient.GetValidToken);
            
            List<RemoteBridge> bridges = await _client.GetBridgesAsync();
            
            var key = await _client.RegisterAsync(bridges[0].Id, "Sample App");
        }
        // _client.Initialize(hueBridgeAttribute.AppKey);
    }
    
    public async Task AddAsync(HueLight item, CancellationToken cancellationToken = new CancellationToken())
    {
        var light = await _client.GetLightAsync(item.LightId.ToString());
        var command = new LightCommandMapper(light).MapLightStateToCommand(item.LightState);
        await _client.SendCommandAsync(command, new List<string>() {item.LightId.ToString()});
    }

    public Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }
}