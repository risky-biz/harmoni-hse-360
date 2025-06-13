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
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faMapPin } from '@fortawesome/free-solid-svg-icons';
import { ACTION_ICONS, CONTEXT_ICONS, FILE_TYPE_ICONS, getCategoryIcon, PRIORITY_ICONS, LOCATION_ICONS } from '../../utils/iconMappings';
import {
  useCreateIncidentMutation,
  useUploadIncidentAttachmentsMutation,
  CreateIncidentRequest,
} from '../../features/incidents/incidentApi';
import { 
  useGetDepartmentsQuery, 
  useGetIncidentCategoriesQuery, 
  useGetIncidentLocationsQuery 
} from '../../api/configurationApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { DemoModeOnly } from '../../components/common/DemoModeWrapper';

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
    .oneOf(
      ['Minor', 'Moderate', 'Serious', 'Critical'],
      'Please select a valid severity level'
    ),
  incidentDate: yup.string().required('Incident date and time is required'),
  location: yup
    .string()
    .required('Location is required')
    .min(3, 'Location must be at least 3 characters'),
  categoryId: yup
    .number()
    .transform((value) => (isNaN(value) ? undefined : value))
    .min(1, 'Please select a valid incident category')
    .required('Incident category is required'),
  departmentId: yup
    .number()
    .transform((value) => (isNaN(value) ? undefined : value))
    .nullable()
    .optional(),
  locationId: yup
    .number()
    .transform((value) => (isNaN(value) ? undefined : value))
    .nullable()
    .optional(),
  latitude: yup.number().optional(),
  longitude: yup.number().optional(),
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
  categoryId?: number;
  departmentId?: number;
  locationId?: number;
  involvedPersons?: string;
  immediateActions?: string;
  latitude?: number;
  longitude?: number;
}

