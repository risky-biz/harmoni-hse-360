// Training Management Types
export type TrainingType = 
  | 'SafetyOrientation'
  | 'EmergencyResponse'
  | 'K3Training'
  | 'FireSafety'
  | 'ConfinedSpaceEntry'
  | 'ChemicalHandling'
  | 'EnvironmentalAwareness'
  | 'PPEUsage'
  | 'IncidentReporting'
  | 'Compliance'
  | 'LeadershipDevelopment'
  | 'TechnicalTraining'
  | 'EquipmentOperation'
  | 'MaintenanceProcedures'
  | 'ElectricalSafety'
  | 'WorkAtHeight'
  | 'LiftingOperations'
  | 'DrivingSafety'
  | 'FirstAid'
  | 'Other';

export type TrainingStatus = 
  | 'Draft' 
  | 'Scheduled' 
  | 'InProgress' 
  | 'Completed' 
  | 'Cancelled' 
  | 'Postponed';

export type TrainingCategory = 
  | 'MandatoryCompliance'
  | 'SafetyTraining'
  | 'SpecializedTraining'
  | 'ProfessionalDevelopment'
  | 'LeadershipTraining'
  | 'TechnicalSkills'
  | 'SoftSkills'
  | 'Induction'
  | 'Refresher'
  | 'Certification';

export type TrainingPriority = 
  | 'Low' 
  | 'Medium' 
  | 'High' 
  | 'Critical';

export type TrainingDeliveryMethod = 
  | 'InPerson'
  | 'Online'
  | 'Hybrid'
  | 'Workshop'
  | 'OnTheJob'
  | 'SelfPaced'
  | 'Simulation'
  | 'Practical';

export type ParticipantStatus = 
  | 'Enrolled'
  | 'Attended'
  | 'Absent'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'
  | 'InProgress'
  | 'Certified';

export type CertificationType = 
  | 'K3Certificate'
  | 'Professional'
  | 'Competency'
  | 'Compliance'
  | 'ISO_Certificate'
  | 'Internal'
  | 'External'
  | 'Government'
  | 'Industry'
  | 'Safety';

export type CertificationStatus = 
  | 'Valid'
  | 'Expired'
  | 'Expiring'
  | 'Revoked'
  | 'Pending'
  | 'Suspended';

export type TrainingAttachmentType = 
  | 'CourseSlides'
  | 'Handbook'
  | 'Video'
  | 'Assessment'
  | 'Certificate'
  | 'Reference'
  | 'Checklist'
  | 'Procedure'
  | 'ComplianceDocument'
  | 'K3Material'
  | 'Other';

export type AssessmentMethod = 
  | 'Written'
  | 'Practical'
  | 'Oral'
  | 'Observation'
  | 'Simulation'
  | 'Portfolio'
  | 'Continuous'
  | 'Final';

export type TrainingCommentType = 
  | 'General'
  | 'AdminNote'
  | 'InstructorNote'
  | 'Feedback'
  | 'Question'
  | 'Issue'
  | 'Improvement';

export type ValidityPeriod = 
  | 'OneMonth'
  | 'ThreeMonths'
  | 'SixMonths'
  | 'OneYear'
  | 'TwoYears'
  | 'ThreeYears'
  | 'FiveYears'
  | 'Indefinite';

// DTOs matching backend models
export interface TrainingDto {
  id: number;
  trainingCode: string;
  title: string;
  description: string;
  type: TrainingType;
  category: TrainingCategory;
  status: TrainingStatus;
  priority: TrainingPriority;
  deliveryMethod: TrainingDeliveryMethod;

  // Schedule & Execution
  scheduledStartDate: string;
  scheduledEndDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  durationHours: number;
  venue?: string;
  venueAddress?: string;
  geoLocation?: {
    latitude: number;
    longitude: number;
  };

  // Instructor & Organization
  instructorName?: string;
  instructorQualifications?: string;
  instructorContact?: string;
  isExternalInstructor: boolean;
  maxParticipants: number;
  minParticipants: number;
  currentParticipants: number;

  // Certification & Compliance
  requiresCertification: boolean;
  certificationType?: CertificationType;
  certificateValidityPeriod?: ValidityPeriod;
  certifyingBody?: string;
  passingScore?: number;
  assessmentMethod?: AssessmentMethod;

