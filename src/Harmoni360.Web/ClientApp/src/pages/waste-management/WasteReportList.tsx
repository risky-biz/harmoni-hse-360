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
  CFormSelect,
  CPagination,
  CPaginationItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
} from '@coreui/react';
import { cilSearch, cilPlus, cilOptions } from '@coreui/icons';
import CIcon from '@coreui/icons-react';
import { useGetWasteReportsQuery } from '../../api/wasteManagementApi';
import { useNavigate } from 'react-router-dom';

const WasteReportList: React.FC = () => {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');
  const [status, setStatus] = useState('');
  const [sortBy, setSortBy] = useState('generatedDate');
  const [sortDescending, setSortDescending] = useState(true);

  const { data, isLoading, error } = useGetWasteReportsQuery({
    page,
    pageSize,
    search: search || undefined,
    category: category || undefined,
    status: status || undefined,
    sortBy,
    sortDescending,
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending': return 'warning';
      case 'disposed': return 'success';
      case 'intransit': return 'info';
      case 'approved': return 'primary';
      case 'rejected': return 'danger';
      default: return 'secondary';
    }
  };

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

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load waste reports. Please try again.
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <strong>Waste Reports</strong>
            <CButton
              color="primary"
              onClick={() => navigate('/waste-management/create')}
            >
              <CIcon icon={cilPlus} className="me-2" />
              New Report
            </CButton>
          </CCardHeader>
          <CCardBody>
            {/* Filters */}
            <CRow className="mb-3">
              <CCol md={4}>
                <CInputGroup>
                  <CFormInput
                    placeholder="Search reports..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                  />
                  <CButton variant="outline" color="secondary">
                    <CIcon icon={cilSearch} />
                  </CButton>
                </CInputGroup>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={category}
                  onChange={(e) => setCategory(e.target.value)}
                >
                  <option value="">All Categories</option>
                  <option value="NonHazardous">Non-Hazardous</option>
                  <option value="HazardousChemical">Hazardous Chemical</option>
                  <option value="HazardousBiological">Hazardous Biological</option>
                  <option value="Recyclable">Recyclable</option>
                  <option value="Electronic">Electronic</option>
                  <option value="Medical">Medical</option>
                  <option value="Organic">Organic</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={status}
                  onChange={(e) => setStatus(e.target.value)}
                >
                  <option value="">All Statuses</option>
                  <option value="Draft">Draft</option>
                  <option value="Submitted">Submitted</option>
                  <option value="Approved">Approved</option>
                  <option value="Pending">Pending</option>
                  <option value="InTransit">In Transit</option>
                  <option value="Disposed">Disposed</option>
                  <option value="Rejected">Rejected</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CFormSelect
                  value={pageSize}
                  onChange={(e) => setPageSize(Number(e.target.value))}
                >
                  <option value={10}>10 per page</option>
                  <option value={20}>20 per page</option>
                  <option value={50}>50 per page</option>
                  <option value={100}>100 per page</option>
                </CFormSelect>
              </CCol>
              <CCol md={2}>
                <CDropdown>
                  <CDropdownToggle color="secondary" variant="outline">
                    <CIcon icon={cilOptions} className="me-2" />
                    Sort
                  </CDropdownToggle>
                  <CDropdownMenu>
                    <CDropdownItem onClick={() => handleSort('generatedDate')}>
                      Date {getSortIcon('generatedDate')}
                    </CDropdownItem>
                    <CDropdownItem onClick={() => handleSort('title')}>
                      Title {getSortIcon('title')}
                    </CDropdownItem>
                    <CDropdownItem onClick={() => handleSort('category')}>
                      Category {getSortIcon('category')}
                    </CDropdownItem>
                    <CDropdownItem onClick={() => handleSort('status')}>
                      Status {getSortIcon('status')}
                    </CDropdownItem>
                    <CDropdownItem onClick={() => handleSort('location')}>
                      Location {getSortIcon('location')}
                    </CDropdownItem>
                  </CDropdownMenu>
                </CDropdown>
              </CCol>
            </CRow>

            {/* Table */}
            {isLoading ? (
              <div className="text-center">
                <CSpinner />
              </div>
            ) : (
              <>
                <CTable hover responsive>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('title')}
                      >
                        Title {getSortIcon('title')}
                      </CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('category')}
                      >
                        Category {getSortIcon('category')}
                      </CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('status')}
                      >
                        Status {getSortIcon('status')}
                      </CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('location')}
                      >
                        Location {getSortIcon('location')}
                      </CTableHeaderCell>
                      <CTableHeaderCell>Reporter</CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('generatedDate')}
                      >
                        Date {getSortIcon('generatedDate')}
                      </CTableHeaderCell>
                      <CTableHeaderCell>Attachments</CTableHeaderCell>
                      <CTableHeaderCell>Actions</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {data?.items?.map((report) => (
                      <CTableRow key={report.id}>
                        <CTableDataCell>
                          <strong>{report.title}</strong>
                          <br />
                          <small className="text-muted">
                            {report.description.length > 50
                              ? `${report.description.substring(0, 50)}...`
                              : report.description}
                          </small>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color="info" shape="rounded-pill">
                            {report.category}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={getStatusColor(report.status)}>
                            {report.status}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>{report.location}</CTableDataCell>
                        <CTableDataCell>{report.reporterName || 'Unknown'}</CTableDataCell>
                        <CTableDataCell>
                          {new Date(report.generatedDate).toLocaleDateString()}
                        </CTableDataCell>
                        <CTableDataCell>
                          {report.attachmentsCount > 0 && (
                            <CBadge color="secondary">
                              {report.attachmentsCount} files
                            </CBadge>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>
                          <CButton
                            color="primary"
                            variant="outline"
                            size="sm"
                            onClick={() => navigate(`/waste-management/${report.id}`)}
                          >
                            View
                          </CButton>
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>

                {/* Pagination */}
                {data && data.totalPages > 1 && (
                  <div className="d-flex justify-content-between align-items-center mt-3">
                    <div>
                      Showing {((page - 1) * pageSize) + 1} to {Math.min(page * pageSize, data.totalCount)} of {data.totalCount} results
                    </div>
                    <CPagination aria-label="Page navigation">
                      <CPaginationItem
                        disabled={!data.hasPreviousPage}
                        onClick={() => setPage(page - 1)}
                      >
                        Previous
                      </CPaginationItem>
                      {Array.from({ length: Math.min(5, data.totalPages) }, (_, i) => {
                        const pageNum = Math.max(1, page - 2) + i;
                        if (pageNum <= data.totalPages) {
                          return (
                            <CPaginationItem
                              key={pageNum}
                              active={pageNum === page}
                              onClick={() => setPage(pageNum)}
                            >
                              {pageNum}
                            </CPaginationItem>
                          );
                        }
                        return null;
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

                {data?.items?.length === 0 && (
                  <div className="text-center text-muted py-4">
                    <p>No waste reports found.</p>
                    <CButton
                      color="primary"
                      onClick={() => navigate('/waste-management/create')}
                    >
                      Create First Report
                    </CButton>
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

export default WasteReportList;
