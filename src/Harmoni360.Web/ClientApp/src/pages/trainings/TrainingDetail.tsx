import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CBadge,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
  CSpinner,
  CAlert,
  CTable,
  CTableHead,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CTableRow,
  CListGroup,
  CListGroupItem,
  CButtonGroup,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormTextarea,
  CFormLabel
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faGraduationCap,
  faEdit,
  faTrash,
  faPlay,
  faCheck,
  faTimes,
  faArrowLeft,
  faCalendarAlt,
  faClock,
  faUsers,
  faUserPlus,
  faDownload,
  faFileAlt,
  faCertificate,
  faExclamationTriangle,
  faChartLine,
  faComments,
  faHistory,
  faShare,
  faPrint
} from '@fortawesome/free-solid-svg-icons';
import { format, formatDistanceToNow } from 'date-fns';

import {
  useGetTrainingByIdQuery,
  useDeleteTrainingMutation,
  useStartTrainingMutation,
  useCompleteTrainingMutation,
  useCancelTrainingMutation,
  useEnrollParticipantMutation,
  useGetTrainingParticipantsQuery,
  useGetTrainingProgressQuery,
  useGetTrainingCommentsQuery,
  useAddTrainingCommentMutation
} from '../../features/trainings/trainingApi';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { useApplicationMode } from '../../hooks/useApplicationMode';
import { TrainingAttachmentManager } from '../../components/trainings';
import { TrainingDto } from '../../types/training';

const TrainingDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isDemo } = useApplicationMode();

  const [activeTab, setActiveTab] = useState('overview');
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [showCommentModal, setShowCommentModal] = useState(false);
  const [newComment, setNewComment] = useState('');

  // API queries
  const { data: training, isLoading, error, refetch } = useGetTrainingByIdQuery(id!);
  const { data: participants } = useGetTrainingParticipantsQuery(id!);
  const { data: progress } = useGetTrainingProgressQuery(id!);
  const { data: comments } = useGetTrainingCommentsQuery(id!);

  // Mutations
  const [deleteTraining, { isLoading: isDeleting }] = useDeleteTrainingMutation();
  const [startTraining, { isLoading: isStarting }] = useStartTrainingMutation();
  const [completeTraining, { isLoading: isCompleting }] = useCompleteTrainingMutation();
  const [cancelTraining, { isLoading: isCancelling }] = useCancelTrainingMutation();
  const [enrollParticipant] = useEnrollParticipantMutation();
  const [addComment, { isLoading: isAddingComment }] = useAddTrainingCommentMutation();

  const handleDeleteConfirm = async () => {
    if (!training) return;

    try {
      await deleteTraining(training.id).unwrap();
      navigate('/trainings');
    } catch (error) {
      console.error('Delete failed:', error);
    }
  };

  const handleStartTraining = async () => {
    if (!training) return;

    try {
      await startTraining(training.id).unwrap();
      refetch();
    } catch (error) {
      console.error('Start training failed:', error);
    }
  };

  const handleCompleteTraining = async () => {
    if (!training) return;

    try {
      await completeTraining({ id: training.id }).unwrap();
      refetch();
    } catch (error) {
      console.error('Complete training failed:', error);
    }
  };

  const handleCancelTraining = async () => {
    if (!training || !cancelReason.trim()) return;

    try {
      await cancelTraining({ id: training.id, reason: cancelReason }).unwrap();
      setShowCancelModal(false);
      setCancelReason('');
      refetch();
    } catch (error) {
      console.error('Cancel training failed:', error);
    }
  };

  const handleAddComment = async () => {
    if (!training || !newComment.trim()) return;

    try {
      await addComment({
        trainingId: training.id,
        comment: newComment
      }).unwrap();
      setNewComment('');
      setShowCommentModal(false);
    } catch (error) {
      console.error('Add comment failed:', error);
    }
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

  const canEdit = training?.canEdit && training.status === 'Draft';
  const canStart = training?.canStart && training.status === 'Scheduled';
  const canComplete = training?.canComplete && training.status === 'InProgress';
  const canCancel = training?.status && ['Draft', 'Scheduled', 'InProgress'].includes(training.status);
  const canEnroll = training?.canEnroll && ['Scheduled', 'InProgress'].includes(training.status);

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner color="primary" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
        Failed to load training details. Please try again.
      </CAlert>
    );
  }

  if (!training) {
    return (
      <CAlert color="warning">
        Training not found.
      </CAlert>
    );
  }

  return (
    <>
      <CRow>
        <CCol xs={12}>
          {/* Header */}
          <div className="d-flex justify-content-between align-items-center mb-4">
            <div>
              <CButton
                color="link"
                className="p-0 me-3"
                onClick={() => navigate('/trainings')}
              >
                <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
                Back to Trainings
              </CButton>
              <h2 className="mb-1">{training.title}</h2>
              <div className="d-flex align-items-center gap-2">
                <small className="text-muted">{training.trainingCode}</small>
                {training.isK3MandatoryTraining && (
                  <CBadge color="warning">K3 Mandatory</CBadge>
                )}
                {training.requiresCertification && (
                  <CBadge color="info">
                    <FontAwesomeIcon icon={faCertificate} className="me-1" />
                    Certification
                  </CBadge>
                )}
              </div>
            </div>
            <div className="d-flex gap-2">
              <CButtonGroup>
                <CButton color="secondary" variant="outline" onClick={() => window.print()}>
                  <FontAwesomeIcon icon={faPrint} />
                </CButton>
                <CButton color="secondary" variant="outline">
                  <FontAwesomeIcon icon={faShare} />
                </CButton>
              </CButtonGroup>

              <PermissionGuard
                moduleType={ModuleType.TrainingManagement}
                permissionType={PermissionType.Update}
              >
                {canEdit && (
                  <CButton
                    color="primary"
                    onClick={() => navigate(`/trainings/${training.id}/edit`)}
                  >
                    <FontAwesomeIcon icon={faEdit} className="me-1" />
                    Edit
                  </CButton>
                )}

                {canStart && (
                  <CButton
                    color="success"
                    onClick={handleStartTraining}
                    disabled={isStarting}
                  >
                    <FontAwesomeIcon icon={faPlay} className="me-1" />
                    Start Training
                  </CButton>
                )}

                {canComplete && (
                  <CButton
                    color="info"
                    onClick={handleCompleteTraining}
                    disabled={isCompleting}
                  >
                    <FontAwesomeIcon icon={faCheck} className="me-1" />
                    Complete
                  </CButton>
                )}

                {canEnroll && (
                  <CButton
                    color="warning"
                    onClick={() => navigate(`/trainings/${training.id}/enroll`)}
                  >
                    <FontAwesomeIcon icon={faUserPlus} className="me-1" />
                    Enroll
                  </CButton>
                )}

                {canCancel && (
                  <CButton
                    color="secondary"
                    variant="outline"
                    onClick={() => setShowCancelModal(true)}
                  >
                    <FontAwesomeIcon icon={faTimes} className="me-1" />
                    Cancel
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
                    onClick={() => setShowDeleteModal(true)}
                  >
                    <FontAwesomeIcon icon={faTrash} />
                  </CButton>
                )}
              </PermissionGuard>
            </div>
          </div>

          {/* Status and Key Info Cards */}
          <CRow className="mb-4">
            <CCol md={3}>
              <CCard className="h-100">
                <CCardBody className="text-center">
                  <div className="mb-2">{getStatusBadge(training.status)}</div>
                  <small className="text-muted">Status</small>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="h-100">
                <CCardBody className="text-center">
                  <div className="mb-2">{getPriorityBadge(training.priority)}</div>
                  <small className="text-muted">Priority</small>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="h-100">
                <CCardBody className="text-center">
                  <div className="mb-2">
                    <FontAwesomeIcon icon={faUsers} className="me-1" />
                    {training.currentParticipants}/{training.maxParticipants}
                  </div>
                  <small className="text-muted">Participants</small>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="h-100">
                <CCardBody className="text-center">
                  <div className="mb-2">
                    <FontAwesomeIcon icon={faClock} className="me-1" />
                    {training.durationHours}h
                  </div>
                  <small className="text-muted">Duration</small>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>

          {/* Tabs */}
          <CCard>
            <CCardHeader className="p-0">
              <CNav variant="tabs" role="tablist">
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'overview'}
                    onClick={() => setActiveTab('overview')}
                    role="tab"
                  >
                    <FontAwesomeIcon icon={faGraduationCap} className="me-1" />
                    Overview
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'participants'}
                    onClick={() => setActiveTab('participants')}
                    role="tab"
                  >
                    <FontAwesomeIcon icon={faUsers} className="me-1" />
                    Participants ({training.currentParticipants})
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'progress'}
                    onClick={() => setActiveTab('progress')}
                    role="tab"
                  >
                    <FontAwesomeIcon icon={faChartLine} className="me-1" />
                    Progress
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'materials'}
                    onClick={() => setActiveTab('materials')}
                    role="tab"
                  >
                    <FontAwesomeIcon icon={faFileAlt} className="me-1" />
                    Materials
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'comments'}
                    onClick={() => setActiveTab('comments')}
                    role="tab"
                  >
                    <FontAwesomeIcon icon={faComments} className="me-1" />
                    Comments ({comments?.length || 0})
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'history'}
                    onClick={() => setActiveTab('history')}
                    role="tab"
                  >
                    <FontAwesomeIcon icon={faHistory} className="me-1" />
                    History
                  </CNavLink>
                </CNavItem>
              </CNav>
            </CCardHeader>

            <CCardBody>
              <CTabContent>
                {/* Overview Tab */}
                <CTabPane visible={activeTab === 'overview'}>
                  <CRow>
                    <CCol md={8}>
                      <h5>Training Details</h5>
                      <CTable responsive>
                        <tbody>
                          <tr>
                            <td className="fw-semibold" width="200">Training Code:</td>
                            <td>{training.trainingCode}</td>
                          </tr>
                          <tr>
                            <td className="fw-semibold">Type:</td>
                            <td>{training.type.replace(/([A-Z])/g, ' $1').trim()}</td>
                          </tr>
                          <tr>
                            <td className="fw-semibold">Category:</td>
                            <td>{training.category.replace(/([A-Z])/g, ' $1').trim()}</td>
                          </tr>
                          <tr>
                            <td className="fw-semibold">Delivery Method:</td>
                            <td>{training.deliveryMethod.replace(/([A-Z])/g, ' $1').trim()}</td>
                          </tr>
                          <tr>
                            <td className="fw-semibold">Instructor:</td>
                            <td>
                              {training.instructorName || 'TBD'}
                              {training.isExternalInstructor && (
                                <CBadge color="secondary" className="ms-2">External</CBadge>
                              )}
                            </td>
                          </tr>
                          <tr>
                            <td className="fw-semibold">Duration:</td>
                            <td>{training.durationHours} hours</td>
                          </tr>
                          <tr>
                            <td className="fw-semibold">Capacity:</td>
                            <td>{training.currentParticipants}/{training.maxParticipants} participants</td>
                          </tr>
                          <tr>
                            <td className="fw-semibold">Location:</td>
                            <td>{training.location || 'TBD'}</td>
                          </tr>
                          <tr>
                            <td className="fw-semibold">Schedule:</td>
                            <td>
                              {format(new Date(training.scheduledStartDate), 'EEEE, MMMM dd, yyyy')}
                              <br />
                              <small className="text-muted">
                                Start: {format(new Date(training.scheduledStartDate), 'h:mm a')} - 
                                End: {training.scheduledEndDate ? format(new Date(training.scheduledEndDate), 'h:mm a') : 'TBD'}
                              </small>
                            </td>
                          </tr>
                        </tbody>
                      </CTable>

                      <div className="mt-4">
                        <h6>Description</h6>
                        <p>{training.description || 'No description provided.'}</p>
                      </div>

                      {training.prerequisites && (
                        <div className="mt-3">
                          <h6>Prerequisites</h6>
                          <p>{training.prerequisites}</p>
                        </div>
                      )}

                      {training.learningObjectives && (
                        <div className="mt-3">
                          <h6>Learning Objectives</h6>
                          <p>{training.learningObjectives}</p>
                        </div>
                      )}
                    </CCol>

                    <CCol md={4}>
                      <h5>Training Requirements</h5>
                      <CListGroup flush>
                        <CListGroupItem className="d-flex justify-content-between align-items-center">
                          K3 Mandatory Training
                          <CBadge color={training.isK3MandatoryTraining ? 'success' : 'secondary'}>
                            {training.isK3MandatoryTraining ? 'Yes' : 'No'}
                          </CBadge>
                        </CListGroupItem>
                        <CListGroupItem className="d-flex justify-content-between align-items-center">
                          Certification Required
                          <CBadge color={training.requiresCertification ? 'success' : 'secondary'}>
                            {training.requiresCertification ? 'Yes' : 'No'}
                          </CBadge>
                        </CListGroupItem>
                        <CListGroupItem className="d-flex justify-content-between align-items-center">
                          Refresher Training
                          <CBadge color={training.isRefresherTraining ? 'warning' : 'secondary'}>
                            {training.isRefresherTraining ? 'Yes' : 'No'}
                          </CBadge>
                        </CListGroupItem>
                        {training.refresherDurationMonths && (
                          <CListGroupItem className="d-flex justify-content-between align-items-center">
                            Refresher Period
                            <CBadge color="info">
                              {training.refresherDurationMonths} months
                            </CBadge>
                          </CListGroupItem>
                        )}
                      </CListGroup>

                      {training.complianceFramework && (
                        <div className="mt-4">
                          <h6>Compliance Framework</h6>
                          <CBadge color="primary">{training.complianceFramework}</CBadge>
                        </div>
                      )}
                    </CCol>
                  </CRow>
                </CTabPane>

                {/* Participants Tab */}
                <CTabPane visible={activeTab === 'participants'}>
                  <div className="d-flex justify-content-between align-items-center mb-3">
                    <h5>Training Participants</h5>
                    {canEnroll && (
                      <CButton
                        color="primary"
                        size="sm"
                        onClick={() => navigate(`/trainings/${training.id}/enroll`)}
                      >
                        <FontAwesomeIcon icon={faUserPlus} className="me-1" />
                        Enroll Participants
                      </CButton>
                    )}
                  </div>

                  {participants && participants.length > 0 ? (
                    <CTable responsive hover>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Participant</CTableHeaderCell>
                          <CTableHeaderCell>Department</CTableHeaderCell>
                          <CTableHeaderCell>Enrollment Date</CTableHeaderCell>
                          <CTableHeaderCell>Status</CTableHeaderCell>
                          <CTableHeaderCell>Completion</CTableHeaderCell>
                          <CTableHeaderCell>Actions</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {participants.map((participant) => (
                          <CTableRow key={participant.id}>
                            <CTableDataCell>
                              <div>
                                <div className="fw-semibold">{participant.participantName}</div>
                                <small className="text-muted">{participant.participantEmail}</small>
                              </div>
                            </CTableDataCell>
                            <CTableDataCell>{participant.department}</CTableDataCell>
                            <CTableDataCell>
                              {format(new Date(participant.enrolledAt), 'MMM dd, yyyy')}
                            </CTableDataCell>
                            <CTableDataCell>
                              <CBadge color={
                                participant.status === 'Completed' ? 'success' :
                                participant.status === 'InProgress' ? 'warning' :
                                participant.status === 'Failed' ? 'danger' : 'info'
                              }>
                                {participant.status}
                              </CBadge>
                            </CTableDataCell>
                            <CTableDataCell>
                              {participant.completionPercentage}%
                            </CTableDataCell>
                            <CTableDataCell>
                              <CButtonGroup size="sm">
                                <CButton color="primary" variant="outline">
                                  View
                                </CButton>
                                {participant.certificateId && (
                                  <CButton color="success" variant="outline">
                                    <FontAwesomeIcon icon={faDownload} />
                                  </CButton>
                                )}
                              </CButtonGroup>
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                  ) : (
                    <div className="text-center text-muted py-4">
                      <FontAwesomeIcon icon={faUsers} size="2x" className="mb-2" />
                      <p>No participants enrolled yet.</p>
                    </div>
                  )}
                </CTabPane>

                {/* Progress Tab */}
                <CTabPane visible={activeTab === 'progress'}>
                  <h5>Training Progress Overview</h5>
                  {progress ? (
                    <CRow>
                      <CCol md={6}>
                        <CCard>
                          <CCardHeader>Completion Statistics</CCardHeader>
                          <CCardBody>
                            <div className="mb-3">
                              <div className="d-flex justify-content-between">
                                <span>Overall Progress</span>
                                <span>{progress.completionPercentage}%</span>
                              </div>
                              <div className="progress mb-2">
                                <div 
                                  className="progress-bar" 
                                  style={{ width: `${progress.completionPercentage}%` }}
                                ></div>
                              </div>
                            </div>
                            <CTable size="sm">
                              <tbody>
                                <tr>
                                  <td>Enrolled:</td>
                                  <td>{progress.totalEnrolled}</td>
                                </tr>
                                <tr>
                                  <td>Completed:</td>
                                  <td>{progress.totalCompleted}</td>
                                </tr>
                                <tr>
                                  <td>In Progress:</td>
                                  <td>{progress.totalInProgress}</td>
                                </tr>
                                <tr>
                                  <td>Failed:</td>
                                  <td>{progress.totalFailed}</td>
                                </tr>
                              </tbody>
                            </CTable>
                          </CCardBody>
                        </CCard>
                      </CCol>
                      <CCol md={6}>
                        <CCard>
                          <CCardHeader>Assessment Results</CCardHeader>
                          <CCardBody>
                            {progress.averageScore !== null ? (
                              <>
                                <div className="text-center mb-3">
                                  <h3 className="text-primary">{progress.averageScore}%</h3>
                                  <small className="text-muted">Average Score</small>
                                </div>
                                <CTable size="sm">
                                  <tbody>
                                    <tr>
                                      <td>Pass Rate:</td>
                                      <td>{progress.passRate}%</td>
                                    </tr>
                                    <tr>
                                      <td>Highest Score:</td>
                                      <td>{progress.highestScore}%</td>
                                    </tr>
                                    <tr>
                                      <td>Lowest Score:</td>
                                      <td>{progress.lowestScore}%</td>
                                    </tr>
                                  </tbody>
                                </CTable>
                              </>
                            ) : (
                              <div className="text-center text-muted py-3">
                                <p>No assessment results available yet.</p>
                              </div>
                            )}
                          </CCardBody>
                        </CCard>
                      </CCol>
                    </CRow>
                  ) : (
                    <div className="text-center text-muted py-4">
                      <FontAwesomeIcon icon={faChartLine} size="2x" className="mb-2" />
                      <p>No progress data available yet.</p>
                    </div>
                  )}
                </CTabPane>

                {/* Materials Tab */}
                <CTabPane visible={activeTab === 'materials'}>
                  <TrainingAttachmentManager
                    trainingId={training.id.toString()}
                    attachments={training.attachments}
                    allowUpload={canEdit}
                    allowDelete={canEdit}
                    readonly={!canEdit}
                  />
                </CTabPane>

                {/* Comments Tab */}
                <CTabPane visible={activeTab === 'comments'}>
                  <div className="d-flex justify-content-between align-items-center mb-3">
                    <h5>Training Comments</h5>
                    <CButton
                      color="primary"
                      size="sm"
                      onClick={() => setShowCommentModal(true)}
                    >
                      <FontAwesomeIcon icon={faComments} className="me-1" />
                      Add Comment
                    </CButton>
                  </div>

                  {comments && comments.length > 0 ? (
                    <div className="timeline">
                      {comments.map((comment) => (
                        <div key={comment.id} className="timeline-item mb-3">
                          <CCard>
                            <CCardBody>
                              <div className="d-flex justify-content-between align-items-start mb-2">
                                <div>
                                  <strong>{comment.authorName}</strong>
                                  <small className="text-muted ms-2">
                                    {formatDistanceToNow(new Date(comment.createdAt))} ago
                                  </small>
                                </div>
                                <CBadge color="primary">{comment.commentType}</CBadge>
                              </div>
                              <p className="mb-0">{comment.comment}</p>
                            </CCardBody>
                          </CCard>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="text-center text-muted py-4">
                      <FontAwesomeIcon icon={faComments} size="2x" className="mb-2" />
                      <p>No comments yet.</p>
                    </div>
                  )}
                </CTabPane>

                {/* History Tab */}
                <CTabPane visible={activeTab === 'history'}>
                  <h5>Training History</h5>
                  <div className="timeline">
                    <div className="timeline-item mb-3">
                      <CCard>
                        <CCardBody>
                          <div className="d-flex justify-content-between align-items-center">
                            <div>
                              <strong>Training Created</strong>
                              <div className="text-muted small">
                                Created by {training.createdBy} • {formatDistanceToNow(new Date(training.createdAt))} ago
                              </div>
                            </div>
                            <CBadge color="info">Created</CBadge>
                          </div>
                        </CCardBody>
                      </CCard>
                    </div>
                    
                    {training.lastModifiedAt && (
                      <div className="timeline-item mb-3">
                        <CCard>
                          <CCardBody>
                            <div className="d-flex justify-content-between align-items-center">
                              <div>
                                <strong>Training Modified</strong>
                                <div className="text-muted small">
                                  Modified by {training.lastModifiedBy} • {formatDistanceToNow(new Date(training.lastModifiedAt))} ago
                                </div>
                              </div>
                              <CBadge color="warning">Modified</CBadge>
                            </div>
                          </CCardBody>
                        </CCard>
                      </div>
                    )}
                  </div>
                </CTabPane>
              </CTabContent>
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
          Are you sure you want to delete this training? This action cannot be undone.
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

      {/* Cancel Training Modal */}
      <CModal visible={showCancelModal} onClose={() => setShowCancelModal(false)}>
        <CModalHeader>
          <CModalTitle>Cancel Training</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel>Cancellation Reason *</CFormLabel>
              <CFormTextarea
                rows={3}
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
                placeholder="Please provide a reason for cancelling this training..."
                required
              />
            </div>
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowCancelModal(false)}>
            Cancel
          </CButton>
          <CButton 
            color="warning" 
            onClick={handleCancelTraining} 
            disabled={isCancelling || !cancelReason.trim()}
          >
            {isCancelling ? (
              <>
                <CSpinner size="sm" className="me-1" />
                Cancelling...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faTimes} className="me-1" />
                Cancel Training
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>

      {/* Add Comment Modal */}
      <CModal visible={showCommentModal} onClose={() => setShowCommentModal(false)}>
        <CModalHeader>
          <CModalTitle>Add Comment</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel>Comment *</CFormLabel>
              <CFormTextarea
                rows={4}
                value={newComment}
                onChange={(e) => setNewComment(e.target.value)}
                placeholder="Enter your comment about this training..."
                required
              />
            </div>
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowCommentModal(false)}>
            Cancel
          </CButton>
          <CButton 
            color="primary" 
            onClick={handleAddComment} 
            disabled={isAddingComment || !newComment.trim()}
          >
            {isAddingComment ? (
              <>
                <CSpinner size="sm" className="me-1" />
                Adding...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faComments} className="me-1" />
                Add Comment
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default TrainingDetail;