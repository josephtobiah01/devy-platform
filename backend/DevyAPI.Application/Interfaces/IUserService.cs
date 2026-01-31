using DevyAPI.Application.DTOs;
using DevyAPI.Shared.Common;

namespace DevyAPI.Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterUserDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<PagedResult<UserResponseDto>>> GetUsersAsync(int pageNumber, int pageSize);
    Task<ApiResponse<UserResponseDto>> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
}