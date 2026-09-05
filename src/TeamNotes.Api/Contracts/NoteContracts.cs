using System.ComponentModel.DataAnnotations;

namespace TeamNotes.Api.Contracts;

public sealed record CreateNoteRequest(
    [param: Required, StringLength(200, MinimumLength = 1)] string Title,
    [param: Required, StringLength(100_000, MinimumLength = 1)] string Content);

public sealed record UpdateNoteRequest(
    [param: Required, StringLength(200, MinimumLength = 1)] string Title,
    [param: Required, StringLength(100_000, MinimumLength = 1)] string Content);

public sealed record NoteResponse(
    int Id,
    int TeamId,
    int AuthorId,
    string Title,
    string Content,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

