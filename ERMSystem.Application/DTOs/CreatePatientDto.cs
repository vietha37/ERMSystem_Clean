using System;
using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class CreatePatientDto
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
    }
}
