using System;
using System.Collections.Generic;

namespace ERMSystem.Domain.Entities
{
    public class MedicalRecord
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public virtual Appointment Appointment { get; set; } = null!;
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
