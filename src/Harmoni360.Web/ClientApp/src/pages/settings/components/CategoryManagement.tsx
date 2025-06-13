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
  faTag,
  faSave,
  faTimes
} from '@fortawesome/free-solid-svg-icons';

// Define interfaces for IncidentCategory
interface IncidentCategory {
  id: number;
  name: string;
  code: string;
  description?: string;
  color?: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

interface CategoryCreateRequest {
  name: string;
  code: string;
  description?: string;
  color?: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  displayOrder: number;
  isActive: boolean;
}

const CategoryManagement: React.FC = () => {
  const [categories, setCategories] = useState<IncidentCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingCategory, setEditingCategory] = useState<IncidentCategory | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Form state
  const [formData, setFormData] = useState<CategoryCreateRequest>({
    name: '',
    code: '',
    description: '',
    color: '#007bff',
    severity: 'Medium',
    displayOrder: 0,
    isActive: true,
  });

  // Predefined category options
  const predefinedCategories = [
    { name: 'Slip, Trip, Fall', code: 'STF', severity: 'Medium' as const, color: '#fd7e14' },
    { name: 'Equipment Malfunction', code: 'EQUIP', severity: 'High' as const, color: '#dc3545' },
    { name: 'Chemical Exposure', code: 'CHEM', severity: 'Critical' as const, color: '#6f42c1' },
    { name: 'Fire/Explosion', code: 'FIRE', severity: 'Critical' as const, color: '#dc3545' },
    { name: 'Vehicle Accident', code: 'VEH', severity: 'High' as const, color: '#fd7e14' },
    { name: 'Environmental Spill', code: 'SPILL', severity: 'High' as const, color: '#198754' },
    { name: 'Electrical Incident', code: 'ELEC', severity: 'High' as const, color: '#ffc107' },
    { name: 'Personal Injury', code: 'INJ', severity: 'Medium' as const, color: '#0dcaf0' },
    { name: 'Near Miss', code: 'MISS', severity: 'Low' as const, color: '#6c757d' },
    { name: 'Property Damage', code: 'PROP', severity: 'Medium' as const, color: '#fd7e14' },
  ];

  // Load categories on component mount
  useEffect(() => {
    loadCategories();
  }, []);

  const getAuthHeaders = (): HeadersInit => {
    const state = store.getState();
    const token = state.auth.token;
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };
    
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
    
