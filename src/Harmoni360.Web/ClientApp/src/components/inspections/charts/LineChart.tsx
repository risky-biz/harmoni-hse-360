import React from 'react';
import { CChart } from '@coreui/react-chartjs';

interface LineChartProps {
  data: {
    labels: string[];
    datasets: {
      label: string;
      data: number[];
      borderColor?: string;
      backgroundColor?: string;
      tension?: number;
      fill?: boolean;
    }[];
  };
  title?: string;
  height?: number;
}

export const LineChart: React.FC<LineChartProps> = ({ 
  data, 
  title, 
  height = 300
}) => {
  const options = {
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
        display: true,
        grid: {
          display: true,
          color: 'rgba(0, 0, 0, 0.1)'
        }
      },
      y: {
        display: true,
        beginAtZero: true,
        grid: {
          display: true,
          color: 'rgba(0, 0, 0, 0.1)'
        }
      }
    },
    interaction: {
      mode: 'nearest' as const,
      axis: 'x' as const,
      intersect: false
    },
    maintainAspectRatio: false,
    responsive: true,
    elements: {
      line: {
        tension: 0.3
      },
      point: {
        radius: 4,
        hoverRadius: 6
      }
    }
  };

  return (
    <div style={{ height }}>
      {title && (
        <h6 className="text-center mb-3 text-muted">{title}</h6>
      )}
      <CChart
        type="line"
        data={data}
        options={options}
      />
    </div>
  );
};