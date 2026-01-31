using System.Text;
using DevyAPI.Api.Middleware;
using DevyAPI.Application.Interfaces;
using DevyAPI.Application.Validators;
using DevyAPI.Infrastructure.Data;
using DevyAPI.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// TEMPORARY DEBUG - Remove after fixing
// ============================================
Console.WriteLine("=== Configuration Debug Info ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Content Root: {builder.Environment.ContentRootPath}");
Console.WriteLine($"Base Path: {AppContext.BaseDirectory}");

// Check if appsettings.json exists in different locations
var contentRootPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.json");
var baseDirectoryPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

Console.WriteLine($"appsettings.json in ContentRoot: {File.Exists(contentRootPath)} - {contentRootPath}");
Console.WriteLine($"appsettings.json in BaseDirectory: {File.Exists(baseDirectoryPath)} - {baseDirectoryPath}");

// Try to read the config
var jwtSecretValue = builder.Configuration["Jwt:Secret"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"JWT:Secret loaded: {!string.IsNullOrEmpty(jwtSecretValue)}");
Console.WriteLine($"JWT:Secret value: {(jwtSecretValue != null ? jwtSecretValue.Substring(0, Math.Min(20, jwtSecretValue.Length)) + "..." : "NULL")}");
Console.WriteLine($"ConnectionString loaded: {!string.IsNullOrEmpty(connectionString)}");
Console.WriteLine("================================");
// ============================================
// END TEMPORARY DEBUG
// ============================================

// ============================================
// Add services to the container
// ============================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ============================================
// Swagger/OpenAPI Configuration
// ============================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Devy API", 
        Version = "v1",
        Description = "Developer Registration Platform API",
        Contact = new OpenApiContact
        {
            Name = "Devy Platform",
            Email = "support@devy.com"
        }
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================
// Database Configuration
// ============================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => 
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ============================================
// JWT Authentication Configuration
// ============================================
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? throw new InvalidOperationException("JWT Issuer is not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ============================================
// CORS Configuration
// ============================================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevyPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Token-Expired");
    });
});

// ============================================
// FluentValidation Configuration
// ============================================
// builder.Services.AddFluentValidationAutoValidation(config =>
// {
//     config.DisableDataAnnotationsValidation = false;
// });
// builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

// ============================================
// Application Services Registration
// ============================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ============================================
// Health Checks
// ============================================
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgres",
        tags: new[] { "db", "sql", "postgresql" });

// ============================================
// Build the application
// ============================================
var app = builder.Build();

// ============================================
// Configure the HTTP request pipeline
// ============================================

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Devy API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("DevyPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => new
{
    name = "Devy API",
    version = "1.0.0",
    status = "Running",
    timestamp = DateTime.UtcNow,
    endpoints = new[]
    {
        "/swagger - API Documentation",
        "/health - Health Check",
        "/api/users - User Management",
        "/api/locations - Location Data"
    }
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var canConnect = await context.Database.CanConnectAsync();
        if (canConnect)
        {
            app.Logger.LogInformation("✅ Database connection successful");
        }
        else
        {
            app.Logger.LogWarning("⚠️ Cannot connect to database");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ An error occurred while connecting to the database");
    }
}

app.Logger.LogInformation("🚀 Devy API is starting...");
app.Run();