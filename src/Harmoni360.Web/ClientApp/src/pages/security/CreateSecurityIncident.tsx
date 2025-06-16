import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CForm,
  CFormInput,
  CFormTextarea,
  CFormSelect,
  CFormLabel,
  CFormFeedback,
  CButton,
  CAlert,
  CSpinner,
  CInputGroup,
  CInputGroupText,
  CCallout,
  CBadge,
  CAccordion,
  CAccordionBody,
  CAccordionHeader,
  CAccordionItem,
  CFormCheck,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faMapPin } from '@fortawesome/free-solid-svg-icons';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import {
  useCreateSecurityIncidentMutation,
} from '../../features/security/securityApi';
import {
  CreateSecurityIncidentRequest,
  SecurityIncidentType,
  SecurityIncidentCategory,
  SecuritySeverity,
  SecurityImpact,
  ThreatActorType,
} from '../../types/security';

interface SecurityIncidentFormData {
  title: string;
  description: string;
  incidentType: SecurityIncidentType;
  category: SecurityIncidentCategory;
  severity: SecuritySeverity;
  incidentDateTime: string;
  location: string;
  impact: SecurityImpact;
  latitude?: number;
  longitude?: number;
  threatActorType?: ThreatActorType;
  threatActorDescription?: string;
  isInternalThreat: boolean;
  dataBreachSuspected: boolean;
  affectedPersonsCount?: number;
  estimatedLoss?: number;
  containmentActions?: string;
  detectionDateTime?: string;
  assignedToId?: number;
  investigatorId?: number;
}

// Validation schema for Security Incident creation
const schema: yup.ObjectSchema<SecurityIncidentFormData> = yup.object({
  title: yup
    .string()
    .required('Security incident title is required')
    .min(5, 'Title must be at least 5 characters')
    .max(200, 'Title must not exceed 200 characters'),
  description: yup
    .string()
    .required('Description is required')
    .min(10, 'Description must be at least 10 characters')
    .max(2000, 'Description must not exceed 2000 characters'),
  incidentType: yup
    .number()
    .required('Incident type is required')
    .oneOf([1, 2, 3, 4], 'Please select a valid incident type') as yup.Schema<SecurityIncidentType>,
  category: yup
    .number()
    .required('Incident category is required') as yup.Schema<SecurityIncidentCategory>,
  severity: yup
    .number()
    .required('Severity level is required')
    .oneOf([1, 2, 3, 4], 'Please select a valid severity level') as yup.Schema<SecuritySeverity>,
  incidentDateTime: yup.string().required('Incident date and time is required'),
  location: yup
    .string()
    .required('Location is required')
    .min(3, 'Location must be at least 3 characters'),
  impact: yup
    .number()
    .required('Impact level is required')
    .oneOf([0, 1, 2, 3, 4], 'Please select a valid impact level') as yup.Schema<SecurityImpact>,
  latitude: yup.number().optional().min(-90).max(90),
  longitude: yup.number().optional().min(-180).max(180),
  threatActorType: yup.number().optional() as yup.Schema<ThreatActorType | undefined>,
  threatActorDescription: yup
    .string()
    .optional()
    .max(500, 'Threat actor description must not exceed 500 characters'),
  isInternalThreat: yup.boolean().required(),
  dataBreachSuspected: yup.boolean().required(),
  affectedPersonsCount: yup
    .number()
    .optional()
    .min(0, 'Affected persons count cannot be negative'),
  estimatedLoss: yup
    .number()
    .optional()
    .min(0, 'Estimated loss cannot be negative'),
  containmentActions: yup
    .string()
    .optional()
    .max(1000, 'Containment actions must not exceed 1000 characters'),
  detectionDateTime: yup.string().optional(),
  assignedToId: yup.number().optional(),
  investigatorId: yup.number().optional(),
});

