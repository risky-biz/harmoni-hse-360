import { useDefaultMapCenter } from '../contexts/CompanyConfigurationContext';
import { MAPS_DEFAULT_CENTER } from '../config/mapsConfig';

/**
 * Hook to get the appropriate map center coordinates
 * Uses company configuration if available, otherwise falls back to default
 */
export const useMapCenter = (): { lat: number; lng: number } => {
  const companyMapCenter = useDefaultMapCenter();
  
  // Return company configured center or fallback to default
  return companyMapCenter || MAPS_DEFAULT_CENTER;
};