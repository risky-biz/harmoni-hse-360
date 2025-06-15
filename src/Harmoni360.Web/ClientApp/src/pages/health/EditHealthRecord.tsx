import React, { useState, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CForm,
  CFormLabel,
  CFormInput,
  CFormSelect,
  CFormTextarea,
  CButton,
  CRow,
  CCol,
  CAlert,
  CSpinner
} from '@coreui/react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  useGetHealthRecordQuery, 
  useUpdateHealthRecordMutation 
} from '../../features/health/healthApi';
import { BloodType, UpdateHealthRecordRequest } from '../../types/health';
import { formatDateForInput } from '../../utils/dateUtils';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSave, faArrowLeft, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';

const EditHealthRecord: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const {
    data: healthRecord,
    isLoading: isLoadingRecord,
    error: loadError
  } = useGetHealthRecordQuery(id!);
  
  const [updateHealthRecord, { isLoading: isUpdating, error: updateError }] = useUpdateHealthRecordMutation();

  const [formData, setFormData] = useState<UpdateHealthRecordRequest>({
    bloodType: undefined,
    medicalNotes: '',
    primaryDoctorName: '',
    primaryDoctorContact: '',
    insuranceProvider: '',
    insurancePolicyNumber: '',
    lastHealthCheckDate: '',
    nextHealthCheckDate: ''
  });

  const [formErrors, setFormErrors] = useState<Partial<UpdateHealthRecordRequest>>({});

  // Initialize form data when health record loads
  useEffect(() => {
    if (healthRecord) {
      setFormData({
        bloodType: healthRecord.bloodType || undefined,
        medicalNotes: healthRecord.medicalNotes || '',
        primaryDoctorName: healthRecord.primaryDoctorName || '',
        primaryDoctorContact: healthRecord.primaryDoctorContact || '',
        insuranceProvider: healthRecord.insuranceProvider || '',
        insurancePolicyNumber: healthRecord.insurancePolicyNumber || '',
        lastHealthCheckDate: healthRecord.lastHealthCheckDate ? 
          formatDateForInput(healthRecord.lastHealthCheckDate) : '',
        nextHealthCheckDate: healthRecord.nextHealthCheckDate ? 
          formatDateForInput(healthRecord.nextHealthCheckDate) : ''
      });
    }
  }, [healthRecord]);

  const handleInputChange = (field: keyof UpdateHealthRecordRequest, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));

    // Clear error when user starts typing
    if (formErrors[field]) {
      setFormErrors(prev => ({
        ...prev,
        [field]: undefined
      }));
    }
  };

  const validateForm = (): boolean => {
    const errors: Partial<UpdateHealthRecordRequest> = {};

    // Validate date fields
    if (formData.lastHealthCheckDate) {
      const lastCheckDate = new Date(formData.lastHealthCheckDate);
      const today = new Date();
      if (lastCheckDate > today) {
        errors.lastHealthCheckDate = 'Last health check date cannot be in the future';
      }
    }

    if (formData.nextHealthCheckDate && formData.lastHealthCheckDate) {
      const nextCheckDate = new Date(formData.nextHealthCheckDate);
      const lastCheckDate = new Date(formData.lastHealthCheckDate);
      if (nextCheckDate <= lastCheckDate) {
        errors.nextHealthCheckDate = 'Next health check must be after last health check';
      }
    }

    // Validate doctor information consistency
    if (formData.primaryDoctorContact && !formData.primaryDoctorName?.trim()) {
      errors.primaryDoctorName = 'Doctor name is required when contact is provided';
    }

    // Validate insurance information consistency
    if (formData.insurancePolicyNumber && !formData.insuranceProvider?.trim()) {
      errors.insuranceProvider = 'Insurance provider is required when policy number is provided';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      const sanitizedData: UpdateHealthRecordRequest = {
        bloodType: formData.bloodType || undefined,
        medicalNotes: formData.medicalNotes?.trim() || undefined,
        primaryDoctorName: formData.primaryDoctorName?.trim() || undefined,
        primaryDoctorContact: formData.primaryDoctorContact?.trim() || undefined,
        insuranceProvider: formData.insuranceProvider?.trim() || undefined,
        insurancePolicyNumber: formData.insurancePolicyNumber?.trim() || undefined,
        lastHealthCheckDate: formData.lastHealthCheckDate || undefined,
        nextHealthCheckDate: formData.nextHealthCheckDate || undefined
      };

      await updateHealthRecord({ id: id!, updates: sanitizedData }).unwrap();
      
      // Navigate back to detail page
      navigate(`/health/detail/${id}`);
    } catch (err) {
      console.error('Failed to update health record:', err);
    }
  };

  const handleCancel = () => {
    navigate(`/health/detail/${id}`);
  };

  if (isLoadingRecord) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '400px' }}>
        <CSpinner color="primary" size="lg" />
      </div>
    );
  }

  if (loadError) {
    return (
      <CAlert color="danger" className="d-flex align-items-center">
        <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="lg" />
        <div>
          Failed to load health record for editing.
        </div>
      </CAlert>
    );
  }

  if (!healthRecord) {
    return (
      <CAlert color="warning">
        Health record not found.
      </CAlert>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div className="d-flex align-items-center">
          <CButton color="ghost" onClick={handleCancel} className="me-2">
            <FontAwesomeIcon icon={faArrowLeft} />
          </CButton>
          <h2>Edit Health Record</h2>
        </div>
      </div>

      {updateError && (
        <CAlert color="danger" className="d-flex align-items-center mb-4">
          <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="lg" />
          <div>
            {'data' in updateError && updateError.data ? 
              (typeof updateError.data === 'string' ? updateError.data : 'Failed to update health record') :
              'An unexpected error occurred'
            }
          </div>
        </CAlert>
      )}

      {/* Person Info Display */}
      <CCard className="mb-4">
        <CCardBody>
          <h5>{healthRecord.personName}</h5>
          <div className="text-muted">{healthRecord.personEmail} • {healthRecord.personType}</div>
          <div className="small text-muted">
            Date of Birth: {new Date(healthRecord.dateOfBirth).toLocaleDateString()}
          </div>
        </CCardBody>
      </CCard>

      <CCard>
        <CCardHeader>
          <strong>Health Information</strong>
        </CCardHeader>
        <CCardBody>
          <CForm onSubmit={handleSubmit}>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="bloodType">Blood Type</CFormLabel>
                <CFormSelect
                  id="bloodType"
                  value={formData.bloodType || ''}
                  onChange={(e) => handleInputChange('bloodType', e.target.value)}
                >
                  <option value="">Select blood type</option>
                  <option value={BloodType.APositive}>A+</option>
                  <option value={BloodType.ANegative}>A-</option>
                  <option value={BloodType.BPositive}>B+</option>
                  <option value={BloodType.BNegative}>B-</option>
                  <option value={BloodType.ABPositive}>AB+</option>
                  <option value={BloodType.ABNegative}>AB-</option>
                  <option value={BloodType.OPositive}>O+</option>
                  <option value={BloodType.ONegative}>O-</option>
                </CFormSelect>
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={12}>
                <CFormLabel htmlFor="medicalNotes">Medical Notes</CFormLabel>
                <CFormTextarea
                  id="medicalNotes"
                  rows={3}
                  value={formData.medicalNotes || ''}
                  onChange={(e) => handleInputChange('medicalNotes', e.target.value)}
                  placeholder="Enter any relevant medical notes or observations"
                />
              </CCol>
            </CRow>

            <hr className="my-4" />
            <h5>Primary Doctor Information</h5>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="primaryDoctorName">Doctor Name</CFormLabel>
                <CFormInput
                  id="primaryDoctorName"
                  value={formData.primaryDoctorName || ''}
                  onChange={(e) => handleInputChange('primaryDoctorName', e.target.value)}
                  invalid={!!formErrors.primaryDoctorName}
                  feedback={formErrors.primaryDoctorName}
                  placeholder="Dr. Smith"
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="primaryDoctorContact">Doctor Contact</CFormLabel>
                <CFormInput
                  id="primaryDoctorContact"
                  value={formData.primaryDoctorContact || ''}
                  onChange={(e) => handleInputChange('primaryDoctorContact', e.target.value)}
                  placeholder="Phone number or clinic name"
                />
              </CCol>
            </CRow>

            <hr className="my-4" />
            <h5>Insurance Information</h5>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="insuranceProvider">Insurance Provider</CFormLabel>
                <CFormInput
                  id="insuranceProvider"
                  value={formData.insuranceProvider || ''}
                  onChange={(e) => handleInputChange('insuranceProvider', e.target.value)}
                  invalid={!!formErrors.insuranceProvider}
                  feedback={formErrors.insuranceProvider}
                  placeholder="Insurance company name"
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="insurancePolicyNumber">Policy Number</CFormLabel>
                <CFormInput
                  id="insurancePolicyNumber"
                  value={formData.insurancePolicyNumber || ''}
                  onChange={(e) => handleInputChange('insurancePolicyNumber', e.target.value)}
                  placeholder="Policy or member ID"
                />
              </CCol>
            </CRow>

            <hr className="my-4" />
            <h5>Health Check Schedule</h5>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="lastHealthCheckDate">Last Health Check</CFormLabel>
                <CFormInput
                  type="date"
                  id="lastHealthCheckDate"
                  value={formData.lastHealthCheckDate || ''}
                  onChange={(e) => handleInputChange('lastHealthCheckDate', e.target.value)}
                  invalid={!!formErrors.lastHealthCheckDate}
                  feedback={formErrors.lastHealthCheckDate}
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="nextHealthCheckDate">Next Health Check</CFormLabel>
                <CFormInput
                  type="date"
                  id="nextHealthCheckDate"
                  value={formData.nextHealthCheckDate || ''}
                  onChange={(e) => handleInputChange('nextHealthCheckDate', e.target.value)}
                  invalid={!!formErrors.nextHealthCheckDate}
                  feedback={formErrors.nextHealthCheckDate}
                />
              </CCol>
            </CRow>

            <hr className="my-4" />

            <div className="d-flex gap-2">
              <CButton 
                type="submit" 
                color="primary" 
                disabled={isUpdating}
              >
                {isUpdating ? (
                  <>
                    <CSpinner size="sm" className="me-1" />
                    Updating...
                  </>
                ) : (
                  <>
                    <FontAwesomeIcon icon={faSave} className="me-1" />
                    Update Health Record
                  </>
                )}
              </CButton>
              <CButton 
                type="button" 
                color="secondary" 
                variant="outline" 
                onClick={handleCancel}
                disabled={isUpdating}
              >
                Cancel
              </CButton>
            </div>
          </CForm>
        </CCardBody>
      </CCard>
    </div>
  );
};

export default EditHealthRecord;