namespace AzureCloudHue.FanOutFanInFunction;

public class LightStateJson
{
    public bool on { get; set; }
    
    // TODO är Float egentligen
    public double[] xy { get; set; }
    public int bri { get; set; }
    public int transitiontime { get; set; }

    public LightStateJson(bool @on, double[] xy, int bri, int transitiontime)
    {
        this.on = @on;
        this.xy = xy;
        this.bri = bri;
        transitiontime = transitiontime;
    }

    public override string ToString()
    {
        return $"On: {@on}, x: {xy[0]}, y: {xy[1]}, Bri: {bri}, transitionTime: {transitiontime}";
    }
}