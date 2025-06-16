import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CNav,
  CNavItem,
  CNavLink,
  CRow,
  CTabContent,
  CTabPane,
  CAlert,
  CSpinner,
  CButton,
  CBadge,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CInputGroup,
  CFormInput,
  CFormSelect,
  CButtonGroup,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faUserPlus,
  faTools,
  faClipboardCheck,
  faHistory,
  faExclamationTriangle,
  faSearch,
  faFilter,
  faDownload,
  faEye,
  faUndo,
  faCalendarAlt,
} from '@fortawesome/free-solid-svg-icons';

import { useGetPPEItemsQuery } from '../../features/ppe/ppeApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { formatDate, getPPEStatusBadge, getPPEConditionBadge } from '../../utils/ppeUtils';

const PPEOperationalManagement: React.FC = () => {
  const [activeTab, setActiveTab] = useState('assignments');
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [currentPage, setCurrentPage] = useState(1);

  // Fetch PPE items for operational management
  const { data: ppeData, isLoading, error } = useGetPPEItemsQuery({
    pageNumber: currentPage,
    pageSize: 50,
    searchTerm: searchTerm || undefined,
    status: statusFilter || undefined,
    sortBy: 'itemCode',
    sortDirection: 'asc',
  });

  const items = ppeData?.items || [];

  // Filter items by different operational categories
  const assignedItems = items.filter(item => item.status === 'Assigned');
  const availableItems = items.filter(item => item.status === 'Available');
  const maintenanceItems = items.filter(item => item.isMaintenanceDue || item.status === 'InMaintenance');
  const inspectionItems = items.filter(item => item.isInspectionDue || item.status === 'InInspection');
  const expiringItems = items.filter(item => item.isExpired || item.isExpiringSoon);

  const AssignmentsTab = () => (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h5 className="mb-1">PPE Assignments Management</h5>
          <p className="text-muted mb-0">Manage PPE assignments, returns, and transfers</p>
        </div>
        <div className="d-flex gap-2">
          <CButton color="primary" size="sm">
            <FontAwesomeIcon icon={faUserPlus} className="me-1" />
            Bulk Assign
          </CButton>
          <CButton color="secondary" variant="outline" size="sm">
            <FontAwesomeIcon icon={faDownload} className="me-1" />
            Export
          </CButton>
        </div>
      </div>

      <CRow className="mb-3">
        <CCol md={4}>
          <CInputGroup size="sm">
            <CFormInput
              placeholder="Search assigned items..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
            <CButton type="button" color="primary" variant="outline">
              <FontAwesomeIcon icon={faSearch} />
            </CButton>
          </CInputGroup>
        </CCol>
        <CCol md={3}>
          <CFormSelect size="sm" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="">All Assignments</option>
            <option value="Assigned">Currently Assigned</option>
            <option value="Available">Available</option>
            <option value="InMaintenance">In Maintenance</option>
          </CFormSelect>
        </CCol>
      </CRow>

      <CTable hover responsive>
        <CTableHead>
          <CTableRow>
            <CTableHeaderCell>Item Code</CTableHeaderCell>
            <CTableHeaderCell>Item Name</CTableHeaderCell>
            <CTableHeaderCell>Assigned To</CTableHeaderCell>
            <CTableHeaderCell>Assignment Date</CTableHeaderCell>
            <CTableHeaderCell>Status</CTableHeaderCell>
            <CTableHeaderCell>Actions</CTableHeaderCell>
          </CTableRow>
        </CTableHead>
        <CTableBody>
          {assignedItems.map((item) => (
            <CTableRow key={item.id}>
              <CTableDataCell>
                <strong>{item.itemCode}</strong>
              </CTableDataCell>
              <CTableDataCell>{item.name}</CTableDataCell>
              <CTableDataCell>
                {item.assignedToName || <span className="text-muted">Unassigned</span>}
              </CTableDataCell>
              <CTableDataCell>
                {'-'}
              </CTableDataCell>
              <CTableDataCell>
                {getPPEStatusBadge(item.status)}
              </CTableDataCell>
              <CTableDataCell>
                <CButtonGroup size="sm">
                  <CButton color="primary" variant="outline" size="sm">
                    <FontAwesomeIcon icon={faEye} />
                  </CButton>
                  {item.status === 'Assigned' && (
                    <CButton color="warning" variant="outline" size="sm">
                      <FontAwesomeIcon icon={faUndo} />
                    </CButton>
                  )}
                </CButtonGroup>
              </CTableDataCell>
            </CTableRow>
          ))}
        </CTableBody>
      </CTable>
    </div>
  );

  const MaintenanceTab = () => (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h5 className="mb-1">Maintenance Management</h5>
          <p className="text-muted mb-0">Schedule and track PPE maintenance activities</p>
        </div>
        <div className="d-flex gap-2">
          <CButton color="warning" size="sm">
            <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
            Schedule Maintenance
          </CButton>
          <CButton color="secondary" variant="outline" size="sm">
            <FontAwesomeIcon icon={faDownload} className="me-1" />
            Export Report
          </CButton>
        </div>
      </div>

      <CRow className="mb-4">
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-warning py-2 px-3">
            <div className="text-medium-emphasis small">Due Today</div>
            <div className="fs-5 fw-semibold text-warning">
              {maintenanceItems.filter(item => item.isMaintenanceDue).length}
            </div>
          </div>
        </CCol>
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-danger py-2 px-3">
            <div className="text-medium-emphasis small">Overdue</div>
            <div className="fs-5 fw-semibold text-danger">
              {maintenanceItems.filter(item => item.status === 'InMaintenance').length}
            </div>
          </div>
        </CCol>
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-info py-2 px-3">
            <div className="text-medium-emphasis small">In Progress</div>
            <div className="fs-5 fw-semibold text-info">
              {maintenanceItems.filter(item => item.status === 'InMaintenance').length}
            </div>
          </div>
        </CCol>
      </CRow>

      <CTable hover responsive>
        <CTableHead>
          <CTableRow>
            <CTableHeaderCell>Item Code</CTableHeaderCell>
            <CTableHeaderCell>Item Name</CTableHeaderCell>
            <CTableHeaderCell>Last Maintenance</CTableHeaderCell>
            <CTableHeaderCell>Next Due</CTableHeaderCell>
            <CTableHeaderCell>Status</CTableHeaderCell>
            <CTableHeaderCell>Priority</CTableHeaderCell>
            <CTableHeaderCell>Actions</CTableHeaderCell>
          </CTableRow>
        </CTableHead>
        <CTableBody>
          {maintenanceItems.map((item) => (
            <CTableRow key={item.id}>
              <CTableDataCell>
                <strong>{item.itemCode}</strong>
              </CTableDataCell>
              <CTableDataCell>{item.name}</CTableDataCell>
              <CTableDataCell>
                {'Never'}
              </CTableDataCell>
              <CTableDataCell>
                {'-'}
              </CTableDataCell>
              <CTableDataCell>
                {getPPEStatusBadge(item.status)}
              </CTableDataCell>
              <CTableDataCell>
                {item.isMaintenanceDue ? (
                  <CBadge color="warning">Due</CBadge>
                ) : (
                  <CBadge color="success">On Track</CBadge>
                )}
              </CTableDataCell>
              <CTableDataCell>
                <CButtonGroup size="sm">
                  <CButton color="warning" variant="outline" size="sm">
                    <FontAwesomeIcon icon={faTools} />
                  </CButton>
                  <CButton color="primary" variant="outline" size="sm">
                    <FontAwesomeIcon icon={faEye} />
                  </CButton>
                </CButtonGroup>
              </CTableDataCell>
            </CTableRow>
          ))}
        </CTableBody>
      </CTable>
    </div>
  );

  const InspectionTab = () => (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h5 className="mb-1">Inspection Management</h5>
          <p className="text-muted mb-0">Schedule and track PPE safety inspections</p>
        </div>
        <div className="d-flex gap-2">
          <CButton color="info" size="sm">
            <FontAwesomeIcon icon={faClipboardCheck} className="me-1" />
            Schedule Inspection
          </CButton>
          <CButton color="secondary" variant="outline" size="sm">
            <FontAwesomeIcon icon={faDownload} className="me-1" />
            Export Report
          </CButton>
        </div>
      </div>

      <CRow className="mb-4">
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-info py-2 px-3">
            <div className="text-medium-emphasis small">Due This Week</div>
            <div className="fs-5 fw-semibold text-info">
              {inspectionItems.filter(item => item.isInspectionDue).length}
            </div>
          </div>
        </CCol>
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-success py-2 px-3">
            <div className="text-medium-emphasis small">Completed</div>
            <div className="fs-5 fw-semibold text-success">
              {items.filter(item => !item.isInspectionDue && item.status !== 'InInspection').length}
            </div>
          </div>
        </CCol>
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-warning py-2 px-3">
            <div className="text-medium-emphasis small">In Progress</div>
            <div className="fs-5 fw-semibold text-warning">
              {items.filter(item => item.status === 'InInspection').length}
            </div>
          </div>
        </CCol>
      </CRow>

      <CTable hover responsive>
        <CTableHead>
          <CTableRow>
            <CTableHeaderCell>Item Code</CTableHeaderCell>
            <CTableHeaderCell>Item Name</CTableHeaderCell>
            <CTableHeaderCell>Last Inspection</CTableHeaderCell>
            <CTableHeaderCell>Next Due</CTableHeaderCell>
            <CTableHeaderCell>Inspector</CTableHeaderCell>
            <CTableHeaderCell>Status</CTableHeaderCell>
            <CTableHeaderCell>Actions</CTableHeaderCell>
          </CTableRow>
        </CTableHead>
        <CTableBody>
          {inspectionItems.map((item) => (
            <CTableRow key={item.id}>
              <CTableDataCell>
                <strong>{item.itemCode}</strong>
              </CTableDataCell>
              <CTableDataCell>{item.name}</CTableDataCell>
              <CTableDataCell>-</CTableDataCell>
              <CTableDataCell>-</CTableDataCell>
              <CTableDataCell>-</CTableDataCell>
              <CTableDataCell>
                {item.isInspectionDue ? (
                  <CBadge color="warning">Due</CBadge>
                ) : (
                  <CBadge color="success">Current</CBadge>
                )}
              </CTableDataCell>
              <CTableDataCell>
                <CButtonGroup size="sm">
                  <CButton color="info" variant="outline" size="sm">
                    <FontAwesomeIcon icon={faClipboardCheck} />
                  </CButton>
                  <CButton color="primary" variant="outline" size="sm">
                    <FontAwesomeIcon icon={faEye} />
                  </CButton>
                </CButtonGroup>
              </CTableDataCell>
            </CTableRow>
          ))}
        </CTableBody>
      </CTable>
    </div>
  );

  const ComplianceTab = () => (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h5 className="mb-1">Compliance Monitoring</h5>
          <p className="text-muted mb-0">Monitor PPE expiry dates and compliance status</p>
        </div>
        <div className="d-flex gap-2">
          <CButton color="danger" size="sm">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
            View Alerts
          </CButton>
          <CButton color="secondary" variant="outline" size="sm">
            <FontAwesomeIcon icon={faDownload} className="me-1" />
            Compliance Report
          </CButton>
        </div>
      </div>

      <CRow className="mb-4">
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-danger py-2 px-3">
            <div className="text-medium-emphasis small">Expired Items</div>
            <div className="fs-5 fw-semibold text-danger">
              {expiringItems.filter(item => item.isExpired).length}
            </div>
          </div>
        </CCol>
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-warning py-2 px-3">
            <div className="text-medium-emphasis small">Expiring Soon</div>
            <div className="fs-5 fw-semibold text-warning">
              {expiringItems.filter(item => item.isExpiringSoon && !item.isExpired).length}
            </div>
          </div>
        </CCol>
        <CCol md={3}>
          <div className="border-start border-start-4 border-start-success py-2 px-3">
            <div className="text-medium-emphasis small">Compliant Items</div>
            <div className="fs-5 fw-semibold text-success">
              {items.filter(item => !item.isExpired && !item.isExpiringSoon).length}
            </div>
          </div>
        </CCol>
      </CRow>

      <CTable hover responsive>
        <CTableHead>
          <CTableRow>
            <CTableHeaderCell>Item Code</CTableHeaderCell>
            <CTableHeaderCell>Item Name</CTableHeaderCell>
            <CTableHeaderCell>Expiry Date</CTableHeaderCell>
            <CTableHeaderCell>Days Remaining</CTableHeaderCell>
            <CTableHeaderCell>Compliance Status</CTableHeaderCell>
            <CTableHeaderCell>Actions</CTableHeaderCell>
          </CTableRow>
        </CTableHead>
        <CTableBody>
          {expiringItems.map((item) => (
            <CTableRow key={item.id}>
              <CTableDataCell>
                <strong>{item.itemCode}</strong>
              </CTableDataCell>
              <CTableDataCell>{item.name}</CTableDataCell>
              <CTableDataCell>
                {item.expiryDate ? formatDate(item.expiryDate) : 'No expiry'}
              </CTableDataCell>
              <CTableDataCell>
                {item.isExpired ? (
                  <span className="text-danger">Expired</span>
                ) : item.isExpiringSoon ? (
                  <span className="text-warning">30 days</span>
                ) : (
                  <span className="text-success">Safe</span>
                )}
              </CTableDataCell>
              <CTableDataCell>
                {item.isExpired ? (
                  <CBadge color="danger">Non-Compliant</CBadge>
                ) : item.isExpiringSoon ? (
                  <CBadge color="warning">Warning</CBadge>
                ) : (
                  <CBadge color="success">Compliant</CBadge>
                )}
              </CTableDataCell>
              <CTableDataCell>
                <CButtonGroup size="sm">
                  <CButton color="primary" variant="outline" size="sm">
                    <FontAwesomeIcon icon={faEye} />
                  </CButton>
                  {item.isExpired && (
                    <CButton color="danger" variant="outline" size="sm">
                      <FontAwesomeIcon icon={faExclamationTriangle} />
                    </CButton>
                  )}
                </CButtonGroup>
              </CTableDataCell>
            </CTableRow>
          ))}
        </CTableBody>
      </CTable>
    </div>
  );

  return (
    <PermissionGuard 
      module={ModuleType.PPEManagement} 
      permission={PermissionType.Read}
      fallback={
        <div className="text-center p-4">
          <h3>Access Denied</h3>
          <p>You don't have permission to access PPE operational management.</p>
        </div>
      }
    >
      <div className="ppe-operational-management">
        <CRow className="mb-4">
          <CCol>
            <h2 className="mb-0">PPE Operational Management</h2>
            <p className="text-medium-emphasis">
              Manage day-to-day PPE operations, assignments, maintenance, and compliance
            </p>
          </CCol>
        </CRow>

        {/* Operational Overview Stats */}
        <CRow className="mb-4">
          <CCol>
            <CCard>
              <CCardHeader>
                <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                Operational Overview
              </CCardHeader>
              <CCardBody>
                {isLoading && (
                  <div className="text-center">
                    <CSpinner color="primary" />
                  </div>
                )}
                {error && (
                  <CAlert color="danger">
                    Failed to load operational data
                  </CAlert>
                )}
                {ppeData && (
                  <CRow>
                    <CCol md={3}>
                      <div className="border-start border-start-4 border-start-primary py-2 px-3">
                        <div className="text-medium-emphasis small">Currently Assigned</div>
                        <div className="fs-5 fw-semibold">{assignedItems.length}</div>
                      </div>
                    </CCol>
                    <CCol md={3}>
                      <div className="border-start border-start-4 border-start-success py-2 px-3">
                        <div className="text-medium-emphasis small">Available</div>
                        <div className="fs-5 fw-semibold">{availableItems.length}</div>
                      </div>
                    </CCol>
                    <CCol md={3}>
                      <div className="border-start border-start-4 border-start-warning py-2 px-3">
                        <div className="text-medium-emphasis small">Maintenance Due</div>
                        <div className="fs-5 fw-semibold">{maintenanceItems.length}</div>
                      </div>
                    </CCol>
                    <CCol md={3}>
                      <div className="border-start border-start-4 border-start-danger py-2 px-3">
                        <div className="text-medium-emphasis small">Compliance Issues</div>
                        <div className="fs-5 fw-semibold">{expiringItems.length}</div>
                      </div>
                    </CCol>
                  </CRow>
                )}
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>

        {/* Operational Management Tabs */}
        <CCard>
          <CCardHeader>
            <CNav variant="tabs" role="tablist">
              <CNavItem>
                <CNavLink
                  active={activeTab === 'assignments'}
                  onClick={() => setActiveTab('assignments')}
                  style={{ cursor: 'pointer' }}
                >
                  <FontAwesomeIcon icon={faUserPlus} className="me-2" />
                  Assignments
                </CNavLink>
              </CNavItem>
              <CNavItem>
                <CNavLink
                  active={activeTab === 'maintenance'}
                  onClick={() => setActiveTab('maintenance')}
                  style={{ cursor: 'pointer' }}
                >
                  <FontAwesomeIcon icon={faTools} className="me-2" />
                  Maintenance
                </CNavLink>
              </CNavItem>
              <CNavItem>
                <CNavLink
                  active={activeTab === 'inspection'}
                  onClick={() => setActiveTab('inspection')}
                  style={{ cursor: 'pointer' }}
                >
                  <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                  Inspections
                </CNavLink>
              </CNavItem>
              <CNavItem>
                <CNavLink
                  active={activeTab === 'compliance'}
                  onClick={() => setActiveTab('compliance')}
                  style={{ cursor: 'pointer' }}
                >
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                  Compliance
                </CNavLink>
              </CNavItem>
            </CNav>
          </CCardHeader>
          <CCardBody>
            <CTabContent>
              <CTabPane visible={activeTab === 'assignments'}>
                <AssignmentsTab />
              </CTabPane>
              <CTabPane visible={activeTab === 'maintenance'}>
                <MaintenanceTab />
              </CTabPane>
              <CTabPane visible={activeTab === 'inspection'}>
                <InspectionTab />
              </CTabPane>
              <CTabPane visible={activeTab === 'compliance'}>
                <ComplianceTab />
              </CTabPane>
            </CTabContent>
          </CCardBody>
        </CCard>
      </div>
    </PermissionGuard>
  );
};

export default PPEOperationalManagement;