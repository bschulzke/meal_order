using MealOrder.Api.Data;
using MealOrder.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealOrder.Api.Controllers;

public record CreateTaxRequest(string Name, decimal Percentage);
public record UpdateTaxRequest(string Name, decimal Percentage);

[ApiController]
[Route("api/[controller]")]
public class TaxesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var taxes = await db.Taxes
            .Where(t => t.IsActive)
            .Select(t => new { t.Id, t.Name, t.Percentage })
            .ToListAsync();

        return Ok(taxes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tax = await db.Taxes.FindAsync(id);
        if (tax is null)
            return NotFound();

        return Ok(new { tax.Id, tax.Name, tax.Percentage, tax.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaxRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Percentage <= 0 || request.Percentage > 100)
            return BadRequest("Percentage must be between 0 and 100.");

        var tax = new Tax
        {
            Name = request.Name.Trim(),
            Percentage = request.Percentage,
        };

        db.Taxes.Add(tax);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tax.Id }, new { tax.Id, tax.Name, tax.Percentage });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaxRequest request)
    {
        var tax = await db.Taxes.FindAsync(id);
        if (tax is null || !tax.IsActive)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Percentage <= 0 || request.Percentage > 100)
            return BadRequest("Percentage must be between 0 and 100.");

        tax.Name = request.Name.Trim();
        tax.Percentage = request.Percentage;
        await db.SaveChangesAsync();

        return Ok(new { tax.Id, tax.Name, tax.Percentage });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tax = await db.Taxes.FindAsync(id);
        if (tax is null || !tax.IsActive)
            return NotFound();

        tax.IsActive = false;
        await db.SaveChangesAsync();

        return NoContent();
    }
}
