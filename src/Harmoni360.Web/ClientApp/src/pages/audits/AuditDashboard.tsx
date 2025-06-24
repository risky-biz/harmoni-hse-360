import React, { useState } from 'react';
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
  CDropdownItem,
  CDropdownDivider
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faArrowRotateRight,
  faCog,
  faClipboardList,
  faClipboardCheck,
  faExclamationTriangle,
  faClock,
  faCheckCircle,
  faTimesCircle,
  faSearch,
  faChartBar
} from '@fortawesome/free-solid-svg-icons';

import { useGetAuditDashboardQuery } from '../../features/audits/auditApi';
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
import type { AuditStatus, AuditPriority, RiskLevel, AuditType } from '../../types/audit';

const AuditDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { isDemoMode } = useApplicationMode();
  const [timeRange, setTimeRange] = useState<string>('last30days');

  // For now, we'll fetch dashboard data without date filters to fix the infinite loading issue
  // The backend should handle default date ranges
  const {
    data: dashboardData,
    error,
    isLoading,
    refetch
  } = useGetAuditDashboardQuery({});

  const handleCreateAudit = () => {
    navigate('/audits/create');
  };

  const handleViewAllAudits = () => {
    navigate('/audits');
  };

  const handleViewMyAudits = () => {
    navigate('/audits/my-audits');
  };

  const handleViewPendingAudits = () => {
    navigate('/audits/pending');
  };

  const handleViewOverdueAudits = () => {
    navigate('/audits/overdue');
  };

  const getStatusColor = (status: AuditStatus) => {
    const statusColors: Record<AuditStatus, string> = {
      'Draft': 'secondary',
      'Scheduled': 'info',
      'InProgress': 'warning',
      'Completed': 'success',
      'Overdue': 'danger',
      'Cancelled': 'dark',
      'Archived': 'light',
      'UnderReview': 'primary'
    };
    return statusColors[status] || 'secondary';
  };

  const getPriorityColor = (priority: AuditPriority) => {
    const priorityColors: Record<AuditPriority, string> = {
      'Low': 'success',
      'Medium': 'warning',
      'High': 'danger',
      'Critical': 'danger'
    };
    return priorityColors[priority] || 'secondary';
  };

  const getRiskLevelColor = (riskLevel: RiskLevel) => {
    const riskColors: Record<RiskLevel, string> = {
      'Low': 'success',
      'Medium': 'warning',
      'High': 'danger',
      'Critical': 'danger'
    };
    return riskColors[riskLevel] || 'secondary';
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
        Failed to load audit dashboard data. Please try again.
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

  // RTK Query might still be loading even if isLoading is false
  // This can happen during revalidation or background refetching
  if (!dashboardData && !error) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner color="primary" />
      </div>
    );
  }

  const auditsByTypeData = dashboardData?.auditsByType?.map(item => ({
    label: item.type,
    value: item.count,
    color: `hsl(${Math.floor(Math.random() * 360)}, 70%, 60%)`
  })) || [];

  const monthlyTrendData = dashboardData?.monthlyTrends?.map(trend => ({
    label: trend.month,
    value: trend.completedAudits
  })) || [];

  return (
    <div className="audit-dashboard">
      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h1 className="h3 mb-1">Audit Management</h1>
              <p className="text-muted mb-0">Monitor and manage audits across your organization</p>
            </div>
            <CButtonToolbar className="gap-2">
              <CFormSelect
                value={timeRange}
                onChange={(e) => setTimeRange(e.target.value)}
                style={{ width: 'auto' }}
              >
                <option value="last7days">Last 7 Days</option>
                <option value="last30days">Last 30 Days</option>
                <option value="last90days">Last 90 Days</option>
                <option value="last12months">Last 12 Months</option>
                <option value="all">All Time</option>
              </CFormSelect>
              
              <PermissionGuard
                module={ModuleType.AuditManagement}
                permission={PermissionType.Create}
              >
                <CButton
                  color="primary"
                  onClick={handleCreateAudit}
                  disabled={false}
                >
                  <FontAwesomeIcon icon={faPlus} className="me-2" />
                  New Audit
                </CButton>
              </PermissionGuard>
              
              <CDropdown>
                <CDropdownToggle color="secondary" variant="outline">
                  <FontAwesomeIcon icon={faCog} className="me-2" />
                  Actions
                </CDropdownToggle>
                <CDropdownMenu>
                  <CDropdownItem onClick={handleViewAllAudits}>
                    <FontAwesomeIcon icon={faClipboardList} className="me-2" />
                    View All Audits
                  </CDropdownItem>
                  <CDropdownItem onClick={handleViewMyAudits}>
                    <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                    My Audits
                  </CDropdownItem>
                  <CDropdownItem onClick={handleViewPendingAudits}>
                    <FontAwesomeIcon icon={faClock} className="me-2" />
                    Pending Audits
                  </CDropdownItem>
                  <CDropdownItem onClick={handleViewOverdueAudits}>
                    <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                    Overdue Audits
                  </CDropdownItem>
                  <CDropdownDivider />
                  <CDropdownItem onClick={() => navigate('/audits/reports')}>
                    <FontAwesomeIcon icon={faChartBar} className="me-2" />
                    Reports & Analytics
                  </CDropdownItem>
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
            title="Total Audits"
            value={dashboardData?.totalAudits || 0}
            icon={faClipboardList}
            color="primary"
            trend={{ value: 0, isPositive: true, label: "this month" }}
            onClick={handleViewAllAudits}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Scheduled"
            value={dashboardData?.scheduledAudits || 0}
            icon={faClock}
            color="info"
            trend={{ value: 0, isPositive: true, label: "this month" }}
            onClick={() => navigate('/audits?status=Scheduled')}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="In Progress"
            value={dashboardData?.inProgressAudits || 0}
            icon={faClipboardCheck}
            color="warning"
            trend={{ value: 0, isPositive: true, label: "this month" }}
            onClick={() => navigate('/audits?status=InProgress')}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Completed"
            value={dashboardData?.completedAudits || 0}
            icon={faCheckCircle}
            color="success"
            trend={{ value: 0, isPositive: true, label: "this month" }}
            onClick={() => navigate('/audits?status=Completed')}
          />
        </CCol>
      </CRow>

      {/* Alert Metrics */}
      <CRow className="mb-4">
        <CCol md={3}>
          <StatsCard
            title="Overdue Audits"
            value={dashboardData?.overdueAudits || 0}
            icon={faExclamationTriangle}
            color="danger"
            onClick={handleViewOverdueAudits}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Total Findings"
            value={dashboardData?.totalFindings || 0}
            icon={faSearch}
            color="warning"
            onClick={() => navigate('/audits/findings')}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Open Findings"
            value={dashboardData?.openFindings || 0}
            icon={faExclamationTriangle}
            color="danger"
            onClick={() => navigate('/audits/findings?status=Open')}
          />
        </CCol>
        <CCol md={3}>
          <StatsCard
            title="Critical Findings"
            value={dashboardData?.criticalFindings || 0}
            icon={faTimesCircle}
            color="danger"
            onClick={() => navigate('/audits/findings?severity=Critical')}
          />
        </CCol>
      </CRow>

      {/* Performance Metrics */}
      <CRow className="mb-4">
        <CCol md={4}>
          <CCard>
            <CCardHeader>
              <h5 className="mb-0">Compliance Score</h5>
            </CCardHeader>
            <CCardBody className="text-center">
              <div className="display-4 mb-2" style={{ color: (dashboardData?.completionRate ?? 0) >= 80 ? '#28a745' : (dashboardData?.completionRate ?? 0) >= 60 ? '#ffc107' : '#dc3545' }}>
                {dashboardData?.completionRate ?? 0}%
              </div>
              <p className="text-muted mb-0">Overall Compliance</p>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={4}>
          <CCard>
            <CCardHeader>
              <h5 className="mb-0">Avg Completion Time</h5>
            </CCardHeader>
            <CCardBody className="text-center">
              <div className="display-4 mb-2 text-info">
                {dashboardData?.averageScore || 0}
              </div>
              <p className="text-muted mb-0">Days</p>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={4}>
          <ChartCard title="Audits by Type">
            {auditsByTypeData.length > 0 ? (
              <DonutChart data={auditsByTypeData} />
            ) : (
              <div className="text-center text-muted py-4">
                No audit data available
              </div>
            )}
          </ChartCard>
        </CCol>
      </CRow>

      {/* Charts */}
      <CRow className="mb-4">
        <CCol md={12}>
          <ChartCard title="Monthly Audit Trends">
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
                <h5 className="mb-0">Recent Audits</h5>
                <CButton
                  color="primary"
                  variant="outline"
                  size="sm"
                  onClick={handleViewAllAudits}
                >
                  View All
                </CButton>
              </div>
            </CCardHeader>
            <CCardBody>
              {dashboardData?.recentAudits && dashboardData.recentAudits.length > 0 ? (
                <RecentItemsList
                  items={dashboardData.recentAudits.map(audit => ({
                    id: audit.id.toString(),
                    title: audit.title,
                    subtitle: `${audit.type} - ${audit.auditorName}`,
                    timestamp: formatDistanceToNow(new Date(audit.scheduledDate), { addSuffix: true }),
                    status: audit.status,
                    badge: {
                      text: audit.status,
                      color: getStatusColor(audit.status)
                    },
                    onClick: () => navigate(`/audits/${audit.id}`)
                  }))}
                />
              ) : (
                <div className="text-center text-muted py-4">
                  No recent audits found
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
        
        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <div className="d-flex justify-content-between align-items-center">
                <h5 className="mb-0">High Priority Audits</h5>
                <CButton
                  color="danger"
                  variant="outline"
                  size="sm"
                  onClick={() => navigate('/audits?priority=High')}
                >
                  View All
                </CButton>
              </div>
            </CCardHeader>
            <CCardBody>
              {dashboardData?.highPriorityAudits && dashboardData.highPriorityAudits.length > 0 ? (
                <RecentItemsList
                  items={dashboardData.highPriorityAudits.map(audit => ({
                    id: audit.id.toString(),
                    title: audit.title,
                    subtitle: `${audit.type} - ${audit.auditorName}`,
                    timestamp: formatDistanceToNow(new Date(audit.scheduledDate), { addSuffix: true }),
                    status: audit.status,
                    badge: {
                      text: audit.priority,
                      color: getPriorityColor(audit.priority as AuditPriority)
                    },
                    secondaryBadge: {
                      text: audit.riskLevel,
                      color: getRiskLevelColor(audit.riskLevel as RiskLevel)
                    },
                    onClick: () => navigate(`/audits/${audit.id}`)
                  }))}
                />
              ) : (
                <div className="text-center text-muted py-4">
                  No high priority audits found
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

    </div>
  );
};

export default AuditDashboard;