import React from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CBadge,
  CSpinner,
  CAlert,
  CProgress,
} from '@coreui/react';
import { useGetWasteDashboardQuery } from '../../api/wasteManagementApi';
import DonutChart from '../../components/dashboard/DonutChart';
import StatsCard from '../../components/dashboard/StatsCard';

const WasteDashboard: React.FC = () => {
  const { data: dashboard, isLoading, error } = useGetWasteDashboardQuery();

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center">
        <CSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load dashboard data. Please try again.
      </CAlert>
    );
  }

  if (!dashboard) {
    return null;
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending': return 'warning';
      case 'disposed': return 'success';
      case 'intransit': return 'info';
      case 'approved': return 'primary';
      default: return 'secondary';
    }
  };

  const getProviderStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active': return 'success';
      case 'expired': return 'danger';
      case 'suspended': return 'warning';
      default: return 'secondary';
    }
  };

  const complianceRate = dashboard.totalReports > 0 
    ? Math.round((dashboard.completedReports / dashboard.totalReports) * 100) 
    : 0;

  return (
    <>
      <CRow>
        <CCol>
          <CCard className="mb-4">
            <CCardHeader>
              <strong>Waste Management Dashboard</strong>
            </CCardHeader>
            <CCardBody>
              <p>
                Monitor waste generation, disposal status, and compliance metrics across your organization.
              </p>
              
              {/* Key Metrics */}
              <CRow className="mb-4">
                <CCol md={3}>
                  <StatsCard
                    title="Total Reports"
                    value={dashboard.totalReports.toString()}
                    icon="cilClipboard"
                    color="primary"
                  />
                </CCol>
                <CCol md={3}>
                  <StatsCard
                    title="Pending Disposal"
                    value={dashboard.pendingReports.toString()}
                    icon="cilClock"
                    color="warning"
                  />
                </CCol>
                <CCol md={3}>
                  <StatsCard
                    title="Completed"
                    value={dashboard.completedReports.toString()}
                    icon="cilCheckCircle"
                    color="success"
                  />
                </CCol>
                <CCol md={3}>
                  <StatsCard
                    title="Compliance Rate"
                    value={`${complianceRate}%`}
                    icon="cilChart"
                    color={complianceRate >= 90 ? "success" : complianceRate >= 70 ? "warning" : "danger"}
                  />
                </CCol>
              </CRow>

              {/* Charts and Analysis */}
              <CRow className="mb-4">
                <CCol md={6}>
                  <CCard>
                    <CCardHeader>
                      <strong>Waste Categories Distribution</strong>
                    </CCardHeader>
                    <CCardBody>
                      {dashboard.categoryStats.length > 0 ? (
                        <>
                          <DonutChart
                            data={dashboard.categoryStats.map(stat => ({
                              label: stat.category,
                              value: stat.count,
                              color: `hsl(${Math.random() * 360}, 70%, 50%)`
                            }))}
                          />
                          <div className="mt-3">
                            {dashboard.categoryStats.map((stat, index) => (
                              <div key={index} className="d-flex justify-content-between align-items-center mb-1">
                                <span>{stat.category}</span>
                                <span>
                                  <strong>{stat.count}</strong> ({stat.percentage.toFixed(1)}%)
                                </span>
                              </div>
                            ))}
                          </div>
                        </>
                      ) : (
                        <div className="text-muted text-center">No data available</div>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>

                <CCol md={6}>
                  <CCard>
                    <CCardHeader>
                      <strong>Monthly Trends</strong>
                    </CCardHeader>
                    <CCardBody>
                      {dashboard.monthlyStats.length > 0 ? (
                        <div>
                          {dashboard.monthlyStats.map((stat, index) => (
                            <div key={index} className="mb-3">
                              <div className="d-flex justify-content-between align-items-center mb-1">
                                <span>{stat.month}</span>
                                <span><strong>{stat.reportCount}</strong> reports</span>
                              </div>
                              <CProgress
                                value={(stat.reportCount / Math.max(...dashboard.monthlyStats.map(s => s.reportCount))) * 100}
                                color="info"
                                height={8}
                              />
                            </div>
                          ))}
                        </div>
                      ) : (
                        <div className="text-muted text-center">No monthly data available</div>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>

              {/* Recent Reports */}
              <CRow className="mb-4">
                <CCol md={8}>
                  <CCard>
                    <CCardHeader>
                      <strong>Recent Waste Reports</strong>
                    </CCardHeader>
                    <CCardBody>
                      {dashboard.recentReports.length > 0 ? (
                        <CTable hover responsive>
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Title</CTableHeaderCell>
                              <CTableHeaderCell>Category</CTableHeaderCell>
                              <CTableHeaderCell>Status</CTableHeaderCell>
                              <CTableHeaderCell>Reporter</CTableHeaderCell>
                              <CTableHeaderCell>Date</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {dashboard.recentReports.map((report) => (
                              <CTableRow key={report.id}>
                                <CTableDataCell>
                                  <strong>{report.title}</strong>
                                </CTableDataCell>
                                <CTableDataCell>{report.category}</CTableDataCell>
                                <CTableDataCell>
                                  <CBadge color={getStatusColor(report.status)}>
                                    {report.status}
                                  </CBadge>
                                </CTableDataCell>
                                <CTableDataCell>{report.reporterName || 'Unknown'}</CTableDataCell>
                                <CTableDataCell>
                                  {new Date(report.generatedDate).toLocaleDateString()}
                                </CTableDataCell>
                              </CTableRow>
                            ))}
                          </CTableBody>
                        </CTable>
                      ) : (
                        <div className="text-muted text-center">No recent reports</div>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>

                <CCol md={4}>
                  <CCard>
                    <CCardHeader>
                      <strong>Disposal Providers</strong>
                    </CCardHeader>
                    <CCardBody>
                      <div className="mb-3">
                        <div className="d-flex justify-content-between align-items-center">
                          <span>Active Providers</span>
                          <strong>{dashboard.activeDisposalProviders}</strong>
                        </div>
                        <div className="d-flex justify-content-between align-items-center">
                          <span>Total Providers</span>
                          <strong>{dashboard.totalDisposalProviders}</strong>
                        </div>
                      </div>

                      {dashboard.expiringProviders.length > 0 && (
                        <>
                          <hr />
                          <h6 className="text-warning">⚠️ Expiring Licenses</h6>
                          {dashboard.expiringProviders.map((provider) => (
                            <div key={provider.id} className="mb-2 p-2 border rounded">
                              <div className="d-flex justify-content-between align-items-center">
                                <span className="fw-bold">{provider.name}</span>
                                <CBadge color={getProviderStatusColor(provider.status)}>
                                  {provider.status}
                                </CBadge>
                              </div>
                              <small className="text-muted">
                                Expires: {new Date(provider.licenseExpiryDate).toLocaleDateString()}
                              </small>
                            </div>
                          ))}
                        </>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default WasteDashboard;