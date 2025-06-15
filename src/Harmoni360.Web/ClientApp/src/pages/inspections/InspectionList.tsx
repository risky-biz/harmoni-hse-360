import React, { useState, useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CTable,
  CTableHead,
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CFormInput,
  CFormSelect,
  CInputGroup,
  CInputGroupText,
  CPagination,
  CPaginationItem,
  CBadge,
  CSpinner,
  CAlert,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CFormCheck,
  CCollapse,
  CButtonGroup
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faSearch,
  faFilter,
  faSort,
  faSortUp,
  faSortDown,
  faEye,
  faEdit,
  faPlay,
  faCheck,
  faTimes,
  faExclamationTriangle,
  faCalendarAlt,
  faUser,
  faBuilding,
  faClipboardCheck,
  faDownload,
  faRefresh
} from '@fortawesome/free-solid-svg-icons';
import { useGetInspectionsQuery } from '../../features/inspections/inspectionApi';
import { InspectionDto, InspectionStatus, InspectionType, InspectionCategory, InspectionPriority } from '../../types/inspection';
import DemoModeWrapper from '../../components/common/DemoModeWrapper';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { formatDistanceToNow, format } from 'date-fns';
import { useDebounce } from '../../hooks/useDebounce';
import { exportInspectionsToExcel, exportInspectionsToPDF } from '../../utils/exportUtils';
import { useMemoizedCalculation, performanceMonitor } from '../../utils/performanceUtils';

