using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-notification-deliveries")]
[Authorize(Roles = "Admin,Receptionist")]
public class HospitalNotificationDeliveriesController : ControllerBase
{
    private readonly IHospitalNotificationDeliveryService _service;

    public HospitalNotificationDeliveriesController(IHospitalNotificationDeliveryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetDeliveries(
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _service.GetDeliveriesAsync(status, pageNumber, pageSize, ct);
        return Ok(result);
    }

    [HttpPost("{deliveryId:guid}/retry")]
    public async Task<IActionResult> RetryDelivery(Guid deliveryId, CancellationToken ct)
    {
        var retried = await _service.RetryDeliveryAsync(deliveryId, ct);
        if (!retried)
        {
            return NotFound(new { message = "Khong tim thay delivery can retry." });
        }

        return Ok(new { message = "Da dua delivery ve trang thai Queued." });
    }
}
