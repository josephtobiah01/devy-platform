using DevyAPI.Application.DTOs;
using DevyAPI.Application.Interfaces;
using DevyAPI.Infrastructure.Data;
using DevyAPI.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace DevyAPI.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly ApplicationDbContext _context;

    public LocationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<LocationResponseDto>> GetLocationDataAsync()
    {
        var countries = await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto(c.Id, c.Name, c.Code, c.PhoneCode))
            .ToListAsync();

        var workPreferences = await _context.WorkPreferences
            .Where(w => w.IsActive)
            .OrderBy(w => w.SortOrder)
            .Select(w => new WorkPreferenceDto(w.Id, w.Name, w.Description))
            .ToListAsync();

        var response = new LocationResponseDto(countries, workPreferences);

        return ApiResponse<LocationResponseDto>.SuccessResponse(response);
    }

    public async Task<ApiResponse<List<CountryDto>>> GetCountriesAsync()
    {
        var countries = await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto(c.Id, c.Name, c.Code, c.PhoneCode))
            .ToListAsync();

        return ApiResponse<List<CountryDto>>.SuccessResponse(countries);
    }

    public async Task<ApiResponse<List<CityDto>>> GetCitiesByCountryAsync(int countryId)
    {
        var cities = await _context.Cities
            .Where(c => c.CountryId == countryId && c.IsActive)
            .Include(c => c.Country)
            .OrderBy(c => c.Name)
            .Select(c => new CityDto(c.Id, c.Name, c.CountryId, c.Country.Name, c.StateProvince))
            .ToListAsync();

        return ApiResponse<List<CityDto>>.SuccessResponse(cities);
    }

    public async Task<ApiResponse<List<WorkPreferenceDto>>> GetWorkPreferencesAsync()
    {
        var workPreferences = await _context.WorkPreferences
            .Where(w => w.IsActive)
            .OrderBy(w => w.SortOrder)
            .Select(w => new WorkPreferenceDto(w.Id, w.Name, w.Description))
            .ToListAsync();

        return ApiResponse<List<WorkPreferenceDto>>.SuccessResponse(workPreferences);
    }
}