const CreateSecurityIncident: React.FC = () => {
  const navigate = useNavigate();
  const [createSecurityIncident, { isLoading: isSubmitting }] =
    useCreateSecurityIncidentMutation();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [locationLoading, setLocationLoading] = useState(false);
  const [selectedIncidentType, setSelectedIncidentType] = useState<SecurityIncidentType | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset,
  } = useForm<SecurityIncidentFormData>({
    resolver: yupResolver(schema),
    defaultValues: {
      incidentDateTime: new Date().toISOString().slice(0, 16), // Default to current time
      isInternalThreat: false,
      dataBreachSuspected: false,
    },
  });

  const watchedIncidentType = watch('incidentType');
  const watchedIsInternalThreat = watch('isInternalThreat');
  const watchedDataBreachSuspected = watch('dataBreachSuspected');

  // Update selected incident type when form value changes
  useEffect(() => {
    setSelectedIncidentType(watchedIncidentType);
  }, [watchedIncidentType]);

  // Get current location
  const getCurrentLocation = () => {
    setLocationLoading(true);
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          setValue('latitude', position.coords.latitude);
          setValue('longitude', position.coords.longitude);
          setLocationLoading(false);
        },
        (error) => {
          console.error('Error getting location:', error);
          setLocationLoading(false);
        }
      );
    } else {
      setLocationLoading(false);
    }
  };

  // Get category options based on incident type
  const getCategoryOptions = (type: SecurityIncidentType) => {
    switch (type) {
      case SecurityIncidentType.PhysicalSecurity:
        return [
          { value: 101, label: 'Unauthorized Access' },
          { value: 102, label: 'Theft' },
          { value: 103, label: 'Vandalism' },
          { value: 104, label: 'Perimeter Breach' },
          { value: 105, label: 'Suspicious Activity' },
          { value: 106, label: 'Physical Threat' },
        ];
      case SecurityIncidentType.Cybersecurity:
        return [
          { value: 201, label: 'Data Breach' },
          { value: 202, label: 'Malware Infection' },
          { value: 203, label: 'Phishing Attempt' },
          { value: 204, label: 'System Intrusion' },
          { value: 205, label: 'Service Disruption' },
          { value: 206, label: 'Unauthorized Change' },
        ];
      case SecurityIncidentType.PersonnelSecurity:
        return [
          { value: 301, label: 'Background Check Failure' },
          { value: 302, label: 'Policy Violation' },
          { value: 303, label: 'Insider Threat' },
          { value: 304, label: 'Credential Misuse' },
          { value: 305, label: 'Security Training Failure' },
        ];
      case SecurityIncidentType.InformationSecurity:
        return [
          { value: 201, label: 'Data Breach' },
          { value: 206, label: 'Unauthorized Change' },
          { value: 304, label: 'Credential Misuse' },
        ];
      default:
        return [];
    }
  };

  const onSubmit = async (data: SecurityIncidentFormData) => {
    try {
      setSubmitError(null);
      
      const request: CreateSecurityIncidentRequest = {
        incidentType: data.incidentType,
        category: data.category,
        title: data.title,
        description: data.description,
        severity: data.severity,
        incidentDateTime: data.incidentDateTime,
        location: data.location,
        latitude: data.latitude,
        longitude: data.longitude,
        threatActorType: data.threatActorType,
        threatActorDescription: data.threatActorDescription,
        isInternalThreat: data.isInternalThreat,
        dataBreachSuspected: data.dataBreachSuspected,
        affectedPersonsCount: data.affectedPersonsCount,
        estimatedLoss: data.estimatedLoss,
        impact: data.impact,
        assignedToId: data.assignedToId,
        investigatorId: data.investigatorId,
        containmentActions: data.containmentActions,
        detectionDateTime: data.detectionDateTime,
      };

      const result = await createSecurityIncident(request).unwrap();
      
      navigate(`/security/incidents/${result.id}`, {
        state: { message: 'Security incident created successfully!' },
      });
    } catch (error: any) {
      console.error('Failed to create security incident:', error);
      setSubmitError(
        error?.data?.message || 'Failed to create security incident. Please try again.'
      );
    }
  };

  const categoryOptions = selectedIncidentType ? getCategoryOptions(selectedIncidentType) : [];

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0">Report Security Incident</h4>
              <small className="text-muted">
                Create a new security incident report
              </small>
            </div>
            <CButton
              color="secondary"
              variant="outline"
              onClick={() => navigate('/security/incidents')}
            >
              <FontAwesomeIcon icon={ACTION_ICONS.back} className="me-2" />
              Back to List
            </CButton>
          </CCardHeader>

          <CCardBody>
            {submitError && (
              <CAlert color="danger" dismissible onClose={() => setSubmitError(null)}>
                {submitError}
              </CAlert>
            )}

            <CForm onSubmit={handleSubmit(onSubmit)}>
              <CAccordion activeItemKey="basic" alwaysOpen>
                {/* Basic Information */}
                <CAccordionItem itemKey="basic">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={CONTEXT_ICONS.incident} className="me-2" />
                    Basic Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="incidentType">
                          Incident Type <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormSelect
                          id="incidentType"
                          {...register('incidentType', { valueAsNumber: true })}
                          invalid={!!errors.incidentType}
                        >
                          <option value="">Select incident type</option>
                          <option value={SecurityIncidentType.PhysicalSecurity}>Physical Security</option>
                          <option value={SecurityIncidentType.Cybersecurity}>Cybersecurity</option>
                          <option value={SecurityIncidentType.PersonnelSecurity}>Personnel Security</option>
                          <option value={SecurityIncidentType.InformationSecurity}>Information Security</option>
                        </CFormSelect>
                        <CFormFeedback invalid>{errors.incidentType?.message}</CFormFeedback>
                      </CCol>

                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="category">
                          Category <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormSelect
                          id="category"
                          {...register('category', { valueAsNumber: true })}
                          invalid={!!errors.category}
                          disabled={!selectedIncidentType}
                        >
                          <option value="">Select category</option>
                          {categoryOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                              {option.label}
                            </option>
                          ))}
                        </CFormSelect>
                        <CFormFeedback invalid>{errors.category?.message}</CFormFeedback>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={12} className="mb-3">
                        <CFormLabel htmlFor="title">
                          Incident Title <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormInput
                          type="text"
                          id="title"
                          placeholder="Brief, clear title describing the security incident"
                          {...register('title')}
                          invalid={!!errors.title}
                        />
                        <CFormFeedback invalid>{errors.title?.message}</CFormFeedback>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={12} className="mb-3">
                        <CFormLabel htmlFor="description">
                          Description <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormTextarea
                          id="description"
                          rows={4}
                          placeholder="Detailed description of what happened, when, and any immediate observations..."
                          {...register('description')}
                          invalid={!!errors.description}
                        />
                        <CFormFeedback invalid>{errors.description?.message}</CFormFeedback>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={4} className="mb-3">
                        <CFormLabel htmlFor="severity">
                          Severity <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormSelect
                          id="severity"
                          {...register('severity', { valueAsNumber: true })}
                          invalid={!!errors.severity}
                        >
                          <option value="">Select severity</option>
                          <option value={SecuritySeverity.Low}>Low</option>
                          <option value={SecuritySeverity.Medium}>Medium</option>
                          <option value={SecuritySeverity.High}>High</option>
                          <option value={SecuritySeverity.Critical}>Critical</option>
                        </CFormSelect>
                        <CFormFeedback invalid>{errors.severity?.message}</CFormFeedback>
                      </CCol>

                      <CCol md={4} className="mb-3">
                        <CFormLabel htmlFor="impact">
                          Impact Level <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormSelect
                          id="impact"
                          {...register('impact', { valueAsNumber: true })}
                          invalid={!!errors.impact}
                        >
                          <option value="">Select impact</option>
                          <option value={SecurityImpact.None}>None</option>
                          <option value={SecurityImpact.Minor}>Minor</option>
                          <option value={SecurityImpact.Moderate}>Moderate</option>
                          <option value={SecurityImpact.Major}>Major</option>
                          <option value={SecurityImpact.Severe}>Severe</option>
                        </CFormSelect>
                        <CFormFeedback invalid>{errors.impact?.message}</CFormFeedback>
                      </CCol>

                      <CCol md={4} className="mb-3">
                        <CFormLabel htmlFor="incidentDateTime">
                          Incident Date & Time <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormInput
                          type="datetime-local"
                          id="incidentDateTime"
                          {...register('incidentDateTime')}
                          invalid={!!errors.incidentDateTime}
                        />
                        <CFormFeedback invalid>{errors.incidentDateTime?.message}</CFormFeedback>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Location Information */}
                <CAccordionItem itemKey="location">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faMapPin} className="me-2" />
                    Location Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={8} className="mb-3">
                        <CFormLabel htmlFor="location">
                          Location <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormInput
                          type="text"
                          id="location"
                          placeholder="Specific location where the incident occurred"
                          {...register('location')}
                          invalid={!!errors.location}
                        />
                        <CFormFeedback invalid>{errors.location?.message}</CFormFeedback>
                      </CCol>
                      <CCol md={4} className="mb-3">
                        <CFormLabel>&nbsp;</CFormLabel>
                        <div>
                          <CButton
                            type="button"
                            color="secondary"
                            variant="outline"
                            onClick={getCurrentLocation}
                            disabled={locationLoading}
                            className="w-100"
                          >
                            {locationLoading ? (
                              <CSpinner size="sm" />
                            ) : (
                              <FontAwesomeIcon icon={faMapPin} />
                            )}
                            <span className="ms-2">Get GPS Location</span>
                          </CButton>
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="latitude">Latitude</CFormLabel>
                        <CFormInput
                          type="number"
                          id="latitude"
                          step="any"
                          placeholder="e.g., -6.1751"
                          {...register('latitude', { valueAsNumber: true })}
                          invalid={!!errors.latitude}
                        />
                        <CFormFeedback invalid>{errors.latitude?.message}</CFormFeedback>
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="longitude">Longitude</CFormLabel>
                        <CFormInput
                          type="number"
                          id="longitude"
                          step="any"
                          placeholder="e.g., 106.8650"
                          {...register('longitude', { valueAsNumber: true })}
                          invalid={!!errors.longitude}
                        />
                        <CFormFeedback invalid>{errors.longitude?.message}</CFormFeedback>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="detectionDateTime">Detection Date & Time</CFormLabel>
                        <CFormInput
                          type="datetime-local"
                          id="detectionDateTime"
                          {...register('detectionDateTime')}
                          invalid={!!errors.detectionDateTime}
                        />
                        <small className="text-muted">When the incident was first detected (if different from incident time)</small>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Threat Information */}
                <CAccordionItem itemKey="threat">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={CONTEXT_ICONS.hazard} className="me-2" />
                    Threat Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="threatActorType">Threat Actor Type</CFormLabel>
                        <CFormSelect
                          id="threatActorType"
                          {...register('threatActorType', { valueAsNumber: true })}
                        >
                          <option value="">Select threat actor type</option>
                          <option value={ThreatActorType.External}>External</option>
                          <option value={ThreatActorType.Internal}>Internal</option>
                          <option value={ThreatActorType.Partner}>Partner/Third-party</option>
                          <option value={ThreatActorType.Unknown}>Unknown</option>
                        </CFormSelect>
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <div className="mt-4">
                          <CFormCheck
                            id="isInternalThreat"
                            label="Internal threat suspected"
                            {...register('isInternalThreat')}
                          />
                          <CFormCheck
                            id="dataBreachSuspected"
                            label="Data breach suspected"
                            {...register('dataBreachSuspected')}
                            className="mt-2"
                          />
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={12} className="mb-3">
                        <CFormLabel htmlFor="threatActorDescription">Threat Actor Description</CFormLabel>
                        <CFormTextarea
                          id="threatActorDescription"
                          rows={3}
                          placeholder="Describe what is known about the threat actor (behavior, capabilities, motives, etc.)"
                          {...register('threatActorDescription')}
                          invalid={!!errors.threatActorDescription}
                        />
                        <CFormFeedback invalid>{errors.threatActorDescription?.message}</CFormFeedback>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Impact Assessment */}
                <CAccordionItem itemKey="impact">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={CONTEXT_ICONS.incident} className="me-2" />
                    Impact Assessment
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="affectedPersonsCount">Affected Persons Count</CFormLabel>
                        <CFormInput
                          type="number"
                          id="affectedPersonsCount"
                          min="0"
                          placeholder="Number of people affected"
                          {...register('affectedPersonsCount', { valueAsNumber: true })}
                          invalid={!!errors.affectedPersonsCount}
                        />
                        <CFormFeedback invalid>{errors.affectedPersonsCount?.message}</CFormFeedback>
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="estimatedLoss">Estimated Loss (USD)</CFormLabel>
                        <CInputGroup>
                          <CInputGroupText>$</CInputGroupText>
                          <CFormInput
                            type="number"
                            id="estimatedLoss"
                            min="0"
                            step="0.01"
                            placeholder="0.00"
                            {...register('estimatedLoss', { valueAsNumber: true })}
                            invalid={!!errors.estimatedLoss}
                          />
                        </CInputGroup>
                        <CFormFeedback invalid>{errors.estimatedLoss?.message}</CFormFeedback>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Initial Response */}
                <CAccordionItem itemKey="response">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={ACTION_ICONS.create} className="me-2" />
                    Initial Response
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={12} className="mb-3">
                        <CFormLabel htmlFor="containmentActions">Immediate Containment Actions</CFormLabel>
                        <CFormTextarea
                          id="containmentActions"
                          rows={4}
                          placeholder="Describe any immediate actions taken to contain or mitigate the incident..."
                          {...register('containmentActions')}
                          invalid={!!errors.containmentActions}
                        />
                        <CFormFeedback invalid>{errors.containmentActions?.message}</CFormFeedback>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>
              </CAccordion>

              {/* Form Actions */}
              <div className="d-flex justify-content-between mt-4">
                <CButton
                  type="button"
                  color="secondary"
                  variant="outline"
                  onClick={() => navigate('/security/incidents')}
                >
                  Cancel
                </CButton>
                <div className="d-flex gap-2">
                  <CButton
                    type="button"
                    color="secondary"
                    variant="outline"
                    onClick={() => reset()}
                  >
                    Reset Form
                  </CButton>
                  <CButton
                    type="submit"
                    color="primary"
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? (
                      <>
                        <CSpinner size="sm" className="me-2" />
                        Creating...
                      </>
                    ) : (
                      <>
                        <FontAwesomeIcon icon={ACTION_ICONS.create} className="me-2" />
                        Create Security Incident
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
  );
};

export default CreateSecurityIncident;