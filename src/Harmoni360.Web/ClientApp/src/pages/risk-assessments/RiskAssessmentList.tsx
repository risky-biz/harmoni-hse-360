import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
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
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
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
  faEllipsisV,
  faClipboardCheck,
  faChartLine,
} from '@fortawesome/free-solid-svg-icons';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import { Icon } from '../../components/common/Icon';
import ApiUnavailableMessage from '../../components/common/ApiUnavailableMessage';
import {
  useGetRiskAssessmentsQuery,
  RiskAssessmentDto,
  GetRiskAssessmentsParams,
} from '../../features/risk-assessments/riskAssessmentApi';
import {
  getRiskLevelBadge,
  getAssessmentTypeBadge,
  getApprovalStatusBadge,
  getReviewStatusBadge,
  formatDate,
  formatDateShort,
  isAssessmentDueForReview,
} from '../../utils/riskAssessmentUtils';

type SortField = 'hazardTitle' | 'riskLevel' | 'type' | 'assessmentDate' | 'nextReviewDate' | 'createdAt';
type SortDirection = 'asc' | 'desc';

const RiskAssessmentList: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [riskLevelFilter, setRiskLevelFilter] = useState('');
  const [assessmentTypeFilter, setAssessmentTypeFilter] = useState('');
  const [approvalStatusFilter, setApprovalStatusFilter] = useState('');
  const [showAdvancedFilters, setShowAdvancedFilters] = useState(false);
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const {
    data: riskAssessmentsResponse,
    isLoading,
    error,
    refetch,
  } = useGetRiskAssessmentsQuery({
    pageNumber,
    pageSize,
    searchTerm: searchTerm || undefined,
    riskLevel: riskLevelFilter || undefined,
    assessmentType: assessmentTypeFilter || undefined,
    isApproved: approvalStatusFilter === 'approved' ? true : approvalStatusFilter === 'pending' ? false : undefined,
    isActive: true, // Only show active assessments by default
  });

  // Auto-search when filters change
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      setPageNumber(1);
      refetch();
    }, 300);
    
    return () => clearTimeout(timeoutId);
  }, [searchTerm, riskLevelFilter, assessmentTypeFilter, approvalStatusFilter, refetch]);

  // Handle success messages from navigation state
  useEffect(() => {
    if (location.state?.message) {
      setSuccessMessage(location.state.message);
      // Clear the message from navigation state
      window.history.replaceState({}, document.title);
      // Auto-hide success message after 5 seconds
      const timer = setTimeout(() => setSuccessMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [location.state]);

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
    setRiskLevelFilter('');
    setAssessmentTypeFilter('');
    setApprovalStatusFilter('');
    setPageNumber(1);
  };

  const getSortIcon = (field: SortField) => {
    if (sortField !== field) return faSort;
    return sortDirection === 'asc' ? faSortUp : faSortDown;
  };

  // Client-side sorting and filtering for better UX
  const sortedRiskAssessments = React.useMemo(() => {
    const assessments = riskAssessmentsResponse?.riskAssessments || [];
    return [...assessments].sort((a, b) => {
      let aValue: any = a[sortField];
      let bValue: any = b[sortField];
      
      if (sortField === 'assessmentDate' || sortField === 'nextReviewDate' || sortField === 'createdAt') {
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
  }, [riskAssessmentsResponse?.riskAssessments, sortField, sortDirection]);

  const totalCount = riskAssessmentsResponse?.totalCount || 0;
  const totalPages = Math.ceil(totalCount / pageSize);

  const activeFiltersCount = [
    searchTerm,
    riskLevelFilter,
    assessmentTypeFilter,
    approvalStatusFilter
  ].filter(Boolean).length;

  if (isLoading && !riskAssessmentsResponse) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading risk assessments...</span>
      </div>
    );
  }

  if (error) {
    return (
      <ApiUnavailableMessage
        title="Failed to load risk assessments"
        message="Unable to retrieve risk assessments from the backend API."
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
                  icon={faClipboardCheck}
                  size="lg"
                  className="me-2 text-primary"
                />
                Risk Assessments
              </h4>
              <div className="d-flex align-items-center gap-2">
                <small className="text-muted">
                  {totalCount} assessment{totalCount !== 1 ? 's' : ''} found
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
                color="secondary"
                variant="outline"
                onClick={() => refetch()}
                disabled={isLoading}
                className="d-flex align-items-center"
              >
                <FontAwesomeIcon icon={faRefresh} className="me-2" />
                Refresh
              </CButton>
              <CButton
                color="primary"
                onClick={() => navigate('/risk-assessments/create')}
                className="d-flex align-items-center"
              >
                <FontAwesomeIcon icon={faPlus} className="me-2" />
                New Assessment
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            {/* Success Message */}
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

            {/* Basic Search and Filters */}
            <CRow className="mb-3">
              <CCol lg={5}>
                <CInputGroup>
                  <CInputGroupText>
                    <FontAwesomeIcon icon={faSearch} />
                  </CInputGroupText>
                  <CFormInput
                    type="text"
                    placeholder="Search by hazard title, consequences, or assessor..."
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
                  value={riskLevelFilter}
                  onChange={(e) => setRiskLevelFilter(e.target.value)}
                >
                  <option value="">All Risk Levels</option>
                  <option value="VeryLow">Very Low</option>
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </CFormSelect>
              </CCol>
              <CCol lg={2}>
                <CFormSelect
                  value={assessmentTypeFilter}
                  onChange={(e) => setAssessmentTypeFilter(e.target.value)}
                >
                  <option value="">All Types</option>
                  <option value="General">General</option>
                  <option value="HIRA">HIRA</option>
                  <option value="JSA">JSA</option>
                  <option value="Environmental">Environmental</option>
                  <option value="Fire">Fire Safety</option>
                </CFormSelect>
              </CCol>
              <CCol lg={3} className="d-flex gap-2">
                <CFormSelect
                  value={approvalStatusFilter}
                  onChange={(e) => setApprovalStatusFilter(e.target.value)}
                >
                  <option value="">All Statuses</option>
                  <option value="approved">Approved</option>
                  <option value="pending">Pending Approval</option>
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
                      <label className="form-label">Sort By</label>
                      <CFormSelect
                        value={sortField}
                        onChange={(e) => setSortField(e.target.value as SortField)}
                      >
                        <option value="createdAt">Date Created</option>
                        <option value="assessmentDate">Assessment Date</option>
                        <option value="nextReviewDate">Review Date</option>
                        <option value="hazardTitle">Hazard Title</option>
                        <option value="riskLevel">Risk Level</option>
                        <option value="type">Assessment Type</option>
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
                    <CCol md={3}>
                      <label className="form-label">Page Size</label>
                      <CFormSelect
                        value={pageSize}
                        onChange={(e) => setPageSize(Number(e.target.value))}
                      >
                        <option value={10}>10</option>
                        <option value={25}>25</option>
                        <option value={50}>50</option>
                        <option value={100}>100</option>
                      </CFormSelect>
                    </CCol>
                    <CCol md={3} className="d-flex align-items-end">
                      <CButton
                        color="info"
                        variant="outline"
                        onClick={() => navigate('/risk-assessments/dashboard')}
                        className="w-100"
                      >
                        <FontAwesomeIcon icon={faChartLine} className="me-2" />
                        Dashboard
                      </CButton>
                    </CCol>
                  </CRow>
                </CCardBody>
              </CCard>
            </CCollapse>

            {sortedRiskAssessments.length === 0 ? (
              <CCallout color="info" className="text-center py-4">
                <FontAwesomeIcon
                  icon={faClipboardCheck}
                  size="2xl"
                  className="mb-3 text-muted"
                />
                <h5>No risk assessments found</h5>
                <p className="text-muted mb-3">
                  {searchTerm || riskLevelFilter || assessmentTypeFilter || approvalStatusFilter
                    ? 'No risk assessments match your current filters.'
                    : "No risk assessments have been created yet."}
                </p>
                <div className="d-flex justify-content-center gap-2">
                  {(searchTerm || riskLevelFilter || assessmentTypeFilter || approvalStatusFilter) && (
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
                    onClick={() => navigate('/risk-assessments/create')}
                  >
                    <FontAwesomeIcon icon={faPlus} className="me-2" />
                    Create First Assessment
                  </CButton>
                </div>
              </CCallout>
            ) : (
              <>
                {/* Risk Assessments Table */}
                <div className="table-responsive">
                  <CTable hover className="border">
                    <CTableHead className="table-light">
                      <CTableRow>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('hazardTitle')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Hazard
                            <FontAwesomeIcon 
                              icon={getSortIcon('hazardTitle')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('type')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Type
                            <FontAwesomeIcon 
                              icon={getSortIcon('type')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('riskLevel')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Risk Level
                            <FontAwesomeIcon 
                              icon={getSortIcon('riskLevel')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell>Status</CTableHeaderCell>
                        <CTableHeaderCell>Review Status</CTableHeaderCell>
                        <CTableHeaderCell>Assessor</CTableHeaderCell>
                        <CTableHeaderCell 
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('assessmentDate')}
                        >
                          <div className="d-flex align-items-center justify-content-between">
                            Assessment Date
                            <FontAwesomeIcon 
                              icon={getSortIcon('assessmentDate')} 
                              className="text-muted"
                              size="sm"
                            />
                          </div>
                        </CTableHeaderCell>
                        <CTableHeaderCell>Actions</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {sortedRiskAssessments.map((assessment: RiskAssessmentDto) => (
                        <CTableRow 
                          key={assessment.id}
                          style={{ cursor: 'pointer' }}
                          onClick={() => navigate(`/risk-assessments/${assessment.id}`)}
                        >
                          <CTableDataCell>
                            <div>
                              <div className="fw-semibold text-primary">{assessment.hazardTitle}</div>
                              {assessment.potentialConsequences && (
                                <div className="text-muted small mt-1">
                                  {assessment.potentialConsequences.length > 80
                                    ? `${assessment.potentialConsequences.substring(0, 80)}...`
                                    : assessment.potentialConsequences}
                                </div>
                              )}
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            {getAssessmentTypeBadge(assessment.type)}
                          </CTableDataCell>
                          <CTableDataCell>
                            {getRiskLevelBadge(assessment.riskLevel)}
                          </CTableDataCell>
                          <CTableDataCell>
                            {getApprovalStatusBadge(assessment.isApproved)}
                          </CTableDataCell>
                          <CTableDataCell>
                            {getReviewStatusBadge(assessment.nextReviewDate)}
                          </CTableDataCell>
                          <CTableDataCell>
                            <div className="text-muted small">
                              {assessment.assessorName}
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div className="d-flex align-items-center text-muted">
                              <FontAwesomeIcon icon={faCalendarAlt} className="me-2" size="sm" />
                              <small>{formatDateShort(assessment.assessmentDate)}</small>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell onClick={(e) => e.stopPropagation()}>
                            <CDropdown>
                              <CDropdownToggle color="light" size="sm" caret={false}>
                                <FontAwesomeIcon icon={faEllipsisV} />
                              </CDropdownToggle>
                              <CDropdownMenu>
                                <CDropdownItem onClick={() => navigate(`/risk-assessments/${assessment.id}`)}>
                                  <FontAwesomeIcon icon={faEye} className="me-2" />
                                  View Details
                                </CDropdownItem>
                                <CDropdownItem onClick={() => navigate(`/risk-assessments/${assessment.id}/edit`)}>
                                  <FontAwesomeIcon icon={faEdit} className="me-2" />
                                  Edit Assessment
                                </CDropdownItem>
                                <CDropdownItem onClick={() => navigate(`/hazards/${assessment.hazardId}`)}>
                                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                                  View Hazard
                                </CDropdownItem>
                                {isAssessmentDueForReview(assessment.nextReviewDate) && (
                                  <CDropdownItem onClick={() => navigate(`/risk-assessments/${assessment.id}/reassess`)}>
                                    <FontAwesomeIcon icon={faRefresh} className="me-2" />
                                    Reassess
                                  </CDropdownItem>
                                )}
                              </CDropdownMenu>
                            </CDropdown>
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
                    Showing {sortedRiskAssessments.length} of {totalCount} assessments
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

export default RiskAssessmentList;