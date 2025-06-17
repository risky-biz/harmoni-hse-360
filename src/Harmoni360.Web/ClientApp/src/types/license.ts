// License types and interfaces for the License Management System

export interface LicenseDto {
  id: number;
  licenseNumber: string;
  title: string;
  description: string;
  type: string;
  typeDisplay: string;
  status: string;
  statusDisplay: string;
  priority: string;
  priorityDisplay: string;

  // License Details
  issuingAuthority: string;
  holderName: string;
  department: string;
  
  // Dates
  issuedDate: string;
  expiryDate: string;
  submittedDate?: string;
  approvedDate?: string;
  activatedDate?: string;
  suspendedDate?: string;
  revokedDate?: string;

  // Risk and Compliance
  riskLevel: string;
  riskLevelDisplay: string;
  isCriticalLicense: boolean;
  requiresInsurance: boolean;
  requiredInsuranceAmount?: number;
  licenseFee?: number;
  currency: string;

  // Additional Properties
  scope: string;
  restrictions: string;
  conditionsText: string;
  statusNotes: string;

  // Regulatory Information
  regulatoryFramework: string;
  applicableRegulations: string;
  complianceStandards: string;

  // Renewal Information
  renewalRequired: boolean;
  renewalPeriodDays: number;
  nextRenewalDate?: string;
  autoRenewal: boolean;
  renewalProcedure: string;

  // Collections
  attachments: LicenseAttachmentDto[];
  renewals: LicenseRenewalDto[];
  conditions: LicenseConditionDto[];
  
  // Audit Information
  createdAt: string;
  createdBy: string;
  updatedAt?: string;
  updatedBy?: string;

  // Computed Properties
  canEdit?: boolean;
  canSubmit?: boolean;
  canApprove?: boolean;
  canActivate?: boolean;
  canSuspend?: boolean;
  canRenew?: boolean;
  isExpired?: boolean;
  isExpiring?: boolean;
  isActive?: boolean;
  isHighRisk?: boolean;
  hasRequiredInfo?: boolean;
  isRenewalDue?: boolean;
  daysUntilExpiry?: number;
  daysUntilRenewal?: number;
}

export interface LicenseAttachmentDto {
  id: number;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  uploadedBy: string;
  uploadedAt: string;
  attachmentType: string;
  attachmentTypeDisplay: string;
  description: string;
  isRequired: boolean;
  validUntil?: string;
  isExpired: boolean;
}

export interface LicenseRenewalDto {
  id: number;
  renewalNumber: string;
  applicationDate: string;
  submittedDate?: string;
  approvedDate?: string;
  rejectedDate?: string;
  newExpiryDate: string;
  status: string;
  statusDisplay: string;
  renewalNotes: string;
  renewalFee?: number;
  documentsRequired: boolean;
  inspectionRequired: boolean;
  inspectionDate?: string;
  processedBy: string;
  createdAt: string;
  createdBy: string;
}

export interface LicenseConditionDto {
  id: number;
  conditionType: string;
  description: string;
  isMandatory: boolean;
  dueDate?: string;
  status: string;
  statusDisplay: string;
  complianceEvidence: string;
  complianceDate?: string;
  verifiedBy: string;
  responsiblePerson: string;
  notes: string;
  isOverdue: boolean;
  isCompleted: boolean;
  daysUntilDue: number;
}

export interface LicenseAuditLogDto {
  id: number;
  action: string;
  actionDescription: string;
  performedAt: string;
  performedBy: string;
  ipAddress?: string;
  userAgent?: string;
  oldValues?: string;
  newValues?: string;
  comments?: string;
}

export interface LicenseDashboardDto {
  totalLicenses: number;
  draftLicenses: number;
  pendingSubmissionLicenses: number;
  submittedLicenses: number;
  underReviewLicenses: number;
  approvedLicenses: number;
  activeLicenses: number;
  rejectedLicenses: number;
  expiredLicenses: number;
  suspendedLicenses: number;
  revokedLicenses: number;
  pendingRenewalLicenses: number;

  expiringThisMonth: number;
  expiringThisQuarter: number;
  renewalsDue: number;
  highRiskLicenses: number;
  criticalLicenses: number;
  overdueConditions: number;

  averageLicenseFee: number;
  totalLicenseFees: number;
  totalRenewalFees: number;

  licensesByType: LicenseTypeStatDto[];
  monthlyTrends: LicenseMonthlyTrendDto[];
  recentLicenses: LicenseDto[];
  expiringLicenses: LicenseDto[];
  highPriorityLicenses: LicenseDto[];
}

export interface LicenseTypeStatDto {
  type: string;
  typeDisplay: string;
  count: number;
  percentage: number;
  active: number;
  expired: number;
  expiringThisMonth: number;
}

export interface LicenseMonthlyTrendDto {
  month: string;
  totalLicenses: number;
  newLicenses: number;
  renewedLicenses: number;
  expiredLicenses: number;
  averageFee: number;
}

