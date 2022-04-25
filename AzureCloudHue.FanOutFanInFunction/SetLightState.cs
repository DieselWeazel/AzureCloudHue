using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using HueClient.Bindings;
using HueClient.Bindings.HueAPIOutputBinding;
using HueClient.Bindings.OAuth2DecryptorBinding;
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

    [FunctionName("HueLamp_SetLightState")]
    public async Task<string> SetLightStateFunction([ActivityTrigger] TokenWithHueLight tokenWithHueLight, 
        [Cryptographer
            (VaultName = "%VAULT_NAME%", PublicKeyVaultKey = "%PUBLIC_KEY%", SecretKeyVaultKey = "%SECRET_KEY%")]
        CryptographerFluentBinding cryptographer,
        ILogger log)
    {
        // TODO allt det här borde egentligen till sin egen binding med.
        // Det skulle även se bra ut med username som då kan bli en miljövariabel.
        HttpClient client = new HttpClient();

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
        string accessToken = await cryptographer.GetDecryptedToken(tokenWithHueLight.Token.AccessToken);
        client.DefaultRequestHeaders.Authorization = newAuthenticationHeaderValue("Bearer", accessToken);

        var responseMessage = await client.SendAsync(requestMessage);

        if (!responseMessage.IsSuccessStatusCode)
        {
            log.LogInformation($"Could not Set Hue Light, status code = {responseMessage.StatusCode}");
            throw new FunctionFailedException($"Could not set Hue lamp with id {tokenWithHueLight.HueLight.LightId},\n\r" +
                                              $"could not call Hue Bridge. Status code = {responseMessage.StatusCode}");
        }
        
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

        var xyCoordinates = new[] {point.x, point.y};

        LightStateJson lightStateJson = new LightStateJson
        (lightState.On, xyCoordinates, lightState.Brightness, (int) lightState.TransitionTimeInMs
        );
        
        return JsonConvert.SerializeObject(lightStateJson);
    }
}