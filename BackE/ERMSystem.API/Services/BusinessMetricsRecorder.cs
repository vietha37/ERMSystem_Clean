using ERMSystem.Application.Interfaces;

namespace ERMSystem.API.Services;

public class BusinessMetricsRecorder : IBusinessMetricsRecorder
{
    private readonly ApiMetricsCollector _collector;

    public BusinessMetricsRecorder(ApiMetricsCollector collector)
    {
        _collector = collector;
    }

    public void IncrementEvent(string workflow, string eventName, IReadOnlyDictionary<string, string?>? labels = null)
    {
        _collector.RecordBusinessEvent(workflow, eventName, labels);
    }
}
