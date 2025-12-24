using BudgetOnline.Api.Contracts;
using BudgetOnline.Domain.Entities;
using BudgetOnline.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetOnline.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase    
{
    private readonly ApplicationDbContext _context;

    public TransactionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions() {
        var transactions = await _context.Transactions.ToListAsync();

        var response = transactions.Select(t => new TransactionResponse(
            t.Id,
            t.Amount,
            t.Description,
            t.Date
        ));
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(CreateTransactionRequest request)
    {
        var transaction = new Transaction
        (
            Guid.NewGuid(),
            request.Amount,
            request.Description,
            request.Date,
            Guid.Empty
        );
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var response = new TransactionResponse(
            transaction.Id,
            transaction.Amount,
            transaction.Description,
            transaction.Date
        );

        return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, response);
    }
}
