import React from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
} from '@coreui/react';

const MyWasteReports: React.FC = () => {
  return (
    <>
      <CRow>
        <CCol>
          <CCard className="mb-4">
            <CCardHeader>
              <strong>My Waste Reports</strong>
            </CCardHeader>
            <CCardBody>
              <p>
                View and manage waste reports that you have submitted.
              </p>
              
              <div className="text-muted">
                User-specific waste report listing and management coming soon...
              </div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default MyWasteReports;