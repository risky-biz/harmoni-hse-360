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
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import { useGetHazardsQuery } from '../../features/hazards/hazardApi';
import { useGetHazardCategoriesQuery } from '../../api/hazardConfigurationApi';
import {
  getSeverityBadge,
  getStatusBadge,
  formatDate,
} from '../../utils/hazardUtils';
import { GetHazardsParams } from '../../types/hazard';

const HazardList: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Use RTK Query to fetch hazards
  const { data, error, isLoading, refetch } = useGetHazardsQuery(
    {
      pageNumber: currentPage,
      pageSize: itemsPerPage,
      status: statusFilter || undefined,
      severity: severityFilter || undefined,
      category: categoryFilter || undefined,
      searchTerm: searchTerm || undefined,
    },
    {
      // Ensure we refetch when the component mounts
      refetchOnMountOrArgChange: true,
      // Refetch when the window regains focus
      refetchOnFocus: true,
    }
  );

  // Fetch hazard categories for dynamic filtering
  const { data: categoriesData, isLoading: categoriesLoading } = useGetHazardCategoriesQuery();

  // Extract hazards from response
  const hazards = data?.hazards || [];
  const totalCount = data?.totalCount || 0;
  const totalPages = data?.totalPages || 0;

  // Extract categories for filter dropdown
  const categories = categoriesData || [];

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

  // Handle page changes
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Handle search with debouncing
  const handleSearchChange = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset to first page when searching
  };

  // Handle filter changes
  const handleFilterChange = (filterType: string, value: string) => {
    switch (filterType) {
      case 'status':
        setStatusFilter(value);
        break;
      case 'severity':
        setSeverityFilter(value);
        break;
      case 'category':
        setCategoryFilter(value);
        break;
    }
    setCurrentPage(1); // Reset to first page when filtering
  };

  // Clear all filters
  const clearFilters = () => {
    setSearchTerm('');
    setStatusFilter('');
    setSeverityFilter('');
    setCategoryFilter('');
    setCurrentPage(1);
  };

  if (isLoading || categoriesLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading hazards...</span>
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
                <FontAwesomeIcon icon={CONTEXT_ICONS.hazard} className="me-2" />
                Hazard Register
              </h4>
              <small className="text-muted">
                Manage and track all hazard reports
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
                title="Refresh hazard list"
              >
                <FontAwesomeIcon icon={ACTION_ICONS.refresh} className="me-2" />
                Refresh
              </CButton>
              <CButton
                color="warning"
                onClick={() => navigate('/hazards/create')}
                className="d-flex align-items-center"
              >
                <FontAwesomeIcon
                  icon={ACTION_ICONS.create}
                  size="sm"
                  className="me-2"
                />
                Report Hazard
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            {successMessage && (
              <CAlert
                color="success"
                dismissible
                onClose={() => setSuccessMessage(null)}
                className="mb-3"
              >
                {successMessage}
              </CAlert>
            )}

            {error && (
              <CAlert color="danger" className="mb-3">
                Failed to load hazards. Please try again.
              </CAlert>
            )}

            {/* Search and Filter Controls */}
            <CRow className="mb-3">
              <CCol md={4}>
                <CInputGroup>
                  <CFormInput
                    placeholder="Search hazards by title, description, or location..."
                    value={searchTerm}
                    onChange={(e) => handleSearchChange(e.target.value)}
                  />
                  <CButton color="primary" variant="outline">
                    <FontAwesomeIcon icon={ACTION_ICONS.search} />
                  </CButton>
                </CInputGroup>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={statusFilter}
                  onChange={(e) => handleFilterChange('status', e.target.value)}
                >
                  <option value="">All Statuses</option>
                  <option value="Reported">Reported</option>
                  <option value="UnderAssessment">Under Assessment</option>
                  <option value="ActionRequired">Action Required</option>
                  <option value="Mitigating">Mitigating</option>
                  <option value="Monitoring">Monitoring</option>
                  <option value="Resolved">Resolved</option>
                  <option value="Closed">Closed</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={severityFilter}
                  onChange={(e) => handleFilterChange('severity', e.target.value)}
                >
                  <option value="">All Severities</option>
                  <option value="Catastrophic">Catastrophic</option>
                  <option value="Major">Major</option>
                  <option value="Moderate">Moderate</option>
                  <option value="Minor">Minor</option>
                  <option value="Negligible">Negligible</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={categoryFilter}
                  onChange={(e) => handleFilterChange('category', e.target.value)}
                >
                  <option value="">All Categories</option>
                  {categories.map((category) => (
                    <option key={category.id} value={category.name}>
                      {category.name}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CButton
                  color="secondary"
                  variant="outline"
                  onClick={clearFilters}
                  className="w-100"
                >
                  Clear Filters
                </CButton>
              </CCol>
            </CRow>

            {/* Hazards Table */}
            {hazards.length === 0 ? (
              <div className="text-center py-5">
                <FontAwesomeIcon
                  icon={CONTEXT_ICONS.hazard}
                  size="3x"
                  className="text-muted mb-3"
                />
                <h5 className="text-muted">No hazards found</h5>
                <p className="text-muted">
                  {searchTerm || statusFilter || severityFilter || categoryFilter
                    ? 'Try adjusting your search or filter criteria.'
                    : 'Get started by reporting your first hazard.'}
                </p>
                <CButton
                  color="warning"
                  onClick={() => navigate('/hazards/create')}
                >
                  <FontAwesomeIcon icon={ACTION_ICONS.create} className="me-2" />
                  Report First Hazard
                </CButton>
              </div>
            ) : (
              <>
                <CTable hover responsive className="border">
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell scope="col" style={{ width: '25%' }}>
                        Title
                      </CTableHeaderCell>
                      <CTableHeaderCell scope="col" style={{ width: '12%' }}>
                        Category
                      </CTableHeaderCell>
                      <CTableHeaderCell scope="col" style={{ width: '12%' }}>
                        Severity
                      </CTableHeaderCell>
                      <CTableHeaderCell scope="col" style={{ width: '12%' }}>
                        Status
                      </CTableHeaderCell>
                      <CTableHeaderCell scope="col" style={{ width: '15%' }}>
                        Location
                      </CTableHeaderCell>
                      <CTableHeaderCell scope="col" style={{ width: '12%' }}>
                        Reporter
                      </CTableHeaderCell>
                      <CTableHeaderCell scope="col" style={{ width: '12%' }}>
                        Date
                      </CTableHeaderCell>
                      <CTableHeaderCell scope="col" style={{ width: '100px' }}>
                        Actions
                      </CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {hazards.map((hazard) => (
                      <CTableRow key={hazard.id}>
                        <CTableDataCell>
                          <div
                            className="fw-semibold text-primary cursor-pointer"
                            onClick={() => navigate(`/hazards/${hazard.id}`)}
                            role="button"
                            title="View hazard details"
                          >
                            {hazard.title}
                          </div>
                          {hazard.description && (
                            <small className="text-muted d-block mt-1">
                              {hazard.description.length > 80
                                ? `${hazard.description.substring(0, 80)}...`
                                : hazard.description}
                            </small>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>
                          {hazard.category || 'N/A'}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getSeverityBadge(hazard.severity)}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getStatusBadge(hazard.status)}
                        </CTableDataCell>
                        <CTableDataCell>
                          <div className="text-truncate" style={{ maxWidth: '150px' }}>
                            {hazard.location || 'Not specified'}
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <div className="text-truncate" style={{ maxWidth: '120px' }}>
                            {hazard.reporter?.name || 'Unknown'}
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <small>{formatDate(hazard.identifiedDate)}</small>
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
                                onClick={() => navigate(`/hazards/${hazard.id}`)}
                              >
                                <FontAwesomeIcon
                                  icon={ACTION_ICONS.view}
                                  className="me-2"
                                />
                                View Details
                              </CDropdownItem>
                              <CDropdownItem
                                onClick={() => navigate(`/hazards/${hazard.id}/edit`)}
                              >
                                <FontAwesomeIcon
                                  icon={ACTION_ICONS.edit}
                                  className="me-2"
                                />
                                Edit Hazard
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
                  <div className="d-flex justify-content-between align-items-center mt-3">
                    <div className="text-muted">
                      Showing {((currentPage - 1) * itemsPerPage) + 1} to{' '}
                      {Math.min(currentPage * itemsPerPage, totalCount)} of{' '}
                      {totalCount} hazards
                    </div>
                    <CPagination className="mb-0">
                      <CPaginationItem
                        disabled={currentPage === 1}
                        onClick={() => handlePageChange(currentPage - 1)}
                      >
                        Previous
                      </CPaginationItem>
                      {Array.from({ length: totalPages }, (_, i) => i + 1)
                        .filter(
                          (page) =>
                            page === 1 ||
                            page === totalPages ||
                            Math.abs(page - currentPage) <= 2
                        )
                        .map((page, index, array) => (
                          <React.Fragment key={page}>
                            {index > 0 && array[index - 1] !== page - 1 && (
                              <CPaginationItem disabled>...</CPaginationItem>
                            )}
                            <CPaginationItem
                              active={page === currentPage}
                              onClick={() => handlePageChange(page)}
                            >
                              {page}
                            </CPaginationItem>
                          </React.Fragment>
                        ))}
                      <CPaginationItem
                        disabled={currentPage === totalPages}
                        onClick={() => handlePageChange(currentPage + 1)}
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

export default HazardList;