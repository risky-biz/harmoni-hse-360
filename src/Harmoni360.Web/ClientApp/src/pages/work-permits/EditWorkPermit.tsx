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
  faFileContract,
  faPlus,
  faTrash,
  faCheck,
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetWorkPermitByIdQuery,
  useUpdateWorkPermitMutation 
} from '../../features/work-permits/workPermitApi';
import WorkPermitAttachmentManager from '../../components/work-permits/WorkPermitAttachmentManager';
import {
  WORK_PERMIT_TYPES,
  WORK_PERMIT_PRIORITIES,
  RISK_LEVELS,
  HazardCategory,
  PrecautionCategory,
  WorkPermitHazardDto,
  WorkPermitPrecautionDto,
} from '../../types/workPermit';
import { formatDateTime } from '../../utils/dateUtils';
import { format } from 'date-fns';

// Work Permit Icon Mappings
const WORKPERMIT_ICONS = {
  workPermit: faFileContract,
  basicInformation: faInfoCircle,
  workDetails: faUsers,
  safetyRequirements: faShieldAlt,
  k3Compliance: faCertificate,
  riskAssessment: faExclamationTriangle,
  attachments: faPaperclip,
  create: faPlus,
  save: faSave,
  back: faArrowLeft,
  check: faCheck,
};

// Form data interface with all work permit fields
interface EditWorkPermitFormData {
  title: string;
  description: string;
  type: string;
  priority: string;
  workLocation: string;
  plannedStartDate: string;
  plannedEndDate: string;
  estimatedDuration: number;
  contactPhone: string;
  workSupervisor: string;
  safetyOfficer: string;
  workScope: string;
  equipmentToBeUsed: string;
  materialsInvolved: string;
  numberOfWorkers: number;
  contractorCompany: string;
  requiresHotWorkPermit: boolean;
  requiresConfinedSpaceEntry: boolean;
  requiresElectricalIsolation: boolean;
  requiresHeightWork: boolean;
  requiresRadiationWork: boolean;
  requiresExcavation: boolean;
  requiresFireWatch: boolean;
  requiresGasMonitoring: boolean;
  k3LicenseNumber: string;
  companyWorkPermitNumber: string;
  isJamsostekCompliant: boolean;
  hasSMK3Compliance: boolean;
  environmentalPermitNumber: string;
  riskLevel: string;
  riskAssessmentSummary: string;
  emergencyProcedures: string;
  hazards: WorkPermitHazardDto[];
  precautions: WorkPermitPrecautionDto[];
}

