using System.Collections.Generic;

namespace AzureCloudHue.Model
{
    public class HueLightRotation
    {
        public int LightId { get; set; }
        
        public List<LightState> LightStates { get; set; }
    }
}