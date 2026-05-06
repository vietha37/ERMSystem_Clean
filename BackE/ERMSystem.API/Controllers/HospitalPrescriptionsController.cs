using System.Security.Claims;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-prescriptions")]
[Authorize(Roles = "Admin,Doctor,Receptionist")]
public class HospitalPrescriptionsController : ControllerBase
{
    private readonly IHospitalPrescriptionService _hospitalPrescriptionService;

    public HospitalPrescriptionsController(IHospitalPrescriptionService hospitalPrescriptionService)
    {
        _hospitalPrescriptionService = hospitalPrescriptionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorklist(
        [FromQuery] HospitalPrescriptionWorklistRequestDto request,
        CancellationToken ct)
    {
        var result = await _hospitalPrescriptionService.GetWorklistAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("eligible-encounters")]
    public async Task<IActionResult> GetEligibleEncounters(CancellationToken ct)
    {
        var result = await _hospitalPrescriptionService.GetEligibleEncountersAsync(ct);
        return Ok(result);
    }

    [HttpGet("medicine-catalog")]
    public async Task<IActionResult> GetMedicineCatalog(CancellationToken ct)
    {
        var result = await _hospitalPrescriptionService.GetMedicineCatalogAsync(ct);
        return Ok(result);
    }

    [HttpGet("{prescriptionId:guid}")]
    public async Task<IActionResult> GetById(Guid prescriptionId, CancellationToken ct)
    {
        var result = await _hospitalPrescriptionService.GetByIdAsync(prescriptionId, ct);
        if (result == null)
        {
            return NotFound(new { message = "Khong tim thay don thuoc." });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateHospitalPrescriptionDto request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _hospitalPrescriptionService.CreateAsync(request, ResolveActorUserId(), ct);
            return CreatedAtAction(nameof(GetById), new { prescriptionId = result.PrescriptionId }, result);
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

    [HttpDelete("{prescriptionId:guid}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Delete(Guid prescriptionId, CancellationToken ct)
    {
        try
        {
            await _hospitalPrescriptionService.DeleteAsync(prescriptionId, ct);
            return NoContent();
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

    private Guid? ResolveActorUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(ClaimTypes.Name)
                     ?? User.FindFirstValue("sub");

        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }
}
