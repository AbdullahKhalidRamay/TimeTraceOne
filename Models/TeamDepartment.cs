namespace TimeTraceOne.Models;

public class TeamDepartment
{
    public Guid TeamId { get; set; }
    public Guid DepartmentId { get; set; }
    
    // Navigation properties
    public virtual Team Team { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
}
