import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Country, City, WorkPreference } from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getCountries(): Observable<Country[]> {
    return this.http.get<any>(`${this.baseUrl}/Locations/countries`)
      .pipe(
        map(response => {
          console.log('Raw countries response:', response);
          // Handle different response formats
          if (Array.isArray(response)) {
            return response;
          } else if (response.data && Array.isArray(response.data)) {
            return response.data;
          } else if (response.$values && Array.isArray(response.$values)) {
            return response.$values;
          }
          return [];
        })
      );
  }

  getCitiesByCountry(countryId: number): Observable<City[]> {
    return this.http.get<any>(`${this.baseUrl}/Locations/countries/${countryId}/cities`)
      .pipe(
        map(response => {
          console.log('Raw cities response:', response);
          if (Array.isArray(response)) {
            return response;
          } else if (response.data && Array.isArray(response.data)) {
            return response.data;
          } else if (response.$values && Array.isArray(response.$values)) {
            return response.$values;
          }
          return [];
        })
      );
  }

  getWorkPreferences(): Observable<WorkPreference[]> {
    return this.http.get<any>(`${this.baseUrl}/Locations/work-preferences`)
      .pipe(
        map(response => {
          console.log('Raw work preferences response:', response);
          if (Array.isArray(response)) {
            return response;
          } else if (response.data && Array.isArray(response.data)) {
            return response.data;
          } else if (response.$values && Array.isArray(response.$values)) {
            return response.$values;
          }
          return [];
        })
      );
  }
}
