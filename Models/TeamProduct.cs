namespace TimeTraceOne.Models;

public class TeamProduct
{
    public Guid TeamId { get; set; }
    public Guid ProductId { get; set; }
    
    // Navigation properties
    public virtual Team Team { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
