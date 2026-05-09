namespace ERMSystem.Application.Interfaces;

public interface IAuthSecurityMonitor
{
    Task<bool> IsLoginBlockedAsync(string username, CancellationToken ct = default);
    Task RecordFailedLoginAttemptAsync(string username, CancellationToken ct = default);
    Task ClearFailedLoginAttemptsAsync(string username, CancellationToken ct = default);
    Task RecordRefreshTokenMisuseAsync(Guid userId, string username, CancellationToken ct = default);
}
