using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureCloudHue.Model;
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
        
        public HueService(ILogger<HueService> logger)
        {
            _logger = logger;
            _client = new LocalHueClient("192.168.1.5");
            _client.Initialize("3ioWgarB3Z6YFdK3aBsewSxPsSSI0DXtxu7loYto");
        }
        
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

        public async Task<HueResults> SetIndividualLight(HueLight hueLight)
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
            
            return await SetLightLight(hueLight.LightState, new List<string>{hueLight.LightId.ToString()});
        }

        public async Task<HueResults> SetGroupLights(HueGroup hueGroup)
        {
            var group = await _client.GetGroupAsync(hueGroup.GroupId.ToString());

            return await SetLightLight(hueGroup.LightState, group.Lights);
        }

        private async Task<HueResults> SetLightLight(LightState lightState, List<string> lights)
        {
            LightCommand command = new LightCommand();

            // BaseHueApi#public Task<HueResponse<Light>> GetLights() => HueGetRequest<Light>(LightUrl);
            // ^ Denna kan hämta lampan bättre.
            // Kan vara bra för att checka om lampan är tänd!
            // Light light = await _client.GetLightAsync(hueLight.LightId.ToString());
            
            // if (light.On != null)
            // {
            command.On = lightState.On;
            // }
            
            CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(lightState.HexColor), null);
            command.SetColor(point.x, point.y);
            
            command.TransitionTime = TimeSpan.FromMilliseconds(lightState.TransitionTimeInMs);
            
            command.Brightness = Convert.ToByte(lightState.Brightness);
            
            return await _client.SendCommandAsync(command, lights);
        }
    }
}