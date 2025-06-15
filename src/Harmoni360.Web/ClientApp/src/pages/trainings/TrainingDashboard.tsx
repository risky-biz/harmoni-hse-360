import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CBadge,
  CTable,
  CTableHead,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CTableRow,
  CSpinner,
  CAlert,
  CButtonGroup,
  CFormSelect,
  CProgress,
  CProgressBar,
  CListGroup,
  CListGroupItem
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faGraduationCap,
  faPlus,
  faUsers,
  faCalendarAlt,
  faClock,
  faCheck,
  faTimes,
  faExclamationTriangle,
  faChartLine,
  faEye,
  faEdit,
  faPlay,
  faCertificate,
  faFileAlt,
  faArrowUp,
  faArrowDown,
  faMinus,
  faWarning,
  faInfoCircle,
  faTrendUp,
  faTrendDown
} from '@fortawesome/free-solid-svg-icons';
import { format, formatDistanceToNow, subDays, subMonths } from 'date-fns';

import {
  useGetTrainingDashboardStatsQuery,
  useGetRecentTrainingsQuery,
  useGetUpcomingTrainingsQuery,
  useGetOverdueTrainingsQuery,
  useGetTrainingComplianceQuery,
  useGetTrainingTrendsQuery,
  useGetPopularTrainingsQuery
} from '../../features/trainings/trainingApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { useApplicationMode } from '../../hooks/useApplicationMode';

const TrainingDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { isDemo } = useApplicationMode();

  const [timeFilter, setTimeFilter] = useState('month'); // week, month, quarter, year
  const [chartType, setChartType] = useState('completion'); // completion, enrollment, compliance

  // API queries
  const { data: dashboardStats, isLoading: isLoadingStats } = useGetTrainingDashboardStatsQuery({ 
    period: timeFilter 
  });
  const { data: recentTrainings, isLoading: isLoadingRecent } = useGetRecentTrainingsQuery({ 
    limit: 10 
  });
  const { data: upcomingTrainings } = useGetUpcomingTrainingsQuery({ 
    limit: 10 
  });
  const { data: overdueTrainings } = useGetOverdueTrainingsQuery();
  const { data: complianceData } = useGetTrainingComplianceQuery();
  const { data: trendsData } = useGetTrainingTrendsQuery({ 
    period: timeFilter 
  });
  const { data: popularTrainings } = useGetPopularTrainingsQuery({ 
    limit: 5 
  });

  const getStatusBadge = (status: string) => {
    const config: Record<string, { color: string; icon: any }> = {
      'Draft': { color: 'secondary', icon: faEdit },
      'Scheduled': { color: 'info', icon: faCalendarAlt },
      'InProgress': { color: 'warning', icon: faPlay },
      'Completed': { color: 'success', icon: faCheck },
      'Cancelled': { color: 'danger', icon: faTimes },
      'Postponed': { color: 'warning', icon: faClock }
    };

    const { color, icon } = config[status] || { color: 'secondary', icon: faGraduationCap };

    return (
      <CBadge color={color} className="d-flex align-items-center">
        <FontAwesomeIcon icon={icon} className="me-1" size="sm" />
        {status}
      </CBadge>
    );
  };

  const getTrendIcon = (trend: 'up' | 'down' | 'neutral') => {
    switch (trend) {
      case 'up':
        return <FontAwesomeIcon icon={faArrowUp} className="text-success" />;
      case 'down':
        return <FontAwesomeIcon icon={faArrowDown} className="text-danger" />;
      default:
        return <FontAwesomeIcon icon={faMinus} className="text-muted" />;
    }
  };

  const getComplianceColor = (percentage: number) => {
    if (percentage >= 90) return 'success';
    if (percentage >= 70) return 'warning';
    return 'danger';
  };

  if (isLoadingStats || isLoadingRecent) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner color="primary" />
      </div>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        {/* Header */}
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h2 className="mb-1">Training Dashboard</h2>
            <p className="text-muted mb-0">Monitor training activities and performance</p>
          </div>
          <div className="d-flex gap-2">
            <CFormSelect
              size="sm"
              value={timeFilter}
              onChange={(e) => setTimeFilter(e.target.value)}
              style={{ width: 'auto' }}
            >
              <option value="week">This Week</option>
              <option value="month">This Month</option>
              <option value="quarter">This Quarter</option>
              <option value="year">This Year</option>
            </CFormSelect>
            <PermissionGuard
              moduleType={ModuleType.TrainingManagement}
              permissionType={PermissionType.Create}
            >
              <CButton
                color="primary"
                onClick={() => navigate('/trainings/create')}
                disabled={isDemo}
              >
                <FontAwesomeIcon icon={faPlus} className="me-1" />
                New Training
              </CButton>
            </PermissionGuard>
          </div>
        </div>

        {/* Key Metrics */}
        {dashboardStats && (
          <CRow className="mb-4">
            <CCol md={3}>
              <CCard className="text-center h-100">
                <CCardBody>
                  <FontAwesomeIcon icon={faGraduationCap} size="2x" className="text-primary mb-2" />
                  <h4 className="mb-1">
                    {dashboardStats.totalTrainings}
                    <small className="ms-2 text-muted">
                      {getTrendIcon(dashboardStats.totalTrainingsTrend)}
                    </small>
                  </h4>
                  <small className="text-muted">Total Trainings</small>
                  {dashboardStats.totalTrainingsChange !== 0 && (
                    <div className="mt-1">
                      <small className={dashboardStats.totalTrainingsChange > 0 ? 'text-success' : 'text-danger'}>
                        {dashboardStats.totalTrainingsChange > 0 ? '+' : ''}{dashboardStats.totalTrainingsChange}% from last period
                      </small>
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center h-100">
                <CCardBody>
                  <FontAwesomeIcon icon={faUsers} size="2x" className="text-info mb-2" />
                  <h4 className="mb-1">
                    {dashboardStats.totalParticipants}
                    <small className="ms-2 text-muted">
                      {getTrendIcon(dashboardStats.totalParticipantsTrend)}
                    </small>
                  </h4>
                  <small className="text-muted">Total Participants</small>
                  {dashboardStats.totalParticipantsChange !== 0 && (
                    <div className="mt-1">
                      <small className={dashboardStats.totalParticipantsChange > 0 ? 'text-success' : 'text-danger'}>
                        {dashboardStats.totalParticipantsChange > 0 ? '+' : ''}{dashboardStats.totalParticipantsChange}% from last period
                      </small>
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center h-100">
                <CCardBody>
                  <FontAwesomeIcon icon={faCheck} size="2x" className="text-success mb-2" />
                  <h4 className="mb-1">
                    {dashboardStats.completionRate}%
                    <small className="ms-2 text-muted">
                      {getTrendIcon(dashboardStats.completionRateTrend)}
                    </small>
                  </h4>
                  <small className="text-muted">Completion Rate</small>
                  {dashboardStats.completionRateChange !== 0 && (
                    <div className="mt-1">
                      <small className={dashboardStats.completionRateChange > 0 ? 'text-success' : 'text-danger'}>
                        {dashboardStats.completionRateChange > 0 ? '+' : ''}{dashboardStats.completionRateChange}% from last period
                      </small>
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center h-100">
                <CCardBody>
                  <FontAwesomeIcon icon={faCertificate} size="2x" className="text-warning mb-2" />
                  <h4 className="mb-1">
                    {dashboardStats.certificatesIssued}
                    <small className="ms-2 text-muted">
                      {getTrendIcon(dashboardStats.certificatesIssuedTrend)}
                    </small>
                  </h4>
                  <small className="text-muted">Certificates Issued</small>
                  {dashboardStats.certificatesIssuedChange !== 0 && (
                    <div className="mt-1">
                      <small className={dashboardStats.certificatesIssuedChange > 0 ? 'text-success' : 'text-danger'}>
                        {dashboardStats.certificatesIssuedChange > 0 ? '+' : ''}{dashboardStats.certificatesIssuedChange}% from last period
                      </small>
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>
        )}

        <CRow>
          {/* Recent Training Activity */}
          <CCol md={8}>
            <CCard className="mb-4">
              <CCardHeader>
                <div className="d-flex justify-content-between align-items-center">
                  <h6 className="mb-0">
                    <FontAwesomeIcon icon={faChartLine} className="me-2" />
                    Recent Training Activity
                  </h6>
                  <CButton
                    color="link"
                    size="sm"
                    onClick={() => navigate('/trainings')}
                  >
                    View All
                  </CButton>
                </div>
              </CCardHeader>
              <CCardBody className="p-0">
                {recentTrainings && recentTrainings.length > 0 ? (
                  <CTable hover responsive className="mb-0">
                    <CTableHead>
                      <CTableRow>
                        <CTableHeaderCell>Training</CTableHeaderCell>
                        <CTableHeaderCell>Status</CTableHeaderCell>
                        <CTableHeaderCell>Participants</CTableHeaderCell>
                        <CTableHeaderCell>Schedule</CTableHeaderCell>
                        <CTableHeaderCell>Actions</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {recentTrainings.slice(0, 5).map((training) => (
                        <CTableRow key={training.id}>
                          <CTableDataCell>
                            <div>
                              <div className="fw-semibold">{training.title}</div>
                              <small className="text-muted">
                                {training.trainingCode}
                                {training.isK3MandatoryTraining && (
                                  <CBadge color="warning" className="ms-1">K3</CBadge>
                                )}
                              </small>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            {getStatusBadge(training.status)}
                          </CTableDataCell>
                          <CTableDataCell>
                            <div className="d-flex align-items-center">
                              <FontAwesomeIcon icon={faUsers} className="me-1 text-muted" />
                              <span>{training.currentParticipants}/{training.maxParticipants}</span>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div>
                              <div className="fw-semibold">
                                {format(new Date(training.scheduledStartDate), 'MMM dd')}
                              </div>
                              <small className="text-muted">
                                {formatDistanceToNow(new Date(training.scheduledStartDate))} ago
                              </small>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <CButtonGroup size="sm">
                              <CButton
                                color="primary"
                                variant="outline"
                                onClick={() => navigate(`/trainings/${training.id}`)}
                              >
                                <FontAwesomeIcon icon={faEye} />
                              </CButton>
                              {training.canEdit && (
                                <CButton
                                  color="secondary"
                                  variant="outline"
                                  onClick={() => navigate(`/trainings/${training.id}/edit`)}
                                >
                                  <FontAwesomeIcon icon={faEdit} />
                                </CButton>
                              )}
                            </CButtonGroup>
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>
                ) : (
                  <div className="text-center py-4">
                    <FontAwesomeIcon icon={faGraduationCap} size="2x" className="text-muted mb-2" />
                    <p className="text-muted mb-0">No recent training activity</p>
                  </div>
                )}
              </CCardBody>
            </CCard>
          </CCol>

          {/* Quick Actions & Alerts */}
          <CCol md={4}>
            <CCard className="mb-4">
              <CCardHeader>
                <h6 className="mb-0">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                  Alerts & Actions
                </h6>
              </CCardHeader>
              <CCardBody>
                <CListGroup flush>
                  {overdueTrainings && overdueTrainings.length > 0 && (
                    <CListGroupItem className="d-flex justify-content-between align-items-center border-start border-danger border-4">
                      <div>
                        <div className="fw-semibold text-danger">Overdue Trainings</div>
                        <small className="text-muted">{overdueTrainings.length} trainings are overdue</small>
                      </div>
                      <CButton color="danger" size="sm" variant="outline">
                        <FontAwesomeIcon icon={faEye} />
                      </CButton>
                    </CListGroupItem>
                  )}
                  
                  {upcomingTrainings && upcomingTrainings.length > 0 && (
                    <CListGroupItem className="d-flex justify-content-between align-items-center border-start border-warning border-4">
                      <div>
                        <div className="fw-semibold text-warning">Upcoming Trainings</div>
                        <small className="text-muted">{upcomingTrainings.length} trainings this week</small>
                      </div>
                      <CButton color="warning" size="sm" variant="outline">
                        <FontAwesomeIcon icon={faCalendarAlt} />
                      </CButton>
                    </CListGroupItem>
                  )}
                  
                  {complianceData && complianceData.expiringSoon > 0 && (
                    <CListGroupItem className="d-flex justify-content-between align-items-center border-start border-info border-4">
                      <div>
                        <div className="fw-semibold text-info">Expiring Certificates</div>
                        <small className="text-muted">{complianceData.expiringSoon} certificates expiring soon</small>
                      </div>
                      <CButton color="info" size="sm" variant="outline">
                        <FontAwesomeIcon icon={faCertificate} />
                      </CButton>
                    </CListGroupItem>
                  )}

                  <CListGroupItem className="d-flex justify-content-between align-items-center border-start border-success border-4">
                    <div>
                      <div className="fw-semibold text-success">Create New Training</div>
                      <small className="text-muted">Schedule a new training session</small>
                    </div>
                    <PermissionGuard
                      moduleType={ModuleType.TrainingManagement}
                      permissionType={PermissionType.Create}
                    >
                      <CButton 
                        color="success" 
                        size="sm" 
                        variant="outline"
                        onClick={() => navigate('/trainings/create')}
                      >
                        <FontAwesomeIcon icon={faPlus} />
                      </CButton>
                    </PermissionGuard>
                  </CListGroupItem>
                </CListGroup>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>

        <CRow>
          {/* Training Compliance */}
          <CCol md={6}>
            <CCard className="mb-4">
              <CCardHeader>
                <h6 className="mb-0">
                  <FontAwesomeIcon icon={faCertificate} className="me-2" />
                  Training Compliance
                </h6>
              </CCardHeader>
              <CCardBody>
                {complianceData ? (
                  <>
                    <div className="mb-3">
                      <div className="d-flex justify-content-between mb-1">
                        <span>Overall Compliance</span>
                        <span>{complianceData.overallCompliance}%</span>
                      </div>
                      <CProgress height={12}>
                        <CProgressBar 
                          value={complianceData.overallCompliance}
                          color={getComplianceColor(complianceData.overallCompliance)}
                        />
                      </CProgress>
                    </div>

                    <div className="mb-3">
                      <div className="d-flex justify-content-between mb-1">
                        <span>K3 Compliance</span>
                        <span>{complianceData.k3Compliance}%</span>
                      </div>
                      <CProgress height={12}>
                        <CProgressBar 
                          value={complianceData.k3Compliance}
                          color={getComplianceColor(complianceData.k3Compliance)}
                        />
                      </CProgress>
                    </div>

                    <CTable size="sm">
                      <tbody>
                        <tr>
                          <td>Active Certificates:</td>
                          <td className="text-end">{complianceData.activeCertificates}</td>
                        </tr>
                        <tr>
                          <td>Expiring Soon:</td>
                          <td className="text-end text-warning">{complianceData.expiringSoon}</td>
                        </tr>
                        <tr>
                          <td>Expired:</td>
                          <td className="text-end text-danger">{complianceData.expired}</td>
                        </tr>
                        <tr>
                          <td>Refresher Required:</td>
                          <td className="text-end text-info">{complianceData.refresherRequired}</td>
                        </tr>
                      </tbody>
                    </CTable>
                  </>
                ) : (
                  <div className="text-center py-3">
                    <CSpinner size="sm" />
                    <div className="mt-2">Loading compliance data...</div>
                  </div>
                )}
              </CCardBody>
            </CCard>
          </CCol>

          {/* Popular Trainings */}
          <CCol md={6}>
            <CCard className="mb-4">
              <CCardHeader>
                <h6 className="mb-0">
                  <FontAwesomeIcon icon={faTrendUp} className="me-2" />
                  Popular Trainings
                </h6>
              </CCardHeader>
              <CCardBody>
                {popularTrainings && popularTrainings.length > 0 ? (
                  <CListGroup flush>
                    {popularTrainings.map((training, index) => (
                      <CListGroupItem key={training.id} className="d-flex justify-content-between align-items-center">
                        <div>
                          <div className="fw-semibold">{training.title}</div>
                          <small className="text-muted">
                            {training.totalEnrollments} enrollments • {training.completionRate}% completion
                          </small>
                        </div>
                        <div className="d-flex align-items-center">
                          <CBadge color="primary" className="me-2">#{index + 1}</CBadge>
                          <CButton
                            color="primary"
                            size="sm"
                            variant="outline"
                            onClick={() => navigate(`/trainings/${training.id}`)}
                          >
                            <FontAwesomeIcon icon={faEye} />
                          </CButton>
                        </div>
                      </CListGroupItem>
                    ))}
                  </CListGroup>
                ) : (
                  <div className="text-center py-3">
                    <FontAwesomeIcon icon={faGraduationCap} size="2x" className="text-muted mb-2" />
                    <p className="text-muted mb-0">No training data available</p>
                  </div>
                )}
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>

        {/* Training Trends */}
        {trendsData && (
          <CRow>
            <CCol xs={12}>
              <CCard>
                <CCardHeader>
                  <div className="d-flex justify-content-between align-items-center">
                    <h6 className="mb-0">
                      <FontAwesomeIcon icon={faChartLine} className="me-2" />
                      Training Trends ({timeFilter})
                    </h6>
                    <CFormSelect
                      size="sm"
                      value={chartType}
                      onChange={(e) => setChartType(e.target.value)}
                      style={{ width: 'auto' }}
                    >
                      <option value="completion">Completion Rate</option>
                      <option value="enrollment">Enrollment Numbers</option>
                      <option value="compliance">Compliance Rate</option>
                    </CFormSelect>
                  </div>
                </CCardHeader>
                <CCardBody>
                  <div className="row text-center">
                    {trendsData.map((trend, index) => (
                      <div key={index} className="col">
                        <div className="border-end">
                          <div className="fs-5 fw-semibold">{trend.value}%</div>
                          <div className="text-uppercase text-muted small">{trend.label}</div>
                          <div className="mt-1">
                            {trend.trend === 'up' ? (
                              <FontAwesomeIcon icon={faTrendUp} className="text-success" />
                            ) : trend.trend === 'down' ? (
                              <FontAwesomeIcon icon={faTrendDown} className="text-danger" />
                            ) : (
                              <FontAwesomeIcon icon={faMinus} className="text-muted" />
                            )}
                            <small className="ms-1 text-muted">{trend.change}%</small>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>
        )}
      </CCol>
    </CRow>
  );
};

export default TrainingDashboard;