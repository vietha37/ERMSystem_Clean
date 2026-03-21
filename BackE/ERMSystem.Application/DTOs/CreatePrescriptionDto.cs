using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class CreatePrescriptionDto
    {
        [Required]
        public Guid MedicalRecordId { get; set; }
    }
}
