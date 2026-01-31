using DevyAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevyAPI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<WorkPreference> WorkPreferences { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema
        modelBuilder.HasDefaultSchema("app");

        ConfigureUser(modelBuilder);
        ConfigureCountry(modelBuilder);
        ConfigureCity(modelBuilder);
        ConfigureWorkPreference(modelBuilder);
        ConfigureUserProfile(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.MobileNumber).HasColumnName("mobile_number").HasMaxLength(20);
            entity.Property(e => e.CountryCode).HasColumnName("country_code").HasMaxLength(10);
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.WorkPreferenceId).HasColumnName("work_preference_id");
            entity.Property(e => e.ProfileImageUrl).HasColumnName("profile_image_url");
            entity.Property(e => e.VideoIntroUrl).HasColumnName("video_intro_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsEmailVerified).HasColumnName("is_email_verified");
            entity.Property(e => e.EmailVerificationToken).HasColumnName("email_verification_token");
            entity.Property(e => e.PasswordResetToken).HasColumnName("password_reset_token");
            entity.Property(e => e.PasswordResetExpires).HasColumnName("password_reset_expires");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.City)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Country)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.WorkPreference)
                .WithMany(w => w.Users)
                .HasForeignKey(e => e.WorkPreferenceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UserProfile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureCountry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("countries");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasColumnName("code").IsRequired().HasMaxLength(3);
            entity.Property(e => e.PhoneCode).HasColumnName("phone_code").IsRequired().HasMaxLength(10);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Code).IsUnique();
        });
    }

    private void ConfigureCity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("cities");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.StateProvince).HasColumnName("state_province").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => new { e.Name, e.CountryId }).IsUnique();

            entity.HasOne(e => e.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureWorkPreference(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkPreference>(entity =>
        {
            entity.ToTable("work_preferences");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private void ConfigureUserProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Skills).HasColumnName("skills");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.LinkedInUrl).HasColumnName("linkedin_url").HasMaxLength(255);
            entity.Property(e => e.GitHubUrl).HasColumnName("github_url").HasMaxLength(255);
            entity.Property(e => e.PortfolioUrl).HasColumnName("portfolio_url").HasMaxLength(255);
            entity.Property(e => e.ResumeUrl).HasColumnName("resume_url").HasMaxLength(255);
            entity.Property(e => e.PreferredTechnologies).HasColumnName("preferred_technologies");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.UserId).IsUnique();
        });
    }

    private void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Token).HasColumnName("token").IsRequired().HasMaxLength(500);
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.ReplacedByToken).HasColumnName("replaced_by_token").HasMaxLength(500);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}