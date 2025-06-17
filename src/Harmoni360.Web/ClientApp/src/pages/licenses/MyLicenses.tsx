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
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faFileContract,
  faSearch,
  faEye,
  faExclamationTriangle,
  faCheckCircle,
  faTimesCircle,
  faClock,
  faPause,
  faBan,
  faRedo,
  faDownload,
  faCalendarAlt,
} from '@fortawesome/free-solid-svg-icons';
import { format, differenceInDays, isAfter } from 'date-fns';

import { useGetMyLicensesQuery } from '../../features/licenses/licenseApi';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { useDebounce } from '../../hooks/useDebounce';
import { LICENSE_STATUSES, LICENSE_TYPES } from '../../types/license';

interface MyLicenseFilters {
  search: string;
  status: string;
  type: string;
  isExpiring: boolean | undefined;
  isExpired: boolean | undefined;
  sortBy: string;
  sortDirection: 'asc' | 'desc';
}

const STATUS_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'Draft', label: 'Draft' },
  { value: 'Submitted', label: 'Submitted' },
  { value: 'UnderReview', label: 'Under Review' },
  { value: 'Approved', label: 'Approved' },
  { value: 'Active', label: 'Active' },
  { value: 'Suspended', label: 'Suspended' },
  { value: 'Revoked', label: 'Revoked' },
  { value: 'Expired', label: 'Expired' },
  { value: 'Rejected', label: 'Rejected' },
];

