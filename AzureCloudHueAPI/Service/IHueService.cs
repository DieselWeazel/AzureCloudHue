using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Service
{
    public interface IHueService
    {
        Task InitiateClient();
        
        Task<HueResults> SetAllLights(LightState lightState);

        Task<List<HueResults>> SetIndividualLight(HueLightRotation hueLightRotation);

        Task<string> SetGroupLights(HueGroupRotation hueGroupRotation);
    }
}