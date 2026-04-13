using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealOrder.Api.Models;

public class Tax
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Percentage { get; set; }

    public bool IsActive { get; set; } = true;
}
