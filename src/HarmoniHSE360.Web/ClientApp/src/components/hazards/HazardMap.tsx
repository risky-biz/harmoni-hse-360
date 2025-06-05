import React from 'react';
import { CCard, CCardBody, CCardHeader, CAlert } from '@coreui/react';

interface HazardMapProps {
  height?: string;
  showControls?: boolean;
  selectedHazardId?: number;
  onHazardSelect?: (hazard: any) => void;
  filterParams?: any;
}

const HazardMap: React.FC<HazardMapProps> = ({
  height = '500px',
}) => {
  return (
    <CCard style={{ height }}>
      <CCardHeader>
        <strong>Hazard Map</strong>
      </CCardHeader>
      <CCardBody>
        <CAlert color="info">
          Google Maps integration temporarily disabled for build compatibility.
          This will be restored with proper environment configuration.
        </CAlert>
      </CCardBody>
    </CCard>
  );
};

export default HazardMap;