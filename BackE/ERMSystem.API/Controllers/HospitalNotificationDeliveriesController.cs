using ERMSystem.Application.Interfaces;
using ERMSystem.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-notification-deliveries")]
[Authorize]
public class HospitalNotificationDeliveriesController : ControllerBase
{
    private readonly IHospitalNotificationDeliveryService _service;

    public HospitalNotificationDeliveriesController(IHospitalNotificationDeliveryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = AppPermissions.HospitalNotifications.Read)]
    public async Task<IActionResult> GetDeliveries(
        [FromQuery] string? status,
        [FromQuery] string? channelCode,
        [FromQuery] string? recipient,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _service.GetDeliveriesAsync(status, channelCode, recipient, pageNumber, pageSize, ct);
        return Ok(result);
    }

    [HttpPost("{deliveryId:guid}/retry")]
    [Authorize(Policy = AppPermissions.HospitalNotifications.Retry)]
    public async Task<IActionResult> RetryDelivery(Guid deliveryId, CancellationToken ct)
    {
        var retried = await _service.RetryDeliveryAsync(deliveryId, ct);
        if (retried == Application.DTOs.NotificationDeliveryRetryResult.NotFound)
        {
            return NotFound(new { message = "Khong tim thay delivery can retry." });
        }

        if (retried == Application.DTOs.NotificationDeliveryRetryResult.InvalidStatus)
        {
            return Conflict(new { message = "Chi duoc retry delivery dang Failed hoac Skipped." });
        }

        return Ok(new { message = "Da dua delivery ve trang thai Queued." });
    }
}
