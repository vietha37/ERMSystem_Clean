using System;
using System.Collections.Generic;

namespace ERMSystem.Application.DTOs
{
    public class PotentialDuplicatePatientDto
    {
        public PatientDto Patient { get; set; } = new PatientDto();
        public IReadOnlyCollection<string> MatchReasons { get; set; } = Array.Empty<string>();
    }
}