const EditWorkPermit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  // State for managing hazards and precautions
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

  // API calls
  const { 
    data: workPermit, 
    error: loadError, 
    isLoading 
  } = useGetWorkPermitByIdQuery(id!);
  
  const [updateWorkPermit, { isLoading: isUpdating, error: updateError }] = useUpdateWorkPermitMutation();

  // Form management using react-hook-form
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch,
    getValues,
  } = useForm<EditWorkPermitFormData>();

  // Populate form with existing data
  useEffect(() => {
    if (workPermit) {
      reset({
        title: workPermit.title || '',
        description: workPermit.description || '',
        type: workPermit.type || 'General',
        priority: workPermit.priority || 'Medium',
        workLocation: workPermit.workLocation || '',
        plannedStartDate: workPermit.plannedStartDate ? format(new Date(workPermit.plannedStartDate), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd'),
        plannedEndDate: workPermit.plannedEndDate ? format(new Date(workPermit.plannedEndDate), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd'),
        estimatedDuration: workPermit.estimatedDuration || 8,
        contactPhone: workPermit.contactPhone || '',
        workSupervisor: workPermit.workSupervisor || '',
        safetyOfficer: workPermit.safetyOfficer || '',
        workScope: workPermit.workScope || '',
        equipmentToBeUsed: workPermit.equipmentToBeUsed || '',
        materialsInvolved: workPermit.materialsInvolved || '',
        numberOfWorkers: workPermit.numberOfWorkers || 1,
        contractorCompany: workPermit.contractorCompany || '',
        requiresHotWorkPermit: workPermit.requiresHotWorkPermit || false,
        requiresConfinedSpaceEntry: workPermit.requiresConfinedSpaceEntry || false,
        requiresElectricalIsolation: workPermit.requiresElectricalIsolation || false,
        requiresHeightWork: workPermit.requiresHeightWork || false,
        requiresRadiationWork: workPermit.requiresRadiationWork || false,
        requiresExcavation: workPermit.requiresExcavation || false,
        requiresFireWatch: workPermit.requiresFireWatch || false,
        requiresGasMonitoring: workPermit.requiresGasMonitoring || false,
        k3LicenseNumber: workPermit.k3LicenseNumber || '',
        companyWorkPermitNumber: workPermit.companyWorkPermitNumber || '',
        isJamsostekCompliant: workPermit.isJamsostekCompliant || false,
        hasSMK3Compliance: workPermit.hasSMK3Compliance || false,
        environmentalPermitNumber: workPermit.environmentalPermitNumber || '',
        riskLevel: workPermit.riskLevel || 'Medium',
        riskAssessmentSummary: workPermit.riskAssessmentSummary || '',
        emergencyProcedures: workPermit.emergencyProcedures || '',
        hazards: workPermit.hazards || [],
        precautions: workPermit.precautions || [],
      });
    }
  }, [workPermit, reset]);

  // Hazard and Precaution management functions
  const addHazard = useCallback(() => {
    if (!currentHazard.hazardDescription?.trim()) return;

    const hazard: WorkPermitHazardDto = {
      ...currentHazard,
      id: Math.random().toString(36).substr(2, 9),
      riskLevel: calculateRiskLevel(currentHazard.likelihood || 3, currentHazard.severity || 3),
      riskScore: (currentHazard.likelihood || 3) * (currentHazard.severity || 3),
      isControlImplemented: false,
    } as WorkPermitHazardDto;

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

    const precaution: WorkPermitPrecautionDto = {
      ...currentPrecaution,
      id: Math.random().toString(36).substr(2, 9),
      isCompleted: false,
      requiresVerification: true,
      isVerified: false
    } as WorkPermitPrecautionDto;

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

  const calculateRiskLevel = (likelihood: number, severity: number) => {
    const score = likelihood * severity;
    if (score <= 4) return 'Low';
    if (score <= 9) return 'Medium';
    if (score <= 16) return 'High';
    return 'Critical';
  };

  const getRiskLevelColor = (riskLevel: string) => {
    switch (riskLevel) {
      case 'Low': return 'success';
      case 'Medium': return 'warning';
      case 'High': return 'danger';
      case 'Critical': return 'danger';
      default: return 'secondary';
    }
  };

  const onSubmit = async (data: EditWorkPermitFormData) => {
    if (!workPermit?.id) return;

    try {
      await updateWorkPermit({
        id: workPermit.id,
        ...data,
      }).unwrap();
      navigate(`/work-permits/${workPermit.id}`);
    } catch (error) {
      console.error('Failed to update work permit:', error);
    }
  };

  // Check if work permit can be edited
  const canEdit = workPermit?.status === 'Draft' || workPermit?.status === 'Rejected';

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading work permit...</span>
      </div>
    );
  }

  if (loadError || !workPermit) {
    return (
      <CAlert color="danger">
        Failed to load work permit. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/work-permits')}>
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            Back to List
          </CButton>
        </div>
      </CAlert>
    );
  }

  if (!canEdit) {
    return (
      <CAlert color="warning">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        This work permit cannot be edited in its current status: {workPermit.status}
        <div className="mt-3">
          <CButton
            color="primary"
            onClick={() => navigate(`/work-permits/${workPermit.id}`)}
          >
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            View Work Permit
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0">
                <FontAwesomeIcon icon={WORKPERMIT_ICONS.workPermit} size="lg" className="me-2 text-primary" />
                Edit Work Permit
              </h4>
              <small className="text-muted">#{workPermit.permitNumber} - {workPermit.title}</small>
            </div>
            <div className="d-flex align-items-center gap-2">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate(`/work-permits/${id}`)}
              >
                <FontAwesomeIcon icon={WORKPERMIT_ICONS.back} className="me-2" />
                Back to Details
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            <CForm onSubmit={handleSubmit(onSubmit)}>
              {updateError && (
                <CAlert
                  color="danger"
                  dismissible
                >
                  Failed to update work permit. Please try again.
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
                          {...register('title', { required: 'Title is required' })}
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
                          {...register('description', { required: 'Description is required' })}
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
                          {...register('workLocation', { required: 'Work location is required' })}
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
                          {...register('workScope', { required: 'Work scope is required' })}
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
                          {...register('numberOfWorkers', { valueAsNumber: true, required: 'Number of workers is required' })}
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
                          {...register('contactPhone', { required: 'Contact phone is required' })}
                          invalid={!!errors.contactPhone}
                          placeholder="Primary contact number"
                        />
                        {errors.contactPhone && <div className="invalid-feedback d-block">{errors.contactPhone.message}</div>}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="workSupervisor">Work Supervisor *</CFormLabel>
                        <CFormInput
                          id="workSupervisor"
                          {...register('workSupervisor', { required: 'Work supervisor is required' })}
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
                          </div>
                          <div className="mb-3">
                            <CFormCheck
                              id="requiresGasMonitoring"
                              {...register('requiresGasMonitoring')}
                              label="Gas Monitoring Required"
                            />
                            <small className="text-muted">Atmospheric testing and monitoring</small>
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
                      <strong>K3 Compliance</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="k3LicenseNumber">K3 License Number</CFormLabel>
                        <CFormInput
                          id="k3LicenseNumber"
                          {...register('k3LicenseNumber')}
                          placeholder="K3 license reference number"
                        />
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="companyWorkPermitNumber">Company Work Permit Number</CFormLabel>
                        <CFormInput
                          id="companyWorkPermitNumber"
                          {...register('companyWorkPermitNumber')}
                          placeholder="Internal work permit reference"
                        />
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormCheck
                            id="isJamsostekCompliant"
                            {...register('isJamsostekCompliant')}
                            label="Jamsostek Compliant"
                          />
                          <small className="text-muted">Workers covered by social security</small>
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormCheck
                            id="hasSMK3Compliance"
                            {...register('hasSMK3Compliance')}
                            label="SMK3 Compliance"
                          />
                          <small className="text-muted">Occupational Health and Safety Management System</small>
                        </div>
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="environmentalPermitNumber">Environmental Permit Number</CFormLabel>
                        <CFormInput
                          id="environmentalPermitNumber"
                          {...register('environmentalPermitNumber')}
                          placeholder="Environmental clearance reference"
                        />
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
                            color={getRiskLevelColor(watch('riskLevel') || 'Medium')} 
                            size="lg"
                          >
                            {watch('riskLevel') || 'Medium'}
                          </CBadge>
                        </div>
                      </CCol>
                    </CRow>

                    <div className="mb-4">
                      <CFormLabel htmlFor="riskAssessmentSummary">Risk Assessment Summary *</CFormLabel>
                      <CFormTextarea
                        id="riskAssessmentSummary"
                        rows={4}
                        {...register('riskAssessmentSummary', { required: 'Risk assessment summary is required' })}
                        invalid={!!errors.riskAssessmentSummary}
                        placeholder="Comprehensive risk assessment including identified hazards, likelihood, severity, and control measures"
                      />
                      {errors.riskAssessmentSummary && <div className="invalid-feedback d-block">{errors.riskAssessmentSummary.message}</div>}
                    </div>

                    <div className="mb-4">
                      <CFormLabel htmlFor="emergencyProcedures">Emergency Procedures</CFormLabel>
                      <CFormTextarea
                        id="emergencyProcedures"
                        rows={3}
                        {...register('emergencyProcedures')}
                        placeholder="Emergency response procedures and contact information"
                      />
                    </div>

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
                          <CRow>
                            <CCol md={6}>
                              <div className="mb-3">
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
                              </div>
                            </CCol>
                            <CCol md={6}>
                              <div className="mb-3">
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
                              </div>
                            </CCol>
                          </CRow>
                          <CRow>
                            <CCol md={3}>
                              <div className="mb-3">
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
                              </div>
                            </CCol>
                            <CCol md={3}>
                              <div className="mb-3">
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
                              </div>
                            </CCol>
                            <CCol md={3}>
                              <div className="mb-3">
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
                              </div>
                            </CCol>
                            <CCol md={3}>
                              <div className="mb-3">
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
                              </div>
                            </CCol>
                          </CRow>
                        </CCardBody>
                      </CCard>
                    </div>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Section 6: Attachments */}
                <CAccordionItem itemKey={6}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.attachments} className="me-2 text-secondary" />
                      <strong>Attachments ({workPermit?.attachments?.length || 0})</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <WorkPermitAttachmentManager
                      workPermitId={Number(id)}
                      allowUpload={true}
                      allowDelete={true}
                    />
                  </CAccordionBody>
                </CAccordionItem>
              </CAccordion>

              <hr />

              <div className="d-flex justify-content-between">
                <CButton
                  color="light"
                  onClick={() => navigate(`/work-permits/${id}`)}
                  disabled={isUpdating}
                >
                  <FontAwesomeIcon icon={WORKPERMIT_ICONS.back} className="me-2" />
                  Cancel
                </CButton>
                <CButton color="primary" type="submit" disabled={isUpdating}>
                  {isUpdating ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Updating...
                    </>
                  ) : (
                    <>
                      <FontAwesomeIcon icon={WORKPERMIT_ICONS.save} className="me-2" />
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
  );
};

export default EditWorkPermit;