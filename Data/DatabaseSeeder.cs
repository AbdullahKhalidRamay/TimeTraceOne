using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Models;

namespace TimeTraceOne.Data;

public static class DatabaseSeeder
{
    // Use predictable GUIDs for testing
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid JohnDoeUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid JaneSmithUserId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid BobJohnsonUserId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    
    private static readonly Guid EngineeringDeptId = Guid.Parse("00000000-0000-0000-0000-000000000005");
    private static readonly Guid DesignDeptId = Guid.Parse("00000000-0000-0000-0000-000000000006");
    private static readonly Guid MarketingDeptId = Guid.Parse("00000000-0000-0000-0000-000000000007");
    
    private static readonly Guid MobileAppProjectId = Guid.Parse("00000000-0000-0000-0000-000000000008");
    private static readonly Guid WebPlatformProjectId = Guid.Parse("00000000-0000-0000-0000-000000000009");
    
    private static readonly Guid FrontendTeamId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    
    private static readonly Guid TestProductId = Guid.Parse("00000000-0000-0000-0000-000000000011");
    
    public static async Task SeedAsync(TimeFlowDbContext context)
    {
        try
        {
            // Always seed users first
            if (!context.Users.Any())
            {
                await SeedUsersAsync(context);
            }
            
            // Always seed departments
            if (!context.Departments.Any())
            {
                await SeedDepartmentsAsync(context);
            }
            
            // Always seed projects
            if (!context.Projects.Any())
            {
                await SeedProjectsAsync(context);
            }
            
            // Always seed teams
            if (!context.Teams.Any())
            {
                await SeedTeamsAsync(context);
            }
            
            // Always seed products
            if (!context.Products.Any())
            {
                await SeedProductsAsync(context);
            }
            
            // Always seed time entries (force reseed)
            context.TimeEntries.RemoveRange(context.TimeEntries);
            await context.SaveChangesAsync();
            await SeedTimeEntriesAsync(context);
            
            // Always seed notifications (force reseed)
            context.Notifications.RemoveRange(context.Notifications);
            await context.SaveChangesAsync();
            await SeedNotificationsAsync(context);
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            Console.WriteLine($"Database seeding error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    private static async Task SeedUsersAsync(TimeFlowDbContext context)
    {
        var users = new List<User>
        {
            new User
            {
                Id = AdminUserId,
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
                Id = JohnDoeUserId,
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
                Id = JaneSmithUserId,
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
                Id = BobJohnsonUserId,
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
        var departments = new List<Department>
        {
            new Department
            {
                Id = EngineeringDeptId,
                Name = "Engineering",
                DepartmentDescription = "Software engineering department",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = AdminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = DesignDeptId,
                Name = "Design",
                DepartmentDescription = "UI/UX design department",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = AdminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = MarketingDeptId,
                Name = "Marketing",
                DepartmentDescription = "Marketing and sales department",
                IsBillable = false,
                Status = ProjectStatus.Active,
                CreatedBy = AdminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedProjectsAsync(TimeFlowDbContext context)
    {
        var projects = new List<Project>
        {
            new Project
            {
                Id = MobileAppProjectId,
                Name = "Mobile App Development",
                Description = "Development of mobile application for iOS and Android",
                ProjectType = ProjectType.TimeAndMaterial,
                ClientName = "Mobile Corp",
                ClientEmail = "contact@mobilecorp.com",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = AdminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = WebPlatformProjectId,
                Name = "Web Platform",
                Description = "Web application platform development",
                ProjectType = ProjectType.FixedCost,
                ClientName = "Web Solutions",
                ClientEmail = "info@websolutions.com",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = AdminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedTeamsAsync(TimeFlowDbContext context)
    {
        var team = new Team
        {
            Id = FrontendTeamId,
            Name = "Frontend Development Team",
            Description = "Team responsible for frontend development",
            DepartmentId = EngineeringDeptId,
            LeaderId = JaneSmithUserId,
            CreatedBy = AdminUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        
        // Add team members
        var teamMember = new TeamMember
        {
            TeamId = FrontendTeamId,
            UserId = JaneSmithUserId,
            JoinedAt = DateTime.UtcNow
        };
        
        context.TeamMembers.Add(teamMember);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedProductsAsync(TimeFlowDbContext context)
    {
        var products = new List<Product>
        {
            new Product
            {
                Id = TestProductId,
                Name = "Test Product",
                ProductDescription = "A test product for development",
                IsBillable = true,
                Status = ProjectStatus.Active,
                CreatedBy = AdminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedTimeEntriesAsync(TimeFlowDbContext context)
    {
        try
        {
            Console.WriteLine("Starting to seed time entries...");
            
            var timeEntries = new List<TimeEntry>
            {
                new TimeEntry
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                    UserId = BobJohnsonUserId,
                    Date = DateTime.Today.AddDays(-1),
                    ActualHours = 8.0m,
                    BillableHours = 8.0m,
                    TotalHours = 8.0m,
                    AvailableHours = 8.0m,
                    Task = "Development work on mobile app",
                    ProjectDetails = "{\"feature\": \"User authentication\"}",
                    IsBillable = true,
                    Status = EntryStatus.Approved,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TimeEntry
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000013"),
                    UserId = JaneSmithUserId,
                    Date = DateTime.Today.AddDays(-2),
                    ActualHours = 6.5m,
                    BillableHours = 6.5m,
                    TotalHours = 6.5m,
                    AvailableHours = 8.0m,
                    Task = "Project planning and coordination",
                    ProjectDetails = "{\"activity\": \"Sprint planning\"}",
                    IsBillable = true,
                    Status = EntryStatus.Approved,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            
            Console.WriteLine($"Adding {timeEntries.Count} time entries to context...");
            context.TimeEntries.AddRange(timeEntries);
            
            Console.WriteLine("Saving time entries to database...");
            await context.SaveChangesAsync();
            
            Console.WriteLine("Time entries seeded successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding time entries: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    private static async Task SeedNotificationsAsync(TimeFlowDbContext context)
    {
        var notifications = new List<Notification>
        {
            new Notification
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000014"),
                Title = "Test Notification",
                Message = "This is a test notification",
                Type = NotificationType.System,
                UserId = AdminUserId,
                RelatedEntryId = Guid.Parse("00000000-0000-0000-0000-000000000012"),
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        
        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
    }
}
