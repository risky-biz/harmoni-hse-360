import React from 'react';
import { CCard, CCardBody, CCardHeader, CAlert, CSpinner } from '@coreui/react';
import { getMapsConfig } from '../../config/mapsConfig';
import GoogleMapsSetupGuide from '../common/GoogleMapsSetupGuide';

interface HazardMapProps {
  height?: string;
  showControls?: boolean;
  selectedHazardId?: number;
  onHazardSelect?: (hazard: any) => void;
  filterParams?: {
    severity?: string;
    status?: string;
    category?: string;
    department?: string;
    dateFrom?: string;
    dateTo?: string;
  };
}

const HazardMap: React.FC<HazardMapProps> = ({
  height = '500px',
  showControls = true,
  selectedHazardId,
  filterParams,
}) => {
  const mapsConfig = getMapsConfig();

  // If Maps API is not configured, show setup guide
  if (!mapsConfig.isConfigured) {
    return (
      <div style={{ height }}>
        <GoogleMapsSetupGuide className="h-100" />
      </div>
    );
  }

  // TODO: Implement actual Google Maps integration once @googlemaps/react-wrapper is installed
  // For now, show a placeholder indicating configuration is ready
  return (
    <CCard style={{ height }}>
      <CCardHeader>
        <strong>Hazard Map</strong>
        {showControls && (
          <div className="float-end">
            <small className="text-success">API Configured ✓</small>
          </div>
        )}
      </CCardHeader>
      <CCardBody className="d-flex flex-column justify-content-center align-items-center">
        <CSpinner color="primary" className="mb-3" />
        <CAlert color="info" className="text-center mb-0">
          <strong>Maps Integration Ready!</strong>
          <p className="mb-2">Google Maps API key is configured and ready to use.</p>
          <small className="text-muted">
            Install mapping library: <code>npm install @googlemaps/react-wrapper</code>
          </small>
        </CAlert>
        
        {/* Development info */}
        {import.meta.env.MODE === 'development' && (
          <div className="mt-3 text-muted small">
            <p className="mb-1"><strong>Development Mode - Configuration Status:</strong></p>
            <ul className="mb-0">
              <li>API Key: {mapsConfig.apiKey ? '✓ Configured' : '✗ Missing'}</li>
              <li>Selected Hazard: {selectedHazardId || 'None'}</li>
              <li>Filter Params: {filterParams ? JSON.stringify(filterParams) : 'None'}</li>
            </ul>
          </div>
        )}
      </CCardBody>
    </CCard>
  );
};

export default HazardMap;