import React, { useState, useMemo } from 'react';
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
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CBadge,
  CAlert,
  CSpinner,
  CProgress,
  CListGroup,
  CListGroupItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CTooltip
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faFileContract,
  faPlus,
  faTachometerAlt,
  faChartBar,
  faChartPie,
  faChartLine,
  faExclamationTriangle,
  faClock,
  faCheckCircle,
  faTimesCircle,
  faPause,
  faBan,
  faRedo,
  faShieldAlt,
  faCalendarAlt,
  faBuilding,
  faCertificate,
  faUsers,
  faDollarSign,
  faFileAlt,
  faEye,
  faFilter,
  faRefresh,
  faDownload,
  faArrowUp,
  faArrowDown,
  faMinus,
  faInfoCircle,
  faWarning
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetLicenseDashboardQuery,
  useGetExpiringLicensesQuery,
  useGetLicensesQuery
} from '../../features/licenses/licenseApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import { 
  LicenseDto,
  LicenseDashboardDto,
  getStatusColor,
  getPriorityColor,
  getRiskLevelColor
} from '../../types/license';
import { format, subDays, startOfMonth, endOfMonth } from 'date-fns';

// Dashboard Icon Mappings
const DASHBOARD_ICONS = {
  dashboard: faTachometerAlt,
  license: faFileContract,
  create: faPlus,
  chart: faChartBar,
  pie: faChartPie,
  line: faChartLine,
  warning: faExclamationTriangle,
  clock: faClock,
  check: faCheckCircle,
  times: faTimesCircle,
  pause: faPause,
  ban: faBan,
  renew: faRedo,
  shield: faShieldAlt,
  calendar: faCalendarAlt,
  building: faBuilding,
  certificate: faCertificate,
  users: faUsers,
  dollar: faDollarSign,
  file: faFileAlt,
  eye: faEye,
  filter: faFilter,
  refresh: faRefresh,
  download: faDownload,
  up: faArrowUp,
  down: faArrowDown,
  neutral: faMinus,
  info: faInfoCircle
};

interface DashboardFilters {
  departmentId?: number;
  fromDate: string;
  toDate: string;
}

