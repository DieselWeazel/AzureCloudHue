using System.Collections.Generic;

namespace AzureCloudHue.Model
{
    public class HueGroupRotation
    {
        public int GroupId { get; set; }
        
        public List<LightState> LightStates { get; set; }
        
        public bool Repeat { get; set; }
    }
}