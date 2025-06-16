import React, { useState, useEffect } from 'react';
import {
  CButton,
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CForm,
  CFormInput,
  CFormLabel,
  CFormTextarea,
  CInputGroup,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CRow,
  CSpinner,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CAlert,
  CBadge,
  CFormSelect,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { store } from '../../../store';
import { 
  faPlus, 
  faEdit, 
  faTrash, 
  faSearch, 
  faList,
  faSave,
  faTimes
} from '@fortawesome/free-solid-svg-icons';
import { HAZARD_ICONS } from '../../../utils/iconMappings';

// Define interfaces for HazardType
interface HazardType {
  id: number;
  name: string;
  code: string;
  description?: string;
  categoryId?: number;
  categoryName?: string;
  riskMultiplier: number;
  requiresPermit: boolean;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

interface HazardTypeCreateRequest {
  name: string;
  code: string;
  description?: string;
  categoryId?: number;
  riskMultiplier: number;
  requiresPermit: boolean;
  displayOrder: number;
  isActive: boolean;
}

const HazardTypeManagement: React.FC = () => {
  const [types, setTypes] = useState<HazardType[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingType, setEditingType] = useState<HazardType | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Form state
  const [formData, setFormData] = useState<HazardTypeCreateRequest>({
    name: '',
    code: '',
    description: '',
    categoryId: undefined,
    riskMultiplier: 1.0,
    requiresPermit: false,
    displayOrder: 0,
    isActive: true,
  });


  // Load types and categories on component mount
  useEffect(() => {
    loadTypes();
    loadCategories();
  }, []);

  const getAuthHeaders = (): Record<string, string> => {
    const state = store.getState();
    const token = state.auth.token;
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };
    
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    
    return headers;
  };

  const loadTypes = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const response = await fetch('/api/configuration/hazard-types', {
        headers: getAuthHeaders(),
      });
      if (!response.ok) {
        throw new Error(`Failed to load hazard types: ${response.statusText}`);
      }
      
      const data = await response.json();
      setTypes(data || []);
    } catch (error) {
      console.error('Error loading hazard types:', error);
      setError(error instanceof Error ? error.message : 'Failed to load hazard types');
    } finally {
      setIsLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const response = await fetch('/api/configuration/hazard-categories', {
        headers: getAuthHeaders(),
      });
      if (response.ok) {
        const data = await response.json();
        setCategories(data || []);
      }
    } catch (error) {
      console.error('Error loading hazard categories:', error);
    }
  };

  const handleOpenModal = (type?: HazardType) => {
    if (type) {
      setEditingType(type);
      setFormData({
        name: type.name,
        code: type.code,
        description: type.description || '',
        categoryId: type.categoryId,
        riskMultiplier: type.riskMultiplier,
        requiresPermit: type.requiresPermit,
        displayOrder: type.displayOrder,
        isActive: type.isActive,
      });
    } else {
      setEditingType(null);
      setFormData({
        name: '',
        code: '',
        description: '',
        categoryId: undefined,
        riskMultiplier: 1.0,
        requiresPermit: false,
        displayOrder: types.length,
        isActive: true,
      });
    }
    setShowModal(true);
    setError(null);
    setSuccess(null);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingType(null);
    setError(null);
    setSuccess(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setIsSubmitting(true);
      setError(null);

      const url = editingType 
        ? `/api/configuration/hazard-types/${editingType.id}`
        : '/api/configuration/hazard-types';
      
      const method = editingType ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: getAuthHeaders(),
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Failed to ${editingType ? 'update' : 'create'} hazard type`);
      }

      setSuccess(`Hazard type ${editingType ? 'updated' : 'created'} successfully!`);
      await loadTypes();
      handleCloseModal();
    } catch (error) {
      console.error('Error saving hazard type:', error);
      setError(error instanceof Error ? error.message : 'Failed to save hazard type');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this hazard type?')) {
      return;
    }

    try {
      setError(null);
      const response = await fetch(`/api/configuration/hazard-types/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to delete hazard type');
      }

      setSuccess('Hazard type deleted successfully!');
      await loadTypes();
    } catch (error) {
      console.error('Error deleting hazard type:', error);
      setError(error instanceof Error ? error.message : 'Failed to delete hazard type');
    }
  };


  const handleInputChange = (field: keyof HazardTypeCreateRequest, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const getRiskMultiplierColor = (multiplier: number) => {
    if (multiplier >= 2.0) return 'danger';
    if (multiplier >= 1.5) return 'warning';
    if (multiplier >= 1.2) return 'info';
    return 'success';
  };

  const filteredTypes = types.filter(type =>
    type.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    type.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (type.categoryName && type.categoryName.toLowerCase().includes(searchTerm.toLowerCase()))
  );

  return (
    <div>
      {/* Alert Messages */}
      {error && (
        <CAlert color="danger" dismissible onClose={() => setError(null)}>
          {error}
        </CAlert>
      )}
      {success && (
        <CAlert color="success" dismissible onClose={() => setSuccess(null)}>
          {success}
        </CAlert>
      )}

      {/* Header with Search and Add Button */}
      <CRow className="mb-4">
        <CCol md={8}>
          <CInputGroup>
            <CFormInput
              type="text"
              placeholder="Search hazard types..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
            <CButton color="primary" variant="outline">
              <FontAwesomeIcon icon={faSearch} />
            </CButton>
          </CInputGroup>
        </CCol>
        <CCol md={4} className="text-end">
          <CButton color="warning" onClick={() => handleOpenModal()}>
            <FontAwesomeIcon icon={faPlus} className="me-1" />
            Add Hazard Type
          </CButton>
        </CCol>
      </CRow>


      {/* Types Table */}
      <CCard>
        <CCardHeader>
          <FontAwesomeIcon icon={faList} className="me-2" />
          <strong>Hazard Types ({filteredTypes.length})</strong>
        </CCardHeader>
        <CCardBody>
          {isLoading ? (
            <div className="text-center py-4">
              <CSpinner color="primary" />
              <div className="mt-2">Loading hazard types...</div>
            </div>
          ) : (
            <CTable hover responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Code</CTableHeaderCell>
                  <CTableHeaderCell>Name</CTableHeaderCell>
                  <CTableHeaderCell>Category</CTableHeaderCell>
                  <CTableHeaderCell>Risk Multiplier</CTableHeaderCell>
                  <CTableHeaderCell>Permit Required</CTableHeaderCell>
                  <CTableHeaderCell>Status</CTableHeaderCell>
                  <CTableHeaderCell>Actions</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {filteredTypes.length === 0 ? (
                  <CTableRow>
                    <CTableDataCell colSpan={7} className="text-center text-muted py-4">
                      No hazard types found. {searchTerm && 'Try adjusting your search or '}
                      <CButton
                        color="link"
                        className="p-0"
                        onClick={() => handleOpenModal()}
                      >
                        add a new type
                      </CButton>
                    </CTableDataCell>
                  </CTableRow>
                ) : (
                  filteredTypes.map((type) => (
                    <CTableRow key={type.id}>
                      <CTableDataCell>
                        <CBadge color={getRiskMultiplierColor(type.riskMultiplier)}>
                          {type.code}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <strong>{type.name}</strong>
                        {type.description && (
                          <div className="small text-muted">
                            {type.description}
                          </div>
                        )}
                      </CTableDataCell>
                      <CTableDataCell>
                        {type.categoryName ? (
                          <CBadge color="info" className="text-white">
                            {type.categoryName}
                          </CBadge>
                        ) : (
                          <span className="text-muted">-</span>
                        )}
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={getRiskMultiplierColor(type.riskMultiplier)}>
                          {type.riskMultiplier}x
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        {type.requiresPermit ? (
                          <CBadge color="warning">
                            🔒 Required
                          </CBadge>
                        ) : (
                          <CBadge color="success">
                            ✓ Not Required
                          </CBadge>
                        )}
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={type.isActive ? 'success' : 'secondary'}>
                          {type.isActive ? 'Active' : 'Inactive'}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex gap-1">
                          <CButton
                            color="info"
                            variant="outline"
                            size="sm"
                            onClick={() => handleOpenModal(type)}
                            title="Edit type"
                          >
                            <FontAwesomeIcon icon={faEdit} />
                          </CButton>
                          <CButton
                            color="danger"
                            variant="outline"
                            size="sm"
                            onClick={() => handleDelete(type.id)}
                            title="Delete type"
                          >
                            <FontAwesomeIcon icon={faTrash} />
                          </CButton>
                        </div>
                      </CTableDataCell>
                    </CTableRow>
                  ))
                )}
              </CTableBody>
            </CTable>
          )}
        </CCardBody>
      </CCard>

      {/* Add/Edit Modal */}
      <CModal visible={showModal} onClose={handleCloseModal} size="lg">
        <CModalHeader>
          <CModalTitle>
            <FontAwesomeIcon icon={editingType ? faEdit : faPlus} className="me-2" />
            {editingType ? 'Edit Hazard Type' : 'Add New Hazard Type'}
          </CModalTitle>
        </CModalHeader>
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="name">Type Name *</CFormLabel>
                <CFormInput
                  id="name"
                  value={formData.name}
                  onChange={(e) => handleInputChange('name', e.target.value)}
                  placeholder="Enter type name"
                  required
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="code">Code *</CFormLabel>
                <CFormInput
                  id="code"
                  value={formData.code}
                  onChange={(e) => handleInputChange('code', e.target.value.toUpperCase())}
                  placeholder="Enter code (e.g., TOX)"
                  maxLength={10}
                  required
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="categoryId">Hazard Category</CFormLabel>
                <CFormSelect
                  id="categoryId"
                  value={formData.categoryId || ''}
                  onChange={(e) => handleInputChange('categoryId', e.target.value ? parseInt(e.target.value) : undefined)}
                >
                  <option value="">Select Category</option>
                  {categories.map(category => (
                    <option key={category.id} value={category.id}>
                      {category.name}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="riskMultiplier">Risk Multiplier *</CFormLabel>
                <CFormInput
                  id="riskMultiplier"
                  type="number"
                  step="0.1"
                  min="0.1"
                  max="5.0"
                  value={formData.riskMultiplier}
                  onChange={(e) => handleInputChange('riskMultiplier', parseFloat(e.target.value))}
                  required
                />
                <small className="text-muted">1.0 = normal risk, higher values increase risk assessment</small>
              </CCol>
            </CRow>

            <div className="mb-3">
              <CFormLabel htmlFor="description">Description</CFormLabel>
              <CFormTextarea
                id="description"
                rows={3}
                value={formData.description}
                onChange={(e) => handleInputChange('description', e.target.value)}
                placeholder="Enter type description"
              />
            </div>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="displayOrder">Display Order</CFormLabel>
                <CFormInput
                  id="displayOrder"
                  type="number"
                  value={formData.displayOrder}
                  onChange={(e) => handleInputChange('displayOrder', parseInt(e.target.value))}
                  min={0}
                />
              </CCol>
              <CCol md={6} className="d-flex align-items-end gap-3">
                <div className="form-check">
                  <input
                    className="form-check-input"
                    type="checkbox"
                    id="requiresPermit"
                    checked={formData.requiresPermit}
                    onChange={(e) => handleInputChange('requiresPermit', e.target.checked)}
                  />
                  <label className="form-check-label" htmlFor="requiresPermit">
                    Requires Permit
                  </label>
                </div>
                <div className="form-check">
                  <input
                    className="form-check-input"
                    type="checkbox"
                    id="isActive"
                    checked={formData.isActive}
                    onChange={(e) => handleInputChange('isActive', e.target.checked)}
                  />
                  <label className="form-check-label" htmlFor="isActive">
                    Active
                  </label>
                </div>
              </CCol>
            </CRow>
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" onClick={handleCloseModal}>
              <FontAwesomeIcon icon={faTimes} className="me-1" />
              Cancel
            </CButton>
            <CButton color="warning" type="submit" disabled={isSubmitting}>
              {isSubmitting && <CSpinner size="sm" className="me-2" />}
              <FontAwesomeIcon icon={faSave} className="me-1" />
              {editingType ? 'Update' : 'Create'} Type
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </div>
  );
};

export default HazardTypeManagement;