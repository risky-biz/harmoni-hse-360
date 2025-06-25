import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CRow,
  CCol,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CBadge,
  CButton,
  CSpinner,
  CAlert,
  CProgress,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CListGroup,
  CListGroupItem,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane
} from '@coreui/react';
import { Icon } from '../common/Icon';
import { 
  cilSettings, 
  cilCheckCircle, 
  cilXCircle, 
  cilWarning, 
  cilInfo,
  cilClock,
  cilUser,
  cilChild,
  cilChart,
  cilList
} from '@coreui/icons';
import {
  useGetModuleConfigurationDashboardQuery,
  useGetRecentModuleActivityQuery,
  ModuleConfigurationDashboardDto,
  ModuleConfigurationAuditLogDto,
  getModuleDisplayName,
  getModuleIcon
} from '../../services/moduleConfigurationApi';
import StatsCard from '../dashboard/StatsCard';
import { formatDate } from '../../utils/formatters';

interface StatsSectionProps {
  dashboard: ModuleConfigurationDashboardDto;
}

const StatsSection: React.FC<StatsSectionProps> = ({ dashboard }) => {
  const enabledPercentage = dashboard.totalModules > 0 
    ? Math.round((dashboard.enabledModules / dashboard.totalModules) * 100)
    : 0;

  return (
    <CRow className="mb-4">
      <CCol sm={6} md={3}>
        <StatsCard
          title="Total Modules"
          value={dashboard.totalModules}
          icon={cilSettings}
          color="primary"
        />
      </CCol>
      <CCol sm={6} md={3}>
        <StatsCard
          title="Enabled"
          value={dashboard.enabledModules}
          icon={cilCheckCircle}
          color="success"
          subtitle={`${enabledPercentage}% of total`}
        />
      </CCol>
      <CCol sm={6} md={3}>
        <StatsCard
          title="Disabled"
          value={dashboard.disabledModules}
          icon={cilXCircle}
          color="secondary"
        />
      </CCol>
      <CCol sm={6} md={3}>
        <StatsCard
          title="Critical Modules"
          value={dashboard.criticalModules}
          icon={cilChild}
          color="warning"
          subtitle="Cannot be disabled"
        />
      </CCol>
    </CRow>
  );
};

interface ModuleStatusSummaryProps {
  dashboard: ModuleConfigurationDashboardDto;
}

const ModuleStatusSummary: React.FC<ModuleStatusSummaryProps> = ({ dashboard }) => {
  return (
    <CCard className="mb-4">
      <CCardHeader>
        <h6 className="mb-0">
          <Icon icon={cilList} className="me-2" />
          Module Status Summary
        </h6>
      </CCardHeader>
      <CCardBody>
        <CTable responsive>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell>Module</CTableHeaderCell>
              <CTableHeaderCell>Status</CTableHeaderCell>
              <CTableHeaderCell>Dependencies</CTableHeaderCell>
              <CTableHeaderCell>Dependents</CTableHeaderCell>
              <CTableHeaderCell>Can Disable</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {dashboard.moduleStatusSummary.map((module, index) => (
              <CTableRow key={index}>
                <CTableDataCell>
                  <div className="d-flex align-items-center">
                    <span className="fw-semibold">{module.moduleName}</span>
                  </div>
                </CTableDataCell>
                <CTableDataCell>
                  <CBadge color={module.isEnabled ? 'success' : 'secondary'}>
                    {module.isEnabled ? 'Enabled' : 'Disabled'}
                  </CBadge>
                </CTableDataCell>
                <CTableDataCell>
                  <span className="text-muted">{module.dependenciesCount}</span>
                </CTableDataCell>
                <CTableDataCell>
                  <span className="text-muted">{module.dependentModulesCount}</span>
                </CTableDataCell>
                <CTableDataCell>
                  <CBadge color={module.canBeDisabled ? 'success' : 'danger'}>
                    {module.canBeDisabled ? 'Yes' : 'No'}
                  </CBadge>
                </CTableDataCell>
              </CTableRow>
            ))}
          </CTableBody>
        </CTable>
      </CCardBody>
    </CCard>
  );
};

interface RecentActivityProps {
  activity: ModuleConfigurationAuditLogDto[];
  isLoading: boolean;
}

