import React, { useState, useEffect, useRef } from 'react';
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
import CIcon from '@coreui/icons-react';
import {
  cilWarning,
  cilShieldAlt,
  cilTask,
  cilClock,
  cilTrendUp,
  cilPeople,
  cilLocationPin,
  cilChartLine,
  cilReload,
  cilOptions
} from '@coreui/icons';

import { useGetIncidentDashboardQuery } from '../../features/incidents/incidentApi';
import { useSignalR } from '../../hooks/useSignalR';
import {
  StatsCard,
  ChartCard,
  ProgressCard,
  RecentItemsList,
  DonutChart,
  BarChart,
  LineChart
} from '../../components/dashboard';
import { formatDistanceToNow } from 'date-fns';

const IncidentDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [timeRange, setTimeRange] = useState<string>('all');
  const [department, setDepartment] = useState<string>('');
  const [autoRefreshInterval, setAutoRefreshInterval] = useState<number>(0); // 0 = disabled
  const [lastRefreshTime, setLastRefreshTime] = useState<Date>(new Date());
  const autoRefreshIntervalRef = useRef<NodeJS.Timeout | null>(null);

  // Initialize SignalR
  const { connectionState } = useSignalR();

  // Calculate date range based on selection
  const getDateRange = () => {
    const now = new Date();
    switch (timeRange) {
      case '7d':
        return {
          fromDate: new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000).toISOString(),
          toDate: now.toISOString()
        };
      case '30d':
        return {
          fromDate: new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000).toISOString(),
          toDate: now.toISOString()
        };
      case '90d':
        return {
          fromDate: new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000).toISOString(),
          toDate: now.toISOString()
        };
      default:
        return {};
    }
  };

  const { data: dashboardData, isLoading, error, refetch, dataUpdatedAt } = useGetIncidentDashboardQuery({
    ...getDateRange(),
    department: department || undefined,
    includeResolved: true
  });

  // Update last refresh time when data changes
  useEffect(() => {
    if (dataUpdatedAt) {
      setLastRefreshTime(new Date(dataUpdatedAt));
    }
  }, [dataUpdatedAt]);

  // Handle auto-refresh
  useEffect(() => {
    if (autoRefreshIntervalRef.current) {
      clearInterval(autoRefreshIntervalRef.current);
    }

    if (autoRefreshInterval > 0) {
      autoRefreshIntervalRef.current = setInterval(() => {
        refetch();
      }, autoRefreshInterval * 1000);
    }

    return () => {
      if (autoRefreshIntervalRef.current) {
        clearInterval(autoRefreshIntervalRef.current);
      }
    };
  }, [autoRefreshInterval, refetch]);

  // Helper functions for data transformation
  const getStatusColor = (status: string): string => {
    const colors: Record<string, string> = {
      'Reported': 'primary',
      'UnderInvestigation': 'warning',
      'AwaitingAction': 'danger',
      'Resolved': 'success',
      'Closed': 'secondary'
    };
    return colors[status] || 'secondary';
  };

  const getSeverityColor = (severity: string): string => {
    const colors: Record<string, string> = {
      'Minor': 'success',
      'Moderate': 'warning',
      'Serious': 'danger',
      'Critical': 'dark'
    };
    return colors[severity] || 'secondary';
  };

  if (error) {
    return (
      <div className="mb-4">
        <h1>Incident Management Dashboard</h1>
        <CAlert color="danger" className="mt-3">
          <strong>Error loading dashboard data.</strong> Please try refreshing the page.
          <CButton color="danger" variant="outline" size="sm" className="ms-2" onClick={() => refetch()}>
            Retry
          </CButton>
        </CAlert>
      </div>
    );
  }

  return (
    <div className="dashboard-container">
      {/* Header */}
      <div className="mb-4 dashboard-header">
        <div className="d-flex justify-content-between align-items-start flex-wrap gap-3">
          <div>
            <h1 className="h2 h1-md">Incident Management Dashboard</h1>
            <p className="text-medium-emphasis mb-0 d-none d-md-block">
              Comprehensive incident analytics and management overview
            </p>
          </div>
          <CButton 
            color="primary" 
            onClick={() => navigate('/incidents/create')}
            className="btn-block-mobile"
          >
            <CIcon icon={cilWarning} className="me-2" />
            <span className="d-none d-sm-inline">Report Incident</span>
            <span className="d-sm-none">Report</span>
          </CButton>
        </div>
      </div>

      {/* Filters */}
      <CCard className="mb-4 dashboard-filters">
        <CCardBody className="py-3">
          <CRow className="align-items-center filter-controls">
            <CCol lg={5} className="mb-3 mb-lg-0">
              <div className="d-flex align-items-center flex-column flex-sm-row gap-2">
                <label className="form-label mb-0 fw-semibold text-nowrap d-none d-sm-block">Time Range:</label>
                <CButtonGroup size="sm" className="w-100 w-sm-auto">
                  <CButton
                    color={timeRange === 'all' ? 'primary' : 'outline-primary'}
                    onClick={() => setTimeRange('all')}
                  >
                    All
                  </CButton>
                  <CButton
                    color={timeRange === '7d' ? 'primary' : 'outline-primary'}
                    onClick={() => setTimeRange('7d')}
                  >
                    7D
                  </CButton>
                  <CButton
                    color={timeRange === '30d' ? 'primary' : 'outline-primary'}
                    onClick={() => setTimeRange('30d')}
                  >
                    30D
                  </CButton>
                  <CButton
                    color={timeRange === '90d' ? 'primary' : 'outline-primary'}
                    onClick={() => setTimeRange('90d')}
                  >
                    90D
                  </CButton>
                </CButtonGroup>
              </div>
            </CCol>
            <CCol lg={3} className="mb-3 mb-lg-0">
              <CFormSelect
                value={department}
                onChange={(e) => setDepartment(e.target.value)}
                size="sm"
              >
                <option value="">All Departments</option>
                <option value="Operations">Operations</option>
                <option value="Maintenance">Maintenance</option>
                <option value="Security">Security</option>
                <option value="Administration">Administration</option>
                <option value="Facilities">Facilities</option>
              </CFormSelect>
            </CCol>
            <CCol lg={4} className="text-end refresh-controls">
              <div className="d-flex align-items-center justify-content-end gap-2 flex-wrap">
                <CButtonToolbar className="gap-2">
                  <CButton
                    color="primary"
                    variant="outline"
                    size="sm"
                    onClick={() => refetch()}
                    disabled={isLoading}
                  >
                    <CIcon icon={cilReload} className="me-1" />
                    <span className="d-none d-sm-inline">{isLoading ? 'Refreshing...' : 'Refresh'}</span>
                    <span className="d-sm-none">{isLoading ? '...' : 'Refresh'}</span>
                  </CButton>
                  <CDropdown variant="btn-group" size="sm">
                    <CDropdownToggle color="secondary" variant="outline">
                      <CIcon icon={cilOptions} className="me-1 d-none d-sm-inline" />
                      <span className="d-none d-md-inline">Auto-refresh: </span>
                      {autoRefreshInterval === 0 ? 'Off' : `${autoRefreshInterval}s`}
                    </CDropdownToggle>
                    <CDropdownMenu>
                      <CDropdownItem onClick={() => setAutoRefreshInterval(0)}>
                        Disabled
                      </CDropdownItem>
                      <CDropdownItem onClick={() => setAutoRefreshInterval(30)}>
                        Every 30 seconds
                      </CDropdownItem>
                      <CDropdownItem onClick={() => setAutoRefreshInterval(60)}>
                        Every minute
                      </CDropdownItem>
                      <CDropdownItem onClick={() => setAutoRefreshInterval(300)}>
                        Every 5 minutes
                      </CDropdownItem>
                    </CDropdownMenu>
                  </CDropdown>
                </CButtonToolbar>
                <div className="text-medium-emphasis small text-end flex-shrink-0">
                  <div className="text-truncate-mobile">Last: {formatDistanceToNow(lastRefreshTime, { addSuffix: true })}</div>
                  {connectionState === 1 && (
                    <div className="text-success d-none d-sm-block">
                      <small>● Live updates</small>
                    </div>
                  )}
                </div>
              </div>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {/* Overall Statistics */}
      <CRow className="mb-4 stats-row">
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Total Incidents"
            value={dashboardData?.overallStats.totalIncidents || 0}
            icon={cilWarning}
            color="primary"
            isLoading={isLoading}
            onClick={() => navigate('/incidents')}
            trend={dashboardData ? {
              value: dashboardData.overallStats.monthOverMonthChange,
              isPositive: dashboardData.overallStats.monthOverMonthChange >= 0,
              label: 'vs last month'
            } : undefined}
          />
        </CCol>
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Open Incidents"
            value={dashboardData?.overallStats.openIncidents || 0}
            icon={cilTask}
            color="warning"
            isLoading={isLoading}
            onClick={() => navigate('/incidents?status=Reported,UnderInvestigation,AwaitingAction')}
          />
        </CCol>
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Critical Incidents"
            value={dashboardData?.overallStats.criticalIncidents || 0}
            icon={cilShieldAlt}
            color="danger"
            isLoading={isLoading}
            onClick={() => navigate('/incidents?severity=Critical')}
          />
        </CCol>
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Overdue Items"
            value={dashboardData?.overallStats.overdueIncidents || 0}
            icon={cilClock}
            color="dark"
            isLoading={isLoading}
          />
        </CCol>
      </CRow>

      {/* Charts Row 1 */}
      <CRow className="mb-4">
        <CCol lg={4}>
          <ChartCard
            title="Status Distribution"
            subtitle="Current incident status breakdown"
            isLoading={isLoading}
            height="350px"
          >
            {dashboardData && (
              <DonutChart
                data={dashboardData.statusStats.map(stat => ({
                  label: stat.status,
                  value: stat.count,
                  color: `var(--cui-${stat.color})`
                }))}
                size={280}
                strokeWidth={25}
              />
            )}
          </ChartCard>
        </CCol>
        <CCol lg={4}>
          <ChartCard
            title="Severity Analysis"
            subtitle="Incident severity distribution"
            isLoading={isLoading}
            height="350px"
          >
            {dashboardData && (
              <BarChart
                data={dashboardData.severityStats.map(stat => ({
                  label: stat.severity,
                  value: stat.count,
                  color: `var(--cui-${stat.color})`
                }))}
                height={300}
                horizontal={true}
                showValues={true}
              />
            )}
          </ChartCard>
        </CCol>
        <CCol lg={4}>
          <ChartCard
            title="Monthly Trend"
            subtitle="Incident trend over time"
            isLoading={isLoading}
            height="350px"
          >
            {dashboardData && (
              <LineChart
                data={dashboardData.trendData.slice(-6).map(trend => ({
                  label: trend.periodLabel,
                  value: trend.totalIncidents
                }))}
                height={300}
                showDots={true}
                showGrid={true}
              />
            )}
          </ChartCard>
        </CCol>
      </CRow>

      {/* Performance Metrics */}
      <CRow className="mb-4">
        <CCol lg={3} md={6}>
          <ProgressCard
            title="SLA Compliance"
            value={dashboardData?.responseTimeStats.incidentsWithinSLA || 0}
            total={dashboardData?.responseTimeStats.totalIncidentsWithResponse || 0}
            percentage={dashboardData?.responseTimeStats.slaCompliancePercentage || 0}
            color="success"
            description="24hr response target"
            isLoading={isLoading}
          />
        </CCol>
        <CCol lg={3} md={6}>
          <ProgressCard
            title="Resolution Rate"
            value={dashboardData?.resolutionTimeStats.resolvedIncidents || 0}
            total={(dashboardData?.resolutionTimeStats.resolvedIncidents || 0) + (dashboardData?.resolutionTimeStats.pendingResolution || 0)}
            percentage={dashboardData?.resolutionTimeStats.resolutionRate || 0}
            color="primary"
            description="Overall resolution"
            isLoading={isLoading}
          />
        </CCol>
        <CCol lg={3} md={6}>
          <ProgressCard
            title="Corrective Actions"
            value={Math.round((dashboardData?.performanceMetrics.correctiveActionCompletionRate || 0))}
            total={100}
            percentage={dashboardData?.performanceMetrics.correctiveActionCompletionRate || 0}
            color="info"
            description="Completion rate"
            isLoading={isLoading}
          />
        </CCol>
        <CCol lg={3} md={6}>
          <ProgressCard
            title="Employee Engagement"
            value={Math.round((dashboardData?.performanceMetrics.employeeReportingEngagement || 0))}
            total={100}
            percentage={dashboardData?.performanceMetrics.employeeReportingEngagement || 0}
            color="warning"
            description="Reporting participation"
            isLoading={isLoading}
          />
        </CCol>
      </CRow>

      {/* Charts Row 2 */}
      <CRow className="mb-4">
        <CCol lg={6}>
          <ChartCard
            title="Category Breakdown"
            subtitle="Incidents by category type"
            isLoading={isLoading}
            height="400px"
          >
            {dashboardData && (
              <BarChart
                data={dashboardData.categoryStats.slice(0, 8).map(stat => ({
                  label: stat.category,
                  value: stat.count
                }))}
                height={350}
                showValues={true}
              />
            )}
          </ChartCard>
        </CCol>
        <CCol lg={6}>
          <ChartCard
            title="Department Performance"
            subtitle="Incidents and compliance by department"
            isLoading={isLoading}
            height="400px"
          >
            {dashboardData && (
              <div className="h-100 overflow-auto department-stats">
                {dashboardData.departmentStats.map((dept, index) => (
                  <div key={index} className="mb-3 p-3 border rounded department-item">
                    <div className="d-flex justify-content-between align-items-center mb-2 flex-wrap gap-2">
                      <h6 className="mb-0">{dept.department}</h6>
                      <CBadge 
                        color={dept.complianceScore >= 80 ? 'success' : dept.complianceScore >= 60 ? 'warning' : 'danger'}
                      >
                        {dept.complianceScore.toFixed(1)}% <span className="d-none d-sm-inline">Compliance</span>
                      </CBadge>
                    </div>
                    <div className="small text-medium-emphasis">
                      <div>Total: <span className="fw-semibold">{dept.incidentCount}</span></div>
                      <div>Critical: <span className="fw-semibold text-danger">{dept.criticalCount}</span></div>
                      <div>Avg Res: <span className="fw-semibold">{dept.averageResolutionDays.toFixed(1)} days</span></div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </ChartCard>
        </CCol>
      </CRow>

      {/* Recent Incidents and Response Times */}
      <CRow className="mb-4">
        <CCol lg={8}>
          <RecentItemsList
            title="Recent Incidents"
            items={dashboardData?.recentIncidents.map(incident => ({
              id: incident.id,
              title: incident.title,
              subtitle: `${incident.location} • Reported by ${incident.reporterName}`,
              status: incident.severity,
              statusColor: getSeverityColor(incident.severity),
              timestamp: incident.createdAt,
              isOverdue: incident.isOverdue,
              onClick: () => navigate(`/incidents/${incident.id}`)
            })) || []}
            isLoading={isLoading}
            maxItems={8}
            showAllLink={{
              text: 'View All Incidents',
              onClick: () => navigate('/incidents')
            }}
          />
        </CCol>
        <CCol lg={4}>
          <CCard className="h-100">
            <CCardHeader>
              <h5 className="card-title mb-0">Response Time Analytics</h5>
            </CCardHeader>
            <CCardBody>
              {isLoading ? (
                <div className="d-flex justify-content-center py-4">
                  <CSpinner color="primary" />
                </div>
              ) : dashboardData ? (
                <div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between">
                      <span className="text-medium-emphasis">Average Response Time</span>
                      <span className="fw-semibold">
                        {dashboardData.responseTimeStats.averageResponseTimeHours.toFixed(1)}h
                      </span>
                    </div>
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between">
                      <span className="text-medium-emphasis">Median Response Time</span>
                      <span className="fw-semibold">
                        {dashboardData.responseTimeStats.medianResponseTimeHours.toFixed(1)}h
                      </span>
                    </div>
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between">
                      <span className="text-medium-emphasis">Critical Incidents Avg</span>
                      <span className="fw-semibold text-danger">
                        {dashboardData.responseTimeStats.criticalIncidentsAvgResponseHours.toFixed(1)}h
                      </span>
                    </div>
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between">
                      <span className="text-medium-emphasis">Average Resolution Time</span>
                      <span className="fw-semibold">
                        {dashboardData.resolutionTimeStats.averageResolutionTimeDays.toFixed(1)} days
                      </span>
                    </div>
                  </div>
                  <div className="mt-4">
                    <div className="small text-medium-emphasis mb-2">
                      Quick Actions
                    </div>
                    <div className="d-grid gap-2">
                      <CButton 
                        color="primary" 
                        variant="outline" 
                        size="sm"
                        onClick={() => navigate('/incidents?status=AwaitingAction')}
                      >
                        <CIcon icon={cilTask} className="me-1" />
                        View Pending Actions
                      </CButton>
                      <CButton 
                        color="warning" 
                        variant="outline" 
                        size="sm"
                        onClick={() => navigate('/incidents?severity=Critical')}
                      >
                        <CIcon icon={cilWarning} className="me-1" />
                        Critical Incidents
                      </CButton>
                    </div>
                  </div>
                </div>
              ) : null}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </div>
  );
};

export default IncidentDashboard;