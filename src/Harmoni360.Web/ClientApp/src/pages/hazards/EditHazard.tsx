import React, { useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CForm,
  CFormInput,
  CFormLabel,
  CFormTextarea,
  CFormSelect,
  CRow,
  CCol,
  CButton,
  CAlert,
  CSpinner,
} from '@coreui/react';
import { Icon } from '../../components/common/Icon';
import { faArrowLeft, faSave } from '@fortawesome/free-solid-svg-icons';
import { useGetHazardQuery, useUpdateHazardMutation } from '../../features/hazards/hazardApi';
import HazardAttachmentManager from '../../components/hazards/HazardAttachmentManager';
import MitigationActionsManager from '../../components/hazards/MitigationActionsManager';
import { 
  HAZARD_CATEGORIES, 
  HAZARD_TYPES, 
  HAZARD_SEVERITIES,
  HAZARD_STATUSES 
} from '../../types/hazard';
import { formatDateTime } from '../../utils/dateUtils';

interface EditHazardFormData {
  title: string;
  description: string;
  category: string;
  type: string;
  location: string;
  severity: string;
  status: string;
  expectedResolutionDate?: string;
  statusChangeReason?: string;
}

const EditHazard: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const hazardId = parseInt(id || '0');
  
  const { 
    data: hazard, 
    error: loadError, 
    isLoading: isLoadingHazard, 
    refetch 
  } = useGetHazardQuery({
    id: hazardId,
    includeAttachments: true,
    includeRiskAssessments: true,
    includeMitigationActions: true,
    includeReassessments: false,
  });
  
  const [updateHazard, { isLoading: isUpdating, error: updateError }] = useUpdateHazardMutation();
  
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
  } = useForm<EditHazardFormData>();

  const statusValue = watch('status');

  useEffect(() => {
    if (hazard) {
      reset({
        title: hazard.title,
        description: hazard.description,
        category: hazard.category,
        type: hazard.type,
        location: hazard.location,
        status: hazard.status,
        severity: hazard.severity,
        expectedResolutionDate: hazard.expectedResolutionDate || '',
        statusChangeReason: '',
      });
    }
  }, [hazard, reset]);

  const onSubmit = async (data: EditHazardFormData) => {
    try {
      await updateHazard({
        id: hazardId,
        title: data.title,
        description: data.description,
        category: data.category,
        type: data.type,
        location: data.location,
        status: data.status,
        severity: data.severity,
        expectedResolutionDate: data.expectedResolutionDate || undefined,
        statusChangeReason: data.statusChangeReason || undefined,
      }).unwrap();
      
      navigate(`/hazards/${hazardId}`);
    } catch (error) {
      console.error('Failed to update hazard:', error);
    }
  };

  if (isLoadingHazard) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading hazard...</span>
      </div>
    );
  }

  if (loadError || !hazard) {
    return (
      <CAlert color="danger">
        Failed to load hazard. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/hazards')}>
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
            <h4 className="mb-0">Edit Hazard</h4>
          </CCardHeader>
          <CCardBody>
            {updateError && (
              <CAlert color="danger" dismissible>
                Failed to update hazard. Please try again.
              </CAlert>
            )}

            <CForm onSubmit={handleSubmit(onSubmit)}>
              <CRow>
                <CCol md={8}>
                  <div className="mb-3">
                    <CFormLabel htmlFor="title">Hazard Title *</CFormLabel>
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
                        <CFormLabel htmlFor="category">Category *</CFormLabel>
                        <CFormSelect
                          id="category"
                          {...register('category', {
                            required: 'Category is required',
                          })}
                          invalid={!!errors.category}
                        >
                          <option value="">Select Category</option>
                          {HAZARD_CATEGORIES.map(category => (
                            <option key={category} value={category}>{category}</option>
                          ))}
                        </CFormSelect>
                        {errors.category && (
                          <div className="invalid-feedback d-block">
                            {errors.category.message}
                          </div>
                        )}
                      </div>
                    </CCol>

                    <CCol md={6}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="type">Type *</CFormLabel>
                        <CFormSelect
                          id="type"
                          {...register('type', {
                            required: 'Type is required',
                          })}
                          invalid={!!errors.type}
                        >
                          <option value="">Select Type</option>
                          {HAZARD_TYPES.map(type => (
                            <option key={type} value={type}>{type}</option>
                          ))}
                        </CFormSelect>
                        {errors.type && (
                          <div className="invalid-feedback d-block">
                            {errors.type.message}
                          </div>
                        )}
                      </div>
                    </CCol>
                  </CRow>

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
                          <option value="">Select Severity</option>
                          {HAZARD_SEVERITIES.map(severity => (
                            <option key={severity} value={severity}>{severity}</option>
                          ))}
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
                          <option value="">Select Status</option>
                          {HAZARD_STATUSES.map(status => (
                            <option key={status} value={status}>{status}</option>
                          ))}
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
                    <CFormInput
                      id="location"
                      type="text"
                      {...register('location', { required: 'Location is required' })}
                      invalid={!!errors.location}
                    />
                    {errors.location && (
                      <div className="invalid-feedback d-block">
                        {errors.location.message}
                      </div>
                    )}
                  </div>

                  <div className="mb-3">
                    <CFormLabel htmlFor="expectedResolutionDate">Expected Resolution Date</CFormLabel>
                    <CFormInput
                      type="date"
                      id="expectedResolutionDate"
                      {...register('expectedResolutionDate')}
                    />
                  </div>

                  {statusValue && statusValue !== hazard.status && (
                    <div className="mb-3">
                      <CFormLabel htmlFor="statusChangeReason">Reason for Status Change</CFormLabel>
                      <CFormTextarea
                        id="statusChangeReason"
                        rows={3}
                        {...register('statusChangeReason')}
                        placeholder="Please explain why the status is being changed..."
                      />
                    </div>
                  )}
                </CCol>

                <CCol md={4}>
                  <div className="border-start ps-4">
                    <h6 className="text-muted mb-3">Hazard Information</h6>
                    <div className="mb-2">
                      <strong>ID:</strong> {hazard.id}
                    </div>
                    <div className="mb-2">
                      <strong>Identified Date:</strong>{' '}
                      {formatDateTime(hazard.identifiedDate)}
                    </div>
                    <div className="mb-2">
                      <strong>Reporter:</strong> {hazard.reporterName}
                    </div>
                    <div className="mb-2">
                      <strong>Department:</strong> {hazard.reporterDepartment}
                    </div>
                    <div className="mb-2">
                      <strong>Created:</strong>{' '}
                      {formatDateTime(hazard.createdAt)}
                    </div>
                    {hazard.lastModifiedAt && (
                      <div className="mb-2">
                        <strong>Last Modified:</strong>{' '}
                        {formatDateTime(hazard.lastModifiedAt)}
                      </div>
                    )}
                    {hazard.currentRiskLevel && (
                      <div className="mb-2">
                        <strong>Risk Level:</strong>{' '}
                        <span className={`badge bg-${hazard.currentRiskLevel === 'High' || hazard.currentRiskLevel === 'Critical' ? 'danger' : hazard.currentRiskLevel === 'Medium' ? 'warning' : 'success'}`}>
                          {hazard.currentRiskLevel}
                        </span>
                      </div>
                    )}
                  </div>
                </CCol>
              </CRow>

              <hr />

              <div className="d-flex justify-content-between">
                <CButton
                  color="light"
                  onClick={() => navigate(`/hazards/${hazardId}`)}
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
        <HazardAttachmentManager
          hazardId={hazardId}
          allowUpload={true}
          allowDelete={true}
        />
        
        {/* Mitigation Actions Section */}
        <MitigationActionsManager
          hazardId={hazardId}
          mitigationActions={hazard.mitigationActions || []}
          allowEdit={true}
          onRefresh={refetch}
        />
      </CCol>
    </CRow>
  );
};

export default EditHazard;