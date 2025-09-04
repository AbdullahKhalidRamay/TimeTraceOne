using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<TimeEntryService> _logger;
    
    public TimeEntryService(TimeFlowDbContext context, ILogger<TimeEntryService> logger)
    {
        _context = context;
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
            
        if (filter.IsBillable.HasValue)
            query = query.Where(t => t.IsBillable == filter.IsBillable.Value);
            
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(t => t.Task.Contains(filter.Search) || 
                                   t.ProjectDetails.Contains(filter.Search));
            
        var timeEntries = await query
            .OrderByDescending(t => t.Date)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync();
            
        return timeEntries.Select(MapToDto).ToList();
    }
    
    public async Task<TimeEntryDto?> GetTimeEntryByIdAsync(Guid id)
    {
        var timeEntry = await _context.TimeEntries
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        return timeEntry != null ? MapToDto(timeEntry) : null;
    }
    
    public async Task<TimeEntryDto> CreateTimeEntryAsync(CreateTimeEntryDto dto, Guid userId)
    {
        var timeEntry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = dto.Date,
            ActualHours = dto.ActualHours,
            BillableHours = dto.BillableHours,
            TotalHours = dto.ActualHours,
            AvailableHours = 8.0m, // Default value, should come from user settings
            Task = dto.Task,
            ProjectDetails = JsonSerializer.Serialize(dto.ProjectDetails),
            IsBillable = dto.IsBillable,
            Status = EntryStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.TimeEntries.Add(timeEntry);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created time entry {Id} for user {UserId}", timeEntry.Id, userId);
        
        return await GetTimeEntryByIdAsync(timeEntry.Id) ?? throw new InvalidOperationException("Failed to retrieve created time entry");
    }
    
    public async Task<TimeEntryDto> UpdateTimeEntryAsync(Guid id, UpdateTimeEntryDto dto, Guid userId, string userRole)
    {
        var timeEntry = await _context.TimeEntries
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (timeEntry == null)
            throw new InvalidOperationException("Time entry not found");
            
        // Check if user can update this time entry
        if (timeEntry.UserId != userId && userRole != "Owner" && userRole != "Manager")
            throw new InvalidOperationException("Access denied - you can only update your own time entries");
            
        if (dto.ActualHours.HasValue)
            timeEntry.ActualHours = dto.ActualHours.Value;
            
        if (dto.BillableHours.HasValue)
            timeEntry.BillableHours = dto.BillableHours.Value;
            
        if (dto.Task != null)
            timeEntry.Task = dto.Task;
            
        if (dto.ProjectDetails != null)
            timeEntry.ProjectDetails = JsonSerializer.Serialize(dto.ProjectDetails);
            
        if (dto.IsBillable.HasValue)
            timeEntry.IsBillable = dto.IsBillable.Value;
            
        timeEntry.TotalHours = timeEntry.ActualHours;
        timeEntry.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated time entry {Id} for user {UserId}", id, userId);
        
        return await GetTimeEntryByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated time entry");
    }
    
    public async Task DeleteTimeEntryAsync(Guid id, Guid userId, string userRole)
    {
        var timeEntry = await _context.TimeEntries
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (timeEntry == null)
            throw new InvalidOperationException("Time entry not found");
            
        // Check if user can delete this time entry
        if (timeEntry.UserId != userId && userRole != "Owner" && userRole != "Manager")
            throw new InvalidOperationException("Access denied - you can only delete your own time entries");
            
        _context.TimeEntries.Remove(timeEntry);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted time entry {Id} for user {UserId}", id, userId);
    }
    
    public async Task<WeeklyBulkResponseDto> CreateWeeklyBulkAsync(WeeklyBulkRequestDto dto, Guid userId)
    {
        var weekStart = DateTime.Parse(dto.WeekStart);
        var weekEnd = weekStart.AddDays(6);
        var createdEntries = 0;
        var skippedEntries = 0;
        
        foreach (var entry in dto.Entries)
        {
            foreach (var dailyEntry in entry.DailyHours)
            {
                if (dailyEntry.Value.ActualHours <= 0 && dailyEntry.Value.BillableHours <= 0)
                {
                    skippedEntries++;
                    continue;
                }
                
                var timeEntry = new TimeEntry
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Date = DateTime.Parse(dailyEntry.Key),
                    ActualHours = dailyEntry.Value.ActualHours,
                    BillableHours = dailyEntry.Value.BillableHours,
                    TotalHours = dailyEntry.Value.ActualHours,
                    AvailableHours = 8.0m,
                    Task = dailyEntry.Value.Task,
                    ProjectDetails = JsonSerializer.Serialize(new ProjectDetailsDto
                    {
                        Category = "project", // Default category
                        Name = "Weekly Entry",
                        Task = dailyEntry.Value.Task,
                        Description = dailyEntry.Value.Task
                    }),
                    IsBillable = true, // Default to billable
                    Status = EntryStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.TimeEntries.Add(timeEntry);
                createdEntries++;
            }
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created {CreatedEntries} weekly bulk time entries for user {UserId}", createdEntries, userId);
        
        return new WeeklyBulkResponseDto
        {
            Message = "Weekly time entries created successfully",
            CreatedEntries = createdEntries,
            SkippedEntries = skippedEntries,
            WeekStart = dto.WeekStart,
            WeekEnd = weekEnd.ToString("yyyy-MM-dd")
        };
    }
    
    public async Task<WeeklyTimeEntriesDto> GetWeeklyTimeEntriesAsync(string weekStart, Guid? userId = null)
    {
        var startDate = DateTime.Parse(weekStart);
        var endDate = startDate.AddDays(6);
        
        var query = _context.TimeEntries
            .Include(t => t.User)
            .Where(t => t.Date >= startDate && t.Date <= endDate);
            
        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);
            
        var timeEntries = await query.ToListAsync();
        
        var dailyEntries = new List<DailyEntriesDto>();
        var totalActualHours = 0.0m;
        var totalBillableHours = 0.0m;
        var totalEntries = 0;
        var pendingEntries = 0;
        var approvedEntries = 0;
        
        for (int i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            var dayEntries = timeEntries.Where(t => t.Date.Date == date.Date).ToList();
            
            var dailyEntry = new DailyEntriesDto
            {
                Date = date,
                Entries = dayEntries.Select(MapToDto).ToList(),
                TotalActualHours = dayEntries.Sum(t => t.ActualHours),
                TotalBillableHours = dayEntries.Sum(t => t.BillableHours)
            };
            
            dailyEntries.Add(dailyEntry);
            totalActualHours += dailyEntry.TotalActualHours;
            totalBillableHours += dailyEntry.TotalBillableHours;
            totalEntries += dailyEntry.Entries.Count;
            pendingEntries += dailyEntry.Entries.Count(e => e.Status == EntryStatus.Pending);
            approvedEntries += dailyEntry.Entries.Count(e => e.Status == EntryStatus.Approved);
        }
        
        var weeklySummary = new WeeklySummaryDto
        {
            TotalActualHours = totalActualHours,
            TotalBillableHours = totalBillableHours,
            TotalEntries = totalEntries,
            PendingEntries = pendingEntries,
            ApprovedEntries = approvedEntries
        };
        
        return new WeeklyTimeEntriesDto
        {
            WeekStart = weekStart,
            WeekEnd = endDate.ToString("yyyy-MM-dd"),
            Entries = dailyEntries,
            WeeklySummary = weeklySummary
        };
    }
    
    public async Task<TimeEntryDto> ApproveTimeEntryAsync(Guid id, string? message, Guid approverId)
    {
        var timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry == null)
            throw new InvalidOperationException("Time entry not found");
            
        var previousStatus = timeEntry.Status;
        timeEntry.Status = EntryStatus.Approved;
        timeEntry.UpdatedAt = DateTime.UtcNow;
        
        // Create approval history
        var approvalHistory = new ApprovalHistory
        {
            Id = Guid.NewGuid(),
            EntryId = id,
            PreviousStatus = previousStatus.ToString(),
            NewStatus = EntryStatus.Approved.ToString(),
            Message = message,
            ApprovedBy = approverId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.ApprovalHistory.Add(approvalHistory);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Time entry {Id} approved by {ApproverId}", id, approverId);
        
        return await GetTimeEntryByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve approved time entry");
    }
    
    public async Task<TimeEntryDto> RejectTimeEntryAsync(Guid id, string message, Guid rejectorId)
    {
        var timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry == null)
            throw new InvalidOperationException("Time entry not found");
            
        var previousStatus = timeEntry.Status;
        timeEntry.Status = EntryStatus.Rejected;
        timeEntry.UpdatedAt = DateTime.UtcNow;
        
        // Create approval history
        var approvalHistory = new ApprovalHistory
        {
            Id = Guid.NewGuid(),
            EntryId = id,
            PreviousStatus = previousStatus.ToString(),
            NewStatus = EntryStatus.Rejected.ToString(),
            Message = message,
            ApprovedBy = rejectorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.ApprovalHistory.Add(approvalHistory);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Time entry {Id} rejected by {RejectorId}", id, rejectorId);
        
        return await GetTimeEntryByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve rejected time entry");
    }
    
    private static TimeEntryDto MapToDto(TimeEntry timeEntry)
    {
        var projectDetails = new ProjectDetailsDto();
        try
        {
            if (!string.IsNullOrEmpty(timeEntry.ProjectDetails))
            {
                projectDetails = JsonSerializer.Deserialize<ProjectDetailsDto>(timeEntry.ProjectDetails) ?? new ProjectDetailsDto();
            }
        }
        catch
        {
            // If deserialization fails, use default values
        }
        
        return new TimeEntryDto
        {
            Id = timeEntry.Id,
            UserId = timeEntry.UserId,
            UserName = timeEntry.User?.Name ?? string.Empty,
            Date = timeEntry.Date,
            ActualHours = timeEntry.ActualHours,
            BillableHours = timeEntry.BillableHours,
            TotalHours = timeEntry.TotalHours,
            AvailableHours = timeEntry.AvailableHours,
            Task = timeEntry.Task,
            ProjectDetails = projectDetails,
            IsBillable = timeEntry.IsBillable,
            Status = timeEntry.Status,
            CreatedAt = timeEntry.CreatedAt,
            UpdatedAt = timeEntry.UpdatedAt
        };
    }
    
    public async Task<TimeEntryStatusDto> GetTimeEntryStatusAsync(string date, Guid? userId = null)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            throw new InvalidOperationException("Invalid date format");
            
        var query = _context.TimeEntries.Where(t => t.Date.Date == parsedDate.Date);
        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);
            
        var entries = await query.ToListAsync();
        
        var hasEntries = entries.Any();
        var totalActualHours = entries.Sum(t => t.ActualHours);
        var totalBillableHours = entries.Sum(t => t.BillableHours);
        var entriesCount = entries.Count;
        var statuses = entries.Select(t => t.Status).Distinct().ToList();
        
        var statusEntries = entries.Select(t => new TimeEntryStatusEntryDto
        {
            Id = t.Id,
            ProjectDetails = JsonSerializer.Deserialize<ProjectDetailsDto>(t.ProjectDetails ?? "{}") ?? new ProjectDetailsDto(),
            ActualHours = t.ActualHours,
            BillableHours = t.BillableHours,
            Status = t.Status
        }).ToList();
        
        return new TimeEntryStatusDto
        {
            Date = date,
            Status = new TimeEntryStatusInfoDto
            {
                HasEntries = hasEntries,
                TotalActualHours = totalActualHours,
                TotalBillableHours = totalBillableHours,
                EntriesCount = entriesCount,
                Statuses = statuses,
                Entries = statusEntries
            }
        };
    }
    
    public async Task<TimeEntrySearchResultDto> SearchTimeEntriesAsync(string query, Guid? userId = null, string? startDate = null, string? endDate = null)
    {
        var searchQuery = _context.TimeEntries
            .Include(t => t.User)
            .Where(t => t.Task.Contains(query) || t.ProjectDetails.Contains(query));
            
        if (userId.HasValue)
            searchQuery = searchQuery.Where(t => t.UserId == userId.Value);
            
        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            searchQuery = searchQuery.Where(t => t.Date >= start);
            
        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            searchQuery = searchQuery.Where(t => t.Date <= end);
            
        var results = await searchQuery.ToListAsync();
        
        var searchResults = results.Select(t => new TimeEntrySearchItemDto
        {
            Id = t.Id,
            Task = t.Task,
            ProjectDetails = JsonSerializer.Deserialize<ProjectDetailsDto>(t.ProjectDetails ?? "{}") ?? new ProjectDetailsDto(),
            Date = t.Date.ToString("yyyy-MM-dd"),
            ActualHours = t.ActualHours,
            Status = t.Status
        }).ToList();
        
        return new TimeEntrySearchResultDto
        {
            Query = query,
            Results = searchResults,
            TotalResults = searchResults.Count
        };
    }
    
    public async Task<TimeEntryFilterResultDto> FilterTimeEntriesAsync(TimeEntryAdvancedFilterDto filter)
    {
        var query = _context.TimeEntries
            .Include(t => t.User)
            .AsQueryable();
            
        if (filter.UserId.HasValue)
            query = query.Where(t => t.UserId == filter.UserId.Value);
            
        if (filter.ProjectId.HasValue)
            query = query.Where(t => t.ProjectDetails.Contains(filter.ProjectId.Value.ToString()));
            
        if (filter.ProductId.HasValue)
            query = query.Where(t => t.ProjectDetails.Contains(filter.ProductId.Value.ToString()));
            
        if (filter.DepartmentId.HasValue)
            query = query.Where(t => t.ProjectDetails.Contains(filter.DepartmentId.Value.ToString()));
            
        if (filter.Status != null && filter.Status.Any())
            query = query.Where(t => filter.Status.Contains(t.Status));
            
        if (filter.IsBillable.HasValue)
            query = query.Where(t => t.IsBillable == filter.IsBillable.Value);
            
        if (filter.MinHours.HasValue)
            query = query.Where(t => t.ActualHours >= filter.MinHours.Value);
            
        if (filter.MaxHours.HasValue)
            query = query.Where(t => t.ActualHours <= filter.MaxHours.Value);
            
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.Date >= filter.StartDate.Value);
            
        if (filter.EndDate.HasValue)
            query = query.Where(t => t.Date <= filter.EndDate.Value);
            
        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "date" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.Date) : query.OrderBy(t => t.Date),
            "hours" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.ActualHours) : query.OrderBy(t => t.ActualHours),
            "status" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            _ => query.OrderByDescending(t => t.Date)
        };
            
        var results = await query.ToListAsync();
        
        var filterResults = results.Select(t => new TimeEntryFilterItemDto
        {
            Id = t.Id,
            Date = t.Date,
            ActualHours = t.ActualHours,
            BillableHours = t.BillableHours,
            Status = t.Status
        }).ToList();
        
        var summary = new TimeEntryFilterSummaryDto
        {
            TotalActualHours = results.Sum(t => t.ActualHours),
            TotalBillableHours = results.Sum(t => t.BillableHours),
            AverageHoursPerDay = results.Any() ? results.Average(t => t.ActualHours) : 0
        };
        
        return new TimeEntryFilterResultDto
        {
            Filters = filter,
            Results = filterResults,
            TotalResults = filterResults.Count,
            Summary = summary
        };
    }
    
    public async Task<WeeklyBulkResponseDto> UpdateWeeklyTimeEntriesAsync(string weekStart, UpdateWeeklyTimeEntriesDto dto, Guid userId)
    {
        if (!DateTime.TryParse(weekStart, out var startDate))
            throw new InvalidOperationException("Invalid week start date format");
            
        var endDate = startDate.AddDays(6);
        var updatedEntries = 0;
        
        foreach (var entryUpdate in dto.Entries)
        {
            if (!Guid.TryParse(entryUpdate.Id, out var entryId))
                continue;
                
            var timeEntry = await _context.TimeEntries.FindAsync(entryId);
            if (timeEntry == null || timeEntry.UserId != userId)
                continue;
                
            if (entryUpdate.ActualHours.HasValue)
                timeEntry.ActualHours = entryUpdate.ActualHours.Value;
                
            if (entryUpdate.BillableHours.HasValue)
                timeEntry.BillableHours = entryUpdate.BillableHours.Value;
                
            if (!string.IsNullOrEmpty(entryUpdate.Task))
                timeEntry.Task = entryUpdate.Task;
                
            timeEntry.UpdatedAt = DateTime.UtcNow;
            updatedEntries++;
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated {UpdatedEntries} weekly time entries for user {UserId}", updatedEntries, userId);
        
        return new WeeklyBulkResponseDto
        {
            Message = "Weekly time entries updated successfully",
            CreatedEntries = 0,
            SkippedEntries = 0,
            WeekStart = weekStart,
            WeekEnd = endDate.ToString("yyyy-MM-dd")
        };
    }
    
    public async Task<List<TimeEntryDto>> GetTimeEntriesByDateAsync(string date, Guid? userId = null)
    {
        if (!DateTime.TryParse(date, out var targetDate))
            throw new InvalidOperationException("Invalid date format");
            
        var query = _context.TimeEntries
            .Include(t => t.User)
            .Where(t => t.Date.Date == targetDate.Date);
            
        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);
            
        var timeEntries = await query
            .OrderBy(t => t.Date)
            .ToListAsync();
            
        return timeEntries.Select(MapToDto).ToList();
    }
    
    public async Task<List<TimeEntryDto>> GetTimeEntriesByUserAsync(Guid userId, string? startDate = null, string? endDate = null)
    {
        var query = _context.TimeEntries
            .Include(t => t.User)
            .Where(t => t.UserId == userId);
            
        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            query = query.Where(t => t.Date >= start.Date);
            
        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            query = query.Where(t => t.Date <= end.Date);
            
        var timeEntries = await query
            .OrderByDescending(t => t.Date)
            .ToListAsync();
            
        return timeEntries.Select(MapToDto).ToList();
    }
    
    public async Task<List<TimeEntryDto>> GetTimeEntriesByProjectAsync(Guid projectId, string? startDate = null, string? endDate = null)
    {
        var query = _context.TimeEntries
            .Include(t => t.User)
            .Where(t => t.ProjectDetails.Contains(projectId.ToString()));
            
        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            query = query.Where(t => t.Date >= start.Date);
            
        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            query = query.Where(t => t.Date <= end.Date);
            
        var timeEntries = await query
            .OrderByDescending(t => t.Date)
            .ToListAsync();
            
        return timeEntries.Select(MapToDto).ToList();
    }
    
    public async Task<List<TimeEntryDto>> GetTimeEntriesByDateRangeAsync(string startDate, string endDate, Guid? userId = null, Guid? projectId = null)
    {
        if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
            throw new InvalidOperationException("Invalid date format");
            
        var query = _context.TimeEntries
            .Include(t => t.User)
            .Where(t => t.Date >= start.Date && t.Date <= end.Date);
            
        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);
            
        if (projectId.HasValue)
            query = query.Where(t => t.ProjectDetails.Contains(projectId.ToString()));
            
        var timeEntries = await query
            .OrderBy(t => t.Date)
            .ToListAsync();
            
        return timeEntries.Select(MapToDto).ToList();
    }
}
