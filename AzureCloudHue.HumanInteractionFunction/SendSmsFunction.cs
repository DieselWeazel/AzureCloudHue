using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AzureCloudHue.HumanInteractionFunction;

public class SendSmsFunction
{
    [FunctionName("E4_SendSmsChallenge")]
    public static int SendSmsChallenge(
        [ActivityTrigger] string phoneNumber,
        ILogger log,
        [TwilioSms(AccountSidSetting = "TwilioAccountSid", AuthTokenSetting = "TwilioAuthToken", From = "%TwilioPhoneNumber%")]
        out CreateMessageOptions message)
    {
        // Get a random number generator with a random seed (not time-based)
        var rand = new Random(Guid.NewGuid().GetHashCode());
        int challengeCode = rand.Next(10000);

        log.LogInformation($"Sending verification code {challengeCode} to {phoneNumber}.");

        message = new CreateMessageOptions(new PhoneNumber(phoneNumber));
        // message.Body = $"Your verification code is {challengeCode:0000}";
        message.Body = $"Testing Message";

        return challengeCode;
    }
}