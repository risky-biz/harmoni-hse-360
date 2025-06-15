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
  CModalFooter
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSearch,
  faFilter,
  faPlus,
  faEdit,
  faEye,
  faTrash,
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
  faArchive,
  faCalendarAlt
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetAuditsQuery, 
  useDeleteAuditMutation,
  useStartAuditMutation,
  useCompleteAuditMutation,
  useCancelAuditMutation,
  useArchiveAuditMutation
} from '../../features/audits/auditApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
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
  RiskLevel,
  AuditCategory
} from '../../types/audit';
import { formatDistanceToNow, format } from 'date-fns';

interface AuditFilters {
  search: string;
  status: AuditStatus | '';
  type: AuditType | '';
  category: AuditCategory | '';
  priority: AuditPriority | '';
  riskLevel: RiskLevel | '';
  departmentId: string;
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
  { value: 'Cancelled', label: 'Cancelled', color: 'dark' },
  { value: 'Archived', label: 'Archived', color: 'light' },
  { value: 'UnderReview', label: 'Under Review', color: 'primary' }
];

const AUDIT_TYPES: { value: AuditType | ''; label: string }[] = [
  { value: '', label: 'All Types' },
  { value: 'Safety', label: 'Safety Audit' },
  { value: 'Environmental', label: 'Environmental Audit' },
  { value: 'Equipment', label: 'Equipment Audit' },
  { value: 'Process', label: 'Process Audit' },
  { value: 'Compliance', label: 'Compliance Audit' },
  { value: 'Fire', label: 'Fire Safety Audit' },
  { value: 'Chemical', label: 'Chemical Safety Audit' },
  { value: 'Ergonomic', label: 'Ergonomic Audit' },
  { value: 'Emergency', label: 'Emergency Preparedness Audit' },
  { value: 'Management', label: 'Management System Audit' }
];

const AUDIT_CATEGORIES: { value: AuditCategory | ''; label: string }[] = [
  { value: '', label: 'All Categories' },
  { value: 'Routine', label: 'Routine' },
  { value: 'Planned', label: 'Planned' },
  { value: 'Unplanned', label: 'Unplanned' },
  { value: 'Regulatory', label: 'Regulatory' },
  { value: 'Internal', label: 'Internal' },
  { value: 'External', label: 'External' },
  { value: 'Incident', label: 'Incident Follow-up' },
  { value: 'Maintenance', label: 'Maintenance' }
];

const AUDIT_PRIORITIES: { value: AuditPriority | ''; label: string; color: string }[] = [
  { value: '', label: 'All Priorities', color: 'secondary' },
  { value: 'Low', label: 'Low', color: 'success' },
  { value: 'Medium', label: 'Medium', color: 'warning' },
  { value: 'High', label: 'High', color: 'danger' },
  { value: 'Critical', label: 'Critical', color: 'danger' }
];

const RISK_LEVELS: { value: RiskLevel | ''; label: string; color: string }[] = [
  { value: '', label: 'All Risk Levels', color: 'secondary' },
  { value: 'Low', label: 'Low', color: 'success' },
  { value: 'Medium', label: 'Medium', color: 'warning' },
  { value: 'High', label: 'High', color: 'danger' },
  { value: 'Critical', label: 'Critical', color: 'danger' }
];

