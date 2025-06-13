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
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CBadge
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSave,
  faArrowLeft,
  faInfoCircle,
  faExclamationTriangle,
  faShieldAlt,
  faUsers,
  faMapMarkerAlt,
  faCheck,
  faPlus,
  faTrash,
  faFileContract,
  faCog,
  faIndustry,
  faFileUpload
} from '@fortawesome/free-solid-svg-icons';

import { useCreateWorkPermitMutation, useUploadAttachmentMutation } from '../../features/work-permits/workPermitApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import {
  WorkPermitFormData,
  WORK_PERMIT_TYPES,
  WORK_PERMIT_PRIORITIES,
  RISK_LEVELS,
  HazardCategory,
  PrecautionCategory,
  WorkPermitHazardDto,
  WorkPermitPrecautionDto
} from '../../types/workPermit';
import { WorkPermitAttachmentManager } from '../../components/work-permits';
import { format, addDays } from 'date-fns';

// Validation schema
const schema = yup.object({
  title: yup.string().required('Title is required').max(200, 'Title must not exceed 200 characters'),
  description: yup.string().required('Description is required').max(1000, 'Description must not exceed 1000 characters'),
  workLocation: yup.string().required('Work location is required').max(200, 'Work location must not exceed 200 characters'),
  plannedStartDate: yup.string().required('Start date is required'),
  plannedEndDate: yup.string()
    .required('End date is required')
    .test('is-after-start', 'End date must be after start date', function(value) {
      const { plannedStartDate } = this.parent;
      if (!value || !plannedStartDate) return true;
      
      // Create dates at start of day to compare just the dates
      const startDate = new Date(plannedStartDate + 'T00:00:00');
      const endDate = new Date(value + 'T00:00:00');
      
      // Must be strictly after start date (backend requirement)
      return endDate > startDate;
    }),
  workScope: yup.string().required('Work scope is required').max(1000, 'Work scope must not exceed 1000 characters'),
  numberOfWorkers: yup.number().required('Number of workers is required').min(1, 'Must have at least 1 worker'),
  contactPhone: yup.string().required('Contact phone is required').max(20, 'Phone number must not exceed 20 characters'),
  workSupervisor: yup.string().required('Work supervisor is required').max(100, 'Supervisor name must not exceed 100 characters'),
  riskAssessmentSummary: yup.string().required('Risk assessment summary is required').max(2000, 'Risk assessment must not exceed 2000 characters'),
  emergencyProcedures: yup.string().when(['type', 'riskLevel', 'requiresHotWorkPermit', 'requiresConfinedSpaceEntry', 'requiresRadiationWork'], {
    is: (type: string, riskLevel: string, requiresHotWorkPermit: boolean, requiresConfinedSpaceEntry: boolean, requiresRadiationWork: boolean) => 
      ['Special', 'HotWork', 'ConfinedSpace'].includes(type) || 
      ['High', 'Critical'].includes(riskLevel) ||
      requiresHotWorkPermit || requiresConfinedSpaceEntry || requiresRadiationWork,
    then: (schema) => schema.required('Emergency procedures are required for high-risk work'),
    otherwise: (schema) => schema
  }),
});

// Work Permit Icon Mappings
const WORKPERMIT_ICONS = {
  // Main work permit icon
  workPermit: faFileContract,
  
  // Section icons following Harmoni360 standards
  basicInformation: faInfoCircle,
  workDetails: faUsers,
  safetyRequirements: faShieldAlt,
  k3Compliance: faCheck,
  riskAssessment: faExclamationTriangle,
  attachments: faFileUpload,
  reviewSubmit: faCheck,
  
  // Action icons
  create: faPlus,
  save: faSave,
  back: faArrowLeft,
  location: faMapMarkerAlt,
  settings: faCog,
  industry: faIndustry
};

