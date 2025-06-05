import { CBadge } from '@coreui/react';

// PPE Status Badge Helper
export const getPPEStatusBadge = (status: string) => {
  const statusConfig = {
    Available: { color: 'success', text: 'Available' },
    Assigned: { color: 'primary', text: 'Assigned' },
    InMaintenance: { color: 'warning', text: 'In Maintenance' },
    InInspection: { color: 'info', text: 'In Inspection' },
    OutOfService: { color: 'danger', text: 'Out of Service' },
    RequiresReturn: { color: 'warning', text: 'Requires Return' },
    Lost: { color: 'dark', text: 'Lost' },
    Retired: { color: 'secondary', text: 'Retired' },
  };

  const config = statusConfig[status as keyof typeof statusConfig] || {
    color: 'secondary',
    text: status,
  };

  return (
    <CBadge color={config.color} shape="rounded-pill">
      {config.text}
    </CBadge>
  );
};

// PPE Condition Badge Helper
export const getPPEConditionBadge = (condition: string) => {
  const conditionConfig = {
    New: { color: 'success', text: 'New' },
    Excellent: { color: 'success', text: 'Excellent' },
    Good: { color: 'primary', text: 'Good' },
    Fair: { color: 'warning', text: 'Fair' },
    Poor: { color: 'warning', text: 'Poor' },
    Damaged: { color: 'danger', text: 'Damaged' },
    Expired: { color: 'danger', text: 'Expired' },
    Retired: { color: 'secondary', text: 'Retired' },
  };

  const config = conditionConfig[condition as keyof typeof conditionConfig] || {
    color: 'secondary',
    text: condition,
  };

  return (
    <CBadge color={config.color} shape="rounded-pill">
      {config.text}
    </CBadge>
  );
};

// PPE Priority Badge Helper
export const getPPEPriorityBadge = (priority: string) => {
  const priorityConfig = {
    Low: { color: 'success', text: 'Low' },
    Medium: { color: 'warning', text: 'Medium' },
    High: { color: 'danger', text: 'High' },
    Urgent: { color: 'danger', text: 'Urgent' },
  };

  const config = priorityConfig[priority as keyof typeof priorityConfig] || {
    color: 'secondary',
    text: priority,
  };

  return (
    <CBadge color={config.color} shape="rounded-pill">
      {config.text}
    </CBadge>
  );
};

// PPE Request Status Badge Helper
export const getPPERequestStatusBadge = (status: string) => {
  const statusConfig = {
    Draft: { color: 'secondary', text: 'Draft' },
    Submitted: { color: 'info', text: 'Submitted' },
    UnderReview: { color: 'warning', text: 'Under Review' },
    Approved: { color: 'success', text: 'Approved' },
    Rejected: { color: 'danger', text: 'Rejected' },
    Fulfilled: { color: 'success', text: 'Fulfilled' },
    Cancelled: { color: 'secondary', text: 'Cancelled' },
  };

  const config = statusConfig[status as keyof typeof statusConfig] || {
    color: 'secondary',
    text: status,
  };

  return (
    <CBadge color={config.color} shape="rounded-pill">
      {config.text}
    </CBadge>
  );
};

// PPE Type Mapping
export const PPE_TYPES = {
  HeadProtection: 'Head Protection',
  EyeProtection: 'Eye Protection',
  HearingProtection: 'Hearing Protection',
  RespiratoryProtection: 'Respiratory Protection',
  HandProtection: 'Hand Protection',
  FootProtection: 'Foot Protection',
  BodyProtection: 'Body Protection',
  FallProtection: 'Fall Protection',
  HighVisibility: 'High Visibility',
  EmergencyEquipment: 'Emergency Equipment',
};

// Date Formatting Helper
export const formatDate = (dateString: string | undefined | null): string => {
  if (!dateString) return 'N/A';
  
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return 'Invalid Date';
  }
};

// Date and Time Formatting Helper
export const formatDateTime = (dateString: string | undefined | null): string => {
  if (!dateString) return 'N/A';
  
  try {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return 'Invalid Date';
  }
};

