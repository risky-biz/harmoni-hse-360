import React, { Suspense, useEffect } from 'react';
import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
  useLocation,
} from 'react-router-dom';
import { Provider } from 'react-redux';
import { CSpinner } from '@coreui/react';

// CoreUI styles
import '@coreui/coreui/dist/css/coreui.min.css';

// Custom styles with Harmoni branding
import './styles/app.scss';
import './styles/hsse-dashboard.css';

// Store
import { store } from './store';

// Theme Provider
import { ThemeProvider } from './contexts/ThemeContext';

// Layouts
import DefaultLayout from './layouts/DefaultLayout';
import AuthLayout from './layouts/AuthLayout';

// Guards
import PrivateRoute from './components/auth/PrivateRoute';
import AdminRoute from './components/auth/AdminRoute';
import AuthErrorBoundary from './components/common/AuthErrorBoundary';

// Hooks
import { useSignalR } from './hooks/useSignalR';

// Performance optimizations
import { PerformanceMonitor } from './utils/performance';
import { initializeOptimizations, addResourceHints, lazy } from './utils/optimization';

// Demo reset service
import { demoResetService } from './services/demoResetService';

// Add UnauthorizedAccess component
const UnauthorizedAccess = React.lazy(() =>
  import('./components/common/UnauthorizedAccess').catch((err) => {
    console.error('Failed to load UnauthorizedAccess:', err);
    return {
      default: () => <div>Error loading unauthorized page. Please refresh.</div>,
    };
  })
);


// Lazy load pages with optimized error handling
const Dashboard = React.lazy(() =>
  import('./pages/Dashboard').catch((err) => {
    console.error('Failed to load Dashboard:', err);
    return {
      default: () => <div>Error loading Dashboard. Please refresh.</div>,
    };
  })
);
const Login = React.lazy(() =>
  import('./pages/auth/Login').catch((err) => {
    console.error('Failed to load Login:', err);
    return { default: () => <div>Error loading Login. Please refresh.</div> };
  })
);
const IncidentList = React.lazy(() =>
  import('./pages/incidents/IncidentList').catch((err) => {
    console.error('Failed to load IncidentList:', err);
    return {
      default: () => <div>Error loading Incident List. Please refresh.</div>,
    };
  })
);
const CreateIncident = React.lazy(() =>
  import('./pages/incidents/CreateIncident').catch((err) => {
    console.error('Failed to load CreateIncident:', err);
    return {
      default: () => <div>Error loading Create Incident. Please refresh.</div>,
    };
  })
);
const IncidentDetailEnhanced = React.lazy(() =>
  import('./pages/incidents/IncidentDetailEnhanced').catch((err) => {
    console.error('Failed to load IncidentDetailEnhanced:', err);
    return {
      default: () => <div>Error loading Incident Detail. Please refresh.</div>,
    };
  })
);
const EditIncident = React.lazy(() =>
  import('./pages/incidents/EditIncident').catch((err) => {
    console.error('Failed to load EditIncident:', err);
    return {
      default: () => <div>Error loading Edit Incident. Please refresh.</div>,
    };
  })
);
const MyReports = React.lazy(() =>
  import('./pages/incidents/MyReports').catch((err) => {
    console.error('Failed to load MyReports:', err);
    return {
      default: () => <div>Error loading My Reports. Please refresh.</div>,
    };
  })
);
const IncidentDashboard = React.lazy(() =>
  import('./pages/incidents/IncidentDashboard').catch((err) => {
    console.error('Failed to load IncidentDashboard:', err);
    return {
      default: () => <div>Error loading Incident Dashboard. Please refresh.</div>,
    };
  })
);
const QuickReport = React.lazy(() =>
  import('./pages/incidents/QuickReport').catch((err) => {
    console.error('Failed to load QuickReport:', err);
    return {
      default: () => <div>Error loading Quick Report. Please refresh.</div>,
    };
  })
);
const QrScanner = React.lazy(() =>
  import('./pages/incidents/QrScanner').catch((err) => {
    console.error('Failed to load QrScanner:', err);
    return {
      default: () => <div>Error loading QR Scanner. Please refresh.</div>,
    };
  })
);

