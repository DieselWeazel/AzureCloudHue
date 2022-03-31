using Q42.HueApi.Models;

namespace AzureCloudHue
{
    
    /*
     *  TODO:
     * Temp solution, don't wanna mess around with Authentication yet
     * since I have a token already
     * 
     */
    public class AccessTokenResponsePremade : AccessTokenResponse
    {
        public AccessTokenResponsePremade(string access_token, int access_token_expires_in,
            string refresh_token, string token_type)
        {
            Access_token = access_token;
            Expires_in = access_token_expires_in;
            Refresh_token = refresh_token;
            Token_type = token_type;
        }
    }
}