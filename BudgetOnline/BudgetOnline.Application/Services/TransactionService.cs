using BudgetOnline.Application.Abstractions;
using BudgetOnline.Application.Contracts;
using BudgetOnline.Application.Interfaces;
using BudgetOnline.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetOnline.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IApplicationDbContext _context;
    public TransactionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TransactionResponse>> GetAllTransactionsAsync()
    {
        var transactions = await _context.Transactions.Include(t => t.Category).ToListAsync();

        return transactions.Select(t => new TransactionResponse(
            t.Id,
            t.Amount,
            t.Description,
            t.ExpenseDate,
            t.Category.Name
        ));
    }

    public async Task<TransactionResponse?> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Transaction amount must be positive");

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists) return null;


        var transaction = new Transaction
        (
            request.Amount,
            request.CategoryId,
            request.Description,
            request.Date,
            TransactionType.Original,
            Guid.NewGuid()

        );
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(ct);

        var categoryName = await _context.Categories
            .Where(c => c.Id == transaction.CategoryId)
            .Select(c => c.Name)
            .SingleAsync(ct);

        return new TransactionResponse(
            transaction.Id,
            transaction.Amount,
            transaction.Description,
            transaction.ExpenseDate,
            categoryName
        );
    }

    public async Task<bool> VoidTransactionAsync(Guid id, CancellationToken ct)
    {
        var transaction = await _context.Transactions.FindAsync([id], ct);
        if (transaction == null) return false;

        if (transaction.Type != TransactionType.Original)
            throw new InvalidOperationException("Only original transactions can be voided");

        await CheckIfReversedAsync(transaction.CorrelationId, ct);

        var reversal = new Transaction
        (
            transaction.Amount,
            transaction.CategoryId,
            $"Reversal of: {transaction.Description}",
            transaction.ExpenseDate,
            TransactionType.Reversal,
            transaction.CorrelationId
        );

        _context.Transactions.Add(reversal);
        await _context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> ReclassifyTransactionAsync(Guid id, Guid newCategoryId, CancellationToken ct)
    {
        var originalTransaction = await _context.Transactions.FindAsync([id], ct);
        if (originalTransaction == null) return false;

        if (originalTransaction.Type != TransactionType.Original)
            throw new InvalidOperationException("Only original transactions can be reclassified");

        await CheckIfReversedAsync(originalTransaction.CorrelationId, ct);

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == newCategoryId, ct);
        if (!categoryExists) throw new InvalidOperationException("New Category Id does not exist");

        var reversal = new Transaction
        (
            originalTransaction.Amount,
            originalTransaction.CategoryId,
            $"Reversal of: {originalTransaction.Description}",
            originalTransaction.ExpenseDate,
            TransactionType.Reversal,
            originalTransaction.CorrelationId
        );
        var reclassification = new Transaction
        (
            originalTransaction.Amount,
            newCategoryId,
            originalTransaction.Description,
            originalTransaction.ExpenseDate,
            TransactionType.Original,
            originalTransaction.CorrelationId
        );

        _context.Transactions.AddRange(reversal, reclassification); 
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task CheckIfReversedAsync(Guid correlationId, CancellationToken ct)
    {
        var alreadyReversed = await _context.Transactions
            .AnyAsync(t =>
                t.CorrelationId == correlationId &&
                t.Type == TransactionType.Reversal,
                ct);

        if (alreadyReversed)
            throw new InvalidOperationException("This transaction has already been reversed.");
    }

}
