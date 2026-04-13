using MealOrder.Api.Data;
using MealOrder.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealOrder.Api.Controllers;

public record CreateDiscountRequest(string Name, string Type, decimal Amount);
public record UpdateDiscountRequest(string Name, string Type, decimal Amount);

[ApiController]
[Route("api/[controller]")]
public class DiscountsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var discounts = await db.Discounts
            .Where(d => d.IsActive)
            .Select(d => new { d.Id, d.Name, Type = d.Type.ToString().ToLower(), d.Amount })
            .ToListAsync();

        return Ok(discounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var discount = await db.Discounts.FindAsync(id);
        if (discount is null)
            return NotFound();

        return Ok(new { discount.Id, discount.Name, Type = discount.Type.ToString().ToLower(), discount.Amount, discount.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDiscountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (!Enum.TryParse<DiscountType>(request.Type, ignoreCase: true, out var type))
            return BadRequest("Type must be 'fixed' or 'percent'.");

        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        if (type == DiscountType.Percent && request.Amount > 100)
            return BadRequest("Percent discount cannot exceed 100.");

        var discount = new Discount
        {
            Name = request.Name.Trim(),
            Type = type,
            Amount = request.Amount,
        };

        db.Discounts.Add(discount);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = discount.Id }, new { discount.Id, discount.Name, Type = discount.Type.ToString().ToLower(), discount.Amount });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDiscountRequest request)
    {
        var discount = await db.Discounts.FindAsync(id);
        if (discount is null || !discount.IsActive)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (!Enum.TryParse<DiscountType>(request.Type, ignoreCase: true, out var type))
            return BadRequest("Type must be 'fixed' or 'percent'.");

        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        if (type == DiscountType.Percent && request.Amount > 100)
            return BadRequest("Percent discount cannot exceed 100.");

        discount.Name = request.Name.Trim();
        discount.Type = type;
        discount.Amount = request.Amount;
        await db.SaveChangesAsync();

        return Ok(new { discount.Id, discount.Name, Type = discount.Type.ToString().ToLower(), discount.Amount });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var discount = await db.Discounts.FindAsync(id);
        if (discount is null || !discount.IsActive)
            return NotFound();

        discount.IsActive = false;
        await db.SaveChangesAsync();

        return NoContent();
    }
}
