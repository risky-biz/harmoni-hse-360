import React, { useMemo } from 'react';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ChartOptions,
} from 'chart.js';
import { CCard, CCardBody, CCardHeader } from '@coreui/react';
import { HsseTrendPointDto } from '../../types/hsse';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface MultiLineTrendChartProps {
  data: HsseTrendPointDto[];
  title?: string;
  height?: number;
  showLegend?: boolean;
  className?: string;
}

const MultiLineTrendChart: React.FC<MultiLineTrendChartProps> = ({
  data,
  title = 'HSSE Trend Analysis',
  height = 400,
  showLegend = true,
  className = '',
}) => {
  const { chartData, chartOptions } = useMemo(() => {
    const labels = data.map(item => item.periodLabel);
    
    const chartData = {
      labels,
      datasets: [
        {
          label: 'Safety Incidents',
          data: data.map(item => item.incidentCount),
          borderColor: '#dc3545',
          backgroundColor: 'rgba(220, 53, 69, 0.1)',
          borderWidth: 2,
          fill: false,
          tension: 0.2,
          pointRadius: 4,
          pointHoverRadius: 6,
        },
        {
          label: 'Hazards',
          data: data.map(item => item.hazardCount),
          borderColor: '#fd7e14',
          backgroundColor: 'rgba(253, 126, 20, 0.1)',
          borderWidth: 2,
          fill: false,
          tension: 0.2,
          pointRadius: 4,
          pointHoverRadius: 6,
        },
        {
          label: 'Security Incidents',
          data: data.map(item => item.securityIncidentCount),
          borderColor: '#6f42c1',
          backgroundColor: 'rgba(111, 66, 193, 0.1)',
          borderWidth: 2,
          fill: false,
          tension: 0.2,
          pointRadius: 4,
          pointHoverRadius: 6,
        },
        {
          label: 'Health Incidents',
          data: data.map(item => item.healthIncidentCount),
          borderColor: '#20c997',
          backgroundColor: 'rgba(32, 201, 151, 0.1)',
          borderWidth: 2,
          fill: false,
          tension: 0.2,
          pointRadius: 4,
          pointHoverRadius: 6,
        },
      ],
    };

    // Responsive chart configuration
    const isMobile = window.innerWidth < 768;
    const isTablet = window.innerWidth >= 768 && window.innerWidth < 1024;
    
    const chartOptions: ChartOptions<'line'> = {
      responsive: true,
      maintainAspectRatio: false,
      devicePixelRatio: window.devicePixelRatio || 1,
      interaction: {
        mode: 'index' as const,
        intersect: false,
      },
      plugins: {
        title: {
          display: false,
        },
        legend: {
          display: showLegend && !isMobile, // Hide legend on mobile to save space
          position: isMobile ? 'bottom' as const : 'top' as const,
          labels: {
            padding: isMobile ? 10 : 20,
            usePointStyle: true,
            font: {
              size: isMobile ? 10 : isTablet ? 11 : 12,
            },
            boxWidth: isMobile ? 8 : 12,
            boxHeight: isMobile ? 8 : 12,
          },
        },
        tooltip: {
          enabled: true,
          backgroundColor: 'rgba(0, 0, 0, 0.85)',
          titleColor: '#ffffff',
          bodyColor: '#ffffff',
          borderColor: '#666',
          borderWidth: 1,
          cornerRadius: 8,
          displayColors: true,
          padding: isMobile ? 8 : 12,
          titleFont: {
            size: isMobile ? 11 : 13,
            weight: 'bold',
          },
          bodyFont: {
            size: isMobile ? 10 : 12,
          },
          // Touch-friendly tooltip
          intersect: false,
          mode: 'nearest' as const,
          callbacks: {
            title: (context) => {
              return `Period: ${context[0].label}`;
            },
            label: (context) => {
              return `${context.dataset.label}: ${context.parsed.y} incidents`;
            },
          },
        },
      },
      scales: {
        x: {
          display: true,
          title: {
            display: !isMobile, // Hide axis titles on mobile
            text: 'Time Period',
            font: {
              size: isMobile ? 10 : isTablet ? 11 : 12,
              weight: 'bold',
            },
          },
          grid: {
            display: false,
          },
          ticks: {
            font: {
              size: isMobile ? 9 : isTablet ? 10 : 11,
            },
            maxRotation: isMobile ? 45 : 0, // Rotate labels on mobile
            minRotation: isMobile ? 45 : 0,
            maxTicksLimit: isMobile ? 6 : isTablet ? 8 : 12, // Limit ticks on smaller screens
          },
        },
        y: {
          display: true,
          title: {
            display: !isMobile, // Hide axis titles on mobile
            text: 'Number of Incidents',
            font: {
              size: isMobile ? 10 : isTablet ? 11 : 12,
              weight: 'bold',
            },
          },
          beginAtZero: true,
          grid: {
            color: 'rgba(0, 0, 0, 0.1)',
          },
          ticks: {
            stepSize: 1,
            font: {
              size: isMobile ? 9 : isTablet ? 10 : 11,
            },
            maxTicksLimit: isMobile ? 5 : 8, // Limit y-axis ticks on mobile
          },
        },
      },
      elements: {
        point: {
          radius: isMobile ? 3 : 4,
          hoverRadius: isMobile ? 5 : 6,
          hoverBackgroundColor: '#fff',
          hoverBorderWidth: isMobile ? 1 : 2,
          borderWidth: isMobile ? 1 : 2,
        },
        line: {
          borderWidth: isMobile ? 2 : 3,
        },
      },
      // Animation configuration
      animation: {
        duration: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 0 : 1000,
        easing: 'easeOutQuart' as const,
      },
    };

    return { chartData, chartOptions };
  }, [data, showLegend]);

  // Responsive height calculation
  const getResponsiveHeight = () => {
    const isMobile = window.innerWidth < 768;
    const isTablet = window.innerWidth >= 768 && window.innerWidth < 1024;
    
    if (isMobile) {
      return Math.min(height * 0.7, 250); // Reduce height on mobile
    } else if (isTablet) {
      return Math.min(height * 0.85, 350); // Slightly reduce on tablet
    } else {
      return height;
    }
  };

  if (!data || data.length === 0) {
    return (
      <CCard className={`${className} h-100`}>
        <CCardHeader className="pb-2">
          <h5 className="mb-0 d-none d-sm-block">{title}</h5>
          <h6 className="mb-0 d-sm-none">{title.length > 25 ? `${title.substring(0, 25)}...` : title}</h6>
        </CCardHeader>
        <CCardBody className="d-flex align-items-center justify-content-center p-2 p-md-3" style={{ height: `${getResponsiveHeight()}px` }}>
          <div className="text-muted text-center">
            <p className="mb-1 fs-6">No trend data available</p>
            <small className="d-none d-sm-block">Data will appear here once incidents are recorded</small>
          </div>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <CCard className={`${className} h-100`}>
      <CCardHeader className="pb-2">
        <h5 className="mb-0 d-none d-sm-block">{title}</h5>
        <h6 className="mb-0 d-sm-none">{title.length > 25 ? `${title.substring(0, 25)}...` : title}</h6>
        <small className="text-muted d-none d-md-block">
          Showing trends across all HSSE modules over time
        </small>
      </CCardHeader>
      <CCardBody className="p-2 p-md-3">
        <div style={{ height: `${getResponsiveHeight()}px`, position: 'relative' }}>
          <Line data={chartData} options={chartOptions} />
        </div>
        {/* Mobile legend when chart legend is hidden */}
        {window.innerWidth < 768 && showLegend && (
          <div className="mt-2 d-flex flex-wrap justify-content-center gap-2">
            {chartData.datasets.map((dataset, index) => (
              <small key={index} className="d-flex align-items-center">
                <span 
                  className="me-1 rounded" 
                  style={{ 
                    width: '10px', 
                    height: '10px', 
                    backgroundColor: dataset.borderColor,
                    display: 'inline-block'
                  }}
                ></span>
                {dataset.label}
              </small>
            ))}
          </div>
        )}
      </CCardBody>
    </CCard>
  );
};

export default MultiLineTrendChart;