const CreateIncident: React.FC = () => {
  const navigate = useNavigate();
  const { isDemoMode, maxAttachmentSizeMB } = useApplicationMode();
  const [createIncident, { isLoading: isSubmitting }] =
    useCreateIncidentMutation();
  const [uploadAttachments, { isLoading: isUploading }] =
    useUploadIncidentAttachmentsMutation();
  
  // Configuration data queries
  const { data: departments = [], isLoading: isDepartmentsLoading } = useGetDepartmentsQuery({});
  const { data: categories = [], isLoading: isCategoriesLoading } = useGetIncidentCategoriesQuery({});
  const { data: locations = [], isLoading: isLocationsLoading } = useGetIncidentLocationsQuery({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [autoSaveStatus, setAutoSaveStatus] = useState<
    'saved' | 'saving' | 'error' | null
  >(null);
  const [locationLoading, setLocationLoading] = useState(false);
  const [locationInputMode, setLocationInputMode] = useState<
    'dropdown' | 'text'
  >('dropdown');
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
      categoryId: undefined,
      departmentId: undefined,
      locationId: undefined,
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
        Object.keys(parsedDraft).forEach((key) => {
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
          setValue(
            'location',
            `${position.coords.latitude.toFixed(6)}, ${position.coords.longitude.toFixed(6)}`
          );
          setLocationLoading(false);
        },
        (error) => {
          console.error('Geolocation error:', error);
          setLocationLoading(false);
          setLocationInputMode('dropdown'); // Revert to dropdown on error
          alert(
            'Unable to get your location. Please enter the location manually.'
          );
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
    const maxSizeMB = isDemoMode ? maxAttachmentSizeMB : 50;
    const validFiles = files.filter((file) => {
      const isImage = file.type.startsWith('image/');
      const isVideo = file.type.startsWith('video/');
      const isValidSize = file.size <= maxSizeMB * 1024 * 1024;

      return (isImage || isVideo) && isValidSize;
    });

    if (validFiles.length !== files.length) {
      alert(
        `Some files were skipped. Only images and videos under ${maxSizeMB}MB are allowed.`
      );
    }

    setUploadedFiles((prev) => [...prev, ...validFiles]);
  };

  // Remove uploaded file
  const removeFile = (index: number) => {
    setUploadedFiles((prev) => prev.filter((_, i) => i !== index));
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
        categoryId: data.categoryId || undefined,
        departmentId: data.departmentId || undefined,
        locationId: data.locationId || undefined,
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
            files: uploadedFiles,
          }).unwrap();
          console.log(
            `Successfully uploaded ${uploadedFiles.length} files for incident ${result.id}`
          );
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
          type: 'success',
        },
      });
    } catch (error: any) {
      // Handle API error
      if (error.data?.message) {
        setSubmitError(error.data.message);
      } else if (error.data?.errors) {
        // Handle validation errors
        const errorMessages = Object.values(error.data.errors)
          .flat()
          .join(', ');
        setSubmitError(errorMessages);
      } else {
        setSubmitError('Failed to submit incident report. Please try again.');
      }
      console.error('Submit error:', error);
    }
  };


  return (
    <PermissionGuard 
      module={ModuleType.IncidentManagement} 
      permission={PermissionType.Create}
      fallback={
        <div className="text-center p-4">
          <h3>Access Denied</h3>
          <p>You don't have permission to create incident reports.</p>
        </div>
      }
    >
      <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4
                className="mb-0"
                style={{
                  color: 'var(--harmoni-charcoal)',
                  fontFamily: 'Poppins, sans-serif',
                }}
              >
                <FontAwesomeIcon
                  icon={CONTEXT_ICONS.incident}
                  size="lg"
                  className="me-2 text-warning"
                />
                Report New Incident
              </h4>
              <small className="text-muted">
                Fill out all required information to report an incident
              </small>
            </div>
            <div className="d-flex align-items-center gap-2">
              {autoSaveStatus && (
                <CBadge
                  color={
                    autoSaveStatus === 'saved'
                      ? 'success'
                      : autoSaveStatus === 'saving'
                        ? 'info'
                        : 'danger'
                  }
                  className="d-flex align-items-center"
                >
                  {autoSaveStatus === 'saving' && (
                    <CSpinner size="sm" className="me-1" />
                  )}
                  <FontAwesomeIcon
                    icon={autoSaveStatus === 'saved' ? CONTEXT_ICONS.report : ACTION_ICONS.info}
                    size="sm"
                    className="me-1"
                  />
                  {autoSaveStatus === 'saved'
                    ? 'Auto-saved'
                    : autoSaveStatus === 'saving'
                      ? 'Saving...'
                      : 'Save failed'}
                </CBadge>
              )}
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate('/incidents')}
              >
                <FontAwesomeIcon icon={ACTION_ICONS.back} size="sm" className="me-1" />
                Back to List
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            <CForm onSubmit={handleSubmit(onSubmit)}>
              {submitError && (
                <CAlert
                  color="danger"
                  dismissible
                  onClose={() => setSubmitError(null)}
                >
                  {submitError}
                </CAlert>
              )}

              {/* Development: Show validation errors */}
              {process.env.NODE_ENV === 'development' && Object.keys(errors).length > 0 && (
                <CAlert color="warning" className="mb-4">
                  <FontAwesomeIcon icon={ACTION_ICONS.warning} className="me-2" />
                  <strong>Please fix the following errors:</strong>
                  <ul className="mb-0 mt-2">
                    {Object.entries(errors).map(([field, error]) => (
                      <li key={field}>
                        <strong>{field}:</strong> {error?.message}
                      </li>
                    ))}
                  </ul>
                </CAlert>
              )}

              <CCallout color="info" className="mb-4">
                <FontAwesomeIcon icon={ACTION_ICONS.info} className="me-2" />
                <strong>Important:</strong> Report incidents as soon as
                possible. All serious incidents must be reported within 2 hours
                according to Indonesian regulations.
              </CCallout>

              {(isDepartmentsLoading || isCategoriesLoading || isLocationsLoading) && (
                <CAlert color="info" className="mb-4">
                  <CSpinner size="sm" className="me-2" />
                  Loading configuration data...
                  {isDepartmentsLoading && ' Departments'}
                  {isCategoriesLoading && ' Categories'} 
                  {isLocationsLoading && ' Locations'}
                </CAlert>
              )}

              <DemoModeOnly>
                <CCallout color="info" className="mb-4">
                  <FontAwesomeIcon icon={ACTION_ICONS.info} className="me-2" />
                  <strong>Demo Mode:</strong> You're using sample data. Attachments are limited to {maxAttachmentSizeMB}MB in demo mode.
                </CCallout>
              </DemoModeOnly>

              {/* Basic Information Section */}
              <CAccordion>
                <CAccordionItem itemKey={1}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={CONTEXT_ICONS.basicInformation} className="me-2 text-primary" />
                      <strong>Basic Information</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={12}>
                        <CFormLabel htmlFor="title">
                          Incident Title *
                        </CFormLabel>
                        <CFormInput
                          id="title"
                          {...register('title')}
                          invalid={!!errors.title}
                          placeholder="Brief description of what happened"
                        />
                        {errors.title && (
                          <div className="invalid-feedback d-block">
                            {errors.title.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={3}>
                        <CFormLabel htmlFor="severity">
                          Severity Level *
                        </CFormLabel>
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
                          <div className="invalid-feedback d-block">
                            {errors.severity.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="categoryId">Category *</CFormLabel>
                        <CFormSelect
                          id="categoryId"
                          {...register('categoryId', { valueAsNumber: true })}
                          invalid={!!errors.categoryId}
                          disabled={isCategoriesLoading}
                        >
                          <option value="">
                            {isCategoriesLoading ? 'Loading categories...' : 'Select category...'}
                          </option>
                          {categories.map((category) => (
                            <option key={category.id} value={category.id}>
                              {category.name}
                              {category.requiresImmediateAction ? ' [URGENT]' : ''}
                            </option>
                          ))}
                        </CFormSelect>
                        {errors.categoryId && (
                          <div className="invalid-feedback d-block">
                            {errors.categoryId.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={5}>
                        <CFormLabel htmlFor="departmentId">Department</CFormLabel>
                        <CFormSelect
                          id="departmentId"
                          {...register('departmentId', { valueAsNumber: true })}
                          disabled={isDepartmentsLoading}
                        >
                          <option value="">
                            {isDepartmentsLoading ? 'Loading departments...' : 'Select department (optional)'}
                          </option>
                          {departments.map((dept) => (
                            <option key={dept.id} value={dept.id}>
                              {dept.name} ({dept.code})
                            </option>
                          ))}
                        </CFormSelect>
                        <small className="text-muted">
                          Which department should handle this incident?
                        </small>
                        {errors.departmentId && (
                          <div className="invalid-feedback d-block">
                            {errors.departmentId.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol xs={12}>
                        <CFormLabel htmlFor="description">
                          Detailed Description *
                        </CFormLabel>
                        <CFormTextarea
                          id="description"
                          rows={4}
                          {...register('description')}
                          invalid={!!errors.description}
                          placeholder="Describe what happened in detail. Include any relevant circumstances, conditions, or factors that may have contributed to the incident."
                        />
                        {errors.description && (
                          <div className="invalid-feedback d-block">
                            {errors.description.message}
                          </div>
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
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={CONTEXT_ICONS.locationTime} className="me-2 text-success" />
                      <strong>Location and Time</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="incidentDate">
                          Incident Date and Time *
                        </CFormLabel>
                        <CInputGroup>
                          <CInputGroupText>
                            <FontAwesomeIcon icon={CONTEXT_ICONS.report} />
                          </CInputGroupText>
                          <CFormInput
                            id="incidentDate"
                            type="datetime-local"
                            {...register('incidentDate')}
                            invalid={!!errors.incidentDate}
                          />
                        </CInputGroup>
                        {errors.incidentDate && (
                          <div className="invalid-feedback d-block">
                            {errors.incidentDate.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="location">Location *</CFormLabel>
                        <CInputGroup>
                          {locationInputMode === 'dropdown' ? (
                            <CFormSelect
                              id="locationId"
                              {...register('locationId', { 
                                valueAsNumber: true,
                                onChange: (e) => {
                                  const selectedLocation = locations.find(loc => loc.id === parseInt(e.target.value));
                                  if (selectedLocation) {
                                    setValue('location', selectedLocation.fullLocation);
                                    setValue('latitude', selectedLocation.latitude);
                                    setValue('longitude', selectedLocation.longitude);
                                  }
                                }
                              })}
                              invalid={!!errors.location}
                              disabled={isLocationsLoading}
                            >
                              <option value="">
                                {isLocationsLoading ? 'Loading locations...' : 'Select location...'}
                              </option>
                              {locations.map((location) => (
                                <option key={location.id} value={location.id}>
                                  {location.name}
                                  {location.building && ` - ${location.building}`}
                                  {location.isHighRisk && ' [HIGH RISK]'}
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
                                setValue('locationId', undefined);
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
                          <div className="invalid-feedback d-block">
                            {errors.location.message}
                          </div>
                        )}
                        <small className="text-muted">
                          {locationInputMode === 'dropdown'
                            ? 'Select a predefined location or click GPS button for coordinates'
                            : 'GPS coordinates will be stored. Click ↩ to use dropdown again.'}
                        </small>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Additional Details Section */}
                <CAccordionItem itemKey={3}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={CONTEXT_ICONS.additionalDetails} className="me-2 text-info" />
                      <strong>Additional Details</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="involvedPersons">
                          Involved Persons
                        </CFormLabel>
                        <CFormTextarea
                          id="involvedPersons"
                          rows={3}
                          {...register('involvedPersons')}
                          invalid={!!errors.involvedPersons}
                          placeholder="List any persons involved (witnesses, injured parties, etc.)"
                        />
                        {errors.involvedPersons && (
                          <div className="invalid-feedback d-block">
                            {errors.involvedPersons.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="immediateActions">
                          Immediate Actions Taken
                        </CFormLabel>
                        <CFormTextarea
                          id="immediateActions"
                          rows={3}
                          {...register('immediateActions')}
                          invalid={!!errors.immediateActions}
                          placeholder="Describe any immediate actions taken to address the incident"
                        />
                        {errors.immediateActions && (
                          <div className="invalid-feedback d-block">
                            {errors.immediateActions.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Evidence Upload Section */}
                <CAccordionItem itemKey={4}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={CONTEXT_ICONS.evidence} className="me-2 text-warning" />
                      <strong>Evidence (Photos/Videos)</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <CFormLabel htmlFor="files">
                        Upload Photos or Videos
                      </CFormLabel>
                      <CInputGroup>
                        <CFormInput
                          id="files"
                          type="file"
                          multiple
                          accept="image/*,video/*"
                          onChange={handleFileUpload}
                        />
                        <CInputGroupText>
                          <FontAwesomeIcon icon={FILE_TYPE_ICONS.default} />
                        </CInputGroupText>
                      </CInputGroup>
                      <small className="text-muted">
                        Maximum 5 photos, 2-minute videos. Files must be under
                        {isDemoMode ? ` ${maxAttachmentSizeMB}MB each (demo limit).` : ' 50MB each.'}
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
                            <FontAwesomeIcon icon={FILE_TYPE_ICONS.default} size="sm" className="me-1" />
                            {file.name}
                            <CButton
                              size="sm"
                              color="light"
                              className="ms-2 p-0 d-flex align-items-center justify-content-center"
                              style={{ width: '25px', height: '25px', minWidth: '25px' }}
                              onClick={() => removeFile(index)}
                              title="Remove file"
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
                    <FontAwesomeIcon icon={ACTION_ICONS.info} size="sm" className="me-1" />
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
                        <FontAwesomeIcon icon={CONTEXT_ICONS.report} size="sm" className="me-2" />
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
    </PermissionGuard>
  );
};

export default CreateIncident;
