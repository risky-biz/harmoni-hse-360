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
  CProgress,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faEdit,
  faTrash,
  faSearch,
  faEye,
  faEyeSlash,
  faMapMarkerAlt,
  faPhone,
  faUser,
  faWarehouse,
} from '@fortawesome/free-solid-svg-icons';

import {
  useGetPPEStorageLocationsQuery,
  useCreatePPEStorageLocationMutation,
  useUpdatePPEStorageLocationMutation,
  useDeletePPEStorageLocationMutation,
  PPEStorageLocation,
  CreatePPEStorageLocationRequest,
} from '../../../features/ppe/ppeManagementApi';

const PPEStorageLocationsManagement: React.FC = () => {
  const [showModal, setShowModal] = useState(false);
  const [editingLocation, setEditingLocation] = useState<PPEStorageLocation | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [showInactive, setShowInactive] = useState(false);
  const [formData, setFormData] = useState<CreatePPEStorageLocationRequest>({
    name: '',
    code: '',
    description: '',
    address: '',
    contactPerson: '',
    contactPhone: '',
    capacity: 100,
  });

  const { data: locations, isLoading, error } = useGetPPEStorageLocationsQuery({
    isActive: showInactive ? undefined : true,
    searchTerm: searchTerm || undefined,
  });

  const [createLocation] = useCreatePPEStorageLocationMutation();
  const [updateLocation] = useUpdatePPEStorageLocationMutation();
  const [deleteLocation] = useDeletePPEStorageLocationMutation();

  const resetForm = () => {
    setFormData({
      name: '',
      code: '',
      description: '',
      address: '',
      contactPerson: '',
      contactPhone: '',
      capacity: 100,
    });
    setEditingLocation(null);
  };

  const handleOpenModal = (location?: PPEStorageLocation) => {
    if (location) {
      setEditingLocation(location);
      setFormData({
        name: location.name,
        code: location.code,
        description: location.description || '',
        address: location.address || '',
        contactPerson: location.contactPerson || '',
        contactPhone: location.contactPhone || '',
        capacity: location.capacity,
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
      if (editingLocation) {
        await updateLocation({ id: editingLocation.id, data: formData }).unwrap();
      } else {
        await createLocation(formData).unwrap();
      }
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save location:', error);
    }
  };

  const handleDelete = async (location: PPEStorageLocation) => {
    if (window.confirm(`Are you sure you want to delete "${location.name}"?`)) {
      try {
        await deleteLocation(location.id).unwrap();
      } catch (error) {
        console.error('Failed to delete location:', error);
      }
    }
  };

  const filteredLocations = locations?.filter(location =>
    location.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    location.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
    location.address?.toLowerCase().includes(searchTerm.toLowerCase())
  ) || [];

  const getUtilizationColor = (percentage: number) => {
    if (percentage >= 90) return 'danger';
    if (percentage >= 75) return 'warning';
    if (percentage >= 50) return 'info';
    return 'success';
  };

  return (
    <div>
      <CRow className="mb-3">
        <CCol md={6}>
          <CInputGroup>
            <CInputGroupText>
              <FontAwesomeIcon icon={faSearch} />
            </CInputGroupText>
            <CFormInput
              placeholder="Search locations..."
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
            Add Location
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
          Failed to load PPE storage locations
        </CAlert>
      )}

      {locations && (
        <CTable hover responsive>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell>Location</CTableHeaderCell>
              <CTableHeaderCell>Code</CTableHeaderCell>
              <CTableHeaderCell>Address</CTableHeaderCell>
              <CTableHeaderCell>Contact</CTableHeaderCell>
              <CTableHeaderCell>Capacity</CTableHeaderCell>
              <CTableHeaderCell>Utilization</CTableHeaderCell>
              <CTableHeaderCell>Status</CTableHeaderCell>
              <CTableHeaderCell>Actions</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {filteredLocations.map((location) => (
              <CTableRow key={location.id}>
                <CTableDataCell>
                  <div>
                    <div className="fw-semibold">{location.name}</div>
                    {location.description && (
                      <small className="text-muted">{location.description}</small>
                    )}
                  </div>
                </CTableDataCell>
                <CTableDataCell>
                  <CBadge color="secondary">{location.code}</CBadge>
                </CTableDataCell>
                <CTableDataCell>
                  {location.address ? (
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1 text-muted" />
                      <small>{location.address}</small>
                    </div>
                  ) : (
                    '-'
                  )}
                </CTableDataCell>
                <CTableDataCell>
                  {location.contactPerson ? (
                    <div>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faUser} className="me-1 text-muted" />
                        <small>{location.contactPerson}</small>
                      </div>
                      {location.contactPhone && (
                        <div className="d-flex align-items-center">
                          <FontAwesomeIcon icon={faPhone} className="me-1 text-muted" />
                          <small>{location.contactPhone}</small>
                        </div>
                      )}
                    </div>
                  ) : (
                    '-'
                  )}
                </CTableDataCell>
                <CTableDataCell>
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon icon={faWarehouse} className="me-1 text-muted" />
                    <span>{location.currentStock}/{location.capacity}</span>
                  </div>
                </CTableDataCell>
                <CTableDataCell>
                  <div>
                    <CProgress
                      value={location.utilizationPercentage}
                      color={getUtilizationColor(location.utilizationPercentage)}
                      height={8}
                      className="mb-1"
                    />
                    <small className="text-muted">
                      {location.utilizationPercentage.toFixed(1)}%
                    </small>
                  </div>
                </CTableDataCell>
                <CTableDataCell>
                  <CBadge color={location.isActive ? 'success' : 'secondary'}>
                    {location.isActive ? 'Active' : 'Inactive'}
                  </CBadge>
                </CTableDataCell>
                <CTableDataCell>
                  <div className="d-flex gap-1">
                    <CButton
                      color="outline-primary"
                      size="sm"
                      onClick={() => handleOpenModal(location)}
                    >
                      <FontAwesomeIcon icon={faEdit} />
                    </CButton>
                    <CButton
                      color="outline-danger"
                      size="sm"
                      onClick={() => handleDelete(location)}
                    >
                      <FontAwesomeIcon icon={faTrash} />
                    </CButton>
                  </div>
                </CTableDataCell>
              </CTableRow>
            ))}
            {filteredLocations.length === 0 && !isLoading && (
              <CTableRow>
                <CTableDataCell colSpan={8} className="text-center text-muted">
                  No storage locations found
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
            {editingLocation ? 'Edit Storage Location' : 'Add New Storage Location'}
          </CModalTitle>
        </CModalHeader>
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            <CRow className="mb-3">
              <CCol md={6}>
                <label className="form-label">Location Name *</label>
                <CFormInput
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g., Main Safety Office, Warehouse A"
                />
              </CCol>
              <CCol md={6}>
                <label className="form-label">Code *</label>
                <CFormInput
                  required
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  placeholder="e.g., MSO, WH-A"
                />
              </CCol>
            </CRow>
            
            <div className="mb-3">
              <label className="form-label">Description</label>
              <CFormTextarea
                rows={2}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Optional description for this storage location"
              />
            </div>

            <div className="mb-3">
              <label className="form-label">Address</label>
              <CFormInput
                value={formData.address}
                onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                placeholder="Physical address or location details"
              />
            </div>

            <CRow className="mb-3">
              <CCol md={6}>
                <label className="form-label">Contact Person</label>
                <CFormInput
                  value={formData.contactPerson}
                  onChange={(e) => setFormData({ ...formData, contactPerson: e.target.value })}
                  placeholder="Responsible person's name"
                />
              </CCol>
              <CCol md={6}>
                <label className="form-label">Contact Phone</label>
                <CFormInput
                  value={formData.contactPhone}
                  onChange={(e) => setFormData({ ...formData, contactPhone: e.target.value })}
                  placeholder="Contact phone number"
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <label className="form-label">Storage Capacity *</label>
                <CFormInput
                  type="number"
                  min="1"
                  required
                  value={formData.capacity}
                  onChange={(e) => setFormData({ ...formData, capacity: parseInt(e.target.value) || 100 })}
                  placeholder="Maximum number of items"
                />
              </CCol>
            </CRow>
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" onClick={handleCloseModal}>
              Cancel
            </CButton>
            <CButton color="primary" type="submit">
              {editingLocation ? 'Update' : 'Create'} Location
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </div>
  );
};

export default PPEStorageLocationsManagement;