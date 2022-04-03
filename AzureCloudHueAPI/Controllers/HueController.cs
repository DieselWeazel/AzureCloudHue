using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using AzureCloudHue.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Controllers
{
    // Route är Hue, Hue[Controller].
    [ApiController]
    [Route("[controller]")]
    public class HueController : ControllerBase
    {

        private readonly ILogger<HueController> _logger;

        private readonly IHueService _hueService;
        
        public HueController(ILogger<HueController> logger,
            IHueService hueService)
        {
            _logger = logger;
            _hueService = hueService;
        }

        [HttpPost("/SetStateOfAllLamps")]
        public async Task<HueResults> SetStateOfAllLamps([FromBody] LightState lightState)
        {
            return await _hueService.SetAllLights(lightState);
        }

        [HttpPost("/SetStateOfIndividualLamp")]
        public async Task<List<HueResults>> SetStateOfIndividualLamp([FromBody] HueLightRotation hueLightRotation)
        {
            return await _hueService.SetIndividualLight(hueLightRotation);
        }

        [HttpPost("/SetStateOfGroup")]
        public async Task<string> SetStateOfGroup([FromBody] HueGroupRotation hueGroupRotation)
        {
            return await _hueService.SetGroupLights(hueGroupRotation);
        }
    }
}