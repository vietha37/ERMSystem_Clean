using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs
{
    public class UpdateMedicineDto
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
