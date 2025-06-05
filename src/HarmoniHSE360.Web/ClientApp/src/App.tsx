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
import '@coreui/icons/css/all.min.css';

// Custom styles with Harmoni branding
import './styles/app.scss';

// Store
import { store } from './store';

// Layouts
import DefaultLayout from './layouts/DefaultLayout';
import AuthLayout from './layouts/AuthLayout';

// Guards
import PrivateRoute from './components/auth/PrivateRoute';
import AdminRoute from './components/auth/AdminRoute';

// Hooks
import { useSignalR } from './hooks/useSignalR';

// Lazy load pages with better error handling
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
const IncidentDetail = React.lazy(() =>
  import('./pages/incidents/IncidentDetail').catch((err) => {
    console.error('Failed to load IncidentDetail:', err);
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

// Loading component
const Loading = () => (
  <div className="d-flex justify-content-center align-items-center min-vh-100">
    <CSpinner color="primary" />
  </div>
);

// Route change handler component
const RouteChangeHandler = () => {
  const location = useLocation();

  useEffect(() => {
    // Scroll to top on route change
    window.scrollTo(0, 0);

    // Log route changes for debugging
    console.log('Route changed to:', location.pathname);
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
    // Initialize service worker for offline support
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.register('/sw.js').catch(() => {
        // Service worker registration failed, but that's okay for dev
      });
    }
  }, []);

  return (
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

                {/* Incident Management */}
                <Route path="/incidents" element={<IncidentList />} />
                <Route path="/incidents/dashboard" element={<IncidentDashboard />} />
                <Route path="/incidents/create" element={<CreateIncident />} />
                <Route
                  path="/incidents/quick-report"
                  element={<QuickReport />}
                />
                <Route path="/incidents/qr-scanner" element={<QrScanner />} />
                <Route path="/incidents/:id" element={<IncidentDetail />} />
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

                {/* PPE Management */}
                <Route path="/ppe" element={<PPEList />} />
                <Route path="/ppe/dashboard" element={<PPEDashboard />} />
                <Route path="/ppe/create" element={<CreatePPE />} />
                <Route path="/ppe/:id" element={<PPEDetail />} />
                <Route path="/ppe/:id/edit" element={<EditPPE />} />

                {/* Settings Routes - Protected by AdminRoute */}
                <Route path="/settings/*" element={
                  <AdminRoute>
                    <Routes>
                      <Route path="ppe" element={<PPEManagement />} />
                      <Route path="incidents" element={
                        <div className="p-4">
                          <h2>Incident Management Settings</h2>
                          <p>Configuration options for incident management coming soon...</p>
                        </div>
                      } />
                      <Route path="risks" element={
                        <div className="p-4">
                          <h2>Risk Management Settings</h2>
                          <p>Configuration options for risk management coming soon...</p>
                        </div>
                      } />
                      <Route path="users" element={
                        <div className="p-4">
                          <h2>User Management Settings</h2>
                          <p>User and role management coming soon...</p>
                        </div>
                      } />
                      <Route path="system" element={
                        <div className="p-4">
                          <h2>System Configuration</h2>
                          <p>General system settings coming soon...</p>
                        </div>
                      } />
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
  );
}

export default App;
