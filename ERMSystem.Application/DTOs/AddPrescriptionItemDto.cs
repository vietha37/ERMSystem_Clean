using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class AddPrescriptionItemDto
    {
        [Required]
        public Guid MedicineId { get; set; }

        [Required]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        public string Duration { get; set; } = string.Empty;
    }
}
