export interface HsseStatisticsDto {
  totalIncidents: number;
  totalHazards: number;
  totalSecurityIncidents: number;
  totalHealthIncidents: number;
  trir: number;
  ltifr: number;
  severityRate: number;
  complianceRate: number;
}

export interface HsseTrendPointDto {
  period: string;
  periodLabel: string;
  incidentCount: number;
  hazardCount: number;
  securityIncidentCount: number;
  healthIncidentCount: number;
}

export interface KpiInputs {
  hoursWorked: number;
  lostTimeInjuries: number;
  daysLost: number;
  compliantRecords: number;
  totalRecords: number;
}

export interface KpiTarget {
  trir: number;
  ltifr: number;
  severityRate: number;
  complianceRate: number;
}

export interface KpiMetric {
  value: number;
  target: number;
  title: string;
  description: string;
  benchmark?: string;
  isGoodDirectionLow: boolean;
}

// Comprehensive HSSE Dashboard Types
export interface HSSEDashboard {
  hazardStatistics: HazardStatistics;
  monthlyHazards: MonthlyHazard[];
  hazardClassifications: HazardClassification[];
  nonConformanceCriteria: NonConformanceCriteria[];
  topUnsafeConditions: UnsafeCondition[];
  responsibleActions: ResponsibleActionSummary;
  hazardCaseStatus: HazardCaseStatus;
  incidentFrequencyRates: IncidentFrequencyRate[];
  safetyPerformance: SafetyPerformance[];
  lostTimeInjury: LostTimeInjury;
}

export interface HazardStatistics {
  totalHazards: number;
  nearMiss: number;
  accidents: number;
  openCases: number;
  closedCases: number;
  completionRate: number;
}

export interface MonthlyHazard {
  year: number;
  month: number;
  monthName: string;
  hazardCount: number;
  nearnessCount: number;
  accidentCount: number;
  riskLevel: 'Low' | 'Medium' | 'High';
}

export interface HazardClassification {
  type: string;
  count: number;
  percentage: number;
  color: string;
}

export interface NonConformanceCriteria {
  category: string;
  count: number;
  description: string;
  location: string;
}

export interface UnsafeCondition {
  rank: number;
  description: string;
  count: number;
  percentage: number;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
}

export interface ResponsibleActionSummary {
  totalActions: number;
  openActions: number;
  closedActions: number;
  overdueActions: number;
  completionRate: number;
  topActions: ResponsibleActionItem[];
}

export interface ResponsibleActionItem {
  id: number;
  description: string;
  status: string;
  dueDate: string;
  assignedTo: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
}

export interface HazardCaseStatus {
  totalCases: number;
  openCases: number;
  closedCases: number;
  openPercentage: number;
  closedPercentage: number;
  startDate: string;
  endDate: string;
}

export interface IncidentFrequencyRate {
  year: number;
  totalRecordableIncidentFrequencyRate: number; // TRIFR
  totalRecordableSeverityRate: number; // TRSR
  studyRelatedIFR: number;
  workRelatedIFR: number;
  studyRelatedSR: number;
  workRelatedSR: number;
}

export interface SafetyPerformance {
  year: number;
  nearMissCount: number;
  hazardCount: number;
  accidentCount: number;
  ifrStudyRelated: number;
  ifrWorkRelated: number;
  totalIFR: number;
  performanceLevel: 'Excellent' | 'Good' | 'Average' | 'Poor';
  colorCode: 'Green' | 'LightGreen' | 'Yellow' | 'Red';
  department?: string;
  notes?: string;
}

export interface LostTimeInjury {
  year: number;
  ltiStudyRelatedRate: number;
  ltiWorkRelatedRate: number;
  totalLTICaseRate: number;
  totalLTICases: number;
  studyRelatedCases: number;
  workRelatedCases: number;
  department?: string;
}

// API Request/Response types
export interface GetHSSEDashboardParams {
  startDate?: string;
  endDate?: string;
  department?: string;
  location?: string;
  includeTrends?: boolean;
  includeComparisons?: boolean;
}

// Chart data types
export interface ChartDataPoint {
  label: string;
  value: number;
  color?: string;
}

export interface TrendDataPoint {
  period: string;
  value: number;
  category?: string;
}

export interface PerformanceIndicator {
  name: string;
  value: number;
  target?: number;
  unit?: string;
  trend?: 'up' | 'down' | 'stable';
  status: 'excellent' | 'good' | 'average' | 'poor';
}
