using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllAsync(CancellationToken ct = default);
        Task<(IEnumerable<Patient> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? textSearch = null,
            CancellationToken ct = default);
        Task<int> GetTotalCountAsync(CancellationToken ct = default);
        Task<Dictionary<DateTime, int>> GetCreatedCountByDayAsync(DateTime fromUtc, CancellationToken ct = default);
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Patient?> GetByAppUserIdAsync(Guid appUserId, CancellationToken ct = default);
        Task<int> GetAppointmentCountAsync(Guid patientId, CancellationToken ct = default);
        Task<IReadOnlyCollection<Patient>> GetPotentialDuplicateCandidatesAsync(
            Guid patientId,
            string fullName,
            DateTime dateOfBirth,
            string phone,
            string? emergencyContactPhone,
            CancellationToken ct = default);
        Task<int> ReassignAppointmentsAsync(Guid sourcePatientId, Guid targetPatientId, CancellationToken ct = default);
        Task AddAsync(Patient patient, CancellationToken ct = default);
        Task UpdateAsync(Patient patient, CancellationToken ct = default);
        Task MergeAsync(Patient sourcePatient, Patient targetPatient, CancellationToken ct = default);
        Task DeleteAsync(Patient patient, CancellationToken ct = default);
    }
}