const LicenseDashboard: React.FC = () => {
  const navigate = useNavigate();

  // State management
  const [filters, setFilters] = useState<DashboardFilters>({
    fromDate: format(startOfMonth(new Date()), 'yyyy-MM-dd'),
    toDate: format(endOfMonth(new Date()), 'yyyy-MM-dd')
  });

  // API calls
  const { data: departments } = useGetDepartmentsQuery({});
  const { 
    data: dashboardData, 
    isLoading: isDashboardLoading, 
    error: dashboardError,
    refetch: refetchDashboard
  } = useGetLicenseDashboardQuery(filters);

  const { 
    data: expiringLicenses,
    isLoading: isExpiringLoading 
  } = useGetExpiringLicensesQuery({
    daysAhead: 30,
    pageSize: 5
  });

  const {
    data: recentLicenses,
    isLoading: isRecentLoading
  } = useGetLicensesQuery({
    pageSize: 5,
    sortBy: 'createdAt',
    sortDirection: 'desc'
  });

  // Handle filter changes
  const handleFilterChange = (field: keyof DashboardFilters, value: any) => {
    setFilters(prev => ({ ...prev, [field]: value }));
  };

  // Calculate trend direction
  const getTrendIcon = (current: number, previous: number) => {
    if (current > previous) return { icon: DASHBOARD_ICONS.up, color: 'success' };
    if (current < previous) return { icon: DASHBOARD_ICONS.down, color: 'danger' };
    return { icon: DASHBOARD_ICONS.neutral, color: 'secondary' };
  };

  // Status distribution chart data
  const statusChartData = useMemo(() => {
    if (!dashboardData) return [];
    
    return [
      { label: 'Active', value: dashboardData.activeLicenses, color: 'success' },
      { label: 'Pending', value: dashboardData.submittedLicenses + dashboardData.underReviewLicenses, color: 'warning' },
      { label: 'Expired', value: dashboardData.expiredLicenses, color: 'danger' },
      { label: 'Suspended', value: dashboardData.suspendedLicenses, color: 'warning' },
      { label: 'Draft', value: dashboardData.draftLicenses, color: 'secondary' }
    ].filter(item => item.value > 0);
  }, [dashboardData]);

  // Risk level distribution
  const riskDistribution = useMemo(() => {
    if (!dashboardData) return [];
    
    const total = dashboardData.totalLicenses;
    return [
      { 
        label: 'High Risk', 
        value: dashboardData.highRiskLicenses, 
        percentage: total > 0 ? Math.round((dashboardData.highRiskLicenses / total) * 100) : 0,
        color: 'danger' 
      },
      { 
        label: 'Critical', 
        value: dashboardData.criticalLicenses, 
        percentage: total > 0 ? Math.round((dashboardData.criticalLicenses / total) * 100) : 0,
        color: 'warning' 
      }
    ];
  }, [dashboardData]);

  if (isDashboardLoading) {
    return (
      <div className="text-center p-4">
        <CSpinner />
        <div className="mt-2">Loading dashboard...</div>
      </div>
    );
  }

  if (dashboardError || !dashboardData) {
    return (
      <CRow>
        <CCol xs={12}>
          <CAlert color="danger">
            <FontAwesomeIcon icon={DASHBOARD_ICONS.warning} className="me-2" />
            Error loading dashboard data. Please try again.
          </CAlert>
        </CCol>
      </CRow>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        {/* Dashboard Header */}
        <CCard className="mb-4">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div className="d-flex align-items-center">
              <FontAwesomeIcon icon={DASHBOARD_ICONS.dashboard} className="me-2" />
              <h4 className="mb-0">License Management Dashboard</h4>
            </div>
            <div className="d-flex gap-2">
              <CButton
                color="primary"
                onClick={() => navigate('/licenses/create')}
              >
                <FontAwesomeIcon icon={DASHBOARD_ICONS.create} className="me-1" />
                Create License
              </CButton>
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => refetchDashboard()}
              >
                <FontAwesomeIcon icon={DASHBOARD_ICONS.refresh} className="me-1" />
                Refresh
              </CButton>
            </div>
          </CCardHeader>
          <CCardBody>
            {/* Filters */}
            <CRow className="mb-3">
              <CCol md={4}>
                <CFormSelect
                  value={filters.departmentId || ''}
                  onChange={(e) => handleFilterChange('departmentId', e.target.value ? parseInt(e.target.value) : undefined)}
                >
                  <option value="">All Departments</option>
                  {departments?.items?.map(dept => (
                    <option key={dept.id} value={dept.id}>
                      {dept.name}
                    </option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={3}>
                <input
                  type="date"
                  className="form-control"
                  value={filters.fromDate}
                  onChange={(e) => handleFilterChange('fromDate', e.target.value)}
                />
              </CCol>
              <CCol md={3}>
                <input
                  type="date"
                  className="form-control"
                  value={filters.toDate}
                  onChange={(e) => handleFilterChange('toDate', e.target.value)}
                />
              </CCol>
              <CCol md={2}>
                <CButtonGroup className="w-100">
                  <CButton
                    color="info"
                    variant="outline"
                    onClick={() => navigate('/licenses')}
                  >
                    <FontAwesomeIcon icon={DASHBOARD_ICONS.eye} className="me-1" />
                    View All
                  </CButton>
                </CButtonGroup>
              </CCol>
            </CRow>
          </CCardBody>
        </CCard>

        {/* Key Metrics Cards */}
        <CRow className="mb-4">
          <CCol sm={6} lg={3}>
            <CCard className="text-center">
              <CCardBody>
                <div className="text-primary display-6 mb-2">
                  <FontAwesomeIcon icon={DASHBOARD_ICONS.license} />
                </div>
                <div className="h4 mb-1">{dashboardData.totalLicenses.toLocaleString()}</div>
                <div className="text-muted">Total Licenses</div>
                <CBadge color="info" className="mt-2">
                  {dashboardData.activeLicenses} Active
                </CBadge>
              </CCardBody>
            </CCard>
          </CCol>
          
          <CCol sm={6} lg={3}>
            <CCard className="text-center">
              <CCardBody>
                <div className="text-warning display-6 mb-2">
                  <FontAwesomeIcon icon={DASHBOARD_ICONS.clock} />
                </div>
                <div className="h4 mb-1">{dashboardData.expiringThisMonth}</div>
                <div className="text-muted">Expiring This Month</div>
                <CBadge color="warning" className="mt-2">
                  {dashboardData.renewalsDue} Renewals Due
                </CBadge>
              </CCardBody>
            </CCard>
          </CCol>
          
          <CCol sm={6} lg={3}>
            <CCard className="text-center">
              <CCardBody>
                <div className="text-danger display-6 mb-2">
                  <FontAwesomeIcon icon={DASHBOARD_ICONS.warning} />
                </div>
                <div className="h4 mb-1">{dashboardData.expiredLicenses}</div>
                <div className="text-muted">Expired Licenses</div>
                <CBadge color="danger" className="mt-2">
                  {dashboardData.overdueConditions} Overdue Items
                </CBadge>
              </CCardBody>
            </CCard>
          </CCol>
          
          <CCol sm={6} lg={3}>
            <CCard className="text-center">
              <CCardBody>
                <div className="text-success display-6 mb-2">
                  <FontAwesomeIcon icon={DASHBOARD_ICONS.dollar} />
                </div>
                <div className="h4 mb-1">
                  {dashboardData.totalLicenseFees > 0 ? 
                    `$${(dashboardData.totalLicenseFees / 1000000).toFixed(1)}M` : 
                    '$0'
                  }
                </div>
                <div className="text-muted">Total License Value</div>
                <CBadge color="success" className="mt-2">
                  ${dashboardData.averageLicenseFee?.toLocaleString() || 0} Avg
                </CBadge>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>

        <CRow>
          {/* License Status Distribution */}
          <CCol lg={6}>
            <CCard className="mb-4">
              <CCardHeader>
                <FontAwesomeIcon icon={DASHBOARD_ICONS.pie} className="me-2" />
                License Status Distribution
              </CCardHeader>
              <CCardBody>
                {statusChartData.map((item, index) => (
                  <div key={index} className="d-flex justify-content-between align-items-center mb-3">
                    <div className="d-flex align-items-center">
                      <CBadge color={item.color} className="me-2">
                        {item.label}
                      </CBadge>
                    </div>
                    <div className="flex-grow-1 mx-3">
                      <CProgress 
                        value={(item.value / dashboardData.totalLicenses) * 100} 
                        color={item.color}
                        height={20}
                      />
                    </div>
                    <div className="text-end">
                      <strong>{item.value}</strong>
                      <div className="text-muted small">
                        {((item.value / dashboardData.totalLicenses) * 100).toFixed(1)}%
                      </div>
                    </div>
                  </div>
                ))}
              </CCardBody>
            </CCard>
          </CCol>

          {/* License Types */}
          <CCol lg={6}>
            <CCard className="mb-4">
              <CCardHeader>
                <FontAwesomeIcon icon={DASHBOARD_ICONS.chart} className="me-2" />
                License Types Overview
              </CCardHeader>
              <CCardBody>
                {dashboardData.licensesByType.slice(0, 5).map((type, index) => (
                  <div key={index} className="d-flex justify-content-between align-items-center mb-3">
                    <div>
                      <strong>{type.typeDisplay}</strong>
                      <div className="text-muted small">
                        {type.active} active • {type.expired} expired
                      </div>
                    </div>
                    <div className="text-end">
                      <CBadge color="info" size="lg">
                        {type.count}
                      </CBadge>
                      <div className="text-muted small">
                        {type.percentage.toFixed(1)}%
                      </div>
                    </div>
                  </div>
                ))}
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>

        <CRow>
          {/* Expiring Licenses */}
          <CCol lg={6}>
            <CCard className="mb-4">
              <CCardHeader className="d-flex justify-content-between align-items-center">
                <div>
                  <FontAwesomeIcon icon={DASHBOARD_ICONS.warning} className="me-2" />
                  Expiring Soon
                </div>
                <CButton
                  color="warning"
                  variant="outline"
                  size="sm"
                  onClick={() => navigate('/licenses?isExpiring=true')}
                >
                  View All
                </CButton>
              </CCardHeader>
              <CCardBody>
                {isExpiringLoading ? (
                  <div className="text-center">
                    <CSpinner size="sm" />
                  </div>
                ) : expiringLicenses?.items.length === 0 ? (
                  <CAlert color="success" className="mb-0">
                    <FontAwesomeIcon icon={DASHBOARD_ICONS.check} className="me-2" />
                    No licenses expiring in the next 30 days.
                  </CAlert>
                ) : (
                  <CListGroup flush>
                    {expiringLicenses?.items.map((license) => (
                      <CListGroupItem
                        key={license.id}
                        className="d-flex justify-content-between align-items-center"
                        action
                        onClick={() => navigate(`/licenses/${license.id}`)}
                        style={{ cursor: 'pointer' }}
                      >
                        <div>
                          <strong>{license.title}</strong>
                          <div className="text-muted small">
                            #{license.licenseNumber} • {license.typeDisplay}
                          </div>
                        </div>
                        <div className="text-end">
                          <CBadge color={license.daysUntilExpiry <= 7 ? 'danger' : 'warning'}>
                            {license.daysUntilExpiry} days
                          </CBadge>
                          <div className="text-muted small">
                            {format(new Date(license.expiryDate), 'MMM dd')}
                          </div>
                        </div>
                      </CListGroupItem>
                    ))}
                  </CListGroup>
                )}
              </CCardBody>
            </CCard>
          </CCol>

          {/* Recent Licenses */}
          <CCol lg={6}>
            <CCard className="mb-4">
              <CCardHeader className="d-flex justify-content-between align-items-center">
                <div>
                  <FontAwesomeIcon icon={DASHBOARD_ICONS.file} className="me-2" />
                  Recent Licenses
                </div>
                <CButton
                  color="info"
                  variant="outline"
                  size="sm"
                  onClick={() => navigate('/licenses')}
                >
                  View All
                </CButton>
              </CCardHeader>
              <CCardBody>
                {isRecentLoading ? (
                  <div className="text-center">
                    <CSpinner size="sm" />
                  </div>
                ) : recentLicenses?.items.length === 0 ? (
                  <CAlert color="info" className="mb-0">
                    <FontAwesomeIcon icon={DASHBOARD_ICONS.info} className="me-2" />
                    No recent licenses found.
                  </CAlert>
                ) : (
                  <CListGroup flush>
                    {recentLicenses?.items.map((license) => (
                      <CListGroupItem
                        key={license.id}
                        className="d-flex justify-content-between align-items-center"
                        action
                        onClick={() => navigate(`/licenses/${license.id}`)}
                        style={{ cursor: 'pointer' }}
                      >
                        <div>
                          <strong>{license.title}</strong>
                          <div className="text-muted small">
                            #{license.licenseNumber} • {license.department}
                          </div>
                        </div>
                        <div className="text-end">
                          <CBadge color={getStatusColor(license.status)}>
                            {license.statusDisplay}
                          </CBadge>
                          <div className="text-muted small">
                            {format(new Date(license.createdAt), 'MMM dd')}
                          </div>
                        </div>
                      </CListGroupItem>
                    ))}
                  </CListGroup>
                )}
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>

        {/* Risk and Compliance Summary */}
        <CRow>
          <CCol lg={6}>
            <CCard className="mb-4">
              <CCardHeader>
                <FontAwesomeIcon icon={DASHBOARD_ICONS.shield} className="me-2" />
                Risk & Compliance Summary
              </CCardHeader>
              <CCardBody>
                <CRow className="text-center">
                  <CCol xs={6}>
                    <div className="border-end">
                      <div className="h3 text-danger">{dashboardData.highRiskLicenses}</div>
                      <div className="text-muted">High Risk</div>
                    </div>
                  </CCol>
                  <CCol xs={6}>
                    <div className="h3 text-warning">{dashboardData.criticalLicenses}</div>
                    <div className="text-muted">Critical</div>
                  </CCol>
                </CRow>
                
                <hr />

                <div className="mb-3">
                  <div className="d-flex justify-content-between mb-1">
                    <span>Compliance Rate</span>
                    <span>
                      {dashboardData.totalLicenses > 0 ? 
                        Math.round(((dashboardData.totalLicenses - dashboardData.overdueConditions) / dashboardData.totalLicenses) * 100) : 100
                      }%
                    </span>
                  </div>
                  <CProgress 
                    value={dashboardData.totalLicenses > 0 ? 
                      ((dashboardData.totalLicenses - dashboardData.overdueConditions) / dashboardData.totalLicenses) * 100 : 100
                    }
                    color="success"
                  />
                </div>

                <CListGroup flush>
                  <CListGroupItem className="d-flex justify-content-between align-items-center">
                    <span>Overdue Conditions</span>
                    <CBadge color="danger">
                      {dashboardData.overdueConditions}
                    </CBadge>
                  </CListGroupItem>
                  <CListGroupItem className="d-flex justify-content-between align-items-center">
                    <span>Pending Renewals</span>
                    <CBadge color="warning">
                      {dashboardData.pendingRenewalLicenses}
                    </CBadge>
                  </CListGroupItem>
                </CListGroup>
              </CCardBody>
            </CCard>
          </CCol>

          {/* Quick Actions */}
          <CCol lg={6}>
            <CCard className="mb-4">
              <CCardHeader>
                <FontAwesomeIcon icon={DASHBOARD_ICONS.certificate} className="me-2" />
                Quick Actions
              </CCardHeader>
              <CCardBody>
                <div className="d-grid gap-2">
                  <CButton
                    color="primary"
                    onClick={() => navigate('/licenses/create')}
                  >
                    <FontAwesomeIcon icon={DASHBOARD_ICONS.create} className="me-2" />
                    Create New License
                  </CButton>
                  
                  <CButton
                    color="warning"
                    variant="outline"
                    onClick={() => navigate('/licenses?isExpiring=true')}
                  >
                    <FontAwesomeIcon icon={DASHBOARD_ICONS.clock} className="me-2" />
                    Review Expiring Licenses ({dashboardData.expiringThisMonth})
                  </CButton>
                  
                  <CButton
                    color="danger"
                    variant="outline"
                    onClick={() => navigate('/licenses?isExpired=true')}
                  >
                    <FontAwesomeIcon icon={DASHBOARD_ICONS.warning} className="me-2" />
                    Review Expired Licenses ({dashboardData.expiredLicenses})
                  </CButton>
                  
                  <CButton
                    color="info"
                    variant="outline"
                    onClick={() => navigate('/licenses?renewalDue=true')}
                  >
                    <FontAwesomeIcon icon={DASHBOARD_ICONS.renew} className="me-2" />
                    Process Renewals ({dashboardData.renewalsDue})
                  </CButton>
                  
                  <CButton
                    color="success"
                    variant="outline"
                    onClick={() => window.open('/api/licenses/export', '_blank')}
                  >
                    <FontAwesomeIcon icon={DASHBOARD_ICONS.download} className="me-2" />
                    Export All Data
                  </CButton>
                </div>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      </CCol>
    </CRow>
  );
};

export default LicenseDashboard;