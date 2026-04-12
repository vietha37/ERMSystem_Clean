using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/notifications/today
        [HttpGet("today")]
        public async Task<IActionResult> GetToday(CancellationToken ct)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var username = User.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                           ?? User.FindFirstValue(ClaimTypes.Name)
                           ?? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

            var data = await _notificationService.GetTodayNotificationsAsync(role, username, ct);
            return Ok(data);
        }
    }
}
