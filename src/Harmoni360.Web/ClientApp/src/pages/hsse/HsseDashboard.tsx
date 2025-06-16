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
  faCalculator,
  faChartLine,
  faTachometerAlt,
  faArrowUp,
  faArrowDown,
} from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { useGetStatisticsQuery, useLazyExportStatisticsQuery, useGetTrendsQuery } from '../../features/statistics/statisticsApi';
import {
  KpiInputs,
  KpiGaugeChart,
  MultiLineTrendChart,
  ResponsiveStatsCard,
} from '../../components/hsse';
import { useSignalR } from '../../hooks/useSignalR';
import { useResponsive, useResponsiveValue, useChartDimensions } from '../../hooks/useResponsive';
import { KpiInputs as KpiInputsType, KpiMetric } from '../../types/hsse';
import SkeletonLoader from '../../components/common/SkeletonLoader';

const modules = [
  { label: 'All Modules', value: 'All' },
  { label: 'Health', value: 'HealthMonitoring' },
  { label: 'Safety', value: 'IncidentManagement' },
  { label: 'Security', value: 'SecurityIncidentManagement' },
  { label: 'Environment', value: 'RiskManagement' },
];

const HsseDashboard: React.FC = () => {
  const [selectedModule, setSelectedModule] = useState('All');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [activeTab, setActiveTab] = useState('kpi-dashboard');
  const [kpiInputs, setKpiInputs] = useState<KpiInputsType>({
    hoursWorked: 2080000,
    lostTimeInjuries: 0,
    daysLost: 0,
    compliantRecords: 0,
    totalRecords: 0,
  });

  // Responsive hooks
  const { isMobile, isTablet, isDesktop } = useResponsive();
  const chartHeight = useChartDimensions(400).height;
  
  // Responsive grid columns
  const kpiColumns = useResponsiveValue({
    mobile: 1,
    tablet: 2,
    desktop: 4,
    default: 4,
  });
  
  const { data, isLoading, error, refetch } = useGetStatisticsQuery({
    module: selectedModule === 'All' ? undefined : selectedModule,
    startDate: startDate || undefined,
    endDate: endDate || undefined,
  });
  
  const { data: trendData, isLoading: trendsLoading } = useGetTrendsQuery({
    module: selectedModule === 'All' ? undefined : selectedModule,
    startDate: startDate || undefined,
    endDate: endDate || undefined,
  });
  
  const [triggerExport, { isLoading: exportLoading }] = useLazyExportStatisticsQuery();
  useSignalR();

  const handleExport = async () => {
    try {
      const res = await triggerExport({
        module: selectedModule === 'All' ? undefined : selectedModule,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
      }).unwrap();
      const url = window.URL.createObjectURL(res);
      const link = document.createElement('a');
      link.href = url;
      link.download = `hsse_statistics_${selectedModule}_${new Date().toISOString().slice(0,10)}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Export failed:', error);
    }
  };

  const handleKpiCalculate = () => {
    refetch();
  };

  const kpiMetrics: KpiMetric[] = useMemo(() => {
    if (!data) return [];
    
    return [
      {
        value: data.trir || 0,
        target: 3.0,
        title: 'TRIR',
        description: 'Total Recordable Incident Rate',
        benchmark: 'Industry Average: 2.8',
        isGoodDirectionLow: true,
      },
      {
        value: data.ltifr || 0,
        target: 1.0,
        title: 'LTIFR',
        description: 'Lost Time Injury Frequency Rate',
        benchmark: 'Industry Average: 0.8',
        isGoodDirectionLow: true,
      },
      {
        value: data.severityRate || 0,
        target: 50.0,
        title: 'Severity Rate',
        description: 'Days Lost per 200,000 Hours',
        benchmark: 'Industry Average: 45',
        isGoodDirectionLow: true,
      },
      {
        value: data.complianceRate || 0,
        target: 95.0,
        title: 'Compliance Rate',
        description: 'Safety Compliance Percentage',
        benchmark: 'Target: >95%',
        isGoodDirectionLow: false,
      },
    ];
  }, [data]);

  return (
    <CContainer fluid className="px-3 px-md-4 py-3 hsse-dashboard-container fade-in">
      {/* Header Section */}
      <div className="d-flex flex-column flex-md-row justify-content-between align-items-start align-items-md-center mb-4">
        <div className="mb-3 mb-md-0">
          <h1 className="h2 mb-1">HSSE Statistics Dashboard</h1>
          <p className="text-muted mb-0">Health, Safety, Security & Environment Analytics</p>
        </div>
        <CButton
          color="primary"
          onClick={handleExport}
          disabled={exportLoading}
          className="d-flex align-items-center"
        >
          <FontAwesomeIcon icon={faFileExport} className="me-2" />
          {exportLoading ? 'Exporting...' : 'Export PDF'}
        </CButton>
      </div>

      {/* Filters Section */}
      <CCard className="mb-4">
        <CCardBody>
          <CRow className="g-3 align-items-end">
            <CCol md={3} sm={6}>
              <label className="form-label fw-semibold">Module</label>
              <CFormSelect
                value={selectedModule}
                onChange={(e) => setSelectedModule(e.target.value)}
                options={modules.map((m) => ({ label: m.label, value: m.value }))}
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
          <span className="ms-3">Loading HSSE statistics...</span>
        </div>
      )}

      {/* Error State */}
      {error && (
        <CAlert color="danger" className="d-flex align-items-center justify-content-between">
          <div>
            <strong>Error:</strong> Failed to load statistics.
          </div>
          <CButton color="danger" variant="outline" size="sm" onClick={() => refetch()}>
            Retry
          </CButton>
        </CAlert>
      )}

      {/* Main Content */}
      {data && (
        <>
          {/* Statistics Overview Cards */}
          <CRow className="mb-4">
            <CCol xl={3} lg={6} md={6} sm={6} className="mb-3">
              <ResponsiveStatsCard
                title="Safety Incidents"
                value={data.totalIncidents}
                icon={faExclamationTriangle}
                color="danger"
                subtitle="Total recorded incidents"
              />
            </CCol>
            <CCol xl={3} lg={6} md={6} sm={6} className="mb-3">
              <ResponsiveStatsCard
                title="Security Events"
                value={data.totalSecurityIncidents}
                icon={faShieldAlt}
                color="warning"
                subtitle="Security-related incidents"
              />
            </CCol>
            <CCol xl={3} lg={6} md={6} sm={6} className="mb-3">
              <ResponsiveStatsCard
                title="Health Incidents"
                value={data.totalHealthIncidents}
                icon={faHeartbeat}
                color="info"
                subtitle="Health monitoring events"
              />
            </CCol>
            <CCol xl={3} lg={6} md={6} sm={6} className="mb-3">
              <ResponsiveStatsCard
                title="Hazards Identified"
                value={data.totalHazards}
                icon={faLeaf}
                color="success"
                subtitle="Environmental & safety hazards"
              />
            </CCol>
          </CRow>

          {/* Tabbed Content */}
          <CCard className="mb-4">
            <CCardHeader className="bg-light">
              <CNav variant="tabs" className="card-header-tabs">
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'kpi-dashboard'}
                    onClick={() => setActiveTab('kpi-dashboard')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={faTachometerAlt} className="me-2" />
                    KPI Dashboard
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'trend-analysis'}
                    onClick={() => setActiveTab('trend-analysis')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={faChartLine} className="me-2" />
                    Trend Analysis
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'kpi-calculator'}
                    onClick={() => setActiveTab('kpi-calculator')}
                    style={{ cursor: 'pointer' }}
                  >
                    <FontAwesomeIcon icon={faCalculator} className="me-2" />
                    KPI Calculator
                  </CNavLink>
                </CNavItem>
              </CNav>
            </CCardHeader>
            
            <CCardBody>
              <CTabContent>
                {/* KPI Dashboard Tab */}
                <CTabPane visible={activeTab === 'kpi-dashboard'}>
                {isLoading ? (
                  <CRow>
                    {[1, 2, 3, 4].map((i) => (
                      <CCol xl={3} lg={6} md={6} sm={12} key={i} className="mb-4">
                        <SkeletonLoader type="gauge" showHeader={true} />
                      </CCol>
                    ))}
                  </CRow>
                ) : (
                  <>
                    {/* KPI Gauges Grid */}
                    <div 
                      className="dashboard-grid mb-4"
                      style={{
                        display: 'grid',
                        gridTemplateColumns: `repeat(${kpiColumns}, 1fr)`,
                        gap: isMobile ? '1rem' : isTablet ? '1.25rem' : '1.5rem'
                      }}
                    >
                      {kpiMetrics.map((metric, index) => (
                        <KpiGaugeChart 
                          key={index} 
                          metric={metric} 
                          size={isMobile ? 'sm' : isTablet ? 'md' : 'md'}
                        />
                      ))}
                    </div>
                    
                    {/* KPI Stats Cards */}
                    <div 
                      className="dashboard-grid"
                      style={{
                        display: 'grid',
                        gridTemplateColumns: `repeat(${kpiColumns}, 1fr)`,
                        gap: isMobile ? '0.75rem' : isTablet ? '1rem' : '1.25rem'
                      }}
                    >
                      {kpiMetrics.map((metric, index) => (
                        <ResponsiveStatsCard
                          key={`card-${index}`}
                          title={metric.title}
                          value={metric.value.toFixed(2)}
                          isKpi={true}
                          target={metric.target}
                          benchmark={metric.benchmark}
                          subtitle={metric.description}
                          size={isMobile ? 'sm' : 'sm'}
                        />
                      ))}
                    </div>
                  </>
                )}
              </CTabPane>

                {/* Trend Analysis Tab */}
                <CTabPane visible={activeTab === 'trend-analysis'}>
                {trendsLoading ? (
                  <SkeletonLoader type="chart" size="lg" showHeader={true} />
                ) : (
                  <MultiLineTrendChart
                    data={trendData || []}
                    title="HSSE Trends Over Time"
                    height={chartHeight}
                    showLegend={!isMobile}
                  />
                )}
              </CTabPane>

                {/* KPI Calculator Tab */}
                <CTabPane visible={activeTab === 'kpi-calculator'}>
                <KpiInputs
                  inputs={kpiInputs}
                  onInputChange={setKpiInputs}
                  onCalculate={handleKpiCalculate}
                  isLoading={isLoading}
                />
                </CTabPane>
              </CTabContent>
            </CCardBody>
          </CCard>
        </>
      )}
    </CContainer>
  );
};

export default HsseDashboard;
