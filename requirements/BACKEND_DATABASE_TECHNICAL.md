# TimeFlow Backend Requirements - Part 4: Database Schema & Technical Requirements (ASP.NET Core 8.0)

## Overview
This document contains the database schema, technical architecture, and implementation requirements for the TimeFlow backend using ASP.NET Core 8.0.

---

## 🗄️ Database Schema

### **Database Technology**
- **Primary Database**: SQL Server 2022 (recommended) or Azure SQL Database
- **Caching**: Redis for session management and frequently accessed data
- **Backup**: Automated daily backups with point-in-time recovery

### **Core Tables**

#### Users Table
```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'employee',
    JobTitle NVARCHAR(100),
    AvailableHours DECIMAL(5,2) DEFAULT 8.0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
```

#### TimeEntries Table (Updated - No Clock Fields)
```sql
CREATE TABLE TimeEntries (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Date DATE NOT NULL,
    ActualHours DECIMAL(5,2) NOT NULL,
    BillableHours DECIMAL(5,2) NOT NULL,
    TotalHours DECIMAL(5,2) NOT NULL,
    AvailableHours DECIMAL(5,2) NOT NULL,
    Task NVARCHAR(MAX) NOT NULL,
    ProjectDetails NVARCHAR(MAX) NOT NULL, -- JSON field
    IsBillable BIT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'pending',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_TimeEntries_Hours CHECK (ActualHours >= 0 AND BillableHours >= 0),
    CONSTRAINT CK_TimeEntries_Status CHECK (Status IN ('pending', 'approved', 'rejected'))
);

CREATE INDEX IX_TimeEntries_UserId ON TimeEntries(UserId);
CREATE INDEX IX_TimeEntries_Date ON TimeEntries(Date);
CREATE INDEX IX_TimeEntries_Status ON TimeEntries(Status);
CREATE INDEX IX_TimeEntries_IsBillable ON TimeEntries(IsBillable);
CREATE INDEX IX_TimeEntries_UserDate ON TimeEntries(UserId, Date);
```

#### Projects Table
```sql
CREATE TABLE Projects (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    ProjectType NVARCHAR(50) NOT NULL,
    ClientName NVARCHAR(100),
    ClientEmail NVARCHAR(255),
    IsBillable BIT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Projects_Status ON Projects(Status);
CREATE INDEX IX_Projects_IsBillable ON Projects(IsBillable);
CREATE INDEX IX_Projects_CreatedBy ON Projects(CreatedBy);
```

#### Products Table
```sql
CREATE TABLE Products (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    ProductDescription NVARCHAR(MAX),
    IsBillable BIT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Products_Status ON Products(Status);
CREATE INDEX IX_Products_IsBillable ON Products(IsBillable);
```

#### Departments Table
```sql
CREATE TABLE Departments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    DepartmentDescription NVARCHAR(MAX),
    IsBillable BIT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'active',
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Departments_Status ON Departments(Status);
CREATE INDEX IX_Departments_IsBillable ON Departments(IsBillable);
```

#### Teams Table
```sql
CREATE TABLE Teams (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    DepartmentId UNIQUEIDENTIFIER NOT NULL,
    LeaderId UNIQUEIDENTIFIER,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    FOREIGN KEY (LeaderId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Teams_DepartmentId ON Teams(DepartmentId);
CREATE INDEX IX_Teams_LeaderId ON Teams(LeaderId);
```

#### Team Members Junction Table
```sql
CREATE TABLE TeamMembers (
    TeamId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    PRIMARY KEY (TeamId, UserId),
    FOREIGN KEY (TeamId) REFERENCES Teams(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_TeamMembers_UserId ON TeamMembers(UserId);
```