// Hazard Management Pages
const HazardDashboard = React.lazy(() =>
  import('./pages/hazards/HazardDashboard').catch((err) => {
    console.error('Failed to load HazardDashboard:', err);
    return {
      default: () => <div>Error loading Hazard Dashboard. Please refresh.</div>,
    };
  })
);
const CreateHazard = React.lazy(() =>
  import('./pages/hazards/CreateHazard').catch((err) => {
    console.error('Failed to load CreateHazard:', err);
    return {
      default: () => <div>Error loading Create Hazard. Please refresh.</div>,
    };
  })
);
const HazardList = React.lazy(() =>
  import('./pages/hazards/HazardList').catch((err) => {
    console.error('Failed to load HazardList:', err);
    return {
      default: () => <div>Error loading Hazard List. Please refresh.</div>,
    };
  })
);
const HazardDetail = React.lazy(() =>
  import('./pages/hazards/HazardDetail').catch((err) => {
    console.error('Failed to load HazardDetail:', err);
    return {
      default: () => <div>Error loading Hazard Detail. Please refresh.</div>,
    };
  })
);
const EditHazard = React.lazy(() =>
  import('./pages/hazards/EditHazard').catch((err) => {
    console.error('Failed to load EditHazard:', err);
    return {
      default: () => <div>Error loading Edit Hazard. Please refresh.</div>,
    };
  })
);
const MyHazards = React.lazy(() =>
  import('./pages/hazards/MyHazards').catch((err) => {
    console.error('Failed to load MyHazards:', err);
    return {
      default: () => <div>Error loading My Hazards. Please refresh.</div>,
    };
  })
);
const RiskAssessments = React.lazy(() =>
  import('./pages/hazards/RiskAssessments').catch((err) => {
    console.error('Failed to load RiskAssessments:', err);
    return {
      default: () => <div>Error loading Risk Assessments. Please refresh.</div>,
    };
  })
);

// Risk Assessment Management Pages
const RiskAssessmentList = React.lazy(() =>
  import('./pages/risk-assessments/RiskAssessmentList').catch((err) => {
    console.error('Failed to load RiskAssessmentList:', err);
    return {
      default: () => <div>Error loading Risk Assessment List. Please refresh.</div>,
    };
  })
);
const RiskAssessmentDetail = React.lazy(() =>
  import('./pages/risk-assessments/RiskAssessmentDetail').catch((err) => {
    console.error('Failed to load RiskAssessmentDetail:', err);
    return {
      default: () => <div>Error loading Risk Assessment Detail. Please refresh.</div>,
    };
  })
);
const CreateRiskAssessment = React.lazy(() =>
  import('./pages/risk-assessments/CreateRiskAssessment').catch((err) => {
    console.error('Failed to load CreateRiskAssessment:', err);
    return {
      default: () => <div>Error loading Create Risk Assessment. Please refresh.</div>,
    };
  })
);
const HazardAnalytics = React.lazy(() =>
  import('./pages/hazards/HazardAnalytics').catch((err) => {
    console.error('Failed to load HazardAnalytics:', err);
    return {
      default: () => <div>Error loading Hazard Analytics. Please refresh.</div>,
    };
  })
);
const MitigationActions = React.lazy(() =>
  import('./pages/hazards/MitigationActions').catch((err) => {
    console.error('Failed to load MitigationActions:', err);
    return {
      default: () => <div>Error loading Mitigation Actions. Please refresh.</div>,
    };
  })
);
const HazardMapping = React.lazy(() =>
  import('./pages/hazards/HazardMapping').catch((err) => {
    console.error('Failed to load HazardMapping:', err);
    return {
      default: () => <div>Error loading Hazard Mapping. Please refresh.</div>,
    };
  })
);
const MobileHazardReport = React.lazy(() =>
  import('./pages/hazards/MobileHazardReport').catch((err) => {
    console.error('Failed to load MobileHazardReport:', err);
    return {
      default: () => <div>Error loading Mobile Hazard Report. Please refresh.</div>,
    };
  })
);

// PPE Management Pages
const PPEDashboard = React.lazy(() =>
  import('./pages/ppe/PPEDashboard').catch((err) => {
    console.error('Failed to load PPEDashboard:', err);
    return {
      default: () => <div>Error loading PPE Dashboard. Please refresh.</div>,
    };
  })
);
const PPEManagement = React.lazy(() =>
  import('./pages/ppe/PPEManagement').catch((err) => {
    console.error('Failed to load PPEManagement:', err);
    return {
      default: () => <div>Error loading PPE Management. Please refresh.</div>,
    };
  })
);
const PPEOperationalManagement = React.lazy(() =>
  import('./pages/ppe/PPEOperationalManagement').catch((err) => {
    console.error('Failed to load PPEOperationalManagement:', err);
    return {
      default: () => <div>Error loading PPE Operational Management. Please refresh.</div>,
    };
  })
);
const PPEList = React.lazy(() =>
  import('./pages/ppe/PPEList').catch((err) => {
    console.error('Failed to load PPEList:', err);
    return {
      default: () => <div>Error loading PPE List. Please refresh.</div>,
    };
  })
);
const CreatePPE = React.lazy(() =>
  import('./pages/ppe/CreatePPE').catch((err) => {
    console.error('Failed to load CreatePPE:', err);
    return {
      default: () => <div>Error loading Create PPE. Please refresh.</div>,
    };
  })
);
const EditPPE = React.lazy(() =>
  import('./pages/ppe/EditPPE').catch((err) => {
    console.error('Failed to load EditPPE:', err);
    return {
      default: () => <div>Error loading Edit PPE. Please refresh.</div>,
    };
  })
);
const PPEDetail = React.lazy(() =>
  import('./pages/ppe/PPEDetail').catch((err) => {
    console.error('Failed to load PPEDetail:', err);
    return {
      default: () => <div>Error loading PPE Detail. Please refresh.</div>,
    };
  })
);

