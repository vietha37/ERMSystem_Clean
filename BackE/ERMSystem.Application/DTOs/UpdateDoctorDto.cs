using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class UpdateDoctorDto
    {
        [Required]
        [MinLength(2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        public string Specialty { get; set; } = string.Empty;
    }
}
