using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: api/dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats(CancellationToken ct)
        {
            var stats = await _dashboardService.GetDashboardStatsAsync(ct);
            return Ok(stats);
        }

        // GET: api/dashboard/trends?period=daily|monthly&fromDate=2026-03-01&toDate=2026-03-31
        [HttpGet("trends")]
        public async Task<IActionResult> GetDashboardTrends(
            [FromQuery] string period = "daily",
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken ct = default)
        {
            var trends = await _dashboardService.GetDashboardTrendsAsync(period, fromDate, toDate, ct);
            return Ok(trends);
        }
    }
}
