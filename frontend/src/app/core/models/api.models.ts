// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: any;
}

// Registration request - UPDATED to match backend
export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  mobileNumber?: string;
  countryCode?: string;
  cityId?: number;
  countryId?: number;
  workPreferenceId?: number;
}

// Location models
export interface Country {
  id: number;
  name: string;
  code: string;
  phoneCode: string;
}

export interface City {
  id: number;
  name: string;
  countryId: number;
  countryName: string;
  stateProvince?: string;
}

export interface WorkPreference {
  id: number;
  name: string;
  description?: string;
}

// User response - UPDATED to match backend
export interface User {
  id: string;
  email: string;
  fullName: string;
  mobileNumber?: string;
  countryCode?: string;
  cityId?: number;
  cityName?: string;
  countryId?: number;
  countryName?: string;
  workPreferenceId?: number;
  workPreferenceName?: string;
  profileImageUrl?: string;
  videoIntroUrl?: string;
  isActive: boolean;
  isEmailVerified: boolean;
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}