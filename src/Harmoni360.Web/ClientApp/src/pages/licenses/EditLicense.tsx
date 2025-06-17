import React, { useState, useCallback, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
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
  CBadge,
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
  faShieldAlt,
  faFileContract,
  faCalendarAlt,
  faCertificate,
  faBuilding,
  faGavel,
  faClipboardCheck,
  faEdit,
  faPlus,
  faTrash,
  faPaperclip,
  faUpload,
  faDownload,
  faTasks
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetLicenseByIdQuery,
  useUpdateLicenseMutation,
  useAddLicenseConditionMutation,
  useUpdateLicenseConditionMutation,
  useDeleteLicenseConditionMutation
} from '../../features/licenses/licenseApi';
import {
  LicenseFormData,
  LicenseConditionFormData,
  LICENSE_TYPES,
  LICENSE_PRIORITIES,
  RISK_LEVELS,
  CONDITION_TYPES,
  CURRENCIES
} from '../../types/license';
import { format } from 'date-fns';

// Hardcoded departments list - TODO: Replace with API call from Configuration table
const DEPARTMENTS = [
  { id: 1, name: 'Health and Safety' },
  { id: 2, name: 'Environmental' },
  { id: 3, name: 'Operations' },
  { id: 4, name: 'Engineering' },
  { id: 5, name: 'Maintenance' },
  { id: 6, name: 'Quality Assurance' },
  { id: 7, name: 'Security' },
  { id: 8, name: 'Human Resources' },
  { id: 9, name: 'Finance' },
  { id: 10, name: 'Procurement' },
  { id: 11, name: 'Legal' },
  { id: 12, name: 'IT' },
  { id: 13, name: 'Construction' },
  { id: 14, name: 'Production' },
  { id: 15, name: 'Transportation' },
  { id: 16, name: 'Logistics' },
  { id: 17, name: 'Research & Development' },
  { id: 18, name: 'Food Services' },
  { id: 19, name: 'Administration' },
  { id: 20, name: 'Customer Service' }
];

// Helper functions to convert enum strings to numbers
const getPriorityEnum = (priority: string): number => {
  const priorityMap: { [key: string]: number } = {
    'Low': 1,
    'Medium': 2, 
    'High': 3,
    'Critical': 4
  };
  return priorityMap[priority] || 2; // Default to Medium
};

const getRiskLevelEnum = (riskLevel: string): number => {
  const riskMap: { [key: string]: number } = {
    'Low': 0,
    'Medium': 1,
    'High': 2, 
    'Critical': 3
  };
  return riskMap[riskLevel] || 1; // Default to Medium
};

// Validation schema
const schema = yup.object({
  title: yup.string().required('Title is required').max(200, 'Title must not exceed 200 characters'),
  description: yup.string().required('Description is required').max(2000, 'Description must not exceed 2000 characters'),
  type: yup.string().required('License type is required'),
  licenseNumber: yup.string().required('License number is required').max(50, 'License number must not exceed 50 characters'),
  issuingAuthority: yup.string().required('Issuing authority is required').max(200, 'Issuing authority must not exceed 200 characters'),
  holderName: yup.string().required('Holder name is required').max(200, 'Holder name must not exceed 200 characters'),
  department: yup.string().required('Department is required'),
  issuedDate: yup.string().required('Issued date is required'),
  expiryDate: yup.string().required('Expiry date is required'),
  priority: yup.string().required('Priority is required'),
  riskLevel: yup.string().required('Risk level is required'),
  scope: yup.string().required('Scope is required').max(1000, 'Scope must not exceed 1000 characters'),
  currency: yup.string().required('Currency is required'),
  regulatoryFramework: yup.string().required('Regulatory framework is required'),
  renewalPeriodDays: yup.number().min(1, 'Renewal period must be at least 1 day').max(3650, 'Renewal period cannot exceed 10 years')
});

