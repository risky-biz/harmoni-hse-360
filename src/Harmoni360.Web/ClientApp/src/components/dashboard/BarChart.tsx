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
  
  // Responsive configuration
  const isMobile = window.innerWidth < 768;
  const isTablet = window.innerWidth >= 768 && window.innerWidth < 1024;
  
  // Responsive height calculation
  const responsiveHeight = isMobile ? Math.min(height * 0.7, 200) : isTablet ? Math.min(height * 0.85, 250) : height;

  if (data.length === 0) {
    return (
      <div className={`bar-chart ${className}`} style={{ height: responsiveHeight }}>
        <div className="d-flex justify-content-center align-items-center h-100">
          <span className="text-medium-emphasis">No data available</span>
        </div>
      </div>
    );
  }

  if (horizontal) {
    return (
      <div className={`bar-chart horizontal ${className}`} style={{ maxHeight: responsiveHeight, overflowY: 'auto' }}>
        {data.map((item, index) => {
          const percentage = (item.value / chartMaxValue) * 100;
          const color = item.color || barColors[index % barColors.length];
          
          return (
            <div key={index} className={`mb-${isMobile ? '2' : '3'}`}>
              <div className="d-flex justify-content-between align-items-center mb-1">
                <span 
                  className={`fw-medium ${isMobile ? 'small' : ''}`}
                  style={{ 
                    fontSize: isMobile ? '0.8rem' : '0.875rem',
                    lineHeight: '1.2'
                  }}
                >
                  {isMobile && item.label.length > 12 ? `${item.label.substring(0, 12)}...` : item.label}
                </span>
                {showValues && (
                  <span 
                    className="text-medium-emphasis fw-semibold"
                    style={{ fontSize: isMobile ? '0.75rem' : '0.8rem' }}
                  >
                    {item.value}
                  </span>
                )}
              </div>
              <div 
                className="progress" 
                style={{ 
                  height: isMobile ? '6px' : '8px',
                  borderRadius: '3px'
                }}
              >
                <div
                  className="progress-bar"
                  style={{
                    width: `${percentage}%`,
                    backgroundColor: color,
                    borderRadius: '3px',
                    transition: 'width 0.6s ease-in-out'
                  }}
                  role="progressbar"
                  aria-valuenow={item.value}
                  aria-valuemin={0}
                  aria-valuemax={chartMaxValue}
                  aria-label={`${item.label}: ${item.value}`}
                />
              </div>
            </div>
          );
        })}
      </div>
    );
  }

  const barWidth = 100 / data.length;
  const adaptiveChartPadding = isMobile ? Math.min(30, (100 / data.length) * 0.3) : Math.min(40, (100 / data.length) * 0.4);
  const adaptiveLabelHeight = isMobile ? 25 : 30;

  return (
    <div className={`bar-chart vertical ${className}`}>
      <svg 
        width="100%" 
        height={responsiveHeight} 
        className="bar-chart-svg"
        role="img"
        aria-label="Bar chart visualization"
      >
        {/* Chart area */}
        <g transform={`translate(${adaptiveChartPadding}, ${isMobile ? 5 : 10})`}>
          {data.map((item, index) => {
            const barHeight = Math.max(0, ((item.value / chartMaxValue) * (responsiveHeight - adaptiveChartPadding - adaptiveLabelHeight)));
            const availableWidth = Math.max(10, (100 - 2 * adaptiveChartPadding));
            const x = (index * barWidth * availableWidth) / 100;
            const y = responsiveHeight - adaptiveChartPadding - adaptiveLabelHeight - barHeight;
            const color = item.color || barColors[index % barColors.length];
            const rectWidth = Math.max(2, (barWidth * availableWidth) / 100 - (isMobile ? 2 : 4));
            
            return (
              <g key={index}>
                {/* Bar */}
                <rect
                  x={x}
                  y={y}
                  width={rectWidth}
                  height={barHeight}
                  fill={color}
                  rx={isMobile ? "1" : "2"}
                  role="graphics-symbol"
                  aria-label={`${item.label}: ${item.value}`}
                  style={{
                    transition: 'height 0.6s ease-in-out, y 0.6s ease-in-out',
                    cursor: 'pointer'
                  }}
                />
                
                {/* Value label */}
                {showValues && (
                  <text
                    x={x + rectWidth / 2}
                    y={y - (isMobile ? 3 : 5)}
                    textAnchor="middle"
                    fontSize={isMobile ? "10" : "12"}
                    fill="currentColor"
                    className="text-medium-emphasis"
                  >
                    {item.value}
                  </text>
                )}
                
                {/* X-axis label */}
                <text
                  x={x + rectWidth / 2}
                  y={responsiveHeight - adaptiveChartPadding + (isMobile ? 12 : 15)}
                  textAnchor="middle"
                  fontSize={isMobile ? "9" : "11"}
                  fill="currentColor"
                  className="text-medium-emphasis"
                  style={{
                    maxWidth: `${rectWidth}px`,
                    overflow: 'hidden',
                    textOverflow: 'ellipsis'
                  }}
                >
                  {isMobile && item.label.length > 8 
                    ? `${item.label.substring(0, 8)}...` 
                    : item.label}
                </text>
              </g>
            );
          })}
          
          {/* Y-axis line */}
          <line
            x1="0"
            y1="0"
            x2="0"
            y2={responsiveHeight - adaptiveChartPadding - adaptiveLabelHeight}
            stroke="currentColor"
            strokeOpacity="0.2"
            strokeWidth={isMobile ? "0.5" : "1"}
          />
          
          {/* X-axis line */}
          <line
            x1="0"
            y1={responsiveHeight - adaptiveChartPadding - adaptiveLabelHeight}
            x2="100%"
            y2={responsiveHeight - adaptiveChartPadding - adaptiveLabelHeight}
            stroke="currentColor"
            strokeOpacity="0.2"
            strokeWidth={isMobile ? "0.5" : "1"}
          />
        </g>
      </svg>
    </div>
  );
};

export default BarChart;