using System.Security.Claims;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-billing")]
[Authorize(Roles = "Admin,Receptionist")]
public class HospitalBillingController : ControllerBase
{
    private readonly IHospitalBillingService _hospitalBillingService;

    public HospitalBillingController(IHospitalBillingService hospitalBillingService)
    {
        _hospitalBillingService = hospitalBillingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorklist([FromQuery] HospitalInvoiceWorklistRequestDto request, CancellationToken ct)
    {
        var result = await _hospitalBillingService.GetWorklistAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("eligible-encounters")]
    public async Task<IActionResult> GetEligibleEncounters(CancellationToken ct)
    {
        var result = await _hospitalBillingService.GetEligibleEncountersAsync(ct);
        return Ok(result);
    }

    [HttpGet("{invoiceId:guid}")]
    public async Task<IActionResult> GetById(Guid invoiceId, CancellationToken ct)
    {
        var result = await _hospitalBillingService.GetByIdAsync(invoiceId, ct);
        if (result == null)
        {
            return NotFound(new { message = "Khong tim thay hoa don." });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateHospitalInvoiceDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _hospitalBillingService.CreateInvoiceAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { invoiceId = result.InvoiceId }, result);
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

    [HttpPost("{invoiceId:guid}/payments")]
    public async Task<IActionResult> ReceivePayment(Guid invoiceId, [FromBody] ReceiveHospitalPaymentDto request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _hospitalBillingService.ReceivePaymentAsync(
                invoiceId,
                request,
                ResolveActorUserId(),
                ResolveActorUsername(),
                ct);
            if (result == null)
            {
                return NotFound(new { message = "Khong tim thay hoa don." });
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
        => User.FindFirstValue(ClaimTypes.Name)
           ?? User.FindFirstValue(ClaimTypes.Upn)
           ?? User.FindFirstValue("unique_name");
}
