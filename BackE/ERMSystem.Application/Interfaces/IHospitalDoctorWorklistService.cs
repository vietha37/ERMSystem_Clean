using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces;

public interface IHospitalDoctorWorklistService
{
    Task<HospitalDoctorWorklistResponseDto> GetWorklistAsync(
        HospitalDoctorWorklistRequestDto request,
        string currentRole,
        string? currentUsername,
        CancellationToken ct = default);
}
