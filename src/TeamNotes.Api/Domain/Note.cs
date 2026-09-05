namespace TeamNotes.Api.Domain;

public sealed class Note
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public User Author { get; set; } = null!;
}

