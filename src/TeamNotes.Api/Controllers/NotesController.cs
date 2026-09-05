using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamNotes.Api.Contracts;
using TeamNotes.Api.Data;
using TeamNotes.Api.Domain;

namespace TeamNotes.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class NotesController(AppDbContext db) : ControllerBase
{
    [HttpGet("teams/{teamId:int}/notes")]
    public async Task<ActionResult<IReadOnlyCollection<NoteResponse>>> GetNotes(
        int teamId, [FromHeader(Name = "X-User-Id")] int? userId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (userId is null) return Unauthorized();
        if (page < 1 || pageSize is < 1 or > 100)
            return BadRequest("page must be at least 1 and pageSize must be between 1 and 100.");
        if (!await IsMember(teamId, userId.Value, cancellationToken)) return Forbid();

        var notes = await db.Notes.AsNoTracking()
            .Where(note => note.TeamId == teamId && !note.IsDeleted)
            .OrderByDescending(note => note.UpdatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(note => new NoteResponse(note.Id, note.TeamId, note.AuthorId,
                note.Title, note.Content, note.CreatedAtUtc, note.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(notes);
    }

    [HttpPost("teams/{teamId:int}/notes")]
    public async Task<ActionResult<NoteResponse>> CreateNote(
        int teamId, CreateNoteRequest request,
        [FromHeader(Name = "X-User-Id")] int? userId,
        CancellationToken cancellationToken)
    {
        if (userId is null) return Unauthorized();
        var role = await GetRole(teamId, userId.Value, cancellationToken);
        if (role is null) return Forbid();
        if (role < TeamRole.Editor) return StatusCode(403, "Editor access is required.");

        var note = new Note { TeamId = teamId, AuthorId = userId.Value,
            Title = request.Title.Trim(), Content = request.Content.Trim() };
        db.Notes.Add(note);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetNote), new { noteId = note.Id }, ToResponse(note));
    }

    [HttpGet("notes/{noteId:int}")]
    public async Task<ActionResult<NoteResponse>> GetNote(
        int noteId, [FromHeader(Name = "X-User-Id")] int? userId,
        CancellationToken cancellationToken)
    {
        if (userId is null) return Unauthorized();
        var note = await db.Notes.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == noteId && !item.IsDeleted, cancellationToken);
        if (note is null) return NotFound();
        if (!await IsMember(note.TeamId, userId.Value, cancellationToken)) return Forbid();
        return Ok(ToResponse(note));
    }

    [HttpPut("notes/{noteId:int}")]
    public async Task<ActionResult<NoteResponse>> UpdateNote(
        int noteId, UpdateNoteRequest request,
        [FromHeader(Name = "X-User-Id")] int? userId,
        CancellationToken cancellationToken)
    {
        if (userId is null) return Unauthorized();
        var note = await db.Notes.SingleOrDefaultAsync(item => item.Id == noteId && !item.IsDeleted, cancellationToken);
        if (note is null) return NotFound();
        var role = await GetRole(note.TeamId, userId.Value, cancellationToken);
        if (role is null) return Forbid();
        if (role < TeamRole.Editor) return StatusCode(403, "Editor access is required.");

        note.Title = request.Title.Trim();
        note.Content = request.Content.Trim();
        note.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(note));
    }

    [HttpDelete("notes/{noteId:int}")]
    public async Task<IActionResult> DeleteNote(
        int noteId, [FromHeader(Name = "X-User-Id")] int? userId,
        CancellationToken cancellationToken)
    {
        if (userId is null) return Unauthorized();
        var note = await db.Notes.SingleOrDefaultAsync(item => item.Id == noteId && !item.IsDeleted, cancellationToken);
        if (note is null) return NotFound();
        var role = await GetRole(note.TeamId, userId.Value, cancellationToken);
        if (role is null) return Forbid();
        if (role < TeamRole.TeamLead) return StatusCode(403, "Team Lead access is required.");

        note.IsDeleted = true;
        note.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Task<bool> IsMember(int teamId, int userId, CancellationToken cancellationToken) =>
        db.TeamMembers.AnyAsync(member => member.TeamId == teamId && member.UserId == userId, cancellationToken);

    private Task<TeamRole?> GetRole(int teamId, int userId, CancellationToken cancellationToken) =>
        db.TeamMembers.Where(member => member.TeamId == teamId && member.UserId == userId)
            .Select(member => (TeamRole?)member.Role).SingleOrDefaultAsync(cancellationToken);

    private static NoteResponse ToResponse(Note note) => new(note.Id, note.TeamId, note.AuthorId,
        note.Title, note.Content, note.CreatedAtUtc, note.UpdatedAtUtc);
}
