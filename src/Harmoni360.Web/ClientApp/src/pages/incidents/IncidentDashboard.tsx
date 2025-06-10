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
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faExclamationTriangle,
  faShieldAlt,
  faClipboardCheck,
  faClock,
  faRedo,
  faCog,
  faPlus,
  faArrowRotateRight
} from '@fortawesome/free-solid-svg-icons';
import { HubConnectionState } from '@microsoft/signalr';

import { useGetIncidentDashboardQuery } from '../../features/incidents/incidentApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import { useSignalR } from '../../hooks/useSignalR';
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

const IncidentDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { isDemo } = useApplicationMode();
  const [timeRange, setTimeRange] = useState<string>('all');
  const [department, setDepartment] = useState<string>('');
  const [autoRefreshInterval, setAutoRefreshInterval] = useState<number>(0); // 0 = disabled
  const [lastRefreshTime, setLastRefreshTime] = useState<Date>(new Date());
  const autoRefreshIntervalRef = useRef<NodeJS.Timeout | null>(null);

  // Initialize SignalR
  const { connectionState } = useSignalR();
  
  // Get departments from database
  const { data: departments } = useGetDepartmentsQuery();

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

  // Generate demo data if in demo mode and no real data available
  const generateDemoData = () => {
    if (!isDemo) return null;
    
    const now = new Date();
    const months = [];
    for (let i = 11; i >= 0; i--) {
      const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
      months.push({
        period: date.toISOString().slice(0, 7),
        periodLabel: date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' }),
        totalIncidents: Math.floor(Math.random() * 15) + 5,
        criticalIncidents: Math.floor(Math.random() * 3) + 1,
        resolvedIncidents: Math.floor(Math.random() * 12) + 3
      });
    }
    
    return {
      overallStats: {
        totalIncidents: 127,
        openIncidents: 23,
        criticalIncidents: 8,
        overdueIncidents: 5,
        monthOverMonthChange: 12.5
      },
      statusStats: [
        { status: 'Reported', count: 12, color: 'primary' },
        { status: 'Under Investigation', count: 8, color: 'warning' },
        { status: 'Awaiting Action', count: 3, color: 'danger' },
        { status: 'Resolved', count: 89, color: 'success' },
        { status: 'Closed', count: 15, color: 'secondary' }
      ],
      severityStats: [
        { severity: 'Critical', count: 8, color: 'danger' },
        { severity: 'Serious', count: 15, color: 'warning' },
        { severity: 'Moderate', count: 42, color: 'info' },
        { severity: 'Minor', count: 62, color: 'success' }
      ],
      categoryStats: [
        { category: 'Slip/Trip/Fall', count: 28 },
        { category: 'Equipment Malfunction', count: 22 },
        { category: 'Chemical Exposure', count: 15 },
        { category: 'Fire/Explosion', count: 8 },
        { category: 'Medical Emergency', count: 18 },
        { category: 'Security Breach', count: 12 },
        { category: 'Vehicle Accident', count: 9 },
        { category: 'Near Miss', count: 15 }
      ],
      departmentStats: [
        { department: 'Operations', incidentCount: 35, criticalCount: 3, averageResolutionDays: 4.2, complianceScore: 85.5 },
        { department: 'Maintenance', incidentCount: 28, criticalCount: 2, averageResolutionDays: 3.8, complianceScore: 92.1 },
        { department: 'Security', incidentCount: 22, criticalCount: 1, averageResolutionDays: 2.1, complianceScore: 95.8 },
        { department: 'Administration', incidentCount: 18, criticalCount: 1, averageResolutionDays: 5.5, complianceScore: 78.3 },
        { department: 'Facilities', incidentCount: 24, criticalCount: 1, averageResolutionDays: 4.0, complianceScore: 88.7 }
      ],
      trendData: months,
      responseTimeStats: {
        averageResponseTimeHours: 8.5,
        medianResponseTimeHours: 6.2,
        criticalIncidentsAvgResponseHours: 2.8,
        incidentsWithinSLA: 108,
        totalIncidentsWithResponse: 127,
        slaCompliancePercentage: 85.0
      },
      resolutionTimeStats: {
        averageResolutionTimeDays: 4.2,
        resolvedIncidents: 104,
        pendingResolution: 23,
        resolutionRate: 81.9
      },
      performanceMetrics: {
        correctiveActionCompletionRate: 78.5,
        employeeReportingEngagement: 92.3
      },
      recentIncidents: [
        {
          id: 1,
          title: 'Slip on wet floor in cafeteria',
          severity: 'Moderate',
          location: 'Main Building - Cafeteria',
          reporterName: 'John Smith',
          createdAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
          isOverdue: false
        },
        {
          id: 2,
          title: 'Chemical spill in laboratory',
          severity: 'Critical',
          location: 'Science Building - Lab 3',
          reporterName: 'Sarah Johnson',
          createdAt: new Date(Date.now() - 6 * 60 * 60 * 1000).toISOString(),
          isOverdue: true
        },
        {
          id: 3,
          title: 'Equipment malfunction in workshop',
          severity: 'Serious',
          location: 'Workshop Building',
          reporterName: 'Mike Wilson',
          createdAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
          isOverdue: false
        }
      ]
    };
  };

  const { data: apiData, isLoading, error, refetch, dataUpdatedAt } = useGetIncidentDashboardQuery({
    ...getDateRange(),
    department: department || undefined,
    includeResolved: true
  });

  // Use demo data if in demo mode and no API data, otherwise use API data
  const dashboardData = apiData || (isDemo ? generateDemoData() : null);

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
          <PermissionGuard 
            module={ModuleType.IncidentManagement} 
            permission={PermissionType.Create}
          >
            <CButton 
              color="primary" 
              onClick={() => navigate('/incidents/create')}
              className="btn-block-mobile"
            >
              <FontAwesomeIcon icon={faPlus} className="me-2" />
              <span className="d-none d-sm-inline">Report Incident</span>
              <span className="d-sm-none">Report</span>
            </CButton>
          </PermissionGuard>
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
                className="department-select"
              >
                <option value="">All Departments</option>
                {departments?.map((dept) => (
                  <option key={dept.id} value={dept.name}>
                    {dept.name}
                  </option>
                )) || (
                  // Fallback for demo mode or when departments haven't loaded
                  <>
                    <option value="Operations">Operations</option>
                    <option value="Maintenance">Maintenance</option>
                    <option value="Security">Security</option>
                    <option value="Administration">Administration</option>
                    <option value="Facilities">Facilities</option>
                    <option value="Academic">Academic</option>
                    <option value="Finance">Finance</option>
                    <option value="Human Resources">Human Resources</option>
                  </>
                )}
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
                    <FontAwesomeIcon icon={faArrowRotateRight} className="me-1" />
                    <span className="d-none d-sm-inline">{isLoading ? 'Refreshing...' : 'Refresh'}</span>
                    <span className="d-sm-none">{isLoading ? '...' : 'Refresh'}</span>
                  </CButton>
                  <CDropdown variant="btn-group">
                    <CDropdownToggle color="secondary" variant="outline">
                      <FontAwesomeIcon icon={faCog} className="me-1 d-none d-sm-inline" />
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
                  {connectionState === HubConnectionState.Connected && (
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
            icon={faExclamationTriangle}
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
            icon={faClipboardCheck}
            color="warning"
            isLoading={isLoading}
            onClick={() => navigate('/incidents?status=Reported,UnderInvestigation,AwaitingAction')}
          />
        </CCol>
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Critical Incidents"
            value={dashboardData?.overallStats.criticalIncidents || 0}
            icon={faShieldAlt}
            color="danger"
            isLoading={isLoading}
            onClick={() => navigate('/incidents?severity=Critical')}
          />
        </CCol>
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Overdue Items"
            value={dashboardData?.overallStats.overdueIncidents || 0}
            icon={faClock}
            color="dark"
            isLoading={isLoading}
          />
        </CCol>
      </CRow>

      {/* Charts Row 1 */}
      <CRow className="mb-4">
        <CCol lg={4} className="mb-4">
          <ChartCard
            title="Status Distribution"
            subtitle="Current incident status breakdown"
            isLoading={isLoading}
            height="400px"
            className="status-distribution-card"
          >
            {dashboardData && (
              <div className="status-distribution-container">
                <div className="chart-container mb-3">
                  <DonutChart
                    data={dashboardData.statusStats.map(stat => ({
                      label: stat.status,
                      value: stat.count,
                      color: `var(--cui-${stat.color})`
                    }))}
                    size={200}
                    strokeWidth={20}
                  />
                </div>
                <div className="status-legend">
                  {dashboardData.statusStats.map((stat, index) => (
                    <div key={index} className="legend-item d-flex align-items-center justify-content-between mb-2">
                      <div className="d-flex align-items-center">
                        <div 
                          className="legend-color me-2" 
                          style={{ 
                            backgroundColor: `var(--cui-${stat.color})`,
                            width: '12px',
                            height: '12px',
                            borderRadius: '2px'
                          }}
                        ></div>
                        <span className="small">{stat.status}</span>
                      </div>
                      <span className="fw-semibold small">{stat.count}</span>
                    </div>
                  ))}
                </div>
              </div>
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
        <CCol lg={6} className="mb-4">
          <ChartCard
            title="Category Breakdown"
            subtitle="Incidents by category type"
            isLoading={isLoading}
            height="400px"
            className="category-breakdown-card"
          >
            {dashboardData && (
              <div className="category-chart-container">
                <BarChart
                  data={dashboardData.categoryStats.slice(0, 8).map(stat => ({
                    label: stat.category.length > 15 ? 
                      stat.category.substring(0, 12) + '...' : 
                      stat.category,
                    fullLabel: stat.category,
                    value: stat.count
                  }))}
                  height={320}
                  showValues={true}
                  horizontal={window.innerWidth < 768}
                />
                {/* Legend for mobile/small screens */}
                <div className="category-legend d-md-none mt-3">
                  <h6 className="small fw-semibold mb-2">Legend:</h6>
                  <div className="row">
                    {dashboardData.categoryStats.slice(0, 8).map((stat, index) => (
                      <div key={index} className="col-6 mb-2">
                        <div className="d-flex align-items-center">
                          <div 
                            className="legend-color me-2" 
                            style={{ 
                              backgroundColor: `hsl(${(index * 45) % 360}, 70%, 50%)`,
                              width: '12px',
                              height: '12px',
                              borderRadius: '2px'
                            }}
                          ></div>
                          <span className="small text-truncate" title={stat.category}>
                            {stat.category}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
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
        <CCol lg={4} className="mb-4">
          <CCard className="h-100 response-time-card">
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
                  {/* SLA Compliance Progress */}
                  <div className="mb-4">
                    <div className="d-flex justify-content-between align-items-center mb-2">
                      <span className="text-medium-emphasis small">SLA Compliance</span>
                      <span className="fw-semibold">{dashboardData.responseTimeStats.slaCompliancePercentage.toFixed(1)}%</span>
                    </div>
                    <div className="progress" style={{ height: '8px' }}>
                      <div 
                        className={`progress-bar ${
                          dashboardData.responseTimeStats.slaCompliancePercentage >= 90 ? 'bg-success' :
                          dashboardData.responseTimeStats.slaCompliancePercentage >= 70 ? 'bg-warning' : 'bg-danger'
                        }`}
                        style={{ width: `${dashboardData.responseTimeStats.slaCompliancePercentage}%` }}
                      ></div>
                    </div>
                    <div className="small text-muted mt-1">
                      {dashboardData.responseTimeStats.incidentsWithinSLA} of {dashboardData.responseTimeStats.totalIncidentsWithResponse} within 24h target
                    </div>
                  </div>

                  {/* Metrics Grid */}
                  <div className="metrics-grid">
                    <div className="metric-item mb-3">
                      <div className="d-flex justify-content-between align-items-center">
                        <span className="text-medium-emphasis small">Avg Response</span>
                        <span className="fw-semibold">{dashboardData.responseTimeStats.averageResponseTimeHours.toFixed(1)}h</span>
                      </div>
                    </div>
                    <div className="metric-item mb-3">
                      <div className="d-flex justify-content-between align-items-center">
                        <span className="text-medium-emphasis small">Median Response</span>
                        <span className="fw-semibold">{dashboardData.responseTimeStats.medianResponseTimeHours.toFixed(1)}h</span>
                      </div>
                    </div>
                    <div className="metric-item mb-3">
                      <div className="d-flex justify-content-between align-items-center">
                        <span className="text-medium-emphasis small">Critical Avg</span>
                        <span className="fw-semibold text-danger">{dashboardData.responseTimeStats.criticalIncidentsAvgResponseHours.toFixed(1)}h</span>
                      </div>
                    </div>
                    <div className="metric-item mb-3">
                      <div className="d-flex justify-content-between align-items-center">
                        <span className="text-medium-emphasis small">Avg Resolution</span>
                        <span className="fw-semibold">{dashboardData.resolutionTimeStats.averageResolutionTimeDays.toFixed(1)} days</span>
                      </div>
                    </div>
                  </div>

                  {/* Quick Actions */}
                  <div className="mt-4">
                    <div className="small text-medium-emphasis mb-2 fw-semibold">
                      Quick Actions
                    </div>
                    <div className="d-grid gap-2">
                      <PermissionGuard 
                        module={ModuleType.IncidentManagement} 
                        permission={PermissionType.Update}
                      >
                        <CButton 
                          color="danger" 
                          variant="outline" 
                          size="sm"
                          onClick={() => navigate('/incidents?status=AwaitingAction')}
                          className="d-flex align-items-center justify-content-between"
                        >
                          <div className="d-flex align-items-center">
                            <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                            <span>Awaiting Action</span>
                          </div>
                          <span className="badge bg-danger">{dashboardData.statusStats.find(s => s.status === 'Awaiting Action')?.count || 0}</span>
                        </CButton>
                      </PermissionGuard>
                      <PermissionGuard 
                        module={ModuleType.IncidentManagement} 
                        permission={PermissionType.Read}
                      >
                        <CButton 
                          color="warning" 
                          variant="outline" 
                          size="sm"
                          onClick={() => navigate('/incidents?severity=Critical')}
                          className="d-flex align-items-center justify-content-between"
                        >
                          <div className="d-flex align-items-center">
                            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                            <span>Critical</span>
                          </div>
                          <span className="badge bg-warning">{dashboardData.overallStats.criticalIncidents}</span>
                        </CButton>
                      </PermissionGuard>
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