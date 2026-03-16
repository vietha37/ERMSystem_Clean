using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        // GET: api/medicalrecords
        [HttpGet]
        public async Task<IActionResult> GetAllMedicalRecords()
        {
            var records = await _medicalRecordService.GetAllMedicalRecordsAsync();
            return Ok(records);
        }

        // GET: api/medicalrecords/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedicalRecordById(Guid id)
        {
            var record = await _medicalRecordService.GetMedicalRecordByIdAsync(id);
            if (record == null)
                return NotFound($"MedicalRecord with ID {id} not found.");

            return Ok(record);
        }

        // GET: api/medicalrecords/by-appointment/{appointmentId}
        [HttpGet("by-appointment/{appointmentId}")]
        public async Task<IActionResult> GetByAppointmentId(Guid appointmentId)
        {
            var record = await _medicalRecordService.GetMedicalRecordByAppointmentIdAsync(appointmentId);
            if (record == null)
                return NotFound($"No MedicalRecord found for Appointment {appointmentId}.");

            return Ok(record);
        }

        // POST: api/medicalrecords
        [HttpPost]
        public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordDto createMedicalRecordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _medicalRecordService.CreateMedicalRecordAsync(createMedicalRecordDto);
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
        public async Task<IActionResult> UpdateMedicalRecord(Guid id, [FromBody] UpdateMedicalRecordDto updateMedicalRecordDto)
        {
            if (id != updateMedicalRecordDto.Id)
                return BadRequest("MedicalRecord ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _medicalRecordService.UpdateMedicalRecordAsync(updateMedicalRecordDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/medicalrecords/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalRecord(Guid id)
        {
            try
            {
                await _medicalRecordService.DeleteMedicalRecordAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }
    }
}
