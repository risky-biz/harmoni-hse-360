import React, { useState, useEffect, useMemo } from 'react';
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
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CProgress,
  CProgressBar
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faRefresh,
  faCog,
  faClipboardCheck,
  faExclamationTriangle,
  faClock,
  faCheckCircle,
  faTimesCircle,
  faChartLine,
  faChartPie,
  faChartBar,
  faDownload,
  faEye,
  faUsers,
  faBuilding,
  faFlag,
  faSearchPlus,
  faCalendarAlt,
  faTrendUp,
  faTrendDown
} from '@fortawesome/free-solid-svg-icons';
import { useGetDashboardQuery, useGetStatisticsQuery } from '../../features/inspections/inspectionApi';
import DemoModeWrapper from '../../components/common/DemoModeWrapper';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { format, formatDistanceToNow, subDays, subMonths } from 'date-fns';
import { InspectionStatus, InspectionPriority, InspectionType } from '../../types/inspection';
import { DonutChart, BarChart, LineChart } from '../../components/inspections/charts';
import { exportDashboardToPDF, exportInspectionsToExcel } from '../../utils/exportUtils';

export const InspectionDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [timeRange, setTimeRange] = useState<string>('30days');
  const [selectedDepartment, setSelectedDepartment] = useState<string>('');

  // Calculate date range based on selection
  const dateRange = useMemo(() => {
    const now = new Date();
    switch (timeRange) {
      case '7days':
        return { startDate: subDays(now, 7), endDate: now };
      case '30days':
        return { startDate: subDays(now, 30), endDate: now };
      case '90days':
        return { startDate: subDays(now, 90), endDate: now };
      case '6months':
        return { startDate: subMonths(now, 6), endDate: now };
      case '1year':
        return { startDate: subMonths(now, 12), endDate: now };
      default:
        return { startDate: subDays(now, 30), endDate: now };
    }
  }, [timeRange]);

  const {
    data: dashboardData,
    error,
    isLoading,
    refetch
  } = useGetDashboardQuery();

  const {
    data: statisticsData,
    isLoading: isLoadingStats
  } = useGetStatisticsQuery({
    startDate: dateRange.startDate.toISOString(),
    endDate: dateRange.endDate.toISOString(),
    departmentId: selectedDepartment ? Number(selectedDepartment) : undefined
  });

  // Mock departments - Replace with actual API call
  const departments = [
    { id: 1, name: 'Operations' },
    { id: 2, name: 'Maintenance' },
    { id: 3, name: 'Safety' },
    { id: 4, name: 'Environmental' }
  ];

  const handleCreateInspection = () => {
    navigate('/inspections/create');
  };

  const handleViewAllInspections = () => {
    navigate('/inspections');
  };

  const handleViewMyInspections = () => {
    navigate('/inspections/my-inspections');
  };

  const handleViewInspection = (id: number) => {
    navigate(`/inspections/${id}`);
  };

  const handleExportDashboard = () => {
    if (dashboardData) {
      exportDashboardToPDF(dashboardData, {
        filename: `inspection-dashboard-${format(new Date(), 'yyyy-MM-dd')}`,
        title: 'Inspection Dashboard Report'
      });
    }
  };

  const handleExportRecentInspections = () => {
    if (dashboardData?.recentInspections) {
      exportInspectionsToExcel(dashboardData.recentInspections, {
        filename: `recent-inspections-${format(new Date(), 'yyyy-MM-dd')}`
      });
    }
  };

  const getStatusColor = (status: InspectionStatus) => {
    const statusConfig = {
      [InspectionStatus.Draft]: 'secondary',
      [InspectionStatus.Scheduled]: 'info',
      [InspectionStatus.InProgress]: 'warning',
      [InspectionStatus.Completed]: 'success',
      [InspectionStatus.Cancelled]: 'danger'
    };
    return statusConfig[status] || 'secondary';
  };

  const getPriorityColor = (priority: InspectionPriority) => {
    const priorityConfig = {
      [InspectionPriority.Low]: 'success',
      [InspectionPriority.Medium]: 'warning',
      [InspectionPriority.High]: 'danger',
      [InspectionPriority.Critical]: 'dark'
    };
    return priorityConfig[priority] || 'secondary';
  };

  const getTypeColor = (type: InspectionType) => {
    const typeConfig = {
      [InspectionType.Safety]: 'danger',
      [InspectionType.Environmental]: 'success',
      [InspectionType.Quality]: 'primary',
      [InspectionType.Security]: 'warning',
      [InspectionType.Maintenance]: 'info',
      [InspectionType.Compliance]: 'dark'
    };
    return typeConfig[type] || 'secondary';
  };

  if (error) {
    return (
      <CAlert color="danger">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Error loading dashboard data. Please try again.
      </CAlert>
    );
  }

  return (
    <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Read}>
      <DemoModeWrapper>
        {/* Header */}
        <CRow className="mb-4">
          <CCol>
            <div className="d-flex justify-content-between align-items-center">
              <div>
                <h4 className="mb-0">
                  <FontAwesomeIcon icon={faChartLine} className="me-2 text-primary" />
                  Inspection Dashboard
                </h4>
                <small className="text-medium-emphasis">
                  Overview of inspection metrics and performance
                </small>
              </div>
              <div className="d-flex align-items-center gap-2">
                {/* Time Range Selector */}
                <CFormSelect
                  value={timeRange}
                  onChange={(e) => setTimeRange(e.target.value)}
                  style={{ width: 'auto' }}
                  size="sm"
                >
                  <option value="7days">Last 7 Days</option>
                  <option value="30days">Last 30 Days</option>
                  <option value="90days">Last 90 Days</option>
                  <option value="6months">Last 6 Months</option>
                  <option value="1year">Last Year</option>
                </CFormSelect>

                {/* Department Filter */}
                <CFormSelect
                  value={selectedDepartment}
                  onChange={(e) => setSelectedDepartment(e.target.value)}
                  style={{ width: 'auto' }}
                  size="sm"
                >
                  <option value="">All Departments</option>
                  {departments.map(dept => (
                    <option key={dept.id} value={dept.id}>{dept.name}</option>
                  ))}
                </CFormSelect>

                <CButton
                  color="light"
                  variant="outline"
                  size="sm"
                  onClick={() => refetch()}
                  disabled={isLoading}
                >
                  <FontAwesomeIcon icon={faRefresh} className="me-1" />
                  Refresh
                </CButton>

                <CDropdown>
                  <CDropdownToggle color="primary" variant="outline" size="sm">
                    <FontAwesomeIcon icon={faCog} className="me-1" />
                    Actions
                  </CDropdownToggle>
                  <CDropdownMenu>
                    <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Create}>
                      <CDropdownItem onClick={handleCreateInspection}>
                        <FontAwesomeIcon icon={faPlus} className="me-2" />
                        Create Inspection
                      </CDropdownItem>
                    </PermissionGuard>
                    <CDropdownItem onClick={handleViewAllInspections}>
                      <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                      View All Inspections
                    </CDropdownItem>
                    <CDropdownItem onClick={handleViewMyInspections}>
                      <FontAwesomeIcon icon={faUsers} className="me-2" />
                      My Inspections
                    </CDropdownItem>
                    <CDropdownItem divider />
                    <CDropdownItem onClick={handleExportDashboard}>
                      <FontAwesomeIcon icon={faDownload} className="me-2" />
                      Export Dashboard (PDF)
                    </CDropdownItem>
                    <CDropdownItem onClick={handleExportRecentInspections}>
                      <FontAwesomeIcon icon={faDownload} className="me-2" />
                      Export Recent Inspections (Excel)
                    </CDropdownItem>
                  </CDropdownMenu>
                </CDropdown>
              </div>
            </div>
          </CCol>
        </CRow>

        {isLoading ? (
          <div className="text-center py-5">
            <CSpinner color="primary" />
            <div className="mt-2">Loading dashboard...</div>
          </div>
        ) : (
          <>
            {/* KPI Cards */}
            <CRow className="mb-4">
              <CCol sm={6} lg={3}>
                <CCard className="mb-4">
                  <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                    <div>
                      <div className="fs-4 fw-semibold text-primary">
                        {dashboardData?.totalInspections || 0}
                      </div>
                      <div>Total Inspections</div>
                      <small className="text-medium-emphasis">
                        {timeRange === '30days' ? 'Last 30 days' : timeRange}
                      </small>
                    </div>
                    <CDropdown>
                      <CDropdownToggle
                        color="transparent"
                        caret={false}
                        className="p-0"
                      >
                        <FontAwesomeIcon icon={faClipboardCheck} size="xl" className="text-primary" />
                      </CDropdownToggle>
                    </CDropdown>
                  </CCardBody>
                </CCard>
              </CCol>

              <CCol sm={6} lg={3}>
                <CCard className="mb-4">
                  <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                    <div>
                      <div className="fs-4 fw-semibold text-warning">
                        {dashboardData?.inProgressInspections || 0}
                      </div>
                      <div>In Progress</div>
                      <small className="text-medium-emphasis">
                        Currently active
                      </small>
                    </div>
                    <FontAwesomeIcon icon={faClock} size="xl" className="text-warning" />
                  </CCardBody>
                </CCard>
              </CCol>

              <CCol sm={6} lg={3}>
                <CCard className="mb-4">
                  <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                    <div>
                      <div className="fs-4 fw-semibold text-danger">
                        {dashboardData?.overdueInspections || 0}
                      </div>
                      <div>Overdue</div>
                      <small className="text-medium-emphasis">
                        Require attention
                      </small>
                    </div>
                    <FontAwesomeIcon icon={faExclamationTriangle} size="xl" className="text-danger" />
                  </CCardBody>
                </CCard>
              </CCol>

              <CCol sm={6} lg={3}>
                <CCard className="mb-4">
                  <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                    <div>
                      <div className="fs-4 fw-semibold text-success">
                        {dashboardData?.complianceRate || 0}%
                      </div>
                      <div>Compliance Rate</div>
                      <small className="text-medium-emphasis">
                        Overall performance
                      </small>
                    </div>
                    <FontAwesomeIcon icon={faCheckCircle} size="xl" className="text-success" />
                  </CCardBody>
                </CCard>
              </CCol>
            </CRow>

            {/* Secondary Metrics */}
            <CRow className="mb-4">
              <CCol sm={6} lg={3}>
                <div className="border-start border-start-4 border-start-info py-1 px-3 mb-3">
                  <div className="text-medium-emphasis small">Completed This Month</div>
                  <div className="fs-5 fw-semibold">{dashboardData?.completedInspections || 0}</div>
                </div>
              </CCol>
              <CCol sm={6} lg={3}>
                <div className="border-start border-start-4 border-start-danger py-1 px-3 mb-3">
                  <div className="text-medium-emphasis small">Critical Findings</div>
                  <div className="fs-5 fw-semibold">{dashboardData?.criticalFindings || 0}</div>
                </div>
              </CCol>
              <CCol sm={6} lg={3}>
                <div className="border-start border-start-4 border-start-primary py-1 px-3 mb-3">
                  <div className="text-medium-emphasis small">Avg. Completion Time</div>
                  <div className="fs-5 fw-semibold">{dashboardData?.averageCompletionTime || 0}h</div>
                </div>
              </CCol>
              <CCol sm={6} lg={3}>
                <div className="border-start border-start-4 border-start-warning py-1 px-3 mb-3">
                  <div className="text-medium-emphasis small">Scheduled This Week</div>
                  <div className="fs-5 fw-semibold">{dashboardData?.scheduledInspections || 0}</div>
                </div>
              </CCol>
            </CRow>

            <CRow>
              {/* Inspection Status Distribution */}
              <CCol lg={6}>
                <CCard className="mb-4">
                  <CCardHeader>
                    <FontAwesomeIcon icon={faChartPie} className="me-2" />
                    Status Distribution
                  </CCardHeader>
                  <CCardBody>
                    {dashboardData?.inspectionsByStatus && dashboardData.inspectionsByStatus.length > 0 ? (
                      <DonutChart
                        data={{
                          labels: dashboardData.inspectionsByStatus.map(item => item.statusName),
                          values: dashboardData.inspectionsByStatus.map(item => item.count),
                          colors: dashboardData.inspectionsByStatus.map(item => {
                            const statusColors = {
                              'Draft': '#6c757d',
                              'Scheduled': '#0dcaf0',
                              'InProgress': '#fd7e14',
                              'Completed': '#198754',
                              'Cancelled': '#dc3545'
                            };
                            return statusColors[item.statusName] || '#6c757d';
                          })
                        }}
                        height={350}
                      />
                    ) : (
                      <div className="text-center py-3 text-muted">
                        No status data available
                      </div>
                    )}
                  </CCardBody>
                </CCard>
              </CCol>

              {/* Inspection Type Distribution */}
              <CCol lg={6}>
                <CCard className="mb-4">
                  <CCardHeader>
                    <FontAwesomeIcon icon={faChartBar} className="me-2" />
                    Type Distribution
                  </CCardHeader>
                  <CCardBody>
                    {dashboardData?.inspectionsByType && dashboardData.inspectionsByType.length > 0 ? (
                      <BarChart
                        data={{
                          labels: dashboardData.inspectionsByType.map(item => item.typeName),
                          datasets: [{
                            label: 'Inspections',
                            data: dashboardData.inspectionsByType.map(item => item.count),
                            backgroundColor: [
                              '#dc3545', // Safety - red
                              '#198754', // Environmental - green  
                              '#0d6efd', // Quality - blue
                              '#fd7e14', // Security - orange
                              '#0dcaf0', // Maintenance - cyan
                              '#6f42c1'  // Compliance - purple
                            ],
                            borderColor: [
                              '#dc3545',
                              '#198754',
                              '#0d6efd', 
                              '#fd7e14',
                              '#0dcaf0',
                              '#6f42c1'
                            ]
                          }]
                        }}
                        height={350}
                      />
                    ) : (
                      <div className="text-center py-3 text-muted">
                        No type data available
                      </div>
                    )}
                  </CCardBody>
                </CCard>
              </CCol>
            </CRow>

            <CRow>
              {/* Recent Inspections */}
              <CCol lg={8}>
                <CCard className="mb-4">
                  <CCardHeader className="d-flex justify-content-between align-items-center">
                    <div>
                      <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                      Recent Inspections
                    </div>
                    <CButton
                      color="primary"
                      variant="outline"
                      size="sm"
                      onClick={handleViewAllInspections}
                    >
                      View All
                    </CButton>
                  </CCardHeader>
                  <CCardBody className="p-0">
                    {dashboardData?.recentInspections && dashboardData.recentInspections.length > 0 ? (
                      <CTable hover responsive className="mb-0">
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Inspection</CTableHeaderCell>
                            <CTableHeaderCell>Status</CTableHeaderCell>
                            <CTableHeaderCell>Progress</CTableHeaderCell>
                            <CTableHeaderCell>Date</CTableHeaderCell>
                            <CTableHeaderCell></CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {dashboardData.recentInspections.slice(0, 5).map((inspection) => (
                            <CTableRow key={inspection.id}>
                              <CTableDataCell>
                                <div>
                                  <div className="fw-semibold">{inspection.title}</div>
                                  <small className="text-muted">#{inspection.inspectionNumber}</small>
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                <CBadge color={getStatusColor(inspection.status)}>
                                  {inspection.statusName}
                                </CBadge>
                              </CTableDataCell>
                              <CTableDataCell>
                                <div className="d-flex align-items-center">
                                  <CProgress height={4} className="flex-grow-1 me-2">
                                    <CProgressBar 
                                      value={(inspection.completedItemsCount / inspection.itemsCount) * 100}
                                      color="primary"
                                    />
                                  </CProgress>
                                  <small className="text-muted">
                                    {inspection.completedItemsCount}/{inspection.itemsCount}
                                  </small>
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                <small className="text-muted">
                                  {formatDistanceToNow(new Date(inspection.createdAt))} ago
                                </small>
                              </CTableDataCell>
                              <CTableDataCell>
                                <CButton
                                  color="primary"
                                  variant="outline"
                                  size="sm"
                                  onClick={() => handleViewInspection(inspection.id)}
                                >
                                  <FontAwesomeIcon icon={faEye} />
                                </CButton>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    ) : (
                      <div className="text-center py-4 text-muted">
                        No recent inspections
                      </div>
                    )}
                  </CCardBody>
                </CCard>
              </CCol>

              {/* Critical Findings */}
              <CCol lg={4}>
                <CCard className="mb-4">
                  <CCardHeader className="bg-danger text-white">
                    <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                    Critical Findings
                  </CCardHeader>
                  <CCardBody className="p-0">
                    {dashboardData?.criticalFindingsList && dashboardData.criticalFindingsList.length > 0 ? (
                      <div>
                        {dashboardData.criticalFindingsList.slice(0, 5).map((finding, index) => (
                          <div key={index} className="border-bottom p-3">
                            <div className="d-flex justify-content-between align-items-start mb-2">
                              <CBadge color="danger" className="mb-1">Critical</CBadge>
                              <small className="text-muted">
                                {formatDistanceToNow(new Date(finding.createdAt))} ago
                              </small>
                            </div>
                            <div className="fw-semibold mb-1">#{finding.findingNumber}</div>
                            <p className="small mb-1">{finding.description}</p>
                            {finding.location && (
                              <small className="text-muted">
                                <FontAwesomeIcon icon={faFlag} className="me-1" />
                                {finding.location}
                              </small>
                            )}
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-4 text-muted">
                        No critical findings
                      </div>
                    )}
                  </CCardBody>
                </CCard>
              </CCol>
            </CRow>

            {/* Upcoming Inspections */}
            <CRow>
              <CCol>
                <CCard className="mb-4">
                  <CCardHeader className="d-flex justify-content-between align-items-center">
                    <div>
                      <FontAwesomeIcon icon={faCalendarAlt} className="me-2" />
                      Upcoming Inspections
                    </div>
                    <CButton
                      color="primary"
                      variant="outline"
                      size="sm"
                      onClick={handleViewMyInspections}
                    >
                      View My Inspections
                    </CButton>
                  </CCardHeader>
                  <CCardBody className="p-0">
                    {dashboardData?.upcomingInspections && dashboardData.upcomingInspections.length > 0 ? (
                      <CTable hover responsive className="mb-0">
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Inspection</CTableHeaderCell>
                            <CTableHeaderCell>Type</CTableHeaderCell>
                            <CTableHeaderCell>Inspector</CTableHeaderCell>
                            <CTableHeaderCell>Department</CTableHeaderCell>
                            <CTableHeaderCell>Scheduled</CTableHeaderCell>
                            <CTableHeaderCell>Priority</CTableHeaderCell>
                            <CTableHeaderCell></CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {dashboardData.upcomingInspections.slice(0, 10).map((inspection) => (
                            <CTableRow key={inspection.id}>
                              <CTableDataCell>
                                <div>
                                  <div className="fw-semibold">{inspection.title}</div>
                                  <small className="text-muted">#{inspection.inspectionNumber}</small>
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                <CBadge color={getTypeColor(inspection.type)}>
                                  {inspection.typeName}
                                </CBadge>
                              </CTableDataCell>
                              <CTableDataCell>
                                <FontAwesomeIcon icon={faUsers} className="me-1 text-muted" />
                                {inspection.inspectorName}
                              </CTableDataCell>
                              <CTableDataCell>
                                <FontAwesomeIcon icon={faBuilding} className="me-1 text-muted" />
                                {inspection.departmentName}
                              </CTableDataCell>
                              <CTableDataCell>
                                <div>
                                  {format(new Date(inspection.scheduledDate), 'MMM dd, yyyy')}
                                </div>
                                <small className="text-muted">
                                  {format(new Date(inspection.scheduledDate), 'HH:mm')}
                                </small>
                              </CTableDataCell>
                              <CTableDataCell>
                                <CBadge color={getPriorityColor(inspection.priority)}>
                                  {inspection.priorityName}
                                </CBadge>
                              </CTableDataCell>
                              <CTableDataCell>
                                <CButton
                                  color="primary"
                                  variant="outline"
                                  size="sm"
                                  onClick={() => handleViewInspection(inspection.id)}
                                >
                                  <FontAwesomeIcon icon={faEye} />
                                </CButton>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    ) : (
                      <div className="text-center py-4 text-muted">
                        No upcoming inspections scheduled
                      </div>
                    )}
                  </CCardBody>
                </CCard>
              </CCol>
            </CRow>

            {/* Monthly Trends Chart */}
            <CRow>
              <CCol lg={12}>
                <CCard className="mb-4">
                  <CCardHeader>
                    <FontAwesomeIcon icon={faChartLine} className="me-2" />
                    Monthly Inspection Trends
                  </CCardHeader>
                  <CCardBody>
                    {dashboardData?.monthlyTrends && dashboardData.monthlyTrends.length > 0 ? (
                      <LineChart
                        data={{
                          labels: dashboardData.monthlyTrends.map(trend => `${trend.month} ${trend.year}`),
                          datasets: [
                            {
                              label: 'Scheduled',
                              data: dashboardData.monthlyTrends.map(trend => trend.scheduled),
                              borderColor: '#0dcaf0',
                              backgroundColor: 'rgba(13, 202, 240, 0.1)',
                              tension: 0.3,
                              fill: false
                            },
                            {
                              label: 'Completed',
                              data: dashboardData.monthlyTrends.map(trend => trend.completed),
                              borderColor: '#198754',
                              backgroundColor: 'rgba(25, 135, 84, 0.1)',
                              tension: 0.3,
                              fill: false
                            },
                            {
                              label: 'Overdue',
                              data: dashboardData.monthlyTrends.map(trend => trend.overdue),
                              borderColor: '#dc3545',
                              backgroundColor: 'rgba(220, 53, 69, 0.1)',
                              tension: 0.3,
                              fill: false
                            },
                            {
                              label: 'Critical Findings',
                              data: dashboardData.monthlyTrends.map(trend => trend.criticalFindings),
                              borderColor: '#fd7e14',
                              backgroundColor: 'rgba(253, 126, 20, 0.1)',
                              tension: 0.3,
                              fill: false
                            }
                          ]
                        }}
                        height={400}
                      />
                    ) : (
                      <div className="text-center py-4 text-muted">
                        No trend data available
                      </div>
                    )}
                  </CCardBody>
                </CCard>
              </CCol>
            </CRow>
          </>
        )}
      </DemoModeWrapper>
    </PermissionGuard>
  );
};