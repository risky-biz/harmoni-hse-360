import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CRow,
  CCol,
  CFormSelect,
  CSpinner,
  CAlert,
} from '@coreui/react';
import { useGetHazardDashboardQuery } from '../../features/hazards/hazardApi';
import StatsCard from '../../components/dashboard/StatsCard';
import ProgressCard from '../../components/dashboard/ProgressCard';
import ChartCard from '../../components/dashboard/ChartCard';
import DonutChart from '../../components/dashboard/DonutChart';
import BarChart from '../../components/dashboard/BarChart';
import LineChart from '../../components/dashboard/LineChart';
import RecentItemsList from '../../components/dashboard/RecentItemsList';

const HazardDashboard: React.FC = () => {
  const [dateFilter, setDateFilter] = useState('30');
  const [departmentFilter, setDepartmentFilter] = useState('');

  // Helper function to get status color
  const getStatusColor = (severity: string): string => {
    switch (severity?.toLowerCase()) {
      case 'critical':
      case 'error':
      case 'danger':
        return 'danger';
      case 'warning':
        return 'warning';
      case 'success':
        return 'success';
      case 'info':
      default:
        return 'info';
    }
  };

  const { data, isLoading, error } = useGetHazardDashboardQuery({
    dateFrom: getDateFromDays(parseInt(dateFilter)),
    department: departmentFilter || undefined,
    includeTrends: true,
    includeLocationAnalytics: true,
    includeComplianceMetrics: true,
    includePerformanceMetrics: true,
  });

  function getDateFromDays(days: number): string {
    const date = new Date();
    date.setDate(date.getDate() - days);
    return date.toISOString().split('T')[0];
  }

  if (isLoading) {
    return (
      <div className="text-center py-4">
        <CSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load hazard dashboard. Please try again.
      </CAlert>
    );
  }

  if (!data) {
    return (
      <CAlert color="info">
        No hazard data available.
      </CAlert>
    );
  }

  return (
    <>
      {/* Filter Controls */}
      <CRow className="mb-4">
        <CCol md={3}>
          <CFormSelect 
            value={dateFilter} 
            onChange={(e) => setDateFilter(e.target.value)}
          >
            <option value="7">Last 7 days</option>
            <option value="30">Last 30 days</option>
            <option value="90">Last 90 days</option>
            <option value="365">Last year</option>
          </CFormSelect>
        </CCol>
        <CCol md={3}>
          <CFormSelect 
            value={departmentFilter} 
            onChange={(e) => setDepartmentFilter(e.target.value)}
          >
            <option value="">All Departments</option>
            <option value="Safety">Safety</option>
            <option value="Facilities">Facilities</option>
            <option value="Academic">Academic</option>
            <option value="Administration">Administration</option>
          </CFormSelect>
        </CCol>
      </CRow>

      {/* Overview Stats */}
      <CRow className="mb-4">
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Total Hazards"
            value={data.overview?.totalHazards || 0}
            change={data.overview?.totalHazardsChange}
            color="primary"
            icon="exclamation-triangle"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Open Hazards"
            value={data.overview?.openHazards || 0}
            color="warning"
            icon="exclamation-circle"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="High Risk"
            value={data.overview?.highRiskHazards || 0}
            change={data.overview?.highRiskChange}
            color="danger"
            icon="exclamation-triangle"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Unassessed"
            value={data.overview?.unassessedHazards || 0}
            color="info"
            icon="question-circle"
          />
        </CCol>
      </CRow>

      {/* Progress Cards */}
      <CRow className="mb-4">
        <CCol md={4}>
          <ProgressCard
            title="Risk Assessment Progress"
            value={data.riskAnalysis?.riskAssessmentsCompleted || 0}
            total={(data.riskAnalysis?.riskAssessmentsCompleted || 0) + (data.riskAnalysis?.riskAssessmentsPending || 0)}
            percentage={data.riskAnalysis?.riskAssessmentCompletionRate || 0}
            description={`${data.riskAnalysis?.riskAssessmentsCompleted || 0} of ${(data.riskAnalysis?.riskAssessmentsCompleted || 0) + (data.riskAnalysis?.riskAssessmentsPending || 0)} completed`}
            color="success"
          />
        </CCol>
        <CCol md={4}>
          <ProgressCard
            title="Mitigation Action Completion"
            value={data.performance?.completedMitigationActions || 0}
            total={(data.performance?.completedMitigationActions || 0) + (data.performance?.totalMitigationActions || 0)}
            percentage={data.performance?.mitigationActionCompletionRate || 0}
            description={`${data.performance?.completedMitigationActions || 0} of ${data.performance?.totalMitigationActions || 0} completed`}
            color="primary"
          />
        </CCol>
        <CCol md={4}>
          <ProgressCard
            title="Compliance Score"
            value={data.compliance?.complianceViolations || 0}
            total={100}
            percentage={data.compliance?.overallComplianceScore || 0}
            description={`${data.compliance?.complianceViolations || 0} violations found`}
            color={(data.compliance?.overallComplianceScore || 0) >= 80 ? 'success' : 'warning'}
          />
        </CCol>
      </CRow>

      {/* Charts */}
      <CRow className="mb-4">
        <CCol md={6}>
          <ChartCard title="Risk Level Distribution">
            <DonutChart
              data={Object.entries(data.riskAnalysis?.riskLevelDistribution || {}).map(([label, value]) => ({
                label,
                value: typeof value === 'number' ? value : 0
              }))}
            />
          </ChartCard>
        </CCol>
        <CCol md={6}>
          <ChartCard title="Hazards by Category">
            <BarChart
              data={Object.entries(data.riskAnalysis?.categoryDistribution || {}).map(([label, value]) => ({
                label,
                value: typeof value === 'number' ? value : 0
              }))}
            />
          </ChartCard>
        </CCol>
      </CRow>

      <CRow className="mb-4">
        <CCol md={8}>
          <ChartCard title="Hazard Reporting Trend">
            <LineChart
              data={(data.trends?.hazardReportingTrend || []).map(point => ({
                label: point?.label || 'Unknown',
                value: typeof point?.value === 'number' ? point.value : 0
              }))}
            />
          </ChartCard>
        </CCol>
        <CCol md={4}>
          <ChartCard title="Severity Distribution">
            <DonutChart
              data={Object.entries(data.riskAnalysis?.severityDistribution || {}).map(([label, value]) => ({
                label,
                value: typeof value === 'number' ? value : 0
              }))}
            />
          </ChartCard>
        </CCol>
      </CRow>

      {/* Recent Activities and Alerts */}
      <CRow>
        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <strong>Recent Activities</strong>
            </CCardHeader>
            <CCardBody>
              <RecentItemsList
                title="Recent Activities"
                items={(data.recentActivities || []).map((activity, index) => ({
                  id: activity?.id || index,
                  title: activity?.title || 'Unknown Activity',
                  subtitle: activity?.description,
                  status: activity?.severity || 'info',
                  statusColor: getStatusColor(activity?.severity || 'info'),
                  timestamp: activity?.timestamp || new Date().toISOString(),
                  onClick: () => window.location.href = `/hazards/${activity?.relatedEntityId || ''}`
                }))}
              />
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={6}>
          <CCard>
            <CCardHeader>
              <strong>Active Alerts</strong>
            </CCardHeader>
            <CCardBody>
              <RecentItemsList
                title="Active Alerts"
                items={(data.alerts || []).map((alert, index) => ({
                  id: alert?.id || index + 1000,
                  title: alert?.title || 'Unknown Alert',
                  subtitle: alert?.message,
                  status: alert?.severity || 'warning',
                  statusColor: getStatusColor(alert?.severity || 'warning'),
                  timestamp: alert?.createdAt || new Date().toISOString(),
                  onClick: () => window.location.href = `/hazards/${alert?.hazardId || ''}`
                }))}
              />
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default HazardDashboard;