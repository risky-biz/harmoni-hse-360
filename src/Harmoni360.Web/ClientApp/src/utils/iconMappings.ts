import {
  faPlus,
  faEdit,
  faTrash,
  faEye,
  faSearch,
  faSearchPlus,
  faArrowLeft,
  faArrowRight,
  faSave,
  faDownload,
  faUpload,
  faFile,
  faFileImage,
  faFilePdf,
  faFileWord,
  faFileVideo,
  faFileContract,
  faMapMarkerAlt,
  faCalendarAlt,
  faClock,
  faExclamationTriangle,
  faCheckCircle,
  faInfoCircle,
  faTimes,
  faEllipsisV,
  faFilter,
  faSort,
  faRefresh,
  faArrowRotateRight,
  faHome,
  faClipboardList,
  faFileAlt,
  faBell,
  faExclamation,
  faSpinner,
  faUser,
  faUsers,
  faBuilding,
  faIndustry,
  faHeartbeat,
  faMedkit,
  faShieldAlt,
  faStethoscope,
  faUserMd,
  faPhone,
  faChartLine,
  faPrint,
  faTachometerAlt,
  faChartPie,
  faLock,
  faBars,
  faFolderOpen,
  faTag,
  faMapPin,
  faFire,
  faBolt,
  faFlask,
  faCarCrash,
  faHammer,
  faLeaf,
  faDatabase,
  faUtensils,
  faPersonFalling,
  faPersonFallingBurst,
  faGear,
  faBriefcaseMedical,
  faHandFist,
  faRadiation,
  faHardHat,
  faTriangleExclamation,
  faCamera,
  faStickyNote,
  faClipboardCheck,
  faListCheck,
  faTasks,
  faRecycle,
  faTrashAlt,
} from '@fortawesome/free-solid-svg-icons';

// Action Icons - Common actions across the incident management system
export const ACTION_ICONS = {
  // CRUD Operations
  create: faPlus,
  add: faPlus,
  edit: faEdit,
  update: faEdit,
  delete: faTrash,
  remove: faTrash,
  view: faEye,
  details: faEye,

  // Search and Filter
  search: faSearch,
  filter: faFilter,
  sort: faSort,

  // Navigation
  back: faArrowLeft,
  next: faArrowRight,
  home: faHome,

  // Form Actions
  save: faSave,
  cancel: faTimes,
  submit: faCheckCircle,

  // File Operations
  upload: faUpload,
  download: faDownload,
  attach: faFile,

  // UI Controls
  menu: faEllipsisV,
  options: faEllipsisV,
  refresh: faArrowRotateRight,
  loading: faSpinner,

  // Status Actions
  approve: faCheckCircle,
  reject: faTimes,
  warning: faExclamationTriangle,
  info: faInfoCircle,

  // Assessment and Analysis
  assessment: faShieldAlt,
  analyze: faSearchPlus,
};

// File Type Icons - For attachment management
export const FILE_TYPE_ICONS = {
  default: faFile,
  image: faFileImage,
  pdf: faFilePdf,
  document: faFileWord,
  word: faFileWord,
  text: faFileAlt,
  video: faFileVideo,
};

// Hazard Management Icons - Standardized hazard reporting iconography
export const HAZARD_ICONS = {
  // Primary hazard reporting icon - Industry standard warning symbol
  general: faTriangleExclamation,
  reporting: faTriangleExclamation,
  warning: faTriangleExclamation,
  
  // Specific hazard type icons (used within content, not for navigation)
  fire: faFire,
  chemical: faFlask,
  electrical: faBolt,
  mechanical: faGear,
  biological: faTriangleExclamation, // Use general warning, not specialized biohazard
  radiation: faTriangleExclamation, // Use general warning, not specialized radiation
  physical: faTriangleExclamation,
  environmental: faLeaf,
  ergonomic: faTriangleExclamation,
  psychological: faTriangleExclamation,
};

// Context Icons - Specific to incident management context
export const CONTEXT_ICONS = {
  // Incident Related
  incident: faPersonFallingBurst,
  report: faClipboardList,
  reports: faClipboardList,
  dashboard: faHome,

  // Hazard Management - Use standardized hazard icons
  hazard: HAZARD_ICONS.general,
  hazardReporting: HAZARD_ICONS.reporting,

  // Location and Time
  location: faMapMarkerAlt,
  pin: faMapPin,
  date: faCalendarAlt,
  time: faClock,

  // Categories and Types
  category: faFolderOpen,
  type: faTag,

  // People and Organization
  user: faUser,
  users: faUsers,
  reporter: faUser,
  investigator: faUser,
  department: faBuilding,
  company: faIndustry,

  // Notifications
  notification: faBell,
  alert: faExclamation,

  // Health Management
  health: faHeartbeat,
  medical: faMedkit,
  vaccination: faShieldAlt,
  doctor: faUserMd,
  stethoscope: faStethoscope,
  emergency: faPhone,
  analytics: faChartLine,

  // Inspection Management
  inspection: faClipboardCheck,
  inspections: faClipboardCheck,
  checklist: faListCheck,
  audit: faTasks,
  quality: faClipboardCheck,
  compliance: faClipboardCheck,

  // Training Management
  training: faClipboardCheck,
  license: faFileContract,

  // Waste Management
  waste: faRecycle,
  wasteManagement: faRecycle,
  disposal: faTrashAlt,
  
  // System & Interface
  print: faPrint,
  dashboard_speedometer: faTachometerAlt,
  chart_pie: faChartPie,
  security: faLock,
  menu: faBars,

  // Form Section Icons for CreateIncident
  basicInformation: faInfoCircle,
  locationTime: faMapMarkerAlt,
  additionalDetails: faStickyNote,
  evidence: faCamera,
};