export const InspectionList: React.FC = () => {
  const navigate = useNavigate();
  
  // State for filters and pagination
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(25); // Increased default page size for better performance
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [selectedType, setSelectedType] = useState<string>('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [selectedPriority, setSelectedPriority] = useState<string>('');
  const [selectedInspector, setSelectedInspector] = useState<string>('');
  const [selectedDepartment, setSelectedDepartment] = useState<string>('');
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');
  const [showOverdueOnly, setShowOverdueOnly] = useState(false);
  const [sortBy, setSortBy] = useState('scheduledDate');
  const [sortDescending, setSortDescending] = useState(true);
  const [showFilters, setShowFilters] = useState(false);

  // Debounce search term to avoid too many API calls
  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  // Optimized query parameters with memoization
  const queryParams = useMemoizedCalculation(() => ({
    page,
    pageSize,
    searchTerm: debouncedSearchTerm,
    status: selectedStatus || undefined,
    type: selectedType || undefined,
    category: selectedCategory || undefined,
    priority: selectedPriority || undefined,
    inspectorId: selectedInspector ? Number(selectedInspector) : undefined,
    departmentId: selectedDepartment ? Number(selectedDepartment) : undefined,
    startDate: startDate || undefined,
    endDate: endDate || undefined,
    isOverdue: showOverdueOnly || undefined,
    sortBy,
    sortDescending
  }), [
    page, pageSize, debouncedSearchTerm, selectedStatus, selectedType, selectedCategory,
    selectedPriority, selectedInspector, selectedDepartment, startDate, endDate,
    showOverdueOnly, sortBy, sortDescending
  ]);

  // API query
  const { data, isLoading, error, refetch } = useGetInspectionsQuery(queryParams);

  // Mock data for dropdowns - Replace with actual API calls
  const inspectors = [
    { id: 1, name: 'John Smith' },
    { id: 2, name: 'Jane Doe' },
    { id: 3, name: 'Mike Johnson' },
    { id: 4, name: 'Sarah Wilson' }
  ];

  const departments = [
    { id: 1, name: 'Operations' },
    { id: 2, name: 'Maintenance' },
    { id: 3, name: 'Safety' },
    { id: 4, name: 'Environmental' }
  ];

  // Event handlers
  const handleCreateInspection = () => {
    navigate('/inspections/create');
  };

  const handleViewInspection = (id: number) => {
    navigate(`/inspections/${id}`);
  };

  const handleEditInspection = (id: number) => {
    navigate(`/inspections/${id}/edit`);
  };

  const handleSort = useCallback((column: string) => {
    if (sortBy === column) {
      setSortDescending(!sortDescending);
    } else {
      setSortBy(column);
      setSortDescending(false);
    }
  }, [sortBy, sortDescending]);

  const clearFilters = () => {
    setSearchTerm('');
    setSelectedStatus('');
    setSelectedType('');
    setSelectedCategory('');
    setSelectedPriority('');
    setSelectedInspector('');
    setSelectedDepartment('');
    setStartDate('');
    setEndDate('');
    setShowOverdueOnly(false);
    setPage(1);
  };

  const handleExportExcel = useCallback(() => {
    if (data?.items) {
      performanceMonitor.measureTimeAsync(async () => {
        exportInspectionsToExcel(data.items, {
          filename: `inspections-export-${format(new Date(), 'yyyy-MM-dd')}`
        });
      }, 'Excel Export');
    }
  }, [data?.items]);

  const handleExportPDF = useCallback(() => {
    if (data?.items) {
      performanceMonitor.measureTimeAsync(async () => {
        exportInspectionsToPDF(data.items, {
          filename: `inspections-report-${format(new Date(), 'yyyy-MM-dd')}`,
          title: 'Inspection Report'
        });
      }, 'PDF Export');
    }
  }, [data?.items]);

  // Memoized badge configurations for better performance
  const statusConfig = useMemoizedCalculation(() => ({
    [InspectionStatus.Draft]: { color: 'secondary', text: 'Draft' },
    [InspectionStatus.Scheduled]: { color: 'info', text: 'Scheduled' },
    [InspectionStatus.InProgress]: { color: 'warning', text: 'In Progress' },
    [InspectionStatus.Completed]: { color: 'success', text: 'Completed' },
    [InspectionStatus.Cancelled]: { color: 'danger', text: 'Cancelled' }
  }), []);

  const priorityConfig = useMemoizedCalculation(() => ({
    [InspectionPriority.Low]: { color: 'success', text: 'Low' },
    [InspectionPriority.Medium]: { color: 'warning', text: 'Medium' },
    [InspectionPriority.High]: { color: 'danger', text: 'High' },
    [InspectionPriority.Critical]: { color: 'dark', text: 'Critical' }
  }), []);

  const getStatusBadge = useCallback((status: InspectionStatus) => {
    const config = statusConfig[status] || { color: 'secondary', text: status };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  }, [statusConfig]);

  const getPriorityBadge = useCallback((priority: InspectionPriority) => {
    const config = priorityConfig[priority] || { color: 'secondary', text: priority };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  }, [priorityConfig]);

  const getSortIcon = (column: string) => {
    if (sortBy !== column) return faSort;
    return sortDescending ? faSortDown : faSortUp;
  };

  if (error) {
    return (
      <CAlert color="danger">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Error loading inspections. Please try again.
      </CAlert>
    );
  }

  return (
    <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Read}>
      <DemoModeWrapper>
        <CRow>
          <CCol>
            <CCard>
              <CCardHeader className="d-flex justify-content-between align-items-center">
                <div>
                  <h4 className="mb-0">
                    <FontAwesomeIcon icon={faClipboardCheck} className="me-2 text-primary" />
                    Inspections
                  </h4>
                  <small className="text-medium-emphasis">
                    Manage and track safety inspections
                  </small>
                </div>
                <div>
                  <CDropdown className="me-2">
                    <CDropdownToggle color="light" variant="outline">
                      <FontAwesomeIcon icon={faDownload} className="me-1" />
                      Export
                    </CDropdownToggle>
                    <CDropdownMenu>
                      <CDropdownItem onClick={handleExportExcel}>
                        <FontAwesomeIcon icon={faDownload} className="me-2" />
                        Export to Excel
                      </CDropdownItem>
                      <CDropdownItem onClick={handleExportPDF}>
                        <FontAwesomeIcon icon={faDownload} className="me-2" />
                        Export to PDF
                      </CDropdownItem>
                    </CDropdownMenu>
                  </CDropdown>
                  <CButton
                    color="light"
                    variant="outline"
                    className="me-2"
                    onClick={() => refetch()}
                    disabled={isLoading}
                  >
                    <FontAwesomeIcon icon={faRefresh} className="me-1" />
                    Refresh
                  </CButton>
                  <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Create}>
                    <CButton
                      color="primary"
                      onClick={handleCreateInspection}
                    >
                      <FontAwesomeIcon icon={faPlus} className="me-1" />
                      Create Inspection
                    </CButton>
                  </PermissionGuard>
                </div>
              </CCardHeader>
              <CCardBody>
                {/* Search and Filters */}
                <CRow className="mb-3">
                  <CCol md={6}>
                    <CInputGroup>
                      <CInputGroupText>
                        <FontAwesomeIcon icon={faSearch} />
                      </CInputGroupText>
                      <CFormInput
                        placeholder="Search inspections..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                      />
                    </CInputGroup>
                  </CCol>
                  <CCol md={6} className="d-flex justify-content-end align-items-center">
                    <CButtonGroup>
                      <CButton
                        color="light"
                        variant="outline"
                        onClick={() => setShowFilters(!showFilters)}
                      >
                        <FontAwesomeIcon icon={faFilter} className="me-1" />
                        Filters
                      </CButton>
                      {(selectedStatus || selectedType || selectedCategory || selectedPriority || 
                        selectedInspector || selectedDepartment || startDate || endDate || showOverdueOnly) && (
                        <CButton
                          color="light"
                          variant="outline"
                          onClick={clearFilters}
                        >
                          Clear
                        </CButton>
                      )}
                    </CButtonGroup>
                  </CCol>
                </CRow>

                {/* Advanced Filters */}
                <CCollapse visible={showFilters}>
                  <CCard className="mb-3">
                    <CCardBody>
                      <CRow>
                        <CCol md={3}>
                          <CFormSelect
                            value={selectedStatus}
                            onChange={(e) => setSelectedStatus(e.target.value)}
                          >
                            <option value="">All Statuses</option>
                            <option value="Draft">Draft</option>
                            <option value="Scheduled">Scheduled</option>
                            <option value="InProgress">In Progress</option>
                            <option value="Completed">Completed</option>
                            <option value="Cancelled">Cancelled</option>
                          </CFormSelect>
                        </CCol>
                        <CCol md={3}>
                          <CFormSelect
                            value={selectedType}
                            onChange={(e) => setSelectedType(e.target.value)}
                          >
                            <option value="">All Types</option>
                            <option value="Safety">Safety</option>
                            <option value="Environmental">Environmental</option>
                            <option value="Quality">Quality</option>
                            <option value="Security">Security</option>
                            <option value="Maintenance">Maintenance</option>
                            <option value="Compliance">Compliance</option>
                          </CFormSelect>
                        </CCol>
                        <CCol md={3}>
                          <CFormSelect
                            value={selectedCategory}
                            onChange={(e) => setSelectedCategory(e.target.value)}
                          >
                            <option value="">All Categories</option>
                            <option value="Routine">Routine</option>
                            <option value="Scheduled">Scheduled</option>
                            <option value="Emergency">Emergency</option>
                            <option value="Incident">Incident-Based</option>
                            <option value="Audit">Audit</option>
                          </CFormSelect>
                        </CCol>
                        <CCol md={3}>
                          <CFormSelect
                            value={selectedPriority}
                            onChange={(e) => setSelectedPriority(e.target.value)}
                          >
                            <option value="">All Priorities</option>
                            <option value="Low">Low</option>
                            <option value="Medium">Medium</option>
                            <option value="High">High</option>
                            <option value="Critical">Critical</option>
                          </CFormSelect>
                        </CCol>
                      </CRow>
                      <CRow className="mt-3">
                        <CCol md={3}>
                          <CFormSelect
                            value={selectedInspector}
                            onChange={(e) => setSelectedInspector(e.target.value)}
                          >
                            <option value="">All Inspectors</option>
                            {inspectors.map(inspector => (
                              <option key={inspector.id} value={inspector.id}>
                                {inspector.name}
                              </option>
                            ))}
                          </CFormSelect>
                        </CCol>
                        <CCol md={3}>
                          <CFormSelect
                            value={selectedDepartment}
                            onChange={(e) => setSelectedDepartment(e.target.value)}
                          >
                            <option value="">All Departments</option>
                            {departments.map(department => (
                              <option key={department.id} value={department.id}>
                                {department.name}
                              </option>
                            ))}
                          </CFormSelect>
                        </CCol>
                        <CCol md={2}>
                          <CFormInput
                            type="date"
                            value={startDate}
                            onChange={(e) => setStartDate(e.target.value)}
                            placeholder="Start Date"
                          />
                        </CCol>
                        <CCol md={2}>
                          <CFormInput
                            type="date"
                            value={endDate}
                            onChange={(e) => setEndDate(e.target.value)}
                            placeholder="End Date"
                          />
                        </CCol>
                        <CCol md={2}>
                          <CFormCheck
                            id="showOverdueOnly"
                            checked={showOverdueOnly}
                            onChange={(e) => setShowOverdueOnly(e.target.checked)}
                            label="Overdue Only"
                          />
                        </CCol>
                      </CRow>
                    </CCardBody>
                  </CCard>
                </CCollapse>

                {/* Results Info */}
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <div>
                    {data && (
                      <small className="text-medium-emphasis">
                        Showing {((page - 1) * pageSize) + 1} to {Math.min(page * pageSize, data.totalCount)} of {data.totalCount} inspections
                      </small>
                    )}
                  </div>
                  <div className="d-flex align-items-center">
                    <span className="me-2">Show:</span>
                    <CFormSelect
                      size="sm"
                      value={pageSize}
                      onChange={(e) => setPageSize(Number(e.target.value))}
                      style={{ width: 'auto' }}
                    >
                      <option value={10}>10</option>
                      <option value={25}>25</option>
                      <option value={50}>50</option>
                      <option value={100}>100</option>
                    </CFormSelect>
                  </div>
                </div>

                {/* Inspections Table */}
                {isLoading ? (
                  <div className="text-center py-4">
                    <CSpinner color="primary" />
                    <div className="mt-2">Loading inspections...</div>
                  </div>
                ) : (
                  <CTable hover responsive>
                    <CTableHead>
                      <CTableRow>
                        <CTableHeaderCell
                          scope="col"
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('title')}
                        >
                          Title
                          <FontAwesomeIcon icon={getSortIcon('title')} className="ms-1" />
                        </CTableHeaderCell>
                        <CTableHeaderCell
                          scope="col"
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('type')}
                        >
                          Type
                          <FontAwesomeIcon icon={getSortIcon('type')} className="ms-1" />
                        </CTableHeaderCell>
                        <CTableHeaderCell
                          scope="col"
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('status')}
                        >
                          Status
                          <FontAwesomeIcon icon={getSortIcon('status')} className="ms-1" />
                        </CTableHeaderCell>
                        <CTableHeaderCell
                          scope="col"
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('priority')}
                        >
                          Priority
                          <FontAwesomeIcon icon={getSortIcon('priority')} className="ms-1" />
                        </CTableHeaderCell>
                        <CTableHeaderCell
                          scope="col"
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('scheduledDate')}
                        >
                          Scheduled Date
                          <FontAwesomeIcon icon={getSortIcon('scheduledDate')} className="ms-1" />
                        </CTableHeaderCell>
                        <CTableHeaderCell
                          scope="col"
                          style={{ cursor: 'pointer' }}
                          onClick={() => handleSort('inspector')}
                        >
                          Inspector
                          <FontAwesomeIcon icon={getSortIcon('inspector')} className="ms-1" />
                        </CTableHeaderCell>
                        <CTableHeaderCell scope="col">Progress</CTableHeaderCell>
                        <CTableHeaderCell scope="col">Actions</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {data?.items.map((inspection: InspectionDto) => (
                        <CTableRow key={inspection.id}>
                          <CTableDataCell>
                            <div>
                              <div className="fw-semibold">{inspection.title}</div>
                              <small className="text-medium-emphasis">
                                #{inspection.inspectionNumber}
                              </small>
                              {inspection.isOverdue && (
                                <CBadge color="danger" className="ms-1">
                                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                                  Overdue
                                </CBadge>
                              )}
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <CBadge color="info" className="me-1">
                              {inspection.typeName}
                            </CBadge>
                            <div>
                              <small className="text-medium-emphasis">
                                {inspection.categoryName}
                              </small>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            {getStatusBadge(inspection.status)}
                          </CTableDataCell>
                          <CTableDataCell>
                            {getPriorityBadge(inspection.priority)}
                          </CTableDataCell>
                          <CTableDataCell>
                            <div>
                              <FontAwesomeIcon icon={faCalendarAlt} className="me-1 text-muted" />
                              {format(new Date(inspection.scheduledDate), 'MMM dd, yyyy HH:mm')}
                            </div>
                            {inspection.status === InspectionStatus.Completed && inspection.completedDate && (
                              <small className="text-success">
                                Completed {formatDistanceToNow(new Date(inspection.completedDate))} ago
                              </small>
                            )}
                          </CTableDataCell>
                          <CTableDataCell>
                            <div>
                              <FontAwesomeIcon icon={faUser} className="me-1 text-muted" />
                              {inspection.inspectorName}
                            </div>
                            <small className="text-medium-emphasis">
                              <FontAwesomeIcon icon={faBuilding} className="me-1" />
                              {inspection.departmentName}
                            </small>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div className="d-flex align-items-center">
                              <div className="flex-grow-1 me-2">
                                <div className="progress" style={{ height: '4px' }}>
                                  <div
                                    className="progress-bar"
                                    role="progressbar"
                                    style={{ width: `${(inspection.completedItemsCount / inspection.itemsCount) * 100}%` }}
                                  />
                                </div>
                              </div>
                              <small className="text-muted">
                                {inspection.completedItemsCount}/{inspection.itemsCount}
                              </small>
                            </div>
                            {inspection.criticalFindingsCount > 0 && (
                              <small className="text-danger">
                                <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                                {inspection.criticalFindingsCount} critical finding{inspection.criticalFindingsCount > 1 ? 's' : ''}
                              </small>
                            )}
                          </CTableDataCell>
                          <CTableDataCell>
                            <CDropdown>
                              <CDropdownToggle color="light" variant="outline" size="sm">
                                Actions
                              </CDropdownToggle>
                              <CDropdownMenu>
                                <CDropdownItem onClick={() => handleViewInspection(inspection.id)}>
                                  <FontAwesomeIcon icon={faEye} className="me-2" />
                                  View Details
                                </CDropdownItem>
                                {inspection.canEdit && (
                                  <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                                    <CDropdownItem onClick={() => handleEditInspection(inspection.id)}>
                                      <FontAwesomeIcon icon={faEdit} className="me-2" />
                                      Edit
                                    </CDropdownItem>
                                  </PermissionGuard>
                                )}
                                {inspection.canStart && (
                                  <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                                    <CDropdownItem>
                                      <FontAwesomeIcon icon={faPlay} className="me-2" />
                                      Start Inspection
                                    </CDropdownItem>
                                  </PermissionGuard>
                                )}
                                {inspection.canComplete && (
                                  <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                                    <CDropdownItem>
                                      <FontAwesomeIcon icon={faCheck} className="me-2" />
                                      Complete
                                    </CDropdownItem>
                                  </PermissionGuard>
                                )}
                              </CDropdownMenu>
                            </CDropdown>
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>
                )}

                {/* Pagination */}
                {data && data.totalPages > 1 && (
                  <div className="d-flex justify-content-center mt-3">
                    <CPagination>
                      <CPaginationItem
                        disabled={!data.hasPrevious}
                        onClick={() => setPage(page - 1)}
                      >
                        Previous
                      </CPaginationItem>
                      {Array.from({ length: Math.min(5, data.totalPages) }, (_, i) => {
                        const pageNumber = Math.max(1, Math.min(data.totalPages - 4, page - 2)) + i;
                        return (
                          <CPaginationItem
                            key={pageNumber}
                            active={pageNumber === page}
                            onClick={() => setPage(pageNumber)}
                          >
                            {pageNumber}
                          </CPaginationItem>
                        );
                      })}
                      <CPaginationItem
                        disabled={!data.hasNext}
                        onClick={() => setPage(page + 1)}
                      >
                        Next
                      </CPaginationItem>
                    </CPagination>
                  </div>
                )}

                {/* Empty State */}
                {data && data.items.length === 0 && (
                  <div className="text-center py-4">
                    <FontAwesomeIcon icon={faClipboardCheck} size="3x" className="text-muted mb-3" />
                    <h5>No inspections found</h5>
                    <p className="text-muted">
                      {searchTerm || selectedStatus || selectedType ? 
                        'Try adjusting your search criteria or filters.' :
                        'Get started by creating your first inspection.'
                      }
                    </p>
                    {!searchTerm && !selectedStatus && !selectedType && (
                      <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Create}>
                        <CButton color="primary" onClick={handleCreateInspection}>
                          <FontAwesomeIcon icon={faPlus} className="me-1" />
                          Create Inspection
                        </CButton>
                      </PermissionGuard>
                    )}
                  </div>
                )}
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      </DemoModeWrapper>
    </PermissionGuard>
  );
};