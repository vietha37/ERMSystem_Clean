namespace ERMSystem.API.Services;

public class ApiErrorResponse
{
    public string ErrorCode { get; set; } = "unexpected_error";
    public string Message { get; set; } = "An unexpected error occurred.";
    public string CorrelationId { get; set; } = string.Empty;
    public object? Details { get; set; }
}
