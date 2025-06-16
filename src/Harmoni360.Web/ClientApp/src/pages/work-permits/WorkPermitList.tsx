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
  faFileContract,
  faExclamationTriangle,
  faClock,
  faSort,
  faSortUp,
  faSortDown,
  faDownload
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetWorkPermitsQuery, 
  useDeleteWorkPermitMutation,
  useSubmitWorkPermitMutation,
  useApproveWorkPermitMutation,
  useRejectWorkPermitMutation,
  useStartWorkMutation
} from '../../features/work-permits/workPermitApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { useDebounce } from '../../hooks/useDebounce';
import {
  WorkPermitDto,
  WorkPermitFilters,
  WORK_PERMIT_STATUSES,
  WORK_PERMIT_TYPES,
  WORK_PERMIT_PRIORITIES,
  RISK_LEVELS
} from '../../types/workPermit';
import { formatDistanceToNow, format } from 'date-fns';

const WorkPermitList: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { isDemoMode } = useApplicationMode();

  // State
  const [filters, setFilters] = useState<WorkPermitFilters>({
    search: searchParams.get('search') || '',
    status: (searchParams.get('status') as any) || '',
    type: (searchParams.get('type') as any) || '',
    priority: (searchParams.get('priority') as any) || '',
    riskLevel: (searchParams.get('riskLevel') as any) || '',
    department: searchParams.get('department') || '',
    location: searchParams.get('location') || '',
    startDate: searchParams.get('startDate') || '',
    endDate: searchParams.get('endDate') || '',
    sortBy: searchParams.get('sortBy') || 'CreatedAt',
    sortDescending: searchParams.get('sortDescending') === 'true'
  });

  const [currentPage, setCurrentPage] = useState(Number(searchParams.get('page')) || 1);
  const [pageSize, setPageSize] = useState(Number(searchParams.get('pageSize')) || 10);
  const [showFilters, setShowFilters] = useState(false);
  const [selectedPermits, setSelectedPermits] = useState<string[]>([]);
  const [deleteModal, setDeleteModal] = useState<{ show: boolean; permitId?: string; permitTitle?: string }>({
    show: false
  });

  // Debounced search
  const debouncedSearch = useDebounce(filters.search, 300);

  // API calls
  const { data: departments } = useGetDepartmentsQuery({});
  
  const {
    data: permitsData,
    error,
    isLoading,
    refetch
  } = useGetWorkPermitsQuery({
    page: currentPage,
    pageSize,
    search: debouncedSearch,
    status: filters.status || undefined,
    type: filters.type || undefined,
    priority: filters.priority || undefined,
    riskLevel: filters.riskLevel || undefined,
    department: filters.department || undefined,
    location: filters.location || undefined,
    startDate: filters.startDate || undefined,
    endDate: filters.endDate || undefined,
    sortBy: filters.sortBy,
    sortDescending: filters.sortDescending
  });

  const [deletePermit, { isLoading: isDeleting }] = useDeleteWorkPermitMutation();
  const [submitPermit] = useSubmitWorkPermitMutation();
  const [approvePermit] = useApproveWorkPermitMutation();
  const [rejectPermit] = useRejectWorkPermitMutation();
  const [startWork] = useStartWorkMutation();

  // Update URL params when filters change
  useEffect(() => {
    const params = new URLSearchParams();
    
    if (filters.search) params.set('search', filters.search);
    if (filters.status) params.set('status', filters.status);
    if (filters.type) params.set('type', filters.type);
    if (filters.priority) params.set('priority', filters.priority);
    if (filters.riskLevel) params.set('riskLevel', filters.riskLevel);
    if (filters.department) params.set('department', filters.department);
    if (filters.location) params.set('location', filters.location);
    if (filters.startDate) params.set('startDate', filters.startDate);
    if (filters.endDate) params.set('endDate', filters.endDate);
    if (filters.sortBy !== 'CreatedAt') params.set('sortBy', filters.sortBy);
    if (filters.sortDescending) params.set('sortDescending', 'true');
    if (currentPage > 1) params.set('page', currentPage.toString());
    if (pageSize !== 10) params.set('pageSize', pageSize.toString());

    setSearchParams(params);
  }, [filters, currentPage, pageSize, setSearchParams]);

  const handleFilterChange = (key: keyof WorkPermitFilters, value: any) => {
    setFilters(prev => ({
      ...prev,
      [key]: value
    }));
    setCurrentPage(1);
  };

  const handleSort = (column: string) => {
    if (filters.sortBy === column) {
      handleFilterChange('sortDescending', !filters.sortDescending);
    } else {
      handleFilterChange('sortBy', column);
      handleFilterChange('sortDescending', false);
    }
  };

  const clearFilters = () => {
    setFilters({
      search: '',
      status: '',
      type: '',
      priority: '',
      riskLevel: '',
      department: '',
      location: '',
      startDate: '',
      endDate: '',
      sortBy: 'CreatedAt',
      sortDescending: true
    });
    setCurrentPage(1);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteModal.permitId) return;

    try {
      await deletePermit(deleteModal.permitId).unwrap();
      setDeleteModal({ show: false });
      setSelectedPermits(prev => prev.filter(id => id !== deleteModal.permitId));
    } catch (error) {
      console.error('Failed to delete work permit:', error);
    }
  };

  const handleAction = async (action: string, permitId: string) => {
    try {
      switch (action) {
        case 'submit':
          await submitPermit(permitId).unwrap();
          break;
        case 'approve':
          await approvePermit({ id: permitId }).unwrap();
          break;
        case 'reject':
          await rejectPermit({ id: permitId, rejectionReason: 'Rejected from list view' }).unwrap();
          break;
        case 'start':
          await startWork(permitId).unwrap();
          break;
      }
    } catch (error) {
      console.error(`Failed to ${action} work permit:`, error);
    }
  };

  const getStatusBadgeColor = (status: string) => {
    const statusConfig = WORK_PERMIT_STATUSES.find(s => s.value === status);
    return statusConfig?.color || 'secondary';
  };

  const getPriorityBadgeColor = (priority: string) => {
    const priorityConfig = WORK_PERMIT_PRIORITIES.find(p => p.value === priority);
    return priorityConfig?.color || 'secondary';
  };

  const getRiskLevelBadgeColor = (riskLevel: string) => {
    const riskConfig = RISK_LEVELS.find(r => r.value === riskLevel);
    return riskConfig?.color || 'secondary';
  };

  const getSortIcon = (column: string) => {
    if (filters.sortBy !== column) return faSort;
    return filters.sortDescending ? faSortDown : faSortUp;
  };

  const getAvailableActions = (permit: WorkPermitDto) => {
    const actions = [];

    if (permit.status === 'Draft') {
      actions.push({ key: 'submit', label: 'Submit for Approval', icon: faFileContract, color: 'primary' });
    }
    
    if (permit.status === 'PendingApproval') {
      actions.push({ key: 'approve', label: 'Approve', icon: faCheck, color: 'success' });
      actions.push({ key: 'reject', label: 'Reject', icon: faTimes, color: 'danger' });
    }
    
    if (permit.status === 'Approved') {
      actions.push({ key: 'start', label: 'Start Work', icon: faPlay, color: 'info' });
    }

    return actions;
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
        Failed to load work permits. Please try again.
      </CAlert>
    );
  }

  return (
    <div className="work-permit-list">
      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h1 className="h3 mb-1">Work Permits</h1>
              <p className="text-muted mb-0">
                {permitsData?.totalCount || 0} permits found
              </p>
            </div>
            <div className="d-flex gap-2">
              <PermissionGuard
                module={ModuleType.WorkPermitManagement}
                permission={PermissionType.Create}
              >
                <CButton
                  color="primary"
                  onClick={() => navigate('/work-permits/create')}
                  disabled={false}
                >
                  <FontAwesomeIcon icon={faPlus} className="me-2" />
                  New Work Permit
                </CButton>
              </PermissionGuard>
            </div>
          </div>
        </CCol>
      </CRow>

      {/* Filters */}
      <CCard className="mb-4">
        <CCardHeader>
          <div className="d-flex justify-content-between align-items-center">
            <span>Search & Filter</span>
            <CButton
              color="link"
              className="p-0"
              onClick={() => setShowFilters(!showFilters)}
            >
              <FontAwesomeIcon icon={faFilter} className="me-1" />
              {showFilters ? 'Hide' : 'Show'} Filters
            </CButton>
          </div>
        </CCardHeader>
        <CCardBody>
          <CRow>
            <CCol md={6}>
              <CInputGroup className="mb-3">
                <CInputGroupText>
                  <FontAwesomeIcon icon={faSearch} />
                </CInputGroupText>
                <CFormInput
                  placeholder="Search permits by title, description, or permit number..."
                  value={filters.search}
                  onChange={(e) => handleFilterChange('search', e.target.value)}
                />
              </CInputGroup>
            </CCol>
            <CCol md={3}>
              <CFormSelect
                value={filters.status}
                onChange={(e) => handleFilterChange('status', e.target.value)}
              >
                <option value="">All Statuses</option>
                {WORK_PERMIT_STATUSES.map(status => (
                  <option key={status.value} value={status.value}>
                    {status.label}
                  </option>
                ))}
              </CFormSelect>
            </CCol>
            <CCol md={3}>
              <CFormSelect
                value={filters.type}
                onChange={(e) => handleFilterChange('type', e.target.value)}
              >
                <option value="">All Types</option>
                {WORK_PERMIT_TYPES.map(type => (
                  <option key={type.value} value={type.value}>
                    {type.label}
                  </option>
                ))}
              </CFormSelect>
            </CCol>
          </CRow>

          {showFilters && (
            <>
              <CRow>
                <CCol md={3}>
                  <CFormSelect
                    value={filters.priority}
                    onChange={(e) => handleFilterChange('priority', e.target.value)}
                  >
                    <option value="">All Priorities</option>
                    {WORK_PERMIT_PRIORITIES.map(priority => (
                      <option key={priority.value} value={priority.value}>
                        {priority.label}
                      </option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={3}>
                  <CFormSelect
                    value={filters.riskLevel}
                    onChange={(e) => handleFilterChange('riskLevel', e.target.value)}
                  >
                    <option value="">All Risk Levels</option>
                    {RISK_LEVELS.map(risk => (
                      <option key={risk.value} value={risk.value}>
                        {risk.label}
                      </option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={3}>
                  <CFormSelect
                    value={filters.department}
                    onChange={(e) => handleFilterChange('department', e.target.value)}
                  >
                    <option value="">All Departments</option>
                    {departments?.map(dept => (
                      <option key={dept.id} value={dept.name}>
                        {dept.name}
                      </option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={3}>
                  <CFormInput
                    type="text"
                    placeholder="Location"
                    value={filters.location}
                    onChange={(e) => handleFilterChange('location', e.target.value)}
                  />
                </CCol>
              </CRow>
              <CRow className="mt-3">
                <CCol md={3}>
                  <CFormInput
                    type="date"
                    placeholder="Start Date"
                    value={filters.startDate}
                    onChange={(e) => handleFilterChange('startDate', e.target.value)}
                  />
                </CCol>
                <CCol md={3}>
                  <CFormInput
                    type="date"
                    placeholder="End Date"
                    value={filters.endDate}
                    onChange={(e) => handleFilterChange('endDate', e.target.value)}
                  />
                </CCol>
                <CCol md={6} className="d-flex justify-content-end">
                  <CButton
                    color="secondary"
                    variant="outline"
                    onClick={clearFilters}
                  >
                    Clear Filters
                  </CButton>
                </CCol>
              </CRow>
            </>
          )}
        </CCardBody>
      </CCard>

      {/* Results */}
      <CCard>
        <CCardHeader>
          <div className="d-flex justify-content-between align-items-center">
            <span>Work Permits ({permitsData?.totalCount || 0})</span>
            <div className="d-flex align-items-center gap-2">
              <span className="text-muted small">Show:</span>
              <CFormSelect
                size="sm"
                style={{ width: 'auto' }}
                value={pageSize}
                onChange={(e) => {
                  setPageSize(Number(e.target.value));
                  setCurrentPage(1);
                }}
              >
                <option value={10}>10</option>
                <option value={25}>25</option>
                <option value={50}>50</option>
                <option value={100}>100</option>
              </CFormSelect>
            </div>
          </div>
        </CCardHeader>
        <CCardBody className="p-0">
          <CTable responsive hover>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell style={{ width: '50px' }}>
                  {/* Bulk selection checkbox could go here */}
                </CTableHeaderCell>
                <CTableHeaderCell 
                  style={{ cursor: 'pointer' }}
                  onClick={() => handleSort('PermitNumber')}
                >
                  Permit Number
                  <FontAwesomeIcon 
                    icon={getSortIcon('PermitNumber')} 
                    className="ms-1 text-muted"
                  />
                </CTableHeaderCell>
                <CTableHeaderCell 
                  style={{ cursor: 'pointer' }}
                  onClick={() => handleSort('Title')}
                >
                  Title
                  <FontAwesomeIcon 
                    icon={getSortIcon('Title')} 
                    className="ms-1 text-muted"
                  />
                </CTableHeaderCell>
                <CTableHeaderCell>Type</CTableHeaderCell>
                <CTableHeaderCell>Status</CTableHeaderCell>
                <CTableHeaderCell>Priority</CTableHeaderCell>
                <CTableHeaderCell>Risk Level</CTableHeaderCell>
                <CTableHeaderCell>Location</CTableHeaderCell>
                <CTableHeaderCell 
                  style={{ cursor: 'pointer' }}
                  onClick={() => handleSort('PlannedStartDate')}
                >
                  Start Date
                  <FontAwesomeIcon 
                    icon={getSortIcon('PlannedStartDate')} 
                    className="ms-1 text-muted"
                  />
                </CTableHeaderCell>
                <CTableHeaderCell>Actions</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              {permitsData?.items?.map((permit) => (
                <CTableRow key={permit.id}>
                  <CTableDataCell>
                    {/* Individual selection checkbox could go here */}
                  </CTableDataCell>
                  <CTableDataCell>
                    <strong>{permit.permitNumber}</strong>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div>
                      <div className="fw-semibold">{permit.title}</div>
                      <div className="text-muted small">
                        {permit.requestedByName} • {permit.requestedByDepartment}
                      </div>
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <span className="text-muted">{permit.typeDisplay}</span>
                  </CTableDataCell>
                  <CTableDataCell>
                    <CBadge color={getStatusBadgeColor(permit.status)}>
                      {permit.statusDisplay}
                    </CBadge>
                    {permit.isOverdue && (
                      <CBadge color="danger" className="ms-1">
                        <FontAwesomeIcon icon={faClock} className="me-1" />
                        Overdue
                      </CBadge>
                    )}
                  </CTableDataCell>
                  <CTableDataCell>
                    <CBadge color={getPriorityBadgeColor(permit.priority)}>
                      {permit.priorityDisplay}
                    </CBadge>
                  </CTableDataCell>
                  <CTableDataCell>
                    <CBadge color={getRiskLevelBadgeColor(permit.riskLevel)}>
                      {permit.riskLevelDisplay}
                    </CBadge>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div className="text-muted small">
                      {permit.workLocation}
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div className="text-muted small">
                      {format(new Date(permit.plannedStartDate), 'MMM dd, yyyy')}
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <CButtonGroup size="sm">
                      <CButton
                        color="info"
                        variant="outline"
                        onClick={() => navigate(`/work-permits/${permit.id}`)}
                      >
                        <FontAwesomeIcon icon={faEye} />
                      </CButton>
                      
                      {permit.status === 'Draft' && (
                        <PermissionGuard
                          module={ModuleType.WorkPermitManagement}
                          permission={PermissionType.Update}
                        >
                          <CButton
                            color="primary"
                            variant="outline"
                            onClick={() => navigate(`/work-permits/${permit.id}/edit`)}
                            disabled={false}
                          >
                            <FontAwesomeIcon icon={faEdit} />
                          </CButton>
                        </PermissionGuard>
                      )}

                      {getAvailableActions(permit).map((action) => (
                        <PermissionGuard
                          key={action.key}
                          module={ModuleType.WorkPermitManagement}
                          permission={action.key === 'approve' || action.key === 'reject' ? PermissionType.Approve : PermissionType.Update}
                        >
                          <CButton
                            color={action.color}
                            variant="outline"
                            onClick={() => handleAction(action.key, permit.id.toString())}
                            disabled={false}
                            title={action.label}
                          >
                            <FontAwesomeIcon icon={action.icon} />
                          </CButton>
                        </PermissionGuard>
                      ))}

                      {permit.status === 'Draft' && (
                        <PermissionGuard
                          module={ModuleType.WorkPermitManagement}
                          permission={PermissionType.Delete}
                        >
                          <CButton
                            color="danger"
                            variant="outline"
                            onClick={() => setDeleteModal({
                              show: true,
                              permitId: permit.id.toString(),
                              permitTitle: permit.title
                            })}
                            disabled={false}
                          >
                            <FontAwesomeIcon icon={faTrash} />
                          </CButton>
                        </PermissionGuard>
                      )}
                    </CButtonGroup>
                  </CTableDataCell>
                </CTableRow>
              ))}
            </CTableBody>
          </CTable>

          {(!permitsData?.items || permitsData.items.length === 0) && (
            <div className="text-center py-4 text-muted">
              <FontAwesomeIcon icon={faFileContract} size="3x" className="mb-3 opacity-50" />
              <div>No work permits found</div>
              <div className="small">Try adjusting your search criteria</div>
            </div>
          )}
        </CCardBody>

        {/* Pagination */}
        {permitsData && permitsData.totalCount > 0 && (
          <div className="card-footer">
            <div className="d-flex justify-content-between align-items-center">
              <div className="text-muted small">
                Showing {((currentPage - 1) * pageSize) + 1} to {Math.min(currentPage * pageSize, permitsData.totalCount)} of {permitsData.totalCount} permits
              </div>
              
              {permitsData.pageCount > 1 && (
                <CPagination>
                  <CPaginationItem
                    disabled={currentPage === 1}
                    onClick={() => setCurrentPage(1)}
                  >
                    First
                  </CPaginationItem>
                  <CPaginationItem
                    disabled={!permitsData.hasPreviousPage}
                    onClick={() => setCurrentPage(currentPage - 1)}
                  >
                    Previous
                  </CPaginationItem>
                  
                  {Array.from({ length: Math.min(5, permitsData.pageCount) }, (_, i) => {
                    const pageNumber = Math.max(1, currentPage - 2) + i;
                    if (pageNumber <= permitsData.pageCount) {
                      return (
                        <CPaginationItem
                          key={pageNumber}
                          active={pageNumber === currentPage}
                          onClick={() => setCurrentPage(pageNumber)}
                        >
                          {pageNumber}
                        </CPaginationItem>
                      );
                    }
                    return null;
                  })}
                  
                  <CPaginationItem
                    disabled={!permitsData.hasNextPage}
                    onClick={() => setCurrentPage(currentPage + 1)}
                  >
                    Next
                  </CPaginationItem>
                  <CPaginationItem
                    disabled={currentPage === permitsData.pageCount}
                    onClick={() => setCurrentPage(permitsData.pageCount)}
                  >
                    Last
                  </CPaginationItem>
                </CPagination>
              )}
            </div>
          </div>
        )}
      </CCard>

      {/* Delete Confirmation Modal */}
      <CModal
        visible={deleteModal.show}
        onClose={() => setDeleteModal({ show: false })}
      >
        <CModalHeader>
          <CModalTitle>Confirm Deletion</CModalTitle>
        </CModalHeader>
        <CModalBody>
          Are you sure you want to delete the work permit "{deleteModal.permitTitle}"? This action cannot be undone.
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={() => setDeleteModal({ show: false })}
          >
            Cancel
          </CButton>
          <CButton
            color="danger"
            onClick={handleDeleteConfirm}
            disabled={isDeleting}
          >
            {isDeleting && <CSpinner size="sm" className="me-2" />}
            Delete
          </CButton>
        </CModalFooter>
      </CModal>
    </div>
  );
};

export default WorkPermitList;