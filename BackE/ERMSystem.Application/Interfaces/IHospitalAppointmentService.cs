using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IHospitalAppointmentService
    {
        Task<HospitalAppointmentBookingResultDto> BookPublicAppointmentAsync(
            PublicHospitalAppointmentBookingRequestDto request,
            CancellationToken ct = default);
    }
}