// Duration Helper
export const formatDuration = (startDate: string, endDate?: string): string => {
  try {
    const start = new Date(startDate);
    const end = endDate ? new Date(endDate) : new Date();
    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays === 1) return '1 day';
    if (diffDays < 30) return `${diffDays} days`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)} months`;
    return `${Math.floor(diffDays / 365)} years`;
  } catch {
    return 'Unknown';
  }
};

// Currency Formatting Helper
export const formatCurrency = (amount: number, currency: string = 'IDR'): string => {
  try {
    return new Intl.NumberFormat('id-ID', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  } catch {
    return `${currency} ${amount.toLocaleString()}`;
  }
};

// File Size Helper
export const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

// Days until/overdue calculation
export const getDaysUntil = (dateString: string | undefined | null): number | null => {
  if (!dateString) return null;
  
  try {
    const targetDate = new Date(dateString);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    targetDate.setHours(0, 0, 0, 0);
    
    const diffTime = targetDate.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  } catch {
    return null;
  }
};

// PPE Item Code Generator Helper
export const generatePPEItemCode = (categoryPrefix: string, sequence: number): string => {
  return `${categoryPrefix}-${sequence.toString().padStart(4, '0')}`;
};

// Validation Helpers
export const isValidItemCode = (itemCode: string): boolean => {
  // PPE item codes should follow pattern: PREFIX-NNNN
  const pattern = /^[A-Z]{2,4}-\d{4}$/;
  return pattern.test(itemCode);
};

export const isValidCost = (cost: number): boolean => {
  return cost >= 0 && cost <= 999999999; // Reasonable cost limits
};

export const isValidSize = (size: string): boolean => {
  // Common PPE sizes
  const validSizes = [
    'XS', 'S', 'M', 'L', 'XL', 'XXL', 'XXXL',
    'One Size', 'Universal',
    '6', '6.5', '7', '7.5', '8', '8.5', '9', '9.5', '10', '10.5', '11', '11.5', '12', // Shoe sizes
    '52', '54', '56', '58', '60', '62', '64', // Head circumference
  ];
  return validSizes.includes(size) || /^\d+(\.\d+)?$/.test(size); // Also allow numeric sizes
};

// Search/Filter Helpers
export const filterPPEItems = (items: any[], searchTerm: string): any[] => {
  if (!searchTerm.trim()) return items;
  
  const term = searchTerm.toLowerCase();
  return items.filter(item =>
    item.itemCode.toLowerCase().includes(term) ||
    item.name.toLowerCase().includes(term) ||
    item.description?.toLowerCase().includes(term) ||
    item.manufacturer.toLowerCase().includes(term) ||
    item.model.toLowerCase().includes(term) ||
    item.categoryName.toLowerCase().includes(term) ||
    item.location.toLowerCase().includes(term) ||
    item.assignedToName?.toLowerCase().includes(term)
  );
};

// Sort Helpers
export const sortPPEItems = (items: any[], sortBy: string, direction: 'asc' | 'desc' = 'asc'): any[] => {
  return [...items].sort((a, b) => {
    let aVal = a[sortBy];
    let bVal = b[sortBy];
    
    // Handle date strings
    if (sortBy.includes('Date') || sortBy.includes('date')) {
      aVal = aVal ? new Date(aVal).getTime() : 0;
      bVal = bVal ? new Date(bVal).getTime() : 0;
    }
    
    // Handle strings
    if (typeof aVal === 'string' && typeof bVal === 'string') {
      aVal = aVal.toLowerCase();
      bVal = bVal.toLowerCase();
    }
    
    // Handle nulls/undefined
    if (aVal == null && bVal == null) return 0;
    if (aVal == null) return direction === 'asc' ? 1 : -1;
    if (bVal == null) return direction === 'asc' ? -1 : 1;
    
    const result = aVal < bVal ? -1 : aVal > bVal ? 1 : 0;
    return direction === 'desc' ? -result : result;
  });
};