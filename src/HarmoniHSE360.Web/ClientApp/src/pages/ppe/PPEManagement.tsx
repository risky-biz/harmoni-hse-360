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
  CButton,
  CAlert,
  CSpinner,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faLayerGroup,
  faRulerCombined,
  faWarehouse,
  faPlus,
  faChartLine,
} from '@fortawesome/free-solid-svg-icons';

import { useGetPPEManagementStatsQuery } from '../../features/ppe/ppeManagementApi';
import PPECategoriesManagement from './components/PPECategoriesManagement';
import PPESizesManagement from './components/PPESizesManagement';
import PPEStorageLocationsManagement from './components/PPEStorageLocationsManagement';

const PPEManagement: React.FC = () => {
  const [activeTab, setActiveTab] = useState('categories');
  const { data: stats, isLoading: statsLoading, error: statsError } = useGetPPEManagementStatsQuery();

  return (
    <div className="ppe-management">
      <CRow className="mb-4">
        <CCol>
          <h2 className="mb-0">PPE Management Settings</h2>
          <p className="text-medium-emphasis">
            Manage PPE categories, sizes, and storage locations for your organization
          </p>
        </CCol>
      </CRow>

      {/* Stats Overview */}
      <CRow className="mb-4">
        <CCol>
          <CCard>
            <CCardHeader>
              <FontAwesomeIcon icon={faChartLine} className="me-2" />
              Management Overview
            </CCardHeader>
            <CCardBody>
              {statsLoading && (
                <div className="text-center">
                  <CSpinner color="primary" />
                </div>
              )}
              {statsError && (
                <CAlert color="danger">
                  Failed to load management statistics
                </CAlert>
              )}
              {stats && (
                <CRow>
                  <CCol md={3}>
                    <div className="border-start border-start-4 border-start-info py-2 px-3">
                      <div className="text-medium-emphasis small">Categories</div>
                      <div className="fs-5 fw-semibold">
                        {stats.activeCategories} / {stats.totalCategories}
                      </div>
                    </div>
                  </CCol>
                  <CCol md={3}>
                    <div className="border-start border-start-4 border-start-warning py-2 px-3">
                      <div className="text-medium-emphasis small">Sizes</div>
                      <div className="fs-5 fw-semibold">
                        {stats.activeSizes} / {stats.totalSizes}
                      </div>
                    </div>
                  </CCol>
                  <CCol md={3}>
                    <div className="border-start border-start-4 border-start-success py-2 px-3">
                      <div className="text-medium-emphasis small">Storage Locations</div>
                      <div className="fs-5 fw-semibold">
                        {stats.activeStorageLocations} / {stats.totalStorageLocations}
                      </div>
                    </div>
                  </CCol>
                  <CCol md={3}>
                    <div className="border-start border-start-4 border-start-primary py-2 px-3">
                      <div className="text-medium-emphasis small">Total PPE Items</div>
                      <div className="fs-5 fw-semibold">{stats.totalPPEItems}</div>
                    </div>
                  </CCol>
                </CRow>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Management Tabs */}
      <CCard>
        <CCardHeader>
          <CNav variant="tabs" role="tablist">
            <CNavItem>
              <CNavLink
                active={activeTab === 'categories'}
                onClick={() => setActiveTab('categories')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faLayerGroup} className="me-2" />
                PPE Categories
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'sizes'}
                onClick={() => setActiveTab('sizes')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faRulerCombined} className="me-2" />
                Sizes
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'locations'}
                onClick={() => setActiveTab('locations')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faWarehouse} className="me-2" />
                Storage Locations
              </CNavLink>
            </CNavItem>
          </CNav>
        </CCardHeader>
        <CCardBody>
          <CTabContent>
            <CTabPane visible={activeTab === 'categories'}>
              <PPECategoriesManagement />
            </CTabPane>
            <CTabPane visible={activeTab === 'sizes'}>
              <PPESizesManagement />
            </CTabPane>
            <CTabPane visible={activeTab === 'locations'}>
              <PPEStorageLocationsManagement />
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>
    </div>
  );
};

export default PPEManagement;