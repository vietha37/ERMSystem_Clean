using System;
using System.Collections.Generic;

namespace ERMSystem.Domain.Entities
{
    public class Prescription
    {
        public Guid Id { get; set; }
        public Guid MedicalRecordId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual MedicalRecord MedicalRecord { get; set; } = null!;
        public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}
