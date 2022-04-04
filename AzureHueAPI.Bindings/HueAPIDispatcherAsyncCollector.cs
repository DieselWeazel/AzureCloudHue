using System.Net.Http.Headers;
using AzureCloudHue.Model;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace HueClient.Bindings;

public class HueAPIDispatcherAsyncCollector : IAsyncCollector<HueLight>
{
    private AzureHueAPIAttribute _azureHueApiAttribute;

    private HttpClient _httpClient;
    
    public HueAPIDispatcherAsyncCollector(AzureHueAPIAttribute azureHueApiAttribute)
    {
        _azureHueApiAttribute = azureHueApiAttribute;
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

        _httpClient = new HttpClient(clientHandler);
    }
    
    public async Task AddAsync(HueLight item, CancellationToken cancellationToken = new CancellationToken())
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
            $"{_azureHueApiAttribute.Address}SetStateOfIndividualLamp");

        string hueLightJson = JsonConvert.SerializeObject(item);
        
        request.Content = new StringContent(hueLightJson);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        await _httpClient.SendAsync(request);
    }

    public Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }
}