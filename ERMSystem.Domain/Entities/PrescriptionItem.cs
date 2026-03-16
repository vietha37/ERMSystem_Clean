using System;

namespace ERMSystem.Domain.Entities
{
    public class PrescriptionItem
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public Guid MedicineId { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;

        public virtual Prescription Prescription { get; set; } = null!;
        public virtual Medicine Medicine { get; set; } = null!;
    }
}