// Severity Icons - For incident severity levels
export const SEVERITY_ICONS = {
  Critical: faPersonFallingBurst,
  Serious: faPersonFallingBurst,
  Moderate: faInfoCircle,
  Minor: faCheckCircle,
};

// Status Icons - For incident status levels
export const STATUS_ICONS = {
  Reported: faClipboardList,
  UnderInvestigation: faClock,
  AwaitingAction: faPersonFallingBurst,
  Resolved: faCheckCircle,
  Closed: faCheckCircle,
};

// Category Icons - For incident categories
export const CATEGORY_ICONS = {
  'Slip, Trip, Fall': faPersonFalling,
  'Equipment Malfunction': faGear,
  'Chemical Exposure': faFlask,
  'Fire/Explosion': faFire,
  'Medical Emergency': faBriefcaseMedical,
  'Security Breach': faShieldAlt,
  'Vehicle Accident': faCarCrash,
  'Workplace Violence': faHandFist,
  'Environmental': faLeaf,
  'Electrical': faBolt,
  'Structural': faBuilding,
  'Food Safety': faUtensils,
  'Data Breach': faDatabase,
  'Near Miss': faPersonFallingBurst,
  'Property Damage': faHammer,
  default: faPersonFallingBurst,
};

// Priority Icons - For immediate action requirements
export const PRIORITY_ICONS = {
  immediate: faPersonFallingBurst,
  high: faPersonFallingBurst,
  medium: faInfoCircle,
  low: faCheckCircle,
  critical: faPersonFallingBurst,
};

// Location Risk Icons
export const LOCATION_ICONS = {
  highRisk: faPersonFallingBurst,
  normal: faMapMarkerAlt,
  safe: faCheckCircle,
};

// Helper function to get file type icon based on filename
export const getFileTypeIcon = (fileName: string) => {
  const extension = fileName.split('.').pop()?.toLowerCase();

  switch (extension) {
    case 'pdf':
      return FILE_TYPE_ICONS.pdf;
    case 'doc':
    case 'docx':
      return FILE_TYPE_ICONS.document;
    case 'txt':
      return FILE_TYPE_ICONS.text;
    case 'jpg':
    case 'jpeg':
    case 'png':
    case 'gif':
      return FILE_TYPE_ICONS.image;
    case 'mp4':
    case 'avi':
    case 'mov':
      return FILE_TYPE_ICONS.video;
    default:
      return FILE_TYPE_ICONS.default;
  }
};

// Helper function to get category icon
export const getCategoryIcon = (categoryName: string) => {
  return CATEGORY_ICONS[categoryName as keyof typeof CATEGORY_ICONS] || CATEGORY_ICONS.default;
};

// Helper function to get priority icon
export const getPriorityIcon = (priority: string) => {
  return PRIORITY_ICONS[priority.toLowerCase() as keyof typeof PRIORITY_ICONS] || PRIORITY_ICONS.medium;
};

// Helper function to get hazard-specific icon
export const getHazardIcon = (hazardType?: string) => {
  if (!hazardType) return HAZARD_ICONS.general;
  
  const type = hazardType.toLowerCase();
  const hazardIconMap: { [key: string]: any } = {
    'fire': HAZARD_ICONS.fire,
    'chemical': HAZARD_ICONS.chemical,
    'electrical': HAZARD_ICONS.electrical,
    'mechanical': HAZARD_ICONS.mechanical,
    'environmental': HAZARD_ICONS.environmental,
    'biological': HAZARD_ICONS.biological,
    'radiation': HAZARD_ICONS.radiation,
    'physical': HAZARD_ICONS.physical,
    'ergonomic': HAZARD_ICONS.ergonomic,
    'psychological': HAZARD_ICONS.psychological,
  };
  
  return hazardIconMap[type] || HAZARD_ICONS.general;
};

// Helper function to get standardized hazard reporting icon
export const getHazardReportingIcon = () => {
  return HAZARD_ICONS.reporting;
};
