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
  CTable,
  CTableHead,
  CTableBody,
  CTableHeaderCell,
  CTableDataCell,
  CTableRow,
  CBadge,
} from '@coreui/react';
import { useGetHazardDashboardQuery } from '../../features/hazards/hazardApi';
import StatsCard from '../../components/dashboard/StatsCard';
import ChartCard from '../../components/dashboard/ChartCard';
import DonutChart from '../../components/dashboard/DonutChart';
import BarChart from '../../components/dashboard/BarChart';
import LineChart from '../../components/dashboard/LineChart';

const HazardAnalytics: React.FC = () => {
  const [dateFilter, setDateFilter] = useState('90');
  const [departmentFilter, setDepartmentFilter] = useState('');

  const { data, isLoading, error } = useGetHazardDashboardQuery({
    dateFrom: getDateFromDays(parseInt(dateFilter)),
    department: departmentFilter || undefined,
    includeTrends: true,
    includeLocationAnalytics: true,
    includeComplianceMetrics: true,
    includePerformanceMetrics: true,
    personalizedView: false, // Full analytics view
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
        Failed to load hazard analytics. Please try again.
      </CAlert>
    );
  }

  if (!data) {
    return (
      <CAlert color="info">
        No analytics data available.
      </CAlert>
    );
  }

  return (
    <>
      {/* Page Header */}
      <CRow className="mb-4">
        <CCol>
          <h2>Hazard Analytics & Insights</h2>
          <p className="text-muted">
            Comprehensive analysis of hazard trends, risk patterns, and safety performance metrics
          </p>
        </CCol>
      </CRow>

      {/* Filter Controls */}
      <CRow className="mb-4">
        <CCol md={3}>
          <CFormSelect 
            value={dateFilter} 
            onChange={(e) => setDateFilter(e.target.value)}
          >
            <option value="30">Last 30 days</option>
            <option value="90">Last 90 days</option>
            <option value="180">Last 6 months</option>
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

      {/* Key Performance Indicators */}
      <CRow className="mb-4">
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Total Hazards"
            value={data.overview.totalHazards}
            change={data.overview.totalHazardsChange}
            color="primary"
            icon="exclamation-triangle"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Risk Assessment Rate"
            value={`${data.riskAnalysis.riskAssessmentCompletionRate}%`}
            color="success"
            icon="check-circle"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Mitigation Completion"
            value={`${data.performance.mitigationActionCompletionRate}%`}
            color="info"
            icon="tasks"
          />
        </CCol>
        <CCol sm={6} lg={3}>
          <StatsCard
            title="Compliance Score"
            value={`${data.compliance.overallComplianceScore}%`}
            color={data.compliance.overallComplianceScore >= 80 ? 'success' : 'warning'}
            icon="shield-alt"
          />
        </CCol>
      </CRow>

      {/* Trend Analysis */}
      <CRow className="mb-4">
        <CCol md={8}>
          <ChartCard title="Hazard Reporting Trends">
            <LineChart
              data={data.trends.hazardReportingTrend.map(point => ({
                label: point.label,
                value: point.value
              }))}
            />
            <div className="mt-3">
              <strong>Trend Direction:</strong> 
              <CBadge 
                color={data.trends.trendDirection === 'Increasing' ? 'danger' : 'success'} 
                className="ms-2"
              >
                {data.trends.trendDirection}
              </CBadge>
            </div>
            {data.trends.keyInsights.length > 0 && (
              <div className="mt-2">
                <strong>Key Insights:</strong>
                <ul className="mt-1 mb-0">
                  {data.trends.keyInsights.map((insight, index) => (
                    <li key={index} className="small">{insight}</li>
                  ))}
                </ul>
              </div>
            )}
          </ChartCard>
        </CCol>
        <CCol md={4}>
          <ChartCard title="Risk Level Distribution">
            <DonutChart
              data={Object.entries(data.riskAnalysis.riskLevelDistribution).map(([label, value]) => ({
                label,
                value
              }))}
            />
            <div className="mt-3 small">
              <div><strong>Average Risk Score:</strong> {data.riskAnalysis.averageRiskScore.toFixed(1)}</div>
            </div>
          </ChartCard>
        </CCol>
      </CRow>

      {/* Category and Performance Analysis */}
      <CRow className="mb-4">
        <CCol md={6}>
          <ChartCard title="Hazards by Category">
            <BarChart
              data={Object.entries(data.riskAnalysis.categoryDistribution).map(([label, value]) => ({
                label,
                value
              }))}
            />
          </ChartCard>
        </CCol>
        <CCol md={6}>
          <ChartCard title="Mitigation Action Types">
            <DonutChart
              data={Object.entries(data.performance.actionTypeDistribution).map(([label, value]) => ({
                label,
                value
              }))}
            />
          </ChartCard>
        </CCol>
      </CRow>

      {/* Location Analysis */}
      <CRow className="mb-4">
        <CCol md={8}>
          <CCard>
            <CCardHeader>
              <strong>Hazard Hotspots</strong>
            </CCardHeader>
            <CCardBody>
              <CTable striped hover responsive>
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>Location</CTableHeaderCell>
                    <CTableHeaderCell>Total Hazards</CTableHeaderCell>
                    <CTableHeaderCell>High Risk</CTableHeaderCell>
                    <CTableHeaderCell>Department</CTableHeaderCell>
                    <CTableHeaderCell>Risk Ratio</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {data.locationData.hotspotLocations.map((location, index) => (
                    <CTableRow key={index}>
                      <CTableDataCell>{location.location}</CTableDataCell>
                      <CTableDataCell>{location.hazardCount}</CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={location.highRiskCount > 0 ? 'danger' : 'success'}>
                          {location.highRiskCount}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>{location.department}</CTableDataCell>
                      <CTableDataCell>
                        {location.hazardCount > 0 
                          ? `${Math.round((location.highRiskCount / location.hazardCount) * 100)}%`
                          : '0%'
                        }
                      </CTableDataCell>
                    </CTableRow>
                  ))}
                </CTableBody>
              </CTable>
              
              <div className="mt-3">
                <strong>Most Affected Area:</strong> {data.locationData.mostAffectedArea}
              </div>
              <div>
                <strong>Locations with Hazards:</strong> {data.locationData.locationsWithHazards}
              </div>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={4}>
          <ChartCard title="Department Distribution">
            <DonutChart
              data={Object.entries(data.locationData.departmentDistribution).map(([label, value]) => ({
                label,
                value
              }))}
            />
          </ChartCard>
        </CCol>
      </CRow>

      {/* Performance Metrics */}
      <CRow className="mb-4">
        <CCol md={4}>
          <CCard>
            <CCardHeader>
              <strong>Performance Metrics</strong>
            </CCardHeader>
            <CCardBody>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Average Resolution Time</span>
                  <strong>{data.performance.averageResolutionTime} days</strong>
                </div>
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Action Effectiveness</span>
                  <strong>{data.performance.averageActionEffectiveness.toFixed(1)}/5</strong>
                </div>
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Cost Savings</span>
                  <strong>${data.performance.costSavingsFromMitigation.toLocaleString()}</strong>
                </div>
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Overdue Actions</span>
                  <CBadge color={data.performance.overdueMitigationActions > 0 ? 'danger' : 'success'}>
                    {data.performance.overdueMitigationActions}
                  </CBadge>
                </div>
              </div>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={4}>
          <CCard>
            <CCardHeader>
              <strong>Compliance Status</strong>
            </CCardHeader>
            <CCardBody>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Compliance Violations</span>
                  <CBadge color={data.compliance.complianceViolations > 0 ? 'danger' : 'success'}>
                    {data.compliance.complianceViolations}
                  </CBadge>
                </div>
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Audit Findings</span>
                  <strong>{data.compliance.auditFindings}</strong>
                </div>
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Regulatory Compliance</span>
                  <strong>{data.compliance.regulatoryReportingCompliance}%</strong>
                </div>
              </div>
              {data.compliance.lastComplianceReview && (
                <div className="mb-3">
                  <div className="d-flex justify-content-between">
                    <span>Last Review</span>
                    <strong>{new Date(data.compliance.lastComplianceReview).toLocaleDateString()}</strong>
                  </div>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
        <CCol md={4}>
          <CCard>
            <CCardHeader>
              <strong>Risk Analysis Summary</strong>
            </CCardHeader>
            <CCardBody>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Assessments Completed</span>
                  <strong>{data.riskAnalysis.riskAssessmentsCompleted}</strong>
                </div>
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Assessments Pending</span>
                  <CBadge color={data.riskAnalysis.riskAssessmentsPending > 0 ? 'warning' : 'success'}>
                    {data.riskAnalysis.riskAssessmentsPending}
                  </CBadge>
                </div>
              </div>
              <div className="mb-3">
                <div className="d-flex justify-content-between">
                  <span>Average Risk Score</span>
                  <strong>{data.riskAnalysis.averageRiskScore.toFixed(1)}</strong>
                </div>
              </div>
              <div>
                <div className="d-flex justify-content-between">
                  <span>Completion Rate</span>
                  <strong>{data.riskAnalysis.riskAssessmentCompletionRate}%</strong>
                </div>
              </div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default HazardAnalytics;