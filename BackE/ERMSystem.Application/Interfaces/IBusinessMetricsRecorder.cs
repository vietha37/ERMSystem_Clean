using System.Collections.Generic;

namespace ERMSystem.Application.Interfaces;

public interface IBusinessMetricsRecorder
{
    void IncrementEvent(string workflow, string eventName, IReadOnlyDictionary<string, string?>? labels = null);
}
