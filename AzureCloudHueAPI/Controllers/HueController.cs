﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using AzureCloudHue.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Q42.HueApi.Models.Groups;

namespace AzureCloudHue.Controllers
{
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

        [HttpGet("/GetStateOfLamp/{lightId}")]
        public async Task<HueLight> GetStateOfLamp(string lightId)
        {
            return await _hueService.GetStateOfLamp(lightId);
        }

        [HttpPost("/SetStateOfIndividualLamp")]
        public async Task<HueResults> SetStateOfIndividualLamp([FromBody] HueLight hueLight)
        {
            return await _hueService.SetIndividualLight(hueLight);
        }
        
        [HttpPost("/SetRotationStateOfIndividualLamp")]
        public async Task<List<HueResults>> SetRotationStateOfIndividualLamp([FromBody] HueLightRotation hueLightRotation)
        {
            return await _hueService.SetIndividualLightRotation(hueLightRotation);
        }
        
        [HttpPost("/SetRotationStateOfIndividualLampBlocking")]
        public async Task<List<HueResults>> SetRotationStateOfIndividualLampBlocking([FromBody] HueLightRotation hueLightRotation)
        {
            return await _hueService.SetIndividualLightRotationBlocking(hueLightRotation);
        }

        [HttpPost("/SetRotationStateOfGroup")]
        public async Task<string> SetRotationStateOfGroup([FromBody] HueGroupRotation hueGroupRotation)
        {
            return await _hueService.SetGroupLightsRotation(hueGroupRotation);
        }
    }
}