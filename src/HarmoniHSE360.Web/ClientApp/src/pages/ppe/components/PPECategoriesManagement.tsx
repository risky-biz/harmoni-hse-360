import React, { useState } from 'react';
import {
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CButton,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormInput,
  CFormTextarea,
  CFormSelect,
  CFormCheck,
  CAlert,
  CBadge,
  CSpinner,
  CInputGroup,
  CInputGroupText,
  CRow,
  CCol,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faEdit,
  faTrash,
  faSearch,
  faEye,
  faEyeSlash,
} from '@fortawesome/free-solid-svg-icons';

import {
  useGetPPECategoriesQuery,
  useCreatePPECategoryMutation,
  useUpdatePPECategoryMutation,
  useDeletePPECategoryMutation,
  PPECategory,
  CreatePPECategoryRequest,
} from '../../../features/ppe/ppeManagementApi';

const PPE_TYPES = [
  { value: 'HeadProtection', label: 'Head Protection' },
  { value: 'EyeProtection', label: 'Eye Protection' },
  { value: 'HearingProtection', label: 'Hearing Protection' },
  { value: 'RespiratoryProtection', label: 'Respiratory Protection' },
  { value: 'HandProtection', label: 'Hand Protection' },
  { value: 'FootProtection', label: 'Foot Protection' },
  { value: 'BodyProtection', label: 'Body Protection' },
  { value: 'FallProtection', label: 'Fall Protection' },
  { value: 'HighVisibility', label: 'High Visibility' },
  { value: 'EmergencyEquipment', label: 'Emergency Equipment' },
];