// Inspection Management Pages
const InspectionDashboard = React.lazy(() =>
  import('./pages/inspections/InspectionDashboard').then(module => ({
    default: module.InspectionDashboard
  })).catch((err) => {
    console.error('Failed to load InspectionDashboard:', err);
    return {
      default: () => <div>Error loading Inspection Dashboard. Please refresh.</div>,
    };
  })
);
const InspectionList = React.lazy(() =>
  import('./pages/inspections/InspectionList').then(module => ({
    default: module.InspectionList
  })).catch((err) => {
    console.error('Failed to load InspectionList:', err);
    return {
      default: () => <div>Error loading Inspection List. Please refresh.</div>,
    };
  })
);
const CreateInspection = React.lazy(() =>
  import('./pages/inspections/CreateInspection').then(module => ({
    default: module.CreateInspection
  })).catch((err) => {
    console.error('Failed to load CreateInspection:', err);
    return {
      default: () => <div>Error loading Create Inspection. Please refresh.</div>,
    };
  })
);
const InspectionDetail = React.lazy(() =>
  import('./pages/inspections/InspectionDetail').then(module => ({
    default: module.InspectionDetail
  })).catch((err) => {
    console.error('Failed to load InspectionDetail:', err);
    return {
      default: () => <div>Error loading Inspection Detail. Please refresh.</div>,
    };
  })
);
const EditInspection = React.lazy(() =>
  import('./pages/inspections/EditInspection').then(module => ({
    default: module.EditInspection
  })).catch((err) => {
    console.error('Failed to load EditInspection:', err);
    return {
      default: () => <div>Error loading Edit Inspection. Please refresh.</div>,
    };
  })
);
const MyInspections = React.lazy(() =>
  import('./pages/inspections/MyInspections').then(module => ({
    default: module.MyInspections
  })).catch((err) => {
    console.error('Failed to load MyInspections:', err);
    return {
      default: () => <div>Error loading My Inspections. Please refresh.</div>,
    };
  })
);

// Work Permit Management Pages
const WorkPermitDashboard = React.lazy(() =>
  import('./pages/work-permits/WorkPermitDashboard').catch((err) => {
    console.error('Failed to load WorkPermitDashboard:', err);
    return {
      default: () => <div>Error loading Work Permit Dashboard. Please refresh.</div>,
    };
  })
);
const WorkPermitList = React.lazy(() =>
  import('./pages/work-permits/WorkPermitList').catch((err) => {
    console.error('Failed to load WorkPermitList:', err);
    return {
      default: () => <div>Error loading Work Permit List. Please refresh.</div>,
    };
  })
);
const CreateWorkPermit = React.lazy(() =>
  import('./pages/work-permits/CreateWorkPermit').catch((err) => {
    console.error('Failed to load CreateWorkPermit:', err);
    return {
      default: () => <div>Error loading Create Work Permit. Please refresh.</div>,
    };
  })
);
const EditWorkPermit = React.lazy(() =>
  import('./pages/work-permits/EditWorkPermit').catch((err) => {
    console.error('Failed to load EditWorkPermit:', err);
    return {
      default: () => <div>Error loading Edit Work Permit. Please refresh.</div>,
    };
  })
);
const WorkPermitDetail = React.lazy(() =>
  import('./pages/work-permits/WorkPermitDetail').catch((err) => {
    console.error('Failed to load WorkPermitDetail:', err);
    return {
      default: () => <div>Error loading Work Permit Detail. Please refresh.</div>,
    };
  })
);
const WorkPermitApproval = React.lazy(() =>
  import('./pages/work-permits/WorkPermitApproval').catch((err) => {
    console.error('Failed to load WorkPermitApproval:', err);
    return {
      default: () => <div>Error loading Work Permit Approval. Please refresh.</div>,
    };
  })
);
const MyWorkPermits = React.lazy(() =>
  import('./pages/work-permits/MyWorkPermits').catch((err) => {
    console.error('Failed to load MyWorkPermits:', err);
    return {
      default: () => <div>Error loading My Work Permits. Please refresh.</div>,
    };
  })
);

