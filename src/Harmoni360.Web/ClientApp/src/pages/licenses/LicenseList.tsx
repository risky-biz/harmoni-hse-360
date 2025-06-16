import React, { useState, useCallback, useMemo } from 'react';
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
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CDropdownDivider,
  CSpinner,
  CAlert,
  CFormCheck,
  CTooltip,
  CButtonGroup,
  CForm
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faSearch,
  faFilter,
  faEye,
  faEdit,
  faTrash,
  faFileContract,
  faExclamationTriangle,
  faClock,
  faCheckCircle,
  faTimesCircle,
  faPause,
  faBan,
  faRedo,
  faDownload,
  faFileExport,
  faCog,
  faSort,
  faSortUp,
  faSortDown,
  faCalendarAlt,
  faBuilding,
  faShieldAlt,
  faCertificate,
  faInfoCircle
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetLicensesQuery, 
  useDeleteLicenseMutation,
  useSubmitLicenseMutation,
  useActivateLicenseMutation,
  useSuspendLicenseMutation,
  useRevokeLicenseMutation 
} from '../../features/licenses/licenseApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import { 
  LicenseDto,
  LICENSE_TYPES,
  LICENSE_PRIORITIES,
  LICENSE_STATUSES,
  RISK_LEVELS,
  getStatusColor,
  getPriorityColor,
  getRiskLevelColor
} from '../../types/license';
import { useDebounce } from '../../hooks/useDebounce';
import { format } from 'date-fns';

// License Management Icon Mappings
const LICENSE_ICONS = {
  license: faFileContract,
  create: faPlus,
  search: faSearch,
  filter: faFilter,
  view: faEye,
  edit: faEdit,
  delete: faTrash,
  submit: faCheckCircle,
  activate: faCheckCircle,
  suspend: faPause,
  revoke: faBan,
  renew: faRedo,
  export: faFileExport,
  download: faDownload,
  settings: faCog,
  warning: faExclamationTriangle,
  clock: faClock,
  calendar: faCalendarAlt,
  building: faBuilding,
  shield: faShieldAlt,
  certificate: faCertificate,
  info: faInfoCircle
};

interface FilterState {
  searchTerm: string;
  status: string;
  type: string;
  priority: string;
  riskLevel: string;
  department: string;
  issuingAuthority: string;
  isExpiring: boolean;
  isExpired: boolean;
  renewalDue: boolean;
  expiryDateFrom: string;
  expiryDateTo: string;
}

interface SortState {
  sortBy: string;
  sortDirection: 'asc' | 'desc';
}

