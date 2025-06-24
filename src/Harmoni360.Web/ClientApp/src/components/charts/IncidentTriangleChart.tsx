import React from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import { CCard, CCardBody, CCardHeader } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';

interface IncidentTriangleData {
  level: string;
  count: number;
  description: string;
  color: string;
  severity: number;
}

interface IncidentTriangleChartProps {
  title?: string;
  data?: IncidentTriangleData[];
  height?: number;
  className?: string;
}

const defaultData: IncidentTriangleData[] = [
  {
    level: 'Fatalities',
    count: 1,
    description: 'Fatal incidents requiring immediate investigation',
    color: '#8B0000',
    severity: 5
  },
  {
    level: 'Serious Injuries',
    count: 10,
    description: 'Lost time injuries and serious medical treatment',
    color: '#DC143C',
    severity: 4
  },
  {
    level: 'Minor Injuries',
    count: 30,
    description: 'First aid cases and minor medical treatment',
    color: '#FF6347',
    severity: 3
  },
  {
    level: 'Near Misses',
    count: 100,
    description: 'Incidents with potential for injury',
    color: '#FFA500',
    severity: 2
  },
  {
    level: 'Unsafe Conditions',
    count: 300,
    description: 'Hazardous conditions and unsafe acts',
    color: '#FFD700',
    severity: 1
  }
];

const CustomTooltip = ({ active, payload, label }: any) => {
  if (active && payload && payload.length) {
    const data = payload[0].payload;
    return (
      <div className="bg-white p-3 border border-gray-300 rounded shadow-lg">
        <p className="font-semibold text-gray-800 mb-1">{label}</p>
        <p className="text-blue-600 font-bold text-lg mb-1">
          Count: {data.count.toLocaleString()}
        </p>
        <p className="text-gray-600 text-sm">
          {data.description}
        </p>
        <p className="text-gray-500 text-xs mt-1">
          Severity Level: {data.severity}/5
        </p>
      </div>
    );
  }
  return null;
};

const IncidentTriangleChart: React.FC<IncidentTriangleChartProps> = ({
  title = "Safety Incident Triangle",
  data = defaultData,
  height = 400,
  className = ""
}) => {
  // Sort data by severity (highest to lowest) for proper triangle visualization
  const sortedData = [...data].sort((a, b) => b.severity - a.severity);

  const formatYAxisTick = (value: number) => {
    if (value >= 1000) {
      return `${(value / 1000).toFixed(1)}k`;
    }
    return value.toString();
  };

  return (
    <CCard className={`h-100 ${className}`}>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <h6 className="mb-0">
          <FontAwesomeIcon icon={faTriangleExclamation} className="me-2 text-warning" />
          {title}
        </h6>
        <small className="text-muted">Safety Pyramid Model</small>
      </CCardHeader>
      <CCardBody>
        <ResponsiveContainer width="100%" height={height}>
          <BarChart
            data={sortedData}
            margin={{
              top: 20,
              right: 30,
              left: 20,
              bottom: 60
            }}
          >
            <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
            <XAxis 
              dataKey="level" 
              angle={-45}
              textAnchor="end"
              height={100}
              interval={0}
              tick={{ fontSize: 12 }}
              stroke="#666"
            />
            <YAxis 
              scale="log"
              domain={[1, 'dataMax']}
              tickFormatter={formatYAxisTick}
              tick={{ fontSize: 12 }}
              stroke="#666"
            />
            <Tooltip content={<CustomTooltip />} />
            <Bar 
              dataKey="count" 
              stroke="#333"
              strokeWidth={1}
              radius={[4, 4, 0, 0]}
            >
              {sortedData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.color} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
        
        {/* Legend */}
        <div className="mt-3">
          <div className="d-flex flex-wrap justify-content-center gap-3">
            {sortedData.map((item, index) => (
              <div key={index} className="d-flex align-items-center">
                <div 
                  className="rounded me-2" 
                  style={{ 
                    width: '12px', 
                    height: '12px', 
                    backgroundColor: item.color 
                  }}
                />
                <span className="small text-muted">{item.level}</span>
              </div>
            ))}
          </div>
          <p className="text-center text-muted small mt-2 mb-0">
            Based on Heinrich's Safety Pyramid - Higher incident counts indicate lower severity levels
          </p>
        </div>
      </CCardBody>
    </CCard>
  );
};

export default IncidentTriangleChart;