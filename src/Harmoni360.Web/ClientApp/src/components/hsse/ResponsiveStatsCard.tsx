import React from 'react';
import { CCard, CCardBody, CCardHeader, CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { faArrowUp, faArrowDown } from '@fortawesome/free-solid-svg-icons';

interface ResponsiveStatsCardProps {
  title: string;
  value: number | string;
  icon?: IconDefinition;
  color?: string;
  trend?: {
    value: number;
    direction: 'up' | 'down';
    period: string;
  };
  subtitle?: string;
  className?: string;
  size?: 'sm' | 'md' | 'lg';
  isKpi?: boolean;
  target?: number;
  benchmark?: string;
}

const ResponsiveStatsCard: React.FC<ResponsiveStatsCardProps> = ({
  title,
  value,
  icon,
  color = 'primary',
  trend,
  subtitle,
  className = '',
  size = 'md',
  isKpi = false,
  target,
  benchmark,
}) => {
  const sizeClasses = {
    sm: 'stats-card-sm',
    md: 'stats-card-md',
    lg: 'stats-card-lg',
  };

  // Responsive sizing based on screen size
  const getResponsiveStyle = () => {
    const isMobile = window.innerWidth < 768;
    const isTablet = window.innerWidth >= 768 && window.innerWidth < 1024;
    
    if (isMobile) {
      return {
        sm: { minHeight: '100px' },
        md: { minHeight: '120px' },
        lg: { minHeight: '140px' },
      };
    } else if (isTablet) {
      return {
        sm: { minHeight: '120px' },
        md: { minHeight: '140px' },
        lg: { minHeight: '180px' },
      };
    } else {
      return {
        sm: { minHeight: '120px' },
        md: { minHeight: '160px' },
        lg: { minHeight: '200px' },
      };
    }
  };
  
  const responsiveStyle = getResponsiveStyle();

  const getTrendColor = (direction: 'up' | 'down') => {
    if (isKpi) {
      // For KPIs, up might be bad or good depending on the metric
      return direction === 'up' ? 'danger' : 'success';
    }
    return direction === 'up' ? 'danger' : 'success';
  };

  const getPerformanceColor = () => {
    if (!isKpi || !target || typeof value !== 'number') return color;
    
    const ratio = value / target;
    if (ratio <= 0.8) return 'success';
    if (ratio <= 1.0) return 'warning';
    if (ratio <= 1.2) return 'danger';
    return 'dark';
  };

  const performanceColor = isKpi ? getPerformanceColor() : color;

  // Responsive font sizing
  const getFontSizes = () => {
    const isMobile = window.innerWidth < 768;
    const isTablet = window.innerWidth >= 768 && window.innerWidth < 1024;
    
    if (isMobile) {
      return {
        title: size === 'lg' ? '0.9rem' : size === 'md' ? '0.85rem' : '0.8rem',
        value: size === 'lg' ? '1.75rem' : size === 'md' ? '1.5rem' : '1.25rem',
        subtitle: '0.75rem',
        badge: '0.7rem'
      };
    } else if (isTablet) {
      return {
        title: size === 'lg' ? '1rem' : size === 'md' ? '0.9rem' : '0.85rem',
        value: size === 'lg' ? '2rem' : size === 'md' ? '1.75rem' : '1.5rem',
        subtitle: '0.8rem',
        badge: '0.75rem'
      };
    } else {
      return {
        title: size === 'lg' ? '1.1rem' : size === 'md' ? '1rem' : '0.9rem',
        value: size === 'lg' ? '2.5rem' : size === 'md' ? '2rem' : '1.75rem',
        subtitle: '0.875rem',
        badge: '0.8rem'
      };
    }
  };
  
  const fontSizes = getFontSizes();

  return (
    <CCard 
      className={`${className} ${sizeClasses[size]} h-100 shadow-sm border-0`}
      style={responsiveStyle[size]}
    >
      <CCardHeader className={`bg-${performanceColor} text-white border-0 p-3`}>
        <div className="d-flex align-items-center justify-content-between">
          <div className="flex-grow-1">
            <h6 
              className="mb-0 text-white fw-bold"
              style={{ fontSize: fontSizes.title, lineHeight: '1.3' }}
            >
              {window.innerWidth < 576 && title.length > 20 ? `${title.substring(0, 20)}...` : title}
            </h6>
            {subtitle && (
              <small 
                className="text-white-50 d-block mt-1 d-none d-sm-block"
                style={{ fontSize: fontSizes.subtitle }}
              >
                {subtitle}
              </small>
            )}
          </div>
          {icon && (
            <div className="ms-2">
              <FontAwesomeIcon 
                icon={icon} 
                size="lg" 
                className="text-white-50" 
              />
            </div>
          )}
        </div>
      </CCardHeader>
      
      <CCardBody className="p-2 p-md-3 d-flex flex-column justify-content-between">
        <div className="text-center mb-2 mb-md-3">
          <div 
            className="fw-bold mb-2" 
            style={{ 
              fontSize: fontSizes.value,
              color: `var(--cui-${performanceColor})`,
              lineHeight: '1.1'
            }}
          >
            {typeof value === 'number' ? value.toLocaleString() : value}
          </div>
          
          {trend && (
            <div className="d-flex align-items-center justify-content-center mb-2">
              <CBadge 
                color={getTrendColor(trend.direction)} 
                className="px-2 py-1"
                style={{ fontSize: fontSizes.badge }}
              >
                <FontAwesomeIcon 
                  icon={trend.direction === 'up' ? faArrowUp : faArrowDown} 
                  className="me-1" 
                  size={window.innerWidth < 768 ? 'xs' : 'sm'}
                />
                {Math.abs(trend.value)}%
              </CBadge>
              <small 
                className="text-muted ms-2 d-none d-sm-inline"
                style={{ fontSize: fontSizes.subtitle }}
              >
                vs {trend.period}
              </small>
            </div>
          )}
        </div>
        
        {isKpi && target && (
          <div className="mt-auto">
            <div className="d-flex justify-content-between align-items-center mb-1">
              <small className="text-muted">Target:</small>
              <small className="fw-bold">{target.toFixed(2)}</small>
            </div>
            {benchmark && (
              <div className="text-center">
                <small className="text-muted">{benchmark}</small>
              </div>
            )}
          </div>
        )}
      </CCardBody>
    </CCard>
  );
};

export default ResponsiveStatsCard;