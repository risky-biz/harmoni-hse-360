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

  const cardStyle = {
    sm: { minHeight: '120px' },
    md: { minHeight: '160px' },
    lg: { minHeight: '200px' },
  };

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

  return (
    <CCard 
      className={`${className} ${sizeClasses[size]} h-100 shadow-sm border-0`}
      style={cardStyle[size]}
    >
      <CCardHeader className={`bg-${performanceColor} text-white border-0 p-3`}>
        <div className="d-flex align-items-center justify-content-between">
          <div className="flex-grow-1">
            <h6 className="mb-0 text-white fw-bold">
              {title}
            </h6>
            {subtitle && (
              <small className="text-white-50 d-block mt-1">
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
      
      <CCardBody className="p-3 d-flex flex-column justify-content-between">
        <div className="text-center mb-3">
          <div className={`display-${size === 'lg' ? '4' : size === 'md' ? '5' : '6'} fw-bold text-${performanceColor} mb-2`}>
            {typeof value === 'number' ? value.toLocaleString() : value}
          </div>
          
          {trend && (
            <div className="d-flex align-items-center justify-content-center mb-2">
              <CBadge 
                color={getTrendColor(trend.direction)} 
                className="px-2 py-1"
              >
                <FontAwesomeIcon 
                  icon={trend.direction === 'up' ? faArrowUp : faArrowDown} 
                  className="me-1" 
                />
                {Math.abs(trend.value)}%
              </CBadge>
              <small className="text-muted ms-2">vs {trend.period}</small>
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