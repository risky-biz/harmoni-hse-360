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
  useUpdateHealthRecordMutation,
  UpdateHealthRecordCommand
} from '../../features/health/healthApi';
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
  } = useGetHealthRecordQuery({ id: parseInt(id!) });
  
  const [updateHealthRecord, { isLoading: isUpdating, error: updateError }] = useUpdateHealthRecordMutation();

  const [formData, setFormData] = useState<Partial<UpdateHealthRecordCommand>>({
    bloodType: undefined,
    medicalNotes: '',
    isActive: true
  });

  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  // Initialize form data when health record loads
  useEffect(() => {
    if (healthRecord) {
      setFormData({
        bloodType: healthRecord.bloodType || undefined,
        medicalNotes: healthRecord.medicalNotes || '',
        dateOfBirth: healthRecord.dateOfBirth ? formatDateForInput(healthRecord.dateOfBirth) : '',
        isActive: healthRecord.isActive
      });
    }
  }, [healthRecord]);

  const handleInputChange = (field: string, value: string | boolean) => {
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

    // Validate date fields
    if (formData.dateOfBirth) {
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
      const sanitizedData: UpdateHealthRecordCommand = {
        id: parseInt(id!),
        bloodType: formData.bloodType || undefined,
        medicalNotes: formData.medicalNotes?.trim() || undefined,
        dateOfBirth: formData.dateOfBirth || undefined,
        isActive: formData.isActive
      };

      await updateHealthRecord(sanitizedData).unwrap();
      
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
        <CSpinner color="primary" size="sm" />
      </div>
    );
  }

  if (loadError) {
    return (
      <CAlert color="danger" className="d-flex align-items-center">
        <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="sm" />
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
          <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="sm" />
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
            Date of Birth: {healthRecord.dateOfBirth ? new Date(healthRecord.dateOfBirth).toLocaleDateString() : 'Not specified'}
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

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="dateOfBirth">Date of Birth</CFormLabel>
                <CFormInput
                  type="date"
                  id="dateOfBirth"
                  value={formData.dateOfBirth || ''}
                  onChange={(e) => handleInputChange('dateOfBirth', e.target.value)}
                  invalid={!!formErrors.dateOfBirth}
                  feedback={formErrors.dateOfBirth}
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="isActive">Status</CFormLabel>
                <CFormSelect
                  id="isActive"
                  value={formData.isActive ? 'true' : 'false'}
                  onChange={(e) => handleInputChange('isActive', e.target.value === 'true')}
                >
                  <option value="true">Active</option>
                  <option value="false">Inactive</option>
                </CFormSelect>
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