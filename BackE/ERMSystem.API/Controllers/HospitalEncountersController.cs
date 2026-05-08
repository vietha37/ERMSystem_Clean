using System.Security.Claims;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-encounters")]
[Authorize(Roles = "Admin,Doctor,Receptionist")]
public class HospitalEncountersController : ControllerBase
{
    private readonly IHospitalEncounterService _hospitalEncounterService;

    public HospitalEncountersController(IHospitalEncounterService hospitalEncounterService)
    {
        _hospitalEncounterService = hospitalEncounterService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorklist(
        [FromQuery] HospitalEncounterWorklistRequestDto request,
        CancellationToken ct)
    {
        var result = await _hospitalEncounterService.GetWorklistAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("eligible-appointments")]
    public async Task<IActionResult> GetEligibleAppointments(CancellationToken ct)
    {
        var result = await _hospitalEncounterService.GetEligibleAppointmentsAsync(ct);
        return Ok(result);
    }

    [HttpGet("{encounterId:guid}")]
    public async Task<IActionResult> GetById(Guid encounterId, CancellationToken ct)
    {
        var result = await _hospitalEncounterService.GetByIdAsync(encounterId, ct);
        if (result == null)
        {
            return NotFound(new { message = "Khong tim thay encounter." });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateHospitalEncounterDto request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _hospitalEncounterService.CreateAsync(
                request,
                ResolveActorUserId(),
                ResolveActorUsername(),
                ct);
            return CreatedAtAction(nameof(GetById), new { encounterId = result.EncounterId }, result);
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

    [HttpPut("{encounterId:guid}")]
    public async Task<IActionResult> Update(
        Guid encounterId,
        [FromBody] UpdateHospitalEncounterDto request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _hospitalEncounterService.UpdateAsync(
                encounterId,
                request,
                ResolveActorUserId(),
                ResolveActorUsername(),
                ct);
            if (result == null)
            {
                return NotFound(new { message = "Khong tim thay encounter can cap nhat." });
            }

            return Ok(result);
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

    private string? ResolveActorUsername()
    {
        return User.FindFirstValue(ClaimTypes.Name)
               ?? User.FindFirstValue(ClaimTypes.Upn)
               ?? User.FindFirstValue("unique_name");
    }
}
