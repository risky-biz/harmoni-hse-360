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
      plugins: {
        legend: {
          display: false,
        },
        tooltip: {
          enabled: false,
        },
      },
      rotation: -90,
      circumference: 180,
    };

    return { chartData, chartOptions, performanceLevel: level, performanceColor: color };
  }, [metric]);

  const sizeClasses = {
    sm: 'gauge-sm',
    md: 'gauge-md',
    lg: 'gauge-lg',
  };

  const heightStyle = {
    sm: { height: '120px' },
    md: { height: '160px' },
    lg: { height: '200px' },
  };

  return (
    <CCard className={`${className} ${sizeClasses[size]}`}>
      {showTitle && (
        <CCardHeader className="pb-2">
          <h6 className="mb-1">{metric.title}</h6>
          <small className="text-muted">{metric.description}</small>
        </CCardHeader>
      )}
      <CCardBody className="d-flex flex-column align-items-center justify-content-center">
        <div className="position-relative" style={heightStyle[size]}>
          <Doughnut data={chartData} options={chartOptions} />
          <div className="position-absolute top-50 start-50 translate-middle text-center">
            <div className="h4 mb-0 fw-bold" style={{ color: performanceColor }}>
              {metric.value.toFixed(2)}
            </div>
            <small className="text-muted">Target: {metric.target.toFixed(2)}</small>
          </div>
        </div>
        
        <div className="mt-3 text-center w-100">
          <CBadge color={performanceColor} className="px-3 py-2">
            {performanceLevel}
          </CBadge>
          {metric.benchmark && (
            <div className="mt-2">
              <small className="text-muted">{metric.benchmark}</small>
            </div>
          )}
        </div>
      </CCardBody>
    </CCard>
  );
};

export default KpiGaugeChart;