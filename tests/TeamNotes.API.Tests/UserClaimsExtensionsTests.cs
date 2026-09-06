using System.Security.Claims;
using TeamNotes.Api.Security;

namespace TeamNotes.Api.Tests;

public class UserClaimsExtensionsTests
{
    [Fact]
    public void GetUserId_ReturnsUserIdFromNameIdentifierClaim()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42")
            },
            authenticationType: "Test");

        var principal = new ClaimsPrincipal(identity);

        var result = principal.GetUserId();

        Assert.Equal(42, result);
    }

    [Fact]
    public void GetUserId_ReturnsNullWhenClaimIsMissing()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(authenticationType: "Test"));

        var result = principal.GetUserId();

        Assert.Null(result);
    }
}