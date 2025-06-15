export interface MapsConfig {
  apiKey: string | undefined;
  isConfigured: boolean;
  errorMessage?: string;
}

export const getMapsConfig = (): MapsConfig => {
  const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;
  
  if (!apiKey || apiKey === 'your_google_maps_api_key_here') {
    return {
      apiKey: undefined,
      isConfigured: false,
      errorMessage: 'Google Maps API key not configured'
    };
  }
  
  return {
    apiKey,
    isConfigured: true
  };
};

export const getGeocodingConfig = () => {
  const geocodingKey = import.meta.env.VITE_GEOCODING_API_KEY;
  const mapsKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;
  
  // Use geocoding key if available, otherwise fallback to maps key
  return geocodingKey && geocodingKey !== 'your_geocoding_api_key_here' 
    ? geocodingKey 
    : mapsKey;
};

export const MAPS_DEFAULT_CENTER = {
  lat: -6.1751, // Jakarta, Indonesia (British School Jakarta area)
  lng: 106.8650
};

export const MAPS_DEFAULT_ZOOM = 15;