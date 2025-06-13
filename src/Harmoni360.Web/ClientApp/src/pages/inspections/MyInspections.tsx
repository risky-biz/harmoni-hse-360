import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CBadge,
  CButton,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CSpinner,
  CAlert,
  CInputGroup,
  CFormInput,
  CButtonGroup,
  CFormSelect,
  CProgress,
  CProgressBar,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faSearch,
  faFilter,
  faEye,
  faEdit,
  faCheck,
  faClock,
  faExclamationTriangle,
  faClipboardCheck,
  faUsers,
  faTasks,
  faCalendarAlt,
  faUser,
  faBuilding,
  faChartLine,
  faRefresh,
  faPlay,
  faSearchPlus
} from '@fortawesome/free-solid-svg-icons';
import { useGetMyInspectionsQuery } from '../../features/inspections/inspectionApi';
import { InspectionDto, InspectionStatus, InspectionPriority } from '../../types/inspection';
import DemoModeWrapper from '../../components/common/DemoModeWrapper';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { format, formatDistanceToNow, isAfter } from 'date-fns';
import { useDebounce } from '../../hooks/useDebounce';

type InspectionTab = 'all' | 'draft' | 'assigned' | 'overdue' | 'completed';

interface TabStats {
  all: number;
  draft: number;
  assigned: number;
  overdue: number;
  completed: number;
}

