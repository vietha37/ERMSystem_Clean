using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace ERMSystem.API.Services;

public class ApiMetricsCollector
{
    private readonly ConcurrentDictionary<string, long> _requestCounts = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, RouteMetricAggregate> _routeAggregates = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, long> _businessEventCounts = new(StringComparer.Ordinal);

    private long _inFlightRequests;
    private long _totalRequests;
    private long _totalSlowRequests;
    private long _totalServerErrors;

    public void OnRequestStarted()
    {
        Interlocked.Increment(ref _inFlightRequests);
    }

    public void OnRequestCompleted(string method, string route, int statusCode, long elapsedMs, bool isSlowRequest)
    {
        Interlocked.Decrement(ref _inFlightRequests);
        Interlocked.Increment(ref _totalRequests);

        if (isSlowRequest)
        {
            Interlocked.Increment(ref _totalSlowRequests);
        }

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            Interlocked.Increment(ref _totalServerErrors);
        }

        var normalizedMethod = string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method.Trim().ToUpperInvariant();
        var normalizedRoute = string.IsNullOrWhiteSpace(route) ? "/" : route.Trim();
        var requestKey = $"{normalizedMethod}|{normalizedRoute}|{statusCode}";
        _requestCounts.AddOrUpdate(requestKey, 1, static (_, current) => current + 1);

