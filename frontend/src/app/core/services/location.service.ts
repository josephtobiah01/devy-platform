import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, Country, City, WorkPreference } from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getCountries(): Observable<Country[]> {
    return this.http
      .get<ApiResponse<Country[]>>(`${this.baseUrl}/countries`)
      .pipe(map(response => response.data));
  }

  getCitiesByCountry(countryId: number): Observable<City[]> {
    return this.http
      .get<ApiResponse<City[]>>(`${this.baseUrl}/countries/${countryId}/cities`)
      .pipe(map(response => response.data));
  }

  getWorkPreferences(): Observable<WorkPreference[]> {
    return this.http
      .get<ApiResponse<WorkPreference[]>>(`${this.baseUrl}/work-preferences`)
      .pipe(map(response => response.data));
  }
}