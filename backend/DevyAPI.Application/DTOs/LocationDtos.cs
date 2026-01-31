namespace DevyAPI.Application.DTOs;

public record CountryDto(
    int Id,
    string Name,
    string Code,
    string PhoneCode
);

public record CityDto(
    int Id,
    string Name,
    int CountryId,
    string CountryName,
    string? StateProvince
);

public record LocationResponseDto(
    List<CountryDto> Countries,
    List<WorkPreferenceDto> WorkPreferences
);

public record WorkPreferenceDto(
    int Id,
    string Name,
    string? Description
);