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
import { useCreateHealthRecordMutation, CreateHealthRecordCommand } from '../../features/health/healthApi';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSave, faArrowLeft, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';

const CreateHealthRecord: React.FC = () => {
  const navigate = useNavigate();
  const [createHealthRecord, { isLoading, error }] = useCreateHealthRecordMutation();

  const [formData, setFormData] = useState<Partial<CreateHealthRecordCommand>>({
    personId: 0,
    personType: 'Student',
    dateOfBirth: '',
    bloodType: undefined,
    medicalNotes: ''
  });

  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  const handleInputChange = (field: string, value: string | number) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));

    // Clear error when user starts typing
    if (formErrors[field]) {
      setFormErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!formData.personId || formData.personId === 0) {
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

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      const sanitizedData: CreateHealthRecordCommand = {
        personId: formData.personId as number,
        personType: formData.personType as string,
        dateOfBirth: formData.dateOfBirth,
        bloodType: formData.bloodType || undefined,
        medicalNotes: formData.medicalNotes?.trim() || undefined
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
                  type="number"
                  id="personId"
                  value={formData.personId || ''}
                  onChange={(e) => handleInputChange('personId', parseInt(e.target.value) || 0)}
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
                  <option value="Student">Student</option>
                  <option value="Staff">Staff</option>
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
                  <option value="APositive">A+</option>
                  <option value="ANegative">A-</option>
                  <option value="BPositive">B+</option>
                  <option value="BNegative">B-</option>
                  <option value="ABPositive">AB+</option>
                  <option value="ABNegative">AB-</option>
                  <option value="OPositive">O+</option>
                  <option value="ONegative">O-</option>
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

            {/* Additional fields can be added via health record editing after creation */}

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