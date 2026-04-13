using MealOrder.Api.Models;

namespace MealOrder.Api.Extensions;

public static class HttpContextExtensions
{
    public static int GetAuthenticatedUserId(this HttpContext context)
        => (int)context.Items["UserId"]!;

    public static User GetAuthenticatedUser(this HttpContext context)
        => (User)context.Items["User"]!;

    public static string GetSessionToken(this HttpContext context)
        => (string)context.Items["SessionToken"]!;
}
