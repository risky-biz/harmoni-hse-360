import React, { useState, useMemo } from 'react';
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
  CSpinner,
  CAlert,
  CBadge,
  CInputGroup,
  CFormInput,
  CFormSelect,
  CPagination,
  CPaginationItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faEdit,
  faTrash,
  faExclamationTriangle,
  faShieldAlt,
  faUsers,
  faUser,
  faSearch,
  faEllipsisV
} from '@fortawesome/free-solid-svg-icons';
import { useNavigate } from 'react-router-dom';
import { useGetHealthRecordsQuery, HealthRecordDto } from '../../features/health/healthApi';
import { formatDate } from '../../utils/dateUtils';

const HealthList: React.FC = () => {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [personType, setPersonType] = useState<string>('');
  const [isActive, setIsActive] = useState<boolean | ''>('');
  const [vaccinationStatus, setVaccinationStatus] = useState<string>('');
  const [sortBy, setSortBy] = useState('lastModifiedAt');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

  const queryParams = useMemo(() => ({
    pageNumber: page,
    pageSize,
    searchTerm: searchTerm || undefined,
    personType: personType || undefined,
    includeInactive: isActive === '' ? undefined : !isActive,
    sortBy,
    sortDescending: sortOrder === 'desc'
  }), [page, pageSize, searchTerm, personType, isActive, sortBy, sortOrder]);

  const {
    data: healthRecordsData,
    isLoading,
    error,
    refetch
  } = useGetHealthRecordsQuery(queryParams);

  const handleCreateNew = () => {
    navigate('/health/create');
  };

  const handleEditRecord = (id: string) => {
    navigate(`/health/edit/${id}`);
  };

  const handleViewRecord = (id: string) => {
    navigate(`/health/detail/${id}`);
  };

  const handleSort = (field: string) => {
    if (sortBy === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(field);
      setSortOrder('asc');
    }
  };

  const getPersonTypeIcon = (personType: string) => {
    return faUser; // Using faUser for both student and staff for consistency
  };

  const getPersonTypeBadge = (personType: string) => {
    switch (personType) {
      case 'Student':
        return 'info';
      case 'Staff':
        return 'warning';
      case 'Visitor':
        return 'secondary';
      case 'Contractor':
        return 'success';
      default:
        return 'secondary';
    }
  };

  const getVaccinationStatusBadge = (record: HealthRecordDto) => {
    if (record.expiringVaccinationsCount > 0) return 'danger';
    if (record.criticalAllergyAlerts?.length > 0) return 'warning';
    return 'success';
  };

  const getVaccinationStatusText = (record: HealthRecordDto) => {
    if (record.expiringVaccinationsCount > 0) return `${record.expiringVaccinationsCount} Expiring`;
    if (record.criticalAllergyAlerts?.length > 0) return 'Allergies';
    return 'Compliant';
  };

  const getCriticalConditionsCount = (record: HealthRecordDto) => {
    return record.hasCriticalConditions ? 1 : 0;
  };

  const totalPages = healthRecordsData ? Math.ceil(healthRecordsData.totalCount / pageSize) : 0;

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '400px' }}>
        <CSpinner color="primary" size="sm" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger" className="d-flex align-items-center">
        <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="lg" />
        <div>
          Failed to load health records. 
          <CButton color="link" onClick={() => refetch()} className="p-0 ms-2">
            Try again
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Health Records</h2>
        <CButton color="primary" onClick={handleCreateNew}>
          <FontAwesomeIcon icon={faPlus} className="me-1" />
          Create Health Record
        </CButton>
      </div>

      <CCard>
        <CCardHeader>
          <CRow>
            <CCol md={6}>
              <CInputGroup>
                <CFormInput
                  placeholder="Search health records..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
                <CButton type="button" color="outline-secondary">
                  <FontAwesomeIcon icon={faSearch} />
                </CButton>
              </CInputGroup>
            </CCol>
            <CCol md={6}>
              <CRow>
                <CCol md={4}>
                  <CFormSelect
                    value={personType}
                    onChange={(e) => setPersonType(e.target.value)}
                  >
                    <option value="">All Types</option>
                    <option value="Student">Students</option>
                    <option value="Staff">Staff</option>
                    <option value="Visitor">Visitors</option>
                    <option value="Contractor">Contractors</option>
                  </CFormSelect>
                </CCol>
                <CCol md={4}>
                  <CFormSelect
                    value={isActive.toString()}
                    onChange={(e) => setIsActive(e.target.value === '' ? '' : e.target.value === 'true')}
                  >
                    <option value="">All Status</option>
                    <option value="true">Active</option>
                    <option value="false">Inactive</option>
                  </CFormSelect>
                </CCol>
                <CCol md={4}>
                  <CFormSelect
                    value={vaccinationStatus}
                    onChange={(e) => setVaccinationStatus(e.target.value)}
                  >
                    <option value="">All Vaccinations</option>
                    <option value="Administered">Compliant</option>
                    <option value="Due">Due</option>
                    <option value="Overdue">Overdue</option>
                    <option value="Exempted">Exempted</option>
                  </CFormSelect>
                </CCol>
              </CRow>
            </CCol>
          </CRow>
        </CCardHeader>
        <CCardBody className="p-0">
          {healthRecordsData?.records && healthRecordsData.records.length > 0 ? (
            <>
              <CTable hover responsive>
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell 
                      scope="col" 
                      className="cursor-pointer"
                      onClick={() => handleSort('personName')}
                    >
                      Person {sortBy === 'personName' && (sortOrder === 'asc' ? '↑' : '↓')}
                    </CTableHeaderCell>
                    <CTableHeaderCell scope="col">Type</CTableHeaderCell>
                    <CTableHeaderCell scope="col">Blood Type</CTableHeaderCell>
                    <CTableHeaderCell scope="col">Vaccination Status</CTableHeaderCell>
                    <CTableHeaderCell scope="col">Medical Conditions</CTableHeaderCell>
                    <CTableHeaderCell scope="col">Emergency Contacts</CTableHeaderCell>
                    <CTableHeaderCell 
                      scope="col" 
                      className="cursor-pointer"
                      onClick={() => handleSort('lastModifiedAt')}
                    >
                      Last Updated {sortBy === 'lastModifiedAt' && (sortOrder === 'asc' ? '↑' : '↓')}
                    </CTableHeaderCell>
                    <CTableHeaderCell scope="col">Status</CTableHeaderCell>
                    <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {healthRecordsData.records.map((record) => (
                    <CTableRow key={record.id} className="cursor-pointer">
                      <CTableDataCell onClick={() => handleViewRecord(record.id.toString())}>
                        <div className="d-flex align-items-center">
                          <FontAwesomeIcon 
                            icon={getPersonTypeIcon(record.personType)} 
                            className="me-2"
                          />
                          <div>
                            <div className="fw-semibold">{record.personName}</div>
                            <div className="small text-muted">{record.personEmail}</div>
                          </div>
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={getPersonTypeBadge(record.personType)}>
                          {record.personType}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        {record.bloodType || '-'}
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={getVaccinationStatusBadge(record)}>
                          {getVaccinationStatusText(record)}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex align-items-center">
                          <span className="me-2">{record.medicalConditionsCount}</span>
                          {record.hasCriticalConditions && (
                            <CBadge color="danger" className="ms-1">
                              <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" size="xs" />
                              Critical
                            </CBadge>
                          )}
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex align-items-center">
                          <span className="me-2">{record.emergencyContactsCount}</span>
                          {record.emergencyContactsCount === 0 && (
                            <CBadge color="warning">
                              <FontAwesomeIcon icon={faExclamationTriangle} size="xs" />
                            </CBadge>
                          )}
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="small text-muted">
                          {record.lastModifiedAt ? formatDate(record.lastModifiedAt) : formatDate(record.createdAt)}
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={record.isActive ? 'success' : 'secondary'}>
                          {record.isActive ? 'Active' : 'Inactive'}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CDropdown>
                          <CDropdownToggle color="ghost" size="sm">
                            <FontAwesomeIcon icon={faEllipsisV} />
                          </CDropdownToggle>
                          <CDropdownMenu>
                            <CDropdownItem onClick={() => handleViewRecord(record.id.toString())}>
                              View Details
                            </CDropdownItem>
                            <CDropdownItem onClick={() => handleEditRecord(record.id.toString())}>
                              <FontAwesomeIcon icon={faEdit} className="me-1" />
                              Edit
                            </CDropdownItem>
                            <hr className="dropdown-divider" />
                            <CDropdownItem className="text-danger">
                              <FontAwesomeIcon icon={faTrash} className="me-1" />
                              Deactivate
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
                <div className="d-flex justify-content-between align-items-center p-3">
                  <div className="small text-muted">
                    Showing {((page - 1) * pageSize) + 1} to {Math.min(page * pageSize, healthRecordsData.totalCount)} of {healthRecordsData.totalCount} records
                  </div>
                  <CPagination aria-label="Health records pagination">
                    <CPaginationItem
                      disabled={page === 1}
                      onClick={() => setPage(page - 1)}
                    >
                      Previous
                    </CPaginationItem>
                    
                    {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                      let startPage = Math.max(1, page - 2);
                      const endPage = Math.min(startPage + 4, totalPages);
                      
                      if (endPage - startPage < 4) {
                        startPage = Math.max(1, endPage - 4);
                      }
                      
                      const pageNum = startPage + i;
                      if (pageNum > totalPages) return null;
                      
                      return (
                        <CPaginationItem
                          key={`page-${pageNum}`}
                          active={pageNum === page}
                          onClick={() => setPage(pageNum)}
                        >
                          {pageNum}
                        </CPaginationItem>
                      );
                    }).filter(Boolean)}
                    
                    <CPaginationItem
                      disabled={page === totalPages}
                      onClick={() => setPage(page + 1)}
                    >
                      Next
                    </CPaginationItem>
                  </CPagination>
                </div>
              )}
            </>
          ) : (
            <div className="text-center p-4">
              <FontAwesomeIcon icon={faUsers} size="3x" className="text-muted mb-3" />
              <h5 className="text-muted">No Health Records Found</h5>
              <p className="text-muted">Start by creating a new health record.</p>
              <CButton color="primary" onClick={handleCreateNew}>
                <FontAwesomeIcon icon={faPlus} className="me-1" />
                Create First Health Record
              </CButton>
            </div>
          )}
        </CCardBody>
      </CCard>
    </div>
  );
};

export default HealthList;