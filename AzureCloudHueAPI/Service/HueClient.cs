using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;

namespace AzureCloudHue.Service
{
    public class HueClient
    {
        private IHueClient _client;

        private string _localAdress;
        private string _accessToken;
        private string _accessTokenExpiresIn;
        private string _refreshToken;
        
        public async Task Init()
        {
            if (_localAdress != null)
            {
                _client = new LocalHueClient(_localAdress);
            }
            else
            {
                string appId = "testingapp";
                string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
                string clientSecret = "L34YTv3nU8KZMNh1";

                IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
                AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade(_accessToken,
                    Convert.ToInt32(_accessTokenExpiresIn), _refreshToken, "Bearer_token");
                authClient.Initialize(storedAccessToken);

                _client = new RemoteHueClient(authClient.GetValidToken);
            
                List<RemoteBridge> bridges = await _client.GetBridgesAsync();
            
                var key = await _client.RegisterAsync(bridges[0].Id, "Sample App");
            }
        }
    }
}