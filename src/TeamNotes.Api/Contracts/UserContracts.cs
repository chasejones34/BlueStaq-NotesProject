using System.ComponentModel.DataAnnotations;

namespace TeamNotes.Api.Contracts;

public sealed record CreateUserRequest(
    [param: Required, StringLength(100, MinimumLength = 3)] string Username,
    [param: Required, EmailAddress, StringLength(320)] string Email,
    [param: Required, StringLength(100, MinimumLength = 8)] string Password);

public sealed record UserResponse(int Id, string Username, string Email, DateTime CreatedAtUtc);

