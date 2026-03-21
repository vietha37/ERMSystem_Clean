using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.DTOs.Common;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IPrescriptionItemService _prescriptionItemService;

        public PrescriptionsController(
            IPrescriptionService prescriptionService,
            IPrescriptionItemService prescriptionItemService)
        {
            _prescriptionService = prescriptionService;
            _prescriptionItemService = prescriptionItemService;
        }

        // GET: api/prescriptions
        [HttpGet]
        public async Task<IActionResult> GetAllPrescriptions([FromQuery] PaginationRequest request, CancellationToken ct)
        {
            var result = await _prescriptionService.GetAllPrescriptionsAsync(request, ct);
            return Ok(result);
        }

        // GET: api/prescriptions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescriptionById(Guid id, CancellationToken ct)
        {
            var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id, ct);
            if (prescription == null)
                return NotFound($"Prescription with ID {id} not found.");
            return Ok(prescription);
        }

        // GET: api/prescriptions/by-medicalrecord/{medicalRecordId}
        [HttpGet("by-medicalrecord/{medicalRecordId}")]
        public async Task<IActionResult> GetByMedicalRecord(Guid medicalRecordId, CancellationToken ct)
        {
            var prescription = await _prescriptionService.GetPrescriptionByMedicalRecordIdAsync(medicalRecordId, ct);
            if (prescription == null)
                return NotFound($"No Prescription found for MedicalRecord {medicalRecordId}.");
            return Ok(prescription);
        }

        // POST: api/prescriptions
        [HttpPost]
        public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionDto createPrescriptionDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _prescriptionService.CreatePrescriptionAsync(createPrescriptionDto, ct);
                return CreatedAtAction(nameof(GetPrescriptionById), new { id = created.Id }, created);
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

        // DELETE: api/prescriptions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(Guid id, CancellationToken ct)
        {
            try
            {
                await _prescriptionService.DeletePrescriptionAsync(id, ct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        // POST: api/prescriptions/{id}/items
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(Guid id, [FromBody] AddPrescriptionItemDto addPrescriptionItemDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _prescriptionItemService.AddItemToPrescriptionAsync(id, addPrescriptionItemDto, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // DELETE: api/prescriptions/{id}/items/{itemId}
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> RemoveItem(Guid id, Guid itemId, CancellationToken ct)
        {
            try
            {
                var updated = await _prescriptionItemService.RemoveItemFromPrescriptionAsync(id, itemId, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
