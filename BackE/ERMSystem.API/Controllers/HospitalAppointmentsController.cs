using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-appointments")]
[AllowAnonymous]
public class HospitalAppointmentsController : ControllerBase
{
    private readonly IHospitalAppointmentService _hospitalAppointmentService;

    public HospitalAppointmentsController(IHospitalAppointmentService hospitalAppointmentService)
    {
        _hospitalAppointmentService = hospitalAppointmentService;
    }

    [HttpPost("public-booking")]
    public async Task<IActionResult> BookPublicAppointment(
        [FromBody] PublicHospitalAppointmentBookingRequestDto request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _hospitalAppointmentService.BookPublicAppointmentAsync(request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
