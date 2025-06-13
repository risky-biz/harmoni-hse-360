import React from 'react';

interface LineChartData {
  label: string;
  value: number;
}

interface LineChartProps {
  data: LineChartData[];
  height?: number;
  color?: string;
  showDots?: boolean;
  showGrid?: boolean;
  className?: string;
}

const LineChart: React.FC<LineChartProps> = ({
  data,
  height = 300,
  color = 'var(--cui-primary)',
  showDots = true,
  showGrid = true,
  className = ''
}) => {
  // Validate and clean data
  const validData = data.filter(d => d && typeof d.value === 'number' && !isNaN(d.value) && d.label);
  
  if (validData.length === 0) {
    return (
      <div className={`line-chart ${className}`} style={{ height }}>
        <div className="d-flex justify-content-center align-items-center h-100">
          <span className="text-medium-emphasis">No valid data available</span>
        </div>
      </div>
    );
  }

  const padding = 40;
  const chartWidth = 100; // percentage
  const chartHeight = height - 2 * padding;
  
  const maxValue = Math.max(...validData.map(d => d.value)) || 1;
  const minValue = Math.min(...validData.map(d => d.value)) || 0;
  const valueRange = maxValue - minValue || 1;
  
  // Calculate points
  const points = validData.map((item, index) => {
    const x = validData.length === 1 ? 50 : (index / (validData.length - 1)) * chartWidth + padding;
    const y = padding + ((maxValue - item.value) / valueRange) * chartHeight;
    return { 
      x: isNaN(x) ? padding : x, 
      y: isNaN(y) ? padding : y, 
      value: item.value, 
      label: item.label 
    };
  });
  
  // Create path
  const pathData = points.reduce((path, point, index) => {
    const command = index === 0 ? 'M' : 'L';
    return `${path} ${command} ${point.x} ${point.y}`;
  }, '');

  return (
    <div className={`line-chart ${className}`}>
      <svg width="100%" height={height} className="line-chart-svg">
        {/* Grid lines */}
        {showGrid && (
          <g className="grid" opacity="0.2">
            {/* Horizontal grid lines */}
            {Array.from({ length: 5 }, (_, i) => {
              const y = padding + (i * chartHeight) / 4;
              return (
                <line
                  key={`h-${i}`}
                  x1={padding}
                  y1={y}
                  x2={chartWidth + padding}
                  y2={y}
                  stroke="currentColor"
                  strokeWidth="1"
                />
              );
            })}
            
            {/* Vertical grid lines */}
            {points.map((point, index) => (
              <line
                key={`v-${index}`}
                x1={point.x}
                y1={padding}
                x2={point.x}
                y2={padding + chartHeight}
                stroke="currentColor"
                strokeWidth="1"
              />
            ))}
          </g>
        )}
        
        {/* Line */}
        <path
          d={pathData}
          fill="none"
          stroke={color}
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        
        {/* Area under line */}
        <path
          d={`${pathData} L ${points[points.length - 1].x} ${padding + chartHeight} L ${points[0].x} ${padding + chartHeight} Z`}
          fill={color}
          fillOpacity="0.1"
        />
        
        {/* Data points */}
        {showDots && points.map((point, index) => (
          <g key={index}>
            <circle
              cx={point.x}
              cy={point.y}
              r="4"
              fill={color}
              stroke="white"
              strokeWidth="2"
            />
            
            {/* Tooltip on hover */}
            <circle
              cx={point.x}
              cy={point.y}
              r="12"
              fill="transparent"
              className="chart-hover-area"
            >
              <title>{`${point.label}: ${point.value}`}</title>
            </circle>
          </g>
        ))}
        
        {/* Y-axis labels */}
        {Array.from({ length: 5 }, (_, i) => {
          const value = maxValue - (i * valueRange) / 4;
          const y = padding + (i * chartHeight) / 4;
          return (
            <text
              key={`y-label-${i}`}
              x={padding - 10}
              y={y}
              textAnchor="end"
              dy="0.3em"
              fontSize="11"
              fill="currentColor"
              className="text-medium-emphasis"
            >
              {Math.round(value)}
            </text>
          );
        })}
        
        {/* X-axis labels */}
        {points.map((point, index) => (
          <text
            key={`x-label-${index}`}
            x={point.x}
            y={padding + chartHeight + 20}
            textAnchor="middle"
            fontSize="11"
            fill="currentColor"
            className="text-medium-emphasis"
          >
            {point.label}
          </text>
        ))}
      </svg>
    </div>
  );
};

export default LineChart;