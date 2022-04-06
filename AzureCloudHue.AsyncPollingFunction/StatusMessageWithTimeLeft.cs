using Dynamitey.DynamicObjects;

namespace AzureCloudHue.AsyncPollingFunction;

public class StatusMessageWithTimeLeft
{
    public double RemainingTime { get; }
    public string Message { get; }

    public StatusMessageWithTimeLeft(double remainingTime, string message)
    {
        RemainingTime = remainingTime;
        Message = message;
    }
}