using MealOrder.Api.Data;
using MealOrder.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealOrder.Api.Controllers;

public record OrderItemRequest(int MenuItemId, int Quantity);
public record CreateOrderRequest(int UserId, List<OrderItemRequest> Items, List<int> DiscountIds, List<int> TaxIds);
public record UpdateOrderRequest(List<OrderItemRequest> Items, List<int> DiscountIds, List<int> TaxIds);

[ApiController]
[Route("api/[controller]")]
public class OrdersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Discounts).ThenInclude(d => d.Discount)
            .Include(o => o.Taxes).ThenInclude(t => t.Tax)
            .Select(o => FormatOrder(o))
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Discounts).ThenInclude(d => d.Discount)
            .Include(o => o.Taxes).ThenInclude(t => t.Tax)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        return Ok(FormatOrder(order));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        if (!await db.Users.AnyAsync(u => u.Id == request.UserId))
            return BadRequest("User not found.");

        if (request.Items is null || request.Items.Count == 0)
            return BadRequest("Order must have at least one item.");

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return BadRequest($"Quantity must be greater than zero for menu item {item.MenuItemId}.");
        }

        var menuItemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var menuItems = await db.MenuItems
            .Where(m => menuItemIds.Contains(m.Id) && m.IsActive)
            .ToDictionaryAsync(m => m.Id);

        var missingItems = menuItemIds.Except(menuItems.Keys).ToList();
        if (missingItems.Count > 0)
            return BadRequest($"Menu items not found or inactive: {string.Join(", ", missingItems)}");

        var discounts = new Dictionary<int, Discount>();
        if (request.DiscountIds?.Count > 0)
        {
            var discountIds = request.DiscountIds.Distinct().ToList();
            discounts = await db.Discounts
                .Where(d => discountIds.Contains(d.Id) && d.IsActive)
                .ToDictionaryAsync(d => d.Id);

            var missingDiscounts = discountIds.Except(discounts.Keys).ToList();
            if (missingDiscounts.Count > 0)
                return BadRequest($"Discounts not found or inactive: {string.Join(", ", missingDiscounts)}");
        }

        var taxes = new Dictionary<int, Tax>();
        if (request.TaxIds?.Count > 0)
        {
            var taxIds = request.TaxIds.Distinct().ToList();
            taxes = await db.Taxes
                .Where(t => taxIds.Contains(t.Id) && t.IsActive)
                .ToDictionaryAsync(t => t.Id);

            var missingTaxes = taxIds.Except(taxes.Keys).ToList();
            if (missingTaxes.Count > 0)
                return BadRequest($"Taxes not found or inactive: {string.Join(", ", missingTaxes)}");
        }

        var order = new Order
        {
            UserId = request.UserId,
            Items = request.Items.Select(i => new OrderItem
            {
                MenuItemId = i.MenuItemId,
                Quantity = i.Quantity,
                UnitPrice = menuItems[i.MenuItemId].Price,
            }).ToList(),
            Discounts = discounts.Values.Select(d => new OrderDiscount
            {
                DiscountId = d.Id,
                Type = d.Type,
                Amount = d.Amount,
            }).ToList(),
            Taxes = taxes.Values.Select(t => new OrderTax
            {
                TaxId = t.Id,
                Percentage = t.Percentage,
            }).ToList(),
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Reload with navigation properties for response
        var created = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Discounts).ThenInclude(d => d.Discount)
            .Include(o => o.Taxes).ThenInclude(t => t.Tax)
            .FirstAsync(o => o.Id == order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, FormatOrder(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderRequest request)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.Discounts)
            .Include(o => o.Taxes)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        if (request.Items is null || request.Items.Count == 0)
            return BadRequest("Order must have at least one item.");

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return BadRequest($"Quantity must be greater than zero for menu item {item.MenuItemId}.");
        }

        // --- Items (Option C) ---
        var existingItems = order.Items.ToDictionary(i => i.MenuItemId);
        var incomingItemIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var newItemIds = incomingItemIds.Except(existingItems.Keys).ToList();

        // Validate new items are active
        var newMenuItems = new Dictionary<int, MenuItem>();
        if (newItemIds.Count > 0)
        {
            newMenuItems = await db.MenuItems
                .Where(m => newItemIds.Contains(m.Id) && m.IsActive)
                .ToDictionaryAsync(m => m.Id);

            var missingItems = newItemIds.Except(newMenuItems.Keys).ToList();
            if (missingItems.Count > 0)
                return BadRequest($"Menu items not found or inactive: {string.Join(", ", missingItems)}");
        }

        // Remove items no longer in the order
        var removedItems = existingItems.Keys.Except(incomingItemIds).ToList();
        foreach (var menuItemId in removedItems)
            db.OrderItems.Remove(existingItems[menuItemId]);

        // Update retained items, add new items
        foreach (var incoming in request.Items)
        {
            if (existingItems.TryGetValue(incoming.MenuItemId, out var existing))
            {
                existing.Quantity = incoming.Quantity;
            }
            else
            {
                order.Items.Add(new OrderItem
                {
                    MenuItemId = incoming.MenuItemId,
                    Quantity = incoming.Quantity,
                    UnitPrice = newMenuItems[incoming.MenuItemId].Price,
                });
            }
        }

        // --- Discounts (Option C) ---
        var existingDiscounts = order.Discounts.ToDictionary(d => d.DiscountId);
        var incomingDiscountIds = request.DiscountIds?.Distinct().ToList() ?? [];
        var newDiscountIds = incomingDiscountIds.Except(existingDiscounts.Keys).ToList();

        var newDiscounts = new Dictionary<int, Discount>();
        if (newDiscountIds.Count > 0)
        {
            newDiscounts = await db.Discounts
                .Where(d => newDiscountIds.Contains(d.Id) && d.IsActive)
                .ToDictionaryAsync(d => d.Id);

            var missingDiscounts = newDiscountIds.Except(newDiscounts.Keys).ToList();
            if (missingDiscounts.Count > 0)
                return BadRequest($"Discounts not found or inactive: {string.Join(", ", missingDiscounts)}");
        }

        var removedDiscounts = existingDiscounts.Keys.Except(incomingDiscountIds).ToList();
        foreach (var discountId in removedDiscounts)
            db.OrderDiscounts.Remove(existingDiscounts[discountId]);

        foreach (var discountId in newDiscountIds)
        {
            var d = newDiscounts[discountId];
            order.Discounts.Add(new OrderDiscount
            {
                DiscountId = d.Id,
                Type = d.Type,
                Amount = d.Amount,
            });
        }

        // --- Taxes (Option C) ---
        var existingTaxes = order.Taxes.ToDictionary(t => t.TaxId);
        var incomingTaxIds = request.TaxIds?.Distinct().ToList() ?? [];
        var newTaxIds = incomingTaxIds.Except(existingTaxes.Keys).ToList();

        var newTaxes = new Dictionary<int, Tax>();
        if (newTaxIds.Count > 0)
        {
            newTaxes = await db.Taxes
                .Where(t => newTaxIds.Contains(t.Id) && t.IsActive)
                .ToDictionaryAsync(t => t.Id);

            var missingTaxes = newTaxIds.Except(newTaxes.Keys).ToList();
            if (missingTaxes.Count > 0)
                return BadRequest($"Taxes not found or inactive: {string.Join(", ", missingTaxes)}");
        }

        var removedTaxes = existingTaxes.Keys.Except(incomingTaxIds).ToList();
        foreach (var taxId in removedTaxes)
            db.OrderTaxes.Remove(existingTaxes[taxId]);

        foreach (var taxId in newTaxIds)
        {
            var t = newTaxes[taxId];
            order.Taxes.Add(new OrderTax
            {
                TaxId = t.Id,
                Percentage = t.Percentage,
            });
        }

        await db.SaveChangesAsync();

        // Reload with navigation properties for response
        var updated = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Discounts).ThenInclude(d => d.Discount)
            .Include(o => o.Taxes).ThenInclude(t => t.Tax)
            .FirstAsync(o => o.Id == id);

        return Ok(FormatOrder(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null)
            return NotFound();

        db.Orders.Remove(order);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static object FormatOrder(Order o) => new
    {
        o.Id,
        o.UserId,
        o.CreatedAt,
        Items = o.Items.Select(i => new
        {
            i.MenuItemId,
            MenuItemName = i.MenuItem.Name,
            i.Quantity,
            i.UnitPrice,
        }),
        Discounts = o.Discounts.Select(d => new
        {
            d.DiscountId,
            Name = d.Discount.Name,
            Type = d.Type.ToString().ToLower(),
            d.Amount,
        }),
        Taxes = o.Taxes.Select(t => new
        {
            t.TaxId,
            Name = t.Tax.Name,
            t.Percentage,
        }),
    };
}
