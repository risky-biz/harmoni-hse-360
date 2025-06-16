import React, { useState, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CContainer,
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
  CButtonGroup
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
  faFileAlt,
  faUsers,
  faTasks
} from '@fortawesome/free-solid-svg-icons';
import { useWorkPermitApi } from '../../features/work-permits/workPermitApi';
import type { WorkPermitDto } from '../../types/workPermit';
import { PermissionGuard } from '../../components/auth';
import { PermissionType, ModuleType } from '../../types/permissions';

type WorkPermitTab = 'all' | 'submitted' | 'assigned' | 'supervising';

interface TabStats {
  all: number;
  submitted: number;
  assigned: number;
  supervising: number;
}

const MyWorkPermits: React.FC = () => {
  const [activeTab, setActiveTab] = useState<WorkPermitTab>('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [stats, setStats] = useState<TabStats>({
    all: 0,
    submitted: 0,
    assigned: 0,
    supervising: 0
  });

  const {
    data: workPermitsData,
    isLoading,
    error,
    refetch
  } = useWorkPermitApi.useGetMyWorkPermitsQuery({
    pageNumber: currentPage,
    pageSize: 20,
    searchTerm: searchTerm || undefined,
    filterType: activeTab === 'all' ? undefined : activeTab
  });

  // Calculate stats from data
  useEffect(() => {
    if (workPermitsData?.items) {
      const permits = workPermitsData.items;
      const newStats: TabStats = {
        all: permits.length,
        submitted: permits.filter(p => p.requestedById === 1).length, // Current user submitted
        assigned: permits.filter(p => p.workSupervisor === 'Current User' || p.safetyOfficer === 'Current User').length, // User is assigned
        supervising: permits.filter(p => p.workSupervisor === 'Current User').length // User is supervising
      };
      setStats(newStats);
    }
  }, [workPermitsData]);

  const getStatusBadge = (status: string) => {
    const statusConfig = {
      Draft: { color: 'secondary', icon: faFileAlt },
      PendingApproval: { color: 'warning', icon: faClock },
      Approved: { color: 'success', icon: faCheck },
      Rejected: { color: 'danger', icon: faExclamationTriangle },
      InProgress: { color: 'info', icon: faTasks },
      Completed: { color: 'success', icon: faCheck },
      Cancelled: { color: 'dark', icon: faExclamationTriangle },
      Expired: { color: 'danger', icon: faExclamationTriangle }
    };

    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig.Draft;

    return (
      <CBadge color={config.color} className="d-flex align-items-center gap-1">
        <FontAwesomeIcon icon={config.icon} size="sm" />
        {status}
      </CBadge>
    );
  };

  const getPriorityBadge = (priority: string) => {
    const priorityColors = {
      Low: 'info',
      Medium: 'warning',
      High: 'danger',
      Critical: 'danger'
    };

    return (
      <CBadge color={priorityColors[priority as keyof typeof priorityColors] || 'secondary'}>
        {priority}
      </CBadge>
    );
  };

  const getRiskLevelBadge = (riskLevel: string) => {
    const riskColors = {
      Low: 'success',
      Medium: 'warning',
      High: 'danger',
      Critical: 'danger'
    };

    return (
      <CBadge color={riskColors[riskLevel as keyof typeof riskColors] || 'secondary'}>
        {riskLevel}
      </CBadge>
    );
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const getMyRole = (permit: WorkPermitDto): string => {
    const roles = [];
    // In a real app, you'd check against current user ID/name
    if (permit.requestedById === 1) roles.push('Requester');
    if (permit.workSupervisor === 'Current User') roles.push('Supervisor');
    if (permit.safetyOfficer === 'Current User') roles.push('Safety Officer');
    return roles.join(', ') || 'Observer';
  };

  const canEdit = (permit: WorkPermitDto): boolean => {
    return (permit.status === 'Draft' || permit.status === 'Submitted') && (permit.requestedById === 1); // Current user is requester
  };

  const canApprove = (permit: WorkPermitDto): boolean => {
    return permit.status === 'PendingApproval' && (permit.workSupervisor === 'Current User' || permit.safetyOfficer === 'Current User');
  };

  if (error) {
    return (
      <CContainer fluid>
        <CAlert color="danger">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
          Error loading work permits: {error.toString()}
        </CAlert>
      </CContainer>
    );
  }

  return (
    <CContainer fluid>
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h2 className="text-primary mb-1">
                <FontAwesomeIcon icon={faUsers} className="me-2" />
                My Work Permits
              </h2>
              <p className="text-muted mb-0">
                Manage work permits you've submitted, are assigned to, or supervising
              </p>
            </div>
            <PermissionGuard module={ModuleType.WorkPermitManagement} permission={PermissionType.Create}>
              <CButton
                color="primary"
                href="#/work-permits/create"
                className="d-flex align-items-center gap-2"
              >
                <FontAwesomeIcon icon={faPlus} />
                New Work Permit
              </CButton>
            </PermissionGuard>
          </div>
        </CCol>
      </CRow>

      {/* Search and Filter */}
      <CRow className="mb-4">
        <CCol md={6}>
          <CInputGroup>
            <CFormInput
              placeholder="Search work permits..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
            <CButton color="outline-secondary" variant="outline">
              <FontAwesomeIcon icon={faSearch} />
            </CButton>
          </CInputGroup>
        </CCol>
        <CCol md={6} className="d-flex justify-content-end">
          <CButton color="outline-secondary" className="me-2">
            <FontAwesomeIcon icon={faFilter} className="me-2" />
            Filters
          </CButton>
        </CCol>
      </CRow>

      {/* Tabs */}
      <CRow>
        <CCol>
          <CCard>
            <CCardHeader>
              <CNav variant="tabs" className="card-header-tabs">
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'all'}
                    onClick={() => setActiveTab('all')}
                    style={{ cursor: 'pointer' }}
                  >
                    All Permits
                    {stats.all > 0 && (
                      <CBadge color="primary" className="ms-2">
                        {stats.all}
                      </CBadge>
                    )}
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'submitted'}
                    onClick={() => setActiveTab('submitted')}
                    style={{ cursor: 'pointer' }}
                  >
                    Submitted by Me
                    {stats.submitted > 0 && (
                      <CBadge color="info" className="ms-2">
                        {stats.submitted}
                      </CBadge>
                    )}
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'assigned'}
                    onClick={() => setActiveTab('assigned')}
                    style={{ cursor: 'pointer' }}
                  >
                    Assigned to Me
                    {stats.assigned > 0 && (
                      <CBadge color="warning" className="ms-2">
                        {stats.assigned}
                      </CBadge>
                    )}
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'supervising'}
                    onClick={() => setActiveTab('supervising')}
                    style={{ cursor: 'pointer' }}
                  >
                    Supervising
                    {stats.supervising > 0 && (
                      <CBadge color="success" className="ms-2">
                        {stats.supervising}
                      </CBadge>
                    )}
                  </CNavLink>
                </CNavItem>
              </CNav>
            </CCardHeader>

            <CCardBody>
              <CTabContent>
                <CTabPane visible={true}>
                  {isLoading ? (
                    <div className="text-center py-4">
                      <CSpinner color="primary" />
                      <div className="mt-2 text-muted">Loading work permits...</div>
                    </div>
                  ) : workPermitsData?.items?.length === 0 ? (
                    <div className="text-center py-5">
                      <FontAwesomeIcon 
                        icon={faFileAlt} 
                        size="3x" 
                        className="text-muted mb-3" 
                      />
                      <h5 className="text-muted">No Work Permits Found</h5>
                      <p className="text-muted mb-4">
                        {activeTab === 'all' 
                          ? "You don't have any work permits yet."
                          : `No work permits found for '${activeTab}' filter.`
                        }
                      </p>
                      <PermissionGuard module={ModuleType.WorkPermitManagement} permission={PermissionType.Create}>
                        <CButton
                          color="primary"
                          href="#/work-permits/create"
                          className="d-flex align-items-center gap-2 mx-auto"
                        >
                          <FontAwesomeIcon icon={faPlus} />
                          Create Your First Work Permit
                        </CButton>
                      </PermissionGuard>
                    </div>
                  ) : (
                    <CTable hover responsive>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Permit #</CTableHeaderCell>
                          <CTableHeaderCell>Title</CTableHeaderCell>
                          <CTableHeaderCell>Type</CTableHeaderCell>
                          <CTableHeaderCell>Status</CTableHeaderCell>
                          <CTableHeaderCell>Priority</CTableHeaderCell>
                          <CTableHeaderCell>Risk Level</CTableHeaderCell>
                          <CTableHeaderCell>My Role</CTableHeaderCell>
                          <CTableHeaderCell>Start Date</CTableHeaderCell>
                          <CTableHeaderCell>Location</CTableHeaderCell>
                          <CTableHeaderCell>Actions</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {workPermitsData?.items?.map((permit) => (
                          <CTableRow key={permit.id}>
                            <CTableDataCell>
                              <strong className="text-primary">{permit.permitNumber}</strong>
                            </CTableDataCell>
                            <CTableDataCell>
                              <div>
                                <strong>{permit.title}</strong>
                                {permit.description && (
                                  <div className="text-muted small">
                                    {permit.description.length > 50 
                                      ? `${permit.description.substring(0, 50)}...`
                                      : permit.description
                                    }
                                  </div>
                                )}
                              </div>
                            </CTableDataCell>
                            <CTableDataCell>
                              <CBadge color="info">{permit.type}</CBadge>
                            </CTableDataCell>
                            <CTableDataCell>
                              {getStatusBadge(permit.status)}
                            </CTableDataCell>
                            <CTableDataCell>
                              {getPriorityBadge(permit.priority)}
                            </CTableDataCell>
                            <CTableDataCell>
                              {getRiskLevelBadge(permit.riskLevel)}
                            </CTableDataCell>
                            <CTableDataCell>
                              <small className="text-muted">{getMyRole(permit)}</small>
                            </CTableDataCell>
                            <CTableDataCell>
                              <div>
                                {formatDate(permit.plannedStartDate)}
                              </div>
                            </CTableDataCell>
                            <CTableDataCell>
                              <small className="text-muted">{permit.workLocation}</small>
                            </CTableDataCell>
                            <CTableDataCell>
                              <CButtonGroup size="sm">
                                <CButton
                                  color="outline-primary"
                                  size="sm"
                                  href={`#/work-permits/${permit.id}`}
                                  title="View Details"
                                >
                                  <FontAwesomeIcon icon={faEye} />
                                </CButton>
                                {canEdit(permit) && (
                                  <PermissionGuard module={ModuleType.WorkPermitManagement} permission={PermissionType.Update}>
                                    <CButton
                                      color="outline-secondary"
                                      size="sm"
                                      href={`#/work-permits/${permit.id}/edit`}
                                      title="Edit"
                                    >
                                      <FontAwesomeIcon icon={faEdit} />
                                    </CButton>
                                  </PermissionGuard>
                                )}
                                {canApprove(permit) && (
                                  <PermissionGuard module={ModuleType.WorkPermitManagement} permission={PermissionType.Approve}>
                                    <CButton
                                      color="outline-success"
                                      size="sm"
                                      href={`#/work-permits/${permit.id}/approve`}
                                      title="Approve"
                                    >
                                      <FontAwesomeIcon icon={faCheck} />
                                    </CButton>
                                  </PermissionGuard>
                                )}
                              </CButtonGroup>
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                  )}
                </CTabPane>
              </CTabContent>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </CContainer>
  );
};

export default MyWorkPermits;