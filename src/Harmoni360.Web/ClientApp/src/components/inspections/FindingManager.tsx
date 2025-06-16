import React, { useState } from 'react';
import {
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormInput,
  CFormLabel,
  CFormSelect,
  CFormTextarea,
  CButton,
  CRow,
  CCol,
  CAlert,
  CSpinner,
  CBadge
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faTimes,
  faExclamationTriangle,
  faMapMarkerAlt,
  faIndustry,
  faUser,
  faCalendarAlt,
  faSave
} from '@fortawesome/free-solid-svg-icons';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { toast } from 'react-toastify';
import { useAddFindingMutation, useUpdateFindingMutation } from '../../features/inspections/inspectionApi';
import { InspectionFindingDto, FindingType, FindingSeverity } from '../../types/inspection';

// Form validation schema
const findingSchema: yup.ObjectSchema<FindingFormData> = yup.object().shape({
  description: yup.string().required('Description is required'),
  type: yup.string().required('Finding type is required'),
  severity: yup.string().required('Severity is required'),
  location: yup.string().optional(),
  equipment: yup.string().optional(),
  rootCause: yup.string().optional(),
  immediateAction: yup.string().optional(),
  correctiveAction: yup.string().optional(),
  dueDate: yup.date().nullable().optional(),
  responsiblePersonId: yup.number().nullable().optional(),
  regulation: yup.string().optional()
});

interface FindingFormData {
  description: string;
  type: string;
  severity: string;
  location?: string;
  equipment?: string;
  rootCause?: string;
  immediateAction?: string;
  correctiveAction?: string;
  dueDate?: Date | null;
  responsiblePersonId?: number | null;
  regulation?: string;
}

interface FindingManagerProps {
  inspectionId: number;
  finding?: InspectionFindingDto;
  isVisible: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

const findingTypes = [
  { value: 'Observation', label: 'Observation' },
  { value: 'NonCompliance', label: 'Non-Compliance' },
  { value: 'Improvement', label: 'Improvement Opportunity' },
  { value: 'PositiveFinding', label: 'Positive Finding' },
  { value: 'Violation', label: 'Violation' }
];

const severityLevels = [
  { value: 'Minor', label: 'Minor' },
  { value: 'Major', label: 'Major' },
  { value: 'Critical', label: 'Critical' },
  { value: 'Catastrophic', label: 'Catastrophic' }
];

// Mock data - Replace with actual API calls
const responsiblePersons = [
  { id: 1, name: 'John Smith' },
  { id: 2, name: 'Jane Doe' },
  { id: 3, name: 'Mike Johnson' },
  { id: 4, name: 'Sarah Wilson' }
];

export const FindingManager: React.FC<FindingManagerProps> = ({
  inspectionId,
  finding,
  isVisible,
  onClose,
  onSuccess
}) => {
  const isEditing = !!finding;
  
  const [addFinding, { isLoading: isAdding }] = useAddFindingMutation();
  const [updateFinding, { isLoading: isUpdating }] = useUpdateFindingMutation();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch
  } = useForm<FindingFormData>({
    resolver: yupResolver(findingSchema),
    defaultValues: finding ? {
      description: finding.description,
      type: finding.type,
      severity: finding.severity,
      location: finding.location || '',
      equipment: finding.equipment || '',
      rootCause: finding.rootCause || '',
      immediateAction: finding.immediateAction || '',
      correctiveAction: finding.correctiveAction || '',
      dueDate: finding.dueDate ? new Date(finding.dueDate) : null,
      responsiblePersonId: finding.responsiblePersonId || null,
      regulation: finding.regulation || ''
    } : {
      description: '',
      type: 'Observation',
      severity: 'Minor',
      location: '',
      equipment: '',
      rootCause: '',
      immediateAction: '',
      correctiveAction: '',
      dueDate: null,
      responsiblePersonId: null,
      regulation: ''
    }
  });

  const watchedSeverity = watch('severity');
  const watchedCorrectiveAction = watch('correctiveAction');