const AuditList: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { isDemo } = useApplicationMode();

  // State
  const [filters, setFilters] = useState<AuditFilters>({
    search: searchParams.get('search') || '',
    status: (searchParams.get('status') as AuditStatus) || '',
    type: (searchParams.get('type') as AuditType) || '',
    category: (searchParams.get('category') as AuditCategory) || '',
    priority: (searchParams.get('priority') as AuditPriority) || '',
    riskLevel: (searchParams.get('riskLevel') as RiskLevel) || '',
    departmentId: searchParams.get('departmentId') || '',
    startDate: searchParams.get('startDate') || '',
    endDate: searchParams.get('endDate') || '',
    sortBy: searchParams.get('sortBy') || 'ScheduledDate',
    sortDescending: searchParams.get('sortDescending') === 'true'
  });

  const [currentPage, setCurrentPage] = useState(Number(searchParams.get('page')) || 1);
  const [pageSize] = useState(20);
  const [selectedAudit, setSelectedAudit] = useState<AuditSummaryDto | null>(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showArchiveModal, setShowArchiveModal] = useState(false);
  const [showCompleteModal, setShowCompleteModal] = useState(false);

  // Debounce search
  const debouncedSearch = useDebounce(filters.search, 500);

  // API calls
  const { data: departments } = useGetDepartmentsQuery();
  
  const queryParams: GetAuditsParams = {
    page: currentPage,
    pageSize,
    search: debouncedSearch || undefined,
    status: filters.status || undefined,
    type: filters.type || undefined,
    category: filters.category || undefined,
    priority: filters.priority || undefined,
    riskLevel: filters.riskLevel || undefined,
    departmentId: filters.departmentId ? Number(filters.departmentId) : undefined,
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
  } = useGetAuditsQuery(queryParams);

  // Mutations
  const [deleteAudit] = useDeleteAuditMutation();
  const [startAudit] = useStartAuditMutation();
  const [completeAudit] = useCompleteAuditMutation();
  const [cancelAudit] = useCancelAuditMutation();
  const [archiveAudit] = useArchiveAuditMutation();

  // Update URL when filters change
  useEffect(() => {
    const params = new URLSearchParams();
    
    if (filters.search) params.set('search', filters.search);
    if (filters.status) params.set('status', filters.status);
    if (filters.type) params.set('type', filters.type);
    if (filters.category) params.set('category', filters.category);
    if (filters.priority) params.set('priority', filters.priority);
    if (filters.riskLevel) params.set('riskLevel', filters.riskLevel);
    if (filters.departmentId) params.set('departmentId', filters.departmentId);
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

  const getRiskLevelColor = (riskLevel: RiskLevel) => {
    const riskConfig = RISK_LEVELS.find(r => r.value === riskLevel);
    return riskConfig?.color || 'secondary';
  };

  const handleFilterChange = (key: keyof AuditFilters, value: any) => {
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

  const handleDeleteAudit = async () => {
    if (!selectedAudit) return;
    
    try {
      await deleteAudit(selectedAudit.id).unwrap();
      setShowDeleteModal(false);
      setSelectedAudit(null);
      refetch();
    } catch (error) {
      console.error('Failed to delete audit:', error);
    }
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
      await completeAudit({ id: selectedAudit.id }).unwrap();
      setShowCompleteModal(false);
      setSelectedAudit(null);
      refetch();
    } catch (error) {
      console.error('Failed to complete audit:', error);
    }
  };

  const handleArchiveAudit = async () => {
    if (!selectedAudit) return;
    
    try {
      await archiveAudit({ id: selectedAudit.id }).unwrap();
      setShowArchiveModal(false);
      setSelectedAudit(null);
      refetch();
    } catch (error) {
      console.error('Failed to archive audit:', error);
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
      category: '',
      priority: '',
      riskLevel: '',
      departmentId: '',
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
        Failed to load audits. Please try again.
      </CAlert>
    );
  }

  return (
    <div className="audit-list">
      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h1 className="h3 mb-1">Audit Management</h1>
              <p className="text-muted mb-0">
                {auditsData?.totalCount || 0} audit{(auditsData?.totalCount || 0) !== 1 ? 's' : ''} found
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
                    placeholder="Search audits..."
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
                  value={filters.category}
                  onChange={(e) => handleFilterChange('category', e.target.value)}
                  className="mb-3"
                >
                  {AUDIT_CATEGORIES.map(category => (
                    <option key={category.value} value={category.value}>
                      {category.label}
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
            </CRow>
            <CRow>
              <CCol md={2}>
                <CFormSelect
                  value={filters.riskLevel}
                  onChange={(e) => handleFilterChange('riskLevel', e.target.value)}
                  className="mb-3"
                >
                  {RISK_LEVELS.map(risk => (
                    <option key={risk.value} value={risk.value}>
                      {risk.label}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={filters.departmentId}
                  onChange={(e) => handleFilterChange('departmentId', e.target.value)}
                  className="mb-3"
                >
                  <option value="">All Departments</option>
                  {departments?.map(dept => (
                    <option key={dept.id} value={dept.id.toString()}>
                      {dept.name}
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
              <CCol md={2}>
                <CFormInput
                  type="date"
                  placeholder="End Date"
                  value={filters.endDate}
                  onChange={(e) => handleFilterChange('endDate', e.target.value)}
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
                <CTableHeaderCell>Risk Level</CTableHeaderCell>
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
                <CTableHeaderCell>Auditor</CTableHeaderCell>
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
                    <CBadge color={getRiskLevelColor(audit.riskLevel)}>
                      {audit.riskLevelDisplay}
                    </CBadge>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div>{format(new Date(audit.scheduledDate), 'MMM dd, yyyy')}</div>
                    <small className="text-muted">
                      {formatDistanceToNow(new Date(audit.scheduledDate), { addSuffix: true })}
                    </small>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div>{audit.auditorName}</div>
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
                        
                        <PermissionGuard
                          module={ModuleType.AuditManagement}
                          permission={PermissionType.Update}
                        >
                          {canEditAudit(audit) && (
                            <CDropdownItem onClick={() => handleEditAudit(audit)}>
                              <FontAwesomeIcon icon={faEdit} className="me-2" />
                              Edit
                            </CDropdownItem>
                          )}
                        </PermissionGuard>

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

                        <CDropdownItem divider />

                        {audit.status === 'Completed' && (
                          <CDropdownItem 
                            onClick={() => {
                              setSelectedAudit(audit);
                              setShowArchiveModal(true);
                            }}
                            disabled={isDemo}
                          >
                            <FontAwesomeIcon icon={faArchive} className="me-2" />
                            Archive
                          </CDropdownItem>
                        )}

                        <PermissionGuard
                          module={ModuleType.AuditManagement}
                          permission={PermissionType.Delete}
                        >
                          <CDropdownItem 
                            onClick={() => {
                              setSelectedAudit(audit);
                              setShowDeleteModal(true);
                            }}
                            className="text-danger"
                            disabled={isDemo || audit.status === 'InProgress'}
                          >
                            <FontAwesomeIcon icon={faTrash} className="me-2" />
                            Delete
                          </CDropdownItem>
                        </PermissionGuard>
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
              <p className="text-muted">Try adjusting your filters or create a new audit.</p>
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

      {/* Delete Modal */}
      <CModal visible={showDeleteModal} onClose={() => setShowDeleteModal(false)}>
        <CModalHeader>
          <CModalTitle>Confirm Delete</CModalTitle>
        </CModalHeader>
        <CModalBody>
          Are you sure you want to delete the audit "{selectedAudit?.title}"? This action cannot be undone.
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowDeleteModal(false)}>
            Cancel
          </CButton>
          <CButton color="danger" onClick={handleDeleteAudit}>
            Delete
          </CButton>
        </CModalFooter>
      </CModal>

      {/* Archive Modal */}
      <CModal visible={showArchiveModal} onClose={() => setShowArchiveModal(false)}>
        <CModalHeader>
          <CModalTitle>Confirm Archive</CModalTitle>
        </CModalHeader>
        <CModalBody>
          Are you sure you want to archive the audit "{selectedAudit?.title}"? Archived audits can be restored later.
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowArchiveModal(false)}>
            Cancel
          </CButton>
          <CButton color="warning" onClick={handleArchiveAudit}>
            Archive
          </CButton>
        </CModalFooter>
      </CModal>

      {/* Complete Modal */}
      <CModal visible={showCompleteModal} onClose={() => setShowCompleteModal(false)}>
        <CModalHeader>
          <CModalTitle>Complete Audit</CModalTitle>
        </CModalHeader>
        <CModalBody>
          Are you sure you want to mark the audit "{selectedAudit?.title}" as completed? 
          Make sure all audit items have been assessed and findings documented.
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowCompleteModal(false)}>
            Cancel
          </CButton>
          <CButton color="success" onClick={handleCompleteAudit}>
            Complete Audit
          </CButton>
        </CModalFooter>
      </CModal>
    </div>
  );
};

export default AuditList;