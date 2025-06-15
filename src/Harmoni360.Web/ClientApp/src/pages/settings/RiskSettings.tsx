import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { HAZARD_ICONS } from '../../utils/iconMappings';
import HazardCategoryManagement from './components/HazardCategoryManagement';
import HazardTypeManagement from './components/HazardTypeManagement';

const RiskSettings: React.FC = () => {
  const [activeTab, setActiveTab] = useState('categories');

  return (
    <div>
      <div className="mb-4 d-flex align-items-center">
        <FontAwesomeIcon icon={HAZARD_ICONS.reporting} className="me-2 text-warning" size="lg" />
        <h1 className="mb-0">Risk Management Settings</h1>
      </div>
      
      <CCard>
        <CCardHeader>
          <CNav variant="tabs">
            <CNavItem>
              <CNavLink
                active={activeTab === 'categories'}
                onClick={() => setActiveTab('categories')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={HAZARD_ICONS.general} className="me-1" />
                Hazard Categories
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'types'}
                onClick={() => setActiveTab('types')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={HAZARD_ICONS.warning} className="me-1" />
                Hazard Types
              </CNavLink>
            </CNavItem>
          </CNav>
        </CCardHeader>
        <CCardBody>
          <CTabContent>
            <CTabPane visible={activeTab === 'categories'}>
              <HazardCategoryManagement />
            </CTabPane>
            <CTabPane visible={activeTab === 'types'}>
              <HazardTypeManagement />
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>
    </div>
  );
};

export default RiskSettings;