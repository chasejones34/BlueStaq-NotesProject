using System.ComponentModel.DataAnnotations;

namespace TeamNotes.Api.Contracts;

public sealed record CreateUserRequest(
    [property: Required, StringLength(100, MinimumLength = 3)] string Username,
    [property: Required, EmailAddress, StringLength(320)] string Email,
    [property: Required, StringLength(100, MinimumLength = 8)] string Password);

public sealed record UserResponse(int Id, string Username, string Email, DateTime CreatedAtUtc);

