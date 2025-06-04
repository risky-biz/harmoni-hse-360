import {
  faPlus,
  faEdit,
  faTrash,
  faEye,
  faSearch,
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
  faHome,
  faClipboardList,
  faFileAlt,
  faBell,
  faExclamation,
  faSpinner,
  faUser,
  faUsers,
  faBuilding,
  faIndustry
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
  refresh: faRefresh,
  loading: faSpinner,
  
  // Status Actions
  approve: faCheckCircle,
  reject: faTimes,
  warning: faExclamationTriangle,
  info: faInfoCircle
};

// File Type Icons - For attachment management
export const FILE_TYPE_ICONS = {
  default: faFile,
  image: faFileImage,
  pdf: faFilePdf,
  document: faFileWord,
  word: faFileWord,
  text: faFileAlt,
  video: faFileVideo
};

// Context Icons - Specific to incident management context
export const CONTEXT_ICONS = {
  // Incident Related
  incident: faExclamationTriangle,
  report: faClipboardList,
  reports: faClipboardList,
  dashboard: faHome,
  
  // Location and Time
  location: faMapMarkerAlt,
  date: faCalendarAlt,
  time: faClock,
  
  // People and Organization
  user: faUser,
  users: faUsers,
  reporter: faUser,
  investigator: faUser,
  department: faBuilding,
  company: faIndustry,
  
  // Notifications
  notification: faBell,
  alert: faExclamation
};

// Severity Icons - For incident severity levels
export const SEVERITY_ICONS = {
  Critical: faExclamationTriangle,
  Serious: faExclamationTriangle, 
  Moderate: faInfoCircle,
  Minor: faCheckCircle
};

// Status Icons - For incident status levels  
export const STATUS_ICONS = {
  Reported: faClipboardList,
  UnderInvestigation: faClock,
  AwaitingAction: faExclamationTriangle,
  Resolved: faCheckCircle,
  Closed: faCheckCircle
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