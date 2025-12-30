using BudgetOnline.Application.Contracts;

namespace BudgetOnline.Application.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionResponse>> GetAllTransactionsAsync();
    Task<TransactionResponse?> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken ct);
    Task<bool> VoidTransactionAsync(Guid id, CancellationToken ct);
    Task<bool> ReclassifyTransactionAsync(Guid id, Guid newCategoryId, CancellationToken ct);
}
