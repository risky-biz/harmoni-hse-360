import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CForm,
  CFormInput,
  CFormLabel,
  CFormSelect,
  CFormTextarea,
  CFormCheck,
  CButton,
  CAlert,
  CSpinner,
  CInputGroup,
  CInputGroupText,
  CAccordion,
  CAccordionItem,
  CAccordionHeader,
  CAccordionBody,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CBadge,
  CBreadcrumb,
  CBreadcrumbItem,
  CCallout,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSave,
  faArrowLeft,
  faInfoCircle,
  faUsers,
  faShieldAlt,
  faCertificate,
  faExclamationTriangle,
  faPaperclip,
  faClipboardCheck,
  faPlus,
  faTrash,
  faCheck,
  faHome,
  faEdit,
  faTasks,
  faCalendarAlt,
  faMapMarkerAlt,
  faBuilding,
  faUser,
  faClock,
  faFlag,
  faTag,
  faFileAlt,
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetAuditByIdQuery,
  useUpdateAuditMutation 
} from '../../features/audits/auditApi';
import {
  AuditType,
  AuditCategory,
  AuditPriority,
  UpdateAuditRequest,
  AuditItemDto,
} from '../../types/audit';
import { formatDateTime } from '../../utils/dateUtils';
import { format } from 'date-fns';
import { 
  getAuditStatusBadge,
  canEditAudit,
} from '../../utils/auditUtils';

// Audit Icon Mappings
const AUDIT_ICONS = {
  audit: faClipboardCheck,
  basicInformation: faInfoCircle,
  auditDetails: faFileAlt,
  checklistItems: faTasks,
  standards: faCertificate,
  attachments: faPaperclip,
  create: faPlus,
  save: faSave,
  back: faArrowLeft,
  check: faCheck,
};

// Form data interface for audit item creation
interface AuditItemFormData {
  description: string;
  type: string;
  isRequired: boolean;
  category: string;
  expectedResult: string;
  maxPoints: number;
}

// Form data interface with all audit fields
interface EditAuditFormData {
  title: string;
  description: string;
  type: AuditType;
  category: AuditCategory;
  priority: AuditPriority;
  scheduledDate: string;
  estimatedDurationMinutes: number;
  location: string;
  departmentId: number | null;
  facilityId: number | null;
  standardsApplied: string;
  isRegulatory: boolean;
  regulatoryReference: string;
  items: AuditItemDto[];
}

