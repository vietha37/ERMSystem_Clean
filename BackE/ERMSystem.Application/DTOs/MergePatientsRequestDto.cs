using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class MergePatientsRequestDto
    {
        [Required]
        public Guid SourcePatientId { get; set; }

        [Required]
        public Guid TargetPatientId { get; set; }
    }
}