const PPECategoriesManagement: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingCategory, setEditingCategory] = useState<PPECategory | null>(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [categoryToDelete, setCategoryToDelete] = useState<PPECategory | null>(null);

  const [formData, setFormData] = useState<CreatePPECategoryRequest>({
    name: '',
    code: '',
    description: '',
    type: 'HeadProtection',
    requiresCertification: false,
    requiresInspection: false,
    inspectionIntervalDays: undefined,
    requiresExpiry: false,
    defaultExpiryDays: undefined,
    complianceStandard: '',
  });

  const { data: categories, isLoading, error } = useGetPPECategoriesQuery({
    isActive: showInactive ? undefined : true,
    searchTerm: searchTerm || undefined,
  });

  const [createCategory, { isLoading: isCreating }] = useCreatePPECategoryMutation();
  const [updateCategory, { isLoading: isUpdating }] = useUpdatePPECategoryMutation();
  const [deleteCategory, { isLoading: isDeleting }] = useDeletePPECategoryMutation();

  const handleOpenModal = (category?: PPECategory) => {
    if (category) {
      setEditingCategory(category);
      setFormData({
        name: category.name,
        code: category.code,
        description: category.description,
        type: category.type,
        requiresCertification: category.requiresCertification,
        requiresInspection: category.requiresInspection,
        inspectionIntervalDays: category.inspectionIntervalDays,
        requiresExpiry: category.requiresExpiry,
        defaultExpiryDays: category.defaultExpiryDays,
        complianceStandard: category.complianceStandard || '',
      });
    } else {
      setEditingCategory(null);
      setFormData({
        name: '',
        code: '',
        description: '',
        type: 'HeadProtection',
        requiresCertification: false,
        requiresInspection: false,
        inspectionIntervalDays: undefined,
        requiresExpiry: false,
        defaultExpiryDays: undefined,
        complianceStandard: '',
      });
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingCategory(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingCategory) {
        await updateCategory({ id: editingCategory.id, data: formData }).unwrap();
      } else {
        await createCategory(formData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save category:', error);
    }
  };

  const handleDelete = async () => {
    if (categoryToDelete) {
      try {
        await deleteCategory(categoryToDelete.id).unwrap();
        setShowDeleteModal(false);
        setCategoryToDelete(null);
      } catch (error) {
        console.error('Failed to delete category:', error);
      }
    }
  };

  const handleFormChange = (field: keyof CreatePPECategoryRequest, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  return (
    <div className="ppe-categories-management">
      {/* Header and Controls */}
      <CRow className="mb-3">
        <CCol md={6}>
          <CInputGroup>
            <CInputGroupText>
              <FontAwesomeIcon icon={faSearch} />
            </CInputGroupText>
            <CFormInput
              placeholder="Search categories..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </CInputGroup>
        </CCol>
        <CCol md={3}>
          <CFormCheck
            id="showInactive"
            label="Show inactive categories"
            checked={showInactive}
            onChange={(e) => setShowInactive(e.target.checked)}
          />
        </CCol>
        <CCol md={3} className="text-end">
          <CButton color="primary" onClick={() => handleOpenModal()}>
            <FontAwesomeIcon icon={faPlus} className="me-2" />
            Add Category
          </CButton>
        </CCol>
      </CRow>

      {/* Categories Table */}
      {isLoading && (
        <div className="text-center py-4">
          <CSpinner color="primary" />
        </div>
      )}
      
      {error && (
        <CAlert color="danger">
          Failed to load PPE categories
        </CAlert>
      )}

      {categories && (
        <CTable hover responsive>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell>Name</CTableHeaderCell>
              <CTableHeaderCell>Code</CTableHeaderCell>
              <CTableHeaderCell>Type</CTableHeaderCell>
              <CTableHeaderCell>Items</CTableHeaderCell>
              <CTableHeaderCell>Certification</CTableHeaderCell>
              <CTableHeaderCell>Inspection</CTableHeaderCell>
              <CTableHeaderCell>Status</CTableHeaderCell>
              <CTableHeaderCell>Actions</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {categories.map((category) => (
              <CTableRow key={category.id}>
                <CTableDataCell>
                  <div>
                    <strong>{category.name}</strong>
                    <br />
                    <small className="text-medium-emphasis">
                      {category.description}
                    </small>
                  </div>
                </CTableDataCell>
                <CTableDataCell>
                  <code>{category.code}</code>
                </CTableDataCell>
                <CTableDataCell>
                  {PPE_TYPES.find(t => t.value === category.type)?.label || category.type}
                </CTableDataCell>
                <CTableDataCell>
                  <CBadge color="info">{category.itemCount}</CBadge>
                </CTableDataCell>
                <CTableDataCell>
                  {category.requiresCertification ? (
                    <CBadge color="warning">Required</CBadge>
                  ) : (
                    <CBadge color="secondary">Optional</CBadge>
                  )}
                </CTableDataCell>
                <CTableDataCell>
                  {category.requiresInspection ? (
                    <CBadge color="info">
                      Every {category.inspectionIntervalDays} days
                    </CBadge>
                  ) : (
                    <CBadge color="secondary">Not required</CBadge>
                  )}
                </CTableDataCell>
                <CTableDataCell>
                  <CBadge color={category.isActive ? 'success' : 'secondary'}>
                    {category.isActive ? 'Active' : 'Inactive'}
                  </CBadge>
                </CTableDataCell>
                <CTableDataCell>
                  <CButton
                    color="outline-primary"
                    size="sm"
                    className="me-2"
                    onClick={() => handleOpenModal(category)}
                  >
                    <FontAwesomeIcon icon={faEdit} />
                  </CButton>
                  <CButton
                    color="outline-danger"
                    size="sm"
                    onClick={() => {
                      setCategoryToDelete(category);
                      setShowDeleteModal(true);
                    }}
                    disabled={category.itemCount > 0}
                  >
                    <FontAwesomeIcon icon={faTrash} />
                  </CButton>
                </CTableDataCell>
              </CTableRow>
            ))}
          </CTableBody>
        </CTable>
      )}

      {/* Add/Edit Modal */}
      <CModal visible={showModal} onClose={handleCloseModal} size="lg">
        <CModalHeader>
          <CModalTitle>
            {editingCategory ? 'Edit Category' : 'Add New Category'}
          </CModalTitle>
        </CModalHeader>
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            <CRow>
              <CCol md={6}>
                <CFormInput
                  type="text"
                  label="Name"
                  placeholder="Enter category name"
                  value={formData.name}
                  onChange={(e) => handleFormChange('name', e.target.value)}
                  required
                />
              </CCol>
              <CCol md={6}>
                <CFormInput
                  type="text"
                  label="Code"
                  placeholder="Enter category code"
                  value={formData.code}
                  onChange={(e) => handleFormChange('code', e.target.value)}
                  required
                />
              </CCol>
            </CRow>
            <CRow>
              <CCol md={12}>
                <CFormTextarea
                  label="Description"
                  placeholder="Enter category description"
                  value={formData.description}
                  onChange={(e) => handleFormChange('description', e.target.value)}
                  rows={3}
                  required
                />
              </CCol>
            </CRow>
            <CRow>
              <CCol md={6}>
                <CFormSelect
                  label="PPE Type"
                  value={formData.type}
                  onChange={(e) => handleFormChange('type', e.target.value)}
                  required
                >
                  {PPE_TYPES.map((type) => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={6}>
                <CFormInput
                  type="text"
                  label="Compliance Standard"
                  placeholder="e.g., EN 397, ANSI Z89.1"
                  value={formData.complianceStandard}
                  onChange={(e) => handleFormChange('complianceStandard', e.target.value)}
                />
              </CCol>
            </CRow>
            <CRow>
              <CCol md={6}>
                <CFormCheck
                  id="requiresCertification"
                  label="Requires Certification"
                  checked={formData.requiresCertification}
                  onChange={(e) => handleFormChange('requiresCertification', e.target.checked)}
                />
              </CCol>
              <CCol md={6}>
                <CFormCheck
                  id="requiresExpiry"
                  label="Has Expiry Date"
                  checked={formData.requiresExpiry}
                  onChange={(e) => handleFormChange('requiresExpiry', e.target.checked)}
                />
              </CCol>
            </CRow>
            <CRow>
              <CCol md={6}>
                <CFormCheck
                  id="requiresInspection"
                  label="Requires Regular Inspection"
                  checked={formData.requiresInspection}
                  onChange={(e) => handleFormChange('requiresInspection', e.target.checked)}
                />
                {formData.requiresInspection && (
                  <CFormInput
                    type="number"
                    label="Inspection Interval (days)"
                    placeholder="e.g., 30, 90"
                    value={formData.inspectionIntervalDays || ''}
                    onChange={(e) => handleFormChange('inspectionIntervalDays', parseInt(e.target.value) || undefined)}
                    className="mt-2"
                  />
                )}
              </CCol>
              <CCol md={6}>
                {formData.requiresExpiry && (
                  <CFormInput
                    type="number"
                    label="Default Expiry (days)"
                    placeholder="e.g., 365, 730"
                    value={formData.defaultExpiryDays || ''}
                    onChange={(e) => handleFormChange('defaultExpiryDays', parseInt(e.target.value) || undefined)}
                  />
                )}
              </CCol>
            </CRow>
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" onClick={handleCloseModal}>
              Cancel
            </CButton>
            <CButton color="primary" type="submit" disabled={isCreating || isUpdating}>
              {(isCreating || isUpdating) && <CSpinner size="sm" className="me-2" />}
              {editingCategory ? 'Update' : 'Create'}
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>

      {/* Delete Confirmation Modal */}
      <CModal visible={showDeleteModal} onClose={() => setShowDeleteModal(false)}>
        <CModalHeader>
          <CModalTitle>Confirm Delete</CModalTitle>
        </CModalHeader>
        <CModalBody>
          Are you sure you want to delete the category "{categoryToDelete?.name}"?
          This action cannot be undone.
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowDeleteModal(false)}>
            Cancel
          </CButton>
          <CButton color="danger" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting && <CSpinner size="sm" className="me-2" />}
            Delete
          </CButton>
        </CModalFooter>
      </CModal>
    </div>
  );
};

export default PPECategoriesManagement;