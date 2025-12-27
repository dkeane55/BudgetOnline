using BudgetOnline.Api.Contracts;
using BudgetOnline.Domain.Entities;
using BudgetOnline.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetOnline.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await context.Categories.ToListAsync();

        return Ok(categories);
    }

    [HttpPost]  
    public async Task<ActionResult<Category>> CreateCategory(CreateCategoryRequest request)
    {
        var exists = await context.Categories.AnyAsync(c => c.Name.ToLower() == request.Name.ToLower());
        if (exists) return BadRequest("A category with this name already exists");
        
        var category = new Category
        (
            Guid.NewGuid(),
            request.Name
        );
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }
}
