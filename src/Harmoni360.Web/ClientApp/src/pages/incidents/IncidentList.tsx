import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
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
  CFormSelect,
  CInputGroup,
  CFormInput,
  CSpinner,
  CAlert,
  CPagination,
  CPaginationItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import {
  useGetIncidentsQuery,
  useDeleteIncidentMutation,
} from '../../features/incidents/incidentApi';
// import type { IncidentDto } from '../../features/incidents/incidentApi';
import {
  getSeverityBadge,
  getStatusBadge,
  formatDate,
} from '../../utils/incidentUtils';

const IncidentList: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  // Use RTK Query to fetch incidents
  const { data, error, isLoading, refetch } = useGetIncidentsQuery(
    {
      pageNumber: currentPage,
      pageSize: itemsPerPage,
      status: statusFilter || undefined,
      severity: severityFilter || undefined,
      searchTerm: searchTerm || undefined,
    },
    {
      // Ensure we refetch when the component mounts
      refetchOnMountOrArgChange: true,
      // Refetch when the window regains focus
      refetchOnFocus: true,
    }
  );

  const [deleteIncident, { isLoading: isDeleting }] =
    useDeleteIncidentMutation();

  // Extract incidents from response
  const incidents = data?.incidents || [];
  const totalCount = data?.totalCount || 0;
  const totalPages = data?.totalPages || 0;

  // Handle success message from navigation state
  useEffect(() => {
    if (location.state?.message) {
      setSuccessMessage(location.state.message);
      // Clear the navigation state
      window.history.replaceState({}, document.title);
      // Auto-hide message after 5 seconds
      const timer = setTimeout(() => setSuccessMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [location.state]);

  // Helper functions (removed - now imported from incidentUtils)

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading incidents...</span>
      </div>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4
                className="mb-0"
                style={{
                  color: 'var(--harmoni-charcoal)',
                  fontFamily: 'Poppins, sans-serif',
                }}
              >
                Incident Reports
              </h4>
              <small className="text-muted">
                Manage and track all incident reports
              </small>
            </div>
            <div className="d-flex gap-2">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => {
                  console.log('Manual refresh triggered');
                  refetch();
                }}
                title="Refresh incident list"
              >
                <FontAwesomeIcon icon={ACTION_ICONS.refresh} className="me-2" />
                Refresh
              </CButton>
              <CButton
                color="primary"
                onClick={() => navigate('/incidents/create')}
                className="d-flex align-items-center"
              >
                <FontAwesomeIcon
                  icon={ACTION_ICONS.create}
                  size="sm"
                  className="me-2"
                />
                Report Incident
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            {successMessage && (
              <CAlert
                color="success"
                dismissible
                onClose={() => setSuccessMessage(null)}
              >
                {successMessage}
              </CAlert>
            )}

            {error && (
              <CAlert color="danger" dismissible onClose={() => refetch()}>
                {typeof error === 'string'
                  ? error
                  : 'Failed to load incidents. Please try again.'}
              </CAlert>
            )}

            {/* Filters and Search */}
            <CRow className="mb-4">
              <CCol md={4}>
                <CInputGroup>
                  <CFormInput
                    placeholder="Search incidents..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                  <CButton type="button" color="primary" variant="outline">
                    <FontAwesomeIcon icon={ACTION_ICONS.search} />
                  </CButton>
                </CInputGroup>
              </CCol>
              <CCol md={3}>
                <CFormSelect
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
                >
                  <option value="">All Statuses</option>
                  <option value="Reported">Reported</option>
                  <option value="UnderInvestigation">
                    Under Investigation
                  </option>
                  <option value="AwaitingAction">Awaiting Action</option>
                  <option value="Resolved">Resolved</option>
                  <option value="Closed">Closed</option>
                </CFormSelect>
              </CCol>
              <CCol md={3}>
                <CFormSelect
                  value={severityFilter}
                  onChange={(e) => setSeverityFilter(e.target.value)}
                >
                  <option value="">All Severities</option>
                  <option value="Minor">Minor</option>
                  <option value="Moderate">Moderate</option>
                  <option value="Serious">Serious</option>
                  <option value="Critical">Critical</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CButton
                  color="secondary"
                  variant="outline"
                  className="w-100"
                  onClick={() => {
                    setSearchTerm('');
                    setStatusFilter('');
                    setSeverityFilter('');
                    setCurrentPage(1);
                  }}
                >
                  <FontAwesomeIcon
                    icon={ACTION_ICONS.cancel}
                    size="sm"
                    className="me-2"
                  />
                  Clear
                </CButton>
              </CCol>
            </CRow>

            {/* Incidents Table */}
            {incidents.length === 0 ? (
              <div className="text-center py-5">
                <FontAwesomeIcon
                  icon={CONTEXT_ICONS.incident}
                  className="text-muted mb-3"
                  style={{ fontSize: '3rem' }}
                />
                <h5 className="text-muted">No incidents found</h5>
                <p className="text-muted">
                  {searchTerm || statusFilter || severityFilter
                    ? 'Try adjusting your filters or search criteria.'
                    : 'No incidents have been reported yet.'}
                </p>
                <CButton
                  color="primary"
                  onClick={() => navigate('/incidents/create')}
                  className="mt-3"
                >
                  <FontAwesomeIcon
                    icon={ACTION_ICONS.create}
                    size="sm"
                    className="me-2"
                  />
                  Report First Incident
                </CButton>
              </div>
            ) : (
              <>
                <CTable responsive hover>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell scope="col">Title</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Severity</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Status</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Location</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Reporter</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Date</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {incidents.map((incident) => (
                      <CTableRow key={incident.id}>
                        <CTableDataCell>
                          <div>
                            <strong>{incident.title}</strong>
                            <br />
                            <small className="text-muted">
                              {incident.description.length > 50
                                ? `${incident.description.substring(0, 50)}...`
                                : incident.description}
                            </small>
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          {getSeverityBadge(incident.severity)}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getStatusBadge(incident.status)}
                        </CTableDataCell>
                        <CTableDataCell>{incident.location}</CTableDataCell>
                        <CTableDataCell>{incident.reporterName}</CTableDataCell>
                        <CTableDataCell>
                          <div>
                            <small>{formatDate(incident.incidentDate)}</small>
                            <br />
                            <small className="text-muted">
                              Reported: {formatDate(incident.createdAt)}
                            </small>
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CDropdown>
                            <CDropdownToggle
                              color="light"
                              size="sm"
                              caret={false}
                            >
                              <FontAwesomeIcon icon={ACTION_ICONS.menu} />
                            </CDropdownToggle>
                            <CDropdownMenu>
                              <CDropdownItem
                                onClick={() =>
                                  navigate(`/incidents/${incident.id}`)
                                }
                              >
                                <FontAwesomeIcon
                                  icon={ACTION_ICONS.view}
                                  size="sm"
                                  className="me-2"
                                />
                                View Details
                              </CDropdownItem>
                              <CDropdownItem
                                onClick={() =>
                                  navigate(`/incidents/${incident.id}/edit`)
                                }
                              >
                                <FontAwesomeIcon
                                  icon={ACTION_ICONS.edit}
                                  size="sm"
                                  className="me-2"
                                />
                                Edit
                              </CDropdownItem>
                              <CDropdownItem
                                style={{ borderTop: '1px solid #dee2e6' }}
                              ></CDropdownItem>
                              <CDropdownItem
                                className="text-danger"
                                onClick={async () => {
                                  if (
                                    window.confirm(
                                      `Are you sure you want to delete "${incident.title}"? This action cannot be undone.`
                                    )
                                  ) {
                                    try {
                                      await deleteIncident(
                                        incident.id
                                      ).unwrap();
                                      // Optimistic update should handle immediate UI update
                                      // Force refresh as backup only if needed
                                    } catch (error) {
                                      console.error(
                                        'Failed to delete incident:',
                                        error
                                      );
                                      alert(
                                        'Failed to delete incident. Please try again.'
                                      );
                                    }
                                  }
                                }}
                                disabled={isDeleting}
                              >
                                <FontAwesomeIcon
                                  icon={ACTION_ICONS.delete}
                                  size="sm"
                                  className="me-2"
                                />
                                Delete
                              </CDropdownItem>
                            </CDropdownMenu>
                          </CDropdown>
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="d-flex justify-content-between align-items-center mt-4">
                    <div className="text-muted">
                      Showing {(currentPage - 1) * itemsPerPage + 1} to{' '}
                      {Math.min(currentPage * itemsPerPage, totalCount)} of{' '}
                      {totalCount} incidents
                    </div>
                    <CPagination aria-label="Incidents pagination">
                      <CPaginationItem
                        disabled={currentPage === 1}
                        onClick={() => setCurrentPage(currentPage - 1)}
                      >
                        Previous
                      </CPaginationItem>
                      {[...Array(totalPages)].map((_, index) => (
                        <CPaginationItem
                          key={index + 1}
                          active={currentPage === index + 1}
                          onClick={() => setCurrentPage(index + 1)}
                        >
                          {index + 1}
                        </CPaginationItem>
                      ))}
                      <CPaginationItem
                        disabled={currentPage === totalPages}
                        onClick={() => setCurrentPage(currentPage + 1)}
                      >
                        Next
                      </CPaginationItem>
                    </CPagination>
                  </div>
                )}
              </>
            )}
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default IncidentList;
