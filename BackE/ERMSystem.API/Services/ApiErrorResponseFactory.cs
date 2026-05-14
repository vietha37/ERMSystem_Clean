using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ERMSystem.API.Services;

public static class ApiErrorResponseFactory
{
    public static ApiErrorResponse Create(
        HttpContext context,
        string errorCode,
        string message,
        object? details = null)
    {
        return new ApiErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            CorrelationId = context.TraceIdentifier,
            Details = details
        };
    }

    public static object BuildValidationDetails(ModelStateDictionary modelState)
    {
        return modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error =>
                        string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "Invalid value."
                            : error.ErrorMessage)
                    .ToArray());
    }
}
