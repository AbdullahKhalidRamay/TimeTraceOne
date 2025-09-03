using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Models;

namespace TimeTraceOne.Data;

public class TimeFlowDbContext : DbContext
{
    public TimeFlowDbContext(DbContextOptions<TimeFlowDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<TeamProject> TeamProjects { get; set; }
    public DbSet<TeamProduct> TeamProducts { get; set; }
    public DbSet<TeamDepartment> TeamDepartments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ApprovalHistory> ApprovalHistory { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.AvailableHours).HasPrecision(5, 2);
        });
        
        // TimeEntry configuration
        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActualHours).HasPrecision(5, 2);
            entity.Property(e => e.BillableHours).HasPrecision(5, 2);
            entity.Property(e => e.TotalHours).HasPrecision(5, 2);
            entity.Property(e => e.AvailableHours).HasPrecision(5, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.TimeEntries)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProjectType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasOne(e => e.Creator)
                  .WithMany(u => u.CreatedProjects)
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasOne(e => e.Creator)
                  .WithMany(u => u.CreatedProducts)
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasOne(e => e.Creator)
                  .WithMany(u => u.CreatedDepartments)
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Team configuration
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Teams)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Leader)
                  .WithMany()
                  .HasForeignKey(e => e.LeaderId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Creator)
                  .WithMany(u => u.CreatedTeams)
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // TeamMember configuration (many-to-many)
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.UserId });
            
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Members)
                  .HasForeignKey(e => e.TeamId);
                  
            entity.HasOne(e => e.User)
                  .WithMany(u => u.TeamMemberships)
                  .HasForeignKey(e => e.UserId);
        });
        
        // TeamProject configuration (many-to-many)
        modelBuilder.Entity<TeamProject>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.ProjectId });
            
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.TeamProjects)
                  .HasForeignKey(e => e.TeamId);
                  
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.TeamProjects)
                  .HasForeignKey(e => e.ProjectId);
        });
        
        // TeamProduct configuration (many-to-many)
        modelBuilder.Entity<TeamProduct>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.ProductId });
            
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.TeamProducts)
                  .HasForeignKey(e => e.TeamId);
                  
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.TeamProducts)
                  .HasForeignKey(e => e.ProductId);
        });
        
        // TeamDepartment configuration (many-to-many)
        modelBuilder.Entity<TeamDepartment>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.DepartmentId });
            
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.TeamDepartments)
                  .HasForeignKey(e => e.TeamId);
                  
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.TeamDepartments)
                  .HasForeignKey(e => e.DepartmentId);
        });
        
        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.RelatedEntry)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedEntryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // ApprovalHistory configuration
        modelBuilder.Entity<ApprovalHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Entry)
                  .WithMany(t => t.ApprovalHistory)
                  .HasForeignKey(e => e.EntryId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Approver)
                  .WithMany(u => u.ApprovalHistory)
                  .HasForeignKey(e => e.ApprovedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
