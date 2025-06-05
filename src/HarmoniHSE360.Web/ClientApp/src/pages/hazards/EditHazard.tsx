import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
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
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowLeft } from '@fortawesome/free-solid-svg-icons';
import { useGetHazardQuery, useUpdateHazardMutation } from '../../features/hazards/hazardApi';
import { 
  HAZARD_CATEGORIES, 
  HAZARD_TYPES, 
  HAZARD_SEVERITIES,
  HAZARD_STATUSES,
  UpdateHazardRequest 
} from '../../types/hazard';

const EditHazard: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const hazardId = parseInt(id || '0');
  
  const { data: hazard, isLoading: isLoadingHazard } = useGetHazardQuery({
    id: hazardId,
    includeAttachments: false,
    includeRiskAssessments: false,
    includeMitigationActions: false,
    includeReassessments: false,
  });
  
  const [updateHazard, { isLoading: isUpdating, error }] = useUpdateHazardMutation();
  
  const [formData, setFormData] = useState<Partial<UpdateHazardRequest>>({
    id: hazardId,
    title: '',
    description: '',
    category: '',
    type: '',
    location: '',
    status: '',
    severity: '',
    expectedResolutionDate: '',
    statusChangeReason: '',
  });

  const [newAttachments, setNewAttachments] = useState<File[]>([]);

  useEffect(() => {
    if (hazard) {
      setFormData({
        id: hazard.id,
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
  }, [hazard]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const updateData: UpdateHazardRequest = {
        ...formData as UpdateHazardRequest,
        newAttachments: newAttachments.length > 0 ? newAttachments : undefined,
      };
      
      await updateHazard(updateData).unwrap();
      navigate(`/hazards/${hazardId}`);
    } catch (err) {
      console.error('Failed to update hazard:', err);
    }
  };

  const handleInputChange = (field: keyof UpdateHazardRequest, value: string | number) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setNewAttachments(Array.from(e.target.files));
    }
  };

  if (isLoadingHazard) {
    return (
      <div className="text-center py-4">
        <CSpinner />
      </div>
    );
  }

  if (!hazard) {
    return (
      <CAlert color="danger">
        Hazard not found.
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <div className="mb-3">
          <CButton 
            color="secondary" 
            variant="outline"
            onClick={() => navigate(`/hazards/${hazardId}`)}
          >
            <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
            Back to Hazard Detail
          </CButton>
        </div>

        <CCard className="mb-4">
          <CCardHeader>
            <strong>Edit Hazard: {hazard.title}</strong>
          </CCardHeader>
          <CCardBody>
            {error && (
              <CAlert color="danger" className="mb-3">
                Failed to update hazard. Please try again.
              </CAlert>
            )}
            
            <CForm onSubmit={handleSubmit}>
              <CRow className="mb-3">
                <CCol md={6}>
                  <CFormLabel htmlFor="title">Hazard Title *</CFormLabel>
                  <CFormInput
                    id="title"
                    value={formData.title}
                    onChange={(e) => handleInputChange('title', e.target.value)}
                    required
                  />
                </CCol>
                <CCol md={6}>
                  <CFormLabel htmlFor="location">Location *</CFormLabel>
                  <CFormInput
                    id="location"
                    value={formData.location}
                    onChange={(e) => handleInputChange('location', e.target.value)}
                    required
                  />
                </CCol>
              </CRow>

              <CRow className="mb-3">
                <CCol md={3}>
                  <CFormLabel htmlFor="category">Category *</CFormLabel>
                  <CFormSelect
                    id="category"
                    value={formData.category}
                    onChange={(e) => handleInputChange('category', e.target.value)}
                    required
                  >
                    <option value="">Select Category</option>
                    {HAZARD_CATEGORIES.map(category => (
                      <option key={category} value={category}>{category}</option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={3}>
                  <CFormLabel htmlFor="type">Type *</CFormLabel>
                  <CFormSelect
                    id="type"
                    value={formData.type}
                    onChange={(e) => handleInputChange('type', e.target.value)}
                    required
                  >
                    <option value="">Select Type</option>
                    {HAZARD_TYPES.map(type => (
                      <option key={type} value={type}>{type}</option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={3}>
                  <CFormLabel htmlFor="severity">Severity *</CFormLabel>
                  <CFormSelect
                    id="severity"
                    value={formData.severity}
                    onChange={(e) => handleInputChange('severity', e.target.value)}
                    required
                  >
                    <option value="">Select Severity</option>
                    {HAZARD_SEVERITIES.map(severity => (
                      <option key={severity} value={severity}>{severity}</option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={3}>
                  <CFormLabel htmlFor="status">Status *</CFormLabel>
                  <CFormSelect
                    id="status"
                    value={formData.status}
                    onChange={(e) => handleInputChange('status', e.target.value)}
                    required
                  >
                    <option value="">Select Status</option>
                    {HAZARD_STATUSES.map(status => (
                      <option key={status} value={status}>{status}</option>
                    ))}
                  </CFormSelect>
                </CCol>
              </CRow>

              <div className="mb-3">
                <CFormLabel htmlFor="description">Description *</CFormLabel>
                <CFormTextarea
                  id="description"
                  rows={4}
                  value={formData.description}
                  onChange={(e) => handleInputChange('description', e.target.value)}
                  required
                />
              </div>

              <CRow className="mb-3">
                <CCol md={6}>
                  <CFormLabel htmlFor="expectedResolutionDate">Expected Resolution Date</CFormLabel>
                  <CFormInput
                    type="date"
                    id="expectedResolutionDate"
                    value={formData.expectedResolutionDate}
                    onChange={(e) => handleInputChange('expectedResolutionDate', e.target.value)}
                  />
                </CCol>
                <CCol md={6}>
                  <CFormLabel htmlFor="newAttachments">Add New Attachments</CFormLabel>
                  <CFormInput
                    type="file"
                    id="newAttachments"
                    multiple
                    accept="image/*,.pdf,.doc,.docx"
                    onChange={handleFileChange}
                  />
                </CCol>
              </CRow>

              {formData.status !== hazard.status && (
                <div className="mb-3">
                  <CFormLabel htmlFor="statusChangeReason">Reason for Status Change</CFormLabel>
                  <CFormTextarea
                    id="statusChangeReason"
                    rows={3}
                    value={formData.statusChangeReason}
                    onChange={(e) => handleInputChange('statusChangeReason', e.target.value)}
                    placeholder="Please explain why the status is being changed..."
                  />
                </div>
              )}

              <div className="d-flex gap-2">
                <CButton 
                  type="submit" 
                  color="primary" 
                  disabled={isUpdating}
                >
                  {isUpdating && <CSpinner size="sm" className="me-2" />}
                  Update Hazard
                </CButton>
                <CButton 
                  type="button" 
                  color="secondary" 
                  onClick={() => navigate(`/hazards/${hazardId}`)}
                >
                  Cancel
                </CButton>
              </div>
            </CForm>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default EditHazard;