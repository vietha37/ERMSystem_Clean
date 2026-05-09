using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERMSystem.Application.Authorization;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // GET: api/doctors
        [HttpGet]
        [Authorize(Policy = AppPermissions.Doctors.Read)]
        public async Task<IActionResult> GetAllDoctors([FromQuery] PaginationRequest request, CancellationToken ct)
        {
            var result = await _doctorService.GetAllDoctorsAsync(request, ct);
            return Ok(result);
        }

        // GET: api/doctors/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = AppPermissions.Doctors.Read)]
        public async Task<IActionResult> GetDoctorById(Guid id, CancellationToken ct)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id, ct);
            if (doctor == null)
                return NotFound($"Doctor with ID {id} not found.");
            return Ok(doctor);
        }

        // POST: api/doctors
        [HttpPost]
        [Authorize(Policy = AppPermissions.Doctors.Create)]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDto createDoctorDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _doctorService.CreateDoctorAsync(createDoctorDto, ct);
            return CreatedAtAction(nameof(GetDoctorById), new { id = created.Id }, created);
        }

        // PUT: api/doctors/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = AppPermissions.Doctors.Update)]
        public async Task<IActionResult> UpdateDoctor(Guid id, [FromBody] UpdateDoctorDto updateDoctorDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _doctorService.UpdateDoctorAsync(id, updateDoctorDto, ct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/doctors/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPermissions.Doctors.Delete)]
        public async Task<IActionResult> DeleteDoctor(Guid id, CancellationToken ct)
        {
            try
            {
                await _doctorService.DeleteDoctorAsync(id, ct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }
    }
}