const CreateWorkPermit: React.FC = () => {
  const navigate = useNavigate();

  // API calls
  const { data: departments } = useGetDepartmentsQuery({});
  const [createWorkPermit, { isLoading }] = useCreateWorkPermitMutation();
  const [uploadAttachment] = useUploadAttachmentMutation();

  // Form state
  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
    getValues,
  } = useForm<WorkPermitFormData>({
    resolver: yupResolver(schema) as any,
    defaultValues: {
      title: '',
      description: '',
      type: 'General',
      priority: 'Medium',
      workLocation: '',
      plannedStartDate: format(new Date(), 'yyyy-MM-dd'),
      plannedEndDate: format(addDays(new Date(), 1), 'yyyy-MM-dd'), // Next day to ensure end > start
      estimatedDuration: 8,
      contactPhone: '',
      workSupervisor: '',
      safetyOfficer: '',
      workScope: '',
      equipmentToBeUsed: '',
      materialsInvolved: '',
      numberOfWorkers: 1,
      contractorCompany: '',
      requiresHotWorkPermit: false,
      requiresConfinedSpaceEntry: false,
      requiresElectricalIsolation: false,
      requiresHeightWork: false,
      requiresRadiationWork: false,
      requiresExcavation: false,
      requiresFireWatch: false,
      requiresGasMonitoring: false,
      k3LicenseNumber: '',
      companyWorkPermitNumber: '',
      isJamsostekCompliant: false,
      hasSMK3Compliance: false,
      environmentalPermitNumber: '',
      riskLevel: 'Medium',
      riskAssessmentSummary: '',
      emergencyProcedures: '',
      hazards: [],
      precautions: []
    },
  });

  // Additional state for complex data
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [pendingAttachments, setPendingAttachments] = useState<any[]>([]); // For file attachments in create mode
  const [currentHazard, setCurrentHazard] = useState<Partial<WorkPermitHazardDto>>({
    hazardDescription: '',
    category: 'Physical',
    likelihood: 3,
    severity: 3,
    controlMeasures: '',
    responsiblePerson: ''
  });
  const [currentPrecaution, setCurrentPrecaution] = useState<Partial<WorkPermitPrecautionDto>>({
    precautionDescription: '',
    category: 'PersonalProtectiveEquipment',
    isRequired: true,
    priority: 1,
    responsiblePerson: '',
    verificationMethod: '',
    isK3Requirement: false,
    k3StandardReference: '',
    isMandatoryByLaw: false
  });

  // Submit form (following incident pattern exactly)
  const onSubmit = async (data: WorkPermitFormData) => {
    console.log('🚀 onSubmit called with data:', data);
    setSubmitError(null);

    try {
      // Map enum strings to integers for backend
      const typeMapping: Record<string, number> = {
        'General': 0,
        'HotWork': 1,
        'ColdWork': 2,
        'ConfinedSpace': 3,
        'ElectricalWork': 4,
        'Special': 5
      };

      const hazardCategoryMapping: Record<string, number> = {
        'Physical': 0,
        'Chemical': 1,
        'Biological': 2,
        'Ergonomic': 3,
        'Fire': 4,
        'Electrical': 5,
        'Mechanical': 6,
        'Environmental': 7,
        'Radiological': 8,
        'Behavioral': 9
      };

      const precautionCategoryMapping: Record<string, number> = {
        'PersonalProtectiveEquipment': 0,
        'Isolation': 1,
        'FireSafety': 2,
        'GasMonitoring': 3,
        'VentilationControl': 4,
        'AccessControl': 5,
        'EmergencyProcedures': 6,
        'EnvironmentalProtection': 7,
        'TrafficControl': 8,
        'WeatherPrecautions': 9,
        'EquipmentSafety': 10,
        'MaterialHandling': 11,
        'WasteManagement': 12,
        'CommunicationProtocol': 13,
        'K3_Compliance': 14,
        'BPJS_Compliance': 15,
        'Environmental_Permit': 16,
        'Other': 17
      };

      // Prepare the request data exactly like incident form
      const command = {
        title: data.title,
        description: data.description,
        type: typeMapping[data.type] ?? 0, // Convert string to enum integer
        workLocation: data.workLocation,
        plannedStartDate: new Date(data.plannedStartDate).toISOString(),
        plannedEndDate: new Date(data.plannedEndDate).toISOString(),
        workScope: data.workScope,
        numberOfWorkers: data.numberOfWorkers,
        contactPhone: data.contactPhone,
        workSupervisor: data.workSupervisor,
        safetyOfficer: data.safetyOfficer || '',
        contractorCompany: data.contractorCompany || '',
        equipmentToBeUsed: data.equipmentToBeUsed || '',
        materialsInvolved: data.materialsInvolved || '',
        latitude: null,
        longitude: null,
        address: '',
        locationDescription: '',
        requiresHotWorkPermit: data.requiresHotWorkPermit || false,
        requiresConfinedSpaceEntry: data.requiresConfinedSpaceEntry || false,
        requiresElectricalIsolation: data.requiresElectricalIsolation || false,
        requiresHeightWork: data.requiresHeightWork || false,
        requiresRadiationWork: data.requiresRadiationWork || false,
        requiresExcavation: data.requiresExcavation || false,
        requiresFireWatch: data.requiresFireWatch || false,
        requiresGasMonitoring: data.requiresGasMonitoring || false,
        k3LicenseNumber: data.k3LicenseNumber || '',
        companyWorkPermitNumber: data.companyWorkPermitNumber || '',
        isJamsostekCompliant: data.isJamsostekCompliant || false,
        hasSMK3Compliance: data.hasSMK3Compliance || false,
        environmentalPermitNumber: data.environmentalPermitNumber || '',
        riskAssessmentSummary: data.riskAssessmentSummary,
        emergencyProcedures: data.emergencyProcedures || '',
        hazards: data.hazards?.map(hazard => ({
          hazardDescription: hazard.hazardDescription || '',
          category: hazardCategoryMapping[hazard.category || 'Physical'] ?? 0,
          likelihood: hazard.likelihood || 1,
          severity: hazard.severity || 1,
          controlMeasures: hazard.controlMeasures || '',
          responsiblePerson: hazard.responsiblePerson || ''
        })) || [],
        precautions: data.precautions?.map(precaution => ({
          precautionDescription: precaution.precautionDescription || '',
          category: precautionCategoryMapping[precaution.category || 'PersonalProtectiveEquipment'] ?? 0,
          isRequired: precaution.isRequired !== undefined ? precaution.isRequired : true,
          priority: precaution.priority || 1,
          responsiblePerson: precaution.responsiblePerson || '',
          verificationMethod: precaution.verificationMethod || '',
          isK3Requirement: precaution.isK3Requirement || false,
          k3StandardReference: precaution.k3StandardReference || '',
          isMandatoryByLaw: precaution.isMandatoryByLaw || false
        })) || []
      };

      // Submit to API
      const result = await createWorkPermit(command).unwrap();
      console.log('✅ Work permit created:', result);

      // Upload pending attachments if any
      if (pendingAttachments.length > 0 && result.id) {
        console.log(`📎 Uploading ${pendingAttachments.length} attachments...`);
        try {
          for (const attachment of pendingAttachments) {
            await uploadAttachment({
              workPermitId: result.id.toString(),
              file: attachment.file,
              attachmentType: attachment.attachmentType,
              description: attachment.description,
            }).unwrap();
          }
          console.log('✅ All attachments uploaded successfully');
        } catch (uploadError) {
          console.error('❌ Failed to upload some attachments:', uploadError);
          // Continue anyway - the work permit was created successfully
        }
      }

      // Navigate to work permits list with success message
      navigate('/work-permits', {
        state: {
          message: `Work permit created successfully!${pendingAttachments.length > 0 ? ` ${pendingAttachments.length} attachment(s) uploaded.` : ''}`,
          type: 'success',
        },
      });
    } catch (error: any) {
      console.error('Failed to create work permit:', error);
      
      // Extract error message
      let errorMessage = 'Failed to create work permit. Please try again.';
      if (error?.data?.message) {
        errorMessage = error.data.message;
      } else if (error?.data?.title) {
        errorMessage = error.data.title;
      } else if (error?.message) {
        errorMessage = error.message;
      }
      
      setSubmitError(errorMessage);
    }
  };

  // Watch form values for dynamic updates
  const watchType = watch('type');
  const watchRiskLevel = watch('riskLevel');
  const watchRequiresHotWorkPermit = watch('requiresHotWorkPermit');
  const watchRequiresConfinedSpaceEntry = watch('requiresConfinedSpaceEntry');

  // Auto-set safety requirements based on work type
  React.useEffect(() => {
    if (watchType === 'HotWork' || watchRequiresHotWorkPermit) {
      setValue('requiresFireWatch', true);
    }
    if (watchType === 'ConfinedSpace' || watchRequiresConfinedSpaceEntry) {
      setValue('requiresGasMonitoring', true);
    }
  }, [watchType, watchRequiresHotWorkPermit, watchRequiresConfinedSpaceEntry, setValue]);

  const addHazard = useCallback(() => {
    if (!currentHazard.hazardDescription?.trim()) return;

    const riskScore = (currentHazard.likelihood || 1) * (currentHazard.severity || 1);
    const hazard: Partial<WorkPermitHazardDto> = {
      ...currentHazard,
      id: Math.random().toString(36).substr(2, 9),
      riskScore,
      riskLevel: riskScore <= 6 ? 'Low' : riskScore <= 12 ? 'Medium' : riskScore <= 20 ? 'High' : 'Critical',
      residualRiskLevel: 'Medium',
      isControlImplemented: false
    };

    const currentHazards = getValues('hazards') || [];
    setValue('hazards', [...currentHazards, hazard]);

    setCurrentHazard({
      hazardDescription: '',
      category: 'Physical',
      likelihood: 3,
      severity: 3,
      controlMeasures: '',
      responsiblePerson: ''
    });
  }, [currentHazard, getValues, setValue]);

  const removeHazard = useCallback((index: number) => {
    const currentHazards = getValues('hazards') || [];
    setValue('hazards', currentHazards.filter((_, i) => i !== index));
  }, [getValues, setValue]);

  const addPrecaution = useCallback(() => {
    if (!currentPrecaution.precautionDescription?.trim()) return;
    if (currentPrecaution.isK3Requirement && !currentPrecaution.k3StandardReference?.trim()) {
      alert('K3 Standard Reference is required for K3 compliance requirements.');
      return;
    }

    const precaution: Partial<WorkPermitPrecautionDto> = {
      ...currentPrecaution,
      id: Math.random().toString(36).substr(2, 9),
      isCompleted: false,
      requiresVerification: true,
      isVerified: false
    };

    const currentPrecautions = getValues('precautions') || [];
    setValue('precautions', [...currentPrecautions, precaution]);

    setCurrentPrecaution({
      precautionDescription: '',
      category: 'PersonalProtectiveEquipment',
      isRequired: true,
      priority: 1,
      responsiblePerson: '',
      verificationMethod: '',
      isK3Requirement: false,
      k3StandardReference: '',
      isMandatoryByLaw: false
    });
  }, [currentPrecaution, getValues, setValue]);

  const removePrecaution = useCallback((index: number) => {
    const currentPrecautions = getValues('precautions') || [];
    setValue('precautions', currentPrecautions.filter((_, i) => i !== index));
  }, [getValues, setValue]);


  const getRiskLevelColor = (riskLevel: string) => {
    // Map risk levels to CoreUI color system
    switch (riskLevel) {
      case 'Low':
        return 'success';
      case 'Medium':
        return 'warning';
      case 'High':
        return 'danger';
      case 'Critical':
        return 'danger';
      default:
        return 'secondary';
    }
  };

  return (
    <CRow>
      <CCol>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0">
                <FontAwesomeIcon icon={WORKPERMIT_ICONS.workPermit} size="lg" className="me-2 text-primary" />
                Create Work Permit
              </h4>
              <small className="text-muted">Complete all required sections to create a new work permit</small>
            </div>
            <div className="d-flex align-items-center gap-2">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate('/work-permits')}
              >
                <FontAwesomeIcon icon={WORKPERMIT_ICONS.back} className="me-2" />
                Back to List
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            <CForm onSubmit={handleSubmit(onSubmit, (errors) => console.error('❌ Form validation errors:', errors))}>
              {submitError && (
                <CAlert
                  color="danger"
                  dismissible
                  onClose={() => setSubmitError(null)}
                >
                  {submitError}
                </CAlert>
              )}
              
              {/* Debug: Show current form errors */}
              {Object.keys(errors).length > 0 && (
                <CAlert color="warning">
                  <strong>Form Validation Errors:</strong>
                  <ul>
                    {Object.entries(errors).map(([field, error]) => (
                      <li key={field}>
                        <strong>{field}:</strong> {(error as any)?.message || 'Invalid'}
                      </li>
                    ))}
                  </ul>
                </CAlert>
              )}
              
              <CAccordion>
                {/* Section 1: Basic Information */}
                <CAccordionItem itemKey={1}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.basicInformation} className="me-2 text-primary" />
                      <strong>Basic Information</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="title">Work Permit Title *</CFormLabel>
                        <CFormInput
                          id="title"
                          {...register('title')}
                          invalid={!!errors.title}
                          placeholder="Enter descriptive title for the work"
                        />
                        {errors.title && <div className="invalid-feedback d-block">{errors.title.message}</div>}
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="type">Work Type *</CFormLabel>
                        <CFormSelect
                          id="type"
                          {...register('type')}
                        >
                          {WORK_PERMIT_TYPES.map(type => (
                            <option key={type.value} value={type.value}>
                              {type.label}
                            </option>
                          ))}
                        </CFormSelect>
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="priority">Priority *</CFormLabel>
                        <CFormSelect
                          id="priority"
                          {...register('priority')}
                        >
                          {WORK_PERMIT_PRIORITIES.map(priority => (
                            <option key={priority.value} value={priority.value}>
                              {priority.label}
                            </option>
                          ))}
                        </CFormSelect>
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol>
                        <CFormLabel htmlFor="description">Description *</CFormLabel>
                        <CFormTextarea
                          id="description"
                          rows={3}
                          {...register('description')}
                          invalid={!!errors.description}
                          placeholder="Provide detailed description of the work to be performed"
                        />
                        {errors.description && <div className="invalid-feedback d-block">{errors.description.message}</div>}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="workLocation">Work Location *</CFormLabel>
                        <CFormInput
                          id="workLocation"
                          {...register('workLocation')}
                          invalid={!!errors.workLocation}
                          placeholder="Building, room, area, or specific location"
                        />
                        {errors.workLocation && <div className="invalid-feedback d-block">{errors.workLocation.message}</div>}
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="plannedStartDate">Planned Start Date *</CFormLabel>
                        <CFormInput
                          type="date"
                          id="plannedStartDate"
                          {...register('plannedStartDate')}
                          min={format(new Date(), 'yyyy-MM-dd')}
                        />
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="plannedEndDate">Planned End Date *</CFormLabel>
                        <CFormInput
                          type="date"
                          id="plannedEndDate"
                          {...register('plannedEndDate')}
                          invalid={!!errors.plannedEndDate}
                          min={watch('plannedStartDate')}
                        />
                        {errors.plannedEndDate && <div className="invalid-feedback d-block">{errors.plannedEndDate.message}</div>}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="estimatedDuration">Estimated Duration (hours)</CFormLabel>
                        <CInputGroup>
                          <CFormInput
                            type="number"
                            id="estimatedDuration"
                            {...register('estimatedDuration', { valueAsNumber: true })}
                            min={1}
                            max={8760}
                          />
                          <CInputGroupText>hours</CInputGroupText>
                        </CInputGroup>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Section 2: Work Details */}
                <CAccordionItem itemKey={2}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.workDetails} className="me-2 text-info" />
                      <strong>Work Details & Personnel</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol>
                        <CFormLabel htmlFor="workScope">Work Scope *</CFormLabel>
                        <CFormTextarea
                          id="workScope"
                          rows={3}
                          {...register('workScope')}
                          invalid={!!errors.workScope}
                          placeholder="Detailed description of work activities and procedures"
                        />
                        {errors.workScope && <div className="invalid-feedback d-block">{errors.workScope.message}</div>}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="equipmentToBeUsed">Equipment to be Used</CFormLabel>
                        <CFormTextarea
                          id="equipmentToBeUsed"
                          rows={2}
                          {...register('equipmentToBeUsed')}
                          placeholder="List equipment, tools, and machinery"
                        />
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="materialsInvolved">Materials Involved</CFormLabel>
                        <CFormTextarea
                          id="materialsInvolved"
                          rows={2}
                          {...register('materialsInvolved')}
                          placeholder="List chemicals, substances, and materials"
                        />
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={4}>
                        <CFormLabel htmlFor="numberOfWorkers">Number of Workers *</CFormLabel>
                        <CFormInput
                          type="number"
                          id="numberOfWorkers"
                          {...register('numberOfWorkers', { valueAsNumber: true })}
                          invalid={!!errors.numberOfWorkers}
                          min={1}
                          max={100}
                        />
                        {errors.numberOfWorkers && <div className="invalid-feedback d-block">{errors.numberOfWorkers.message}</div>}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="contactPhone">Contact Phone *</CFormLabel>
                        <CFormInput
                          id="contactPhone"
                          {...register('contactPhone')}
                          invalid={!!errors.contactPhone}
                          placeholder="Primary contact number"
                        />
                        {errors.contactPhone && <div className="invalid-feedback d-block">{errors.contactPhone.message}</div>}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="workSupervisor">Work Supervisor *</CFormLabel>
                        <CFormInput
                          id="workSupervisor"
                          {...register('workSupervisor')}
                          invalid={!!errors.workSupervisor}
                          placeholder="Supervisor name"
                        />
                        {errors.workSupervisor && <div className="invalid-feedback d-block">{errors.workSupervisor.message}</div>}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="safetyOfficer">Safety Officer</CFormLabel>
                        <CFormInput
                          id="safetyOfficer"
                          {...register('safetyOfficer')}
                          placeholder="Assigned safety officer"
                        />
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="contractorCompany">Contractor Company</CFormLabel>
                        <CFormInput
                          id="contractorCompany"
                          {...register('contractorCompany')}
                          placeholder="External contractor name (if applicable)"
                        />
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Section 3: Safety Requirements */}
                <CAccordionItem itemKey={3}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.safetyRequirements} className="me-2 text-warning" />
                      <strong>Safety Requirements</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-4">
                      <h6>Special Work Requirements</h6>
                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresHotWorkPermit"
                              {...register('requiresHotWorkPermit')}
                              label="Hot Work Permit Required"
                            />
                            <small className="text-muted">Welding, cutting, grinding operations</small>
                          </div>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresConfinedSpaceEntry"
                              {...register('requiresConfinedSpaceEntry')}
                              label="Confined Space Entry"
                            />
                            <small className="text-muted">Work in tanks, vessels, enclosed spaces</small>
                          </div>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresElectricalIsolation"
                              {...register('requiresElectricalIsolation')}
                              label="Electrical Isolation Required"
                            />
                            <small className="text-muted">Lock-out/Tag-out procedures</small>
                          </div>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresHeightWork"
                              {...register('requiresHeightWork')}
                              label="Work at Height"
                            />
                            <small className="text-muted">Work above 2 meters</small>
                          </div>
                        </CCol>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresRadiationWork"
                              {...register('requiresRadiationWork')}
                              label="Radiation Work"
                            />
                            <small className="text-muted">Radioactive materials or sources</small>
                          </div>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresExcavation"
                              {...register('requiresExcavation')}
                              label="Excavation Work"
                            />
                            <small className="text-muted">Digging, trenching activities</small>
                          </div>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresFireWatch"
                              {...register('requiresFireWatch')}
                              label="Fire Watch Required"
                            />
                            <small className="text-muted">Fire safety monitoring</small>
                            {errors.requiresFireWatch && <div className="text-danger small">{errors.requiresFireWatch.message}</div>}
                          </div>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresGasMonitoring"
                              {...register('requiresGasMonitoring')}
                              label="Gas Monitoring Required"
                            />
                            <small className="text-muted">Atmospheric testing and monitoring</small>
                            {errors.requiresGasMonitoring && <div className="text-danger small">{errors.requiresGasMonitoring.message}</div>}
                          </div>
                        </CCol>
                      </CRow>
                    </div>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Section 4: K3 Compliance */}
                <CAccordionItem itemKey={4}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.k3Compliance} className="me-2 text-success" />
                      <strong>Indonesian K3 Compliance</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="k3LicenseNumber">K3 License Number</CFormLabel>
                        <CFormInput
                          id="k3LicenseNumber"
                          {...register('k3LicenseNumber')}
                          invalid={!!errors.k3LicenseNumber}
                          placeholder="Keselamatan dan Kesehatan Kerja license"
                        />
                        {errors.k3LicenseNumber && <div className="invalid-feedback d-block">{errors.k3LicenseNumber.message}</div>}
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="companyWorkPermitNumber">Company Work Permit Number</CFormLabel>
                        <CFormInput
                          id="companyWorkPermitNumber"
                          {...register('companyWorkPermitNumber')}
                          placeholder="Internal company permit reference"
                        />
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormCheck
                            id="isJamsostekCompliant"
                            {...register('isJamsostekCompliant')}
                            label="BPJS Ketenagakerjaan Compliant"
                          />
                          <small className="text-muted">Worker insurance compliance</small>
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormCheck
                            id="hasSMK3Compliance"
                            {...register('hasSMK3Compliance')}
                            label="SMK3 Compliance"
                          />
                          <small className="text-muted">Safety Management System K3</small>
                        </div>
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol>
                        <CFormLabel htmlFor="environmentalPermitNumber">Environmental Permit Number</CFormLabel>
                        <CFormInput
                          id="environmentalPermitNumber"
                          {...register('environmentalPermitNumber')}
                          placeholder="AMDAL/UKL-UPL permit number"
                        />
                        <small className="text-muted">Environmental impact assessment permit</small>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Section 5: Risk Assessment */}
                <CAccordionItem itemKey={5}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.riskAssessment} className="me-2 text-danger" />
                      <strong>Risk Assessment</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-4">
                      <CCol md={6}>
                        <CFormLabel htmlFor="riskLevel">Overall Risk Level *</CFormLabel>
                        <CFormSelect
                          id="riskLevel"
                          {...register('riskLevel')}
                        >
                          {RISK_LEVELS.map(risk => (
                            <option key={risk.value} value={risk.value}>
                              {risk.label}
                            </option>
                          ))}
                        </CFormSelect>
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel>Current Risk Level</CFormLabel>
                        <div className="p-2">
                          <CBadge 
                            color={getRiskLevelColor(watch('riskLevel'))}
                            className="text-white"
                          >
                            {watch('riskLevel')}
                          </CBadge>
                        </div>
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol>
                        <CFormLabel htmlFor="riskAssessmentSummary">Risk Assessment Summary *</CFormLabel>
                        <CFormTextarea
                          id="riskAssessmentSummary"
                          rows={4}
                          {...register('riskAssessmentSummary')}
                          invalid={!!errors.riskAssessmentSummary}
                          placeholder="Comprehensive risk assessment including identified hazards, likelihood, severity, and control measures"
                        />
                        {errors.riskAssessmentSummary && <div className="invalid-feedback d-block">{errors.riskAssessmentSummary.message}</div>}
                      </CCol>
                    </CRow>

                    {(watch('riskLevel') === 'High' || watch('riskLevel') === 'Critical' || watch('type') === 'Special' || watch('type') === 'HotWork' || watch('type') === 'ConfinedSpace') && (
                      <CRow className="mb-3">
                        <CCol>
                          <CFormLabel htmlFor="emergencyProcedures">Emergency Procedures *</CFormLabel>
                          <CFormTextarea
                            id="emergencyProcedures"
                            rows={3}
                            {...register('emergencyProcedures')}
                            invalid={!!errors.emergencyProcedures}
                            placeholder="Emergency response procedures, evacuation plans, and emergency contacts"
                          />
                          {errors.emergencyProcedures && <div className="invalid-feedback d-block">{errors.emergencyProcedures.message}</div>}
                        </CCol>
                      </CRow>
                    )}

                    {/* Hazards Management */}
                    <div className="mb-4">
                      <h6>Identified Hazards</h6>
                      
                      {(watch('hazards') || []).length > 0 && (
                        <CTable responsive className="mb-3">
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Description</CTableHeaderCell>
                              <CTableHeaderCell>Category</CTableHeaderCell>
                              <CTableHeaderCell>Risk</CTableHeaderCell>
                              <CTableHeaderCell>Controls</CTableHeaderCell>
                              <CTableHeaderCell>Actions</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {(watch('hazards') || []).map((hazard, index) => (
                              <CTableRow key={index}>
                                <CTableDataCell>{hazard.hazardDescription}</CTableDataCell>
                                <CTableDataCell>{hazard.category}</CTableDataCell>
                                <CTableDataCell>
                                  <CBadge color={getRiskLevelColor(hazard.riskLevel || 'Medium')}>
                                    {hazard.riskLevel} ({hazard.riskScore})
                                  </CBadge>
                                </CTableDataCell>
                                <CTableDataCell>{hazard.controlMeasures}</CTableDataCell>
                                <CTableDataCell>
                                  <CButton
                                    color="danger"
                                    variant="outline"
                                    size="sm"
                                    onClick={() => removeHazard(index)}
                                  >
                                    <FontAwesomeIcon icon={faTrash} />
                                  </CButton>
                                </CTableDataCell>
                              </CTableRow>
                            ))}
                          </CTableBody>
                        </CTable>
                      )}

                      {/* Add Hazard Form */}
                      <CCard className="border-dashed">
                        <CCardBody>
                          <h6 className="mb-3">Add New Hazard</h6>
                          <CRow className="mb-3">
                            <CCol md={6}>
                              <CFormLabel>Hazard Description</CFormLabel>
                              <CFormTextarea
                                rows={2}
                                value={currentHazard.hazardDescription}
                                onChange={(e) => setCurrentHazard(prev => ({
                                  ...prev,
                                  hazardDescription: e.target.value
                                }))}
                                placeholder="Describe the hazard"
                              />
                            </CCol>
                            <CCol md={6}>
                              <CFormLabel>Control Measures</CFormLabel>
                              <CFormTextarea
                                rows={2}
                                value={currentHazard.controlMeasures}
                                onChange={(e) => setCurrentHazard(prev => ({
                                  ...prev,
                                  controlMeasures: e.target.value
                                }))}
                                placeholder="Control measures to mitigate risk"
                              />
                            </CCol>
                          </CRow>
                          <CRow className="mb-3">
                            <CCol md={3}>
                              <CFormLabel>Category</CFormLabel>
                              <CFormSelect
                                value={currentHazard.category}
                                onChange={(e) => setCurrentHazard(prev => ({
                                  ...prev,
                                  category: e.target.value as HazardCategory
                                }))}
                              >
                                <option value="Physical">Physical</option>
                                <option value="Chemical">Chemical</option>
                                <option value="Biological">Biological</option>
                                <option value="Ergonomic">Ergonomic</option>
                                <option value="Fire">Fire</option>
                                <option value="Electrical">Electrical</option>
                                <option value="Mechanical">Mechanical</option>
                              </CFormSelect>
                            </CCol>
                            <CCol md={3}>
                              <CFormLabel>Likelihood (1-5)</CFormLabel>
                              <CFormSelect
                                value={currentHazard.likelihood}
                                onChange={(e) => setCurrentHazard(prev => ({
                                  ...prev,
                                  likelihood: Number(e.target.value)
                                }))}
                              >
                                {[1, 2, 3, 4, 5].map(n => (
                                  <option key={n} value={n}>{n}</option>
                                ))}
                              </CFormSelect>
                            </CCol>
                            <CCol md={3}>
                              <CFormLabel>Severity (1-5)</CFormLabel>
                              <CFormSelect
                                value={currentHazard.severity}
                                onChange={(e) => setCurrentHazard(prev => ({
                                  ...prev,
                                  severity: Number(e.target.value)
                                }))}
                              >
                                {[1, 2, 3, 4, 5].map(n => (
                                  <option key={n} value={n}>{n}</option>
                                ))}
                              </CFormSelect>
                            </CCol>
                            <CCol md={3}>
                              <CFormLabel>&nbsp;</CFormLabel>
                              <div>
                                <CButton
                                  color="primary"
                                  onClick={addHazard}
                                  disabled={!currentHazard.hazardDescription?.trim()}
                                >
                                  <FontAwesomeIcon icon={faPlus} className="me-1" />
                                  Add Hazard
                                </CButton>
                              </div>
                            </CCol>
                          </CRow>
                        </CCardBody>
                      </CCard>
                    </div>

                    {/* Precautions Management */}
                    <div className="mb-4">
                      <h6>Safety Precautions</h6>
                      
                      {(watch('precautions') || []).length > 0 && (
                        <CTable responsive className="mb-3">
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Description</CTableHeaderCell>
                              <CTableHeaderCell>Category</CTableHeaderCell>
                              <CTableHeaderCell>Priority</CTableHeaderCell>
                              <CTableHeaderCell>Required</CTableHeaderCell>
                              <CTableHeaderCell>Actions</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {(watch('precautions') || []).map((precaution, index) => (
                              <CTableRow key={index}>
                                <CTableDataCell>{precaution.precautionDescription}</CTableDataCell>
                                <CTableDataCell>{precaution.category}</CTableDataCell>
                                <CTableDataCell>
                                  <CBadge color={precaution.priority === 1 ? 'danger' : precaution.priority === 2 ? 'warning' : 'info'}>
                                    {precaution.priority}
                                  </CBadge>
                                </CTableDataCell>
                                <CTableDataCell>
                                  <CBadge color={precaution.isRequired ? 'success' : 'secondary'}>
                                    {precaution.isRequired ? 'Required' : 'Optional'}
                                  </CBadge>
                                </CTableDataCell>
                                <CTableDataCell>
                                  <CButton
                                    color="danger"
                                    variant="outline"
                                    size="sm"
                                    onClick={() => removePrecaution(index)}
                                  >
                                    <FontAwesomeIcon icon={faTrash} />
                                  </CButton>
                                </CTableDataCell>
                              </CTableRow>
                            ))}
                          </CTableBody>
                        </CTable>
                      )}

                      {/* Add Precaution Form */}
                      <CCard className="border-dashed">
                        <CCardBody>
                          <h6 className="mb-3">Add Safety Precaution</h6>
                          <CRow className="mb-3">
                            <CCol md={8}>
                              <CFormLabel>Precaution Description</CFormLabel>
                              <CFormTextarea
                                rows={2}
                                value={currentPrecaution.precautionDescription}
                                onChange={(e) => setCurrentPrecaution(prev => ({
                                  ...prev,
                                  precautionDescription: e.target.value
                                }))}
                                placeholder="Describe the safety precaution"
                              />
                            </CCol>
                            <CCol md={4}>
                              <CFormLabel>Category</CFormLabel>
                              <CFormSelect
                                value={currentPrecaution.category}
                                onChange={(e) => setCurrentPrecaution(prev => ({
                                  ...prev,
                                  category: e.target.value as PrecautionCategory
                                }))}
                              >
                                <option value="PersonalProtectiveEquipment">PPE</option>
                                <option value="Isolation">Isolation</option>
                                <option value="FireSafety">Fire Safety</option>
                                <option value="GasMonitoring">Gas Monitoring</option>
                                <option value="AccessControl">Access Control</option>
                                <option value="EmergencyProcedures">Emergency Procedures</option>
                                <option value="K3_Compliance">K3 Compliance</option>
                              </CFormSelect>
                            </CCol>
                          </CRow>
                          <CRow className="mb-3">
                            <CCol md={3}>
                              <CFormLabel>Priority (1-5)</CFormLabel>
                              <CFormSelect
                                value={currentPrecaution.priority}
                                onChange={(e) => setCurrentPrecaution(prev => ({
                                  ...prev,
                                  priority: Number(e.target.value)
                                }))}
                              >
                                {[1, 2, 3, 4, 5].map(n => (
                                  <option key={n} value={n}>{n}</option>
                                ))}
                              </CFormSelect>
                            </CCol>
                            <CCol md={3}>
                              <div className="mb-3">
                                <CFormCheck
                                  checked={currentPrecaution.isRequired}
                                  onChange={(e) => setCurrentPrecaution(prev => ({
                                    ...prev,
                                    isRequired: e.target.checked
                                  }))}
                                  label="Required"
                                />
                              </div>
                            </CCol>
                            <CCol md={3}>
                              <div className="mb-3">
                                <CFormCheck
                                  checked={currentPrecaution.isK3Requirement}
                                  onChange={(e) => setCurrentPrecaution(prev => ({
                                    ...prev,
                                    isK3Requirement: e.target.checked
                                  }))}
                                  label="K3 Requirement"
                                />
                              </div>
                            </CCol>
                            <CCol md={3}>
                              <CFormLabel>&nbsp;</CFormLabel>
                              <div>
                                <CButton
                                  color="primary"
                                  onClick={addPrecaution}
                                  disabled={
                                    !currentPrecaution.precautionDescription?.trim() ||
                                    (currentPrecaution.isK3Requirement && !currentPrecaution.k3StandardReference?.trim())
                                  }
                                >
                                  <FontAwesomeIcon icon={faPlus} className="me-1" />
                                  Add Precaution
                                </CButton>
                              </div>
                            </CCol>
                          </CRow>
                          {currentPrecaution.isK3Requirement && (
                            <CRow className="mb-3">
                              <CCol md={12}>
                                <CFormLabel>K3 Standard Reference *</CFormLabel>
                                <CFormInput
                                  value={currentPrecaution.k3StandardReference}
                                  onChange={(e) => setCurrentPrecaution(prev => ({
                                    ...prev,
                                    k3StandardReference: e.target.value
                                  }))}
                                  placeholder="Enter K3 standard reference (required for K3 compliance)"
                                />
                              </CCol>
                            </CRow>
                          )}
                        </CCardBody>
                      </CCard>
                    </div>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Section 6: Attachments */}
                <CAccordionItem itemKey={6}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.attachments} className="me-2 text-info" />
                      <strong>Supporting Documents & Attachments</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <p className="text-muted">
                        Upload supporting documents such as work plans, safety procedures, risk assessments, 
                        method statements, and other relevant documentation for this work permit.
                      </p>
                    </div>
                    
                    <WorkPermitAttachmentManager
                      attachments={[]}
                      onAttachmentsChange={setPendingAttachments}
                      allowUpload={true}
                      allowDelete={true}
                      readonly={false}
                    />
                    
                    {pendingAttachments.length > 0 && (
                      <CAlert color="info" className="mt-3">
                        <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                        {pendingAttachments.length} file{pendingAttachments.length !== 1 ? 's' : ''} will be uploaded after the work permit is created.
                      </CAlert>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* Section 7: Review & Submit */}
                <CAccordionItem itemKey={7}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.reviewSubmit} className="me-2 text-success" />
                      <strong>Review & Submit</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-4">
                      <h5>Work Permit Summary</h5>
                      
                      <CRow className="mb-3">
                        <CCol md={6}>
                          <strong>Title:</strong> {watch('title') || 'Not specified'}
                        </CCol>
                        <CCol md={6}>
                          <strong>Type:</strong> {WORK_PERMIT_TYPES.find(t => t.value === watch('type'))?.label}
                        </CCol>
                      </CRow>
                      
                      <CRow className="mb-3">
                        <CCol md={6}>
                          <strong>Location:</strong> {watch('workLocation') || 'Not specified'}
                        </CCol>
                        <CCol md={6}>
                          <strong>Priority:</strong> 
                          <CBadge color={WORK_PERMIT_PRIORITIES.find(p => p.value === watch('priority'))?.color} className="ms-2">
                            {watch('priority')}
                          </CBadge>
                        </CCol>
                      </CRow>
                      
                      <CRow className="mb-3">
                        <CCol md={6}>
                          <strong>Start Date:</strong> {watch('plannedStartDate') ? format(new Date(watch('plannedStartDate')), 'MMM dd, yyyy') : 'Not specified'}
                        </CCol>
                        <CCol md={6}>
                          <strong>End Date:</strong> {watch('plannedEndDate') ? format(new Date(watch('plannedEndDate')), 'MMM dd, yyyy') : 'Not specified'}
                        </CCol>
                      </CRow>
                      
                      <CRow className="mb-3">
                        <CCol md={6}>
                          <strong>Workers:</strong> {watch('numberOfWorkers')}
                        </CCol>
                        <CCol md={6}>
                          <strong>Risk Level:</strong> 
                          <CBadge color={getRiskLevelColor(watch('riskLevel'))} className="ms-2">
                            {watch('riskLevel')}
                          </CBadge>
                        </CCol>
                      </CRow>

                      {(watch('hazards') || []).length > 0 && (
                        <div className="mb-3">
                          <strong>Hazards Identified:</strong> {(watch('hazards') || []).length}
                        </div>
                      )}

                      {(watch('precautions') || []).length > 0 && (
                        <div className="mb-3">
                          <strong>Safety Precautions:</strong> {(watch('precautions') || []).length}
                        </div>
                      )}

                      {pendingAttachments.length > 0 && (
                        <div className="mb-3">
                          <strong>Attachments:</strong> {pendingAttachments.length} document{pendingAttachments.length !== 1 ? 's' : ''} ready for upload
                        </div>
                      )}

                      {watch('description') && (
                        <div className="mb-3">
                          <strong>Description:</strong>
                          <div className="mt-1 text-muted">{watch('description')}</div>
                        </div>
                      )}
                    </div>

                    {submitError && (
                      <CAlert color="danger" className="mb-3">
                        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                        {submitError}
                      </CAlert>
                    )}
                  </CAccordionBody>
                </CAccordionItem>
              </CAccordion>

              {/* Form Footer */}
              <div className="d-flex justify-content-between align-items-center mt-4 pt-3 border-top">
                <div className="text-muted">
                  <small>All required fields must be completed</small>
                </div>
                <div className="d-flex gap-2">
                  <CButton 
                    type="button" 
                    color="secondary" 
                    variant="outline"
                    onClick={() => navigate('/work-permits')}
                  >
                    Cancel
                  </CButton>
                  <CButton 
                    type="submit" 
                    color="success" 
                    disabled={isLoading}
                    onClick={() => console.log('🔘 Submit button clicked')}
                  >
                    {isLoading ? (
                      <CSpinner size="sm" className="me-2" />
                    ) : (
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.save} className="me-2" />
                    )}
                    Create Work Permit
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

export default CreateWorkPermit;