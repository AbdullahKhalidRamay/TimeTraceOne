using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Models;

namespace TimeTraceOne.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(TimeFlowDbContext context)
    {
        if (!context.Users.Any())
        {
            await SeedUsersAsync(context);
        }
        
        if (!context.Departments.Any())
        {
            await SeedDepartmentsAsync(context);
        }
        
        if (!context.Projects.Any())
        {
            await SeedProjectsAsync(context);
        }
        
        if (!context.Teams.Any())
        {
            await SeedTeamsAsync(context);
        }
    }
    
    private static async Task SeedUsersAsync(TimeFlowDbContext context)
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin User",
                Email = "admin@timeflow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Owner,
                JobTitle = "System Administrator",
                AvailableHours = 8.0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john.doe@timeflow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = UserRole.Owner,
                JobTitle = "CEO",
                AvailableHours = 8.0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Jane Smith",
                Email = "jane.smith@timeflow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = UserRole.Manager,
                JobTitle = "Project Manager",
                AvailableHours = 8.0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Bob Johnson",
                Email = "bob.johnson@timeflow.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = UserRole.Employee,
                JobTitle = "Developer",
                AvailableHours = 8.0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedDepartmentsAsync(TimeFlowDbContext context)
    {
        var owner = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Owner);
        if (owner == null) return;
        
        var departments = new List<Department>
        {
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "Engineering",
                DepartmentDescription = "Software engineering department",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = owner.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "Design",
                DepartmentDescription = "UI/UX design department",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = owner.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "Marketing",
                DepartmentDescription = "Marketing and sales department",
                IsBillable = false,
                Status = ProjectStatus.Active,
                CreatedBy = owner.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedProjectsAsync(TimeFlowDbContext context)
    {
        var owner = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Owner);
        if (owner == null) return;
        
        var projects = new List<Project>
        {
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Mobile App Development",
                Description = "Development of mobile application for iOS and Android",
                ProjectType = ProjectType.TimeAndMaterial,
                ClientName = "Mobile Corp",
                ClientEmail = "contact@mobilecorp.com",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = owner.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Web Platform",
                Description = "Web application platform development",
                ProjectType = ProjectType.FixedCost,
                ClientName = "Web Solutions",
                ClientEmail = "info@websolutions.com",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = owner.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedTeamsAsync(TimeFlowDbContext context)
    {
        var owner = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Owner);
        var manager = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Manager);
        var engineeringDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering");
        
        if (owner == null || manager == null || engineeringDept == null) return;
        
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Frontend Development Team",
            Description = "Team responsible for frontend development",
            DepartmentId = engineeringDept.Id,
            LeaderId = manager.Id,
            CreatedBy = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        
        // Add team members
        var teamMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = manager.Id,
            JoinedAt = DateTime.UtcNow
        };
        
        context.TeamMembers.Add(teamMember);
        await context.SaveChangesAsync();
    }
}
