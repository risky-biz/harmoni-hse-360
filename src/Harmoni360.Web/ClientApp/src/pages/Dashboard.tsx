import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCompanyName } from '../contexts/CompanyConfigurationContext';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CAlert,
  CButton,
  CSpinner,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../utils/iconMappings';

import { useAuth } from '../hooks/useAuth';
import { useGetIncidentDashboardQuery } from '../features/incidents/incidentApi';
import DashboardWidget from '../components/dashboard/DashboardWidget';
import ApiUnavailableMessage from '../components/common/ApiUnavailableMessage';
import { DashboardWidget as WidgetConfig, WidgetData } from '../types/dashboard';
import { dashboardLayouts } from '../config/dashboardLayouts';
import { dashboardService } from '../services/dashboardService';

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const companyName = useCompanyName();
  const [widgetData, setWidgetData] = useState<Map<string, WidgetData>>(new Map());
  const [loadingWidgets, setLoadingWidgets] = useState<Set<string>>(new Set());
  const [errorWidgets, setErrorWidgets] = useState<Map<string, string>>(new Map());

  // Get dashboard data from API
  const { data: dashboardData, isLoading: statsLoading, error: statsError } = useGetIncidentDashboardQuery({});

  // Determine which dashboard layout to use based on user role
  const currentLayout = useMemo(() => {
    if (!user) return dashboardLayouts.find(layout => layout.id === 'default');
    
    // Map user roles to dashboard layouts
    const roleToLayout: Record<string, string> = {
      'Admin': 'executive',
      'HSE Manager': 'executive',
      'HSE Officer': 'safety-officer',
      'HSE Coordinator': 'safety-officer',
      'Department Head': 'safety-officer',
      'Employee': 'basic-user',
      'Contractor': 'basic-user',
    };

    const layoutId = roleToLayout[user.roles?.[0]] || 'default';
    return dashboardLayouts.find(layout => layout.id === layoutId) || 
           dashboardLayouts.find(layout => layout.id === 'default');
  }, [user]);

  const widgets = currentLayout?.widgets || [];

  // Load widget data
  const loadWidgetData = async (widget: WidgetConfig) => {
    try {
      setLoadingWidgets(prev => new Set(prev).add(widget.id));
      setErrorWidgets(prev => {
        const newMap = new Map(prev);
        newMap.delete(widget.id);
        return newMap;
      });

      const data = await dashboardService.getWidgetData(widget);
      if (data) {
        setWidgetData(prev => new Map(prev).set(widget.id, data));
      }
    } catch (error) {
      console.error(`Error loading widget ${widget.id}:`, error);
      setErrorWidgets(prev => new Map(prev).set(widget.id, error instanceof Error ? error.message : 'Unknown error'));
    } finally {
      setLoadingWidgets(prev => {
        const newSet = new Set(prev);
        newSet.delete(widget.id);
        return newSet;
      });
    }
  };

  // Initial data load
  useEffect(() => {
    widgets.forEach(widget => {
      loadWidgetData(widget);
    });
  }, [widgets]);

  // Auto-refresh for widgets that support it
  useEffect(() => {
    const intervals: NodeJS.Timeout[] = [];

    widgets.forEach(widget => {
      if (widget.config.autoRefresh && widget.refreshInterval) {
        const interval = setInterval(() => {
          loadWidgetData(widget);
        }, widget.refreshInterval * 1000);
        intervals.push(interval);
      }
    });

    return () => {
      intervals.forEach(interval => clearInterval(interval));
    };
  }, [widgets]);

  if (!user) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '50vh' }}>
        <CSpinner size="sm" />
      </div>
    );
  }

  return (
    <>
      <div className="mb-4">
        <h1>{currentLayout?.name || 'Dashboard'}</h1>
        <p className="text-medium-emphasis">
          Welcome back, {user?.name}! Here's your HSE overview.
        </p>
      </div>

      <CAlert color="info" className="d-flex align-items-center mb-4">
        <FontAwesomeIcon icon={CONTEXT_ICONS.vaccination} className="flex-shrink-0 me-2" />
        <div>
          <strong>Welcome to Harmoni360!</strong> This system manages health,
          safety, security and environmental data for {companyName}.
          <CButton
            color="primary"
            size="sm"
            className="ms-3"
            onClick={() => navigate('/incidents/create')}
          >
            <FontAwesomeIcon icon={ACTION_ICONS.add} size="sm" className="me-1" />
            Report Incident
          </CButton>
        </div>
      </CAlert>

      {statsError && (
        <ApiUnavailableMessage
          title="Unable to load dashboard statistics"
          message="The dashboard data could not be loaded from the backend API."
          onRefresh={() => window.location.reload()}
          className="mb-4"
        />
      )}

      {/* Modular Widget Dashboard */}
      <CRow>
        {widgets.map((widget) => (
          <DashboardWidget
            key={widget.id}
            widget={widget}
            data={widgetData.get(widget.id)}
            isLoading={loadingWidgets.has(widget.id)}
            error={errorWidgets.get(widget.id)}
            onRefresh={() => loadWidgetData(widget)}
          />
        ))}
      </CRow>

      {/* Dashboard Layout Info for Debug (remove in production) */}
      {process.env.NODE_ENV === 'development' && (
        <CRow className="mt-4">
          <CCol>
            <CCard>
              <CCardHeader>
                <small className="text-muted">
                  Debug: Current Layout - {currentLayout?.name} ({widgets.length} widgets)
                </small>
              </CCardHeader>
            </CCard>
          </CCol>
        </CRow>
      )}
    </>
  );
};

export default Dashboard;