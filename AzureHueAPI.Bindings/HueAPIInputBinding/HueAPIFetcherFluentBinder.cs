using AzureCloudHue.Model;
using Newtonsoft.Json;

namespace HueClient.Bindings.HueAPIInputBinding;

public class HueAPIFetcherFluentBinder : HueLight
{
    private CurrentLightStateBindingAttribute _currentLightStateBindingAttribute;
    
    private HttpClient _httpClient;
    
    public HueAPIFetcherFluentBinder(CurrentLightStateBindingAttribute currentLightStateBindingAttribute)
    {
        _currentLightStateBindingAttribute = currentLightStateBindingAttribute;
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

        _httpClient = new HttpClient(clientHandler);
    }

    public HueLight FetchLightStateFromAttribute()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
            $"{_currentLightStateBindingAttribute.Address}GetStateOfLamp/{_currentLightStateBindingAttribute.LightId}");
        
        var response = _httpClient.Send(request);
        var light = JsonConvert.DeserializeObject<HueLight>(new StreamReader(response.Content.ReadAsStream()).ReadToEnd());
        
        return light;
    }
}