using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IHospitalDoctorRepository
    {
        Task<IReadOnlyList<HospitalDoctorDto>> GetDoctorsAsync(Guid? specialtyId = null, CancellationToken ct = default);
        Task<HospitalDoctorDto?> GetDoctorByIdAsync(Guid doctorProfileId, CancellationToken ct = default);
    }
}