  // Indonesian Compliance
  isK3MandatoryTraining: boolean;
  k3RegulationReference?: string;
  isBPJSCompliant: boolean;
  bpjsReference?: string;
  skkniReference?: string;
  requiresGovernmentCertification: boolean;

  // Content & Resources
  learningObjectives?: string;
  courseOutline?: string;
  prerequisites?: string;
  materials?: string;
  onlineLink?: string;
  onlinePlatform?: string;

  // Assessment & Results
  totalParticipants: number;
  completedParticipants: number;
  passedParticipants: number;
  averageScore?: number;
  completionRate: number;
  passRate: number;
  averageRating?: number;
  totalRatings: number;

  // Status flags
  isActive: boolean;
  isCancelled: boolean;
  isCompleted: boolean;
  isOverdue: boolean;
  canEdit: boolean;
  canStart: boolean;
  canComplete: boolean;
  canCancel: boolean;
  canEnroll: boolean;

  // Audit fields
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface TrainingParticipantDto {
  id: number;
  trainingId: number;
  userId: number;
  userName: string;
  userEmail: string;
  department?: string;
  position?: string;
  status: ParticipantStatus;
  enrolledAt: string;
  enrolledBy: string;
  
  // Attendance
  attendanceMarked: boolean;
  attendanceDate?: string;
  
  // Assessment
  finalScore?: number;
  passed: boolean;
  completedAt?: string;
  
  // Certification
  certificateIssued: boolean;
  certificateNumber?: string;
  certificateIssuedAt?: string;
  
  // Feedback
  feedback?: string;
  rating?: number;
}

export interface TrainingRequirementDto {
  id: number;
  trainingId: number;
  description: string;
  isMandatory: boolean;
  dueDate?: string;
  isCompleted: boolean;
  completedAt?: string;
  competencyLevel?: string;
  verifiedBy?: string;
  verifiedAt?: string;
}

export interface TrainingAttachmentDto {
  id: number;
  trainingId: number;
  fileName: string;
  originalFileName: string;
  filePath: string;
  fileSize: number;
  contentType: string;
  attachmentType: TrainingAttachmentType;
  description?: string;
  uploadedBy: string;
  uploadedAt: string;
  isTrainingMaterial: boolean;
  isPublic: boolean;
  version: number;
  isApproved: boolean;
  approvedBy?: string;
  approvedAt?: string;
}

export interface TrainingCommentDto {
  id: number;
  trainingId: number;
  content: string;
  commentType: TrainingCommentType;
  authorId?: number;
  authorName: string;
  isInternal: boolean;
  isVisible: boolean;
  isPinned: boolean;
  parentCommentId?: number;
  replies?: TrainingCommentDto[];
  createdAt: string;
  updatedAt?: string;
}

export interface TrainingCertificationDto {
  id: number;
  trainingId: number;
  participantUserId: number;
  participantName: string;
  certificateNumber: string;
  certificationType: CertificationType;
  issuedDate: string;
  expiryDate?: string;
  validUntil?: string;
  status: CertificationStatus;
  score?: number;
  grade?: string;
  certifyingBody?: string;
  digitalCertificateUrl?: string;
  verificationCode?: string;
  isK3Certificate: boolean;
  governmentRegistrationNumber?: string;
  ministryApprovalReference?: string;
}

export interface TrainingFormData {
  title: string;
  description: string;
  type: TrainingType;
  category: TrainingCategory;
  priority: TrainingPriority;
  deliveryMethod: TrainingDeliveryMethod;
  scheduledStartDate: string;
  scheduledEndDate: string;
  venue?: string;
  venueAddress?: string;
  maxParticipants: number;
  minParticipants: number;
  instructorName?: string;
  instructorQualifications?: string;
  instructorContact?: string;
  isExternalInstructor: boolean;
  requiresCertification: boolean;
  certificationType?: CertificationType;
  certificateValidityPeriod?: ValidityPeriod;
  certifyingBody?: string;
  passingScore?: number;
  assessmentMethod?: AssessmentMethod;
  isK3MandatoryTraining: boolean;
  k3RegulationReference?: string;
  isBPJSCompliant: boolean;
  bpjsReference?: string;
  skkniReference?: string;
  requiresGovernmentCertification: boolean;
  learningObjectives?: string;
  courseOutline?: string;
  prerequisites?: string;
  materials?: string;
  onlineLink?: string;
  onlinePlatform?: string;
}

// Constants for dropdowns
export const TRAINING_TYPES = [
  { value: 'SafetyOrientation', label: 'Safety Orientation' },
  { value: 'EmergencyResponse', label: 'Emergency Response' },
  { value: 'K3Training', label: 'K3 Training' },
  { value: 'FireSafety', label: 'Fire Safety' },
  { value: 'ConfinedSpaceEntry', label: 'Confined Space Entry' },
  { value: 'ChemicalHandling', label: 'Chemical Handling' },
  { value: 'EnvironmentalAwareness', label: 'Environmental Awareness' },
  { value: 'PPEUsage', label: 'PPE Usage' },
  { value: 'IncidentReporting', label: 'Incident Reporting' },
  { value: 'Compliance', label: 'Compliance' },
  { value: 'LeadershipDevelopment', label: 'Leadership Development' },
  { value: 'TechnicalTraining', label: 'Technical Training' },
  { value: 'EquipmentOperation', label: 'Equipment Operation' },
  { value: 'MaintenanceProcedures', label: 'Maintenance Procedures' },
  { value: 'ElectricalSafety', label: 'Electrical Safety' },
  { value: 'WorkAtHeight', label: 'Work at Height' },
  { value: 'LiftingOperations', label: 'Lifting Operations' },
  { value: 'DrivingSafety', label: 'Driving Safety' },
  { value: 'FirstAid', label: 'First Aid' },
  { value: 'Other', label: 'Other' }
];

export const TRAINING_CATEGORIES = [
  { value: 'MandatoryCompliance', label: 'Mandatory Compliance' },
  { value: 'SafetyTraining', label: 'Safety Training' },
  { value: 'SpecializedTraining', label: 'Specialized Training' },
  { value: 'ProfessionalDevelopment', label: 'Professional Development' },
  { value: 'LeadershipTraining', label: 'Leadership Training' },
  { value: 'TechnicalSkills', label: 'Technical Skills' },
  { value: 'SoftSkills', label: 'Soft Skills' },
  { value: 'Induction', label: 'Induction' },
  { value: 'Refresher', label: 'Refresher' },
  { value: 'Certification', label: 'Certification' }
];

export const TRAINING_PRIORITIES = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' }
];

