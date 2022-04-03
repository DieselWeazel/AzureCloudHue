namespace AzureCloudHue.Model
{
    public class LightState
    {
        public bool On { get; set; }
        
        public int Brightness { get; set; }
        
        public string HexColor { get; set; }
        
        public double TransitionTimeInMs { get; set; }

        public override string ToString()
        {
            return
                $"On: {On}, Brightness: {Brightness}, HexColor: {HexColor}, TransitionTimeInMs: {TransitionTimeInMs}";
        }
    }
}