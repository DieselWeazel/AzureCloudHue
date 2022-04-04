using AzureCloudHue;
using AzureCloudHue.Model;
using AzureCloudHue.Service;
using AzureCloudHue.Util;
using Microsoft.Azure.WebJobs;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;

namespace HueClient.Bindings;

public class HueDispatcherAsyncCollector : IAsyncCollector<HueLight>
{
    // private readonly BridgeAddressAttribute _bridgeAddressAttribute;

    private AzureCloudHue.Service.HueClient _hueClient;

    private HueBridgeAttribute hueBridgeAttribute;

    private Task _initationRemoteClient;

    private RemoteHueClient _client;

    private Task<List<RemoteBridge>?> bridges;

    private List<RemoteBridge> _remoteBridges;

    private RemoteHueClient remoteHueClient;
    
    public HueDispatcherAsyncCollector(HueBridgeAttribute hueBridgeAttribute, AzureCloudHue.Service.HueClient hueClient)
    {
        this.hueBridgeAttribute = hueBridgeAttribute;
        _hueClient = hueClient;
        // _bridgeAddressAttribute = bridgeAddressAttribute;
        if (!string.IsNullOrEmpty(hueBridgeAttribute.Address))
        {
            // 192.168.1.5
            _hueClient.InitLocalClient(hueBridgeAttribute.Address);
            
        }
        else
        {
            _initationRemoteClient = _hueClient.InitRemoteClient(hueBridgeAttribute.AccessToken, hueBridgeAttribute.AccessTokenExpiresIn,
                hueBridgeAttribute.RefreshToken);
            
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
            AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade(hueBridgeAttribute.AccessToken,
                Convert.ToInt32(hueBridgeAttribute.AccessTokenExpiresIn), hueBridgeAttribute.RefreshToken, "Bearer_token");
            authClient.Initialize(storedAccessToken);
            remoteHueClient = new RemoteHueClient(authClient.GetValidToken);
            
            bridges = remoteHueClient.GetBridgesAsync();
            _client = remoteHueClient;
            // var key = await remoteHueClient.RegisterAsync(bridges[0].Id, "Sample App");

            // remoteHueClient.Initialize(bridges.First().Id, "lm6t-LayA8fkxmgT9QTOg68Bn1K4KmyaN4GxA186");
        }
        // _client.Initialize(hueBridgeAttribute.AppKey);
    }
    
    public async Task AddAsync(HueLight item, CancellationToken cancellationToken = new CancellationToken())
    {
        // await _hueClient.InitRemoteClient(hueBridgeAttribute.AccessToken, hueBridgeAttribute.AccessTokenExpiresIn,
        //     hueBridgeAttribute.RefreshToken);
        if (!bridges.IsCompletedSuccessfully)
        {
            _remoteBridges = new List<RemoteBridge>();
            // foreach (var remoteBridge in bridges.Result) _remoteBridges.Add(remoteBridge);
            // await remoteHueClient.RegisterAsync(_remoteBridges[0].Id, "Sample App");
            Console.WriteLine("I'm waiting for the initation");
            await _initationRemoteClient;
        }
        var light = await _hueClient.GetLightAsync(item.LightId.ToString());
        var command = new LightCommandMapper(light).MapLightStateToCommand(item.LightState);
        await _hueClient.SendCommandAsync(command, new List<string>() {item.LightId.ToString()});
    }

    public Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }
}