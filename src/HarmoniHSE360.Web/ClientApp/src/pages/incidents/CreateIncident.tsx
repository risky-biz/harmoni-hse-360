import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CForm,
  CFormInput,
  CFormTextarea,
  CFormSelect,
  CFormLabel,
  CButton,
  CAlert,
  CSpinner,
  CInputGroup,
  CInputGroupText,
  CCallout,
  CBadge,
  CAccordion,
  CAccordionBody,
  CAccordionHeader,
  CAccordionItem,
} from '@coreui/react';
import CIcon from '@coreui/icons-react';
import {
  cilWarning,
  cilInfo,
  cilClipboard,
  cilFile,
  cilTask,
  cilSpeedometer,
} from '@coreui/icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faMapPin } from '@fortawesome/free-solid-svg-icons';
import { useCreateIncidentMutation, useUploadIncidentAttachmentsMutation, CreateIncidentRequest } from '../../features/incidents/incidentApi';

// Validation schema based on FRQ-INC-001 requirements
const schema = yup.object({
  title: yup
    .string()
    .required('Incident title is required')
    .min(5, 'Title must be at least 5 characters')
    .max(100, 'Title must not exceed 100 characters'),
  description: yup
    .string()
    .required('Description is required')
    .min(10, 'Description must be at least 10 characters')
    .max(1000, 'Description must not exceed 1000 characters'),
  severity: yup
    .string()
    .required('Severity level is required')
    .oneOf(['Minor', 'Moderate', 'Serious', 'Critical'], 'Please select a valid severity level'),
  incidentDate: yup
    .string()
    .required('Incident date and time is required'),
  location: yup
    .string()
    .required('Location is required')
    .min(3, 'Location must be at least 3 characters'),
  category: yup
    .string()
    .required('Incident category is required'),
  latitude: yup
    .number()
    .optional(),
  longitude: yup
    .number()
    .optional(),
  involvedPersons: yup
    .string()
    .optional()
    .max(500, 'Involved persons description must not exceed 500 characters'),
  immediateActions: yup
    .string()
    .optional()
    .max(500, 'Immediate actions must not exceed 500 characters'),
});

interface IncidentFormData {
  title: string;
  description: string;
  severity: 'Minor' | 'Moderate' | 'Serious' | 'Critical';
  incidentDate: string;
  location: string;
  category: string;
  involvedPersons?: string;
  immediateActions?: string;
  latitude?: number;
  longitude?: number;
}