  const onSubmit = async (data: FindingFormData) => {
    try {
      // Clean up the data to match the API contract
      const submitData: any = {
        description: data.description!,
        type: data.type!,
        severity: data.severity!,
      };
      
      // Only include optional fields if they have values
      if (data.location && data.location.trim()) submitData.location = data.location;
      if (data.equipment && data.equipment.trim()) submitData.equipment = data.equipment;
      if (data.rootCause && data.rootCause.trim()) submitData.rootCause = data.rootCause;
      if (data.immediateAction && data.immediateAction.trim()) submitData.immediateAction = data.immediateAction;
      if (data.correctiveAction && data.correctiveAction.trim()) submitData.correctiveAction = data.correctiveAction;
      if (data.regulation && data.regulation.trim()) submitData.regulation = data.regulation;
      if (data.dueDate) submitData.dueDate = data.dueDate.toISOString();
      if (data.responsiblePersonId) submitData.responsiblePersonId = data.responsiblePersonId;
      
      if (isEditing && finding) {
        await updateFinding({
          inspectionId,
          findingId: finding.id,
          ...submitData
        }).unwrap();
        toast.success('Finding updated successfully');
      } else {
        await addFinding({
          inspectionId,
          ...submitData
        }).unwrap();
        toast.success('Finding added successfully');
      }
      
      reset();
      onSuccess();
      onClose();
    } catch (error: any) {
      console.error('Error saving finding:', error);
      toast.error(error?.data?.message || 'Failed to save finding');
    }
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'Minor': return 'info';
      case 'Major': return 'warning';
      case 'Critical': return 'danger';
      case 'Catastrophic': return 'dark';
      default: return 'secondary';
    }
  };

  return (
    <CModal visible={isVisible} onClose={handleClose} size="lg" backdrop="static">
      <CModalHeader>
        <CModalTitle>
          <FontAwesomeIcon icon={isEditing ? faExclamationTriangle : faPlus} className="me-2" />
          {isEditing ? 'Edit Finding' : 'Add New Finding'}
        </CModalTitle>
      </CModalHeader>
      <CForm onSubmit={handleSubmit(onSubmit)}>
        <CModalBody>
          {/* Finding Type and Severity */}
          <CRow className="mb-3">
            <CCol md={6}>
              <CFormLabel htmlFor="type">Finding Type *</CFormLabel>
              <CFormSelect
                {...register('type')}
                id="type"
                invalid={!!errors.type}
              >
                {findingTypes.map(type => (
                  <option key={type.value} value={type.value}>
                    {type.label}
                  </option>
                ))}
              </CFormSelect>
              {errors.type && (
                <div className="invalid-feedback d-block">
                  {errors.type.message}
                </div>
              )}
            </CCol>
            <CCol md={6}>
              <CFormLabel htmlFor="severity">Severity *</CFormLabel>
              <div className="d-flex align-items-center">
                <CFormSelect
                  {...register('severity')}
                  id="severity"
                  invalid={!!errors.severity}
                  className="me-2"
                >
                  {severityLevels.map(severity => (
                    <option key={severity.value} value={severity.value}>
                      {severity.label}
                    </option>
                  ))}
                </CFormSelect>
                <CBadge color={getSeverityColor(watchedSeverity)}>
                  {watchedSeverity}
                </CBadge>
              </div>
              {errors.severity && (
                <div className="invalid-feedback d-block">
                  {errors.severity.message}
                </div>
              )}
            </CCol>
          </CRow>

          {/* Description */}
          <div className="mb-3">
            <CFormLabel htmlFor="description">Description *</CFormLabel>
            <CFormTextarea
              {...register('description')}
              id="description"
              rows={3}
              placeholder="Describe the finding in detail..."
              invalid={!!errors.description}
            />
            {errors.description && (
              <div className="invalid-feedback d-block">
                {errors.description.message}
              </div>
            )}
          </div>

          {/* Location and Equipment */}
          <CRow className="mb-3">
            <CCol md={6}>
              <CFormLabel htmlFor="location">
                <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                Location
              </CFormLabel>
              <CFormInput
                {...register('location')}
                id="location"
                placeholder="Specific location where finding was observed"
              />
            </CCol>
            <CCol md={6}>
              <CFormLabel htmlFor="equipment">
                <FontAwesomeIcon icon={faIndustry} className="me-1" />
                Equipment/Asset
              </CFormLabel>
              <CFormInput
                {...register('equipment')}
                id="equipment"
                placeholder="Equipment or asset involved"
              />
            </CCol>
          </CRow>

          {/* Root Cause */}
          <div className="mb-3">
            <CFormLabel htmlFor="rootCause">Root Cause Analysis</CFormLabel>
            <CFormTextarea
              {...register('rootCause')}
              id="rootCause"
              rows={2}
              placeholder="What is the underlying cause of this finding?"
            />
          </div>

          {/* Immediate Action */}
          <div className="mb-3">
            <CFormLabel htmlFor="immediateAction">Immediate Action Taken</CFormLabel>
            <CFormTextarea
              {...register('immediateAction')}
              id="immediateAction"
              rows={2}
              placeholder="What immediate actions were taken to address this finding?"
            />
          </div>

          {/* Corrective Action */}
          <div className="mb-3">
            <CFormLabel htmlFor="correctiveAction">Corrective Action Required</CFormLabel>
            <CFormTextarea
              {...register('correctiveAction')}
              id="correctiveAction"
              rows={2}
              placeholder="What corrective actions are needed to prevent recurrence?"
            />
          </div>

          {/* Assignment and Due Date (only if corrective action is specified) */}
          {watchedCorrectiveAction && (
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="responsiblePersonId">
                  <FontAwesomeIcon icon={faUser} className="me-1" />
                  Responsible Person
                </CFormLabel>
                <CFormSelect
                  {...register('responsiblePersonId')}
                  id="responsiblePersonId"
                >
                  <option value="">Select person...</option>
                  {responsiblePersons.map(person => (
                    <option key={person.id} value={person.id}>
                      {person.name}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="dueDate">
                  <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                  Due Date
                </CFormLabel>
                <CFormInput
                  {...register('dueDate')}
                  type="date"
                  id="dueDate"
                />
              </CCol>
            </CRow>
          )}

          {/* Regulation Reference */}
          <div className="mb-3">
            <CFormLabel htmlFor="regulation">Regulation/Standard Reference</CFormLabel>
            <CFormInput
              {...register('regulation')}
              id="regulation"
              placeholder="Relevant regulation, standard, or procedure"
            />
          </div>

          {/* High Severity Warning */}
          {(watchedSeverity === 'Critical' || watchedSeverity === 'Catastrophic') && (
            <CAlert color="warning" className="mt-3">
              <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
              <strong>High Severity Finding:</strong> This finding requires immediate attention and escalation to management.
            </CAlert>
          )}
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={handleClose}
            disabled={isAdding || isUpdating}
          >
            <FontAwesomeIcon icon={faTimes} className="me-1" />
            Cancel
          </CButton>
          <CButton
            color="primary"
            type="submit"
            disabled={isAdding || isUpdating}
          >
            {(isAdding || isUpdating) ? (
              <>
                <CSpinner size="sm" className="me-2" />
                {isEditing ? 'Updating...' : 'Adding...'}
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faSave} className="me-1" />
                {isEditing ? 'Update Finding' : 'Add Finding'}
              </>
            )}
          </CButton>
        </CModalFooter>
      </CForm>
    </CModal>
  );
};