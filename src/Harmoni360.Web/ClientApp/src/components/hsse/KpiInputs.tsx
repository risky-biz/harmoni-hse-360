import React from 'react';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CFormInput,
  CFormLabel,
  CButton,
  CFormText,
} from '@coreui/react';
import { KpiInputs as KpiInputsType } from '../../types/hsse';

interface KpiInputsProps {
  inputs: KpiInputsType;
  onInputChange: (inputs: KpiInputsType) => void;
  onCalculate: () => void;
  isLoading?: boolean;
}

const KpiInputs: React.FC<KpiInputsProps> = ({
  inputs,
  onInputChange,
  onCalculate,
  isLoading = false,
}) => {
  const handleInputChange = (field: keyof KpiInputsType, value: string) => {
    const numericValue = parseFloat(value) || 0;
    onInputChange({
      ...inputs,
      [field]: numericValue,
    });
  };

  return (
    <CCard className="mb-4">
      <CCardHeader>
        <h5 className="mb-0">KPI Calculation Parameters</h5>
        <CFormText>
          Enter the required values to calculate HSSE Key Performance Indicators
        </CFormText>
      </CCardHeader>
      <CCardBody>
        <CRow>
          <CCol md={6} lg={4} className="mb-3">
            <CFormLabel htmlFor="hoursWorked">
              Total Hours Worked <span className="text-danger">*</span>
            </CFormLabel>
            <CFormInput
              id="hoursWorked"
              type="number"
              min="0"
              step="0.01"
              placeholder="e.g., 2080000"
              value={inputs.hoursWorked || ''}
              onChange={(e) => handleInputChange('hoursWorked', e.target.value)}
            />
            <CFormText>Total employee hours worked in the period</CFormText>
          </CCol>
          
          <CCol md={6} lg={4} className="mb-3">
            <CFormLabel htmlFor="lostTimeInjuries">
              Lost Time Injuries <span className="text-danger">*</span>
            </CFormLabel>
            <CFormInput
              id="lostTimeInjuries"
              type="number"
              min="0"
              placeholder="e.g., 12"
              value={inputs.lostTimeInjuries || ''}
              onChange={(e) => handleInputChange('lostTimeInjuries', e.target.value)}
            />
            <CFormText>Number of injuries resulting in lost time</CFormText>
          </CCol>
          
          <CCol md={6} lg={4} className="mb-3">
            <CFormLabel htmlFor="daysLost">
              Days Lost <span className="text-danger">*</span>
            </CFormLabel>
            <CFormInput
              id="daysLost"
              type="number"
              min="0"
              placeholder="e.g., 240"
              value={inputs.daysLost || ''}
              onChange={(e) => handleInputChange('daysLost', e.target.value)}
            />
            <CFormText>Total days lost due to incidents</CFormText>
          </CCol>
          
          <CCol md={6} lg={4} className="mb-3">
            <CFormLabel htmlFor="compliantRecords">
              Compliant Records <span className="text-danger">*</span>
            </CFormLabel>
            <CFormInput
              id="compliantRecords"
              type="number"
              min="0"
              placeholder="e.g., 950"
              value={inputs.compliantRecords || ''}
              onChange={(e) => handleInputChange('compliantRecords', e.target.value)}
            />
            <CFormText>Number of compliant safety records</CFormText>
          </CCol>
          
          <CCol md={6} lg={4} className="mb-3">
            <CFormLabel htmlFor="totalRecords">
              Total Records <span className="text-danger">*</span>
            </CFormLabel>
            <CFormInput
              id="totalRecords"
              type="number"
              min="0"
              placeholder="e.g., 1000"
              value={inputs.totalRecords || ''}
              onChange={(e) => handleInputChange('totalRecords', e.target.value)}
            />
            <CFormText>Total number of safety records</CFormText>
          </CCol>
          
          <CCol md={6} lg={4} className="mb-3 d-flex align-items-end">
            <CButton
              color="primary"
              onClick={onCalculate}
              disabled={isLoading || !inputs.hoursWorked}
              className="w-100"
            >
              {isLoading ? 'Calculating...' : 'Calculate KPIs'}
            </CButton>
          </CCol>
        </CRow>
        
        <div className="mt-3 p-3 bg-light rounded">
          <h6>KPI Calculation Formulas:</h6>
          <ul className="mb-0 small">
            <li><strong>TRIR:</strong> (Total Recordable Incidents × 200,000) ÷ Total Hours Worked</li>
            <li><strong>LTIFR:</strong> (Lost Time Injuries × 1,000,000) ÷ Total Hours Worked</li>
            <li><strong>Severity Rate:</strong> (Total Days Lost × 200,000) ÷ Total Hours Worked</li>
            <li><strong>Compliance Rate:</strong> (Compliant Records × 100) ÷ Total Records</li>
          </ul>
        </div>
      </CCardBody>
    </CCard>
  );
};

export default KpiInputs;