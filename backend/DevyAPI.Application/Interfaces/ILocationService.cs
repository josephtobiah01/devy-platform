using DevyAPI.Application.DTOs;
using DevyAPI.Shared.Common;

namespace DevyAPI.Application.Interfaces;

public interface ILocationService
{
    Task<ApiResponse<LocationResponseDto>> GetLocationDataAsync();
    Task<ApiResponse<List<CountryDto>>> GetCountriesAsync();
    Task<ApiResponse<List<CityDto>>> GetCitiesByCountryAsync(int countryId);
    Task<ApiResponse<List<WorkPreferenceDto>>> GetWorkPreferencesAsync();
}