const EditAudit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  // State for managing audit items
  const [currentItem, setCurrentItem] = useState<AuditItemFormData>({
    description: '',
    type: 'YesNo',
    isRequired: true,
    category: 'Routine',
    expectedResult: '',
    maxPoints: 10,
  });

  // API calls
  const { 
    data: audit, 
    error: loadError, 
    isLoading 
  } = useGetAuditByIdQuery(parseInt(id!));
  
  const [updateAudit, { isLoading: isUpdating, error: updateError }] = useUpdateAuditMutation();

  // Form management
  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors, isDirty },
    reset
  } = useForm<EditAuditFormData>();

  const watchedItems = watch('items') || [];

  // Load existing audit data into form
  useEffect(() => {
    if (audit) {
      const formData: EditAuditFormData = {
        title: audit.title || '',
        description: audit.description || '',
        type: audit.type,
        category: audit.category,
        priority: audit.priority,
        scheduledDate: audit.scheduledDate ? format(new Date(audit.scheduledDate), 'yyyy-MM-dd\'T\'HH:mm') : '',
        estimatedDurationMinutes: audit.estimatedDurationMinutes || 120,
        location: audit.locationName || '',
        departmentId: audit.departmentId || null,
        facilityId: audit.facilityId || null,
        standardsApplied: audit.standardsApplied || '',
        isRegulatory: audit.isRegulatory || false,
        regulatoryReference: audit.regulatoryReference || '',
        items: audit.items || [],
      };
      
      reset(formData);
    }
  }, [audit, reset]);

  // Handle form submission
  const onSubmit = async (data: EditAuditFormData) => {
    try {
      const updateRequest: UpdateAuditRequest = {
        id: parseInt(id!),
        ...data,
        departmentId: data.departmentId || undefined,
        facilityId: data.facilityId || undefined,
        riskLevel: audit?.riskLevel || 'Medium', // Keep existing risk level
        locationId: audit?.locationId || undefined, // Use existing locationId
      };

      await updateAudit({ id: parseInt(id!), audit: updateRequest }).unwrap();
      navigate(`/audits/${id}`, { state: { message: 'Audit updated successfully!' } });
    } catch (error) {
      console.error('Failed to update audit:', error);
    }
  };

  // Handle adding new audit item
  const handleAddItem = useCallback(() => {
    if (!currentItem.description) {
      alert('Please enter an item description');
      return;
    }

    const newItem: AuditItemDto = {
      id: Date.now(), // Temporary ID for new items
      auditId: parseInt(id!),
      checklistItemNumber: `AI-${Date.now()}`,
      checklistItemText: currentItem.description,
      status: 'NotStarted',
      isMandatory: currentItem.isRequired,
      category: (currentItem.category || 'Routine') as AuditCategory,
      maxScore: currentItem.maxPoints,
      weight: 1,
      priority: 'Medium' as AuditPriority,
      isApplicable: true,
      requiresEvidence: false,
      isCompliant: false,
      isNonCompliant: false,
      isNotApplicable: false,
      needsAttention: false,
      scorePercentage: 0,
      requiredEvidence: currentItem.expectedResult || undefined,
      section: undefined,
      subsection: undefined,
      result: undefined,
      score: undefined,
      assessedDate: undefined,
      assessedBy: undefined,
      assessedByName: undefined,
      comments: undefined,
      evidenceNotes: undefined,
      complianceNotes: undefined,
      correctiveActions: undefined,
    };

    const updatedItems = [...watchedItems, newItem];
    setValue('items', updatedItems, { shouldDirty: true });

    // Reset current item form
    setCurrentItem({
      description: '',
      type: 'YesNo',
      isRequired: true,
      category: 'Routine',
      expectedResult: '',
      maxPoints: 10,
    });
  }, [currentItem, watchedItems, setValue, id]);

  // Handle removing audit item
  const handleRemoveItem = useCallback((index: number) => {
    const updatedItems = watchedItems.filter((_, i) => i !== index);
    setValue('items', updatedItems, { shouldDirty: true });
  }, [watchedItems, setValue]);

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner color="primary" />
      </div>
    );
  }

  if (loadError) {
    return (
      <CAlert color="danger">
        <h4>Error Loading Audit</h4>
        <p>Unable to load audit for editing. Please try again later.</p>
        <hr />
        <CButton
          color="danger"
          variant="outline"
          onClick={() => navigate('/audits')}
        >
          <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
          Back to Audits
        </CButton>
      </CAlert>
    );
  }

  if (!audit) {
    return (
      <CAlert color="warning">
        <h4>Audit Not Found</h4>
        <p>The requested audit could not be found.</p>
        <hr />
        <CButton
          color="warning"
          variant="outline"
          onClick={() => navigate('/audits')}
        >
          <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
          Back to Audits
        </CButton>
      </CAlert>
    );
  }

  if (!canEditAudit(audit)) {
    return (
      <CAlert color="warning">
        <h4>Cannot Edit Audit</h4>
        <p>This audit cannot be edited in its current status: {getAuditStatusBadge(audit.status)}</p>
        <p>Only audits in 'Draft' or 'Scheduled' status can be edited.</p>
        <hr />
        <div className="d-flex gap-2">
          <CButton
            color="warning"
            variant="outline"
            onClick={() => navigate('/audits')}
          >
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            Back to Audits
          </CButton>
          <CButton
            color="primary"
            onClick={() => navigate(`/audits/${id}`)}
          >
            View Audit Details
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <div className="container-fluid">
      {/* Breadcrumb */}
      <CBreadcrumb className="mb-4">
        <CBreadcrumbItem>
          <FontAwesomeIcon icon={faHome} className="me-1" />
          Dashboard
        </CBreadcrumbItem>
        <CBreadcrumbItem href="/audits">Audits</CBreadcrumbItem>
        <CBreadcrumbItem href={`/audits/${id}`}>Audit Details</CBreadcrumbItem>
        <CBreadcrumbItem active>Edit Audit</CBreadcrumbItem>
      </CBreadcrumb>

      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h1 className="h3 mb-2">
                <FontAwesomeIcon icon={AUDIT_ICONS.audit} className="me-2" />
                Edit Audit: {audit.title}
              </h1>
              <div className="d-flex align-items-center gap-3 text-muted">
                <span>Audit #: {audit.auditNumber}</span>
                <span>Status: {getAuditStatusBadge(audit.status)}</span>
                <span>Created: {formatDateTime(audit.createdAt)}</span>
              </div>
            </div>
            <div className="d-flex gap-2">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate(`/audits/${id}`)}
              >
                <FontAwesomeIcon icon={AUDIT_ICONS.back} className="me-2" />
                Cancel
              </CButton>
            </div>
          </div>
        </CCol>
      </CRow>

      {/* Form Errors */}
      {updateError && (
        <CAlert color="danger" className="mb-4">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
          Failed to update audit. Please check your inputs and try again.
        </CAlert>
      )}

      {/* Unsaved Changes Warning */}
      {isDirty && (
        <CCallout color="warning" className="mb-4">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
          You have unsaved changes. Don't forget to save your work!
        </CCallout>
      )}

      {/* Edit Form */}
      <CForm onSubmit={handleSubmit(onSubmit)}>
        <CAccordion alwaysOpen>
          {/* Basic Information */}
          <CAccordionItem itemKey="basic-info">
            <CAccordionHeader>
              <FontAwesomeIcon icon={AUDIT_ICONS.basicInformation} className="me-2" />
              Basic Information
            </CAccordionHeader>
            <CAccordionBody>
              <CRow className="g-3">
                <CCol md={8}>
                  <CFormLabel htmlFor="title">Audit Title *</CFormLabel>
                  <CFormInput
                    id="title"
                    type="text"
                    invalid={!!errors.title}
                    {...register('title', { 
                      required: 'Audit title is required',
                      maxLength: { value: 200, message: 'Title must be less than 200 characters' }
                    })}
                  />
                  {errors.title && (
                    <div className="invalid-feedback d-block">{errors.title.message}</div>
                  )}
                </CCol>

                <CCol md={4}>
                  <CFormLabel htmlFor="type">Audit Type *</CFormLabel>
                  <CFormSelect
                    id="type"
                    invalid={!!errors.type}
                    {...register('type', { required: 'Audit type is required' })}
                  >
                    <option value="">Select Type...</option>
                    <option value="Safety">Safety</option>
                    <option value="Environmental">Environmental</option>
                    <option value="Equipment">Equipment</option>
                    <option value="Compliance">Compliance</option>
                    <option value="Fire">Fire</option>
                    <option value="Chemical">Chemical</option>
                    <option value="Ergonomic">Ergonomic</option>
                    <option value="Emergency">Emergency</option>
                    <option value="Management">Management</option>
                    <option value="Process">Process</option>
                  </CFormSelect>
                  {errors.type && (
                    <div className="invalid-feedback d-block">{errors.type.message}</div>
                  )}
                </CCol>

                <CCol md={12}>
                  <CFormLabel htmlFor="description">Description</CFormLabel>
                  <CFormTextarea
                    id="description"
                    rows={3}
                    placeholder="Describe the audit scope, objectives, and key areas to be assessed..."
                    {...register('description')}
                  />
                </CCol>

                <CCol md={4}>
                  <CFormLabel htmlFor="category">Category</CFormLabel>
                  <CFormSelect
                    id="category"
                    {...register('category')}
                  >
                    <option value="">Select Category...</option>
                    <option value="Routine">Routine</option>
                    <option value="Planned">Planned</option>
                    <option value="Unplanned">Unplanned</option>
                    <option value="Regulatory">Regulatory</option>
                    <option value="Internal">Internal</option>
                    <option value="External">External</option>
                    <option value="Incident">Incident</option>
                    <option value="Maintenance">Maintenance</option>
                  </CFormSelect>
                </CCol>

                <CCol md={4}>
                  <CFormLabel htmlFor="priority">Priority</CFormLabel>
                  <CFormSelect
                    id="priority"
                    {...register('priority')}
                  >
                    <option value="">Select Priority...</option>
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Critical">Critical</option>
                  </CFormSelect>
                </CCol>

                <CCol md={4}>
                  <CFormLabel htmlFor="scheduledDate">Scheduled Date</CFormLabel>
                  <CFormInput
                    id="scheduledDate"
                    type="datetime-local"
                    {...register('scheduledDate')}
                  />
                </CCol>
              </CRow>
            </CAccordionBody>
          </CAccordionItem>

          {/* Audit Details */}
          <CAccordionItem itemKey="audit-details">
            <CAccordionHeader>
              <FontAwesomeIcon icon={AUDIT_ICONS.auditDetails} className="me-2" />
              Audit Details
            </CAccordionHeader>
            <CAccordionBody>
              <CRow className="g-3">
                <CCol md={6}>
                  <CFormLabel htmlFor="location">Location</CFormLabel>
                  <CInputGroup>
                    <CInputGroupText>
                      <FontAwesomeIcon icon={faMapMarkerAlt} />
                    </CInputGroupText>
                    <CFormInput
                      id="location"
                      type="text"
                      placeholder="e.g., Building A - Production Floor"
                      {...register('location')}
                    />
                  </CInputGroup>
                </CCol>

                <CCol md={6}>
                  <CFormLabel htmlFor="estimatedDurationMinutes">Estimated Duration (minutes)</CFormLabel>
                  <CInputGroup>
                    <CInputGroupText>
                      <FontAwesomeIcon icon={faClock} />
                    </CInputGroupText>
                    <CFormInput
                      id="estimatedDurationMinutes"
                      type="number"
                      min="30"
                      max="1440"
                      placeholder="120"
                      {...register('estimatedDurationMinutes', {
                        valueAsNumber: true,
                        min: { value: 30, message: 'Duration must be at least 30 minutes' },
                        max: { value: 1440, message: 'Duration cannot exceed 24 hours' }
                      })}
                    />
                  </CInputGroup>
                  {errors.estimatedDurationMinutes && (
                    <div className="invalid-feedback d-block">{errors.estimatedDurationMinutes.message}</div>
                  )}
                </CCol>

                <CCol md={12}>
                  <CFormLabel htmlFor="standardsApplied">Standards Applied</CFormLabel>
                  <CFormTextarea
                    id="standardsApplied"
                    rows={2}
                    placeholder="e.g., ISO 45001:2018, OSHA 29 CFR 1910, Local safety regulations..."
                    {...register('standardsApplied')}
                  />
                </CCol>

                <CCol md={6}>
                  <CFormCheck
                    id="isRegulatory"
                    label="Regulatory Audit"
                    {...register('isRegulatory')}
                  />
                </CCol>

                <CCol md={6}>
                  <CFormLabel htmlFor="regulatoryReference">Regulatory Reference</CFormLabel>
                  <CFormInput
                    id="regulatoryReference"
                    type="text"
                    placeholder="e.g., OSHA 1926.95, EPA 40 CFR"
                    {...register('regulatoryReference')}
                  />
                </CCol>
              </CRow>
            </CAccordionBody>
          </CAccordionItem>

          {/* Checklist Items */}
          <CAccordionItem itemKey="checklist-items">
            <CAccordionHeader>
              <FontAwesomeIcon icon={AUDIT_ICONS.checklistItems} className="me-2" />
              Checklist Items ({watchedItems.length})
            </CAccordionHeader>
            <CAccordionBody>
              {/* Add New Item Form */}
              <CCard className="mb-4">
                <CCardHeader>
                  <FontAwesomeIcon icon={faPlus} className="me-2" />
                  Add Checklist Item
                </CCardHeader>
                <CCardBody>
                  <CRow className="g-3">
                    <CCol md={8}>
                      <CFormLabel htmlFor="itemDescription">Item Description</CFormLabel>
                      <CFormInput
                        id="itemDescription"
                        type="text"
                        placeholder="e.g., Verify all emergency exits are clearly marked and unobstructed"
                        value={currentItem.description}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, description: e.target.value }))}
                      />
                    </CCol>

                    <CCol md={4}>
                      <CFormLabel htmlFor="itemType">Item Type</CFormLabel>
                      <CFormSelect
                        id="itemType"
                        value={currentItem.type}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, type: e.target.value }))}
                      >
                        <option value="YesNo">Yes/No</option>
                        <option value="Text">Text</option>
                        <option value="Number">Number</option>
                        <option value="MultipleChoice">Multiple Choice</option>
                        <option value="Checklist">Checklist</option>
                        <option value="Photo">Photo</option>
                        <option value="Measurement">Measurement</option>
                        <option value="Rating">Rating</option>
                      </CFormSelect>
                    </CCol>

                    <CCol md={4}>
                      <CFormLabel htmlFor="itemCategory">Category</CFormLabel>
                      <CFormInput
                        id="itemCategory"
                        type="text"
                        placeholder="e.g., Safety, Equipment, Procedures"
                        value={currentItem.category}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, category: e.target.value }))}
                      />
                    </CCol>

                    <CCol md={4}>
                      <CFormLabel htmlFor="expectedResult">Expected Result</CFormLabel>
                      <CFormInput
                        id="expectedResult"
                        type="text"
                        placeholder="e.g., Compliant, 100%, Pass"
                        value={currentItem.expectedResult}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, expectedResult: e.target.value }))}
                      />
                    </CCol>

                    <CCol md={2}>
                      <CFormLabel htmlFor="maxPoints">Max Points</CFormLabel>
                      <CFormInput
                        id="maxPoints"
                        type="number"
                        min="1"
                        max="100"
                        value={currentItem.maxPoints}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, maxPoints: parseInt(e.target.value) || 10 }))}
                      />
                    </CCol>

                    <CCol md={2}>
                      <div className="d-flex align-items-end h-100">
                        <CFormCheck
                          id="isRequired"
                          label="Required"
                          checked={currentItem.isRequired}
                          onChange={(e) => setCurrentItem(prev => ({ ...prev, isRequired: e.target.checked }))}
                        />
                      </div>
                    </CCol>

                    <CCol md={12}>
                      <CButton
                        color="primary"
                        onClick={handleAddItem}
                        disabled={!currentItem.description}
                      >
                        <FontAwesomeIcon icon={faPlus} className="me-2" />
                        Add Item
                      </CButton>
                    </CCol>
                  </CRow>
                </CCardBody>
              </CCard>

              {/* Existing Items List */}
              {watchedItems.length > 0 ? (
                <CTable responsive striped hover>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell>#</CTableHeaderCell>
                      <CTableHeaderCell>Description</CTableHeaderCell>
                      <CTableHeaderCell>Type</CTableHeaderCell>
                      <CTableHeaderCell>Category</CTableHeaderCell>
                      <CTableHeaderCell>Required</CTableHeaderCell>
                      <CTableHeaderCell>Points</CTableHeaderCell>
                      <CTableHeaderCell>Actions</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {watchedItems.map((item, index) => (
                      <CTableRow key={item.id || index}>
                        <CTableDataCell>{index + 1}</CTableDataCell>
                        <CTableDataCell>{item.checklistItemText}</CTableDataCell>
                        <CTableDataCell>
                          <CBadge color="info">Checklist</CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          {item.category && (
                            <CBadge color="secondary">{item.category}</CBadge>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={item.isMandatory ? 'danger' : 'secondary'}>
                            {item.isMandatory ? 'Required' : 'Optional'}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>{item.maxScore || '-'}</CTableDataCell>
                        <CTableDataCell>
                          <CButton
                            color="danger"
                            variant="outline"
                            size="sm"
                            onClick={() => handleRemoveItem(index)}
                          >
                            <FontAwesomeIcon icon={faTrash} />
                          </CButton>
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>
              ) : (
                <CAlert color="info">
                  <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                  No checklist items added yet. Add items above to define what will be assessed during the audit.
                </CAlert>
              )}
            </CAccordionBody>
          </CAccordionItem>
        </CAccordion>

        {/* Action Buttons */}
        <CRow className="mt-4">
          <CCol>
            <div className="d-flex justify-content-between">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate(`/audits/${id}`)}
                disabled={isUpdating}
              >
                <FontAwesomeIcon icon={AUDIT_ICONS.back} className="me-2" />
                Cancel
              </CButton>

              <div className="d-flex gap-2">
                <CButton
                  color="primary"
                  type="submit"
                  disabled={isUpdating || !isDirty}
                >
                  {isUpdating ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Updating...
                    </>
                  ) : (
                    <>
                      <FontAwesomeIcon icon={AUDIT_ICONS.save} className="me-2" />
                      Update Audit
                    </>
                  )}
                </CButton>
              </div>
            </div>
          </CCol>
        </CRow>
      </CForm>
    </div>
  );
};

export default EditAudit;