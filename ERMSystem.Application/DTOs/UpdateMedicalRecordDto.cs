using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class UpdateMedicalRecordDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Symptoms { get; set; } = string.Empty;

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}
