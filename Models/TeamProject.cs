namespace TimeTraceOne.Models;

public class TeamProject
{
    public Guid TeamId { get; set; }
    public Guid ProjectId { get; set; }
    
    // Navigation properties
    public virtual Team Team { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
}
