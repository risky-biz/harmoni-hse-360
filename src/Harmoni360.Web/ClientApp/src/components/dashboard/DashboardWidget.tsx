import React, { useState, useEffect, useCallback } from 'react';
import { CCard, CCardBody, CCardHeader, CSpinner, CButton, CAlert } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faRefresh, faEllipsisV } from '@fortawesome/free-solid-svg-icons';
import { DashboardWidget as WidgetConfig, WidgetData } from '../../types/dashboard';
import { usePermissions } from '../../hooks/usePermissions';

// Import specific widget components
import StatsCard from './StatsCard';
import ChartCard from './ChartCard';
import DonutChart from './DonutChart';
import BarChart from './BarChart';
import LineChart from './LineChart';
import RecentItemsList from './RecentItemsList';
import ProgressCard from './ProgressCard';
import QuickActionsWidget from './QuickActionsWidget';
import AlertBannerWidget from './AlertBannerWidget';

interface DashboardWidgetProps {
  widget: WidgetConfig;
  data?: WidgetData;
  isLoading?: boolean;
  error?: string;
  onRefresh?: () => void;
  onRemove?: () => void;
  onConfigure?: () => void;
  className?: string;
}

const DashboardWidget: React.FC<DashboardWidgetProps> = ({
  widget,
  data,
  isLoading = false,
  error,
  onRefresh,
  onRemove,
  onConfigure,
  className = '',
}) => {
  const permissions = usePermissions();
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Check if user has permission to view this widget
  const hasPermission = !widget.permissions || 
    widget.permissions.every(permission => permissions.permissions.includes(permission));

  if (!hasPermission) {
    return null;
  }

  // Only hide if explicitly set to false
  if (widget.isVisible === false) {
    return null;
  }

  const handleRefresh = useCallback(async () => {
    if (onRefresh && !isRefreshing) {
      setIsRefreshing(true);
      try {
        await onRefresh();
      } finally {
        setIsRefreshing(false);
      }
    }
  }, [onRefresh, isRefreshing]);

  // Auto-refresh functionality
  useEffect(() => {
    if (widget.config.autoRefresh && widget.refreshInterval && onRefresh) {
      const interval = setInterval(handleRefresh, widget.refreshInterval * 1000);
      return () => clearInterval(interval);
    }
  }, [widget.config.autoRefresh, widget.refreshInterval, handleRefresh]);

  const getWidgetSize = () => {
    switch (widget.size) {
      case 'small': return 'col-lg-3 col-md-6';
      case 'medium': return 'col-lg-6 col-md-12';
      case 'large': return 'col-lg-9 col-md-12';
      case 'full': return 'col-12';
      default: return 'col-lg-6 col-md-12';
    }
  };

  const renderWidgetContent = () => {
    if (error) {
      return (
        <CAlert color="warning" className="m-0">
          <small>Error loading widget: {error}</small>
        </CAlert>
      );
    }

    if (isLoading || isRefreshing) {
      return (
        <div className="d-flex justify-content-center align-items-center p-4">
          <CSpinner size="sm" className="me-2" />
          <span>Loading...</span>
        </div>
      );
    }

    switch (widget.type) {
      case 'stats-card':
        return (
          <StatsCard
            title={widget.title}
            value={data?.stats?.value || 0}
            subtitle={data?.stats?.subtitle}
            trend={data?.stats?.trend}
            color={widget.config.styling?.color as any}
            onClick={widget.config.clickAction?.type === 'navigate' ? 
              () => window.location.href = widget.config.clickAction?.target || '#' : undefined}
            className="h-100"
          />
        );

      case 'chart-donut':
        return (
          <ChartCard 
            title={widget.title}
            subtitle={widget.subtitle}
            isLoading={isLoading}
          >
            <DonutChart data={data?.chartData?.map(item => ({ 
              label: item.label, 
              value: item.value, 
              color: item.color || '#000' 
            })) || []} />
          </ChartCard>
        );

      case 'chart-bar':
        return (
          <ChartCard 
            title={widget.title}
            subtitle={widget.subtitle}
            isLoading={isLoading}
          >
            <BarChart data={data?.chartData || []} />
          </ChartCard>
        );

      case 'chart-line':
        return (
          <ChartCard 
            title={widget.title}
            subtitle={widget.subtitle}
            isLoading={isLoading}
          >
            <LineChart data={data?.timeSeriesData?.map(item => ({ 
              label: item.date || 'N/A', 
              value: item.value || 0 
            })) || []} />
          </ChartCard>
        );

      case 'recent-items':
        return (
          <CCard className="h-100">
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">{widget.title}</h5>
              {widget.config.clickAction?.type === 'navigate' && (
                <CButton
                  size="sm"
                  color="primary"
                  variant="outline"
                  onClick={() => window.location.href = widget.config.clickAction?.target || '#'}
                >
                  View All
                </CButton>
              )}
            </CCardHeader>
            <CCardBody>
              <RecentItemsList 
                items={data?.listData || []}
                maxItems={widget.config.customProps?.maxItems || 5}
                showTimestamp={widget.config.customProps?.showTimestamp}
              />
            </CCardBody>
          </CCard>
        );

      case 'quick-actions':
        return <QuickActionsWidget title={widget.title} />;

      case 'progress-card':
        return (
          <ProgressCard
            title={widget.title}
            value={data?.stats?.value as number || 0}
            total={100}
            percentage={data?.stats?.value as number || 0}
            color={widget.config.styling?.color as any}
            description={data?.stats?.subtitle}
          />
        );

      case 'alert-banner':
        return (
          <AlertBannerWidget
            title={widget.title}
            message={data?.customData?.message || ''}
            color={widget.config.styling?.color as any}
            data={data?.customData}
          />
        );

      case 'custom':
        // Handle custom components
        const CustomComponent = widget.config.customProps?.component;
        if (CustomComponent) {
          return (
            <CCard className="h-100">
              <CCardHeader>
                <h5 className="mb-0">{widget.title}</h5>
              </CCardHeader>
              <CCardBody>
                <CustomComponent data={data} config={widget.config} />
              </CCardBody>
            </CCard>
          );
        }
        return <div>Custom widget not implemented</div>;

      default:
        return <div>Unknown widget type: {widget.type}</div>;
    }
  };

  // For stats cards, render without wrapper
  if (widget.type === 'stats-card') {
    return (
      <div className={`${getWidgetSize()} mb-4 ${className}`}>
        {renderWidgetContent()}
      </div>
    );
  }

  // For other widgets, render with card wrapper and controls
  return (
    <div className={`${getWidgetSize()} mb-4 ${className}`}>
      <CCard className="h-100">
        <CCardHeader className="d-flex justify-content-between align-items-center">
          <div>
            <h5 className="mb-0">{widget.title}</h5>
            {widget.subtitle && (
              <small className="text-medium-emphasis">{widget.subtitle}</small>
            )}
          </div>
          <div className="d-flex align-items-center gap-1">
            {onRefresh && (
              <CButton
                size="sm"
                color="ghost"
                onClick={handleRefresh}
                disabled={isRefreshing}
              >
                <FontAwesomeIcon 
                  icon={faRefresh} 
                  className={isRefreshing ? 'fa-spin' : ''} 
                />
              </CButton>
            )}
            {(onConfigure || onRemove) && (
              <CButton size="sm" color="ghost">
                <FontAwesomeIcon icon={faEllipsisV} />
              </CButton>
            )}
          </div>
        </CCardHeader>
        <CCardBody className="p-0">
          {renderWidgetContent()}
        </CCardBody>
        {data?.lastUpdated && (
          <div className="card-footer text-muted small">
            Last updated: {new Date(data.lastUpdated).toLocaleTimeString()}
          </div>
        )}
      </CCard>
    </div>
  );
};

export default DashboardWidget;