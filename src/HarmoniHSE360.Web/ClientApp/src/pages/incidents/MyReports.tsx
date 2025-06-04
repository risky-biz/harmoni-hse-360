import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CSpinner,
  CAlert,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CFormInput,
  CFormSelect,
  CPagination,
  CPaginationItem,
  CInputGroup,
  CInputGroupText,
  CCallout,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import { Icon } from '../../components/common/Icon';
import { useGetMyIncidentsQuery, IncidentDto } from '../../features/incidents/incidentApi';
import { getSeverityBadge, getStatusBadge, formatDate } from '../../utils/incidentUtils';

const MyReports: React.FC = () => {
  const navigate = useNavigate();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');

  const { 
    data: myIncidentsResponse, 
    isLoading, 
    error,
    refetch 
  } = useGetMyIncidentsQuery({
    pageNumber,
    pageSize,
    searchTerm: searchTerm || undefined,
    status: statusFilter || undefined,
    severity: severityFilter || undefined,
  });

  // Helper functions (removed - now imported from incidentUtils)

  const handleSearch = () => {
    setPageNumber(1);
    refetch();
  };

  const handleClearFilters = () => {
    setSearchTerm('');
    setStatusFilter('');
    setSeverityFilter('');
    setPageNumber(1);
  };

  const incidents = myIncidentsResponse?.incidents || [];
  const totalCount = myIncidentsResponse?.totalCount || 0;
  const totalPages = Math.ceil(totalCount / pageSize);

  if (isLoading && !myIncidentsResponse) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading your reports...</span>
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load your reports. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => refetch()}>
            <FontAwesomeIcon icon={ACTION_ICONS.refresh} className="me-2" />
            Retry
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0" style={{ color: 'var(--harmoni-charcoal)', fontFamily: 'Poppins, sans-serif' }}>
                <FontAwesomeIcon icon={CONTEXT_ICONS.reports} size="lg" className="me-2 text-primary" />
                My Incident Reports
              </h4>
              <small className="text-muted">
                {totalCount} report{totalCount !== 1 ? 's' : ''} found
              </small>
            </div>
            <CButton
              color="primary"
              onClick={() => navigate('/incidents/create')}
              className="d-flex align-items-center"
            >
              <Icon icon={ACTION_ICONS.create} size="sm" className="me-2" />
              Report New Incident
            </CButton>
          </CCardHeader>

          <CCardBody>
            {/* Search and Filters */}
            <CRow className="mb-4">
              <CCol md={4}>
                <CInputGroup>
                  <CInputGroupText>
                    <Icon icon={ACTION_ICONS.search} />
                  </CInputGroupText>
                  <CFormInput
                    type="text"
                    placeholder="Search incidents..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                  />
                </CInputGroup>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
                >
                  <option value="">All Statuses</option>
                  <option value="Reported">Reported</option>
                  <option value="UnderInvestigation">Under Investigation</option>
                  <option value="AwaitingAction">Awaiting Action</option>
                  <option value="Resolved">Resolved</option>
                  <option value="Closed">Closed</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={severityFilter}
                  onChange={(e) => setSeverityFilter(e.target.value)}
                >
                  <option value="">All Severities</option>
                  <option value="Critical">Critical</option>
                  <option value="Serious">Serious</option>
                  <option value="Moderate">Moderate</option>
                  <option value="Minor">Minor</option>
                </CFormSelect>
              </CCol>
              <CCol md={4} className="d-flex gap-2">
                <CButton color="primary" onClick={handleSearch}>
                  <Icon icon={ACTION_ICONS.search} size="sm" className="me-1" />
                  Search
                </CButton>
                <CButton 
                  color="secondary" 
                  variant="outline" 
                  onClick={handleClearFilters}
                >
                  Clear
                </CButton>
              </CCol>
            </CRow>

            {incidents.length === 0 ? (
              <CCallout color="info" className="text-center py-4">
                <FontAwesomeIcon icon={CONTEXT_ICONS.incident} size="2xl" className="mb-3 text-muted" />
                <h5>No incidents found</h5>
                <p className="text-muted mb-3">
                  {searchTerm || statusFilter || severityFilter 
                    ? "No incidents match your current filters." 
                    : "You haven't reported any incidents yet."
                  }
                </p>
                <CButton 
                  color="primary" 
                  onClick={() => navigate('/incidents/create')}
                >
                  <Icon icon={ACTION_ICONS.create} size="sm" className="me-2" />
                  Report Your First Incident
                </CButton>
              </CCallout>
            ) : (
              <>
                {/* Incidents Table */}
                <CTable hover responsive className="border">
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell>Title</CTableHeaderCell>
                      <CTableHeaderCell>Severity</CTableHeaderCell>
                      <CTableHeaderCell>Status</CTableHeaderCell>
                      <CTableHeaderCell>Date</CTableHeaderCell>
                      <CTableHeaderCell>Location</CTableHeaderCell>
                      <CTableHeaderCell>Actions</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {incidents.map((incident: IncidentDto) => (
                      <CTableRow key={incident.id}>
                        <CTableDataCell>
                          <div>
                            <strong>{incident.title}</strong>
                            {incident.description && (
                              <div className="text-muted small">
                                {incident.description.length > 100 
                                  ? `${incident.description.substring(0, 100)}...`
                                  : incident.description
                                }
                              </div>
                            )}
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          {getSeverityBadge(incident.severity)}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getStatusBadge(incident.status)}
                        </CTableDataCell>
                        <CTableDataCell>
                          <small>{formatDate(incident.incidentDate)}</small>
                        </CTableDataCell>
                        <CTableDataCell>
                          <small>{incident.location}</small>
                        </CTableDataCell>
                        <CTableDataCell>
                          <div className="d-flex gap-1">
                            <CButton
                              color="info"
                              variant="outline"
                              size="sm"
                              onClick={() => navigate(`/incidents/${incident.id}`)}
                              title="View Details"
                            >
                              <Icon icon={ACTION_ICONS.view} size="sm" />
                            </CButton>
                            <CButton
                              color="primary"
                              variant="outline"
                              size="sm"
                              onClick={() => navigate(`/incidents/${incident.id}/edit`)}
                              title="Edit"
                            >
                              <Icon icon={ACTION_ICONS.edit} size="sm" />
                            </CButton>
                          </div>
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="d-flex justify-content-center mt-4">
                    <CPagination>
                      <CPaginationItem 
                        disabled={pageNumber === 1}
                        onClick={() => setPageNumber(pageNumber - 1)}
                      >
                        Previous
                      </CPaginationItem>
                      
                      {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                        const page = Math.max(1, Math.min(totalPages - 4, pageNumber - 2)) + i;
                        return (
                          <CPaginationItem
                            key={page}
                            active={page === pageNumber}
                            onClick={() => setPageNumber(page)}
                          >
                            {page}
                          </CPaginationItem>
                        );
                      })}
                      
                      <CPaginationItem 
                        disabled={pageNumber === totalPages}
                        onClick={() => setPageNumber(pageNumber + 1)}
                      >
                        Next
                      </CPaginationItem>
                    </CPagination>
                  </div>
                )}

                {/* Summary Info */}
                <div className="mt-3 text-center text-muted small">
                  Showing {incidents.length} of {totalCount} reports
                  {totalPages > 1 && ` (Page ${pageNumber} of ${totalPages})`}
                </div>
              </>
            )}
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default MyReports;