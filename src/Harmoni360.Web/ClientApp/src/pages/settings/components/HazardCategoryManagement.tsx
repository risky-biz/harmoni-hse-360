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
  CFormText,
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
import { HAZARD_ICONS } from '../../../utils/iconMappings';

// Define interfaces for HazardCategory
interface HazardCategory {
  id: number;
  name: string;
  code: string;
  description?: string;
  color?: string;
  riskLevel: 'Low' | 'Medium' | 'High' | 'Critical';
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

interface HazardCategoryCreateRequest {
  name: string;
  code: string;
  description?: string;
  color?: string;
  riskLevel: 'Low' | 'Medium' | 'High' | 'Critical';
  displayOrder: number;
  isActive: boolean;
}

const HazardCategoryManagement: React.FC = () => {
  const [categories, setCategories] = useState<HazardCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingCategory, setEditingCategory] = useState<HazardCategory | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  // Form state
  const [formData, setFormData] = useState<HazardCategoryCreateRequest>({
    name: '',
    code: '',
    description: '',
    color: '#fd7e14',
    riskLevel: 'Medium',
    displayOrder: 0,
    isActive: true,
  });


  // Load categories on component mount
  useEffect(() => {
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

  const loadCategories = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const response = await fetch('/api/configuration/hazard-categories', {
        headers: getAuthHeaders(),
      });
      if (!response.ok) {
        throw new Error(`Failed to load hazard categories: ${response.statusText}`);
      }
      
      const data = await response.json();
      setCategories(data || []);
    } catch (error) {
      console.error('Error loading hazard categories:', error);
      setError(error instanceof Error ? error.message : 'Failed to load hazard categories');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenModal = (category?: HazardCategory) => {
    if (category) {
      setEditingCategory(category);
      setFormData({
        name: category.name,
        code: category.code,
        description: category.description || '',
        color: category.color || '#fd7e14',
        riskLevel: category.riskLevel,
        displayOrder: category.displayOrder,
        isActive: category.isActive,
      });
    } else {
      setEditingCategory(null);
      setFormData({
        name: '',
        code: '',
        description: '',
        color: '#fd7e14',
        riskLevel: 'Medium',
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
    setValidationErrors({});
    setFormData({
      name: '',
      code: '',
      description: '',
      color: '#fd7e14',
      riskLevel: 'Medium',
      displayOrder: 0,
      isActive: true,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate form before submission
    if (!validateForm()) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);

      const url = editingCategory 
        ? `/api/configuration/hazard-categories/${editingCategory.id}`
        : '/api/configuration/hazard-categories';
      
      const method = editingCategory ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: getAuthHeaders(),
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Failed to ${editingCategory ? 'update' : 'create'} hazard category`);
      }

      setSuccess(`Hazard category ${editingCategory ? 'updated' : 'created'} successfully!`);
      await loadCategories();
      handleCloseModal();
    } catch (error) {
      console.error('Error saving hazard category:', error);
      setError(error instanceof Error ? error.message : 'Failed to save hazard category');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this hazard category?')) {
      return;
    }

    try {
      setError(null);
      const response = await fetch(`/api/configuration/hazard-categories/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to delete hazard category');
      }

      setSuccess('Hazard category deleted successfully!');
      await loadCategories();
    } catch (error) {
      console.error('Error deleting hazard category:', error);
      setError(error instanceof Error ? error.message : 'Failed to delete hazard category');
    }
  };


  const handleInputChange = (field: keyof HazardCategoryCreateRequest, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    
    // Clear validation error for this field
    if (validationErrors[field]) {
      setValidationErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
    
    // Validate color field
    if (field === 'color') {
      validateColorField(value);
    }
  };

  const validateColorField = (color: string) => {
    const hexColorRegex = /^#[0-9A-Fa-f]{6}$/;
    
    if (!color) {
      setValidationErrors(prev => ({ ...prev, color: 'Color is required' }));
      return false;
    }
    
    if (!hexColorRegex.test(color)) {
      setValidationErrors(prev => ({ ...prev, color: 'Color must be a valid hex code (e.g., #FF5733)' }));
      return false;
    }
    
    // Clear error if valid
    setValidationErrors(prev => {
      const newErrors = { ...prev };
      delete newErrors.color;
      return newErrors;
    });
    return true;
  };

  const validateForm = () => {
    const errors: Record<string, string> = {};
    
    if (!formData.name.trim()) {
      errors.name = 'Category name is required';
    }
    
    if (!formData.code.trim()) {
      errors.code = 'Code is required';
    }
    
    if (!validateColorField(formData.color || '')) {
      // Error already set by validateColorField
    }
    
    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const getRiskLevelColor = (riskLevel: string) => {
    switch (riskLevel) {
      case 'Critical': return 'danger';
      case 'High': return 'warning';
      case 'Medium': return 'info';
      case 'Low': return 'success';
      default: return 'secondary';
    }
  };

  const filteredCategories = categories.filter(category =>
    category.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    category.code.toLowerCase().includes(searchTerm.toLowerCase())
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
              placeholder="Search hazard categories..."
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
            Add Hazard Category
          </CButton>
        </CCol>
      </CRow>


      {/* Categories Table */}
      <CCard>
        <CCardHeader>
          <FontAwesomeIcon icon={faTag} className="me-2" />
          <strong>Hazard Categories ({filteredCategories.length})</strong>
        </CCardHeader>
        <CCardBody>
          {isLoading ? (
            <div className="text-center py-4">
              <CSpinner color="primary" />
              <div className="mt-2">Loading hazard categories...</div>
            </div>
          ) : (
            <CTable hover responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Code</CTableHeaderCell>
                  <CTableHeaderCell>Name</CTableHeaderCell>
                  <CTableHeaderCell>Description</CTableHeaderCell>
                  <CTableHeaderCell>Risk Level</CTableHeaderCell>
                  <CTableHeaderCell>Status</CTableHeaderCell>
                  <CTableHeaderCell>Actions</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {filteredCategories.length === 0 ? (
                  <CTableRow>
                    <CTableDataCell colSpan={6} className="text-center text-muted py-4">
                      No hazard categories found. {searchTerm && 'Try adjusting your search or '}
                      <CButton
                        color="link"
                        className="p-0"
                        onClick={() => handleOpenModal()}
                      >
                        add a new category
                      </CButton>
                    </CTableDataCell>
                  </CTableRow>
                ) : (
                  filteredCategories.map((category) => (
                    <CTableRow key={category.id}>
                      <CTableDataCell>
                        <span
                          className="badge"
                          style={{ backgroundColor: category.color, color: 'white' }}
                        >
                          {category.code}
                        </span>
                      </CTableDataCell>
                      <CTableDataCell>
                        <strong>{category.name}</strong>
                      </CTableDataCell>
                      <CTableDataCell>
                        {category.description || '-'}
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={getRiskLevelColor(category.riskLevel)}>
                          {category.riskLevel}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={category.isActive ? 'success' : 'secondary'}>
                          {category.isActive ? 'Active' : 'Inactive'}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex gap-1">
                          <CButton
                            color="info"
                            variant="outline"
                            size="sm"
                            onClick={() => handleOpenModal(category)}
                            title="Edit category"
                          >
                            <FontAwesomeIcon icon={faEdit} />
                          </CButton>
                          <CButton
                            color="danger"
                            variant="outline"
                            size="sm"
                            onClick={() => handleDelete(category.id)}
                            title="Delete category"
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
            <FontAwesomeIcon icon={editingCategory ? faEdit : faPlus} className="me-2" />
            {editingCategory ? 'Edit Hazard Category' : 'Add New Hazard Category'}
          </CModalTitle>
        </CModalHeader>
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="name">Category Name *</CFormLabel>
                <CFormInput
                  id="name"
                  value={formData.name}
                  onChange={(e) => handleInputChange('name', e.target.value)}
                  placeholder="Enter category name"
                  required
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="code">Code *</CFormLabel>
                <CFormInput
                  id="code"
                  value={formData.code}
                  onChange={(e) => handleInputChange('code', e.target.value.toUpperCase())}
                  placeholder="Enter code (e.g., CHEM)"
                  maxLength={10}
                  required
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="riskLevel">Risk Level *</CFormLabel>
                <CFormSelect
                  id="riskLevel"
                  value={formData.riskLevel}
                  onChange={(e) => handleInputChange('riskLevel', e.target.value)}
                  required
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </CFormSelect>
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="color">Color *</CFormLabel>
                <CInputGroup>
                  <CFormInput
                    id="color"
                    type="color"
                    value={formData.color}
                    onChange={(e) => handleInputChange('color', e.target.value)}
                    style={{ maxWidth: '60px' }}
                  />
                  <CFormInput
                    value={formData.color}
                    onChange={(e) => handleInputChange('color', e.target.value)}
                    placeholder="#fd7e14"
                    pattern="^#[0-9A-Fa-f]{6}$"
                    maxLength={7}
                    invalid={!!validationErrors.color}
                  />
                </CInputGroup>
                {validationErrors.color && (
                  <div className="invalid-feedback d-block">
                    {validationErrors.color}
                  </div>
                )}
                <CFormText className="text-muted">
                  Enter a valid hex color code (e.g., #FF5733)
                </CFormText>
              </CCol>
            </CRow>

            <div className="mb-3">
              <CFormLabel htmlFor="description">Description</CFormLabel>
              <CFormTextarea
                id="description"
                rows={3}
                value={formData.description}
                onChange={(e) => handleInputChange('description', e.target.value)}
                placeholder="Enter category description"
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
              <CCol md={6} className="d-flex align-items-end">
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
              {editingCategory ? 'Update' : 'Create'} Category
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </div>
  );
};

export default HazardCategoryManagement;