using System.Net.Sockets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ERMSystem.API.HealthChecks;

public sealed class DependencyReadinessHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DependencyReadinessHealthCheck> _logger;

    public DependencyReadinessHealthCheck(
        IConfiguration configuration,
        ILogger<DependencyReadinessHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var failures = new List<string>();

        await CheckSqlAsync("appDb", _configuration.GetConnectionString("DefaultConnection"), data, failures, cancellationToken);
        await CheckSqlAsync(
            "hospitalDb",
            _configuration.GetConnectionString("HospitalConnection") ?? _configuration.GetConnectionString("DefaultConnection"),
            data,
            failures,
            cancellationToken);

        await CheckTcpAsync("redis", _configuration["Redis:ConnectionString"], 6379, data, failures, cancellationToken);
        await CheckTcpAsync(
            "rabbitMq",
            $"{_configuration["RabbitMQ:Host"]}:{_configuration["RabbitMQ:Port"]}",
            5672,
            data,
            failures,
            cancellationToken);

        return failures.Count == 0
            ? HealthCheckResult.Healthy("All dependencies are reachable.", data: data)
            : HealthCheckResult.Unhealthy(
                $"Dependency readiness failed: {string.Join("; ", failures)}",
                data: data);
    }

    private async Task CheckSqlAsync(
        string name,
        string? connectionString,
        IDictionary<string, object> data,
        ICollection<string> failures,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            data[name] = "missing-connection-string";
            failures.Add($"{name} missing connection string");
            return;
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 5;
            await command.ExecuteScalarAsync(cancellationToken);
            data[name] = "ok";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dependency check failed for {Dependency}.", name);
            data[name] = ex.GetType().Name;
            failures.Add($"{name} unreachable");
        }
    }

    private async Task CheckTcpAsync(
        string name,
        string? endpointValue,
        int defaultPort,
        IDictionary<string, object> data,
        ICollection<string> failures,
        CancellationToken cancellationToken)
    {
        if (!TryParseEndpoint(endpointValue, defaultPort, out var host, out var port))
        {
            data[name] = "missing-endpoint";
            failures.Add($"{name} missing endpoint");
            return;
        }

        try
        {
            using var tcpClient = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));
            await tcpClient.ConnectAsync(host, port, timeoutCts.Token);
            data[name] = "ok";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dependency check failed for {Dependency}.", name);
            data[name] = ex.GetType().Name;
            failures.Add($"{name} unreachable");
        }
    }

    private static bool TryParseEndpoint(
        string? endpointValue,
        int defaultPort,
        out string host,
        out int port)
    {
        host = string.Empty;
        port = defaultPort;

        if (string.IsNullOrWhiteSpace(endpointValue))
        {
            return false;
        }

        var firstEndpoint = endpointValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(firstEndpoint))
        {
            return false;
        }

        var parts = firstEndpoint.Split(':', StringSplitOptions.TrimEntries);
        host = parts[0];
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        if (parts.Length >= 2 && int.TryParse(parts[^1], out var parsedPort))
        {
            port = parsedPort;
        }

        return true;
    }
}
