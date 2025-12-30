using BudgetOnline.Application.Abstractions;
using BudgetOnline.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetOnline.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Transaction>()
        .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted);

        if (entries.Any())
        {
            throw new InvalidOperationException(
                "Transactions are immutable. Modification or deletion is not permitted.");
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.CorrelationId);

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.CategoryId);

        var foodId = Guid.NewGuid();
        var rentId = Guid.NewGuid();

        modelBuilder.Entity<Category>().HasData(
            new Category(foodId, "Food", 100),
            new Category(rentId, "Rent", 500)
        );
    }
}
