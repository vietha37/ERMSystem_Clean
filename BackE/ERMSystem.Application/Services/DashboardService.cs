using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public DashboardService(
            IPatientRepository patientRepository,
            IAppointmentRepository appointmentRepository,
            IMedicalRecordRepository medicalRecordRepository)
        {
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
            _medicalRecordRepository = medicalRecordRepository;
        }

        // public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
        // {
        //     var patientsTask = _patientRepository.GetTotalCountAsync(ct);
        //     var todayAppointmentsTask = _appointmentRepository.GetAppointmentsTodayCountAsync(ct);
        //     var completedAppointmentsTask = _appointmentRepository.GetCompletedAppointmentsCountAsync(ct);
        //     var topDiagnosesTask = _medicalRecordRepository.GetTopDiagnosesAsync(5, ct);

        //     await Task.WhenAll(
        //         patientsTask,
        //         todayAppointmentsTask,
        //         completedAppointmentsTask,
        //         topDiagnosesTask
        //     );

        //     return new DashboardStatsDto
        //     {
        //         TotalPatients = await patientsTask,
        //         AppointmentsToday = await todayAppointmentsTask,
        //         CompletedAppointments = await completedAppointmentsTask,
        //         TopDiagnoses = await topDiagnosesTask
        //     };
        // }
        public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
{
    // Chúng ta không khởi tạo Task trước nữa mà await trực tiếp từng dòng
    // Điều này đảm bảo mỗi truy vấn kết thúc hoàn toàn trước khi truy vấn tiếp theo bắt đầu
    
    var patientsCount = await _patientRepository.GetTotalCountAsync(ct);
    
    var todayAppointmentsCount = await _appointmentRepository.GetAppointmentsTodayCountAsync(ct);
    
    var completedAppointmentsCount = await _appointmentRepository.GetCompletedAppointmentsCountAsync(ct);
    
    var topDiagnoses = await _medicalRecordRepository.GetTopDiagnosesAsync(5, ct);

    return new DashboardStatsDto 
    {
        TotalPatients = patientsCount,
        AppointmentsToday = todayAppointmentsCount,
        CompletedAppointments = completedAppointmentsCount,
        TopDiagnoses = topDiagnoses
    };
}
    }
}
