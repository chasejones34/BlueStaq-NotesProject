namespace TeamNotes.Api.Domain;

public sealed class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}

