import React, { Suspense, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom';
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

// Lazy load pages with better error handling
const Dashboard = React.lazy(() => import('./pages/Dashboard').catch(err => {
  console.error('Failed to load Dashboard:', err);
  return { default: () => <div>Error loading Dashboard. Please refresh.</div> };
}));
const Login = React.lazy(() => import('./pages/auth/Login').catch(err => {
  console.error('Failed to load Login:', err);
  return { default: () => <div>Error loading Login. Please refresh.</div> };
}));
const IncidentList = React.lazy(() => import('./pages/incidents/IncidentList').catch(err => {
  console.error('Failed to load IncidentList:', err);
  return { default: () => <div>Error loading Incident List. Please refresh.</div> };
}));
const CreateIncident = React.lazy(() => import('./pages/incidents/CreateIncident').catch(err => {
  console.error('Failed to load CreateIncident:', err);
  return { default: () => <div>Error loading Create Incident. Please refresh.</div> };
}));

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
          <Suspense fallback={<Loading />}>
            <Routes>
            {/* Auth Routes */}
            <Route element={<AuthLayout />}>
              <Route path="/login" element={<Login />} />
            </Route>
            
            {/* Protected Routes */}
            <Route element={<PrivateRoute><DefaultLayout /></PrivateRoute>}>
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<Dashboard />} />
              
              {/* Incident Management */}
              <Route path="/incidents" element={<IncidentList />} />
              <Route path="/incidents/create" element={<CreateIncident />} />
              <Route path="/incidents/my-reports" element={<IncidentList />} />
              
              {/* Profile & Settings (placeholder pages) */}
              <Route path="/profile" element={<div className="p-4"><h2>Profile Page</h2><p>Coming soon...</p></div>} />
              <Route path="/settings" element={<div className="p-4"><h2>Settings Page</h2><p>Coming soon...</p></div>} />
              
              {/* Catch all other routes and redirect to dashboard */}
              <Route path="*" element={<Navigate to="/dashboard" replace />} />
            </Route>
          </Routes>
        </Suspense>
      </BrowserRouter>
    </Provider>
    </ErrorBoundary>
  );
}

export default App;