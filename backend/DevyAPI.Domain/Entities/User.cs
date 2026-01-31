namespace DevyAPI.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string? CountryCode { get; set; }
    public int? CityId { get; set; }
    public int? CountryId { get; set; }
    public int? WorkPreferenceId { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? VideoIntroUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpires { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public City? City { get; set; }
    public Country? Country { get; set; }
    public WorkPreference? WorkPreference { get; set; }
    public UserProfile? UserProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}