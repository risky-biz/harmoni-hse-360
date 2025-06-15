import { DashboardLayout, DashboardWidget, QuickAction } from '../types/dashboard';
import { ACTION_ICONS, CONTEXT_ICONS, HAZARD_ICONS } from '../utils/iconMappings';

// Quick Actions Configuration
export const getQuickActions = (navigate: (path: string) => void): QuickAction[] => [
  {
    id: 'report-incident',
    label: 'Report Incident',
    icon: CONTEXT_ICONS.incident,
    action: () => navigate('/incidents/create'),
    color: 'primary',
    variant: 'solid',
  },
  {
    id: 'view-incidents',
    label: 'View All Incidents',
    icon: CONTEXT_ICONS.report,
    action: () => navigate('/incidents'),
    color: 'secondary',
    variant: 'outline',
  },
  {
    id: 'my-reports',
    label: 'My Reports',
    icon: CONTEXT_ICONS.user,
    action: () => navigate('/incidents/my-reports'),
    color: 'info',
    variant: 'outline',
  },
  {
    id: 'hazard-report',
    label: 'Report Hazard',
    icon: HAZARD_ICONS.reporting,
    action: () => navigate('/hazards/create'),
    color: 'warning',
    variant: 'outline',
    permissions: ['HazardManagement.Create'],
  },
  {
    id: 'health-records',
    label: 'Health Records',
    icon: CONTEXT_ICONS.health,
    action: () => navigate('/health'),
    color: 'success',
    variant: 'outline',
    permissions: ['HealthManagement.Read'],
  },
  {
    id: 'ppe-management',
    label: 'PPE Management',
    icon: CONTEXT_ICONS.vaccination, // Represents protection
    action: () => navigate('/ppe'),
    color: 'info',
    variant: 'outline',
    permissions: ['PPEManagement.Read'],
  },
];

// Base widgets that are commonly used
const createBaseWidgets = (): DashboardWidget[] => [
  {
    id: 'total-incidents-stats',
    type: 'stats-card',
    title: 'Total Incidents',
    size: 'small',
    position: { row: 1, col: 1 },
    config: {
      dataSource: 'incident-dashboard',
      apiEndpoint: '/api/incident/dashboard',
      refreshOnLoad: true,
      autoRefresh: true,
      clickAction: {
        type: 'navigate',
        target: '/incidents',
      },
      styling: {
        color: 'primary',
      },
    },
  },
  {
    id: 'open-incidents-stats',
    type: 'stats-card',
    title: 'Open Incidents',
    size: 'small',
    position: { row: 1, col: 2 },
    config: {
      dataSource: 'incident-dashboard',
      apiEndpoint: '/api/incident/dashboard',
      refreshOnLoad: true,
      autoRefresh: true,
      clickAction: {
        type: 'navigate',
        target: '/incidents?status=Reported,UnderInvestigation,AwaitingAction',
      },
      styling: {
        color: 'warning',
      },
    },
  },
  {
    id: 'critical-incidents-stats',
    type: 'stats-card',
    title: 'Critical Incidents',
    size: 'small',
    position: { row: 1, col: 3 },
    config: {
      dataSource: 'incident-dashboard',
      apiEndpoint: '/api/incident/dashboard',
      refreshOnLoad: true,
      autoRefresh: true,
      clickAction: {
        type: 'navigate',
        target: '/incidents?severity=Critical',
      },
      styling: {
        color: 'danger',
      },
    },
  },
  {
    id: 'resolved-incidents-stats',
    type: 'stats-card',
    title: 'Resolved Incidents',
    size: 'small',
    position: { row: 1, col: 4 },
    config: {
      dataSource: 'incident-dashboard',
      apiEndpoint: '/api/incident/dashboard',
      refreshOnLoad: true,
      autoRefresh: true,
      clickAction: {
        type: 'navigate',
        target: '/incidents?status=Resolved,Closed',
      },
      styling: {
        color: 'success',
      },
    },
  },
  {
    id: 'quick-actions-widget',
    type: 'quick-actions',
    title: 'Quick Actions',
    size: 'medium',
    position: { row: 2, col: 1 },
    config: {
      refreshOnLoad: false,
      autoRefresh: false,
    },
  },
  {
    id: 'recent-incidents-list',
    type: 'recent-items',
    title: 'Recent Incidents',
    size: 'medium',
    position: { row: 2, col: 2 },
    config: {
      dataSource: 'incident-dashboard',
      apiEndpoint: '/api/incident/dashboard',
      refreshOnLoad: true,
      autoRefresh: true,
      customProps: {
        maxItems: 5,
        showTimestamp: true,
        itemClickAction: 'navigate',
      },
    },
  },
];

