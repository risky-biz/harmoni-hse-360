import React, { useState, useEffect } from 'react';
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
  CButton,
  CForm,
  CFormInput,
  CFormSelect,
  CFormTextarea,
  CFormLabel,
  CFormCheck,
  CInputGroup,
  CInputGroupText,
  CSpinner,
  CAlert,
  CAccordion,
  CAccordionItem,
  CAccordionHeader,
  CAccordionBody,
  CButtonGroup,
  CBadge
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faGraduationCap,
  faSave,
  faTimes,
  faArrowLeft,
  faInfoCircle,
  faUsers,
  faCalendarAlt,
  faMapMarkerAlt,
  faFileAlt,
  faExclamationTriangle,
  faCheckCircle
} from '@fortawesome/free-solid-svg-icons';
import { format } from 'date-fns';

import {
  useGetTrainingByIdQuery,
  useUpdateTrainingMutation
} from '../../features/trainings/trainingApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { TrainingAttachmentManager } from '../../components/trainings';
import {
  TrainingType,
  TrainingCategory,
  TrainingPriority,
  DeliveryMethod,
  ComplianceFramework,
  TRAINING_TYPES,
  TRAINING_CATEGORIES,
  TRAINING_PRIORITIES,
  DELIVERY_METHODS,
  COMPLIANCE_FRAMEWORKS
} from '../../types/training';

interface TrainingFormData {
  title: string;
  description: string;
  type: TrainingType;
  category: TrainingCategory;
  priority: TrainingPriority;
  deliveryMethod: DeliveryMethod;
  durationHours: number;
  maxParticipants: number;
  location: string;
  scheduledStartDate: string;
  scheduledEndDate: string;
  prerequisites: string;
  learningObjectives: string;
  instructorName: string;
  instructorEmail: string;
  instructorPhone: string;
  isExternalInstructor: boolean;
  externalInstructorCompany: string;
  isK3MandatoryTraining: boolean;
  requiresCertification: boolean;
  certificateValidityMonths: number;
  isRefresherTraining: boolean;
  refresherDurationMonths: number;
  complianceFramework: ComplianceFramework;
  cost: number;
  currency: string;
  materialsCost: number;
  venue: string;
  maxClassSize: number;
  minimumPassScore: number;
  assessmentMethod: string;
  trainingMethods: string;
  targetAudience: string;
  department: string;
  notes: string;
}

const schema = yup.object().shape({
  title: yup.string().required('Training title is required').min(3, 'Title must be at least 3 characters'),
  description: yup.string().required('Description is required').min(10, 'Description must be at least 10 characters'),
  type: yup.string().required('Training type is required'),
  category: yup.string().required('Training category is required'),
  priority: yup.string().required('Priority is required'),
  deliveryMethod: yup.string().required('Delivery method is required'),
  durationHours: yup.number().required('Duration is required').min(0.5, 'Duration must be at least 30 minutes').max(40, 'Duration cannot exceed 40 hours'),
  maxParticipants: yup.number().required('Maximum participants is required').min(1, 'Must allow at least 1 participant').max(1000, 'Cannot exceed 1000 participants'),
  scheduledStartDate: yup.string().required('Start date is required'),
  scheduledEndDate: yup.string().required('End date is required'),
  instructorName: yup.string().when('isExternalInstructor', {
    is: true,
    then: (schema) => schema.required('External instructor name is required'),
    otherwise: (schema) => schema
  }),
  instructorEmail: yup.string().when('isExternalInstructor', {
    is: true,
    then: (schema) => schema.required('External instructor email is required').email('Invalid email format'),
    otherwise: (schema) => schema.email('Invalid email format')
  }),
  certificateValidityMonths: yup.number().when('requiresCertification', {
    is: true,
    then: (schema) => schema.required('Certificate validity period is required').min(1, 'Must be at least 1 month'),
    otherwise: (schema) => schema
  }),
  refresherDurationMonths: yup.number().when('isRefresherTraining', {
    is: true,
    then: (schema) => schema.required('Refresher duration is required').min(1, 'Must be at least 1 month'),
    otherwise: (schema) => schema
  }),
  cost: yup.number().min(0, 'Cost cannot be negative'),
  materialsCost: yup.number().min(0, 'Materials cost cannot be negative'),
  minimumPassScore: yup.number().min(0, 'Score cannot be negative').max(100, 'Score cannot exceed 100%')
});

