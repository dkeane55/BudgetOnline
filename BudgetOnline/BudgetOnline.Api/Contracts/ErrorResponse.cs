namespace BudgetOnline.Api.Contracts;

public record ErrorResponse(
    int StatusCode,
    string Message,
    string? Details = null
);
