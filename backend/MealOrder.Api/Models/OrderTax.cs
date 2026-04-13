using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealOrder.Api.Models;

public class OrderTax
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [Required]
    public int TaxId { get; set; }
    public Tax Tax { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Percentage { get; set; }
}
