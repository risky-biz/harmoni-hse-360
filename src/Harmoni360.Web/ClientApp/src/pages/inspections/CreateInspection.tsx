import React, { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
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
  CBadge
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
  faFileContract
} from '@fortawesome/free-solid-svg-icons';
import { toast } from 'react-toastify';
import { useCreateInspectionMutation, CreateInspectionCommand } from '../../features/inspections/inspectionApi';
import InspectionAttachmentManager from '../../components/inspections/AttachmentManager';
import DemoModeWrapper from '../../components/common/DemoModeWrapper';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';

// Form validation schema
const inspectionSchema = yup.object().shape({
  title: yup.string().required('Title is required'),
  description: yup.string().required('Description is required'),
  type: yup.number().required('Inspection type is required'),
  category: yup.number().required('Category is required'),
  priority: yup.number().required('Priority is required'),
  scheduledDate: yup.date().required('Scheduled date is required'),
  inspectorId: yup.number().required('Inspector is required'),
  locationId: yup.number().required('Location is required'),
  departmentId: yup.number().required('Department is required'),
  facilityId: yup.number().required('Facility is required'),
  estimatedDurationMinutes: yup.number().min(1, 'Duration must be at least 1 minute').required('Estimated duration is required'),
  checklistItems: yup.array().of(
    yup.object().shape({
      question: yup.string().required('Question is required'),
      type: yup.number().required('Item type is required'),
      isRequired: yup.boolean().required(),
      description: yup.string().optional(),
      expectedValue: yup.string().optional(),
      unit: yup.string().optional(),
      minValue: yup.number().nullable().optional(),
      maxValue: yup.number().nullable().optional(),
      options: yup.string().optional(),
      sortOrder: yup.number().required()
    })
  ).min(1, 'At least one checklist item is required')
});

type InspectionFormData = yup.InferType<typeof inspectionSchema>;

import { InspectionType, InspectionCategory, InspectionPriority, InspectionItemType } from '../../types/inspection';

// Mock data - Replace with actual API calls
const inspectionTypes = [
  { value: InspectionType.Safety, label: 'Safety Inspection' },
  { value: InspectionType.Environmental, label: 'Environmental Inspection' },
  { value: InspectionType.Equipment, label: 'Equipment Inspection' },
  { value: InspectionType.Compliance, label: 'Compliance Inspection' },
  { value: InspectionType.Fire, label: 'Fire Inspection' },
  { value: InspectionType.Chemical, label: 'Chemical Inspection' },
  { value: InspectionType.Ergonomic, label: 'Ergonomic Inspection' },
  { value: InspectionType.Emergency, label: 'Emergency Inspection' }
];

const inspectionCategories = [
  { value: InspectionCategory.Routine, label: 'Routine' },
  { value: InspectionCategory.Planned, label: 'Planned' },
  { value: InspectionCategory.Unplanned, label: 'Unplanned' },
  { value: InspectionCategory.Regulatory, label: 'Regulatory' },
  { value: InspectionCategory.Audit, label: 'Audit' },
  { value: InspectionCategory.Incident, label: 'Incident-Based' },
  { value: InspectionCategory.Maintenance, label: 'Maintenance' }
];

const priorities = [
  { value: InspectionPriority.Low, label: 'Low' },
  { value: InspectionPriority.Medium, label: 'Medium' },
  { value: InspectionPriority.High, label: 'High' },
  { value: InspectionPriority.Critical, label: 'Critical' }
];

const itemTypes = [
  { value: InspectionItemType.YesNo, label: 'Yes/No' },
  { value: InspectionItemType.Text, label: 'Text Input' },
  { value: InspectionItemType.Number, label: 'Number' },
  { value: InspectionItemType.MultipleChoice, label: 'Multiple Choice' },
  { value: InspectionItemType.Checklist, label: 'Checklist' },
  { value: InspectionItemType.Photo, label: 'Photo' }
];

