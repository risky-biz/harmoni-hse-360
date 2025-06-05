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
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faShieldAlt,
  faInfo,
  faTasks,
  faDashboard,
  faCertificate,
  faTools,
} from '@fortawesome/free-solid-svg-icons';
import {
  useCreatePPEItemMutation,
  useGetPPECategoriesQuery,
  CreatePPEItemRequest,
} from '../../features/ppe/ppeApi';

// Validation schema for PPE item creation
const schema = yup.object({
  itemCode: yup
    .string()
    .required('Item code is required')
    .matches(/^[A-Z]{2,4}-\d{4}$/, 'Item code must follow format: PREFIX-NNNN (e.g., HEL-0001)')
    .max(20, 'Item code must not exceed 20 characters'),
  name: yup
    .string()
    .required('Item name is required')
    .min(3, 'Name must be at least 3 characters')
    .max(100, 'Name must not exceed 100 characters'),
  description: yup
    .string()
    .required('Description is required')
    .min(10, 'Description must be at least 10 characters')
    .max(500, 'Description must not exceed 500 characters'),
  categoryId: yup
    .number()
    .required('Category is required')
    .positive('Please select a valid category'),
  manufacturer: yup
    .string()
    .required('Manufacturer is required')
    .min(2, 'Manufacturer must be at least 2 characters')
    .max(100, 'Manufacturer must not exceed 100 characters'),
  model: yup
    .string()
    .required('Model is required')
    .min(1, 'Model must be at least 1 character')
    .max(100, 'Model must not exceed 100 characters'),
  size: yup
    .string()
    .required('Size is required')
    .max(20, 'Size must not exceed 20 characters'),
  purchaseDate: yup
    .string()
    .required('Purchase date is required'),
  cost: yup
    .number()
    .required('Cost is required')
    .min(0, 'Cost must be a positive number')
    .max(999999999, 'Cost exceeds maximum allowed value')
    .test('multiple-of-1000', 'Cost must be a multiple of 1,000', (value) => {
      return value !== undefined && value % 1000 === 0;
    }),
  location: yup
    .string()
    .required('Location is required')
    .min(3, 'Location must be at least 3 characters')
    .max(100, 'Location must not exceed 100 characters'),
  expiryDate: yup.string().optional(),
  color: yup.string().optional().max(50, 'Color must not exceed 50 characters'),
  notes: yup.string().optional().max(1000, 'Notes must not exceed 1000 characters'),
  
  // Certification fields
  certificationNumber: yup.string().optional().max(100, 'Certification number must not exceed 100 characters'),
  certifyingBody: yup.string().optional().max(100, 'Certifying body must not exceed 100 characters'),
  certificationDate: yup.string().optional(),
  certificationExpiryDate: yup.string().optional(),
  certificationStandard: yup.string().optional().max(100, 'Certification standard must not exceed 100 characters'),
  
  // Maintenance fields
  maintenanceIntervalDays: yup.number().optional().min(1, 'Maintenance interval must be at least 1 day').max(3650, 'Maintenance interval cannot exceed 10 years'),
  lastMaintenanceDate: yup.string().optional(),
  maintenanceInstructions: yup.string().optional().max(500, 'Maintenance instructions must not exceed 500 characters'),
});

interface PPEFormData {
  itemCode: string;
  name: string;
  description: string;
  categoryId: number;
  manufacturer: string;
  model: string;
  size: string;
  color?: string;
  purchaseDate: string;
  cost: number;
  location: string;
  expiryDate?: string;
  notes?: string;
  
  // Certification Info
  certificationNumber?: string;
  certifyingBody?: string;
  certificationDate?: string;
  certificationExpiryDate?: string;
  certificationStandard?: string;
  
  // Maintenance Info
  maintenanceIntervalDays?: number;
  lastMaintenanceDate?: string;
  maintenanceInstructions?: string;
}

