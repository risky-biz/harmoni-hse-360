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

    const chartOptions: ChartOptions<'line'> = {
      responsive: true,
      maintainAspectRatio: false,
      interaction: {
        mode: 'index' as const,
        intersect: false,
      },
      plugins: {
        title: {
          display: false,
        },
        legend: {
          display: showLegend,
          position: 'top' as const,
          labels: {
            padding: 20,
            usePointStyle: true,
            font: {
              size: 12,
            },
          },
        },
        tooltip: {
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          titleColor: '#333',
          bodyColor: '#666',
          borderColor: '#ddd',
          borderWidth: 1,
          cornerRadius: 8,
          displayColors: true,
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
            display: true,
            text: 'Time Period',
            font: {
              size: 12,
              weight: 'bold',
            },
          },
          grid: {
            display: false,
          },
        },
        y: {
          display: true,
          title: {
            display: true,
            text: 'Number of Incidents',
            font: {
              size: 12,
              weight: 'bold',
            },
          },
          beginAtZero: true,
          grid: {
            color: 'rgba(0, 0, 0, 0.1)',
          },
          ticks: {
            stepSize: 1,
          },
        },
      },
      elements: {
        point: {
          hoverBackgroundColor: '#fff',
          hoverBorderWidth: 2,
        },
      },
    };

    return { chartData, chartOptions };
  }, [data, showLegend]);

  if (!data || data.length === 0) {
    return (
      <CCard className={className}>
        <CCardHeader>
          <h5 className="mb-0">{title}</h5>
        </CCardHeader>
        <CCardBody className="d-flex align-items-center justify-content-center" style={{ height: `${height}px` }}>
          <div className="text-muted text-center">
            <p className="mb-0">No trend data available</p>
            <small>Data will appear here once incidents are recorded</small>
          </div>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <CCard className={className}>
      <CCardHeader>
        <h5 className="mb-0">{title}</h5>
        <small className="text-muted">
          Showing trends across all HSSE modules over time
        </small>
      </CCardHeader>
      <CCardBody>
        <div style={{ height: `${height}px` }}>
          <Line data={chartData} options={chartOptions} />
        </div>
      </CCardBody>
    </CCard>
  );
};

export default MultiLineTrendChart;