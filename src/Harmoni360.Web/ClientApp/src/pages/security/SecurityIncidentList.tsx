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
  CBadge,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import {
  useGetSecurityIncidentsQuery,
  useDeleteSecurityIncidentMutation,
} from '../../features/security/securityApi';
import type { SecurityIncidentList as SecurityIncidentListType } from '../../types/security';
import { SecurityIncidentType, SecuritySeverity, SecurityIncidentStatus, ThreatLevel } from '../../types/security';
import { formatDate } from '../../utils/dateUtils';

const SecurityIncidentList: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchTerm, setSearchTerm] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Use RTK Query to fetch security incidents
  const { data, error, isLoading, refetch } = useGetSecurityIncidentsQuery(
    {
      page: currentPage,
      pageSize: itemsPerPage,
      type: typeFilter || undefined,
      severity: severityFilter || undefined,
      status: statusFilter || undefined,
      searchTerm: searchTerm || undefined,
    },
    {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
    }
  );

  const [deleteSecurityIncident, { isLoading: isDeleting }] =
    useDeleteSecurityIncidentMutation();

  // Extract incidents from response
  const incidents = data?.items || [];
  const totalCount = data?.totalCount || 0;
  const totalPages = data?.totalPages || 0;

  // Handle success message from navigation state
  useEffect(() => {
    if (location.state?.message) {
      setSuccessMessage(location.state.message);
      window.history.replaceState({}, document.title);
      const timer = setTimeout(() => setSuccessMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [location.state]);

  const getSeverityBadge = (severity: SecuritySeverity) => {
    const colorMap = {
      [SecuritySeverity.Low]: 'success',
      [SecuritySeverity.Medium]: 'warning',
      [SecuritySeverity.High]: 'danger',
      [SecuritySeverity.Critical]: 'dark',
    };
    
    const textMap = {
      [SecuritySeverity.Low]: 'Low',
      [SecuritySeverity.Medium]: 'Medium',
      [SecuritySeverity.High]: 'High',
      [SecuritySeverity.Critical]: 'Critical',
    };

    return (
      <CBadge color={colorMap[severity]} shape="rounded-pill">
        {textMap[severity]}
      </CBadge>
    );
  };

  const getStatusBadge = (status: SecurityIncidentStatus) => {
    const colorMap = {
      [SecurityIncidentStatus.Open]: 'danger',
      [SecurityIncidentStatus.Assigned]: 'warning',
      [SecurityIncidentStatus.Investigating]: 'info',
      [SecurityIncidentStatus.Contained]: 'warning',
      [SecurityIncidentStatus.Eradicating]: 'info',
      [SecurityIncidentStatus.Recovering]: 'info',
      [SecurityIncidentStatus.Resolved]: 'success',
      [SecurityIncidentStatus.Closed]: 'secondary',
    };

    const textMap = {
      [SecurityIncidentStatus.Open]: 'Open',
      [SecurityIncidentStatus.Assigned]: 'Assigned',
      [SecurityIncidentStatus.Investigating]: 'Investigating',
      [SecurityIncidentStatus.Contained]: 'Contained',
      [SecurityIncidentStatus.Eradicating]: 'Eradicating',
      [SecurityIncidentStatus.Recovering]: 'Recovering',
      [SecurityIncidentStatus.Resolved]: 'Resolved',
      [SecurityIncidentStatus.Closed]: 'Closed',
    };

    return (
      <CBadge color={colorMap[status]} shape="rounded-pill">
        {textMap[status]}
      </CBadge>
    );
  };

  const getThreatLevelBadge = (threatLevel: ThreatLevel) => {
    const colorMap = {
      [ThreatLevel.Minimal]: 'success',
      [ThreatLevel.Low]: 'info',
      [ThreatLevel.Medium]: 'warning',
      [ThreatLevel.High]: 'danger',
      [ThreatLevel.Severe]: 'dark',
    };

    const textMap = {
      [ThreatLevel.Minimal]: 'Minimal',
      [ThreatLevel.Low]: 'Low',
      [ThreatLevel.Medium]: 'Medium',
      [ThreatLevel.High]: 'High',
      [ThreatLevel.Severe]: 'Severe',
    };

    return (
      <CBadge color={colorMap[threatLevel]} shape="rounded-pill">
        {textMap[threatLevel]}
      </CBadge>
    );
  };

  const getTypeText = (type: SecurityIncidentType) => {
    const typeMap = {
      [SecurityIncidentType.PhysicalSecurity]: 'Physical',
      [SecurityIncidentType.Cybersecurity]: 'Cyber',
      [SecurityIncidentType.PersonnelSecurity]: 'Personnel',
      [SecurityIncidentType.InformationSecurity]: 'Information',
    };
    return typeMap[type];
  };

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading security incidents...</span>
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
                Security Incidents
              </h4>
              <small className="text-muted">
                Manage and track all security incident reports
              </small>
            </div>
            <div className="d-flex gap-2">
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => refetch()}
                disabled={isLoading}
                title="Refresh security incident list"
              >
                <FontAwesomeIcon 
                  icon={ACTION_ICONS.refresh} 
                  className="me-2" 
                  spin={isLoading}
                />
                {isLoading ? 'Refreshing...' : 'Refresh'}
              </CButton>
              <CButton
                color="primary"
                onClick={() => navigate('/security/incidents/create')}
                className="d-flex align-items-center"
              >
                <FontAwesomeIcon
                  icon={ACTION_ICONS.create}
                  size="sm"
                  className="me-2"
                />
                Report Security Incident
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
                  : 'Failed to load security incidents. Please try again.'}
              </CAlert>
            )}

            {/* Filters and Search */}
            <CRow className="mb-4">
              <CCol md={3}>
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
              <CCol md={2}>
                <CFormSelect
                  value={typeFilter}
                  onChange={(e) => setTypeFilter(e.target.value)}
                >
                  <option value="">All Types</option>
                  <option value="1">Physical Security</option>
                  <option value="2">Cybersecurity</option>
                  <option value="3">Personnel Security</option>
                  <option value="4">Information Security</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={severityFilter}
                  onChange={(e) => setSeverityFilter(e.target.value)}
                >
                  <option value="">All Severities</option>
                  <option value="1">Low</option>
                  <option value="2">Medium</option>
                  <option value="3">High</option>
                  <option value="4">Critical</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
                >
                  <option value="">All Statuses</option>
                  <option value="1">Open</option>
                  <option value="2">Assigned</option>
                  <option value="3">Investigating</option>
                  <option value="4">Contained</option>
                  <option value="5">Eradicating</option>
                  <option value="6">Recovering</option>
                  <option value="7">Resolved</option>
                  <option value="8">Closed</option>
                </CFormSelect>
              </CCol>
              <CCol md={1}>
                <CButton
                  color="secondary"
                  variant="outline"
                  className="w-100"
                  onClick={() => {
                    setSearchTerm('');
                    setTypeFilter('');
                    setSeverityFilter('');
                    setStatusFilter('');
                    setCurrentPage(1);
                  }}
                  title="Clear all filters"
                >
                  <FontAwesomeIcon icon={ACTION_ICONS.cancel} size="sm" />
                </CButton>
              </CCol>
            </CRow>

            {/* Security Incidents Table */}
            {isLoading ? (
              <div className="text-center py-5">
                <CSpinner color="primary" className="mb-3" />
                <h5 className="text-muted">Loading security incidents...</h5>
                <p className="text-muted">
                  Please wait while we retrieve the latest security incident data.
                </p>
              </div>
            ) : incidents.length === 0 ? (
              <div className="text-center py-5">
                <FontAwesomeIcon
                  icon={CONTEXT_ICONS.incident}
                  className="text-muted mb-3"
                  style={{ fontSize: '3rem' }}
                />
                <h5 className="text-muted">No security incidents found</h5>
                <p className="text-muted">
                  {searchTerm || typeFilter || severityFilter || statusFilter
                    ? 'Try adjusting your filters or search criteria.'
                    : 'No security incidents have been reported yet.'}
                </p>
                <CButton
                  color="primary"
                  onClick={() => navigate('/security/incidents/create')}
                  className="mt-3"
                >
                  <FontAwesomeIcon
                    icon={ACTION_ICONS.create}
                    size="sm"
                    className="me-2"
                  />
                  Report First Security Incident
                </CButton>
              </div>
            ) : (
              <>
                <CTable responsive hover>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell scope="col">Incident #</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Title</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Type</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Severity</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Status</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Threat Level</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Location</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Reporter</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Date</CTableHeaderCell>
                      <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {incidents.map((incident: SecurityIncidentListType) => (
                      <CTableRow key={incident.id} className={incident.isOverdue ? 'table-warning' : ''}>
                        <CTableDataCell>
                          <strong>{incident.incidentNumber}</strong>
                          {incident.isOverdue && (
                            <CBadge color="warning" shape="rounded-pill" className="ms-2">
                              Overdue
                            </CBadge>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>
                          <div>
                            <strong>{incident.title}</strong>
                            <br />
                            <small className="text-muted">
                              {incident.daysOpen > 0 && `${incident.daysOpen} days open`}
                            </small>
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color="info" shape="rounded-pill">
                            {getTypeText(incident.incidentType)}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          {getSeverityBadge(incident.severity)}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getStatusBadge(incident.status)}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getThreatLevelBadge(incident.threatLevel)}
                        </CTableDataCell>
                        <CTableDataCell>{incident.location}</CTableDataCell>
                        <CTableDataCell>
                          {incident.reporterName || 'Anonymous'}
                        </CTableDataCell>
                        <CTableDataCell>
                          <div>
                            <small>{formatDate(incident.incidentDateTime)}</small>
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
                                  navigate(`/security/incidents/${incident.id}`)
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
                                  navigate(`/security/incidents/${incident.id}/edit`)
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
                                onClick={() =>
                                  navigate(`/security/incidents/${incident.id}/threat-assessment`)
                                }
                              >
                                <FontAwesomeIcon
                                  icon={CONTEXT_ICONS.hazard}
                                  size="sm"
                                  className="me-2"
                                />
                                Threat Assessment
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
                                      await deleteSecurityIncident(
                                        incident.id
                                      ).unwrap();
                                    } catch (error) {
                                      console.error(
                                        'Failed to delete security incident:',
                                        error
                                      );
                                      alert(
                                        'Failed to delete security incident. Please try again.'
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
                      {totalCount} security incidents
                    </div>
                    <CPagination aria-label="Security incidents pagination">
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

export default SecurityIncidentList;