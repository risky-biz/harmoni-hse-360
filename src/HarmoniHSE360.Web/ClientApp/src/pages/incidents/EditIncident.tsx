import React, { useEffect } from 'react';
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
} from '@coreui/react';
import { Icon } from '../../components/common/Icon';
import { faArrowLeft, faSave } from '@fortawesome/free-solid-svg-icons';
import { useGetIncidentQuery, useUpdateIncidentMutation } from '../../features/incidents/incidentApi';
import AttachmentManager from '../../components/common/AttachmentManager';
import { formatDateTime } from '../../utils/dateUtils';

interface EditIncidentFormData {
  title: string;
  description: string;
  severity: string;
  status: string;
  location: string;
}

const EditIncident: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: incident, error: loadError, isLoading } = useGetIncidentQuery(Number(id));
  const [updateIncident, { isLoading: isUpdating, error: updateError }] = useUpdateIncidentMutation();

  // Campus locations dropdown options
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

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<EditIncidentFormData>();

  useEffect(() => {
    if (incident) {
      reset({
        title: incident.title,
        description: incident.description,
        severity: incident.severity,
        status: incident.status,
        location: incident.location,
      });
    }
  }, [incident, reset]);

  const onSubmit = async (data: EditIncidentFormData) => {
    try {
      await updateIncident({
        id: Number(id),
        incident: {
          ...data,
          severity: data.severity as 'Minor' | 'Moderate' | 'Serious' | 'Critical',
          status: data.status as 'Reported' | 'UnderInvestigation' | 'AwaitingAction' | 'Resolved' | 'Closed',
        },
      }).unwrap();
      
      navigate(`/incidents/${id}`);
    } catch (error) {
      console.error('Failed to update incident:', error);
    }
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
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
                      {...register('description', { required: 'Description is required' })}
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
                          {...register('severity', { required: 'Severity is required' })}
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
                          {...register('status', { required: 'Status is required' })}
                          invalid={!!errors.status}
                        >
                          <option value="Reported">Reported</option>
                          <option value="UnderInvestigation">Under Investigation</option>
                          <option value="AwaitingAction">Awaiting Action</option>
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
                    <CFormSelect
                      id="location"
                      {...register('location', { required: 'Location is required' })}
                      invalid={!!errors.location}
                    >
                      <option value="">Select location...</option>
                      {campusLocations.map(location => (
                        <option key={location} value={location}>
                          {location}
                        </option>
                      ))}
                    </CFormSelect>
                    {errors.location && (
                      <div className="invalid-feedback d-block">
                        {errors.location.message}
                      </div>
                    )}
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
                <CButton
                  color="primary"
                  type="submit"
                  disabled={isUpdating}
                >
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