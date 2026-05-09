using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs;

public class ResetPasswordRequestDto
{
    [Required]
    [MaxLength(150)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ResetToken { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
}
