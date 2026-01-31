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

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}