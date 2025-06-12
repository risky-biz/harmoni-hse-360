import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Navigate } from 'react-router-dom';
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
  CFormLabel,
  CFormInput,
  CFormTextarea,
  CFormSelect,
  CButton,
  CSpinner,
  CAlert,
  CAccordion,
  CAccordionBody,
  CAccordionHeader,
  CAccordionItem,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faShieldAlt,
  faArrowLeft,
  faSave,
  faCertificate,
  faTools,
} from '@fortawesome/free-solid-svg-icons';
import {
  useGetPPEItemQuery,
  useUpdatePPEItemMutation,
  UpdatePPEItemRequest,
} from '../../features/ppe/ppeApi';
import { formatDateTime } from '../../utils/dateUtils';

// Validation schema for PPE item editing
const schema = yup.object({
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
  location: yup
    .string()
    .required('Location is required')
    .min(3, 'Location must be at least 3 characters')
    .max(100, 'Location must not exceed 100 characters'),
  expiryDate: yup.string().optional(),
  color: yup.string().optional().max(50, 'Color must not exceed 50 characters'),
  notes: yup.string().optional().max(1000, 'Notes must not exceed 1000 characters'),
});

interface EditPPEFormData {
  name: string;
  description: string;
  manufacturer: string;
  model: string;
  size: string;
  color?: string;
  location: string;
  expiryDate?: string;
  notes?: string;
}