const EditLicense: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const licenseId = parseInt(id || '0', 10);

  const [showSuccess, setShowSuccess] = useState(false);
  const [showError, setShowError] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const hasLoadedConditions = useRef(false);

  // API calls
  const {
    data: license,
    isLoading: isLoadingLicense,
    error: licenseError
  } = useGetLicenseByIdQuery(licenseId, { skip: !licenseId });

  const [updateLicense, { isLoading: isUpdating }] = useUpdateLicenseMutation();
  const [addLicenseCondition] = useAddLicenseConditionMutation();
  const [updateLicenseCondition] = useUpdateLicenseConditionMutation();
  const [deleteLicenseCondition] = useDeleteLicenseConditionMutation();

  // Form setup
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
    watch,
    setValue,
    control
  } = useForm<LicenseFormData>({
    resolver: yupResolver(schema) as any,
    defaultValues: {
      renewalRequired: false,
      autoRenewal: false,
      isCriticalLicense: false,
      requiresInsurance: false,
      currency: 'USD',
      renewalPeriodDays: 365,
      licenseConditions: []
    }
  });

  const { fields: conditionFields, append: appendCondition, remove: removeCondition } = useFieldArray({
    control,
    name: 'licenseConditions'
  });

  // Watch for changes
  const watchRenewalRequired = watch('renewalRequired');
  const watchRequiresInsurance = watch('requiresInsurance');

  // Load license data into form when license is loaded
  useEffect(() => {
    if (license && !hasLoadedConditions.current) {
      console.log('Loading license data:', license);
      console.log('License conditions:', license.conditions);
      
      const formData: LicenseFormData = {
        title: license.title,
        description: license.description,
        type: license.type,
        priority: license.priority,
        licenseNumber: license.licenseNumber,
        issuingAuthority: license.issuingAuthority,
        holderName: license.holderName,
        department: license.department,
        issuedDate: format(new Date(license.issuedDate), 'yyyy-MM-dd'),
        expiryDate: format(new Date(license.expiryDate), 'yyyy-MM-dd'),
        scope: license.scope,
        restrictions: license.restrictions,
        conditions: license.conditionsText,
        riskLevel: license.riskLevel,
        licenseFee: license.licenseFee,
        currency: license.currency,
        isCriticalLicense: license.isCriticalLicense,
        requiresInsurance: license.requiresInsurance,
        requiredInsuranceAmount: license.requiredInsuranceAmount,
        regulatoryFramework: license.regulatoryFramework,
        applicableRegulations: license.applicableRegulations,
        complianceStandards: license.complianceStandards,
        renewalRequired: license.renewalRequired,
        renewalPeriodDays: license.renewalPeriodDays,
        autoRenewal: license.autoRenewal,
        renewalProcedure: license.renewalProcedure,
        licenseConditions: license.conditions.map(c => ({
          conditionType: c.conditionType,
          description: c.description,
          isMandatory: c.isMandatory,
          dueDate: c.dueDate ? format(new Date(c.dueDate), 'yyyy-MM-dd') : undefined,
          responsiblePerson: c.responsiblePerson,
          notes: c.notes
        }))
      };
      
      // Reset the form with the new data
      reset(formData);
      
      // Clear existing conditions first
      if (conditionFields.length > 0) {
        for (let i = conditionFields.length - 1; i >= 0; i--) {
          removeCondition(i);
        }
      }
      
      // Add license conditions
      license.conditions.forEach((condition) => {
        appendCondition({
          conditionType: condition.conditionType,
          description: condition.description,
          isMandatory: condition.isMandatory,
          dueDate: condition.dueDate ? format(new Date(condition.dueDate), 'yyyy-MM-dd') : undefined,
          responsiblePerson: condition.responsiblePerson,
          notes: condition.notes
        });
      });
      
      hasLoadedConditions.current = true;
    }
  }, [license, reset, removeCondition, appendCondition]);
  
  // Reset the loaded flag when license ID changes
  useEffect(() => {
    hasLoadedConditions.current = false;
  }, [licenseId]);

  // Handle form submission
  const onSubmit = useCallback(async (data: LicenseFormData) => {
    try {
      // Map form data to UpdateLicenseCommand structure with proper enum conversions
      const updateCommand = {
        id: licenseId,
        title: data.title,
        description: data.description,
        priority: getPriorityEnum(data.priority),
        scope: data.scope,
        restrictions: data.restrictions,
        conditions: data.conditions,
        regulatoryFramework: data.regulatoryFramework,
        applicableRegulations: data.applicableRegulations,
        complianceStandards: data.complianceStandards,
        riskLevel: getRiskLevelEnum(data.riskLevel),
        isCriticalLicense: data.isCriticalLicense,
        requiresInsurance: data.requiresInsurance,
        requiredInsuranceAmount: data.requiredInsuranceAmount,
        renewalRequired: data.renewalRequired,
        renewalPeriodDays: data.renewalPeriodDays,
        autoRenewal: data.autoRenewal,
        renewalProcedure: data.renewalProcedure
      };

      console.log('Update command being sent:', updateCommand);

      // Update the main license data
      await updateLicense({
        id: licenseId,
        data: updateCommand
      }).unwrap();

      console.log('License updated successfully, now handling conditions...');

      // Handle license conditions separately
      await handleLicenseConditions(data.licenseConditions);

      console.log('All updates completed successfully!');

      setShowSuccess(true);
      setTimeout(() => {
        navigate(`/licenses/${licenseId}`);
      }, 2000);
    } catch (error: any) {
      console.error('Error updating license:', error);
      setErrorMessage(error?.data?.message || 'Failed to update license. Please try again.');
      setShowError(true);
    }
  }, [updateLicense, licenseId, navigate]);

  // Handle license conditions synchronization
  const handleLicenseConditions = useCallback(async (formConditions: LicenseConditionFormData[]) => {
    if (!license) return;

    console.log('Syncing conditions:', { 
      existing: license.conditions, 
      form: formConditions 
    });

    // Create maps for efficient lookups
    const existingConditionsMap = new Map(
      license.conditions.map((condition, index) => [index, condition])
    );

    // Handle updates and additions
    for (const [index, formCondition] of formConditions.entries()) {
      try {
        const existingCondition = existingConditionsMap.get(index);
        
        if (existingCondition && existingCondition.id) {
          // Update existing condition
          console.log(`Updating condition ${existingCondition.id}:`, formCondition);
          await updateLicenseCondition({
            licenseId: licenseId,
            conditionId: existingCondition.id,
            conditionType: formCondition.conditionType,
            description: formCondition.description,
            isMandatory: formCondition.isMandatory,
            dueDate: formCondition.dueDate,
            responsiblePerson: formCondition.responsiblePerson,
            notes: formCondition.notes
          }).unwrap();
        } else {
          // Add new condition
          console.log('Adding new condition:', formCondition);
          await addLicenseCondition({
            licenseId: licenseId,
            conditionType: formCondition.conditionType,
            description: formCondition.description,
            isMandatory: formCondition.isMandatory,
            dueDate: formCondition.dueDate,
            responsiblePerson: formCondition.responsiblePerson,
            notes: formCondition.notes
          }).unwrap();
        }
      } catch (error) {
        console.error(`Error handling condition ${index}:`, error);
        throw error; // Re-throw to be caught by the main handler
      }
    }

    // Handle deletions - conditions that exist in DB but not in form
    for (const [index, existingCondition] of license.conditions.entries()) {
      if (index >= formConditions.length && existingCondition.id) {
        try {
          console.log(`Deleting condition ${existingCondition.id}`);
          await deleteLicenseCondition({
            licenseId: licenseId,
            conditionId: existingCondition.id
          }).unwrap();
        } catch (error) {
          console.error(`Error deleting condition ${existingCondition.id}:`, error);
          throw error;
        }
      }
    }

    console.log('License conditions synchronized successfully');
  }, [license, licenseId, addLicenseCondition, updateLicenseCondition, deleteLicenseCondition]);

  // Handle adding new condition
  const handleAddCondition = useCallback(() => {
    appendCondition({
      conditionType: '',
      description: '',
      isMandatory: false,
      responsiblePerson: '',
      notes: ''
    });
  }, [appendCondition]);

  if (isLoadingLicense) {
    return (
      <div className="text-center p-4">
        <CSpinner />
        <div className="mt-2">Loading license details...</div>
      </div>
    );
  }

  if (licenseError || !license) {
    return (
      <CRow>
        <CCol xs={12}>
          <CAlert color="danger">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
            License not found or error loading details.
          </CAlert>
        </CCol>
      </CRow>
    );
  }

  if (!license.canEdit) {
    return (
      <CRow>
        <CCol xs={12}>
          <CAlert color="warning">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
            This license cannot be edited in its current status.
          </CAlert>
          <CButton color="secondary" onClick={() => navigate(`/licenses/${licenseId}`)}>
            <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
            Back to License Details
          </CButton>
        </CCol>
      </CRow>
    );
  }

  return (
    <CForm onSubmit={handleSubmit(onSubmit as any)}>
      <CRow>
        <CCol xs={12}>
          {/* Success Alert */}
          {showSuccess && (
            <CAlert color="success" dismissible onClose={() => setShowSuccess(false)}>
              <FontAwesomeIcon icon={faCertificate} className="me-2" />
              License updated successfully! Redirecting to license details...
            </CAlert>
          )}

          {/* Error Alert */}
          {showError && (
            <CAlert color="danger" dismissible onClose={() => setShowError(false)}>
              <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
              {errorMessage}
            </CAlert>
          )}

          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div className="d-flex align-items-center">
                <CButton
                  variant="outline"
                  color="secondary"
                  className="me-3"
                  onClick={() => navigate(`/licenses/${licenseId}`)}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
                  Back to License Details
                </CButton>
                <div>
                  <h4 className="mb-0">
                    <FontAwesomeIcon icon={faEdit} className="me-2" />
                    Edit License
                  </h4>
                  <div className="text-muted">
                    License #{license.licenseNumber} • {license.typeDisplay || license.type}
                  </div>
                </div>
              </div>
              <div className="d-flex gap-2">
                <CButton
                  type="submit"
                  color="primary"
                  disabled={isSubmitting || isUpdating}
                >
                  {(isSubmitting || isUpdating) ? (
                    <>
                      <CSpinner size="sm" className="me-1" />
                      Updating...
                    </>
                  ) : (
                    <>
                      <FontAwesomeIcon icon={faSave} className="me-1" />
                      Update License
                    </>
                  )}
                </CButton>
              </div>
            </CCardHeader>

            <CCardBody>
              {/* Information Alert */}
              <CAlert color="info" className="mb-4">
                <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                <strong>Edit License Information:</strong> Some core license details (like license number, type, issuing authority, and dates) cannot be modified after creation for compliance reasons. Only business-related fields can be updated.
              </CAlert>

              <CAccordion>
                {/* Basic Information */}
                <CAccordionItem itemKey="basic">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                    Basic Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="title">Title *</CFormLabel>
                          <CFormInput
                            id="title"
                            type="text"
                            {...register('title')}
                            invalid={!!errors.title}
                            placeholder="Enter license title"
                          />
                          {errors.title && (
                            <div className="invalid-feedback">{errors.title.message}</div>
                          )}
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="licenseNumber">License Number *</CFormLabel>
                          <CFormInput
                            id="licenseNumber"
                            type="text"
                            {...register('licenseNumber')}
                            invalid={!!errors.licenseNumber}
                            placeholder="Enter license number"
                            disabled
                          />
                          <small className="text-muted">License number cannot be changed after creation</small>
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="type">License Type *</CFormLabel>
                          <CFormSelect
                            id="type"
                            {...register('type')}
                            invalid={!!errors.type}
                            disabled
                          >
                            <option value="">Select type...</option>
                            {LICENSE_TYPES.map(type => (
                              <option key={type.value} value={type.value}>
                                {type.label}
                              </option>
                            ))}
                          </CFormSelect>
                          <small className="text-muted">Type cannot be changed</small>
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
                            <option value="">Select priority...</option>
                            {LICENSE_PRIORITIES.map(priority => (
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
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="riskLevel">Risk Level *</CFormLabel>
                          <CFormSelect
                            id="riskLevel"
                            {...register('riskLevel')}
                            invalid={!!errors.riskLevel}
                          >
                            <option value="">Select risk level...</option>
                            {RISK_LEVELS.map(level => (
                              <option key={level.value} value={level.value}>
                                {level.label}
                              </option>
                            ))}
                          </CFormSelect>
                          {errors.riskLevel && (
                            <div className="invalid-feedback">{errors.riskLevel.message}</div>
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
                        placeholder="Enter detailed description of the license"
                      />
                      {errors.description && (
                        <div className="invalid-feedback">{errors.description.message}</div>
                      )}
                    </div>
                  </CAccordionBody>
                </CAccordionItem>

                {/* License Details */}
                <CAccordionItem itemKey="details">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faFileContract} className="me-2" />
                    License Details
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="issuingAuthority">Issuing Authority *</CFormLabel>
                          <CFormInput
                            id="issuingAuthority"
                            type="text"
                            {...register('issuingAuthority')}
                            invalid={!!errors.issuingAuthority}
                            placeholder="Enter issuing authority"
                            disabled
                          />
                          <small className="text-muted">Cannot be changed after creation</small>
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="holderName">Holder Name *</CFormLabel>
                          <CFormInput
                            id="holderName"
                            type="text"
                            {...register('holderName')}
                            invalid={!!errors.holderName}
                            placeholder="Enter license holder name"
                            disabled
                          />
                          <small className="text-muted">Cannot be changed after creation</small>
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="department">Department *</CFormLabel>
                          <CFormSelect
                            id="department"
                            {...register('department')}
                            invalid={!!errors.department}
                            disabled
                          >
                            <option value="">Select department...</option>
                            {DEPARTMENTS.map(dept => (
                              <option key={dept.id} value={dept.name}>
                                {dept.name}
                              </option>
                            ))}
                          </CFormSelect>
                          <small className="text-muted">Cannot be changed</small>
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="issuedDate">Issued Date *</CFormLabel>
                          <CFormInput
                            id="issuedDate"
                            type="date"
                            {...register('issuedDate')}
                            invalid={!!errors.issuedDate}
                            disabled
                          />
                          <small className="text-muted">Cannot be changed</small>
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="expiryDate">Expiry Date *</CFormLabel>
                          <CFormInput
                            id="expiryDate"
                            type="date"
                            {...register('expiryDate')}
                            invalid={!!errors.expiryDate}
                            disabled
                          />
                          <small className="text-muted">Cannot be changed</small>
                        </div>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Financial Information */}
                <CAccordionItem itemKey="financial">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faBuilding} className="me-2" />
                    Financial Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="licenseFee">License Fee</CFormLabel>
                          <CInputGroup>
                            <CFormInput
                              id="licenseFee"
                              type="number"
                              step="0.01"
                              min="0"
                              {...register('licenseFee', { valueAsNumber: true })}
                              placeholder="0.00"
                              disabled
                            />
                            <CInputGroupText>
                              <CFormSelect
                                style={{ border: 'none', background: 'transparent' }}
                                {...register('currency')}
                                disabled
                              >
                                {CURRENCIES.map(currency => (
                                  <option key={currency.value} value={currency.value}>
                                    {currency.value}
                                  </option>
                                ))}
                              </CFormSelect>
                            </CInputGroupText>
                          </CInputGroup>
                          <small className="text-muted">Fee and currency cannot be changed</small>
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormCheck
                            id="isCriticalLicense"
                            label="Critical License"
                            {...register('isCriticalLicense')}
                          />
                          <small className="text-muted">
                            Mark as critical if this license is essential for operations
                          </small>
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormCheck
                            id="requiresInsurance"
                            label="Requires Insurance"
                            {...register('requiresInsurance')}
                          />
                          <small className="text-muted">
                            Check if insurance coverage is required
                          </small>
                        </div>
                      </CCol>
                    </CRow>

                    {watchRequiresInsurance && (
                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="requiredInsuranceAmount">Required Insurance Amount</CFormLabel>
                            <CFormInput
                              id="requiredInsuranceAmount"
                              type="number"
                              step="0.01"
                              min="0"
                              {...register('requiredInsuranceAmount', { valueAsNumber: true })}
                              placeholder="Enter required insurance amount"
                            />
                          </div>
                        </CCol>
                      </CRow>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* Scope and Restrictions */}
                <CAccordionItem itemKey="scope">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                    Scope and Restrictions
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <CFormLabel htmlFor="scope">Scope *</CFormLabel>
                      <CFormTextarea
                        id="scope"
                        rows={3}
                        {...register('scope')}
                        invalid={!!errors.scope}
                        placeholder="Define the scope and coverage areas of this license"
                      />
                      {errors.scope && (
                        <div className="invalid-feedback">{errors.scope.message}</div>
                      )}
                    </div>

                    <div className="mb-3">
                      <CFormLabel htmlFor="restrictions">Restrictions</CFormLabel>
                      <CFormTextarea
                        id="restrictions"
                        rows={3}
                        {...register('restrictions')}
                        placeholder="Enter any restrictions or limitations"
                      />
                    </div>

                    <div className="mb-3">
                      <CFormLabel htmlFor="conditions">Conditions</CFormLabel>
                      <CFormTextarea
                        id="conditions"
                        rows={3}
                        {...register('conditions')}
                        placeholder="Enter license conditions and requirements"
                      />
                    </div>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Regulatory Information */}
                <CAccordionItem itemKey="regulatory">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faGavel} className="me-2" />
                    Regulatory Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <CFormLabel htmlFor="regulatoryFramework">Regulatory Framework *</CFormLabel>
                      <CFormInput
                        id="regulatoryFramework"
                        type="text"
                        {...register('regulatoryFramework')}
                        invalid={!!errors.regulatoryFramework}
                        placeholder="Enter applicable regulatory framework"
                      />
                      {errors.regulatoryFramework && (
                        <div className="invalid-feedback">{errors.regulatoryFramework.message}</div>
                      )}
                    </div>

                    <div className="mb-3">
                      <CFormLabel htmlFor="applicableRegulations">Applicable Regulations</CFormLabel>
                      <CFormTextarea
                        id="applicableRegulations"
                        rows={3}
                        {...register('applicableRegulations')}
                        placeholder="List applicable regulations and laws"
                      />
                    </div>

                    <div className="mb-3">
                      <CFormLabel htmlFor="complianceStandards">Compliance Standards</CFormLabel>
                      <CFormTextarea
                        id="complianceStandards"
                        rows={3}
                        {...register('complianceStandards')}
                        placeholder="Enter relevant compliance standards and certifications"
                      />
                    </div>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Renewal Information */}
                <CAccordionItem itemKey="renewal">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faCalendarAlt} className="me-2" />
                    Renewal Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormCheck
                            id="renewalRequired"
                            label="Renewal Required"
                            {...register('renewalRequired')}
                          />
                          <small className="text-muted">
                            Check if this license requires periodic renewal
                          </small>
                        </div>
                      </CCol>
                      {watchRenewalRequired && (
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormCheck
                              id="autoRenewal"
                              label="Auto Renewal"
                              {...register('autoRenewal')}
                            />
                            <small className="text-muted">
                              Enable automatic renewal process
                            </small>
                          </div>
                        </CCol>
                      )}
                    </CRow>

                    {watchRenewalRequired && (
                      <>
                        <CRow>
                          <CCol md={6}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="renewalPeriodDays">Renewal Period (Days) *</CFormLabel>
                              <CFormInput
                                id="renewalPeriodDays"
                                type="number"
                                min="1"
                                max="3650"
                                {...register('renewalPeriodDays', { valueAsNumber: true })}
                                invalid={!!errors.renewalPeriodDays}
                                placeholder="365"
                              />
                              {errors.renewalPeriodDays && (
                                <div className="invalid-feedback">{errors.renewalPeriodDays.message}</div>
                              )}
                              <small className="text-muted">
                                Number of days before expiry to start renewal process
                              </small>
                            </div>
                          </CCol>
                        </CRow>

                        <div className="mb-3">
                          <CFormLabel htmlFor="renewalProcedure">Renewal Procedure</CFormLabel>
                          <CFormTextarea
                            id="renewalProcedure"
                            rows={3}
                            {...register('renewalProcedure')}
                            placeholder="Describe the renewal process and requirements"
                          />
                        </div>
                      </>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* License Conditions */}
                <CAccordionItem itemKey="conditions">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faTasks} className="me-2" />
                    License Conditions ({conditionFields.length})
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="d-flex justify-content-between align-items-center mb-3">
                      <h6 className="mb-0">Specific License Conditions</h6>
                      <CButton
                        color="primary"
                        variant="outline"
                        size="sm"
                        onClick={handleAddCondition}
                      >
                        <FontAwesomeIcon icon={faPlus} className="me-1" />
                        Add Condition
                      </CButton>
                    </div>

                    {conditionFields.length === 0 ? (
                      <CAlert color="info">
                        <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                        No specific conditions added. Click "Add Condition" to add license-specific requirements.
                      </CAlert>
                    ) : (
                      <CTable responsive>
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Type</CTableHeaderCell>
                            <CTableHeaderCell>Description</CTableHeaderCell>
                            <CTableHeaderCell>Mandatory</CTableHeaderCell>
                            <CTableHeaderCell>Due Date</CTableHeaderCell>
                            <CTableHeaderCell>Responsible Person</CTableHeaderCell>
                            <CTableHeaderCell>Actions</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {conditionFields.map((field, index) => (
                            <CTableRow key={field.id}>
                              <CTableDataCell>
                                <CFormSelect
                                  {...register(`licenseConditions.${index}.conditionType`)}
                                  size="sm"
                                >
                                  <option value="">Select type...</option>
                                  {CONDITION_TYPES.map((type) => (
                                    <option key={type.value} value={type.value}>
                                      {type.label}
                                    </option>
                                  ))}
                                </CFormSelect>
                              </CTableDataCell>
                              <CTableDataCell>
                                <CFormInput
                                  {...register(`licenseConditions.${index}.description`)}
                                  placeholder="Condition description"
                                  size="sm"
                                />
                              </CTableDataCell>
                              <CTableDataCell>
                                <CFormCheck
                                  {...register(`licenseConditions.${index}.isMandatory`)}
                                />
                              </CTableDataCell>
                              <CTableDataCell>
                                <CFormInput
                                  type="date"
                                  {...register(`licenseConditions.${index}.dueDate`)}
                                  size="sm"
                                />
                              </CTableDataCell>
                              <CTableDataCell>
                                <CFormInput
                                  {...register(`licenseConditions.${index}.responsiblePerson`)}
                                  placeholder="Responsible person"
                                  size="sm"
                                />
                              </CTableDataCell>
                              <CTableDataCell>
                                <CButton
                                  color="danger"
                                  variant="outline"
                                  size="sm"
                                  onClick={() => removeCondition(index)}
                                >
                                  <FontAwesomeIcon icon={faTrash} />
                                </CButton>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* Attachments */}
                <CAccordionItem itemKey="attachments">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={faPaperclip} className="me-2" />
                    Document Attachments ({license.attachments.length})
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CAlert color="info">
                      <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                      Current attachments are displayed below. To manage attachments, please use the main license detail page.
                    </CAlert>

                    {license.attachments.length === 0 ? (
                      <CAlert color="warning">
                        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                        No documents attached to this license.
                      </CAlert>
                    ) : (
                      <CTable responsive>
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>File Name</CTableHeaderCell>
                            <CTableHeaderCell>Type</CTableHeaderCell>
                            <CTableHeaderCell>Size</CTableHeaderCell>
                            <CTableHeaderCell>Uploaded</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {license.attachments.map((attachment) => (
                            <CTableRow key={attachment.id}>
                              <CTableDataCell>
                                <FontAwesomeIcon icon={faDownload} className="me-2" />
                                {attachment.originalFileName}
                              </CTableDataCell>
                              <CTableDataCell>
                                <CBadge color="info">
                                  {attachment.attachmentTypeDisplay || attachment.attachmentType}
                                </CBadge>
                              </CTableDataCell>
                              <CTableDataCell>
                                {(attachment.fileSize / 1024).toFixed(1)} KB
                              </CTableDataCell>
                              <CTableDataCell>
                                {format(new Date(attachment.uploadedAt), 'MMM dd, yyyy')}
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    )}
                  </CAccordionBody>
                </CAccordionItem>
              </CAccordion>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </CForm>
  );
};

export default EditLicense;