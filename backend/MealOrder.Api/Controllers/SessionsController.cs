using MealOrder.Api.Data;
using MealOrder.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace MealOrder.Api.Controllers;

public record LoginRequest(string Username, string Password);

[ApiController]
[Route("api/[controller]")]
public class SessionsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Username or password is incorrect.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var session = new Session
        {
            UserId = user.Id,
            Token = token,
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSession), new { token = session.Token }, new { session.Token, session.ExpiresAt });
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetSession(string token)
    {
        var session = await db.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Token == token);

        if (session is null)
            return NotFound();

        if (session.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Session has expired.");

        return Ok(new { session.User.Id, session.User.Username, session.User.FirstName, session.User.LastName });
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> Logout(string token)
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => s.Token == token);

        if (session is null)
            return NotFound();

        db.Sessions.Remove(session);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
