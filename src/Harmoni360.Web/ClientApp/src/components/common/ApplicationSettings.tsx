import React, { useState, useCallback, useRef, useEffect } from 'react';
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

interface ApplicationSettingsProps {
  className?: string;
}

const ApplicationSettings: React.FC<ApplicationSettingsProps> = ({
  className = '',
}) => {
  const { user } = useAuth();
  const [visible, setVisible] = useState(false);
  const navigate = useNavigate();
  const timeoutRef = useRef<number | null>(null);
  const isNavigatingRef = useRef(false);
  const scrollTimeoutRef = useRef<number | null>(null);
  const menuRef = useRef<HTMLUListElement | null>(null);
  const isScrollingRef = useRef(false);

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
      name: 'PPE Management',
      description: 'Manage PPE categories, sizes, and storage locations',
      path: '/settings/ppe',
      icon: 'faShieldAlt',
    },
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
      name: 'Work Permit Management',
      description: 'Configure work permit settings and safety induction videos',
      path: '/settings/work-permits',
      icon: 'faClipboardCheck',
    },
    {
      name: 'Module Configuration',
      description: 'Enable/disable modules and configure module settings',
      path: '/settings/modules',
      icon: 'faCog',
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

  // Debounced scroll handler to prevent excessive scroll events
  const handleScroll = useCallback(() => {
    if (!menuRef.current) return;
    
    isScrollingRef.current = true;
    
    // Clear any existing scroll timeout
    if (scrollTimeoutRef.current) {
      clearTimeout(scrollTimeoutRef.current);
    }
    
    // Add smooth scrolling class during scroll
    menuRef.current.classList.add('is-scrolling');
    
    // Debounce scroll end detection
    scrollTimeoutRef.current = window.setTimeout(() => {
      isScrollingRef.current = false;
      if (menuRef.current) {
        menuRef.current.classList.remove('is-scrolling');
      }
    }, 150); // 150ms debounce for scroll end detection
  }, []);

  // Enhanced hover handlers that respect scroll state
  const handleItemMouseEnter = useCallback((event: React.MouseEvent) => {
    // Don't apply hover effects while scrolling
    if (isScrollingRef.current) {
      event.preventDefault();
      return;
    }
    
    const target = event.currentTarget as HTMLElement;
    target.classList.add('item-hover');
  }, []);

  const handleItemMouseLeave = useCallback((event: React.MouseEvent) => {
    const target = event.currentTarget as HTMLElement;
    target.classList.remove('item-hover');
  }, []);

  // Cleanup timeouts on unmount
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
      if (scrollTimeoutRef.current) {
        clearTimeout(scrollTimeoutRef.current);
      }
    };
  }, []);

  // Setup scroll event listener when menu becomes visible
  useEffect(() => {
    if (visible && menuRef.current) {
      const menuElement = menuRef.current;
      menuElement.addEventListener('scroll', handleScroll, { passive: true });
      
      return () => {
        menuElement.removeEventListener('scroll', handleScroll);
      };
    }
  }, [visible, handleScroll]);

  // Handle dropdown visibility with debouncing to prevent glitching
  const handleShow = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    if (!isNavigatingRef.current) {
      setVisible(true);
    }
  }, []);

  const handleHide = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    // Add small delay to prevent glitching when user moves mouse quickly
    timeoutRef.current = window.setTimeout(() => {
      if (!isNavigatingRef.current) {
        setVisible(false);
      }
    }, 100);
  }, []);

  const handleModuleClick = useCallback((path: string) => {
    isNavigatingRef.current = true;
    
    // Clear any pending timeouts
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    
    // Immediately hide dropdown
    setVisible(false);
    
    // Navigate with slight delay to ensure dropdown closes smoothly
    window.setTimeout(() => {
      navigate(path);
      // Reset navigation flag after navigation
      window.setTimeout(() => {
        isNavigatingRef.current = false;
      }, 100);
    }, 50);
  }, [navigate]);

  return (
    <div className={`application-settings ${className}`}>
      <CDropdown
        placement="top-start"
        visible={visible}
        onShow={handleShow}
        onHide={handleHide}
      >
        <CDropdownToggle
          as={CButton}
          color="dark"
          variant="ghost"
          className="w-100 d-flex align-items-center justify-content-between text-start application-settings-toggle"
        >
          <div className="d-flex align-items-center">
            <FontAwesomeIcon icon={faCog} className="me-2" />
            <span>Application Settings</span>
          </div>
          <FontAwesomeIcon
            icon={faChevronUp}
            className={`transition-transform ${visible ? 'rotate-180' : ''}`}
            style={{ fontSize: '12px' }}
          />
        </CDropdownToggle>

        <CDropdownMenu 
          className="application-settings-menu"
          ref={menuRef}
        >
          <CDropdownHeader>
            <strong>System Modules</strong>
          </CDropdownHeader>
          <CDropdownDivider />

          {modules.map((module, index) => (
            <CDropdownItem
              key={index}
              onClick={() => handleModuleClick(module.path)}
              onMouseEnter={handleItemMouseEnter}
              onMouseLeave={handleItemMouseLeave}
              className="application-settings-item"
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

export default ApplicationSettings;
