import React from 'react';
import { useParams } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
} from '@coreui/react';

const WasteReportDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  return (
    <>
      <CRow>
        <CCol>
          <CCard className="mb-4">
            <CCardHeader>
              <strong>Waste Report Details - #{id}</strong>
            </CCardHeader>
            <CCardBody>
              <p>
                Detailed view for waste report ID: {id}
              </p>
              
              <div className="text-muted">
                Full waste report details, status tracking, and attachment management coming soon...
              </div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default WasteReportDetail;