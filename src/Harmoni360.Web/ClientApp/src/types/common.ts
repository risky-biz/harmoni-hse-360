// Common types used across the application

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
  page: number;
  pageSize: number;
}

export interface PagedList<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
  page: number;
  pageSize: number;
}

export interface QueryParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface SelectOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

export interface DateRange {
  startDate: Date | null;
  endDate: Date | null;
}

export interface FileInfo {
  id: string;
  name: string;
  size: number;
  type: string;
  url?: string;
  uploadedAt: Date;
  uploadedBy: string;
}

export interface AuditInfo {
  createdAt: Date;
  createdBy: string;
  lastModifiedAt?: Date;
  lastModifiedBy?: string;
}

export interface ValidationError {
  field: string;
  message: string;
}

export interface ApiError {
  message: string;
  details?: string;
  statusCode?: number;
  validationErrors?: ValidationError[];
}

// Generic filter interface that can be extended
export interface BaseFilter extends QueryParams {
  status?: string;
  category?: string;
  type?: string;
  priority?: string;
  assignedTo?: string;
  department?: string;
  location?: string;
  dateFrom?: string;
  dateTo?: string;
}

// Tab interface for detail pages
export interface TabDefinition {
  key: string;
  label: string;
  icon?: string;
  disabled?: boolean;
  badge?: number | string;
}

// Chart data interfaces
export interface ChartDataPoint {
  label: string;
  value: number;
  color?: string;
}

export interface DonutChartData {
  label: string;
  value: number;
  color: string;
}

export interface LineChartDataPoint {
  x: string | number;
  y: number;
}

export interface BarChartData {
  label: string;
  value: number;
  color?: string;
}

// Dashboard widget interfaces
export interface StatisticCard {
  title: string;
  value: number;
  change?: number;
  changeType?: 'increase' | 'decrease' | 'neutral';
  color?: 'primary' | 'success' | 'warning' | 'danger' | 'info';
  icon?: string;
  format?: 'number' | 'percentage' | 'currency';
}

export interface RecentActivity {
  id: string;
  title: string;
  description: string;
  timestamp: Date;
  type: string;
  status?: string;
  priority?: string;
  url?: string;
}

// Form validation types
export interface FieldError {
  type: string;
  message: string;
}

export interface FormErrors {
  [fieldName: string]: FieldError | undefined;
}

// Permission-related types for components
export interface PermissionCheck {
  module?: string;
  permission?: string;
  role?: string;
  customCheck?: () => boolean;
}

// Notification types
export interface NotificationMessage {
  id: string;
  title: string;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  timestamp: Date;
  read: boolean;
  actionUrl?: string;
  actionText?: string;
}

// Export/Import types
export interface ExportOptions {
  format: 'excel' | 'pdf' | 'csv';
  dateRange?: DateRange;
  includeAttachments?: boolean;
  includeComments?: boolean;
  filters?: Record<string, any>;
}

export interface ImportResult {
  totalRows: number;
  successCount: number;
  errorCount: number;
  errors: Array<{
    row: number;
    message: string;
  }>;
}

// Generic status type
export type Status = 'active' | 'inactive' | 'pending' | 'approved' | 'rejected' | 'draft' | 'completed' | 'cancelled';

// Generic priority type
export type Priority = 'low' | 'medium' | 'high' | 'critical';

// Generic severity type
export type Severity = 'minor' | 'moderate' | 'major' | 'critical';