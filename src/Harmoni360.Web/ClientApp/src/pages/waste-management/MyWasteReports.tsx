import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CBadge,
  CSpinner,
  CAlert,
  CButton,
  CInputGroup,
  CFormInput,
  CPagination,
  CPaginationItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CCallout,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faSearch, 
  faPlus, 
  faEllipsisV,
  faEye,
  faEdit,
  faTrash,
  faFileAlt,
  faCalendarAlt,
  faMapMarkerAlt,
  faUser
} from '@fortawesome/free-solid-svg-icons';
import { useGetMyWasteReportsQuery } from '../../api/wasteManagementApi';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';

const MyWasteReports: React.FC = () => {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');

  const { data, isLoading, error, refetch } = useGetMyWasteReportsQuery({
    page,
    pageSize,
  });

  // Filter data on frontend since backend doesn't support search
  const filteredData = React.useMemo(() => {
    if (!data?.items) return data;
    
    const filteredItems = search
      ? data.items.filter(report => 
          report.title.toLowerCase().includes(search.toLowerCase()) ||
          report.description?.toLowerCase().includes(search.toLowerCase()) ||
          (report.classificationDisplay && report.classificationDisplay.toLowerCase().includes(search.toLowerCase()))
        )
      : data.items;
    
    return {
      ...data,
      items: filteredItems,
      totalCount: filteredItems.length
    };
  }, [data, search]);

  const getStatusColor = (status: any) => {
    if (!status || typeof status !== 'string') {
      return 'secondary';
    }
    switch (status.toLowerCase()) {
      case 'pending': return 'warning';
      case 'disposed': return 'success';
      case 'intransit': return 'info';
      case 'approved': return 'primary';
      case 'rejected': return 'danger';
      default: return 'secondary';
    }
  };

  // Note: Backend doesn't support sorting, so this is just for UI consistency
  const [sortBy, setSortBy] = useState('generatedDate');
  const [sortDescending, setSortDescending] = useState(true);
  
  const handleSort = (field: string) => {
    if (sortBy === field) {
      setSortDescending(!sortDescending);
    } else {
      setSortBy(field);
      setSortDescending(true);
    }
  };

  const getSortIcon = (field: string) => {
    if (sortBy !== field) return null;
    return sortDescending ? '↓' : '↑';
  };

  const handleView = (reportId: number) => {
    navigate(`/waste/reports/${reportId}`);
  };

  const handleEdit = (reportId: number) => {
    navigate(`/waste/reports/edit/${reportId}`);
  };

  if (error) {
    return (
      <CRow>
        <CCol>
          <CAlert color="danger">
            <FontAwesomeIcon icon={faFileAlt} className="me-2" />
            Failed to load your waste reports. Please try again.
            <CButton 
              color="danger" 
              variant="outline" 
              size="sm" 
              className="ms-2"
              onClick={() => refetch()}
            >
              Retry
            </CButton>
          </CAlert>
        </CCol>
      </CRow>
    );
  }

  return (
    <>
      <CRow>
        <CCol>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div>
                <h5 className="mb-0">
                  <FontAwesomeIcon icon={faUser} className="me-2 text-primary" />
                  My Waste Reports
                </h5>
                <small className="text-muted">
                  View and manage waste reports that you have submitted
                </small>
              </div>
              <CButton
                color="primary"
                onClick={() => navigate('/waste/reports/create')}
              >
                <FontAwesomeIcon icon={faPlus} className="me-2" />
                New Report
              </CButton>
            </CCardHeader>
            <CCardBody>
              {/* Search and Filters */}
              <CRow className="mb-3">
                <CCol md={6}>
                  <CInputGroup>
                    <CFormInput
                      placeholder="Search my reports..."
                      value={search}
                      onChange={(e) => setSearch(e.target.value)}
                    />
                    <CButton color="outline-secondary" type="button">
                      <FontAwesomeIcon icon={faSearch} />
                    </CButton>
                  </CInputGroup>
                </CCol>
                <CCol md={3}>
                  <select 
                    className="form-select"
                    value={pageSize}
                    onChange={(e) => setPageSize(Number(e.target.value))}
                  >
                    <option value={10}>10 per page</option>
                    <option value={20}>20 per page</option>
                    <option value={50}>50 per page</option>
                  </select>
                </CCol>
              </CRow>

              {isLoading ? (
                <div className="text-center py-4">
                  <CSpinner color="primary" />
                  <div className="mt-2">Loading your waste reports...</div>
                </div>
              ) : filteredData?.items && filteredData.items.length > 0 ? (
                <>
                  <div className="table-responsive">
                    <CTable hover>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell
                            scope="col"
                            style={{ cursor: 'pointer' }}
                            onClick={() => handleSort('title')}
                          >
                            Title {getSortIcon('title')}
                          </CTableHeaderCell>
                          <CTableHeaderCell
                            scope="col"
                            style={{ cursor: 'pointer' }}
                            onClick={() => handleSort('category')}
                          >
                            Category {getSortIcon('category')}
                          </CTableHeaderCell>
                          <CTableHeaderCell
                            scope="col"
                            style={{ cursor: 'pointer' }}
                            onClick={() => handleSort('status')}
                          >
                            Status {getSortIcon('status')}
                          </CTableHeaderCell>
                          <CTableHeaderCell
                            scope="col"
                            style={{ cursor: 'pointer' }}
                            onClick={() => handleSort('generatedDate')}
                          >
                            <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                            Date {getSortIcon('generatedDate')}
                          </CTableHeaderCell>
                          <CTableHeaderCell scope="col">
                            <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                            Location
                          </CTableHeaderCell>
                          <CTableHeaderCell scope="col" style={{ width: '100px' }}>Actions</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {filteredData.items.map((report) => (
                          <CTableRow key={report.id}>
                            <CTableDataCell>
                              <div>
                                <strong>{report.title}</strong>
                                {report.description && (
                                  <div className="small text-muted text-truncate" style={{maxWidth: '200px'}}>
                                    {report.description}
                                  </div>
                                )}
                              </div>
                            </CTableDataCell>
                            <CTableDataCell>
                              <CBadge color="info">{report.classificationDisplay || 'Unknown'}</CBadge>
                            </CTableDataCell>
                            <CTableDataCell>
                              <CBadge color={getStatusColor(report.statusDisplay)}>
                                {report.statusDisplay || 'Unknown'}
                              </CBadge>
                            </CTableDataCell>
                            <CTableDataCell>
                              <div className="small">
                                {format(new Date(report.reportDate), 'MMM dd, yyyy')}
                              </div>
                              <div className="small text-muted">
                                {format(new Date(report.reportDate), 'HH:mm')}
                              </div>
                            </CTableDataCell>
                            <CTableDataCell>
                              <div className="small text-truncate" style={{maxWidth: '150px'}}>
                                {report.location || 'Not specified'}
                              </div>
                            </CTableDataCell>
                            <CTableDataCell>
                              <CDropdown variant="btn-group">
                                <CDropdownToggle color="ghost" size="sm">
                                  <FontAwesomeIcon icon={faEllipsisV} />
                                </CDropdownToggle>
                                <CDropdownMenu>
                                  <CDropdownItem onClick={() => handleView(report.id)}>
                                    <FontAwesomeIcon icon={faEye} className="me-2" />
                                    View Details
                                  </CDropdownItem>
                                  <CDropdownItem onClick={() => handleEdit(report.id)}>
                                    <FontAwesomeIcon icon={faEdit} className="me-2" />
                                    Edit Report
                                  </CDropdownItem>
                                </CDropdownMenu>
                              </CDropdown>
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                  </div>

                  {/* Pagination */}
                  {data && data.totalPages > 1 && (
                    <div className="d-flex justify-content-between align-items-center mt-3">
                      <div className="text-muted">
                        Showing {((page - 1) * pageSize) + 1} to {Math.min(page * pageSize, data.totalCount)} of {data.totalCount} reports
                        {search && ` (filtered from ${data.totalCount} total)`}
                      </div>
                      <CPagination>
                        <CPaginationItem
                          disabled={!data.hasPreviousPage}
                          onClick={() => setPage(page - 1)}
                        >
                          Previous
                        </CPaginationItem>
                        {Array.from({ length: Math.min(5, data.totalPages) }, (_, i) => {
                          const pageNum = i + 1;
                          return (
                            <CPaginationItem
                              key={pageNum}
                              active={pageNum === page}
                              onClick={() => setPage(pageNum)}
                            >
                              {pageNum}
                            </CPaginationItem>
                          );
                        })}
                        <CPaginationItem
                          disabled={!data.hasNextPage}
                          onClick={() => setPage(page + 1)}
                        >
                          Next
                        </CPaginationItem>
                      </CPagination>
                    </div>
                  )}
                </>
              ) : (
                <CCallout color="info">
                  <div className="text-center py-4">
                    <FontAwesomeIcon icon={faFileAlt} size="3x" className="text-muted mb-3" />
                    <h5>No Waste Reports Found</h5>
                    <p className="text-muted mb-3">
                      You haven't submitted any waste reports yet.
                    </p>
                    <CButton
                      color="primary"
                      onClick={() => navigate('/waste/reports/create')}
                    >
                      <FontAwesomeIcon icon={faPlus} className="me-2" />
                      Create Your First Report
                    </CButton>
                  </div>
                </CCallout>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default MyWasteReports;