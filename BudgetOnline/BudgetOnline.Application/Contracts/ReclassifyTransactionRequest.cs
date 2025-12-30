namespace BudgetOnline.Application.Contracts;

public record ReclassifyTransactionRequest(
    Guid NewCategoryId
);
