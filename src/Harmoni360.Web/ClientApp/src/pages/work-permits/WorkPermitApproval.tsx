import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CContainer,
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CAlert
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faArrowLeft,
  faInfoCircle
} from '@fortawesome/free-solid-svg-icons';

const WorkPermitApproval: React.FC = () => {
  const navigate = useNavigate();

  return (
    <CContainer fluid>
      <CRow>
        <CCol>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <h4 className="mb-0">Work Permit Approvals</h4>
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate('/work-permits')}
              >
                <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                Back to List
              </CButton>
            </CCardHeader>
            <CCardBody>
              <CAlert color="info">
                <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                Work permit approval functionality is coming soon.
              </CAlert>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </CContainer>
  );
};

export default WorkPermitApproval;