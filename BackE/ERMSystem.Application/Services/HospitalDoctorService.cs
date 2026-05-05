using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.Application.Services
{
    public class HospitalDoctorService : IHospitalDoctorService
    {
        private readonly IHospitalDoctorRepository _hospitalDoctorRepository;

        public HospitalDoctorService(IHospitalDoctorRepository hospitalDoctorRepository)
        {
            _hospitalDoctorRepository = hospitalDoctorRepository;
        }

        public Task<IReadOnlyList<HospitalDoctorDto>> GetDoctorsAsync(Guid? specialtyId = null, CancellationToken ct = default)
            => _hospitalDoctorRepository.GetDoctorsAsync(specialtyId, ct);

        public Task<HospitalDoctorDto?> GetDoctorByIdAsync(Guid doctorProfileId, CancellationToken ct = default)
            => _hospitalDoctorRepository.GetDoctorByIdAsync(doctorProfileId, ct);
    }
}
