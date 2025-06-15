import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CRow,
  CCol,
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
  CButton,
} from '@coreui/react';
import { Link, useNavigate } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEye, faPlus } from '@fortawesome/free-solid-svg-icons';
import { useGetUnassessedHazardsQuery } from '../../features/hazards/hazardApi';
import { 
  HAZARD_CATEGORIES, 
  HAZARD_SEVERITIES,
  GetHazardsParams 
} from '../../types/hazard';

const RiskAssessments: React.FC = () => {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<GetHazardsParams>({
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    category: '',
    severity: '',
    onlyUnassessed: true,
  });

  const { data, isLoading, error } = useGetUnassessedHazardsQuery(filters);

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

  const getDaysOpen = (identifiedDate: string) => {
    const today = new Date();
    const identified = new Date(identifiedDate);
    const diffTime = Math.abs(today.getTime() - identified.getTime());
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  };

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load risk assessments. Please try again.
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="mb-4">
          <CCardHeader>
            <div className="d-flex justify-content-between align-items-center">
              <div>
                <strong>Risk Assessments</strong>
                <div className="text-muted small">Hazards requiring risk assessment</div>
              </div>
              <CButton 
                color="primary" 
                onClick={() => navigate('/hazards')}
              >
                <FontAwesomeIcon icon={faEye} className="me-2" />
                View All Hazards
              </CButton>
            </div>
          </CCardHeader>
          <CCardBody>
            {/* Summary Stats */}
            {data && (
              <CRow className="mb-4">
                <CCol md={3}>
                  <div className="border-start border-warning ps-3">
                    <div className="text-warning">Pending Assessments</div>
                    <div className="fs-5 fw-semibold">{data.summary.unassessedHazards}</div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-danger ps-3">
                    <div className="text-danger">High Severity</div>
                    <div className="fs-5 fw-semibold">
                      {data.hazards.filter(h => h.severity === 'Major' || h.severity === 'Catastrophic').length}
                    </div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-info ps-3">
                    <div className="text-info">Average Days Open</div>
                    <div className="fs-5 fw-semibold">
                      {data.hazards.length > 0 
                        ? Math.round(data.hazards.reduce((sum, h) => sum + getDaysOpen(h.identifiedDate), 0) / data.hazards.length)
                        : 0
                      }
                    </div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-secondary ps-3">
                    <div className="text-secondary">Total Hazards</div>
                    <div className="fs-5 fw-semibold">{data.totalCount}</div>
                  </div>
                </CCol>
              </CRow>
            )}

            {/* Filters */}
            <CRow className="mb-3">
              <CCol md={4}>
                <CFormInput
                  placeholder="Search hazards..."
                  value={filters.searchTerm}
                  onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                />
              </CCol>
              <CCol md={4}>
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
              <CCol md={4}>
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
                    <CAlert color="warning" className="mb-3">
                      <strong>Assessment Required:</strong> These hazards need risk assessment to determine appropriate mitigation strategies.
                    </CAlert>

                    <CTable striped hover responsive>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Priority</CTableHeaderCell>
                          <CTableHeaderCell>Title</CTableHeaderCell>
                          <CTableHeaderCell>Category</CTableHeaderCell>
                          <CTableHeaderCell>Severity</CTableHeaderCell>
                          <CTableHeaderCell>Location</CTableHeaderCell>
                          <CTableHeaderCell>Reporter</CTableHeaderCell>
                          <CTableHeaderCell>Days Open</CTableHeaderCell>
                          <CTableHeaderCell>Actions</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {data.hazards.map(hazard => {
                          const daysOpen = getDaysOpen(hazard.identifiedDate);
                          const isUrgent = daysOpen > 7 || hazard.severity === 'Major' || hazard.severity === 'Catastrophic';
                          
                          return (
                            <CTableRow key={hazard.id} className={isUrgent ? 'table-warning' : ''}>
                              <CTableDataCell>
                                {isUrgent ? (
                                  <CBadge color="danger">Urgent</CBadge>
                                ) : (
                                  <CBadge color="info">Normal</CBadge>
                                )}
                              </CTableDataCell>
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
                              <CTableDataCell>{hazard.location}</CTableDataCell>
                              <CTableDataCell>{hazard.reporterName}</CTableDataCell>
                              <CTableDataCell>
                                <span className={daysOpen > 7 ? 'text-danger fw-bold' : ''}>
                                  {daysOpen}
                                </span>
                              </CTableDataCell>
                              <CTableDataCell>
                                <div className="d-flex gap-1">
                                  <CButton
                                    color="primary"
                                    variant="outline"
                                    size="sm"
                                    onClick={() => navigate(`/hazards/${hazard.id}`)}
                                  >
                                    <FontAwesomeIcon icon={faEye} />
                                  </CButton>
                                  <CButton
                                    color="success"
                                    size="sm"
                                    onClick={() => navigate(`/hazards/${hazard.id}#assessment`)}
                                  >
                                    <FontAwesomeIcon icon={faPlus} className="me-1" />
                                    Assess
                                  </CButton>
                                </div>
                              </CTableDataCell>
                            </CTableRow>
                          );
                        })}
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
                      Showing {data.hazards.length} of {data.totalCount} hazards requiring assessment
                    </div>
                  </>
                ) : (
                  <CAlert color="success">
                    <h5>All hazards have been assessed!</h5>
                    <p>There are currently no hazards requiring risk assessment. Great work on maintaining safety standards!</p>
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

export default RiskAssessments;