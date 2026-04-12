using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;

namespace ERMSystem.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public NotificationService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<TodayNotificationsDto> GetTodayNotificationsAsync(
            string? role,
            string? username,
            CancellationToken ct = default)
        {
            var startOfDay = DateTime.UtcNow.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var appointments = await _appointmentRepository.GetByDateRangeAsync(startOfDay, endOfDay, ct);

            var filtered = appointments.AsEnumerable();
            if (string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var normalizedUsername = NormalizeText(username);
                filtered = appointments.Where(a => IsDoctorMatched(a.Doctor?.FullName, normalizedUsername));
            }

            var notifications = filtered
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentNotificationDto
                {
                    AppointmentId = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    PatientName = a.Patient?.FullName ?? "Unknown patient",
                    DoctorName = a.Doctor?.FullName ?? "Unknown doctor",
                    Status = a.Status,
                    Message = $"Lich kham {a.AppointmentDate:HH:mm} - {a.Patient?.FullName ?? "Unknown patient"} ({a.Status})"
                })
                .ToList();

            return new TodayNotificationsDto
            {
                UnreadCount = notifications.Count,
                Notifications = notifications
            };
        }

        private static bool IsDoctorMatched(string? doctorFullName, string normalizedUsername)
        {
            if (string.IsNullOrWhiteSpace(doctorFullName) || string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return false;
            }

            var doctorName = NormalizeText(doctorFullName);
            var compactDoctorName = RemoveSpaces(doctorName);
            var compactUsername = RemoveSpaces(normalizedUsername);

            // Support common naming styles:
            // - username == full name
            // - username == short name without prefixes/titles
            return doctorName == normalizedUsername
                || doctorName.Contains(normalizedUsername, StringComparison.Ordinal)
                || normalizedUsername.Contains(doctorName, StringComparison.Ordinal)
                || compactDoctorName == compactUsername
                || compactDoctorName.Contains(compactUsername, StringComparison.Ordinal)
                || compactUsername.Contains(compactDoctorName, StringComparison.Ordinal);
        }

        private static string NormalizeText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.Trim().ToLowerInvariant();
            var decomposed = normalized.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var ch in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
                    {
                        builder.Append(ch);
                    }
                }
            }

            return builder
                .ToString()
                .Replace("bac si", string.Empty, StringComparison.Ordinal)
                .Replace("dr", string.Empty, StringComparison.Ordinal)
                .Trim();
        }

        private static string RemoveSpaces(string value)
            => value.Replace(" ", string.Empty, StringComparison.Ordinal);
    }
}
