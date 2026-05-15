using System;

namespace ERMSystem.Application.DTOs
{
    public class MergePatientsResultDto
    {
        public Guid SourcePatientId { get; set; }
        public Guid TargetPatientId { get; set; }
        public int ReassignedAppointmentCount { get; set; }
        public bool AppUserLinkMoved { get; set; }
    }
}
