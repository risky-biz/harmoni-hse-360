import React, { useState, useMemo, useCallback, useEffect } from 'react';
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
  CFormInput,
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
  faSearch,
  faTimes,
  faChevronDown,
} from '@fortawesome/free-solid-svg-icons';
import { CONTEXT_ICONS, HAZARD_ICONS } from '../utils/iconMappings';
import { 
  createNavigationConfig, 
  filterNavigationByPermissions,
  NavigationItem,
  isNavTitle,
  isNavGroup,
  isNavItem,
  hasSubmodules,
  hasItems
} from '../utils/navigationUtils';

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
    'Environment Management': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Environment': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Waste Management': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Waste Dashboard': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Waste Reports': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Create Report': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'My Reports': <FontAwesomeIcon icon={CONTEXT_ICONS.waste} className="nav-icon" />,
    'Disposal Providers': <FontAwesomeIcon icon={CONTEXT_ICONS.disposal} className="nav-icon" />,
    'HSSE Dashboard': <FontAwesomeIcon icon={faChartLine} className="nav-icon" />,
    'HSSE Statistics': <FontAwesomeIcon icon={faChartLine} className="nav-icon" />,
    'Security': <FontAwesomeIcon icon={faLock} className="nav-icon" />,
    'Health Records': <FontAwesomeIcon icon={CONTEXT_ICONS.health} className="nav-icon" />,
    'User Management': <FontAwesomeIcon icon={faUser} className="nav-icon" />,
    'System Settings': <FontAwesomeIcon icon={faCog} className="nav-icon" />,
    'Reports': <FontAwesomeIcon icon={faChartLine} className="nav-icon" />,
  };
  return iconMap[name] || null;
};

