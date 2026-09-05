using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamNotes.Api.Contracts;
using TeamNotes.Api.Data;
using TeamNotes.Api.Domain;

namespace TeamNotes.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(AppDbContext db, IPasswordHasher<User> passwordHasher) : ControllerBase
{
    // This endpoint is intentionally a simple registration starting point.
    // JWT login and production account policies will be added next.
    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(user => user.Username == username || user.Email == email, cancellationToken))
            return Conflict("A user with that username or email already exists.");

        var user = new User { Username = username, Email = email };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(CreateUser), new { id = user.Id },
            new UserResponse(user.Id, user.Username, user.Email, user.CreatedAtUtc));
    }
}
