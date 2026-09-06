using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TeamNotes.Api.Contracts;
using TeamNotes.Api.Data;
using TeamNotes.Api.Domain;

namespace TeamNotes.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AppDbContext db,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var login = request.UsernameOrEmail.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(
            item => item.Username == login || item.Email == login,
            cancellationToken);

        if (user is null || passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid username/email or password.");

        var issuer = configuration["Jwt:Issuer"] ?? "TeamNotes.Api";
        var audience = configuration["Jwt:Audience"] ?? "TeamNotes.Api";
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var expiresAtUtc = DateTime.UtcNow.AddHours(1);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer, audience, claims, expires: expiresAtUtc, signingCredentials: credentials);

        return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc));
    }
}
