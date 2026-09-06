using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamNotes.Api.Contracts;
using TeamNotes.Api.Data;
using TeamNotes.Api.Domain;
using TeamNotes.Api.Security;

namespace TeamNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/teams")]
public sealed class TeamsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TeamResponse>>> GetTeams(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var teams = await db.TeamMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId.Value)
            .Select(member => new TeamResponse(
                member.Team.Id,
                member.Team.Name,
                member.Team.CreatedAtUtc,
                member.Role))
            .ToListAsync(cancellationToken);

        return Ok(teams);
    }

    [HttpPost]
    public async Task<ActionResult<TeamResponse>> CreateTeam(
        CreateTeamRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        if (!await db.Users.AnyAsync(
                user => user.Id == userId.Value,
                cancellationToken))
        {
            return Unauthorized();
        }

        var team = new Team
        {
            Name = request.Name.Trim()
        };

        team.Members.Add(new TeamMember
        {
            UserId = userId.Value,
            Role = TeamRole.Owner
        });

        db.Teams.Add(team);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetTeams),
            new { id = team.Id },
            new TeamResponse(
                team.Id,
                team.Name,
                team.CreatedAtUtc,
                TeamRole.Owner));
    }

    [HttpPost("{teamId:int}/members")]
    public async Task<ActionResult<TeamMemberResponse>> AddMember(
        int teamId,
        AddTeamMemberRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = User.GetUserId();

        if (actorId is null)
            return Unauthorized();

        var actorRole = await db.TeamMembers
            .Where(member =>
                member.TeamId == teamId &&
                member.UserId == actorId.Value)
            .Select(member => (TeamRole?)member.Role)
            .SingleOrDefaultAsync(cancellationToken);

        if (actorRole is null)
            return Forbid();

        if (actorRole != TeamRole.Owner)
        {
            return StatusCode(
                403,
                "Owner access is required to manage team members.");
        }

        if (!Enum.IsDefined(request.Role) ||
            request.Role == TeamRole.Owner)
        {
            return BadRequest(
                "New members must be assigned Member, Editor, or TeamLead.");
        }

        var username = request.Username.Trim().ToLowerInvariant();

        var user = await db.Users
            .SingleOrDefaultAsync(
                item => item.Username == username,
                cancellationToken);

        if (user is null)
            return NotFound("The specified user does not exist.");

        var alreadyMember = await db.TeamMembers.AnyAsync(
            member =>
                member.TeamId == teamId &&
                member.UserId == user.Id,
            cancellationToken);

        if (alreadyMember)
            return Conflict(
                "The user is already a member of this team.");

        var teamMember = new TeamMember
        {
            TeamId = teamId,
            UserId = user.Id,
            Role = request.Role
        };

        db.TeamMembers.Add(teamMember);
        await db.SaveChangesAsync(cancellationToken);

        var response = new TeamMemberResponse(
            user.Id,
            user.Username,
            teamMember.Role,
            teamMember.JoinedAtUtc);

        return Created(
            $"/api/teams/{teamId}/members/{user.Id}",
            response);
    }
}