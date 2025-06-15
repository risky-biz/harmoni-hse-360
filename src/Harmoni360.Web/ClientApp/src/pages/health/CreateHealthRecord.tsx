import React, { useState } from 'react';
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
import { useNavigate } from 'react-router-dom';
import { useCreateHealthRecordMutation } from '../../features/health/healthApi';
import { PersonType, BloodType, CreateHealthRecordRequest } from '../../types/health';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSave, faArrowLeft, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';

const CreateHealthRecord: React.FC = () => {
  const navigate = useNavigate();
  const [createHealthRecord, { isLoading, error }] = useCreateHealthRecordMutation();

  const [formData, setFormData] = useState<CreateHealthRecordRequest>({
    personId: '',
    personType: PersonType.Student,
    dateOfBirth: '',
    bloodType: undefined,
    medicalNotes: '',
    primaryDoctorName: '',
    primaryDoctorContact: '',
    insuranceProvider: '',
    insurancePolicyNumber: ''
  });

  const [formErrors, setFormErrors] = useState<Partial<CreateHealthRecordRequest>>({});

  const handleInputChange = (field: keyof CreateHealthRecordRequest, value: string) => {
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
    const errors: Partial<CreateHealthRecordRequest> = {};

    if (!formData.personId.trim()) {
      errors.personId = 'Person ID is required';
    }

    if (!formData.dateOfBirth) {
      errors.dateOfBirth = 'Date of birth is required';
    } else {
      const birthDate = new Date(formData.dateOfBirth);
      const today = new Date();
      if (birthDate > today) {
        errors.dateOfBirth = 'Date of birth cannot be in the future';
      }
    }

    if (formData.primaryDoctorContact && !formData.primaryDoctorName.trim()) {
      errors.primaryDoctorName = 'Doctor name is required when contact is provided';
    }

    if (formData.insurancePolicyNumber && !formData.insuranceProvider.trim()) {
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
      const sanitizedData: CreateHealthRecordRequest = {
        ...formData,
        bloodType: formData.bloodType || undefined,
        medicalNotes: formData.medicalNotes?.trim() || undefined,
        primaryDoctorName: formData.primaryDoctorName?.trim() || undefined,
        primaryDoctorContact: formData.primaryDoctorContact?.trim() || undefined,
        insuranceProvider: formData.insuranceProvider?.trim() || undefined,
        insurancePolicyNumber: formData.insurancePolicyNumber?.trim() || undefined
      };

      const result = await createHealthRecord(sanitizedData).unwrap();
      
      // Navigate to the created health record
      navigate(`/health/detail/${result.id}`);
    } catch (err) {
      console.error('Failed to create health record:', err);
    }
  };

  const handleCancel = () => {
    navigate('/health');
  };

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Create Health Record</h2>
        <CButton color="secondary" variant="outline" onClick={handleCancel}>
          <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
          Back to List
        </CButton>
      </div>

      {error && (
        <CAlert color="danger" className="d-flex align-items-center mb-4">
          <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="lg" />
          <div>
            {'data' in error && error.data ? 
              (typeof error.data === 'string' ? error.data : 'Failed to create health record') :
              'An unexpected error occurred'
            }
          </div>
        </CAlert>
      )}

      <CCard>
        <CCardHeader>
          <strong>Basic Information</strong>
        </CCardHeader>
        <CCardBody>
          <CForm onSubmit={handleSubmit}>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="personId">Person ID *</CFormLabel>
                <CFormInput
                  id="personId"
                  value={formData.personId}
                  onChange={(e) => handleInputChange('personId', e.target.value)}
                  invalid={!!formErrors.personId}
                  feedback={formErrors.personId}
                  placeholder="Enter person ID (student/staff ID)"
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="personType">Person Type *</CFormLabel>
                <CFormSelect
                  id="personType"
                  value={formData.personType}
                  onChange={(e) => handleInputChange('personType', e.target.value)}
                >
                  <option value={PersonType.Student}>Student</option>
                  <option value={PersonType.Staff}>Staff</option>
                </CFormSelect>
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="dateOfBirth">Date of Birth *</CFormLabel>
                <CFormInput
                  type="date"
                  id="dateOfBirth"
                  value={formData.dateOfBirth}
                  onChange={(e) => handleInputChange('dateOfBirth', e.target.value)}
                  invalid={!!formErrors.dateOfBirth}
                  feedback={formErrors.dateOfBirth}
                />
              </CCol>
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

            <div className="d-flex gap-2">
              <CButton 
                type="submit" 
                color="primary" 
                disabled={isLoading}
              >
                {isLoading ? (
                  <>
                    <CSpinner size="sm" className="me-1" />
                    Creating...
                  </>
                ) : (
                  <>
                    <FontAwesomeIcon icon={faSave} className="me-1" />
                    Create Health Record
                  </>
                )}
              </CButton>
              <CButton 
                type="button" 
                color="secondary" 
                variant="outline" 
                onClick={handleCancel}
                disabled={isLoading}
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

export default CreateHealthRecord;