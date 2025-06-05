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
  useGetPPESizesQuery,
  useCreatePPESizeMutation,
  useUpdatePPESizeMutation,
  useDeletePPESizeMutation,
  PPESize,
  CreatePPESizeRequest,
} from '../../../features/ppe/ppeManagementApi';

const PPESizesManagement: React.FC = () => {
  const [showModal, setShowModal] = useState(false);
  const [editingSize, setEditingSize] = useState<PPESize | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [formData, setFormData] = useState<CreatePPESizeRequest>({
    name: '',
    code: '',
    description: '',
    sortOrder: 1,
  });

  const { data: sizes, isLoading, error } = useGetPPESizesQuery({
    isActive: showInactive ? undefined : true,
    searchTerm: searchTerm || undefined,
  });

  const [createSize] = useCreatePPESizeMutation();
  const [updateSize] = useUpdatePPESizeMutation();
  const [deleteSize] = useDeletePPESizeMutation();

  const resetForm = () => {
    setFormData({
      name: '',
      code: '',
      description: '',
      sortOrder: 1,
    });
    setEditingSize(null);
  };

  const handleOpenModal = (size?: PPESize) => {
    if (size) {
      setEditingSize(size);
      setFormData({
        name: size.name,
        code: size.code,
        description: size.description || '',
        sortOrder: size.sortOrder,
      });
    } else {
      resetForm();
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    resetForm();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (editingSize) {
        await updateSize({ id: editingSize.id, data: formData }).unwrap();
      } else {
        await createSize(formData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save size:', error);
    }
  };

  const handleDelete = async (size: PPESize) => {
    if (window.confirm(`Are you sure you want to delete "${size.name}"?`)) {
      try {
        await deleteSize(size.id).unwrap();
      } catch (error) {
        console.error('Failed to delete size:', error);
      }
    }
  };

  const filteredSizes = sizes?.filter(size =>
    size.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    size.code.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  return (
    <div>
      <CRow className="mb-3">
        <CCol md={6}>
          <CInputGroup>
            <CInputGroupText>
              <FontAwesomeIcon icon={faSearch} />
            </CInputGroupText>
            <CFormInput
              placeholder="Search sizes..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </CInputGroup>
        </CCol>
        <CCol md={6} className="d-flex justify-content-end align-items-center gap-2">
          <CButton
            color="outline-secondary"
            onClick={() => setShowInactive(!showInactive)}
          >
            <FontAwesomeIcon icon={showInactive ? faEyeSlash : faEye} className="me-1" />
            {showInactive ? 'Hide Inactive' : 'Show Inactive'}
          </CButton>
          <CButton color="primary" onClick={() => handleOpenModal()}>
            <FontAwesomeIcon icon={faPlus} className="me-1" />
            Add Size
          </CButton>
        </CCol>
      </CRow>

      {isLoading && (
        <div className="text-center p-3">
          <CSpinner />
        </div>
      )}
      
      {error && (
        <CAlert color="danger">
          Failed to load PPE sizes
        </CAlert>
      )}

      {sizes && (
        <CTable hover responsive>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell>Name</CTableHeaderCell>
              <CTableHeaderCell>Code</CTableHeaderCell>
              <CTableHeaderCell>Description</CTableHeaderCell>
              <CTableHeaderCell>Sort Order</CTableHeaderCell>
              <CTableHeaderCell>Status</CTableHeaderCell>
              <CTableHeaderCell>Actions</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {filteredSizes.map((size) => (
              <CTableRow key={size.id}>
                <CTableDataCell className="fw-semibold">{size.name}</CTableDataCell>
                <CTableDataCell>
                  <CBadge color="secondary">{size.code}</CBadge>
                </CTableDataCell>
                <CTableDataCell>{size.description || '-'}</CTableDataCell>
                <CTableDataCell>{size.sortOrder}</CTableDataCell>
                <CTableDataCell>
                  <CBadge color={size.isActive ? 'success' : 'secondary'}>
                    {size.isActive ? 'Active' : 'Inactive'}
                  </CBadge>
                </CTableDataCell>
                <CTableDataCell>
                  <div className="d-flex gap-1">
                    <CButton
                      color="outline-primary"
                      size="sm"
                      onClick={() => handleOpenModal(size)}
                    >
                      <FontAwesomeIcon icon={faEdit} />
                    </CButton>
                    <CButton
                      color="outline-danger"
                      size="sm"
                      onClick={() => handleDelete(size)}
                    >
                      <FontAwesomeIcon icon={faTrash} />
                    </CButton>
                  </div>
                </CTableDataCell>
              </CTableRow>
            ))}
            {filteredSizes.length === 0 && !isLoading && (
              <CTableRow>
                <CTableDataCell colSpan={6} className="text-center text-muted">
                  No sizes found
                </CTableDataCell>
              </CTableRow>
            )}
          </CTableBody>
        </CTable>
      )}

      {/* Add/Edit Modal */}
      <CModal visible={showModal} onClose={handleCloseModal} size="lg">
        <CModalHeader>
          <CModalTitle>
            {editingSize ? 'Edit Size' : 'Add New Size'}
          </CModalTitle>
        </CModalHeader>
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            <CRow className="mb-3">
              <CCol md={6}>
                <label className="form-label">Name *</label>
                <CFormInput
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g., Medium, Large, Size 10"
                />
              </CCol>
              <CCol md={6}>
                <label className="form-label">Code *</label>
                <CFormInput
                  required
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  placeholder="e.g., M, L, 10"
                />
              </CCol>
            </CRow>
            
            <CRow className="mb-3">
              <CCol md={6}>
                <label className="form-label">Sort Order</label>
                <CFormInput
                  type="number"
                  min="1"
                  value={formData.sortOrder}
                  onChange={(e) => setFormData({ ...formData, sortOrder: parseInt(e.target.value) || 1 })}
                />
              </CCol>
            </CRow>

            <div className="mb-3">
              <label className="form-label">Description</label>
              <CFormTextarea
                rows={2}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Optional description for this size"
              />
            </div>
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" onClick={handleCloseModal}>
              Cancel
            </CButton>
            <CButton color="primary" type="submit">
              {editingSize ? 'Update' : 'Create'} Size
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </div>
  );
};

export default PPESizesManagement;