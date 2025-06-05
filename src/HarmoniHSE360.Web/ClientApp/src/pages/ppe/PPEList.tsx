import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CButton,
  CFormSelect,
  CInputGroup,
  CFormInput,
  CSpinner,
  CAlert,
  CPagination,
  CPaginationItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CDropdownDivider,
  CBadge,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import {
  useGetPPEItemsQuery,
  useDeletePPEItemMutation,
  useGetPPECategoriesQuery,
} from '../../features/ppe/ppeApi';
import {
  getPPEStatusBadge,
  getPPEConditionBadge,
} from '../../utils/ppeUtils';

const PPEList: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [conditionFilter, setConditionFilter] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [locationFilter, setLocationFilter] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Fetch PPE items
  const { data, error, isLoading, refetch } = useGetPPEItemsQuery(
    {
      pageNumber: currentPage,
      pageSize: itemsPerPage,
      searchTerm: searchTerm || undefined,
      categoryId: categoryFilter ? parseInt(categoryFilter) : undefined,
      status: statusFilter || undefined,
      condition: conditionFilter || undefined,
      location: locationFilter || undefined,
      sortBy: 'itemCode',
      sortDirection: 'asc',
    },
    {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
    }
  );

  // Fetch categories for filter dropdown
  const { data: categories } = useGetPPECategoriesQuery();

  const [deletePPEItem, { isLoading: isDeleting }] = useDeletePPEItemMutation();

  // Extract items from response
  const items = data?.items || [];
  const totalCount = data?.totalCount || 0;
  const totalPages = data?.totalPages || 0;

  // Handle success message from navigation state
  useEffect(() => {
    if (location.state?.message) {
      setSuccessMessage(location.state.message);
      // Clear the navigation state
      window.history.replaceState({}, document.title);
      // Auto-hide message after 5 seconds
      const timer = setTimeout(() => setSuccessMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [location.state]);

  const getWarningBadges = (item: any) => {
    const badges = [];
    
    if (item.isExpired) {
      badges.push(
        <CBadge key="expired" color="danger" className="me-1">
          Expired
        </CBadge>
      );
    } else if (item.isExpiringSoon) {
      badges.push(
        <CBadge key="expiring" color="warning" className="me-1">
          Expiring Soon
        </CBadge>
      );
    }
    
    if (item.isMaintenanceDue) {
      badges.push(
        <CBadge key="maintenance" color="warning" className="me-1">
          Maintenance Due
        </CBadge>
      );
    }
    
    if (item.isInspectionDue) {
      badges.push(
        <CBadge key="inspection" color="info" className="me-1">
          Inspection Due
        </CBadge>
      );
    }
    
    return badges;
  };

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading PPE items...</span>
      </div>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4
                className="mb-0"
                style={{
                  color: 'var(--harmoni-charcoal)',
                  fontFamily: 'Poppins, sans-serif',
                }}
              >
                PPE Inventory
              </h4>
              <small className="text-muted">
                Manage and track all Personal Protective Equipment
              </small>
            </div>
            <div className="d-flex gap-2">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => {
                  console.log('Manual refresh triggered');
                  refetch();
                }}
                title="Refresh PPE list"
              >
                <FontAwesomeIcon icon={ACTION_ICONS.refresh} className="me-2" />
                Refresh
              </CButton>
              <CButton
                color="primary"
                onClick={() => navigate('/ppe/create')}
                className="d-flex align-items-center"
              >
                <FontAwesomeIcon
                  icon={ACTION_ICONS.create}
                  size="sm"
                  className="me-2"
                />
                Add PPE Item
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            {successMessage && (
              <CAlert
                color="success"
                dismissible
                onClose={() => setSuccessMessage(null)}
              >
                {successMessage}
              </CAlert>
            )}

            {error && (
              <CAlert color="danger" dismissible onClose={() => refetch()}>
                {typeof error === 'string'
                  ? error
                  : 'Failed to load PPE items. Please try again.'}
              </CAlert>
            )}

            {/* Filters and Search */}
            <CRow className="mb-4">
              <CCol md={3}>
                <CInputGroup>
                  <CFormInput
                    placeholder="Search PPE items..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                  <CButton type="button" color="primary" variant="outline">
                    <FontAwesomeIcon icon={ACTION_ICONS.search} />
                  </CButton>
                </CInputGroup>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={categoryFilter}
                  onChange={(e) => setCategoryFilter(e.target.value)}
                >
                  <option value="">All Categories</option>
                  {categories?.map((category) => (
                    <option key={category.id} value={category.id}>
                      {category.name}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
                >
                  <option value="">All Statuses</option>
                  <option value="Available">Available</option>
                  <option value="Assigned">Assigned</option>
                  <option value="InMaintenance">In Maintenance</option>
                  <option value="InInspection">In Inspection</option>
                  <option value="OutOfService">Out of Service</option>
                  <option value="Lost">Lost</option>
                  <option value="Retired">Retired</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={conditionFilter}
                  onChange={(e) => setConditionFilter(e.target.value)}
                >
                  <option value="">All Conditions</option>
                  <option value="New">New</option>
                  <option value="Excellent">Excellent</option>
                  <option value="Good">Good</option>
                  <option value="Fair">Fair</option>
                  <option value="Poor">Poor</option>
                  <option value="Damaged">Damaged</option>
                  <option value="Expired">Expired</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormInput
                  placeholder="Location filter..."
                  value={locationFilter}
                  onChange={(e) => setLocationFilter(e.target.value)}
                />
              </CCol>
              <CCol md={1}>
                <CButton
                  color="secondary"
                  variant="outline"
                  className="w-100"
                  onClick={() => {
                    setSearchTerm('');
                    setStatusFilter('');
                    setConditionFilter('');
                    setCategoryFilter('');
                    setLocationFilter('');
                    setCurrentPage(1);
                  }}
                  title="Clear all filters"
                >
                  <FontAwesomeIcon icon={ACTION_ICONS.cancel} size="sm" />
                </CButton>
              </CCol>
            </CRow>

            {/* PPE Items Table */}
            {items.length === 0 ? (
              <div className="text-center py-5">
                <FontAwesomeIcon
                  icon={CONTEXT_ICONS.incident}
                  className="text-muted mb-3"
                  style={{ fontSize: '3rem' }}
                />
                <h5 className="text-muted">No PPE items found</h5>
                <p className="text-muted">
                  {searchTerm || statusFilter || conditionFilter || categoryFilter || locationFilter
                    ? 'Try adjusting your filters or search criteria.'
                    : 'No PPE items have been added yet.'}
                </p>
                <CButton
                  color="primary"
                  onClick={() => navigate('/ppe/create')}
                  className="mt-3"
                >
                  <FontAwesomeIcon
                    icon={ACTION_ICONS.create}
                    size="sm"
                    className="me-2"
                  />
                  Add First PPE Item
                </CButton>
              </div>
            ) : (
              <>
                <CTable responsive hover>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell scope="col">Item Code</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Name</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Category</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Status</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Condition</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Location</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Assigned To</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Warnings</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {items.map((item) => (
                      <CTableRow key={item.id}>
                        <CTableDataCell>
                          <div>
                            <strong>{item.itemCode}</strong>
                            <br />
                            <small className="text-muted">
                              {item.manufacturer} {item.model}
                            </small>
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <div>
                            <strong>{item.name}</strong>
                            <br />
                            <small className="text-muted">Size: {item.size}</small>
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>{item.categoryName}</CTableDataCell>
                        <CTableDataCell>
                          {getPPEStatusBadge(item.status)}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getPPEConditionBadge(item.condition)}
                        </CTableDataCell>
                        <CTableDataCell>{item.location}</CTableDataCell>
                        <CTableDataCell>
                          {item.assignedToName || <span className="text-muted">Unassigned</span>}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getWarningBadges(item)}
                        </CTableDataCell>
                        <CTableDataCell>
                          <CDropdown>
                            <CDropdownToggle
                              color="light"
                              size="sm"
                              caret={false}
                            >
                              <FontAwesomeIcon icon={ACTION_ICONS.menu} />
                            </CDropdownToggle>
                            <CDropdownMenu>
                              <CDropdownItem
                                onClick={() => navigate(`/ppe/${item.id}`)}
                              >
                                <FontAwesomeIcon
                                  icon={ACTION_ICONS.view}
                                  size="sm"
                                  className="me-2"
                                />
                                View Details
                              </CDropdownItem>
                              <CDropdownItem
                                onClick={() => navigate(`/ppe/${item.id}/edit`)}
                              >
                                <FontAwesomeIcon
                                  icon={ACTION_ICONS.edit}
                                  size="sm"
                                  className="me-2"
                                />
                                Edit
                              </CDropdownItem>
                              {item.status === 'Available' && (
                                <CDropdownItem
                                  onClick={() => navigate(`/ppe/${item.id}/assign`)}
                                >
                                  <FontAwesomeIcon
                                    icon={ACTION_ICONS.add}
                                    size="sm"
                                    className="me-2"
                                  />
                                  Assign
                                </CDropdownItem>
                              )}
                              <CDropdownDivider />
                              <CDropdownItem
                                className="text-danger"
                                onClick={async () => {
                                  if (
                                    window.confirm(
                                      `Are you sure you want to delete "${item.itemCode} - ${item.name}"? This action cannot be undone.`
                                    )
                                  ) {
                                    try {
                                      await deletePPEItem(item.id).unwrap();
                                    } catch (error) {
                                      console.error('Failed to delete PPE item:', error);
                                      alert('Failed to delete PPE item. Please try again.');
                                    }
                                  }
                                }}
                                disabled={isDeleting}
                              >
                                <FontAwesomeIcon
                                  icon={ACTION_ICONS.delete}
                                  size="sm"
                                  className="me-2"
                                />
                                Delete
                              </CDropdownItem>
                            </CDropdownMenu>
                          </CDropdown>
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="d-flex justify-content-between align-items-center mt-4">
                    <div className="text-muted">
                      Showing {(currentPage - 1) * itemsPerPage + 1} to{' '}
                      {Math.min(currentPage * itemsPerPage, totalCount)} of{' '}
                      {totalCount} items
                    </div>
                    <CPagination aria-label="PPE Items pagination">
                      <CPaginationItem
                        disabled={currentPage === 1}
                        onClick={() => setCurrentPage(currentPage - 1)}
                      >
                        Previous
                      </CPaginationItem>
                      {[...Array(totalPages)].map((_, index) => (
                        <CPaginationItem
                          key={index + 1}
                          active={currentPage === index + 1}
                          onClick={() => setCurrentPage(index + 1)}
                        >
                          {index + 1}
                        </CPaginationItem>
                      ))}
                      <CPaginationItem
                        disabled={currentPage === totalPages}
                        onClick={() => setCurrentPage(currentPage + 1)}
                      >
                        Next
                      </CPaginationItem>
                    </CPagination>
                  </div>
                )}
              </>
            )}
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default PPEList;