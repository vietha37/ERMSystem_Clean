using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface INotificationService
    {
        Task<TodayNotificationsDto> GetTodayNotificationsAsync(string? role, string? username, CancellationToken ct = default);
    }
}
