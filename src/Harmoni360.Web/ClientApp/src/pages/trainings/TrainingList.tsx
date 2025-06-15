import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CForm,
  CFormInput,
  CFormSelect,
  CInputGroup,
  CInputGroupText,
  CTable,
  CTableHead,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CTableRow,
  CBadge,
  CSpinner,
  CAlert,
  CPagination,
  CPaginationItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CButtonGroup,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSearch,
  faFilter,
  faPlus,
  faEdit,
  faEye,
  faTrash,
  faPlay,
  faCheck,
  faTimes,
  faGraduationCap,
  faExclamationTriangle,
  faClock,
  faSort,
  faSortUp,
  faSortDown,
  faDownload,
  faUsers,
  faUserPlus,
  faCalendarAlt,
  faCertificate
} from '@fortawesome/free-solid-svg-icons';

import { 
  useGetTrainingsQuery, 
  useDeleteTrainingMutation,
  useStartTrainingMutation,
  useCompleteTrainingMutation,
  useCancelTrainingMutation
} from '../../features/trainings/trainingApi';
import { useGetDepartmentsQuery } from '../../api/configurationApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { useDebounce } from '../../hooks/useDebounce';
import {
  TrainingDto,
  TRAINING_TYPES,
  TRAINING_CATEGORIES,
  TRAINING_PRIORITIES,
  DELIVERY_METHODS
} from '../../types/training';
import { formatDistanceToNow, format } from 'date-fns';

interface TrainingFilters {
  search: string;
  status: string;
  type: string;
  category: string;
  priority: string;
  deliveryMethod: string;
  instructorName: string;
  dateFrom: string;
  dateTo: string;
  isK3Training?: boolean;
  sortBy: string;
  sortDescending: boolean;
}

