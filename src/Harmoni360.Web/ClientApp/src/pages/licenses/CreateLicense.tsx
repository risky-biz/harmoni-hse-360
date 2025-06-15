import React, { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
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
  faFileContract,
  faCalendarAlt,
  faCertificate,
  faCheck,
  faPlus,
  faTrash,
  faCog,
  faBuilding,
  faGavel,
  faClipboardCheck,
  faRedo
} from '@fortawesome/free-solid-svg-icons';

import { useCreateLicenseMutation } from '../../features/licenses/licenseApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import {
  LicenseFormData,
  LicenseConditionFormData,
  LICENSE_TYPES,
  LICENSE_PRIORITIES,
  RISK_LEVELS,
  CONDITION_TYPES,
  CURRENCIES
} from '../../types/license';
import { format, addDays } from 'date-fns';

// Validation schema
const schema = yup.object({
  title: yup.string().required('Title is required').max(200, 'Title must not exceed 200 characters'),
  description: yup.string().required('Description is required').max(2000, 'Description must not exceed 2000 characters'),
  type: yup.string().required('License type is required'),
  licenseNumber: yup.string().required('License number is required').max(50, 'License number must not exceed 50 characters'),
  issuingAuthority: yup.string().required('Issuing authority is required').max(200, 'Issuing authority must not exceed 200 characters'),
  holderName: yup.string().required('License holder name is required').max(100, 'Holder name must not exceed 100 characters'),
  department: yup.string().required('Department is required'),
  issuedDate: yup.string().required('Issued date is required'),
  expiryDate: yup.string()
    .required('Expiry date is required')
    .test('is-after-issued', 'Expiry date must be after issued date', function(value) {
      const { issuedDate } = this.parent;
      if (!value || !issuedDate) return true;
      
      const issued = new Date(issuedDate + 'T00:00:00');
      const expiry = new Date(value + 'T00:00:00');
      
      return expiry > issued;
    }),
  scope: yup.string().max(2000, 'Scope must not exceed 2000 characters'),
  restrictions: yup.string().max(2000, 'Restrictions must not exceed 2000 characters'),
  conditions: yup.string().max(2000, 'Conditions must not exceed 2000 characters'),
  licenseFee: yup.number().min(0, 'License fee cannot be negative'),
  requiredInsuranceAmount: yup.number()
    .when('requiresInsurance', {
      is: true,
      then: (schema) => schema.required('Insurance amount is required when insurance is required').min(0, 'Insurance amount cannot be negative'),
      otherwise: (schema) => schema.min(0, 'Insurance amount cannot be negative')
    }),
  renewalPeriodDays: yup.number()
    .when('renewalRequired', {
      is: true,
      then: (schema) => schema.required('Renewal period is required').min(1, 'Renewal period must be at least 1 day').max(1095, 'Renewal period cannot exceed 3 years'),
      otherwise: (schema) => schema.min(0, 'Renewal period cannot be negative')
    }),
  renewalProcedure: yup.string()
    .when('renewalRequired', {
      is: true,
      then: (schema) => schema.required('Renewal procedure is required when renewal is required'),
      otherwise: (schema) => schema
    })
    .max(2000, 'Renewal procedure must not exceed 2000 characters'),
});

// License Management Icon Mappings
const LICENSE_ICONS = {
  // Main license icon
  license: faFileContract,
  
  // Section icons following Harmoni360 standards
  basicInformation: faInfoCircle,
  licenseDetails: faCertificate,
  regulatory: faGavel,
  riskCompliance: faShieldAlt,
  renewalInfo: faRedo,
  conditions: faClipboardCheck,
  reviewSubmit: faCheck,
  
  // Action icons
  create: faPlus,
  save: faSave,
  back: faArrowLeft,
  settings: faCog,
  building: faBuilding,
  calendar: faCalendarAlt
};

