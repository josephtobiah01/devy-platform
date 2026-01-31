import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { LocationService } from '../../../core/services/location.service';
import { Country, City, WorkPreference } from '../../../core/models/api.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private locationService = inject(LocationService);

  registerForm!: FormGroup;
  countries: Country[] = [];
  cities: City[] = [];
  workPreferences: WorkPreference[] = [];
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  ngOnInit(): void {
    console.log('Component initialized');
    this.initForm();
    this.loadData();
  }

  private initForm(): void {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      fullName: ['', Validators.required], // Changed from firstName/lastName
      countryCode: [''], // Changed from phoneCode
      mobileNumber: [''], // Changed from phoneNumber
      countryId: ['', Validators.required],
      cityId: ['', Validators.required],
      workPreferenceId: ['', Validators.required],
      password: ['Password123!'] // Hidden field with default value for test
    });

    // Watch country changes to load cities
    this.registerForm.get('countryId')?.valueChanges.subscribe(countryId => {
      console.log('Country changed:', countryId);
      if (countryId) {
        this.loadCities(countryId);
      }
    });
  }

  private loadData(): void {
    console.log('Loading countries...');
    this.locationService.getCountries().subscribe({
      next: (countries) => {
        console.log('Countries loaded:', countries);
        this.countries = countries;
      },
      error: (error) => {
        console.error('Error loading countries:', error);
      }
    });

    console.log('Loading work preferences...');
    this.locationService.getWorkPreferences().subscribe({
      next: (prefs) => {
        console.log('Work preferences loaded:', prefs);
        this.workPreferences = prefs;
      },
      error: (error) => {
        console.error('Error loading work preferences:', error);
      }
    });
  }

  private loadCities(countryId: number): void {
    console.log('Loading cities for country:', countryId);
    this.locationService.getCitiesByCountry(countryId).subscribe({
      next: (cities) => {
        console.log('Cities loaded:', cities);
        this.cities = cities;
        this.registerForm.patchValue({ cityId: '' });
      },
      error: (error) => {
        console.error('Error loading cities:', error);
      }
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.registerForm.value;
    
    // Build request matching backend schema exactly
    const request = {
      email: formValue.email,
      password: formValue.password,
      fullName: formValue.fullName,
      mobileNumber: formValue.mobileNumber || undefined,
      countryCode: formValue.countryCode || undefined,
      cityId: formValue.cityId ? Number(formValue.cityId) : undefined,
      countryId: formValue.countryId ? Number(formValue.countryId) : undefined,
      workPreferenceId: formValue.workPreferenceId ? Number(formValue.workPreferenceId) : undefined
    };

    console.log('Submitting registration:', request);

    this.authService.register(request).subscribe({
      next: (response) => {
        console.log('Registration response:', response);
        this.successMessage = 'Registration successful! Welcome to Devy Platform.';
        this.registerForm.reset();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Registration error:', error);
        this.errorMessage = error.message || 'Registration failed. Please try again.';
        this.isLoading = false;
      }
    });
  }

  onIntroduceYourself(): void {
    // Placeholder for video intro functionality
    alert('Video introduction feature coming soon!');
  }

  hasError(controlName: string): boolean {
    const control = this.registerForm.get(controlName);
    return !!(control?.invalid && (control?.dirty || control?.touched));
  }
}