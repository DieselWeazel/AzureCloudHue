using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Service
{
    public interface IHueService
    {
        Task<HueResults> SetAllLights(LightState lightState);

        Task<List<HueResults>> SetIndividualLight(HueLight hueLight);

        Task<string> SetGroupLights(HueGroup hueGroup);
    }
}