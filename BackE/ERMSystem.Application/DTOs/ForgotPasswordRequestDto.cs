using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs;

public class ForgotPasswordRequestDto
{
    [Required]
    [MaxLength(150)]
    public string Username { get; set; } = string.Empty;
}
