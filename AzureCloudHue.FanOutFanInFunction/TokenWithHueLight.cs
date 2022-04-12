using AzureCloudHue.Model;

namespace AzureCloudHue.FanOutFanInFunction;

/*
 * TODO, this is just for testing, and shall be refactored.
 */
public class TokenWithHueLight
{
 public OAuth2Token Token { get; set; }
 public HueLight HueLight { get; set; }
 public TokenWithHueLight(HueLight hueLight, OAuth2Token token)
 {
  HueLight = hueLight;
  Token = token;
 }
 
}