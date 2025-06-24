import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CForm,
  CFormInput,
  CFormTextarea,
  CFormSelect,
  CFormLabel,
  CRow,
  CCol,
  CButton,
  CButtonGroup,
  CSpinner,
  CAlert,
  CInputGroup,
  CInputGroupText,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faRecycle,
  faSave,
  faTimes,
  faCalendarAlt,
  faMapMarkerAlt,
  faWeight,
  faDollarSign,
  faFileAlt,
} from '@fortawesome/free-solid-svg-icons';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  useGetWasteReportByIdQuery, 
  useUpdateWasteReportMutation,
  UpdateWasteReportRequest
} from '../../services/wasteReportsApi';
import { formatDateForInput } from '../../utils/dateUtils';
import { WasteClassification, WasteReportStatus } from '../../types/enums';

// Validation schema
const wasteReportSchema = yup.object({
  title: yup.string().required('Title is required').max(100, 'Title must be less than 100 characters'),
  description: yup.string().required('Description is required').max(500, 'Description must be less than 500 characters'),
  classification: yup.number().required('Waste classification is required'),
  location: yup.string().notRequired().max(100, 'Location must be less than 100 characters'),
  estimatedQuantity: yup.number().notRequired().min(0, 'Quantity must be positive').nullable(),
  quantityUnit: yup.string().notRequired().max(20, 'Unit must be less than 20 characters'),
  disposalMethod: yup.string().notRequired().max(100, 'Disposal method must be less than 100 characters'),
  disposalDate: yup.string().notRequired().nullable(),
  disposedBy: yup.string().notRequired().max(100, 'Disposed by must be less than 100 characters'),
  disposalCost: yup.number().notRequired().min(0, 'Cost must be positive').nullable(),
  contractorName: yup.string().notRequired().max(100, 'Contractor name must be less than 100 characters'),
  manifestNumber: yup.string().notRequired().max(50, 'Manifest number must be less than 50 characters'),
  treatment: yup.string().notRequired().max(200, 'Treatment must be less than 200 characters'),
  notes: yup.string().notRequired().max(1000, 'Notes must be less than 1000 characters'),
});

interface WasteReportFormData {
  title: string;
  description: string;
  classification: number;
  location?: string;
  estimatedQuantity?: number;
  quantityUnit?: string;
  disposalMethod?: string;
  disposalDate?: string;
  disposedBy?: string;
  disposalCost?: number;
  contractorName?: string;
  manifestNumber?: string;
  treatment?: string;
  notes?: string;
}

const wasteClassificationOptions = [
  { value: WasteClassification.NonHazardous, label: 'Non-Hazardous Waste' },
  { value: WasteClassification.HazardousChemical, label: 'Hazardous Chemical Waste' },
  { value: WasteClassification.HazardousBiological, label: 'Hazardous Biological Waste' },
  { value: WasteClassification.HazardousRadioactive, label: 'Hazardous Radioactive Waste' },
  { value: WasteClassification.Recyclable, label: 'Recyclable Waste' },
  { value: WasteClassification.Organic, label: 'Organic Waste' },
  { value: WasteClassification.Electronic, label: 'Electronic Waste' },
  { value: WasteClassification.Construction, label: 'Construction Waste' },
  { value: WasteClassification.Medical, label: 'Medical Waste' },
  { value: WasteClassification.Universal, label: 'Universal Waste' },
];

const quantityUnitOptions = [
  { value: 'kg', label: 'Kilograms (kg)' },
  { value: 'lbs', label: 'Pounds (lbs)' },
  { value: 'L', label: 'Liters (L)' },
  { value: 'gal', label: 'Gallons (gal)' },
  { value: 'm³', label: 'Cubic Meters (m³)' },
  { value: 'ft³', label: 'Cubic Feet (ft³)' },
];

