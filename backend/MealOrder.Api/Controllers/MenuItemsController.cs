using MealOrder.Api.Data;
using MealOrder.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealOrder.Api.Controllers;

public record CreateMenuItemRequest(string Name, decimal Price);
public record UpdateMenuItemRequest(string Name, decimal Price);

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await db.MenuItems
            .Where(m => m.IsActive)
            .Select(m => new { m.Id, m.Name, m.Price })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await db.MenuItems.FindAsync(id);
        if (item is null)
            return NotFound();

        return Ok(new { item.Id, item.Name, item.Price, item.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Price <= 0)
            return BadRequest("Price must be greater than zero.");

        if (await db.MenuItems.AnyAsync(m => m.Name == request.Name && m.IsActive))
            return Conflict("An active menu item with this name already exists.");

        var item = new MenuItem
        {
            Name = request.Name.Trim(),
            Price = request.Price,
        };

        db.MenuItems.Add(item);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, new { item.Id, item.Name, item.Price });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuItemRequest request)
    {
        var item = await db.MenuItems.FindAsync(id);
        if (item is null || !item.IsActive)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Price <= 0)
            return BadRequest("Price must be greater than zero.");

        if (await db.MenuItems.AnyAsync(m => m.Name == request.Name && m.IsActive && m.Id != id))
            return Conflict("An active menu item with this name already exists.");

        item.Name = request.Name.Trim();
        item.Price = request.Price;
        await db.SaveChangesAsync();

        return Ok(new { item.Id, item.Name, item.Price });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.MenuItems.FindAsync(id);
        if (item is null || !item.IsActive)
            return NotFound();

        item.IsActive = false;
        await db.SaveChangesAsync();

        return NoContent();
    }
}
