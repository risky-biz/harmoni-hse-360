import React from 'react';
import { CChart } from '@coreui/react-chartjs';

interface BarChartProps {
  data: {
    labels: string[];
    datasets: {
      label: string;
      data: number[];
      backgroundColor?: string | string[];
      borderColor?: string | string[];
    }[];
  };
  title?: string;
  height?: number;
  horizontal?: boolean;
}

export const BarChart: React.FC<BarChartProps> = ({ 
  data, 
  title, 
  height = 300,
  horizontal = false
}) => {
  const options = {
    indexAxis: horizontal ? 'y' as const : 'x' as const,
    plugins: {
      legend: {
        position: 'top' as const,
        labels: {
          usePointStyle: true,
          padding: 15
        }
      },
      tooltip: {
        mode: 'index' as const,
        intersect: false
      }
    },
    scales: {
      x: {
        beginAtZero: true,
        grid: {
          display: true,
          color: 'rgba(0, 0, 0, 0.1)'
        }
      },
      y: {
        beginAtZero: true,
        grid: {
          display: true,
          color: 'rgba(0, 0, 0, 0.1)'
        }
      }
    },
    maintainAspectRatio: false,
    responsive: true
  };

  return (
    <div style={{ height }}>
      {title && (
        <h6 className="text-center mb-3 text-muted">{title}</h6>
      )}
      <CChart
        type="bar"
        data={data}
        options={options}
      />
    </div>
  );
};