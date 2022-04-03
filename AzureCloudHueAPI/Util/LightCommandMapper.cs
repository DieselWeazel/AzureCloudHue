using System;
using AzureCloudHue.Model;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Gamut;
using Q42.HueApi.Models.Gamut;

namespace AzureCloudHue.Util
{
    public class LightCommandMapper
    {
        private CIE1931Gamut _gamut;
        private bool _isCurrentlyOn;
        private string _currentHexColor;
        private byte _currentBrightness;
        private string _hexFromState;
        
        public LightCommandMapper(Light light)
        {
            State state = light.State;

            _isCurrentlyOn = state.On;
            _gamut = HueColorConverter.GetLightGamut(light.ModelId);
            _currentHexColor =
                HueColorConverter.XYToRgb(new CIE1931Point(state.ColorCoordinates[0], state.ColorCoordinates[1]), _gamut).ToHex();
            _hexFromState = HueColorConverter.HexFromState(state, null);
            _currentBrightness = state.Brightness;
        }
        
        public LightCommand MapLightStateToCommand(LightState lightState)
        {
            bool hasChanges = false;

            LightCommand command = new LightCommand();
            
            if (!_isCurrentlyOn.Equals(lightState.On))
            {
                hasChanges = true;
                command.On = lightState.On;
            }
            
            byte newBrightness = Convert.ToByte(lightState.Brightness);

            if (!_currentBrightness.Equals(newBrightness))
            {
                hasChanges = true;
                command.Brightness = newBrightness;
            }

            // Checking if the color is the same is a bit of a hassle
            // Leaving this for future investigation. The Color Converter seems to give a different RGB
            // Than when it goes from XY to RGB.
            
            // Same goes for HexFromState. It gives the same result as XY to RGB..
            if (lightState.HexColor != null)
            {
                hasChanges = true;
                CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(lightState.HexColor), _gamut);
                command.SetColor(point.x, point.y);
            }

            if (hasChanges)
            {
                double transitionTimeNotNegative = lightState.TransitionTimeInMs > 0 ? lightState.TransitionTimeInMs : 0;
                command.TransitionTime = TimeSpan.FromMilliseconds(transitionTimeNotNegative);
                return command;
            }
            
            return null;
        }
    }
}