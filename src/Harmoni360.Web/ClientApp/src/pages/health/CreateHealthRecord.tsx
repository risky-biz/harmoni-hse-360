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

  const [useExistingPerson, setUseExistingPerson] = useState(false);
  const [formData, setFormData] = useState<Partial<CreateHealthRecordCommand>>({
    personType: 1, // Student = 1
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

    if (useExistingPerson) {
      // Validate existing person mode
      if (!formData.personId || formData.personId === 0) {
        errors.personId = 'Person ID is required';
      } else if (formData.personId > 2147483647) {
        errors.personId = 'Person ID is too large (maximum value is 2,147,483,647)';
      } else if (formData.personId < 1) {
        errors.personId = 'Person ID must be a positive number';
      }
    } else {
      // Validate new person mode
      if (!formData.personName?.trim()) {
        errors.personName = 'Person name is required';
      } else if (formData.personName.length > 200) {
        errors.personName = 'Person name cannot exceed 200 characters';
      }

      if (!formData.personEmail?.trim()) {
        errors.personEmail = 'Person email is required';
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.personEmail)) {
        errors.personEmail = 'Please enter a valid email address';
      } else if (formData.personEmail.length > 256) {
        errors.personEmail = 'Person email cannot exceed 256 characters';
      }
    }

    // Validate date of birth if provided
    if (formData.dateOfBirth) {
      const birthDate = new Date(formData.dateOfBirth);
      const today = new Date();
      if (birthDate > today) {
        errors.dateOfBirth = 'Date of birth cannot be in the future';
      }
    }

    // Validate date of birth is required for students
    if (formData.personType === 1 && !formData.dateOfBirth) { // Student = 1
      errors.dateOfBirth = 'Date of birth is required for student records';
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
        personType: formData.personType as number,
        dateOfBirth: formData.dateOfBirth,
        bloodType: formData.bloodType || undefined,
        medicalNotes: formData.medicalNotes?.trim() || undefined
      };

      if (useExistingPerson) {
        sanitizedData.personId = formData.personId as number;
      } else {
        sanitizedData.personName = formData.personName?.trim();
        sanitizedData.personEmail = formData.personEmail?.trim();
        sanitizedData.personPhoneNumber = formData.personPhoneNumber?.trim() || undefined;
        sanitizedData.personDepartment = formData.personDepartment?.trim() || undefined;
        sanitizedData.personPosition = formData.personPosition?.trim() || undefined;
        sanitizedData.personEmployeeId = formData.personEmployeeId?.trim() || undefined;
      }

      const result = await createHealthRecord(sanitizedData).unwrap();
      
      // Navigate to the health records list or detail page
      navigate('/health');
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
          <strong>Person Information</strong>
        </CCardHeader>
        <CCardBody>
          <CForm onSubmit={handleSubmit}>
            {/* Person Mode Toggle */}
            <CRow className="mb-4">
              <CCol md={12}>
                <div className="d-flex gap-3">
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="radio"
                      id="newPerson"
                      name="personMode"
                      checked={!useExistingPerson}
                      onChange={() => setUseExistingPerson(false)}
                    />
                    <label className="form-check-label" htmlFor="newPerson">
                      Create New Person
                    </label>
                  </div>
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="radio"
                      id="existingPerson"
                      name="personMode"
                      checked={useExistingPerson}
                      onChange={() => setUseExistingPerson(true)}
                    />
                    <label className="form-check-label" htmlFor="existingPerson">
                      Use Existing Person
                    </label>
                  </div>
                </div>
              </CCol>
            </CRow>

            {useExistingPerson ? (
              /* Existing Person Mode */
              <CRow className="mb-3">
                <CCol md={6}>
                  <CFormLabel htmlFor="personId">Person ID *</CFormLabel>
                  <CFormInput
                    type="number"
                    id="personId"
                    value={formData.personId || ''}
                    onChange={(e) => {
                      const value = parseInt(e.target.value) || 0;
                      if (value <= 2147483647) {
                        handleInputChange('personId', value);
                      }
                    }}
                    invalid={!!formErrors.personId}
                    feedback={formErrors.personId}
                    placeholder="Enter existing person ID"
                    min="1"
                    max="2147483647"
                  />
                  <div className="form-text">
                    <small className="text-muted">
                      Enter the ID of an existing person in the system
                    </small>
                  </div>
                </CCol>
                <CCol md={6}>
                  <CFormLabel htmlFor="personType">Person Type *</CFormLabel>
                  <CFormSelect
                    id="personType"
                    value={formData.personType}
                    onChange={(e) => handleInputChange('personType', parseInt(e.target.value))}
                  >
                    <option value={1}>Student</option>
                    <option value={2}>Staff</option>
                    <option value={3}>Visitor</option>
                    <option value={4}>Contractor</option>
                  </CFormSelect>
                </CCol>
              </CRow>
            ) : (
              /* New Person Mode */
              <>
                <CRow className="mb-3">
                  <CCol md={6}>
                    <CFormLabel htmlFor="personName">Full Name *</CFormLabel>
                    <CFormInput
                      type="text"
                      id="personName"
                      value={formData.personName || ''}
                      onChange={(e) => handleInputChange('personName', e.target.value)}
                      invalid={!!formErrors.personName}
                      feedback={formErrors.personName}
                      placeholder="Enter person's full name"
                      maxLength={200}
                    />
                  </CCol>
                  <CCol md={6}>
                    <CFormLabel htmlFor="personEmail">Email Address *</CFormLabel>
                    <CFormInput
                      type="email"
                      id="personEmail"
                      value={formData.personEmail || ''}
                      onChange={(e) => handleInputChange('personEmail', e.target.value)}
                      invalid={!!formErrors.personEmail}
                      feedback={formErrors.personEmail}
                      placeholder="Enter email address"
                      maxLength={256}
                    />
                  </CCol>
                </CRow>

                <CRow className="mb-3">
                  <CCol md={6}>
                    <CFormLabel htmlFor="personType">Person Type *</CFormLabel>
                    <CFormSelect
                      id="personType"
                      value={formData.personType}
                      onChange={(e) => handleInputChange('personType', parseInt(e.target.value))}
                    >
                      <option value={1}>Student</option>
                      <option value={2}>Staff</option>
                      <option value={3}>Visitor</option>
                      <option value={4}>Contractor</option>
                    </CFormSelect>
                  </CCol>
                  <CCol md={6}>
                    <CFormLabel htmlFor="personPhoneNumber">Phone Number</CFormLabel>
                    <CFormInput
                      type="tel"
                      id="personPhoneNumber"
                      value={formData.personPhoneNumber || ''}
                      onChange={(e) => handleInputChange('personPhoneNumber', e.target.value)}
                      placeholder="Enter phone number"
                      maxLength={50}
                    />
                  </CCol>
                </CRow>

                <CRow className="mb-3">
                  <CCol md={6}>
                    <CFormLabel htmlFor="personDepartment">Department</CFormLabel>
                    <CFormInput
                      type="text"
                      id="personDepartment"
                      value={formData.personDepartment || ''}
                      onChange={(e) => handleInputChange('personDepartment', e.target.value)}
                      placeholder="Enter department"
                      maxLength={100}
                    />
                  </CCol>
                  <CCol md={6}>
                    <CFormLabel htmlFor="personPosition">Position</CFormLabel>
                    <CFormInput
                      type="text"
                      id="personPosition"
                      value={formData.personPosition || ''}
                      onChange={(e) => handleInputChange('personPosition', e.target.value)}
                      placeholder="Enter position/title"
                      maxLength={100}
                    />
                  </CCol>
                </CRow>

                <CRow className="mb-3">
                  <CCol md={6}>
                    <CFormLabel htmlFor="personEmployeeId">Employee ID</CFormLabel>
                    <CFormInput
                      type="text"
                      id="personEmployeeId"
                      value={formData.personEmployeeId || ''}
                      onChange={(e) => handleInputChange('personEmployeeId', e.target.value)}
                      placeholder="Enter employee/student ID"
                      maxLength={50}
                    />
                  </CCol>
                </CRow>
              </>
            )}
          </CForm>
        </CCardBody>
      </CCard>

      <CCard className="mt-3">
        <CCardHeader>
          <strong>Health Information</strong>
        </CCardHeader>
        <CCardBody>
          <CForm onSubmit={handleSubmit}>
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
                  onChange={(e) => handleInputChange('bloodType', e.target.value ? parseInt(e.target.value) : undefined)}
                >
                  <option value="">Select blood type</option>
                  <option value={1}>A+</option>
                  <option value={2}>A-</option>
                  <option value={3}>B+</option>
                  <option value={4}>B-</option>
                  <option value={5}>AB+</option>
                  <option value={6}>AB-</option>
                  <option value={7}>O+</option>
                  <option value={8}>O-</option>
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