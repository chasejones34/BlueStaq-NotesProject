namespace TeamNotes.Api.Domain;

public sealed class TeamMember
{
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public TeamRole Role { get; set; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;

    public Team Team { get; set; } = null!;
    public User User { get; set; } = null!;
}

