using DevyAPI.Application.DTOs;
using DevyAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevyAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}", dto.Email);
        
        var result = await _userService.RegisterAsync(dto);
        
        if (!result.Success)
        {
            _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}", 
                dto.Email, string.Join(", ", result.Errors ?? new List<string>()));
            return BadRequest(result);
        }

        _logger.LogInformation("User registered successfully: {Email}", dto.Email);
        return Ok(result);
    }

    /// <summary>
    /// Login user
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", dto.Email);
        
        var result = await _userService.LoginAsync(dto);
        
        if (!result.Success)
        {
            _logger.LogWarning("Login failed for email: {Email}", dto.Email);
            return Unauthorized(result);
        }

        _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        _logger.LogInformation("Attempting to refresh token");
        
        var result = await _userService.RefreshTokenAsync(dto.RefreshToken);
        
        if (!result.Success)
        {
            _logger.LogWarning("Token refresh failed");
            return Unauthorized(result);
        }

        _logger.LogInformation("Token refreshed successfully");
        return Ok(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", id);
        
        var result = await _userService.GetUserByIdAsync(id);
        
        if (!result.Success)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get current logged-in user
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid user ID in token");
            return Unauthorized();
        }

        _logger.LogInformation("Fetching current user: {UserId}", userId);
        
        var result = await _userService.GetUserByIdAsync(userId);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Maximum page size

        _logger.LogInformation("Fetching users - Page: {PageNumber}, Size: {PageSize}", 
            pageNumber, pageSize);
        
        var result = await _userService.GetUsersAsync(pageNumber, pageSize);
        
        return Ok(result);
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        // Users can only update their own profile
        if (currentUserId != id)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to update user {TargetUserId}", 
                currentUserId, id);
            return Forbid();
        }

        _logger.LogInformation("Updating user: {UserId}", id);
        
        var result = await _userService.UpdateUserAsync(id, dto);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        // Users can only delete their own account
        if (currentUserId != id)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to delete user {TargetUserId}", 
                currentUserId, id);
            return Forbid();
        }

        _logger.LogInformation("Deleting user: {UserId}", id);
        
        var result = await _userService.DeleteUserAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}