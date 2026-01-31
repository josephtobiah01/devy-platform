using DevyAPI.Application.DTOs;
using DevyAPI.Application.Interfaces;
using DevyAPI.Domain.Entities;
using DevyAPI.Infrastructure.Data;
using DevyAPI.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace DevyAPI.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public UserService(ApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Email already registered",
                new List<string> { "A user with this email already exists" }
            );
        }

        var user = new User
        {
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            MobileNumber = dto.MobileNumber,
            CountryCode = dto.CountryCode,
            CityId = dto.CityId,
            CountryId = dto.CountryId,
            WorkPreferenceId = dto.WorkPreferenceId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await LoadUserNavigationProperties(user);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        var userResponse = MapToUserResponseDto(user);
        var authResponse = new AuthResponseDto(accessToken, refreshToken, userResponse);

        return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "User registered successfully");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.City).ThenInclude(c => c!.Country)
            .Include(u => u.Country)
            .Include(u => u.WorkPreference)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Invalid credentials",
                new List<string> { "Email or password is incorrect" }
            );
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse(
                "Account is inactive",
                new List<string> { "Your account has been deactivated" }
            );
        }

        user.LastLoginAt = DateTime.UtcNow;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        var userResponse = MapToUserResponseDto(user);
        var authResponse = new AuthResponseDto(accessToken, refreshToken, userResponse);

        return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Login successful");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _tokenService.ValidateRefreshTokenAsync(refreshToken);

        if (tokenEntity == null)
        {
            return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid or expired refresh token");
        }

        var user = tokenEntity.User;
        await LoadUserNavigationProperties(user);

        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        var userResponse = MapToUserResponseDto(user);
        var authResponse = new AuthResponseDto(newAccessToken, newRefreshToken, userResponse);

        return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Token refreshed successfully");
    }

    public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.City).ThenInclude(c => c!.Country)
            .Include(u => u.Country)
            .Include(u => u.WorkPreference)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return ApiResponse<UserResponseDto>.ErrorResponse("User not found");
        }

        var userResponse = MapToUserResponseDto(user);
        return ApiResponse<UserResponseDto>.SuccessResponse(userResponse);
    }

    public async Task<ApiResponse<PagedResult<UserResponseDto>>> GetUsersAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.Users.CountAsync();

        var users = await _context.Users
            .Include(u => u.City).ThenInclude(c => c!.Country)
            .Include(u => u.Country)
            .Include(u => u.WorkPreference)
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = users.Select(MapToUserResponseDto).ToList();

        var pagedResult = new PagedResult<UserResponseDto>
        {
            Items = userDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ApiResponse<PagedResult<UserResponseDto>>.SuccessResponse(pagedResult);
    }

    public async Task<ApiResponse<UserResponseDto>> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return ApiResponse<UserResponseDto>.ErrorResponse("User not found");
        }

        if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
        if (dto.MobileNumber != null) user.MobileNumber = dto.MobileNumber;
        if (dto.CountryCode != null) user.CountryCode = dto.CountryCode;
        if (dto.CityId.HasValue) user.CityId = dto.CityId;
        if (dto.CountryId.HasValue) user.CountryId = dto.CountryId;
        if (dto.WorkPreferenceId.HasValue) user.WorkPreferenceId = dto.WorkPreferenceId;
        if (dto.ProfileImageUrl != null) user.ProfileImageUrl = dto.ProfileImageUrl;
        if (dto.VideoIntroUrl != null) user.VideoIntroUrl = dto.VideoIntroUrl;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await LoadUserNavigationProperties(user);

        var userResponse = MapToUserResponseDto(user);
        return ApiResponse<UserResponseDto>.SuccessResponse(userResponse, "User updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
    }

    private async Task LoadUserNavigationProperties(User user)
    {
        await _context.Entry(user)
            .Reference(u => u.City)
            .Query()
            .Include(c => c!.Country)
            .LoadAsync();

        await _context.Entry(user).Reference(u => u.Country).LoadAsync();
        await _context.Entry(user).Reference(u => u.WorkPreference).LoadAsync();
    }

    private UserResponseDto MapToUserResponseDto(User user)
    {
        return new UserResponseDto(
            user.Id,
            user.Email,
            user.FullName,
            user.MobileNumber,
            user.CountryCode,
            user.CityId,
            user.City?.Name,
            user.CountryId,
            user.Country?.Name,
            user.WorkPreferenceId,
            user.WorkPreference?.Name,
            user.ProfileImageUrl,
            user.VideoIntroUrl,
            user.IsActive,
            user.IsEmailVerified,
            user.CreatedAt
        );
    }
}