import React from 'react';
import { CCard, CCardBody, CCardHeader, CSpinner } from '@coreui/react';

interface ChartCardProps {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
  isLoading?: boolean;
  height?: string | number;
  className?: string;
  actions?: React.ReactNode;
}

const ChartCard: React.FC<ChartCardProps> = ({
  title,
  subtitle,
  children,
  isLoading = false,
  height = '300px',
  className = '',
  actions
}) => {
  return (
    <CCard className={`h-100 ${className}`}>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <div>
          <h5 className="card-title mb-0">{title}</h5>
          {subtitle && (
            <small className="text-medium-emphasis">{subtitle}</small>
          )}
        </div>
        {actions && <div>{actions}</div>}
      </CCardHeader>
      <CCardBody className="p-3">
        {isLoading ? (
          <div 
            className="d-flex justify-content-center align-items-center"
            style={{ height }}
          >
            <CSpinner color="primary" />
          </div>
        ) : (
          <div style={{ height }}>
            {children}
          </div>
        )}
      </CCardBody>
    </CCard>
  );
};

export default ChartCard;