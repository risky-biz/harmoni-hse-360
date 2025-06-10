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
  faBuilding,
  faSave,
  faTimes
} from '@fortawesome/free-solid-svg-icons';

// Define interfaces for Department
interface Department {
  id: number;
  name: string;
  code: string;
  description?: string;
  headOfDepartment?: string;
  contact?: string;
  location?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

interface DepartmentCreateRequest {
  name: string;
  code: string;
  description?: string;
  headOfDepartment?: string;
  contact?: string;
  location?: string;
  displayOrder: number;
  isActive: boolean;
}

const DepartmentManagement: React.FC = () => {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingDepartment, setEditingDepartment] = useState<Department | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Form state
  const [formData, setFormData] = useState<DepartmentCreateRequest>({
    name: '',
    code: '',
    description: '',
    headOfDepartment: '',
    contact: '',
    location: '',
    displayOrder: 0,
    isActive: true,
  });

  // Load departments on component mount
  useEffect(() => {
    loadDepartments();
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

  const loadDepartments = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const response = await fetch('/api/configuration/departments', {
        headers: getAuthHeaders(),
      });
      if (!response.ok) {
        throw new Error(`Failed to load departments: ${response.statusText}`);
      }
      
      const data = await response.json();
      setDepartments(data || []);
    } catch (error) {
      console.error('Error loading departments:', error);
      setError(error instanceof Error ? error.message : 'Failed to load departments');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenModal = (department?: Department) => {
    if (department) {
      setEditingDepartment(department);
      setFormData({
        name: department.name,
        code: department.code,
        description: department.description || '',
        headOfDepartment: department.headOfDepartment || '',
        contact: department.contact || '',
        location: department.location || '',
        displayOrder: department.displayOrder,
        isActive: department.isActive,
      });
    } else {
      setEditingDepartment(null);
      setFormData({
        name: '',
        code: '',
        description: '',
        headOfDepartment: '',
        contact: '',
        location: '',
        displayOrder: departments.length,
        isActive: true,
      });
    }
    setShowModal(true);
    setError(null);
    setSuccess(null);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingDepartment(null);
    setError(null);
    setSuccess(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setIsSubmitting(true);
      setError(null);

      const url = editingDepartment 
        ? `/api/configuration/departments/${editingDepartment.id}`
        : '/api/configuration/departments';
      
      const method = editingDepartment ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: getAuthHeaders(),
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Failed to ${editingDepartment ? 'update' : 'create'} department`);
      }

      setSuccess(`Department ${editingDepartment ? 'updated' : 'created'} successfully`);
      handleCloseModal();
      await loadDepartments();
    } catch (error) {
      console.error('Error saving department:', error);
      setError(error instanceof Error ? error.message : 'Failed to save department');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (department: Department) => {
    if (!window.confirm(`Are you sure you want to delete the department "${department.name}"?`)) {
      return;
    }

    try {
      setError(null);
      
      const response = await fetch(`/api/configuration/departments/${department.id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to delete department');
      }

      setSuccess('Department deleted successfully');
      await loadDepartments();
    } catch (error) {
      console.error('Error deleting department:', error);
      setError(error instanceof Error ? error.message : 'Failed to delete department');
    }
  };

  const filteredDepartments = departments.filter((department) =>
    department.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    department.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (department.headOfDepartment && department.headOfDepartment.toLowerCase().includes(searchTerm.toLowerCase()))
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
                placeholder="Search departments..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </CInputGroup>
          </CInputGroup>
        </CCol>
        <CCol md={6} className="text-end">
          <CButton color="primary" onClick={() => handleOpenModal()}>
            <FontAwesomeIcon icon={faPlus} className="me-2" />
            Add Department
          </CButton>
        </CCol>
      </CRow>

      <CCard>
        <CCardHeader>
          <FontAwesomeIcon icon={faBuilding} className="me-2" />
          Departments ({filteredDepartments.length})
        </CCardHeader>
        <CCardBody>
          {isLoading ? (
            <div className="text-center py-4">
              <CSpinner color="primary" />
              <div className="mt-2">Loading departments...</div>
            </div>
          ) : (
            <CTable hover responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Name</CTableHeaderCell>
                  <CTableHeaderCell>Code</CTableHeaderCell>
                  <CTableHeaderCell>Head of Department</CTableHeaderCell>
                  <CTableHeaderCell>Location</CTableHeaderCell>
                  <CTableHeaderCell>Status</CTableHeaderCell>
                  <CTableHeaderCell>Actions</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {filteredDepartments.length === 0 ? (
                  <CTableRow>
                    <CTableDataCell colSpan={6} className="text-center py-4">
                      <div className="text-muted">
                        <FontAwesomeIcon icon={faBuilding} size="2x" className="mb-2 opacity-50" />
                        <p className="mb-0">No departments found</p>
                        {searchTerm && <small>Try adjusting your search criteria</small>}
                      </div>
                    </CTableDataCell>
                  </CTableRow>
                ) : (
                  filteredDepartments.map((department) => (
                    <CTableRow key={department.id}>
                      <CTableDataCell>
                        <div className="fw-semibold">{department.name}</div>
                        {department.description && (
                          <small className="text-muted">{department.description}</small>
                        )}
                      </CTableDataCell>
                      <CTableDataCell>
                        <code className="bg-light px-2 py-1 rounded">{department.code}</code>
                      </CTableDataCell>
                      <CTableDataCell>
                        {department.headOfDepartment || <span className="text-muted">—</span>}
                      </CTableDataCell>
                      <CTableDataCell>
                        {department.location || <span className="text-muted">—</span>}
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={department.isActive ? 'success' : 'secondary'}>
                          {department.isActive ? 'Active' : 'Inactive'}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex gap-2">
                          <CButton
                            color="primary"
                            variant="outline"
                            size="sm"
                            onClick={() => handleOpenModal(department)}
                          >
                            <FontAwesomeIcon icon={faEdit} />
                          </CButton>
                          <CButton
                            color="danger"
                            variant="outline"
                            size="sm"
                            onClick={() => handleDelete(department)}
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
            {editingDepartment ? 'Edit Department' : 'Add New Department'}
          </CModalTitle>
        </CModalHeader>
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="name">Department Name *</CFormLabel>
                <CFormInput
                  type="text"
                  id="name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="code">Department Code *</CFormLabel>
                <CFormInput
                  type="text"
                  id="code"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  placeholder="e.g. IT, HR, SAFETY"
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
                  placeholder="Brief description of the department"
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="headOfDepartment">Head of Department</CFormLabel>
                <CFormInput
                  type="text"
                  id="headOfDepartment"
                  value={formData.headOfDepartment}
                  onChange={(e) => setFormData({ ...formData, headOfDepartment: e.target.value })}
                  placeholder="Name of department head"
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="contact">Contact Information</CFormLabel>
                <CFormInput
                  type="text"
                  id="contact"
                  value={formData.contact}
                  onChange={(e) => setFormData({ ...formData, contact: e.target.value })}
                  placeholder="Email or phone number"
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="location">Location</CFormLabel>
                <CFormInput
                  type="text"
                  id="location"
                  value={formData.location}
                  onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                  placeholder="Physical location or building"
                />
              </CCol>
              <CCol md={3}>
                <CFormLabel htmlFor="displayOrder">Display Order</CFormLabel>
                <CFormInput
                  type="number"
                  id="displayOrder"
                  value={formData.displayOrder}
                  onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 0 })}
                  min="0"
                />
              </CCol>
              <CCol md={3}>
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
              {editingDepartment ? 'Update' : 'Create'} Department
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </>
  );
};

export default DepartmentManagement;