// Mock departments and locations - Replace with actual API calls
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

const inspectors = [
  { id: 1, name: 'John Smith' },
  { id: 2, name: 'Jane Doe' },
  { id: 3, name: 'Mike Johnson' },
  { id: 4, name: 'Sarah Wilson' }
];

export const CreateInspection: React.FC = () => {
  const navigate = useNavigate();
  const [createInspection, { isLoading }] = useCreateInspectionMutation();
  const [checklistItems, setChecklistItems] = useState([
    {
      question: '',
      description: '',
      type: InspectionItemType.YesNo,
      isRequired: true,
      expectedValue: '',
      unit: '',
      minValue: null as number | null,
      maxValue: null as number | null,
      options: '',
      sortOrder: 1
    }
  ]);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue
  } = useForm<InspectionFormData>({
    resolver: yupResolver(inspectionSchema) as any,
    defaultValues: {
      priority: 2, // Medium = 2
      estimatedDurationMinutes: 60,
      checklistItems: checklistItems
    }
  });

  const watchedType = watch('type');

  const addChecklistItem = useCallback(() => {
    const newItem = {
      question: '',
      description: '',
      type: InspectionItemType.YesNo,
      isRequired: true,
      expectedValue: '',
      unit: '',
      minValue: null as number | null,
      maxValue: null as number | null,
      options: '',
      sortOrder: checklistItems.length + 1
    };
    const updatedItems = [...checklistItems, newItem];
    setChecklistItems(updatedItems);
    setValue('checklistItems', updatedItems);
  }, [checklistItems, setValue]);

  const removeChecklistItem = useCallback((index: number) => {
    const updatedItems = checklistItems.filter((_, i) => i !== index).map((item, idx) => ({
      ...item,
      sortOrder: idx + 1
    }));
    setChecklistItems(updatedItems);
    setValue('checklistItems', updatedItems);
  }, [checklistItems, setValue]);

  const updateChecklistItem = useCallback((index: number, field: string, value: any) => {
    const updatedItems = [...checklistItems];
    updatedItems[index] = { ...updatedItems[index], [field]: value };
    // Ensure sortOrder is maintained
    if (field !== 'sortOrder') {
      updatedItems[index].sortOrder = index + 1;
    }
    setChecklistItems(updatedItems);
    setValue('checklistItems', updatedItems);
  }, [checklistItems, setValue]);

  const onSubmit = async (data: InspectionFormData) => {
    try {
      const inspectionData: CreateInspectionCommand = {
        title: data.title,
        description: data.description,
        type: Number(data.type),
        category: Number(data.category),
        priority: Number(data.priority),
        scheduledDate: data.scheduledDate.toISOString(),
        inspectorId: data.inspectorId || 1,
        locationId: data.locationId,
        departmentId: data.departmentId,
        facilityId: data.facilityId,
        estimatedDurationMinutes: data.estimatedDurationMinutes,
        items: checklistItems.map((item, index) => ({
          ...item,
          type: Number(item.type),
          sortOrder: index + 1,
          options: item.options ? item.options.split(',').map(o => o.trim()) : []
        }))
      };

      await createInspection(inspectionData).unwrap();
      toast.success('Inspection created successfully!');
      navigate('/inspections');
    } catch (error: any) {
      console.error('Error creating inspection:', error);
      toast.error(error?.data?.message || 'Failed to create inspection');
    }
  };

  const handleCancel = () => {
    navigate('/inspections');
  };

  return (
    <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Create}>
      <DemoModeWrapper>
        <CRow>
          <CCol>
            <CCard>
              <CCardHeader className="d-flex justify-content-between align-items-center">
                <div>
                  <h4 className="mb-0">
                    <FontAwesomeIcon icon={faClipboardCheck} size="lg" className="me-2 text-primary" />
                    Create New Inspection
                  </h4>
                  <small className="text-medium-emphasis">
                    Set up a new inspection with checklist items and requirements
                  </small>
                </div>
                <CButton
                  color="light"
                  variant="outline"
                  onClick={handleCancel}
                  disabled={isLoading}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
                  Back to List
                </CButton>
              </CCardHeader>
              <CCardBody>
                <CForm onSubmit={handleSubmit(onSubmit)}>
                  <CAccordion>
                    {/* Basic Information */}
                    <CAccordionItem itemKey="basic">
                      <CAccordionHeader>
                        <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                        Basic Information
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
                              {...register('type', { valueAsNumber: true })}
                              id="type"
                              invalid={!!errors.type}
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
                              {...register('category', { valueAsNumber: true })}
                              id="category"
                              invalid={!!errors.category}
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
                              {...register('priority', { valueAsNumber: true })}
                              id="priority"
                              invalid={!!errors.priority}
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
                          <CCol md={6}>
                            <CFormLabel htmlFor="inspectorId">
                              Inspector *
                            </CFormLabel>
                            <CFormSelect
                              {...register('inspectorId')}
                              id="inspectorId"
                              invalid={!!errors.inspectorId}
                            >
                              <option value="">Select inspector</option>
                              {inspectors.map(inspector => (
                                <option key={inspector.id} value={inspector.id}>
                                  {inspector.name}
                                </option>
                              ))}
                            </CFormSelect>
                            {errors.inspectorId && (
                              <div className="invalid-feedback d-block">
                                {errors.inspectorId.message}
                              </div>
                            )}
                          </CCol>
                        </CRow>
                      </CAccordionBody>
                    </CAccordionItem>

                    {/* Location & Facility */}
                    <CAccordionItem itemKey="location">
                      <CAccordionHeader>
                        <FontAwesomeIcon icon={faMapMarkerAlt} className="me-2" />
                        Location & Facility
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
                        <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                        Checklist Items *
                        <CBadge color="info" className="ms-2">
                          {checklistItems.length} {checklistItems.length === 1 ? 'item' : 'items'}
                        </CBadge>
                      </CAccordionHeader>
                      <CAccordionBody>
                        {checklistItems.map((item, index) => (
                          <CCard key={index} className="mb-3">
                            <CCardBody>
                              <div className="d-flex justify-content-between align-items-center mb-3">
                                <h6 className="mb-0">Checklist Item {index + 1}</h6>
                                {checklistItems.length > 1 && (
                                  <CButton
                                    color="danger"
                                    variant="outline"
                                    size="sm"
                                    onClick={() => removeChecklistItem(index)}
                                  >
                                    Remove
                                  </CButton>
                                )}
                              </div>

                              <CRow className="mb-3">
                                <CCol md={8}>
                                  <CFormLabel>Question *</CFormLabel>
                                  <CFormInput
                                    value={item.question}
                                    onChange={(e) => updateChecklistItem(index, 'question', e.target.value)}
                                    placeholder="Enter inspection question"
                                  />
                                </CCol>
                                <CCol md={4}>
                                  <CFormLabel>Item Type *</CFormLabel>
                                  <CFormSelect
                                    value={item.type.toString()}
                                    onChange={(e) => updateChecklistItem(index, 'type', Number(e.target.value))}
                                  >
                                    {itemTypes.map(type => (
                                      <option key={type.value} value={type.value}>
                                        {type.label}
                                      </option>
                                    ))}
                                  </CFormSelect>
                                </CCol>
                              </CRow>

                              <CRow className="mb-3">
                                <CCol>
                                  <CFormLabel>Description</CFormLabel>
                                  <CFormTextarea
                                    value={item.description}
                                    onChange={(e) => updateChecklistItem(index, 'description', e.target.value)}
                                    placeholder="Additional details or instructions"
                                    rows={2}
                                  />
                                </CCol>
                              </CRow>

                              {Number(item.type) === InspectionItemType.MultipleChoice && (
                                <CRow className="mb-3">
                                  <CCol>
                                    <CFormLabel>Options (comma-separated)</CFormLabel>
                                    <CFormInput
                                      value={item.options}
                                      onChange={(e) => updateChecklistItem(index, 'options', e.target.value)}
                                      placeholder="Option 1, Option 2, Option 3"
                                    />
                                  </CCol>
                                </CRow>
                              )}

                              {(Number(item.type) === InspectionItemType.Number || Number(item.type) === InspectionItemType.Checklist) && (
                                <CRow className="mb-3">
                                  <CCol md={4}>
                                    <CFormLabel>Expected Value</CFormLabel>
                                    <CFormInput
                                      value={item.expectedValue}
                                      onChange={(e) => updateChecklistItem(index, 'expectedValue', e.target.value)}
                                      placeholder="Expected value"
                                    />
                                  </CCol>
                                  <CCol md={4}>
                                    <CFormLabel>Min Value</CFormLabel>
                                    <CFormInput
                                      type="number"
                                      value={item.minValue || ''}
                                      onChange={(e) => updateChecklistItem(index, 'minValue', e.target.value ? Number(e.target.value) : null)}
                                    />
                                  </CCol>
                                  <CCol md={4}>
                                    <CFormLabel>Max Value</CFormLabel>
                                    <CFormInput
                                      type="number"
                                      value={item.maxValue || ''}
                                      onChange={(e) => updateChecklistItem(index, 'maxValue', e.target.value ? Number(e.target.value) : null)}
                                    />
                                  </CCol>
                                </CRow>
                              )}

                              {Number(item.type) === InspectionItemType.Number && (
                                <CRow className="mb-3">
                                  <CCol md={6}>
                                    <CFormLabel>Unit</CFormLabel>
                                    <CFormInput
                                      value={item.unit}
                                      onChange={(e) => updateChecklistItem(index, 'unit', e.target.value)}
                                      placeholder="e.g., °C, PSI, mm"
                                    />
                                  </CCol>
                                </CRow>
                              )}
                            </CCardBody>
                          </CCard>
                        ))}

                        <CButton
                          color="primary"
                          variant="outline"
                          onClick={addChecklistItem}
                          className="w-100"
                        >
                          <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                          Add Checklist Item
                        </CButton>

                        {errors.checklistItems && (
                          <CAlert color="danger" className="mt-3">
                            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                            {errors.checklistItems.message}
                          </CAlert>
                        )}
                      </CAccordionBody>
                    </CAccordionItem>

                    {/* Attachments */}
                    <CAccordionItem itemKey="attachments">
                      <CAccordionHeader>
                        <FontAwesomeIcon icon={faFileContract} className="me-2" />
                        Attachments
                      </CAccordionHeader>
                      <CAccordionBody>
                        <div className="text-center py-4">
                          <FontAwesomeIcon icon={faFileContract} size="2x" className="text-muted mb-3" />
                          <h5>Attachments</h5>
                          <p className="text-muted">
                            Attachments can be added after the inspection is created. You'll be able to upload documents, 
                            images, and other files once you save this inspection.
                          </p>
                          <CBadge color="info">
                            Available after creation
                          </CBadge>
                        </div>
                      </CAccordionBody>
                    </CAccordionItem>
                  </CAccordion>

                  <div className="d-flex justify-content-between mt-4">
                    <CButton
                      color="light"
                      variant="outline"
                      onClick={handleCancel}
                      disabled={isLoading}
                    >
                      <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
                      Cancel
                    </CButton>
                    <CButton
                      color="primary"
                      type="submit"
                      disabled={isLoading}
                    >
                      {isLoading ? (
                        <>
                          <CSpinner size="sm" className="me-2" />
                          Creating...
                        </>
                      ) : (
                        <>
                          <FontAwesomeIcon icon={faSave} className="me-1" />
                          Create Inspection
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