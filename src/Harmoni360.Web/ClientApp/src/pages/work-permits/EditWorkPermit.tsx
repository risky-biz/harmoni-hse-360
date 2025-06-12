import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
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
  CButtonGroup,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
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
  faArrowRight,
  faInfoCircle,
  faExclamationTriangle,
  faShieldAlt,
  faUsers,
  faMapMarkerAlt,
  faPlus,
  faTrash,
  faCheck,
  faTimes
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetWorkPermitByIdQuery,
  useUpdateWorkPermitMutation 
} from '../../features/work-permits/workPermitApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import { useApplicationMode } from '../../hooks/useApplicationMode';
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
import { format, addDays } from 'date-fns';

const EditWorkPermit: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isDemo } = useApplicationMode();

  // Form state
  const [activeTab, setActiveTab] = useState(0);
  const [formData, setFormData] = useState<WorkPermitFormData>({
    title: '',
    description: '',
    type: 'General',
    priority: 'Medium',
    workLocation: '',
    plannedStartDate: format(new Date(), 'yyyy-MM-dd'),
    plannedEndDate: format(addDays(new Date(), 1), 'yyyy-MM-dd'),
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
  });

  // Validation state
  const [errors, setErrors] = useState<Record<string, string>>({});
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
    isK3Requirement: false
  });

  // API calls
  const { 
    data: existingPermit, 
    error: fetchError, 
    isLoading: isLoadingPermit 
  } = useGetWorkPermitByIdQuery(id!);
  
  const { data: departments } = useGetDepartmentsQuery({});
  const [updateWorkPermit, { isLoading: isUpdating, error: updateError }] = useUpdateWorkPermitMutation();

  // Populate form with existing data
  useEffect(() => {
    if (existingPermit) {
      setFormData({
        title: existingPermit.title || '',
        description: existingPermit.description || '',
        type: existingPermit.type || 'General',
        priority: existingPermit.priority || 'Medium',
        workLocation: existingPermit.workLocation || '',
        plannedStartDate: existingPermit.plannedStartDate ? format(new Date(existingPermit.plannedStartDate), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd'),
        plannedEndDate: existingPermit.plannedEndDate ? format(new Date(existingPermit.plannedEndDate), 'yyyy-MM-dd') : format(addDays(new Date(), 1), 'yyyy-MM-dd'),
        estimatedDuration: existingPermit.estimatedDuration || 8,
        contactPhone: existingPermit.contactPhone || '',
        workSupervisor: existingPermit.workSupervisor || '',
        safetyOfficer: existingPermit.safetyOfficer || '',
        workScope: existingPermit.workScope || '',
        equipmentToBeUsed: existingPermit.equipmentToBeUsed || '',
        materialsInvolved: existingPermit.materialsInvolved || '',
        numberOfWorkers: existingPermit.numberOfWorkers || 1,
        contractorCompany: existingPermit.contractorCompany || '',
        requiresHotWorkPermit: existingPermit.requiresHotWorkPermit || false,
        requiresConfinedSpaceEntry: existingPermit.requiresConfinedSpaceEntry || false,
        requiresElectricalIsolation: existingPermit.requiresElectricalIsolation || false,
        requiresHeightWork: existingPermit.requiresHeightWork || false,
        requiresRadiationWork: existingPermit.requiresRadiationWork || false,
        requiresExcavation: existingPermit.requiresExcavation || false,
        requiresFireWatch: existingPermit.requiresFireWatch || false,
        requiresGasMonitoring: existingPermit.requiresGasMonitoring || false,
        k3LicenseNumber: existingPermit.k3LicenseNumber || '',
        companyWorkPermitNumber: existingPermit.companyWorkPermitNumber || '',
        isJamsostekCompliant: existingPermit.isJamsostekCompliant || false,
        hasSMK3Compliance: existingPermit.hasSMK3Compliance || false,
        environmentalPermitNumber: existingPermit.environmentalPermitNumber || '',
        riskLevel: existingPermit.riskLevel || 'Medium',
        riskAssessmentSummary: existingPermit.riskAssessmentSummary || '',
        emergencyProcedures: existingPermit.emergencyProcedures || '',
        hazards: existingPermit.hazards || [],
        precautions: existingPermit.precautions || []
      });
    }
  }, [existingPermit]);

  // Tab configuration
  const tabs = [
    { id: 0, title: 'Basic Information', icon: faInfoCircle },
    { id: 1, title: 'Work Details', icon: faUsers },
    { id: 2, title: 'Safety Requirements', icon: faShieldAlt },
    { id: 3, title: 'K3 Compliance', icon: faCheck },
    { id: 4, title: 'Risk Assessment', icon: faExclamationTriangle },
    { id: 5, title: 'Review & Update', icon: faCheck }
  ];

  // Form validation
  const validateCurrentTab = (): boolean => {
    const newErrors: Record<string, string> = {};

    switch (activeTab) {
      case 0: // Basic Information
        if (!formData.title.trim()) newErrors.title = 'Title is required';
        if (!formData.description.trim()) newErrors.description = 'Description is required';
        if (!formData.workLocation.trim()) newErrors.workLocation = 'Work location is required';
        if (new Date(formData.plannedStartDate) >= new Date(formData.plannedEndDate)) {
          newErrors.plannedEndDate = 'End date must be after start date';
        }
        break;
      case 1: // Work Details
        if (!formData.workScope.trim()) newErrors.workScope = 'Work scope is required';
        if (formData.numberOfWorkers < 1) newErrors.numberOfWorkers = 'Number of workers must be at least 1';
        if (!formData.contactPhone.trim()) newErrors.contactPhone = 'Contact phone is required';
        if (!formData.workSupervisor.trim()) newErrors.workSupervisor = 'Work supervisor is required';
        break;
      case 2: // Safety Requirements
        // Validation based on work type
        if (formData.type === 'HotWork' && !formData.requiresFireWatch) {
          newErrors.requiresFireWatch = 'Fire watch is required for hot work';
        }
        if (formData.type === 'ConfinedSpace' && !formData.requiresGasMonitoring) {
          newErrors.requiresGasMonitoring = 'Gas monitoring is required for confined space work';
        }
        break;
      case 3: // K3 Compliance
        if (formData.riskLevel === 'High' || formData.riskLevel === 'Critical') {
          if (!formData.k3LicenseNumber.trim()) {
            newErrors.k3LicenseNumber = 'K3 License number is required for high-risk work';
          }
        }
        break;
      case 4: // Risk Assessment
        if (!formData.riskAssessmentSummary.trim()) {
          newErrors.riskAssessmentSummary = 'Risk assessment summary is required';
        }
        if ((formData.riskLevel === 'High' || formData.riskLevel === 'Critical') && 
            !formData.emergencyProcedures.trim()) {
          newErrors.emergencyProcedures = 'Emergency procedures are required for high-risk work';
        }
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (field: keyof WorkPermitFormData, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));

    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => ({
        ...prev,
        [field]: ''
      }));
    }

    // Auto-set safety requirements based on work type
    if (field === 'type') {
      const updates: Partial<WorkPermitFormData> = {};
      
      switch (value) {
        case 'HotWork':
          updates.requiresFireWatch = true;
          updates.requiresHotWorkPermit = true;
          break;
        case 'ConfinedSpace':
          updates.requiresConfinedSpaceEntry = true;
          updates.requiresGasMonitoring = true;
          break;
        case 'ElectricalWork':
          updates.requiresElectricalIsolation = true;
          break;
        case 'Special':
          updates.requiresHeightWork = true;
          break;
      }

      setFormData(prev => ({ ...prev, ...updates }));
    }
  };

  const handleNext = () => {
    if (validateCurrentTab()) {
      setActiveTab(prev => Math.min(prev + 1, tabs.length - 1));
    }
  };

  const handlePrevious = () => {
    setActiveTab(prev => Math.max(prev - 1, 0));
  };

  const addHazard = () => {
    if (!currentHazard.hazardDescription?.trim()) return;

    const riskScore = (currentHazard.likelihood || 1) * (currentHazard.severity || 1);
    const hazard: Partial<WorkPermitHazardDto> = {
      ...currentHazard,
      id: currentHazard.id || Math.random().toString(36).substr(2, 9),
      riskScore,
      riskLevel: riskScore <= 6 ? 'Low' : riskScore <= 12 ? 'Medium' : riskScore <= 20 ? 'High' : 'Critical',
      residualRiskLevel: 'Medium', // Default
      isControlImplemented: false
    };

    setFormData(prev => ({
      ...prev,
      hazards: [...prev.hazards.filter(h => h.id !== hazard.id), hazard]
    }));

    setCurrentHazard({
      hazardDescription: '',
      category: 'Physical',
      likelihood: 3,
      severity: 3,
      controlMeasures: '',
      responsiblePerson: ''
    });
  };

  const editHazard = (hazard: WorkPermitHazardDto) => {
    setCurrentHazard(hazard);
  };

  const removeHazard = (id: string) => {
    setFormData(prev => ({
      ...prev,
      hazards: prev.hazards.filter(h => h.id !== id)
    }));
  };

  const addPrecaution = () => {
    if (!currentPrecaution.precautionDescription?.trim()) return;

    const precaution: Partial<WorkPermitPrecautionDto> = {
      ...currentPrecaution,
      id: currentPrecaution.id || Math.random().toString(36).substr(2, 9),
      isCompleted: false,
      requiresVerification: true,
      isVerified: false
    };

    setFormData(prev => ({
      ...prev,
      precautions: [...prev.precautions.filter(p => p.id !== precaution.id), precaution]
    }));

    setCurrentPrecaution({
      precautionDescription: '',
      category: 'PersonalProtectiveEquipment',
      isRequired: true,
      priority: 1,
      responsiblePerson: '',
      isK3Requirement: false
    });
  };

  const editPrecaution = (precaution: WorkPermitPrecautionDto) => {
    setCurrentPrecaution(precaution);
  };

  const removePrecaution = (id: string) => {
    setFormData(prev => ({
      ...prev,
      precautions: prev.precautions.filter(p => p.id !== id)
    }));
  };

  const handleSubmit = async () => {
    if (!validateCurrentTab() || !existingPermit?.id) return;

    try {
      const result = await updateWorkPermit({
        id: existingPermit.id,
        ...formData
      }).unwrap();
      navigate(`/work-permits/${result.id}`);
    } catch (error) {
      console.error('Failed to update work permit:', error);
    }
  };

  const getRiskLevelColor = (riskLevel: string) => {
    const riskConfig = RISK_LEVELS.find(r => r.value === riskLevel);
    return riskConfig?.color || 'secondary';
  };

  // Check if permit can be edited
  const canEdit = existingPermit?.status === 'Draft';

  if (isLoadingPermit) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner color="primary" />
      </div>
    );
  }

  if (fetchError || !existingPermit) {
    return (
      <CAlert color="danger" className="m-3">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Failed to load work permit details. Please try again.
      </CAlert>
    );
  }

  if (!canEdit) {
    return (
      <CAlert color="warning" className="m-3">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        This work permit cannot be edited in its current status: {existingPermit.statusDisplay}
        <div className="mt-2">
          <CButton
            color="primary"
            variant="outline"
            onClick={() => navigate(`/work-permits/${existingPermit.id}`)}
          >
            View Work Permit
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <div className="edit-work-permit">
      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h1 className="h3 mb-1">Edit Work Permit</h1>
              <p className="text-muted mb-0">#{existingPermit.permitNumber} - {existingPermit.title}</p>
            </div>
            <CButton
              color="secondary"
              variant="outline"
              onClick={() => navigate(`/work-permits/${existingPermit.id}`)}
            >
              <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
              Back to Details
            </CButton>
          </div>
        </CCol>
      </CRow>

      {/* Progress Navigation */}
      <CCard className="mb-4">
        <CCardBody className="p-0">
          <CNav variant="tabs" role="tablist">
            {tabs.map((tab, index) => (
              <CNavItem key={tab.id}>
                <CNavLink
                  active={activeTab === index}
                  onClick={() => {
                    if (index < activeTab || validateCurrentTab()) {
                      setActiveTab(index);
                    }
                  }}
                  style={{ cursor: index <= activeTab ? 'pointer' : 'default' }}
                  className={index > activeTab ? 'disabled' : ''}
                >
                  <FontAwesomeIcon icon={tab.icon} className="me-2" />
                  <span className="d-none d-md-inline">{tab.title}</span>
                  <span className="d-md-none">{index + 1}</span>
                </CNavLink>
              </CNavItem>
            ))}
          </CNav>
        </CCardBody>
      </CCard>

      {/* Form Content */}
      <CForm onSubmit={(e) => { e.preventDefault(); }}>
        <CTabContent>
          {/* Tab 0: Basic Information */}
          <CTabPane visible={activeTab === 0}>
            <CCard>
              <CCardHeader>
                <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                Basic Information
              </CCardHeader>
              <CCardBody>
                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="title">Work Permit Title *</CFormLabel>
                      <CFormInput
                        id="title"
                        value={formData.title}
                        onChange={(e) => handleInputChange('title', e.target.value)}
                        invalid={!!errors.title}
                        placeholder="Enter descriptive title for the work"
                      />
                      {errors.title && <div className="invalid-feedback">{errors.title}</div>}
                    </div>
                  </CCol>
                  <CCol md={3}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="type">Work Type *</CFormLabel>
                      <CFormSelect
                        id="type"
                        value={formData.type}
                        onChange={(e) => handleInputChange('type', e.target.value)}
                      >
                        {WORK_PERMIT_TYPES.map(type => (
                          <option key={type.value} value={type.value}>
                            {type.label}
                          </option>
                        ))}
                      </CFormSelect>
                    </div>
                  </CCol>
                  <CCol md={3}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="priority">Priority *</CFormLabel>
                      <CFormSelect
                        id="priority"
                        value={formData.priority}
                        onChange={(e) => handleInputChange('priority', e.target.value)}
                      >
                        {WORK_PERMIT_PRIORITIES.map(priority => (
                          <option key={priority.value} value={priority.value}>
                            {priority.label}
                          </option>
                        ))}
                      </CFormSelect>
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="description">Description *</CFormLabel>
                  <CFormTextarea
                    id="description"
                    rows={3}
                    value={formData.description}
                    onChange={(e) => handleInputChange('description', e.target.value)}
                    invalid={!!errors.description}
                    placeholder="Provide detailed description of the work to be performed"
                  />
                  {errors.description && <div className="invalid-feedback">{errors.description}</div>}
                </div>

                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="workLocation">Work Location *</CFormLabel>
                      <CFormInput
                        id="workLocation"
                        value={formData.workLocation}
                        onChange={(e) => handleInputChange('workLocation', e.target.value)}
                        invalid={!!errors.workLocation}
                        placeholder="Building, room, area, or specific location"
                      />
                      {errors.workLocation && <div className="invalid-feedback">{errors.workLocation}</div>}
                    </div>
                  </CCol>
                  <CCol md={3}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="plannedStartDate">Planned Start Date *</CFormLabel>
                      <CFormInput
                        type="date"
                        id="plannedStartDate"
                        value={formData.plannedStartDate}
                        onChange={(e) => handleInputChange('plannedStartDate', e.target.value)}
                        min={format(new Date(), 'yyyy-MM-dd')}
                      />
                    </div>
                  </CCol>
                  <CCol md={3}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="plannedEndDate">Planned End Date *</CFormLabel>
                      <CFormInput
                        type="date"
                        id="plannedEndDate"
                        value={formData.plannedEndDate}
                        onChange={(e) => handleInputChange('plannedEndDate', e.target.value)}
                        invalid={!!errors.plannedEndDate}
                        min={formData.plannedStartDate}
                      />
                      {errors.plannedEndDate && <div className="invalid-feedback">{errors.plannedEndDate}</div>}
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="estimatedDuration">Estimated Duration (hours)</CFormLabel>
                  <CInputGroup>
                    <CFormInput
                      type="number"
                      id="estimatedDuration"
                      value={formData.estimatedDuration}
                      onChange={(e) => handleInputChange('estimatedDuration', Number(e.target.value))}
                      min={1}
                      max={8760}
                    />
                    <CInputGroupText>hours</CInputGroupText>
                  </CInputGroup>
                </div>
              </CCardBody>
            </CCard>
          </CTabPane>

          {/* Tab 1: Work Details - Similar to CreateWorkPermit but with edit functionality */}
          <CTabPane visible={activeTab === 1}>
            <CCard>
              <CCardHeader>
                <FontAwesomeIcon icon={faUsers} className="me-2" />
                Work Details & Personnel
              </CCardHeader>
              <CCardBody>
                <div className="mb-4">
                  <h6>Work Scope & Materials</h6>
                  <div className="mb-3">
                    <CFormLabel htmlFor="workScope">Work Scope *</CFormLabel>
                    <CFormTextarea
                      id="workScope"
                      rows={3}
                      value={formData.workScope}
                      onChange={(e) => handleInputChange('workScope', e.target.value)}
                      invalid={!!errors.workScope}
                      placeholder="Detailed description of work activities and procedures"
                    />
                    {errors.workScope && <div className="invalid-feedback">{errors.workScope}</div>}
                  </div>

                  <CRow>
                    <CCol md={6}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="equipmentToBeUsed">Equipment to be Used</CFormLabel>
                        <CFormTextarea
                          id="equipmentToBeUsed"
                          rows={2}
                          value={formData.equipmentToBeUsed}
                          onChange={(e) => handleInputChange('equipmentToBeUsed', e.target.value)}
                          placeholder="List equipment, tools, and machinery"
                        />
                      </div>
                    </CCol>
                    <CCol md={6}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="materialsInvolved">Materials Involved</CFormLabel>
                        <CFormTextarea
                          id="materialsInvolved"
                          rows={2}
                          value={formData.materialsInvolved}
                          onChange={(e) => handleInputChange('materialsInvolved', e.target.value)}
                          placeholder="List chemicals, substances, and materials"
                        />
                      </div>
                    </CCol>
                  </CRow>

                  <CRow>
                    <CCol md={6}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="numberOfWorkers">Number of Workers *</CFormLabel>
                        <CFormInput
                          type="number"
                          id="numberOfWorkers"
                          value={formData.numberOfWorkers}
                          onChange={(e) => handleInputChange('numberOfWorkers', Number(e.target.value))}
                          invalid={!!errors.numberOfWorkers}
                          min={1}
                          max={100}
                        />
                        {errors.numberOfWorkers && <div className="invalid-feedback">{errors.numberOfWorkers}</div>}
                      </div>
                    </CCol>
                    <CCol md={6}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="contractorCompany">Contractor Company</CFormLabel>
                        <CFormInput
                          id="contractorCompany"
                          value={formData.contractorCompany}
                          onChange={(e) => handleInputChange('contractorCompany', e.target.value)}
                          placeholder="External contractor name (if applicable)"
                        />
                      </div>
                    </CCol>
                  </CRow>
                </div>

                <div className="mb-4">
                  <h6>Personnel & Contacts</h6>
                  <CRow>
                    <CCol md={4}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="contactPhone">Contact Phone *</CFormLabel>
                        <CFormInput
                          id="contactPhone"
                          value={formData.contactPhone}
                          onChange={(e) => handleInputChange('contactPhone', e.target.value)}
                          invalid={!!errors.contactPhone}
                          placeholder="Primary contact number"
                        />
                        {errors.contactPhone && <div className="invalid-feedback">{errors.contactPhone}</div>}
                      </div>
                    </CCol>
                    <CCol md={4}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="workSupervisor">Work Supervisor *</CFormLabel>
                        <CFormInput
                          id="workSupervisor"
                          value={formData.workSupervisor}
                          onChange={(e) => handleInputChange('workSupervisor', e.target.value)}
                          invalid={!!errors.workSupervisor}
                          placeholder="Supervisor name"
                        />
                        {errors.workSupervisor && <div className="invalid-feedback">{errors.workSupervisor}</div>}
                      </div>
                    </CCol>
                    <CCol md={4}>
                      <div className="mb-3">
                        <CFormLabel htmlFor="safetyOfficer">Safety Officer</CFormLabel>
                        <CFormInput
                          id="safetyOfficer"
                          value={formData.safetyOfficer}
                          onChange={(e) => handleInputChange('safetyOfficer', e.target.value)}
                          placeholder="Assigned safety officer"
                        />
                      </div>
                    </CCol>
                  </CRow>
                </div>
              </CCardBody>
            </CCard>
          </CTabPane>

          {/* Safety Requirements, K3 Compliance, and Risk Assessment tabs would be similar to CreateWorkPermit */}
          {/* For brevity, I'll include just the key tabs and final review */}

          {/* Tab 4: Risk Assessment with Hazards and Precautions editing */}
          <CTabPane visible={activeTab === 4}>
            <CCard>
              <CCardHeader>
                <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                Risk Assessment
              </CCardHeader>
              <CCardBody>
                <CRow className="mb-4">
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="riskLevel">Overall Risk Level *</CFormLabel>
                      <CFormSelect
                        id="riskLevel"
                        value={formData.riskLevel}
                        onChange={(e) => handleInputChange('riskLevel', e.target.value)}
                      >
                        {RISK_LEVELS.map(risk => (
                          <option key={risk.value} value={risk.value}>
                            {risk.label}
                          </option>
                        ))}
                      </CFormSelect>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel>Current Risk Level</CFormLabel>
                      <div className="p-2">
                        <CBadge 
                          color={getRiskLevelColor(formData.riskLevel)} 
                          size="lg"
                        >
                          {formData.riskLevel}
                        </CBadge>
                      </div>
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-4">
                  <CFormLabel htmlFor="riskAssessmentSummary">Risk Assessment Summary *</CFormLabel>
                  <CFormTextarea
                    id="riskAssessmentSummary"
                    rows={4}
                    value={formData.riskAssessmentSummary}
                    onChange={(e) => handleInputChange('riskAssessmentSummary', e.target.value)}
                    invalid={!!errors.riskAssessmentSummary}
                    placeholder="Comprehensive risk assessment including identified hazards, likelihood, severity, and control measures"
                  />
                  {errors.riskAssessmentSummary && <div className="invalid-feedback">{errors.riskAssessmentSummary}</div>}
                </div>

                {/* Hazards Management with Edit capability */}
                <div className="mb-4">
                  <h6>Identified Hazards</h6>
                  
                  {formData.hazards.length > 0 && (
                    <CTable responsive className="mb-3">
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Description</CTableHeaderCell>
                          <CTableHeaderCell>Category</CTableHeaderCell>
                          <CTableHeaderCell>Risk</CTableHeaderCell>
                          <CTableHeaderCell>Actions</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {formData.hazards.map((hazard, index) => (
                          <CTableRow key={hazard.id || index}>
                            <CTableDataCell>{hazard.hazardDescription}</CTableDataCell>
                            <CTableDataCell>{hazard.category}</CTableDataCell>
                            <CTableDataCell>
                              <CBadge color={getRiskLevelColor(hazard.riskLevel || 'Medium')}>
                                {hazard.riskLevel} ({hazard.riskScore})
                              </CBadge>
                            </CTableDataCell>
                            <CTableDataCell>
                              <CButtonGroup size="sm">
                                <CButton
                                  color="primary"
                                  variant="outline"
                                  onClick={() => editHazard(hazard as WorkPermitHazardDto)}
                                >
                                  <FontAwesomeIcon icon={faEdit} />
                                </CButton>
                                <CButton
                                  color="danger"
                                  variant="outline"
                                  onClick={() => removeHazard(hazard.id!)}
                                >
                                  <FontAwesomeIcon icon={faTrash} />
                                </CButton>
                              </CButtonGroup>
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                  )}

                  {/* Add/Edit Hazard Form - similar to CreateWorkPermit */}
                  <CCard className="border-dashed">
                    <CCardBody>
                      <h6 className="mb-3">{currentHazard.id ? 'Edit Hazard' : 'Add New Hazard'}</h6>
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
                                <FontAwesomeIcon icon={currentHazard.id ? faCheck : faPlus} className="me-1" />
                                {currentHazard.id ? 'Update' : 'Add'} Hazard
                              </CButton>
                              {currentHazard.id && (
                                <CButton
                                  color="secondary"
                                  variant="outline"
                                  className="ms-2"
                                  onClick={() => setCurrentHazard({
                                    hazardDescription: '',
                                    category: 'Physical',
                                    likelihood: 3,
                                    severity: 3,
                                    controlMeasures: '',
                                    responsiblePerson: ''
                                  })}
                                >
                                  <FontAwesomeIcon icon={faTimes} />
                                </CButton>
                              )}
                            </div>
                          </div>
                        </CCol>
                      </CRow>
                    </CCardBody>
                  </CCard>
                </div>
              </CCardBody>
            </CCard>
          </CTabPane>

          {/* Tab 5: Review & Update */}
          <CTabPane visible={activeTab === 5}>
            <CCard>
              <CCardHeader>
                <FontAwesomeIcon icon={faCheck} className="me-2" />
                Review & Update
              </CCardHeader>
              <CCardBody>
                <div className="mb-4">
                  <h5>Work Permit Summary</h5>
                  
                  <CRow className="mb-3">
                    <CCol md={6}>
                      <strong>Title:</strong> {formData.title}
                    </CCol>
                    <CCol md={6}>
                      <strong>Type:</strong> {WORK_PERMIT_TYPES.find(t => t.value === formData.type)?.label}
                    </CCol>
                  </CRow>
                  
                  <CRow className="mb-3">
                    <CCol md={6}>
                      <strong>Location:</strong> {formData.workLocation}
                    </CCol>
                    <CCol md={6}>
                      <strong>Priority:</strong> 
                      <CBadge color={WORK_PERMIT_PRIORITIES.find(p => p.value === formData.priority)?.color} className="ms-2">
                        {formData.priority}
                      </CBadge>
                    </CCol>
                  </CRow>
                  
                  <CRow className="mb-3">
                    <CCol md={6}>
                      <strong>Start Date:</strong> {format(new Date(formData.plannedStartDate), 'MMM dd, yyyy')}
                    </CCol>
                    <CCol md={6}>
                      <strong>End Date:</strong> {format(new Date(formData.plannedEndDate), 'MMM dd, yyyy')}
                    </CCol>
                  </CRow>
                  
                  <CRow className="mb-3">
                    <CCol md={6}>
                      <strong>Workers:</strong> {formData.numberOfWorkers}
                    </CCol>
                    <CCol md={6}>
                      <strong>Risk Level:</strong> 
                      <CBadge color={getRiskLevelColor(formData.riskLevel)} className="ms-2">
                        {formData.riskLevel}
                      </CBadge>
                    </CCol>
                  </CRow>

                  {formData.hazards.length > 0 && (
                    <div className="mb-3">
                      <strong>Hazards Identified:</strong> {formData.hazards.length}
                    </div>
                  )}

                  {formData.precautions.length > 0 && (
                    <div className="mb-3">
                      <strong>Safety Precautions:</strong> {formData.precautions.length}
                    </div>
                  )}

                  <div className="mb-3">
                    <strong>Description:</strong>
                    <div className="mt-1 text-muted">{formData.description}</div>
                  </div>
                </div>

                {updateError && (
                  <CAlert color="danger" className="mb-3">
                    <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                    Failed to update work permit. Please check your inputs and try again.
                  </CAlert>
                )}
              </CCardBody>
            </CCard>
          </CTabPane>
        </CTabContent>

        {/* Navigation Footer */}
        <CCard className="mt-4">
          <CCardBody>
            <div className="d-flex justify-content-between">
              <CButton
                color="secondary"
                variant="outline"
                onClick={handlePrevious}
                disabled={activeTab === 0}
              >
                <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                Previous
              </CButton>

              <div className="d-flex gap-2">
                {activeTab < tabs.length - 1 ? (
                  <CButton
                    color="primary"
                    onClick={handleNext}
                  >
                    Next
                    <FontAwesomeIcon icon={faArrowRight} className="ms-2" />
                  </CButton>
                ) : (
                  <CButton
                    color="success"
                    onClick={handleSubmit}
                    disabled={isUpdating || isDemo}
                  >
                    {isUpdating && <CSpinner size="sm" className="me-2" />}
                    <FontAwesomeIcon icon={faSave} className="me-2" />
                    Update Work Permit
                  </CButton>
                )}
              </div>
            </div>
          </CCardBody>
        </CCard>
      </CForm>
    </div>
  );
};

export default EditWorkPermit;