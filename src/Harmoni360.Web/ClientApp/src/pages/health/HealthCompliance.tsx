import React, { useState, useMemo } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CButton,
  CSpinner,
  CAlert,
  CBadge,
  CFormSelect,
  CProgress,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
  CButtonGroup
} from '@coreui/react';
import { 
  useGetVaccinationComplianceQuery,
  useGetHealthRiskAssessmentQuery,
  useGetEmergencyContactValidationQuery 
} from '../../features/health/healthApi';
import { PersonType } from '../../types/health';
import { ChartCard } from '../../components/dashboard';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faShieldAlt,
  faExclamationTriangle,
  faCheckCircle,
  faUsers,
  faChartLine,
  faPhone,
  faDownload,
  faPrint,
  faRefresh
} from '@fortawesome/free-solid-svg-icons';

const HealthCompliance: React.FC = () => {
  const [activeTab, setActiveTab] = useState('vaccination');
  const [populationType, setPopulationType] = useState<PersonType | ''>('');
  const [reportFormat, setReportFormat] = useState('summary');

  const {
    data: vaccinationCompliance,
    isLoading: isLoadingVaccination,
    error: vaccinationError,
    refetch: refetchVaccination
  } = useGetVaccinationComplianceQuery({
    populationType: populationType || undefined,
    includeDetails: true
  });

  const {
    data: riskAssessment,
    isLoading: isLoadingRisk,
    error: riskError,
    refetch: refetchRisk
  } = useGetHealthRiskAssessmentQuery({
    includeVaccinations: true,
    includeMedicalConditions: true,
    includeHealthIncidents: true
  });

  const {
    data: contactValidation,
    isLoading: isLoadingContacts,
    error: contactError,
    refetch: refetchContacts
  } = useGetEmergencyContactValidationQuery();

  const vaccinationChartData = useMemo(() => {
    if (!vaccinationCompliance) return null;

    return {
      labels: ['Compliant', 'Overdue', 'Exempted'],
      datasets: [{
        data: [
          vaccinationCompliance.totalCompliant,
          vaccinationCompliance.totalOverdue,
          vaccinationCompliance.totalExempted
        ],
        backgroundColor: ['#28a745', '#dc3545', '#ffc107'],
        borderWidth: 0
      }]
    };
  }, [vaccinationCompliance]);

  const populationBreakdownChart = useMemo(() => {
    if (!vaccinationCompliance?.populationBreakdown) return null;

    const data = vaccinationCompliance.populationBreakdown;
    return {
      labels: data.map(p => p.populationType),
      datasets: [{
        label: 'Compliance Rate (%)',
        data: data.map(p => p.complianceRate),
        backgroundColor: data.map(p => p.complianceRate >= 95 ? '#28a745' : 
                                      p.complianceRate >= 85 ? '#ffc107' : '#dc3545'),
        borderWidth: 1
      }]
    };
  }, [vaccinationCompliance]);

  const handleExportReport = (format: 'pdf' | 'excel' | 'csv') => {
    // In a real implementation, this would trigger a download
    console.log(`Exporting ${activeTab} compliance report as ${format}`);
  };

  const handlePrintReport = () => {
    window.print();
  };

  const handleRefreshData = () => {
    refetchVaccination();
    refetchRisk();
    refetchContacts();
  };

  const getComplianceColor = (rate: number) => {
    if (rate >= 95) return 'success';
    if (rate >= 85) return 'warning';
    return 'danger';
  };

  const getComplianceText = (rate: number) => {
    if (rate >= 95) return 'Excellent';
    if (rate >= 85) return 'Good';
    if (rate >= 70) return 'Needs Attention';
    return 'Critical';
  };

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Health Compliance & Reporting</h2>
        <div className="d-flex gap-2">
          <CFormSelect
            value={populationType}
            onChange={(e) => setPopulationType(e.target.value as PersonType | '')}
            style={{ width: '150px' }}
          >
            <option value="">All Population</option>
            <option value={PersonType.Student}>Students</option>
            <option value={PersonType.Staff}>Staff</option>
          </CFormSelect>
          <CButton color="info" onClick={handleRefreshData}>
            <FontAwesomeIcon icon={faRefresh} className="me-1" />
            Refresh
          </CButton>
        </div>
      </div>

      {/* Navigation Tabs */}
      <CNav variant="tabs" className="mb-3">
        <CNavItem>
          <CNavLink
            active={activeTab === 'vaccination'}
            onClick={() => setActiveTab('vaccination')}
            className="cursor-pointer"
          >
            <FontAwesomeIcon icon={faShieldAlt} className="me-1" />
            Vaccination Compliance
          </CNavLink>
        </CNavItem>
        <CNavItem>
          <CNavLink
            active={activeTab === 'risk'}
            onClick={() => setActiveTab('risk')}
            className="cursor-pointer"
          >
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
            Risk Assessment
          </CNavLink>
        </CNavItem>
        <CNavItem>
          <CNavLink
            active={activeTab === 'contacts'}
            onClick={() => setActiveTab('contacts')}
            className="cursor-pointer"
          >
            <FontAwesomeIcon icon={faPhone} className="me-1" />
            Emergency Contacts
          </CNavLink>
        </CNavItem>
      </CNav>

      {/* Tab Content */}
      <CTabContent>
        {/* Vaccination Compliance Tab */}
        <CTabPane visible={activeTab === 'vaccination'}>
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h4>Vaccination Compliance Report</h4>
            <CButtonGroup>
              <CButton color="outline-primary" onClick={handlePrintReport}>
                <FontAwesomeIcon icon={faPrint} className="me-1" />
                Print
              </CButton>
              <CButton color="outline-success" onClick={() => handleExportReport('excel')}>
                <FontAwesomeIcon icon={faDownload} className="me-1" />
                Export Excel
              </CButton>
              <CButton color="outline-danger" onClick={() => handleExportReport('pdf')}>
                <FontAwesomeIcon icon={faDownload} className="me-1" />
                Export PDF
              </CButton>
            </CButtonGroup>
          </div>

          {isLoadingVaccination ? (
            <div className="d-flex justify-content-center p-4">
              <CSpinner color="primary" size="lg" />
            </div>
          ) : vaccinationError ? (
            <CAlert color="danger">
              Failed to load vaccination compliance data.
            </CAlert>
          ) : vaccinationCompliance ? (
            <>
              {/* Summary Cards */}
              <CRow className="mb-4">
                <CCol md={3}>
                  <CCard className="text-center">
                    <CCardBody>
                      <div className="fs-4 fw-semibold text-success">
                        {vaccinationCompliance.complianceRate.toFixed(1)}%
                      </div>
                      <div className="text-muted">Overall Compliance</div>
                      <CBadge color={getComplianceColor(vaccinationCompliance.complianceRate)} className="mt-1">
                        {getComplianceText(vaccinationCompliance.complianceRate)}
                      </CBadge>
                    </CCardBody>
                  </CCard>
                </CCol>
                <CCol md={3}>
                  <CCard className="text-center">
                    <CCardBody>
                      <div className="fs-4 fw-semibold text-success">
                        {vaccinationCompliance.totalCompliant}
                      </div>
                      <div className="text-muted">Compliant</div>
                      <div className="small text-muted">
                        of {vaccinationCompliance.totalRequired} required
                      </div>
                    </CCardBody>
                  </CCard>
                </CCol>
                <CCol md={3}>
                  <CCard className="text-center">
                    <CCardBody>
                      <div className="fs-4 fw-semibold text-danger">
                        {vaccinationCompliance.totalOverdue}
                      </div>
                      <div className="text-muted">Overdue</div>
                      <div className="small text-muted">Requiring attention</div>
                    </CCardBody>
                  </CCard>
                </CCol>
                <CCol md={3}>
                  <CCard className="text-center">
                    <CCardBody>
                      <div className="fs-4 fw-semibold text-secondary">
                        {vaccinationCompliance.totalExempted}
                      </div>
                      <div className="text-muted">Exempted</div>
                      <div className="small text-muted">With documentation</div>
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>

              {/* Charts */}
              <CRow className="mb-4">
                <CCol lg={6}>
                  {vaccinationChartData && (
                    <ChartCard
                      title="Vaccination Status Distribution"
                      type="doughnut"
                      data={vaccinationChartData}
                      height={300}
                    />
                  )}
                </CCol>
                <CCol lg={6}>
                  {populationBreakdownChart && (
                    <ChartCard
                      title="Compliance by Population Type"
                      type="bar"
                      data={populationBreakdownChart}
                      height={300}
                    />
                  )}
                </CCol>
              </CRow>

              {/* Detailed Breakdown */}
              <CRow>
                <CCol lg={6}>
                  <CCard>
                    <CCardHeader>
                      <strong>Vaccine-Specific Compliance</strong>
                    </CCardHeader>
                    <CCardBody className="p-0">
                      {vaccinationCompliance.vaccineBreakdown.length > 0 ? (
                        <CTable hover responsive>
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Vaccine</CTableHeaderCell>
                              <CTableHeaderCell>Compliance</CTableHeaderCell>
                              <CTableHeaderCell>Status</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {vaccinationCompliance.vaccineBreakdown.map((vaccine) => (
                              <CTableRow key={vaccine.vaccineName}>
                                <CTableDataCell>
                                  <strong>{vaccine.vaccineName}</strong>
                                </CTableDataCell>
                                <CTableDataCell>
                                  <div className="d-flex align-items-center">
                                    <span className="me-2">{vaccine.complianceRate.toFixed(1)}%</span>
                                    <CProgress
                                      value={vaccine.complianceRate}
                                      color={getComplianceColor(vaccine.complianceRate)}
                                      style={{ width: '80px', height: '8px' }}
                                    />
                                  </div>
                                  <div className="small text-muted">
                                    {vaccine.compliant}/{vaccine.required} compliant, {vaccine.overdue} overdue
                                  </div>
                                </CTableDataCell>
                                <CTableDataCell>
                                  <CBadge color={getComplianceColor(vaccine.complianceRate)}>
                                    {getComplianceText(vaccine.complianceRate)}
                                  </CBadge>
                                </CTableDataCell>
                              </CTableRow>
                            ))}
                          </CTableBody>
                        </CTable>
                      ) : (
                        <div className="p-3 text-muted text-center">
                          No vaccination data available
                        </div>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>
                <CCol lg={6}>
                  <CCard>
                    <CCardHeader>
                      <strong>Population Breakdown</strong>
                    </CCardHeader>
                    <CCardBody className="p-0">
                      {vaccinationCompliance.populationBreakdown.length > 0 ? (
                        <CTable hover responsive>
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Population</CTableHeaderCell>
                              <CTableHeaderCell>Compliance Rate</CTableHeaderCell>
                              <CTableHeaderCell>Status</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {vaccinationCompliance.populationBreakdown.map((population) => (
                              <CTableRow key={population.populationType}>
                                <CTableDataCell>
                                  <FontAwesomeIcon icon={faUsers} className="me-1" />
                                  <strong>{population.populationType}</strong>
                                  <div className="small text-muted">
                                    {population.compliant}/{population.totalRequired} people
                                  </div>
                                </CTableDataCell>
                                <CTableDataCell>
                                  <div className="d-flex align-items-center">
                                    <span className="me-2">{population.complianceRate.toFixed(1)}%</span>
                                    <CProgress
                                      value={population.complianceRate}
                                      color={getComplianceColor(population.complianceRate)}
                                      style={{ width: '80px', height: '8px' }}
                                    />
                                  </div>
                                </CTableDataCell>
                                <CTableDataCell>
                                  <CBadge color={getComplianceColor(population.complianceRate)}>
                                    {getComplianceText(population.complianceRate)}
                                  </CBadge>
                                </CTableDataCell>
                              </CTableRow>
                            ))}
                          </CTableBody>
                        </CTable>
                      ) : (
                        <div className="p-3 text-muted text-center">
                          No population data available
                        </div>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>
            </>
          ) : (
            <CAlert color="info">No vaccination compliance data available.</CAlert>
          )}
        </CTabPane>

        {/* Risk Assessment Tab */}
        <CTabPane visible={activeTab === 'risk'}>
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h4>Health Risk Assessment Report</h4>
            <CButtonGroup>
              <CButton color="outline-primary" onClick={handlePrintReport}>
                <FontAwesomeIcon icon={faPrint} className="me-1" />
                Print
              </CButton>
              <CButton color="outline-success" onClick={() => handleExportReport('excel')}>
                <FontAwesomeIcon icon={faDownload} className="me-1" />
                Export Excel
              </CButton>
            </CButtonGroup>
          </div>

          {isLoadingRisk ? (
            <div className="d-flex justify-content-center p-4">
              <CSpinner color="primary" size="lg" />
            </div>
          ) : riskError ? (
            <CAlert color="danger">
              Failed to load risk assessment data.
            </CAlert>
          ) : riskAssessment ? (
            <CCard>
              <CCardHeader>
                <strong>Risk Assessment Summary</strong>
              </CCardHeader>
              <CCardBody>
                <p>Risk assessment functionality would be implemented here with detailed health risk analysis.</p>
              </CCardBody>
            </CCard>
          ) : (
            <CAlert color="info">No risk assessment data available.</CAlert>
          )}
        </CTabPane>

        {/* Emergency Contacts Tab */}
        <CTabPane visible={activeTab === 'contacts'}>
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h4>Emergency Contact Validation Report</h4>
            <CButtonGroup>
              <CButton color="outline-primary" onClick={handlePrintReport}>
                <FontAwesomeIcon icon={faPrint} className="me-1" />
                Print
              </CButton>
              <CButton color="outline-success" onClick={() => handleExportReport('excel')}>
                <FontAwesomeIcon icon={faDownload} className="me-1" />
                Export Excel
              </CButton>
            </CButtonGroup>
          </div>

          {isLoadingContacts ? (
            <div className="d-flex justify-content-center p-4">
              <CSpinner color="primary" size="lg" />
            </div>
          ) : contactError ? (
            <CAlert color="danger">
              Failed to load emergency contact validation data.
            </CAlert>
          ) : contactValidation ? (
            <CCard>
              <CCardHeader>
                <strong>Emergency Contact Validation</strong>
              </CCardHeader>
              <CCardBody>
                <p>Emergency contact validation functionality would be implemented here.</p>
              </CCardBody>
            </CCard>
          ) : (
            <CAlert color="info">No emergency contact validation data available.</CAlert>
          )}
        </CTabPane>
      </CTabContent>
    </div>
  );
};

export default HealthCompliance;