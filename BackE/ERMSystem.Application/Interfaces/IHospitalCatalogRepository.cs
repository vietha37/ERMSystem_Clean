using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;

namespace ERMSystem.Application.Interfaces
{
    public interface IHospitalCatalogRepository
    {
        Task<IReadOnlyList<HospitalDepartmentDto>> GetDepartmentsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<HospitalSpecialtyDto>> GetSpecialtiesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<HospitalClinicDto>> GetClinicsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<HospitalServiceCatalogDto>> GetServicesAsync(CancellationToken ct = default);
    }
}
