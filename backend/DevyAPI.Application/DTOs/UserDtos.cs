namespace DevyAPI.Application.DTOs;

public record RegisterUserDto(
    string Email,
    string Password,
    string FullName,
    string? MobileNumber,
    string? CountryCode,
    int? CityId,
    int? CountryId,
    int? WorkPreferenceId
);

public record UserResponseDto(
    Guid Id,
    string Email,
    string FullName,
    string? MobileNumber,
    string? CountryCode,
    int? CityId,
    string? CityName,
    int? CountryId,
    string? CountryName,
    int? WorkPreferenceId,
    string? WorkPreferenceName,
    string? ProfileImageUrl,
    string? VideoIntroUrl,
    bool IsActive,
    bool IsEmailVerified,
    DateTime CreatedAt
);

public record UpdateUserDto(
    string? FullName,
    string? MobileNumber,
    string? CountryCode,
    int? CityId,
    int? CountryId,
    int? WorkPreferenceId,
    string? ProfileImageUrl,
    string? VideoIntroUrl
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    UserResponseDto User
);

public record RefreshTokenDto(
    string RefreshToken
);