// License Management Pages
const LicenseDashboard = React.lazy(() =>
  import('./pages/licenses/LicenseDashboard').catch((err) => {
    console.error('Failed to load LicenseDashboard:', err);
    return {
      default: () => <div>Error loading License Dashboard. Please refresh.</div>,
    };
  })
);
const LicenseList = React.lazy(() =>
  import('./pages/licenses/LicenseList').catch((err) => {
    console.error('Failed to load LicenseList:', err);
    return {
      default: () => <div>Error loading License List. Please refresh.</div>,
    };
  })
);
const CreateLicense = React.lazy(() =>
  import('./pages/licenses/CreateLicense').catch((err) => {
    console.error('Failed to load CreateLicense:', err);
    return {
      default: () => <div>Error loading Create License. Please refresh.</div>,
    };
  })
);
const LicenseDetail = React.lazy(() =>
  import('./pages/licenses/LicenseDetail').catch((err) => {
    console.error('Failed to load LicenseDetail:', err);
    return {
      default: () => <div>Error loading License Detail. Please refresh.</div>,
    };
  })
);

// Audit Management Pages
const AuditDashboard = React.lazy(() =>
  import('./pages/audits/AuditDashboard').catch((err) => {
    console.error('Failed to load AuditDashboard:', err);
    return {
      default: () => <div>Error loading Audit Dashboard. Please refresh.</div>,
    };
  })
);
const AuditList = React.lazy(() =>
  import('./pages/audits/AuditList').catch((err) => {
    console.error('Failed to load AuditList:', err);
    return {
      default: () => <div>Error loading Audit List. Please refresh.</div>,
    };
  })
);
const CreateAudit = React.lazy(() =>
  import('./pages/audits/CreateAudit').catch((err) => {
    console.error('Failed to load CreateAudit:', err);
    return {
      default: () => <div>Error loading Create Audit. Please refresh.</div>,
    };
  })
);
const MyAudits = React.lazy(() =>
  import('./pages/audits/MyAudits').catch((err) => {
    console.error('Failed to load MyAudits:', err);
    return {
      default: () => <div>Error loading My Audits. Please refresh.</div>,
    };
  })
);
const AuditDetail = React.lazy(() =>
  import('./pages/audits/AuditDetail').catch((err) => {
    console.error('Failed to load AuditDetail:', err);
    return {
      default: () => <div>Error loading Audit Detail. Please refresh.</div>,
    };
  })
);
const EditAudit = React.lazy(() =>
  import('./pages/audits/EditAudit').catch((err) => {
    console.error('Failed to load EditAudit:', err);
    return {
      default: () => <div>Error loading Edit Audit. Please refresh.</div>,
    };
  })
);

// Security Management Pages
const SecurityDashboard = React.lazy(() =>
  import('./pages/security/SecurityDashboard').catch((err) => {
    console.error('Failed to load SecurityDashboard:', err);
    return {
      default: () => <div>Error loading Security Dashboard. Please refresh.</div>,
    };
  })
);
// HSSE Statistics Dashboard
const HsseDashboard = React.lazy(() =>
  import('./pages/hsse/HsseDashboard').catch((err) => {
    console.error('Failed to load HsseDashboard:', err);
    return {
      default: () => <div>Error loading HSSE Dashboard. Please refresh.</div>,
    };
  })
);
const SecurityIncidentList = React.lazy(() =>
  import('./pages/security/SecurityIncidentList').catch((err) => {
    console.error('Failed to load SecurityIncidentList:', err);
    return {
      default: () => <div>Error loading Security Incident List. Please refresh.</div>,
    };
  })
);
const CreateSecurityIncident = React.lazy(() =>
  import('./pages/security/CreateSecurityIncident').catch((err) => {
    console.error('Failed to load CreateSecurityIncident:', err);
    return {
      default: () => <div>Error loading Create Security Incident. Please refresh.</div>,
    };
  })
);
const SecurityIncidentDetail = React.lazy(() =>
  import('./pages/security/SecurityIncidentDetail').catch((err) => {
    console.error('Failed to load SecurityIncidentDetail:', err);
    return {
      default: () => <div>Error loading Security Incident Detail. Please refresh.</div>,
    };
  })
);

