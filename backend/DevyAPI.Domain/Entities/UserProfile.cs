namespace DevyAPI.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public string[]? Skills { get; set; }
    public int? ExperienceYears { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? ResumeUrl { get; set; }
    public string[]? PreferredTechnologies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}