        var aggregateKey = $"{normalizedMethod}|{normalizedRoute}";
        var aggregate = _routeAggregates.GetOrAdd(aggregateKey, _ => new RouteMetricAggregate());
        aggregate.Record(elapsedMs, isSlowRequest, statusCode >= StatusCodes.Status500InternalServerError);
    }

    public void RecordBusinessEvent(string workflow, string eventName, IReadOnlyDictionary<string, string?>? labels = null)
    {
        var normalizedWorkflow = NormalizeMetricToken(workflow, "unknown_workflow");
        var normalizedEvent = NormalizeMetricToken(eventName, "unknown_event");
        var keyBuilder = new StringBuilder()
            .Append("workflow=").Append(normalizedWorkflow)
            .Append("|event=").Append(normalizedEvent);

        if (labels != null)
        {
            foreach (var label in labels
                         .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
                         .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                keyBuilder
                    .Append('|')
                    .Append(NormalizeMetricToken(label.Key, "label"))
                    .Append('=')
                    .Append(NormalizeMetricToken(label.Value!, "unknown"));
            }
        }

        _businessEventCounts.AddOrUpdate(keyBuilder.ToString(), 1, static (_, current) => current + 1);
    }

    public string RenderPrometheus()
    {
        var builder = new StringBuilder(4096);

        builder.AppendLine("# HELP ermsystem_api_requests_in_flight Current in-flight HTTP requests.");
        builder.AppendLine("# TYPE ermsystem_api_requests_in_flight gauge");
        builder.Append("ermsystem_api_requests_in_flight ").Append(Volatile.Read(ref _inFlightRequests)).AppendLine();

        builder.AppendLine("# HELP ermsystem_api_requests_total Total completed HTTP requests.");
        builder.AppendLine("# TYPE ermsystem_api_requests_total counter");
        builder.Append("ermsystem_api_requests_total ").Append(Volatile.Read(ref _totalRequests)).AppendLine();

        builder.AppendLine("# HELP ermsystem_api_slow_requests_total Total slow HTTP requests.");
        builder.AppendLine("# TYPE ermsystem_api_slow_requests_total counter");
        builder.Append("ermsystem_api_slow_requests_total ").Append(Volatile.Read(ref _totalSlowRequests)).AppendLine();

        builder.AppendLine("# HELP ermsystem_api_server_errors_total Total HTTP requests returning 5xx.");
        builder.AppendLine("# TYPE ermsystem_api_server_errors_total counter");
        builder.Append("ermsystem_api_server_errors_total ").Append(Volatile.Read(ref _totalServerErrors)).AppendLine();

        builder.AppendLine("# HELP ermsystem_api_requests_by_route_total Completed HTTP requests by method, route, and status code.");
        builder.AppendLine("# TYPE ermsystem_api_requests_by_route_total counter");
        foreach (var item in _requestCounts.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            var segments = item.Key.Split('|');
            if (segments.Length != 3)
            {
                continue;
            }

            builder
                .Append("ermsystem_api_requests_by_route_total{method=\"")
                .Append(EscapeLabelValue(segments[0]))
                .Append("\",route=\"")
                .Append(EscapeLabelValue(segments[1]))
                .Append("\",status_code=\"")
                .Append(EscapeLabelValue(segments[2]))
                .Append("\"} ")
                .Append(item.Value)
                .AppendLine();
        }

        builder.AppendLine("# HELP ermsystem_api_request_duration_ms_sum Total HTTP request duration in milliseconds by method and route.");
        builder.AppendLine("# TYPE ermsystem_api_request_duration_ms_sum counter");
        builder.AppendLine("# HELP ermsystem_api_request_duration_ms_count Total HTTP request samples by method and route.");
        builder.AppendLine("# TYPE ermsystem_api_request_duration_ms_count counter");
        builder.AppendLine("# HELP ermsystem_api_request_duration_ms_max Maximum HTTP request duration in milliseconds by method and route.");
        builder.AppendLine("# TYPE ermsystem_api_request_duration_ms_max gauge");
        builder.AppendLine("# HELP ermsystem_api_route_slow_requests_total Slow HTTP requests by method and route.");
        builder.AppendLine("# TYPE ermsystem_api_route_slow_requests_total counter");
        builder.AppendLine("# HELP ermsystem_api_route_server_errors_total 5xx HTTP requests by method and route.");
        builder.AppendLine("# TYPE ermsystem_api_route_server_errors_total counter");

        foreach (var item in _routeAggregates.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            var segments = item.Key.Split('|');
            if (segments.Length != 2)
            {
                continue;
            }

            var snapshot = item.Value.GetSnapshot();
            var labels = $"{{method=\"{EscapeLabelValue(segments[0])}\",route=\"{EscapeLabelValue(segments[1])}\"}}";

            builder.Append("ermsystem_api_request_duration_ms_sum").Append(labels).Append(' ').Append(snapshot.DurationSumMs).AppendLine();
            builder.Append("ermsystem_api_request_duration_ms_count").Append(labels).Append(' ').Append(snapshot.RequestCount).AppendLine();
            builder.Append("ermsystem_api_request_duration_ms_max").Append(labels).Append(' ').Append(snapshot.MaxDurationMs).AppendLine();
            builder.Append("ermsystem_api_route_slow_requests_total").Append(labels).Append(' ').Append(snapshot.SlowRequestCount).AppendLine();
            builder.Append("ermsystem_api_route_server_errors_total").Append(labels).Append(' ').Append(snapshot.ServerErrorCount).AppendLine();
        }

        builder.AppendLine("# HELP ermsystem_business_events_total Total business workflow events.");
        builder.AppendLine("# TYPE ermsystem_business_events_total counter");
        foreach (var item in _businessEventCounts.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            var labels = ParseMetricLabels(item.Key);
            builder.Append("ermsystem_business_events_total").Append(labels).Append(' ').Append(item.Value).AppendLine();
        }

        return builder.ToString();
    }

    private static string ParseMetricLabels(string key)
    {
        var parts = key.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var labels = new List<string>(parts.Length);
        foreach (var part in parts)
        {
            var separatorIndex = part.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex >= part.Length - 1)
            {
                continue;
            }

            var name = part[..separatorIndex];
            var value = part[(separatorIndex + 1)..];
            labels.Add($"{name}=\"{EscapeLabelValue(value)}\"");
        }

        return labels.Count == 0 ? string.Empty : "{" + string.Join(",", labels) + "}";
    }

    private static string NormalizeMetricToken(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var chars = value.Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray();

        var normalized = new string(chars).Trim('_');
        while (normalized.Contains("__", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("__", "_", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }

    private static string EscapeLabelValue(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }

    private sealed class RouteMetricAggregate
    {
        private long _requestCount;
        private long _durationSumMs;
        private long _maxDurationMs;
        private long _slowRequestCount;
        private long _serverErrorCount;

        public void Record(long elapsedMs, bool isSlowRequest, bool isServerError)
        {
            Interlocked.Increment(ref _requestCount);
            Interlocked.Add(ref _durationSumMs, elapsedMs);

            if (isSlowRequest)
            {
                Interlocked.Increment(ref _slowRequestCount);
            }

            if (isServerError)
            {
                Interlocked.Increment(ref _serverErrorCount);
            }

            long currentMax;
            do
            {
                currentMax = Volatile.Read(ref _maxDurationMs);
                if (elapsedMs <= currentMax)
                {
                    break;
                }
            }
            while (Interlocked.CompareExchange(ref _maxDurationMs, elapsedMs, currentMax) != currentMax);
        }

        public RouteMetricSnapshot GetSnapshot()
        {
            return new RouteMetricSnapshot(
                Volatile.Read(ref _requestCount),
                Volatile.Read(ref _durationSumMs),
                Volatile.Read(ref _maxDurationMs),
                Volatile.Read(ref _slowRequestCount),
                Volatile.Read(ref _serverErrorCount));
        }
    }

    private sealed record RouteMetricSnapshot(
        long RequestCount,
        long DurationSumMs,
        long MaxDurationMs,
        long SlowRequestCount,
        long ServerErrorCount);
}