    return headers;
  };

  const loadCategories = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const response = await fetch('/api/configuration/incident-categories', {
        headers: getAuthHeaders(),
      });
      if (!response.ok) {
        throw new Error(`Failed to load categories: ${response.statusText}`);
      }
      
      const data = await response.json();
      setCategories(data || []);
    } catch (error) {
      console.error('Error loading categories:', error);
      setError(error instanceof Error ? error.message : 'Failed to load categories');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenModal = (category?: IncidentCategory) => {
    if (category) {
      setEditingCategory(category);
      setFormData({
        name: category.name,
        code: category.code,
        description: category.description || '',
        color: category.color || '#007bff',
        severity: category.severity,
        displayOrder: category.displayOrder,
        isActive: category.isActive,
      });
    } else {
      setEditingCategory(null);
      setFormData({
        name: '',
        code: '',
        description: '',
        color: '#007bff',
        severity: 'Medium',
        displayOrder: categories.length,
        isActive: true,
      });
    }
    setShowModal(true);
    setError(null);
    setSuccess(null);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingCategory(null);
    setError(null);
    setSuccess(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setIsSubmitting(true);
      setError(null);

      const url = editingCategory 
        ? `/api/configuration/incident-categories/${editingCategory.id}`
        : '/api/configuration/incident-categories';
      
      const method = editingCategory ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: getAuthHeaders(),
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Failed to ${editingCategory ? 'update' : 'create'} category`);
      }

      setSuccess(`Category ${editingCategory ? 'updated' : 'created'} successfully`);
      handleCloseModal();
      await loadCategories();
    } catch (error) {
      console.error('Error saving category:', error);
      setError(error instanceof Error ? error.message : 'Failed to save category');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (category: IncidentCategory) => {
    if (!window.confirm(`Are you sure you want to delete the category "${category.name}"?`)) {
      return;
    }

    try {
      setError(null);
      
      const response = await fetch(`/api/configuration/incident-categories/${category.id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to delete category');
      }

      setSuccess('Category deleted successfully');
      await loadCategories();
    } catch (error) {
      console.error('Error deleting category:', error);
      setError(error instanceof Error ? error.message : 'Failed to delete category');
    }
  };

  const handleAddPredefined = (predefined: typeof predefinedCategories[0]) => {
    setFormData({
      name: predefined.name,
      code: predefined.code,
      description: '',
      color: predefined.color,
      severity: predefined.severity,
      displayOrder: categories.length,
      isActive: true,
    });
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'Critical': return 'danger';
      case 'High': return 'warning';
      case 'Medium': return 'info';
      case 'Low': return 'secondary';
      default: return 'primary';
    }
  };

  const filteredCategories = categories.filter((category) =>
    category.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    category.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
    category.severity.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <>
      {error && (
        <CAlert color="danger" className="mb-3" dismissible onClose={() => setError(null)}>
          {error}
        </CAlert>
      )}

      {success && (
        <CAlert color="success" className="mb-3" dismissible onClose={() => setSuccess(null)}>
          {success}
        </CAlert>
      )}

      <CRow className="mb-3">
        <CCol md={6}>
          <CInputGroup>
            <CInputGroup className="has-validation">
              <span className="input-group-text">
                <FontAwesomeIcon icon={faSearch} />
              </span>
              <CFormInput
                type="text"
                placeholder="Search categories..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </CInputGroup>
          </CInputGroup>
        </CCol>
        <CCol md={6} className="text-end">
          <CButton color="primary" onClick={() => handleOpenModal()}>
            <FontAwesomeIcon icon={faPlus} className="me-2" />
            Add Category
          </CButton>
        </CCol>
      </CRow>

      <CCard>
        <CCardHeader>
          <FontAwesomeIcon icon={faTag} className="me-2" />
          Incident Categories ({filteredCategories.length})
        </CCardHeader>
        <CCardBody>
          {isLoading ? (
            <div className="text-center py-4">
              <CSpinner color="primary" />
              <div className="mt-2">Loading categories...</div>
            </div>
          ) : (
            <CTable hover responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Name</CTableHeaderCell>
                  <CTableHeaderCell>Code</CTableHeaderCell>
                  <CTableHeaderCell>Severity</CTableHeaderCell>
                  <CTableHeaderCell>Color</CTableHeaderCell>
                  <CTableHeaderCell>Status</CTableHeaderCell>
                  <CTableHeaderCell>Actions</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {filteredCategories.length === 0 ? (
                  <CTableRow>
                    <CTableDataCell colSpan={6} className="text-center py-4">
                      <div className="text-muted">
                        <FontAwesomeIcon icon={faTag} size="2x" className="mb-2 opacity-50" />
                        <p className="mb-0">No categories found</p>
                        {searchTerm && <small>Try adjusting your search criteria</small>}
                      </div>
                    </CTableDataCell>
                  </CTableRow>
                ) : (
                  filteredCategories.map((category) => (
                    <CTableRow key={category.id}>
                      <CTableDataCell>
                        <div className="fw-semibold">{category.name}</div>
                        {category.description && (
                          <small className="text-muted">{category.description}</small>
                        )}
                      </CTableDataCell>
                      <CTableDataCell>
                        <code className="bg-light px-2 py-1 rounded">{category.code}</code>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={getSeverityColor(category.severity)}>
                          {category.severity}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex align-items-center">
                          <div
                            className="rounded-circle me-2"
                            style={{
                              width: '16px',
                              height: '16px',
                              backgroundColor: category.color || '#007bff',
                            }}
                          />
                          <small className="text-muted">{category.color}</small>
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={category.isActive ? 'success' : 'secondary'}>
                          {category.isActive ? 'Active' : 'Inactive'}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex gap-2">
                          <CButton
                            color="primary"
                            variant="outline"
                            size="sm"
                            onClick={() => handleOpenModal(category)}
                          >
                            <FontAwesomeIcon icon={faEdit} />
                          </CButton>
                          <CButton
                            color="danger"
                            variant="outline"
                            size="sm"
                            onClick={() => handleDelete(category)}
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
            {editingCategory ? 'Edit Category' : 'Add New Category'}
          </CModalTitle>
        </CModalHeader>
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            {!editingCategory && (
              <CAlert color="info" className="mb-3">
                <strong>Quick Add:</strong> Click on a predefined category to auto-fill the form.
                <div className="mt-2">
                  <div className="d-flex flex-wrap gap-1">
                    {predefinedCategories.map((predefined, index) => (
                      <CButton
                        key={index}
                        size="sm"
                        color="light"
                        variant="outline"
                        onClick={() => handleAddPredefined(predefined)}
                      >
                        {predefined.name}
                      </CButton>
                    ))}
                  </div>
                </div>
              </CAlert>
            )}

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="name">Category Name *</CFormLabel>
                <CFormInput
                  type="text"
                  id="name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="code">Category Code *</CFormLabel>
                <CFormInput
                  type="text"
                  id="code"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  placeholder="e.g. STF, EQUIP, CHEM"
                  required
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol>
                <CFormLabel htmlFor="description">Description</CFormLabel>
                <CFormTextarea
                  id="description"
                  rows={3}
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Brief description of this category"
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={4}>
                <CFormLabel htmlFor="severity">Severity Level *</CFormLabel>
                <CFormSelect
                  id="severity"
                  value={formData.severity}
                  onChange={(e) => setFormData({ ...formData, severity: e.target.value as any })}
                  required
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </CFormSelect>
              </CCol>
              <CCol md={4}>
                <CFormLabel htmlFor="color">Color</CFormLabel>
                <CFormInput
                  type="color"
                  id="color"
                  value={formData.color}
                  onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                />
              </CCol>
              <CCol md={4}>
                <CFormLabel htmlFor="isActive">Status</CFormLabel>
                <CFormSelect
                  id="isActive"
                  value={formData.isActive ? 'true' : 'false'}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.value === 'true' })}
                >
                  <option value="true">Active</option>
                  <option value="false">Inactive</option>
                </CFormSelect>
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="displayOrder">Display Order</CFormLabel>
                <CFormInput
                  type="number"
                  id="displayOrder"
                  value={formData.displayOrder}
                  onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 0 })}
                  min="0"
                />
              </CCol>
            </CRow>
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" onClick={handleCloseModal}>
              <FontAwesomeIcon icon={faTimes} className="me-2" />
              Cancel
            </CButton>
            <CButton color="primary" type="submit" disabled={isSubmitting}>
              {isSubmitting ? (
                <CSpinner size="sm" className="me-2" />
              ) : (
                <FontAwesomeIcon icon={faSave} className="me-2" />
              )}
              {editingCategory ? 'Update' : 'Create'} Category
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </>
  );
};

export default CategoryManagement;