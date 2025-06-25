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
  CDropdownItem,
  CListGroup,
  CListGroupItem,
  CProgress,
  CProgressBar,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faShieldAlt,
  faExclamationTriangle,
  faClock,
  faRedo,
  faCog,
  faEye,
  faChartLine,
  faBug,
  faUser,
  faServer,
  faLock,
} from '@fortawesome/free-solid-svg-icons';
import { HubConnectionState } from '@microsoft/signalr';

import { useGetSecurityDashboardQuery } from '../../features/security/securityApi';
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
import {
  SecurityDashboard as SecurityDashboardData,
  SecurityIncidentType,
  SecuritySeverity,
  SecurityIncidentStatus,
  ThreatLevel,
  SecurityIncidentList,
} from '../../types/security';
import { formatDistanceToNow } from 'date-fns';
import { formatDate } from '../../utils/dateUtils';

const SecurityDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [timeRange, setTimeRange] = useState<string>('30d');
  const [autoRefreshInterval, setAutoRefreshInterval] = useState<number>(0); // 0 = disabled
  const [lastRefreshTime, setLastRefreshTime] = useState<Date>(new Date());
  const autoRefreshIntervalRef = useRef<NodeJS.Timeout | null>(null);

  // Initialize SignalR
  const { connectionState } = useSignalR();

  // Calculate date range based on selection - memoized to prevent infinite re-renders
  const dateRange = React.useMemo(() => {
    const now = new Date();
    switch (timeRange) {
      case '7d':
        return {
          startDate: new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000).toISOString(),
          endDate: now.toISOString()
        };
      case '30d':
        return {
          startDate: new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000).toISOString(),
          endDate: now.toISOString()
        };
      case '90d':
        return {
          startDate: new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000).toISOString(),
          endDate: now.toISOString()
        };
      default:
        return {};
    }
  }, [timeRange]);

  const { 
    data: dashboardData, 
    isLoading, 
    error, 
    refetch
  } = useGetSecurityDashboardQuery({
    ...dateRange,
    includeThreatIntel: true,
    includeTrends: true,
    includeMetrics: true,
  });

  // Update last refresh time when data changes
  useEffect(() => {
    setLastRefreshTime(new Date());
  }, [dashboardData]);

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

  const getSeverityColor = (severity: SecuritySeverity) => {
    switch (severity) {
      case SecuritySeverity.Critical: return 'danger';
      case SecuritySeverity.High: return 'warning';
      case SecuritySeverity.Medium: return 'info';
      case SecuritySeverity.Low: return 'success';
      default: return 'secondary';
    }
  };

  const getTypeIcon = (type: SecurityIncidentType) => {
    switch (type) {
      case SecurityIncidentType.PhysicalSecurity: return faShieldAlt;
      case SecurityIncidentType.Cybersecurity: return faBug;
      case SecurityIncidentType.PersonnelSecurity: return faUser;
      case SecurityIncidentType.InformationSecurity: return faLock;
      default: return faExclamationTriangle;
    }
  };

  const getTypeColor = (type: SecurityIncidentType) => {
    switch (type) {
      case SecurityIncidentType.PhysicalSecurity: return 'primary';
      case SecurityIncidentType.Cybersecurity: return 'danger';
      case SecurityIncidentType.PersonnelSecurity: return 'warning';
      case SecurityIncidentType.InformationSecurity: return 'info';
      default: return 'secondary';
    }
  };

  const getThreatLevelColor = (level: ThreatLevel) => {
    switch (level) {
      case ThreatLevel.Severe: return 'danger';
      case ThreatLevel.High: return 'warning';
      case ThreatLevel.Medium: return 'info';
      case ThreatLevel.Low: return 'success';
      case ThreatLevel.Minimal: return 'light';
      default: return 'secondary';
    }
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading security dashboard...</span>
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load security dashboard data. Please try again.
        <div className="mt-2">
          <CButton color="primary" onClick={() => refetch()}>
            Retry
          </CButton>
        </div>
      </CAlert>
    );
  }

  const metrics = dashboardData?.metrics;
  const trends = dashboardData?.trends;
  const recentIncidents = dashboardData?.recentIncidents || [];
  const criticalIncidents = dashboardData?.criticalIncidents || [];
  const overdueIncidents = dashboardData?.overdueIncidents || [];
  const complianceStatus = dashboardData?.complianceStatus;

  return (
    <>
      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0">Security Dashboard</h4>
              <small className="text-muted">
                Real-time security incident monitoring and analytics
                {connectionState === HubConnectionState.Connected && (
                  <CBadge color="success" className="ms-2">Live</CBadge>
                )}
              </small>
            </div>
            
            <CButtonToolbar>
              <CButtonGroup className="me-2">
                <CButton
                  color={timeRange === '7d' ? 'primary' : 'outline-primary'}
                  size="sm"
                  onClick={() => setTimeRange('7d')}
                >
                  7 Days
                </CButton>
                <CButton
                  color={timeRange === '30d' ? 'primary' : 'outline-primary'}
                  size="sm"
                  onClick={() => setTimeRange('30d')}
                >
                  30 Days
                </CButton>
                <CButton
                  color={timeRange === '90d' ? 'primary' : 'outline-primary'}
                  size="sm"
                  onClick={() => setTimeRange('90d')}
                >
                  90 Days
                </CButton>
              </CButtonGroup>

              <CDropdown className="me-2">
                <CDropdownToggle color="secondary" size="sm">
                  <FontAwesomeIcon icon={faCog} />
                </CDropdownToggle>
                <CDropdownMenu>
                  <CDropdownItem className="dropdown-header">Auto Refresh</CDropdownItem>
                  <CDropdownItem onClick={() => setAutoRefreshInterval(0)}>
                    {autoRefreshInterval === 0 && '✓ '}Disabled
                  </CDropdownItem>
                  <CDropdownItem onClick={() => setAutoRefreshInterval(30)}>
                    {autoRefreshInterval === 30 && '✓ '}30 seconds
                  </CDropdownItem>
                  <CDropdownItem onClick={() => setAutoRefreshInterval(60)}>
                    {autoRefreshInterval === 60 && '✓ '}1 minute
                  </CDropdownItem>
                  <CDropdownItem onClick={() => setAutoRefreshInterval(300)}>
                    {autoRefreshInterval === 300 && '✓ '}5 minutes
                  </CDropdownItem>
                </CDropdownMenu>
              </CDropdown>

              <CButton
                color="primary"
                variant="outline"
                size="sm"
                onClick={() => refetch()}
                disabled={isLoading}
              >
                <FontAwesomeIcon 
                  icon={faRedo} 
                  spin={isLoading}
                  className={isLoading ? 'text-muted' : ''}
                />
                <span className="d-none d-md-inline ms-1">
                  {isLoading ? 'Refreshing...' : 'Refresh'}
                </span>
              </CButton>
            </CButtonToolbar>
          </div>
          <small className="text-muted">
            {isLoading ? (
              <span className="text-primary">
                <CSpinner size="sm" className="me-1" />
                Updating dashboard data...
              </span>
            ) : (
              `Last updated: ${formatDistanceToNow(lastRefreshTime)} ago`
            )}
          </small>
        </CCol>
      </CRow>

      {/* Key Metrics */}
      <CRow className="mb-4">
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Total Incidents"
            value={metrics?.totalIncidents || 0}
            icon={faExclamationTriangle}
            color="primary"
            trend={metrics?.incidentTrend ? {
              value: Math.abs(metrics.incidentTrend),
              isPositive: metrics.incidentTrend >= 0,
              label: 'vs last period'
            } : undefined}
            subtitle={`${metrics?.openIncidents || 0} open`}
            isLoading={isLoading}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Critical Incidents"
            value={metrics?.criticalIncidents || 0}
            icon={faShieldAlt}
            color="danger"
            subtitle={`${metrics?.highSeverityIncidents || 0} high severity`}
            isLoading={isLoading}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Data Breaches"
            value={metrics?.dataBreachIncidents || 0}
            icon={faLock}
            color="warning"
            subtitle="Incidents involving data"
            isLoading={isLoading}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Avg Resolution"
            value={`${metrics?.averageResolutionTimeHours?.toFixed(1) || 0}h`}
            icon={faClock}
            color="info"
            subtitle="Time to resolution"
            isLoading={isLoading}
          />
        </CCol>
      </CRow>

      {/* Incident Types and Threat Levels */}
      <CRow className="mb-4">
        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <h6 className="mb-0">Incidents by Type</h6>
            </CCardHeader>
            <CCardBody>
              <CRow>
                <CCol xs={6}>
                  <div className="text-center mb-3">
                    <FontAwesomeIcon
                      icon={faShieldAlt}
                      size="2x"
                      className="text-primary mb-2"
                    />
                    <h5 className="mb-0">{metrics?.physicalSecurityIncidents || 0}</h5>
                    <small className="text-muted">Physical Security</small>
                  </div>
                </CCol>
                <CCol xs={6}>
                  <div className="text-center mb-3">
                    <FontAwesomeIcon
                      icon={faBug}
                      size="2x"
                      className="text-danger mb-2"
                    />
                    <h5 className="mb-0">{metrics?.cybersecurityIncidents || 0}</h5>
                    <small className="text-muted">Cybersecurity</small>
                  </div>
                </CCol>
                <CCol xs={6}>
                  <div className="text-center mb-3">
                    <FontAwesomeIcon
                      icon={faUser}
                      size="2x"
                      className="text-warning mb-2"
                    />
                    <h5 className="mb-0">{metrics?.personnelSecurityIncidents || 0}</h5>
                    <small className="text-muted">Personnel Security</small>
                  </div>
                </CCol>
                <CCol xs={6}>
                  <div className="text-center mb-3">
                    <FontAwesomeIcon
                      icon={faLock}
                      size="2x"
                      className="text-info mb-2"
                    />
                    <h5 className="mb-0">{metrics?.informationSecurityIncidents || 0}</h5>
                    <small className="text-muted">Information Security</small>
                  </div>
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        </CCol>

        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <h6 className="mb-0">Compliance Status</h6>
            </CCardHeader>
            <CCardBody>
              {complianceStatus ? (
                <>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between align-items-center mb-2">
                      <span>Overall Compliance Score</span>
                      <strong>{complianceStatus.complianceScore}%</strong>
                    </div>
                    <CProgress>
                      <CProgressBar
                        value={complianceStatus.complianceScore}
                        color={complianceStatus.complianceScore >= 80 ? 'success' : 
                               complianceStatus.complianceScore >= 60 ? 'warning' : 'danger'}
                      />
                    </CProgress>
                  </div>

                  <div className="row text-center">
                    <div className="col-4">
                      <CBadge color={complianceStatus.iso27001Compliant ? 'success' : 'danger'}>
                        ISO 27001
                      </CBadge>
                      <div><small className="text-muted">Information Security</small></div>
                    </div>
                    <div className="col-4">
                      <CBadge color={complianceStatus.iteLawCompliant ? 'success' : 'danger'}>
                        ITE Law
                      </CBadge>
                      <div><small className="text-muted">Data Protection</small></div>
                    </div>
                    <div className="col-4">
                      <CBadge color={complianceStatus.smk3Compliant ? 'success' : 'danger'}>
                        SMK3
                      </CBadge>
                      <div><small className="text-muted">Safety Management</small></div>
                    </div>
                  </div>

                  {complianceStatus.issues && complianceStatus.issues.length > 0 && (
                    <div className="mt-3">
                      <small className="text-muted">
                        {complianceStatus.issues.length} compliance issue(s) require attention
                      </small>
                    </div>
                  )}
                </>
              ) : (
                <div className="text-center py-3">
                  <small className="text-muted">Compliance data not available</small>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Critical and Recent Incidents */}
      <CRow className="mb-4">
        <CCol md={6}>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <h6 className="mb-0">Critical Incidents</h6>
              <CBadge color="danger">{criticalIncidents.length}</CBadge>
            </CCardHeader>
            <CCardBody>
              {criticalIncidents.length > 0 ? (
                <CListGroup flush>
                  {criticalIncidents.slice(0, 5).map((incident: SecurityIncidentList) => (
                    <CListGroupItem
                      key={incident.id}
                      className="d-flex justify-content-between align-items-start cursor-pointer"
                      onClick={() => navigate(`/security/incidents/${incident.id}`)}
                    >
                      <div className="flex-grow-1">
                        <div className="fw-semibold">{incident.title}</div>
                        <small className="text-muted">
                          {incident.location} • {formatDate(incident.incidentDateTime)}
                        </small>
                      </div>
                      <div className="text-end">
                        <CBadge color={getTypeColor(incident.incidentType)} className="mb-1">
                          {incident.incidentType === SecurityIncidentType.PhysicalSecurity ? 'Physical' :
                           incident.incidentType === SecurityIncidentType.Cybersecurity ? 'Cyber' :
                           incident.incidentType === SecurityIncidentType.PersonnelSecurity ? 'Personnel' : 'Information'}
                        </CBadge>
                        <div>
                          <CBadge color={getThreatLevelColor(incident.threatLevel)}>
                            {incident.threatLevel === ThreatLevel.Severe ? 'Severe' :
                             incident.threatLevel === ThreatLevel.High ? 'High' :
                             incident.threatLevel === ThreatLevel.Medium ? 'Medium' :
                             incident.threatLevel === ThreatLevel.Low ? 'Low' : 'Minimal'}
                          </CBadge>
                        </div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              ) : (
                <div className="text-center py-3">
                  <FontAwesomeIcon icon={faShieldAlt} className="text-muted mb-2" size="2x" />
                  <div><small className="text-muted">No critical incidents</small></div>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>

        <CCol md={6}>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <h6 className="mb-0">Recent Incidents</h6>
              <CButton
                color="primary"
                variant="outline"
                size="sm"
                onClick={() => navigate('/security/incidents')}
              >
                <FontAwesomeIcon icon={faEye} className="me-1" />
                View All
              </CButton>
            </CCardHeader>
            <CCardBody>
              {recentIncidents.length > 0 ? (
                <CListGroup flush>
                  {recentIncidents.slice(0, 5).map((incident: SecurityIncidentList) => (
                    <CListGroupItem
                      key={incident.id}
                      className="d-flex justify-content-between align-items-start cursor-pointer"
                      onClick={() => navigate(`/security/incidents/${incident.id}`)}
                    >
                      <div className="flex-grow-1">
                        <div className="fw-semibold">{incident.title}</div>
                        <small className="text-muted">
                          {incident.location} • {formatDistanceToNow(new Date(incident.incidentDateTime))} ago
                        </small>
                      </div>
                      <div className="text-end">
                        <CBadge color={getSeverityColor(incident.severity)} className="mb-1">
                          {incident.severity === SecuritySeverity.Critical ? 'Critical' :
                           incident.severity === SecuritySeverity.High ? 'High' :
                           incident.severity === SecuritySeverity.Medium ? 'Medium' : 'Low'}
                        </CBadge>
                        {incident.isOverdue && (
                          <div>
                            <CBadge color="warning">Overdue</CBadge>
                          </div>
                        )}
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              ) : (
                <div className="text-center py-3">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="text-muted mb-2" size="2x" />
                  <div><small className="text-muted">No recent incidents</small></div>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Overdue Incidents Alert */}
      {overdueIncidents.length > 0 && (
        <CRow className="mb-4">
          <CCol>
            <CAlert color="warning">
              <div className="d-flex justify-content-between align-items-center">
                <div>
                  <FontAwesomeIcon icon={faClock} className="me-2" />
                  <strong>{overdueIncidents.length} security incidents are overdue</strong>
                  <div className="mt-1">
                    <small>These incidents require immediate attention to maintain security posture.</small>
                  </div>
                </div>
                <CButton
                  color="warning"
                  variant="outline"
                  size="sm"
                  onClick={() => navigate('/security/incidents?status=overdue')}
                >
                  Review Overdue
                </CButton>
              </div>
            </CAlert>
          </CCol>
        </CRow>
      )}

      {/* Quick Actions */}
      <CRow>
        <CCol>
          <CCard>
            <CCardHeader>
              <h6 className="mb-0">Quick Actions</h6>
            </CCardHeader>
            <CCardBody>
              <div className="d-flex gap-2 flex-wrap">
                <CButton
                  color="primary"
                  onClick={() => navigate('/security/incidents/create')}
                >
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                  Report Security Incident
                </CButton>
                <CButton
                  color="info"
                  variant="outline"
                  onClick={() => navigate('/security/incidents?status=open')}
                >
                  <FontAwesomeIcon icon={faEye} className="me-2" />
                  View Open Incidents
                </CButton>
                <CButton
                  color="warning"
                  variant="outline"
                  onClick={() => navigate('/security/analytics')}
                >
                  <FontAwesomeIcon icon={faChartLine} className="me-2" />
                  Security Analytics
                </CButton>
              </div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default SecurityDashboard;