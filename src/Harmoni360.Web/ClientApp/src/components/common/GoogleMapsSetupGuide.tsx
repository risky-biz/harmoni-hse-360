import React, { useState } from 'react';
import {
  CAlert,
  CButton,
  CCard,
  CCardBody,
  CCardHeader,
  CCollapse,
  CListGroup,
  CListGroupItem,
  CBadge,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faChevronDown, 
  faChevronRight, 
  faExternalLinkAlt,
  faKey,
  faCog,
  faCheckCircle 
} from '@fortawesome/free-solid-svg-icons';

interface GoogleMapsSetupGuideProps {
  className?: string;
}

const GoogleMapsSetupGuide: React.FC<GoogleMapsSetupGuideProps> = ({ className }) => {
  const [showInstructions, setShowInstructions] = useState(false);

  return (
    <CCard className={className}>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <div>
          <FontAwesomeIcon icon={faKey} className="me-2 text-warning" />
          <strong>Google Maps Configuration Required</strong>
        </div>
        <CBadge color="warning">Setup Required</CBadge>
      </CCardHeader>
      <CCardBody>
        <CAlert color="warning" className="mb-3">
          <strong>Maps functionality is disabled.</strong> Configure your Google Maps API key to enable location mapping features.
        </CAlert>

        <div className="mb-3">
          <CButton
            color="primary"
            variant="outline"
            onClick={() => setShowInstructions(!showInstructions)}
            className="me-2"
          >
            <FontAwesomeIcon 
              icon={showInstructions ? faChevronDown : faChevronRight} 
              className="me-2" 
            />
            Show Setup Instructions
          </CButton>
          <CButton
            color="info"
            variant="outline"
            href="https://console.cloud.google.com/google/maps-apis/"
            target="_blank"
            rel="noopener noreferrer"
          >
            <FontAwesomeIcon icon={faExternalLinkAlt} className="me-2" />
            Google Cloud Console
          </CButton>
        </div>

        <CCollapse visible={showInstructions}>
          <CCard>
            <CCardHeader>
              <FontAwesomeIcon icon={faCog} className="me-2" />
              <strong>Setup Instructions</strong>
            </CCardHeader>
            <CCardBody>
              <h6 className="mb-3">Step 1: Create Google Maps API Key</h6>
              <CListGroup className="mb-4">
                <CListGroupItem className="d-flex justify-content-between align-items-start">
                  <div>
                    <strong>1.</strong> Go to the <a href="https://console.cloud.google.com/google/maps-apis/" target="_blank" rel="noopener noreferrer">Google Cloud Console</a>
                  </div>
                </CListGroupItem>
                <CListGroupItem>
                  <strong>2.</strong> Create a new project or select an existing one
                </CListGroupItem>
                <CListGroupItem>
                  <strong>3.</strong> Enable the following APIs:
                  <ul className="mt-2 mb-0">
                    <li>Maps JavaScript API</li>
                    <li>Places API (optional)</li>
                    <li>Geocoding API (optional)</li>
                  </ul>
                </CListGroupItem>
                <CListGroupItem>
                  <strong>4.</strong> Create credentials (API Key)
                </CListGroupItem>
                <CListGroupItem>
                  <strong>5.</strong> Configure API key restrictions (recommended):
                  <ul className="mt-2 mb-0">
                    <li>HTTP referrer restrictions for your domain</li>
                    <li>API restrictions for only needed APIs</li>
                  </ul>
                </CListGroupItem>
              </CListGroup>

              <h6 className="mb-3">Step 2: Configure Environment Variables</h6>
              <CListGroup className="mb-4">
                <CListGroupItem>
                  <strong>1.</strong> Create a <code>.env</code> file in the ClientApp directory
                </CListGroupItem>
                <CListGroupItem>
                  <strong>2.</strong> Add your API key:
                  <div className="mt-2">
                    <code className="d-block p-2 bg-light border rounded">
                      VITE_GOOGLE_MAPS_API_KEY=your_actual_api_key_here
                    </code>
                  </div>
                </CListGroupItem>
                <CListGroupItem>
                  <strong>3.</strong> Restart the application to load the new environment variables
                </CListGroupItem>
              </CListGroup>

              <h6 className="mb-3">Step 3: Security Best Practices</h6>
              <CListGroup>
                <CListGroupItem className="d-flex align-items-start">
                  <FontAwesomeIcon icon={faCheckCircle} className="text-success me-2 mt-1" />
                  <div>
                    <strong>Restrict API Key Usage:</strong> Configure HTTP referrer restrictions in Google Cloud Console
                  </div>
                </CListGroupItem>
                <CListGroupItem className="d-flex align-items-start">
                  <FontAwesomeIcon icon={faCheckCircle} className="text-success me-2 mt-1" />
                  <div>
                    <strong>Monitor Usage:</strong> Set up billing alerts and usage quotas
                  </div>
                </CListGroupItem>
                <CListGroupItem className="d-flex align-items-start">
                  <FontAwesomeIcon icon={faCheckCircle} className="text-success me-2 mt-1" />
                  <div>
                    <strong>Environment Security:</strong> Never commit API keys to version control
                  </div>
                </CListGroupItem>
              </CListGroup>
            </CCardBody>
          </CCard>
        </CCollapse>
      </CCardBody>
    </CCard>
  );
};

export default GoogleMapsSetupGuide;