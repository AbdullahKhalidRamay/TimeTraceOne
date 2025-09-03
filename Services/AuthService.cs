using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;
using BCrypt.Net;

namespace TimeTraceOne.Services;

public class AuthService : IAuthService
{
    private readonly TimeFlowDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(TimeFlowDbContext context, IJwtService jwtService, ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }
    
    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
                
            if (user == null)
            {
                return ApiResponse<LoginResponseDto>.Error("Invalid email or password");
            }
            
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponseDto>.Error("Invalid email or password");
            }
            
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            // In a real application, you would store the refresh token in the database
            // For now, we'll just return it
            
            var response = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    JobTitle = user.JobTitle,
                    AvailableHours = user.AvailableHours,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            };
            
            _logger.LogInformation("User {Email} logged in successfully", user.Email);
            
            return ApiResponse<LoginResponseDto>.Success(response, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            return ApiResponse<LoginResponseDto>.Error("An error occurred during login");
        }
    }
    
    public async Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        try
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshToken);
            if (principal == null)
            {
                return ApiResponse<RefreshTokenResponseDto>.Error("Invalid refresh token");
            }
            
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
            {
                return ApiResponse<RefreshTokenResponseDto>.Error("Invalid token");
            }
            
            var user = await _context.Users.FindAsync(userIdGuid);
            if (user == null || !user.IsActive)
            {
                return ApiResponse<RefreshTokenResponseDto>.Error("User not found or inactive");
            }
            
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            
            var response = new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken
            };
            
            _logger.LogInformation("Token refreshed for user {UserId}", userId);
            
            return ApiResponse<RefreshTokenResponseDto>.Success(response, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return ApiResponse<RefreshTokenResponseDto>.Error("An error occurred while refreshing token");
        }
    }
    
    public Task<ApiResponse<string>> LogoutAsync(LogoutRequestDto request)
    {
        try
        {
            // In a real application, you would invalidate the refresh token in the database
            // For now, we'll just return success
            
            _logger.LogInformation("User logged out successfully");
            
            return Task.FromResult(ApiResponse<string>.Success("Logged out successfully", "Logout successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Task.FromResult(ApiResponse<string>.Error("An error occurred during logout"));
        }
    }
    
    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ApiResponse<UserDto>.Error("User not found or inactive");
            }
            
            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                JobTitle = user.JobTitle,
                AvailableHours = user.AvailableHours,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
            
            return ApiResponse<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user {UserId}", userId);
            return ApiResponse<UserDto>.Error("An error occurred while getting user information");
        }
    }
}
