import React from 'react';

interface BarChartData {
  label: string;
  value: number;
  color?: string;
}

interface BarChartProps {
  data: BarChartData[];
  height?: number;
  maxValue?: number;
  showValues?: boolean;
  horizontal?: boolean;
  className?: string;
}

const BarChart: React.FC<BarChartProps> = ({
  data,
  height = 300,
  maxValue,
  showValues = true,
  horizontal = false,
  className = ''
}) => {
  const chartMaxValue = maxValue || Math.max(...data.map(d => d.value)) || 1;
  const barColors = ['var(--cui-primary)', 'var(--cui-info)', 'var(--cui-success)', 'var(--cui-warning)', 'var(--cui-danger)'];

  if (data.length === 0) {
    return (
      <div className={`bar-chart ${className}`} style={{ height }}>
        <div className="d-flex justify-content-center align-items-center h-100">
          <span className="text-medium-emphasis">No data available</span>
        </div>
      </div>
    );
  }

  if (horizontal) {
    return (
      <div className={`bar-chart horizontal ${className}`}>
        {data.map((item, index) => {
          const percentage = (item.value / chartMaxValue) * 100;
          const color = item.color || barColors[index % barColors.length];
          
          return (
            <div key={index} className="mb-3">
              <div className="d-flex justify-content-between align-items-center mb-1">
                <span className="small fw-medium">{item.label}</span>
                {showValues && <span className="small text-medium-emphasis">{item.value}</span>}
              </div>
              <div className="progress" style={{ height: '8px' }}>
                <div
                  className="progress-bar"
                  style={{
                    width: `${percentage}%`,
                    backgroundColor: color
                  }}
                />
              </div>
            </div>
          );
        })}
      </div>
    );
  }

  const barWidth = 100 / data.length;
  const chartPadding = Math.min(40, (100 / data.length) * 0.4); // Adaptive padding
  const labelHeight = 30;

  return (
    <div className={`bar-chart vertical ${className}`}>
      <svg width="100%" height={height} className="bar-chart-svg">
        {/* Chart area */}
        <g transform={`translate(${chartPadding}, 10)`}>
          {data.map((item, index) => {
            const barHeight = Math.max(0, ((item.value / chartMaxValue) * (height - chartPadding - labelHeight)));
            const availableWidth = Math.max(10, (100 - 2 * chartPadding));
            const x = (index * barWidth * availableWidth) / 100;
            const y = height - chartPadding - labelHeight - barHeight;
            const color = item.color || barColors[index % barColors.length];
            const rectWidth = Math.max(2, (barWidth * availableWidth) / 100 - 4);
            
            return (
              <g key={index}>
                {/* Bar */}
                <rect
                  x={x}
                  y={y}
                  width={rectWidth}
                  height={barHeight}
                  fill={color}
                  rx="2"
                />
                
                {/* Value label */}
                {showValues && (
                  <text
                    x={x + rectWidth / 2}
                    y={y - 5}
                    textAnchor="middle"
                    fontSize="12"
                    fill="currentColor"
                    className="text-medium-emphasis"
                  >
                    {item.value}
                  </text>
                )}
                
                {/* X-axis label */}
                <text
                  x={x + rectWidth / 2}
                  y={height - chartPadding + 15}
                  textAnchor="middle"
                  fontSize="11"
                  fill="currentColor"
                  className="text-medium-emphasis"
                >
                  {item.label}
                </text>
              </g>
            );
          })}
          
          {/* Y-axis line */}
          <line
            x1="0"
            y1="0"
            x2="0"
            y2={height - chartPadding - labelHeight}
            stroke="currentColor"
            strokeOpacity="0.2"
          />
          
          {/* X-axis line */}
          <line
            x1="0"
            y1={height - chartPadding - labelHeight}
            x2="100%"
            y2={height - chartPadding - labelHeight}
            stroke="currentColor"
            strokeOpacity="0.2"
          />
        </g>
      </svg>
    </div>
  );
};

export default BarChart;