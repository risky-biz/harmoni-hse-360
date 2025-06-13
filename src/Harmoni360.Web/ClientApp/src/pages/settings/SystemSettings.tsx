import React from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CBreadcrumb,
  CBreadcrumbItem,
  CAlert,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faCog,
  faHome,
  faServer,
  faShieldAlt,
  faDatabase,
  faClock,
} from '@fortawesome/free-solid-svg-icons';
import { useNavigate } from 'react-router-dom';

const SystemSettings: React.FC = () => {
  const navigate = useNavigate();

  const systemSettingsSections = [
    {
      title: 'General Configuration',
      icon: faServer,
      description: 'System-wide settings including email, notifications, and integrations',
      status: 'Coming Soon',
      color: 'primary',
    },
    {
      title: 'Security Settings',
      icon: faShieldAlt,
      description: 'Password policies, authentication methods, and security protocols',
      status: 'Coming Soon',
      color: 'warning',
    },
    {
      title: 'Backup & Recovery',
      icon: faDatabase,
      description: 'Database backup schedules and disaster recovery configuration',
      status: 'Coming Soon',
      color: 'info',
    },
    {
      title: 'Scheduled Tasks',
      icon: faClock,
      description: 'Configure automated reports, alerts, and maintenance tasks',
      status: 'Coming Soon',
      color: 'success',
    },
  ];

  return (
    <>
      <CBreadcrumb>
        <CBreadcrumbItem 
          href="#"
          onClick={(e) => {
            e.preventDefault();
            navigate('/');
          }}
        >
          <FontAwesomeIcon icon={faHome} className="me-1" />
          Dashboard
        </CBreadcrumbItem>
        <CBreadcrumbItem active>System Configuration</CBreadcrumbItem>
      </CBreadcrumb>

      <div className="mb-4">
        <h1>
          <FontAwesomeIcon icon={faCog} className="me-2" />
          System Configuration
        </h1>
        <p className="text-medium-emphasis">
          Configure system-wide settings and preferences for Harmoni360.
        </p>
      </div>

      <CAlert color="info" className="mb-4">
        <strong>Note:</strong> For incident-specific configuration such as departments, categories, and locations, 
        please visit the <a href="#" onClick={(e) => { e.preventDefault(); navigate('/settings/incidents'); }}>Incident Management Settings</a>.
      </CAlert>

      <CRow>
        {systemSettingsSections.map((section, index) => (
          <CCol lg={6} key={index} className="mb-4">
            <CCard className="h-100">
              <CCardHeader className={`bg-${section.color} bg-gradient text-white`}>
                <h5 className="mb-0">
                  <FontAwesomeIcon icon={section.icon} className="me-2" />
                  {section.title}
                </h5>
              </CCardHeader>
              <CCardBody>
                <p className="text-muted mb-3">{section.description}</p>
                <div className="text-center py-3">
                  <span className="badge bg-secondary">{section.status}</span>
                </div>
              </CCardBody>
            </CCard>
          </CCol>
        ))}
      </CRow>

      <CRow className="mt-4">
        <CCol>
          <CCard>
            <CCardHeader>
              <h5 className="mb-0">System Information</h5>
            </CCardHeader>
            <CCardBody>
              <dl className="row">
                <dt className="col-sm-3">Application Version</dt>
                <dd className="col-sm-9">Harmoni360 v1.0.0</dd>
                
                <dt className="col-sm-3">Environment</dt>
                <dd className="col-sm-9">{process.env.NODE_ENV || 'Production'}</dd>
                
                <dt className="col-sm-3">API Endpoint</dt>
                <dd className="col-sm-9">{window.location.origin}/api</dd>
                
                <dt className="col-sm-3">Last Updated</dt>
                <dd className="col-sm-9">{new Date().toLocaleDateString()}</dd>
              </dl>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default SystemSettings;