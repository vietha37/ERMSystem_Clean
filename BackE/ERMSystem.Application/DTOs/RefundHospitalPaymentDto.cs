using System.ComponentModel.DataAnnotations;

namespace ERMSystem.Application.DTOs;

public class RefundHospitalPaymentDto
{
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? PaymentReference { get; set; }

    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(250)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? ExternalTransactionId { get; set; }
}
