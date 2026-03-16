using System;

namespace ERMSystem.Application.DTOs
{
    public class PrescriptionItemDto
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public Guid MedicineId { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }
}
