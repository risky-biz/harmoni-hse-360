import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CForm,
  CFormInput,
  CFormLabel,
  CFormTextarea,
  CFormSelect,
  CButton,
  CAlert,
  CSpinner,
} from '@coreui/react';
import { useNavigate } from 'react-router-dom';
import { useCreateWasteReportMutation } from '../../api/wasteManagementApi';

const CreateWasteReport: React.FC = () => {
  const navigate = useNavigate();
  const [createWasteReport, { isLoading, error }] = useCreateWasteReportMutation();

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    category: '',
    generatedDate: new Date().toISOString().split('T')[0],
    location: '',
  });

  const [selectedFiles, setSelectedFiles] = useState<FileList | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  const validateForm = () => {
    const errors: Record<string, string> = {};

    if (!formData.title.trim()) {
      errors.title = 'Title is required';
    } else if (formData.title.length > 200) {
      errors.title = 'Title must not exceed 200 characters';
    }

    if (!formData.description.trim()) {
      errors.description = 'Description is required';
    } else if (formData.description.length > 2000) {
      errors.description = 'Description must not exceed 2000 characters';
    }

    if (!formData.category) {
      errors.category = 'Category is required';
    }

    if (!formData.location.trim()) {
      errors.location = 'Location is required';
    } else if (formData.location.length > 500) {
      errors.location = 'Location must not exceed 500 characters';
    }

    if (!formData.generatedDate) {
      errors.generatedDate = 'Generated date is required';
    } else if (new Date(formData.generatedDate) > new Date()) {
      errors.generatedDate = 'Generated date cannot be in the future';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear validation error when user starts typing
    if (validationErrors[field]) {
      setValidationErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSelectedFiles(e.target.files);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      const formDataToSubmit = new FormData();
      formDataToSubmit.append('title', formData.title);
      formDataToSubmit.append('description', formData.description);
      formDataToSubmit.append('category', formData.category);
      formDataToSubmit.append('generatedDate', formData.generatedDate);
      formDataToSubmit.append('location', formData.location);

      if (selectedFiles) {
        for (let i = 0; i < selectedFiles.length; i++) {
          formDataToSubmit.append('attachments', selectedFiles[i]);
        }
      }

      const result = await createWasteReport(formDataToSubmit).unwrap();
      navigate(`/waste-management/${result.id}`);
    } catch (err) {
      console.error('Failed to create waste report:', err);
    }
  };

  return (
    <CRow>
      <CCol md={8} className="mx-auto">
        <CCard>
          <CCardHeader>
            <strong>Create New Waste Report</strong>
          </CCardHeader>
          <CCardBody>
            {error && (
              <CAlert color="danger" className="mb-4">
                Failed to create waste report. Please try again.
              </CAlert>
            )}

            <CForm onSubmit={handleSubmit}>
              <CRow className="mb-3">
                <CCol md={6}>
                  <CFormLabel htmlFor="title">Title *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="title"
                    value={formData.title}
                    onChange={(e) => handleInputChange('title', e.target.value)}
                    invalid={!!validationErrors.title}
                    placeholder="Enter report title"
                  />
                  {validationErrors.title && (
                    <div className="invalid-feedback d-block">
                      {validationErrors.title}
                    </div>
                  )}
                </CCol>
                <CCol md={6}>
                  <CFormLabel htmlFor="category">Category *</CFormLabel>
                  <CFormSelect
                    id="category"
                    value={formData.category}
                    onChange={(e) => handleInputChange('category', e.target.value)}
                    invalid={!!validationErrors.category}
                  >
                    <option value="">Select category</option>
                    <option value="NonHazardous">Non-Hazardous</option>
                    <option value="HazardousChemical">Hazardous Chemical</option>
                    <option value="HazardousBiological">Hazardous Biological</option>
                    <option value="HazardousRadioactive">Hazardous Radioactive</option>
                    <option value="Recyclable">Recyclable</option>
                    <option value="Electronic">Electronic</option>
                    <option value="Construction">Construction</option>
                    <option value="Organic">Organic</option>
                    <option value="Medical">Medical</option>
                    <option value="Universal">Universal</option>
                  </CFormSelect>
                  {validationErrors.category && (
                    <div className="invalid-feedback d-block">
                      {validationErrors.category}
                    </div>
                  )}
                </CCol>
              </CRow>

              <CRow className="mb-3">
                <CCol md={6}>
                  <CFormLabel htmlFor="location">Location *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="location"
                    value={formData.location}
                    onChange={(e) => handleInputChange('location', e.target.value)}
                    invalid={!!validationErrors.location}
                    placeholder="Enter waste generation location"
                  />
                  {validationErrors.location && (
                    <div className="invalid-feedback d-block">
                      {validationErrors.location}
                    </div>
                  )}
                </CCol>
                <CCol md={6}>
                  <CFormLabel htmlFor="generatedDate">Generated Date *</CFormLabel>
                  <CFormInput
                    type="date"
                    id="generatedDate"
                    value={formData.generatedDate}
                    onChange={(e) => handleInputChange('generatedDate', e.target.value)}
                    invalid={!!validationErrors.generatedDate}
                  />
                  {validationErrors.generatedDate && (
                    <div className="invalid-feedback d-block">
                      {validationErrors.generatedDate}
                    </div>
                  )}
                </CCol>
              </CRow>

              <div className="mb-3">
                <CFormLabel htmlFor="description">Description *</CFormLabel>
                <CFormTextarea
                  id="description"
                  rows={4}
                  value={formData.description}
                  onChange={(e) => handleInputChange('description', e.target.value)}
                  invalid={!!validationErrors.description}
                  placeholder="Describe the waste, quantity, and any relevant details"
                />
                {validationErrors.description && (
                  <div className="invalid-feedback d-block">
                    {validationErrors.description}
                  </div>
                )}
                <small className="text-muted">
                  {formData.description.length}/2000 characters
                </small>
              </div>

              <div className="mb-4">
                <CFormLabel htmlFor="attachments">Attachments</CFormLabel>
                <CFormInput
                  type="file"
                  id="attachments"
                  multiple
                  onChange={handleFileChange}
                  accept=".pdf,.jpg,.jpeg,.png,.gif,.doc,.docx,.xls,.xlsx,.txt"
                />
                <small className="text-muted">
                  Accepted formats: PDF, JPG, PNG, GIF, DOC, DOCX, XLS, XLSX, TXT (Max 10MB per file)
                </small>
                {selectedFiles && selectedFiles.length > 0 && (
                  <div className="mt-2">
                    <strong>Selected files:</strong>
                    <ul className="mb-0">
                      {Array.from(selectedFiles).map((file, index) => (
                        <li key={index}>
                          {file.name} ({(file.size / 1024 / 1024).toFixed(2)} MB)
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>

              <div className="d-flex justify-content-between">
                <CButton
                  type="button"
                  color="secondary"
                  variant="outline"
                  onClick={() => navigate('/waste-management')}
                >
                  Cancel
                </CButton>
                <CButton
                  type="submit"
                  color="primary"
                  disabled={isLoading}
                >
                  {isLoading ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Creating...
                    </>
                  ) : (
                    'Create Report'
                  )}
                </CButton>
              </div>
            </CForm>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default CreateWasteReport;