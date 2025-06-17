import React, { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CInputGroup,
  CFormInput,
  CFormSelect,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CBadge,
  CPagination,
  CPaginationItem,
  CSpinner,
  CAlert,
  CTooltip,
  CButtonGroup,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faExclamationTriangle,
  faSearch,
  faEye,
  faRedo,
  faCalendarAlt,
  faFileContract,
  faClock,
  faDownload,
  faFilter,
} from '@fortawesome/free-solid-svg-icons';
import { format, differenceInDays, isAfter } from 'date-fns';

import { useGetExpiringLicensesQuery } from '../../features/licenses/licenseApi';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { useDebounce } from '../../hooks/useDebounce';
import { LICENSE_STATUSES, getStatusColor } from '../../types/license';

interface ExpiringLicenseFilters {
  search: string;
  daysAhead: number;
  sortBy: string;
  sortDirection: 'asc' | 'desc';
}

const DAYS_AHEAD_OPTIONS = [
  { value: 30, label: 'Next 30 days' },
  { value: 60, label: 'Next 60 days' },
  { value: 90, label: 'Next 90 days' },
  { value: 180, label: 'Next 6 months' },
  { value: 365, label: 'Next year' },
];

const ExpiringLicenses: React.FC = () => {
  const navigate = useNavigate();
  const { isDemoMode } = useApplicationMode();

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);
  const [filters, setFilters] = useState<ExpiringLicenseFilters>({
    search: '',
    daysAhead: 90,
    sortBy: 'expiryDate',
    sortDirection: 'asc',
  });

  const debouncedSearch = useDebounce(filters.search, 300);

  const { data, isLoading, error, refetch } = useGetExpiringLicensesQuery({
    page: currentPage,
    pageSize,
    daysAhead: filters.daysAhead,
  });

  const handleFilterChange = useCallback((field: keyof ExpiringLicenseFilters, value: any) => {
    setFilters((prev) => ({ ...prev, [field]: value }));
    setCurrentPage(1);
  }, []);

  const handleViewLicense = (id: number) => {
    navigate(`/licenses/${id}`);
  };

  const getExpiryStatus = (expiryDate: string | null) => {
    if (!expiryDate) return null;

    const expiry = new Date(expiryDate);
    const today = new Date();
    const daysToExpiry = differenceInDays(expiry, today);

    if (isAfter(today, expiry)) {
      return {
        badge: <CBadge color="danger">Expired {Math.abs(daysToExpiry)} days ago</CBadge>,
        urgency: 'critical'
      };
    } else if (daysToExpiry <= 7) {
      return {
        badge: <CBadge color="danger">Expires in {daysToExpiry} days</CBadge>,
        urgency: 'critical'
      };
    } else if (daysToExpiry <= 30) {
      return {
        badge: <CBadge color="warning">Expires in {daysToExpiry} days</CBadge>,
        urgency: 'high'
      };
    } else if (daysToExpiry <= 90) {
      return {
        badge: <CBadge color="info">Expires in {daysToExpiry} days</CBadge>,
        urgency: 'medium'
      };
    }

    return {
      badge: <CBadge color="success">Expires in {daysToExpiry} days</CBadge>,
      urgency: 'low'
    };
  };

  const getUrgencyIcon = (urgency: string) => {
    switch (urgency) {
      case 'critical':
        return <FontAwesomeIcon icon={faExclamationTriangle} className="text-danger me-2" />;
      case 'high':
        return <FontAwesomeIcon icon={faClock} className="text-warning me-2" />;
      case 'medium':
        return <FontAwesomeIcon icon={faCalendarAlt} className="text-info me-2" />;
      default:
        return <FontAwesomeIcon icon={faCalendarAlt} className="text-success me-2" />;
    }
  };

  // Filter the data based on search term if we have data
  const filteredData = React.useMemo(() => {
    if (!data?.items || !debouncedSearch) return data?.items || [];
    
    const searchLower = debouncedSearch.toLowerCase();
    return data.items.filter(license => 
      license.title.toLowerCase().includes(searchLower) ||
      license.licenseNumber.toLowerCase().includes(searchLower) ||
      license.issuingAuthority.toLowerCase().includes(searchLower) ||
      license.holderName.toLowerCase().includes(searchLower)
    );
  }, [data?.items, debouncedSearch]);

  const renderLicenseTable = () => {
    if (!filteredData.length) {
      return (
        <div className="text-center py-5">
          <FontAwesomeIcon icon={faFileContract} size="3x" className="text-muted mb-3" />
          <p className="text-muted">No expiring licenses found</p>
          <CButton color="primary" variant="outline" onClick={refetch}>
            <FontAwesomeIcon icon={faRedo} className="me-1" />
            Refresh
          </CButton>
        </div>
      );
    }

    return (
      <CTable hover responsive>
        <CTableHead>
          <CTableRow>
            <CTableHeaderCell>Priority</CTableHeaderCell>
            <CTableHeaderCell>License</CTableHeaderCell>
            <CTableHeaderCell>Type</CTableHeaderCell>
            <CTableHeaderCell>Status</CTableHeaderCell>
            <CTableHeaderCell>Holder</CTableHeaderCell>
            <CTableHeaderCell>Issuing Authority</CTableHeaderCell>
            <CTableHeaderCell>Expiry Date</CTableHeaderCell>
            <CTableHeaderCell>Expiry Status</CTableHeaderCell>
            <CTableHeaderCell>Actions</CTableHeaderCell>
          </CTableRow>
        </CTableHead>
        <CTableBody>
          {filteredData.map((license) => {
            const expiryStatus = getExpiryStatus(license.expiryDate);
            return (
              <CTableRow key={license.id}>
                <CTableDataCell>
                  {expiryStatus && getUrgencyIcon(expiryStatus.urgency)}
                </CTableDataCell>
                <CTableDataCell>
                  <div>
                    <strong>{license.licenseNumber}</strong>
                    <div className="text-muted small">{license.title}</div>
                  </div>
                </CTableDataCell>
                <CTableDataCell>
                  <CBadge color="primary">{license.type}</CBadge>
                </CTableDataCell>
                <CTableDataCell>
                  <CBadge color={getStatusColor(license.status)}>
                    {license.status}
                  </CBadge>
                </CTableDataCell>
                <CTableDataCell>
                  <div>
                    <strong>{license.holderName}</strong>
                    <div className="text-muted small">{license.department}</div>
                  </div>
                </CTableDataCell>
                <CTableDataCell>
                  <div className="small">{license.issuingAuthority}</div>
                </CTableDataCell>
                <CTableDataCell>
                  <div>
                    <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                    {license.expiryDate ? format(new Date(license.expiryDate), 'MMM dd, yyyy') : '-'}
                  </div>
                </CTableDataCell>
                <CTableDataCell>
                  {expiryStatus?.badge}
                </CTableDataCell>
                <CTableDataCell>
                  <CButtonGroup size="sm">
                    <CTooltip content="View Details">
                      <CButton
                        color="primary"
                        variant="outline"
                        onClick={() => handleViewLicense(license.id)}
                      >
                        <FontAwesomeIcon icon={faEye} />
                      </CButton>
                    </CTooltip>
                    {license.renewalRequired && (
                      <CTooltip content="Renew License">
                        <CButton
                          color="warning"
                          variant="outline"
                          onClick={() => navigate(`/licenses/${license.id}?action=renew`)}
                        >
                          <FontAwesomeIcon icon={faRedo} />
                        </CButton>
                      </CTooltip>
                    )}
                  </CButtonGroup>
                </CTableDataCell>
              </CTableRow>
            );
          })}
        </CTableBody>
      </CTable>
    );
  };

  return (
    <>
      <CRow>
        <CCol xs={12}>
          <CCard className="mb-4">
            <CCardHeader>
              <div className="d-flex justify-content-between align-items-center">
                <h4 className="mb-0">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2 text-warning" />
                  Expiring Licenses
                </h4>
                <CButton color="primary" onClick={() => navigate('/licenses/create')}>
                  Add New License
                </CButton>
              </div>
            </CCardHeader>
            <CCardBody>
              {/* Summary Cards */}
              <CRow className="mb-4">
                <CCol md={3}>
                  <CCard className="border-start border-danger border-4">
                    <CCardBody>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faExclamationTriangle} className="text-danger fa-2x me-3" />
                        <div>
                          <div className="h4 text-danger mb-0">
                            {filteredData.filter(l => {
                              const status = getExpiryStatus(l.expiryDate);
                              return status?.urgency === 'critical';
                            }).length}
                          </div>
                          <div className="text-muted">Critical</div>
                        </div>
                      </div>
                    </CCardBody>
                  </CCard>
                </CCol>
                <CCol md={3}>
                  <CCard className="border-start border-warning border-4">
                    <CCardBody>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faClock} className="text-warning fa-2x me-3" />
                        <div>
                          <div className="h4 text-warning mb-0">
                            {filteredData.filter(l => {
                              const status = getExpiryStatus(l.expiryDate);
                              return status?.urgency === 'high';
                            }).length}
                          </div>
                          <div className="text-muted">High Priority</div>
                        </div>
                      </div>
                    </CCardBody>
                  </CCard>
                </CCol>
                <CCol md={3}>
                  <CCard className="border-start border-info border-4">
                    <CCardBody>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faCalendarAlt} className="text-info fa-2x me-3" />
                        <div>
                          <div className="h4 text-info mb-0">
                            {filteredData.filter(l => {
                              const status = getExpiryStatus(l.expiryDate);
                              return status?.urgency === 'medium';
                            }).length}
                          </div>
                          <div className="text-muted">Medium Priority</div>
                        </div>
                      </div>
                    </CCardBody>
                  </CCard>
                </CCol>
                <CCol md={3}>
                  <CCard className="border-start border-primary border-4">
                    <CCardBody>
                      <div className="d-flex align-items-center">
                        <FontAwesomeIcon icon={faFileContract} className="text-primary fa-2x me-3" />
                        <div>
                          <div className="h4 text-primary mb-0">{filteredData.length}</div>
                          <div className="text-muted">Total Expiring</div>
                        </div>
                      </div>
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>

              {/* Filters */}
              <CRow className="mb-3">
                <CCol md={4}>
                  <CInputGroup>
                    <CFormInput
                      placeholder="Search licenses..."
                      value={filters.search}
                      onChange={(e) => handleFilterChange('search', e.target.value)}
                    />
                    <CButton color="primary" variant="outline">
                      <FontAwesomeIcon icon={faSearch} />
                    </CButton>
                  </CInputGroup>
                </CCol>
                <CCol md={3}>
                  <CFormSelect
                    value={filters.daysAhead}
                    onChange={(e) => handleFilterChange('daysAhead', parseInt(e.target.value))}
                  >
                    {DAYS_AHEAD_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={2}>
                  <CButton color="secondary" variant="outline" onClick={refetch}>
                    <FontAwesomeIcon icon={faRedo} className="me-1" />
                    Refresh
                  </CButton>
                </CCol>
              </CRow>

              {/* Loading/Error/Data */}
              {isLoading && (
                <div className="text-center py-5">
                  <CSpinner color="primary" />
                  <div className="mt-2">Loading expiring licenses...</div>
                </div>
              )}

              {error && (
                <CAlert color="danger">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                  Failed to load expiring licenses. Please try again.
                </CAlert>
              )}

              {data && (
                <>
                  {renderLicenseTable()}

                  {/* Pagination */}
                  {data.totalPages > 1 && (
                    <CPagination className="mt-3">
                      <CPaginationItem
                        disabled={currentPage <= 1}
                        onClick={() => setCurrentPage((prev) => prev - 1)}
                      >
                        Previous
                      </CPaginationItem>
                      {[...Array(data.totalPages)].map((_, index) => (
                        <CPaginationItem
                          key={index + 1}
                          active={index + 1 === currentPage}
                          onClick={() => setCurrentPage(index + 1)}
                        >
                          {index + 1}
                        </CPaginationItem>
                      ))}
                      <CPaginationItem
                        disabled={currentPage >= data.totalPages}
                        onClick={() => setCurrentPage((prev) => prev + 1)}
                      >
                        Next
                      </CPaginationItem>
                    </CPagination>
                  )}
                </>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Demo Mode Alert */}
      {isDemoMode && (
        <CAlert color="info" className="mt-3">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
          <strong>Demo Mode:</strong> This is sample data for demonstration purposes.
        </CAlert>
      )}
    </>
  );
};

export default ExpiringLicenses;