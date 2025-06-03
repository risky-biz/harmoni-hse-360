import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
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
  CBadge,
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
import CIcon from '@coreui/icons-react';
import {
  cilFile,
  cilWarning,
  cilClipboard,
  cilTask,
  cilShieldAlt,
} from '@coreui/icons';

// Types
interface Incident {
  id: number;
  title: string;
  description: string;
  severity: 'Minor' | 'Moderate' | 'Serious' | 'Critical';
  status: 'Reported' | 'UnderInvestigation' | 'AwaitingAction' | 'Resolved' | 'Closed';
  incidentDate: string;
  location: string;
  reporterName: string;
  createdAt: string;
}

const IncidentList: React.FC = () => {
  const navigate = useNavigate();
  const [incidents, setIncidents] = useState<Incident[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);

  // Debug logging
  console.log('IncidentList component rendering');

  // Mock data for demonstration
  useEffect(() => {
    const loadIncidents = async () => {
      try {
        setLoading(true);
        
        // Simulate API call delay
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        // Mock data (replace with actual API call)
        const mockIncidents: Incident[] = [
          {
            id: 1,
            title: 'Student injured in Chemistry Lab',
            description: 'Student suffered minor burns during experiment',
            severity: 'Moderate',
            status: 'UnderInvestigation',
            incidentDate: '2025-06-02T14:30:00Z',
            location: 'Chemistry Lab - Room 205',
            reporterName: 'Dr. Sarah Johnson',
            createdAt: '2025-06-02T14:35:00Z'
          },
          {
            id: 2,
            title: 'Slip and fall in hallway',
            description: 'Wet floor caused student to slip near entrance',
            severity: 'Minor',
            status: 'Resolved',
            incidentDate: '2025-06-01T09:15:00Z',
            location: 'Main Building - Ground Floor',
            reporterName: 'Mr. David Wilson',
            createdAt: '2025-06-01T09:20:00Z'
          },
          {
            id: 3,
            title: 'Fire alarm system malfunction',
            description: 'False alarm triggered evacuating entire building',
            severity: 'Serious',
            status: 'AwaitingAction',
            incidentDate: '2025-05-30T11:45:00Z',
            location: 'East Wing - 3rd Floor',
            reporterName: 'Ms. Emily Chen',
            createdAt: '2025-05-30T11:50:00Z'
          }
        ];
        
        setIncidents(mockIncidents);
        setError(null);
      } catch (err) {
        setError('Failed to load incidents. Please try again.');
        console.error('Error loading incidents:', err);
      } finally {
        setLoading(false);
      }
    };

    loadIncidents();
  }, []);

  // Helper functions
  const getSeverityBadge = (severity: string) => {
    const variants = {
      Minor: 'success',
      Moderate: 'warning',
      Serious: 'danger',
      Critical: 'dark'
    };
    return <CBadge color={variants[severity as keyof typeof variants]}>{severity}</CBadge>;
  };

  const getStatusBadge = (status: string) => {
    const variants = {
      Reported: 'info',
      UnderInvestigation: 'warning',
      AwaitingAction: 'danger',
      Resolved: 'success',
      Closed: 'secondary'
    };
    const icons = {
      Reported: cilClipboard,
      UnderInvestigation: cilWarning,
      AwaitingAction: cilWarning,
      Resolved: cilTask,
      Closed: cilShieldAlt
    };
    
    return (
      <CBadge color={variants[status as keyof typeof variants]}>
        <CIcon icon={icons[status as keyof typeof icons]} size="sm" className="me-1" />
        {status.replace(/([A-Z])/g, ' $1').trim()}
      </CBadge>
    );
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  // Filter incidents
  const filteredIncidents = incidents.filter(incident => {
    const matchesSearch = incident.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         incident.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         incident.location.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = !statusFilter || incident.status === statusFilter;
    const matchesSeverity = !severityFilter || incident.severity === severityFilter;
    
    return matchesSearch && matchesStatus && matchesSeverity;
  });

  // Pagination
  const totalPages = Math.ceil(filteredIncidents.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const paginatedIncidents = filteredIncidents.slice(startIndex, startIndex + itemsPerPage);

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
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
              <h4 className="mb-0" style={{ color: 'var(--harmoni-charcoal)', fontFamily: 'Poppins, sans-serif' }}>
                Incident Reports
              </h4>
              <small className="text-muted">Manage and track all incident reports</small>
            </div>
            <CButton
              color="primary"
              onClick={() => navigate('/incidents/create')}
              className="d-flex align-items-center"
            >
              <CIcon icon={cilClipboard} size="sm" className="me-2" />
              Report Incident
            </CButton>
          </CCardHeader>
          
          <CCardBody>
            {error && (
              <CAlert color="danger" dismissible onClose={() => setError(null)}>
                {error}
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
                    <CIcon icon={cilFile} />
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
                  <option value="UnderInvestigation">Under Investigation</option>
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
                  <CIcon icon={cilFile} size="sm" className="me-2" />
                  Clear
                </CButton>
              </CCol>
            </CRow>

            {/* Incidents Table */}
            {filteredIncidents.length === 0 ? (
              <div className="text-center py-5">
                <CIcon icon={cilWarning} className="text-muted mb-3" style={{ fontSize: '3rem' }} />
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
                  <CIcon icon={cilClipboard} size="sm" className="me-2" />
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
                    {paginatedIncidents.map((incident) => (
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
                        <CTableDataCell>{getSeverityBadge(incident.severity)}</CTableDataCell>
                        <CTableDataCell>{getStatusBadge(incident.status)}</CTableDataCell>
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
                            <CDropdownToggle color="light" size="sm" caret={false}>
                              <CIcon icon={cilTask} />
                            </CDropdownToggle>
                            <CDropdownMenu>
                              <CDropdownItem onClick={() => navigate(`/incidents/${incident.id}`)}>
                                <CIcon icon={cilFile} size="sm" className="me-2" />
                                View Details
                              </CDropdownItem>
                              <CDropdownItem onClick={() => navigate(`/incidents/${incident.id}/edit`)}>
                                <CIcon icon={cilFile} size="sm" className="me-2" />
                                Edit
                              </CDropdownItem>
                              <CDropdownItem style={{ borderTop: '1px solid #dee2e6' }}></CDropdownItem>
                              <CDropdownItem className="text-danger">
                                <CIcon icon={cilWarning} size="sm" className="me-2" />
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
                      Showing {startIndex + 1} to {Math.min(startIndex + itemsPerPage, filteredIncidents.length)} of {filteredIncidents.length} incidents
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