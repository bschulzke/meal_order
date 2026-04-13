namespace MealOrder.Api.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(25)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(25)]
    public string LastName { get; set; } = string.Empty;
}