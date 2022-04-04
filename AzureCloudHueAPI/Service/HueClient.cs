using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Service
{
    public class HueClient
    {
        private IHueClient _client;

        public HueClient()
        {
            
        }
        
        public void InitLocalClient(string localAddress)
        {
            _client = new LocalHueClient(localAddress);
        }
        
        public async Task InitRemoteClient(string accessToken, string accessTokenExpiresIn, string refreshToken)
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
            AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade(accessToken,
                    Convert.ToInt32(accessTokenExpiresIn), refreshToken, "Bearer_token");
            authClient.Initialize(storedAccessToken);
            var remoteHueClient = new RemoteHueClient(authClient.GetValidToken);
            
            List<RemoteBridge> bridges = await remoteHueClient.GetBridgesAsync();
            
            await remoteHueClient.RegisterAsync(bridges[0].Id, "Sample App");
                
            _client = remoteHueClient;
        }

        public async Task<Group> GetGroupAsync(string groupId)
        {
            return await _client.GetGroupAsync(groupId);
        }

        public async Task<Light> GetLightAsync(string id)
        {
            return await _client.GetLightAsync(id);
        }

        public async Task<HueResults> SendCommandAsync(LightCommand command, 
            IEnumerable<string> lightList)
        {
            return await _client.SendCommandAsync(command, lightList);
        }

        public async Task<HueResults> SendCommandAsync(LightCommand command)
        {
            return await _client.SendCommandAsync(command);
        }
    }
}