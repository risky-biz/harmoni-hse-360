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
  CFormCheck,
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
  faGraduationCap,
  faCalendarAlt,
  faUsers,
  faCertificate,
  faBook,
  faFileUpload,
  faCheck,
  faPlus,
  faTrash,
  faMapMarkerAlt,
  faClock,
  faUserTie,
  faClipboardCheck
} from '@fortawesome/free-solid-svg-icons';

import { useCreateTrainingMutation, useUploadAttachmentMutation } from '../../features/trainings/trainingApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import {
  TrainingFormData,
  TRAINING_TYPES,
  TRAINING_CATEGORIES,
  TRAINING_PRIORITIES,
  DELIVERY_METHODS,
  CERTIFICATE_TYPES,
  VALIDITY_PERIODS,
  ASSESSMENT_METHODS
} from '../../types/training';
import { TrainingAttachmentManager } from '../../components/trainings';
import { format, addDays } from 'date-fns';

// Validation schema
const schema = yup.object({
  title: yup.string().required('Title is required').max(200, 'Title must not exceed 200 characters'),
  description: yup.string().required('Description is required').max(2000, 'Description must not exceed 2000 characters'),
  type: yup.string().required('Training type is required'),
  category: yup.string().required('Training category is required'),
  priority: yup.string().required('Priority is required'),
  deliveryMethod: yup.string().required('Delivery method is required'),
  scheduledStartDate: yup.string().required('Start date is required'),
  scheduledEndDate: yup.string()
    .required('End date is required')
    .test('is-after-start', 'End date must be after start date', function(value) {
      const { scheduledStartDate } = this.parent;
      if (!value || !scheduledStartDate) return true;
      
      const startDate = new Date(scheduledStartDate + 'T00:00:00');
      const endDate = new Date(value + 'T00:00:00');
      
      return endDate >= startDate;
    }),
  venue: yup.string().max(200, 'Venue must not exceed 200 characters'),
  maxParticipants: yup.number().required('Maximum participants is required').min(1, 'Must allow at least 1 participant'),
  minParticipants: yup.number()
    .required('Minimum participants is required')
    .min(1, 'Must require at least 1 participant')
    .test('min-less-than-max', 'Minimum participants must not exceed maximum', function(value) {
      const { maxParticipants } = this.parent;
      if (!value || !maxParticipants) return true;
      return value <= maxParticipants;
    }),
  instructorName: yup.string().max(100, 'Instructor name must not exceed 100 characters'),
  instructorContact: yup.string().max(50, 'Contact must not exceed 50 characters'),
  passingScore: yup.number().when('requiresCertification', {
    is: true,
    then: (schema) => schema.required('Passing score is required for certification').min(0).max(100),
    otherwise: (schema) => schema.nullable()
  }),
  certifyingBody: yup.string().when('requiresCertification', {
    is: true,
    then: (schema) => schema.required('Certifying body is required for certification').max(200),
    otherwise: (schema) => schema.nullable()
  }),
});

// Training Icon Mappings
const TRAINING_ICONS = {
  // Main training icon
  training: faGraduationCap,
  
  // Section icons following Harmoni360 standards
  basicInformation: faInfoCircle,
  trainingDetails: faBook,
  schedule: faCalendarAlt,
  participants: faUsers,
  instructor: faUserTie,
  certification: faCertificate,
  compliance: faClipboardCheck,
  attachments: faFileUpload,
  reviewSubmit: faCheck,
  
  // Action icons
  create: faPlus,
  save: faSave,
  back: faArrowLeft,
  location: faMapMarkerAlt,
  time: faClock
};