const RecentActivity: React.FC<RecentActivityProps> = ({ activity, isLoading }) => {
  const [selectedActivity, setSelectedActivity] = useState<ModuleConfigurationAuditLogDto | null>(null);
  const [detailsModalOpen, setDetailsModalOpen] = useState(false);

  const openDetailsModal = (activityItem: ModuleConfigurationAuditLogDto) => {
    setSelectedActivity(activityItem);
    setDetailsModalOpen(true);
  };

  const getActionBadgeColor = (action: string) => {
    switch (action.toLowerCase()) {
      case 'enabled':
        return 'success';
      case 'disabled':
        return 'danger';
      case 'settings updated':
        return 'info';
      default:
        return 'secondary';
    }
  };

  return (
    <>
      <CCard>
        <CCardHeader>
          <h6 className="mb-0">
            <Icon icon={cilClock} className="me-2" />
            Recent Activity
          </h6>
        </CCardHeader>
        <CCardBody>
          {isLoading ? (
            <div className="text-center">
              <CSpinner />
            </div>
          ) : activity.length === 0 ? (
            <div className="text-center text-muted">
              <Icon icon={cilInfo} size="lg" className="mb-2" />
              <div>No recent activity found</div>
            </div>
          ) : (
            <CListGroup>
              {activity.map((item) => (
                <CListGroupItem 
                  key={item.id}
                  as="button"
                  onClick={() => openDetailsModal(item)}
                  className="d-flex justify-content-between align-items-start"
                >
                  <div className="me-auto">
                    <div className="d-flex align-items-center mb-1">
                      <CBadge color={getActionBadgeColor(item.action)} className="me-2">
                        {item.action}
                      </CBadge>
                      <span className="fw-semibold">{item.moduleTypeName}</span>
                    </div>
                    <div className="small text-muted">
                      <Icon icon={cilUser} className="me-1" />
                      {item.userName} ({item.userEmail})
                    </div>
                    {item.context && (
                      <div className="small text-muted mt-1">
                        <em>{item.context}</em>
                      </div>
                    )}
                  </div>
                  <div className="text-end">
                    <small className="text-muted">
                      {formatDate(item.timestamp)}
                    </small>
                  </div>
                </CListGroupItem>
              ))}
            </CListGroup>
          )}
        </CCardBody>
      </CCard>

      <CModal visible={detailsModalOpen} onClose={() => setDetailsModalOpen(false)} size="lg">
        <CModalHeader>
          <CModalTitle>Activity Details</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {selectedActivity && (
            <div>
              <CRow className="mb-3">
                <CCol sm={4}><strong>Module:</strong></CCol>
                <CCol>{selectedActivity.moduleTypeName}</CCol>
              </CRow>
              <CRow className="mb-3">
                <CCol sm={4}><strong>Action:</strong></CCol>
                <CCol>
                  <CBadge color={getActionBadgeColor(selectedActivity.action)}>
                    {selectedActivity.action}
                  </CBadge>
                </CCol>
              </CRow>
              <CRow className="mb-3">
                <CCol sm={4}><strong>User:</strong></CCol>
                <CCol>{selectedActivity.userName} ({selectedActivity.userEmail})</CCol>
              </CRow>
              <CRow className="mb-3">
                <CCol sm={4}><strong>Timestamp:</strong></CCol>
                <CCol>{formatDate(selectedActivity.timestamp)}</CCol>
              </CRow>
              {selectedActivity.oldValue && (
                <CRow className="mb-3">
                  <CCol sm={4}><strong>Old Value:</strong></CCol>
                  <CCol><code>{selectedActivity.oldValue}</code></CCol>
                </CRow>
              )}
              {selectedActivity.newValue && (
                <CRow className="mb-3">
                  <CCol sm={4}><strong>New Value:</strong></CCol>
                  <CCol><code>{selectedActivity.newValue}</code></CCol>
                </CRow>
              )}
              {selectedActivity.context && (
                <CRow className="mb-3">
                  <CCol sm={4}><strong>Context:</strong></CCol>
                  <CCol><em>{selectedActivity.context}</em></CCol>
                </CRow>
              )}
              {selectedActivity.ipAddress && (
                <CRow className="mb-3">
                  <CCol sm={4}><strong>IP Address:</strong></CCol>
                  <CCol>{selectedActivity.ipAddress}</CCol>
                </CRow>
              )}
              {selectedActivity.userAgent && (
                <CRow className="mb-3">
                  <CCol sm={4}><strong>User Agent:</strong></CCol>
                  <CCol><small className="text-muted">{selectedActivity.userAgent}</small></CCol>
                </CRow>
              )}
            </div>
          )}
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setDetailsModalOpen(false)}>
            Close
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

interface WarningsSectionProps {
  warnings: any[];
}

const WarningsSection: React.FC<WarningsSectionProps> = ({ warnings }) => {
  if (!warnings || warnings.length === 0) {
    return (
      <CCard className="mb-4">
        <CCardHeader>
          <h6 className="mb-0">
            <Icon icon={cilWarning} className="me-2" />
            Configuration Warnings
          </h6>
        </CCardHeader>
        <CCardBody>
          <div className="text-center text-muted">
            <Icon icon={cilCheckCircle} size="lg" className="text-success mb-2" />
            <div>No configuration warnings detected</div>
          </div>
        </CCardBody>
      </CCard>
    );
  }

  const getSeverityColor = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'high':
        return 'danger';
      case 'medium':
        return 'warning';
      case 'low':
        return 'info';
      default:
        return 'secondary';
    }
  };

  return (
    <CCard className="mb-4">
      <CCardHeader>
        <h6 className="mb-0">
          <Icon icon={cilWarning} className="me-2" />
          Configuration Warnings ({warnings.length})
        </h6>
      </CCardHeader>
      <CCardBody>
        <CListGroup>
          {warnings.map((warning, index) => (
            <CListGroupItem key={index}>
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <div className="d-flex align-items-center mb-1">
                    <CBadge color={getSeverityColor(warning.severity)} className="me-2">
                      {warning.severity}
                    </CBadge>
                    <span className="fw-semibold">{warning.warningType}</span>
                  </div>
                  <div className="mb-1">{warning.message}</div>
                  <small className="text-muted">Module: {warning.moduleName}</small>
                </div>
              </div>
            </CListGroupItem>
          ))}
        </CListGroup>
      </CCardBody>
    </CCard>
  );
};

