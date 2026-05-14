using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace ERMSystem.API.Services;

public class RequestObservabilityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestObservabilityMiddleware> _logger;
    private readonly RequestObservabilityOptions _options;

    public RequestObservabilityMiddleware(
        RequestDelegate next,
        ILogger<RequestObservabilityMiddleware> logger,
        IOptions<RequestObservabilityOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationHeaderName = string.IsNullOrWhiteSpace(_options.CorrelationHeaderName)
            ? "X-Correlation-Id"
            : _options.CorrelationHeaderName.Trim();

        var correlationId = ResolveCorrelationId(context, correlationHeaderName);
        context.TraceIdentifier = correlationId;
        context.Response.Headers[correlationHeaderName] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path.Value,
            ["RequestMethod"] = context.Request.Method
        });

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;
            var logLevel = ResolveLogLevel(statusCode, elapsedMs);

            _logger.Log(
                logLevel,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                elapsedMs);
        }
    }

    private static string ResolveCorrelationId(HttpContext context, string headerName)
    {
        if (context.Request.Headers.TryGetValue(headerName, out var headerValue))
        {
            var candidate = headerValue.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
    }

    private LogLevel ResolveLogLevel(int statusCode, long elapsedMs)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogLevel.Error;
        }

        if (elapsedMs >= _options.SlowRequestThresholdMs)
        {
            return LogLevel.Warning;
        }

        return LogLevel.Information;
    }
}
