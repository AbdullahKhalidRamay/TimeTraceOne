using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class UserService : IUserService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<UserService> _logger;
    
    public UserService(TimeFlowDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<PaginatedResponse<UserListDto>> GetUsersAsync(UserFilterDto filter)
    {
        var query = _context.Users
            .Include(u => u.TeamMemberships)
                .ThenInclude(tm => tm.Team)
            .AsQueryable();
            
        // Apply filters
        if (filter.Role.HasValue)
            query = query.Where(u => u.Role == filter.Role.Value);
            
        if (filter.DepartmentId.HasValue)
            query = query.Where(u => u.TeamMemberships.Any(tm => tm.Team.DepartmentId == filter.DepartmentId.Value));
            
        if (filter.TeamId.HasValue)
            query = query.Where(u => u.TeamMemberships.Any(tm => tm.TeamId == filter.TeamId.Value));
            
        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
            
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(u => u.Name.Contains(filter.Search) || u.Email.Contains(filter.Search));
            
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / filter.Limit);
        
        var users = await query
            .OrderBy(u => u.Name)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync();
            
        var userDtos = users.Select(MapToUserListDto).ToList();
        
        return PaginatedResponse<UserListDto>.CreateSuccess(userDtos, filter.Page, filter.Limit, total, totalPages);
    }
    
    public async Task<UserDetailDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.TeamMemberships)
                .ThenInclude(tm => tm.Team)
            .Include(u => u.TeamMemberships)
                .ThenInclude(tm => tm.Team.Department)
            .FirstOrDefaultAsync(u => u.Id == id);
            
        return user != null ? MapToUserDetailDto(user) : null;
    }
    
    public async Task<UserDetailDto> CreateUserAsync(CreateUserDto dto)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("User with this email already exists");
            
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            JobTitle = dto.JobTitle,
            AvailableHours = dto.AvailableHours,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Users.Add(user);
        
        // Add team memberships if specified
        if (dto.TeamIds?.Any() == true)
        {
            foreach (var teamId in dto.TeamIds)
            {
                var teamMember = new TeamMember
                {
                    TeamId = teamId,
                    UserId = user.Id,
                    JoinedAt = DateTime.UtcNow
                };
                _context.TeamMembers.Add(teamMember);
            }
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created user {UserId} with email {Email}", user.Id, user.Email);
        
        return await GetUserByIdAsync(user.Id) ?? throw new InvalidOperationException("Failed to retrieve created user");
    }
    
    public async Task<UserDetailDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _context.Users
            .Include(u => u.TeamMemberships)
            .FirstOrDefaultAsync(u => u.Id == id);
            
        if (user == null)
            throw new InvalidOperationException("User not found");
            
        if (dto.Name != null)
            user.Name = dto.Name;
            
        if (dto.JobTitle != null)
            user.JobTitle = dto.JobTitle;
            
        if (dto.AvailableHours.HasValue)
            user.AvailableHours = dto.AvailableHours.Value;
            
        if (dto.Role.HasValue)
            user.Role = dto.Role.Value;
            
        if (dto.IsActive.HasValue)
            user.IsActive = dto.IsActive.Value;
            
        user.UpdatedAt = DateTime.UtcNow;
        
        // Update team memberships if specified
        if (dto.TeamIds != null)
        {
            // Remove existing memberships
            var existingMemberships = user.TeamMemberships.ToList();
            foreach (var membership in existingMemberships)
            {
                _context.TeamMembers.Remove(membership);
            }
            
            // Add new memberships
            foreach (var teamId in dto.TeamIds)
            {
                var teamMember = new TeamMember
                {
                    TeamId = teamId,
                    UserId = user.Id,
                    JoinedAt = DateTime.UtcNow
                };
                _context.TeamMembers.Add(teamMember);
            }
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated user {UserId}", id);
        
        return await GetUserByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated user");
    }
    
    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new InvalidOperationException("User not found");
            
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted user {UserId}", id);
    }
    
    public async Task<UserStatisticsDto> GetUserStatisticsAsync(Guid userId, string? startDate, string? endDate)
    {
        var query = _context.TimeEntries.Where(t => t.UserId == userId);
        
        if (!string.IsNullOrEmpty(startDate))
        {
            var start = DateTime.Parse(startDate);
            query = query.Where(t => t.Date >= start);
        }
        
        if (!string.IsNullOrEmpty(endDate))
        {
            var end = DateTime.Parse(endDate);
            query = query.Where(t => t.Date <= end);
        }
        
        var timeEntries = await query.ToListAsync();
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
            throw new InvalidOperationException("User not found");
            
        var totalEntries = timeEntries.Count;
        var approvedEntries = timeEntries.Count(t => t.Status == EntryStatus.Approved);
        var pendingEntries = timeEntries.Count(t => t.Status == EntryStatus.Pending);
        var rejectedEntries = timeEntries.Count(t => t.Status == EntryStatus.Rejected);
        
        var totalActualHours = timeEntries.Sum(t => t.ActualHours);
        var totalBillableHours = timeEntries.Sum(t => t.BillableHours);
        
        var workingDays = timeEntries.Select(t => t.Date.Date).Distinct().Count();
        var averageHoursPerDay = workingDays > 0 ? totalActualHours / workingDays : 0;
        
        var overtimeHours = Math.Max(0, totalActualHours - (workingDays * user.AvailableHours));
        
        var approvalRate = totalEntries > 0 ? (decimal)approvedEntries / totalEntries * 100 : 0;
        var billableRate = totalActualHours > 0 ? totalBillableHours / totalActualHours * 100 : 0;
        
        return new UserStatisticsDto
        {
            UserId = userId,
            UserName = user.Name,
            TotalEntries = totalEntries,
            ApprovedEntries = approvedEntries,
            PendingEntries = pendingEntries,
            RejectedEntries = rejectedEntries,
            TotalActualHours = totalActualHours,
            TotalBillableHours = totalBillableHours,
            AverageHoursPerDay = averageHoursPerDay,
            OvertimeHours = overtimeHours,
            ApprovalRate = approvalRate,
            BillableRate = billableRate
        };
    }
    
    public async Task<UserWeeklyReportDto> GetUserWeeklyReportAsync(Guid userId, string weekStart)
    {
        var startDate = DateTime.Parse(weekStart);
        var endDate = startDate.AddDays(6);
        
        var timeEntries = await _context.TimeEntries
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();
            
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");
            
        var dailyBreakdown = new List<DailyBreakdownDto>();
        var totalActualHours = 0.0m;
        var totalBillableHours = 0.0m;
        var totalEntries = 0;
        
        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            var dayEntries = timeEntries.Where(t => t.Date.Date == date.Date).ToList();
            
            var dailyEntry = new DailyBreakdownDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                ActualHours = dayEntries.Sum(t => t.ActualHours),
                BillableHours = dayEntries.Sum(t => t.BillableHours),
                EntriesCount = dayEntries.Count
            };
            
            dailyBreakdown.Add(dailyEntry);
            totalActualHours += dailyEntry.ActualHours;
            totalBillableHours += dailyEntry.BillableHours;
            totalEntries += dailyEntry.EntriesCount;
        }
        
        var averageHoursPerDay = totalActualHours / 7;
        var overtimeHours = Math.Max(0, totalActualHours - (7 * user.AvailableHours));
        
        return new UserWeeklyReportDto
        {
            UserId = userId,
            UserName = user.Name,
            WeekStart = weekStart,
            WeekEnd = endDate.ToString("yyyy-MM-dd"),
            TotalActualHours = totalActualHours,
            TotalBillableHours = totalBillableHours,
            AverageHoursPerDay = averageHoursPerDay,
            OvertimeHours = overtimeHours,
            EntriesCount = totalEntries,
            DailyBreakdown = dailyBreakdown
        };
    }
    
    public async Task<UserMonthlyReportDto> GetUserMonthlyReportAsync(Guid userId, string month)
    {
        var startDate = DateTime.Parse(month + "-01");
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        var timeEntries = await _context.TimeEntries
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();
            
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");
            
        var totalActualHours = timeEntries.Sum(t => t.ActualHours);
        var totalBillableHours = timeEntries.Sum(t => t.BillableHours);
        var totalEntries = timeEntries.Count;
        
        var workingDays = timeEntries.Select(t => t.Date.Date).Distinct().Count();
        var averageHoursPerDay = workingDays > 0 ? totalActualHours / workingDays : 0;
        
        var overtimeHours = Math.Max(0, totalActualHours - (workingDays * user.AvailableHours));
        
        // Weekly breakdown
        var weeklyBreakdown = new List<WeeklyBreakdownDto>();
        var currentWeekStart = startDate;
        
        while (currentWeekStart <= endDate)
        {
            var weekEnd = currentWeekStart.AddDays(6);
            if (weekEnd > endDate) weekEnd = endDate;
            
            var weekEntries = timeEntries.Where(t => t.Date >= currentWeekStart && t.Date <= weekEnd).ToList();
            
            var weeklyEntry = new WeeklyBreakdownDto
            {
                WeekStart = currentWeekStart.ToString("yyyy-MM-dd"),
                WeekEnd = weekEnd.ToString("yyyy-MM-dd"),
                ActualHours = weekEntries.Sum(t => t.ActualHours),
                BillableHours = weekEntries.Sum(t => t.BillableHours)
            };
            
            weeklyBreakdown.Add(weeklyEntry);
            currentWeekStart = currentWeekStart.AddDays(7);
        }
        
        return new UserMonthlyReportDto
        {
            UserId = userId,
            UserName = user.Name,
            Month = month,
            TotalActualHours = totalActualHours,
            TotalBillableHours = totalBillableHours,
            AverageHoursPerDay = averageHoursPerDay,
            OvertimeHours = overtimeHours,
            EntriesCount = totalEntries,
            WorkingDays = workingDays,
            WeeklyBreakdown = weeklyBreakdown
        };
    }
    
    public async Task<UserAvailableHoursDto> GetUserAvailableHoursAsync(Guid userId, string date)
    {
        var targetDate = DateTime.Parse(date);
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
            throw new InvalidOperationException("User not found");
            
        var usedHours = await _context.TimeEntries
            .Where(t => t.UserId == userId && t.Date.Date == targetDate.Date)
            .SumAsync(t => t.ActualHours);
            
        var availableHours = user.AvailableHours;
        var remainingHours = Math.Max(0, availableHours - usedHours);
        var overtimeHours = Math.Max(0, usedHours - availableHours);
        
        return new UserAvailableHoursDto
        {
            UserId = userId,
            Date = date,
            AvailableHours = availableHours,
            UsedHours = usedHours,
            RemainingHours = remainingHours,
            OvertimeHours = overtimeHours
        };
    }
    
    public async Task<List<ProjectSummaryDto>> GetUserAssociatedProjectsAsync(Guid userId)
    {
        var projects = await _context.Projects
            .Where(p => p.CreatedBy == userId || 
                       p.TeamProjects.Any(tp => tp.Team.Members.Any(m => m.UserId == userId)))
            .Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                IsBillable = p.IsBillable,
                Status = p.Status
            })
            .ToListAsync();
            
        return projects;
    }
    
    private static UserListDto MapToUserListDto(User user)
    {
        return new UserListDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            JobTitle = user.JobTitle,
            AvailableHours = user.AvailableHours,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            DepartmentIds = user.TeamMemberships.Select(tm => tm.Team.DepartmentId).Distinct().ToList(),
            TeamIds = user.TeamMemberships.Select(tm => tm.TeamId).ToList()
        };
    }
    
    private static UserDetailDto MapToUserDetailDto(User user)
    {
        var userListDto = MapToUserListDto(user);
        
        return new UserDetailDto
        {
            Id = userListDto.Id,
            Name = userListDto.Name,
            Email = userListDto.Email,
            Role = userListDto.Role,
            JobTitle = userListDto.JobTitle,
            AvailableHours = userListDto.AvailableHours,
            IsActive = userListDto.IsActive,
            CreatedAt = userListDto.CreatedAt,
            UpdatedAt = userListDto.UpdatedAt,
            DepartmentIds = userListDto.DepartmentIds,
            TeamIds = userListDto.TeamIds,
            DepartmentNames = user.TeamMemberships.Select(tm => tm.Team.Department.Name).Distinct().ToList(),
            TeamNames = user.TeamMemberships.Select(tm => tm.Team.Name).ToList()
        };
    }
}
