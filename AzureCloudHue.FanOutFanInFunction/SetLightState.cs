using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using HueClient.Bindings;
using HueClient.Bindings.HueAPIOutputBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.Models.Gamut;

namespace AzureCloudHue.FanOutFanInFunction;

public class SetLightState
{
    // private string GET_LIGHTS_URL = "https://api.meethue.com/bridge/Rf7RBx4nC2TaSu9TKE1xeJnP49JCjflklG0earcn/lights/";

    
    [FunctionName("HueLamp_SetLightState")]
    public async Task<string> SetLightStateFunction([ActivityTrigger] TokenWithHueLight tokenWithHueLight, 
        [AzureHueAPI(Address = "%HueAPIAddress%")]IAsyncCollector<HueLight> bridge,
        ILogger log)
    {
        HttpClient _client = new HttpClient();
        // log.LogInformation($"Setting light with id {hueLight.LightId}");
        // log.LogInformation($"New state to set is {hueLight.LightState}");
        //
        // await bridge.AddAsync(hueLight);
        //
        // return JsonConvert.SerializeObject(new OkObjectResult(hueLight));

        /*
         {"on":true,
        "xy": [
                0.278757,
                0.329774
                    ],
              "bri":192,
              "transitiontime": 0
            }
         */

        var hueLight = tokenWithHueLight.HueLight;

        // TODO miljövariabel!
        var username = "Rf7RBx4nC2TaSu9TKE1xeJnP49JCjflklG0earcn";
        var jsonContent = GetJsonFormattedState(hueLight.LightState);

        log.LogInformation($"JsonContent to be sent to bridge: {jsonContent}");
        
        // TODO, varför reagerar Rider med {{}}? Är det miljövariabler som går in då?
        // var url = $"https://api.meethue.com/bridge/{{username}}/lights/{hueLight.LightId}/state";

        var url = $"https://api.meethue.com/bridge/{username}/lights/{hueLight.LightId}/state";
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
        requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Authorization = newAuthenticationHeaderValue("Bearer", tokenWithHueLight.Token.AccessToken);
        var responseMessage = await _client.SendAsync(requestMessage);
        
        var streamReader = new StreamReader(responseMessage.Content.ReadAsStream());
        string content = await streamReader.ReadToEndAsync();
        
        return JsonConvert.SerializeObject(new OkObjectResult(content));
    }
    
    private AuthenticationHeaderValue newAuthenticationHeaderValue(string bearer, string tokenAccessToken)
    {
        return new AuthenticationHeaderValue(bearer, tokenAccessToken);
    }

    private string GetJsonFormattedState(LightState lightState)
    {
        // TODO Gamut är nu null.
        CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(lightState.HexColor), null);

        var xyCoordinates = new double[] {point.x, point.y};

        LightStateJson lightStateJson = new LightStateJson
        (lightState.On, xyCoordinates, lightState.Brightness, (int) lightState.TransitionTimeInMs
        );
        
        
        return JsonConvert.SerializeObject(lightStateJson);
    }
}