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
import DepartmentManagement from './components/DepartmentManagement';
import CategoryManagement from './components/CategoryManagement';
import LocationManagement from './components/LocationManagement';

const IncidentSettings: React.FC = () => {
  const [activeTab, setActiveTab] = useState('departments');

  return (
    <div>
      <h1 className="mb-4">Incident Management Settings</h1>
      
      <CCard>
        <CCardHeader>
          <CNav variant="tabs">
            <CNavItem>
              <CNavLink
                active={activeTab === 'departments'}
                onClick={() => setActiveTab('departments')}
                style={{ cursor: 'pointer' }}
              >
                Departments
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'categories'}
                onClick={() => setActiveTab('categories')}
                style={{ cursor: 'pointer' }}
              >
                Incident Categories
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'locations'}
                onClick={() => setActiveTab('locations')}
                style={{ cursor: 'pointer' }}
              >
                Locations
              </CNavLink>
            </CNavItem>
          </CNav>
        </CCardHeader>
        <CCardBody>
          <CTabContent>
            <CTabPane visible={activeTab === 'departments'}>
              <DepartmentManagement />
            </CTabPane>
            <CTabPane visible={activeTab === 'categories'}>
              <CategoryManagement />
            </CTabPane>
            <CTabPane visible={activeTab === 'locations'}>
              <LocationManagement />
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>
    </div>
  );
};

export default IncidentSettings;