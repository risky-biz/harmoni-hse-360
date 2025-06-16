import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CForm,
  CFormLabel,
  CFormInput,
  CFormTextarea,
  CFormSelect,
  CButton,
  CSpinner,
  CAlert,
  CInputGroup,
  CInputGroupText,
} from '@coreui/react';
import { Icon } from '../../components/common/Icon';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowLeft, faSave, faMapPin } from '@fortawesome/free-solid-svg-icons';
import {
  useGetIncidentQuery,
  useUpdateIncidentMutation,
} from '../../features/incidents/incidentApi';
import { useGetIncidentLocationsQuery } from '../../api/configurationApi';
import AttachmentManager from '../../components/common/AttachmentManager';
import { formatDateTime } from '../../utils/dateUtils';

interface EditIncidentFormData {
  title: string;
  description: string;
  severity: string;
  status: string;
  location: string;
  locationId?: number;
  latitude?: number;
  longitude?: number;
}

const EditIncident: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [locationLoading, setLocationLoading] = useState(false);
  const [locationInputMode, setLocationInputMode] = useState<'dropdown' | 'text'>('dropdown');

  const {
    data: incident,
    error: loadError,
    isLoading,
  } = useGetIncidentQuery(Number(id));
  const [updateIncident, { isLoading: isUpdating, error: updateError }] =
    useUpdateIncidentMutation();
  
  // Get locations from database
  const { data: locations = [], isLoading: isLocationsLoading } = useGetIncidentLocationsQuery({});

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
  } = useForm<EditIncidentFormData>();

  useEffect(() => {
    if (incident) {
      reset({
        title: incident.title,
        description: incident.description,
        severity: incident.severity,
        status: incident.status,
        location: incident.location,
        locationId: undefined, // locationId not available in IncidentDto
        latitude: incident.latitude,
        longitude: incident.longitude,
      });
      
      // If incident has coordinates, switch to text mode
      if (incident.latitude && incident.longitude) {
        setLocationInputMode('text');
      }
    }
  }, [incident, reset]);

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

  const onSubmit = async (data: EditIncidentFormData) => {
    try {
      await updateIncident({
        id: Number(id),
        incident: {
          title: data.title,
          description: data.description,
          severity: data.severity as
            | 'Minor'
            | 'Moderate'
            | 'Serious'
            | 'Critical',
          status: data.status as
            | 'Reported'
            | 'UnderInvestigation'
            | 'AwaitingAction'
            | 'Resolved'
            | 'Closed',
          location: data.location,
        },
      }).unwrap();

      navigate(`/incidents/${id}`);
    } catch (error) {
      console.error('Failed to update incident:', error);
    }
  };

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading incident...</span>
      </div>
    );
  }

  if (loadError || !incident) {
    return (
      <CAlert color="danger">
        Failed to load incident. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/incidents')}>
            <Icon icon={faArrowLeft} className="me-2" />
            Back to List
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader>
            <h4 className="mb-0">Edit Incident</h4>
          </CCardHeader>
          <CCardBody>
            {updateError && (
              <CAlert color="danger" dismissible>
                Failed to update incident. Please try again.
              </CAlert>
            )}

            <CForm onSubmit={handleSubmit(onSubmit)}>
              <CRow>
                <CCol md={8}>
                  <div className="mb-3">
                    <CFormLabel htmlFor="title">Title *</CFormLabel>
                    <CFormInput
                      id="title"
                      type="text"
                      {...register('title', { required: 'Title is required' })}
                      invalid={!!errors.title}
                    />
                    {errors.title && (
                      <div className="invalid-feedback d-block">
                        {errors.title.message}
                      </div>
                    )}
                  </div>

                  <div className="mb-3">
                    <CFormLabel htmlFor="description">Description *</CFormLabel>
                    <CFormTextarea
                      id="description"
                      rows={4}
                      {...register('description', {
                        required: 'Description is required',
                      })}
                      invalid={!!errors.description}
                    />
                    {errors.description && (
                      <div className="invalid-feedback d-block">
                        {errors.description.message}
                      </div>
                    )}
                  </div>

                  <CRow>
                    <CCol md={6}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="severity">Severity *</CFormLabel>
                        <CFormSelect
                          id="severity"
                          {...register('severity', {
                            required: 'Severity is required',
                          })}
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
                      </div>
                    </CCol>

                    <CCol md={6}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="status">Status *</CFormLabel>
                        <CFormSelect
                          id="status"
                          {...register('status', {
                            required: 'Status is required',
                          })}
                          invalid={!!errors.status}
                        >
                          <option value="Reported">Reported</option>
                          <option value="UnderInvestigation">
                            Under Investigation
                          </option>
                          <option value="AwaitingAction">
                            Awaiting Action
                          </option>
                          <option value="Resolved">Resolved</option>
                          <option value="Closed">Closed</option>
                        </CFormSelect>
                        {errors.status && (
                          <div className="invalid-feedback d-block">
                            {errors.status.message}
                          </div>
                        )}
                      </div>
                    </CCol>
                  </CRow>

                  <div className="mb-3">
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
                          {...register('location', {
                            required: 'Location is required',
                          })}
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
                  </div>
                </CCol>

                <CCol md={4}>
                  <div className="border-start ps-4">
                    <h6 className="text-muted mb-3">Incident Information</h6>
                    <div className="mb-2">
                      <strong>ID:</strong> {incident.id}
                    </div>
                    <div className="mb-2">
                      <strong>Incident Date:</strong>{' '}
                      {formatDateTime(incident.incidentDate)}
                    </div>
                    <div className="mb-2">
                      <strong>Reporter:</strong> {incident.reporterName}
                    </div>
                    <div className="mb-2">
                      <strong>Created:</strong>{' '}
                      {formatDateTime(incident.createdAt)}
                    </div>
                    {incident.lastModifiedAt && (
                      <div className="mb-2">
                        <strong>Last Modified:</strong>{' '}
                        {formatDateTime(incident.lastModifiedAt)}
                      </div>
                    )}
                  </div>
                </CCol>
              </CRow>

              <hr />

              <div className="d-flex justify-content-between">
                <CButton
                  color="light"
                  onClick={() => navigate(`/incidents/${id}`)}
                  disabled={isUpdating}
                >
                  <Icon icon={faArrowLeft} className="me-2" />
                  Cancel
                </CButton>
                <CButton color="primary" type="submit" disabled={isUpdating}>
                  {isUpdating ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Updating...
                    </>
                  ) : (
                    <>
                      <Icon icon={faSave} className="me-2" />
                      Save Changes
                    </>
                  )}
                </CButton>
              </div>
            </CForm>
          </CCardBody>
        </CCard>

        {/* Attachments Section */}
        <AttachmentManager
          incidentId={Number(id)}
          allowUpload={true}
          allowDelete={true}
        />
      </CCol>
    </CRow>
  );
};

export default EditIncident;
