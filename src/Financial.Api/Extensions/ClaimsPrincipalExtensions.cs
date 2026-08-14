using System.Security.Claims;

namespace Financial.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("userId") 
            ?? user.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid User ID format in token.");
        }

        return userId;
    }

    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new UnauthorizedAccessException("Email not found in token.");
    }
}
