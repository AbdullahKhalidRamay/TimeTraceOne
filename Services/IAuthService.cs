using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<ApiResponse<string>> LogoutAsync(LogoutRequestDto request);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(Guid userId);
}
