import React from 'react';
import { ResponsiveContainer, Cell, Tooltip } from 'recharts';
import { CCard, CCardBody, CCardHeader, CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faMapMarkerAlt, faThermometerHalf } from '@fortawesome/free-solid-svg-icons';

interface HeatMapDataPoint {
  location: string;
  department: string;
  value: number;
  count: number;
  description?: string;
  coordinates?: { x: number; y: number };
}

interface HeatMapChartProps {
  title?: string;
  data: HeatMapDataPoint[];
  metric?: string;
  height?: number;
  className?: string;
  colorScheme?: 'heat' | 'risk' | 'performance';
  showLegend?: boolean;
}

const getColorScheme = (scheme: 'heat' | 'risk' | 'performance') => {
  switch (scheme) {
    case 'heat':
      return {
        low: '#d4edda',
        medium: '#fff3cd',
        high: '#f8d7da',
        critical: '#721c24'
      };
    case 'risk':
      return {
        low: '#28a745',
        medium: '#ffc107',
        high: '#fd7e14',
        critical: '#dc3545'
      };
    case 'performance':
      return {
        low: '#dc3545',
        medium: '#ffc107',
        high: '#20c997',
        critical: '#28a745'
      };
    default:
      return {
        low: '#d4edda',
        medium: '#fff3cd', 
        high: '#f8d7da',
        critical: '#721c24'
      };
  }
};

const getIntensityLevel = (value: number, maxValue: number) => {
  const percentage = (value / maxValue) * 100;
  if (percentage <= 25) return 'low';
  if (percentage <= 50) return 'medium';
  if (percentage <= 75) return 'high';
  return 'critical';
};

const CustomTooltip = ({ active, payload }: any) => {
  if (active && payload && payload.length) {
    const data = payload[0].payload;
    return (
      <div className="bg-white p-3 border border-gray-300 rounded shadow-lg">
        <p className="font-semibold text-gray-800 mb-1">{data.location}</p>
        <p className="text-sm text-gray-600 mb-1">Department: {data.department}</p>
        <p className="text-blue-600 font-bold">
          Value: {data.value.toFixed(2)}
        </p>
        <p className="text-gray-600 text-sm">
          Count: {data.count} incidents
        </p>
        {data.description && (
          <p className="text-gray-500 text-xs mt-1">
            {data.description}
          </p>
        )}
      </div>
    );
  }
  return null;
};

