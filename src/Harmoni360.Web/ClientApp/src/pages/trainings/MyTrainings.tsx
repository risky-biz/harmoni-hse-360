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
  CButtonGroup,
  CProgress,
  CProgressBar,
  CTabs,
  CTabList,
  CTab,
  CTabContent,
  CTabPanel
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSearch,
  faGraduationCap,
  faCalendarAlt,
  faClock,
  faCheck,
  faPlay,
  faTimes,
  faEye,
  faDownload,
  faCertificate,
  faChartLine,
  faHistory,
  faExclamationTriangle,
  faUser,
  faBookOpen,
  faAward,
  faFileAlt
} from '@fortawesome/free-solid-svg-icons';
import { format, formatDistanceToNow, isAfter, isBefore } from 'date-fns';

import {
  useGetMyTrainingsQuery,
  useGetMyTrainingStatsQuery,
  useGetMyUpcomingTrainingsQuery,
  useGetMyCertificatesQuery,
  useDownloadCertificateMutation
} from '../../features/trainings/trainingApi';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { useDebounce } from '../../hooks/useDebounce';
import {
  MyTrainingDto,
  TrainingEnrollmentStatus,
  TRAINING_TYPES,
  TRAINING_CATEGORIES
} from '../../types/training';

interface MyTrainingFilters {
  search: string;
  status: TrainingEnrollmentStatus | '';
  type: string;
  category: string;
  dateFrom: string;
  dateTo: string;
  sortBy: string;
  sortDescending: boolean;
}

