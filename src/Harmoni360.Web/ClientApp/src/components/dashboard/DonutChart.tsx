import React from 'react';

interface DonutChartData {
  label: string;
  value: number;
  color?: string;
}

interface DonutChartProps {
  data: DonutChartData[];
  size?: number;
  strokeWidth?: number;
  showLegend?: boolean;
  className?: string;
}

const DonutChart: React.FC<DonutChartProps> = ({
  data,
  size = 200,
  strokeWidth = 20,
  showLegend = true,
  className = ''
}) => {
  const total = data.reduce((sum, item) => sum + item.value, 0);
  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  
  let cumulativePercentage = 0;

  if (total === 0) {
    return (
      <div className={`donut-chart ${className}`}>
        <div className="d-flex justify-content-center align-items-center" style={{ height: size }}>
          <span className="text-medium-emphasis">No data available</span>
        </div>
      </div>
    );
  }

  return (
    <div className={`donut-chart ${className}`}>
      <div className="d-flex justify-content-center">
        <svg width={size} height={size} className="donut-chart-svg">
          <circle
            cx={size / 2}
            cy={size / 2}
            r={radius}
            fill="transparent"
            stroke="#e5e5e5"
            strokeWidth={strokeWidth}
          />
          {data.map((item, index) => {
            const percentage = (item.value / total) * 100;
            const strokeDasharray = `${(percentage / 100) * circumference} ${circumference}`;
            const strokeDashoffset = -((cumulativePercentage / 100) * circumference);
            
            cumulativePercentage += percentage;
            
            return (
              <circle
                key={index}
                cx={size / 2}
                cy={size / 2}
                r={radius}
                fill="transparent"
                stroke={item.color}
                strokeWidth={strokeWidth}
                strokeDasharray={strokeDasharray}
                strokeDashoffset={strokeDashoffset}
                strokeLinecap="round"
                transform={`rotate(-90 ${size / 2} ${size / 2})`}
              />
            );
          })}
          
          {/* Center text */}
          <text
            x={size / 2}
            y={size / 2}
            textAnchor="middle"
            dy="0.3em"
            className="fw-bold"
            fontSize="24"
            fill="currentColor"
          >
            {total}
          </text>
          <text
            x={size / 2}
            y={size / 2 + 20}
            textAnchor="middle"
            dy="0.3em"
            className="text-medium-emphasis"
            fontSize="12"
            fill="currentColor"
          >
            Total
          </text>
        </svg>
      </div>
      
      {showLegend && (
        <div className="donut-chart-legend mt-3">
          {data.map((item, index) => (
            <div key={index} className="d-flex align-items-center mb-2">
              <div
                className="me-2"
                style={{
                  width: '12px',
                  height: '12px',
                  backgroundColor: item.color,
                  borderRadius: '2px'
                }}
              />
              <span className="small text-medium-emphasis me-2">{item.label}:</span>
              <span className="small fw-semibold">
                {item.value} ({((item.value / total) * 100).toFixed(1)}%)
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default DonutChart;