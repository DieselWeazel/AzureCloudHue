using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.Extensions.Logging;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Gamut;
using Q42.HueApi.Models.Groups;
using Q42.HueApi.Streaming.Models;

namespace AzureCloudHue.Service.Impl
{
    public class HueService : IHueService
    {
        private readonly ILogger<HueService> _logger;

        private ILocalHueClient _client;
        
        public HueService(ILogger<HueService> logger)
        {
            _logger = logger;
            _client = new LocalHueClient("192.168.1.5");
            _client.Initialize("3ioWgarB3Z6YFdK3aBsewSxPsSSI0DXtxu7loYto");
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
            
            return await _client.SendCommandAsync(command);
        }

        public async Task<List<HueResults>> SetIndividualLight(HueLight hueLight)
        {
            LightCommand command = new LightCommand();

            // TODO -> Om det går att få fram lampan före, så lägg Command här
            // Och hämta tillståndet (on/off) först.
            
            // BaseHueApi#public Task<HueResponse<Light>> GetLights() => HueGetRequest<Light>(LightUrl);
            // ^ Denna kan hämta lampan bättre.
            // Kan vara bra för att checka om lampan är tänd!
            // Light light = await _client.GetLightAsync(hueLight.LightId.ToString());
            
            // LightState lightState = hueLight.LightState;
            // if (light.On != null)
            // {
                // command.On = lightState.On;
            // }
            
            List<HueResults> hueResults = new List<HueResults>();
            for (int i = 0; i < hueLight.LightStates.Count; i++)
            {
                var hueResult = await SetLightList(hueLight.LightStates[i], new List<string>{hueLight.LightId.ToString()});
                hueResults.Add(hueResult);
                var sleepInInt = Convert.ToInt32(hueLight.LightStates[i].TransitionTimeInMs);
                Thread.Sleep(sleepInInt);
            }

            return hueResults;
        }

        public async Task<string> SetGroupLights(HueGroup hueGroup)
        {
            var group = await _client.GetGroupAsync(hueGroup.GroupId.ToString());

            // List<HueResults> hueResults = new List<HueResults>();
            // for (int i = 0; i < hueGroup.LightStates.Count; i++)
            // {
            //     var hueResult = await SetLightLight(hueGroup.LightStates[i], group.Lights);
            //     hueResults.Add(hueResult);
            //     var sleepInInt = Convert.ToInt32(hueGroup.LightStates[i].TransitionTimeInMs);
            //     Thread.Sleep(sleepInInt);
            // }
            if (hueGroup.Repeat)
            {
                StartLightRotationRepeat(hueGroup.LightStates, group.Lights);
            }
            else
            {
                StartLightRotation(hueGroup.LightStates, group.Lights);
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
            LightCommand command = new LightCommand();

            // BaseHueApi#public Task<HueResponse<Light>> GetLights() => HueGetRequest<Light>(LightUrl);
            // ^ Denna kan hämta lampan bättre.
            // Kan vara bra för att checka om lampan är tänd!
            // Light light = await _client.GetLightAsync(hueLight.LightId.ToString());
            
            // if (light.On != null)
            // {
            // command.On = lightState.On;
            // }
            
            CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(lightState.HexColor), null);
            command.SetColor(point.x, point.y);
            
            command.TransitionTime = TimeSpan.FromMilliseconds(lightState.TransitionTimeInMs);
            
            command.Brightness = Convert.ToByte(lightState.Brightness);
            
            return await _client.SendCommandAsync(command, lights);
        }
    }
}