import React, { useState } from 'react';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CFormSelect,
  CSpinner,
  CAlert,
  } from '@coreui/react';
import { useGetStatisticsQuery, useLazyExportStatisticsQuery, useGetTrendsQuery } from '../../features/statistics/statisticsApi';
import { LineChart } from '../../components/dashboard';
import { useSignalR } from '../../hooks/useSignalR';

const modules = [
  { label: 'All Modules', value: 'All' },
  { label: 'Health', value: 'HealthMonitoring' },
  { label: 'Safety', value: 'IncidentManagement' },
  { label: 'Security', value: 'SecurityIncidentManagement' },
  { label: 'Environment', value: 'RiskManagement' },
];

const HsseDashboard: React.FC = () => {
  const [selectedModule, setSelectedModule] = useState('All');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const { data, isLoading, error, refetch } = useGetStatisticsQuery({
    module: selectedModule === 'All' ? undefined : selectedModule,
    startDate: startDate || undefined,
    endDate: endDate || undefined,
  });
  const { data: trendData } = useGetTrendsQuery({
    module: selectedModule === 'All' ? undefined : selectedModule,
    startDate: startDate || undefined,
    endDate: endDate || undefined,
  });
  const [triggerExport] = useLazyExportStatisticsQuery();
  useSignalR();

  const handleExport = async () => {
    const res = await triggerExport({
      module: selectedModule === 'All' ? undefined : selectedModule,
      startDate: startDate || undefined,
      endDate: endDate || undefined,
    }).unwrap();
    const url = window.URL.createObjectURL(res);
    const link = document.createElement('a');
    link.href = url;
    link.download = `hsse_statistics_${new Date().toISOString().slice(0,10)}.pdf`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  };

  return (
    <div className="p-4">
      <h1 className="h2">HSSE Statistics Dashboard</h1>
      <CRow className="mb-3 align-items-end">
        <CCol md={3} sm={6} xs={12}>
          <CFormSelect
            value={selectedModule}
            onChange={(e) => setSelectedModule(e.target.value)}
            options={modules.map((m) => ({ label: m.label, value: m.value }))}
          />
        </CCol>
        <CCol md={3} sm={6} className="mt-2 mt-sm-0">
          <input
            type="date"
            className="form-control"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
          />
        </CCol>
        <CCol md={3} sm={6} className="mt-2 mt-sm-0">
          <input
            type="date"
            className="form-control"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
          />
        </CCol>
        <CCol md={3} sm={6} className="mt-2 mt-sm-0">
          <button className="btn btn-outline-primary w-100" onClick={handleExport}>
            Export PDF
          </button>
        </CCol>
      </CRow>
      {isLoading && (
        <div className="d-flex justify-content-center p-5">
          <CSpinner color="primary" />
        </div>
      )}
      {error && (
        <CAlert color="danger" className="p-3">
          Failed to load statistics. <button onClick={() => refetch()}>Retry</button>
        </CAlert>
      )}
      {data && (
        <CRow>
          <CCol md={3} sm={6} className="mb-3">
            <CCard>
              <CCardHeader>Total Incidents</CCardHeader>
              <CCardBody>{data.totalIncidents}</CCardBody>
            </CCard>
          </CCol>
          <CCol md={3} sm={6} className="mb-3">
            <CCard>
              <CCardHeader>Total Hazards</CCardHeader>
              <CCardBody>{data.totalHazards}</CCardBody>
            </CCard>
          </CCol>
          <CCol md={3} sm={6} className="mb-3">
            <CCard>
              <CCardHeader>Security Incidents</CCardHeader>
              <CCardBody>{data.totalSecurityIncidents}</CCardBody>
            </CCard>
          </CCol>
          <CCol md={3} sm={6} className="mb-3">
            <CCard>
              <CCardHeader>Health Incidents</CCardHeader>
              <CCardBody>{data.totalHealthIncidents}</CCardBody>
            </CCard>
          </CCol>
        </CRow>
      )}
      {trendData && trendData.length > 0 && (
        <div className="mt-4">
          <h5>Monthly Trend</h5>
          <LineChart
            data={trendData.map(t => ({ label: t.periodLabel, value: t.incidentCount + t.hazardCount + t.securityIncidentCount + t.healthIncidentCount }))}
            height={300}
          />
        </div>
      )}
    </div>
  );
};

export default HsseDashboard;
