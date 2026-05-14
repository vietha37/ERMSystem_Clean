namespace ERMSystem.API.Services;

public class RequestObservabilityOptions
{
    public string CorrelationHeaderName { get; set; } = "X-Correlation-Id";
    public int SlowRequestThresholdMs { get; set; } = 1000;
}
