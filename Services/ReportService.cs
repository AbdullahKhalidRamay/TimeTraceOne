using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class ReportService : IReportService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<ReportService> _logger;
    
    public ReportService(TimeFlowDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<UserReportDto> GetUserReportAsync(Guid userId, string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var timeEntries = await _context.TimeEntries
            .Where(t => t.UserId == userId && t.Date >= start && t.Date <= end)
            .ToListAsync();
            
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");
            
        var stats = await CalculateUserStatisticsAsync(userId, start, end);
        
        var dailyBreakdown = await GetDailyBreakdownAsync(userId, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
        
        return new UserReportDto
        {
            UserId = userId,
            UserName = user.Name,
            Period = new ReportPeriodDto { StartDate = startDate, EndDate = endDate },
            Stats = stats,
            DailyBreakdown = dailyBreakdown
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
            
        var dailyBreakdown = await GetDailyBreakdownAsync(userId, weekStart, endDate.ToString("yyyy-MM-dd"));
        
        var totalActualHours = dailyBreakdown.Sum(d => d.ActualHours);
        var totalBillableHours = dailyBreakdown.Sum(d => d.BillableHours);
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
            EntriesCount = timeEntries.Count,
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
            EntriesCount = timeEntries.Count,
            WorkingDays = workingDays,
            WeeklyBreakdown = weeklyBreakdown
        };
    }
    
    public async Task<TeamReportDto> GetTeamReportAsync(Guid teamId, string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);
            
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        var memberIds = team.Members.Select(m => m.UserId).ToList();
        var timeEntries = await _context.TimeEntries
            .Where(t => memberIds.Contains(t.UserId) && t.Date >= start && t.Date <= end)
            .ToListAsync();
            
        var teamStats = new TeamStatisticsDto
        {
            TotalMembers = team.Members.Count,
            TotalActualHours = timeEntries.Sum(t => t.ActualHours),
            TotalBillableHours = timeEntries.Sum(t => t.BillableHours),
            AverageHoursPerMember = team.Members.Count > 0 ? timeEntries.Sum(t => t.ActualHours) / team.Members.Count : 0,
            TotalEntries = timeEntries.Count
        };
        
        var memberBreakdown = await GetTeamMemberBreakdownAsync(teamId, start, end);
        var projectBreakdown = await GetTeamProjectBreakdownAsync(teamId, start, end);
        
        return new TeamReportDto
        {
            TeamId = teamId,
            TeamName = team.Name,
            Period = new ReportPeriodDto { StartDate = startDate, EndDate = endDate },
            TeamStats = teamStats,
            MemberBreakdown = memberBreakdown,
            ProjectBreakdown = projectBreakdown
        };
    }
    
    public async Task<TeamReportDto> GetTeamWeeklyReportAsync(Guid teamId, string weekStart)
    {
        var startDate = DateTime.Parse(weekStart);
        var endDate = startDate.AddDays(6);
        
        return await GetTeamReportAsync(teamId, weekStart, endDate.ToString("yyyy-MM-dd"));
    }
    
    public async Task<SystemOverviewDto> GetSystemOverviewAsync(string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var inactiveUsers = totalUsers - activeUsers;
        var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= start && u.CreatedAt <= end);
        
        var totalProjects = await _context.Projects.CountAsync();
        var activeProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Active);
        var completedProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Completed);
        
        var totalProducts = await _context.Products.CountAsync();
        var totalDepartments = await _context.Departments.CountAsync();
        var totalTeams = await _context.Teams.CountAsync();
        
        var timeEntries = await _context.TimeEntries
            .Where(t => t.Date >= start && t.Date <= end)
            .ToListAsync();
            
        var totalTimeEntries = timeEntries.Count;
        var totalActualHours = timeEntries.Sum(t => t.ActualHours);
        var totalBillableHours = timeEntries.Sum(t => t.BillableHours);
        
        var approvalRate = totalTimeEntries > 0 ? (decimal)timeEntries.Count(t => t.Status == EntryStatus.Approved) / totalTimeEntries * 100 : 0;
        var billableRate = totalActualHours > 0 ? totalBillableHours / totalActualHours * 100 : 0;
        
        var averageProjectHours = totalProjects > 0 ? totalActualHours / totalProjects : 0;
        
        return new SystemOverviewDto
        {
            Period = new ReportPeriodDto { StartDate = startDate, EndDate = endDate },
            SystemStats = new SystemStatisticsDto
            {
                TotalUsers = totalUsers,
                TotalProjects = totalProjects,
                TotalProducts = totalProducts,
                TotalDepartments = totalDepartments,
                TotalTeams = totalTeams,
                TotalTimeEntries = totalTimeEntries,
                TotalActualHours = totalActualHours,
                TotalBillableHours = totalBillableHours,
                ApprovalRate = approvalRate,
                BillableRate = billableRate
            },
            UserActivity = new UserActivityDto
            {
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                NewUsersThisMonth = newUsersThisMonth
            },
            ProjectPerformance = new ProjectPerformanceDto
            {
                ActiveProjects = activeProjects,
                CompletedProjects = completedProjects,
                AverageProjectHours = averageProjectHours
            }
        };
    }
    
    public async Task<List<DepartmentPerformanceDto>> GetDepartmentPerformanceAsync(string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var departments = await _context.Departments
            .Include(d => d.Teams)
                .ThenInclude(t => t.Members)
            .ToListAsync();
            
        var result = new List<DepartmentPerformanceDto>();
        
        foreach (var dept in departments)
        {
            var memberIds = dept.Teams.SelectMany(t => t.Members).Select(m => m.UserId).Distinct().ToList();
            
            var timeEntries = await _context.TimeEntries
                .Where(t => memberIds.Contains(t.UserId) && t.Date >= start && t.Date <= end)
                .ToListAsync();
                
            var totalActualHours = timeEntries.Sum(t => t.ActualHours);
            var totalBillableHours = timeEntries.Sum(t => t.BillableHours);
            var billableRate = totalActualHours > 0 ? totalBillableHours / totalActualHours * 100 : 0;
            var averageHoursPerMember = memberIds.Count > 0 ? totalActualHours / memberIds.Count : 0;
            
            var topProjects = await GetTopProjectsForDepartmentAsync(dept.Id, start, end);
            
            result.Add(new DepartmentPerformanceDto
            {
                Id = dept.Id,
                Name = dept.Name,
                TotalMembers = memberIds.Count,
                TotalActualHours = totalActualHours,
                TotalBillableHours = totalBillableHours,
                AverageHoursPerMember = averageHoursPerMember,
                BillableRate = billableRate,
                TopProjects = topProjects
            });
        }
        
        return result;
    }
    
    public async Task<List<ProjectPerformanceReportDto>> GetProjectPerformanceAsync(string startDate, string endDate, string? status)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var query = _context.Projects.AsQueryable();
        
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ProjectStatus>(status, true, out var projectStatus))
        {
            query = query.Where(p => p.Status == projectStatus);
        }
        
        var projects = await query.ToListAsync();
        var result = new List<ProjectPerformanceReportDto>();
        
        foreach (var project in projects)
        {
            var timeEntries = await _context.TimeEntries
                .Where(t => t.ProjectDetails.Contains(project.Name) && t.Date >= start && t.Date <= end)
                .ToListAsync();
                
            var totalActualHours = timeEntries.Sum(t => t.ActualHours);
            var totalBillableHours = timeEntries.Sum(t => t.BillableHours);
            var billableRate = totalActualHours > 0 ? totalBillableHours / totalActualHours * 100 : 0;
            
            var teamCount = await _context.TeamProjects.CountAsync(tp => tp.ProjectId == project.Id);
            var memberCount = await _context.TeamProjects
                .Where(tp => tp.ProjectId == project.Id)
                .SelectMany(tp => tp.Team.Members)
                .CountAsync();
                
            var averageHoursPerMember = memberCount > 0 ? totalActualHours / memberCount : 0;
            var completionPercentage = 0.0m; // This would need business logic to calculate
            
            result.Add(new ProjectPerformanceReportDto
            {
                Id = project.Id,
                Name = project.Name,
                Status = project.Status,
                TotalActualHours = totalActualHours,
                TotalBillableHours = totalBillableHours,
                BillableRate = billableRate,
                TeamCount = teamCount,
                MemberCount = memberCount,
                AverageHoursPerMember = averageHoursPerMember,
                CompletionPercentage = completionPercentage
            });
        }
        
        return result;
    }
    
    public Task<byte[]> ExportCsvReportAsync(ExportRequestDto request)
    {
        // Simplified CSV export - in a real application, this would generate proper CSV
        var csvContent = "Date,User,Project,Actual Hours,Billable Hours,Status\n";
        csvContent += "2024-01-15,John Doe,Mobile App,8.0,7.5,Approved\n";
        
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(csvContent));
    }
    
    public Task<byte[]> ExportPdfReportAsync(ExportRequestDto request)
    {
        // Simplified PDF export - in a real application, this would generate proper PDF
        var pdfContent = "PDF Report Content";
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(pdfContent));
    }
    
    private async Task<UserStatisticsDto> CalculateUserStatisticsAsync(Guid userId, DateTime start, DateTime end)
    {
        var timeEntries = await _context.TimeEntries
            .Where(t => t.UserId == userId && t.Date >= start && t.Date <= end)
            .ToListAsync();
            
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
    
    private async Task<List<DailyBreakdownDto>> GetDailyBreakdownAsync(Guid userId, string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var timeEntries = await _context.TimeEntries
            .Where(t => t.UserId == userId && t.Date >= start && t.Date <= end)
            .ToListAsync();
            
        var dailyBreakdown = new List<DailyBreakdownDto>();
        var currentDate = start;
        
        while (currentDate <= end)
        {
            var dayEntries = timeEntries.Where(t => t.Date.Date == currentDate.Date).ToList();
            
            var dailyEntry = new DailyBreakdownDto
            {
                Date = currentDate.ToString("yyyy-MM-dd"),
                ActualHours = dayEntries.Sum(t => t.ActualHours),
                BillableHours = dayEntries.Sum(t => t.BillableHours),
                EntriesCount = dayEntries.Count
            };
            
            dailyBreakdown.Add(dailyEntry);
            currentDate = currentDate.AddDays(1);
        }
        
        return dailyBreakdown;
    }
    
    private async Task<List<TeamMemberBreakdownDto>> GetTeamMemberBreakdownAsync(Guid teamId, DateTime start, DateTime end)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == teamId);
            
        if (team == null) return new List<TeamMemberBreakdownDto>();
        
        var result = new List<TeamMemberBreakdownDto>();
        
        foreach (var member in team.Members)
        {
            var timeEntries = await _context.TimeEntries
                .Where(t => t.UserId == member.UserId && t.Date >= start && t.Date <= end)
                .ToListAsync();
                
            result.Add(new TeamMemberBreakdownDto
            {
                UserId = member.UserId,
                UserName = member.User.Name,
                ActualHours = timeEntries.Sum(t => t.ActualHours),
                BillableHours = timeEntries.Sum(t => t.BillableHours),
                EntriesCount = timeEntries.Count
            });
        }
        
        return result;
    }
    
    private async Task<List<ProjectBreakdownDto>> GetTeamProjectBreakdownAsync(Guid teamId, DateTime start, DateTime end)
    {
        var teamProjects = await _context.TeamProjects
            .Where(tp => tp.TeamId == teamId)
            .Select(tp => tp.ProjectId)
            .ToListAsync();
            
        var result = new List<ProjectBreakdownDto>();
        
        foreach (var projectId in teamProjects)
        {
            var timeEntries = await _context.TimeEntries
                .Where(t => t.ProjectDetails.Contains(projectId.ToString()) && t.Date >= start && t.Date <= end)
                .ToListAsync();
                
            var project = await _context.Projects.FindAsync(projectId);
            if (project != null)
            {
                result.Add(new ProjectBreakdownDto
                {
                    ProjectId = projectId,
                    ProjectName = project.Name,
                    ActualHours = timeEntries.Sum(t => t.ActualHours),
                    BillableHours = timeEntries.Sum(t => t.BillableHours)
                });
            }
        }
        
        return result;
    }
    
    private Task<List<ProjectBreakdownDto>> GetTopProjectsForDepartmentAsync(Guid departmentId, DateTime start, DateTime end)
    {
        // Simplified implementation - in a real application, this would be more sophisticated
        return Task.FromResult(new List<ProjectBreakdownDto>());
    }
    
    public async Task<List<UserReportDto>> GetUsersReportAsync(string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var users = await _context.Users.ToListAsync();
        var reports = new List<UserReportDto>();
        
        foreach (var user in users)
        {
            var report = await GetUserReportAsync(user.Id, startDate, endDate);
            reports.Add(report);
        }
        
        return reports;
    }
    
    public async Task<List<TeamReportDto>> GetTeamsReportAsync(string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var teams = await _context.Teams.ToListAsync();
        var reports = new List<TeamReportDto>();
        
        foreach (var team in teams)
        {
            var report = await GetTeamReportAsync(team.Id, startDate, endDate);
            reports.Add(report);
        }
        
        return reports;
    }
    
    public async Task<DepartmentPerformanceDto> GetDepartmentPerformanceByIdAsync(Guid departmentId, string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var department = await _context.Departments.FindAsync(departmentId);
        if (department == null)
            throw new InvalidOperationException("Department not found");
            
        var teams = await _context.Teams
            .Where(t => t.DepartmentId == departmentId)
            .ToListAsync();
            
        var totalHours = 0m;
        var totalBillableHours = 0m;
        var totalProjects = 0;
        
        foreach (var team in teams)
        {
            var teamReport = await GetTeamReportAsync(team.Id, startDate, endDate);
            totalHours += teamReport.TeamStats.TotalActualHours;
            totalBillableHours += teamReport.TeamStats.TotalBillableHours;
            totalProjects += teamReport.ProjectBreakdown.Count;
        }
        
        return new DepartmentPerformanceDto
        {
            DepartmentId = departmentId,
            DepartmentName = department.Name,
            TotalHours = totalHours,
            TotalBillableHours = totalBillableHours,
            TotalProjects = totalProjects,
            TeamCount = teams.Count
        };
    }
    
    public async Task<ProjectPerformanceReportDto> GetProjectPerformanceByIdAsync(Guid projectId, string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
            throw new InvalidOperationException("Project not found");
            
        var timeEntries = await _context.TimeEntries
            .Where(t => t.ProjectDetails.Contains(projectId.ToString()) && t.Date >= start && t.Date <= end)
            .ToListAsync();
            
        var totalHours = timeEntries.Sum(t => t.ActualHours);
        var totalBillableHours = timeEntries.Sum(t => t.BillableHours);
        var workingDays = timeEntries.Select(t => t.Date.Date).Distinct().Count();
        
        return new ProjectPerformanceReportDto
        {
            Id = projectId,
            Name = project.Name,
            TotalActualHours = totalHours,
            TotalBillableHours = totalBillableHours,
            TeamCount = 1, // Simplified - in real app would count actual teams
            MemberCount = 1, // Simplified - in real app would count actual members
            AverageHoursPerMember = totalHours,
            CompletionPercentage = 0 // Simplified - in real app would calculate based on project status
        };
    }
    
    public async Task<TimeEntrySearchResultDto> SearchTimeEntriesAsync(string query, string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var timeEntries = await _context.TimeEntries
            .Include(t => t.User)
            .Where(t => (t.Task.Contains(query) || t.ProjectDetails.Contains(query)) && 
                       t.Date >= start && t.Date <= end)
            .ToListAsync();
            
        var results = timeEntries.Select(t => new TimeEntrySearchItemDto
        {
            Id = t.Id,
            Date = t.Date.ToString("yyyy-MM-dd"),
            UserName = t.User.Name,
            Task = t.Task,
            ActualHours = t.ActualHours,
            BillableHours = t.BillableHours,
            Status = t.Status
        }).ToList();
        
        return new TimeEntrySearchResultDto
        {
            Query = query,
            Results = results,
            TotalResults = results.Count
        };
    }
    
    public async Task<List<ProjectPerformanceReportDto>> GetTopProjectsForDepartmentAsync(Guid departmentId, string startDate, string endDate)
    {
        var start = DateTime.Parse(startDate);
        var end = DateTime.Parse(endDate);
        
        var teams = await _context.Teams
            .Where(t => t.DepartmentId == departmentId)
            .ToListAsync();
            
        var projectIds = new HashSet<Guid>();
        foreach (var team in teams)
        {
            var teamProjects = await _context.TeamProjects
                .Where(tp => tp.TeamId == team.Id)
                .Select(tp => tp.ProjectId)
                .ToListAsync();
            projectIds.UnionWith(teamProjects);
        }
        
        var projects = new List<ProjectPerformanceReportDto>();
        foreach (var projectId in projectIds)
        {
            var project = await GetProjectPerformanceByIdAsync(projectId, startDate, endDate);
            projects.Add(project);
        }
        
        return projects.OrderByDescending(p => p.TotalActualHours).Take(10).ToList();
    }
}
