import React, { useState } from 'react';
import { Outlet, useNavigate, NavLink } from 'react-router-dom';
import {
  CContainer,
  CSidebar,
  CSidebarBrand,
  CSidebarNav,
  CSidebarToggler,
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
import CIcon from '@coreui/icons-react';
import {
  cilBell,
  cilMenu,
  cilSpeedometer,
  cilWarning,
  cilTask,
  cilFile,
  cilClipboard,
  cilShieldAlt,
  cilChartLine,
  cilSettings,
  cilAccountLogout,
  cilUser,
} from '@coreui/icons';

import { useAppDispatch } from '../store/hooks';
import { useAuth } from '../hooks/useAuth';
import { useLogoutMutation } from '../features/auth/authApi';
import { logout } from '../features/auth/authSlice';

// Navigation configuration
const navigation = [
  {
    component: CNavItem,
    name: 'Dashboard',
    to: '/dashboard',
    icon: <CIcon icon={cilSpeedometer} customClassName="nav-icon" />,
  },
  {
    component: CNavTitle,
    name: 'Incident Management',
  },
  {
    component: CNavGroup,
    name: 'Incidents',
    to: '/incidents',
    icon: <CIcon icon={cilWarning} customClassName="nav-icon" />,
    items: [
      {
        component: CNavItem,
        name: 'Report Incident',
        to: '/incidents/create',
      },
      {
        component: CNavItem,
        name: 'View Incidents',
        to: '/incidents',
      },
      {
        component: CNavItem,
        name: 'My Reports',
        to: '/incidents/my-reports',
      },
    ],
  },
  {
    component: CNavTitle,
    name: 'Risk Management',
  },
  {
    component: CNavGroup,
    name: 'Hazards',
    to: '/hazards',
    icon: <CIcon icon={cilTask} customClassName="nav-icon" />,
    items: [
      {
        component: CNavItem,
        name: 'Report Hazard',
        to: '/hazards/report',
      },
      {
        component: CNavItem,
        name: 'Hazard Register',
        to: '/hazards/register',
      },
    ],
  },
  {
    component: CNavItem,
    name: 'Risk Register',
    to: '/risks/register',
    icon: <CIcon icon={cilClipboard} customClassName="nav-icon" />,
  },
  {
    component: CNavTitle,
    name: 'Compliance',
  },
  {
    component: CNavItem,
    name: 'Audits',
    to: '/audits',
    icon: <CIcon icon={cilFile} customClassName="nav-icon" />,
  },
  {
    component: CNavItem,
    name: 'Training',
    to: '/training',
    icon: <CIcon icon={cilShieldAlt} customClassName="nav-icon" />,
  },
  {
    component: CNavTitle,
    name: 'Analytics',
  },
  {
    component: CNavItem,
    name: 'Reports',
    to: '/reports',
    icon: <CIcon icon={cilChartLine} customClassName="nav-icon" />,
  },
];

const DefaultLayout: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const [logoutApi] = useLogoutMutation();
  const [sidebarShow, setSidebarShow] = useState(true);

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
    <div>
      <CSidebar
        position="fixed"
        unfoldable={false}
        visible={sidebarShow}
        onVisibleChange={(visible) => setSidebarShow(visible)}
        className="d-print-none sidebar sidebar-dark"
      >
        <CSidebarBrand className="d-none d-md-flex" href="/">
          <div className="sidebar-brand-full">
            <strong>HarmoniHSE360</strong>
          </div>
          <div className="sidebar-brand-minimized">
            <strong>HSE</strong>
          </div>
        </CSidebarBrand>
        
        <CSidebarNav>
          {navigation.map((item, index) => {
            if (item.component === CNavGroup) {
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
                      <NavLink to={subItem.to} className="nav-link">
                        {subItem.name}
                      </NavLink>
                    </CNavItem>
                  ))}
                </CNavGroup>
              );
            } else if (item.component === CNavTitle) {
              return <CNavTitle key={index}>{item.name}</CNavTitle>;
            } else if (item.to) {
              return (
                <CNavItem key={index}>
                  <NavLink to={item.to} className="nav-link">
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
        
        <CSidebarToggler
          className="d-none d-lg-flex"
          onClick={() => setSidebarShow(!sidebarShow)}
        />
      </CSidebar>
      
      <div className="wrapper d-flex flex-column min-vh-100">
        <CHeader position="sticky" className="mb-4 p-0 ps-2">
          <CContainer fluid className="px-4">
            <CHeaderToggler
              onClick={() => setSidebarShow(!sidebarShow)}
              style={{ marginInlineStart: '-14px' }}
            >
              <CIcon icon={cilMenu} size="lg" />
            </CHeaderToggler>
            
            <CHeaderBrand className="mx-auto d-md-none" href="/">
              HarmoniHSE360
            </CHeaderBrand>
            
            <CHeaderNav className="ms-auto">
              <CDropdown variant="nav-item" placement="bottom-end">
                <CDropdownToggle caret={false}>
                  <CIcon icon={cilBell} size="lg" />
                  <CBadge
                    color="danger"
                    position="top-end"
                    shape="rounded-pill"
                    className="p-1"
                  >
                    3
                  </CBadge>
                </CDropdownToggle>
                <CDropdownMenu className="pt-0">
                  <CDropdownHeader className="bg-light fw-semibold py-2">
                    You have 3 notifications
                  </CDropdownHeader>
                  <CDropdownItem>
                    <div className="d-flex">
                      <div className="flex-grow-1">
                        <h6 className="mb-1">New incident reported</h6>
                        <p className="mb-1 small text-medium-emphasis">
                          Safety incident in Chemistry Lab
                        </p>
                        <small className="text-medium-emphasis">2 min ago</small>
                      </div>
                    </div>
                  </CDropdownItem>
                  <CDropdownDivider />
                  <CDropdownItem href="#" className="text-center fw-semibold">
                    View all notifications
                  </CDropdownItem>
                </CDropdownMenu>
              </CDropdown>
              
              <CDropdown variant="nav-item" placement="bottom-end">
                <CDropdownToggle className="py-0" caret={false}>
                  <CAvatar 
                    size="md" 
                    color="primary" 
                    textColor="white"
                  >
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
                    <CIcon icon={cilUser} className="me-2" />
                    Profile
                  </CDropdownItem>
                  <CDropdownItem onClick={() => navigate('/settings')}>
                    <CIcon icon={cilSettings} className="me-2" />
                    Settings
                  </CDropdownItem>
                  <CDropdownDivider />
                  <CDropdownItem onClick={handleLogout}>
                    <CIcon icon={cilAccountLogout} className="me-2" />
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
            <a href="https://bsj.sch.id" target="_blank" rel="noopener noreferrer">
              British School Jakarta
            </a>
            <span className="ms-1">&copy; 2025 HarmoniHSE360</span>
          </div>
          <div className="ms-auto">
            <span className="me-1">Powered by</span>
            <a href="https://coreui.io/react" target="_blank" rel="noopener noreferrer">
              CoreUI React
            </a>
          </div>
        </CFooter>
      </div>
    </div>
  );
};

export default DefaultLayout;