export const MyInspections: React.FC = () => {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<InspectionTab>('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [sortBy, setSortBy] = useState('scheduledDate');
  const [sortDescending, setSortDescending] = useState(true);
  const [stats, setStats] = useState<TabStats>({
    all: 0,
    draft: 0,
    assigned: 0,
    overdue: 0,
    completed: 0
  });

  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  // Build query parameters based on active tab
  const queryParams = useMemo(() => {
    const baseParams = {
      searchTerm: debouncedSearchTerm || undefined,
      sortBy,
      sortDescending,
      pageSize: 50 // Show more items for personal view
    };

    switch (activeTab) {
      case 'draft':
        return { ...baseParams, status: 'Draft' };
      case 'assigned':
        return { ...baseParams, status: 'Scheduled,InProgress' };
      case 'overdue':
        return { ...baseParams, isOverdue: true };
      case 'completed':
        return { ...baseParams, status: 'Completed' };
      default:
        return baseParams;
    }
  }, [activeTab, debouncedSearchTerm, sortBy, sortDescending]);

  const {
    data: inspectionsData,
    isLoading,
    error,
    refetch
  } = useGetMyInspectionsQuery(queryParams);

  const inspections = inspectionsData?.items || [];

  // Calculate stats from all inspections (not filtered by tab)
  const {
    data: allInspectionsData
  } = useGetMyInspectionsQuery({ pageSize: 1000 }); // Get all for stats

  useEffect(() => {
    if (allInspectionsData?.items) {
      const allInspections = allInspectionsData.items;
      const newStats: TabStats = {
        all: allInspections.length,
        draft: allInspections.filter(i => i.status === InspectionStatus.Draft).length,
        assigned: allInspections.filter(i => 
          i.status === InspectionStatus.Scheduled || i.status === InspectionStatus.InProgress
        ).length,
        overdue: allInspections.filter(i => i.isOverdue).length,
        completed: allInspections.filter(i => i.status === InspectionStatus.Completed).length
      };
      setStats(newStats);
    }
  }, [allInspectionsData]);

  const getStatusBadge = (status: InspectionStatus) => {
    const statusConfig = {
      [InspectionStatus.Draft]: { color: 'secondary', text: 'Draft' },
      [InspectionStatus.Scheduled]: { color: 'info', text: 'Scheduled' },
      [InspectionStatus.InProgress]: { color: 'warning', text: 'In Progress' },
      [InspectionStatus.Completed]: { color: 'success', text: 'Completed' },
      [InspectionStatus.Cancelled]: { color: 'danger', text: 'Cancelled' }
    };
    const config = statusConfig[status] || { color: 'secondary', text: status };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  };

  const getPriorityBadge = (priority: InspectionPriority) => {
    const priorityConfig = {
      [InspectionPriority.Low]: { color: 'success', text: 'Low' },
      [InspectionPriority.Medium]: { color: 'warning', text: 'Medium' },
      [InspectionPriority.High]: { color: 'danger', text: 'High' },
      [InspectionPriority.Critical]: { color: 'dark', text: 'Critical' }
    };
    const config = priorityConfig[priority] || { color: 'secondary', text: priority };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  };

  const handleCreateInspection = () => {
    navigate('/inspections/create');
  };

  const handleViewInspection = (id: number) => {
    navigate(`/inspections/${id}`);
  };

  const handleEditInspection = (id: number) => {
    navigate(`/inspections/${id}/edit`);
  };

  if (error) {
    return (
      <CAlert color="danger">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Error loading inspections. Please try again.
      </CAlert>
    );
  }

  return (
    <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Read}>
      <DemoModeWrapper>
        <CRow>
          <CCol>
            <CCard>
              <CCardHeader className="d-flex justify-content-between align-items-center">
                <div>
                  <h4 className="mb-0">
                    <FontAwesomeIcon icon={faClipboardCheck} className="me-2 text-primary" />
                    My Inspections
                  </h4>
                  <small className="text-medium-emphasis">
                    Inspections assigned to you or created by you
                  </small>
                </div>
                <div>
                  <CButton
                    color="light"
                    variant="outline"
                    className="me-2"
                    onClick={() => refetch()}
                    disabled={isLoading}
                  >
                    <FontAwesomeIcon icon={faRefresh} className="me-1" />
                    Refresh
                  </CButton>
                  <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Create}>
                    <CButton
                      color="primary"
                      onClick={handleCreateInspection}
                    >
                      <FontAwesomeIcon icon={faPlus} className="me-1" />
                      Create Inspection
                    </CButton>
                  </PermissionGuard>
                </div>
              </CCardHeader>

              <CCardBody>
                {/* Quick Stats */}
                <CRow className="mb-4">
                  <CCol sm={6} lg={2}>
                    <div className="border-start border-start-4 border-start-info py-1 px-3">
                      <div className="text-medium-emphasis small">Total</div>
                      <div className="fs-5 fw-semibold">{stats.all}</div>
                    </div>
                  </CCol>
                  <CCol sm={6} lg={2}>
                    <div className="border-start border-start-4 border-start-secondary py-1 px-3">
                      <div className="text-medium-emphasis small">Draft</div>
                      <div className="fs-5 fw-semibold">{stats.draft}</div>
                    </div>
                  </CCol>
                  <CCol sm={6} lg={2}>
                    <div className="border-start border-start-4 border-start-warning py-1 px-3">
                      <div className="text-medium-emphasis small">Assigned</div>
                      <div className="fs-5 fw-semibold">{stats.assigned}</div>
                    </div>
                  </CCol>
                  <CCol sm={6} lg={2}>
                    <div className="border-start border-start-4 border-start-danger py-1 px-3">
                      <div className="text-medium-emphasis small">Overdue</div>
                      <div className="fs-5 fw-semibold">{stats.overdue}</div>
                    </div>
                  </CCol>
                  <CCol sm={6} lg={2}>
                    <div className="border-start border-start-4 border-start-success py-1 px-3">
                      <div className="text-medium-emphasis small">Completed</div>
                      <div className="fs-5 fw-semibold">{stats.completed}</div>
                    </div>
                  </CCol>
                  <CCol sm={6} lg={2}>
                    <div className="border-start border-start-4 border-start-primary py-1 px-3">
                      <div className="text-medium-emphasis small">Completion Rate</div>
                      <div className="fs-5 fw-semibold">
                        {stats.all > 0 ? Math.round((stats.completed / stats.all) * 100) : 0}%
                      </div>
                    </div>
                  </CCol>
                </CRow>

                {/* Tab Navigation */}
                <CNav variant="tabs" className="mb-3">
                  <CNavItem>
                    <CNavLink
                      active={activeTab === 'all'}
                      onClick={() => setActiveTab('all')}
                      style={{ cursor: 'pointer' }}
                    >
                      All ({stats.all})
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      active={activeTab === 'draft'}
                      onClick={() => setActiveTab('draft')}
                      style={{ cursor: 'pointer' }}
                    >
                      Draft ({stats.draft})
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      active={activeTab === 'assigned'}
                      onClick={() => setActiveTab('assigned')}
                      style={{ cursor: 'pointer' }}
                    >
                      Assigned ({stats.assigned})
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      active={activeTab === 'overdue'}
                      onClick={() => setActiveTab('overdue')}
                      style={{ cursor: 'pointer' }}
                    >
                      <CBadge color="danger" className="me-1">!</CBadge>
                      Overdue ({stats.overdue})
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      active={activeTab === 'completed'}
                      onClick={() => setActiveTab('completed')}
                      style={{ cursor: 'pointer' }}
                    >
                      Completed ({stats.completed})
                    </CNavLink>
                  </CNavItem>
                </CNav>

                {/* Search and Controls */}
                <CRow className="mb-3">
                  <CCol md={6}>
                    <CInputGroup>
                      <CInputGroup>
                        <CFormInput
                          placeholder="Search inspections..."
                          value={searchTerm}
                          onChange={(e) => setSearchTerm(e.target.value)}
                        />
                        <CButton color="outline-secondary" type="button">
                          <FontAwesomeIcon icon={faSearch} />
                        </CButton>
                      </CInputGroup>
                    </CInputGroup>
                  </CCol>
                  <CCol md={6} className="d-flex justify-content-end align-items-center">
                    <div className="d-flex align-items-center">
                      <span className="me-2 small">Sort by:</span>
                      <CFormSelect
                        size="sm"
                        value={sortBy}
                        onChange={(e) => setSortBy(e.target.value)}
                        style={{ width: 'auto' }}
                        className="me-2"
                      >
                        <option value="scheduledDate">Scheduled Date</option>
                        <option value="title">Title</option>
                        <option value="priority">Priority</option>
                        <option value="status">Status</option>
                        <option value="createdAt">Created Date</option>
                      </CFormSelect>
                      <CButtonGroup>
                        <CButton
                          color="outline-secondary"
                          size="sm"
                          active={!sortDescending}
                          onClick={() => setSortDescending(false)}
                        >
                          ASC
                        </CButton>
                        <CButton
                          color="outline-secondary"
                          size="sm"
                          active={sortDescending}
                          onClick={() => setSortDescending(true)}
                        >
                          DESC
                        </CButton>
                      </CButtonGroup>
                    </div>
                  </CCol>
                </CRow>

                {/* Inspections Table */}
                <CTabContent>
                  <CTabPane visible={true}>
                    {isLoading ? (
                      <div className="text-center py-4">
                        <CSpinner color="primary" />
                        <div className="mt-2">Loading your inspections...</div>
                      </div>
                    ) : inspections.length > 0 ? (
                      <CTable hover responsive>
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell scope="col">Inspection</CTableHeaderCell>
                            <CTableHeaderCell scope="col">Status</CTableHeaderCell>
                            <CTableHeaderCell scope="col">Priority</CTableHeaderCell>
                            <CTableHeaderCell scope="col">Scheduled</CTableHeaderCell>
                            <CTableHeaderCell scope="col">Department</CTableHeaderCell>
                            <CTableHeaderCell scope="col">Progress</CTableHeaderCell>
                            <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {inspections.map((inspection: InspectionDto) => (
                            <CTableRow key={inspection.id}>
                              <CTableDataCell>
                                <div>
                                  <div className="fw-semibold">{inspection.title}</div>
                                  <small className="text-medium-emphasis">
                                    #{inspection.inspectionNumber} • {inspection.typeName}
                                  </small>
                                  {inspection.isOverdue && (
                                    <CBadge color="danger" className="ms-1">
                                      <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                                      Overdue
                                    </CBadge>
                                  )}
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                {getStatusBadge(inspection.status)}
                              </CTableDataCell>
                              <CTableDataCell>
                                {getPriorityBadge(inspection.priority)}
                              </CTableDataCell>
                              <CTableDataCell>
                                <div>
                                  <FontAwesomeIcon icon={faCalendarAlt} className="me-1 text-muted" />
                                  {format(new Date(inspection.scheduledDate), 'MMM dd, yyyy')}
                                </div>
                                <small className="text-muted">
                                  {format(new Date(inspection.scheduledDate), 'HH:mm')}
                                </small>
                                {inspection.status === InspectionStatus.Completed && inspection.completedDate && (
                                  <div>
                                    <small className="text-success">
                                      Completed {formatDistanceToNow(new Date(inspection.completedDate))} ago
                                    </small>
                                  </div>
                                )}
                              </CTableDataCell>
                              <CTableDataCell>
                                <div>
                                  <FontAwesomeIcon icon={faBuilding} className="me-1 text-muted" />
                                  {inspection.departmentName}
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                <div className="d-flex align-items-center">
                                  <div className="flex-grow-1 me-2">
                                    <CProgress height={4}>
                                      <CProgressBar 
                                        value={(inspection.completedItemsCount / inspection.itemsCount) * 100}
                                        color="primary"
                                      />
                                    </CProgress>
                                  </div>
                                  <small className="text-muted">
                                    {inspection.completedItemsCount}/{inspection.itemsCount}
                                  </small>
                                </div>
                                {inspection.criticalFindingsCount > 0 && (
                                  <small className="text-danger">
                                    <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                                    {inspection.criticalFindingsCount} critical
                                  </small>
                                )}
                              </CTableDataCell>
                              <CTableDataCell>
                                <CDropdown>
                                  <CDropdownToggle color="light" variant="outline" size="sm">
                                    Actions
                                  </CDropdownToggle>
                                  <CDropdownMenu>
                                    <CDropdownItem onClick={() => handleViewInspection(inspection.id)}>
                                      <FontAwesomeIcon icon={faEye} className="me-2" />
                                      View Details
                                    </CDropdownItem>
                                    {inspection.canEdit && (
                                      <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                                        <CDropdownItem onClick={() => handleEditInspection(inspection.id)}>
                                          <FontAwesomeIcon icon={faEdit} className="me-2" />
                                          Edit
                                        </CDropdownItem>
                                      </PermissionGuard>
                                    )}
                                    {inspection.canStart && (
                                      <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                                        <CDropdownItem>
                                          <FontAwesomeIcon icon={faPlay} className="me-2" />
                                          Start Inspection
                                        </CDropdownItem>
                                      </PermissionGuard>
                                    )}
                                    {inspection.canComplete && (
                                      <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                                        <CDropdownItem>
                                          <FontAwesomeIcon icon={faCheck} className="me-2" />
                                          Complete
                                        </CDropdownItem>
                                      </PermissionGuard>
                                    )}
                                  </CDropdownMenu>
                                </CDropdown>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    ) : (
                      <div className="text-center py-4">
                        <FontAwesomeIcon icon={faClipboardCheck} size="3x" className="text-muted mb-3" />
                        <h5>No inspections found</h5>
                        <p className="text-muted">
                          {activeTab === 'all' 
                            ? "You don't have any inspections yet. Create your first inspection to get started."
                            : `No ${activeTab} inspections found.`
                          }
                        </p>
                        {activeTab === 'all' && (
                          <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Create}>
                            <CButton color="primary" onClick={handleCreateInspection}>
                              <FontAwesomeIcon icon={faPlus} className="me-1" />
                              Create Your First Inspection
                            </CButton>
                          </PermissionGuard>
                        )}
                      </div>
                    )}
                  </CTabPane>
                </CTabContent>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      </DemoModeWrapper>
    </PermissionGuard>
  );
};