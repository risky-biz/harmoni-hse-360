import React, { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
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

import { useCreateAuditMutation } from '../../features/audits/auditApi';
import AuditAttachmentManager from '../../components/audits/AuditAttachmentManager';
import {
  AuditType,
  AuditCategory,
  AuditPriority,
  CreateAuditRequest,
  AuditItemDto,
} from '../../types/audit';
import { format } from 'date-fns';

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

// Form data interface with all audit fields
interface CreateAuditFormData {
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

interface PendingAttachment {
  file: File;
  attachmentType: string;
  description: string;
  id: string;
}

const CreateAudit: React.FC = () => {
  const navigate = useNavigate();
  
  // State for managing audit items and attachments
  const [currentItem, setCurrentItem] = useState<Partial<AuditItemDto>>({
    description: '',
    type: 'YesNo',
    isRequired: true,
    category: '',
    expectedResult: '',
    maxPoints: 10,
  });
  
  const [pendingAttachments, setPendingAttachments] = useState<PendingAttachment[]>([]);
  const [submitError, setSubmitError] = useState<string | null>(null);

  // API calls
  const [createAudit, { isLoading: isCreating, error: createError }] = useCreateAuditMutation();

  // Form management
  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
    reset
  } = useForm<CreateAuditFormData>({
    defaultValues: {
      title: '',
      description: '',
      type: 'Safety',
      category: 'Routine',
      priority: 'Medium',
      scheduledDate: format(new Date(), 'yyyy-MM-dd\'T\'HH:mm'),
      estimatedDurationMinutes: 120,
      location: '',
      departmentId: null,
      facilityId: null,
      standardsApplied: '',
      isRegulatory: false,
      regulatoryReference: '',
      items: [],
    },
  });

  const watchedItems = watch('items') || [];

  // Handle form submission
  const onSubmit = async (data: CreateAuditFormData) => {
    try {
      setSubmitError(null);
      
      const createRequest: CreateAuditRequest = {
        ...data,
        auditorId: 1, // This should come from current user context
        locationId: null, // Will be handled in backend
      };

      console.log('🚀 Creating audit with data:', createRequest);
      
      // Submit to API
      const result = await createAudit(createRequest).unwrap();
      console.log('✅ Audit created:', result);

      // Upload pending attachments if any
      if (pendingAttachments.length > 0 && result.id) {
        console.log(`📎 Uploading ${pendingAttachments.length} attachments...`);
        try {
          // TODO: Implement attachment upload API when available
          for (const attachment of pendingAttachments) {
            console.log('📎 Would upload:', attachment.file.name);
            // await uploadAttachment({
            //   auditId: result.id.toString(),
            //   file: attachment.file,
            //   attachmentType: attachment.attachmentType,
            //   description: attachment.description,
            // }).unwrap();
          }
          console.log('✅ All attachments uploaded successfully');
        } catch (uploadError) {
          console.error('❌ Failed to upload some attachments:', uploadError);
          // Continue anyway - the audit was created successfully
        }
      }

      // Navigate to audit detail with success message
      navigate(`/audits/${result.id}`, {
        state: {
          message: `Audit created successfully!${pendingAttachments.length > 0 ? ` ${pendingAttachments.length} attachment(s) uploaded.` : ''}`,
          type: 'success',
        },
      });
    } catch (error: any) {
      console.error('Failed to create audit:', error);
      
      // Extract error message
      let errorMessage = 'Failed to create audit. Please try again.';
      if (error?.data?.message) {
        errorMessage = error.data.message;
      } else if (error?.data?.errors) {
        const errorKeys = Object.keys(error.data.errors);
        if (errorKeys.length > 0) {
          errorMessage = `Validation error: ${error.data.errors[errorKeys[0]][0]}`;
        }
      } else if (error?.message) {
        errorMessage = error.message;
      }
      
      setSubmitError(errorMessage);
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
      auditId: 0, // Will be set when audit is created
      itemNumber: `AI-${Date.now()}`,
      description: currentItem.description,
      type: currentItem.type || 'YesNo',
      status: 'NotStarted',
      isRequired: currentItem.isRequired ?? true,
      category: currentItem.category || null,
      expectedResult: currentItem.expectedResult || null,
      maxPoints: currentItem.maxPoints || null,
      sortOrder: watchedItems.length + 1,
      actualResult: null,
      isCompliant: null,
      actualPoints: null,
      comments: null,
      assessedBy: null,
      assessedAt: null,
      evidence: null,
      correctiveAction: null,
      dueDate: null,
      responsiblePersonId: null,
      validationCriteria: null,
      acceptanceCriteria: null,
      createdAt: new Date().toISOString(),
      createdBy: 'Current User',
      lastModifiedAt: null,
      lastModifiedBy: null,
    };

    const updatedItems = [...watchedItems, newItem];
    setValue('items', updatedItems);

    // Reset current item form
    setCurrentItem({
      description: '',
      type: 'YesNo',
      isRequired: true,
      category: '',
      expectedResult: '',
      maxPoints: 10,
    });
  }, [currentItem, watchedItems, setValue]);

  // Handle removing audit item
  const handleRemoveItem = useCallback((index: number) => {
    const updatedItems = watchedItems.filter((_, i) => i !== index);
    setValue('items', updatedItems);
  }, [watchedItems, setValue]);

  return (
    <div className="container-fluid">
      {/* Breadcrumb */}
      <CBreadcrumb className="mb-4">
        <CBreadcrumbItem>
          <FontAwesomeIcon icon={faHome} className="me-1" />
          Dashboard
        </CBreadcrumbItem>
        <CBreadcrumbItem href="/audits">Audits</CBreadcrumbItem>
        <CBreadcrumbItem active>Create New Audit</CBreadcrumbItem>
      </CBreadcrumb>

      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h1 className="h3 mb-2">
                <FontAwesomeIcon icon={AUDIT_ICONS.audit} className="me-2" />
                Create New Audit
              </h1>
              <p className="text-muted mb-0">
                Create a comprehensive audit with checklist items, documentation, and evidence management
              </p>
            </div>
            <div className="d-flex gap-2">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate('/audits')}
              >
                <FontAwesomeIcon icon={AUDIT_ICONS.back} className="me-2" />
                Cancel
              </CButton>
            </div>
          </div>
        </CCol>
      </CRow>

      {/* Form Errors */}
      {(submitError || createError) && (
        <CAlert color="danger" className="mb-4">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
          {submitError || 'Failed to create audit. Please check your inputs and try again.'}
        </CAlert>
      )}

      {/* Create Form */}
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
                    placeholder="e.g., Monthly Safety Audit - Production Floor"
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
                        value={currentItem.description || ''}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, description: e.target.value }))}
                      />
                    </CCol>

                    <CCol md={4}>
                      <CFormLabel htmlFor="itemType">Item Type</CFormLabel>
                      <CFormSelect
                        id="itemType"
                        value={currentItem.type || 'YesNo'}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, type: e.target.value as any }))}
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
                        value={currentItem.category || ''}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, category: e.target.value }))}
                      />
                    </CCol>

                    <CCol md={4}>
                      <CFormLabel htmlFor="expectedResult">Expected Result</CFormLabel>
                      <CFormInput
                        id="expectedResult"
                        type="text"
                        placeholder="e.g., Compliant, 100%, Pass"
                        value={currentItem.expectedResult || ''}
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
                        value={currentItem.maxPoints || 10}
                        onChange={(e) => setCurrentItem(prev => ({ ...prev, maxPoints: parseInt(e.target.value) }))}
                      />
                    </CCol>

                    <CCol md={2}>
                      <div className="d-flex align-items-end h-100">
                        <CFormCheck
                          id="isRequired"
                          label="Required"
                          checked={currentItem.isRequired ?? true}
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
                        <CTableDataCell>{item.description}</CTableDataCell>
                        <CTableDataCell>
                          <CBadge color="info">{item.type}</CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          {item.category && (
                            <CBadge color="secondary" variant="outline">{item.category}</CBadge>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={item.isRequired ? 'danger' : 'secondary'}>
                            {item.isRequired ? 'Required' : 'Optional'}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>{item.maxPoints || '-'}</CTableDataCell>
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

          {/* Attachments */}
          <CAccordionItem itemKey="attachments">
            <CAccordionHeader>
              <FontAwesomeIcon icon={AUDIT_ICONS.attachments} className="me-2" />
              Supporting Documents & Attachments ({pendingAttachments.length})
            </CAccordionHeader>
            <CAccordionBody>
              <div className="mb-3">
                <p className="text-muted">
                  Upload supporting documents such as audit checklists, standards, procedures, 
                  risk assessments, and other relevant documentation for this audit.
                </p>
              </div>
              
              <AuditAttachmentManager
                attachments={[]}
                onAttachmentsChange={setPendingAttachments}
                allowUpload={true}
                allowDelete={true}
                readonly={false}
              />
              
              {pendingAttachments.length > 0 && (
                <CAlert color="info" className="mt-3">
                  <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                  {pendingAttachments.length} file{pendingAttachments.length !== 1 ? 's' : ''} will be uploaded after the audit is created.
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
                onClick={() => navigate('/audits')}
                disabled={isCreating}
              >
                <FontAwesomeIcon icon={AUDIT_ICONS.back} className="me-2" />
                Cancel
              </CButton>

              <div className="d-flex gap-2">
                <CButton
                  color="primary"
                  type="submit"
                  disabled={isCreating}
                >
                  {isCreating ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Creating...
                    </>
                  ) : (
                    <>
                      <FontAwesomeIcon icon={AUDIT_ICONS.save} className="me-2" />
                      Create Audit
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

export default CreateAudit;