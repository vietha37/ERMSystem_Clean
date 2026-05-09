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
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        public MedicinesController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        // GET: api/medicines
        [HttpGet]
        [Authorize(Policy = AppPermissions.Medicines.Read)]
        public async Task<IActionResult> GetAllMedicines([FromQuery] PaginationRequest request, CancellationToken ct)
        {
            var result = await _medicineService.GetAllMedicinesAsync(request, ct);
            return Ok(result);
        }

        // GET: api/medicines/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = AppPermissions.Medicines.Read)]
        public async Task<IActionResult> GetMedicineById(Guid id, CancellationToken ct)
        {
            var medicine = await _medicineService.GetMedicineByIdAsync(id, ct);
            if (medicine == null)
                return NotFound($"Medicine with ID {id} not found.");
            return Ok(medicine);
        }

        // POST: api/medicines
        [HttpPost]
        [Authorize(Policy = AppPermissions.Medicines.Create)]
        public async Task<IActionResult> CreateMedicine([FromBody] CreateMedicineDto createMedicineDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _medicineService.CreateMedicineAsync(createMedicineDto, ct);
            return CreatedAtAction(nameof(GetMedicineById), new { id = created.Id }, created);
        }

        // PUT: api/medicines/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = AppPermissions.Medicines.Update)]
        public async Task<IActionResult> UpdateMedicine(Guid id, [FromBody] UpdateMedicineDto updateMedicineDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _medicineService.UpdateMedicineAsync(id, updateMedicineDto, ct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/medicines/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = AppPermissions.Medicines.Delete)]
        public async Task<IActionResult> DeleteMedicine(Guid id, CancellationToken ct)
        {
            try
            {
                await _medicineService.DeleteMedicineAsync(id, ct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return NoContent();
        }
    }
}
