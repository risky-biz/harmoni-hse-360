import React from 'react';
import { 
  LineChart, 
  Line, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  Legend, 
  ResponsiveContainer,
  ReferenceLine 
} from 'recharts';
import { CCard, CCardBody, CCardHeader, CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faChartLine, faArrowUp, faArrowDown, faMinus } from '@fortawesome/free-solid-svg-icons';

interface TrendDataPoint {
  period: string;
  date: string;
  trir: number;
  ltifr: number;
  severityRate: number;
  complianceRate: number;
  nearMissCount?: number;
  hazardCount?: number;
}

interface KPITarget {
  trir: number;
  ltifr: number;
  severityRate: number;
  complianceRate: number;
}

interface MultiLineTrendChartProps {
  title?: string;
  data: TrendDataPoint[];
  targets?: KPITarget;
  height?: number;
  className?: string;
  showTargetLines?: boolean;
  selectedMetrics?: string[];
}

const defaultTargets: KPITarget = {
  trir: 3.0,
  ltifr: 1.0,
  severityRate: 50.0,
  complianceRate: 95.0
};

const metricConfig = {
  trir: {
    color: '#DC143C',
    name: 'TRIR',
    description: 'Total Recordable Incident Rate'
  },
  ltifr: {
    color: '#FF6347',
    name: 'LTIFR', 
    description: 'Lost Time Injury Frequency Rate'
  },
  severityRate: {
    color: '#FFA500',
    name: 'Severity Rate',
    description: 'Days Lost per 200,000 Hours'
  },
  complianceRate: {
    color: '#32CD32',
    name: 'Compliance Rate',
    description: 'Safety Compliance Percentage'
  }
};

const CustomTooltip = ({ active, payload, label }: any) => {
  if (active && payload && payload.length) {
    return (
      <div className="bg-white p-4 border border-gray-300 rounded shadow-lg">
        <p className="font-semibold text-gray-800 mb-2">{label}</p>
        {payload.map((entry: any, index: number) => (
          <div key={index} className="flex items-center mb-1">
            <div 
              className="w-3 h-3 rounded mr-2" 
              style={{ backgroundColor: entry.color }}
            />
            <span className="text-sm">
              {entry.name}: <span className="font-semibold">{entry.value.toFixed(2)}</span>
              {entry.dataKey === 'complianceRate' ? '%' : ''}
            </span>
          </div>
        ))}
      </div>
    );
  }
  return null;
};

const getTrendIndicator = (data: TrendDataPoint[], metric: keyof TrendDataPoint) => {
  if (data.length < 2) return { icon: faMinus, color: 'secondary', change: 0 };
  
  const current = data[data.length - 1][metric] as number;
  const previous = data[data.length - 2][metric] as number;
  const change = ((current - previous) / previous) * 100;
  
  if (Math.abs(change) < 1) {
    return { icon: faMinus, color: 'secondary', change: 0 };
  }
  
  // For compliance rate, higher is better
  if (metric === 'complianceRate') {
    return change > 0 
      ? { icon: faArrowUp, color: 'success', change }
      : { icon: faArrowDown, color: 'danger', change };
  }
  
  // For other metrics, lower is better
  return change < 0 
    ? { icon: faArrowDown, color: 'success', change }
    : { icon: faArrowUp, color: 'danger', change };
};