// Training Management Pages
const TrainingDashboard = React.lazy(() =>
  import('./pages/trainings/TrainingDashboard').catch((err) => {
    console.error('Failed to load TrainingDashboard:', err);
    return {
      default: () => <div>Error loading Training Dashboard. Please refresh.</div>,
    };
  })
);
const TrainingList = React.lazy(() =>
  import('./pages/trainings/TrainingList').catch((err) => {
    console.error('Failed to load TrainingList:', err);
    return {
      default: () => <div>Error loading Training List. Please refresh.</div>,
    };
  })
);
const CreateTraining = React.lazy(() =>
  import('./pages/trainings/CreateTraining').catch((err) => {
    console.error('Failed to load CreateTraining:', err);
    return {
      default: () => <div>Error loading Create Training. Please refresh.</div>,
    };
  })
);
const EditTraining = React.lazy(() =>
  import('./pages/trainings/EditTraining').catch((err) => {
    console.error('Failed to load EditTraining:', err);
    return {
      default: () => <div>Error loading Edit Training. Please refresh.</div>,
    };
  })
);
const TrainingDetail = React.lazy(() =>
  import('./pages/trainings/TrainingDetail').catch((err) => {
    console.error('Failed to load TrainingDetail:', err);
    return {
      default: () => <div>Error loading Training Detail. Please refresh.</div>,
    };
  })
);
const MyTrainings = React.lazy(() =>
  import('./pages/trainings/MyTrainings').catch((err) => {
    console.error('Failed to load MyTrainings:', err);
    return {
      default: () => <div>Error loading My Trainings. Please refresh.</div>,
    };
  })
);

// Health Management Pages (Optimized with lazy loading)
const HealthDashboard = React.lazy(() =>
  lazy.HealthDashboard().catch((err) => {
    console.error('Failed to load HealthDashboard:', err);
    return {
      default: () => <div>Error loading Health Dashboard. Please refresh.</div>,
    };
  })
);
const HealthList = React.lazy(() =>
  lazy.HealthList().catch((err) => {
    console.error('Failed to load HealthList:', err);
    return {
      default: () => <div>Error loading Health List. Please refresh.</div>,
    };
  })
);
const CreateHealthRecord = React.lazy(() =>
  lazy.CreateHealthRecord().catch((err) => {
    console.error('Failed to load CreateHealthRecord:', err);
    return {
      default: () => <div>Error loading Create Health Record. Please refresh.</div>,
    };
  })
);
const EditHealthRecord = React.lazy(() =>
  lazy.EditHealthRecord().catch((err) => {
    console.error('Failed to load EditHealthRecord:', err);
    return {
      default: () => <div>Error loading Edit Health Record. Please refresh.</div>,
    };
  })
);
const HealthDetail = React.lazy(() =>
  lazy.HealthDetail().catch((err) => {
    console.error('Failed to load HealthDetail:', err);
    return {
      default: () => <div>Error loading Health Detail. Please refresh.</div>,
    };
  })
);
const VaccinationManagement = React.lazy(() =>
  lazy.VaccinationManagement().catch((err) => {
    console.error('Failed to load VaccinationManagement:', err);
    return {
      default: () => <div>Error loading Vaccination Management. Please refresh.</div>,
    };
  })
);
const HealthCompliance = React.lazy(() =>
  lazy.HealthCompliance().catch((err) => {
    console.error('Failed to load HealthCompliance:', err);
    return {
      default: () => <div>Error loading Health Compliance. Please refresh.</div>,
    };
  })
);

// Waste Management Pages
const WasteReportList = React.lazy(() =>
  import('./pages/waste-management/WasteReportList').catch((err) => {
    console.error('Failed to load WasteReportList:', err);
    return {
      default: () => <div>Error loading Waste Report List. Please refresh.</div>,
    };
  })
);
const WasteReportForm = React.lazy(() =>
  import('./pages/waste-management/WasteReportForm').catch((err) => {
    console.error('Failed to load WasteReportForm:', err);
    return {
      default: () => <div>Error loading Waste Report Form. Please refresh.</div>,
    };
  })
);
const WasteDashboard = React.lazy(() =>
  import('./pages/waste-management/WasteDashboard').catch((err) => {
    console.error('Failed to load WasteDashboard:', err);
    return {
      default: () => <div>Error loading Waste Dashboard. Please refresh.</div>,
    };
  })
);
const WasteReportDetail = React.lazy(() =>
  import('./pages/waste-management/WasteReportDetail').catch((err) => {
    console.error('Failed to load WasteReportDetail:', err);
    return {
      default: () => <div>Error loading Waste Report Detail. Please refresh.</div>,
    };
  })
);
const MyWasteReports = React.lazy(() =>
  import('./pages/waste-management/MyWasteReports').catch((err) => {
    console.error('Failed to load MyWasteReports:', err);
    return {
      default: () => <div>Error loading My Waste Reports. Please refresh.</div>,
    };
  })
);
const DisposalProviders = React.lazy(() =>
  import('./pages/waste-management/DisposalProviders').catch((err) => {
    console.error('Failed to load DisposalProviders:', err);
    return {
      default: () => <div>Error loading Disposal Providers. Please refresh.</div>,
    };
  })
);
const CreateWasteReport = React.lazy(() =>
  import('./pages/waste-management/CreateWasteReport').catch((err) => {
    console.error('Failed to load CreateWasteReport:', err);
    return {
      default: () => <div>Error loading Create Waste Report. Please refresh.</div>,
    };
  })
);

