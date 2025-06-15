import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CButton,
  CSpinner,
  CAlert,
  CBadge,
  CInputGroup,
  CFormInput,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormLabel,
  CFormSelect,
  CFormFeedback,
} from '@coreui/react';
import { cilSearch, cilPlus, cilPencil, cilTrash, cilExclamationTriangle } from '@coreui/icons';
import CIcon from '@coreui/icons-react';
import { 
  useGetDisposalProvidersQuery,
  useSearchDisposalProvidersQuery,
  useCreateDisposalProviderMutation,
  useUpdateDisposalProviderMutation,
  useDeleteDisposalProviderMutation,
  useChangeProviderStatusMutation,
  DisposalProviderDto,
  ProviderStatus,
  CreateDisposalProviderCommand,
  UpdateDisposalProviderRequest 
} from '../../api/disposalProvidersApi';

interface CreateProviderFormData {
  name: string;
  licenseNumber: string;
  licenseExpiryDate: string;
}

const DisposalProviders: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<ProviderStatus | undefined>();
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedProvider, setSelectedProvider] = useState<DisposalProviderDto | null>(null);
  const [formData, setFormData] = useState<CreateProviderFormData>({
    name: '',
    licenseNumber: '',
    licenseExpiryDate: ''
  });
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  // API hooks
  const { 
    data: providers = [], 
    isLoading, 
    error: fetchError 
  } = useGetDisposalProvidersQuery();

  const [createProvider, { isLoading: isCreating }] = useCreateDisposalProviderMutation();
  const [updateProvider, { isLoading: isUpdating }] = useUpdateDisposalProviderMutation();
  const [deleteProvider, { isLoading: isDeleting }] = useDeleteDisposalProviderMutation();
  const [changeStatus] = useChangeProviderStatusMutation();

  // Helper functions
  const getStatusBadge = (status: ProviderStatus) => {
    const statusConfig = {
      [ProviderStatus.Active]: { color: 'success', text: 'Active' },
      [ProviderStatus.Suspended]: { color: 'warning', text: 'Suspended' },
      [ProviderStatus.Expired]: { color: 'danger', text: 'Expired' },
      [ProviderStatus.UnderReview]: { color: 'info', text: 'Under Review' },
      [ProviderStatus.Terminated]: { color: 'dark', text: 'Terminated' },
    };
    const config = statusConfig[status];
    return <CBadge color={config.color}>{config.text}</CBadge>;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const isExpired = (dateString: string) => {
    return new Date(dateString) < new Date();
  };

  const isExpiringSoon = (dateString: string) => {
    const expiryDate = new Date(dateString);
    const thirtyDaysFromNow = new Date();
    thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);
    return expiryDate <= thirtyDaysFromNow;
  };

  // Form validation
  const validateForm = (data: CreateProviderFormData): Record<string, string> => {
    const errors: Record<string, string> = {};
    
    if (!data.name.trim()) {
      errors.name = 'Provider name is required';
    }
    
    if (!data.licenseNumber.trim()) {
      errors.licenseNumber = 'License number is required';
    }
    
    if (!data.licenseExpiryDate) {
      errors.licenseExpiryDate = 'License expiry date is required';
    } else if (new Date(data.licenseExpiryDate) <= new Date()) {
      errors.licenseExpiryDate = 'License expiry date must be in the future';
    }
    
    return errors;
  };

  // Event handlers
  const handleCreate = async () => {
    const errors = validateForm(formData);
    setFormErrors(errors);
    
    if (Object.keys(errors).length > 0) return;

    try {
      await createProvider(formData as CreateDisposalProviderCommand).unwrap();
      setShowCreateModal(false);
      resetForm();
    } catch (error) {
      console.error('Failed to create provider:', error);
    }
  };

  const handleEdit = async () => {
    if (!selectedProvider) return;
    
    const errors = validateForm(formData);
    setFormErrors(errors);
    
    if (Object.keys(errors).length > 0) return;

    try {
      await updateProvider({
        id: selectedProvider.id,
        provider: formData as UpdateDisposalProviderRequest
      }).unwrap();
      setShowEditModal(false);
      resetForm();
    } catch (error) {
      console.error('Failed to update provider:', error);
    }
  };

  const handleDelete = async () => {
    if (!selectedProvider) return;

    try {
      await deleteProvider(selectedProvider.id).unwrap();
      setShowDeleteModal(false);
      setSelectedProvider(null);
    } catch (error) {
      console.error('Failed to delete provider:', error);
    }
  };

  const handleStatusChange = async (provider: DisposalProviderDto, newStatus: ProviderStatus) => {
    try {
      await changeStatus({ id: provider.id, status: newStatus }).unwrap();
    } catch (error) {
      console.error('Failed to change provider status:', error);
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      licenseNumber: '',
      licenseExpiryDate: ''
    });
    setFormErrors({});
    setSelectedProvider(null);
  };

  const openEditModal = (provider: DisposalProviderDto) => {
    setSelectedProvider(provider);
    setFormData({
      name: provider.name,
      licenseNumber: provider.licenseNumber,
      licenseExpiryDate: provider.licenseExpiryDate.split('T')[0] // Format for input[type="date"]
    });
    setShowEditModal(true);
  };

  const openDeleteModal = (provider: DisposalProviderDto) => {
    setSelectedProvider(provider);
    setShowDeleteModal(true);
  };

  // Filter providers based on search and status
  const filteredProviders = providers.filter(provider => {
    const matchesSearch = !searchTerm || 
      provider.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      provider.licenseNumber.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = !statusFilter || provider.status === statusFilter;
    
    return matchesSearch && matchesStatus;
  });

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center">
        <CSpinner />
      </div>
    );
  }

  return (
    <>
      <CRow>
        <CCol>
          <CCard className="mb-4">
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <strong>Disposal Providers</strong>
              <CButton 
                color="primary" 
                onClick={() => setShowCreateModal(true)}
              >
                <CIcon icon={cilPlus} className="me-2" />
                Add Provider
              </CButton>
            </CCardHeader>
            <CCardBody>
              {fetchError && (
                <CAlert color="danger">
                  Failed to load disposal providers. Please try again.
                </CAlert>
              )}

              {/* Search and Filter */}
              <CRow className="mb-3">
                <CCol md={6}>
                  <CInputGroup>
                    <CFormInput
                      placeholder="Search providers..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                    />
                    <CButton variant="outline" color="secondary">
                      <CIcon icon={cilSearch} />
                    </CButton>
                  </CInputGroup>
                </CCol>
                <CCol md={3}>
                  <CFormSelect
                    value={statusFilter || ''}
                    onChange={(e) => setStatusFilter(e.target.value ? Number(e.target.value) as ProviderStatus : undefined)}
                  >
                    <option value="">All Statuses</option>
                    <option value={ProviderStatus.Active}>Active</option>
                    <option value={ProviderStatus.Suspended}>Suspended</option>
                    <option value={ProviderStatus.Expired}>Expired</option>
                    <option value={ProviderStatus.UnderReview}>Under Review</option>
                    <option value={ProviderStatus.Terminated}>Terminated</option>
                  </CFormSelect>
                </CCol>
              </CRow>

              {/* Providers Table */}
              <CTable striped hover responsive>
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>Name</CTableHeaderCell>
                    <CTableHeaderCell>License Number</CTableHeaderCell>
                    <CTableHeaderCell>License Expiry</CTableHeaderCell>
                    <CTableHeaderCell>Status</CTableHeaderCell>
                    <CTableHeaderCell>Created</CTableHeaderCell>
                    <CTableHeaderCell>Actions</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {filteredProviders.map((provider) => (
                    <CTableRow key={provider.id}>
                      <CTableDataCell>
                        <strong>{provider.name}</strong>
                      </CTableDataCell>
                      <CTableDataCell>{provider.licenseNumber}</CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex align-items-center">
                          {formatDate(provider.licenseExpiryDate)}
                          {isExpired(provider.licenseExpiryDate) && (
                            <CBadge color="danger" className="ms-2">Expired</CBadge>
                          )}
                          {!isExpired(provider.licenseExpiryDate) && isExpiringSoon(provider.licenseExpiryDate) && (
                            <CBadge color="warning" className="ms-2">
                              <CIcon icon={cilExclamationTriangle} size="sm" className="me-1" />
                              Expiring Soon
                            </CBadge>
                          )}
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CDropdown>
                          <CDropdownToggle caret={false} size="sm">
                            {getStatusBadge(provider.status)}
                          </CDropdownToggle>
                          <CDropdownMenu>
                            {Object.entries(ProviderStatus)
                              .filter(([key, value]) => typeof value === 'number' && value !== provider.status)
                              .map(([key, value]) => (
                                <CDropdownItem
                                  key={key}
                                  onClick={() => handleStatusChange(provider, value as ProviderStatus)}
                                >
                                  {key}
                                </CDropdownItem>
                              ))}
                          </CDropdownMenu>
                        </CDropdown>
                      </CTableDataCell>
                      <CTableDataCell>
                        <small className="text-muted">
                          {formatDate(provider.createdAt)}
                        </small>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CButton
                          size="sm"
                          color="info"
                          variant="ghost"
                          className="me-2"
                          onClick={() => openEditModal(provider)}
                        >
                          <CIcon icon={cilPencil} />
                        </CButton>
                        <CButton
                          size="sm"
                          color="danger"
                          variant="ghost"
                          onClick={() => openDeleteModal(provider)}
                        >
                          <CIcon icon={cilTrash} />
                        </CButton>
                      </CTableDataCell>
                    </CTableRow>
                  ))}
                </CTableBody>
              </CTable>

              {filteredProviders.length === 0 && (
                <div className="text-center text-muted py-4">
                  No disposal providers found.
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Create Provider Modal */}
      <CModal visible={showCreateModal} onClose={() => setShowCreateModal(false)}>
        <CModalHeader>
          <CModalTitle>Add New Disposal Provider</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel htmlFor="name">Provider Name</CFormLabel>
              <CFormInput
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                invalid={!!formErrors.name}
              />
              {formErrors.name && <CFormFeedback invalid>{formErrors.name}</CFormFeedback>}
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="licenseNumber">License Number</CFormLabel>
              <CFormInput
                id="licenseNumber"
                value={formData.licenseNumber}
                onChange={(e) => setFormData({ ...formData, licenseNumber: e.target.value })}
                invalid={!!formErrors.licenseNumber}
              />
              {formErrors.licenseNumber && <CFormFeedback invalid>{formErrors.licenseNumber}</CFormFeedback>}
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="licenseExpiryDate">License Expiry Date</CFormLabel>
              <CFormInput
                type="date"
                id="licenseExpiryDate"
                value={formData.licenseExpiryDate}
                onChange={(e) => setFormData({ ...formData, licenseExpiryDate: e.target.value })}
                invalid={!!formErrors.licenseExpiryDate}
              />
              {formErrors.licenseExpiryDate && <CFormFeedback invalid>{formErrors.licenseExpiryDate}</CFormFeedback>}
            </div>
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowCreateModal(false)}>
            Cancel
          </CButton>
          <CButton color="primary" onClick={handleCreate} disabled={isCreating}>
            {isCreating ? <CSpinner size="sm" /> : 'Create Provider'}
          </CButton>
        </CModalFooter>
      </CModal>

      {/* Edit Provider Modal */}
      <CModal visible={showEditModal} onClose={() => setShowEditModal(false)}>
        <CModalHeader>
          <CModalTitle>Edit Disposal Provider</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel htmlFor="editName">Provider Name</CFormLabel>
              <CFormInput
                id="editName"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                invalid={!!formErrors.name}
              />
              {formErrors.name && <CFormFeedback invalid>{formErrors.name}</CFormFeedback>}
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="editLicenseNumber">License Number</CFormLabel>
              <CFormInput
                id="editLicenseNumber"
                value={formData.licenseNumber}
                onChange={(e) => setFormData({ ...formData, licenseNumber: e.target.value })}
                invalid={!!formErrors.licenseNumber}
              />
              {formErrors.licenseNumber && <CFormFeedback invalid>{formErrors.licenseNumber}</CFormFeedback>}
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="editLicenseExpiryDate">License Expiry Date</CFormLabel>
              <CFormInput
                type="date"
                id="editLicenseExpiryDate"
                value={formData.licenseExpiryDate}
                onChange={(e) => setFormData({ ...formData, licenseExpiryDate: e.target.value })}
                invalid={!!formErrors.licenseExpiryDate}
              />
              {formErrors.licenseExpiryDate && <CFormFeedback invalid>{formErrors.licenseExpiryDate}</CFormFeedback>}
            </div>
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowEditModal(false)}>
            Cancel
          </CButton>
          <CButton color="primary" onClick={handleEdit} disabled={isUpdating}>
            {isUpdating ? <CSpinner size="sm" /> : 'Update Provider'}
          </CButton>
        </CModalFooter>
      </CModal>

      {/* Delete Confirmation Modal */}
      <CModal visible={showDeleteModal} onClose={() => setShowDeleteModal(false)}>
        <CModalHeader>
          <CModalTitle>Confirm Delete</CModalTitle>
        </CModalHeader>
        <CModalBody>
          Are you sure you want to delete provider "{selectedProvider?.name}"?
          This action cannot be undone.
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowDeleteModal(false)}>
            Cancel
          </CButton>
          <CButton color="danger" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting ? <CSpinner size="sm" /> : 'Delete'}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default DisposalProviders;