export const DELIVERY_METHODS = [
  { value: 'InPerson', label: 'In-Person' },
  { value: 'Online', label: 'Online' },
  { value: 'Hybrid', label: 'Hybrid' },
  { value: 'Workshop', label: 'Workshop' },
  { value: 'OnTheJob', label: 'On-the-Job' },
  { value: 'SelfPaced', label: 'Self-Paced' },
  { value: 'Simulation', label: 'Simulation' },
  { value: 'Practical', label: 'Practical' }
];

export const CERTIFICATE_TYPES = [
  { value: 'K3Certificate', label: 'K3 Certificate' },
  { value: 'Professional', label: 'Professional' },
  { value: 'Competency', label: 'Competency' },
  { value: 'Compliance', label: 'Compliance' },
  { value: 'ISO_Certificate', label: 'ISO Certificate' },
  { value: 'Internal', label: 'Internal' },
  { value: 'External', label: 'External' },
  { value: 'Government', label: 'Government' },
  { value: 'Industry', label: 'Industry' },
  { value: 'Safety', label: 'Safety' }
];

export const VALIDITY_PERIODS = [
  { value: 'OneMonth', label: '1 Month' },
  { value: 'ThreeMonths', label: '3 Months' },
  { value: 'SixMonths', label: '6 Months' },
  { value: 'OneYear', label: '1 Year' },
  { value: 'TwoYears', label: '2 Years' },
  { value: 'ThreeYears', label: '3 Years' },
  { value: 'FiveYears', label: '5 Years' },
  { value: 'Indefinite', label: 'Indefinite' }
];

export const ASSESSMENT_METHODS = [
  { value: 'Written', label: 'Written Test' },
  { value: 'Practical', label: 'Practical Assessment' },
  { value: 'Oral', label: 'Oral Examination' },
  { value: 'Observation', label: 'Observation' },
  { value: 'Simulation', label: 'Simulation' },
  { value: 'Portfolio', label: 'Portfolio' },
  { value: 'Continuous', label: 'Continuous Assessment' },
  { value: 'Final', label: 'Final Examination' }
];