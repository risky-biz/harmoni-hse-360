import React, { useState, useCallback, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
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
  CButton,
  CAccordion,
  CAccordionItem,
  CAccordionHeader,
  CAccordionBody,
  CAlert,
  CSpinner,
  CInputGroup,
  CInputGroupText,
  CBadge,
  CBreadcrumb,
  CBreadcrumbItem,
  CFormCheck,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSave,
  faArrowLeft,
  faInfoCircle,
  faExclamationTriangle,
  faClipboardCheck,
  faMapMarkerAlt,
  faBuilding,
  faUser,
  faCalendarAlt,
  faHome,
  faTasks,
  faEdit,
  faCheck,
  faTimes,
  faPlus,
  faTrash,
  faFileContract
} from '@fortawesome/free-solid-svg-icons';
import { toast } from 'react-toastify';
import { 
  useGetInspectionByIdQuery, 
  useUpdateInspectionMutation 
} from '../../features/inspections/inspectionApi';
import InspectionAttachmentManager from '../../components/inspections/AttachmentManager';
import { InspectionDetailDto, InspectionStatus, InspectionItemDto } from '../../types/inspection';
import DemoModeWrapper from '../../components/common/DemoModeWrapper';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { format } from 'date-fns';

// Form validation schema
const editInspectionSchema = yup.object().shape({
  title: yup.string().required('Title is required'),
  description: yup.string().required('Description is required'),
  type: yup.string().required('Inspection type is required'),
  category: yup.string().required('Category is required'),
  priority: yup.string().required('Priority is required'),
  scheduledDate: yup.date().required('Scheduled date is required'),
  estimatedDurationMinutes: yup.number().min(1, 'Duration must be at least 1 minute').required('Estimated duration is required'),
  locationId: yup.number().required('Location is required'),
  departmentId: yup.number().required('Department is required'),
  facilityId: yup.number().required('Facility is required')
});

type EditInspectionFormData = yup.InferType<typeof editInspectionSchema>;

// Mock data - Replace with actual API calls
const inspectionTypes = [
  { value: 'Safety', label: 'Safety Inspection' },
  { value: 'Environmental', label: 'Environmental Inspection' },
  { value: 'Quality', label: 'Quality Inspection' },
  { value: 'Security', label: 'Security Inspection' },
  { value: 'Maintenance', label: 'Maintenance Inspection' },
  { value: 'Compliance', label: 'Compliance Inspection' }
];

const inspectionCategories = [
  { value: 'Routine', label: 'Routine' },
  { value: 'Scheduled', label: 'Scheduled' },
  { value: 'Emergency', label: 'Emergency' },
  { value: 'Incident', label: 'Incident-Based' },
  { value: 'Audit', label: 'Audit' }
];

const priorities = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' }
];

const departments = [
  { id: 1, name: 'Operations' },
  { id: 2, name: 'Maintenance' },
  { id: 3, name: 'Safety' },
  { id: 4, name: 'Environmental' }
];

const locations = [
  { id: 1, name: 'Plant A' },
  { id: 2, name: 'Plant B' },
  { id: 3, name: 'Warehouse' },
  { id: 4, name: 'Office Building' }
];

const facilities = [
  { id: 1, name: 'Facility 1' },
  { id: 2, name: 'Facility 2' },
  { id: 3, name: 'Facility 3' }
];

