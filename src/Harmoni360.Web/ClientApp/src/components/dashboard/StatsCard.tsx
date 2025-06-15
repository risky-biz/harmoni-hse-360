import React from 'react';
import { CCard, CCardBody, CSpinner } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowUp, faArrowDown } from '@fortawesome/free-solid-svg-icons';

interface StatsCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: any;
  color?: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info' | 'dark';
  trend?: {
    value: number;
    isPositive: boolean;
    label: string;
  };
  /**
   * Temporary backwards compatibility property.
   * @deprecated use `trend` instead
   */
  change?: number;
  isLoading?: boolean;
  onClick?: () => void;
  className?: string;
}

const StatsCard: React.FC<StatsCardProps> = ({
  title,
  value,
  subtitle,
  icon,
  color = 'primary',
  trend,
  change,
  isLoading = false,
  onClick,
  className = ''
}) => {
  const cardClass = `stats-card ${onClick ? 'clickable' : ''} ${className}`;
  const colorClass = `border-start border-start-4 border-${color}`;
  const finalTrend = trend ?? (change !== undefined ? {
    value: change,
    isPositive: change >= 0,
    label: ''
  } : undefined);

  return (
    <CCard 
      className={`${cardClass} ${colorClass} h-100`}
      style={onClick ? { cursor: 'pointer' } : undefined}
      onClick={onClick}
    >
      <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
        <div className="flex-grow-1">
          <div className={`text-medium-emphasis small text-uppercase fw-semibold mb-2`}>
            {title}
          </div>
          <div className="fs-4 fw-semibold">
            {isLoading ? (
              <CSpinner size="sm" color={color} />
            ) : (
              <>
                {value}
                {subtitle && (
                  <span className="fs-6 ms-2 fw-normal text-medium-emphasis">
                    {subtitle}
                  </span>
                )}
              </>
            )}
          </div>
          {finalTrend && !isLoading && (
            <div className="text-medium-emphasis small mt-1">
              <span className={`fw-semibold ${finalTrend.isPositive ? 'text-success' : 'text-danger'}`}>
                <FontAwesomeIcon 
                  icon={trend.isPositive ? faArrowUp : faArrowDown} 
                  size="sm" 
                  className="me-1" 
                />
                {Math.abs(finalTrend.value)}%
              </span>
              <span className="ms-1">{finalTrend.label}</span>
            </div>
          )}
        </div>
        {icon && (
          <div className={`text-${color} opacity-75`}>
            <FontAwesomeIcon icon={icon} size="2x" />
          </div>
        )}
      </CCardBody>
    </CCard>
  );
};

export default StatsCard;
