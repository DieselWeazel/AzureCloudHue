using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCloudHue.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;

namespace AzureCloudHue.FanOutFanInFunction;

public class GetRemoteHueClient
{
    [FunctionName("GetRemoteHueClient")]
    public static async Task<RemoteHueClient> GetClient([ActivityTrigger] RemoteHueClient remoteClient,
        ILogger log)
    {
        string appId = "testingapp";
        string clientId = "L0aAagc4uACK71LBexoYr5AuGVkTeGHR";
        string clientSecret = "L34YTv3nU8KZMNh1";

        IRemoteAuthenticationClient authClient = new RemoteAuthenticationClient(clientId, clientSecret, appId);
            
        AccessTokenResponse storedAccessToken = new AccessTokenResponsePremade("ZyI1HXEYD0P7XUxskYUiCn1snmWo",
            604799, "chjNXITp7sRTQvSm8QOaKLi2qAhZQICk", "Bearer_token");
        authClient.Initialize(storedAccessToken);
        remoteClient = new RemoteHueClient(authClient.GetValidToken);
            
        List<RemoteBridge> bridges = await remoteClient.GetBridgesAsync();
            
        await remoteClient.RegisterAsync(bridges[0].Id, "Sample App");

        return remoteClient;
    }
}