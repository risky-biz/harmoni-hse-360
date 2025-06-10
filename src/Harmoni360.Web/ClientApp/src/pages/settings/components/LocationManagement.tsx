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
  faMapMarkerAlt,
  faSave,
  faTimes
} from '@fortawesome/free-solid-svg-icons';

// Define interfaces for IncidentLocation
interface IncidentLocation {
  id: number;
  name: string;
  code: string;
  description?: string;
  building?: string;
  floor?: string;
  room?: string;
  coordinates?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

interface LocationCreateRequest {
  name: string;
  code: string;
  description?: string;
  building?: string;
  floor?: string;
  room?: string;
  coordinates?: string;
  displayOrder: number;
  isActive: boolean;
}

const LocationManagement: React.FC = () => {
  const [locations, setLocations] = useState<IncidentLocation[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingLocation, setEditingLocation] = useState<IncidentLocation | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Form state
  const [formData, setFormData] = useState<LocationCreateRequest>({
    name: '',
    code: '',
    description: '',
    building: '',
    floor: '',
    room: '',
    coordinates: '',
    displayOrder: 0,
    isActive: true,
  });

  // Predefined location options for British School Jakarta
  const predefinedLocations = [
    { name: 'Elementary Building', code: 'ELEM', building: 'Elementary', description: 'Primary education building' },
    { name: 'Secondary Building', code: 'SEC', building: 'Secondary', description: 'Secondary education building' },
    { name: 'Administration Block', code: 'ADMIN', building: 'Admin', description: 'Administrative offices' },
    { name: 'Science Laboratory', code: 'SCI_LAB', building: 'Secondary', floor: '2nd Floor', description: 'Science laboratory complex' },
    { name: 'Computer Lab', code: 'COMP_LAB', building: 'Secondary', floor: '1st Floor', description: 'Computer and IT laboratory' },
    { name: 'Library', code: 'LIB', building: 'Secondary', floor: '1st Floor', description: 'School library and resource center' },
    { name: 'Gymnasium', code: 'GYM', building: 'Sports', description: 'Indoor sports facility' },
    { name: 'Swimming Pool', code: 'POOL', building: 'Sports', description: 'Swimming pool facility' },
    { name: 'Cafeteria', code: 'CAFE', building: 'Elementary', floor: '1st Floor', description: 'Student dining area' },
    { name: 'Playground', code: 'PLAY', description: 'Outdoor playground area' },
    { name: 'Parking Lot', code: 'PARK', description: 'Vehicle parking area' },
    { name: 'Main Entrance', code: 'ENTRANCE', description: 'Main school entrance and reception' },
  ];

  // Load locations on component mount
  useEffect(() => {
    loadLocations();
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

  const loadLocations = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const response = await fetch('/api/configuration/incident-locations', {
        headers: getAuthHeaders(),
      });
      if (!response.ok) {
        throw new Error(`Failed to load locations: ${response.statusText}`);
      }
      
      const data = await response.json();
      setLocations(data || []);
    } catch (error) {
      console.error('Error loading locations:', error);
      setError(error instanceof Error ? error.message : 'Failed to load locations');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenModal = (location?: IncidentLocation) => {
    if (location) {
      setEditingLocation(location);
      setFormData({
        name: location.name,
        code: location.code,
        description: location.description || '',
        building: location.building || '',
        floor: location.floor || '',
        room: location.room || '',
        coordinates: location.coordinates || '',
        displayOrder: location.displayOrder,
        isActive: location.isActive,
      });
    } else {
      setEditingLocation(null);
      setFormData({
        name: '',
        code: '',
        description: '',
        building: '',
        floor: '',
        room: '',
        coordinates: '',
        displayOrder: locations.length,
        isActive: true,
      });
    }
    setShowModal(true);
    setError(null);
    setSuccess(null);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingLocation(null);
    setError(null);
    setSuccess(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setIsSubmitting(true);
      setError(null);

      const url = editingLocation 
        ? `/api/configuration/incident-locations/${editingLocation.id}`
        : '/api/configuration/incident-locations';
      
      const method = editingLocation ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: getAuthHeaders(),
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Failed to ${editingLocation ? 'update' : 'create'} location`);
      }

      setSuccess(`Location ${editingLocation ? 'updated' : 'created'} successfully`);
      handleCloseModal();
      await loadLocations();
    } catch (error) {
      console.error('Error saving location:', error);
      setError(error instanceof Error ? error.message : 'Failed to save location');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (location: IncidentLocation) => {
    if (!window.confirm(`Are you sure you want to delete the location "${location.name}"?`)) {
      return;
    }

    try {
      setError(null);
      
      const response = await fetch(`/api/configuration/incident-locations/${location.id}`, {
        method: 'DELETE',
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to delete location');
      }

      setSuccess('Location deleted successfully');
      await loadLocations();
    } catch (error) {
      console.error('Error deleting location:', error);
      setError(error instanceof Error ? error.message : 'Failed to delete location');
    }
  };

  const handleAddPredefined = (predefined: typeof predefinedLocations[0]) => {
    setFormData({
      name: predefined.name,
      code: predefined.code,
      description: predefined.description || '',
      building: predefined.building || '',
      floor: predefined.floor || '',
      room: '',
      coordinates: '',
      displayOrder: locations.length,
      isActive: true,
    });
  };

  const filteredLocations = locations.filter((location) =>
    location.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    location.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (location.building && location.building.toLowerCase().includes(searchTerm.toLowerCase())) ||
    (location.floor && location.floor.toLowerCase().includes(searchTerm.toLowerCase()))
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
                placeholder="Search locations..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </CInputGroup>
          </CInputGroup>
        </CCol>
        <CCol md={6} className="text-end">
          <CButton color="primary" onClick={() => handleOpenModal()}>
            <FontAwesomeIcon icon={faPlus} className="me-2" />
            Add Location
          </CButton>
        </CCol>
      </CRow>

      <CCard>
        <CCardHeader>
          <FontAwesomeIcon icon={faMapMarkerAlt} className="me-2" />
          Incident Locations ({filteredLocations.length})
        </CCardHeader>
        <CCardBody>
          {isLoading ? (
            <div className="text-center py-4">
              <CSpinner color="primary" />
              <div className="mt-2">Loading locations...</div>
            </div>
          ) : (
            <CTable hover responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Name</CTableHeaderCell>
                  <CTableHeaderCell>Code</CTableHeaderCell>
                  <CTableHeaderCell>Building</CTableHeaderCell>
                  <CTableHeaderCell>Floor/Room</CTableHeaderCell>
                  <CTableHeaderCell>Status</CTableHeaderCell>
                  <CTableHeaderCell>Actions</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {filteredLocations.length === 0 ? (
                  <CTableRow>
                    <CTableDataCell colSpan={6} className="text-center py-4">
                      <div className="text-muted">
                        <FontAwesomeIcon icon={faMapMarkerAlt} size="2x" className="mb-2 opacity-50" />
                        <p className="mb-0">No locations found</p>
                        {searchTerm && <small>Try adjusting your search criteria</small>}
                      </div>
                    </CTableDataCell>
                  </CTableRow>
                ) : (
                  filteredLocations.map((location) => (
                    <CTableRow key={location.id}>
                      <CTableDataCell>
                        <div className="fw-semibold">{location.name}</div>
                        {location.description && (
                          <small className="text-muted">{location.description}</small>
                        )}
                      </CTableDataCell>
                      <CTableDataCell>
                        <code className="bg-light px-2 py-1 rounded">{location.code}</code>
                      </CTableDataCell>
                      <CTableDataCell>
                        {location.building || <span className="text-muted">—</span>}
                      </CTableDataCell>
                      <CTableDataCell>
                        <div>
                          {location.floor && <div className="small">{location.floor}</div>}
                          {location.room && <div className="small text-muted">{location.room}</div>}
                          {!location.floor && !location.room && <span className="text-muted">—</span>}
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={location.isActive ? 'success' : 'secondary'}>
                          {location.isActive ? 'Active' : 'Inactive'}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex gap-2">
                          <CButton
                            color="primary"
                            variant="outline"
                            size="sm"
                            onClick={() => handleOpenModal(location)}
                          >
                            <FontAwesomeIcon icon={faEdit} />
                          </CButton>
                          <CButton
                            color="danger"
                            variant="outline"
                            size="sm"
                            onClick={() => handleDelete(location)}
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
            {editingLocation ? 'Edit Location' : 'Add New Location'}
          </CModalTitle>
        </CModalHeader>
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            {!editingLocation && (
              <CAlert color="info" className="mb-3">
                <strong>Quick Add:</strong> Click on a predefined location to auto-fill the form.
                <div className="mt-2">
                  <div className="d-flex flex-wrap gap-1">
                    {predefinedLocations.map((predefined, index) => (
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
                <CFormLabel htmlFor="name">Location Name *</CFormLabel>
                <CFormInput
                  type="text"
                  id="name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="code">Location Code *</CFormLabel>
                <CFormInput
                  type="text"
                  id="code"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  placeholder="e.g. ELEM, SCI_LAB, GYM"
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
                  placeholder="Brief description of this location"
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="building">Building</CFormLabel>
                <CFormInput
                  type="text"
                  id="building"
                  value={formData.building}
                  onChange={(e) => setFormData({ ...formData, building: e.target.value })}
                  placeholder="e.g. Elementary, Secondary, Admin"
                />
              </CCol>
              <CCol md={3}>
                <CFormLabel htmlFor="floor">Floor</CFormLabel>
                <CFormInput
                  type="text"
                  id="floor"
                  value={formData.floor}
                  onChange={(e) => setFormData({ ...formData, floor: e.target.value })}
                  placeholder="e.g. 1st Floor, Ground"
                />
              </CCol>
              <CCol md={3}>
                <CFormLabel htmlFor="room">Room</CFormLabel>
                <CFormInput
                  type="text"
                  id="room"
                  value={formData.room}
                  onChange={(e) => setFormData({ ...formData, room: e.target.value })}
                  placeholder="e.g. Room 101, Lab A"
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={8}>
                <CFormLabel htmlFor="coordinates">Coordinates (Optional)</CFormLabel>
                <CFormInput
                  type="text"
                  id="coordinates"
                  value={formData.coordinates}
                  onChange={(e) => setFormData({ ...formData, coordinates: e.target.value })}
                  placeholder="e.g. -6.1944, 106.8229 (latitude, longitude)"
                />
              </CCol>
              <CCol md={2}>
                <CFormLabel htmlFor="displayOrder">Display Order</CFormLabel>
                <CFormInput
                  type="number"
                  id="displayOrder"
                  value={formData.displayOrder}
                  onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 0 })}
                  min="0"
                />
              </CCol>
              <CCol md={2}>
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
              {editingLocation ? 'Update' : 'Create'} Location
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </>
  );
};

export default LocationManagement;