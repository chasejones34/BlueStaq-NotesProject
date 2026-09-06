using System.ComponentModel.DataAnnotations;

namespace TeamNotes.Api.Contracts;

public sealed record LoginRequest(
    [param: Required] string UsernameOrEmail,
    [param: Required] string Password);

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
