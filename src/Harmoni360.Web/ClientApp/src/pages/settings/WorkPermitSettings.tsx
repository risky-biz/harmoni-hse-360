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
  CAlert,
  CSpinner,
  CButton,
  CRow,
  CCol,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faFileAlt,
  faVideo,
  faPlus,
  faCog,
} from '@fortawesome/free-solid-svg-icons';
import { FormConfigurationTab } from '../../components/settings/work-permits/FormConfigurationTab';
import { VideoManagementTab } from '../../components/settings/work-permits/VideoManagementTab';
import { useGetWorkPermitSettingsQuery } from '../../services/workPermitSettingsApi';

const WorkPermitSettings: React.FC = () => {
  const [activeTab, setActiveTab] = useState('form-config');
  const { data: settings, isLoading, error, refetch } = useGetWorkPermitSettingsQuery();

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-50">
        <CSpinner color="primary" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        <h4>Error Loading Settings</h4>
        <p>Failed to load work permit settings. Please try again later.</p>
      </CAlert>
    );
  }

  return (
    <div className="work-permit-settings">
      <CRow className="mb-3 mb-md-4">
        <CCol>
          <h1 className="page-title fs-4 fs-md-3">
            <FontAwesomeIcon icon={faCog} className="me-2" />
            <span className="d-none d-sm-inline">Work Permit Settings</span>
            <span className="d-sm-none">WP Settings</span>
          </h1>
          <p className="text-muted small">
            Configure work permit forms, safety induction videos, and approval workflows
          </p>
        </CCol>
      </CRow>

      <CCard>
        <CCardHeader className="p-2 p-md-3">
          <CNav variant="tabs" role="tablist" className="nav-responsive">
            <CNavItem className="flex-fill">
              <CNavLink
                active={activeTab === 'form-config'}
                onClick={() => setActiveTab('form-config')}
                style={{ cursor: 'pointer' }}
                className="text-center py-2 px-1 px-md-3"
              >
                <FontAwesomeIcon icon={faFileAlt} className="me-1 me-md-2" />
                <span className="d-none d-md-inline">Form Configuration</span>
                <span className="d-md-none small">Form</span>
              </CNavLink>
            </CNavItem>
            <CNavItem className="flex-fill">
              <CNavLink
                active={activeTab === 'videos'}
                onClick={() => setActiveTab('videos')}
                style={{ cursor: 'pointer' }}
                className="text-center py-2 px-1 px-md-3"
              >
                <FontAwesomeIcon icon={faVideo} className="me-1 me-md-2" />
                <span className="d-none d-md-inline">Safety Induction Videos</span>
                <span className="d-md-none small">Videos</span>
              </CNavLink>
            </CNavItem>
          </CNav>
        </CCardHeader>
        <CCardBody className="p-2 p-md-3">
          <CTabContent>
            <CTabPane visible={activeTab === 'form-config'}>
              <FormConfigurationTab settings={settings || []} />
            </CTabPane>
            <CTabPane visible={activeTab === 'videos'}>
              <VideoManagementTab settings={settings || []} onDataChange={refetch} />
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>
    </div>
  );
};

export default WorkPermitSettings;