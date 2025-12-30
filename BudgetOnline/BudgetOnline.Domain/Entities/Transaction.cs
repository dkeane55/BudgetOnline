namespace BudgetOnline.Domain.Entities;

public enum TransactionType
{
    Original = 1,
    Reversal = 2
}

public class Transaction
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime ExpenseDate { get; private set; }
    public Category Category { get; private set; } = null!;

    public TransactionType Type { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Transaction() { }
    public Transaction(decimal amount, Guid categoryId, string description, DateTime expenseDate, 
            TransactionType type, Guid correlationId)
    {
        if(amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Transaction must belong to a valid category.", nameof(categoryId));
        Id = Guid.NewGuid();
        Amount = amount;
        CategoryId = categoryId;
        Description = description;
        ExpenseDate = expenseDate;
        Type = type;
        CorrelationId = correlationId;
        CreatedAt = DateTime.UtcNow;
    }
}
