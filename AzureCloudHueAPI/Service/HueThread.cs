using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Gamut;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Service
{
    public class HueThread
    {
        private List<LightState> _lightStates;
        private List<string> _lights;
        private ILocalHueClient _client;

        public HueThread(List<LightState> lightStates, List<string> lights, ILocalHueClient client)
        {
            _lightStates = lightStates;
            _lights = lights;
            _client = client;
        }

        public void Start()
        {
            Console.WriteLine("Thread is Starting");
            ThreadStart threadStart = StartLoop;
            Thread childThread = new Thread(threadStart);
            childThread.Start();
        }

        private async void StartLoop()
        {
            // List<HueResults> hueResults = new List<HueResults>();
            while (true)
            {
                for (int i = 0; i < _lightStates.Count; i++)
                {
                    // var hueResult = 
                    await SetLightLight(_lightStates[i]);
                    // hueResults.Add(hueResult);
                    var sleepInInt = Convert.ToInt32(_lightStates[i].TransitionTimeInMs);
                    Thread.Sleep(sleepInInt);
                }
            }
        }
        
        private async Task SetLightLight(LightState lightState)
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

            await _client.SendCommandAsync(command, _lights);
        }
    }
}