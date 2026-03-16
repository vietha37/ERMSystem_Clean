using System;
using System.Collections.Generic;

namespace ERMSystem.Application.DTOs
{
    public class PrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid MedicalRecordId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PrescriptionItemDto> Items { get; set; } = new List<PrescriptionItemDto>();
    }
}
