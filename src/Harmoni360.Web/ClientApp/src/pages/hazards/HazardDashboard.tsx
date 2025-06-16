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
  faPlus,
  faArrowRotateRight,
  faCog,
  faExclamationTriangle,
  faExclamationCircle,
  faQuestionCircle,
} from '@fortawesome/free-solid-svg-icons';
import { HubConnectionState } from '@microsoft/signalr';

import { useGetHazardDashboardQuery } from '../../features/hazards/hazardApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import { useSignalR } from '../../hooks/useSignalR';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { HAZARD_ICONS } from '../../utils/iconMappings';
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

const HazardDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { isDemoMode } = useApplicationMode();
  const [timeRange, setTimeRange] = useState<string>('all');
  const [department, setDepartment] = useState<string>('');
  const [autoRefreshInterval, setAutoRefreshInterval] = useState<number>(0); // 0 = disabled
  const [lastRefreshTime, setLastRefreshTime] = useState<Date>(new Date());
  const autoRefreshIntervalRef = useRef<NodeJS.Timeout | null>(null);

  // Initialize SignalR
  const { connectionState } = useSignalR();
  
  // Get departments from database
  const { data: departments } = useGetDepartmentsQuery({ isActive: true });

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

  // Helper function to get status color
  const getStatusColor = (severity: string): string => {
    switch (severity?.toLowerCase()) {
      case 'critical':
      case 'error':
      case 'danger':
        return 'danger';
      case 'warning':
        return 'warning';
      case 'success':
        return 'success';
      case 'info':
      default:
        return 'info';
    }
  };

  const { 
    data, 
    isLoading, 
    error,
    refetch
  } = useGetHazardDashboardQuery({
    ...getDateRange(),
    department: department || undefined,
    includeTrends: true,
    includeLocationAnalytics: true,
    includeComplianceMetrics: true,
    includePerformanceMetrics: true,
  });

  // Update last refresh time when data changes
  useEffect(() => {
    setLastRefreshTime(new Date());
  }, [data]);

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

  if (isLoading) {
    return (
      <div className="text-center py-4">
        <CSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load hazard dashboard. Please try again.
      </CAlert>
    );
  }

  if (!data) {
    return (
      <CAlert color="info">
        No hazard data available.
      </CAlert>
    );
  }

  return (
    <div className="dashboard-container">
      {/* Header */}
      <div className="mb-4 dashboard-header">
        <div className="d-flex justify-content-between align-items-start flex-wrap gap-3">
          <div>
            <h1 className="h2 h1-md">Hazard Management Dashboard</h1>
            <p className="text-medium-emphasis mb-0 d-none d-md-block">
              Comprehensive hazard reporting and risk assessment overview
            </p>
          </div>
          <PermissionGuard 
            module={ModuleType.RiskManagement} 
            permission={PermissionType.Create}
          >
            <CButton 
              color="warning" 
              onClick={() => navigate('/hazards/create')}
              className="btn-block-mobile"
            >
              <FontAwesomeIcon icon={HAZARD_ICONS.reporting} className="me-2" />
              <span className="d-none d-sm-inline">Report Hazard</span>
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

      {/* Overview Stats */}
      <CRow className="mb-4">
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Total Hazards"
            value={data.overview?.totalHazards || 0}
            change={data.overview?.totalHazardsChange}
            color="primary"
            icon={faExclamationTriangle}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Open Hazards"
            value={data.overview?.openHazards || 0}
            color="warning"
            icon={faExclamationCircle}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="High Risk"
            value={data.overview?.highRiskHazards || 0}
            change={data.overview?.highRiskChange}
            color="danger"
            icon={faExclamationTriangle}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Unassessed"
            value={data.overview?.unassessedHazards || 0}
            color="info"
            icon={faQuestionCircle}
          />
        </CCol>
      </CRow>

      {/* Progress Cards */}
      <CRow className="mb-4">
        <CCol md={4}>
          <ProgressCard
            title="Risk Assessment Progress"
            value={data.riskAnalysis?.riskAssessmentsCompleted || 0}
            total={(data.riskAnalysis?.riskAssessmentsCompleted || 0) + (data.riskAnalysis?.riskAssessmentsPending || 0)}
            percentage={data.riskAnalysis?.riskAssessmentCompletionRate || 0}
            description={`${data.riskAnalysis?.riskAssessmentsCompleted || 0} of ${(data.riskAnalysis?.riskAssessmentsCompleted || 0) + (data.riskAnalysis?.riskAssessmentsPending || 0)} completed`}
            color="success"
          />
        </CCol>
        <CCol md={4}>
          <ProgressCard
            title="Mitigation Action Completion"
            value={data.performance?.completedMitigationActions || 0}
            total={(data.performance?.completedMitigationActions || 0) + (data.performance?.totalMitigationActions || 0)}
            percentage={data.performance?.mitigationActionCompletionRate || 0}
            description={`${data.performance?.completedMitigationActions || 0} of ${data.performance?.totalMitigationActions || 0} completed`}
            color="primary"
          />
        </CCol>
        <CCol md={4}>
          <ProgressCard
            title="Compliance Score"
            value={data.compliance?.complianceViolations || 0}
            total={100}
            percentage={data.compliance?.overallComplianceScore || 0}
            description={`${data.compliance?.complianceViolations || 0} violations found`}
            color={(data.compliance?.overallComplianceScore || 0) >= 80 ? 'success' : 'warning'}
          />
        </CCol>
      </CRow>

      {/* Charts */}
      <CRow className="mb-4">
        <CCol md={6}>
          <ChartCard title="Risk Level Distribution">
            <DonutChart
              data={Object.entries(data.riskAnalysis?.riskLevelDistribution || {}).map(([label, value]) => ({
                label,
                value: typeof value === 'number' ? value : 0
              }))}
            />
          </ChartCard>
        </CCol>
        <CCol md={6}>
          <ChartCard title="Hazards by Category">
            <BarChart
              data={Object.entries(data.riskAnalysis?.categoryDistribution || {}).map(([label, value]) => ({
                label,
                value: typeof value === 'number' ? value : 0
              }))}
            />
          </ChartCard>
        </CCol>
      </CRow>

      <CRow className="mb-4">
        <CCol md={8}>
          <ChartCard title="Hazard Reporting Trend">
            <LineChart
              data={(data.trends?.hazardReportingTrend || []).map(point => ({
                label: point?.label || 'Unknown',
                value: typeof point?.value === 'number' ? point.value : 0
              }))}
            />
          </ChartCard>
        </CCol>
        <CCol md={4}>
          <ChartCard title="Severity Distribution">
            <DonutChart
              data={Object.entries(data.riskAnalysis?.severityDistribution || {}).map(([label, value]) => ({
                label,
                value: typeof value === 'number' ? value : 0
              }))}
            />
          </ChartCard>
        </CCol>
      </CRow>

      {/* Recent Activities and Alerts */}
      <CRow>
        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <strong>Recent Activities</strong>
            </CCardHeader>
            <CCardBody>
              <RecentItemsList
                items={(data.recentActivities || []).map((activity, index) => ({
                  id: (activity?.id || index).toString(),
                  title: activity?.title || 'Unknown Activity',
                  subtitle: activity?.description,
                  metadata: {
                    status: activity?.severity || 'info',
                    statusColor: getStatusColor(activity?.severity || 'info'),
                    timestamp: activity?.timestamp || new Date().toISOString()
                  },
                  clickAction: () => window.location.href = `/hazards/${activity?.relatedEntityId || ''}`
                }))}
              />
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <strong>Active Alerts</strong>
            </CCardHeader>
            <CCardBody>
              <RecentItemsList
                items={(data.alerts || []).map((alert, index) => ({
                  id: (alert?.id || index + 1000).toString(),
                  title: alert?.title || 'Unknown Alert',
                  subtitle: alert?.message,
                  metadata: {
                    status: alert?.severity || 'warning',
                    statusColor: getStatusColor(alert?.severity || 'warning'),
                    timestamp: alert?.createdAt || new Date().toISOString()
                  },
                  clickAction: () => window.location.href = `/hazards/${alert?.hazardId || ''}`
                }))}
              />
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </div>
  );
};

export default HazardDashboard;