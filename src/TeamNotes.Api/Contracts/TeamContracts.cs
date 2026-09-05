using System.ComponentModel.DataAnnotations;
using TeamNotes.Api.Domain;

namespace TeamNotes.Api.Contracts;

public sealed record CreateTeamRequest(
    [param: Required, StringLength(150, MinimumLength = 2)] string Name);

public sealed record TeamResponse(int Id, string Name, DateTime CreatedAtUtc, TeamRole CurrentUserRole);

