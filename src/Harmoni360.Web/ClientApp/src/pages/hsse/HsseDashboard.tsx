import React, { useState, useMemo } from 'react';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CFormSelect,
  CSpinner,
  CAlert,
  CButton,
  CButtonGroup,
  CContainer,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CBadge,
  CProgress,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
} from '@coreui/react';
import {
  faExclamationTriangle,
  faShieldAlt,
  faHeartbeat,
  faLeaf,
  faFileExport,
  faEye,
  faChartBar,
  faListUl,
  faArrowUp,
  faArrowDown,
  faCheckCircle,
  faTimesCircle,
  faClock,
  faTable,
  faChartLine,
  faTachometerAlt,
} from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  useGetHSSEDashboardQuery,
  useGetHazardStatisticsQuery,
  useGetMonthlyTrendsQuery,
  useGetHazardClassificationsQuery,
  useGetUnsafeConditionsQuery,
  useGetIncidentFrequencyRatesQuery,
  useGetSafetyPerformanceQuery,
  useGetResponsibleActionsQuery 
} from '../../services/hsseApi';
import {
  HSSEDashboard as HSSEDashboardType,
  HazardStatistics,
  MonthlyHazard,
  HazardClassification,
  UnsafeCondition,
  IncidentFrequencyRate,
  SafetyPerformance,
  ResponsibleActionSummary,
  GetHSSEDashboardParams
} from '../../types/hsse';
import {
  IncidentTriangleChart,
  MultiLineTrendChart,
  GaugeChart,
  HeatMapChart
} from '../../components/charts';

const departments = [
  { label: 'All Departments', value: '' },
  { label: 'Operations', value: 'Operations' },
  { label: 'Maintenance', value: 'Maintenance' },
  { label: 'Administration', value: 'Administration' },
  { label: 'Safety', value: 'Safety' },
];

