import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CSpinner,
  CAlert,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CFormInput,
  CFormSelect,
  CPagination,
  CPaginationItem,
  CInputGroup,
  CInputGroupText,
  CCallout,
  CCollapse,
  CButtonGroup,
  CBadge,
  CTooltip,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faSearch,
  faFilter,
  faPlus,
  faEye,
  faEdit,
  faRefresh,
  faChevronDown,
  faChevronUp,
  faFileAlt,
  faCalendarAlt,
  faExclamationTriangle,
  faSort,
  faSortUp,
  faSortDown,
  faDownload,
  faTimesCircle,
} from '@fortawesome/free-solid-svg-icons';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import { Icon } from '../../components/common/Icon';
import ApiUnavailableMessage from '../../components/common/ApiUnavailableMessage';
import { useGetMyHazardsQuery } from '../../features/hazards/hazardApi';
import { HazardDto } from '../../types/hazard';
import {
  getSeverityBadge,
  getStatusBadge,
  formatDate,
} from '../../utils/hazardUtils';

type SortField = 'title' | 'severity' | 'status' | 'identifiedDate' | 'createdAt';
type SortDirection = 'asc' | 'desc';

const MyHazards: React.FC = () => {
  const navigate = useNavigate();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [showAdvancedFilters, setShowAdvancedFilters] = useState(false);
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc');
  const [dateFromFilter, setDateFromFilter] = useState('');
  const [dateToFilter, setDateToFilter] = useState('');

  const {
    data: myHazardsResponse,
    isLoading,
    error,
    refetch,
  } = useGetMyHazardsQuery({
    pageNumber,
    pageSize,
    searchTerm: searchTerm || undefined,
    status: statusFilter || undefined,
    severity: severityFilter || undefined,
  });

  // Auto-search when filters change
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      setPageNumber(1);
      refetch();
    }, 300);
    
    return () => clearTimeout(timeoutId);
  }, [searchTerm, statusFilter, severityFilter, dateFromFilter, dateToFilter, refetch]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
    setPageNumber(1);
  };

  const handleClearFilters = () => {
    setSearchTerm('');
    setStatusFilter('');
    setSeverityFilter('');
    setDateFromFilter('');
    setDateToFilter('');
    setPageNumber(1);
  };

  const getSortIcon = (field: SortField) => {
    if (sortField !== field) return faSort;
    return sortDirection === 'asc' ? faSortUp : faSortDown;
  };

  // Client-side sorting and filtering for better UX
  const sortedHazards = React.useMemo(() => {
    const hazards = myHazardsResponse?.hazards || [];
    return [...hazards].sort((a, b) => {
      let aValue: any = a[sortField];
      let bValue: any = b[sortField];
      
      if (sortField === 'identifiedDate' || sortField === 'createdAt') {
        aValue = new Date(aValue).getTime();
        bValue = new Date(bValue).getTime();
      } else if (typeof aValue === 'string') {
        aValue = aValue.toLowerCase();
        bValue = bValue.toLowerCase();
      }
      
      if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
      if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }, [myHazardsResponse?.hazards, sortField, sortDirection]);

  const totalCount = myHazardsResponse?.totalCount || 0;
  const totalPages = Math.ceil(totalCount / pageSize);

  const activeFiltersCount = [
    searchTerm,
    statusFilter,
    severityFilter,
    dateFromFilter,
    dateToFilter
  ].filter(Boolean).length;

  if (isLoading && !myHazardsResponse) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading your hazard reports...</span>
      </div>
    );
  }

  if (error) {
    return (
      <ApiUnavailableMessage
        title="Failed to load your hazard reports"
        message="Unable to retrieve your hazard reports from the backend API."
        onRefresh={() => refetch()}
      />
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
                <FontAwesomeIcon
                  icon={faExclamationTriangle}
                  size="lg"
                  className="me-2 text-warning"
                />
                My Hazard Reports
              </h4>
              <div className="d-flex align-items-center gap-2">
                <small className="text-muted">
                  {totalCount} report{totalCount !== 1 ? 's' : ''} found
                </small>
                {activeFiltersCount > 0 && (
                  <CBadge color="info" shape="rounded-pill">
                    {activeFiltersCount} filter{activeFiltersCount !== 1 ? 's' : ''} active
                  </CBadge>
                )}
              </div>
            </div>
            <div className="d-flex gap-2">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => setShowAdvancedFilters(!showAdvancedFilters)}
                className="d-flex align-items-center"
              >
                <FontAwesomeIcon icon={faFilter} className="me-2" />
                Advanced Filters
                <FontAwesomeIcon 
                  icon={showAdvancedFilters ? faChevronUp : faChevronDown} 
                  className="ms-2" 
                  size="sm"
                />
              </CButton>
              <CButton
                color="primary"
                onClick={() => navigate('/hazards/create')}
                className="d-flex align-items-center"
              >
                <FontAwesomeIcon icon={faPlus} className="me-2" />
                Report New Hazard
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            {/* Basic Search and Filters */}
            <CRow className="mb-3">
              <CCol lg={5}>
                <CInputGroup>
                  <CInputGroupText>
                    <FontAwesomeIcon icon={faSearch} />
                  </CInputGroupText>
                  <CFormInput
                    type="text"
                    placeholder="Search by title, description, or location..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                  {searchTerm && (
                    <CButton
                      color="secondary"
                      variant="ghost"
                      onClick={() => setSearchTerm('')}
                      className="border-0"
                    >
                      <FontAwesomeIcon icon={faTimesCircle} />
                    </CButton>
                  )}
                </CInputGroup>
              </CCol>
              <CCol lg={2}>
                <CFormSelect
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
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
              <CCol lg={2}>
                <CFormSelect
                  value={severityFilter}
                  onChange={(e) => setSeverityFilter(e.target.value)}
                >
                  <option value="">All Severities</option>
                  <option value="Negligible">Negligible</option>
                  <option value="Minor">Minor</option>
                  <option value="Moderate">Moderate</option>
                  <option value="Major">Major</option>
                  <option value="Catastrophic">Catastrophic</option>
                </CFormSelect>
              </CCol>
              <CCol lg={3} className="d-flex gap-2">
                <CFormSelect
                  value={pageSize}
                  onChange={(e) => setPageSize(Number(e.target.value))}
                  style={{ maxWidth: '100px' }}
                >
                  <option value={10}>10</option>
                  <option value={25}>25</option>
                  <option value={50}>50</option>
                  <option value={100}>100</option>
                </CFormSelect>
                {activeFiltersCount > 0 && (
                  <CTooltip content="Clear all filters">
                    <CButton
                      color="secondary"
                      variant="outline"
                      onClick={handleClearFilters}
                    >
                      <FontAwesomeIcon icon={faTimesCircle} />
                    </CButton>
                  </CTooltip>
                )}
              </CCol>
            </CRow>

            {/* Advanced Filters */}
            <CCollapse visible={showAdvancedFilters}>
              <CCard className="mb-4 border-secondary">
                <CCardHeader className="bg-light">
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faFilter} className="me-2" />
                    Advanced Filters
                  </h6>
                </CCardHeader>
                <CCardBody>
                  <CRow>
                    <CCol md={3}>
                      <label className="form-label">Date From</label>
                      <CFormInput
                        type="date"
                        value={dateFromFilter}
                        onChange={(e) => setDateFromFilter(e.target.value)}
                      />
                    </CCol>
                    <CCol md={3}>
                      <label className="form-label">Date To</label>
                      <CFormInput
                        type="date"
                        value={dateToFilter}
                        onChange={(e) => setDateToFilter(e.target.value)}
                      />
                    </CCol>
                    <CCol md={3}>
                      <label className="form-label">Sort By</label>
                      <CFormSelect
                        value={sortField}
                        onChange={(e) => setSortField(e.target.value as SortField)}
                      >
                        <option value="createdAt">Date Created</option>
                        <option value="identifiedDate">Date Identified</option>
                        <option value="title">Title</option>
                        <option value="severity">Severity</option>
                        <option value="status">Status</option>
                      </CFormSelect>
                    </CCol>
                    <CCol md={3}>
                      <label className="form-label">Sort Order</label>
                      <CFormSelect
                        value={sortDirection}
                        onChange={(e) => setSortDirection(e.target.value as SortDirection)}
                      >
                        <option value="desc">Newest First</option>
                        <option value="asc">Oldest First</option>
                      </CFormSelect>
                    </CCol>
                  </CRow>
                </CCardBody>
              </CCard>
            </CCollapse>

            {sortedHazards.length === 0 ? (
              <CCallout color="info" className="text-center py-4">
                <FontAwesomeIcon
                  icon={faExclamationTriangle}
                  size="2xl"
                  className="mb-3 text-muted"
                />
                <h5>No hazards found</h5>
                <p className="text-muted mb-3">
                  {searchTerm || statusFilter || severityFilter || dateFromFilter || dateToFilter
                    ? 'No hazards match your current filters.'
                    : "You haven't reported any hazards yet."}
                </p>
                <div className="d-flex justify-content-center gap-2">
                  {(searchTerm || statusFilter || severityFilter || dateFromFilter || dateToFilter) && (
                    <CButton
                      color="secondary"
                      variant="outline"
                      onClick={handleClearFilters}
                    >
                      <FontAwesomeIcon icon={faTimesCircle} className="me-2" />
                      Clear Filters
                    </CButton>
                  )}
                  <CButton
                    color="primary"
                    onClick={() => navigate('/hazards/create')}
                  >
                    <FontAwesomeIcon icon={faPlus} className="me-2" />
                    Report Your First Hazard
                  </CButton>
                </div>
              </CCallout>
            ) : (
              <>
                {/* Hazards Table */}
                <div className="table-responsive">
                  <CTable hover className="border">
                    <CTableHead className="table-light">
                      <CTableRow>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('title')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Title
                            <FontAwesomeIcon 
                              icon={getSortIcon('title')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('severity')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Severity
                            <FontAwesomeIcon 
                              icon={getSortIcon('severity')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('status')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Status
                            <FontAwesomeIcon 
                              icon={getSortIcon('status')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('identifiedDate')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Date Identified
                            <FontAwesomeIcon 
                              icon={getSortIcon('identifiedDate')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell>Location</CTableHeaderCell>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('createdAt')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Created
                            <FontAwesomeIcon 
                              icon={getSortIcon('createdAt')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell>Actions</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {sortedHazards.map((hazard: HazardDto) => (
                        <CTableRow 
                          key={hazard.id}
                          style={{ cursor: 'pointer' }}
                          onClick={() => navigate(`/hazards/${hazard.id}`)}
                        >
                          <CTableDataCell>
                            <div>
                              <div className="fw-semibold text-primary">{hazard.title}</div>
                              {hazard.description && (
                                <div className="text-muted small mt-1">
                                  {hazard.description.length > 80
                                    ? `${hazard.description.substring(0, 80)}...`
                                    : hazard.description}
                                </div>
                              )}
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            {getSeverityBadge(hazard.severity)}
                          </CTableDataCell>
                          <CTableDataCell>
                            {getStatusBadge(hazard.status)}
                          </CTableDataCell>
                          <CTableDataCell>
                            <div className="d-flex align-items-center text-muted">
                              <FontAwesomeIcon icon={faCalendarAlt} className="me-2" size="sm" />
                              <small>{formatDate(hazard.identifiedDate)}</small>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div className="text-muted small d-flex align-items-center">
                              <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" size="sm" />
                              {hazard.location.length > 30 
                                ? `${hazard.location.substring(0, 30)}...`
                                : hazard.location
                              }
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div className="text-muted small">
                              {formatDate(hazard.createdAt)}
                            </div>
                          </CTableDataCell>
                          <CTableDataCell onClick={(e) => e.stopPropagation()}>
                            <CButtonGroup size="sm">
                              <CTooltip content="View Details">
                                <CButton
                                  color="info"
                                  variant="outline"
                                  size="sm"
                                  onClick={() => navigate(`/hazards/${hazard.id}`)}
                                >
                                  <FontAwesomeIcon icon={faEye} />
                                </CButton>
                              </CTooltip>
                              <CTooltip content="Edit Hazard">
                                <CButton
                                  color="primary"
                                  variant="outline"
                                  size="sm"
                                  onClick={() => navigate(`/hazards/${hazard.id}/edit`)}
                                >
                                  <FontAwesomeIcon icon={faEdit} />
                                </CButton>
                              </CTooltip>
                            </CButtonGroup>
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>
                </div>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="d-flex justify-content-center mt-4">
                    <CPagination>
                      <CPaginationItem
                        disabled={pageNumber === 1}
                        onClick={() => setPageNumber(pageNumber - 1)}
                      >
                        Previous
                      </CPaginationItem>

                      {Array.from(
                        { length: Math.min(5, totalPages) },
                        (_, i) => {
                          const page =
                            Math.max(
                              1,
                              Math.min(totalPages - 4, pageNumber - 2)
                            ) + i;
                          return (
                            <CPaginationItem
                              key={page}
                              active={page === pageNumber}
                              onClick={() => setPageNumber(page)}
                            >
                              {page}
                            </CPaginationItem>
                          );
                        }
                      )}

                      <CPaginationItem
                        disabled={pageNumber === totalPages}
                        onClick={() => setPageNumber(pageNumber + 1)}
                      >
                        Next
                      </CPaginationItem>
                    </CPagination>
                  </div>
                )}

                {/* Summary Info */}
                <div className="mt-4 d-flex justify-content-between align-items-center">
                  <div className="text-muted small">
                    Showing {sortedHazards.length} of {totalCount} reports
                    {totalPages > 1 && ` (Page ${pageNumber} of ${totalPages})`}
                  </div>
                  <div className="d-flex gap-2">
                    {isLoading && (
                      <div className="d-flex align-items-center text-muted">
                        <CSpinner size="sm" className="me-2" />
                        <small>Loading...</small>
                      </div>
                    )}
                    <CButton
                      color="secondary"
                      variant="outline"
                      size="sm"
                      onClick={() => refetch()}
                      disabled={isLoading}
                    >
                      <FontAwesomeIcon icon={faRefresh} className="me-1" />
                      Refresh
                    </CButton>
                  </div>
                </div>
              </>
            )}
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default MyHazards;