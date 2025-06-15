import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CSpinner,
  CAlert,
  CBadge,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CProgress,
  CButtonGroup,
  CFormSelect,
  CButtonToolbar,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem
} from '@coreui/react';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { HubConnectionState } from '@microsoft/signalr';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import {
  faShieldAlt,
  faExclamationTriangle,
  faTools,
  faClock,
  faRedo,
  faCog,
  faPlus,
  faSearch,
  faClipboardCheck,
} from '@fortawesome/free-solid-svg-icons';
import { useGetPPEDashboardQuery } from '../../features/ppe/ppeApi';
import { useGetPPECategoriesQuery } from '../../features/ppe/ppeManagementApi';
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
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { formatDate } from '../../utils/ppeUtils';
import { formatDistanceToNow } from 'date-fns';

const PPEDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [timeRange, setTimeRange] = useState<string>('all');
  const [category, setCategory] = useState<string>('');
  const [autoRefreshInterval, setAutoRefreshInterval] = useState<number>(0);
  const [lastRefreshTime, setLastRefreshTime] = useState<Date>(new Date());
  const autoRefreshIntervalRef = useRef<NodeJS.Timeout | null>(null);

  // Initialize SignalR
  const { connectionState } = useSignalR();

  // Calculate date range based on selection
  const getDateRange = () => {
    const now = new Date();
    switch (timeRange) {
      case '7d':
        return {
          fromDate: new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000).toISOString(),
          toDate: now.toISOString()
        };
      case '30d':
        return {
          fromDate: new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000).toISOString(),
          toDate: now.toISOString()
        };
      case '90d':
        return {
          fromDate: new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000).toISOString(),
          toDate: now.toISOString()
        };
      default:
        return {};
    }
  };

  const { data: dashboard, error, isLoading, refetch, dataUpdatedAt } = useGetPPEDashboardQuery({
    ...getDateRange(),
    category: category || undefined
  }, {
    refetchOnMountOrArgChange: true
  });

  // Fetch categories for dropdown
  const { data: categories } = useGetPPECategoriesQuery({ isActive: true });

  // Update last refresh time when data changes
  useEffect(() => {
    if (dataUpdatedAt) {
      setLastRefreshTime(new Date(dataUpdatedAt));
    }
  }, [dataUpdatedAt]);

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

  if (error) {
    return (
      <div className="dashboard-container">
        <div className="mb-4">
          <h1>PPE Management Dashboard</h1>
          <CAlert color="danger" className="mt-3">
            <strong>Error loading dashboard data.</strong> Please try refreshing the page.
            <CButton color="danger" variant="outline" size="sm" className="ms-2" onClick={() => refetch()}>
              Retry
            </CButton>
          </CAlert>
        </div>
      </div>
    );
  }

  const getUtilizationPercentage = () => {
    if (dashboard.totalItems === 0) return 0;
    return Math.round((dashboard.assignedItems / dashboard.totalItems) * 100);
  };

  const getAvailabilityPercentage = () => {
    if (dashboard.totalItems === 0) return 0;
    return Math.round((dashboard.availableItems / dashboard.totalItems) * 100);
  };

  return (
    <div className="dashboard-container">
      {/* Header */}
      <div className="mb-4 dashboard-header">
        <div className="d-flex justify-content-between align-items-start flex-wrap gap-3">
          <div>
            <h1 className="h2 h1-md">PPE Management Dashboard</h1>
            <p className="text-medium-emphasis mb-0 d-none d-md-block">
              Comprehensive PPE inventory analytics and compliance monitoring
            </p>
          </div>
          <PermissionGuard 
            module={ModuleType.PPEManagement} 
            permission={PermissionType.Create}
          >
            <CButton 
              color="primary" 
              onClick={() => navigate('/ppe/create')}
              className="btn-block-mobile"
            >
              <FontAwesomeIcon icon={faPlus} className="me-2" />
              <span className="d-none d-sm-inline">Add PPE Item</span>
              <span className="d-sm-none">Add PPE</span>
            </CButton>
          </PermissionGuard>
        </div>
      </div>

      {/* Filters */}
      <CCard className="mb-4 dashboard-filters">
        <CCardBody className="py-3">
          <CRow className="align-items-center filter-controls">
            <CCol lg={5} className="mb-3 mb-lg-0">
              <div className="d-flex align-items-center flex-column flex-sm-row gap-2">
                <label className="form-label mb-0 fw-semibold text-nowrap d-none d-sm-block">Time Range:</label>
                <CButtonGroup size="sm" className="w-100 w-sm-auto">
                  <CButton
                    color={timeRange === 'all' ? 'primary' : 'outline-primary'}
                    onClick={() => setTimeRange('all')}
                  >
                    All
                  </CButton>
                  <CButton
                    color={timeRange === '7d' ? 'primary' : 'outline-primary'}
                    onClick={() => setTimeRange('7d')}
                  >
                    7D
                  </CButton>
                  <CButton
                    color={timeRange === '30d' ? 'primary' : 'outline-primary'}
                    onClick={() => setTimeRange('30d')}
                  >
                    30D
                  </CButton>
                  <CButton
                    color={timeRange === '90d' ? 'primary' : 'outline-primary'}
                    onClick={() => setTimeRange('90d')}
                  >
                    90D
                  </CButton>
                </CButtonGroup>
              </div>
            </CCol>
            <CCol lg={3} className="mb-3 mb-lg-0">
              <CFormSelect
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                size="sm"
              >
                <option value="">All Categories</option>
                {categories?.map((cat) => (
                  <option key={cat.id} value={cat.id.toString()}>
                    {cat.name}
                  </option>
                ))}
              </CFormSelect>
            </CCol>
            <CCol lg={4} className="text-end refresh-controls">
              <div className="d-flex align-items-center justify-content-end gap-2 flex-wrap">
                <CButtonToolbar className="gap-2">
                  <CButton
                    color="primary"
                    variant="outline"
                    size="sm"
                    onClick={() => refetch()}
                    disabled={isLoading}
                  >
                    <FontAwesomeIcon icon={faRedo} className="me-1" />
                    <span className="d-none d-sm-inline">{isLoading ? 'Refreshing...' : 'Refresh'}</span>
                    <span className="d-sm-none">{isLoading ? '...' : 'Refresh'}</span>
                  </CButton>
                  <CDropdown variant="btn-group">
                    <CDropdownToggle color="secondary" variant="outline">
                      <FontAwesomeIcon icon={faCog} className="me-1 d-none d-sm-inline" />
                      <span className="d-none d-md-inline">Auto-refresh: </span>
                      {autoRefreshInterval === 0 ? 'Off' : `${autoRefreshInterval}s`}
                    </CDropdownToggle>
                    <CDropdownMenu>
                      <CDropdownItem onClick={() => setAutoRefreshInterval(0)}>
                        Disabled
                      </CDropdownItem>
                      <CDropdownItem onClick={() => setAutoRefreshInterval(30)}>
                        Every 30 seconds
                      </CDropdownItem>
                      <CDropdownItem onClick={() => setAutoRefreshInterval(60)}>
                        Every minute
                      </CDropdownItem>
                      <CDropdownItem onClick={() => setAutoRefreshInterval(300)}>
                        Every 5 minutes
                      </CDropdownItem>
                    </CDropdownMenu>
                  </CDropdown>
                </CButtonToolbar>
                <div className="text-medium-emphasis small text-end flex-shrink-0">
                  <div className="text-truncate-mobile">Last: {formatDistanceToNow(lastRefreshTime, { addSuffix: true })}</div>
                  {connectionState === HubConnectionState.Connected && (
                    <div className="text-success d-none d-sm-block">
                      <small>● Live updates</small>
                    </div>
                  )}
                </div>
              </div>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {/* Overall Statistics */}
      <CRow className="mb-4 stats-row">
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Total PPE Items"
            value={dashboard?.totalItems || 0}
            icon={faShieldAlt}
            color="primary"
            isLoading={isLoading}
            onClick={() => navigate('/ppe')}
          />
        </CCol>
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Available Items"
            value={dashboard?.availableItems || 0}
            icon={faTools}
            color="success"
            isLoading={isLoading}
            onClick={() => navigate('/ppe?status=Available')}
          />
        </CCol>
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Expiry Warnings"
            value={(dashboard?.expiredItems || 0) + (dashboard?.expiringSoonItems || 0)}
            icon={faExclamationTriangle}
            color="warning"
            isLoading={isLoading}
            onClick={() => navigate('/ppe?filter=expiring')}
          />
        </CCol>
        <CCol lg={3} md={6} className="mb-3 mb-lg-0">
          <StatsCard
            title="Maintenance Due"
            value={dashboard?.maintenanceDueItems || 0}
            icon={faClock}
            color="info"
            isLoading={isLoading}
            onClick={() => navigate('/ppe?filter=maintenance')}
          />
        </CCol>
      </CRow>

      {/* Charts Row 1 */}
      <CRow className="mb-4">
        <CCol lg={4}>
          <ChartCard
            title="Status Distribution"
            subtitle="Current PPE status breakdown"
            isLoading={isLoading}
            height="350px"
          >
            {dashboard && (
              <DonutChart
                data={dashboard.statusStats.map(stat => ({
                  label: stat.status,
                  value: stat.count,
                  color: stat.status === 'Available' ? 'var(--cui-success)' : 
                         stat.status === 'Assigned' ? 'var(--cui-primary)' :
                         stat.status === 'InMaintenance' ? 'var(--cui-warning)' : 'var(--cui-secondary)'
                }))}
                size={280}
                strokeWidth={25}
              />
            )}
          </ChartCard>
        </CCol>
        <CCol lg={4}>
          <ChartCard
            title="Category Analysis"
            subtitle="PPE distribution by category"
            isLoading={isLoading}
            height="350px"
          >
            {dashboard && (
              <BarChart
                data={dashboard.categoryStats.map(cat => ({
                  label: cat.categoryName,
                  value: cat.totalItems
                }))}
                height={300}
                horizontal={true}
                showValues={true}
              />
            )}
          </ChartCard>
        </CCol>
        <CCol lg={4}>
          <ChartCard
            title="Utilization Metrics"
            subtitle="PPE assignment and availability"
            isLoading={isLoading}
            height="350px"
          >
            {dashboard && (
              <div className="h-100 d-flex flex-column justify-content-center">
                <ProgressCard
                  title="Items in Use"
                  value={dashboard.assignedItems}
                  total={dashboard.totalItems}
                  percentage={getUtilizationPercentage()}
                  color="primary"
                  description="Assignment rate"
                  isLoading={isLoading}
                />
                <div className="mt-3">
                  <ProgressCard
                    title="Available Items"
                    value={dashboard.availableItems}
                    total={dashboard.totalItems}
                    percentage={getAvailabilityPercentage()}
                    color="success"
                    description="Ready for assignment"
                    isLoading={isLoading}
                  />
                </div>
              </div>
            )}
          </ChartCard>
        </CCol>
      </CRow>

      {/* Category Breakdown and Alerts */}
      <CRow className="mb-4">
        <CCol lg={8}>
          <ChartCard
            title="Category Breakdown"
            subtitle="PPE inventory by category type"
            isLoading={isLoading}
            height="400px"
          >
            {dashboard && (
              <div style={{ overflowX: 'auto', overflowY: 'auto', maxHeight: '350px' }}>
                <CTable responsive>
                  <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>Category</CTableHeaderCell>
                    <CTableHeaderCell>Total Items</CTableHeaderCell>
                    <CTableHeaderCell>Available</CTableHeaderCell>
                    <CTableHeaderCell>Assigned</CTableHeaderCell>
                    <CTableHeaderCell>Utilization</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {dashboard.categoryStats.slice(0, 8).map((category) => {
                    const utilization = category.totalItems > 0 
                      ? (category.assignedItems / category.totalItems) * 100 
                      : 0;
                    
                    return (
                      <CTableRow key={category.categoryId}>
                        <CTableDataCell>
                          <strong>{category.categoryName}</strong>
                        </CTableDataCell>
                        <CTableDataCell>{category.totalItems}</CTableDataCell>
                        <CTableDataCell>
                          <CBadge color="success">{category.availableItems}</CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color="primary">{category.assignedItems}</CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          <div style={{ width: '100px' }}>
                            <CProgress value={utilization} />
                            <small className="text-muted">{utilization.toFixed(1)}%</small>
                          </div>
                        </CTableDataCell>
                      </CTableRow>
                    );
                  })}
                </CTableBody>
              </CTable>
              </div>
            )}
          </ChartCard>
        </CCol>
        <CCol lg={4}>
          <ChartCard
            title="Critical Alerts"
            subtitle="Items requiring immediate attention"
            isLoading={isLoading}
            height="400px"
          >
            {dashboard && (
              <div className="h-100 overflow-auto">
                {/* Expired Items */}
                <div className="mb-3 p-3 border rounded alert-item bg-danger-subtle">
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <h6 className="mb-0 text-danger">Expired Items</h6>
                    <CBadge color="danger">{dashboard.expiredItems}</CBadge>
                  </div>
                  <div className="small text-danger">
                    Items past expiry date - immediate replacement required
                  </div>
                </div>
                
                {/* Expiring Soon */}
                <div className="mb-3 p-3 border rounded alert-item bg-warning-subtle">
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <h6 className="mb-0 text-warning">Expiring Soon</h6>
                    <CBadge color="warning">{dashboard.expiringSoonItems}</CBadge>
                  </div>
                  <div className="small text-warning">
                    Items expiring within 30 days
                  </div>
                </div>
                
                {/* Maintenance Due */}
                <div className="mb-3 p-3 border rounded alert-item bg-info-subtle">
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <h6 className="mb-0 text-info">Maintenance Due</h6>
                    <CBadge color="info">{dashboard.maintenanceDueItems}</CBadge>
                  </div>
                  <div className="small text-info">
                    Items requiring maintenance checks
                  </div>
                </div>
                
                {/* Inspection Due */}
                <div className="mb-3 p-3 border rounded alert-item bg-secondary-subtle">
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <h6 className="mb-0 text-secondary">Inspection Due</h6>
                    <CBadge color="secondary">{dashboard.inspectionDueItems}</CBadge>
                  </div>
                  <div className="small text-secondary">
                    Items requiring safety inspections
                  </div>
                </div>
              </div>
            )}
          </ChartCard>
        </CCol>
      </CRow>

      {/* Recent Items and Quick Actions */}
      <CRow className="mb-4">
        <CCol lg={8}>
          {dashboard?.expiryWarnings && (
            <RecentItemsList
              title="Items Requiring Attention"
              items={[
                ...dashboard.expiryWarnings.slice(0, 5).map(warning => ({
                  id: warning.itemId,
                  title: warning.itemCode,
                  subtitle: `${warning.itemName} • Expires: ${formatDate(warning.expiryDate)}`,
                  status: warning.isExpired ? 'Expired' : 'Expiring',
                  statusColor: warning.isExpired ? 'danger' : 'warning',
                  timestamp: warning.expiryDate,
                  isOverdue: warning.isExpired,
                  onClick: () => navigate(`/ppe/${warning.itemId}`)
                })),
                ...dashboard.maintenanceWarnings.slice(0, 3).map(warning => ({
                  id: warning.itemId,
                  title: warning.itemCode,
                  subtitle: `${warning.itemName} • Maintenance due: ${formatDate(warning.dueDate)}`,
                  status: warning.isOverdue ? 'Overdue' : 'Due',
                  statusColor: warning.isOverdue ? 'danger' : 'info',
                  timestamp: warning.dueDate,
                  isOverdue: warning.isOverdue,
                  onClick: () => navigate(`/ppe/${warning.itemId}`)
                }))
              ]}
              isLoading={isLoading}
              maxItems={8}
              showAllLink={{
                text: 'View All PPE Items',
                onClick: () => navigate('/ppe')
              }}
            />
          )}
        </CCol>
        <CCol lg={4}>
          <CCard className="h-100">
            <CCardHeader>
              <h5 className="card-title mb-0">Quick Actions & Analytics</h5>
            </CCardHeader>
            <CCardBody>
              {isLoading ? (
                <div className="d-flex justify-content-center py-4">
                  <CSpinner color="primary" />
                </div>
              ) : dashboard ? (
                <div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between">
                      <span className="text-medium-emphasis">Total Value</span>
                      <span className="fw-semibold">
                        ${(dashboard.totalItems * 150).toLocaleString()}
                      </span>
                    </div>
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between">
                      <span className="text-medium-emphasis">Utilization Rate</span>
                      <span className="fw-semibold">
                        {getUtilizationPercentage()}%
                      </span>
                    </div>
                  </div>
                  <div className="mb-3">
                    <div className="d-flex justify-content-between">
                      <span className="text-medium-emphasis">Availability Rate</span>
                      <span className="fw-semibold text-success">
                        {getAvailabilityPercentage()}%
                      </span>
                    </div>
                  </div>
                  <div className="mt-4">
                    <div className="small text-medium-emphasis mb-2">
                      Quick Actions
                    </div>
                    <div className="d-grid gap-2">
                      <PermissionGuard 
                        module={ModuleType.PPEManagement} 
                        permission={PermissionType.Create}
                      >
                        <CButton 
                          color="primary" 
                          variant="outline" 
                          size="sm"
                          onClick={() => navigate('/ppe/create')}
                        >
                          <FontAwesomeIcon icon={faPlus} className="me-1" />
                          Add PPE Item
                        </CButton>
                      </PermissionGuard>
                      <PermissionGuard 
                        module={ModuleType.PPEManagement} 
                        permission={PermissionType.Read}
                      >
                        <CButton 
                          color="success" 
                          variant="outline" 
                          size="sm"
                          onClick={() => navigate('/ppe?status=Available')}
                        >
                          <FontAwesomeIcon icon={faTools} className="me-1" />
                          Available Items
                        </CButton>
                      </PermissionGuard>
                      <PermissionGuard 
                        module={ModuleType.PPEManagement} 
                        permission={PermissionType.Read}
                      >
                        <CButton 
                          color="warning" 
                          variant="outline" 
                          size="sm"
                          onClick={() => navigate('/ppe?filter=expiring')}
                        >
                          <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                          Expiring Items
                        </CButton>
                      </PermissionGuard>
                    </div>
                  </div>
                </div>
              ) : null}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </div>
  );
};

export default PPEDashboard;
