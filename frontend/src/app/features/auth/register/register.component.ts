import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocationService } from '../../../core/services/location.service';
import { Country, City, WorkPreference } from '../../../shared/models/api.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private locationService = inject(LocationService);
  private router = inject(Router);

  registerForm!: FormGroup;
  countries: Country[] = [];
  cities: City[] = [];
  workPreferences: WorkPreference[] = [];
  isLoading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.initForm();
    this.loadData();
  }

  private initForm(): void {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      countryId: ['', Validators.required],
      cityId: ['', Validators.required],
      workPreferenceId: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });

    // Watch country changes to load cities
    this.registerForm.get('countryId')?.valueChanges.subscribe(countryId => {
      if (countryId) {
        this.loadCities(countryId);
      }
    });
  }

  private passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  private loadData(): void {
    this.locationService.getCountries().subscribe({
      next: (countries) => this.countries = countries,
      error: (error) => console.error('Error loading countries:', error)
    });

    this.locationService.getWorkPreferences().subscribe({
      next: (prefs) => this.workPreferences = prefs,
      error: (error) => console.error('Error loading work preferences:', error)
    });
  }

  private loadCities(countryId: number): void {
    this.locationService.getCitiesByCountry(countryId).subscribe({
      next: (cities) => {
        this.cities = cities;
        this.registerForm.patchValue({ cityId: '' });
      },
      error: (error) => console.error('Error loading cities:', error)
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const formValue = this.registerForm.value;
    const request = {
      email: formValue.email,
      password: formValue.password,
      fullName: `${formValue.firstName} ${formValue.lastName}`,
      countryId: Number(formValue.countryId),
      cityId: Number(formValue.cityId),
      workPreferenceId: Number(formValue.workPreferenceId)
    };

    this.authService.register(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (error) => {
        this.errorMessage = error.message || 'Registration failed. Please try again.';
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  // Helper methods for form validation
  hasError(controlName: string, errorName: string): boolean {
    const control = this.registerForm.get(controlName);
    return !!(control?.hasError(errorName) && (control?.dirty || control?.touched));
  }

  get passwordMismatch(): boolean {
    return !!(this.registerForm.hasError('passwordMismatch') && 
             this.registerForm.get('confirmPassword')?.touched);
  }
}