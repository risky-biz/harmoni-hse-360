import React from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CCallout,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCog, faArrowLeft } from '@fortawesome/free-solid-svg-icons';
import { useNavigate } from 'react-router-dom';

const SecurityControls: React.FC = () => {
  const navigate = useNavigate();

  return (
    <CRow>
      <CCol>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0">
                <FontAwesomeIcon icon={faCog} size="lg" className="me-2 text-primary" />
                Security Controls
              </h4>
              <small className="text-muted">
                Manage security policies, controls, and configurations
              </small>
            </div>
            <CButton
              color="secondary"
              variant="outline"
              onClick={() => navigate('/security/dashboard')}
            >
              <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
              Back to Security Dashboard
            </CButton>
          </CCardHeader>
          <CCardBody>
            <CCallout color="info">
              <div className="text-center py-4">
                <FontAwesomeIcon icon={faCog} size="3x" className="text-muted mb-3" />
                <h5>Security Controls Management</h5>
                <p className="text-muted mb-3">
                  This module will provide comprehensive security controls management including 
                  access controls, security policies, compliance frameworks, and automated 
                  security monitoring configurations.
                </p>
                <p className="text-muted">
                  <strong>Coming Soon:</strong> Security controls configuration and management tools
                </p>
              </div>
            </CCallout>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default SecurityControls;