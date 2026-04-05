using MealOrder.Api.Data;
using MealOrder.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MealOrder.Api.Controllers;

public record CreateUserRequest(string Username, string Password);

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required.");

        if (db.Users.Any(u => u.Username == request.Username))
            return Conflict("Username is already taken.");

        if (request.Username.Length != 4 || !request.Username.All(char.IsDigit))
            return BadRequest("Username must be a 4-digit number.");

        if (request.Password.Length < 4 || request.Password.Length > 16)
            return BadRequest("Password must be between 4 and 16 characters.");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new { user.Id, user.Username });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        return Ok(new { user.Id, user.Username });
    }
}