// Layout configurations for different user roles
export const dashboardLayouts: DashboardLayout[] = [
  {
    id: 'default',
    name: 'Default Dashboard',
    description: 'Standard dashboard for all users',
    widgets: [
      ...createBaseWidgets(),
      {
        id: 'incident-categories-chart',
        type: 'chart-donut',
        title: 'Incident Categories',
        size: 'medium',
        position: { row: 3, col: 1 },
        config: {
          dataSource: 'incident-dashboard',
          apiEndpoint: '/api/incident/dashboard',
          refreshOnLoad: true,
          autoRefresh: true,
        },
      },
      {
        id: 'department-performance-chart',
        type: 'chart-bar',
        title: 'Department Performance',
        size: 'medium',
        position: { row: 3, col: 2 },
        config: {
          dataSource: 'incident-dashboard',
          apiEndpoint: '/api/incident/dashboard',
          refreshOnLoad: true,
          autoRefresh: true,
        },
      },
    ],
    isDefault: true,
  },
  {
    id: 'executive',
    name: 'Executive Dashboard',
    description: 'High-level overview for executives and managers',
    userRole: 'Executive',
    widgets: [
      ...createBaseWidgets(),
      {
        id: 'incident-trends-chart',
        type: 'chart-line',
        title: 'Incident Trends',
        subtitle: 'Monthly incident report trends',
        size: 'large',
        position: { row: 3, col: 1 },
        config: {
          dataSource: 'incident-dashboard',
          apiEndpoint: '/api/incident/dashboard',
          refreshOnLoad: true,
          autoRefresh: true,
          customProps: {
            timeRange: '12months',
            showTrendLine: true,
          },
        },
      },
      {
        id: 'compliance-progress',
        type: 'progress-card',
        title: 'Compliance Score',
        size: 'small',
        position: { row: 3, col: 4 },
        config: {
          dataSource: 'compliance-metrics',
          refreshOnLoad: true,
          autoRefresh: true,
        },
      },
    ],
  },
  {
    id: 'safety-officer',
    name: 'Safety Officer Dashboard',
    description: 'Detailed safety metrics and incident management',
    userRole: 'SafetyOfficer',
    widgets: [
      ...createBaseWidgets(),
      {
        id: 'incident-severity-trends',
        type: 'chart-line',
        title: 'Severity Trends',
        size: 'medium',
        position: { row: 3, col: 1 },
        config: {
          dataSource: 'incident-dashboard',
          refreshOnLoad: true,
          autoRefresh: true,
        },
      },
      {
        id: 'overdue-incidents',
        type: 'alert-banner',
        title: 'Overdue Incidents',
        size: 'medium',
        position: { row: 3, col: 2 },
        config: {
          dataSource: 'overdue-incidents',
          refreshOnLoad: true,
          autoRefresh: true,
          styling: {
            color: 'danger',
          },
        },
      },
      {
        id: 'hazard-locations-map',
        type: 'custom',
        title: 'Hazard Locations',
        size: 'full',
        position: { row: 4, col: 1 },
        config: {
          dataSource: 'hazard-locations',
          refreshOnLoad: true,
          customProps: {
            component: 'HazardMap',
          },
        },
        permissions: ['HazardManagement.Read'],
      },
    ],
  },
  {
    id: 'basic-user',
    name: 'Basic User Dashboard',
    description: 'Simple dashboard for general users',
    userRole: 'User',
    widgets: [
      {
        id: 'my-incidents-stats',
        type: 'stats-card',
        title: 'My Incidents',
        size: 'small',
        position: { row: 1, col: 1 },
        config: {
          dataSource: 'my-incidents',
          refreshOnLoad: true,
          clickAction: {
            type: 'navigate',
            target: '/incidents/my-reports',
          },
          styling: {
            color: 'info',
          },
        },
      },
      {
        id: 'basic-quick-actions',
        type: 'quick-actions',
        title: 'Quick Actions',
        size: 'large',
        position: { row: 1, col: 2 },
        config: {
          customProps: {
            actions: ['report-incident', 'my-reports', 'view-incidents'],
          },
        },
      },
      {
        id: 'safety-tips',
        type: 'alert-banner',
        title: 'Safety Tip of the Day',
        size: 'full',
        position: { row: 2, col: 1 },
        config: {
          dataSource: 'safety-tips',
          styling: {
            color: 'info',
          },
        },
      },
    ],
  },
];

// Get layout by user role or ID
export const getDashboardLayout = (
  userRole?: string,
  layoutId?: string
): DashboardLayout => {
  if (layoutId) {
    const layout = dashboardLayouts.find(l => l.id === layoutId);
    if (layout) return layout;
  }

  if (userRole) {
    const roleLayout = dashboardLayouts.find(l => l.userRole === userRole);
    if (roleLayout) return roleLayout;
  }

  // Return default layout
  return dashboardLayouts.find(l => l.isDefault) || dashboardLayouts[0];
};

// Get available layouts for a user
export const getAvailableLayouts = (userRole?: string): DashboardLayout[] => {
  return dashboardLayouts.filter(layout => 
    !layout.userRole || layout.userRole === userRole || layout.isDefault
  );
};