#### Team Associations Junction Tables
```sql
CREATE TABLE TeamProjects (
    TeamId UNIQUEIDENTIFIER NOT NULL,
    ProjectId UNIQUEIDENTIFIER NOT NULL,
    
    PRIMARY KEY (TeamId, ProjectId),
    FOREIGN KEY (TeamId) REFERENCES Teams(Id),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
);

CREATE TABLE TeamProducts (
    TeamId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    
    PRIMARY KEY (TeamId, ProductId),
    FOREIGN KEY (TeamId) REFERENCES Teams(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE TeamDepartments (
    TeamId UNIQUEIDENTIFIER NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NOT NULL,
    
    PRIMARY KEY (TeamId, DepartmentId),
    FOREIGN KEY (TeamId) REFERENCES Teams(Id),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);
```

#### Notifications Table
```sql
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    RelatedEntryId UNIQUEIDENTIFIER,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (RelatedEntryId) REFERENCES TimeEntries(Id)
);

CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_Notifications_Type ON Notifications(Type);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);
```

#### Approval History Table
```sql
CREATE TABLE ApprovalHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EntryId UNIQUEIDENTIFIER NOT NULL,
    PreviousStatus NVARCHAR(20) NOT NULL,
    NewStatus NVARCHAR(20) NOT NULL,
    Message NVARCHAR(MAX),
    ApprovedBy UNIQUEIDENTIFIER NOT NULL,
    ApprovedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (EntryId) REFERENCES TimeEntries(Id),
    FOREIGN KEY (ApprovedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_ApprovalHistory_EntryId ON ApprovalHistory(EntryId);
CREATE INDEX IX_ApprovalHistory_ApprovedBy ON ApprovalHistory(ApprovedBy);
CREATE INDEX IX_ApprovalHistory_ApprovedAt ON ApprovalHistory(ApprovedAt);
```

---

## 🏗️ Technical Architecture (ASP.NET Core 8.0)

### **Technology Stack**

#### Backend Framework
```csharp
// ASP.NET Core 8.0 Web API Project Structure
TimeFlow.API/
├── Controllers/           // API controllers
├── Services/              // Business logic services
├── Models/                // Data models and DTOs
├── Data/                  // DbContext and repositories
├── Middleware/            // Custom middleware
├── Configuration/         // App settings and configuration
├── Extensions/            // Service collection extensions
├── Filters/               // Action filters
├── Program.cs             // Main application entry point
└── appsettings.json       // Configuration file
```

#### Entity Framework Core Models
```csharp
// Models/User.cs
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public string? JobTitle { get; set; }
    public decimal AvailableHours { get; set; } = 8.0m;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public virtual ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public virtual ICollection<Team> CreatedTeams { get; set; } = new List<Team>();
}

// Models/TimeEntry.cs
public class TimeEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public decimal TotalHours { get; set; }
    public decimal AvailableHours { get; set; }
    public string Task { get; set; } = string.Empty;
    public string ProjectDetails { get; set; } = string.Empty; // JSON string
    public bool IsBillable { get; set; }
    public EntryStatus Status { get; set; } = EntryStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}

// Enums
public enum UserRole
{
    Employee,
    Manager,
    Owner
}

public enum EntryStatus
{
    Pending,
    Approved,
    Rejected
}
```

#### DbContext Configuration
```csharp
// Data/TimeFlowDbContext.cs
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
    }
}
```

### **API Structure**

