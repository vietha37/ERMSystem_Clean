using System;
using System.Collections.Generic;

namespace ERMSystem.Domain.Entities
{
    public class Doctor
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
