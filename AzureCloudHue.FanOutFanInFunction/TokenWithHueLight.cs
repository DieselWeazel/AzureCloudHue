using AzureCloudHue.Model;
using AzureCloudHue.Service;

namespace AzureCloudHue.FanOutFanInFunction;

/*
 * TODO, this is just for testing, and shall be refactored.
 */
public class TokenWithHueLight
{
      public Token Token { get; }
      public HueLight HueLight { get; }
      public TokenWithHueLight(HueLight hueLight, Token token)
      {
       HueLight = hueLight;
       Token = token;
      }
}