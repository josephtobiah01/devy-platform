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

export interface LocationData {
  countries: Country[];
  workPreferences: WorkPreference[];
}