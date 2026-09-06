using System.Security.Claims;

namespace TeamNotes.Api.Security;

public static class UserClaimsExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var value =
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue("nameid");

        return int.TryParse(value, out var userId) ? userId : null;
    }
}