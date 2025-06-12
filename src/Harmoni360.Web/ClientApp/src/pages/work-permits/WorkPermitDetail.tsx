import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  CContainer,
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CSpinner,
  CAlert
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faArrowLeft,
  faExclamationTriangle
} from '@fortawesome/free-solid-svg-icons';

import { useGetWorkPermitByIdQuery } from '../../features/work-permits/workPermitApi';

const WorkPermitDetail: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const {
    data: permit,
    error,
    isLoading
  } = useGetWorkPermitByIdQuery(id!);

  if (isLoading) {
    return (
      <CContainer fluid>
        <div className="text-center py-4">
          <CSpinner color="primary" />
          <div className="mt-2">Loading work permit...</div>
        </div>
      </CContainer>
    );
  }

  if (error) {
    return (
      <CContainer fluid>
        <CAlert color="danger">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
          Error loading work permit
        </CAlert>
      </CContainer>
    );
  }

  return (
    <CContainer fluid>
      <CRow>
        <CCol>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <h4 className="mb-0">Work Permit Detail</h4>
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
              {permit ? (
                <div>
                  <h5>{permit.title}</h5>
                  <p>{permit.description}</p>
                  <p><strong>Type:</strong> {permit.type}</p>
                  <p><strong>Status:</strong> {permit.status}</p>
                  <p><strong>Location:</strong> {permit.workLocation}</p>
                </div>
              ) : (
                <p>Work permit not found</p>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </CContainer>
  );
};

export default WorkPermitDetail;