// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Threading.Tasks;
// using AzureCloudHue.Model;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.Http;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Azure.WebJobs.Host;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
// using Q42.HueApi;
// using Q42.HueApi.ColorConverters;
// using Q42.HueApi.ColorConverters.Gamut;
// using Q42.HueApi.Interfaces;
// using Q42.HueApi.Models.Gamut;
//
// namespace AzureCloudHue.Function
// {
//     public static class HttpTriggerHueLamp
//     {
//         [FunctionName("HttpTriggerHueLamp")]
//         public static async Task<IActionResult> RunAsync(
//             [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HueLight hueLight, ILogger log)
//         {
//             log.LogInformation("C# HTTP trigger function processed a request.");
//             
//             
//             // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
//             // dynamic data = JsonConvert.DeserializeObject(requestBody);
//             
//             LightCommand command = new LightCommand();
//             ILocalHueClient _client = new LocalHueClient("192.168.1.5");
//             _client.Initialize("3ioWgarB3Z6YFdK3aBsewSxPsSSI0DXtxu7loYto");
//
//             LightState lightState = hueLight.LightState;
//             
//             CIE1931Point point = HueColorConverter.RgbToXY(new RGBColor(lightState.HexColor), null);
//             command.SetColor(point.x, point.y);
//             
//             // TODO Behöver Transition Time verifieras? Måhända inte.. Men prova med random shit!
//             command.TransitionTime = TimeSpan.FromMilliseconds(lightState.TransitionTimeInMs);
//             
//             // TODO något som verifierar att brightness är 0-255
//             command.Brightness = Convert.ToByte(lightState.Brightness);
//             
//             var sentCommand = await _client.SendCommandAsync(command, new List<string>() {hueLight.LightId.ToString()});
//
//             return new OkObjectResult(sentCommand);
//             // return lightId != null
//             //     ? (ActionResult) new OkObjectResult($"Hello, lightid: {lightId}, hexColor: {hexColor}, brightness: {brightness}")
//             //     : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
//             
//         }
//     }
// }