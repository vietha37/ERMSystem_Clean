using System;
using System.Collections.Generic;

namespace ERMSystem.Domain.Entities
{
    public class Patient
    {
        public Guid Id { get; set; }
        public Guid? AppUserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
