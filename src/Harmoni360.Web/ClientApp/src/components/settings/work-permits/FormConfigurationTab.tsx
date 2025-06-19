import React, { useState, useEffect } from 'react';
import {
  CForm,
  CFormInput,
  CFormTextarea,
  CFormCheck,
  CButton,
  CRow,
  CCol,
  CCard,
  CCardBody,
  CAlert,
  CSpinner,
  CFormLabel,
  CFormSelect,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSave,
  faPlus,
  faEdit,
  faTrash,
  faCheck,
  faTimes,
} from '@fortawesome/free-solid-svg-icons';
import {
  WorkPermitSettingsResponse,
  CreateWorkPermitSettingsRequest,
  UpdateWorkPermitSettingsRequest,
} from '../../../types/workPermitSettings';
import {
  useCreateWorkPermitSettingsMutation,
  useUpdateWorkPermitSettingsMutation,
  useDeleteWorkPermitSettingsMutation,
} from '../../../services/workPermitSettingsApi';

interface FormConfigurationTabProps {
  settings: WorkPermitSettingsResponse[];
}

export const FormConfigurationTab: React.FC<FormConfigurationTabProps> = ({ settings }) => {
  const [createSettings] = useCreateWorkPermitSettingsMutation();
  const [updateSettings] = useUpdateWorkPermitSettingsMutation();
  const [deleteSettings] = useDeleteWorkPermitSettingsMutation();
  
  const [editingId, setEditingId] = useState<number | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const [formData, setFormData] = useState<Partial<CreateWorkPermitSettingsRequest>>({
    requireSafetyInduction: false,
    enableFormValidation: true,
    allowAttachments: true,
    maxAttachmentSizeMB: 10,
    formInstructions: '',
    isActive: false,
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  const activeSetting = settings.find(s => s.isActive);

  const handleEdit = (setting: WorkPermitSettingsResponse) => {
    setEditingId(setting.id);
    setFormData({
      requireSafetyInduction: setting.requireSafetyInduction,
      enableFormValidation: setting.enableFormValidation,
      allowAttachments: setting.allowAttachments,
      maxAttachmentSizeMB: setting.maxAttachmentSizeMB,
      formInstructions: setting.formInstructions || '',
      isActive: setting.isActive,
    });
    setIsCreating(false);
  };

  const handleCreate = () => {
    setIsCreating(true);
    setEditingId(null);
    setFormData({
      requireSafetyInduction: false,
      enableFormValidation: true,
      allowAttachments: true,
      maxAttachmentSizeMB: 10,
      formInstructions: '',
      isActive: false,
    });
    setErrors({});
  };

  const handleCancel = () => {
    setIsCreating(false);
    setEditingId(null);
    setFormData({});
    setErrors({});
  };

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (formData.maxAttachmentSizeMB && (formData.maxAttachmentSizeMB < 1 || formData.maxAttachmentSizeMB > 100)) {
      newErrors.maxAttachmentSizeMB = 'Max attachment size must be between 1 and 100 MB';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validate()) return;

    try {
      // Transform camelCase to PascalCase for backend compatibility
      const transformedData = {
        RequireSafetyInduction: formData.requireSafetyInduction ?? false,
        EnableFormValidation: formData.enableFormValidation ?? true,
        AllowAttachments: formData.allowAttachments ?? true,
        MaxAttachmentSizeMB: formData.maxAttachmentSizeMB ?? 10,
        FormInstructions: formData.formInstructions || '',
        IsActive: formData.isActive ?? false,
      };

      if (isCreating) {
        await createSettings(transformedData as any).unwrap();
      } else if (editingId) {
        await updateSettings({
          id: Number(editingId),
          RequireSafetyInduction: formData.requireSafetyInduction ?? false,
          EnableFormValidation: formData.enableFormValidation ?? true,
          AllowAttachments: formData.allowAttachments ?? true,
          MaxAttachmentSizeMB: formData.maxAttachmentSizeMB ?? 10,
          FormInstructions: formData.formInstructions || '',
          IsActive: formData.isActive ?? false,
        } as any).unwrap();
      }
      handleCancel();
    } catch (error) {
      console.error('Failed to save settings:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this configuration?')) {
      try {
        await deleteSettings(id.toString()).unwrap();
      } catch (error) {
        console.error('Failed to delete settings:', error);
      }
    }
  };

  return (
    <div className="form-configuration-tab">
      {activeSetting && (
        <CAlert color="info" className="mb-4">
          <strong>Active Configuration:</strong> Work Permit Settings
          {activeSetting.requireSafetyInduction && (
            <span className="ms-2">(Safety induction required)</span>
          )}
        </CAlert>
      )}

      <CRow className="mb-3">
        <CCol className="d-flex justify-content-end">
          {!isCreating && !editingId && (
            <CButton color="primary" onClick={handleCreate} size="sm" className="w-100 w-md-auto">
              <FontAwesomeIcon icon={faPlus} className="me-2" />
              <span className="d-none d-sm-inline">Add Configuration</span>
              <span className="d-sm-none">Add</span>
            </CButton>
          )}
        </CCol>
      </CRow>

      {(isCreating || editingId) && (
        <CCard className="mb-4">
          <CCardBody className="p-2 p-md-3">
            <CForm>
              <CRow>
                <CCol lg={6} className="mb-3">
                  <CFormLabel htmlFor="maxAttachmentSize">Max Attachment Size (MB)</CFormLabel>
                  <CFormInput
                    id="maxAttachmentSize"
                    type="number"
                    min="1"
                    max="100"
                    value={formData.maxAttachmentSizeMB || 10}
                    onChange={(e) => setFormData({ ...formData, maxAttachmentSizeMB: parseInt(e.target.value) })}
                    invalid={!!errors.maxAttachmentSizeMB}
                    feedback={errors.maxAttachmentSizeMB}
                  />
                </CCol>
                <CCol lg={12} className="mb-3">
                  <CFormLabel htmlFor="formInstructions">Form Instructions</CFormLabel>
                  <CFormTextarea
                    id="formInstructions"
                    rows={3}
                    value={formData.formInstructions || ''}
                    onChange={(e) => setFormData({ ...formData, formInstructions: e.target.value })}
                    placeholder="Optional instructions to display on work permit forms"
                  />
                </CCol>
              </CRow>

              <CRow>
                <CCol sm={6} lg={4} className="mb-2 mb-lg-0">
                  <CFormCheck
                    id="requireSafetyInduction"
                    label="Require Safety Induction"
                    checked={formData.requireSafetyInduction || false}
                    onChange={(e) => setFormData({ ...formData, requireSafetyInduction: e.target.checked })}
                  />
                </CCol>
                <CCol sm={6} lg={4} className="mb-2 mb-lg-0">
                  <CFormCheck
                    id="enableFormValidation"
                    label="Enable Form Validation"
                    checked={formData.enableFormValidation || false}
                    onChange={(e) => setFormData({ ...formData, enableFormValidation: e.target.checked })}
                  />
                </CCol>
                <CCol sm={6} lg={4} className="mb-2 mb-lg-0">
                  <CFormCheck
                    id="allowAttachments"
                    label="Allow Attachments"
                    checked={formData.allowAttachments || false}
                    onChange={(e) => setFormData({ ...formData, allowAttachments: e.target.checked })}
                  />
                </CCol>
                <CCol sm={12} lg={4}>
                  <CFormCheck
                    id="isActive"
                    label="Set as Active"
                    checked={formData.isActive || false}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                  />
                </CCol>
              </CRow>

              <div className="d-flex flex-column flex-sm-row justify-content-end gap-2 mt-4">
                <CButton color="secondary" onClick={handleCancel} className="order-2 order-sm-1">
                  <FontAwesomeIcon icon={faTimes} className="me-2" />
                  Cancel
                </CButton>
                <CButton color="primary" onClick={handleSave} className="order-1 order-sm-2">
                  <FontAwesomeIcon icon={faSave} className="me-2" />
                  {isCreating ? 'Create' : 'Update'}
                </CButton>
              </div>
            </CForm>
          </CCardBody>
        </CCard>
      )}

      <div className="configurations-list">
        {settings.map((setting) => (
          <CCard key={setting.id} className="mb-3">
            <CCardBody className="p-2 p-md-3">
              <CRow className="align-items-start">
                <CCol xs={12} md={6} className="mb-2 mb-md-0">
                  <h6 className="mb-1">
                    Work Permit Configuration
                    {setting.isActive && (
                      <span className="badge bg-success ms-2 small">Active</span>
                    )}
                  </h6>
                  {setting.formInstructions && (
                    <p className="text-muted mb-1 small">{setting.formInstructions}</p>
                  )}
                </CCol>
                <CCol xs={12} md={3} className="mb-2 mb-md-0">
                  <div className="text-muted small">
                    {setting.requireSafetyInduction && (
                      <div>Safety Induction: Required</div>
                    )}
                    {setting.allowAttachments && (
                      <div>Max Attachment: {setting.maxAttachmentSizeMB}MB</div>
                    )}
                    {setting.enableFormValidation && (
                      <div>Form Validation: Enabled</div>
                    )}
                  </div>
                </CCol>
                <CCol xs={12} md={3} className="d-flex justify-content-end gap-1">
                  <CButton
                    color="primary"
                    variant="ghost"
                    size="sm"
                    onClick={() => handleEdit(setting)}
                    disabled={editingId !== null || isCreating}
                    className="flex-grow-1 flex-md-grow-0"
                  >
                    <FontAwesomeIcon icon={faEdit} className="me-1 d-md-none" />
                    <span className="d-md-none">Edit</span>
                    <FontAwesomeIcon icon={faEdit} className="d-none d-md-inline" />
                  </CButton>
                  <CButton
                    color="danger"
                    variant="ghost"
                    size="sm"
                    onClick={() => handleDelete(setting.id)}
                    disabled={editingId !== null || isCreating || setting.isActive}
                    className="flex-grow-1 flex-md-grow-0"
                  >
                    <FontAwesomeIcon icon={faTrash} className="me-1 d-md-none" />
                    <span className="d-md-none">Delete</span>
                    <FontAwesomeIcon icon={faTrash} className="d-none d-md-inline" />
                  </CButton>
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        ))}

        {settings.length === 0 && !isCreating && (
          <CAlert color="info">
            No configurations found. Click "Add Configuration" to create one.
          </CAlert>
        )}
      </div>
    </div>
  );
};