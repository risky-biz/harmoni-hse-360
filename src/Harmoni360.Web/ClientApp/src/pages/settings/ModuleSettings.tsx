import React from 'react';
import { 
  CContainer,
  CRow,
  CCol,
  CBreadcrumb,
  CBreadcrumbItem
} from '@coreui/react';
import { ModuleConfigurationList } from '../../components/settings/ModuleConfigurationList';

const ModuleSettings: React.FC = () => {
  return (
    <CContainer fluid>
      <CRow className="mb-3">
        <CCol>
          <CBreadcrumb>
            <CBreadcrumbItem href="#/dashboard">Dashboard</CBreadcrumbItem>
            <CBreadcrumbItem href="#/settings">Settings</CBreadcrumbItem>
            <CBreadcrumbItem active>Module Configuration</CBreadcrumbItem>
          </CBreadcrumb>
        </CCol>
      </CRow>
      
      <CRow>
        <CCol>
          <ModuleConfigurationList showDashboardLink={true} />
        </CCol>
      </CRow>
    </CContainer>
  );
};

export default ModuleSettings;