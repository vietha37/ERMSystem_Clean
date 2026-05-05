using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-catalog")]
[AllowAnonymous]
public class HospitalCatalogController : ControllerBase
{
    private readonly IHospitalCatalogService _hospitalCatalogService;

    public HospitalCatalogController(IHospitalCatalogService hospitalCatalogService)
    {
        _hospitalCatalogService = hospitalCatalogService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var result = await _hospitalCatalogService.GetOverviewAsync(ct);
        return Ok(result);
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
    {
        var result = await _hospitalCatalogService.GetDepartmentsAsync(ct);
        return Ok(result);
    }

    [HttpGet("specialties")]
    public async Task<IActionResult> GetSpecialties(CancellationToken ct)
    {
        var result = await _hospitalCatalogService.GetSpecialtiesAsync(ct);
        return Ok(result);
    }

    [HttpGet("clinics")]
    public async Task<IActionResult> GetClinics(CancellationToken ct)
    {
        var result = await _hospitalCatalogService.GetClinicsAsync(ct);
        return Ok(result);
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServices(CancellationToken ct)
    {
        var result = await _hospitalCatalogService.GetServicesAsync(ct);
        return Ok(result);
    }
}