const CreateTraining: React.FC = () => {
  const navigate = useNavigate();

  // API calls
  const { data: departments } = useGetDepartmentsQuery({});
  const [createTraining, { isLoading }] = useCreateTrainingMutation();
  const [uploadAttachment] = useUploadAttachmentMutation();

  // Form state
  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
    getValues,
  } = useForm<TrainingFormData>({
    resolver: yupResolver(schema) as any,
    defaultValues: {
      title: '',
      description: '',
      type: 'SafetyOrientation',
      category: 'SafetyTraining',
      priority: 'Medium',
      deliveryMethod: 'InPerson',
      scheduledStartDate: format(new Date(), 'yyyy-MM-dd'),
      scheduledEndDate: format(addDays(new Date(), 1), 'yyyy-MM-dd'),
      venue: '',
      venueAddress: '',
      maxParticipants: 20,
      minParticipants: 5,
      instructorName: '',
      instructorQualifications: '',
      instructorContact: '',
      isExternalInstructor: false,
      requiresCertification: false,
      certificationType: 'Competency',
      certificateValidityPeriod: 'OneYear',
      certifyingBody: '',
      passingScore: 80,
      assessmentMethod: 'Written',
      isK3MandatoryTraining: false,
      k3RegulationReference: '',
      isBPJSCompliant: false,
      bpjsReference: '',
      skkniReference: '',
      requiresGovernmentCertification: false,
      learningObjectives: '',
      courseOutline: '',
      prerequisites: '',
      materials: '',
      onlineLink: '',
      onlinePlatform: '',
    },
  });

  // Additional state
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [pendingAttachments, setPendingAttachments] = useState<any[]>([]);

  // Watch form values for conditional logic
  const watchRequiresCertification = watch('requiresCertification');
  const watchDeliveryMethod = watch('deliveryMethod');
  const watchIsK3Training = watch('isK3MandatoryTraining');

  // Submit form
  const onSubmit = async (data: TrainingFormData) => {
    console.log('🚀 Creating training with data:', data);
    setSubmitError(null);

    try {
      // Create the training
      const result = await createTraining(data).unwrap();
      console.log('✅ Training created successfully:', result);

      // Upload attachments if any
      if (pendingAttachments.length > 0) {
        console.log('📎 Uploading attachments...');
        for (const attachment of pendingAttachments) {
          try {
            await uploadAttachment({
              trainingId: result.id,
              file: attachment.file,
              attachmentType: attachment.attachmentType,
              description: attachment.description,
              isTrainingMaterial: attachment.isTrainingMaterial
            }).unwrap();
          } catch (attachmentError) {
            console.warn('⚠️ Failed to upload attachment:', attachment.fileName, attachmentError);
          }
        }
      }

      // Navigate to the created training detail page
      navigate(`/trainings/${result.id}`);

    } catch (error: any) {
      console.error('❌ Failed to create training:', error);
      setSubmitError(error?.data?.message || error?.message || 'Failed to create training. Please try again.');
    }
  };

  const handleCancel = () => {
    navigate('/trainings');
  };

  return (
    <CRow>
      <CCol xs={12}>
        <CCard>
          <CCardHeader>
            <div className="d-flex justify-content-between align-items-center">
              <div className="d-flex align-items-center">
                <FontAwesomeIcon icon={TRAINING_ICONS.training} size="lg" className="me-2 text-primary" />
                <h5 className="mb-0">Create New Training</h5>
              </div>
              <div>
                <CButton
                  color="secondary"
                  variant="outline"
                  className="me-2"
                  onClick={handleCancel}
                  disabled={isLoading}
                >
                  <FontAwesomeIcon icon={TRAINING_ICONS.back} className="me-1" />
                  Cancel
                </CButton>
                <CButton
                  color="primary"
                  type="submit"
                  form="training-form"
                  disabled={isLoading}
                >
                  <FontAwesomeIcon icon={isLoading ? 'spinner' : TRAINING_ICONS.save} className="me-1" spin={isLoading} />
                  {isLoading ? 'Creating...' : 'Create Training'}
                </CButton>
              </div>
            </div>
          </CCardHeader>
          <CCardBody>
            {submitError && (
              <CAlert color="danger" className="mb-4">
                <FontAwesomeIcon icon="exclamation-triangle" className="me-2" />
                {submitError}
              </CAlert>
            )}

            <CForm id="training-form" onSubmit={handleSubmit(onSubmit)}>
              <CAccordion alwaysOpen>
                
                {/* Basic Information */}
                <CAccordionItem itemKey="basic">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={TRAINING_ICONS.basicInformation} className="me-2" />
                    Basic Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={8}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="title">Training Title <span className="text-danger">*</span></CFormLabel>
                          <CFormInput
                            id="title"
                            {...register('title')}
                            invalid={!!errors.title}
                            placeholder="Enter training title"
                          />
                          {errors.title && <div className="invalid-feedback">{errors.title.message}</div>}
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="priority">Priority <span className="text-danger">*</span></CFormLabel>
                          <CFormSelect id="priority" {...register('priority')} invalid={!!errors.priority}>
                            {TRAINING_PRIORITIES.map(priority => (
                              <option key={priority.value} value={priority.value}>{priority.label}</option>
                            ))}
                          </CFormSelect>
                          {errors.priority && <div className="invalid-feedback">{errors.priority.message}</div>}
                        </div>
                      </CCol>
                    </CRow>

                    <div className="mb-3">
                      <CFormLabel htmlFor="description">Description <span className="text-danger">*</span></CFormLabel>
                      <CFormTextarea
                        id="description"
                        rows={4}
                        {...register('description')}
                        invalid={!!errors.description}
                        placeholder="Describe the training content, objectives, and what participants will learn"
                      />
                      {errors.description && <div className="invalid-feedback">{errors.description.message}</div>}
                    </div>

                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="type">Training Type <span className="text-danger">*</span></CFormLabel>
                          <CFormSelect id="type" {...register('type')} invalid={!!errors.type}>
                            {TRAINING_TYPES.map(type => (
                              <option key={type.value} value={type.value}>{type.label}</option>
                            ))}
                          </CFormSelect>
                          {errors.type && <div className="invalid-feedback">{errors.type.message}</div>}
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="category">Category <span className="text-danger">*</span></CFormLabel>
                          <CFormSelect id="category" {...register('category')} invalid={!!errors.category}>
                            {TRAINING_CATEGORIES.map(category => (
                              <option key={category.value} value={category.value}>{category.label}</option>
                            ))}
                          </CFormSelect>
                          {errors.category && <div className="invalid-feedback">{errors.category.message}</div>}
                        </div>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Schedule & Delivery */}
                <CAccordionItem itemKey="schedule">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={TRAINING_ICONS.schedule} className="me-2" />
                    Schedule & Delivery
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="deliveryMethod">Delivery Method <span className="text-danger">*</span></CFormLabel>
                          <CFormSelect id="deliveryMethod" {...register('deliveryMethod')} invalid={!!errors.deliveryMethod}>
                            {DELIVERY_METHODS.map(method => (
                              <option key={method.value} value={method.value}>{method.label}</option>
                            ))}
                          </CFormSelect>
                          {errors.deliveryMethod && <div className="invalid-feedback">{errors.deliveryMethod.message}</div>}
                        </div>
                      </CCol>
                      <CCol md={3}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="scheduledStartDate">Start Date <span className="text-danger">*</span></CFormLabel>
                          <CFormInput
                            id="scheduledStartDate"
                            type="date"
                            {...register('scheduledStartDate')}
                            invalid={!!errors.scheduledStartDate}
                          />
                          {errors.scheduledStartDate && <div className="invalid-feedback">{errors.scheduledStartDate.message}</div>}
                        </div>
                      </CCol>
                      <CCol md={3}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="scheduledEndDate">End Date <span className="text-danger">*</span></CFormLabel>
                          <CFormInput
                            id="scheduledEndDate"
                            type="date"
                            {...register('scheduledEndDate')}
                            invalid={!!errors.scheduledEndDate}
                          />
                          {errors.scheduledEndDate && <div className="invalid-feedback">{errors.scheduledEndDate.message}</div>}
                        </div>
                      </CCol>
                    </CRow>

                    {watchDeliveryMethod !== 'Online' && (
                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="venue">Venue</CFormLabel>
                            <CFormInput
                              id="venue"
                              {...register('venue')}
                              invalid={!!errors.venue}
                              placeholder="Training room, conference hall, etc."
                            />
                            {errors.venue && <div className="invalid-feedback">{errors.venue.message}</div>}
                          </div>
                        </CCol>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="venueAddress">Venue Address</CFormLabel>
                            <CFormInput
                              id="venueAddress"
                              {...register('venueAddress')}
                              placeholder="Full address of the training venue"
                            />
                          </div>
                        </CCol>
                      </CRow>
                    )}

                    {(watchDeliveryMethod === 'Online' || watchDeliveryMethod === 'Hybrid') && (
                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="onlinePlatform">Online Platform</CFormLabel>
                            <CFormInput
                              id="onlinePlatform"
                              {...register('onlinePlatform')}
                              placeholder="Zoom, Teams, Google Meet, etc."
                            />
                          </div>
                        </CCol>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="onlineLink">Online Link</CFormLabel>
                            <CFormInput
                              id="onlineLink"
                              {...register('onlineLink')}
                              placeholder="Meeting URL or platform link"
                            />
                          </div>
                        </CCol>
                      </CRow>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* Participants & Instructor */}
                <CAccordionItem itemKey="participants">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={TRAINING_ICONS.participants} className="me-2" />
                    Participants & Instructor
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={3}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="minParticipants">Min Participants <span className="text-danger">*</span></CFormLabel>
                          <CFormInput
                            id="minParticipants"
                            type="number"
                            min="1"
                            {...register('minParticipants')}
                            invalid={!!errors.minParticipants}
                          />
                          {errors.minParticipants && <div className="invalid-feedback">{errors.minParticipants.message}</div>}
                        </div>
                      </CCol>
                      <CCol md={3}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="maxParticipants">Max Participants <span className="text-danger">*</span></CFormLabel>
                          <CFormInput
                            id="maxParticipants"
                            type="number"
                            min="1"
                            {...register('maxParticipants')}
                            invalid={!!errors.maxParticipants}
                          />
                          {errors.maxParticipants && <div className="invalid-feedback">{errors.maxParticipants.message}</div>}
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="instructorName">Instructor Name</CFormLabel>
                          <CFormInput
                            id="instructorName"
                            {...register('instructorName')}
                            invalid={!!errors.instructorName}
                            placeholder="Primary instructor's name"
                          />
                          {errors.instructorName && <div className="invalid-feedback">{errors.instructorName.message}</div>}
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="instructorQualifications">Instructor Qualifications</CFormLabel>
                          <CFormTextarea
                            id="instructorQualifications"
                            rows={3}
                            {...register('instructorQualifications')}
                            placeholder="Instructor's certifications, experience, and qualifications"
                          />
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="instructorContact">Instructor Contact</CFormLabel>
                          <CFormInput
                            id="instructorContact"
                            {...register('instructorContact')}
                            placeholder="Phone number or email"
                          />
                        </div>
                      </CCol>
                      <CCol md={2}>
                        <div className="mb-3">
                          <CFormLabel>&nbsp;</CFormLabel>
                          <CFormCheck
                            id="isExternalInstructor"
                            label="External Instructor"
                            {...register('isExternalInstructor')}
                          />
                        </div>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Certification */}
                <CAccordionItem itemKey="certification">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={TRAINING_ICONS.certification} className="me-2" />
                    Certification & Assessment
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <CFormCheck
                        id="requiresCertification"
                        label="This training provides certification"
                        {...register('requiresCertification')}
                      />
                    </div>

                    {watchRequiresCertification && (
                      <>
                        <CRow>
                          <CCol md={4}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="certificationType">Certificate Type <span className="text-danger">*</span></CFormLabel>
                              <CFormSelect id="certificationType" {...register('certificationType')} invalid={!!errors.certificationType}>
                                {CERTIFICATE_TYPES.map(type => (
                                  <option key={type.value} value={type.value}>{type.label}</option>
                                ))}
                              </CFormSelect>
                              {errors.certificationType && <div className="invalid-feedback">{errors.certificationType.message}</div>}
                            </div>
                          </CCol>
                          <CCol md={4}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="certificateValidityPeriod">Validity Period</CFormLabel>
                              <CFormSelect id="certificateValidityPeriod" {...register('certificateValidityPeriod')}>
                                {VALIDITY_PERIODS.map(period => (
                                  <option key={period.value} value={period.value}>{period.label}</option>
                                ))}
                              </CFormSelect>
                            </div>
                          </CCol>
                          <CCol md={4}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="assessmentMethod">Assessment Method</CFormLabel>
                              <CFormSelect id="assessmentMethod" {...register('assessmentMethod')}>
                                {ASSESSMENT_METHODS.map(method => (
                                  <option key={method.value} value={method.value}>{method.label}</option>
                                ))}
                              </CFormSelect>
                            </div>
                          </CCol>
                        </CRow>

                        <CRow>
                          <CCol md={6}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="certifyingBody">Certifying Body <span className="text-danger">*</span></CFormLabel>
                              <CFormInput
                                id="certifyingBody"
                                {...register('certifyingBody')}
                                invalid={!!errors.certifyingBody}
                                placeholder="Organization issuing the certificate"
                              />
                              {errors.certifyingBody && <div className="invalid-feedback">{errors.certifyingBody.message}</div>}
                            </div>
                          </CCol>
                          <CCol md={6}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="passingScore">Passing Score (%) <span className="text-danger">*</span></CFormLabel>
                              <CFormInput
                                id="passingScore"
                                type="number"
                                min="0"
                                max="100"
                                {...register('passingScore')}
                                invalid={!!errors.passingScore}
                                placeholder="Minimum score to pass (0-100)"
                              />
                              {errors.passingScore && <div className="invalid-feedback">{errors.passingScore.message}</div>}
                            </div>
                          </CCol>
                        </CRow>
                      </>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* Indonesian K3 Compliance */}
                <CAccordionItem itemKey="compliance">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={TRAINING_ICONS.compliance} className="me-2" />
                    Indonesian K3 Compliance
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <CFormCheck
                        id="isK3MandatoryTraining"
                        label="This is mandatory K3 training"
                        {...register('isK3MandatoryTraining')}
                      />
                    </div>

                    {watchIsK3Training && (
                      <>
                        <CRow>
                          <CCol md={6}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="k3RegulationReference">K3 Regulation Reference</CFormLabel>
                              <CFormInput
                                id="k3RegulationReference"
                                {...register('k3RegulationReference')}
                                placeholder="e.g., Permenaker No. 02/MEN/1992"
                              />
                            </div>
                          </CCol>
                          <CCol md={6}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="skkniReference">SKKNI Reference</CFormLabel>
                              <CFormInput
                                id="skkniReference"
                                {...register('skkniReference')}
                                placeholder="SKKNI standard reference"
                              />
                            </div>
                          </CCol>
                        </CRow>

                        <CRow>
                          <CCol md={6}>
                            <div className="mb-3">
                              <CFormCheck
                                id="isBPJSCompliant"
                                label="BPJS Compliant"
                                {...register('isBPJSCompliant')}
                              />
                            </div>
                          </CCol>
                          <CCol md={6}>
                            <div className="mb-3">
                              <CFormCheck
                                id="requiresGovernmentCertification"
                                label="Requires Government Certification"
                                {...register('requiresGovernmentCertification')}
                              />
                            </div>
                          </CCol>
                        </CRow>

                        {watch('isBPJSCompliant') && (
                          <div className="mb-3">
                            <CFormLabel htmlFor="bpjsReference">BPJS Reference</CFormLabel>
                            <CFormInput
                              id="bpjsReference"
                              {...register('bpjsReference')}
                              placeholder="BPJS regulation or reference number"
                            />
                          </div>
                        )}
                      </>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* Training Content */}
                <CAccordionItem itemKey="content">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={TRAINING_ICONS.trainingDetails} className="me-2" />
                    Training Content & Materials
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <CFormLabel htmlFor="learningObjectives">Learning Objectives</CFormLabel>
                      <CFormTextarea
                        id="learningObjectives"
                        rows={4}
                        {...register('learningObjectives')}
                        placeholder="What will participants learn and be able to do after this training?"
                      />
                    </div>

                    <div className="mb-3">
                      <CFormLabel htmlFor="courseOutline">Course Outline</CFormLabel>
                      <CFormTextarea
                        id="courseOutline"
                        rows={6}
                        {...register('courseOutline')}
                        placeholder="Detailed course schedule and topics (Day 1: Topic A, Day 2: Topic B, etc.)"
                      />
                    </div>

                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="prerequisites">Prerequisites</CFormLabel>
                          <CFormTextarea
                            id="prerequisites"
                            rows={3}
                            {...register('prerequisites')}
                            placeholder="Required qualifications, prior training, or experience"
                          />
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="materials">Materials Provided</CFormLabel>
                          <CFormTextarea
                            id="materials"
                            rows={3}
                            {...register('materials')}
                            placeholder="Training materials, handouts, equipment provided"
                          />
                        </div>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Attachments */}
                <CAccordionItem itemKey="attachments">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={TRAINING_ICONS.attachments} className="me-2" />
                    Supporting Documents & Materials ({pendingAttachments.length})
                  </CAccordionHeader>
                  <CAccordionBody>
                    <TrainingAttachmentManager
                      attachments={[]}
                      onAttachmentsChange={setPendingAttachments}
                      allowUpload={true}
                      allowDelete={true}
                      readonly={false}
                    />
                  </CAccordionBody>
                </CAccordionItem>

              </CAccordion>
            </CForm>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default CreateTraining;