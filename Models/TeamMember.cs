namespace TimeTraceOne.Models;

public class TeamMember
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Team Team { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
