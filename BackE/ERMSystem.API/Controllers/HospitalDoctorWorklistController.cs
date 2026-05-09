using System.Security.Claims;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Authorization;
using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-doctor-worklist")]
[Authorize(Policy = AppPermissions.HospitalDoctorWorklist.Read)]
public class HospitalDoctorWorklistController : ControllerBase
{
    private readonly IHospitalDoctorWorklistService _hospitalDoctorWorklistService;

    public HospitalDoctorWorklistController(IHospitalDoctorWorklistService hospitalDoctorWorklistService)
    {
        _hospitalDoctorWorklistService = hospitalDoctorWorklistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorklist(
        [FromQuery] HospitalDoctorWorklistRequestDto request,
        CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var username = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.Upn)
            ?? User.FindFirstValue("unique_name");

        var result = await _hospitalDoctorWorklistService.GetWorklistAsync(request, role, username, ct);
        return Ok(result);
    }
}
