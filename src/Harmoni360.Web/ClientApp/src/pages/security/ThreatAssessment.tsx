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
import { faShieldAlt, faArrowLeft } from '@fortawesome/free-solid-svg-icons';
import { useNavigate } from 'react-router-dom';

const ThreatAssessment: React.FC = () => {
  const navigate = useNavigate();

  return (
    <CRow>
      <CCol>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0">
                <FontAwesomeIcon icon={faShieldAlt} size="lg" className="me-2 text-primary" />
                Threat Assessment
              </h4>
              <small className="text-muted">
                Evaluate and analyze potential security threats
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
                <FontAwesomeIcon icon={faShieldAlt} size="3x" className="text-muted mb-3" />
                <h5>Threat Assessment Module</h5>
                <p className="text-muted mb-3">
                  This module will provide comprehensive threat analysis and risk assessment tools 
                  for security incident management. Features will include threat intelligence integration, 
                  risk scoring, and automated threat detection capabilities.
                </p>
                <p className="text-muted">
                  <strong>Coming Soon:</strong> Advanced threat assessment tools and analytics
                </p>
              </div>
            </CCallout>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default ThreatAssessment;