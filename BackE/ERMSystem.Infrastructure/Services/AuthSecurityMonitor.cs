using System.Text;
using ERMSystem.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ERMSystem.Infrastructure.Services;

public class AuthSecurityMonitor : IAuthSecurityMonitor
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<AuthSecurityMonitor> _logger;
    private readonly IConfiguration _configuration;

    public AuthSecurityMonitor(
        IDistributedCache distributedCache,
        IConfiguration configuration,
        ILogger<AuthSecurityMonitor> logger)
    {
        _distributedCache = distributedCache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> IsLoginBlockedAsync(string username, CancellationToken ct = default)
    {
        var normalizedUsername = NormalizeUsername(username);
        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return false;
        }

        return await _distributedCache.GetStringAsync(BuildBlockedKey(normalizedUsername), ct) != null;
    }

    public async Task RecordFailedLoginAttemptAsync(string username, CancellationToken ct = default)
    {
        var normalizedUsername = NormalizeUsername(username);
        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return;
        }

        var failedKey = BuildFailedKey(normalizedUsername);
        var failedCount = await ReadCountAsync(failedKey, ct) + 1;
        var maxFailures = ReadIntSetting("Security:SuspiciousLogin:MaxFailedAttempts", 5);
        var windowSeconds = ReadIntSetting("Security:SuspiciousLogin:FailureWindowSeconds", 300);
        var lockoutSeconds = ReadIntSetting("Security:SuspiciousLogin:LockoutSeconds", 900);

        await _distributedCache.SetStringAsync(
            failedKey,
            failedCount.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(windowSeconds)
            },
            ct);

        if (failedCount < maxFailures)
        {
            return;
        }

        await _distributedCache.SetStringAsync(
            BuildBlockedKey(normalizedUsername),
            "1",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(lockoutSeconds)
            },
            ct);

        await _distributedCache.RemoveAsync(failedKey, ct);
        _logger.LogWarning(
            "Tam khoa dang nhap cho username {Username} sau {FailedCount} lan that bai lien tiep.",
            normalizedUsername,
            failedCount);
    }

    public async Task ClearFailedLoginAttemptsAsync(string username, CancellationToken ct = default)
    {
        var normalizedUsername = NormalizeUsername(username);
        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return;
        }

        await _distributedCache.RemoveAsync(BuildFailedKey(normalizedUsername), ct);
        await _distributedCache.RemoveAsync(BuildBlockedKey(normalizedUsername), ct);
    }

    public Task RecordRefreshTokenMisuseAsync(Guid userId, string username, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "Phat hien refresh token bat thuong cho user {Username} ({UserId}). Session hien tai da bi revoke.",
            username,
            userId);
        return Task.CompletedTask;
    }

    private async Task<int> ReadCountAsync(string key, CancellationToken ct)
    {
        var raw = await _distributedCache.GetStringAsync(key, ct);
        return int.TryParse(raw, out var parsed) ? parsed : 0;
    }

    private static string BuildFailedKey(string username)
        => $"auth:failed:{username}";

    private static string BuildBlockedKey(string username)
        => $"auth:blocked:{username}";

    private static string NormalizeUsername(string username)
        => username.Trim().ToLowerInvariant();

    private int ReadIntSetting(string key, int fallbackValue)
    {
        var raw = _configuration[key];
        return int.TryParse(raw, out var parsed) ? parsed : fallbackValue;
    }
}
