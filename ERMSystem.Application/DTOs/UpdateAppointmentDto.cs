using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class UpdateAppointmentDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [RegularExpression("^(Pending|Completed|Cancelled)$",
            ErrorMessage = "Status must be Pending, Completed, or Cancelled.")]
        public string Status { get; set; } = string.Empty;
    }
}