export const EditInspection: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [itemResponses, setItemResponses] = useState<{ [key: number]: { response: string; notes: string } }>({});

  // API calls
  const { data: inspection, isLoading: isLoadingInspection, error } = useGetInspectionByIdQuery(Number(id));
  const [updateInspection, { isLoading: isUpdating }] = useUpdateInspectionMutation();

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset
  } = useForm<EditInspectionFormData>({
    resolver: yupResolver(editInspectionSchema)
  });

  const watchedStatus = inspection?.status;

  // Initialize form data when inspection loads
  useEffect(() => {
    if (inspection) {
      reset({
        title: inspection.title,
        description: inspection.description,
        type: inspection.type,
        category: inspection.category,
        priority: inspection.priority,
        scheduledDate: new Date(inspection.scheduledDate),
        estimatedDurationMinutes: inspection.estimatedDurationMinutes,
        locationId: inspection.locationId,
        departmentId: inspection.departmentId,
        facilityId: inspection.facilityId
      });

      // Initialize item responses
      const responses: { [key: number]: { response: string; notes: string } } = {};
      inspection.items?.forEach(item => {
        responses[item.id] = {
          response: item.response || '',
          notes: item.notes || ''
        };
      });
      setItemResponses(responses);
    }
  }, [inspection, reset]);

  const updateItemResponse = useCallback((itemId: number, field: 'response' | 'notes', value: string) => {
    setItemResponses(prev => ({
      ...prev,
      [itemId]: {
        ...prev[itemId],
        [field]: value
      }
    }));
  }, []);

  const canEditBasicInfo = () => {
    return inspection?.status === InspectionStatus.Draft || inspection?.status === InspectionStatus.Scheduled;
  };

  const canEditResponses = () => {
    return inspection?.status === InspectionStatus.InProgress || inspection?.status === InspectionStatus.Scheduled;
  };

  const onSubmit = async (data: EditInspectionFormData) => {
    if (!inspection) return;

    try {
      const itemUpdates = Object.entries(itemResponses).map(([itemId, values]) => ({
        id: Number(itemId),
        response: values.response,
        notes: values.notes
      }));

      await updateInspection({
        id: inspection.id,
        title: data.title,
        description: data.description,
        type: data.type,
        category: data.category,
        priority: data.priority,
        scheduledDate: data.scheduledDate.toISOString(),
        estimatedDurationMinutes: data.estimatedDurationMinutes,
        locationId: data.locationId,
        departmentId: data.departmentId,
        facilityId: data.facilityId,
        itemUpdates
      }).unwrap();

      toast.success('Inspection updated successfully!');
      navigate(`/inspections/${id}`);
    } catch (error: any) {
      console.error('Error updating inspection:', error);
      toast.error(error?.data?.message || 'Failed to update inspection');
    }
  };

  const handleCancel = () => {
    navigate(`/inspections/${id}`);
  };

  const getStatusBadge = (status: InspectionStatus) => {
    const statusConfig = {
      [InspectionStatus.Draft]: { color: 'secondary', text: 'Draft' },
      [InspectionStatus.Scheduled]: { color: 'info', text: 'Scheduled' },
      [InspectionStatus.InProgress]: { color: 'warning', text: 'In Progress' },
      [InspectionStatus.Completed]: { color: 'success', text: 'Completed' },
      [InspectionStatus.Cancelled]: { color: 'danger', text: 'Cancelled' }
    };
    const config = statusConfig[status] || { color: 'secondary', text: status };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  };

  const getEditabilityMessage = () => {
    if (!inspection) return '';
    
    switch (inspection.status) {
      case InspectionStatus.Draft:
        return 'Full editing is available for draft inspections.';
      case InspectionStatus.Scheduled:
        return 'Basic information and checklist responses can be edited.';
      case InspectionStatus.InProgress:
        return 'Only checklist responses can be edited during inspection.';
      case InspectionStatus.Completed:
        return 'This inspection is completed and cannot be edited.';
      case InspectionStatus.Cancelled:
        return 'This inspection is cancelled and cannot be edited.';
      default:
        return '';
    }
  };

  if (isLoadingInspection) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner className="text-primary" />
        <span className="ms-2">Loading inspection...</span>
      </div>
    );
  }

  if (error || !inspection) {
    return (
      <CAlert color="danger">
        <h4 className="alert-heading">Unable to load inspection</h4>
        <p>The inspection you're trying to edit could not be loaded.</p>
        <CButton color="primary" onClick={() => navigate('/inspections')}>
          <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
          Back to Inspections
        </CButton>
      </CAlert>
    );
  }

  if (!inspection.canEdit) {
    return (
      <CAlert color="warning">
        <h4 className="alert-heading">Edit Not Allowed</h4>
        <p>You don't have permission to edit this inspection or it cannot be edited in its current state.</p>
        <CButton color="primary" onClick={() => navigate(`/inspections/${id}`)}>
          <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
          Back to Inspection
        </CButton>
      </CAlert>
    );
  }

  return (
    <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
      <DemoModeWrapper>
        {/* Breadcrumb */}
        <CBreadcrumb className="mb-4">
          <CBreadcrumbItem>
            <FontAwesomeIcon icon={faHome} className="me-1" />
            Home
          </CBreadcrumbItem>
          <CBreadcrumbItem onClick={() => navigate('/inspections')} style={{ cursor: 'pointer' }}>
            Inspections
          </CBreadcrumbItem>
          <CBreadcrumbItem onClick={() => navigate(`/inspections/${id}`)} style={{ cursor: 'pointer' }}>
            {inspection.inspectionNumber}
          </CBreadcrumbItem>
          <CBreadcrumbItem active>
            Edit
          </CBreadcrumbItem>
        </CBreadcrumb>

        <CRow>
          <CCol>
            <CCard>
              <CCardHeader className="d-flex justify-content-between align-items-center">
                <div>
                  <h4 className="mb-0">
                    <FontAwesomeIcon icon={faEdit} className="me-2 text-primary" />
                    Edit Inspection
                  </h4>
                  <div className="d-flex align-items-center mt-2">
                    <small className="text-medium-emphasis me-3">
                      #{inspection.inspectionNumber} • {inspection.title}
                    </small>
                    {getStatusBadge(inspection.status)}
                  </div>
                </div>
                <CButton
                  color="light"
                  variant="outline"
                  onClick={handleCancel}
                  disabled={isUpdating}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
                  Back to Details
                </CButton>
              </CCardHeader>
              <CCardBody>
                {/* Edit Restrictions Notice */}
                <CAlert color="info" className="mb-4">
                  <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                  <strong>Edit Restrictions:</strong> {getEditabilityMessage()}
                </CAlert>

                <CForm onSubmit={handleSubmit(onSubmit)}>
                  <CAccordion>
                    {/* Basic Information */}
                    <CAccordionItem itemKey="basic">
                      <CAccordionHeader>
                        <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                        Basic Information
                        {!canEditBasicInfo() && (
                          <CBadge color="secondary" className="ms-2">Read Only</CBadge>
                        )}
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CRow className="mb-3">
                          <CCol md={6}>
                            <CFormLabel htmlFor="title">
                              Title *
                            </CFormLabel>
                            <CFormInput
                              {...register('title')}
                              id="title"
                              placeholder="Enter inspection title"
                              invalid={!!errors.title}
                              disabled={!canEditBasicInfo()}
                            />
                            {errors.title && (
                              <div className="invalid-feedback d-block">
                                {errors.title.message}
                              </div>
                            )}
                          </CCol>
                          <CCol md={6}>
                            <CFormLabel htmlFor="type">
                              Inspection Type *
                            </CFormLabel>
                            <CFormSelect
                              {...register('type')}
                              id="type"
                              invalid={!!errors.type}
                              disabled={!canEditBasicInfo()}
                            >
                              <option value="">Select inspection type</option>
                              {inspectionTypes.map(type => (
                                <option key={type.value} value={type.value}>
                                  {type.label}
                                </option>
                              ))}
                            </CFormSelect>
                            {errors.type && (
                              <div className="invalid-feedback d-block">
                                {errors.type.message}
                              </div>
                            )}
                          </CCol>
                        </CRow>

                        <CRow className="mb-3">
                          <CCol md={6}>
                            <CFormLabel htmlFor="category">
                              Category *
                            </CFormLabel>
                            <CFormSelect
                              {...register('category')}
                              id="category"
                              invalid={!!errors.category}
                              disabled={!canEditBasicInfo()}
                            >
                              <option value="">Select category</option>
                              {inspectionCategories.map(category => (
                                <option key={category.value} value={category.value}>
                                  {category.label}
                                </option>
                              ))}
                            </CFormSelect>
                            {errors.category && (
                              <div className="invalid-feedback d-block">
                                {errors.category.message}
                              </div>
                            )}
                          </CCol>
                          <CCol md={6}>
                            <CFormLabel htmlFor="priority">
                              Priority *
                            </CFormLabel>
                            <CFormSelect
                              {...register('priority')}
                              id="priority"
                              invalid={!!errors.priority}
                              disabled={!canEditBasicInfo()}
                            >
                              {priorities.map(priority => (
                                <option key={priority.value} value={priority.value}>
                                  {priority.label}
                                </option>
                              ))}
                            </CFormSelect>
                            {errors.priority && (
                              <div className="invalid-feedback d-block">
                                {errors.priority.message}
                              </div>
                            )}
                          </CCol>
                        </CRow>

                        <CRow className="mb-3">
                          <CCol>
                            <CFormLabel htmlFor="description">
                              Description *
                            </CFormLabel>
                            <CFormTextarea
                              {...register('description')}
                              id="description"
                              rows={3}
                              placeholder="Describe the inspection purpose and scope"
                              invalid={!!errors.description}
                              disabled={!canEditBasicInfo()}
                            />
                            {errors.description && (
                              <div className="invalid-feedback d-block">
                                {errors.description.message}
                              </div>
                            )}
                          </CCol>
                        </CRow>
                      </CAccordionBody>
                    </CAccordionItem>

                    {/* Scheduling & Assignment */}
                    <CAccordionItem itemKey="scheduling">
                      <CAccordionHeader>
                        <FontAwesomeIcon icon={faCalendarAlt} className="me-2" />
                        Scheduling & Assignment
                        {!canEditBasicInfo() && (
                          <CBadge color="secondary" className="ms-2">Read Only</CBadge>
                        )}
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CRow className="mb-3">
                          <CCol md={6}>
                            <CFormLabel htmlFor="scheduledDate">
                              Scheduled Date *
                            </CFormLabel>
                            <CFormInput
                              {...register('scheduledDate')}
                              type="datetime-local"
                              id="scheduledDate"
                              invalid={!!errors.scheduledDate}
                              disabled={!canEditBasicInfo()}
                            />
                            {errors.scheduledDate && (
                              <div className="invalid-feedback d-block">
                                {errors.scheduledDate.message}
                              </div>
                            )}
                          </CCol>
                          <CCol md={6}>
                            <CFormLabel htmlFor="estimatedDurationMinutes">
                              Estimated Duration (minutes) *
                            </CFormLabel>
                            <CInputGroup>
                              <CFormInput
                                {...register('estimatedDurationMinutes')}
                                type="number"
                                id="estimatedDurationMinutes"
                                min="1"
                                invalid={!!errors.estimatedDurationMinutes}
                                disabled={!canEditBasicInfo()}
                              />
                              <CInputGroupText>minutes</CInputGroupText>
                            </CInputGroup>
                            {errors.estimatedDurationMinutes && (
                              <div className="invalid-feedback d-block">
                                {errors.estimatedDurationMinutes.message}
                              </div>
                            )}
                          </CCol>
                        </CRow>

                        <CRow className="mb-3">
                          <CCol>
                            <div className="d-flex">
                              <FontAwesomeIcon icon={faUser} className="text-muted me-3 mt-3" />
                              <div className="flex-grow-1">
                                <CFormLabel>Inspector</CFormLabel>
                                <div className="form-control-plaintext">
                                  {inspection.inspectorName}
                                </div>
                                <small className="text-muted">
                                  Inspector assignment cannot be changed during editing
                                </small>
                              </div>
                            </div>
                          </CCol>
                        </CRow>
                      </CAccordionBody>
                    </CAccordionItem>

                    {/* Location & Facility */}
                    <CAccordionItem itemKey="location">
                      <CAccordionHeader>
                        <FontAwesomeIcon icon={faMapMarkerAlt} className="me-2" />
                        Location & Facility
                        {!canEditBasicInfo() && (
                          <CBadge color="secondary" className="ms-2">Read Only</CBadge>
                        )}
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CRow className="mb-3">
                          <CCol md={4}>
                            <CFormLabel htmlFor="locationId">
                              Location *
                            </CFormLabel>
                            <CFormSelect
                              {...register('locationId')}
                              id="locationId"
                              invalid={!!errors.locationId}
                              disabled={!canEditBasicInfo()}
                            >
                              <option value="">Select location</option>
                              {locations.map(location => (
                                <option key={location.id} value={location.id}>
                                  {location.name}
                                </option>
                              ))}
                            </CFormSelect>
                            {errors.locationId && (
                              <div className="invalid-feedback d-block">
                                {errors.locationId.message}
                              </div>
                            )}
                          </CCol>
                          <CCol md={4}>
                            <CFormLabel htmlFor="departmentId">
                              Department *
                            </CFormLabel>
                            <CFormSelect
                              {...register('departmentId')}
                              id="departmentId"
                              invalid={!!errors.departmentId}
                              disabled={!canEditBasicInfo()}
                            >
                              <option value="">Select department</option>
                              {departments.map(department => (
                                <option key={department.id} value={department.id}>
                                  {department.name}
                                </option>
                              ))}
                            </CFormSelect>
                            {errors.departmentId && (
                              <div className="invalid-feedback d-block">
                                {errors.departmentId.message}
                              </div>
                            )}
                          </CCol>
                          <CCol md={4}>
                            <CFormLabel htmlFor="facilityId">
                              Facility *
                            </CFormLabel>
                            <CFormSelect
                              {...register('facilityId')}
                              id="facilityId"
                              invalid={!!errors.facilityId}
                              disabled={!canEditBasicInfo()}
                            >
                              <option value="">Select facility</option>
                              {facilities.map(facility => (
                                <option key={facility.id} value={facility.id}>
                                  {facility.name}
                                </option>
                              ))}
                            </CFormSelect>
                            {errors.facilityId && (
                              <div className="invalid-feedback d-block">
                                {errors.facilityId.message}
                              </div>
                            )}
                          </CCol>
                        </CRow>
                      </CAccordionBody>
                    </CAccordionItem>

                    {/* Checklist Items */}
                    <CAccordionItem itemKey="checklist">
                      <CAccordionHeader>
                        <FontAwesomeIcon icon={faTasks} className="me-2" />
                        Inspection Checklist
                        {canEditResponses() ? (
                          <CBadge color="success" className="ms-2">Editable</CBadge>
                        ) : (
                          <CBadge color="secondary" className="ms-2">Read Only</CBadge>
                        )}
                      </CAccordionHeader>
                      <CAccordionBody>
                        {inspection.items && inspection.items.length > 0 ? (
                          <div className="checklist-editor">
                            {inspection.items.map((item, index) => (
                              <CCard key={item.id} className="mb-3">
                                <CCardBody>
                                  <div className="d-flex justify-content-between align-items-start mb-3">
                                    <div className="flex-grow-1">
                                      <div className="d-flex align-items-center mb-2">
                                        <span className="fw-semibold">Item {index + 1}</span>
                                        <CBadge color="info" className="ms-2">{item.typeName}</CBadge>
                                        {item.isRequired && (
                                          <CBadge color="warning" className="ms-1">Required</CBadge>
                                        )}
                                        <CBadge 
                                          color={item.isCompleted ? 'success' : item.hasResponse ? 'warning' : 'secondary'}
                                          className="ms-1"
                                        >
                                          {item.statusName}
                                        </CBadge>
                                      </div>
                                      <h6 className="mb-2">{item.question}</h6>
                                      {item.description && (
                                        <p className="text-muted small mb-3">{item.description}</p>
                                      )}
                                    </div>
                                  </div>

                                  {canEditResponses() ? (
                                    <CRow>
                                      <CCol md={6}>
                                        <CFormLabel>Response</CFormLabel>
                                        {item.type === 'YesNo' ? (
                                          <CFormSelect
                                            value={itemResponses[item.id]?.response || ''}
                                            onChange={(e) => updateItemResponse(item.id, 'response', e.target.value)}
                                          >
                                            <option value="">Select...</option>
                                            <option value="Yes">Yes</option>
                                            <option value="No">No</option>
                                            <option value="N/A">N/A</option>
                                          </CFormSelect>
                                        ) : item.type === 'MultipleChoice' && item.options ? (
                                          <CFormSelect
                                            value={itemResponses[item.id]?.response || ''}
                                            onChange={(e) => updateItemResponse(item.id, 'response', e.target.value)}
                                          >
                                            <option value="">Select...</option>
                                            {item.options.map((option, idx) => (
                                              <option key={idx} value={option}>{option}</option>
                                            ))}
                                          </CFormSelect>
                                        ) : (
                                          <CFormInput
                                            type={item.type === 'Number' || item.type === 'Measurement' ? 'number' : 'text'}
                                            value={itemResponses[item.id]?.response || ''}
                                            onChange={(e) => updateItemResponse(item.id, 'response', e.target.value)}
                                            placeholder={`Enter ${item.typeName.toLowerCase()}...`}
                                          />
                                        )}
                                      </CCol>
                                      <CCol md={6}>
                                        <CFormLabel>Notes (Optional)</CFormLabel>
                                        <CFormTextarea
                                          rows={2}
                                          value={itemResponses[item.id]?.notes || ''}
                                          onChange={(e) => updateItemResponse(item.id, 'notes', e.target.value)}
                                          placeholder="Additional notes..."
                                        />
                                      </CCol>
                                    </CRow>
                                  ) : (
                                    <div className="border rounded p-3 bg-light">
                                      {item.response ? (
                                        <div className="mb-2">
                                          <strong>Response:</strong> 
                                          <span className="ms-2">{item.response}</span>
                                          {item.unit && <span className="text-muted"> {item.unit}</span>}
                                        </div>
                                      ) : (
                                        <div className="text-muted mb-2">No response recorded</div>
                                      )}
                                      {item.notes && (
                                        <div>
                                          <strong>Notes:</strong> 
                                          <span className="ms-2 text-muted">{item.notes}</span>
                                        </div>
                                      )}
                                    </div>
                                  )}
                                </CCardBody>
                              </CCard>
                            ))}
                          </div>
                        ) : (
                          <div className="text-center py-4">
                            <FontAwesomeIcon icon={faTasks} size="3x" className="text-muted mb-3" />
                            <h5>No checklist items</h5>
                            <p className="text-muted">This inspection doesn't have any checklist items.</p>
                          </div>
                        )}
                      </CAccordionBody>
                    </CAccordionItem>

                    {/* Attachments */}
                    <CAccordionItem itemKey="attachments">
                      <CAccordionHeader>
                        <FontAwesomeIcon icon={faFileContract} className="me-2" />
                        Attachments
                        {inspection.attachments && inspection.attachments.length > 0 && (
                          <CBadge color="info" className="ms-2">
                            {inspection.attachments.length} file{inspection.attachments.length !== 1 ? 's' : ''}
                          </CBadge>
                        )}
                      </CAccordionHeader>
                      <CAccordionBody>
                        <InspectionAttachmentManager
                          inspectionId={inspection.id}
                          attachments={inspection.attachments || []}
                          allowUpload={inspection.canEdit}
                          allowDelete={inspection.canEdit}
                          allowView={true}
                          compact={false}
                        />
                      </CAccordionBody>
                    </CAccordionItem>
                  </CAccordion>

                  <div className="d-flex justify-content-between mt-4">
                    <CButton
                      color="light"
                      variant="outline"
                      onClick={handleCancel}
                      disabled={isUpdating}
                    >
                      <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
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
                          Saving...
                        </>
                      ) : (
                        <>
                          <FontAwesomeIcon icon={faSave} className="me-1" />
                          Save Changes
                        </>
                      )}
                    </CButton>
                  </div>
                </CForm>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      </DemoModeWrapper>
    </PermissionGuard>
  );
};