#### Controller Organization
```csharp
// Controllers/TimeEntriesController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly ILogger<TimeEntriesController> _logger;
    
    public TimeEntriesController(ITimeEntryService timeEntryService, ILogger<TimeEntriesController> logger)
    {
        _timeEntryService = timeEntryService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TimeEntryDto>>>> GetTimeEntries([FromQuery] TimeEntryFilterDto filter)
    {
        try
        {
            var timeEntries = await _timeEntryService.GetTimeEntriesAsync(filter);
            return Ok(ApiResponse<List<TimeEntryDto>>.Success(timeEntries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time entries");
            return StatusCode(500, ApiResponse<List<TimeEntryDto>>.Error("Internal server error"));
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> CreateTimeEntry([FromBody] CreateTimeEntryDto dto)
    {
        try
        {
            var timeEntry = await _timeEntryService.CreateTimeEntryAsync(dto);
            return CreatedAtAction(nameof(GetTimeEntry), new { id = timeEntry.Id }, 
                ApiResponse<TimeEntryDto>.Success(timeEntry));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<TimeEntryDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating time entry");
            return StatusCode(500, ApiResponse<TimeEntryDto>.Error("Internal server error"));
        }
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> GetTimeEntry(Guid id)
    {
        var timeEntry = await _timeEntryService.GetTimeEntryByIdAsync(id);
        if (timeEntry == null)
            return NotFound(ApiResponse<TimeEntryDto>.Error("Time entry not found"));
            
        return Ok(ApiResponse<TimeEntryDto>.Success(timeEntry));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> UpdateTimeEntry(Guid id, [FromBody] UpdateTimeEntryDto dto)
    {
        try
        {
            var timeEntry = await _timeEntryService.UpdateTimeEntryAsync(id, dto);
            return Ok(ApiResponse<TimeEntryDto>.Success(timeEntry));
        }
        catch (NotFoundException)
        {
            return NotFound(ApiResponse<TimeEntryDto>.Error("Time entry not found"));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<TimeEntryDto>.Error(ex.Message));
        }
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTimeEntry(Guid id)
    {
        try
        {
            await _timeEntryService.DeleteTimeEntryAsync(id);
            return Ok(ApiResponse<string>.Success("Time entry deleted successfully"));
        }
        catch (NotFoundException)
        {
            return NotFound(ApiResponse<string>.Error("Time entry not found"));
        }
    }
}
```

#### Service Layer
```csharp
// Services/ITimeEntryService.cs
public interface ITimeEntryService
{
    Task<List<TimeEntryDto>> GetTimeEntriesAsync(TimeEntryFilterDto filter);
    Task<TimeEntryDto?> GetTimeEntryByIdAsync(Guid id);
    Task<TimeEntryDto> CreateTimeEntryAsync(CreateTimeEntryDto dto);
    Task<TimeEntryDto> UpdateTimeEntryAsync(Guid id, UpdateTimeEntryDto dto);
    Task DeleteTimeEntryAsync(Guid id);
    Task<WeeklyBulkResponseDto> CreateWeeklyBulkAsync(WeeklyBulkRequestDto dto);
}

// Services/TimeEntryService.cs
public class TimeEntryService : ITimeEntryService
{
    private readonly TimeFlowDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TimeEntryService> _logger;
    
    public TimeEntryService(TimeFlowDbContext context, IMapper mapper, ILogger<TimeEntryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<List<TimeEntryDto>> GetTimeEntriesAsync(TimeEntryFilterDto filter)
    {
        var query = _context.TimeEntries
            .Include(t => t.User)
            .AsQueryable();
            
        // Apply filters
        if (filter.UserId.HasValue)
            query = query.Where(t => t.UserId == filter.UserId.Value);
            
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.Date >= filter.StartDate.Value);
            
        if (filter.EndDate.HasValue)
            query = query.Where(t => t.Date <= filter.EndDate.Value);
            
        if (filter.Status?.Any() == true)
            query = query.Where(t => filter.Status.Contains(t.Status));
            
        var timeEntries = await query
            .OrderByDescending(t => t.Date)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync();
            
        return _mapper.Map<List<TimeEntryDto>>(timeEntries);
    }
    
    public async Task<TimeEntryDto> CreateTimeEntryAsync(CreateTimeEntryDto dto)
    {
        var timeEntry = _mapper.Map<TimeEntry>(dto);
        timeEntry.Id = Guid.NewGuid();
        timeEntry.CreatedAt = DateTime.UtcNow;
        timeEntry.UpdatedAt = DateTime.UtcNow;
        
        _context.TimeEntries.Add(timeEntry);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created time entry {Id} for user {UserId}", timeEntry.Id, timeEntry.UserId);
        
        return _mapper.Map<TimeEntryDto>(timeEntry);
    }
    
    // Additional methods...
}
```