const ENROLLMENT_STATUSES = [
  { value: '', label: 'All Statuses' },
  { value: 'Enrolled', label: 'Enrolled' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Failed', label: 'Failed' },
  { value: 'Cancelled', label: 'Cancelled' }
];

const MyTrainings: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { isDemo } = useApplicationMode();

  const [activeTab, setActiveTab] = useState('all-trainings');
  const [filters, setFilters] = useState<MyTrainingFilters>({
    search: searchParams.get('search') || '',
    status: (searchParams.get('status') as TrainingEnrollmentStatus) || '',
    type: searchParams.get('type') || '',
    category: searchParams.get('category') || '',
    dateFrom: searchParams.get('dateFrom') || '',
    dateTo: searchParams.get('dateTo') || '',
    sortBy: searchParams.get('sortBy') || 'EnrollmentDate',
    sortDescending: searchParams.get('sortDescending') === 'true'
  });

  const [currentPage, setCurrentPage] = useState(Number(searchParams.get('page')) || 1);
  const [pageSize] = useState(20);

  // Debounced search
  const debouncedSearch = useDebounce(filters.search, 300);

  // API queries
  const { data: myTrainings, isLoading, error } = useGetMyTrainingsQuery({
    page: currentPage,
    pageSize,
    search: debouncedSearch,
    status: filters.status || undefined,
    type: filters.type || undefined,
    category: filters.category || undefined,
    dateFrom: filters.dateFrom || undefined,
    dateTo: filters.dateTo || undefined,
    sortBy: filters.sortBy,
    sortDirection: filters.sortDescending ? 'desc' : 'asc'
  });

  const { data: trainingStats } = useGetMyTrainingStatsQuery();
  const { data: upcomingTrainings } = useGetMyUpcomingTrainingsQuery();
  const { data: myCertificates } = useGetMyCertificatesQuery();

  const [downloadCertificate, { isLoading: isDownloading }] = useDownloadCertificateMutation();

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

  const handleFilterChange = (key: keyof MyTrainingFilters, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setCurrentPage(1);
  };

  const clearFilters = () => {
    setFilters({
      search: '',
      status: '',
      type: '',
      category: '',
      dateFrom: '',
      dateTo: '',
      sortBy: 'EnrollmentDate',
      sortDescending: true
    });
    setCurrentPage(1);
  };

  const getStatusBadge = (status: string) => {
    const config: Record<string, { color: string; icon: any }> = {
      'Enrolled': { color: 'info', icon: faUser },
      'InProgress': { color: 'warning', icon: faPlay },
      'Completed': { color: 'success', icon: faCheck },
      'Failed': { color: 'danger', icon: faTimes },
      'Cancelled': { color: 'secondary', icon: faTimes }
    };

    const { color, icon } = config[status] || { color: 'secondary', icon: faGraduationCap };

    return (
      <CBadge color={color} className="d-flex align-items-center">
        <FontAwesomeIcon icon={icon} className="me-1" size="sm" />
        {status}
      </CBadge>
    );
  };

  const getProgressColor = (percentage: number) => {
    if (percentage >= 80) return 'success';
    if (percentage >= 60) return 'info';
    if (percentage >= 40) return 'warning';
    return 'danger';
  };

  const handleDownloadCertificate = async (certificateId: number) => {
    try {
      await downloadCertificate(certificateId).unwrap();
    } catch (error) {
      console.error('Certificate download failed:', error);
    }
  };

  const isTrainingUpcoming = (startDate: string) => {
    return isAfter(new Date(startDate), new Date());
  };

  const isTrainingOverdue = (endDate: string, status: string) => {
    return isBefore(new Date(endDate), new Date()) && ['Enrolled', 'InProgress'].includes(status);
  };

  if (error) {
    return (
      <CAlert color="danger">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Failed to load your training data. Please try again.
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        {/* Header */}
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h2 className="mb-1">My Trainings</h2>
            <p className="text-muted mb-0">Track your training progress and certificates</p>
          </div>
        </div>

        {/* Stats Cards */}
        {trainingStats && (
          <CRow className="mb-4">
            <CCol md={3}>
              <CCard className="text-center">
                <CCardBody>
                  <FontAwesomeIcon icon={faGraduationCap} size="2x" className="text-primary mb-2" />
                  <h4 className="mb-1">{trainingStats.totalEnrolled}</h4>
                  <small className="text-muted">Total Enrollments</small>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center">
                <CCardBody>
                  <FontAwesomeIcon icon={faCheck} size="2x" className="text-success mb-2" />
                  <h4 className="mb-1">{trainingStats.totalCompleted}</h4>
                  <small className="text-muted">Completed</small>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center">
                <CCardBody>
                  <FontAwesomeIcon icon={faPlay} size="2x" className="text-warning mb-2" />
                  <h4 className="mb-1">{trainingStats.totalInProgress}</h4>
                  <small className="text-muted">In Progress</small>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center">
                <CCardBody>
                  <FontAwesomeIcon icon={faCertificate} size="2x" className="text-info mb-2" />
                  <h4 className="mb-1">{trainingStats.totalCertificates}</h4>
                  <small className="text-muted">Certificates</small>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>
        )}

        {/* Tabs */}
        <CTabs activeItemKey={activeTab} onActiveTabChange={setActiveTab}>
          <CTabList variant="tabs">
            <CTab itemKey="all-trainings">
              <FontAwesomeIcon icon={faGraduationCap} className="me-1" />
              All Trainings
            </CTab>
            <CTab itemKey="upcoming">
              <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
              Upcoming ({upcomingTrainings?.length || 0})
            </CTab>
            <CTab itemKey="certificates">
              <FontAwesomeIcon icon={faCertificate} className="me-1" />
              Certificates
            </CTab>
            <CTab itemKey="progress">
              <FontAwesomeIcon icon={faChartLine} className="me-1" />
              Progress
            </CTab>
          </CTabList>

          <CTabContent>
            {/* All Trainings Tab */}
            <CTabPanel className="py-3" itemKey="all-trainings">
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
                        {ENROLLMENT_STATUSES.map(status => (
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
                      <CFormInput
                        type="date"
                        placeholder="From date"
                        value={filters.dateFrom}
                        onChange={(e) => handleFilterChange('dateFrom', e.target.value)}
                      />
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
                </CCardBody>
              </CCard>

              {/* Training List */}
              {isLoading ? (
                <div className="text-center py-4">
                  <CSpinner />
                  <div className="mt-2">Loading your trainings...</div>
                </div>
              ) : (
                <>
                  {myTrainings && myTrainings.items.length > 0 ? (
                    <>
                      <CTable hover responsive>
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Training</CTableHeaderCell>
                            <CTableHeaderCell>Type</CTableHeaderCell>
                            <CTableHeaderCell>Status</CTableHeaderCell>
                            <CTableHeaderCell>Progress</CTableHeaderCell>
                            <CTableHeaderCell>Schedule</CTableHeaderCell>
                            <CTableHeaderCell>Score</CTableHeaderCell>
                            <CTableHeaderCell>Actions</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {myTrainings.items.map((training) => (
                            <CTableRow key={training.id}>
                              <CTableDataCell>
                                <div>
                                  <div className="fw-semibold">{training.trainingTitle}</div>
                                  <small className="text-muted">
                                    {training.trainingCode}
                                    {training.isK3MandatoryTraining && (
                                      <CBadge color="warning" className="ms-1">K3</CBadge>
                                    )}
                                    {training.requiresCertification && (
                                      <CBadge color="info" className="ms-1">
                                        <FontAwesomeIcon icon={faCertificate} className="me-1" />
                                        Cert
                                      </CBadge>
                                    )}
                                  </small>
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                <div>
                                  <div>{training.trainingType.replace(/([A-Z])/g, ' $1').trim()}</div>
                                  <small className="text-muted">{training.trainingCategory.replace(/([A-Z])/g, ' $1').trim()}</small>
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                <div className="d-flex flex-column gap-1">
                                  {getStatusBadge(training.enrollmentStatus)}
                                  {isTrainingOverdue(training.scheduledEndDate, training.enrollmentStatus) && (
                                    <CBadge color="danger">
                                      <FontAwesomeIcon icon={faClock} className="me-1" />
                                      Overdue
                                    </CBadge>
                                  )}
                                  {isTrainingUpcoming(training.scheduledStartDate) && (
                                    <CBadge color="info">
                                      <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                                      Upcoming
                                    </CBadge>
                                  )}
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                <div>
                                  <div className="d-flex justify-content-between mb-1">
                                    <small>{training.completionPercentage}%</small>
                                  </div>
                                  <CProgress height={8}>
                                    <CProgressBar 
                                      value={training.completionPercentage}
                                      color={getProgressColor(training.completionPercentage)}
                                    />
                                  </CProgress>
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                <div>
                                  <div className="fw-semibold">
                                    {format(new Date(training.scheduledStartDate), 'MMM dd, yyyy')}
                                  </div>
                                  <small className="text-muted">
                                    {format(new Date(training.scheduledStartDate), 'h:mm a')} - 
                                    {format(new Date(training.scheduledEndDate), 'h:mm a')}
                                  </small>
                                </div>
                              </CTableDataCell>
                              <CTableDataCell>
                                {training.finalScore !== null ? (
                                  <div>
                                    <span className={`fw-semibold ${training.finalScore >= (training.passingScore || 70) ? 'text-success' : 'text-danger'}`}>
                                      {training.finalScore}%
                                    </span>
                                    <br />
                                    <small className="text-muted">
                                      Pass: {training.passingScore || 70}%
                                    </small>
                                  </div>
                                ) : (
                                  <span className="text-muted">-</span>
                                )}
                              </CTableDataCell>
                              <CTableDataCell>
                                <CButtonGroup size="sm">
                                  <CButton
                                    color="primary"
                                    variant="outline"
                                    onClick={() => navigate(`/trainings/${training.trainingId}`)}
                                    title="View Training"
                                  >
                                    <FontAwesomeIcon icon={faEye} />
                                  </CButton>

                                  {training.enrollmentStatus === 'InProgress' && (
                                    <CButton
                                      color="success"
                                      variant="outline"
                                      onClick={() => navigate(`/trainings/${training.trainingId}/learn`)}
                                      title="Continue Learning"
                                    >
                                      <FontAwesomeIcon icon={faPlay} />
                                    </CButton>
                                  )}

                                  {training.certificateId && (
                                    <CButton
                                      color="info"
                                      variant="outline"
                                      onClick={() => handleDownloadCertificate(training.certificateId!)}
                                      disabled={isDownloading}
                                      title="Download Certificate"
                                    >
                                      <FontAwesomeIcon icon={faDownload} />
                                    </CButton>
                                  )}
                                </CButtonGroup>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>

                      {/* Pagination */}
                      {myTrainings && myTrainings.pageCount > 1 && (
                        <CPagination className="mt-3" align="center" size="sm">
                          <CPaginationItem
                            disabled={currentPage === 1}
                            onClick={() => setCurrentPage(1)}
                          >
                            First
                          </CPaginationItem>
                          <CPaginationItem
                            disabled={!myTrainings.hasPreviousPage}
                            onClick={() => setCurrentPage(currentPage - 1)}
                          >
                            Previous
                          </CPaginationItem>

                          {Array.from({ length: Math.min(5, myTrainings.pageCount) }, (_, i) => {
                            const startPage = Math.max(1, currentPage - 2);
                            const pageNumber = startPage + i;
                            if (pageNumber > myTrainings.pageCount) return null;

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
                            disabled={!myTrainings.hasNextPage}
                            onClick={() => setCurrentPage(currentPage + 1)}
                          >
                            Next
                          </CPaginationItem>
                          <CPaginationItem
                            disabled={currentPage === myTrainings.pageCount}
                            onClick={() => setCurrentPage(myTrainings.pageCount)}
                          >
                            Last
                          </CPaginationItem>
                        </CPagination>
                      )}
                    </>
                  ) : (
                    <div className="text-center py-4">
                      <FontAwesomeIcon icon={faGraduationCap} size="3x" className="text-muted mb-3" />
                      <h5 className="text-muted">No trainings found</h5>
                      <p className="text-muted">
                        {filters.search || filters.status || filters.type
                          ? 'Try adjusting your filters.'
                          : 'You haven\'t enrolled in any trainings yet.'}
                      </p>
                    </div>
                  )}
                </>
              )}
            </CTabPanel>

            {/* Upcoming Trainings Tab */}
            <CTabPanel className="py-3" itemKey="upcoming">
              {upcomingTrainings && upcomingTrainings.length > 0 ? (
                <CRow>
                  {upcomingTrainings.map((training) => (
                    <CCol md={6} lg={4} key={training.id} className="mb-3">
                      <CCard className="h-100">
                        <CCardHeader className="pb-2">
                          <div className="d-flex justify-content-between align-items-start">
                            <div>
                              <h6 className="mb-1">{training.trainingTitle}</h6>
                              <small className="text-muted">{training.trainingCode}</small>
                            </div>
                            {training.isK3MandatoryTraining && (
                              <CBadge color="warning">K3</CBadge>
                            )}
                          </div>
                        </CCardHeader>
                        <CCardBody>
                          <div className="mb-2">
                            <FontAwesomeIcon icon={faCalendarAlt} className="me-2 text-muted" />
                            <strong>{format(new Date(training.scheduledStartDate), 'MMM dd, yyyy')}</strong>
                          </div>
                          <div className="mb-2">
                            <FontAwesomeIcon icon={faClock} className="me-2 text-muted" />
                            {format(new Date(training.scheduledStartDate), 'h:mm a')} - 
                            {format(new Date(training.scheduledEndDate), 'h:mm a')}
                          </div>
                          <div className="mb-3">
                            <small className="text-muted">
                              Starts {formatDistanceToNow(new Date(training.scheduledStartDate))} from now
                            </small>
                          </div>
                          <div className="d-flex justify-content-between align-items-center">
                            {getStatusBadge(training.enrollmentStatus)}
                            <CButton
                              color="primary"
                              size="sm"
                              onClick={() => navigate(`/trainings/${training.trainingId}`)}
                            >
                              View Details
                            </CButton>
                          </div>
                        </CCardBody>
                      </CCard>
                    </CCol>
                  ))}
                </CRow>
              ) : (
                <div className="text-center py-4">
                  <FontAwesomeIcon icon={faCalendarAlt} size="3x" className="text-muted mb-3" />
                  <h5 className="text-muted">No upcoming trainings</h5>
                  <p className="text-muted">You don't have any trainings scheduled for the future.</p>
                </div>
              )}
            </CTabPanel>

            {/* Certificates Tab */}
            <CTabPanel className="py-3" itemKey="certificates">
              {myCertificates && myCertificates.length > 0 ? (
                <CRow>
                  {myCertificates.map((certificate) => (
                    <CCol md={6} lg={4} key={certificate.id} className="mb-3">
                      <CCard className="h-100">
                        <CCardHeader className="text-center bg-primary text-white">
                          <FontAwesomeIcon icon={faCertificate} size="2x" className="mb-2" />
                          <h6 className="mb-0">Certificate of Completion</h6>
                        </CCardHeader>
                        <CCardBody>
                          <div className="text-center mb-3">
                            <h6>{certificate.trainingTitle}</h6>
                            <small className="text-muted">{certificate.trainingCode}</small>
                          </div>
                          <div className="mb-2">
                            <strong>Issued:</strong> {format(new Date(certificate.issuedAt), 'MMM dd, yyyy')}
                          </div>
                          {certificate.expiresAt && (
                            <div className="mb-2">
                              <strong>Expires:</strong> {format(new Date(certificate.expiresAt), 'MMM dd, yyyy')}
                              {isBefore(new Date(certificate.expiresAt), new Date()) && (
                                <CBadge color="danger" className="ms-2">Expired</CBadge>
                              )}
                            </div>
                          )}
                          <div className="mb-3">
                            <strong>Score:</strong> {certificate.finalScore}%
                          </div>
                          <div className="text-center">
                            <CButton
                              color="primary"
                              size="sm"
                              onClick={() => handleDownloadCertificate(certificate.id)}
                              disabled={isDownloading}
                            >
                              <FontAwesomeIcon icon={faDownload} className="me-1" />
                              Download
                            </CButton>
                          </div>
                        </CCardBody>
                      </CCard>
                    </CCol>
                  ))}
                </CRow>
              ) : (
                <div className="text-center py-4">
                  <FontAwesomeIcon icon={faCertificate} size="3x" className="text-muted mb-3" />
                  <h5 className="text-muted">No certificates yet</h5>
                  <p className="text-muted">Complete trainings to earn certificates.</p>
                </div>
              )}
            </CTabPanel>

            {/* Progress Tab */}
            <CTabPanel className="py-3" itemKey="progress">
              {trainingStats ? (
                <CRow>
                  <CCol md={6}>
                    <CCard>
                      <CCardHeader>
                        <h6 className="mb-0">
                          <FontAwesomeIcon icon={faChartLine} className="me-2" />
                          Training Progress Overview
                        </h6>
                      </CCardHeader>
                      <CCardBody>
                        <div className="mb-3">
                          <div className="d-flex justify-content-between mb-1">
                            <span>Overall Completion Rate</span>
                            <span>{trainingStats.completionRate}%</span>
                          </div>
                          <CProgress height={12}>
                            <CProgressBar 
                              value={trainingStats.completionRate}
                              color={getProgressColor(trainingStats.completionRate)}
                            />
                          </CProgress>
                        </div>

                        <CTable size="sm">
                          <tbody>
                            <tr>
                              <td>Total Enrolled:</td>
                              <td className="text-end">{trainingStats.totalEnrolled}</td>
                            </tr>
                            <tr>
                              <td>Completed:</td>
                              <td className="text-end text-success">{trainingStats.totalCompleted}</td>
                            </tr>
                            <tr>
                              <td>In Progress:</td>
                              <td className="text-end text-warning">{trainingStats.totalInProgress}</td>
                            </tr>
                            <tr>
                              <td>Failed:</td>
                              <td className="text-end text-danger">{trainingStats.totalFailed}</td>
                            </tr>
                            <tr>
                              <td>Certificates Earned:</td>
                              <td className="text-end text-info">{trainingStats.totalCertificates}</td>
                            </tr>
                          </tbody>
                        </CTable>
                      </CCardBody>
                    </CCard>
                  </CCol>
                  
                  <CCol md={6}>
                    <CCard>
                      <CCardHeader>
                        <h6 className="mb-0">
                          <FontAwesomeIcon icon={faAward} className="me-2" />
                          Learning Achievements
                        </h6>
                      </CCardHeader>
                      <CCardBody>
                        {trainingStats.averageScore !== null ? (
                          <>
                            <div className="text-center mb-3">
                              <h3 className="text-primary">{trainingStats.averageScore}%</h3>
                              <small className="text-muted">Average Score</small>
                            </div>
                            <CTable size="sm">
                              <tbody>
                                <tr>
                                  <td>Highest Score:</td>
                                  <td className="text-end">{trainingStats.highestScore}%</td>
                                </tr>
                                <tr>
                                  <td>K3 Trainings Completed:</td>
                                  <td className="text-end">{trainingStats.k3TrainingsCompleted}</td>
                                </tr>
                                <tr>
                                  <td>Total Training Hours:</td>
                                  <td className="text-end">{trainingStats.totalTrainingHours}h</td>
                                </tr>
                                <tr>
                                  <td>Compliance Rate:</td>
                                  <td className="text-end">{trainingStats.complianceRate}%</td>
                                </tr>
                              </tbody>
                            </CTable>
                          </>
                        ) : (
                          <div className="text-center text-muted py-3">
                            <FontAwesomeIcon icon={faBookOpen} size="2x" className="mb-2" />
                            <p>Complete some trainings to see your achievements!</p>
                          </div>
                        )}
                      </CCardBody>
                    </CCard>
                  </CCol>
                </CRow>
              ) : (
                <div className="text-center py-4">
                  <FontAwesomeIcon icon={faChartLine} size="3x" className="text-muted mb-3" />
                  <h5 className="text-muted">No progress data available</h5>
                  <p className="text-muted">Start taking trainings to track your progress.</p>
                </div>
              )}
            </CTabPanel>
          </CTabContent>
        </CTabs>
      </CCol>
    </CRow>
  );
};

export default MyTrainings;