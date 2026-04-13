using System.ComponentModel.DataAnnotations;

namespace MealOrder.Api.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderDiscount> Discounts { get; set; } = [];
    public ICollection<OrderTax> Taxes { get; set; } = [];
}
