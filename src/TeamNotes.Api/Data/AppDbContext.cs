using Microsoft.EntityFrameworkCore;
using TeamNotes.Api.Domain;

namespace TeamNotes.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Username).IsUnique();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(user => user.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(user => user.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");
            entity.HasKey(team => team.Id);
            entity.Property(team => team.Id).HasColumnName("id");
            entity.Property(team => team.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(team => team.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.ToTable("team_members", table => table.HasCheckConstraint("ck_team_members_role", "role BETWEEN 1 AND 4"));
            entity.HasKey(member => new { member.TeamId, member.UserId });
            entity.Property(member => member.TeamId).HasColumnName("team_id");
            entity.Property(member => member.UserId).HasColumnName("user_id");
            entity.Property(member => member.Role).HasColumnName("role").HasConversion<short>();
            entity.Property(member => member.JoinedAtUtc).HasColumnName("joined_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(member => member.Team)
                .WithMany(team => team.Members)
                .HasForeignKey(member => member.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(member => member.User)
                .WithMany(user => user.TeamMemberships)
                .HasForeignKey(member => member.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToTable("notes");
            entity.HasKey(note => note.Id);
            entity.HasIndex(note => new { note.TeamId, note.IsDeleted, note.UpdatedAtUtc });
            entity.Property(note => note.Id).HasColumnName("id");
            entity.Property(note => note.TeamId).HasColumnName("team_id");
            entity.Property(note => note.AuthorId).HasColumnName("author_id");
            entity.Property(note => note.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(note => note.Content).HasColumnName("content").IsRequired();
            entity.Property(note => note.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(note => note.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(note => note.UpdatedAtUtc).HasColumnName("updated_at_utc").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(note => note.Team)
                .WithMany(team => team.Notes)
                .HasForeignKey(note => note.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(note => note.Author)
                .WithMany(user => user.AuthoredNotes)
                .HasForeignKey(note => note.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