// Component for highlighting search matches in text
const HighlightedText: React.FC<{ text: string; searchQuery: string }> = ({ text, searchQuery }) => {
  if (!searchQuery.trim()) {
    return <>{text}</>;
  }

  const regex = new RegExp(`(${searchQuery.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
  const parts = text.split(regex);

  return (
    <>
      {parts.map((part, index) => 
        regex.test(part) ? (
          <mark key={index} className="search-highlight">{part}</mark>
        ) : (
          part
        )
      )}
    </>
  );
};

const DefaultLayout: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const permissions = usePermissions();
  const [logoutApi] = useLogoutMutation();
  const [sidebarShow, setSidebarShow] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [expandedGroups, setExpandedGroups] = useState<Set<string>>(new Set());
  
  // Get dynamic company configuration
  const companyName = useCompanyName();
  const websiteUrl = useWebsiteUrl();

  // Helper function to get module status class
  const getModuleStatusClass = (item: NavigationItem): string => {
    const classes = [`depth-${0}`]; // Module level is always depth 0
    
    // Add module status classes based on item properties or future module status
    if (item.className?.includes('disabled')) {
      classes.push('module-disabled');
    }
    if (item.className?.includes('maintenance')) {
      classes.push('module-maintenance');
    }
    if (item.className?.includes('coming-soon')) {
      classes.push('module-coming-soon');
    }
    
    return classes.join(' ');
  };

  // Search filter function with auto-expand tracking
  const filterBySearch = useCallback((items: NavigationItem[], query: string): NavigationItem[] => {
    if (!query.trim()) {
      // Clear expanded groups when search is cleared
      setExpandedGroups(new Set());
      return items;
    }
    
    const lowerQuery = query.toLowerCase();
    const newExpandedGroups = new Set<string>();
    
    const filterRecursive = (items: NavigationItem[], parentKey?: string): NavigationItem[] => {
      return items.reduce((filtered: NavigationItem[], item) => {
        let matchFound = false;
        let hasChildMatches = false;
        let filteredItem = { ...item };
        const itemKey = parentKey ? `${parentKey}-${item.name}` : item.name;
        
        // Check if the item name matches
        if (item.name.toLowerCase().includes(lowerQuery)) {
          matchFound = true;
        }
        
        // For CNavTitle with submodules, search recursively
        if (hasSubmodules(item)) {
          const filteredSubmodules = filterRecursive(item.submodules!, itemKey);
          if (filteredSubmodules.length > 0) {
            matchFound = true;
            hasChildMatches = true;
            filteredItem.submodules = filteredSubmodules;
            
            // Mark any groups within submodules for expansion
            filteredSubmodules.forEach(submodule => {
              if (isNavGroup(submodule)) {
                newExpandedGroups.add(`${itemKey}-${submodule.name}`);
              }
            });
          }
        }
        
        // For CNavGroup with items, search recursively
        if (hasItems(item)) {
          const filteredItems = filterRecursive(item.items!, itemKey);
          if (filteredItems.length > 0) {
            matchFound = true;
            hasChildMatches = true;
            filteredItem.items = filteredItems;
          }
        }
        
        // Mark group for expansion if it has child matches (regardless of self match)
        if (isNavGroup(item) && hasChildMatches) {
          newExpandedGroups.add(itemKey);
        }
        
        if (matchFound) {
          filtered.push(filteredItem);
        }
        
        return filtered;
      }, []);
    };
    
    const result = filterRecursive(items);
    
    // Debug logging
    console.log('Search query:', query);
    console.log('Expanded groups:', Array.from(newExpandedGroups));
    
    // Update expanded groups state
    setExpandedGroups(newExpandedGroups);
    
    return result;
  }, []);

  // Hierarchical navigation rendering function with proper module wrapping and highlighting
  const renderNavigationItems = (items: NavigationItem[], depth: number = 0, parentKey: string = ''): React.ReactNode[] => {
    return items.map((item, index) => {
      const itemKey = parentKey ? `${parentKey}-${item.name}` : item.name;
      
      // Handle CNavTitle with submodules - Create module wrapper
      if (isNavTitle(item) && hasSubmodules(item)) {
        const moduleClasses = `nav-module-wrapper ${getModuleStatusClass(item)}`;
        
        return (
          <li key={index} className={moduleClasses} data-module={item.module || item.name}>
            <CNavTitle className={`nav-module-title depth-${depth}`}>
              <HighlightedText text={item.name} searchQuery={searchQuery} />
            </CNavTitle>
            <div className="nav-module-content">
              {renderNavigationItems(item.submodules!, depth + 1, itemKey)}
            </div>
          </li>
        );
      }

      // Handle CNavGroup
      if (isNavGroup(item)) {
        const isExpanded = expandedGroups.has(itemKey);
        
        // Debug logging
        if (searchQuery && item.name.toLowerCase().includes('permit')) {
          console.log(`CNavGroup: ${item.name}, itemKey: ${itemKey}, isExpanded: ${isExpanded}, expandedGroups:`, Array.from(expandedGroups));
        }
        
        // Force expansion by conditionally rendering different components
        if (isExpanded) {
          return (
            <div key={`${index}-expanded-${searchQuery}`} className={`nav-group depth-${depth} nav-group-expanded`}>
              <div className="nav-group-toggler nav-group-toggler-expanded">
                {item.icon}
                <HighlightedText text={item.name} searchQuery={searchQuery} />
                <span className="nav-group-toggle-icon ms-auto">
                  <FontAwesomeIcon icon={faChevronDown} />
                </span>
              </div>
              <div className="nav-group-items show">
                {hasItems(item) && renderNavigationItems(item.items!, depth + 1, itemKey)}
              </div>
            </div>
          );
        } else {
          return (
            <CNavGroup
              key={`${index}-collapsed-${searchQuery}`}
              toggler={
                <>
                  {item.icon}
                  <HighlightedText text={item.name} searchQuery={searchQuery} />
                </>
              }
              className={`nav-group depth-${depth}`}
            >
              {hasItems(item) && renderNavigationItems(item.items!, depth + 1, itemKey)}
            </CNavGroup>
          );
        }
      }

      // Handle CNavItem
      if (isNavItem(item) && item.to) {
        return (
          <CNavItem key={index} className={`nav-item depth-${depth}`}>
            <NavLink 
              to={item.to} 
              className="nav-link" 
              end={item.to === '/incidents' || item.to === '/ppe' || item.to === '/health' || item.to === '/hazards' || item.to === '/work-permits' || item.to === '/trainings' || item.to === '/inspections' || item.to === '/audits' || item.to === '/licenses' || item.to === '/waste-management'}
            >
              {depth < 2 && item.icon}
              <HighlightedText text={item.name} searchQuery={searchQuery} />
            </NavLink>
          </CNavItem>
        );
      }

      return null;
    }).filter(Boolean);
  };


  // Generate filtered navigation based on user permissions and search
  const filteredNavigation = useMemo(() => {
    const baseNavigation = createNavigationConfig();
    const permissionFiltered = filterNavigationByPermissions(baseNavigation, permissions);
    
    // Add icons to the filtered navigation recursively
    const addIcons = (items: NavigationItem[]): NavigationItem[] => {
      return items.map(item => ({
        ...item,
        icon: item.icon || getNavigationIcon(item.name),
        items: item.items ? addIcons(item.items) : undefined,
        submodules: item.submodules ? addIcons(item.submodules) : undefined
      }));
    };
    
    const withIcons = addIcons(permissionFiltered);
    
    // Apply search filter
    return filterBySearch(withIcons, searchQuery);
  }, [permissions, searchQuery, filterBySearch]);

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

        {/* Module Search */}
        <div className="sidebar-search px-3 py-2">
          <div className="position-relative">
            <CFormInput
              type="text"
              placeholder="Search modules..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="ps-5"
              size="sm"
            />
            <FontAwesomeIcon 
              icon={faSearch} 
              className="position-absolute top-50 start-0 translate-middle-y ms-3 text-muted"
              style={{ fontSize: '0.875rem' }}
            />
            {searchQuery && (
              <button
                className="btn btn-link position-absolute top-50 end-0 translate-middle-y me-2 p-0 text-muted"
                onClick={() => setSearchQuery('')}
                style={{ fontSize: '0.875rem' }}
              >
                <FontAwesomeIcon icon={faTimes} />
              </button>
            )}
          </div>
          {searchQuery && (
            <div className="text-muted small mt-1">
              {filteredNavigation.length === 0 ? 'No results found' : `${filteredNavigation.length} result(s) found`}
            </div>
          )}
        </div>

        <CSidebarNav>
          {renderNavigationItems(filteredNavigation)}
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
