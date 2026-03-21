using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class CreateAppointmentDto
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [RegularExpression("^(Pending|Completed|Cancelled)$",
            ErrorMessage = "Status must be Pending, Completed, or Cancelled.")]
        public string Status { get; set; } = "Pending";
    }
}
