using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealOrder.Api.Models;

public class OrderDiscount
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [Required]
    public int DiscountId { get; set; }
    public Discount Discount { get; set; } = null!;

    [Required]
    public DiscountType Type { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }
}
