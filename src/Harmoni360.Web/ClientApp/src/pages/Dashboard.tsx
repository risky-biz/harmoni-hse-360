import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CBadge,
  CWidgetStatsA,
  CAlert,
  CButton,
  CSpinner,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../utils/iconMappings';

import { useAuth } from '../hooks/useAuth';
// import { useGetIncidentStatisticsQuery } from '../features/incidents/incidentApi';

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  // Temporarily disable API call for debugging
  // const { data: stats, isLoading: statsLoading, error: statsError } = useGetIncidentStatisticsQuery();
  const stats = {
    totalIncidents: 0,
    openIncidents: 0,
    closedIncidents: 0,
    criticalIncidents: 0,
  };
  const statsLoading = false;
  const statsError = null;

  // Debug logging
  console.log('Dashboard component rendering', { user, stats });

  if (!user) {
    console.log('No user found in Dashboard');
    return <div>Loading user...</div>;
  }

  return (
    <>
      <div className="mb-4">
        <h1>Dashboard</h1>
        <p className="text-medium-emphasis">
          Welcome back, {user?.name}! Here's your HSE overview.
        </p>
      </div>

      <CAlert color="info" className="d-flex align-items-center">
        <FontAwesomeIcon icon={CONTEXT_ICONS.vaccination} className="flex-shrink-0 me-2" />
        <div>
          <strong>Welcome to Harmoni360!</strong> This system manages health,
          safety, and environmental data for British School Jakarta.
          <CButton
            color="primary"
            size="sm"
            className="ms-3"
            onClick={() => navigate('/incidents/create')}
          >
            <FontAwesomeIcon icon={ACTION_ICONS.add} size="sm" className="me-1" />
            Report Incident
          </CButton>
        </div>
      </CAlert>

      {statsError && (
        <CAlert color="warning" className="mb-4">
          <strong>Unable to load statistics.</strong> Please ensure you're
          logged in and try refreshing the page.
        </CAlert>
      )}

      <CRow className="mb-4">
        <CCol sm={6} lg={3}>
          <CWidgetStatsA
            className="mb-4 dashboard-widget"
            color="primary"
            value={
              statsLoading ? (
                <CSpinner size="sm" />
              ) : (
                <>
                  {stats?.totalIncidents || 0}{' '}
                  <span className="fs-6 fw-normal">incidents</span>
                </>
              )
            }
            title="Total Incidents"
            action={
              <FontAwesomeIcon
                icon={CONTEXT_ICONS.incident}
                size="2x"
                className="my-4 text-white"
              />
            }
            onClick={() => navigate('/incidents')}
            style={{ cursor: 'pointer' }}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <CWidgetStatsA
            className="mb-4 dashboard-widget"
            color="warning"
            value={
              statsLoading ? (
                <CSpinner size="sm" />
              ) : (
                <>
                  {stats?.openIncidents || 0}{' '}
                  <span className="fs-6 fw-normal">open</span>
                </>
              )
            }
            title="Open Incidents"
            action={
              <FontAwesomeIcon icon={CONTEXT_ICONS.report} size="2x" className="my-4 text-white" />
            }
            onClick={() =>
              navigate(
                '/incidents?status=Reported,UnderInvestigation,AwaitingAction'
              )
            }
            style={{ cursor: 'pointer' }}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <CWidgetStatsA
            className="mb-4 dashboard-widget"
            color="danger"
            value={
              statsLoading ? (
                <CSpinner size="sm" />
              ) : (
                <>
                  {stats?.criticalIncidents || 0}{' '}
                  <span className="fs-6 fw-normal">critical</span>
                </>
              )
            }
            title="Critical Incidents"
            action={
              <FontAwesomeIcon
                icon={CONTEXT_ICONS.incident}
                size="2x"
                className="my-4 text-white"
              />
            }
            onClick={() => navigate('/incidents?severity=Critical')}
            style={{ cursor: 'pointer' }}
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <CWidgetStatsA
            className="mb-4 dashboard-widget"
            color="success"
            value={
              statsLoading ? (
                <CSpinner size="sm" />
              ) : (
                <>
                  {stats?.closedIncidents || 0}{' '}
                  <span className="fs-6 fw-normal">resolved</span>
                </>
              )
            }
            title="Resolved Incidents"
            action={
              <FontAwesomeIcon
                icon={CONTEXT_ICONS.vaccination}
                size="2x"
                className="my-4 text-white"
              />
            }
            onClick={() => navigate('/incidents?status=Resolved,Closed')}
            style={{ cursor: 'pointer' }}
          />
        </CCol>
      </CRow>

      <CRow>
        <CCol md={6}>
          <CCard className="mb-4">
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <strong>Quick Actions</strong>
            </CCardHeader>
            <CCardBody>
              <div className="d-grid gap-3">
                <CButton
                  color="primary"
                  className="d-flex align-items-center justify-content-between"
                  onClick={() => navigate('/incidents/create')}
                >
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon icon={CONTEXT_ICONS.incident} className="me-2" />
                    Report New Incident
                  </div>
                  <FontAwesomeIcon icon={ACTION_ICONS.next} size="sm" />
                </CButton>

                <CButton
                  color="secondary"
                  variant="outline"
                  className="d-flex align-items-center justify-content-between"
                  onClick={() => navigate('/incidents')}
                >
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon icon={CONTEXT_ICONS.report} className="me-2" />
                    View All Incidents
                  </div>
                  <FontAwesomeIcon icon={ACTION_ICONS.next} size="sm" />
                </CButton>

                <CButton
                  color="secondary"
                  variant="outline"
                  className="d-flex align-items-center justify-content-between"
                  onClick={() => navigate('/incidents/my-reports')}
                >
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon icon={CONTEXT_ICONS.report} className="me-2" />
                    My Incident Reports
                  </div>
                  <FontAwesomeIcon icon={ACTION_ICONS.next} size="sm" />
                </CButton>

                <CButton
                  color="success"
                  variant="outline"
                  className="d-flex align-items-center justify-content-between"
                  disabled
                >
                  <div className="d-flex align-items-center">
                    <FontAwesomeIcon icon={CONTEXT_ICONS.vaccination} className="me-2" />
                    Risk Assessment (Coming Soon)
                  </div>
                  <FontAwesomeIcon icon={ACTION_ICONS.next} size="sm" />
                </CButton>
              </div>
            </CCardBody>
          </CCard>
        </CCol>

        <CCol md={6}>
          <CCard className="mb-4">
            <CCardHeader>
              <strong>Your Profile</strong>
            </CCardHeader>
            <CCardBody>
              <div className="mb-3">
                <strong>Name:</strong> {user?.name}
              </div>
              <div className="mb-3">
                <strong>Employee ID:</strong> {user?.employeeId}
              </div>
              <div className="mb-3">
                <strong>Department:</strong> {user?.department}
              </div>
              <div className="mb-3">
                <strong>Position:</strong> {user?.position}
              </div>
              <div className="mb-3">
                <strong>Roles:</strong>{' '}
                {user?.roles.map((role, index) => (
                  <CBadge key={index} color="primary" className="me-1">
                    {role}
                  </CBadge>
                ))}
              </div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default Dashboard;