export const ModuleConfigurationDashboard: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview');
  
  const {
    data: dashboard,
    isLoading: dashboardLoading,
    error: dashboardError
  } = useGetModuleConfigurationDashboardQuery({ recentActivityCount: 10 });

  const {
    data: recentActivity = [],
    isLoading: activityLoading
  } = useGetRecentModuleActivityQuery({ count: 20 });

  if (dashboardLoading) {
    return (
      <CCard>
        <CCardBody className="text-center">
          <CSpinner />
          <div className="mt-2">Loading module configuration dashboard...</div>
        </CCardBody>
      </CCard>
    );
  }

  if (dashboardError || !dashboard) {
    return (
      <CCard>
        <CCardBody>
          <CAlert color="danger">
            Failed to load module configuration dashboard. Please try again.
          </CAlert>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <div>
      <CCard className="mb-4">
        <CCardHeader>
          <CRow className="align-items-center">
            <CCol>
              <h4 className="mb-0">
                <Icon icon={cilChart} className="me-2" />
                Module Configuration Dashboard
              </h4>
            </CCol>
            <CCol xs="auto">
              <CButton
                color="primary"
                variant="outline"
                size="sm"
                href="#/settings/modules"
              >
                <Icon icon={cilSettings} className="me-1" />
                Manage Modules
              </CButton>
            </CCol>
          </CRow>
        </CCardHeader>
      </CCard>

      <StatsSection dashboard={dashboard} />

      <CCard>
        <CCardHeader>
          <CNav variant="tabs">
            <CNavItem>
              <CNavLink
                active={activeTab === 'overview'}
                onClick={() => setActiveTab('overview')}
                style={{ cursor: 'pointer' }}
              >
                Overview
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'warnings'}
                onClick={() => setActiveTab('warnings')}
                style={{ cursor: 'pointer' }}
              >
                Warnings {dashboard.warnings?.length > 0 && `(${dashboard.warnings.length})`}
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'activity'}
                onClick={() => setActiveTab('activity')}
                style={{ cursor: 'pointer' }}
              >
                Recent Activity
              </CNavLink>
            </CNavItem>
          </CNav>
        </CCardHeader>
        <CCardBody>
          <CTabContent>
            <CTabPane visible={activeTab === 'overview'}>
              <ModuleStatusSummary dashboard={dashboard} />
            </CTabPane>
            <CTabPane visible={activeTab === 'warnings'}>
              <WarningsSection warnings={dashboard.warnings || []} />
            </CTabPane>
            <CTabPane visible={activeTab === 'activity'}>
              <RecentActivity 
                activity={recentActivity}
                isLoading={activityLoading}
              />
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>
    </div>
  );
};