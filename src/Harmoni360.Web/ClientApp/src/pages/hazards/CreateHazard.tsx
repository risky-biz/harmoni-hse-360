import React, { useState } from 'react';
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
import { useNavigate } from 'react-router-dom';
import { useCreateHazardMutation } from '../../features/hazards/hazardApi';
import { useAuth } from '../../hooks/useAuth';
import { 
  HAZARD_CATEGORIES, 
  HAZARD_TYPES, 
  HAZARD_SEVERITIES,
  CreateHazardRequest 
} from '../../types/hazard';

const CreateHazard: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [createHazard, { isLoading, error }] = useCreateHazardMutation();
  
  const [formData, setFormData] = useState<Partial<CreateHazardRequest>>({
    title: '',
    description: '',
    category: '',
    type: '',
    location: '',
    severity: '',
    reporterId: user?.id || 0,
    reporterDepartment: user?.department || '',
    expectedResolutionDate: '',
  });

  const [attachments, setAttachments] = useState<File[]>([]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const hazardData: CreateHazardRequest = {
        ...formData as CreateHazardRequest,
        attachments: attachments.length > 0 ? attachments : undefined,
      };
      
      const result = await createHazard(hazardData).unwrap();
      navigate(`/hazards/${result.id}`);
    } catch (err) {
      console.error('Failed to create hazard:', err);
    }
  };

  const handleInputChange = (field: keyof CreateHazardRequest, value: string | number) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setAttachments(Array.from(e.target.files));
    }
  };

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="mb-4">
          <CCardHeader>
            <strong>Report New Hazard</strong>
          </CCardHeader>
          <CCardBody>
            {error && (
              <CAlert color="danger" className="mb-3">
                Failed to create hazard. Please try again.
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
                <CCol md={4}>
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
                <CCol md={4}>
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
                <CCol md={4}>
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
                  <CFormLabel htmlFor="attachments">Attachments</CFormLabel>
                  <CFormInput
                    type="file"
                    id="attachments"
                    multiple
                    accept="image/*,.pdf,.doc,.docx"
                    onChange={handleFileChange}
                  />
                </CCol>
              </CRow>

              <div className="d-flex gap-2">
                <CButton 
                  type="submit" 
                  color="primary" 
                  disabled={isLoading}
                >
                  {isLoading && <CSpinner size="sm" className="me-2" />}
                  Submit Hazard Report
                </CButton>
                <CButton 
                  type="button" 
                  color="secondary" 
                  onClick={() => navigate('/hazards')}
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

export default CreateHazard;