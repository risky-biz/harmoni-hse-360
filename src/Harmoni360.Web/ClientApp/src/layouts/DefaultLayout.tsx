import React, { useState, useMemo } from 'react';
import { Outlet, useNavigate, NavLink } from 'react-router-dom';
import { useCompanyName, useWebsiteUrl } from '../contexts/CompanyConfigurationContext';
import {
  CContainer,
  CSidebar,
  CSidebarBrand,
  CSidebarNav,
  CNavItem,
  CNavGroup,
  CNavTitle,
  CHeader,
  CHeaderBrand,
  CHeaderToggler,
  CHeaderNav,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CDropdownHeader,
  CDropdownDivider,
  CAvatar,
  CBadge,
  CFooter,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faBell,
  faBars,
  faTachometerAlt,
  faExclamationTriangle,
  faFileAlt,
  faFileContract,
  faShieldAlt,
  faChartLine,
  faCog,
  faSignOutAlt,
  faUser,
  faLock,
} from '@fortawesome/free-solid-svg-icons';
import { CONTEXT_ICONS, HAZARD_ICONS } from '../utils/iconMappings';
import { createNavigationConfig, filterNavigationByPermissions } from '../utils/navigationUtils';

import { useAppDispatch } from '../store/hooks';
import { useAuth } from '../hooks/useAuth';
import { usePermissions } from '../hooks/usePermissions';
import { useLogoutMutation } from '../features/auth/authApi';
import { logout } from '../features/auth/authSlice';
import ApplicationSettings from '../components/common/ApplicationSettings';
import NotificationBell from '../components/notifications/NotificationBell';
import DemoModeBanner from '../components/common/DemoModeBanner';
// import ThemeToggle from '../components/common/ThemeToggle'; // Temporarily hidden until theme system improvements

// Icon mapping for navigation items
const getNavigationIcon = (name: string) => {
  const iconMap: { [key: string]: React.ReactNode } = {
    'Dashboard': <FontAwesomeIcon icon={faTachometerAlt} className="nav-icon" />,
    'Work Permits': <FontAwesomeIcon icon={faFileAlt} className="nav-icon" />,
    'Work Permit Dashboard': <FontAwesomeIcon icon={faFileAlt} className="nav-icon" />,
    'Submit Work Permit': <FontAwesomeIcon icon={faFileAlt} className="nav-icon" />,
    'View Work Permits': <FontAwesomeIcon icon={faFileAlt} className="nav-icon" />,
    'My Work Permits': <FontAwesomeIcon icon={faFileAlt} className="nav-icon" />,
    'Work Permits (Coming Soon)': <FontAwesomeIcon icon={faFileAlt} className="nav-icon" />,
    'Hazard & Risk': <FontAwesomeIcon icon={HAZARD_ICONS.reporting} className="nav-icon" />,
    'Risk Analytics': <FontAwesomeIcon icon={faChartLine} className="nav-icon" />,
    'Inspections': <FontAwesomeIcon icon={CONTEXT_ICONS.inspection} className="nav-icon" />,
    'Inspection Dashboard': <FontAwesomeIcon icon={CONTEXT_ICONS.inspection} className="nav-icon" />,
    'Create Inspection': <FontAwesomeIcon icon={CONTEXT_ICONS.inspection} className="nav-icon" />,
    'View Inspections': <FontAwesomeIcon icon={CONTEXT_ICONS.inspection} className="nav-icon" />,
    'My Inspections': <FontAwesomeIcon icon={CONTEXT_ICONS.inspection} className="nav-icon" />,
    'Audits': <FontAwesomeIcon icon={CONTEXT_ICONS.audit} className="nav-icon" />,
    'Audit Dashboard': <FontAwesomeIcon icon={CONTEXT_ICONS.audit} className="nav-icon" />,
    'Create Audit': <FontAwesomeIcon icon={CONTEXT_ICONS.audit} className="nav-icon" />,
    'View Audits': <FontAwesomeIcon icon={CONTEXT_ICONS.audit} className="nav-icon" />,
    'My Audits': <FontAwesomeIcon icon={CONTEXT_ICONS.audit} className="nav-icon" />,
    'Incidents': <FontAwesomeIcon icon={CONTEXT_ICONS.incident} className="nav-icon" />,
    'PPE': <FontAwesomeIcon icon={faShieldAlt} className="nav-icon" />,
    'Training': <FontAwesomeIcon icon={CONTEXT_ICONS.training} className="nav-icon" />,
    'Training Dashboard': <FontAwesomeIcon icon={CONTEXT_ICONS.training} className="nav-icon" />,
    'Create Training': <FontAwesomeIcon icon={CONTEXT_ICONS.training} className="nav-icon" />,
    'View Trainings': <FontAwesomeIcon icon={CONTEXT_ICONS.training} className="nav-icon" />,
    'My Trainings': <FontAwesomeIcon icon={CONTEXT_ICONS.training} className="nav-icon" />,
    'Training (Coming Soon)': <FontAwesomeIcon icon={CONTEXT_ICONS.training} className="nav-icon" />,
    'Licenses': <FontAwesomeIcon icon={faFileContract} className="nav-icon" />,
    'Licenses (Coming Soon)': <FontAwesomeIcon icon={faFileContract} className="nav-icon" />,
    'License Dashboard': <FontAwesomeIcon icon={faFileContract} className="nav-icon" />,
    'Create License': <FontAwesomeIcon icon={faFileContract} className="nav-icon" />,
    'View Licenses': <FontAwesomeIcon icon={faFileContract} className="nav-icon" />,
    'My Licenses': <FontAwesomeIcon icon={faFileContract} className="nav-icon" />,
    'Expiring Licenses': <FontAwesomeIcon icon={faFileContract} className="nav-icon" />,
    'Waste Management': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Waste Dashboard': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Waste Reports': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Create Report': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'My Reports': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Disposal Providers': <FontAwesomeIcon icon={CONTEXT_ICONS.disposal} className="nav-icon" />,
    'HSSE Dashboard': <FontAwesomeIcon icon={faChartLine} className="nav-icon" />,
    'Security': <FontAwesomeIcon icon={faLock} className="nav-icon" />,
    'Health Records': <FontAwesomeIcon icon={CONTEXT_ICONS.health} className="nav-icon" />,
    'User Management': <FontAwesomeIcon icon={faUser} className="nav-icon" />,
    'System Settings': <FontAwesomeIcon icon={faCog} className="nav-icon" />,
    'Reports': <FontAwesomeIcon icon={faChartLine} className="nav-icon" />,
  };
  return iconMap[name] || null;
};