---

## 🔐 Security Implementation (ASP.NET Core)

### **JWT Configuration**
```csharp
// Program.cs - JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();
```

### **Password Security with ASP.NET Core Identity**
```csharp
// Services/IJwtService.cs
public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

// Services/JwtService.cs
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    
    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("AvailableHours", user.AvailableHours.ToString())
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### **Input Validation with FluentValidation**
```csharp
// Validators/CreateTimeEntryDtoValidator.cs
public class CreateTimeEntryDtoValidator : AbstractValidator<CreateTimeEntryDto>
{
    public CreateTimeEntryDtoValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .Must(date => date <= DateTime.Today)
            .WithMessage("Date cannot be in the future");
            
        RuleFor(x => x.ActualHours)
            .GreaterThan(0)
            .LessThanOrEqualTo(24)
            .WithMessage("Actual hours must be between 0 and 24");
            
        RuleFor(x => x.BillableHours)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(24)
            .WithMessage("Billable hours must be between 0 and 24");
            
        RuleFor(x => x.BillableHours)
            .LessThanOrEqualTo(x => x.ActualHours)
            .WithMessage("Billable hours cannot exceed actual hours");
            
        RuleFor(x => x.Task)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("Task description is required and cannot exceed 1000 characters");
            
        RuleFor(x => x.ProjectDetails)
            .SetValidator(new ProjectDetailsDtoValidator());
    }
}

// Program.cs - Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTimeEntryDtoValidator>();
```

---

## 📊 Performance & Scalability

### **Database Optimization**
```csharp
// Repository Pattern with Caching
public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly TimeFlowDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TimeEntryRepository> _logger;
    
    public TimeEntryRepository(TimeFlowDbContext context, IDistributedCache cache, ILogger<TimeEntryRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<List<TimeEntry>> GetUserTimeEntriesAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var cacheKey = $"timeentries:{userId}:{startDate:yyyy-MM-dd}:{endDate:yyyy-MM-dd}";
        
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<List<TimeEntry>>(cached)!;
        }
        
        var timeEntries = await _context.TimeEntries
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .Include(t => t.User)
            .AsNoTracking()
            .ToListAsync();
            
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(timeEntries), 
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) });
            
        return timeEntries;
    }
}
```

### **API Response Caching**
```csharp
// Controllers/ProjectsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // Cache for 5 minutes
    public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetProjects()
    {
        // Implementation
    }
}
```

---

## 🧪 Testing Strategy (ASP.NET Core)

### **Unit Tests with xUnit**
```csharp
// Tests/Services/TimeEntryServiceTests.cs
public class TimeEntryServiceTests
{
    private readonly Mock<TimeFlowDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<TimeEntryService>> _mockLogger;
    private readonly TimeEntryService _service;
    
