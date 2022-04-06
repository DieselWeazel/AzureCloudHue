using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Newtonsoft.Json;
using NUnit.Framework;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Tests
{
    public class Tests
    {
        private HttpClient _httpClient;
        
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task Test1()
        {
            // https://localhost:5001/SetRotationStateOfIndividualLamp
            // http://localhost:7071/api/HttpTriggerChangeHueLights_HttpStart
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            _httpClient = new HttpClient(clientHandler);
            
            HueLight hueLight = new HueLight();
            hueLight.LightId = 9;
            LightState lightState = new LightState();

            lightState.Brightness = 254;
            lightState.On = true;
            lightState.HexColor = "0e17c2";
            lightState.TransitionTimeInMs = 1000;
            
            hueLight.LightState = lightState;
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                "https://localhost:5001/SetStateOfIndividualLamp");

            string hueLightJson = JsonConvert.SerializeObject(hueLight);
        
            request.Content = new StringContent(hueLightJson);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var setRotationStateResponse = await _httpClient.SendAsync(request);

            var content = await new StreamReader(setRotationStateResponse.Content.ReadAsStream()).ReadToEndAsync();
            HueResults hueResults = JsonConvert.DeserializeObject<HueResults>(content);

            Assert.IsTrue(hueResults.Count > 0 && hueResults.Any(r => r.Error == null));
        }
    }
}