const EditTraining: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isDemo } = useApplicationMode();

  const [activeAccordion, setActiveAccordion] = useState(['basic-info']);

  // API queries
  const { data: training, isLoading: isLoadingTraining, error } = useGetTrainingByIdQuery(id!);
  const { data: departments } = useGetDepartmentsQuery({});
  const [updateTraining, { isLoading: isUpdating }] = useUpdateTrainingMutation();

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    watch,
    setValue,
    reset
  } = useForm<TrainingFormData>({
    resolver: yupResolver(schema),
    defaultValues: {
      currency: 'IDR',
      cost: 0,
      materialsCost: 0,
      minimumPassScore: 70,
      maxClassSize: 20,
      isK3MandatoryTraining: false,
      requiresCertification: false,
      isRefresherTraining: false,
      isExternalInstructor: false,
      certificateValidityMonths: 12,
      refresherDurationMonths: 12
    }
  });

  // Load training data into form when available
  useEffect(() => {
    if (training) {
      reset({
        title: training.title,
        description: training.description,
        type: training.type,
        category: training.category,
        priority: training.priority,
        deliveryMethod: training.deliveryMethod,
        durationHours: training.durationHours,
        maxParticipants: training.maxParticipants,
        location: training.location,
        scheduledStartDate: format(new Date(training.scheduledStartDate), 'yyyy-MM-dd\'T\'HH:mm'),
        scheduledEndDate: training.scheduledEndDate ? format(new Date(training.scheduledEndDate), 'yyyy-MM-dd\'T\'HH:mm') : '',
        prerequisites: training.prerequisites || '',
        learningObjectives: training.learningObjectives || '',
        instructorName: training.instructorName || '',
        instructorEmail: training.instructorEmail || '',
        instructorPhone: training.instructorPhone || '',
        isExternalInstructor: training.isExternalInstructor,
        externalInstructorCompany: training.externalInstructorCompany || '',
        isK3MandatoryTraining: training.isK3MandatoryTraining,
        requiresCertification: training.requiresCertification,
        certificateValidityMonths: training.certificateValidityMonths || 12,
        isRefresherTraining: training.isRefresherTraining,
        refresherDurationMonths: training.refresherDurationMonths || 12,
        complianceFramework: training.complianceFramework || 'ISO45001',
        cost: training.cost || 0,
        currency: training.currency || 'IDR',
        materialsCost: training.materialsCost || 0,
        venue: training.venue || '',
        maxClassSize: training.maxClassSize || 20,
        minimumPassScore: training.minimumPassScore || 70,
        assessmentMethod: training.assessmentMethod || '',
        trainingMethods: training.trainingMethods || '',
        targetAudience: training.targetAudience || '',
        department: training.department || '',
        notes: training.notes || ''
      });
    }
  }, [training, reset]);

  const watchedFields = watch();

  const onSubmit = async (data: TrainingFormData) => {
    if (!training) return;

    try {
      await updateTraining({
        id: training.id,
        ...data,
        scheduledStartDate: new Date(data.scheduledStartDate).toISOString(),
        scheduledEndDate: new Date(data.scheduledEndDate).toISOString()
      }).unwrap();

      navigate(`/trainings/${training.id}`);
    } catch (error) {
      console.error('Update training failed:', error);
    }
  };

  const handleCancel = () => {
    if (isDirty) {
      if (confirm('You have unsaved changes. Are you sure you want to leave?')) {
        navigate(`/trainings/${id}`);
      }
    } else {
      navigate(`/trainings/${id}`);
    }
  };

  if (isLoadingTraining) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner color="primary" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Failed to load training details. Please try again.
      </CAlert>
    );
  }

  if (!training) {
    return (
      <CAlert color="warning">
        Training not found.
      </CAlert>
    );
  }

  if (training.status !== 'Draft') {
    return (
      <CAlert color="warning">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        This training can only be edited when in Draft status.
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        {/* Header */}
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div>
            <CButton
              color="link"
              className="p-0 me-3"
              onClick={() => navigate(`/trainings/${id}`)}
            >
              <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
              Back to Training
            </CButton>
            <h2 className="mb-1">Edit Training</h2>
            <div className="d-flex align-items-center gap-2">
              <small className="text-muted">{training.trainingCode}</small>
              <CBadge color="secondary">Draft</CBadge>
              {isDirty && <CBadge color="warning">Unsaved Changes</CBadge>}
            </div>
          </div>
          <div className="d-flex gap-2">
            <CButton
              color="secondary"
              variant="outline"
              onClick={handleCancel}
            >
              <FontAwesomeIcon icon={faTimes} className="me-1" />
              Cancel
            </CButton>
            <PermissionGuard
              moduleType={ModuleType.TrainingManagement}
              permissionType={PermissionType.Update}
            >
              <CButton
                color="primary"
                onClick={handleSubmit(onSubmit)}
                disabled={isUpdating || isDemo}
              >
                {isUpdating ? (
                  <>
                    <CSpinner size="sm" className="me-1" />
                    Saving...
                  </>
                ) : (
                  <>
                    <FontAwesomeIcon icon={faSave} className="me-1" />
                    Save Changes
                  </>
                )}
              </CButton>
            </PermissionGuard>
          </div>
        </div>

        <CForm onSubmit={handleSubmit(onSubmit)}>
          <CAccordion 
            activeItemKey={activeAccordion} 
            alwaysOpen
            onChange={(key) => setActiveAccordion(key as string[])}
          >
            {/* Basic Information */}
            <CAccordionItem itemKey="basic-info">
              <CAccordionHeader>
                <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                Basic Information
                {errors.title || errors.description || errors.type || errors.category ? (
                  <CBadge color="danger" className="ms-2">Required</CBadge>
                ) : (
                  <CBadge color="success" className="ms-2">
                    <FontAwesomeIcon icon={faCheckCircle} />
                  </CBadge>
                )}
              </CAccordionHeader>
              <CAccordionBody>
                <CRow>
                  <CCol md={8}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="title">Training Title *</CFormLabel>
                      <CFormInput
                        id="title"
                        {...register('title')}
                        invalid={!!errors.title}
                        placeholder="Enter training title"
                      />
                      {errors.title && (
                        <div className="invalid-feedback">{errors.title.message}</div>
                      )}
                    </div>
                  </CCol>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="priority">Priority *</CFormLabel>
                      <CFormSelect
                        id="priority"
                        {...register('priority')}
                        invalid={!!errors.priority}
                      >
                        <option value="">Select Priority</option>
                        {TRAINING_PRIORITIES.map(priority => (
                          <option key={priority.value} value={priority.value}>
                            {priority.label}
                          </option>
                        ))}
                      </CFormSelect>
                      {errors.priority && (
                        <div className="invalid-feedback">{errors.priority.message}</div>
                      )}
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="description">Description *</CFormLabel>
                  <CFormTextarea
                    id="description"
                    rows={4}
                    {...register('description')}
                    invalid={!!errors.description}
                    placeholder="Provide a detailed description of the training"
                  />
                  {errors.description && (
                    <div className="invalid-feedback">{errors.description.message}</div>
                  )}
                </div>

                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="type">Training Type *</CFormLabel>
                      <CFormSelect
                        id="type"
                        {...register('type')}
                        invalid={!!errors.type}
                      >
                        <option value="">Select Type</option>
                        {TRAINING_TYPES.map(type => (
                          <option key={type.value} value={type.value}>
                            {type.label}
                          </option>
                        ))}
                      </CFormSelect>
                      {errors.type && (
                        <div className="invalid-feedback">{errors.type.message}</div>
                      )}
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="category">Category *</CFormLabel>
                      <CFormSelect
                        id="category"
                        {...register('category')}
                        invalid={!!errors.category}
                      >
                        <option value="">Select Category</option>
                        {TRAINING_CATEGORIES.map(category => (
                          <option key={category.value} value={category.value}>
                            {category.label}
                          </option>
                        ))}
                      </CFormSelect>
                      {errors.category && (
                        <div className="invalid-feedback">{errors.category.message}</div>
                      )}
                    </div>
                  </CCol>
                </CRow>

                <CRow>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="deliveryMethod">Delivery Method *</CFormLabel>
                      <CFormSelect
                        id="deliveryMethod"
                        {...register('deliveryMethod')}
                        invalid={!!errors.deliveryMethod}
                      >
                        <option value="">Select Method</option>
                        {DELIVERY_METHODS.map(method => (
                          <option key={method.value} value={method.value}>
                            {method.label}
                          </option>
                        ))}
                      </CFormSelect>
                      {errors.deliveryMethod && (
                        <div className="invalid-feedback">{errors.deliveryMethod.message}</div>
                      )}
                    </div>
                  </CCol>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="durationHours">Duration (Hours) *</CFormLabel>
                      <CFormInput
                        id="durationHours"
                        type="number"
                        step="0.5"
                        min="0.5"
                        max="40"
                        {...register('durationHours', { valueAsNumber: true })}
                        invalid={!!errors.durationHours}
                        placeholder="e.g. 8"
                      />
                      {errors.durationHours && (
                        <div className="invalid-feedback">{errors.durationHours.message}</div>
                      )}
                    </div>
                  </CCol>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="maxParticipants">Max Participants *</CFormLabel>
                      <CFormInput
                        id="maxParticipants"
                        type="number"
                        min="1"
                        max="1000"
                        {...register('maxParticipants', { valueAsNumber: true })}
                        invalid={!!errors.maxParticipants}
                        placeholder="e.g. 20"
                      />
                      {errors.maxParticipants && (
                        <div className="invalid-feedback">{errors.maxParticipants.message}</div>
                      )}
                    </div>
                  </CCol>
                </CRow>
              </CAccordionBody>
            </CAccordionItem>

            {/* Schedule & Location */}
            <CAccordionItem itemKey="schedule-location">
              <CAccordionHeader>
                <FontAwesomeIcon icon={faCalendarAlt} className="me-2" />
                Schedule & Location
                {errors.scheduledStartDate || errors.scheduledEndDate ? (
                  <CBadge color="danger" className="ms-2">Required</CBadge>
                ) : (
                  <CBadge color="success" className="ms-2">
                    <FontAwesomeIcon icon={faCheckCircle} />
                  </CBadge>
                )}
              </CAccordionHeader>
              <CAccordionBody>
                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="scheduledStartDate">Start Date & Time *</CFormLabel>
                      <CFormInput
                        id="scheduledStartDate"
                        type="datetime-local"
                        {...register('scheduledStartDate')}
                        invalid={!!errors.scheduledStartDate}
                      />
                      {errors.scheduledStartDate && (
                        <div className="invalid-feedback">{errors.scheduledStartDate.message}</div>
                      )}
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="scheduledEndDate">End Date & Time *</CFormLabel>
                      <CFormInput
                        id="scheduledEndDate"
                        type="datetime-local"
                        {...register('scheduledEndDate')}
                        invalid={!!errors.scheduledEndDate}
                      />
                      {errors.scheduledEndDate && (
                        <div className="invalid-feedback">{errors.scheduledEndDate.message}</div>
                      )}
                    </div>
                  </CCol>
                </CRow>

                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="location">Location</CFormLabel>
                      <CInputGroup>
                        <CInputGroupText>
                          <FontAwesomeIcon icon={faMapMarkerAlt} />
                        </CInputGroupText>
                        <CFormInput
                          id="location"
                          {...register('location')}
                          placeholder="Training location"
                        />
                      </CInputGroup>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="venue">Venue Details</CFormLabel>
                      <CFormInput
                        id="venue"
                        {...register('venue')}
                        placeholder="Room, floor, building details"
                      />
                    </div>
                  </CCol>
                </CRow>
              </CAccordionBody>
            </CAccordionItem>

            {/* Instructor Information */}
            <CAccordionItem itemKey="instructor">
              <CAccordionHeader>
                <FontAwesomeIcon icon={faUsers} className="me-2" />
                Instructor Information
                {watchedFields.isExternalInstructor && (errors.instructorName || errors.instructorEmail) ? (
                  <CBadge color="danger" className="ms-2">Required</CBadge>
                ) : (
                  <CBadge color="success" className="ms-2">
                    <FontAwesomeIcon icon={faCheckCircle} />
                  </CBadge>
                )}
              </CAccordionHeader>
              <CAccordionBody>
                <div className="mb-3">
                  <CFormCheck
                    id="isExternalInstructor"
                    label="External Instructor"
                    {...register('isExternalInstructor')}
                  />
                  <small className="text-muted">Check if using an external instructor</small>
                </div>

                {watchedFields.isExternalInstructor && (
                  <>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="instructorName">Instructor Name *</CFormLabel>
                          <CFormInput
                            id="instructorName"
                            {...register('instructorName')}
                            invalid={!!errors.instructorName}
                            placeholder="Full name"
                          />
                          {errors.instructorName && (
                            <div className="invalid-feedback">{errors.instructorName.message}</div>
                          )}
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="externalInstructorCompany">Company</CFormLabel>
                          <CFormInput
                            id="externalInstructorCompany"
                            {...register('externalInstructorCompany')}
                            placeholder="Instructor's company"
                          />
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="instructorEmail">Email *</CFormLabel>
                          <CFormInput
                            id="instructorEmail"
                            type="email"
                            {...register('instructorEmail')}
                            invalid={!!errors.instructorEmail}
                            placeholder="instructor@example.com"
                          />
                          {errors.instructorEmail && (
                            <div className="invalid-feedback">{errors.instructorEmail.message}</div>
                          )}
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="instructorPhone">Phone</CFormLabel>
                          <CFormInput
                            id="instructorPhone"
                            {...register('instructorPhone')}
                            placeholder="+62 xxx xxx xxxx"
                          />
                        </div>
                      </CCol>
                    </CRow>
                  </>
                )}
              </CAccordionBody>
            </CAccordionItem>

            {/* Training Requirements */}
            <CAccordionItem itemKey="requirements">
              <CAccordionHeader>
                <FontAwesomeIcon icon={faFileAlt} className="me-2" />
                Training Requirements & Compliance
              </CAccordionHeader>
              <CAccordionBody>
                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormCheck
                        id="isK3MandatoryTraining"
                        label="K3 Mandatory Training"
                        {...register('isK3MandatoryTraining')}
                      />
                      <small className="text-muted">Required for HSE compliance</small>
                    </div>

                    <div className="mb-3">
                      <CFormCheck
                        id="requiresCertification"
                        label="Requires Certification"
                        {...register('requiresCertification')}
                      />
                      <small className="text-muted">Participants receive certificates upon completion</small>
                    </div>

                    <div className="mb-3">
                      <CFormCheck
                        id="isRefresherTraining"
                        label="Refresher Training"
                        {...register('isRefresherTraining')}
                      />
                      <small className="text-muted">Periodic renewal required</small>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    {watchedFields.requiresCertification && (
                      <div className="mb-3">
                        <CFormLabel htmlFor="certificateValidityMonths">Certificate Validity (Months) *</CFormLabel>
                        <CFormInput
                          id="certificateValidityMonths"
                          type="number"
                          min="1"
                          {...register('certificateValidityMonths', { valueAsNumber: true })}
                          invalid={!!errors.certificateValidityMonths}
                          placeholder="12"
                        />
                        {errors.certificateValidityMonths && (
                          <div className="invalid-feedback">{errors.certificateValidityMonths.message}</div>
                        )}
                      </div>
                    )}

                    {watchedFields.isRefresherTraining && (
                      <div className="mb-3">
                        <CFormLabel htmlFor="refresherDurationMonths">Refresher Period (Months) *</CFormLabel>
                        <CFormInput
                          id="refresherDurationMonths"
                          type="number"
                          min="1"
                          {...register('refresherDurationMonths', { valueAsNumber: true })}
                          invalid={!!errors.refresherDurationMonths}
                          placeholder="12"
                        />
                        {errors.refresherDurationMonths && (
                          <div className="invalid-feedback">{errors.refresherDurationMonths.message}</div>
                        )}
                      </div>
                    )}

                    <div className="mb-3">
                      <CFormLabel htmlFor="complianceFramework">Compliance Framework</CFormLabel>
                      <CFormSelect
                        id="complianceFramework"
                        {...register('complianceFramework')}
                      >
                        {COMPLIANCE_FRAMEWORKS.map(framework => (
                          <option key={framework.value} value={framework.value}>
                            {framework.label}
                          </option>
                        ))}
                      </CFormSelect>
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="prerequisites">Prerequisites</CFormLabel>
                  <CFormTextarea
                    id="prerequisites"
                    rows={3}
                    {...register('prerequisites')}
                    placeholder="Any prerequisites or requirements for participants"
                  />
                </div>

                <div className="mb-3">
                  <CFormLabel htmlFor="learningObjectives">Learning Objectives</CFormLabel>
                  <CFormTextarea
                    id="learningObjectives"
                    rows={3}
                    {...register('learningObjectives')}
                    placeholder="What participants will learn from this training"
                  />
                </div>
              </CAccordionBody>
            </CAccordionItem>

            {/* Additional Details */}
            <CAccordionItem itemKey="additional">
              <CAccordionHeader>
                <FontAwesomeIcon icon={faFileAlt} className="me-2" />
                Additional Details
              </CAccordionHeader>
              <CAccordionBody>
                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="targetAudience">Target Audience</CFormLabel>
                      <CFormInput
                        id="targetAudience"
                        {...register('targetAudience')}
                        placeholder="Who should attend this training"
                      />
                    </div>

                    <div className="mb-3">
                      <CFormLabel htmlFor="department">Department</CFormLabel>
                      <CFormSelect
                        id="department"
                        {...register('department')}
                      >
                        <option value="">All Departments</option>
                        {departments?.items?.map(dept => (
                          <option key={dept.id} value={dept.name}>
                            {dept.name}
                          </option>
                        ))}
                      </CFormSelect>
                    </div>

                    <div className="mb-3">
                      <CFormLabel htmlFor="assessmentMethod">Assessment Method</CFormLabel>
                      <CFormInput
                        id="assessmentMethod"
                        {...register('assessmentMethod')}
                        placeholder="How participants will be assessed"
                      />
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="trainingMethods">Training Methods</CFormLabel>
                      <CFormInput
                        id="trainingMethods"
                        {...register('trainingMethods')}
                        placeholder="Lecture, hands-on, simulation, etc."
                      />
                    </div>

                    <div className="mb-3">
                      <CFormLabel htmlFor="minimumPassScore">Minimum Pass Score (%)</CFormLabel>
                      <CFormInput
                        id="minimumPassScore"
                        type="number"
                        min="0"
                        max="100"
                        {...register('minimumPassScore', { valueAsNumber: true })}
                        invalid={!!errors.minimumPassScore}
                        placeholder="70"
                      />
                      {errors.minimumPassScore && (
                        <div className="invalid-feedback">{errors.minimumPassScore.message}</div>
                      )}
                    </div>

                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="cost">Training Cost</CFormLabel>
                          <CInputGroup>
                            <CFormSelect
                              {...register('currency')}
                              style={{ maxWidth: '100px' }}
                            >
                              <option value="IDR">IDR</option>
                              <option value="USD">USD</option>
                            </CFormSelect>
                            <CFormInput
                              id="cost"
                              type="number"
                              min="0"
                              {...register('cost', { valueAsNumber: true })}
                              invalid={!!errors.cost}
                              placeholder="0"
                            />
                          </CInputGroup>
                          {errors.cost && (
                            <div className="invalid-feedback">{errors.cost.message}</div>
                          )}
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="materialsCost">Materials Cost</CFormLabel>
                          <CFormInput
                            id="materialsCost"
                            type="number"
                            min="0"
                            {...register('materialsCost', { valueAsNumber: true })}
                            invalid={!!errors.materialsCost}
                            placeholder="0"
                          />
                          {errors.materialsCost && (
                            <div className="invalid-feedback">{errors.materialsCost.message}</div>
                          )}
                        </div>
                      </CCol>
                    </CRow>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="notes">Additional Notes</CFormLabel>
                  <CFormTextarea
                    id="notes"
                    rows={3}
                    {...register('notes')}
                    placeholder="Any additional information or special requirements"
                  />
                </div>
              </CAccordionBody>
            </CAccordionItem>

            {/* Training Materials */}
            <CAccordionItem itemKey="materials">
              <CAccordionHeader>
                <FontAwesomeIcon icon={faFileAlt} className="me-2" />
                Training Materials
              </CAccordionHeader>
              <CAccordionBody>
                <TrainingAttachmentManager
                  trainingId={training.id.toString()}
                  attachments={training.attachments}
                  allowUpload={true}
                  allowDelete={true}
                />
              </CAccordionBody>
            </CAccordionItem>
          </CAccordion>

          {/* Footer Actions */}
          <div className="d-flex justify-content-end gap-2 mt-4">
            <CButton
              color="secondary"
              variant="outline"
              onClick={handleCancel}
            >
              <FontAwesomeIcon icon={faTimes} className="me-1" />
              Cancel
            </CButton>
            <PermissionGuard
              moduleType={ModuleType.TrainingManagement}
              permissionType={PermissionType.Update}
            >
              <CButton
                color="primary"
                type="submit"
                disabled={isUpdating || isDemo}
              >
                {isUpdating ? (
                  <>
                    <CSpinner size="sm" className="me-1" />
                    Saving...
                  </>
                ) : (
                  <>
                    <FontAwesomeIcon icon={faSave} className="me-1" />
                    Save Changes
                  </>
                )}
              </CButton>
            </PermissionGuard>
          </div>
        </CForm>
      </CCol>
    </CRow>
  );
};

export default EditTraining;