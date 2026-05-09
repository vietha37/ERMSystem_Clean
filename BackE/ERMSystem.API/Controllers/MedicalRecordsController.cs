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
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        // GET: api/medicalrecords
        [HttpGet]
        [Authorize(Policy = AppPermissions.MedicalRecords.Read)]
        public async Task<IActionResult> GetAllMedicalRecords([FromQuery] PaginationRequest request, CancellationToken ct)
        {
            var result = await _medicalRecordService.GetAllMedicalRecordsAsync(request, ct);
            return Ok(result);
        }

        // GET: api/medicalrecords/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = AppPermissions.MedicalRecords.Read)]
        public async Task<IActionResult> GetMedicalRecordById(Guid id, CancellationToken ct)
        {
            var record = await _medicalRecordService.GetMedicalRecordByIdAsync(id, ct);
            if (record == null)
                return NotFound($"MedicalRecord with ID {id} not found.");
            return Ok(record);
        }

        // GET: api/medicalrecords/by-appointment/{appointmentId}
        [HttpGet("by-appointment/{appointmentId}")]
        [Authorize(Policy = AppPermissions.MedicalRecords.Read)]
        public async Task<IActionResult> GetByAppointmentId(Guid appointmentId, CancellationToken ct)
        {
            var record = await _medicalRecordService.GetMedicalRecordByAppointmentIdAsync(appointmentId, ct);
            if (record == null)
                return NotFound($"No MedicalRecord found for Appointment {appointmentId}.");
            return Ok(record);
        }

        // POST: api/medicalrecords
        [HttpPost]
        [Authorize(Policy = AppPermissions.MedicalRecords.Create)]
        public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordDto createMedicalRecordDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _medicalRecordService.CreateMedicalRecordAsync(createMedicalRecordDto, ct);
                return CreatedAtAction(nameof(GetMedicalRecordById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // PUT: api/medicalrecords/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = AppPermissions.MedicalRecords.Update)]
        public async Task<IActionResult> UpdateMedicalRecord(Guid id, [FromBody] UpdateMedicalRecordDto updateMedicalRecordDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _medicalRecordService.UpdateMedicalRecordAsync(id, updateMedicalRecordDto, ct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/medicalrecords/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPermissions.MedicalRecords.Delete)]
        public async Task<IActionResult> DeleteMedicalRecord(Guid id, CancellationToken ct)
        {
            try
            {
                await _medicalRecordService.DeleteMedicalRecordAsync(id, ct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }
    }
}
