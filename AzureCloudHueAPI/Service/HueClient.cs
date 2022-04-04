using System;
using System.Collections.Generic;
using System.Linq;
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
            Console.WriteLine("INITIATE REMOTE CLIENT!");
            // if (_localAdress != null)
            // {
            //     _client = new LocalHueClient(_localAdress);
            // }
            // else
            // {
                string appId = "testingapp";
                string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
                string clientSecret = "L34YTv3nU8KZMNh1";

                IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
                AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade(accessToken,
                    Convert.ToInt32(accessTokenExpiresIn), refreshToken, "Bearer_token");
                authClient.Initialize(storedAccessToken);
                var remoteHueClient = new RemoteHueClient(authClient.GetValidToken);
            
                List<RemoteBridge> bridges = await remoteHueClient.GetBridgesAsync();
            
                var key = await remoteHueClient.RegisterAsync(bridges[0].Id, "Sample App");
                
                // remoteHueClient.Initialize(bridges.First().Id, "lm6t-LayA8fkxmgT9QTOg68Bn1K4KmyaN4GxA186");
                Console.WriteLine("KEY:");
                // Console.WriteLine(key);
                _client = remoteHueClient;
                Console.WriteLine("hello");
                Console.WriteLine(_client);
            // }
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