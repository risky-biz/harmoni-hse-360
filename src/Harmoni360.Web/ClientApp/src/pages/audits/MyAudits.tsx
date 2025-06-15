import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CForm,
  CFormInput,
  CFormSelect,
  CInputGroup,
  CInputGroupText,
  CTable,
  CTableHead,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CTableRow,
  CBadge,
  CSpinner,
  CAlert,
  CPagination,
  CPaginationItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CButtonGroup,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CFormTextarea
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSearch,
  faFilter,
  faPlus,
  faEdit,
  faEye,
  faPlay,
  faCheck,
  faTimes,
  faClipboardList,
  faExclamationTriangle,
  faClock,
  faSort,
  faSortUp,
  faSortDown,
  faDownload,
  faUser,
  faCalendarAlt
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetMyAuditsQuery, 
  useStartAuditMutation,
  useCompleteAuditMutation,
  useCancelAuditMutation
} from '../../features/audits/auditApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { useDebounce } from '../../hooks/useDebounce';
import type {
  AuditSummaryDto,
  GetAuditsParams,
  AuditStatus,
  AuditType,
  AuditPriority,
  RiskLevel
} from '../../types/audit';
import { formatDistanceToNow, format } from 'date-fns';

interface MyAuditFilters {
  search: string;
  status: AuditStatus | '';
  type: AuditType | '';
  priority: AuditPriority | '';
  startDate: string;
  endDate: string;
  sortBy: string;
  sortDescending: boolean;
}

const AUDIT_STATUSES: { value: AuditStatus | ''; label: string; color: string }[] = [
  { value: '', label: 'All Statuses', color: 'secondary' },
  { value: 'Draft', label: 'Draft', color: 'secondary' },
  { value: 'Scheduled', label: 'Scheduled', color: 'info' },
  { value: 'InProgress', label: 'In Progress', color: 'warning' },
  { value: 'Completed', label: 'Completed', color: 'success' },
  { value: 'Overdue', label: 'Overdue', color: 'danger' },
  { value: 'Cancelled', label: 'Cancelled', color: 'dark' }
];

const AUDIT_TYPES: { value: AuditType | ''; label: string }[] = [
  { value: '', label: 'All Types' },
  { value: 'Safety', label: 'Safety' },
  { value: 'Environmental', label: 'Environmental' },
  { value: 'Equipment', label: 'Equipment' },
  { value: 'Process', label: 'Process' },
  { value: 'Compliance', label: 'Compliance' },
  { value: 'Fire', label: 'Fire Safety' },
  { value: 'Chemical', label: 'Chemical Safety' },
  { value: 'Ergonomic', label: 'Ergonomic' },
  { value: 'Emergency', label: 'Emergency Preparedness' },
  { value: 'Management', label: 'Management System' }
];

const AUDIT_PRIORITIES: { value: AuditPriority | ''; label: string; color: string }[] = [
  { value: '', label: 'All Priorities', color: 'secondary' },
  { value: 'Low', label: 'Low', color: 'success' },
  { value: 'Medium', label: 'Medium', color: 'warning' },
  { value: 'High', label: 'High', color: 'danger' },
  { value: 'Critical', label: 'Critical', color: 'danger' }
];

