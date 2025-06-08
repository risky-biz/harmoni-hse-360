import React from 'react';
import { CCard, CCardBody, CProgress, CSpinner } from '@coreui/react';

interface ProgressCardProps {
  title: string;
  value: number;
  total: number;
  percentage: number;
  color?: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info';
  size?: 'sm' | 'lg';
  showValue?: boolean;
  isLoading?: boolean;
  className?: string;
  description?: string;
}

const ProgressCard: React.FC<ProgressCardProps> = ({
  title,
  value,
  total,
  percentage,
  color = 'primary',
  size,
  showValue = true,
  isLoading = false,
  className = '',
  description
}) => {
  return (
    <CCard className={`h-100 ${className}`}>
      <CCardBody>
        <div className="d-flex justify-content-between align-items-center mb-2">
          <h6 className="card-title mb-0">{title}</h6>
          {showValue && !isLoading && (
            <span className="text-medium-emphasis small">
              {value}/{total}
            </span>
          )}
        </div>
        
        {isLoading ? (
          <div className="d-flex justify-content-center py-3">
            <CSpinner size="sm" color={color} />
          </div>
        ) : (
          <>
            <CProgress 
              color={color} 
              value={isNaN(percentage) ? 0 : percentage} 
              className="mb-2"
              height={size === 'sm' ? 6 : size === 'lg' ? 12 : 8}
            />
            <div className="d-flex justify-content-between align-items-center">
              <span className={`fw-semibold text-${color}`}>
                {isNaN(percentage) ? '0.0' : percentage.toFixed(1)}%
              </span>
              {description && (
                <small className="text-medium-emphasis">{description}</small>
              )}
            </div>
          </>
        )}
      </CCardBody>
    </CCard>
  );
};

export default ProgressCard;