const MyLicenses: React.FC = () => {
  const navigate = useNavigate();
  const { isDemoMode } = useApplicationMode();

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [filters, setFilters] = useState<MyLicenseFilters>({
    search: '',
    status: '',
    type: '',
    isExpiring: undefined,
    isExpired: undefined,
    sortBy: 'expiryDate',
    sortDirection: 'asc',
  });

  const debouncedSearch = useDebounce(filters.search, 300);

  const { data, isLoading, error } = useGetMyLicensesQuery({
    page: currentPage,
    pageSize,
    searchTerm: debouncedSearch,
    status: filters.status || undefined,
    type: filters.type || undefined,
    isExpiring: filters.isExpiring,
    isExpired: filters.isExpired,
    sortBy: filters.sortBy,
    sortDirection: filters.sortDirection,
  });

  const handleFilterChange = useCallback((field: keyof MyLicenseFilters, value: any) => {
    setFilters((prev) => ({ ...prev, [field]: value }));
    setCurrentPage(1);
  }, []);

  const handleViewLicense = (id: number) => {
    navigate(`/licenses/${id}`);
  };

  const getStatusBadge = (status: string) => {
    const statusConfig = {
      Draft: { color: 'secondary', icon: null },
      Submitted: { color: 'info', icon: faClock },
      UnderReview: { color: 'warning', icon: faClock },
      Approved: { color: 'primary', icon: faCheckCircle },
      Active: { color: 'success', icon: faCheckCircle },
      Suspended: { color: 'warning', icon: faPause },
      Revoked: { color: 'danger', icon: faBan },
      Expired: { color: 'dark', icon: faTimesCircle },
      Rejected: { color: 'danger', icon: faTimesCircle },
    };

    const config = statusConfig[status] || { color: 'secondary', icon: null };

    return (
      <CBadge color={config.color}>
        {config.icon && <FontAwesomeIcon icon={config.icon} className="me-1" />}
        {status.replace(/([A-Z])/g, ' $1').trim()}
      </CBadge>
    );
  };

  const getExpiryStatus = (expiryDate: string | null) => {
    if (!expiryDate) return null;

    const expiry = new Date(expiryDate);
    const today = new Date();
    const daysToExpiry = differenceInDays(expiry, today);

    if (isAfter(today, expiry)) {
      return <CBadge color="danger">Expired</CBadge>;
    } else if (daysToExpiry <= 30) {
      return <CBadge color="warning">Expires in {daysToExpiry} days</CBadge>;
    } else if (daysToExpiry <= 90) {
      return <CBadge color="info">Expires in {daysToExpiry} days</CBadge>;
    }

    return null;
  };

  const renderLicenseTable = () => {
    if (!data?.items.length) {
      return (
        <div className="text-center py-5">
          <FontAwesomeIcon icon={faFileContract} size="3x" className="text-muted mb-3" />
          <p className="text-muted">No licenses found</p>
        </div>
      );
    }

    return (
      <CTable hover responsive>
        <CTableHead>
          <CTableRow>
            <CTableHeaderCell>License Number</CTableHeaderCell>
            <CTableHeaderCell>Title</CTableHeaderCell>
            <CTableHeaderCell>Type</CTableHeaderCell>
            <CTableHeaderCell>Status</CTableHeaderCell>
            <CTableHeaderCell>Issuing Authority</CTableHeaderCell>
            <CTableHeaderCell>Issue Date</CTableHeaderCell>
            <CTableHeaderCell>Expiry Date</CTableHeaderCell>
            <CTableHeaderCell>Actions</CTableHeaderCell>
          </CTableRow>
        </CTableHead>
        <CTableBody>
          {data.items.map((license) => (
            <CTableRow key={license.id}>
              <CTableDataCell>
                <strong>{license.licenseNumber}</strong>
              </CTableDataCell>
              <CTableDataCell>{license.title}</CTableDataCell>
              <CTableDataCell>
                <CBadge color="primary">{license.type}</CBadge>
              </CTableDataCell>
              <CTableDataCell>{getStatusBadge(license.status)}</CTableDataCell>
              <CTableDataCell>{license.issuingAuthority}</CTableDataCell>
              <CTableDataCell>
                {license.issuedDate ? format(new Date(license.issuedDate), 'dd MMM yyyy') : '-'}
              </CTableDataCell>
              <CTableDataCell>
                {license.expiryDate ? (
                  <div>
                    {format(new Date(license.expiryDate), 'dd MMM yyyy')}
                    <div>{getExpiryStatus(license.expiryDate)}</div>
                  </div>
                ) : (
                  '-'
                )}
              </CTableDataCell>
              <CTableDataCell>
                <CTooltip content="View Details">
                  <CButton
                    color="primary"
                    size="sm"
                    onClick={() => handleViewLicense(license.id)}
                  >
                    <FontAwesomeIcon icon={faEye} />
                  </CButton>
                </CTooltip>
              </CTableDataCell>
            </CTableRow>
          ))}
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
                  <FontAwesomeIcon icon={faFileContract} className="me-2" />
                  My Licenses
                </h4>
                <CButton color="primary" onClick={() => navigate('/licenses/create')}>
                  Add New License
                </CButton>
              </div>
            </CCardHeader>
            <CCardBody>
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
                <CCol md={2}>
                  <CFormSelect
                    value={filters.status}
                    onChange={(e) => handleFilterChange('status', e.target.value)}
                  >
                    {STATUS_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={2}>
                  <CFormSelect
                    value={filters.type}
                    onChange={(e) => handleFilterChange('type', e.target.value)}
                  >
                    <option value="">All Types</option>
                    {LICENSE_TYPES.map((type) => (
                      <option key={type.value} value={type.value}>
                        {type.label}
                      </option>
                    ))}
                  </CFormSelect>
                </CCol>
                <CCol md={2}>
                  <CFormSelect
                    value={filters.isExpiring === true ? 'expiring' : filters.isExpired === true ? 'expired' : ''}
                    onChange={(e) => {
                      const value = e.target.value;
                      if (value === 'expiring') {
                        handleFilterChange('isExpiring', true);
                        handleFilterChange('isExpired', undefined);
                      } else if (value === 'expired') {
                        handleFilterChange('isExpiring', undefined);
                        handleFilterChange('isExpired', true);
                      } else {
                        handleFilterChange('isExpiring', undefined);
                        handleFilterChange('isExpired', undefined);
                      }
                    }}
                  >
                    <option value="">All Licenses</option>
                    <option value="expiring">Expiring Soon</option>
                    <option value="expired">Expired</option>
                  </CFormSelect>
                </CCol>
              </CRow>

              {/* Loading/Error/Data */}
              {isLoading && (
                <div className="text-center py-5">
                  <CSpinner color="primary" />
                </div>
              )}

              {error && (
                <CAlert color="danger">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                  Failed to load licenses. Please try again.
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

export default MyLicenses;