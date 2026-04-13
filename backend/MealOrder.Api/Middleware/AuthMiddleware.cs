using MealOrder.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MealOrder.Api.Middleware;

public class AuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() is not null)
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authentication required.");
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();

        var session = await db.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Token == token);

        if (session is null || session.ExpiresAt < DateTime.UtcNow)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid or expired session.");
            return;
        }

        context.Items["UserId"] = session.UserId;
        context.Items["User"] = session.User;
        context.Items["SessionToken"] = token;

        await next(context);
    }
}
