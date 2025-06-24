import React from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer } from 'recharts';
import { CCard, CCardBody, CCardHeader, CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTachometerAlt, faBullseye, faArrowUp, faArrowDown } from '@fortawesome/free-solid-svg-icons';

interface GaugeChartProps {
  title: string;
  value: number;
  target: number;
  unit?: string;
  description?: string;
  benchmark?: string;
  isGoodDirectionLow?: boolean; // true for metrics where lower is better (TRIR, LTIFR)
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  showBenchmark?: boolean;
}

const getSizeConfig = (size: 'sm' | 'md' | 'lg') => {
  switch (size) {
    case 'sm':
      return { height: 200, innerRadius: 60, outerRadius: 90, fontSize: '14px', titleSize: '16px' };
    case 'md':
      return { height: 250, innerRadius: 70, outerRadius: 110, fontSize: '16px', titleSize: '18px' };
    case 'lg':
      return { height: 300, innerRadius: 80, outerRadius: 130, fontSize: '18px', titleSize: '20px' };
    default:
      return { height: 250, innerRadius: 70, outerRadius: 110, fontSize: '16px', titleSize: '18px' };
  }
};

const getPerformanceStatus = (value: number, target: number, isGoodDirectionLow: boolean) => {
  const percentage = (value / target) * 100;
  
  if (isGoodDirectionLow) {
    // For metrics where lower is better (TRIR, LTIFR, etc.)
    if (percentage <= 80) return { status: 'excellent', color: '#28a745', label: 'Excellent' };
    if (percentage <= 100) return { status: 'good', color: '#20c997', label: 'Good' };
    if (percentage <= 120) return { status: 'warning', color: '#ffc107', label: 'Needs Attention' };
    return { status: 'danger', color: '#dc3545', label: 'Critical' };
  } else {
    // For metrics where higher is better (Compliance Rate, etc.)
    if (percentage >= 100) return { status: 'excellent', color: '#28a745', label: 'Excellent' };
    if (percentage >= 95) return { status: 'good', color: '#20c997', label: 'Good' };
    if (percentage >= 85) return { status: 'warning', color: '#ffc107', label: 'Needs Attention' };
    return { status: 'danger', color: '#dc3545', label: 'Critical' };
  }
};

const GaugeChart: React.FC<GaugeChartProps> = ({
  title,
  value,
  target,
  unit = '',
  description = '',
  benchmark = '',
  isGoodDirectionLow = true,
  size = 'md',
  className = '',
  showBenchmark = true
}) => {
  const sizeConfig = getSizeConfig(size);
  const performance = getPerformanceStatus(value, target, isGoodDirectionLow);
  const percentage = Math.min((value / target) * 100, 150); // Cap at 150% for display
  
  // Create gauge data - semicircle gauge
  const gaugeData = [
    { value: percentage, fill: performance.color },
    { value: 100 - percentage, fill: '#e9ecef' }
  ];
  
  const fullCircleData = [
    { value: percentage, fill: performance.color },
    { value: 100, fill: '#e9ecef' }
  ];

  // Calculate needle angle (180 degrees = semicircle)
  const needleAngle = -90 + (percentage * 1.8); // -90 to start from left, 1.8 for 180 degree range
  
  const trendIcon = isGoodDirectionLow 
    ? (value <= target ? faArrowDown : faArrowUp)
    : (value >= target ? faArrowUp : faArrowDown);
    
  const trendColor = isGoodDirectionLow 
    ? (value <= target ? 'success' : 'danger')
    : (value >= target ? 'success' : 'danger');

  return (
    <CCard className={`h-100 text-center ${className}`}>
      <CCardHeader className="pb-2">
        <h6 className="mb-1" style={{ fontSize: sizeConfig.titleSize }}>
          <FontAwesomeIcon icon={faTachometerAlt} className="me-2 text-primary" />
          {title}
        </h6>
        {description && (
          <small className="text-muted">{description}</small>
        )}
      </CCardHeader>
      <CCardBody className="d-flex flex-column justify-content-center">
        <div style={{ height: sizeConfig.height, position: 'relative' }}>
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={fullCircleData}
                cx="50%"
                cy="80%"
                startAngle={180}
                endAngle={0}
                innerRadius={sizeConfig.innerRadius}
                outerRadius={sizeConfig.outerRadius}
                dataKey="value"
                stroke="none"
              >
                <Cell fill="#e9ecef" />
                <Cell fill={performance.color} />
              </Pie>
            </PieChart>
          </ResponsiveContainer>
          
          {/* Value Display */}
          <div 
            style={{
              position: 'absolute',
              top: '60%',
              left: '50%',
              transform: 'translate(-50%, -50%)',
              textAlign: 'center'
            }}
          >
            <div 
              className="fw-bold mb-1"
              style={{ 
                fontSize: sizeConfig.fontSize,
                color: performance.color
              }}
            >
              {value.toFixed(2)}{unit}
            </div>
            <CBadge color={performance.status} className="mb-1">
              {performance.label}
            </CBadge>
          </div>
          
          {/* Needle */}
          <div
            style={{
              position: 'absolute',
              top: '80%',
              left: '50%',
              width: '2px',
              height: `${sizeConfig.outerRadius - 20}px`,
              backgroundColor: '#333',
              transformOrigin: 'bottom center',
              transform: `translate(-50%, -100%) rotate(${needleAngle}deg)`,
              zIndex: 10
            }}
          />
          
          {/* Center dot */}
          <div
            style={{
              position: 'absolute',
              top: '80%',
              left: '50%',
              width: '8px',
              height: '8px',
              backgroundColor: '#333',
              borderRadius: '50%',
              transform: 'translate(-50%, -50%)',
              zIndex: 11
            }}
          />
        </div>
        
        {/* Target and Benchmark Info */}
        <div className="mt-3">
          <div className="d-flex justify-content-between align-items-center mb-2">
            <div className="text-start">
              <FontAwesomeIcon icon={faBullseye} className="me-1 text-info" />
              <span className="small">Target: {target.toFixed(1)}{unit}</span>
            </div>
            <CBadge color={trendColor} className="d-flex align-items-center">
              <FontAwesomeIcon icon={trendIcon} className="me-1" size="xs" />
              {((value / target) * 100).toFixed(1)}%
            </CBadge>
          </div>
          
          {showBenchmark && benchmark && (
            <div className="text-center">
              <small className="text-muted">{benchmark}</small>
            </div>
          )}
          
          {/* Performance Scale */}
          <div className="mt-2">
            <div className="d-flex justify-content-between small text-muted">
              <span>0</span>
              <span>Target</span>
              <span>1.5x</span>
            </div>
            <div className="progress" style={{ height: '4px' }}>
              <div 
                className="progress-bar"
                style={{ 
                  width: `${Math.min(percentage, 100)}%`,
                  backgroundColor: performance.color
                }}
              />
            </div>
          </div>
        </div>
      </CCardBody>
    </CCard>
  );
};

export default GaugeChart;