// Admin Management Pages
const UserManagement = React.lazy(() =>
  import('./pages/admin/UserManagement').catch((err) => {
    console.error('Failed to load UserManagement:', err);
    return {
      default: () => <div>Error loading User Management. Please refresh.</div>,
    };
  })
);

// Settings Management Pages
const SystemSettings = React.lazy(() =>
  import('./pages/settings/SystemSettings').catch((err) => {
    console.error('Failed to load SystemSettings:', err);
    return {
      default: () => <div>Error loading System Settings. Please refresh.</div>,
    };
  })
);
const IncidentSettings = React.lazy(() =>
  import('./pages/settings/IncidentSettings').catch((err) => {
    console.error('Failed to load IncidentSettings:', err);
    return {
      default: () => <div>Error loading Incident Settings. Please refresh.</div>,
    };
  })
);
const RiskSettings = React.lazy(() =>
  import('./pages/settings/RiskSettings').catch((err) => {
    console.error('Failed to load RiskSettings:', err);
    return {
      default: () => <div>Error loading Risk Settings. Please refresh.</div>,
    };
  })
);

// Loading component
const Loading = () => (
  <div className="d-flex justify-content-center align-items-center min-vh-100">
    <CSpinner color="primary" />
  </div>
);

// Route change handler component with performance monitoring
const RouteChangeHandler = () => {
  const location = useLocation();

  useEffect(() => {
    // Scroll to top on route change
    window.scrollTo(0, 0);

    // Performance monitoring for route changes
    performance.mark('route-start');
    
    // Log route changes for debugging
    console.log('Route changed to:', location.pathname);
    
    // Monitor page performance
    const performanceMonitor = new PerformanceMonitor();
    performanceMonitor.measurePageLoad();
    
    // End route measurement
    performance.mark('route-end');
    performance.measure('route-change', 'route-start', 'route-end');
  }, [location]);

  return null;
};

// SignalR Connection Manager
const SignalRConnectionManager = () => {
  useSignalR();
  return null;
};

// Error Boundary Component
class ErrorBoundary extends React.Component<
  { children: React.ReactNode },
  { hasError: boolean; error?: Error }
> {
  constructor(props: { children: React.ReactNode }) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Error Boundary caught an error:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="d-flex justify-content-center align-items-center min-vh-100">
          <div className="text-center">
            <h2>Something went wrong</h2>
            <p>Please refresh the page to continue.</p>
            <button
              className="btn btn-primary"
              onClick={() => window.location.reload()}
            >
              Refresh Page
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}