const MyAudits: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { isDemo } = useApplicationMode();

  // State
  const [filters, setFilters] = useState<MyAuditFilters>({
    search: searchParams.get('search') || '',
    status: (searchParams.get('status') as AuditStatus) || '',
    type: (searchParams.get('type') as AuditType) || '',
    priority: (searchParams.get('priority') as AuditPriority) || '',
    startDate: searchParams.get('startDate') || '',
    endDate: searchParams.get('endDate') || '',
    sortBy: searchParams.get('sortBy') || 'ScheduledDate',
    sortDescending: searchParams.get('sortDescending') === 'true'
  });

  const [currentPage, setCurrentPage] = useState(Number(searchParams.get('page')) || 1);
  const [pageSize] = useState(20);
  const [selectedAudit, setSelectedAudit] = useState<AuditSummaryDto | null>(null);
  const [showCompleteModal, setShowCompleteModal] = useState(false);
  const [completionSummary, setCompletionSummary] = useState('');
  const [completionRecommendations, setCompletionRecommendations] = useState('');

  // Debounce search
  const debouncedSearch = useDebounce(filters.search, 500);

  // API calls
  const queryParams: GetAuditsParams = {
    page: currentPage,
    pageSize,
    search: debouncedSearch || undefined,
    status: filters.status || undefined,
    type: filters.type || undefined,
    priority: filters.priority || undefined,
    startDate: filters.startDate || undefined,
    endDate: filters.endDate || undefined,
    sortBy: filters.sortBy,
    sortDescending: filters.sortDescending
  };

  const {
    data: auditsData,
    error,
    isLoading,
    refetch
  } = useGetMyAuditsQuery(queryParams);

  // Mutations
  const [startAudit] = useStartAuditMutation();
  const [completeAudit] = useCompleteAuditMutation();
  const [cancelAudit] = useCancelAuditMutation();

  // Update URL when filters change
  useEffect(() => {
    const params = new URLSearchParams();
    
    if (filters.search) params.set('search', filters.search);
    if (filters.status) params.set('status', filters.status);
    if (filters.type) params.set('type', filters.type);
    if (filters.priority) params.set('priority', filters.priority);
    if (filters.startDate) params.set('startDate', filters.startDate);
    if (filters.endDate) params.set('endDate', filters.endDate);
    if (filters.sortBy) params.set('sortBy', filters.sortBy);
    params.set('sortDescending', filters.sortDescending.toString());
    params.set('page', currentPage.toString());

    setSearchParams(params);
  }, [filters, currentPage, setSearchParams]);

  // Helper functions
  const getStatusColor = (status: AuditStatus) => {
    const statusConfig = AUDIT_STATUSES.find(s => s.value === status);
    return statusConfig?.color || 'secondary';
  };

  const getPriorityColor = (priority: AuditPriority) => {
    const priorityConfig = AUDIT_PRIORITIES.find(p => p.value === priority);
    return priorityConfig?.color || 'secondary';
  };

  const handleFilterChange = (key: keyof MyAuditFilters, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setCurrentPage(1);
  };

  const handleSort = (field: string) => {
    if (filters.sortBy === field) {
      setFilters(prev => ({ ...prev, sortDescending: !prev.sortDescending }));
    } else {
      setFilters(prev => ({ ...prev, sortBy: field, sortDescending: false }));
    }
  };

  const getSortIcon = (field: string) => {
    if (filters.sortBy !== field) return faSort;
    return filters.sortDescending ? faSortDown : faSortUp;
  };

  const handleCreateAudit = () => {
    navigate('/audits/create');
  };

  const handleViewAudit = (audit: AuditSummaryDto) => {
    navigate(`/audits/${audit.id}`);
  };

  const handleEditAudit = (audit: AuditSummaryDto) => {
    navigate(`/audits/${audit.id}/edit`);
  };

  const handleStartAudit = async (audit: AuditSummaryDto) => {
    try {
      await startAudit(audit.id).unwrap();
      refetch();
    } catch (error) {
      console.error('Failed to start audit:', error);
    }
  };

  const handleCompleteAudit = async () => {
    if (!selectedAudit) return;
    
    try {
      await completeAudit({ 
        id: selectedAudit.id, 
        summary: completionSummary,
        recommendations: completionRecommendations
      }).unwrap();
      setShowCompleteModal(false);
      setSelectedAudit(null);
      setCompletionSummary('');
      setCompletionRecommendations('');
      refetch();
    } catch (error) {
      console.error('Failed to complete audit:', error);
    }
  };

  const handleCancelAudit = async (audit: AuditSummaryDto) => {
    try {
      await cancelAudit({ id: audit.id, reason: 'Cancelled by auditor' }).unwrap();
      refetch();
    } catch (error) {
      console.error('Failed to cancel audit:', error);
    }
  };

  const canEditAudit = (audit: AuditSummaryDto) => {
    return audit.status === 'Draft' || audit.status === 'Scheduled';
  };

  const canStartAudit = (audit: AuditSummaryDto) => {
    return audit.status === 'Scheduled';
  };

  const canCompleteAudit = (audit: AuditSummaryDto) => {
    return audit.status === 'InProgress';
  };

  const canCancelAudit = (audit: AuditSummaryDto) => {
    return audit.status === 'Draft' || audit.status === 'Scheduled' || audit.status === 'InProgress';
  };

  const clearFilters = () => {
    setFilters({
      search: '',
      status: '',
      type: '',
      priority: '',
      startDate: '',
      endDate: '',
      sortBy: 'ScheduledDate',
      sortDescending: false
    });
    setCurrentPage(1);
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner color="primary" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger" className="m-3">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Failed to load your audits. Please try again.
      </CAlert>
    );
  }

  return (
    <div className="my-audits">
      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h1 className="h3 mb-1">
                <FontAwesomeIcon icon={faUser} className="me-2" />
                My Audits
              </h1>
              <p className="text-muted mb-0">
                {auditsData?.totalCount || 0} audit{(auditsData?.totalCount || 0) !== 1 ? 's' : ''} assigned to you
              </p>
            </div>
            <PermissionGuard
              module={ModuleType.AuditManagement}
              permission={PermissionType.Create}
            >
              <CButton
                color="primary"
                onClick={handleCreateAudit}
                disabled={isDemo}
              >
                <FontAwesomeIcon icon={faPlus} className="me-2" />
                New Audit
              </CButton>
            </PermissionGuard>
          </div>
        </CCol>
      </CRow>

      {/* Quick Stats */}
      <CRow className="mb-4">
        <CCol md={3}>
          <CCard className="text-center">
            <CCardBody>
              <div className="text-primary display-6">
                {auditsData?.items.filter(a => a.status === 'Scheduled').length || 0}
              </div>
              <div className="text-muted">Scheduled</div>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={3}>
          <CCard className="text-center">
            <CCardBody>
              <div className="text-warning display-6">
                {auditsData?.items.filter(a => a.status === 'InProgress').length || 0}
              </div>
              <div className="text-muted">In Progress</div>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={3}>
          <CCard className="text-center">
            <CCardBody>
              <div className="text-danger display-6">
                {auditsData?.items.filter(a => a.isOverdue).length || 0}
              </div>
              <div className="text-muted">Overdue</div>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={3}>
          <CCard className="text-center">
            <CCardBody>
              <div className="text-success display-6">
                {auditsData?.items.filter(a => a.status === 'Completed').length || 0}
              </div>
              <div className="text-muted">Completed</div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Filters */}
      <CCard className="mb-4">
        <CCardHeader>
          <div className="d-flex justify-content-between align-items-center">
            <h5 className="mb-0">
              <FontAwesomeIcon icon={faFilter} className="me-2" />
              Filters
            </h5>
            <CButton
              color="secondary"
              variant="outline"
              size="sm"
              onClick={clearFilters}
            >
              Clear All
            </CButton>
          </div>
        </CCardHeader>
        <CCardBody>
          <CForm>
            <CRow>
              <CCol md={4}>
                <CInputGroup className="mb-3">
                  <CInputGroupText>
                    <FontAwesomeIcon icon={faSearch} />
                  </CInputGroupText>
                  <CFormInput
                    placeholder="Search my audits..."
                    value={filters.search}
                    onChange={(e) => handleFilterChange('search', e.target.value)}
                  />
                </CInputGroup>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={filters.status}
                  onChange={(e) => handleFilterChange('status', e.target.value)}
                  className="mb-3"
                >
                  {AUDIT_STATUSES.map(status => (
                    <option key={status.value} value={status.value}>
                      {status.label}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={filters.type}
                  onChange={(e) => handleFilterChange('type', e.target.value)}
                  className="mb-3"
                >
                  {AUDIT_TYPES.map(type => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={filters.priority}
                  onChange={(e) => handleFilterChange('priority', e.target.value)}
                  className="mb-3"
                >
                  {AUDIT_PRIORITIES.map(priority => (
                    <option key={priority.value} value={priority.value}>
                      {priority.label}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormInput
                  type="date"
                  placeholder="Start Date"
                  value={filters.startDate}
                  onChange={(e) => handleFilterChange('startDate', e.target.value)}
                  className="mb-3"
                />
              </CCol>
            </CRow>
          </CForm>
        </CCardBody>
      </CCard>

      {/* Audits Table */}
      <CCard>
        <CCardBody>
          <CTable hover responsive>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell
                  style={{ cursor: 'pointer' }}
                  onClick={() => handleSort('AuditNumber')}
                >
                  Audit Number
                  <FontAwesomeIcon 
                    icon={getSortIcon('AuditNumber')} 
                    className="ms-1" 
                    size="sm" 
                  />
                </CTableHeaderCell>
                <CTableHeaderCell
                  style={{ cursor: 'pointer' }}
                  onClick={() => handleSort('Title')}
                >
                  Title
                  <FontAwesomeIcon 
                    icon={getSortIcon('Title')} 
                    className="ms-1" 
                    size="sm" 
                  />
                </CTableHeaderCell>
                <CTableHeaderCell>Type</CTableHeaderCell>
                <CTableHeaderCell>Status</CTableHeaderCell>
                <CTableHeaderCell>Priority</CTableHeaderCell>
                <CTableHeaderCell
                  style={{ cursor: 'pointer' }}
                  onClick={() => handleSort('ScheduledDate')}
                >
                  Scheduled Date
                  <FontAwesomeIcon 
                    icon={getSortIcon('ScheduledDate')} 
                    className="ms-1" 
                    size="sm" 
                  />
                </CTableHeaderCell>
                <CTableHeaderCell>Progress</CTableHeaderCell>
                <CTableHeaderCell>Findings</CTableHeaderCell>
                <CTableHeaderCell>Actions</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              {auditsData?.items.map((audit) => (
                <CTableRow key={audit.id}>
                  <CTableDataCell>
                    <div className="fw-bold">{audit.auditNumber}</div>
                    {audit.departmentName && (
                      <small className="text-muted">{audit.departmentName}</small>
                    )}
                  </CTableDataCell>
                  <CTableDataCell>
                    <div className="fw-bold">{audit.title}</div>
                    {audit.locationName && (
                      <small className="text-muted">{audit.locationName}</small>
                    )}
                  </CTableDataCell>
                  <CTableDataCell>
                    <CBadge color="info">{audit.typeDisplay}</CBadge>
                  </CTableDataCell>
                  <CTableDataCell>
                    <CBadge color={getStatusColor(audit.status)}>
                      {audit.statusDisplay}
                    </CBadge>
                    {audit.isOverdue && (
                      <CBadge color="danger" className="ms-1">
                        Overdue
                      </CBadge>
                    )}
                  </CTableDataCell>
                  <CTableDataCell>
                    <CBadge color={getPriorityColor(audit.priority)}>
                      {audit.priorityDisplay}
                    </CBadge>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div>{format(new Date(audit.scheduledDate), 'MMM dd, yyyy')}</div>
                    <small className="text-muted">
                      {formatDistanceToNow(new Date(audit.scheduledDate), { addSuffix: true })}
                    </small>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div className="d-flex align-items-center">
                      <div className="flex-grow-1 me-2">
                        <div className="progress" style={{ height: '6px' }}>
                          <div 
                            className="progress-bar" 
                            style={{ width: `${audit.completionPercentage}%` }}
                          />
                        </div>
                      </div>
                      <small className="text-muted">{audit.completionPercentage}%</small>
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div className="d-flex align-items-center">
                      <span className="me-2">{audit.findingsCount}</span>
                      {audit.criticalFindingsCount > 0 && (
                        <CBadge color="danger" title="Critical Findings">
                          {audit.criticalFindingsCount}
                        </CBadge>
                      )}
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <CDropdown>
                      <CDropdownToggle color="secondary" variant="outline" size="sm">
                        Actions
                      </CDropdownToggle>
                      <CDropdownMenu>
                        <CDropdownItem onClick={() => handleViewAudit(audit)}>
                          <FontAwesomeIcon icon={faEye} className="me-2" />
                          View Details
                        </CDropdownItem>
                        
                        {canEditAudit(audit) && (
                          <CDropdownItem onClick={() => handleEditAudit(audit)}>
                            <FontAwesomeIcon icon={faEdit} className="me-2" />
                            Edit
                          </CDropdownItem>
                        )}

                        {canStartAudit(audit) && (
                          <CDropdownItem 
                            onClick={() => handleStartAudit(audit)}
                            disabled={isDemo}
                          >
                            <FontAwesomeIcon icon={faPlay} className="me-2" />
                            Start Audit
                          </CDropdownItem>
                        )}

                        {canCompleteAudit(audit) && (
                          <CDropdownItem 
                            onClick={() => {
                              setSelectedAudit(audit);
                              setShowCompleteModal(true);
                            }}
                            disabled={isDemo}
                          >
                            <FontAwesomeIcon icon={faCheck} className="me-2" />
                            Complete
                          </CDropdownItem>
                        )}

                        {canCancelAudit(audit) && (
                          <CDropdownItem 
                            onClick={() => handleCancelAudit(audit)}
                            className="text-warning"
                            disabled={isDemo}
                          >
                            <FontAwesomeIcon icon={faTimes} className="me-2" />
                            Cancel
                          </CDropdownItem>
                        )}
                      </CDropdownMenu>
                    </CDropdown>
                  </CTableDataCell>
                </CTableRow>
              ))}
            </CTableBody>
          </CTable>

          {auditsData?.items.length === 0 && (
            <div className="text-center py-4">
              <FontAwesomeIcon icon={faClipboardList} size="3x" className="text-muted mb-3" />
              <h5 className="text-muted">No audits found</h5>
              <p className="text-muted">You don't have any audits assigned yet.</p>
            </div>
          )}
        </CCardBody>
      </CCard>

      {/* Pagination */}
      {auditsData && auditsData.totalPages > 1 && (
        <div className="d-flex justify-content-center mt-4">
          <CPagination>
            <CPaginationItem
              disabled={currentPage === 1}
              onClick={() => setCurrentPage(1)}
            >
              First
            </CPaginationItem>
            <CPaginationItem
              disabled={currentPage === 1}
              onClick={() => setCurrentPage(currentPage - 1)}
            >
              Previous
            </CPaginationItem>
            
            {Array.from({ length: Math.min(5, auditsData.totalPages) }, (_, i) => {
              const page = Math.max(1, Math.min(auditsData.totalPages - 4, currentPage - 2)) + i;
              return (
                <CPaginationItem
                  key={page}
                  active={page === currentPage}
                  onClick={() => setCurrentPage(page)}
                >
                  {page}
                </CPaginationItem>
              );
            })}
            
            <CPaginationItem
              disabled={currentPage === auditsData.totalPages}
              onClick={() => setCurrentPage(currentPage + 1)}
            >
              Next
            </CPaginationItem>
            <CPaginationItem
              disabled={currentPage === auditsData.totalPages}
              onClick={() => setCurrentPage(auditsData.totalPages)}
            >
              Last
            </CPaginationItem>
          </CPagination>
        </div>
      )}

      {/* Complete Modal */}
      <CModal visible={showCompleteModal} onClose={() => setShowCompleteModal(false)} size="lg">
        <CModalHeader>
          <CModalTitle>Complete Audit</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <p>Complete the audit "{selectedAudit?.title}" by providing a summary and recommendations.</p>
          
          <div className="mb-3">
            <CFormLabel htmlFor="completionSummary">Audit Summary</CFormLabel>
            <CFormTextarea
              id="completionSummary"
              rows={4}
              value={completionSummary}
              onChange={(e) => setCompletionSummary(e.target.value)}
              placeholder="Provide a summary of the audit findings and overall assessment..."
            />
          </div>

          <div className="mb-3">
            <CFormLabel htmlFor="completionRecommendations">Recommendations</CFormLabel>
            <CFormTextarea
              id="completionRecommendations"
              rows={4}
              value={completionRecommendations}
              onChange={(e) => setCompletionRecommendations(e.target.value)}
              placeholder="Provide recommendations for improvement and follow-up actions..."
            />
          </div>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowCompleteModal(false)}>
            Cancel
          </CButton>
          <CButton color="success" onClick={handleCompleteAudit}>
            <FontAwesomeIcon icon={faCheck} className="me-2" />
            Complete Audit
          </CButton>
        </CModalFooter>
      </CModal>
    </div>
  );
};

export default MyAudits;