// Form data interfaces
export interface LicenseFormData {
  title: string;
  description: string;
  type: string;
  priority: string;
  licenseNumber: string;
  issuingAuthority: string;
  holderName: string;
  department: string;
  issuedDate: string;
  expiryDate: string;
  scope: string;
  restrictions: string;
  conditions: string;
  riskLevel: string;
  licenseFee?: number;
  currency: string;
  isCriticalLicense: boolean;
  requiresInsurance: boolean;
  requiredInsuranceAmount?: number;
  regulatoryFramework: string;
  applicableRegulations: string;
  complianceStandards: string;
  renewalRequired: boolean;
  renewalPeriodDays: number;
  autoRenewal: boolean;
  renewalProcedure: string;
  licenseConditions: LicenseConditionFormData[];
}

export interface LicenseConditionFormData {
  conditionType: string;
  description: string;
  isMandatory: boolean;
  dueDate?: string;
  responsiblePerson: string;
  notes: string;
}

// Constants
export const LICENSE_TYPES = [
  { value: 'Environmental', label: 'Environmental' },
  { value: 'Safety', label: 'Safety' },
  { value: 'Health', label: 'Health' },
  { value: 'Construction', label: 'Construction' },
  { value: 'Operating', label: 'Operating' },
  { value: 'Transport', label: 'Transport' },
  { value: 'Waste', label: 'Waste' },
  { value: 'Chemical', label: 'Chemical' },
  { value: 'Radiation', label: 'Radiation' },
  { value: 'Fire', label: 'Fire' },
  { value: 'Electrical', label: 'Electrical' },
  { value: 'Mechanical', label: 'Mechanical' },
  { value: 'Professional', label: 'Professional' },
  { value: 'Business', label: 'Business' },
  { value: 'Import', label: 'Import' },
  { value: 'Export', label: 'Export' },
  { value: 'Other', label: 'Other' }
];

export const LICENSE_PRIORITIES = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' }
];

export const LICENSE_STATUSES = [
  { value: 'Draft', label: 'Draft' },
  { value: 'PendingSubmission', label: 'Pending Submission' },
  { value: 'Submitted', label: 'Submitted' },
  { value: 'UnderReview', label: 'Under Review' },
  { value: 'Approved', label: 'Approved' },
  { value: 'Active', label: 'Active' },
  { value: 'Rejected', label: 'Rejected' },
  { value: 'Expired', label: 'Expired' },
  { value: 'Suspended', label: 'Suspended' },
  { value: 'Revoked', label: 'Revoked' },
  { value: 'PendingRenewal', label: 'Pending Renewal' }
];

export const RISK_LEVELS = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' }
];

export const ATTACHMENT_TYPES = [
  { value: 'Application', label: 'Application Document' },
  { value: 'SupportingDocument', label: 'Supporting Document' },
  { value: 'Certificate', label: 'Certificate' },
  { value: 'Compliance', label: 'Compliance Document' },
  { value: 'Insurance', label: 'Insurance Document' },
  { value: 'TechnicalSpec', label: 'Technical Specification' },
  { value: 'LegalDocument', label: 'Legal Document' },
  { value: 'RenewalDocument', label: 'Renewal Document' },
  { value: 'InspectionReport', label: 'Inspection Report' },
  { value: 'Other', label: 'Other' }
];

export const CONDITION_TYPES = [
  { value: 'Inspection', label: 'Inspection Required' },
  { value: 'Training', label: 'Training Required' },
  { value: 'Certification', label: 'Certification Required' },
  { value: 'Documentation', label: 'Documentation Required' },
  { value: 'Insurance', label: 'Insurance Required' },
  { value: 'Reporting', label: 'Reporting Required' },
  { value: 'Audit', label: 'Audit Required' },
  { value: 'Compliance', label: 'Compliance Check' },
  { value: 'Renewal', label: 'Renewal Action' },
  { value: 'Other', label: 'Other' }
];

export const CURRENCIES = [
  { value: 'USD', label: 'USD - US Dollar' },
  { value: 'IDR', label: 'IDR - Indonesian Rupiah' },
  { value: 'EUR', label: 'EUR - Euro' },
  { value: 'GBP', label: 'GBP - British Pound' },
  { value: 'SGD', label: 'SGD - Singapore Dollar' },
  { value: 'MYR', label: 'MYR - Malaysian Ringgit' }
];

// Status color mappings
export const getStatusColor = (status: string): string => {
  switch (status) {
    case 'Draft':
      return 'secondary';
    case 'PendingSubmission':
      return 'warning';
    case 'Submitted':
      return 'info';
    case 'UnderReview':
      return 'info';
    case 'Approved':
      return 'success';
    case 'Active':
      return 'success';
    case 'Rejected':
      return 'danger';
    case 'Expired':
      return 'danger';
    case 'Suspended':
      return 'warning';
    case 'Revoked':
      return 'danger';
    case 'PendingRenewal':
      return 'warning';
    default:
      return 'secondary';
  }
};

// Priority color mappings
export const getPriorityColor = (priority: string): string => {
  switch (priority) {
    case 'Low':
      return 'success';
    case 'Medium':
      return 'info';
    case 'High':
      return 'warning';
    case 'Critical':
      return 'danger';
    default:
      return 'secondary';
  }
};

// Risk level color mappings
export const getRiskLevelColor = (riskLevel: string): string => {
  switch (riskLevel) {
    case 'Low':
      return 'success';
    case 'Medium':
      return 'info';
    case 'High':
      return 'warning';
    case 'Critical':
      return 'danger';
    default:
      return 'secondary';
  }
};