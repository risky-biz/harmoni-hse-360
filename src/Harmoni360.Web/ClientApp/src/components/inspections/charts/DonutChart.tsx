import React from 'react';
import { CChart } from '@coreui/react-chartjs';

interface DonutChartProps {
  data: {
    labels: string[];
    values: number[];
    colors?: string[];
  };
  title?: string;
  height?: number;
}

const DEFAULT_COLORS = [
  '#321fdb',
  '#39f',
  '#e55353',
  '#f9b115',
  '#2eb85c',
  '#6f42c1',
  '#d63384',
  '#fd7e14'
];

export const DonutChart: React.FC<DonutChartProps> = ({ 
  data, 
  title, 
  height = 300 
}) => {
  const chartData = {
    labels: data.labels,
    datasets: [
      {
        data: data.values,
        backgroundColor: data.colors || DEFAULT_COLORS.slice(0, data.labels.length),
        borderWidth: 2,
        borderColor: '#ffffff'
      }
    ]
  };

  const options = {
    plugins: {
      legend: {
        position: 'bottom' as const,
        labels: {
          usePointStyle: true,
          padding: 15
        }
      },
      tooltip: {
        callbacks: {
          label: (context: any) => {
            const total = context.dataset.data.reduce((sum: number, value: number) => sum + value, 0);
            const percentage = total > 0 ? ((context.raw / total) * 100).toFixed(1) : '0';
            return `${context.label}: ${context.raw} (${percentage}%)`;
          }
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
        type="doughnut"
        data={chartData}
        options={options}
      />
    </div>
  );
};