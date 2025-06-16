import React, { useMemo } from 'react';
import { Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
  TooltipItem,
} from 'chart.js';
import { CCard, CCardBody, CCardHeader, CBadge } from '@coreui/react';
import { KpiMetric } from '../../types/hsse';

ChartJS.register(ArcElement, Tooltip, Legend);

interface KpiGaugeChartProps {
  metric: KpiMetric;
  size?: 'sm' | 'md' | 'lg';
  showTitle?: boolean;
  className?: string;
}

const KpiGaugeChart: React.FC<KpiGaugeChartProps> = ({
  metric,
  size = 'md',
  showTitle = true,
  className = '',
}) => {
  const { chartData, chartOptions, performanceLevel, performanceColor } = useMemo(() => {
    const { value, target, isGoodDirectionLow } = metric;
    
    // Calculate performance percentage
    const maxValue = Math.max(target * 1.5, value * 1.2);
    const percentage = Math.min((value / maxValue) * 100, 100);
    
    // Determine performance level
    let level: string;
    let color: string;
    
    if (isGoodDirectionLow) {
      // For metrics where lower is better (TRIR, LTIFR, Severity Rate)
      if (value <= target * 0.8) {
        level = 'Excellent';
        color = '#28a745';
      } else if (value <= target) {
        level = 'Good';
        color = '#ffc107';
      } else if (value <= target * 1.2) {
        level = 'Needs Improvement';
        color = '#fd7e14';
      } else {
        level = 'Critical';
        color = '#dc3545';
      }
    } else {
      // For metrics where higher is better (Compliance Rate)
      if (value >= target * 1.2) {
        level = 'Excellent';
        color = '#28a745';
      } else if (value >= target) {
        level = 'Good';
        color = '#ffc107';
      } else if (value >= target * 0.8) {
        level = 'Needs Improvement';
        color = '#fd7e14';
      } else {
        level = 'Critical';
        color = '#dc3545';
      }
    }

    const chartData = {
      datasets: [
        {
          data: [percentage, 100 - percentage],
          backgroundColor: [color, '#e9ecef'],
          borderWidth: 0,
          cutout: '70%',
        },
      ],
    };

    const chartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      devicePixelRatio: window.devicePixelRatio || 1,
      plugins: {
        legend: {
          display: false,
        },
        tooltip: {
          enabled: true,
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleColor: '#ffffff',
          bodyColor: '#ffffff',
          borderColor: color,
          borderWidth: 1,
          cornerRadius: 8,
          padding: 12,
          titleFont: {
            size: window.innerWidth < 768 ? 12 : 14,
            weight: 'bold' as const,
          },
          bodyFont: {
            size: window.innerWidth < 768 ? 11 : 13,
          },
          displayColors: false,
          callbacks: {
            title: () => metric.title,
            label: () => [
              `Current: ${value.toFixed(2)}`,
              `Target: ${target.toFixed(2)}`,
              `Performance: ${level}`,
            ],
          },
          // Touch-friendly tooltip
          intersect: false,
          mode: 'nearest' as const,
        },
      },
      rotation: -90,
      circumference: 180,
      // Enhanced interaction for touch devices
      interaction: {
        intersect: false,
        mode: 'nearest' as const,
      },
      // Animation configuration
      animation: {
        duration: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 0 : 1000,
        easing: 'easeOutQuart' as const,
      },
      // Responsive font scaling
      elements: {
        arc: {
          borderWidth: window.innerWidth < 768 ? 0 : 2,
          hoverBorderWidth: window.innerWidth < 768 ? 1 : 3,
        },
      },
    };

    return { chartData, chartOptions, performanceLevel: level, performanceColor: color };
  }, [metric]);

  const sizeClasses = {
    sm: 'gauge-sm',
    md: 'gauge-md',
    lg: 'gauge-lg',
  };

  // Responsive height calculation
  const getResponsiveHeight = () => {
    const isMobile = window.innerWidth < 768;
    const isTablet = window.innerWidth >= 768 && window.innerWidth < 1024;
    
    if (isMobile) {
      return { height: size === 'lg' ? '140px' : size === 'md' ? '120px' : '100px' };
    } else if (isTablet) {
      return { height: size === 'lg' ? '180px' : size === 'md' ? '150px' : '120px' };
    } else {
      return { height: size === 'lg' ? '200px' : size === 'md' ? '160px' : '120px' };
    }
  };

  return (
    <CCard className={`${className} ${sizeClasses[size]} h-100`}>
      {showTitle && (
        <CCardHeader className="pb-2">
          <h6 className="mb-1 d-none d-sm-block">{metric.title}</h6>
          <h6 className="mb-1 d-sm-none" style={{ fontSize: '0.9rem' }}>
            {metric.title.length > 20 ? `${metric.title.substring(0, 20)}...` : metric.title}
          </h6>
          <small className="text-muted d-none d-md-block">{metric.description}</small>
        </CCardHeader>
      )}
      <CCardBody className="d-flex flex-column align-items-center justify-content-center p-2 p-md-3">
        <div 
          className="position-relative w-100 d-flex justify-content-center" 
          style={getResponsiveHeight()}
        >
          <div style={{ width: '100%', maxWidth: '200px' }}>
            <Doughnut data={chartData} options={chartOptions} />
          </div>
          <div className="position-absolute top-50 start-50 translate-middle text-center">
            <div 
              className="fw-bold mb-0" 
              style={{ 
                color: performanceColor,
                fontSize: window.innerWidth < 768 ? '1.2rem' : window.innerWidth < 1024 ? '1.5rem' : '1.75rem'
              }}
            >
              {metric.value.toFixed(2)}
            </div>
            <small 
              className="text-muted d-none d-sm-block"
              style={{ fontSize: window.innerWidth < 768 ? '0.7rem' : '0.8rem' }}
            >
              Target: {metric.target.toFixed(2)}
            </small>
          </div>
        </div>
        
        <div className="mt-2 mt-md-3 text-center w-100">
          <CBadge 
            color={performanceColor} 
            className="px-2 px-md-3 py-1 py-md-2"
            style={{ fontSize: window.innerWidth < 768 ? '0.7rem' : '0.8rem' }}
          >
            {window.innerWidth < 576 && performanceLevel.length > 8 
              ? performanceLevel.substring(0, 8) + '...' 
              : performanceLevel}
          </CBadge>
          {metric.benchmark && (
            <div className="mt-1 mt-md-2 d-none d-md-block">
              <small className="text-muted">{metric.benchmark}</small>
            </div>
          )}
        </div>
      </CCardBody>
    </CCard>
  );
};

export default KpiGaugeChart;