const MultiLineTrendChart: React.FC<MultiLineTrendChartProps> = ({
  title = "KPI Trend Analysis",
  data,
  targets = defaultTargets,
  height = 400,
  className = "",
  showTargetLines = true,
  selectedMetrics = ['trir', 'ltifr', 'severityRate', 'complianceRate']
}) => {
  const formatXAxisTick = (value: string) => {
    // Format date string to short month/year
    const date = new Date(value);
    return date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
  };

  const formatYAxisTick = (value: number) => {
    return value.toFixed(1);
  };

  return (
    <CCard className={`h-100 ${className}`}>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <div>
          <h6 className="mb-0">
            <FontAwesomeIcon icon={faChartLine} className="me-2 text-info" />
            {title}
          </h6>
          <small className="text-muted">Key Performance Indicators Over Time</small>
        </div>
        <div className="d-flex gap-2">
          {selectedMetrics.map(metric => {
            const trend = getTrendIndicator(data, metric as keyof TrendDataPoint);
            return (
              <CBadge key={metric} color={trend.color} className="d-flex align-items-center gap-1">
                <FontAwesomeIcon icon={trend.icon} size="xs" />
                {metricConfig[metric as keyof typeof metricConfig]?.name}
                {trend.change !== 0 && (
                  <span className="ms-1">
                    {Math.abs(trend.change).toFixed(1)}%
                  </span>
                )}
              </CBadge>
            );
          })}
        </div>
      </CCardHeader>
      <CCardBody>
        <ResponsiveContainer width="100%" height={height}>
          <LineChart
            data={data}
            margin={{
              top: 20,
              right: 30,
              left: 20,
              bottom: 60
            }}
          >
            <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
            <XAxis 
              dataKey="period"
              tick={{ fontSize: 12 }}
              stroke="#666"
              angle={-45}
              textAnchor="end"
              height={80}
            />
            <YAxis 
              tick={{ fontSize: 12 }}
              stroke="#666"
              tickFormatter={formatYAxisTick}
            />
            <Tooltip content={<CustomTooltip />} />
            <Legend 
              wrapperStyle={{ paddingTop: '20px' }}
              iconType="line"
            />
            
            {/* Target reference lines */}
            {showTargetLines && (
              <>
                <ReferenceLine y={targets.trir} stroke="#DC143C" strokeDasharray="5 5" strokeOpacity={0.5} />
                <ReferenceLine y={targets.ltifr} stroke="#FF6347" strokeDasharray="5 5" strokeOpacity={0.5} />
              </>
            )}
            
            {/* KPI Lines */}
            {selectedMetrics.includes('trir') && (
              <Line
                type="monotone"
                dataKey="trir"
                stroke={metricConfig.trir.color}
                strokeWidth={3}
                dot={{ fill: metricConfig.trir.color, strokeWidth: 2, r: 4 }}
                activeDot={{ r: 6, stroke: metricConfig.trir.color, strokeWidth: 2 }}
                name={metricConfig.trir.name}
              />
            )}
            
            {selectedMetrics.includes('ltifr') && (
              <Line
                type="monotone"
                dataKey="ltifr"
                stroke={metricConfig.ltifr.color}
                strokeWidth={3}
                dot={{ fill: metricConfig.ltifr.color, strokeWidth: 2, r: 4 }}
                activeDot={{ r: 6, stroke: metricConfig.ltifr.color, strokeWidth: 2 }}
                name={metricConfig.ltifr.name}
              />
            )}
            
            {selectedMetrics.includes('severityRate') && (
              <Line
                type="monotone"
                dataKey="severityRate"
                stroke={metricConfig.severityRate.color}
                strokeWidth={3}
                dot={{ fill: metricConfig.severityRate.color, strokeWidth: 2, r: 4 }}
                activeDot={{ r: 6, stroke: metricConfig.severityRate.color, strokeWidth: 2 }}
                name={metricConfig.severityRate.name}
              />
            )}
            
            {selectedMetrics.includes('complianceRate') && (
              <Line
                type="monotone"
                dataKey="complianceRate"
                stroke={metricConfig.complianceRate.color}
                strokeWidth={3}
                dot={{ fill: metricConfig.complianceRate.color, strokeWidth: 2, r: 4 }}
                activeDot={{ r: 6, stroke: metricConfig.complianceRate.color, strokeWidth: 2 }}
                name={metricConfig.complianceRate.name}
                yAxisId="right"
              />
            )}
          </LineChart>
        </ResponsiveContainer>
        
        {/* KPI Summary */}
        <div className="mt-3">
          <div className="row">
            {selectedMetrics.map(metric => {
              const config = metricConfig[metric as keyof typeof metricConfig];
              const latestValue = data[data.length - 1]?.[metric as keyof TrendDataPoint] as number;
              const target = targets[metric as keyof KPITarget];
              const isOnTarget = metric === 'complianceRate' 
                ? latestValue >= target 
                : latestValue <= target;
              
              return (
                <div key={metric} className="col-md-3 col-6 mb-2">
                  <div className="text-center">
                    <div 
                      className="fw-bold"
                      style={{ color: config?.color }}
                    >
                      {latestValue?.toFixed(2)}
                      {metric === 'complianceRate' ? '%' : ''}
                    </div>
                    <div className="small text-muted">{config?.name}</div>
                    <CBadge color={isOnTarget ? 'success' : 'warning'} className="small">
                      Target: {target}{metric === 'complianceRate' ? '%' : ''}
                    </CBadge>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </CCardBody>
    </CCard>
  );
};

export default MultiLineTrendChart;