const CreateIncident: React.FC = () => {
  const navigate = useNavigate();
  const [createIncident, { isLoading: isSubmitting }] = useCreateIncidentMutation();
  const [uploadAttachments, { isLoading: isUploading }] = useUploadIncidentAttachmentsMutation();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [autoSaveStatus, setAutoSaveStatus] = useState<'saved' | 'saving' | 'error' | null>(null);
  const [locationLoading, setLocationLoading] = useState(false);
  const [locationInputMode, setLocationInputMode] = useState<'dropdown' | 'text'>('dropdown');
  const [uploadedFiles, setUploadedFiles] = useState<File[]>([]);

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    watch,
    setValue,
    getValues,
  } = useForm<IncidentFormData>({
    resolver: yupResolver(schema) as any,
    defaultValues: {
      title: '',
      description: '',
      severity: 'Minor',
      incidentDate: new Date().toISOString().slice(0, 16), // Current date-time
      location: '',
      category: '',
      involvedPersons: '',
      immediateActions: '',
    },
  });

  // Auto-save functionality (per FRQ-INC-001: auto-save every 30 seconds)
  useEffect(() => {
    if (!isDirty) return;

    const autoSaveTimer = setInterval(() => {
      const formData = getValues();
      setAutoSaveStatus('saving');
      
      // Simulate auto-save (replace with actual implementation)
      setTimeout(() => {
        try {
          localStorage.setItem('incident_draft', JSON.stringify(formData));
          setAutoSaveStatus('saved');
          setTimeout(() => setAutoSaveStatus(null), 2000);
        } catch (error) {
          setAutoSaveStatus('error');
          setTimeout(() => setAutoSaveStatus(null), 3000);
        }
      }, 500);
    }, 30000); // 30 seconds

    return () => clearInterval(autoSaveTimer);
  }, [isDirty, getValues]);

  // Load draft from localStorage on component mount
  useEffect(() => {
    const draft = localStorage.getItem('incident_draft');
    if (draft) {
      try {
        const parsedDraft = JSON.parse(draft);
        Object.keys(parsedDraft).forEach(key => {
          setValue(key as keyof IncidentFormData, parsedDraft[key]);
        });
      } catch (error) {
        console.warn('Failed to load draft:', error);
      }
    }
  }, [setValue]);

  // Get current location
  const getCurrentLocation = () => {
    setLocationLoading(true);
    setLocationInputMode('text');
    
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          setValue('latitude', position.coords.latitude);
          setValue('longitude', position.coords.longitude);
          
          // Set coordinates as text in the location field
          setValue('location', `${position.coords.latitude.toFixed(6)}, ${position.coords.longitude.toFixed(6)}`);
          setLocationLoading(false);
        },
        (error) => {
          console.error('Geolocation error:', error);
          setLocationLoading(false);
          setLocationInputMode('dropdown'); // Revert to dropdown on error
          alert('Unable to get your location. Please enter the location manually.');
        }
      );
    } else {
      setLocationLoading(false);
      setLocationInputMode('dropdown'); // Revert to dropdown if not supported
      alert('Geolocation is not supported by this browser.');
    }
  };

  // Handle file upload (photos/videos per FRQ-INC-001)
  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files || []);
    const validFiles = files.filter(file => {
      const isImage = file.type.startsWith('image/');
      const isVideo = file.type.startsWith('video/');
      const isValidSize = file.size <= 50 * 1024 * 1024; // 50MB max
      
      return (isImage || isVideo) && isValidSize;
    });
    
    if (validFiles.length !== files.length) {
      alert('Some files were skipped. Only images and videos under 50MB are allowed.');
    }
    
    setUploadedFiles(prev => [...prev, ...validFiles]);
  };

  // Remove uploaded file
  const removeFile = (index: number) => {
    setUploadedFiles(prev => prev.filter((_, i) => i !== index));
  };

  // Submit form
  const onSubmit = async (data: IncidentFormData) => {
    setSubmitError(null);

    try {
      // Prepare the request data
      const createRequest: CreateIncidentRequest = {
        title: data.title,
        description: data.description,
        severity: data.severity,
        incidentDate: new Date(data.incidentDate).toISOString(),
        location: data.location,
        latitude: data.latitude,
        longitude: data.longitude,
        witnessNames: data.involvedPersons || undefined,
        immediateActionsTaken: data.immediateActions || undefined,
      };

      // Submit to API
      const result = await createIncident(createRequest).unwrap();
      
      // Upload files if any
      if (uploadedFiles.length > 0) {
        try {
          await uploadAttachments({
            incidentId: result.id,
            files: uploadedFiles
          }).unwrap();
          console.log(`Successfully uploaded ${uploadedFiles.length} files for incident ${result.id}`);
        } catch (uploadError) {
          console.error('Failed to upload files:', uploadError);
          // Continue with success flow - incident was created successfully
          // File upload failure is not critical
        }
      }
      
      // Clear draft after successful submission
      localStorage.removeItem('incident_draft');
      
      // Navigate to incidents list with success message
      navigate('/incidents', { 
        state: { 
          message: 'Incident reported successfully!',
          type: 'success'
        }
      });
    } catch (error: any) {
      // Handle API error
      if (error.data?.message) {
        setSubmitError(error.data.message);
      } else if (error.data?.errors) {
        // Handle validation errors
        const errorMessages = Object.values(error.data.errors).flat().join(', ');
        setSubmitError(errorMessages);
      } else {
        setSubmitError('Failed to submit incident report. Please try again.');
      }
      console.error('Submit error:', error);
    }
  };

  // Incident categories based on Epic 1 requirements
  const incidentCategories = [
    { value: 'student_injury', label: 'Student Injury (Sports, Playground, Classroom)' },
    { value: 'staff_injury', label: 'Staff/Teacher Injury' },
    { value: 'visitor_incident', label: 'Visitor Incident' },
    { value: 'property_damage', label: 'Property Damage' },
    { value: 'environmental', label: 'Environmental Incident' },
    { value: 'security', label: 'Security Incident' },
    { value: 'near_miss', label: 'Near-Miss Event' },
    { value: 'medical_emergency', label: 'Medical Emergency' },
    { value: 'lab_accident', label: 'Laboratory Accident' },
    { value: 'transportation', label: 'Transportation Incident' },
  ];

  // Campus locations (replace with dynamic data)
  const campusLocations = [
    'Main Building - Ground Floor',
    'Main Building - 1st Floor',
    'Main Building - 2nd Floor',
    'Science Wing - Chemistry Lab',
    'Science Wing - Physics Lab',
    'Science Wing - Biology Lab',
    'Library - Main Hall',
    'Library - Study Rooms',
    'Gymnasium - Main Court',
    'Gymnasium - Equipment Room',
    'Cafeteria - Dining Area',
    'Cafeteria - Kitchen',
    'Playground - Primary',
    'Playground - Secondary',
    'Swimming Pool Area',
    'Parking Area',
    'Sports Field',
    'Other (specify in description)',
  ];

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0" style={{ color: 'var(--harmoni-charcoal)', fontFamily: 'Poppins, sans-serif' }}>
                <CIcon icon={cilWarning} size="lg" className="me-2 text-warning" />
                Report New Incident
              </h4>
              <small className="text-muted">Fill out all required information to report an incident</small>
            </div>
            <div className="d-flex align-items-center gap-2">
              {autoSaveStatus && (
                <CBadge 
                  color={autoSaveStatus === 'saved' ? 'success' : autoSaveStatus === 'saving' ? 'info' : 'danger'}
                  className="d-flex align-items-center"
                >
                  {autoSaveStatus === 'saving' && <CSpinner size="sm" className="me-1" />}
                  <CIcon 
                    icon={autoSaveStatus === 'saved' ? cilTask : cilInfo} 
                    size="sm" 
                    className="me-1" 
                  />
                  {autoSaveStatus === 'saved' ? 'Auto-saved' : 
                   autoSaveStatus === 'saving' ? 'Saving...' : 'Save failed'}
                </CBadge>
              )}
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate('/incidents')}
              >
                <CIcon icon={cilSpeedometer} size="sm" className="me-1" />
                Back to List
              </CButton>
            </div>
          </CCardHeader>
          
          <CCardBody>
            <CForm onSubmit={handleSubmit(onSubmit)}>
              {submitError && (
                <CAlert color="danger" dismissible onClose={() => setSubmitError(null)}>
                  {submitError}
                </CAlert>
              )}

              <CCallout color="info" className="mb-4">
                <CIcon icon={cilInfo} className="me-2" />
                <strong>Important:</strong> Report incidents as soon as possible. All serious incidents must be reported within 2 hours according to Indonesian regulations.
              </CCallout>

              {/* Basic Information Section */}
              <CAccordion>
                <CAccordionItem itemKey={1}>
                  <CAccordionHeader>
                    <strong>1. Basic Information</strong>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="title">Incident Title *</CFormLabel>
                        <CFormInput
                          id="title"
                          {...register('title')}
                          invalid={!!errors.title}
                          placeholder="Brief description of what happened"
                        />
                        {errors.title && (
                          <div className="invalid-feedback d-block">{errors.title.message}</div>
                        )}
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="severity">Severity Level *</CFormLabel>
                        <CFormSelect
                          id="severity"
                          {...register('severity')}
                          invalid={!!errors.severity}
                        >
                          <option value="Minor">Minor</option>
                          <option value="Moderate">Moderate</option>
                          <option value="Serious">Serious</option>
                          <option value="Critical">Critical</option>
                        </CFormSelect>
                        {errors.severity && (
                          <div className="invalid-feedback d-block">{errors.severity.message}</div>
                        )}
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="category">Category *</CFormLabel>
                        <CFormSelect
                          id="category"
                          {...register('category')}
                          invalid={!!errors.category}
                        >
                          <option value="">Select category...</option>
                          {incidentCategories.map(cat => (
                            <option key={cat.value} value={cat.value}>
                              {cat.label}
                            </option>
                          ))}
                        </CFormSelect>
                        {errors.category && (
                          <div className="invalid-feedback d-block">{errors.category.message}</div>
                        )}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol xs={12}>
                        <CFormLabel htmlFor="description">Detailed Description *</CFormLabel>
                        <CFormTextarea
                          id="description"
                          rows={4}
                          {...register('description')}
                          invalid={!!errors.description}
                          placeholder="Describe what happened in detail. Include any relevant circumstances, conditions, or factors that may have contributed to the incident."
                        />
                        {errors.description && (
                          <div className="invalid-feedback d-block">{errors.description.message}</div>
                        )}
                        <small className="text-muted">
                          {watch('description')?.length || 0}/1000 characters
                        </small>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Location and Time Section */}
                <CAccordionItem itemKey={2}>
                  <CAccordionHeader>
                    <strong>2. Location and Time</strong>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="incidentDate">Incident Date and Time *</CFormLabel>
                        <CInputGroup>
                          <CInputGroupText>
                            <CIcon icon={cilClipboard} />
                          </CInputGroupText>
                          <CFormInput
                            id="incidentDate"
                            type="datetime-local"
                            {...register('incidentDate')}
                            invalid={!!errors.incidentDate}
                          />
                        </CInputGroup>
                        {errors.incidentDate && (
                          <div className="invalid-feedback d-block">{errors.incidentDate.message}</div>
                        )}
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="location">Location *</CFormLabel>
                        <CInputGroup>
                          {locationInputMode === 'dropdown' ? (
                            <CFormSelect
                              id="location"
                              {...register('location')}
                              invalid={!!errors.location}
                            >
                              <option value="">Select location...</option>
                              {campusLocations.map(location => (
                                <option key={location} value={location}>
                                  {location}
                                </option>
                              ))}
                            </CFormSelect>
                          ) : (
                            <CFormInput
                              id="location"
                              {...register('location')}
                              invalid={!!errors.location}
                              placeholder="Enter coordinates or location details"
                              readOnly={locationLoading}
                            />
                          )}
                          <CButton
                            type="button"
                            color="primary"
                            variant="outline"
                            onClick={getCurrentLocation}
                            disabled={locationLoading}
                          >
                            {locationLoading ? (
                              <CSpinner size="sm" />
                            ) : (
                              <FontAwesomeIcon icon={faMapPin} />
                            )}
                          </CButton>
                          {locationInputMode === 'text' && (
                            <CButton
                              type="button"
                              color="secondary"
                              variant="outline"
                              onClick={() => {
                                setLocationInputMode('dropdown');
                                setValue('location', '');
                                setValue('latitude', undefined);
                                setValue('longitude', undefined);
                              }}
                              title="Switch back to dropdown"
                            >
                              ↩
                            </CButton>
                          )}
                        </CInputGroup>
                        {errors.location && (
                          <div className="invalid-feedback d-block">{errors.location.message}</div>
                        )}
                        <small className="text-muted">
                          {locationInputMode === 'dropdown' 
                            ? 'Click the location button to use GPS coordinates' 
                            : 'GPS coordinates will be stored in the database. Click ↩ to use dropdown again.'}
                        </small>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Additional Details Section */}
                <CAccordionItem itemKey={3}>
                  <CAccordionHeader>
                    <strong>3. Additional Details</strong>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="involvedPersons">Involved Persons</CFormLabel>
                        <CFormTextarea
                          id="involvedPersons"
                          rows={3}
                          {...register('involvedPersons')}
                          invalid={!!errors.involvedPersons}
                          placeholder="List any persons involved (witnesses, injured parties, etc.)"
                        />
                        {errors.involvedPersons && (
                          <div className="invalid-feedback d-block">{errors.involvedPersons.message}</div>
                        )}
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="immediateActions">Immediate Actions Taken</CFormLabel>
                        <CFormTextarea
                          id="immediateActions"
                          rows={3}
                          {...register('immediateActions')}
                          invalid={!!errors.immediateActions}
                          placeholder="Describe any immediate actions taken to address the incident"
                        />
                        {errors.immediateActions && (
                          <div className="invalid-feedback d-block">{errors.immediateActions.message}</div>
                        )}
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Evidence Upload Section */}
                <CAccordionItem itemKey={4}>
                  <CAccordionHeader>
                    <strong>4. Evidence (Photos/Videos)</strong>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <CFormLabel htmlFor="files">Upload Photos or Videos</CFormLabel>
                      <CInputGroup>
                        <CFormInput
                          id="files"
                          type="file"
                          multiple
                          accept="image/*,video/*"
                          onChange={handleFileUpload}
                        />
                        <CInputGroupText>
                          <CIcon icon={cilFile} />
                        </CInputGroupText>
                      </CInputGroup>
                      <small className="text-muted">
                        Maximum 5 photos, 2-minute videos. Files must be under 50MB each.
                      </small>
                    </div>

                    {uploadedFiles.length > 0 && (
                      <div>
                        <h6>Uploaded Files:</h6>
                        {uploadedFiles.map((file, index) => (
                          <CBadge
                            key={index}
                            color="success"
                            className="me-2 mb-2 d-inline-flex align-items-center"
                          >
                            <CIcon icon={cilFile} size="sm" className="me-1" />
                            {file.name}
                            <CButton
                              size="sm"
                              color="light"
                              className="ms-2 p-0"
                              style={{ width: '16px', height: '16px' }}
                              onClick={() => removeFile(index)}
                            >
                              ×
                            </CButton>
                          </CBadge>
                        ))}
                      </div>
                    )}
                  </CAccordionBody>
                </CAccordionItem>
              </CAccordion>

              {/* Submit Section */}
              <div className="d-flex justify-content-between align-items-center mt-4 pt-3 border-top">
                <div className="text-muted">
                  <small>
                    <CIcon icon={cilInfo} size="sm" className="me-1" />
                    Form auto-saves every 30 seconds
                  </small>
                </div>
                <div className="d-flex gap-2">
                  <CButton
                    type="button"
                    color="secondary"
                    variant="outline"
                    onClick={() => navigate('/incidents')}
                    disabled={isSubmitting}
                  >
                    Cancel
                  </CButton>
                  <CButton
                    type="submit"
                    color="primary"
                    disabled={isSubmitting || isUploading}
                    className="d-flex align-items-center"
                  >
                    {isSubmitting ? (
                      <>
                        <CSpinner size="sm" className="me-2" />
                        Submitting...
                      </>
                    ) : isUploading ? (
                      <>
                        <CSpinner size="sm" className="me-2" />
                        Uploading files...
                      </>
                    ) : (
                      <>
                        <CIcon icon={cilTask} size="sm" className="me-2" />
                        Submit Report
                      </>
                    )}
                  </CButton>
                </div>
              </div>
            </CForm>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default CreateIncident;