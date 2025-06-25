import React from 'react';
import { 
  CContainer,
  CRow,
  CCol,
  CBreadcrumb,
  CBreadcrumbItem
} from '@coreui/react';
import { ModuleConfigurationDashboard } from '../../components/settings/ModuleConfigurationDashboard';

const ModuleDashboard: React.FC = () => {
  return (
    <CContainer fluid>
      <CRow className="mb-3">
        <CCol>
          <CBreadcrumb>
            <CBreadcrumbItem href="#/dashboard">Dashboard</CBreadcrumbItem>
            <CBreadcrumbItem href="#/settings">Settings</CBreadcrumbItem>
            <CBreadcrumbItem href="#/settings/modules">Module Configuration</CBreadcrumbItem>
            <CBreadcrumbItem active>Dashboard</CBreadcrumbItem>
          </CBreadcrumb>
        </CCol>
      </CRow>
      
      <CRow>
        <CCol>
          <ModuleConfigurationDashboard />
        </CCol>
      </CRow>
    </CContainer>
  );
};

export default ModuleDashboard;