const EditPPE: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  // Validate the ID parameter
  const itemId = id ? parseInt(id, 10) : null;
  
  // Redirect to PPE list if ID is invalid
  if (!id || !itemId || isNaN(itemId)) {
    return <Navigate to="/ppe" replace />;
  }

  const {
    data: ppeItem,
    error: loadError,
    isLoading,
  } = useGetPPEItemQuery(itemId);
  
  
  const [updatePPEItem, { isLoading: isUpdating }] =
    useUpdatePPEItemMutation();

  const [submitError, setSubmitError] = useState<string | null>(null);

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

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
  } = useForm<EditPPEFormData>({
    resolver: yupResolver(schema) as any,
  });

  useEffect(() => {
    if (ppeItem) {
      reset({
        name: ppeItem.name,
        description: ppeItem.description,
        manufacturer: ppeItem.manufacturer,
        model: ppeItem.model,
        size: ppeItem.size,
        color: ppeItem.color || '',
        location: ppeItem.location,
        expiryDate: ppeItem.expiryDate ? ppeItem.expiryDate.slice(0, 10) : '',
        notes: ppeItem.notes || '',
      });
    }
  }, [ppeItem, reset]);

  const onSubmit = async (data: EditPPEFormData) => {
    setSubmitError(null);

    try {
      const updateRequest: UpdatePPEItemRequest = {
        name: data.name,
        description: data.description,
        manufacturer: data.manufacturer,
        model: data.model,
        size: data.size,
        color: data.color || undefined,
        location: data.location,
        expiryDate: data.expiryDate || undefined,
        notes: data.notes || undefined,
      };

      await updatePPEItem({
        id: Number(id),
        item: updateRequest,
      }).unwrap();

      navigate(`/ppe/${id}`, {
        state: {
          message: 'PPE item updated successfully!',
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
        setSubmitError('Failed to update PPE item. Please try again.');
      }
      console.error('Update error:', error);
    }
  };

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading PPE item...</span>
      </div>
    );
  }

  if (loadError || !ppeItem) {
    return (
      <CAlert color="danger">
        Failed to load PPE item. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/ppe')}>
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            Back to List
          </CButton>
        </div>
      </CAlert>
    );
  }

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
                Edit PPE Item
              </h4>
              <small className="text-muted">
                Update information for {ppeItem.itemCode} - {ppeItem.name}
              </small>
            </div>
            <CButton
              color="secondary"
              variant="outline"
              onClick={() => navigate(`/ppe/${id}`)}
            >
              <FontAwesomeIcon icon={faArrowLeft} size="sm" className="me-1" />
              Back to Details
            </CButton>
          </CCardHeader>
          
          <CCardBody>
            {submitError && (
              <CAlert color="danger" dismissible onClose={() => setSubmitError(null)}>
                {submitError}
              </CAlert>
            )}

            <CForm onSubmit={handleSubmit(onSubmit)}>
              <CRow>
                <CCol md={8}>
                  {/* Basic Information Section */}
                  <CAccordion>
                    <CAccordionItem itemKey={1}>
                      <CAccordionHeader>
                        <strong>Basic Information</strong>
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CRow className="mb-3">
                          <CCol md={6}>
                            <CFormLabel htmlFor="name">Item Name *</CFormLabel>
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
                          <CCol md={6}>
                            <CFormLabel htmlFor="size">Size *</CFormLabel>
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

                        <div className="mb-3">
                          <CFormLabel htmlFor="description">Description *</CFormLabel>
                          <CFormTextarea
                            id="description"
                            rows={3}
                            {...register('description')}
                            invalid={!!errors.description}
                            placeholder="Detailed description of the PPE item"
                          />
                          {errors.description && (
                            <div className="invalid-feedback d-block">
                              {errors.description.message}
                            </div>
                          )}
                          <small className="text-muted">
                            {watch('description')?.length || 0}/500 characters
                          </small>
                        </div>
                      </CAccordionBody>
                    </CAccordionItem>

                    {/* Product Details Section */}
                    <CAccordionItem itemKey={2}>
                      <CAccordionHeader>
                        <strong>Product Details</strong>
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CRow className="mb-3">
                          <CCol md={6}>
                            <CFormLabel htmlFor="manufacturer">Manufacturer *</CFormLabel>
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
                          <CCol md={6}>
                            <CFormLabel htmlFor="model">Model *</CFormLabel>
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
                        </CRow>

                        <CRow className="mb-3">
                          <CCol md={4}>
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
                          <CCol md={4}>
                            <CFormLabel htmlFor="location">Storage Location *</CFormLabel>
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
                          <CCol md={4}>
                            <CFormLabel htmlFor="expiryDate">Expiry Date</CFormLabel>
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

                        <div className="mb-3">
                          <CFormLabel htmlFor="notes">Notes</CFormLabel>
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
                        </div>
                      </CAccordionBody>
                    </CAccordionItem>
                  </CAccordion>
                </CCol>

                <CCol md={4}>
                  <div className="border-start ps-4">
                    <h6 className="text-muted mb-3">PPE Item Information</h6>
                    <div className="mb-2">
                      <strong>Item Code:</strong> {ppeItem.itemCode}
                    </div>
                    <div className="mb-2">
                      <strong>Category:</strong> {ppeItem.categoryName}
                    </div>
                    <div className="mb-2">
                      <strong>Status:</strong> {ppeItem.status}
                    </div>
                    <div className="mb-2">
                      <strong>Condition:</strong> {ppeItem.condition}
                    </div>
                    <div className="mb-2">
                      <strong>Purchase Date:</strong>{' '}
                      {formatDateTime(ppeItem.purchaseDate)}
                    </div>
                    <div className="mb-2">
                      <strong>Cost:</strong> Rp {ppeItem.cost.toLocaleString()}
                    </div>
                    {ppeItem.assignedToName && (
                      <div className="mb-2">
                        <strong>Assigned To:</strong> {ppeItem.assignedToName}
                      </div>
                    )}
                    <div className="mb-2">
                      <strong>Created:</strong>{' '}
                      {formatDateTime(ppeItem.createdAt)}
                    </div>
                    <div className="mb-2">
                      <strong>Created By:</strong> {ppeItem.createdBy}
                    </div>
                    {ppeItem.lastModifiedAt && (
                      <div className="mb-2">
                        <strong>Last Modified:</strong>{' '}
                        {formatDateTime(ppeItem.lastModifiedAt)}
                      </div>
                    )}
                    {ppeItem.lastModifiedBy && (
                      <div className="mb-2">
                        <strong>Modified By:</strong> {ppeItem.lastModifiedBy}
                      </div>
                    )}

                    {/* Certification Info */}
                    {(ppeItem.certificationNumber || ppeItem.certifyingBody) && (
                      <div className="mt-4">
                        <h6 className="text-muted mb-2">
                          <FontAwesomeIcon icon={faCertificate} className="me-2 text-warning" />
                          Certification
                        </h6>
                        {ppeItem.certificationNumber && (
                          <div className="mb-1">
                            <strong>Number:</strong> {ppeItem.certificationNumber}
                          </div>
                        )}
                        {ppeItem.certifyingBody && (
                          <div className="mb-1">
                            <strong>Body:</strong> {ppeItem.certifyingBody}
                          </div>
                        )}
                        {ppeItem.certificationStandard && (
                          <div className="mb-1">
                            <strong>Standard:</strong> {ppeItem.certificationStandard}
                          </div>
                        )}
                      </div>
                    )}

                    {/* Maintenance Info */}
                    {(ppeItem.maintenanceIntervalDays || ppeItem.lastMaintenanceDate) && (
                      <div className="mt-4">
                        <h6 className="text-muted mb-2">
                          <FontAwesomeIcon icon={faTools} className="me-2 text-info" />
                          Maintenance
                        </h6>
                        {ppeItem.maintenanceIntervalDays && (
                          <div className="mb-1">
                            <strong>Interval:</strong> {ppeItem.maintenanceIntervalDays} days
                          </div>
                        )}
                        {ppeItem.lastMaintenanceDate && (
                          <div className="mb-1">
                            <strong>Last:</strong> {formatDateTime(ppeItem.lastMaintenanceDate)}
                          </div>
                        )}
                        {ppeItem.nextMaintenanceDate && (
                          <div className="mb-1">
                            <strong>Next:</strong> {formatDateTime(ppeItem.nextMaintenanceDate)}
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </CCol>
              </CRow>

              <hr />

              <div className="d-flex justify-content-between">
                <CButton
                  color="light"
                  onClick={() => navigate(`/ppe/${id}`)}
                  disabled={isUpdating}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
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
                      <FontAwesomeIcon icon={faSave} className="me-2" />
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

export default EditPPE;