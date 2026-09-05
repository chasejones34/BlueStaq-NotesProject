using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamNotes.Api.Contracts;
using TeamNotes.Api.Data;
using TeamNotes.Api.Domain;

namespace TeamNotes.Api.Controllers;

[ApiController]
[Route("api/teams")]
public sealed class TeamsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TeamResponse>>> GetTeams(
        [FromHeader(Name = "X-User-Id")] int? userId,
        CancellationToken cancellationToken)
    {
        if (userId is null)
            return Unauthorized("Supply X-User-Id while development authentication is enabled.");

        var teams = await db.TeamMembers.AsNoTracking()
            .Where(member => member.UserId == userId.Value)
            .Select(member => new TeamResponse(member.Team.Id, member.Team.Name,
                member.Team.CreatedAtUtc, member.Role))
            .ToListAsync(cancellationToken);

        return Ok(teams);
    }

    [HttpPost]
    public async Task<ActionResult<TeamResponse>> CreateTeam(
        CreateTeamRequest request,
        [FromHeader(Name = "X-User-Id")] int? userId,
        CancellationToken cancellationToken)
    {
        if (userId is null)
            return Unauthorized("Supply X-User-Id while development authentication is enabled.");
        if (!await db.Users.AnyAsync(user => user.Id == userId.Value, cancellationToken))
            return Unauthorized("The supplied user does not exist.");

        var team = new Team { Name = request.Name.Trim() };
        team.Members.Add(new TeamMember { UserId = userId.Value, Role = TeamRole.Owner });
        db.Teams.Add(team);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetTeams), new { id = team.Id },
            new TeamResponse(team.Id, team.Name, team.CreatedAtUtc, TeamRole.Owner));
    }
}
