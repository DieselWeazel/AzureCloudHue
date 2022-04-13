using AzureCloudHue.Model;
using AzureCloudHue.Service;

namespace AzureCloudHue.FanOutFanInFunction;

/*
 * TODO, this is just for testing, and shall be refactored.
 */
public class TokenWithHueLight
{
      public string AccessToken { get; }
      public HueLight HueLight { get; }
      public TokenWithHueLight(HueLight hueLight, string accessToken)
      {
       HueLight = hueLight;
       AccessToken = accessToken;
      }
}