using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AzureCloudHue.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HueController : ControllerBase
    {

        private readonly ILogger<HueController> _logger;

        public HueController(ILogger<HueController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "Hello from Azure Cloud Hue";
        }
    }
}