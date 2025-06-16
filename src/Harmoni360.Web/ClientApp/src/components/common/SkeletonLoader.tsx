import React from 'react';
import { CCard, CCardBody, CCardHeader } from '@coreui/react';

interface SkeletonLoaderProps {
  type: 'chart' | 'stats' | 'table' | 'gauge' | 'list';
  size?: 'sm' | 'md' | 'lg';
  showHeader?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

const SkeletonLoader: React.FC<SkeletonLoaderProps> = ({
  type,
  size = 'md',
  showHeader = true,
  className = '',
  style = {},
}) => {
  // Responsive height calculation
  const getResponsiveHeight = () => {
    const isMobile = window.innerWidth < 768;
    const isTablet = window.innerWidth >= 768 && window.innerWidth < 1024;
    
    const heights = {
      chart: {
        sm: isMobile ? 150 : isTablet ? 180 : 200,
        md: isMobile ? 200 : isTablet ? 250 : 300,
        lg: isMobile ? 250 : isTablet ? 350 : 400,
      },
      stats: {
        sm: isMobile ? 100 : isTablet ? 120 : 120,
        md: isMobile ? 120 : isTablet ? 140 : 160,
        lg: isMobile ? 140 : isTablet ? 180 : 200,
      },
      gauge: {
        sm: isMobile ? 120 : isTablet ? 140 : 160,
        md: isMobile ? 160 : isTablet ? 180 : 220,
        lg: isMobile ? 200 : isTablet ? 240 : 280,
      },
      table: {
        sm: 200,
        md: 300,
        lg: 400,
      },
      list: {
        sm: 150,
        md: 250,
        lg: 350,
      },
    };
    
    return heights[type][size];
  };

  const height = getResponsiveHeight();

  const renderHeaderSkeleton = () => (
    <CCardHeader className="pb-2">
      <div className="skeleton-loader mb-2" style={{ height: '20px', width: '60%', borderRadius: '4px' }} />
      <div className="skeleton-loader d-none d-md-block" style={{ height: '14px', width: '40%', borderRadius: '4px' }} />
    </CCardHeader>
  );

  const renderChartSkeleton = () => (
    <CCardBody className="p-2 p-md-3">
      <div 
        className="skeleton-loader"
        style={{ 
          height: `${height - (showHeader ? 80 : 20)}px`,
          borderRadius: '8px',
          position: 'relative'
        }}
      >
        {/* Chart axes simulation */}
        <div 
          style={{
            position: 'absolute',
            bottom: '20px',
            left: '30px',
            right: '20px',
            height: '1px',
            backgroundColor: 'rgba(0,0,0,0.1)'
          }}
        />
        <div 
          style={{
            position: 'absolute',
            bottom: '20px',
            left: '30px',
            top: '20px',
            width: '1px',
            backgroundColor: 'rgba(0,0,0,0.1)'
          }}
        />
      </div>
    </CCardBody>
  );

  const renderStatsSkeleton = () => (
    <CCardBody className="p-2 p-md-3 d-flex flex-column align-items-center justify-content-center">
      <div className="skeleton-loader mb-3" style={{ height: '40px', width: '80px', borderRadius: '4px' }} />
      <div className="skeleton-loader mb-2" style={{ height: '16px', width: '60px', borderRadius: '4px' }} />
      <div className="skeleton-loader" style={{ height: '24px', width: '80px', borderRadius: '12px' }} />
    </CCardBody>
  );

  const renderGaugeSkeleton = () => (
    <CCardBody className="p-2 p-md-3 d-flex flex-column align-items-center justify-content-center">
      <div 
        className="skeleton-loader mb-3" 
        style={{ 
          height: `${Math.min(height - 100, 200)}px`,
          width: `${Math.min(height - 100, 200)}px`,
          borderRadius: '50%',
          position: 'relative'
        }}
      >
        {/* Gauge center */}
        <div 
          style={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: '60%',
            height: '60%',
            backgroundColor: 'var(--cui-body-bg)',
            borderRadius: '50%'
          }}
        />
      </div>
      <div className="skeleton-loader" style={{ height: '24px', width: '80px', borderRadius: '12px' }} />
    </CCardBody>
  );

  const renderTableSkeleton = () => (
    <CCardBody className="p-2 p-md-3">
      {/* Table header */}
      <div className="d-flex mb-3">
        {[1, 2, 3, 4].map((i) => (
          <div 
            key={i}
            className="skeleton-loader me-3 flex-fill" 
            style={{ height: '16px', borderRadius: '4px' }} 
          />
        ))}
      </div>
      
      {/* Table rows */}
      {[1, 2, 3, 4, 5].map((row) => (
        <div key={row} className="d-flex mb-2">
          {[1, 2, 3, 4].map((col) => (
            <div 
              key={col}
              className="skeleton-loader me-3 flex-fill" 
              style={{ height: '14px', borderRadius: '4px' }} 
            />
          ))}
        </div>
      ))}
    </CCardBody>
  );

  const renderListSkeleton = () => (
    <CCardBody className="p-2 p-md-3">
      {[1, 2, 3, 4, 5].map((item) => (
        <div key={item} className="d-flex align-items-center mb-3">
          <div 
            className="skeleton-loader me-3" 
            style={{ 
              width: '40px', 
              height: '40px', 
              borderRadius: '50%',
              flexShrink: 0
            }} 
          />
          <div className="flex-grow-1">
            <div 
              className="skeleton-loader mb-2" 
              style={{ height: '16px', width: '70%', borderRadius: '4px' }} 
            />
            <div 
              className="skeleton-loader" 
              style={{ height: '12px', width: '50%', borderRadius: '4px' }} 
            />
          </div>
          <div 
            className="skeleton-loader" 
            style={{ width: '60px', height: '20px', borderRadius: '10px' }} 
          />
        </div>
      ))}
    </CCardBody>
  );

  const renderContent = () => {
    switch (type) {
      case 'chart':
        return renderChartSkeleton();
      case 'stats':
        return renderStatsSkeleton();
      case 'gauge':
        return renderGaugeSkeleton();
      case 'table':
        return renderTableSkeleton();
      case 'list':
        return renderListSkeleton();
      default:
        return renderChartSkeleton();
    }
  };

  return (
    <CCard 
      className={`${className} h-100`}
      style={{ 
        minHeight: `${height}px`,
        ...style
      }}
    >
      {showHeader && renderHeaderSkeleton()}
      {renderContent()}
    </CCard>
  );
};

export default SkeletonLoader;