const CreatePPE: React.FC = () => {
  const navigate = useNavigate();
  const [createPPEItem, { isLoading: isSubmitting }] = useCreatePPEItemMutation();
  const { data: categories, isLoading: categoriesLoading } = useGetPPECategoriesQuery();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [autoSaveStatus, setAutoSaveStatus] = useState<
    'saved' | 'saving' | 'error' | null
  >(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    watch,
    setValue,
    getValues,
  } = useForm<PPEFormData>({
    resolver: yupResolver(schema) as any,
    defaultValues: {
      itemCode: '',
      name: '',
      description: '',
      categoryId: 0,
      manufacturer: '',
      model: '',
      size: '',
      color: '',
      purchaseDate: new Date().toISOString().slice(0, 10), // Current date
      cost: 0,
      location: '',
      expiryDate: '',
      notes: '',
      certificationNumber: '',
      certifyingBody: '',
      certificationDate: '',
      certificationExpiryDate: '',
      certificationStandard: '',
      maintenanceIntervalDays: undefined,
      lastMaintenanceDate: '',
      maintenanceInstructions: '',
    },
  });

  // Auto-save functionality (every 30 seconds)
  useEffect(() => {
    if (!isDirty) return;

    const autoSaveTimer = setInterval(() => {
      const formData = getValues();
      setAutoSaveStatus('saving');

      setTimeout(() => {
        try {
          localStorage.setItem('ppe_draft', JSON.stringify(formData));
          setAutoSaveStatus('saved');
          setTimeout(() => setAutoSaveStatus(null), 2000);
        } catch (error) {
          setAutoSaveStatus('error');
          setTimeout(() => setAutoSaveStatus(null), 3000);
        }
      }, 500);
    }, 30000);

    return () => clearInterval(autoSaveTimer);
  }, [isDirty, getValues]);

  // Load draft from localStorage on component mount
  useEffect(() => {
    const draft = localStorage.getItem('ppe_draft');
    if (draft) {
      try {
        const parsedDraft = JSON.parse(draft);
        Object.keys(parsedDraft).forEach((key) => {
          setValue(key as keyof PPEFormData, parsedDraft[key]);
        });
      } catch (error) {
        console.warn('Failed to load PPE draft:', error);
      }
    }
  }, [setValue]);

  // Submit form
  const onSubmit = async (data: PPEFormData) => {
    setSubmitError(null);

    try {
      // Prepare the request data
      const createRequest: CreatePPEItemRequest = {
        itemCode: data.itemCode,
        name: data.name,
        description: data.description,
        categoryId: Number(data.categoryId),
        manufacturer: data.manufacturer,
        model: data.model,
        size: data.size,
        color: data.color || undefined,
        purchaseDate: data.purchaseDate,
        cost: Number(data.cost),
        location: data.location,
        expiryDate: data.expiryDate || undefined,
        notes: data.notes || undefined,
        
        // Certification Info
        certificationNumber: data.certificationNumber || undefined,
        certifyingBody: data.certifyingBody || undefined,
        certificationDate: data.certificationDate || undefined,
        certificationExpiryDate: data.certificationExpiryDate || undefined,
        certificationStandard: data.certificationStandard || undefined,
        
        // Maintenance Info
        maintenanceIntervalDays: data.maintenanceIntervalDays || undefined,
        lastMaintenanceDate: data.lastMaintenanceDate || undefined,
        maintenanceInstructions: data.maintenanceInstructions || undefined,
      };

      // Submit to API
      await createPPEItem(createRequest).unwrap();

      // Clear draft after successful submission
      localStorage.removeItem('ppe_draft');

      // Navigate to PPE list with success message
      navigate('/ppe', {
        state: {
          message: 'PPE item created successfully!',
          type: 'success',
        },
      });
    } catch (error: any) {
      // Handle API error
      if (error.data?.message) {
        setSubmitError(error.data.message);
      } else if (error.data?.errors) {
        // Handle validation errors
        const errorMessages = Object.values(error.data.errors)
          .flat()
          .join(', ');
        setSubmitError(errorMessages);
      } else {
        setSubmitError('Failed to create PPE item. Please try again.');
      }
      console.error('Submit error:', error);
    }
  };

  // Common PPE sizes
  const ppeSizes = [
    'XS', 'S', 'M', 'L', 'XL', 'XXL', 'XXXL',
    'One Size', 'Universal',
    '6', '6.5', '7', '7.5', '8', '8.5', '9', '9.5', '10', '10.5', '11', '11.5', '12',
    '52', '54', '56', '58', '60', '62', '64',
  ];

  // Common locations
  const commonLocations = [
    'Safety Equipment Room',
    'Main Warehouse',
    'Emergency Station A',
    'Emergency Station B',
    'First Aid Station',
    'Laboratory Storage',
    'Workshop Area',
    'Construction Site Storage',
    'Vehicle Equipment Bay',
    'Reception Desk',
  ];

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4
                className="mb-0"
                style={{
                  color: 'var(--harmoni-charcoal)',
                  fontFamily: 'Poppins, sans-serif',
                }}
              >
                <FontAwesomeIcon
                  icon={faShieldAlt}
                  className="me-2 text-primary"
                />
                Add New PPE Item
              </h4>
              <small className="text-muted">
                Register new Personal Protective Equipment in the inventory
              </small>
            </div>
            <div className="d-flex align-items-center gap-2">
              {autoSaveStatus && (
                <CBadge
                  color={
                    autoSaveStatus === 'saved'
                      ? 'success'
                      : autoSaveStatus === 'saving'
                        ? 'info'
                        : 'danger'
                  }
                  className="d-flex align-items-center"
                >
                  {autoSaveStatus === 'saving' && (
                    <CSpinner size="sm" className="me-1" />
                  )}
                  <FontAwesomeIcon
                    icon={autoSaveStatus === 'saved' ? faTasks : faInfo}
                    size="sm"
                    className="me-1"
                  />
                  {autoSaveStatus === 'saved'
                    ? 'Auto-saved'
                    : autoSaveStatus === 'saving'
                      ? 'Saving...'
                      : 'Save failed'}
                </CBadge>
              )}
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate('/ppe')}
              >
                <FontAwesomeIcon icon={faDashboard} size="sm" className="me-1" />
                Back to List
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            <CForm onSubmit={handleSubmit(onSubmit)}>
              {submitError && (
                <CAlert
                  color="danger"
                  dismissible
                  onClose={() => setSubmitError(null)}
                >
                  {submitError}
                </CAlert>
              )}

              <CCallout color="info" className="mb-4">
                <FontAwesomeIcon icon={faInfo} className="me-2" />
                <strong>Important:</strong> Ensure all PPE items are properly tagged with unique item codes and include certification information where applicable.
              </CCallout>

              {/* Basic Information Section */}
              <CAccordion>
                <CAccordionItem itemKey={1}>
                  <CAccordionHeader>
                    <strong>1. Basic Information</strong>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={4}>
                        <CFormLabel htmlFor="itemCode">
                          Item Code *
                        </CFormLabel>
                        <CFormInput
                          id="itemCode"
                          {...register('itemCode')}
                          invalid={!!errors.itemCode}
                          placeholder="e.g., HEL-0001, EYE-0002"
                        />
                        {errors.itemCode && (
                          <div className="invalid-feedback d-block">
                            {errors.itemCode.message}
                          </div>
                        )}
                        <small className="text-muted">Format: PREFIX-NNNN</small>
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="name">
                          Item Name *
                        </CFormLabel>
                        <CFormInput
                          id="name"
                          {...register('name')}
                          invalid={!!errors.name}
                          placeholder="e.g., Safety Helmet, Safety Glasses"
                        />
                        {errors.name && (
                          <div className="invalid-feedback d-block">
                            {errors.name.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="categoryId">Category *</CFormLabel>
                        <CFormSelect
                          id="categoryId"
                          {...register('categoryId', { valueAsNumber: true })}
                          invalid={!!errors.categoryId}
                          disabled={categoriesLoading}
                        >
                          <option value="">Select category...</option>
                          {categories?.map((category) => (
                            <option key={category.id} value={category.id}>
                              {category.name}
                            </option>
                          ))}
                        </CFormSelect>
                        {errors.categoryId && (
                          <div className="invalid-feedback d-block">
                            {errors.categoryId.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol xs={12}>
                        <CFormLabel htmlFor="description">
                          Description *
                        </CFormLabel>
                        <CFormTextarea
                          id="description"
                          rows={3}
                          {...register('description')}
                          invalid={!!errors.description}
                          placeholder="Detailed description of the PPE item, including specifications and features"
                        />
                        {errors.description && (
                          <div className="invalid-feedback d-block">
                            {errors.description.message}
                          </div>
                        )}
                        <small className="text-muted">
                          {watch('description')?.length || 0}/500 characters
                        </small>
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Product Details Section */}
                <CAccordionItem itemKey={2}>
                  <CAccordionHeader>
                    <strong>2. Product Details</strong>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={4}>
                        <CFormLabel htmlFor="manufacturer">
                          Manufacturer *
                        </CFormLabel>
                        <CFormInput
                          id="manufacturer"
                          {...register('manufacturer')}
                          invalid={!!errors.manufacturer}
                          placeholder="e.g., 3M, Honeywell, MSA"
                        />
                        {errors.manufacturer && (
                          <div className="invalid-feedback d-block">
                            {errors.manufacturer.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="model">
                          Model *
                        </CFormLabel>
                        <CFormInput
                          id="model"
                          {...register('model')}
                          invalid={!!errors.model}
                          placeholder="Model number or name"
                        />
                        {errors.model && (
                          <div className="invalid-feedback d-block">
                            {errors.model.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="size">
                          Size *
                        </CFormLabel>
                        <CFormSelect
                          id="size"
                          {...register('size')}
                          invalid={!!errors.size}
                        >
                          <option value="">Select size...</option>
                          {ppeSizes.map((size) => (
                            <option key={size} value={size}>
                              {size}
                            </option>
                          ))}
                        </CFormSelect>
                        {errors.size && (
                          <div className="invalid-feedback d-block">
                            {errors.size.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={3}>
                        <CFormLabel htmlFor="color">Color</CFormLabel>
                        <CFormInput
                          id="color"
                          {...register('color')}
                          invalid={!!errors.color}
                          placeholder="e.g., Yellow, Red, Blue"
                        />
                        {errors.color && (
                          <div className="invalid-feedback d-block">
                            {errors.color.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="purchaseDate">
                          Purchase Date *
                        </CFormLabel>
                        <CFormInput
                          id="purchaseDate"
                          type="date"
                          {...register('purchaseDate')}
                          invalid={!!errors.purchaseDate}
                        />
                        {errors.purchaseDate && (
                          <div className="invalid-feedback d-block">
                            {errors.purchaseDate.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="cost">
                          Cost (IDR) *
                        </CFormLabel>
                        <CInputGroup>
                          <CInputGroupText>Rp</CInputGroupText>
                          <CFormInput
                            id="cost"
                            type="number"
                            min="0"
                            step="1000"
                            {...register('cost', { valueAsNumber: true })}
                            invalid={!!errors.cost}
                            placeholder="0"
                          />
                        </CInputGroup>
                        {errors.cost && (
                          <div className="invalid-feedback d-block">
                            {errors.cost.message}
                          </div>
                        )}
                        <small className="text-muted">
                          Enter cost in multiples of 1,000 (e.g., 1000, 2000, 3000)
                        </small>
                      </CCol>
                      <CCol md={3}>
                        <CFormLabel htmlFor="expiryDate">
                          Expiry Date
                        </CFormLabel>
                        <CFormInput
                          id="expiryDate"
                          type="date"
                          {...register('expiryDate')}
                          invalid={!!errors.expiryDate}
                        />
                        {errors.expiryDate && (
                          <div className="invalid-feedback d-block">
                            {errors.expiryDate.message}
                          </div>
                        )}
                        <small className="text-muted">Leave blank if no expiry</small>
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="location">
                          Storage Location *
                        </CFormLabel>
                        <CFormSelect
                          id="location"
                          {...register('location')}
                          invalid={!!errors.location}
                        >
                          <option value="">Select location...</option>
                          {commonLocations.map((location) => (
                            <option key={location} value={location}>
                              {location}
                            </option>
                          ))}
                        </CFormSelect>
                        {errors.location && (
                          <div className="invalid-feedback d-block">
                            {errors.location.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="notes">
                          Notes
                        </CFormLabel>
                        <CFormTextarea
                          id="notes"
                          rows={2}
                          {...register('notes')}
                          invalid={!!errors.notes}
                          placeholder="Additional notes or remarks about the PPE item"
                        />
                        {errors.notes && (
                          <div className="invalid-feedback d-block">
                            {errors.notes.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Certification Section */}
                <CAccordionItem itemKey={3}>
                  <CAccordionHeader>
                    <strong>3. Certification Information</strong>
                    <FontAwesomeIcon icon={faCertificate} className="ms-2 text-warning" />
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={6}>
                        <CFormLabel htmlFor="certificationNumber">
                          Certification Number
                        </CFormLabel>
                        <CFormInput
                          id="certificationNumber"
                          {...register('certificationNumber')}
                          invalid={!!errors.certificationNumber}
                          placeholder="e.g., CE-12345, ANSI-67890"
                        />
                        {errors.certificationNumber && (
                          <div className="invalid-feedback d-block">
                            {errors.certificationNumber.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6}>
                        <CFormLabel htmlFor="certifyingBody">
                          Certifying Body
                        </CFormLabel>
                        <CFormInput
                          id="certifyingBody"
                          {...register('certifyingBody')}
                          invalid={!!errors.certifyingBody}
                          placeholder="e.g., BSI, TUV, NIOSH"
                        />
                        {errors.certifyingBody && (
                          <div className="invalid-feedback d-block">
                            {errors.certifyingBody.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={4}>
                        <CFormLabel htmlFor="certificationDate">
                          Certification Date
                        </CFormLabel>
                        <CFormInput
                          id="certificationDate"
                          type="date"
                          {...register('certificationDate')}
                          invalid={!!errors.certificationDate}
                        />
                        {errors.certificationDate && (
                          <div className="invalid-feedback d-block">
                            {errors.certificationDate.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="certificationExpiryDate">
                          Certification Expiry
                        </CFormLabel>
                        <CFormInput
                          id="certificationExpiryDate"
                          type="date"
                          {...register('certificationExpiryDate')}
                          invalid={!!errors.certificationExpiryDate}
                        />
                        {errors.certificationExpiryDate && (
                          <div className="invalid-feedback d-block">
                            {errors.certificationExpiryDate.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="certificationStandard">
                          Certification Standard
                        </CFormLabel>
                        <CFormInput
                          id="certificationStandard"
                          {...register('certificationStandard')}
                          invalid={!!errors.certificationStandard}
                          placeholder="e.g., EN 397, ANSI Z87.1"
                        />
                        {errors.certificationStandard && (
                          <div className="invalid-feedback d-block">
                            {errors.certificationStandard.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Maintenance Section */}
                <CAccordionItem itemKey={4}>
                  <CAccordionHeader>
                    <strong>4. Maintenance Information</strong>
                    <FontAwesomeIcon icon={faTools} className="ms-2 text-info" />
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={4}>
                        <CFormLabel htmlFor="maintenanceIntervalDays">
                          Maintenance Interval (Days)
                        </CFormLabel>
                        <CFormInput
                          id="maintenanceIntervalDays"
                          type="number"
                          min="1"
                          max="3650"
                          {...register('maintenanceIntervalDays', { valueAsNumber: true })}
                          invalid={!!errors.maintenanceIntervalDays}
                          placeholder="e.g., 30, 90, 365"
                        />
                        {errors.maintenanceIntervalDays && (
                          <div className="invalid-feedback d-block">
                            {errors.maintenanceIntervalDays.message}
                          </div>
                        )}
                        <small className="text-muted">Leave blank if no scheduled maintenance</small>
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="lastMaintenanceDate">
                          Last Maintenance Date
                        </CFormLabel>
                        <CFormInput
                          id="lastMaintenanceDate"
                          type="date"
                          {...register('lastMaintenanceDate')}
                          invalid={!!errors.lastMaintenanceDate}
                        />
                        {errors.lastMaintenanceDate && (
                          <div className="invalid-feedback d-block">
                            {errors.lastMaintenanceDate.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="maintenanceInstructions">
                          Maintenance Instructions
                        </CFormLabel>
                        <CFormTextarea
                          id="maintenanceInstructions"
                          rows={2}
                          {...register('maintenanceInstructions')}
                          invalid={!!errors.maintenanceInstructions}
                          placeholder="Brief maintenance instructions or requirements"
                        />
                        {errors.maintenanceInstructions && (
                          <div className="invalid-feedback d-block">
                            {errors.maintenanceInstructions.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>
              </CAccordion>

              {/* Submit Section */}
              <div className="d-flex justify-content-between align-items-center mt-4 pt-3 border-top">
                <div className="text-muted">
                  <small>
                    <FontAwesomeIcon icon={faInfo} size="sm" className="me-1" />
                    Form auto-saves every 30 seconds
                  </small>
                </div>
                <div className="d-flex gap-2">
                  <CButton
                    type="button"
                    color="secondary"
                    variant="outline"
                    onClick={() => navigate('/ppe')}
                    disabled={isSubmitting}
                  >
                    Cancel
                  </CButton>
                  <CButton
                    type="submit"
                    color="primary"
                    disabled={isSubmitting}
                    className="d-flex align-items-center"
                  >
                    {isSubmitting ? (
                      <>
                        <CSpinner size="sm" className="me-2" />
                        Creating PPE Item...
                      </>
                    ) : (
                      <>
                        <FontAwesomeIcon icon={faTasks} size="sm" className="me-2" />
                        Create PPE Item
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

export default CreatePPE;