const EditWasteReport: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const reportId = parseInt(id || '0', 10);

  const {
    data: report,
    isLoading: isLoadingReport,
    error: loadError,
  } = useGetWasteReportByIdQuery(reportId, {
    skip: !reportId,
  });

  const [updateWasteReport, { isLoading: isUpdating }] = useUpdateWasteReportMutation();

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    reset,
    watch,
  } = useForm({
    resolver: yupResolver(wasteReportSchema),
    mode: 'onChange',
  });

  const watchedClassification = watch('classification');

  // Reset form when report data is loaded
  useEffect(() => {
    if (report) {
      reset({
        title: report.title,
        description: report.description,
        classification: report.classification,
        location: report.location || '',
        estimatedQuantity: report.estimatedQuantity || undefined,
        quantityUnit: report.quantityUnit || 'kg',
        disposalMethod: report.disposalMethod || '',
        disposalDate: report.disposalDate ? formatDateForInput(report.disposalDate) : '',
        disposedBy: report.disposedBy || '',
        disposalCost: report.disposalCost || undefined,
        contractorName: report.contractorName || '',
        manifestNumber: report.manifestNumber || '',
        treatment: report.treatment || '',
        notes: report.notes || '',
      });
    }
  }, [report, reset]);

  const onSubmit = async (data: yup.InferType<typeof wasteReportSchema>) => {
    try {
      const updateRequest: UpdateWasteReportRequest = {
        id: reportId,
        title: data.title,
        description: data.description,
        classification: data.classification,
        location: data.location || undefined,
        estimatedQuantity: data.estimatedQuantity || undefined,
        quantityUnit: data.quantityUnit || undefined,
        disposalMethod: data.disposalMethod || undefined,
        disposalDate: data.disposalDate || undefined,
        disposedBy: data.disposedBy || undefined,
        disposalCost: data.disposalCost || undefined,
        contractorName: data.contractorName || undefined,
        manifestNumber: data.manifestNumber || undefined,
        treatment: data.treatment || undefined,
        notes: data.notes || undefined,
      };
      
      await updateWasteReport(updateRequest).unwrap();
      
      navigate(`/waste/reports/${reportId}`);
    } catch (error) {
      console.error('Failed to update waste report:', error);
    }
  };

  const handleCancel = () => {
    navigate(`/waste/reports/${reportId}`);
  };

  if (isLoadingReport) {
    return (
      <CCard>
        <CCardBody>
          <div className="d-flex justify-content-center p-4">
            <CSpinner size="sm" />
            <span className="ms-2">Loading waste report...</span>
          </div>
        </CCardBody>
      </CCard>
    );
  }

  if (loadError || !report) {
    return (
      <CCard>
        <CCardBody>
          <CAlert color="danger">
            Failed to load waste report. Please try again.
          </CAlert>
          <CButton color="secondary" onClick={() => navigate('/waste/reports')}>
            Back to Reports
          </CButton>
        </CCardBody>
      </CCard>
    );
  }

  if (!report.canEdit) {
    return (
      <CCard>
        <CCardBody>
          <CAlert color="warning">
            This waste report cannot be edited in its current status ({report.statusDisplay}).
          </CAlert>
          <CButton color="secondary" onClick={() => navigate(`/waste/reports/${reportId}`)}>
            Back to Report
          </CButton>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <div className="edit-waste-report">
      <CCard>
        <CCardHeader className="d-flex justify-content-between align-items-center">
          <h5 className="mb-0">
            <FontAwesomeIcon icon={faRecycle} className="me-2" />
            Edit Waste Report: {report.title}
          </h5>
          <CButtonGroup>
            <CButton
              color="secondary"
              variant="outline"
              onClick={handleCancel}
              disabled={isUpdating}
            >
              <FontAwesomeIcon icon={faTimes} className="me-1" />
              Cancel
            </CButton>
            <CButton
              color="primary"
              onClick={handleSubmit(onSubmit)}
              disabled={!isValid || isUpdating}
            >
              {isUpdating ? (
                <CSpinner size="sm" className="me-1" />
              ) : (
                <FontAwesomeIcon icon={faSave} className="me-1" />
              )}
              Save Changes
            </CButton>
          </CButtonGroup>
        </CCardHeader>

        <CCardBody>
          <CForm onSubmit={handleSubmit(onSubmit)}>
            <CRow>
              <CCol lg={8}>
                {/* Basic Information */}
                <CCard className="mb-4">
                  <CCardHeader>
                    <h6 className="mb-0">Basic Information</h6>
                  </CCardHeader>
                  <CCardBody>
                    <CRow>
                      <CCol md={12} className="mb-3">
                        <CFormLabel htmlFor="title">
                          Title <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormInput
                          id="title"
                          {...register('title')}
                          invalid={!!errors.title}
                          placeholder="Enter waste report title"
                        />
                        {errors.title && (
                          <div className="invalid-feedback d-block">
                            {errors.title.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="classification">
                          Waste Classification <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormSelect
                          id="classification"
                          {...register('classification')}
                          invalid={!!errors.classification}
                        >
                          <option value="">Select waste classification...</option>
                          {wasteClassificationOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                              {option.label}
                            </option>
                          ))}
                        </CFormSelect>
                        {errors.classification && (
                          <div className="invalid-feedback d-block">
                            {errors.classification.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="location">
                          <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                          Location
                        </CFormLabel>
                        <CFormInput
                          id="location"
                          {...register('location')}
                          invalid={!!errors.location}
                          placeholder="Enter location"
                        />
                        {errors.location && (
                          <div className="invalid-feedback d-block">
                            {errors.location.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={12} className="mb-3">
                        <CFormLabel htmlFor="description">
                          Description <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormTextarea
                          id="description"
                          {...register('description')}
                          invalid={!!errors.description}
                          rows={3}
                          placeholder="Describe the waste report details"
                        />
                        {errors.description && (
                          <div className="invalid-feedback d-block">
                            {errors.description.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>
                  </CCardBody>
                </CCard>

                {/* Quantity Information */}
                <CCard className="mb-4">
                  <CCardHeader>
                    <h6 className="mb-0">
                      <FontAwesomeIcon icon={faWeight} className="me-2" />
                      Quantity Information
                    </h6>
                  </CCardHeader>
                  <CCardBody>
                    <CRow>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="estimatedQuantity">Estimated Quantity</CFormLabel>
                        <CFormInput
                          id="estimatedQuantity"
                          type="number"
                          step="0.01"
                          min="0"
                          {...register('estimatedQuantity')}
                          invalid={!!errors.estimatedQuantity}
                          placeholder="0.00"
                        />
                        {errors.estimatedQuantity && (
                          <div className="invalid-feedback d-block">
                            {errors.estimatedQuantity.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="quantityUnit">Unit</CFormLabel>
                        <CFormSelect
                          id="quantityUnit"
                          {...register('quantityUnit')}
                          invalid={!!errors.quantityUnit}
                        >
                          {quantityUnitOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                              {option.label}
                            </option>
                          ))}
                        </CFormSelect>
                        {errors.quantityUnit && (
                          <div className="invalid-feedback d-block">
                            {errors.quantityUnit.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>
                  </CCardBody>
                </CCard>

                {/* Disposal Information */}
                <CCard className="mb-4">
                  <CCardHeader>
                    <h6 className="mb-0">Disposal Information</h6>
                  </CCardHeader>
                  <CCardBody>
                    <CRow>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="disposalMethod">Disposal Method</CFormLabel>
                        <CFormInput
                          id="disposalMethod"
                          {...register('disposalMethod')}
                          invalid={!!errors.disposalMethod}
                          placeholder="Enter disposal method"
                        />
                        {errors.disposalMethod && (
                          <div className="invalid-feedback d-block">
                            {errors.disposalMethod.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="disposalDate">
                          <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                          Disposal Date
                        </CFormLabel>
                        <CFormInput
                          id="disposalDate"
                          type="date"
                          {...register('disposalDate')}
                          invalid={!!errors.disposalDate}
                        />
                        {errors.disposalDate && (
                          <div className="invalid-feedback d-block">
                            {errors.disposalDate.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="disposedBy">Disposed By</CFormLabel>
                        <CFormInput
                          id="disposedBy"
                          {...register('disposedBy')}
                          invalid={!!errors.disposedBy}
                          placeholder="Enter who disposed the waste"
                        />
                        {errors.disposedBy && (
                          <div className="invalid-feedback d-block">
                            {errors.disposedBy.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="disposalCost">
                          <FontAwesomeIcon icon={faDollarSign} className="me-1" />
                          Disposal Cost
                        </CFormLabel>
                        <CInputGroup>
                          <CInputGroupText>$</CInputGroupText>
                          <CFormInput
                            id="disposalCost"
                            type="number"
                            step="0.01"
                            min="0"
                            {...register('disposalCost')}
                            invalid={!!errors.disposalCost}
                            placeholder="0.00"
                          />
                        </CInputGroup>
                        {errors.disposalCost && (
                          <div className="invalid-feedback d-block">
                            {errors.disposalCost.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="contractorName">Contractor Name</CFormLabel>
                        <CFormInput
                          id="contractorName"
                          {...register('contractorName')}
                          invalid={!!errors.contractorName}
                          placeholder="Enter contractor name"
                        />
                        {errors.contractorName && (
                          <div className="invalid-feedback d-block">
                            {errors.contractorName.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={6} className="mb-3">
                        <CFormLabel htmlFor="manifestNumber">Manifest Number</CFormLabel>
                        <CFormInput
                          id="manifestNumber"
                          {...register('manifestNumber')}
                          invalid={!!errors.manifestNumber}
                          placeholder="Enter manifest number"
                        />
                        {errors.manifestNumber && (
                          <div className="invalid-feedback d-block">
                            {errors.manifestNumber.message}
                          </div>
                        )}
                      </CCol>
                      <CCol md={12} className="mb-3">
                        <CFormLabel htmlFor="treatment">Treatment Method</CFormLabel>
                        <CFormInput
                          id="treatment"
                          {...register('treatment')}
                          invalid={!!errors.treatment}
                          placeholder="Enter treatment method"
                        />
                        {errors.treatment && (
                          <div className="invalid-feedback d-block">
                            {errors.treatment.message}
                          </div>
                        )}
                      </CCol>
                    </CRow>
                  </CCardBody>
                </CCard>

                {/* Additional Notes */}
                <CCard className="mb-4">
                  <CCardHeader>
                    <h6 className="mb-0">
                      <FontAwesomeIcon icon={faFileAlt} className="me-2" />
                      Additional Notes
                    </h6>
                  </CCardHeader>
                  <CCardBody>
                    <CFormLabel htmlFor="notes">Notes</CFormLabel>
                    <CFormTextarea
                      id="notes"
                      {...register('notes')}
                      invalid={!!errors.notes}
                      rows={4}
                      placeholder="Enter any additional notes or comments"
                    />
                    {errors.notes && (
                      <div className="invalid-feedback d-block">
                        {errors.notes.message}
                      </div>
                    )}
                  </CCardBody>
                </CCard>
              </CCol>

              <CCol lg={4}>
                {/* Safety Warnings */}
                {(watchedClassification === WasteClassification.HazardousChemical || 
                  watchedClassification === WasteClassification.HazardousBiological || 
                  watchedClassification === WasteClassification.HazardousRadioactive) && (
                  <CCard className="mb-4 border-warning">
                    <CCardHeader className="bg-warning bg-opacity-10">
                      <h6 className="mb-0 text-warning">
                        <FontAwesomeIcon icon={faRecycle} className="me-2" />
                        Safety Notice
                      </h6>
                    </CCardHeader>
                    <CCardBody>
                      <CAlert color="warning" className="mb-0">
                        <strong>High Risk Waste Classification</strong><br />
                        This waste classification requires special handling and disposal procedures. 
                        Please ensure all safety protocols are followed.
                      </CAlert>
                    </CCardBody>
                  </CCard>
                )}

                {/* Form Actions */}
                <CCard>
                  <CCardHeader>
                    <h6 className="mb-0">Actions</h6>
                  </CCardHeader>
                  <CCardBody>
                    <div className="d-grid gap-2">
                      <CButton
                        color="primary"
                        onClick={handleSubmit(onSubmit)}
                        disabled={!isValid || isUpdating}
                      >
                        {isUpdating ? (
                          <CSpinner size="sm" className="me-1" />
                        ) : (
                          <FontAwesomeIcon icon={faSave} className="me-1" />
                        )}
                        Save Changes
                      </CButton>
                      <CButton
                        color="secondary"
                        variant="outline"
                        onClick={handleCancel}
                        disabled={isUpdating}
                      >
                        <FontAwesomeIcon icon={faTimes} className="me-1" />
                        Cancel
                      </CButton>
                    </div>
                  </CCardBody>
                </CCard>
              </CCol>
            </CRow>
          </CForm>
        </CCardBody>
      </CCard>
    </div>
  );
};

export default EditWasteReport;