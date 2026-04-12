using System;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
        Task<DashboardTrendsDto> GetDashboardTrendsAsync(
            string period,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken ct = default);
    }
}
