using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;
using Q42.HueApi.Models.Gamut;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue
{
    public class HueDispatcherService
    {
        public HueDispatcherService()
        {
            
        }

        public async Task TestCosmosDB()
        {
            CosmosDBClient cosmosKlienten = new CosmosDBClient();
            await cosmosKlienten.GetStartedDemoAsync();
        }

        public async Task<HueResults> Dispatch()
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
            AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade("ZyI1HXEYD0P7XUxskYUiCn1snmWo",
                604799, "chjNXITp7sRTQvSm8QOaKLi2qAhZQICk", "Bearer_token");
            authClient.Initialize(storedAccessToken);
            IRemoteHueClient client = new RemoteHueClient(authClient.GetValidToken);
            
            List<RemoteBridge> bridges = await client.GetBridgesAsync();
            
            var key = await client.RegisterAsync(bridges[0].Id, "Sample App");
            
            var lightResult = await client.SendCommandAsync(new LightCommand().TurnOff());
            
            Thread.Sleep(1000);

            return await client.SendCommandAsync(new LightCommand().TurnOn());
        }

        public async Task<HueResults> TestTurningSomeLights(string hexColor)
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
            AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade("ZyI1HXEYD0P7XUxskYUiCn1snmWo",
                604799, "chjNXITp7sRTQvSm8QOaKLi2qAhZQICk", "Bearer_token");
            authClient.Initialize(storedAccessToken);
            IRemoteHueClient client = new RemoteHueClient(authClient.GetValidToken);
            
            List<RemoteBridge> bridges = await client.GetBridgesAsync();
            
            var key = await client.RegisterAsync(bridges[0].Id, "Sample App");
            
            var lights = await client.GetLightsAsync();
            var lightList = lights.ToArray();

            Light light = lightList[0];
            LightCommand command = new LightCommand();
            CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(hexColor), null);
            command.SetColor(point.x, point.y);
            command.TransitionTime = TimeSpan.Zero;
            return await client.SendCommandAsync(command);
        }
        
        
        public async Task<HueResults> TestTurningLightZone(string hexColor)
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
            AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade("ZyI1HXEYD0P7XUxskYUiCn1snmWo",
                604799, "chjNXITp7sRTQvSm8QOaKLi2qAhZQICk", "Bearer_token");
            authClient.Initialize(storedAccessToken);
            IRemoteHueClient client = new RemoteHueClient(authClient.GetValidToken);
            
            List<RemoteBridge> bridges = await client.GetBridgesAsync();
            
            var key = await client.RegisterAsync(bridges[0].Id, "Sample App");

            var x = await client.GetGroupAsync("14");
            // x.State.
            var lights = await client.GetLightsAsync();
            var lightList = lights.ToArray();

            Light light = lightList[0];
            LightCommand command = new LightCommand();
            CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(hexColor), null);
            command.SetColor(point.x, point.y);
            command.TransitionTime = TimeSpan.Zero;
            return await client.SendCommandAsync(command, x.Lights);
        }
        
        public async Task<HueResults> TestTurningLightZone_NonRemote(string hexColor)
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            ILocalHueClient client = new LocalHueClient("192.168.1.5");
            
            client.Initialize("3ioWgarB3Z6YFdK3aBsewSxPsSSI0DXtxu7loYto");
            
            var lamporVidSangen = await client.GetGroupAsync("14");
            // x.State.
            var lights = await client.GetLightsAsync();
            var lightList = lights.ToArray();

            Light light = lightList[0];
            LightCommand command = new LightCommand();
            CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(hexColor), null);
            command.SetColor(point.x, point.y);
            command.TransitionTime = TimeSpan.Zero;
            return await client.SendCommandAsync(command, lamporVidSangen.Lights);
        }
        
        public async Task<HueResults> DoLotsOfLights()
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            ILocalHueClient client = new LocalHueClient("192.168.1.5");
            client.Initialize("3ioWgarB3Z6YFdK3aBsewSxPsSSI0DXtxu7loYto");
            
            List<string> colorList = new List<string>()
            {
                "0011ff", "ff1100"
            };
            
            var lamporVidSangen = await client.GetGroupAsync("14");
            var lamporVidDatorn = await client.GetGroupAsync("15");
            // x.State.

            List<Group> groups = new List<Group>()
            {
                lamporVidSangen, lamporVidDatorn
            };
            int colorIndex = 0;
            for (int i = 0; i < 20; i++)
            {
                string color = colorList[colorIndex];

                if (colorIndex == 0)
                {
                    colorIndex = 1;
                }
                else
                {
                    colorIndex = 0;
                }

                for (int j = 0; j < groups.Count; j++)
                {
                    LightCommand command = new LightCommand();
                    CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(color), null);
                    command.SetColor(point.x, point.y);
                    
                    Random rnd = new Random();
                    int randomBrightness  = rnd.Next(0, 256);
                    command.Brightness = Convert.ToByte(randomBrightness);
                    command.TransitionTime = TimeSpan.FromMilliseconds(150);
                    await client.SendCommandAsync(command, groups[j].Lights);
                }
                Thread.Sleep(149);
                
                LightCommand commandFlash = new LightCommand();
                CIE1931Point pointFlash = HueColorConverter.RgbToXY(new RGBColor("e100ff"), null);
                commandFlash.SetColor(pointFlash.x, pointFlash.y);
                commandFlash.TransitionTime = TimeSpan.Zero;
                await client.SendCommandAsync(commandFlash);
                // Thread.Sleep(5);
            }
            
            LightCommand commandz = new LightCommand();
            CIE1931Point pointz = HueColorConverter.RgbToXY(new RGBColor("00ff0d"), null);
            commandz.SetColor(pointz.x, pointz.y);
            commandz.TransitionTime = TimeSpan.Zero;
            return await client.SendCommandAsync(commandz);
        }
        
        public async Task<HueResults> TestLightEffect()
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
            AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade("ZyI1HXEYD0P7XUxskYUiCn1snmWo",
                604799, "chjNXITp7sRTQvSm8QOaKLi2qAhZQICk", "Bearer_token");
            authClient.Initialize(storedAccessToken);
            IRemoteHueClient client = new RemoteHueClient(authClient.GetValidToken);
            
            List<RemoteBridge> bridges = await client.GetBridgesAsync();
            
            var key = await client.RegisterAsync(bridges[0].Id, "Sample App");
            
            var lights = await client.GetLightsAsync();
            var lightList = lights.ToArray();

            Light light = lightList[0];
            LightCommand command = new LightCommand();
            // CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(hexColor), null);
            // command.SetColor(point.x, point.y);
            // command.TransitionTime = TimeSpan.Zero;
            command.Effect = Effect.ColorLoop;
            return await client.SendCommandAsync(command);
        }
        
        
        public async Task<HueResults> TestCreateGroup()
        {
            string appId = "testingapp";
            string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
            string clientSecret = "L34YTv3nU8KZMNh1";

            IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
            AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade("ZyI1HXEYD0P7XUxskYUiCn1snmWo",
                604799, "chjNXITp7sRTQvSm8QOaKLi2qAhZQICk", "Bearer_token");
            authClient.Initialize(storedAccessToken);
            IRemoteHueClient client = new RemoteHueClient(authClient.GetValidToken);
            
            List<RemoteBridge> bridges = await client.GetBridgesAsync();
            
            var key = await client.RegisterAsync(bridges[0].Id, "Sample App");

            List<string> someLights = new List<string>()
            {
                "10", "14", "11", "4"
            };
            
            var newGroup = await client.CreateGroupAsync(someLights, groupType: GroupType.LightGroup);
            var lamporVidSangen = await client.GetGroupAsync(newGroup);
            // x.State.
            var lights = await client.GetLightsAsync();
            var lightList = lights.ToArray();

            Light light = lightList[0];
            LightCommand command = new LightCommand();
            CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor("0011ff"), null);
            command.SetColor(point.x, point.y);
            command.TransitionTime = TimeSpan.Zero;
            return await client.SendCommandAsync(command, lamporVidSangen.Lights);
        }

        /*
         *
    //Create command
      var command = new LightCommand();
      command.TurnOn();
      command.SetColor(new RGBColor("#225566"));

      List<string> lights = new List<string>() { "1", "2", "3" };

      //Send Command
      var result = await _client.SendCommandAsync(command);
      var result2 = await _client.SendCommandAsync(command, lights);

      Assert.IsTrue(result.Count > 0 && result.Any(r => r.Error == null));
      Assert.IsTrue(result2.Count > 0 && result2.Any(r => r.Error == null));
         */
    }
}