function App() {
  useEffect(() => {
    // Initialize performance optimizations
    initializeOptimizations();
    addResourceHints();
    
    // Initialize demo reset service for automated 24-hour reset
    if (demoResetService) {
      console.log('Demo reset service initialized');
    }
    
    // Initialize service worker for offline support
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.register('/sw.js').catch(() => {
        // Service worker registration failed, but that's okay for dev
      });
    }
    
    // Initialize performance monitoring
    const performanceMonitor = new PerformanceMonitor();
    performanceMonitor.measurePageLoad();
    
    // Clean up on unmount
    return () => {
      // Performance cleanup would go here
    };
  }, []);

  return (
    <ThemeProvider>
      <AuthErrorBoundary>
        <ErrorBoundary>
          <Provider store={store}>
            <BrowserRouter>
              <RouteChangeHandler />
              <SignalRConnectionManager />
              <Suspense fallback={<Loading />}>
              <Routes>
              {/* Auth Routes */}
              <Route element={<AuthLayout />}>
                <Route path="/login" element={<Login />} />
              </Route>

              {/* Unauthorized Access Route (No authentication required) */}
              <Route path="/unauthorized" element={<UnauthorizedAccess />} />

              {/* Public Reporting Routes (No Authentication Required) */}
              <Route path="/report/qr/:qrId" element={<QuickReport />} />
              <Route path="/report/anonymous" element={<QuickReport />} />
              <Route path="/report/quick" element={<QuickReport />} />

              {/* Protected Routes */}
              <Route
                element={
                  <PrivateRoute>
                    <DefaultLayout />
                  </PrivateRoute>
                }
              >
                <Route
                  path="/"
                  element={<Navigate to="/dashboard" replace />}
                />
                <Route path="/dashboard" element={<Dashboard />} />
                <Route path="/hsse/dashboard" element={<HsseDashboard />} />

                {/* Incident Management */}
                <Route path="/incidents" element={<IncidentList />} />
                <Route path="/incidents/dashboard" element={<IncidentDashboard />} />
                <Route path="/incidents/create" element={<CreateIncident />} />
                <Route
                  path="/incidents/quick-report"
                  element={<QuickReport />}
                />
                <Route path="/incidents/qr-scanner" element={<QrScanner />} />
                <Route path="/incidents/:id" element={<IncidentDetailEnhanced />} />
                <Route path="/incidents/:id/edit" element={<EditIncident />} />
                <Route path="/incidents/my-reports" element={<MyReports />} />

                {/* Hazard Management */}
                <Route path="/hazards" element={<HazardList />} />
                <Route path="/hazards/dashboard" element={<HazardDashboard />} />
                <Route path="/hazards/create" element={<CreateHazard />} />
                <Route path="/hazards/mobile-report" element={<MobileHazardReport />} />
                <Route path="/hazards/my-hazards" element={<MyHazards />} />
                <Route path="/hazards/assessments" element={<RiskAssessments />} />
                <Route path="/hazards/analytics" element={<HazardAnalytics />} />
                <Route path="/hazards/mapping" element={<HazardMapping />} />
                <Route path="/hazards/:hazardId/mitigation-actions" element={<MitigationActions />} />
                <Route path="/hazards/:id" element={<HazardDetail />} />
                <Route path="/hazards/:id/edit" element={<EditHazard />} />

                {/* Risk Assessment Management */}
                <Route path="/risk-assessments" element={<RiskAssessmentList />} />
                <Route path="/risk-assessments/create" element={<CreateRiskAssessment />} />
                <Route path="/risk-assessments/create/:hazardId" element={<CreateRiskAssessment />} />
                <Route path="/risk-assessments/:id" element={<RiskAssessmentDetail />} />
                <Route path="/risk-assessments/:id/edit" element={<CreateRiskAssessment />} />
                <Route path="/risk-assessments/:id/reassess" element={<CreateRiskAssessment />} />

                {/* Inspection Management */}
                <Route path="/inspections" element={<InspectionList />} />
                <Route path="/inspections/dashboard" element={<InspectionDashboard />} />
                <Route path="/inspections/create" element={<CreateInspection />} />
                <Route path="/inspections/my-inspections" element={<MyInspections />} />
                <Route path="/inspections/:id" element={<InspectionDetail />} />
                <Route path="/inspections/:id/edit" element={<EditInspection />} />

                {/* PPE Management */}
                <Route path="/ppe" element={<PPEList />} />
                <Route path="/ppe/dashboard" element={<PPEDashboard />} />
                <Route path="/ppe/management" element={<PPEOperationalManagement />} />
                <Route path="/ppe/create" element={<CreatePPE />} />
                <Route path="/ppe/:id" element={<PPEDetail />} />
                <Route path="/ppe/:id/edit" element={<EditPPE />} />

                {/* Work Permit Management */}
                <Route path="/work-permits" element={<WorkPermitList />} />
                <Route path="/work-permits/dashboard" element={<WorkPermitDashboard />} />
                <Route path="/work-permits/create" element={<CreateWorkPermit />} />
                <Route path="/work-permits/my-permits" element={<MyWorkPermits />} />
                <Route path="/work-permits/:id" element={<WorkPermitDetail />} />
                <Route path="/work-permits/:id/edit" element={<EditWorkPermit />} />
                <Route path="/work-permits/:id/approve" element={<WorkPermitApproval />} />

                {/* License Management */}
                <Route path="/licenses" element={<LicenseList />} />
                <Route path="/licenses/dashboard" element={<LicenseDashboard />} />
                <Route path="/licenses/create" element={<CreateLicense />} />
                <Route path="/licenses/:id" element={<LicenseDetail />} />

                {/* Audit Management */}
                <Route path="/audits" element={<AuditList />} />
                <Route path="/audits/dashboard" element={<AuditDashboard />} />
                <Route path="/audits/create" element={<CreateAudit />} />
                <Route path="/audits/my-audits" element={<MyAudits />} />
                <Route path="/audits/:id" element={<AuditDetail />} />
                <Route path="/audits/:id/edit" element={<EditAudit />} />

                {/* Health Management */}
                <Route path="/health" element={<HealthList />} />
                <Route path="/health/dashboard" element={<HealthDashboard />} />
                <Route path="/health/create" element={<CreateHealthRecord />} />
                <Route path="/health/detail/:id" element={<HealthDetail />} />
                <Route path="/health/edit/:id" element={<EditHealthRecord />} />
                <Route path="/health/vaccinations" element={<VaccinationManagement />} />
                <Route path="/health/compliance" element={<HealthCompliance />} />

                {/* Security Management */}
                <Route path="/security" element={<SecurityIncidentList />} />
                <Route path="/security/incidents" element={<SecurityIncidentList />} />
                <Route path="/security/dashboard" element={<SecurityDashboard />} />
                <Route path="/security/incidents/create" element={<CreateSecurityIncident />} />
                <Route path="/security/incidents/:id" element={<SecurityIncidentDetail />} />
                <Route path="/security/incidents/:id/edit" element={<CreateSecurityIncident />} />

                {/* Training Management */}
                <Route path="/trainings" element={<TrainingList />} />
                <Route path="/trainings/dashboard" element={<TrainingDashboard />} />
                <Route path="/trainings/create" element={<CreateTraining />} />
                <Route path="/trainings/my-trainings" element={<MyTrainings />} />
                <Route path="/trainings/:id" element={<TrainingDetail />} />
                <Route path="/trainings/:id/edit" element={<EditTraining />} />
                <Route path="/trainings/:id/enroll" element={<TrainingDetail />} />
		{/* Waste Management */}
                <Route path="/waste-management" element={<WasteReportList />} />
                <Route path="/waste-management/dashboard" element={<WasteDashboard />} />
                <Route path="/waste-management/create" element={<CreateWasteReport />} />
                <Route path="/waste-management/my-reports" element={<MyWasteReports />} />
                <Route path="/waste-management/providers" element={<DisposalProviders />} />
                <Route path="/waste-management/:id" element={<WasteReportDetail />} />
                <Route path="/waste-management/:id/edit" element={<WasteReportForm />} />

                {/* Admin Routes - Protected by AdminRoute */}
                <Route
                  path="/admin/users"
                  element={
                    <AdminRoute>
                      <UserManagement />
                    </AdminRoute>
                  }
                />
                <Route
                  path="/admin/settings"
                  element={
                    <AdminRoute>
                      <div className="p-4">
                        <h2>System Settings</h2>
                        <p>General system settings coming soon...</p>
                      </div>
                    </AdminRoute>
                  }
                />

                {/* Settings Routes - Protected by AdminRoute */}
                <Route path="/settings/*" element={
                  <AdminRoute>
                    <Routes>
                      <Route path="ppe" element={<PPEManagement />} />
                      <Route path="incidents" element={<IncidentSettings />} />
                      <Route path="risks" element={<RiskSettings />} />
                      <Route path="users" element={
                        <div className="p-4">
                          <h2>User Management Settings</h2>
                          <p>User and role management coming soon...</p>
                        </div>
                      } />
                      <Route path="system" element={<SystemSettings />} />
                      <Route path="audit" element={
                        <div className="p-4">
                          <h2>Audit & Compliance Settings</h2>
                          <p>Audit and compliance configuration coming soon...</p>
                        </div>
                      } />
                      <Route index element={
                        <div className="p-4">
                          <h2>Application Settings</h2>
                          <p>Select a module from the sidebar to configure system settings.</p>
                        </div>
                      } />
                    </Routes>
                  </AdminRoute>
                } />

                {/* Profile & Settings (placeholder pages) */}
                <Route
                  path="/profile"
                  element={
                    <div className="p-4">
                      <h2>Profile Page</h2>
                      <p>Coming soon...</p>
                    </div>
                  }
                />

                {/* Catch all other routes and redirect to dashboard */}
                <Route
                  path="*"
                  element={<Navigate to="/dashboard" replace />}
                />
              </Route>
            </Routes>
            </Suspense>
          </BrowserRouter>
        </Provider>
      </ErrorBoundary>
    </AuthErrorBoundary>
    </ThemeProvider>
  );
}

export default App;
