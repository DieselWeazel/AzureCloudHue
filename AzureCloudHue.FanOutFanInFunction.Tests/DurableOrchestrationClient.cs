using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AzureFunctionTests;

public class DurableOrchestrationTest
{
    private HttpClient _client;

    private string SEND_REQUEST_URL = "http://localhost:7071/api/HttpTriggerChangeHueLights_HttpStart";

    [SetUp]
    public void Setup()
    {
        _client = new HttpClient();
    }

    [Test]
    public async Task Test1()
    {
        string statusQueryGetUri = "";

        string jsonContent = File.ReadAllText("lights.json");

        var date = DateTime.Now.TimeOfDay;
        var endTime = DateTime.Now.Add(new TimeSpan(0, 0, 5, 0));
        
        // while (DateTime.Now < endTime)
        // {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, SEND_REQUEST_URL);
            requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        
            var responseMessage = await _client.SendAsync(requestMessage);
        
            var streamReader = new StreamReader(responseMessage.Content.ReadAsStream());
            string content = await streamReader.ReadToEndAsync();

            var statusQueryUri = JsonConvert.DeserializeObject<StatusQueryURI>(content);
        
            statusQueryGetUri =
                "http://localhost:7071/runtime/webhooks/durabletask/instances/" +
                $"{statusQueryUri.Id}?taskHub=TestHubName&connection=Storage&code=LtgAn/7aNuqb6M9dmomUxPM6pMPgPwlaza14za0KmbR8hQAOymjt3w==";

            DurableTaskRunTimeStatus durableTaskRunTimeStatus = new DurableTaskRunTimeStatus();
            durableTaskRunTimeStatus.RuntimeStatus = "Starting";

            while (!durableTaskRunTimeStatus.RuntimeStatus.Equals("Completed"))
            {
                Thread.Sleep(1000);
                var getMessage = new HttpRequestMessage(HttpMethod.Get, statusQueryGetUri);
                var respondedGetMessage = await _client.SendAsync(getMessage);
        
                streamReader = new StreamReader(respondedGetMessage.Content.ReadAsStream());
                string statusContent = await streamReader.ReadToEndAsync();
                durableTaskRunTimeStatus = JsonConvert.DeserializeObject<DurableTaskRunTimeStatus>(statusContent);
                Assert.AreEqual(durableTaskRunTimeStatus.InstanceId, statusQueryUri.Id);
            }
            // Thread.Sleep(100);
        // }

        Assert.True(jsonContent.Length > 0);
        
        
    }
}