const TRAINING_STATUSES = [
  { value: '', label: 'All Statuses' },
  { value: 'Draft', label: 'Draft' },
  { value: 'Scheduled', label: 'Scheduled' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Cancelled', label: 'Cancelled' },
  { value: 'Postponed', label: 'Postponed' }
];

const TrainingList: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { isDemo } = useApplicationMode();

  // State
  const [filters, setFilters] = useState<TrainingFilters>({
    search: searchParams.get('search') || '',
    status: searchParams.get('status') || '',
    type: searchParams.get('type') || '',
    category: searchParams.get('category') || '',
    priority: searchParams.get('priority') || '',
    deliveryMethod: searchParams.get('deliveryMethod') || '',
    instructorName: searchParams.get('instructorName') || '',
    dateFrom: searchParams.get('dateFrom') || '',
    dateTo: searchParams.get('dateTo') || '',
    isK3Training: searchParams.get('isK3Training') === 'true' || undefined,
    sortBy: searchParams.get('sortBy') || 'CreatedAt',
    sortDescending: searchParams.get('sortDescending') === 'true'
  });

  const [currentPage, setCurrentPage] = useState(Number(searchParams.get('page')) || 1);
  const [pageSize] = useState(20);
  const [selectedTrainings, setSelectedTrainings] = useState<number[]>([]);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [trainingToDelete, setTrainingToDelete] = useState<TrainingDto | null>(null);

  // Debounced search
  const debouncedSearch = useDebounce(filters.search, 300);

  // API queries
  const { data: departments } = useGetDepartmentsQuery({});
  const { 
    data: trainingsData, 
    isLoading, 
    error 
  } = useGetTrainingsQuery({
    page: currentPage,
    pageSize,
    search: debouncedSearch,
    status: filters.status || undefined,
    type: filters.type || undefined,
    category: filters.category || undefined,
    priority: filters.priority || undefined,
    deliveryMethod: filters.deliveryMethod || undefined,
    instructorName: filters.instructorName || undefined,
    dateFrom: filters.dateFrom || undefined,
    dateTo: filters.dateTo || undefined,
    isK3Training: filters.isK3Training,
    sortBy: filters.sortBy,
    sortDirection: filters.sortDescending ? 'desc' : 'asc'
  });

  // Mutations
  const [deleteTraining, { isLoading: isDeleting }] = useDeleteTrainingMutation();
  const [startTraining, { isLoading: isStarting }] = useStartTrainingMutation();
  const [completeTraining, { isLoading: isCompleting }] = useCompleteTrainingMutation();
  const [cancelTraining, { isLoading: isCancelling }] = useCancelTrainingMutation();

  // Update URL when filters change
  useEffect(() => {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== '' && value !== undefined && value !== null) {
        params.set(key, String(value));
      }
    });
    if (currentPage > 1) params.set('page', String(currentPage));
    setSearchParams(params);
  }, [filters, currentPage, setSearchParams]);

  const handleFilterChange = (key: keyof TrainingFilters, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setCurrentPage(1); // Reset to first page when filtering
  };

  const handleSort = (column: string) => {
    if (filters.sortBy === column) {
      setFilters(prev => ({ ...prev, sortDescending: !prev.sortDescending }));
    } else {
      setFilters(prev => ({ ...prev, sortBy: column, sortDescending: false }));
    }
  };

  const getSortIcon = (column: string) => {
    if (filters.sortBy !== column) return faSort;
    return filters.sortDescending ? faSortDown : faSortUp;
  };

  const getStatusBadge = (status: string) => {
    const config: Record<string, { color: string; icon: any }> = {
      'Draft': { color: 'secondary', icon: faEdit },
      'Scheduled': { color: 'info', icon: faCalendarAlt },
      'InProgress': { color: 'warning', icon: faPlay },
      'Completed': { color: 'success', icon: faCheck },
      'Cancelled': { color: 'danger', icon: faTimes },
      'Postponed': { color: 'warning', icon: faClock }
    };

    const { color, icon } = config[status] || { color: 'secondary', icon: faGraduationCap };

    return (
      <CBadge color={color} className="d-flex align-items-center">
        <FontAwesomeIcon icon={icon} className="me-1" size="sm" />
        {status}
      </CBadge>
    );
  };

  const getPriorityBadge = (priority: string) => {
    const config: Record<string, string> = {
      'Low': 'success',
      'Medium': 'warning',
      'High': 'danger',
      'Critical': 'dark'
    };

    return <CBadge color={config[priority] || 'secondary'}>{priority}</CBadge>;
  };

  const handleDeleteClick = (training: TrainingDto) => {
    setTrainingToDelete(training);
    setShowDeleteModal(true);
  };

  const handleDeleteConfirm = async () => {
    if (!trainingToDelete) return;

    try {
      await deleteTraining(trainingToDelete.id).unwrap();
      setShowDeleteModal(false);
      setTrainingToDelete(null);
    } catch (error) {
      console.error('Delete failed:', error);
    }
  };

  const handleStartTraining = async (trainingId: number) => {
    try {
      await startTraining(trainingId).unwrap();
    } catch (error) {
      console.error('Start training failed:', error);
    }
  };

  const handleCompleteTraining = async (trainingId: number) => {
    try {
      await completeTraining({ id: trainingId }).unwrap();
    } catch (error) {
      console.error('Complete training failed:', error);
    }
  };

  const handleCancelTraining = async (trainingId: number, reason: string = 'Cancelled by user') => {
    try {
      await cancelTraining({ id: trainingId, reason }).unwrap();
    } catch (error) {
      console.error('Cancel training failed:', error);
    }
  };

  const clearFilters = () => {
    setFilters({
      search: '',
      status: '',
      type: '',
      category: '',
      priority: '',
      deliveryMethod: '',
      instructorName: '',
      dateFrom: '',
      dateTo: '',
      isK3Training: undefined,
      sortBy: 'CreatedAt',
      sortDescending: true
    });
    setCurrentPage(1);
  };

  if (error) {
    return (
      <CAlert color="danger">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Failed to load trainings. Please try again.
      </CAlert>
    );
  }

  return (
    <>
      <CRow>
        <CCol xs={12}>
          <CCard>
            <CCardHeader>
              <div className="d-flex justify-content-between align-items-center">
                <div className="d-flex align-items-center">
                  <FontAwesomeIcon icon={faGraduationCap} className="me-2 text-primary" />
                  <h5 className="mb-0">Training Management</h5>
                  {trainingsData && (
                    <CBadge color="info" className="ms-2">
                      {trainingsData.totalCount} total
                    </CBadge>
                  )}
                </div>
                <div>
                  <PermissionGuard
                    moduleType={ModuleType.TrainingManagement}
                    permissionType={PermissionType.Create}
                  >
                    <CButton
                      color="primary"
                      onClick={() => navigate('/trainings/create')}
                      className="me-2"
                    >
                      <FontAwesomeIcon icon={faPlus} className="me-1" />
                      Create Training
                    </CButton>
                  </PermissionGuard>
                </div>
              </div>
            </CCardHeader>

            <CCardBody>
              {/* Filters */}
              <CCard className="mb-4">
                <CCardBody>
                  <CRow className="g-3">
                    <CCol md={3}>
                      <CInputGroup>
                        <CInputGroupText>
                          <FontAwesomeIcon icon={faSearch} />
                        </CInputGroupText>
                        <CFormInput
                          placeholder="Search trainings..."
                          value={filters.search}
                          onChange={(e) => handleFilterChange('search', e.target.value)}
                        />
                      </CInputGroup>
                    </CCol>

                    <CCol md={2}>
                      <CFormSelect
                        value={filters.status}
                        onChange={(e) => handleFilterChange('status', e.target.value)}
                      >
                        {TRAINING_STATUSES.map(status => (
                          <option key={status.value} value={status.value}>{status.label}</option>
                        ))}
                      </CFormSelect>
                    </CCol>

                    <CCol md={2}>
                      <CFormSelect
                        value={filters.type}
                        onChange={(e) => handleFilterChange('type', e.target.value)}
                      >
                        <option value="">All Types</option>
                        {TRAINING_TYPES.map(type => (
                          <option key={type.value} value={type.value}>{type.label}</option>
                        ))}
                      </CFormSelect>
                    </CCol>

                    <CCol md={2}>
                      <CFormSelect
                        value={filters.category}
                        onChange={(e) => handleFilterChange('category', e.target.value)}
                      >
                        <option value="">All Categories</option>
                        {TRAINING_CATEGORIES.map(category => (
                          <option key={category.value} value={category.value}>{category.label}</option>
                        ))}
                      </CFormSelect>
                    </CCol>

                    <CCol md={2}>
                      <CFormSelect
                        value={filters.priority}
                        onChange={(e) => handleFilterChange('priority', e.target.value)}
                      >
                        <option value="">All Priorities</option>
                        {TRAINING_PRIORITIES.map(priority => (
                          <option key={priority.value} value={priority.value}>{priority.label}</option>
                        ))}
                      </CFormSelect>
                    </CCol>

                    <CCol md={1}>
                      <CButton
                        color="secondary"
                        variant="outline"
                        onClick={clearFilters}
                        title="Clear Filters"
                      >
                        <FontAwesomeIcon icon={faTimes} />
                      </CButton>
                    </CCol>
                  </CRow>

                  <CRow className="g-3 mt-2">
                    <CCol md={2}>
                      <CFormSelect
                        value={filters.deliveryMethod}
                        onChange={(e) => handleFilterChange('deliveryMethod', e.target.value)}
                      >
                        <option value="">All Methods</option>
                        {DELIVERY_METHODS.map(method => (
                          <option key={method.value} value={method.value}>{method.label}</option>
                        ))}
                      </CFormSelect>
                    </CCol>

                    <CCol md={2}>
                      <CFormInput
                        placeholder="Instructor name"
                        value={filters.instructorName}
                        onChange={(e) => handleFilterChange('instructorName', e.target.value)}
                      />
                    </CCol>

                    <CCol md={2}>
                      <CFormInput
                        type="date"
                        placeholder="From date"
                        value={filters.dateFrom}
                        onChange={(e) => handleFilterChange('dateFrom', e.target.value)}
                      />
                    </CCol>

                    <CCol md={2}>
                      <CFormInput
                        type="date"
                        placeholder="To date"
                        value={filters.dateTo}
                        onChange={(e) => handleFilterChange('dateTo', e.target.value)}
                      />
                    </CCol>

                    <CCol md={2}>
                      <CFormSelect
                        value={filters.isK3Training === true ? 'true' : filters.isK3Training === false ? 'false' : ''}
                        onChange={(e) => handleFilterChange('isK3Training', e.target.value === '' ? undefined : e.target.value === 'true')}
                      >
                        <option value="">All Training Types</option>
                        <option value="true">K3 Training Only</option>
                        <option value="false">Non-K3 Training</option>
                      </CFormSelect>
                    </CCol>
                  </CRow>
                </CCardBody>
              </CCard>

              {/* Training List */}
              {isLoading ? (
                <div className="text-center py-4">
                  <CSpinner />
                  <div className="mt-2">Loading trainings...</div>
                </div>
              ) : (
                <>
                  <CTable hover responsive>
                    <CTableHead>
                      <CTableRow>
                        <CTableHeaderCell>
                          <button
                            className="btn btn-link p-0 text-decoration-none"
                            onClick={() => handleSort('Title')}
                          >
                            Training
                            <FontAwesomeIcon icon={getSortIcon('Title')} className="ms-1" />
                          </button>
                        </CTableHeaderCell>
                        <CTableHeaderCell>Type & Category</CTableHeaderCell>
                        <CTableHeaderCell>
                          <button
                            className="btn btn-link p-0 text-decoration-none"
                            onClick={() => handleSort('Status')}
                          >
                            Status
                            <FontAwesomeIcon icon={getSortIcon('Status')} className="ms-1" />
                          </button>
                        </CTableHeaderCell>
                        <CTableHeaderCell>
                          <button
                            className="btn btn-link p-0 text-decoration-none"
                            onClick={() => handleSort('Priority')}
                          >
                            Priority
                            <FontAwesomeIcon icon={getSortIcon('Priority')} className="ms-1" />
                          </button>
                        </CTableHeaderCell>
                        <CTableHeaderCell>
                          <button
                            className="btn btn-link p-0 text-decoration-none"
                            onClick={() => handleSort('ScheduledStartDate')}
                          >
                            Schedule
                            <FontAwesomeIcon icon={getSortIcon('ScheduledStartDate')} className="ms-1" />
                          </button>
                        </CTableHeaderCell>
                        <CTableHeaderCell>Participants</CTableHeaderCell>
                        <CTableHeaderCell>Instructor</CTableHeaderCell>
                        <CTableHeaderCell width="120">Actions</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {trainingsData?.items.map((training) => (
                        <CTableRow key={training.id}>
                          <CTableDataCell>
                            <div>
                              <div className="fw-semibold">{training.title}</div>
                              <small className="text-muted">
                                {training.trainingCode}
                                {training.isK3MandatoryTraining && (
                                  <CBadge color="warning" className="ms-1">K3</CBadge>
                                )}
                                {training.requiresCertification && (
                                  <CBadge color="info" className="ms-1">
                                    <FontAwesomeIcon icon={faCertificate} className="me-1" />
                                    Certified
                                  </CBadge>
                                )}
                              </small>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div>
                              <div>{training.type.replace(/([A-Z])/g, ' $1').trim()}</div>
                              <small className="text-muted">{training.category.replace(/([A-Z])/g, ' $1').trim()}</small>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            {getStatusBadge(training.status)}
                          </CTableDataCell>
                          <CTableDataCell>
                            {getPriorityBadge(training.priority)}
                          </CTableDataCell>
                          <CTableDataCell>
                            <div>
                              <div className="fw-semibold">
                                {format(new Date(training.scheduledStartDate), 'MMM dd, yyyy')}
                              </div>
                              <small className="text-muted">
                                {training.durationHours}h • {training.deliveryMethod.replace(/([A-Z])/g, ' $1').trim()}
                              </small>
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div className="d-flex align-items-center">
                              <FontAwesomeIcon icon={faUsers} className="me-1 text-muted" />
                              <span>{training.currentParticipants}/{training.maxParticipants}</span>
                              {training.currentParticipants >= training.maxParticipants && (
                                <CBadge color="warning" className="ms-1">Full</CBadge>
                              )}
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div>
                              <div className="fw-semibold">{training.instructorName || 'TBD'}</div>
                              {training.isExternalInstructor && (
                                <small className="text-muted">External</small>
                              )}
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <CButtonGroup size="sm">
                              <CButton
                                color="primary"
                                variant="outline"
                                onClick={() => navigate(`/trainings/${training.id}`)}
                                title="View Details"
                              >
                                <FontAwesomeIcon icon={faEye} />
                              </CButton>

                              <PermissionGuard
                                moduleType={ModuleType.TrainingManagement}
                                permissionType={PermissionType.Update}
                              >
                                {training.canEdit && (
                                  <CButton
                                    color="secondary"
                                    variant="outline"
                                    onClick={() => navigate(`/trainings/${training.id}/edit`)}
                                    title="Edit Training"
                                  >
                                    <FontAwesomeIcon icon={faEdit} />
                                  </CButton>
                                )}

                                {training.canStart && (
                                  <CButton
                                    color="success"
                                    variant="outline"
                                    onClick={() => handleStartTraining(training.id)}
                                    disabled={isStarting}
                                    title="Start Training"
                                  >
                                    <FontAwesomeIcon icon={faPlay} />
                                  </CButton>
                                )}

                                {training.canComplete && (
                                  <CButton
                                    color="info"
                                    variant="outline"
                                    onClick={() => handleCompleteTraining(training.id)}
                                    disabled={isCompleting}
                                    title="Complete Training"
                                  >
                                    <FontAwesomeIcon icon={faCheck} />
                                  </CButton>
                                )}

                                {training.canEnroll && (
                                  <CButton
                                    color="warning"
                                    variant="outline"
                                    onClick={() => navigate(`/trainings/${training.id}/enroll`)}
                                    title="Enroll Participants"
                                  >
                                    <FontAwesomeIcon icon={faUserPlus} />
                                  </CButton>
                                )}
                              </PermissionGuard>

                              <PermissionGuard
                                moduleType={ModuleType.TrainingManagement}
                                permissionType={PermissionType.Delete}
                              >
                                {training.status === 'Draft' && (
                                  <CButton
                                    color="danger"
                                    variant="outline"
                                    onClick={() => handleDeleteClick(training)}
                                    title="Delete Training"
                                  >
                                    <FontAwesomeIcon icon={faTrash} />
                                  </CButton>
                                )}
                              </PermissionGuard>
                            </CButtonGroup>
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>

                  {trainingsData?.items.length === 0 && (
                    <div className="text-center py-4">
                      <FontAwesomeIcon icon={faGraduationCap} size="3x" className="text-muted mb-3" />
                      <h5 className="text-muted">No trainings found</h5>
                      <p className="text-muted">
                        {filters.search || filters.status || filters.type
                          ? 'Try adjusting your filters or search terms.'
                          : 'Create your first training to get started.'}
                      </p>
                      <PermissionGuard
                        moduleType={ModuleType.TrainingManagement}
                        permissionType={PermissionType.Create}
                      >
                        <CButton color="primary" onClick={() => navigate('/trainings/create')}>
                          <FontAwesomeIcon icon={faPlus} className="me-1" />
                          Create Training
                        </CButton>
                      </PermissionGuard>
                    </div>
                  )}

                  {/* Pagination */}
                  {trainingsData && trainingsData.pageCount > 1 && (
                    <CPagination className="mt-3" align="center" size="sm">
                      <CPaginationItem
                        disabled={currentPage === 1}
                        onClick={() => setCurrentPage(1)}
                      >
                        First
                      </CPaginationItem>
                      <CPaginationItem
                        disabled={!trainingsData.hasPreviousPage}
                        onClick={() => setCurrentPage(currentPage - 1)}
                      >
                        Previous
                      </CPaginationItem>

                      {Array.from({ length: Math.min(5, trainingsData.pageCount) }, (_, i) => {
                        const startPage = Math.max(1, currentPage - 2);
                        const pageNumber = startPage + i;
                        if (pageNumber > trainingsData.pageCount) return null;

                        return (
                          <CPaginationItem
                            key={pageNumber}
                            active={pageNumber === currentPage}
                            onClick={() => setCurrentPage(pageNumber)}
                          >
                            {pageNumber}
                          </CPaginationItem>
                        );
                      })}

                      <CPaginationItem
                        disabled={!trainingsData.hasNextPage}
                        onClick={() => setCurrentPage(currentPage + 1)}
                      >
                        Next
                      </CPaginationItem>
                      <CPaginationItem
                        disabled={currentPage === trainingsData.pageCount}
                        onClick={() => setCurrentPage(trainingsData.pageCount)}
                      >
                        Last
                      </CPaginationItem>
                    </CPagination>
                  )}
                </>
              )}
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Delete Confirmation Modal */}
      <CModal visible={showDeleteModal} onClose={() => setShowDeleteModal(false)}>
        <CModalHeader>
          <CModalTitle>Confirm Delete</CModalTitle>
        </CModalHeader>
        <CModalBody>
          Are you sure you want to delete the training "{trainingToDelete?.title}"? This action cannot be undone.
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowDeleteModal(false)}>
            Cancel
          </CButton>
          <CButton color="danger" onClick={handleDeleteConfirm} disabled={isDeleting}>
            {isDeleting ? (
              <>
                <CSpinner size="sm" className="me-1" />
                Deleting...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faTrash} className="me-1" />
                Delete
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default TrainingList;