const CreateLicense: React.FC = () => {
  const navigate = useNavigate();

  // API calls
  const { data: departments } = useGetDepartmentsQuery({});
  const [createLicense, { isLoading }] = useCreateLicenseMutation();

  // Form state
  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
    control,
    getValues,
  } = useForm<LicenseFormData>({
    resolver: yupResolver(schema) as any,
    defaultValues: {
      title: '',
      description: '',
      type: 'Environmental',
      priority: 'Medium',
      licenseNumber: '',
      issuingAuthority: '',
      holderName: '',
      department: '',
      issuedDate: format(new Date(), 'yyyy-MM-dd'),
      expiryDate: format(addDays(new Date(), 365), 'yyyy-MM-dd'),
      scope: '',
      restrictions: '',
      conditions: '',
      riskLevel: 'Medium',
      licenseFee: undefined,
      currency: 'USD',
      isCriticalLicense: false,
      requiresInsurance: false,
      requiredInsuranceAmount: undefined,
      regulatoryFramework: '',
      applicableRegulations: '',
      complianceStandards: '',
      renewalRequired: true,
      renewalPeriodDays: 90,
      autoRenewal: false,
      renewalProcedure: '',
      licenseConditions: []
    },
  });

  // License conditions field array
  const {
    fields: conditionFields,
    append: appendCondition,
    remove: removeCondition,
  } = useFieldArray({
    control,
    name: 'licenseConditions',
  });

  // Watch values for conditional fields
  const watchRenewalRequired = watch('renewalRequired');
  const watchRequiresInsurance = watch('requiresInsurance');
  const watchType = watch('type');
  const watchRiskLevel = watch('riskLevel');

  // State for form sections
  const [showSuccess, setShowSuccess] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  // Form submission
  const onSubmit = useCallback(async (data: LicenseFormData) => {
    try {
      setErrorMessage('');
      
      const result = await createLicense(data).unwrap();
      
      setShowSuccess(true);
      
      // Navigate to license detail after a brief delay
      setTimeout(() => {
        navigate(`/licenses/${result.id}`);
      }, 2000);
      
    } catch (error: any) {
      console.error('Error creating license:', error);
      setErrorMessage(error?.data?.detail || error?.data?.title || 'Failed to create license. Please try again.');
    }
  }, [createLicense, navigate]);

  // Add condition
  const handleAddCondition = useCallback(() => {
    appendCondition({
      conditionType: 'Inspection',
      description: '',
      isMandatory: true,
      dueDate: '',
      responsiblePerson: '',
      notes: ''
    });
  }, [appendCondition]);

  // Calculate days until expiry
  const calculateDaysUntilExpiry = useCallback(() => {
    const expiryDate = getValues('expiryDate');
    if (!expiryDate) return 0;
    
    const expiry = new Date(expiryDate);
    const today = new Date();
    const diffTime = expiry.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    return diffDays;
  }, [getValues]);

  if (showSuccess) {
    return (
      <CRow>
        <CCol xs={12}>
          <CAlert color="success" className="d-flex align-items-center">
            <FontAwesomeIcon icon={faCheck} className="me-2" />
            License created successfully! Redirecting to license details...
          </CAlert>
        </CCol>
      </CRow>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div className="d-flex align-items-center">
              <FontAwesomeIcon icon={LICENSE_ICONS.license} className="me-2" />
              <h4 className="mb-0">Create New License</h4>
            </div>
            <CButton
              variant="outline"
              color="secondary"
              onClick={() => navigate('/licenses')}
              disabled={isLoading}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.back} className="me-1" />
              Back to Licenses
            </CButton>
          </CCardHeader>
          <CCardBody>
            {errorMessage && (
              <CAlert color="danger" className="mb-3">
                <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                {errorMessage}
              </CAlert>
            )}

            <CForm onSubmit={handleSubmit(onSubmit)}>
              <CAccordion activeItemKey="basicInfo">
                {/* Basic Information */}
                <CAccordionItem itemKey="basicInfo">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={LICENSE_ICONS.basicInformation} className="me-2" />
                    Basic Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="title">
                            License Title <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormInput
                            id="title"
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
                          <CFormLabel htmlFor="licenseNumber">
                            License Number <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormInput
                            id="licenseNumber"
                            {...register('licenseNumber')}
                            invalid={!!errors.licenseNumber}
                            placeholder="Enter license number"
                          />
                          {errors.licenseNumber && (
                            <div className="invalid-feedback">{errors.licenseNumber.message}</div>
                          )}
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="type">
                            License Type <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormSelect
                            id="type"
                            {...register('type')}
                            invalid={!!errors.type}
                          >
                            {LICENSE_TYPES.map((type) => (
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
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="priority">Priority</CFormLabel>
                          <CFormSelect
                            id="priority"
                            {...register('priority')}
                          >
                            {LICENSE_PRIORITIES.map((priority) => (
                              <option key={priority.value} value={priority.value}>
                                {priority.label}
                              </option>
                            ))}
                          </CFormSelect>
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="riskLevel">Risk Level</CFormLabel>
                          <CFormSelect
                            id="riskLevel"
                            {...register('riskLevel')}
                          >
                            {RISK_LEVELS.map((risk) => (
                              <option key={risk.value} value={risk.value}>
                                {risk.label}
                              </option>
                            ))}
                          </CFormSelect>
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol xs={12}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="description">
                            Description <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormTextarea
                            id="description"
                            {...register('description')}
                            invalid={!!errors.description}
                            rows={3}
                            placeholder="Enter license description"
                          />
                          {errors.description && (
                            <div className="invalid-feedback">{errors.description.message}</div>
                          )}
                        </div>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* License Details */}
                <CAccordionItem itemKey="licenseDetails">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={LICENSE_ICONS.licenseDetails} className="me-2" />
                    License Details
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="issuingAuthority">
                            Issuing Authority <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormInput
                            id="issuingAuthority"
                            {...register('issuingAuthority')}
                            invalid={!!errors.issuingAuthority}
                            placeholder="Enter issuing authority"
                          />
                          {errors.issuingAuthority && (
                            <div className="invalid-feedback">{errors.issuingAuthority.message}</div>
                          )}
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="holderName">
                            License Holder <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormInput
                            id="holderName"
                            {...register('holderName')}
                            invalid={!!errors.holderName}
                            placeholder="Enter license holder name"
                          />
                          {errors.holderName && (
                            <div className="invalid-feedback">{errors.holderName.message}</div>
                          )}
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="department">
                            Department <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormSelect
                            id="department"
                            {...register('department')}
                            invalid={!!errors.department}
                          >
                            <option value="">Select Department</option>
                            {departments?.items?.map((dept) => (
                              <option key={dept.id} value={dept.name}>
                                {dept.name}
                              </option>
                            ))}
                          </CFormSelect>
                          {errors.department && (
                            <div className="invalid-feedback">{errors.department.message}</div>
                          )}
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="issuedDate">
                            Issued Date <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormInput
                            id="issuedDate"
                            type="date"
                            {...register('issuedDate')}
                            invalid={!!errors.issuedDate}
                          />
                          {errors.issuedDate && (
                            <div className="invalid-feedback">{errors.issuedDate.message}</div>
                          )}
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="expiryDate">
                            Expiry Date <span className="text-danger">*</span>
                          </CFormLabel>
                          <CFormInput
                            id="expiryDate"
                            type="date"
                            {...register('expiryDate')}
                            invalid={!!errors.expiryDate}
                          />
                          {errors.expiryDate && (
                            <div className="invalid-feedback">{errors.expiryDate.message}</div>
                          )}
                          <small className="text-muted">
                            {calculateDaysUntilExpiry()} days until expiry
                          </small>
                        </div>
                      </CCol>
                    </CRow>

                    <CRow>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="licenseFee">License Fee</CFormLabel>
                          <CInputGroup>
                            <CFormSelect
                              {...register('currency')}
                              style={{ maxWidth: '100px' }}
                            >
                              {CURRENCIES.map((currency) => (
                                <option key={currency.value} value={currency.value}>
                                  {currency.value}
                                </option>
                              ))}
                            </CFormSelect>
                            <CFormInput
                              id="licenseFee"
                              type="number"
                              step="0.01"
                              {...register('licenseFee', { valueAsNumber: true })}
                              invalid={!!errors.licenseFee}
                              placeholder="0.00"
                            />
                          </CInputGroup>
                          {errors.licenseFee && (
                            <div className="invalid-feedback">{errors.licenseFee.message}</div>
                          )}
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormCheck
                            id="isCriticalLicense"
                            {...register('isCriticalLicense')}
                            label="Critical License"
                          />
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormCheck
                            id="requiresInsurance"
                            {...register('requiresInsurance')}
                            label="Requires Insurance"
                          />
                        </div>
                      </CCol>
                    </CRow>

                    {watchRequiresInsurance && (
                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="requiredInsuranceAmount">
                              Required Insurance Amount <span className="text-danger">*</span>
                            </CFormLabel>
                            <CInputGroup>
                              <CInputGroupText>{getValues('currency')}</CInputGroupText>
                              <CFormInput
                                id="requiredInsuranceAmount"
                                type="number"
                                step="0.01"
                                {...register('requiredInsuranceAmount', { valueAsNumber: true })}
                                invalid={!!errors.requiredInsuranceAmount}
                                placeholder="0.00"
                              />
                            </CInputGroup>
                            {errors.requiredInsuranceAmount && (
                              <div className="invalid-feedback">{errors.requiredInsuranceAmount.message}</div>
                            )}
                          </div>
                        </CCol>
                      </CRow>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* Scope and Restrictions */}
                <CAccordionItem itemKey="scopeRestrictions">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={LICENSE_ICONS.settings} className="me-2" />
                    Scope and Restrictions
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol xs={12}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="scope">Scope</CFormLabel>
                          <CFormTextarea
                            id="scope"
                            {...register('scope')}
                            invalid={!!errors.scope}
                            rows={3}
                            placeholder="Describe the scope of the license"
                          />
                          {errors.scope && (
                            <div className="invalid-feedback">{errors.scope.message}</div>
                          )}
                        </div>
                      </CCol>
                    </CRow>
                    
                    <CRow>
                      <CCol xs={12}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="restrictions">Restrictions</CFormLabel>
                          <CFormTextarea
                            id="restrictions"
                            {...register('restrictions')}
                            invalid={!!errors.restrictions}
                            rows={3}
                            placeholder="Describe any restrictions or limitations"
                          />
                          {errors.restrictions && (
                            <div className="invalid-feedback">{errors.restrictions.message}</div>
                          )}
                        </div>
                      </CCol>
                    </CRow>
                    
                    <CRow>
                      <CCol xs={12}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="conditions">General Conditions</CFormLabel>
                          <CFormTextarea
                            id="conditions"
                            {...register('conditions')}
                            invalid={!!errors.conditions}
                            rows={3}
                            placeholder="Describe general conditions and requirements"
                          />
                          {errors.conditions && (
                            <div className="invalid-feedback">{errors.conditions.message}</div>
                          )}
                        </div>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Regulatory Information */}
                <CAccordionItem itemKey="regulatory">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={LICENSE_ICONS.regulatory} className="me-2" />
                    Regulatory Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="regulatoryFramework">Regulatory Framework</CFormLabel>
                          <CFormInput
                            id="regulatoryFramework"
                            {...register('regulatoryFramework')}
                            placeholder="e.g., Environmental Protection Act"
                          />
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="applicableRegulations">Applicable Regulations</CFormLabel>
                          <CFormInput
                            id="applicableRegulations"
                            {...register('applicableRegulations')}
                            placeholder="e.g., EPA Regulation 123/2024"
                          />
                        </div>
                      </CCol>
                    </CRow>
                    
                    <CRow>
                      <CCol xs={12}>
                        <div className="mb-3">
                          <CFormLabel htmlFor="complianceStandards">Compliance Standards</CFormLabel>
                          <CFormTextarea
                            id="complianceStandards"
                            {...register('complianceStandards')}
                            rows={3}
                            placeholder="List applicable compliance standards (one per line)"
                          />
                        </div>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Renewal Information */}
                <CAccordionItem itemKey="renewal">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={LICENSE_ICONS.renewalInfo} className="me-2" />
                    Renewal Information
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormCheck
                            id="renewalRequired"
                            {...register('renewalRequired')}
                            label="Renewal Required"
                          />
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormCheck
                            id="autoRenewal"
                            {...register('autoRenewal')}
                            label="Auto Renewal"
                            disabled={!watchRenewalRequired}
                          />
                        </div>
                      </CCol>
                    </CRow>

                    {watchRenewalRequired && (
                      <>
                        <CRow>
                          <CCol md={6}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="renewalPeriodDays">
                                Renewal Period (Days) <span className="text-danger">*</span>
                              </CFormLabel>
                              <CFormInput
                                id="renewalPeriodDays"
                                type="number"
                                {...register('renewalPeriodDays', { valueAsNumber: true })}
                                invalid={!!errors.renewalPeriodDays}
                                placeholder="90"
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
                        
                        <CRow>
                          <CCol xs={12}>
                            <div className="mb-3">
                              <CFormLabel htmlFor="renewalProcedure">
                                Renewal Procedure <span className="text-danger">*</span>
                              </CFormLabel>
                              <CFormTextarea
                                id="renewalProcedure"
                                {...register('renewalProcedure')}
                                invalid={!!errors.renewalProcedure}
                                rows={3}
                                placeholder="Describe the renewal procedure and requirements"
                              />
                              {errors.renewalProcedure && (
                                <div className="invalid-feedback">{errors.renewalProcedure.message}</div>
                              )}
                            </div>
                          </CCol>
                        </CRow>
                      </>
                    )}
                  </CAccordionBody>
                </CAccordionItem>

                {/* License Conditions */}
                <CAccordionItem itemKey="conditions">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={LICENSE_ICONS.conditions} className="me-2" />
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

                {/* Review and Submit */}
                <CAccordionItem itemKey="reviewSubmit">
                  <CAccordionHeader>
                    <FontAwesomeIcon icon={LICENSE_ICONS.reviewSubmit} className="me-2" />
                    Review and Submit
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CAlert color="info">
                      <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                      Please review all information before creating the license. You can edit the license later if needed.
                    </CAlert>

                    <div className="d-flex justify-content-end">
                      <CButton
                        type="submit"
                        color="primary"
                        disabled={isLoading}
                        className="me-2"
                      >
                        {isLoading ? (
                          <>
                            <CSpinner size="sm" className="me-2" />
                            Creating License...
                          </>
                        ) : (
                          <>
                            <FontAwesomeIcon icon={LICENSE_ICONS.save} className="me-1" />
                            Create License
                          </>
                        )}
                      </CButton>
                      <CButton
                        variant="outline"
                        color="secondary"
                        onClick={() => navigate('/licenses')}
                        disabled={isLoading}
                      >
                        Cancel
                      </CButton>
                    </div>
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

export default CreateLicense;