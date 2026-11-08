using System.Security.Claims;

namespace BlazorApp.Auth;

public static class ClaimsPrincipalExtensions
{
    // Id of the logged-in user, or null if nobody is logged in.
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        string? value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out int id) ? id : null;
    }
}