import React, { useMemo } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CSpinner,
  CAlert,
  CBadge,
  CListGroup,
  CListGroupItem,
  CProgress,
  CButton
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faHeartbeat,
  faShieldAlt,
  faMedkit,
  faExclamationTriangle,
  faUsers,
  faUser,
  faCalendarAlt,
  faChartPie,
  faBell
} from '@fortawesome/free-solid-svg-icons';
import { useGetHealthDashboardQuery, HealthDashboardDto, HealthIncidentDto } from '../../features/health/healthApi';
import { StatsCard, ProgressCard, ChartCard, RecentItemsList, DonutChart, LineChart } from '../../components/dashboard';
import { formatDate } from '../../utils/dateUtils';

const HealthDashboard: React.FC = () => {
  const {
    data: dashboardData,
    isLoading,
    error,
    refetch
  } = useGetHealthDashboardQuery({});

  const complianceChartData = useMemo(() => {
    if (!dashboardData?.vaccinationsByStatus) return null;

    const statusData = dashboardData.vaccinationsByStatus;
    const compliantCount = statusData.find(s => s.status === 'Administered')?.count || 0;
    const overdueCount = statusData.find(s => s.status === 'Overdue')?.count || 0;
    const exemptedCount = statusData.find(s => s.status === 'Exempted')?.count || 0;
    
    return {
      labels: ['Compliant', 'Overdue', 'Exempted'],
      datasets: [{
        data: [compliantCount, overdueCount, exemptedCount],
        backgroundColor: ['#20c997', '#dc3545', '#ffc107'],
        borderWidth: 0
      }]
    };
  }, [dashboardData]);

  const incidentTrendsChartData = useMemo(() => {
    if (!dashboardData?.healthIncidentTrends) return null;

    const trends = dashboardData.healthIncidentTrends;
    return {
      labels: trends.map(t => t.date || ''),
      datasets: [{
        label: 'Health Incidents',
        data: trends.map(t => t.count || 0),
        borderColor: '#321fdb',
        backgroundColor: 'rgba(50, 31, 219, 0.1)',
        tension: 0.4,
        fill: true
      }]
    };
  }, [dashboardData]);

  const formatUpcomingVaccination = (vaccination: any) => ({
    id: (vaccination.vaccinationId || vaccination.healthRecordId)?.toString(),
    title: `${vaccination.personName} - ${vaccination.vaccineName}`,
    subtitle: `Due: ${formatDate(vaccination.expiryDate)}`,
    status: vaccination.isExpired ? 'danger' : vaccination.daysUntilExpiry <= 7 ? 'warning' : 'success',
    timestamp: vaccination.expiryDate,
    badge: vaccination.isExpired ? 'Expired' : `${vaccination.daysUntilExpiry} days`
  });

  const formatRecentIncident = (incident: HealthIncidentDto) => ({
    id: incident.id?.toString(),
    title: `${(incident as any).personName || 'Unknown'} - ${incident.type}`,
    subtitle: incident.symptoms || 'No symptoms recorded',
    status: incident.severity === 'Critical' ? 'danger' : 
            incident.severity === 'Severe' ? 'warning' : 
            incident.severity === 'Moderate' ? 'info' : 'success',
    timestamp: incident.incidentDateTime,
    badge: incident.severity
  });

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '400px' }}>
        <CSpinner color="primary" size="sm" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger" className="d-flex align-items-center">
        <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="lg" />
        <div>
          Failed to load health dashboard data. 
          <CButton color="link" onClick={() => refetch()} className="p-0 ms-2">
            Try again
          </CButton>
        </div>
      </CAlert>
    );
  }

  if (!dashboardData) {
    return (
      <CAlert color="info">
        No health dashboard data available.
      </CAlert>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Health Dashboard</h2>
        <div className="text-muted">
          Last updated: {formatDate(dashboardData.toDate)}
        </div>
      </div>

      {/* Key Metrics Row */}
      <CRow className="mb-4">
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Total Health Records"
            value={dashboardData.totalHealthRecords}
            icon={faHeartbeat}
            color="primary"
            subtitle={`${dashboardData.activeHealthRecords} active`}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Students"
            value={dashboardData.totalStudentRecords}
            icon={faUser}
            color="info"
            subtitle="health records"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Staff"
            value={dashboardData.totalStaffRecords}
            icon={faUser}
            color="warning"
            subtitle="health records"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Critical Conditions"
            value={dashboardData.criticalMedicalConditions}
            icon={faExclamationTriangle}
            color="danger"
            subtitle="requiring attention"
          />
        </CCol>
      </CRow>

      {/* Vaccination Compliance Row */}
      <CRow className="mb-4">
        <CCol lg={8}>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <strong>Vaccination Compliance</strong>
              <CBadge color={dashboardData.vaccinationComplianceRate >= 95 ? 'success' : 
                            dashboardData.vaccinationComplianceRate >= 85 ? 'warning' : 'danger'}>
                {dashboardData.vaccinationComplianceRate.toFixed(1)}% Compliant
              </CBadge>
            </CCardHeader>
            <CCardBody>
              <CRow>
                <CCol md={6}>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between align-items-center mb-1">
                      <span>Compliant</span>
                      <span className="text-success fw-bold">
                        {dashboardData.vaccinationsByStatus?.find(s => s.status === 'Administered')?.count || 0}
                      </span>
                    </div>
                    <CProgress 
                      value={dashboardData.vaccinationsByStatus?.find(s => s.status === 'Administered')?.percentage || 0}
                      color="success"
                      className="mb-2"
                    />
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between align-items-center mb-1">
                      <span>Overdue</span>
                      <span className="text-danger fw-bold">
                        {dashboardData.overdueVaccinations}
                      </span>
                    </div>
                    <CProgress 
                      value={dashboardData.vaccinationsByStatus?.find(s => s.status === 'Overdue')?.percentage || 0}
                      color="danger"
                      className="mb-2"
                    />
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between align-items-center mb-1">
                      <span>Exempted</span>
                      <span className="text-warning fw-bold">
                        {dashboardData.vaccinationsByStatus?.find(s => s.status === 'Exempted')?.count || 0}
                      </span>
                    </div>
                    <CProgress 
                      value={dashboardData.vaccinationsByStatus?.find(s => s.status === 'Exempted')?.percentage || 0}
                      color="warning"
                    />
                  </div>
                </CCol>
                <CCol md={6}>
                  {complianceChartData && (
                    <DonutChart
                      data={complianceChartData.datasets[0].data.map((value, index) => ({
                        label: complianceChartData.labels[index],
                        value: value,
                        color: complianceChartData.datasets[0].backgroundColor[index]
                      }))}
                      size={200}
                    />
                  )}
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol lg={4}>
          <CCard className="h-100">
            <CCardHeader>
              <strong>Health Risk Summary</strong>
            </CCardHeader>
            <CCardBody>
              <div className="d-flex align-items-center mb-3">
                <CBadge 
                  color={dashboardData.criticalHealthIncidents > 0 ? 'danger' :
                         dashboardData.unresolvedHealthIncidents > 5 ? 'warning' : 'success'}
                  className="me-2"
                >
                  {dashboardData.criticalHealthIncidents > 0 ? 'High' :
                   dashboardData.unresolvedHealthIncidents > 5 ? 'Medium' : 'Low'} Risk
                </CBadge>
              </div>
              <div className="small text-muted mb-3">
                <div className="mb-1">
                  <FontAwesomeIcon icon={faUsers} className="me-1" />
                  {dashboardData.lifeThreateningConditions} life-threatening conditions
                </div>
                <div className="mb-1">
                  <FontAwesomeIcon icon={faMedkit} className="me-1" />
                  {dashboardData.criticalMedicalConditions} critical conditions
                </div>
                <div className="mb-1">
                  <FontAwesomeIcon icon={faShieldAlt} className="me-1" />
                  {dashboardData.overdueVaccinations} overdue vaccinations
                </div>
                <div>
                  <FontAwesomeIcon icon={faBell} className="me-1" />
                  {dashboardData.primaryContactsMissing} missing primary contacts
                </div>
              </div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Trends and Activities Row */}
      <CRow className="mb-4">
        <CCol lg={6}>
          <ChartCard title="Health Incident Trends">
            {incidentTrendsChartData && (
              <LineChart
                data={incidentTrendsChartData.labels.map((label, index) => ({
                  label: label,
                  value: incidentTrendsChartData.datasets[0].data[index]
                }))}
                height={280}
              />
            )}
          </ChartCard>
        </CCol>
        <CCol lg={6}>
          <CCard className="h-100">
            <CCardHeader>
              <strong>Upcoming Vaccinations</strong>
            </CCardHeader>
            <CCardBody className="p-0">
              {dashboardData.expiringVaccinationDetails?.length > 0 ? (
                <RecentItemsList
                  items={dashboardData.expiringVaccinationDetails.map(formatUpcomingVaccination)}
                  maxItems={6}
                />
              ) : (
                <div className="p-3 text-muted text-center">
                  No expiring vaccinations
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Recent Health Incidents */}
      <CRow>
        <CCol lg={12}>
          <CCard>
            <CCardHeader>
              <strong>Recent Health Incidents</strong>
            </CCardHeader>
            <CCardBody className="p-0">
              {dashboardData.recentHealthIncidentDetails?.length > 0 ? (
                <RecentItemsList
                  items={dashboardData.recentHealthIncidentDetails.map(formatRecentIncident)}
                  maxItems={8}
                />
              ) : (
                <div className="p-3 text-muted text-center">
                  No recent health incidents
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </div>
  );
};

export default HealthDashboard;