const DefaultLayout: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const permissions = usePermissions();
  const [logoutApi] = useLogoutMutation();
  const [sidebarShow, setSidebarShow] = useState(true);
  
  // Get dynamic company configuration
  const companyName = useCompanyName();
  const websiteUrl = useWebsiteUrl();


  // Generate filtered navigation based on user permissions
  const filteredNavigation = useMemo(() => {
    const baseNavigation = createNavigationConfig();
    const filtered = filterNavigationByPermissions(baseNavigation, permissions);
    
    // Add icons to the filtered navigation
    return filtered.map(item => ({
      ...item,
      icon: item.icon || getNavigationIcon(item.name),
      items: item.items?.map(subItem => ({
        ...subItem,
        icon: subItem.icon || getNavigationIcon(subItem.name)
      }))
    }));
  }, [permissions]);

  const handleLogout = async () => {
    try {
      await logoutApi().unwrap();
    } catch (error) {
      // Even if API call fails, we should still logout locally
      console.warn('Logout API call failed:', error);
    } finally {
      dispatch(logout());
      navigate('/login');
    }
  };

  if (!isAuthenticated || !user) {
    return null;
  }

  return (
    <div className={sidebarShow ? 'sidebar-show' : 'sidebar-hide'}>
      <DemoModeBanner sidebarShow={sidebarShow} />
      <CSidebar
        position="fixed"
        unfoldable={false}
        visible={sidebarShow}
        onVisibleChange={(visible) => setSidebarShow(visible)}
        className="d-print-none sidebar sidebar-dark"
      >
        <CSidebarBrand className="d-none d-md-flex" href="/">
          <div className="sidebar-brand-full">
            <img
              src="/Harmoni_360_Logo.png"
              alt="Harmoni 360"
              className="sidebar-logo"
              height="32"
            />
          </div>
          <div className="sidebar-brand-minimized">
            <img
              src="/Harmoni_360_Icon.png"
              alt="HSE"
              className="sidebar-logo-minimized"
              height="24"
            />
          </div>
        </CSidebarBrand>

        <CSidebarNav>
          {filteredNavigation.map((item, index) => {
            if (item.component === 'CNavGroup') {
              return (
                <CNavGroup
                  key={index}
                  toggler={
                    <>
                      {item.icon}
                      {item.name}
                    </>
                  }
                >
                  {item.items?.map((subItem, subIndex) => (
                    <CNavItem key={subIndex}>
                      <NavLink
                        to={subItem.to || '#'}
                        className="nav-link"
                        end={subItem.to === '/incidents' || subItem.to === '/ppe' || subItem.to === '/health' || subItem.to === '/hazards' || subItem.to === '/work-permits' || subItem.to === '/trainings'}
                      >
                        {subItem.name}
                      </NavLink>
                    </CNavItem>
                  ))}
                </CNavGroup>
              );
            } else if (item.component === 'CNavTitle') {
              return <CNavTitle key={index}>{item.name}</CNavTitle>;
            } else if (item.to) {
              return (
                <CNavItem key={index}>
                  <NavLink to={item.to} className="nav-link" end>
                    {item.icon}
                    {item.name}
                  </NavLink>
                </CNavItem>
              );
            } else {
              return null;
            }
          })}
        </CSidebarNav>

        <ApplicationSettings />
      </CSidebar>

      <div
        className={`wrapper d-flex flex-column min-vh-100 ${sidebarShow ? 'sidebar-visible' : 'sidebar-hidden'}`}
        style={{ paddingTop: 'var(--demo-banner-height, 0px)' }}
      >
        <CHeader position="sticky" className="mb-4 p-0 ps-2">
          <CContainer fluid className="px-4">
            <CHeaderToggler
              onClick={() => setSidebarShow(!sidebarShow)}
              style={{ marginInlineStart: '-14px' }}
            >
              <FontAwesomeIcon icon={faBars} size="lg" />
            </CHeaderToggler>

            <CHeaderBrand className="mx-auto d-md-none" href="/">
              Harmoni360
            </CHeaderBrand>

            <CHeaderNav className="ms-auto">
              {/* <ThemeToggle /> Temporarily hidden until theme system improvements */}
              <NotificationBell className="me-3" />

              <CDropdown variant="nav-item" placement="bottom-end">
                <CDropdownToggle className="py-0" caret={false}>
                  <CAvatar size="md" color="primary" textColor="white">
                    {user.name.charAt(0).toUpperCase()}
                  </CAvatar>
                </CDropdownToggle>
                <CDropdownMenu className="pt-0 pr-5 w-auto">
                  <CDropdownHeader className="bg-light fw-semibold py-2">
                    {user.name}
                  </CDropdownHeader>
                  <CDropdownItem>
                    <div className="small text-medium-emphasis">
                      {user.position}
                    </div>
                    <div className="small text-medium-emphasis">
                      {user.department}
                    </div>
                  </CDropdownItem>
                  <CDropdownDivider />
                  <CDropdownItem onClick={() => navigate('/profile')}>
                    <FontAwesomeIcon icon={faUser} className="me-2" />
                    Profile
                  </CDropdownItem>
                  <CDropdownItem onClick={() => navigate('/settings')}>
                    <FontAwesomeIcon icon={faCog} className="me-2" />
                    Settings
                  </CDropdownItem>
                  <CDropdownDivider />
                  <CDropdownItem onClick={handleLogout}>
                    <FontAwesomeIcon icon={faSignOutAlt} className="me-2" />
                    Logout
                  </CDropdownItem>
                </CDropdownMenu>
              </CDropdown>
            </CHeaderNav>
          </CContainer>
        </CHeader>

        <div className="body flex-grow-1 px-4">
          <CContainer lg>
            <Outlet />
          </CContainer>
        </div>

        <CFooter>
          <div>
            <a
              href={websiteUrl}
              target="_blank"
              rel="noopener noreferrer"
            >
              {companyName}
            </a>
            <span className="ms-1">&copy; 2025 Harmoni360</span>
          </div>
          <div className="ms-auto">
          </div>
        </CFooter>
      </div>
    </div>
  );
};

export default DefaultLayout;
