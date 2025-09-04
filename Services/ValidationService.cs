using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class ValidationService : IValidationService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<ValidationService> _logger;
    
    public ValidationService(TimeFlowDbContext context, ILogger<ValidationService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<TimeEntryValidationResultDto> ValidateTimeEntryAsync(TimeEntryValidationDto dto)
    {
        var errors = new List<string>();
        var validationRules = new ValidationRulesDto();
        
        // Validate date
        if (!DateTime.TryParse(dto.Date, out var targetDate))
        {
            errors.Add("Invalid date format");
            return new TimeEntryValidationResultDto
            {
                IsValid = false,
                ValidationRules = validationRules,
                Errors = errors
            };
        }
        
        // Validate hours
        if (dto.ActualHours < 0 || dto.ActualHours > 24)
        {
            errors.Add("Actual hours must be between 0 and 24");
        }
        
        if (dto.BillableHours < 0 || dto.BillableHours > 24)
        {
            errors.Add("Billable hours must be between 0 and 24");
        }
        
        if (dto.BillableHours > dto.ActualHours)
        {
            errors.Add("Billable hours cannot exceed actual hours");
            validationRules.BillableHoursValid = false;
        }
        else
        {
            validationRules.BillableHoursValid = true;
        }
        
        // Check daily hours limit
        var dailyTotal = await _context.TimeEntries
            .Where(t => t.UserId == dto.UserId && t.Date.Date == targetDate.Date)
            .SumAsync(t => t.ActualHours);
            
        var totalWithNew = dailyTotal + dto.ActualHours;
        if (totalWithNew > 24)
        {
            errors.Add($"Daily hours limit exceeded. Current: {dailyTotal}, New: {dto.ActualHours}, Total: {totalWithNew}");
            validationRules.MaxDailyHours = 24;
        }
        
        // Check weekly hours limit
        var weekStart = targetDate.AddDays(-(int)targetDate.DayOfWeek);
        var weekEnd = weekStart.AddDays(6);
        
        var weeklyTotal = await _context.TimeEntries
            .Where(t => t.UserId == dto.UserId && t.Date >= weekStart && t.Date <= weekEnd)
            .SumAsync(t => t.ActualHours);
            
        var weeklyTotalWithNew = weeklyTotal + dto.ActualHours;
        if (weeklyTotalWithNew > 168)
        {
            errors.Add($"Weekly hours limit exceeded. Current: {weeklyTotal}, New: {dto.ActualHours}, Total: {weeklyTotalWithNew}");
            validationRules.MaxWeeklyHours = 168;
        }
        
        // Check for overlapping entries (simplified)
        var hasOverlap = await _context.TimeEntries
            .AnyAsync(t => t.UserId == dto.UserId && t.Date.Date == targetDate.Date);
            
        validationRules.NoOverlap = !hasOverlap;
        
        return new TimeEntryValidationResultDto
        {
            IsValid = errors.Count == 0,
            ValidationRules = validationRules,
            Errors = errors
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
    
    public async Task<bool> ValidateUserAccessAsync(Guid userId, Guid resourceId, string resourceType)
    {
        // Simplified access validation - in a real application, this would be more sophisticated
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        
        // Owner and Manager have access to everything
        if (user.Role == UserRole.owner || user.Role == UserRole.manager)
            return true;
        
        // Employee access depends on resource type and ownership
        switch (resourceType.ToLower())
        {
            case "timeentry":
                return await _context.TimeEntries.AnyAsync(t => t.Id == resourceId && t.UserId == userId);
            case "project":
                return await _context.Projects.AnyAsync(p => p.Id == resourceId && 
                    (p.CreatedBy == userId || p.TeamProjects.Any(tp => tp.Team.Members.Any(m => m.UserId == userId))));
            case "team":
                return await _context.Teams.AnyAsync(t => t.Id == resourceId && 
                    t.Members.Any(m => m.UserId == userId));
            default:
                return false;
        }
    }
    
    public async Task<bool> ValidateTeamAccessAsync(Guid userId, Guid teamId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        
        // Owner and Manager have access to all teams
        if (user.Role == UserRole.owner || user.Role == UserRole.manager)
            return true;
        
        // Employee must be a member of the team
        return await _context.TeamMembers.AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
    }
    
    public async Task<bool> ValidateProjectAccessAsync(Guid userId, Guid projectId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        
        // Owner and Manager have access to all projects
        if (user.Role == UserRole.owner || user.Role == UserRole.manager)
            return true;
        
        // Employee must be creator or team member
        return await _context.Projects.AnyAsync(p => p.Id == projectId && 
            (p.CreatedBy == userId || p.TeamProjects.Any(tp => tp.Team.Members.Any(m => m.UserId == userId))));
    }
    
    public async Task<bool> ValidateUserAccessAsync(Guid currentUserId, Guid userId)
    {
        var currentUser = await _context.Users.FindAsync(currentUserId);
        if (currentUser == null) return false;
        
        // Owner and Manager have access to all users
        if (currentUser.Role == UserRole.owner || currentUser.Role == UserRole.manager)
            return true;
        
        // Employee can only access their own data
        return currentUserId == userId;
    }
}