    public TimeEntryServiceTests()
    {
        _mockContext = new Mock<TimeFlowDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<TimeEntryService>>();
        _service = new TimeEntryService(_mockContext.Object, _mockMapper.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateTimeEntryAsync_ValidDto_ReturnsTimeEntryDto()
    {
        // Arrange
        var dto = new CreateTimeEntryDto
        {
            Date = DateTime.Today,
            ActualHours = 8.0m,
            BillableHours = 7.5m,
            Task = "Development work"
        };
        
        var timeEntry = new TimeEntry { Id = Guid.NewGuid() };
        var timeEntryDto = new TimeEntryDto { Id = timeEntry.Id };
        
        _mockMapper.Setup(m => m.Map<TimeEntry>(dto)).Returns(timeEntry);
        _mockMapper.Setup(m => m.Map<TimeEntryDto>(timeEntry)).Returns(timeEntryDto);
        
        // Act
        var result = await _service.CreateTimeEntryAsync(dto);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(timeEntry.Id, result.Id);
        _mockContext.Verify(c => c.TimeEntries.Add(It.IsAny<TimeEntry>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### **Integration Tests**
```csharp
// Tests/Integration/TimeEntriesControllerTests.cs
public class TimeEntriesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public TimeEntriesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetTimeEntries_AuthenticatedUser_ReturnsTimeEntries()
    {
        // Arrange
        var token = await GetValidTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.GetAsync("/api/time-entries");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<TimeEntryDto>>>(content);
        Assert.NotNull(result);
        Assert.True(result.Success);
    }
}
```

---

## 🚀 Deployment & DevOps

### **Environment Configuration**
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TimeFlow;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "https://timeflow.com",
    "Audience": "https://timeflow.com"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}

// Program.cs - Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TimeFlowDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
```

### **Docker Configuration**
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TimeFlow.API/TimeFlow.API.csproj", "TimeFlow.API/"]
COPY ["TimeFlow.Core/TimeFlow.Core.csproj", "TimeFlow.Core/"]
COPY ["TimeFlow.Infrastructure/TimeFlow.Infrastructure.csproj", "TimeFlow.Infrastructure/"]
RUN dotnet restore "TimeFlow.API/TimeFlow.API.csproj"
COPY . .
WORKDIR "/src/TimeFlow.API"
RUN dotnet build "TimeFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimeFlow.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimeFlow.API.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=TimeFlow;User Id=sa;Password=Your_password123;TrustServerCertificate=true
    depends_on:
      - db
      - redis
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_password123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
  
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  sqlserver_data:
```

### **CI/CD Pipeline (Azure DevOps)**
```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

stages:
- stage: Build
  jobs:
  - job: Build
    steps:
    - task: DotNetCoreCLI@2
      inputs:
        command: 'restore'
        projects: '**/*.csproj'
        
    - task: DotNetCoreCLI@2
      inputs:
        command: 'build'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration)'
        
    - task: DotNetCoreCLI@2
      inputs:
        command: 'test'
        projects: '**/*Tests/*.csproj'
        arguments: '--configuration $(buildConfiguration) --collect "Code coverage"'
        
    - task: DotNetCoreCLI@2
      inputs:
        command: 'publish'
        publishWebProjects: true
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        zipAfterPublish: true
        
    - task: PublishBuildArtifacts@1
      inputs:
        pathToPublish: '$(Build.ArtifactStagingDirectory)'
        artifactName: 'drop'

- stage: Deploy
  dependsOn: Build
  jobs:
  - deployment: Deploy
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: DotNetCoreCLI@2
            inputs:
              command: 'deploy'
              projects: '**/*.csproj'
              arguments: '--configuration $(buildConfiguration)'
```

---

## 📋 Implementation Checklist (ASP.NET Core 8.0)

### **Phase 1: Core Infrastructure**
- [ ] Create ASP.NET Core 8.0 Web API project
- [ ] Configure Entity Framework Core with SQL Server
- [ ] Set up ASP.NET Core Identity with JWT authentication
- [ ] Configure Redis caching
- [ ] Set up basic logging and error handling

### **Phase 2: Core APIs**
- [ ] Implement user authentication endpoints
- [ ] Create user management APIs
- [ ] Build time entry CRUD operations
- [ ] Implement weekly bulk time entry operations
- [ ] Add FluentValidation and AutoMapper

### **Phase 3: Business Logic**
- [ ] Implement approval workflow system
- [ ] Add team management functionality
- [ ] Create project/product/department management
- [ ] Build reporting and analytics APIs
- [ ] Add notification system

### **Phase 4: Advanced Features**
- [ ] Implement advanced search and filtering
- [ ] Add export functionality (CSV/PDF)
- [ ] Create performance monitoring
- [ ] Add comprehensive testing suite
- [ ] Implement caching strategies

### **Phase 5: Production Ready**
- [ ] Security hardening and penetration testing
- [ ] Performance optimization and load testing
- [ ] Monitoring and alerting setup
- [ ] Documentation and API specification
- [ ] Deployment automation and CI/CD