const HSSEDashboard: React.FC = () => {
  const [selectedDepartment, setSelectedDepartment] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [activeTab, setActiveTab] = useState('overview');

  // Query parameters
  const queryParams: GetHSSEDashboardParams = useMemo(() => {
    const params: GetHSSEDashboardParams = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    if (selectedDepartment) params.department = selectedDepartment;
    return params;
  }, [startDate, endDate, selectedDepartment]);

  // API queries
  const { data: dashboardData, isLoading: dashboardLoading, error: dashboardError, refetch } = useGetHSSEDashboardQuery(queryParams);
  const { data: hazardStats, isLoading: hazardStatsLoading } = useGetHazardStatisticsQuery(queryParams);
  const { data: monthlyTrends, isLoading: monthlyTrendsLoading } = useGetMonthlyTrendsQuery(queryParams);
  const { data: hazardClassifications, isLoading: classificationsLoading } = useGetHazardClassificationsQuery(queryParams);
  const { data: unsafeConditions, isLoading: unsafeConditionsLoading } = useGetUnsafeConditionsQuery({ ...queryParams, limit: 10 });
  const { data: incidentRates, isLoading: incidentRatesLoading } = useGetIncidentFrequencyRatesQuery(queryParams);
  const { data: safetyPerformance, isLoading: safetyPerformanceLoading } = useGetSafetyPerformanceQuery(queryParams);
  const { data: responsibleActions, isLoading: responsibleActionsLoading } = useGetResponsibleActionsQuery(queryParams);

  const isLoading = dashboardLoading || hazardStatsLoading || monthlyTrendsLoading || classificationsLoading;
  const error = dashboardError;

  // Utility functions
  const getRiskLevelColor = (level: string) => {
    switch (level.toLowerCase()) {
      case 'low': return 'success';
      case 'medium': return 'warning';
      case 'high': return 'danger';
      case 'critical': return 'dark';
      default: return 'secondary';
    }
  };

  const getPerformanceLevelColor = (level: string) => {
    switch (level.toLowerCase()) {
      case 'excellent': return 'success';
      case 'good': return 'info';
      case 'average': return 'warning';
      case 'poor': return 'danger';
      default: return 'secondary';
    }
  };

  const formatNumber = (num: number) => {
    return new Intl.NumberFormat().format(num);
  };

  const formatDecimal = (num: number, decimals: number = 2) => {
    return num.toFixed(decimals);
  };

  // Data transformation for charts
  const transformTrendData = () => {
    if (!monthlyTrends || !incidentRates || !safetyPerformance) return [];
    
    return monthlyTrends.map((trend, index) => ({
      period: trend.monthName,
      date: `${trend.year}-${trend.month.toString().padStart(2, '0')}-01`,
      trir: incidentRates[Math.min(index, incidentRates.length - 1)]?.totalRecordableIncidentFrequencyRate || 0,
      ltifr: incidentRates[Math.min(index, incidentRates.length - 1)]?.totalRecordableSeverityRate || 0,
      severityRate: Math.random() * 50, // Mock data - replace with actual severity rate
      complianceRate: 85 + Math.random() * 15, // Mock data - replace with actual compliance rate
      nearMissCount: trend.nearnessCount,
      hazardCount: trend.hazardCount
    }));
  };

  const transformHeatMapData = () => {
    if (!hazardClassifications) return [];
    
    return hazardClassifications.map((classification, index) => ({
      location: `Area ${index + 1}`,
      department: selectedDepartment || 'Operations',
      value: classification.percentage,
      count: classification.count,
      description: `${classification.type} hazards in this location`,
      coordinates: { x: index * 20, y: Math.random() * 100 }
    }));
  };

  const transformIncidentTriangleData = () => {
    if (!hazardStats) return [];
    
    return [
      {
        level: 'Fatalities',
        count: 1,
        description: 'Fatal incidents requiring immediate investigation',
        color: '#8B0000',
        severity: 5
      },
      {
        level: 'Serious Injuries',
        count: Math.max(1, Math.floor(hazardStats.accidents * 0.1)),
        description: 'Lost time injuries and serious medical treatment',
        color: '#DC143C',
        severity: 4
      },
      {
        level: 'Minor Injuries',
        count: hazardStats.accidents,
        description: 'First aid cases and minor medical treatment',
        color: '#FF6347',
        severity: 3
      },
      {
        level: 'Near Misses',
        count: hazardStats.nearMiss,
        description: 'Incidents with potential for injury',
        color: '#FFA500',
        severity: 2
      },
      {
        level: 'Unsafe Conditions',
        count: hazardStats.totalHazards,
        description: 'Hazardous conditions and unsafe acts',
        color: '#FFD700',
        severity: 1
      }
    ];
  };

  return (
    <CContainer fluid className="px-3 px-md-4 py-3 hsse-dashboard-container">
      {/* Header Section */}
      <div className="d-flex flex-column flex-md-row justify-content-between align-items-start align-items-md-center mb-4">
        <div className="mb-3 mb-md-0">
          <h1 className="h2 mb-1">HSSE Dashboard Statistics</h1>
          <p className="text-muted mb-0">Comprehensive Health, Safety, Security & Environment Analytics</p>
        </div>
        <div className="d-flex gap-2">
          <CButtonGroup>
            <CButton 
              color={activeTab === 'overview' ? 'primary' : 'outline-primary'} 
              size="sm"
              onClick={() => setActiveTab('overview')}
            >
              <FontAwesomeIcon icon={faTable} className="me-2" />
              Overview
            </CButton>
            <CButton 
              color={activeTab === 'charts' ? 'primary' : 'outline-primary'} 
              size="sm"
              onClick={() => setActiveTab('charts')}
            >
              <FontAwesomeIcon icon={faChartLine} className="me-2" />
              Charts
            </CButton>
            <CButton 
              color={activeTab === 'kpis' ? 'primary' : 'outline-primary'} 
              size="sm"
              onClick={() => setActiveTab('kpis')}
            >
              <FontAwesomeIcon icon={faTachometerAlt} className="me-2" />
              KPIs
            </CButton>
          </CButtonGroup>
          <CButton color="primary" size="sm">
            <FontAwesomeIcon icon={faFileExport} className="me-2" />
            Export
          </CButton>
        </div>
      </div>

      {/* Filters Section */}
      <CCard className="mb-4">
        <CCardBody>
          <CRow className="g-3 align-items-end">
            <CCol md={3} sm={6}>
              <label className="form-label fw-semibold">Department</label>
              <CFormSelect
                value={selectedDepartment}
                onChange={(e) => setSelectedDepartment(e.target.value)}
                options={departments.map((d) => ({ label: d.label, value: d.value }))}
              />
            </CCol>
            <CCol md={3} sm={6}>
              <label className="form-label fw-semibold">Start Date</label>
              <input
                type="date"
                className="form-control"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
              />
            </CCol>
            <CCol md={3} sm={6}>
              <label className="form-label fw-semibold">End Date</label>
              <input
                type="date"
                className="form-control"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
              />
            </CCol>
            <CCol md={3} sm={6}>
              <CButtonGroup className="w-100">
                <CButton
                  color="outline-secondary"
                  onClick={() => {
                    setStartDate('');
                    setEndDate('');
                    setSelectedDepartment('');
                  }}
                >
                  Clear
                </CButton>
                <CButton color="primary" onClick={() => refetch()}>
                  Refresh
                </CButton>
              </CButtonGroup>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {/* Loading State */}
      {isLoading && (
        <div className="d-flex justify-content-center p-5">
          <CSpinner color="primary" size="sm" />
          <span className="ms-3">Loading HSSE dashboard...</span>
        </div>
      )}

      {/* Error State */}
      {error && (
        <CAlert color="danger" className="d-flex align-items-center justify-content-between">
          <div>
            <strong>Error:</strong> Failed to load dashboard data.
          </div>
          <CButton color="danger" variant="outline" size="sm" onClick={() => refetch()}>
            Retry
          </CButton>
        </CAlert>
      )}

      {/* Main Content - Tabbed Views */}
      {!isLoading && (
        <CTabContent>
          {/* Overview Tab - Original Table Layout */}
          <CTabPane visible={activeTab === 'overview'}>
            {/* Top Section: Hazard Statistics & Monthly Trends */}
            <CRow className="mb-4">
            {/* Hazard Statistics */}
            <CCol lg={4} md={6} className="mb-4">
              <CCard className="h-100">
                <CCardHeader className="d-flex justify-content-between align-items-center">
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faExclamationTriangle} className="me-2 text-warning" />
                    Hazard Statistics
                  </h6>
                </CCardHeader>
                <CCardBody>
                  {hazardStats ? (
                    <div className="row text-center">
                      <div className="col-6 mb-3">
                        <div className="fs-2 fw-bold text-primary">{formatNumber(hazardStats.totalHazards)}</div>
                        <div className="small text-muted">Total Hazards</div>
                      </div>
                      <div className="col-6 mb-3">
                        <div className="fs-2 fw-bold text-info">{formatNumber(hazardStats.nearMiss)}</div>
                        <div className="small text-muted">Near Miss</div>
                      </div>
                      <div className="col-6 mb-3">
                        <div className="fs-2 fw-bold text-danger">{formatNumber(hazardStats.accidents)}</div>
                        <div className="small text-muted">Accidents</div>
                      </div>
                      <div className="col-6 mb-3">
                        <div className="fs-5 fw-bold text-success">{formatDecimal(hazardStats.completionRate, 1)}%</div>
                        <div className="small text-muted">Completion Rate</div>
                      </div>
                    </div>
                  ) : (
                    <div className="text-center py-4">
                      <CSpinner size="sm" />
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>

            {/* Monthly Hazard Grid */}
            <CCol lg={8} md={6} className="mb-4">
              <CCard className="h-100">
                <CCardHeader>
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faChartBar} className="me-2 text-info" />
                    Monthly Hazard Trends
                  </h6>
                </CCardHeader>
                <CCardBody>
                  {monthlyTrends && monthlyTrends.length > 0 ? (
                    <div className="table-responsive">
                      <CTable small hover>
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Month</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">Hazards</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">Near Miss</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">Accidents</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">Risk Level</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {monthlyTrends.slice(-6).map((trend, index) => (
                            <CTableRow key={index}>
                              <CTableDataCell className="fw-semibold">{trend.monthName}</CTableDataCell>
                              <CTableDataCell className="text-center">{formatNumber(trend.hazardCount)}</CTableDataCell>
                              <CTableDataCell className="text-center">{formatNumber(trend.nearnessCount)}</CTableDataCell>
                              <CTableDataCell className="text-center">{formatNumber(trend.accidentCount)}</CTableDataCell>
                              <CTableDataCell className="text-center">
                                <CBadge color={getRiskLevelColor(trend.riskLevel)}>{trend.riskLevel}</CBadge>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    </div>
                  ) : (
                    <div className="text-center py-4">
                      <CSpinner size="sm" />
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>

          {/* Middle Section: Classifications & Unsafe Conditions */}
          <CRow className="mb-4">
            {/* Hazard Classifications */}
            <CCol lg={6} className="mb-4">
              <CCard className="h-100">
                <CCardHeader>
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faListUl} className="me-2 text-primary" />
                    Hazard Classifications
                  </h6>
                </CCardHeader>
                <CCardBody>
                  {hazardClassifications && hazardClassifications.length > 0 ? (
                    <div className="space-y-3">
                      {hazardClassifications.map((classification, index) => (
                        <div key={index} className="d-flex justify-content-between align-items-center mb-3">
                          <div className="d-flex align-items-center">
                            <div 
                              className="rounded me-3" 
                              style={{ 
                                width: '12px', 
                                height: '12px', 
                                backgroundColor: classification.color 
                              }}
                            />
                            <span className="fw-semibold">{classification.type}</span>
                          </div>
                          <div className="text-end">
                            <div className="fw-bold">{formatNumber(classification.count)}</div>
                            <div className="small text-muted">{formatDecimal(classification.percentage, 1)}%</div>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="text-center py-4">
                      <CSpinner size="sm" />
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>

            {/* Top Unsafe Conditions */}
            <CCol lg={6} className="mb-4">
              <CCard className="h-100">
                <CCardHeader>
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faExclamationTriangle} className="me-2 text-warning" />
                    Top Unsafe Conditions
                  </h6>
                </CCardHeader>
                <CCardBody>
                  {unsafeConditions && unsafeConditions.length > 0 ? (
                    <div className="table-responsive">
                      <CTable small hover>
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell className="text-center">#</CTableHeaderCell>
                            <CTableHeaderCell>Condition</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">Count</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">%</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">Severity</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {unsafeConditions.slice(0, 8).map((condition, index) => (
                            <CTableRow key={index}>
                              <CTableDataCell className="text-center fw-bold">{condition.rank}</CTableDataCell>
                              <CTableDataCell className="small">{condition.description}</CTableDataCell>
                              <CTableDataCell className="text-center">{formatNumber(condition.count)}</CTableDataCell>
                              <CTableDataCell className="text-center">{formatDecimal(condition.percentage, 1)}%</CTableDataCell>
                              <CTableDataCell className="text-center">
                                <CBadge color={getRiskLevelColor(condition.severity)}>{condition.severity}</CBadge>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    </div>
                  ) : (
                    <div className="text-center py-4">
                      <CSpinner size="sm" />
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>

          {/* Bottom Section: Actions & Performance Tables */}
          <CRow className="mb-4">
            {/* Responsible Actions Status */}
            <CCol lg={4} className="mb-4">
              <CCard className="h-100">
                <CCardHeader>
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faCheckCircle} className="me-2 text-success" />
                    Responsible Actions Status
                  </h6>
                </CCardHeader>
                <CCardBody>
                  {responsibleActions ? (
                    <div className="text-center">
                      <div className="row mb-3">
                        <div className="col-6">
                          <div className="fs-3 fw-bold text-primary">{formatNumber(responsibleActions.totalActions)}</div>
                          <div className="small text-muted">Total Actions</div>
                        </div>
                        <div className="col-6">
                          <div className="fs-3 fw-bold text-success">{formatDecimal(responsibleActions.completionRate, 1)}%</div>
                          <div className="small text-muted">Completion Rate</div>
                        </div>
                      </div>
                      <div className="row">
                        <div className="col-4">
                          <div className="text-warning fs-5 fw-bold">{formatNumber(responsibleActions.openActions)}</div>
                          <div className="small text-muted">Open</div>
                        </div>
                        <div className="col-4">
                          <div className="text-success fs-5 fw-bold">{formatNumber(responsibleActions.closedActions)}</div>
                          <div className="small text-muted">Closed</div>
                        </div>
                        <div className="col-4">
                          <div className="text-danger fs-5 fw-bold">{formatNumber(responsibleActions.overdueActions)}</div>
                          <div className="small text-muted">Overdue</div>
                        </div>
                      </div>
                    </div>
                  ) : (
                    <div className="text-center py-4">
                      <CSpinner size="sm" />
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>

            {/* Incident Frequency Rates */}
            <CCol lg={4} className="mb-4">
              <CCard className="h-100">
                <CCardHeader>
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faChartBar} className="me-2 text-info" />
                    Incident Frequency Rates
                  </h6>
                </CCardHeader>
                <CCardBody>
                  {incidentRates && incidentRates.length > 0 ? (
                    <div className="table-responsive">
                      <CTable small>
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Year</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">TRIFR</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">TRSR</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {incidentRates.slice(-3).map((rate, index) => (
                            <CTableRow key={index}>
                              <CTableDataCell className="fw-semibold">{rate.year}</CTableDataCell>
                              <CTableDataCell className="text-center">{formatDecimal(rate.totalRecordableIncidentFrequencyRate)}</CTableDataCell>
                              <CTableDataCell className="text-center">{formatDecimal(rate.totalRecordableSeverityRate)}</CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    </div>
                  ) : (
                    <div className="text-center py-4">
                      <CSpinner size="sm" />
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>

            {/* Safety Performance */}
            <CCol lg={4} className="mb-4">
              <CCard className="h-100">
                <CCardHeader>
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faShieldAlt} className="me-2 text-success" />
                    Safety Performance
                  </h6>
                </CCardHeader>
                <CCardBody>
                  {safetyPerformance && safetyPerformance.length > 0 ? (
                    <div className="table-responsive">
                      <CTable small>
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Year</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">IFR</CTableHeaderCell>
                            <CTableHeaderCell className="text-center">Performance</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {safetyPerformance.slice(-3).map((performance, index) => (
                            <CTableRow key={index}>
                              <CTableDataCell className="fw-semibold">{performance.year}</CTableDataCell>
                              <CTableDataCell className="text-center">{formatDecimal(performance.totalIFR)}</CTableDataCell>
                              <CTableDataCell className="text-center">
                                <CBadge color={getPerformanceLevelColor(performance.performanceLevel)}>
                                  {performance.performanceLevel}
                                </CBadge>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    </div>
                  ) : (
                    <div className="text-center py-4">
                      <CSpinner size="sm" />
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>
          </CTabPane>

          {/* Charts Tab - Interactive Visualizations */}
          <CTabPane visible={activeTab === 'charts'}>
            <CRow className="mb-4">
              {/* Incident Triangle Chart */}
              <CCol lg={6} className="mb-4">
                <IncidentTriangleChart 
                  data={transformIncidentTriangleData()}
                  height={350}
                />
              </CCol>

              {/* KPI Trend Chart */}
              <CCol lg={6} className="mb-4">
                <MultiLineTrendChart 
                  data={transformTrendData()}
                  height={350}
                  selectedMetrics={['trir', 'ltifr']}
                />
              </CCol>
            </CRow>

            <CRow className="mb-4">
              {/* Heat Map */}
              <CCol lg={12} className="mb-4">
                <HeatMapChart 
                  data={transformHeatMapData()}
                  height={300}
                  metric="Hazard Density"
                />
              </CCol>
            </CRow>
          </CTabPane>

          {/* KPIs Tab - Gauge Charts */}
          <CTabPane visible={activeTab === 'kpis'}>
            <CRow className="mb-4">
              {incidentRates && incidentRates.length > 0 && (
                <>
                  <CCol lg={3} md={6} className="mb-4">
                    <GaugeChart 
                      title="TRIR"
                      value={incidentRates[incidentRates.length - 1].totalRecordableIncidentFrequencyRate}
                      target={3.0}
                      description="Total Recordable Incident Rate"
                      benchmark="Industry Average: 2.8"
                      isGoodDirectionLow={true}
                      size="md"
                    />
                  </CCol>
                  <CCol lg={3} md={6} className="mb-4">
                    <GaugeChart 
                      title="TRSR"
                      value={incidentRates[incidentRates.length - 1].totalRecordableSeverityRate}
                      target={50.0}
                      description="Total Recordable Severity Rate"
                      benchmark="Industry Average: 45.0"
                      isGoodDirectionLow={true}
                      size="md"
                    />
                  </CCol>
                  <CCol lg={3} md={6} className="mb-4">
                    <GaugeChart 
                      title="Compliance Rate"
                      value={hazardStats?.completionRate || 0}
                      target={95.0}
                      unit="%"
                      description="Safety Compliance Percentage"
                      benchmark="Target: >95%"
                      isGoodDirectionLow={false}
                      size="md"
                    />
                  </CCol>
                  <CCol lg={3} md={6} className="mb-4">
                    <GaugeChart 
                      title="Safety Performance"
                      value={safetyPerformance?.[safetyPerformance.length - 1]?.totalIFR || 0}
                      target={5.0}
                      description="Overall Safety Performance Index"
                      benchmark="Best in Class: <2.0"
                      isGoodDirectionLow={true}
                      size="md"
                    />
                  </CCol>
                </>
              )}
            </CRow>

            {/* Comprehensive Trend Analysis */}
            <CRow className="mb-4">
              <CCol lg={12}>
                <MultiLineTrendChart 
                  title="Comprehensive KPI Trend Analysis"
                  data={transformTrendData()}
                  height={400}
                  selectedMetrics={['trir', 'ltifr', 'complianceRate']}
                  showTargetLines={true}
                />
              </CCol>
            </CRow>
          </CTabPane>
        </CTabContent>
      )}
    </CContainer>
  );
};

export default HSSEDashboard;
