using DevyAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevyAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(ILocationService locationService, ILogger<LocationsController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all location data (countries and work preferences)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationData()
    {
        _logger.LogInformation("Fetching location data");
        
        var result = await _locationService.GetLocationDataAsync();
        
        return Ok(result);
    }

    /// <summary>
    /// Get all countries
    /// </summary>
    [HttpGet("countries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries()
    {
        _logger.LogInformation("Fetching countries");
        
        var result = await _locationService.GetCountriesAsync();
        
        return Ok(result);
    }

    /// <summary>
    /// Get cities by country
    /// </summary>
    [HttpGet("countries/{countryId}/cities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCitiesByCountry(int countryId)
    {
        _logger.LogInformation("Fetching cities for country: {CountryId}", countryId);
        
        var result = await _locationService.GetCitiesByCountryAsync(countryId);
        
        return Ok(result);
    }

    /// <summary>
    /// Get all work preferences
    /// </summary>
    [HttpGet("work-preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkPreferences()
    {
        _logger.LogInformation("Fetching work preferences");
        
        var result = await _locationService.GetWorkPreferencesAsync();
        
        return Ok(result);
    }
}