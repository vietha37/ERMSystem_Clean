using ERMSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERMSystem.API.Controllers;

[ApiController]
[Route("api/hospital-doctors")]
[AllowAnonymous]
public class HospitalDoctorsController : ControllerBase
{
    private readonly IHospitalDoctorService _hospitalDoctorService;

    public HospitalDoctorsController(IHospitalDoctorService hospitalDoctorService)
    {
        _hospitalDoctorService = hospitalDoctorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDoctors([FromQuery] Guid? specialtyId, CancellationToken ct)
    {
        var result = await _hospitalDoctorService.GetDoctorsAsync(specialtyId, ct);
        return Ok(result);
    }

    [HttpGet("{doctorProfileId:guid}")]
    public async Task<IActionResult> GetDoctorById(Guid doctorProfileId, CancellationToken ct)
    {
        var doctor = await _hospitalDoctorService.GetDoctorByIdAsync(doctorProfileId, ct);
        if (doctor == null)
        {
            return NotFound("Khong tim thay bac si.");
        }

        return Ok(doctor);
    }
}