const LicenseList: React.FC = () => {
  const navigate = useNavigate();

  // State management
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [showFilters, setShowFilters] = useState(false);
  const [selectedLicenses, setSelectedLicenses] = useState<number[]>([]);
  const [actionLoading, setActionLoading] = useState<number | null>(null);

  const [filters, setFilters] = useState<FilterState>({
    searchTerm: '',
    status: '',
    type: '',
    priority: '',
    riskLevel: '',
    department: '',
    issuingAuthority: '',
    isExpiring: false,
    isExpired: false,
    renewalDue: false,
    expiryDateFrom: '',
    expiryDateTo: ''
  });

  const [sort, setSort] = useState<SortState>({
    sortBy: 'createdAt',
    sortDirection: 'desc'
  });

  // Debounce search term
  const debouncedSearchTerm = useDebounce(filters.searchTerm, 300);

  // API calls
  const { data: departments } = useGetDepartmentsQuery({});
  const { 
    data: licensesData, 
    isLoading, 
    error,
    refetch 
  } = useGetLicensesQuery({
    page,
    pageSize,
    ...filters,
    searchTerm: debouncedSearchTerm,
    ...sort
  });

  const [deleteLicense] = useDeleteLicenseMutation();
  const [submitLicense] = useSubmitLicenseMutation();
  const [activateLicense] = useActivateLicenseMutation();
  const [suspendLicense] = useSuspendLicenseMutation();
  const [revokeLicense] = useRevokeLicenseMutation();

  // Handle filter changes
  const handleFilterChange = useCallback((field: keyof FilterState, value: any) => {
    setFilters(prev => ({ ...prev, [field]: value }));
    setPage(1); // Reset to first page when filtering
  }, []);

  // Handle sorting
  const handleSort = useCallback((field: string) => {
    setSort(prev => ({
      sortBy: field,
      sortDirection: prev.sortBy === field && prev.sortDirection === 'asc' ? 'desc' : 'asc'
    }));
  }, []);

  // Handle selection
  const handleSelectLicense = useCallback((licenseId: number) => {
    setSelectedLicenses(prev => 
      prev.includes(licenseId) 
        ? prev.filter(id => id !== licenseId)
        : [...prev, licenseId]
    );
  }, []);

  const handleSelectAll = useCallback(() => {
    if (selectedLicenses.length === licensesData?.items.length) {
      setSelectedLicenses([]);
    } else {
      setSelectedLicenses(licensesData?.items.map(license => license.id) || []);
    }
  }, [selectedLicenses, licensesData]);

  // License actions
  const handleLicenseAction = useCallback(async (
    action: 'submit' | 'activate' | 'suspend' | 'revoke' | 'delete',
    licenseId: number,
    license: LicenseDto
  ) => {
    try {
      setActionLoading(licenseId);

      switch (action) {
        case 'submit':
          await submitLicense({ id: licenseId }).unwrap();
          break;
        case 'activate':
          await activateLicense({ id: licenseId }).unwrap();
          break;
        case 'suspend':
          const suspensionReason = prompt('Please provide a reason for suspension:');
          if (!suspensionReason) return;
          await suspendLicense({ 
            id: licenseId, 
            suspensionReason,
            suspensionNotes: ''
          }).unwrap();
          break;
        case 'revoke':
          const revocationReason = prompt('Please provide a reason for revocation:');
          if (!revocationReason) return;
          await revokeLicense({ 
            id: licenseId, 
            revocationReason,
            revocationNotes: ''
          }).unwrap();
          break;
        case 'delete':
          if (window.confirm(`Are you sure you want to delete license "${license.title}"?`)) {
            await deleteLicense(licenseId).unwrap();
          }
          break;
      }

      refetch();
    } catch (error) {
      console.error(`Error ${action} license:`, error);
    } finally {
      setActionLoading(null);
    }
  }, [submitLicense, activateLicense, suspendLicense, revokeLicense, deleteLicense, refetch]);

  // Clear filters
  const clearFilters = useCallback(() => {
    setFilters({
      searchTerm: '',
      status: '',
      type: '',
      priority: '',
      riskLevel: '',
      department: '',
      issuingAuthority: '',
      isExpiring: false,
      isExpired: false,
      renewalDue: false,
      expiryDateFrom: '',
      expiryDateTo: ''
    });
    setPage(1);
  }, []);

  // Export licenses
  const handleExport = useCallback(() => {
    // TODO: Implement export functionality
    console.log('Export licenses functionality to be implemented');
  }, []);

  // Render sort icon
  const renderSortIcon = useCallback((field: string) => {
    if (sort.sortBy !== field) return <FontAwesomeIcon icon={faSort} className="text-muted" />;
    return <FontAwesomeIcon 
      icon={sort.sortDirection === 'asc' ? faSortUp : faSortDown} 
      className="text-primary" 
    />;
  }, [sort]);

  // Get action buttons for a license
  const getActionButtons = useCallback((license: LicenseDto) => {
    const isProcessing = actionLoading === license.id;
    
    return (
      <CButtonGroup size="sm">
        <CTooltip content="View Details">
          <CButton
            color="info"
            variant="outline"
            onClick={() => navigate(`/licenses/${license.id}`)}
            disabled={isProcessing}
          >
            <FontAwesomeIcon icon={LICENSE_ICONS.view} />
          </CButton>
        </CTooltip>

        {license.canEdit && (
          <CTooltip content="Edit License">
            <CButton
              color="warning"
              variant="outline"
              onClick={() => navigate(`/licenses/${license.id}/edit`)}
              disabled={isProcessing}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.edit} />
            </CButton>
          </CTooltip>
        )}

        {license.canSubmit && (
          <CTooltip content="Submit for Review">
            <CButton
              color="success"
              variant="outline"
              onClick={() => handleLicenseAction('submit', license.id, license)}
              disabled={isProcessing}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.submit} />
            </CButton>
          </CTooltip>
        )}

        {license.canActivate && (
          <CTooltip content="Activate License">
            <CButton
              color="success"
              variant="outline"
              onClick={() => handleLicenseAction('activate', license.id, license)}
              disabled={isProcessing}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.activate} />
            </CButton>
          </CTooltip>
        )}

        {license.canSuspend && (
          <CTooltip content="Suspend License">
            <CButton
              color="warning"
              variant="outline"
              onClick={() => handleLicenseAction('suspend', license.id, license)}
              disabled={isProcessing}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.suspend} />
            </CButton>
          </CTooltip>
        )}

        <CDropdown>
          <CDropdownToggle color="secondary" variant="outline" size="sm" disabled={isProcessing}>
            <FontAwesomeIcon icon={LICENSE_ICONS.settings} />
          </CDropdownToggle>
          <CDropdownMenu>
            {license.canRenew && (
              <CDropdownItem onClick={() => navigate(`/licenses/${license.id}/renew`)}>
                <FontAwesomeIcon icon={LICENSE_ICONS.renew} className="me-2" />
                Renew License
              </CDropdownItem>
            )}
            <CDropdownItem onClick={() => handleExport()}>
              <FontAwesomeIcon icon={LICENSE_ICONS.download} className="me-2" />
              Download
            </CDropdownItem>
            <CDropdownDivider />
            <CDropdownItem 
              className="text-danger"
              onClick={() => handleLicenseAction('revoke', license.id, license)}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.revoke} className="me-2" />
              Revoke License
            </CDropdownItem>
            <CDropdownItem 
              className="text-danger"
              onClick={() => handleLicenseAction('delete', license.id, license)}
            >
              <FontAwesomeIcon icon={LICENSE_ICONS.delete} className="me-2" />
              Delete License
            </CDropdownItem>
          </CDropdownMenu>
        </CDropdown>

        {isProcessing && (
          <CSpinner size="sm" className="ms-2" />
        )}
      </CButtonGroup>
    );
  }, [actionLoading, navigate, handleLicenseAction, handleExport]);

  // Calculate active filters count
  const activeFiltersCount = useMemo(() => {
    return Object.entries(filters).reduce((count, [key, value]) => {
      if (key === 'searchTerm') return count;
      if (typeof value === 'boolean' && value) return count + 1;
      if (typeof value === 'string' && value) return count + 1;
      return count;
    }, 0);
  }, [filters]);

  if (error) {
    return (
      <CRow>
        <CCol xs={12}>
          <CAlert color="danger">
            <FontAwesomeIcon icon={LICENSE_ICONS.warning} className="me-2" />
            Error loading licenses. Please try again.
          </CAlert>
        </CCol>
      </CRow>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div className="d-flex align-items-center">
              <FontAwesomeIcon icon={LICENSE_ICONS.license} className="me-2" />
              <h4 className="mb-0">License Management</h4>
              {licensesData && (
                <CBadge color="info" className="ms-2">
                  {licensesData.totalCount} total
                </CBadge>
              )}
            </div>
            <div className="d-flex gap-2">
              <CButton
                color="primary"
                onClick={() => navigate('/licenses/create')}
              >
                <FontAwesomeIcon icon={LICENSE_ICONS.create} className="me-1" />
                Create License
              </CButton>
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => setShowFilters(!showFilters)}
              >
                <FontAwesomeIcon icon={LICENSE_ICONS.filter} className="me-1" />
                Filters
                {activeFiltersCount > 0 && (
                  <CBadge color="primary" className="ms-1">
                    {activeFiltersCount}
                  </CBadge>
                )}
              </CButton>
              <CButton
                color="success"
                variant="outline"
                onClick={handleExport}
              >
                <FontAwesomeIcon icon={LICENSE_ICONS.export} className="me-1" />
                Export
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            {/* Search and Quick Filters */}
            <CRow className="mb-3">
              <CCol md={6}>
                <CInputGroup>
                  <CFormInput
                    placeholder="Search licenses..."
                    value={filters.searchTerm}
                    onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                  />
                  <CButton variant="outline" color="secondary">
                    <FontAwesomeIcon icon={LICENSE_ICONS.search} />
                  </CButton>
                </CInputGroup>
              </CCol>
              <CCol md={6}>
                <div className="d-flex gap-2">
                  <CFormCheck
                    id="filterExpiring"
                    label="Expiring Soon"
                    checked={filters.isExpiring}
                    onChange={(e) => handleFilterChange('isExpiring', e.target.checked)}
                  />
                  <CFormCheck
                    id="filterExpired"
                    label="Expired"
                    checked={filters.isExpired}
                    onChange={(e) => handleFilterChange('isExpired', e.target.checked)}
                  />
                  <CFormCheck
                    id="filterRenewalDue"
                    label="Renewal Due"
                    checked={filters.renewalDue}
                    onChange={(e) => handleFilterChange('renewalDue', e.target.checked)}
                  />
                </div>
              </CCol>
            </CRow>

            {/* Advanced Filters */}
            {showFilters && (
              <CCard className="mb-3">
                <CCardBody>
                  <CForm>
                    <CRow>
                      <CCol md={3}>
                        <div className="mb-3">
                          <CFormSelect
                            value={filters.status}
                            onChange={(e) => handleFilterChange('status', e.target.value)}
                          >
                            <option value="">All Statuses</option>
                            {LICENSE_STATUSES.map(status => (
                              <option key={status.value} value={status.value}>
                                {status.label}
                              </option>
                            ))}
                          </CFormSelect>
                        </div>
                      </CCol>
                      <CCol md={3}>
                        <div className="mb-3">
                          <CFormSelect
                            value={filters.type}
                            onChange={(e) => handleFilterChange('type', e.target.value)}
                          >
                            <option value="">All Types</option>
                            {LICENSE_TYPES.map(type => (
                              <option key={type.value} value={type.value}>
                                {type.label}
                              </option>
                            ))}
                          </CFormSelect>
                        </div>
                      </CCol>
                      <CCol md={3}>
                        <div className="mb-3">
                          <CFormSelect
                            value={filters.priority}
                            onChange={(e) => handleFilterChange('priority', e.target.value)}
                          >
                            <option value="">All Priorities</option>
                            {LICENSE_PRIORITIES.map(priority => (
                              <option key={priority.value} value={priority.value}>
                                {priority.label}
                              </option>
                            ))}
                          </CFormSelect>
                        </div>
                      </CCol>
                      <CCol md={3}>
                        <div className="mb-3">
                          <CFormSelect
                            value={filters.riskLevel}
                            onChange={(e) => handleFilterChange('riskLevel', e.target.value)}
                          >
                            <option value="">All Risk Levels</option>
                            {RISK_LEVELS.map(risk => (
                              <option key={risk.value} value={risk.value}>
                                {risk.label}
                              </option>
                            ))}
                          </CFormSelect>
                        </div>
                      </CCol>
                    </CRow>
                    <CRow>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormSelect
                            value={filters.department}
                            onChange={(e) => handleFilterChange('department', e.target.value)}
                          >
                            <option value="">All Departments</option>
                            {departments?.map((dept: any) => (
                              <option key={dept.id} value={dept.name}>
                                {dept.name}
                              </option>
                            ))}
                          </CFormSelect>
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CFormInput
                            placeholder="Issuing Authority"
                            value={filters.issuingAuthority}
                            onChange={(e) => handleFilterChange('issuingAuthority', e.target.value)}
                          />
                        </div>
                      </CCol>
                      <CCol md={4}>
                        <div className="mb-3">
                          <CButton
                            color="secondary"
                            variant="outline"
                            onClick={clearFilters}
                            className="w-100"
                          >
                            Clear Filters
                          </CButton>
                        </div>
                      </CCol>
                    </CRow>
                    <CRow>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormInput
                            type="date"
                            placeholder="Expiry Date From"
                            value={filters.expiryDateFrom}
                            onChange={(e) => handleFilterChange('expiryDateFrom', e.target.value)}
                          />
                        </div>
                      </CCol>
                      <CCol md={6}>
                        <div className="mb-3">
                          <CFormInput
                            type="date"
                            placeholder="Expiry Date To"
                            value={filters.expiryDateTo}
                            onChange={(e) => handleFilterChange('expiryDateTo', e.target.value)}
                          />
                        </div>
                      </CCol>
                    </CRow>
                  </CForm>
                </CCardBody>
              </CCard>
            )}

            {/* License Table */}
            {isLoading ? (
              <div className="text-center p-4">
                <CSpinner />
                <div className="mt-2">Loading licenses...</div>
              </div>
            ) : licensesData?.items.length === 0 ? (
              <CAlert color="info" className="text-center">
                <FontAwesomeIcon icon={LICENSE_ICONS.info} className="me-2" />
                No licenses found. <a href="/licenses/create">Create your first license</a>
              </CAlert>
            ) : (
              <>
                <CTable responsive hover>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell>
                        <CFormCheck
                          checked={selectedLicenses.length === licensesData?.items.length}
                          onChange={handleSelectAll}
                        />
                      </CTableHeaderCell>
                      <CTableHeaderCell 
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('title')}
                      >
                        License Title {renderSortIcon('title')}
                      </CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('licenseNumber')}
                      >
                        License Number {renderSortIcon('licenseNumber')}
                      </CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('type')}
                      >
                        Type {renderSortIcon('type')}
                      </CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('status')}
                      >
                        Status {renderSortIcon('status')}
                      </CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('priority')}
                      >
                        Priority {renderSortIcon('priority')}
                      </CTableHeaderCell>
                      <CTableHeaderCell
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSort('expiryDate')}
                      >
                        Expiry Date {renderSortIcon('expiryDate')}
                      </CTableHeaderCell>
                      <CTableHeaderCell>Issuing Authority</CTableHeaderCell>
                      <CTableHeaderCell>Department</CTableHeaderCell>
                      <CTableHeaderCell>Actions</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {licensesData?.items.map((license) => (
                      <CTableRow key={license.id}>
                        <CTableDataCell>
                          <CFormCheck
                            checked={selectedLicenses.includes(license.id)}
                            onChange={() => handleSelectLicense(license.id)}
                          />
                        </CTableDataCell>
                        <CTableDataCell>
                          <div>
                            <strong>{license.title}</strong>
                            {license.isCriticalLicense && (
                              <CBadge color="danger" className="ms-1">
                                <FontAwesomeIcon icon={LICENSE_ICONS.warning} className="me-1" />
                                Critical
                              </CBadge>
                            )}
                            {license.isExpiring && (
                              <CBadge color="warning" className="ms-1">
                                <FontAwesomeIcon icon={LICENSE_ICONS.clock} className="me-1" />
                                Expiring
                              </CBadge>
                            )}
                            {license.isExpired && (
                              <CBadge color="danger" className="ms-1">
                                <FontAwesomeIcon icon={LICENSE_ICONS.warning} className="me-1" />
                                Expired
                              </CBadge>
                            )}
                          </div>
                          <small className="text-muted">{license.description}</small>
                        </CTableDataCell>
                        <CTableDataCell>
                          <strong>{license.licenseNumber}</strong>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color="info">
                            {license.typeDisplay}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={getStatusColor(license.status)}>
                            {license.statusDisplay}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={getPriorityColor(license.priority)}>
                            {license.priorityDisplay}
                          </CBadge>
                          <div>
                            <CBadge color={getRiskLevelColor(license.riskLevel)} className="mt-1">
                              <FontAwesomeIcon icon={LICENSE_ICONS.shield} className="me-1" />
                              {license.riskLevelDisplay}
                            </CBadge>
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <div>
                            <FontAwesomeIcon icon={LICENSE_ICONS.calendar} className="me-1" />
                            {format(new Date(license.expiryDate), 'MMM dd, yyyy')}
                          </div>
                          <small className={`text-${license.isExpired ? 'danger' : license.isExpiring ? 'warning' : 'muted'}`}>
                            {license.daysUntilExpiry > 0 
                              ? `${license.daysUntilExpiry} days remaining`
                              : `Expired ${Math.abs(license.daysUntilExpiry)} days ago`
                            }
                          </small>
                        </CTableDataCell>
                        <CTableDataCell>
                          <div>
                            <FontAwesomeIcon icon={LICENSE_ICONS.certificate} className="me-1" />
                            {license.issuingAuthority}
                          </div>
                          <small className="text-muted">
                            Holder: {license.holderName}
                          </small>
                        </CTableDataCell>
                        <CTableDataCell>
                          <FontAwesomeIcon icon={LICENSE_ICONS.building} className="me-1" />
                          {license.department}
                        </CTableDataCell>
                        <CTableDataCell>
                          {getActionButtons(license)}
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>

                {/* Pagination */}
                {licensesData && licensesData.totalPages > 1 && (
                  <div className="d-flex justify-content-between align-items-center mt-3">
                    <div>
                      <CFormSelect
                        value={pageSize}
                        onChange={(e) => {
                          setPageSize(Number(e.target.value));
                          setPage(1);
                        }}
                        style={{ width: 'auto' }}
                      >
                        <option value={10}>10 per page</option>
                        <option value={25}>25 per page</option>
                        <option value={50}>50 per page</option>
                        <option value={100}>100 per page</option>
                      </CFormSelect>
                    </div>
                    
                    <div className="text-muted">
                      Showing {((page - 1) * pageSize) + 1} to {Math.min(page * pageSize, licensesData.totalCount)} of {licensesData.totalCount} entries
                    </div>

                    <CPagination>
                      <CPaginationItem
                        disabled={page === 1}
                        onClick={() => setPage(1)}
                      >
                        First
                      </CPaginationItem>
                      <CPaginationItem
                        disabled={page === 1}
                        onClick={() => setPage(page - 1)}
                      >
                        Previous
                      </CPaginationItem>
                      
                      {Array.from({ length: Math.min(5, licensesData.totalPages) }, (_, i) => {
                        const pageNum = Math.max(1, Math.min(
                          licensesData.totalPages - 4,
                          Math.max(1, page - 2)
                        )) + i;
                        
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
                        disabled={page === licensesData.totalPages}
                        onClick={() => setPage(page + 1)}
                      >
                        Next
                      </CPaginationItem>
                      <CPaginationItem
                        disabled={page === licensesData.totalPages}
                        onClick={() => setPage(licensesData.totalPages)}
                      >
                        Last
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

export default LicenseList;