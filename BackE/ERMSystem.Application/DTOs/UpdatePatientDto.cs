using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class UpdatePatientDto
    {
        [Required]
        [MinLength(2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other.")]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? EmergencyContactName { get; set; }

        [Phone]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(100)]
        public string? EmergencyContactRelationship { get; set; }
    }
}
