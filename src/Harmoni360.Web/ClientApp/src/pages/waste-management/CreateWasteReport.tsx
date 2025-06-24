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
  CAccordion,
  CAccordionItem,
  CAccordionHeader,
  CAccordionBody,
  CCallout,
  CInputGroup,
  CInputGroupText,
  CBadge,
} from '@coreui/react';
import { useNavigate } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faRecycle,
  faInfoCircle,
  faWeight,
  faIndustry,
  faFileContract,
  faClipboardCheck,
  faUpload,
  faSave,
  faArrowLeft,
  faExclamationTriangle,
  faMapMarkerAlt,
  faCalendarAlt,
  faDollarSign,
  faFileAlt,
  faBuilding
} from '@fortawesome/free-solid-svg-icons';
import { useCreateWasteReportMutation } from '../../api/wasteManagementApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';

const CreateWasteReport: React.FC = () => {
  const navigate = useNavigate();
  const [createWasteReport, { isLoading, error }] = useCreateWasteReportMutation();

  const [formData, setFormData] = useState({
    title: '',
    description: '',
    category: '',
    generatedDate: new Date().toISOString().split('T')[0],
    location: '',
    estimatedQuantity: '',
    quantityUnit: 'kg',
    disposalMethod: '',
    disposalDate: '',
    disposalCost: '',
    contractorName: '',
    manifestNumber: '',
    treatment: '',
    notes: '',
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

    if (formData.estimatedQuantity && isNaN(Number(formData.estimatedQuantity))) {
      errors.estimatedQuantity = 'Quantity must be a valid number';
    }

    if (formData.disposalCost && isNaN(Number(formData.disposalCost))) {
      errors.disposalCost = 'Cost must be a valid number';
    }

    if (formData.disposalDate && new Date(formData.disposalDate) <= new Date()) {
      errors.disposalDate = 'Disposal date must be in the future';
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
      
      if (formData.estimatedQuantity) {
        formDataToSubmit.append('estimatedQuantity', formData.estimatedQuantity);
      }
      if (formData.quantityUnit) {
        formDataToSubmit.append('quantityUnit', formData.quantityUnit);
      }
      if (formData.disposalMethod) {
        formDataToSubmit.append('disposalMethod', formData.disposalMethod);
      }
      if (formData.disposalDate) {
        formDataToSubmit.append('disposalDate', formData.disposalDate);
      }
      if (formData.disposalCost) {
        formDataToSubmit.append('disposalCost', formData.disposalCost);
      }
      if (formData.contractorName) {
        formDataToSubmit.append('contractorName', formData.contractorName);
      }
      if (formData.manifestNumber) {
        formDataToSubmit.append('manifestNumber', formData.manifestNumber);
      }
      if (formData.treatment) {
        formDataToSubmit.append('treatment', formData.treatment);
      }
      if (formData.notes) {
        formDataToSubmit.append('notes', formData.notes);
      }

      if (selectedFiles) {
        for (let i = 0; i < selectedFiles.length; i++) {
          formDataToSubmit.append('attachments', selectedFiles[i]);
        }
      }

      const result = await createWasteReport(formDataToSubmit).unwrap();
      navigate(`/waste/reports/${result.id}`);
    } catch (err) {
      console.error('Failed to create waste report:', err);
    }
  };

  const getCategoryColor = (category: string) => {
    switch (category) {
      case '1': return 'danger'; // Hazardous
      case '2': return 'success'; // Non-Hazardous
      case '3': return 'info'; // Recyclable
      default: return 'secondary';
    }
  };

  const getCategoryName = (category: string) => {
    switch (category) {
      case '1': return 'Hazardous';
      case '2': return 'Non-Hazardous';
      case '3': return 'Recyclable';
      default: return 'Select category';
    }
  };

  return (
    <PermissionGuard 
      module={ModuleType.WasteManagement} 
      permission={PermissionType.Create}
      fallback={<div>You do not have permission to create waste reports.</div>}
    >
      <CRow>
        <CCol md={10} className="mx-auto">
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div>
                <h4 className="mb-0">
                  <FontAwesomeIcon icon={faRecycle} size="lg" className="me-2 text-success" />
                  Create New Waste Report
                </h4>
                <small className="text-muted">
                  Complete all required sections to create a comprehensive waste report
                </small>
              </div>
              <div className="d-flex align-items-center gap-2">
                <CButton
                  color="secondary"
                  variant="outline"
                  onClick={() => navigate('/waste/reports')}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                  Back to List
                </CButton>
              </div>
            </CCardHeader>
            
            <CCardBody>
              {error && (
                <CAlert color="danger" className="mb-4">
                  Failed to create waste report. Please try again.
                </CAlert>
              )}

              <CCallout color="info" className="mb-4">
                <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                <strong>Important:</strong> Provide accurate waste information to ensure proper handling and compliance with environmental regulations.
              </CCallout>

              <CForm onSubmit={handleSubmit}>
                <CAccordion alwaysOpen>
                  {/* Section 1: Basic Information */}
                  <CAccordionItem itemKey={1}>
                    <CAccordionHeader>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faInfoCircle} className="me-2 text-primary" />
                        <strong>Basic Information</strong>
                        <CBadge color="danger" className="ms-2">Required</CBadge>
                      </div>
                    </CAccordionHeader>
                    <CAccordionBody>
                      <CRow className="mb-3">
                        <CCol md={8}>
                          <CFormLabel htmlFor="title">Waste Report Title *</CFormLabel>
                          <CFormInput
                            type="text"
                            id="title"
                            value={formData.title}
                            onChange={(e) => handleInputChange('title', e.target.value)}
                            invalid={!!validationErrors.title}
                            placeholder="Enter a descriptive title for the waste report"
                          />
                          {validationErrors.title && (
                            <div className="invalid-feedback d-block">
                              {validationErrors.title}
                            </div>
                          )}
                        </CCol>
                        <CCol md={4}>
                          <CFormLabel htmlFor="category">Waste Category *</CFormLabel>
                          <CFormSelect
                            id="category"
                            value={formData.category}
                            onChange={(e) => handleInputChange('category', e.target.value)}
                            invalid={!!validationErrors.category}
                          >
                            <option value="">Select category</option>
                            <option value="1">Hazardous</option>
                            <option value="2">Non-Hazardous</option>
                            <option value="3">Recyclable</option>
                          </CFormSelect>
                          {validationErrors.category && (
                            <div className="invalid-feedback d-block">
                              {validationErrors.category}
                            </div>
                          )}
                          {formData.category && (
                            <div className="mt-2">
                              <CBadge color={getCategoryColor(formData.category)}>
                                {getCategoryName(formData.category)}
                              </CBadge>
                            </div>
                          )}
                        </CCol>
                      </CRow>

                      <CRow className="mb-3">
                        <CCol md={6}>
                          <CFormLabel htmlFor="location">
                            <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                            Waste Generation Location *
                          </CFormLabel>
                          <CFormInput
                            type="text"
                            id="location"
                            value={formData.location}
                            onChange={(e) => handleInputChange('location', e.target.value)}
                            invalid={!!validationErrors.location}
                            placeholder="Building, room, or area where waste was generated"
                          />
                          {validationErrors.location && (
                            <div className="invalid-feedback d-block">
                              {validationErrors.location}
                            </div>
                          )}
                        </CCol>
                        <CCol md={6}>
                          <CFormLabel htmlFor="generatedDate">
                            <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                            Generation Date *
                          </CFormLabel>
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
                        <CFormLabel htmlFor="description">Waste Description *</CFormLabel>
                        <CFormTextarea
                          id="description"
                          rows={4}
                          value={formData.description}
                          onChange={(e) => handleInputChange('description', e.target.value)}
                          invalid={!!validationErrors.description}
                          placeholder="Describe the waste type, composition, and any relevant details"
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
                    </CAccordionBody>
                  </CAccordionItem>

                  {/* Section 2: Quantity Information */}
                  <CAccordionItem itemKey={2}>
                    <CAccordionHeader>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faWeight} className="me-2 text-warning" />
                        <strong>Quantity Information</strong>
                        <CBadge color="secondary" className="ms-2">Optional</CBadge>
                      </div>
                    </CAccordionHeader>
                    <CAccordionBody>
                      <CRow className="mb-3">
                        <CCol md={6}>
                          <CFormLabel htmlFor="estimatedQuantity">Estimated Quantity</CFormLabel>
                          <CInputGroup>
                            <CFormInput
                              type="number"
                              id="estimatedQuantity"
                              value={formData.estimatedQuantity}
                              onChange={(e) => handleInputChange('estimatedQuantity', e.target.value)}
                              invalid={!!validationErrors.estimatedQuantity}
                              placeholder="Enter estimated quantity"
                              min="0"
                              step="0.01"
                            />
                            <CInputGroupText>
                              <FontAwesomeIcon icon={faWeight} />
                            </CInputGroupText>
                          </CInputGroup>
                          {validationErrors.estimatedQuantity && (
                            <div className="invalid-feedback d-block">
                              {validationErrors.estimatedQuantity}
                            </div>
                          )}
                        </CCol>
                        <CCol md={6}>
                          <CFormLabel htmlFor="quantityUnit">Unit of Measurement</CFormLabel>
                          <CFormSelect
                            id="quantityUnit"
                            value={formData.quantityUnit}
                            onChange={(e) => handleInputChange('quantityUnit', e.target.value)}
                          >
                            <option value="kg">Kilograms (kg)</option>
                            <option value="liter">Liters (L)</option>
                            <option value="ton">Tons</option>
                            <option value="cubic_meter">Cubic Meters (m³)</option>
                            <option value="gallon">Gallons</option>
                            <option value="pound">Pounds (lbs)</option>
                            <option value="unit">Units/Pieces</option>
                            <option value="container">Containers</option>
                          </CFormSelect>
                        </CCol>
                      </CRow>
                    </CAccordionBody>
                  </CAccordionItem>

                  {/* Section 3: Disposal Planning */}
                  <CAccordionItem itemKey={3}>
                    <CAccordionHeader>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faIndustry} className="me-2 text-info" />
                        <strong>Disposal Planning</strong>
                        <CBadge color="secondary" className="ms-2">Optional</CBadge>
                      </div>
                    </CAccordionHeader>
                    <CAccordionBody>
                      <CRow className="mb-3">
                        <CCol md={6}>
                          <CFormLabel htmlFor="disposalMethod">Planned Disposal Method</CFormLabel>
                          <CFormInput
                            type="text"
                            id="disposalMethod"
                            value={formData.disposalMethod}
                            onChange={(e) => handleInputChange('disposalMethod', e.target.value)}
                            placeholder="e.g., Incineration, Landfill, Recycling, Treatment"
                          />
                        </CCol>
                        <CCol md={6}>
                          <CFormLabel htmlFor="disposalDate">Planned Disposal Date</CFormLabel>
                          <CFormInput
                            type="date"
                            id="disposalDate"
                            value={formData.disposalDate}
                            onChange={(e) => handleInputChange('disposalDate', e.target.value)}
                            invalid={!!validationErrors.disposalDate}
                          />
                          {validationErrors.disposalDate && (
                            <div className="invalid-feedback d-block">
                              {validationErrors.disposalDate}
                            </div>
                          )}
                        </CCol>
                      </CRow>

                      <CRow className="mb-3">
                        <CCol md={6}>
                          <CFormLabel htmlFor="disposalCost">
                            <FontAwesomeIcon icon={faDollarSign} className="me-1" />
                            Estimated Disposal Cost
                          </CFormLabel>
                          <CInputGroup>
                            <CInputGroupText>$</CInputGroupText>
                            <CFormInput
                              type="number"
                              id="disposalCost"
                              value={formData.disposalCost}
                              onChange={(e) => handleInputChange('disposalCost', e.target.value)}
                              invalid={!!validationErrors.disposalCost}
                              placeholder="Enter estimated cost"
                              min="0"
                              step="0.01"
                            />
                          </CInputGroup>
                          {validationErrors.disposalCost && (
                            <div className="invalid-feedback d-block">
                              {validationErrors.disposalCost}
                            </div>
                          )}
                        </CCol>
                        <CCol md={6}>
                          <CFormLabel htmlFor="contractorName">
                            <FontAwesomeIcon icon={faBuilding} className="me-1" />
                            Disposal Contractor
                          </CFormLabel>
                          <CFormInput
                            type="text"
                            id="contractorName"
                            value={formData.contractorName}
                            onChange={(e) => handleInputChange('contractorName', e.target.value)}
                            placeholder="Name of disposal contractor/company"
                          />
                        </CCol>
                      </CRow>
                    </CAccordionBody>
                  </CAccordionItem>

                  {/* Section 4: Compliance & Documentation */}
                  <CAccordionItem itemKey={4}>
                    <CAccordionHeader>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faFileContract} className="me-2 text-success" />
                        <strong>Compliance & Documentation</strong>
                        <CBadge color="secondary" className="ms-2">Optional</CBadge>
                      </div>
                    </CAccordionHeader>
                    <CAccordionBody>
                      <CRow className="mb-3">
                        <CCol md={6}>
                          <CFormLabel htmlFor="manifestNumber">Waste Manifest Number</CFormLabel>
                          <CFormInput
                            type="text"
                            id="manifestNumber"
                            value={formData.manifestNumber}
                            onChange={(e) => handleInputChange('manifestNumber', e.target.value)}
                            placeholder="Tracking manifest number"
                          />
                          <small className="text-muted">Enter if manifest is already assigned</small>
                        </CCol>
                        <CCol md={6}>
                          <CFormLabel htmlFor="treatment">Required Treatment Method</CFormLabel>
                          <CFormInput
                            type="text"
                            id="treatment"
                            value={formData.treatment}
                            onChange={(e) => handleInputChange('treatment', e.target.value)}
                            placeholder="Special treatment or handling requirements"
                          />
                        </CCol>
                      </CRow>

                      <div className="mb-3">
                        <CFormLabel htmlFor="notes">Additional Notes</CFormLabel>
                        <CFormTextarea
                          id="notes"
                          rows={3}
                          value={formData.notes}
                          onChange={(e) => handleInputChange('notes', e.target.value)}
                          placeholder="Any additional information, special handling instructions, regulatory considerations, etc."
                        />
                        <small className="text-muted">
                          {formData.notes.length}/1000 characters
                        </small>
                      </div>
                    </CAccordionBody>
                  </CAccordionItem>

                  {/* Section 5: Attachments */}
                  <CAccordionItem itemKey={5}>
                    <CAccordionHeader>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faUpload} className="me-2 text-secondary" />
                        <strong>Attachments</strong>
                        <CBadge color="secondary" className="ms-2">Optional</CBadge>
                      </div>
                    </CAccordionHeader>
                    <CAccordionBody>
                      <div className="mb-4">
                        <CFormLabel htmlFor="attachments">
                          <FontAwesomeIcon icon={faFileAlt} className="me-1" />
                          Upload Supporting Documents
                        </CFormLabel>
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
                          <div className="mt-3">
                            <strong>Selected files:</strong>
                            <ul className="mb-0 mt-2">
                              {Array.from(selectedFiles).map((file, index) => (
                                <li key={index}>
                                  <FontAwesomeIcon icon={faFileAlt} className="me-2 text-muted" />
                                  {file.name} ({(file.size / 1024 / 1024).toFixed(2)} MB)
                                </li>
                              ))}
                            </ul>
                          </div>
                        )}
                      </div>
                    </CAccordionBody>
                  </CAccordionItem>
                </CAccordion>

                {/* Submit Section */}
                <div className="d-flex justify-content-between align-items-center mt-4 pt-3 border-top">
                  <div>
                    <small className="text-muted">
                      <FontAwesomeIcon icon={faInfoCircle} className="me-1" />
                      All required fields must be completed before submission
                    </small>
                  </div>
                  <div className="d-flex gap-2">
                    <CButton
                      type="button"
                      color="secondary"
                      variant="outline"
                      onClick={() => navigate('/waste/reports')}
                    >
                      Cancel
                    </CButton>
                    <CButton
                      type="submit"
                      color="success"
                      disabled={isLoading}
                    >
                      {isLoading ? (
                        <>
                          <CSpinner size="sm" className="me-2" />
                          Creating Report...
                        </>
                      ) : (
                        <>
                          <FontAwesomeIcon icon={faSave} className="me-2" />
                          Create Waste Report
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

export default CreateWasteReport;