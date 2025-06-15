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
import { useGetHealthDashboardQuery } from '../../features/health/healthApi';
import { StatsCard, ProgressCard, ChartCard, RecentItemsList } from '../../components/dashboard';
import { formatDate } from '../../utils/dateUtils';
import { HealthDashboardDto, HealthIncidentDto, UpcomingVaccinationDto } from '../../types/health';

const HealthDashboard: React.FC = () => {
  const {
    data: dashboardData,
    isLoading,
    error,
    refetch
  } = useGetHealthDashboardQuery();

  const complianceChartData = useMemo(() => {
    if (!dashboardData?.vaccinationCompliance) return null;

    const compliance = dashboardData.vaccinationCompliance;
    return {
      labels: ['Compliant', 'Overdue', 'Exempted'],
      datasets: [{
        data: [compliance.totalCompliant, compliance.totalOverdue, compliance.totalExempted],
        backgroundColor: ['#20c997', '#dc3545', '#ffc107'],
        borderWidth: 0
      }]
    };
  }, [dashboardData]);

  const incidentTrendsChartData = useMemo(() => {
    if (!dashboardData?.healthIncidentTrends) return null;

    const trends = dashboardData.healthIncidentTrends;
    return {
      labels: trends.map(t => t.period),
      datasets: [{
        label: 'Health Incidents',
        data: trends.map(t => t.totalIncidents),
        borderColor: '#321fdb',
        backgroundColor: 'rgba(50, 31, 219, 0.1)',
        tension: 0.4,
        fill: true
      }]
    };
  }, [dashboardData]);

  const formatUpcomingVaccination = (vaccination: UpcomingVaccinationDto) => ({
    id: vaccination.healthRecordId,
    title: `${vaccination.personName} - ${vaccination.vaccineName}`,
    subtitle: `Due: ${formatDate(vaccination.dueDate)}`,
    status: vaccination.isOverdue ? 'danger' : vaccination.daysUntilDue <= 7 ? 'warning' : 'success',
    timestamp: vaccination.dueDate,
    badge: vaccination.isOverdue ? 'Overdue' : `${vaccination.daysUntilDue} days`
  });

  const formatRecentIncident = (incident: HealthIncidentDto) => ({
    id: incident.id,
    title: `${incident.personName} - ${incident.type}`,
    subtitle: incident.symptoms || 'No symptoms recorded',
    status: incident.severity === 'Critical' ? 'danger' : 
            incident.severity === 'Severe' ? 'warning' : 
            incident.severity === 'Moderate' ? 'info' : 'success',
    timestamp: incident.dateOccurred,
    badge: incident.severity
  });

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '400px' }}>
        <CSpinner color="primary" size="lg" />
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
          Last updated: {formatDate(dashboardData.lastUpdated)}
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
            value={dashboardData.studentHealthRecords}
            icon={faUser}
            color="info"
            subtitle="health records"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Staff"
            value={dashboardData.staffHealthRecords}
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
              <CBadge color={dashboardData.vaccinationCompliance.complianceRate >= 95 ? 'success' : 
                            dashboardData.vaccinationCompliance.complianceRate >= 85 ? 'warning' : 'danger'}>
                {dashboardData.vaccinationCompliance.complianceRate.toFixed(1)}% Compliant
              </CBadge>
            </CCardHeader>
            <CCardBody>
              <CRow>
                <CCol md={6}>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between align-items-center mb-1">
                      <span>Compliant</span>
                      <span className="text-success fw-bold">
                        {dashboardData.vaccinationCompliance.totalCompliant}
                      </span>
                    </div>
                    <CProgress 
                      value={(dashboardData.vaccinationCompliance.totalCompliant / dashboardData.vaccinationCompliance.totalRequired) * 100}
                      color="success"
                      className="mb-2"
                    />
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between align-items-center mb-1">
                      <span>Overdue</span>
                      <span className="text-danger fw-bold">
                        {dashboardData.vaccinationCompliance.totalOverdue}
                      </span>
                    </div>
                    <CProgress 
                      value={(dashboardData.vaccinationCompliance.totalOverdue / dashboardData.vaccinationCompliance.totalRequired) * 100}
                      color="danger"
                      className="mb-2"
                    />
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between align-items-center mb-1">
                      <span>Exempted</span>
                      <span className="text-warning fw-bold">
                        {dashboardData.vaccinationCompliance.totalExempted}
                      </span>
                    </div>
                    <CProgress 
                      value={(dashboardData.vaccinationCompliance.totalExempted / dashboardData.vaccinationCompliance.totalRequired) * 100}
                      color="warning"
                    />
                  </div>
                </CCol>
                <CCol md={6}>
                  {complianceChartData && (
                    <ChartCard
                      title=""
                      type="doughnut"
                      data={complianceChartData}
                      height={200}
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
                  color={dashboardData.healthRiskSummary.riskLevel === 'Low' ? 'success' :
                         dashboardData.healthRiskSummary.riskLevel === 'Medium' ? 'warning' : 'danger'}
                  className="me-2"
                >
                  {dashboardData.healthRiskSummary.riskLevel} Risk
                </CBadge>
              </div>
              <div className="small text-muted mb-3">
                <div className="mb-1">
                  <FontAwesomeIcon icon={faUsers} className="me-1" />
                  {dashboardData.healthRiskSummary.highRiskIndividuals} high-risk individuals
                </div>
                <div className="mb-1">
                  <FontAwesomeIcon icon={faMedkit} className="me-1" />
                  {dashboardData.healthRiskSummary.criticalConditions} critical conditions
                </div>
                <div className="mb-1">
                  <FontAwesomeIcon icon={faShieldAlt} className="me-1" />
                  {dashboardData.healthRiskSummary.overdueVaccinations} overdue vaccinations
                </div>
                <div>
                  <FontAwesomeIcon icon={faBell} className="me-1" />
                  {dashboardData.healthRiskSummary.missingEmergencyContacts} missing contacts
                </div>
              </div>
              {dashboardData.healthRiskSummary.recommendations.length > 0 && (
                <div>
                  <strong className="small">Recommendations:</strong>
                  <ul className="small text-muted mt-1 mb-0">
                    {dashboardData.healthRiskSummary.recommendations.map((rec, index) => (
                      <li key={index}>{rec}</li>
                    ))}
                  </ul>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Trends and Activities Row */}
      <CRow className="mb-4">
        <CCol lg={6}>
          {incidentTrendsChartData && (
            <ChartCard
              title="Health Incident Trends"
              type="line"
              data={incidentTrendsChartData}
              height={300}
            />
          )}
        </CCol>
        <CCol lg={6}>
          <CCard className="h-100">
            <CCardHeader>
              <strong>Upcoming Vaccinations</strong>
            </CCardHeader>
            <CCardBody className="p-0">
              {dashboardData.upcomingVaccinations.length > 0 ? (
                <RecentItemsList
                  items={dashboardData.upcomingVaccinations.map(formatUpcomingVaccination)}
                  maxItems={6}
                />
              ) : (
                <div className="p-3 text-muted text-center">
                  No upcoming vaccinations
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
              {dashboardData.recentHealthIncidents.length > 0 ? (
                <RecentItemsList
                  items={dashboardData.recentHealthIncidents.map(formatRecentIncident)}
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