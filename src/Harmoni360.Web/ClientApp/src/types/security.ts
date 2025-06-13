// Security Incident Types
export interface SecurityIncident {
  id: number;
  incidentNumber: string;
  title: string;
  description: string;
  incidentType: SecurityIncidentType;
  category: SecurityIncidentCategory;
  severity: SecuritySeverity;
  status: SecurityIncidentStatus;
  threatLevel: ThreatLevel;
  incidentDateTime: string;
  detectionDateTime?: string;
  containmentDateTime?: string;
  resolutionDateTime?: string;
  location: string;
  latitude?: number;
  longitude?: number;
  threatActorType?: ThreatActorType;
  threatActorDescription?: string;
  isInternalThreat: boolean;
  impact: SecurityImpact;
  affectedPersonsCount?: number;
  estimatedLoss?: number;
  dataBreachOccurred: boolean;
  containmentActions?: string;
  rootCause?: string;
  reporterName?: string;
  reporterEmail?: string;
  assignedToName?: string;
  investigatorName?: string;
  createdAt: string;
  lastModifiedAt?: string;
  createdBy?: string;
  lastModifiedBy?: string;
  attachmentCount: number;
  responseCount: number;
  involvedPersonCount: number;
  hasOpenTasks: boolean;
  daysOpen: number;
  isOverdue: boolean;
}

export interface SecurityIncidentList {
  id: number;
  incidentNumber: string;
  title: string;
  incidentType: SecurityIncidentType;
  category: SecurityIncidentCategory;
  severity: SecuritySeverity;
  status: SecurityIncidentStatus;
  threatLevel: ThreatLevel;
  incidentDateTime: string;
  location: string;
  reporterName?: string;
  assignedToName?: string;
  createdAt: string;
  daysOpen: number;
  isOverdue: boolean;
}

export interface SecurityIncidentDetail extends SecurityIncident {
  attachments: SecurityIncidentAttachment[];
  responses: SecurityIncidentResponse[];
  involvedPersons: SecurityIncidentInvolvedPerson[];
  implementedControls: SecurityControl[];
  threatIndicators: ThreatIndicator[];
  currentThreatAssessment?: ThreatAssessment;
}

export interface SecurityIncidentAttachment {
  id: number;
  fileName: string;
  filePath: string;
  fileSize: number;
  fileType: string;
  attachmentType: SecurityAttachmentType;
  description?: string;
  isConfidential: boolean;
  hash?: string;
  uploadedAt: string;
  uploadedBy: string;
  fileSizeFormatted: string;
}

export interface SecurityIncidentResponse {
  id: number;
  responseType: SecurityResponseType;
  actionTaken: string;
  actionDateTime: string;
  wasSuccessful: boolean;
  followUpRequired: boolean;
  followUpDetails?: string;
  followUpDueDate?: string;
  responderName: string;
  cost?: number;
  effortHours?: number;
  toolsUsed?: string;
  resourcesUsed?: string;
}

export interface SecurityIncidentInvolvedPerson {
  id: number;
  personId: number;
  personName: string;
  personEmail: string;
  role: string;
  description?: string;
  isWitness: boolean;
  isVictim: boolean;
  isSuspect: boolean;
  contactInfo?: string;
  interviewDate?: string;
  notes?: string;
}

export interface SecurityControl {
  id: number;
  controlName: string;
  controlDescription: string;
  controlType: SecurityControlType;
  category: SecurityControlCategory;
  status: ControlImplementationStatus;
  implementationDate: string;
  reviewDate?: string;
  implementedByName: string;
  isOverdue: boolean;
  daysUntilReview: number;
}

export interface ThreatIndicator {
  id: number;
  indicatorType: string;
  indicatorValue: string;
  threatType: string;
  confidence: number;
  source: string;
  firstSeen: string;
  lastSeen: string;
  isActive: boolean;
  description?: string;
  tags?: string[];
  confidenceLevel: string;
}

export interface ThreatAssessment {
  id: number;
  currentThreatLevel: ThreatLevel;
  previousThreatLevel: ThreatLevel;
  assessmentRationale: string;
  assessmentDateTime: string;
  assessedByName: string;
  externalThreatIntelUsed: boolean;
  threatIntelSource?: string;
  threatIntelDetails?: string;
  threatCapability: number;
  threatIntent: number;
  targetVulnerability: number;
  impactPotential: number;
  riskScore: number;
  riskLevel: string;
}

// Dashboard Types
export interface SecurityDashboard {
  metrics: SecurityMetrics;
  trends: SecurityTrends;
  recentIncidents: SecurityIncidentList[];
  criticalIncidents: SecurityIncidentList[];
  overdueIncidents: SecurityIncidentList[];
  recentThreatIndicators: ThreatIndicator[];
  complianceStatus: SecurityComplianceStatus;
  generatedAt: string;
}

export interface SecurityMetrics {
  totalIncidents: number;
  openIncidents: number;
  closedIncidents: number;
  criticalIncidents: number;
  highSeverityIncidents: number;
  dataBreachIncidents: number;
  internalThreatIncidents: number;
  physicalSecurityIncidents: number;
  cybersecurityIncidents: number;
  personnelSecurityIncidents: number;
  informationSecurityIncidents: number;
  overdueIncidents: number;
  averageResolutionTimeHours: number;
  incidentTrend: number;
  mostCommonSeverity: SecuritySeverity;
  mostCommonType: SecurityIncidentType;
  mostCommonLocation: string;
}