const HeatMapChart: React.FC<HeatMapChartProps> = ({
  title = "Location Risk Heat Map",
  data,
  metric = "Incident Rate",
  height = 400,
  className = "",
  colorScheme = 'risk',
  showLegend = true
}) => {
  const colors = getColorScheme(colorScheme);
  const maxValue = Math.max(...data.map(d => d.value));
  const minValue = Math.min(...data.map(d => d.value));
  
  // Group data by department for better organization
  const groupedData = data.reduce((acc, item) => {
    if (!acc[item.department]) {
      acc[item.department] = [];
    }
    acc[item.department].push(item);
    return acc;
  }, {} as { [key: string]: HeatMapDataPoint[] });

  const departments = Object.keys(groupedData);
  const maxLocationsPerDept = Math.max(...departments.map(dept => groupedData[dept].length));

  // Calculate grid dimensions
  const cellWidth = 100 / departments.length;
  const cellHeight = 80 / maxLocationsPerDept;

  return (
    <CCard className={`h-100 ${className}`}>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <div>
          <h6 className="mb-0">
            <FontAwesomeIcon icon={faMapMarkerAlt} className="me-2 text-primary" />
            {title}
          </h6>
          <small className="text-muted">{metric} by Location & Department</small>
        </div>
        <div className="d-flex align-items-center gap-2">
          <FontAwesomeIcon icon={faThermometerHalf} className="text-muted" />
          <span className="small text-muted">
            Range: {minValue.toFixed(1)} - {maxValue.toFixed(1)}
          </span>
        </div>
      </CCardHeader>
      <CCardBody>
        <div style={{ height: height, position: 'relative' }}>
          {/* Department Headers */}
          <div className="d-flex mb-2">
            {departments.map((dept, deptIndex) => (
              <div 
                key={dept}
                className="text-center fw-semibold small"
                style={{ width: `${cellWidth}%` }}
              >
                {dept}
              </div>
            ))}
          </div>
          
          {/* Heat Map Grid */}
          <div style={{ position: 'relative', height: '85%' }}>
            {departments.map((dept, deptIndex) => (
              <div key={dept} style={{ position: 'absolute', left: `${deptIndex * cellWidth}%`, width: `${cellWidth}%`, height: '100%' }}>
                {groupedData[dept].map((location, locIndex) => {
                  const intensity = getIntensityLevel(location.value, maxValue);
                  const cellColor = colors[intensity];
                  
                  return (
                    <div
                      key={`${dept}-${location.location}`}
                      style={{
                        position: 'absolute',
                        top: `${locIndex * (100 / maxLocationsPerDept)}%`,
                        left: '2px',
                        right: '2px',
                        height: `${100 / maxLocationsPerDept - 2}%`,
                        backgroundColor: cellColor,
                        border: '1px solid #dee2e6',
                        borderRadius: '4px',
                        cursor: 'pointer',
                        display: 'flex',
                        flexDirection: 'column',
                        justifyContent: 'center',
                        alignItems: 'center',
                        padding: '4px',
                        transition: 'all 0.2s ease'
                      }}
                      className="heat-map-cell"
                      title={`${location.location}: ${location.value.toFixed(2)}`}
                    >
                      <div className="text-center">
                        <div className="fw-bold small" style={{ fontSize: '10px', lineHeight: 1 }}>
                          {location.location.length > 8 ? location.location.substring(0, 8) + '...' : location.location}
                        </div>
                        <div className="fw-bold" style={{ fontSize: '12px' }}>
                          {location.value.toFixed(1)}
                        </div>
                        <div style={{ fontSize: '9px', opacity: 0.8 }}>
                          ({location.count})
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            ))}
          </div>
        </div>
        
        {/* Legend */}
        {showLegend && (
          <div className="mt-3">
            <div className="d-flex justify-content-center align-items-center gap-3 flex-wrap">
              <span className="small fw-semibold me-2">Intensity:</span>
              {Object.entries(colors).map(([level, color]) => (
                <div key={level} className="d-flex align-items-center">
                  <div 
                    className="rounded me-1" 
                    style={{ 
                      width: '16px', 
                      height: '16px', 
                      backgroundColor: color,
                      border: '1px solid #dee2e6'
                    }}
                  />
                  <span className="small text-capitalize">{level}</span>
                </div>
              ))}
            </div>
            
            {/* Statistics Summary */}
            <div className="row mt-3">
              <div className="col-md-3 col-6 text-center">
                <div className="fw-bold text-primary">{data.length}</div>
                <div className="small text-muted">Locations</div>
              </div>
              <div className="col-md-3 col-6 text-center">
                <div className="fw-bold text-info">{departments.length}</div>
                <div className="small text-muted">Departments</div>
              </div>
              <div className="col-md-3 col-6 text-center">
                <div className="fw-bold text-success">{data.reduce((sum, item) => sum + item.count, 0)}</div>
                <div className="small text-muted">Total Incidents</div>
              </div>
              <div className="col-md-3 col-6 text-center">
                <div className="fw-bold text-warning">{(data.reduce((sum, item) => sum + item.value, 0) / data.length).toFixed(2)}</div>
                <div className="small text-muted">Avg {metric}</div>
              </div>
            </div>
          </div>
        )}
        
        <style>{`
          .heat-map-cell:hover {
            transform: scale(1.05);
            box-shadow: 0 2px 8px rgba(0,0,0,0.15);
            z-index: 10;
          }
        `}</style>
      </CCardBody>
    </CCard>
  );
};

export default HeatMapChart;