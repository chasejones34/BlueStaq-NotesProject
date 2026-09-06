using System.ComponentModel.DataAnnotations;
using TeamNotes.Api.Domain;

namespace TeamNotes.Api.Contracts;

public sealed record CreateTeamRequest(
    [param: Required, StringLength(150, MinimumLength = 2)] string Name);

public sealed record AddTeamMemberRequest(
    [param: Required, StringLength(100, MinimumLength = 3)] string Username,
    TeamRole Role);

public sealed record TeamResponse(
    int Id,
    string Name,
    DateTime CreatedAtUtc,
    TeamRole CurrentUserRole);

public sealed record TeamMemberResponse(
    int UserId,
    string Username,
    TeamRole Role,
    DateTime JoinedAtUtc);