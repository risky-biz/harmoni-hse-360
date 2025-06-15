import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CButtonGroup,
  CFormSelect,
  CAlert,
  CSpinner,
  CBadge,
  CButtonToolbar,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faArrowRotateRight,
  faCog,
  faFileContract,
  faClipboardCheck,
  faExclamationTriangle,
  faClock,
  faCheckCircle,
  faTimesCircle
} from '@fortawesome/free-solid-svg-icons';

import { useGetDashboardQuery } from '../../features/work-permits/workPermitApi';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import {
  StatsCard,
  ChartCard,
  ProgressCard,
  RecentItemsList,
  DonutChart,
  BarChart,
  LineChart
} from '../../components/dashboard';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { formatDistanceToNow } from 'date-fns';
import { WORK_PERMIT_STATUSES, WORK_PERMIT_PRIORITIES, RISK_LEVELS } from '../../types/workPermit';

const WorkPermitDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { isDemo } = useApplicationMode();
  const [timeRange, setTimeRange] = useState<string>('all');

  const {
    data: dashboardData,
    error,
    isLoading,
    refetch
  } = useGetDashboardQuery();

  const handleCreatePermit = () => {
    navigate('/work-permits/create');
  };

  const handleViewAllPermits = () => {
    navigate('/work-permits');
  };

  const handleViewMyPermits = () => {
    navigate('/work-permits/my-permits');
  };

  const handleViewPendingApprovals = () => {
    navigate('/work-permits/pending-approval');
  };

  const getStatusColor = (status: string) => {
    const statusConfig = WORK_PERMIT_STATUSES.find(s => s.value === status);
    return statusConfig?.color || 'secondary';
  };

  const getPriorityColor = (priority: string) => {
    const priorityConfig = WORK_PERMIT_PRIORITIES.find(p => p.value === priority);
    return priorityConfig?.color || 'secondary';
  };

  const getRiskLevelColor = (riskLevel: string) => {
    const riskConfig = RISK_LEVELS.find(r => r.value === riskLevel);
    return riskConfig?.color || 'secondary';
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
        Failed to load work permit dashboard data. Please try again.
        <CButton 
          color="danger" 
          variant="outline" 
          size="sm" 
          className="ms-2"
          onClick={() => refetch()}
        >
          <FontAwesomeIcon icon={faArrowRotateRight} className="me-1" />
          Retry
        </CButton>
      </CAlert>
    );
  }

  const permitsByTypeData = dashboardData?.permitsByType?.map(item => ({
    label: item.type,
    value: item.count,
    color: '#' + Math.floor(Math.random()*16777215).toString(16) // Random color for now
  })) || [];

  // Convert API data to format expected by LineChart component
  const monthlyTrendData = dashboardData?.monthlyTrends?.map(trend => ({
    label: trend.month,
    value: trend.totalPermits
  })) || [];

  return (
    <div className="work-permit-dashboard">
      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h1 className="h3 mb-1">Work Permit Management</h1>
              <p className="text-muted mb-0">Monitor and manage work permits across your organization</p>
            </div>
            <CButtonToolbar className="gap-2">
              <PermissionGuard
                module={ModuleType.WorkPermitManagement}
                permission={PermissionType.Create}
              >
                <CButton
                  color="primary"
                  onClick={handleCreatePermit}
                  disabled={isDemo}
                >
                  <FontAwesomeIcon icon={faPlus} className="me-2" />
                  New Work Permit
                </CButton>
              </PermissionGuard>
              
              <CDropdown>
                <CDropdownToggle color="secondary" variant="outline">
                  <FontAwesomeIcon icon={faCog} className="me-2" />
                  Actions
                </CDropdownToggle>
                <CDropdownMenu>
                  <CDropdownItem onClick={handleViewAllPermits}>
                    <FontAwesomeIcon icon={faFileContract} className="me-2" />
                    View All Permits
                  </CDropdownItem>
                  <CDropdownItem onClick={handleViewMyPermits}>
                    <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                    My Work Permits
                  </CDropdownItem>
                  <PermissionGuard
                    module={ModuleType.WorkPermitManagement}
                    permission={PermissionType.Approve}
                  >
                    <CDropdownItem onClick={handleViewPendingApprovals}>
                      <FontAwesomeIcon icon={faClock} className="me-2" />
                      Pending Approvals
                    </CDropdownItem>
                  </PermissionGuard>
                  <CDropdownItem divider={true} />
                  <CDropdownItem onClick={() => refetch()}>
                    <FontAwesomeIcon icon={faArrowRotateRight} className="me-2" />
                    Refresh Data
                  </CDropdownItem>
                </CDropdownMenu>
              </CDropdown>
            </CButtonToolbar>
          </div>
        </CCol>
      </CRow>

      {/* Key Metrics */}
      <CRow className="mb-4">
        <CCol md={3}>
          <StatsCard
            title="Total Permits"
            value={dashboardData?.totalPermits || 0}
            icon={faFileContract}
            color="primary"
            trend={{ value: 0, isPositive: true }}
            onClick={handleViewAllPermits}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Pending Approval"
            value={dashboardData?.pendingApprovalPermits || 0}
            icon={faClock}
            color="warning"
            trend={{ value: 0, isPositive: false }}
            onClick={handleViewPendingApprovals}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="In Progress"
            value={dashboardData?.inProgressPermits || 0}
            icon={faClipboardCheck}
            color="info"
            trend={{ value: 0, isPositive: true }}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Completed"
            value={dashboardData?.completedPermits || 0}
            icon={faCheckCircle}
            color="success"
            trend={{ value: 0, isPositive: true }}
          />
        </CCol>
      </CRow>

      {/* Alert Metrics */}
      <CRow className="mb-4">
        <CCol md={3}>
          <StatsCard
            title="Overdue Permits"
            value={dashboardData?.overduePermits || 0}
            icon={faExclamationTriangle}
            color="danger"
            onClick={() => navigate('/work-permits?filter=overdue')}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="High Risk Permits"
            value={dashboardData?.highRiskPermits || 0}
            icon={faExclamationTriangle}
            color="danger"
            onClick={() => navigate('/work-permits?riskLevel=High,Critical')}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Due Today"
            value={dashboardData?.permitsDueToday || 0}
            icon={faClock}
            color="warning"
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Due This Week"
            value={dashboardData?.permitsDueThisWeek || 0}
            icon={faClock}
            color="info"
          />
        </CCol>
      </CRow>

      {/* Status Overview */}
      <CRow className="mb-4">
        <CCol md={8}>
          <ChartCard title="Permit Status Distribution">
            <CRow>
              <CCol md={6}>
                <ProgressCard
                  title="Draft"
                  value={dashboardData?.draftPermits || 0}
                  total={dashboardData?.totalPermits || 1}
                  color="secondary"
                />
              </CCol>
              <CCol md={6}>
                <ProgressCard
                  title="Pending Approval"
                  value={dashboardData?.pendingApprovalPermits || 0}
                  total={dashboardData?.totalPermits || 1}
                  color="warning"
                />
              </CCol>
              <CCol md={6}>
                <ProgressCard
                  title="Approved"
                  value={dashboardData?.approvedPermits || 0}
                  total={dashboardData?.totalPermits || 1}
                  color="success"
                />
              </CCol>
              <CCol md={6}>
                <ProgressCard
                  title="In Progress"
                  value={dashboardData?.inProgressPermits || 0}
                  total={dashboardData?.totalPermits || 1}
                  color="info"
                />
              </CCol>
              <CCol md={6}>
                <ProgressCard
                  title="Completed"
                  value={dashboardData?.completedPermits || 0}
                  total={dashboardData?.totalPermits || 1}
                  color="success"
                />
              </CCol>
              <CCol md={6}>
                <ProgressCard
                  title="Rejected/Cancelled"
                  value={(dashboardData?.rejectedPermits || 0) + (dashboardData?.cancelledPermits || 0)}
                  total={dashboardData?.totalPermits || 1}
                  color="danger"
                />
              </CCol>
            </CRow>
          </ChartCard>
        </CCol>
        <CCol md={4}>
          <ChartCard title="Permits by Type">
            {permitsByTypeData.length > 0 ? (
              <DonutChart data={permitsByTypeData} />
            ) : (
              <div className="text-center text-muted py-4">
                No permit data available
              </div>
            )}
          </ChartCard>
        </CCol>
      </CRow>

      {/* Charts */}
      <CRow className="mb-4">
        <CCol>
          <ChartCard title="Monthly Permit Trends">
            {monthlyTrendData.length > 0 ? (
              <LineChart data={monthlyTrendData} height={300} />
            ) : (
              <div className="text-center text-muted py-4">
                No trend data available
              </div>
            )}
          </ChartCard>
        </CCol>
      </CRow>

      {/* Recent Activity */}
      <CRow>
        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <div className="d-flex justify-content-between align-items-center">
                <h5 className="mb-0">Recent Work Permits</h5>
                <CButton
                  color="primary"
                  variant="outline"
                  size="sm"
                  onClick={handleViewAllPermits}
                >
                  View All
                </CButton>
              </div>
            </CCardHeader>
            <CCardBody>
              {dashboardData?.recentPermits && dashboardData.recentPermits.length > 0 ? (
                <RecentItemsList
                  items={dashboardData.recentPermits.map(permit => ({
                    id: permit.id,
                    title: permit.title,
                    subtitle: `${permit.typeDisplay} - ${permit.workLocation}`,
                    timestamp: formatDistanceToNow(new Date(permit.createdAt), { addSuffix: true }),
                    status: permit.status,
                    badge: {
                      text: permit.statusDisplay,
                      color: getStatusColor(permit.status)
                    },
                    onClick: () => navigate(`/work-permits/${permit.id}`)
                  }))}
                />
              ) : (
                <div className="text-center text-muted py-4">
                  No recent permits found
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
        
        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <div className="d-flex justify-content-between align-items-center">
                <h5 className="mb-0">High Priority Permits</h5>
                <CButton
                  color="danger"
                  variant="outline"
                  size="sm"
                  onClick={() => navigate('/work-permits?priority=High,Critical')}
                >
                  View All
                </CButton>
              </div>
            </CCardHeader>
            <CCardBody>
              {dashboardData?.highPriorityPermits && dashboardData.highPriorityPermits.length > 0 ? (
                <RecentItemsList
                  items={dashboardData.highPriorityPermits.map(permit => ({
                    id: permit.id,
                    title: permit.title,
                    subtitle: `${permit.typeDisplay} - ${permit.workLocation}`,
                    timestamp: formatDistanceToNow(new Date(permit.plannedStartDate), { addSuffix: true }),
                    status: permit.status,
                    badge: {
                      text: permit.priorityDisplay,
                      color: getPriorityColor(permit.priority)
                    },
                    secondaryBadge: {
                      text: permit.riskLevelDisplay,
                      color: getRiskLevelColor(permit.riskLevel)
                    },
                    onClick: () => navigate(`/work-permits/${permit.id}`)
                  }))}
                />
              ) : (
                <div className="text-center text-muted py-4">
                  No high priority permits found
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </div>
  );
};

export default WorkPermitDashboard;