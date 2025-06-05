import React, { useState } from 'react';
import {
  CButton,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CDropdownHeader,
  CDropdownDivider,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCog, faChevronUp } from '@fortawesome/free-solid-svg-icons';
import { useAuth } from '../../hooks/useAuth';
import { useNavigate } from 'react-router-dom';

interface ProjectSettingsProps {
  className?: string;
}

const ProjectSettings: React.FC<ProjectSettingsProps> = ({
  className = '',
}) => {
  const { user } = useAuth();
  const [visible, setVisible] = useState(false);
  const navigate = useNavigate();

  // Check if user has system configuration permissions
  const hasSystemAccess =
    user?.roles?.some(
      (role) => role === 'SuperAdmin' || role === 'Developer'
    ) ||
    user?.permissions?.some(
      (permission) =>
        permission === 'system.configure' || permission === 'system.modules'
    );

  // Don't render if user doesn't have access
  if (!hasSystemAccess) {
    return null;
  }

  const modules = [
    {
      name: 'Incidents Management',
      description: 'Configure incident reporting and management settings',
      path: '/settings/incidents',
      icon: 'faExclamationTriangle',
    },
    {
      name: 'Risk Management',
      description: 'Configure risk assessment and management settings',
      path: '/settings/risks',
      icon: 'faShieldAlt',
    },
    {
      name: 'User Management',
      description: 'Manage users, roles, and permissions',
      path: '/settings/users',
      icon: 'faUsers',
    },
    {
      name: 'System Configuration',
      description: 'General system settings and configuration',
      path: '/settings/system',
      icon: 'faCog',
    },
    {
      name: 'Audit & Compliance',
      description: 'Configure audit trails and compliance settings',
      path: '/settings/audit',
      icon: 'faFileAlt',
    },
  ];

  const handleModuleClick = (path: string) => {
    setVisible(false);
    navigate(path);
  };

  return (
    <div className={`project-settings ${className}`}>
      <CDropdown
        placement="top-start"
        visible={visible}
        onShow={() => setVisible(true)}
        onHide={() => setVisible(false)}
      >
        <CDropdownToggle
          as={CButton}
          color="dark"
          variant="ghost"
          className="w-100 d-flex align-items-center justify-content-between text-start project-settings-toggle"
        >
          <div className="d-flex align-items-center">
            <FontAwesomeIcon icon={faCog} className="me-2" />
            <span>Project Settings</span>
          </div>
          <FontAwesomeIcon
            icon={faChevronUp}
            className={`transition-transform ${visible ? 'rotate-180' : ''}`}
            style={{ fontSize: '12px' }}
          />
        </CDropdownToggle>

        <CDropdownMenu className="project-settings-menu">
          <CDropdownHeader>
            <strong>System Modules</strong>
          </CDropdownHeader>
          <CDropdownDivider />

          {modules.map((module, index) => (
            <CDropdownItem
              key={index}
              onClick={() => handleModuleClick(module.path)}
              className="project-settings-item"
            >
              <div className="d-flex flex-column">
                <div className="fw-semibold">{module.name}</div>
                <small className="text-muted">{module.description}</small>
              </div>
            </CDropdownItem>
          ))}

          <CDropdownDivider />

          <CDropdownItem className="text-muted small">
            Available to: {user?.roles?.join(', ')}
          </CDropdownItem>
        </CDropdownMenu>
      </CDropdown>
    </div>
  );
};

export default ProjectSettings;
