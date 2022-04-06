using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using AzureCloudHue.Util;
using Microsoft.Extensions.Logging;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Gamut;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Service.Impl
{
    public class HueService : IHueService
    {
        private readonly ILogger<HueService> _logger;

        private ILocalHueClient _client;

        private HueClient _hueClient;
        
        public HueService(ILogger<HueService> logger)
        {
            _logger = logger;
            // _client = new LocalHueClient("192.168.1.5");
            // _client.Initialize("3ioWgarB3Z6YFdK3aBsewSxPsSSI0DXtxu7loYto");
            _hueClient = new HueClient();
        }

        public async Task InitiateClient()
        {
            await _hueClient.InitRemoteClient("ZyI1HXEYD0P7XUxskYUiCn1snmWo", "604799", "chjNXITp7sRTQvSm8QOaKLi2qAhZQICk");
        }
        
        // TODO den här behöver också loopa igenom kanske?
        // Group är ju All.. Kanske går att använda sig av det?
        public async Task<HueResults> SetAllLights(LightState lightState)
        {
            LightCommand command = new LightCommand();
            
            // Den här behöver nog vara checkad.. 
            // Kanske något får hålla koll på varje lampas state..?
            // Hmm.... 
            // Eller kan jag lagra svaret från HueResult som senast lagrat svar?
            if (lightState.On != null)
            {
                command.On = lightState.On;
            }

            // TODO, hur verifierar jag att HEX färgen är ogiltig?
            // Kastas exception kanske, alltså i ramverket?
            CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(lightState.HexColor), null);
            command.SetColor(point.x, point.y);
            
            // TODO Behöver Transition Time verifieras? Måhända inte.. Men prova med random shit!
            command.TransitionTime = TimeSpan.FromMilliseconds(lightState.TransitionTimeInMs);
            
            // TODO något som verifierar att brightness är 0-255
            command.Brightness = Convert.ToByte(lightState.Brightness);

            return await _hueClient.SendCommandAsync(command);
        }

        public async Task<List<HueResults>> SetIndividualLightRotation(HueLightRotation hueLightRotation)
        {
         
            // TODO den här är också blocking, kalla på HueThread i denna likt i Group.
            List<HueResults> hueResults = new List<HueResults>();
            for (int i = 0; i < hueLightRotation.LightStates.Count; i++)
            {
                // TODO behöver inte hämta tidigare lightstate efter index 0 egentligen.
                var light = await _hueClient.GetLightAsync(hueLightRotation.LightId.ToString());
                var command = new LightCommandMapper(light).MapLightStateToCommand(hueLightRotation.LightStates[i]);
                var hueResult = await _hueClient.SendCommandAsync(command, new List<string>() {hueLightRotation.LightId.ToString()});
                hueResults.Add(hueResult);
                var sleepInInt = Convert.ToInt32(hueLightRotation.LightStates[i].TransitionTimeInMs);
                Thread.Sleep(sleepInInt);
            }

            return hueResults;
        }
        
        public async Task<List<HueResults>> SetIndividualLightRotationBlocking(HueLightRotation hueLightRotation)
        {
            List<HueResults> hueResults = new List<HueResults>();
            for (int i = 0; i < hueLightRotation.LightStates.Count; i++)
            {
                var light = await _hueClient.GetLightAsync(hueLightRotation.LightId.ToString());
                var command = new LightCommandMapper(light).MapLightStateToCommand(hueLightRotation.LightStates[i]);
                var hueResult = await _hueClient.SendCommandAsync(command, new List<string>() {hueLightRotation.LightId.ToString()});
                hueResults.Add(hueResult);
                var sleepInInt = Convert.ToInt32(hueLightRotation.LightStates[i].TransitionTimeInMs);
                Thread.Sleep(sleepInInt);
            }

            return hueResults;
        }

        public async Task<HueResults> SetIndividualLight(HueLight hueLight)
        {
            var light = await _hueClient.GetLightAsync(hueLight.LightId.ToString());
            var command = new LightCommandMapper(light).MapLightStateToCommand(hueLight.LightState);
            return await _hueClient.SendCommandAsync(command, new List<string>() {hueLight.LightId.ToString()});
        }

        public async Task<HueLight> GetStateOfLamp(string lightId)
        {
            var light = await _hueClient.GetLightAsync(lightId);
            HueLight hueLight = new HueLight();
            hueLight.LightId = Convert.ToInt32(lightId);

            LightState lightState = new LightState();
            lightState.Brightness = light.State.Brightness;
            lightState.On = light.State.On;
            double[] xyColorCoordinates = light.State.ColorCoordinates;
            RGBColor color = HueColorConverter.XYToRgb(new CIE1931Point(xyColorCoordinates[0], xyColorCoordinates[1]), null);

            lightState.HexColor = color.ToHex();

            hueLight.LightState = lightState;

            return hueLight;
        }
        
        public async Task<string> SetGroupLightsRotation(HueGroupRotation hueGroupRotation)
        {
            var group = await _hueClient.GetGroupAsync(hueGroupRotation.GroupId.ToString());

            // List<HueResults> hueResults = new List<HueResults>();
            // for (int i = 0; i < hueGroup.LightStates.Count; i++)
            // {
            //     var hueResult = await SetLightLight(hueGroup.LightStates[i], group.Lights);
            //     hueResults.Add(hueResult);
            //     var sleepInInt = Convert.ToInt32(hueGroup.LightStates[i].TransitionTimeInMs);
            //     Thread.Sleep(sleepInInt);
            // }
            if (hueGroupRotation.Repeat)
            {
                StartLightRotationRepeat(hueGroupRotation.LightStates, group.Lights);
            }
            else
            {
                StartLightRotation(hueGroupRotation.LightStates, group.Lights);
            }

            return "Done";
        }

        public async Task StartLightRotationRepeat(List<LightState> lightStates, List<string> lights)
        {
            HueThread hueThread = new HueThread(lightStates, lights, _client);
            hueThread.Start();
        }

        public async Task StartLightRotation(List<LightState> lightStates, List<string> lights)
        {
            // List<HueResults> hueResults = new List<HueResults>();
            for (int i = 0; i < lightStates.Count; i++)
            {
                // var hueResult = 
                await SetLightList(lightStates[i], lights);
                // hueResults.Add(hueResult);
                var sleepInInt = Convert.ToInt32(lightStates[i].TransitionTimeInMs);
                Thread.Sleep(sleepInInt);
            }
        }

        private async Task<HueResults> SetLightList(LightState lightState, List<string> lights)
        {
            // TODO LightCommandMapper!
            LightCommand command = new LightCommand();

            CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(lightState.HexColor), null);
            command.SetColor(point.x, point.y);
            
            command.TransitionTime = TimeSpan.FromMilliseconds(lightState.TransitionTimeInMs);
            
            command.Brightness = Convert.ToByte(lightState.Brightness);
            
            return await _hueClient.SendCommandAsync(command, lights);
        }
    }
}