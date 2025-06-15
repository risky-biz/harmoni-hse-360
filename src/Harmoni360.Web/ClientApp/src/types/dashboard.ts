export interface DashboardWidget {
  id: string;
  type: WidgetType;
  title: string;
  subtitle?: string;
  size: WidgetSize;
  position: WidgetPosition;
  config: WidgetConfig;
  permissions?: string[];
  isVisible?: boolean;
  refreshInterval?: number; // in seconds
}

export type WidgetType = 
  | 'stats-card'
  | 'chart-line'
  | 'chart-bar'
  | 'chart-donut'
  | 'recent-items'
  | 'quick-actions'
  | 'progress-card'
  | 'alert-banner'
  | 'custom';

export type WidgetSize = 
  | 'small'   // 1x1 (3 columns)
  | 'medium'  // 1x2 (6 columns)
  | 'large'   // 1x3 (9 columns)
  | 'full';   // 1x4 (12 columns)

export interface WidgetPosition {
  row: number;
  col: number;
  order?: number;
}

export interface WidgetConfig {
  dataSource?: string;
  apiEndpoint?: string;
  refreshOnLoad?: boolean;
  autoRefresh?: boolean;
  clickAction?: WidgetClickAction;
  customProps?: Record<string, any>;
  styling?: WidgetStyling;
}

export interface WidgetClickAction {
  type: 'navigate' | 'modal' | 'action' | 'none';
  target?: string;
  params?: Record<string, any>;
}

export interface WidgetStyling {
  color?: string;
  backgroundColor?: string;
  borderColor?: string;
  textColor?: string;
  customClasses?: string[];
}

export interface DashboardLayout {
  id: string;
  name: string;
  description?: string;
  userRole?: string;
  widgets: DashboardWidget[];
  isDefault?: boolean;
  isPersonalized?: boolean;
}

export interface QuickAction {
  id: string;
  label: string;
  icon: any;
  action: () => void;
  color?: string;
  variant?: 'solid' | 'outline';
  permissions?: string[];
  isVisible?: boolean;
  badge?: {
    text: string;
    color: string;
  };
}

export interface DashboardPreferences {
  selectedLayout: string;
  customWidgets: DashboardWidget[];
  hiddenWidgets: string[];
  refreshInterval: number;
  autoRefresh: boolean;
  compactMode: boolean;
  theme: 'light' | 'dark' | 'auto';
}

// Chart data interfaces
export interface ChartDataPoint {
  label: string;
  value: number;
  color?: string;
  metadata?: Record<string, any>;
}

export interface TimeSeriesDataPoint {
  date: string;
  value: number;
  category?: string;
}

export interface WidgetData {
  stats?: {
    value: number | string;
    subtitle?: string;
    trend?: {
      value: number;
      isPositive: boolean;
      label: string;
    };
  };
  chartData?: ChartDataPoint[];
  timeSeriesData?: TimeSeriesDataPoint[];
  listData?: {
    id: string;
    title: string;
    subtitle?: string;
    metadata?: Record<string, any>;
    clickAction?: () => void;
  }[];
  customData?: any;
  lastUpdated?: string;
  isLoading?: boolean;
  error?: string;
}