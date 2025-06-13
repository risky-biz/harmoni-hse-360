import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CRow,
  CCol,
  CButton,
  CFormInput,
  CFormSelect,
  CTable,
  CTableHead,
  CTableBody,
  CTableHeaderCell,
  CTableDataCell,
  CTableRow,
  CBadge,
  CPagination,
  CPaginationItem,
  CSpinner,
  CAlert,
} from '@coreui/react';
import { Link, useNavigate } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPlus, faEye } from '@fortawesome/free-solid-svg-icons';
import { useGetMyHazardsQuery } from '../../features/hazards/hazardApi';
import { 
  HAZARD_CATEGORIES, 
  HAZARD_STATUSES, 
  HAZARD_SEVERITIES,
  GetHazardsParams 
} from '../../types/hazard';

const MyHazards: React.FC = () => {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<GetHazardsParams>({
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    category: '',
    status: '',
    severity: '',
    onlyMyHazards: true, // This ensures we only get hazards reported by the current user
  });

  const { data, isLoading, error } = useGetMyHazardsQuery(filters);

  const handleFilterChange = (field: keyof GetHazardsParams, value: string | number) => {
    setFilters(prev => ({
      ...prev,
      [field]: field === 'pageNumber' ? Number(value) : value,
      pageNumber: field !== 'pageNumber' ? 1 : Number(value)
    }));
  };

  const getSeverityColor = (severity: string) => {
    const colors: Record<string, string> = {
      'Negligible': 'success',
      'Minor': 'info',
      'Moderate': 'warning',
      'Major': 'danger',
      'Catastrophic': 'dark'
    };
    return colors[severity] || 'secondary';
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      'Reported': 'info',
      'UnderAssessment': 'warning',
      'ActionRequired': 'danger',
      'Mitigating': 'primary',
      'Monitoring': 'warning',
      'Resolved': 'success',
      'Closed': 'secondary'
    };
    return colors[status] || 'secondary';
  };

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load your hazards. Please try again.
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="mb-4">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <strong>My Hazard Reports</strong>
            <CButton 
              color="primary" 
              onClick={() => navigate('/hazards/create')}
            >
              <FontAwesomeIcon icon={faPlus} className="me-2" />
              Report New Hazard
            </CButton>
          </CCardHeader>
          <CCardBody>
            {/* Summary Stats */}
            {data && (
              <CRow className="mb-4">
                <CCol md={3}>
                  <div className="border-start border-info ps-3">
                    <div className="text-info">Total Reports</div>
                    <div className="fs-5 fw-semibold">{data.summary.totalHazards}</div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-warning ps-3">
                    <div className="text-warning">Open</div>
                    <div className="fs-5 fw-semibold">{data.summary.openHazards}</div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-danger ps-3">
                    <div className="text-danger">High Risk</div>
                    <div className="fs-5 fw-semibold">{data.summary.highRiskHazards}</div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-secondary ps-3">
                    <div className="text-secondary">Unassessed</div>
                    <div className="fs-5 fw-semibold">{data.summary.unassessedHazards}</div>
                  </div>
                </CCol>
              </CRow>
            )}

            {/* Filters */}
            <CRow className="mb-3">
              <CCol md={3}>
                <CFormInput
                  placeholder="Search your hazards..."
                  value={filters.searchTerm}
                  onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                />
              </CCol>
              <CCol md={3}>
                <CFormSelect
                  value={filters.category}
                  onChange={(e) => handleFilterChange('category', e.target.value)}
                >
                  <option value="">All Categories</option>
                  {HAZARD_CATEGORIES.map(category => (
                    <option key={category} value={category}>{category}</option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={3}>
                <CFormSelect
                  value={filters.status}
                  onChange={(e) => handleFilterChange('status', e.target.value)}
                >
                  <option value="">All Statuses</option>
                  {HAZARD_STATUSES.map(status => (
                    <option key={status} value={status}>{status}</option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={3}>
                <CFormSelect
                  value={filters.severity}
                  onChange={(e) => handleFilterChange('severity', e.target.value)}
                >
                  <option value="">All Severities</option>
                  {HAZARD_SEVERITIES.map(severity => (
                    <option key={severity} value={severity}>{severity}</option>
                  ))}
                </CFormSelect>
              </CCol>
            </CRow>

            {isLoading ? (
              <div className="text-center py-4">
                <CSpinner />
              </div>
            ) : (
              <>
                {data && data.hazards.length > 0 ? (
                  <>
                    <CTable striped hover responsive>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Title</CTableHeaderCell>
                          <CTableHeaderCell>Category</CTableHeaderCell>
                          <CTableHeaderCell>Severity</CTableHeaderCell>
                          <CTableHeaderCell>Status</CTableHeaderCell>
                          <CTableHeaderCell>Location</CTableHeaderCell>
                          <CTableHeaderCell>Risk Level</CTableHeaderCell>
                          <CTableHeaderCell>Date Reported</CTableHeaderCell>
                          <CTableHeaderCell>Actions</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {data.hazards.map(hazard => (
                          <CTableRow key={hazard.id}>
                            <CTableDataCell>
                              <Link to={`/hazards/${hazard.id}`} className="text-decoration-none">
                                {hazard.title}
                              </Link>
                            </CTableDataCell>
                            <CTableDataCell>{hazard.category}</CTableDataCell>
                            <CTableDataCell>
                              <CBadge color={getSeverityColor(hazard.severity)}>
                                {hazard.severity}
                              </CBadge>
                            </CTableDataCell>
                            <CTableDataCell>
                              <CBadge color={getStatusColor(hazard.status)}>
                                {hazard.status}
                              </CBadge>
                            </CTableDataCell>
                            <CTableDataCell>{hazard.location}</CTableDataCell>
                            <CTableDataCell>
                              {hazard.currentRiskLevel ? (
                                <CBadge color={hazard.currentRiskLevel === 'Critical' ? 'danger' : 'warning'}>
                                  {hazard.currentRiskLevel}
                                </CBadge>
                              ) : (
                                <span className="text-muted">Not assessed</span>
                              )}
                            </CTableDataCell>
                            <CTableDataCell>
                              {new Date(hazard.identifiedDate).toLocaleDateString()}
                            </CTableDataCell>
                            <CTableDataCell>
                              <CButton
                                color="primary"
                                variant="outline"
                                size="sm"
                                onClick={() => navigate(`/hazards/${hazard.id}`)}
                              >
                                <FontAwesomeIcon icon={faEye} />
                              </CButton>
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>

                    {/* Pagination */}
                    {data.totalPages > 1 && (
                      <CPagination className="justify-content-center">
                        <CPaginationItem
                          disabled={!data.hasPreviousPage}
                          onClick={() => handleFilterChange('pageNumber', data.pageNumber - 1)}
                        >
                          Previous
                        </CPaginationItem>
                        {Array.from({ length: data.totalPages }, (_, i) => i + 1).map(page => (
                          <CPaginationItem
                            key={page}
                            active={page === data.pageNumber}
                            onClick={() => handleFilterChange('pageNumber', page)}
                          >
                            {page}
                          </CPaginationItem>
                        ))}
                        <CPaginationItem
                          disabled={!data.hasNextPage}
                          onClick={() => handleFilterChange('pageNumber', data.pageNumber + 1)}
                        >
                          Next
                        </CPaginationItem>
                      </CPagination>
                    )}

                    {/* Summary */}
                    <div className="mt-3 text-muted">
                      Showing {data.hazards.length} of {data.totalCount} hazards you reported
                    </div>
                  </>
                ) : (
                  <CAlert color="info">
                    <h5>No hazards found</h5>
                    <p>You haven't reported any hazards yet. Click the "Report New Hazard" button to get started.</p>
                  </CAlert>
                )}
              </>
            )}
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default MyHazards;