export interface SecurityTrends {
  incidentTrends: SecurityTrendDataPoint[];
  severityTrends: SecurityTrendDataPoint[];
  typeTrends: SecurityTrendDataPoint[];
  threatLevelTrends: SecurityTrendDataPoint[];
  locationTrends: SecurityLocationTrend[];
  threatActorTrends: SecurityThreatActorTrend[];
}

export interface SecurityTrendDataPoint {
  date: string;
  label: string;
  count: number;
  percentage: number;
}

export interface SecurityLocationTrend {
  location: string;
  incidentCount: number;
  highestSeverity: SecuritySeverity;
  mostCommonType: SecurityIncidentType;
  latitude: number;
  longitude: number;
}

export interface SecurityThreatActorTrend {
  actorType: ThreatActorType;
  incidentCount: number;
  averageSeverity: SecuritySeverity;
  targetedTypes: SecurityIncidentType[];
}

export interface SecurityComplianceStatus {
  iso27001Compliant: boolean;
  iteLawCompliant: boolean;
  smk3Compliant: boolean;
  complianceScore: number;
  issues: SecurityComplianceIssue[];
  lastAssessmentDate: string;
  nextAssessmentDue: string;
}

export interface SecurityComplianceIssue {
  title: string;
  description: string;
  severity: SecuritySeverity;
  dueDate: string;
  responsiblePerson: string;
  isOverdue: boolean;
}

// Request Types
export interface CreateSecurityIncidentRequest {
  incidentType: SecurityIncidentType;
  category: SecurityIncidentCategory;
  title: string;
  description: string;
  severity: SecuritySeverity;
  incidentDateTime: string;
  location: string;
  latitude?: number;
  longitude?: number;
  threatActorType?: ThreatActorType;
  threatActorDescription?: string;
  isInternalThreat: boolean;
  dataBreachSuspected: boolean;
  affectedPersonsCount?: number;
  estimatedLoss?: number;
  impact: SecurityImpact;
  assignedToId?: number;
  investigatorId?: number;
  involvedPersonIds?: number[];
  attachments?: File[];
  containmentActions?: string;
  detectionDateTime?: string;
}

export interface UpdateSecurityIncidentRequest {
  title: string;
  description: string;
  severity: SecuritySeverity;
  status: SecurityIncidentStatus;
  incidentDateTime: string;
  location: string;
  latitude?: number;
  longitude?: number;
  threatActorType?: ThreatActorType;
  threatActorDescription?: string;
  isInternalThreat: boolean;
  dataBreachOccurred: boolean;
  affectedPersonsCount?: number;
  estimatedLoss?: number;
  impact: SecurityImpact;
  containmentActions?: string;
  rootCause?: string;
  detectionDateTime?: string;
  containmentDateTime?: string;
  resolutionDateTime?: string;
  assignedToId?: number;
  investigatorId?: number;
}

export interface CreateThreatAssessmentRequest {
  threatLevel: ThreatLevel;
  assessmentRationale: string;
  threatCapability: number;
  threatIntent: number;
  targetVulnerability: number;
  impactPotential: number;
  externalThreatIntelUsed: boolean;
  threatIntelSource?: string;
  threatIntelDetails?: string;
  assessmentDateTime?: string;
}

// Enums
export enum SecurityIncidentType {
  PhysicalSecurity = 1,
  Cybersecurity = 2,
  PersonnelSecurity = 3,
  InformationSecurity = 4,
}

export enum SecurityIncidentCategory {
  // Physical Security
  UnauthorizedAccess = 101,
  Theft = 102,
  Vandalism = 103,
  PerimeterBreach = 104,
  SuspiciousActivity = 105,
  PhysicalThreat = 106,
  
  // Cybersecurity
  DataBreach = 201,
  MalwareInfection = 202,
  PhishingAttempt = 203,
  SystemIntrusion = 204,
  ServiceDisruption = 205,
  UnauthorizedChange = 206,
  
  // Personnel Security
  BackgroundCheckFailure = 301,
  PolicyViolation = 302,
  InsiderThreat = 303,
  CredentialMisuse = 304,
  SecurityTrainingFailure = 305,
}

export enum SecuritySeverity {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4,
}

export enum SecurityIncidentStatus {
  Open = 1,
  Assigned = 2,
  Investigating = 3,
  Contained = 4,
  Eradicating = 5,
  Recovering = 6,
  Resolved = 7,
  Closed = 8,
}

export enum ThreatLevel {
  Minimal = 1,
  Low = 2,
  Medium = 3,
  High = 4,
  Severe = 5,
}

export enum ThreatActorType {
  External = 1,
  Internal = 2,
  Partner = 3,
  Unknown = 4,
}

export enum SecurityImpact {
  None = 0,
  Minor = 1,
  Moderate = 2,
  Major = 3,
  Severe = 4,
}

export enum SecurityAttachmentType {
  Evidence = 1,
  Screenshot = 2,
  Log = 3,
  Report = 4,
  Other = 5,
}

export enum SecurityResponseType {
  Initial = 1,
  Containment = 2,
  Eradication = 3,
  Recovery = 4,
  PostIncident = 5,
  Escalation = 6,
  Investigation = 7,
}

export enum SecurityControlType {
  Preventive = 1,
  Detective = 2,
  Corrective = 3,
}

export enum SecurityControlCategory {
  Technical = 1,
  Administrative = 2,
  Physical = 3,
}

export enum ControlImplementationStatus {
  Planned = 1,
  Implementing = 2,
  Active = 3,
  UnderReview = 4,
  Retired = 5,
}

// Utility Types
export interface PagedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}