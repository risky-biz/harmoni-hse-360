import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CSpinner,
  CAlert,
  CBadge,
  CListGroup,
  CListGroupItem,
  CCallout,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CProgress,
  CButtonGroup,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faClipboardCheck,
  faArrowLeft,
  faEdit,
  faTrash,
  faExclamationTriangle,
  faUser,
  faCalendarAlt,
  faShieldAlt,
  faCheckCircle,
  faTimesCircle,
  faClock,
  faFileAlt,
  faPlus,
  faEye,
  faRefresh,
  faDownload,
  faHistory,
  faExternalLinkAlt,
  faExclamationCircle,
} from '@fortawesome/free-solid-svg-icons';
import ApiUnavailableMessage from '../../components/common/ApiUnavailableMessage';
import {
  useGetRiskAssessmentQuery,
} from '../../features/risk-assessments/riskAssessmentApi';
import {
  getRiskLevelBadge,
  getAssessmentTypeBadge,
  getApprovalStatusBadge,
  getReviewStatusBadge,
  getRiskScoreBadge,
  formatDate,
  formatDateShort,
  isAssessmentDueForReview,
  getDaysUntilReview,
  getRiskLevelDescription,
  getAssessmentTypeDescription,
} from '../../utils/riskAssessmentUtils';

const RiskAssessmentDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showReassessModal, setShowReassessModal] = useState(false);
  const [showApprovalModal, setShowApprovalModal] = useState(false);
  const [showHazardDetailsModal, setShowHazardDetailsModal] = useState(false);

  const {
    data: assessment,
    error,
    isLoading,
    refetch,
  } = useGetRiskAssessmentQuery(Number(id));

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading risk assessment details...</span>
      </div>
    );
  }

  if (error) {
    return (
      <ApiUnavailableMessage
        title="Failed to load risk assessment"
        message="Unable to retrieve risk assessment details from the backend API."
        onRefresh={() => refetch()}
      />
    );
  }

  if (!assessment) {
    return (
      <CAlert color="warning">
        Risk assessment not found.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/risk-assessments')}>
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            Back to Risk Assessments
          </CButton>
        </div>
      </CAlert>
    );
  }

  const daysUntilReview = getDaysUntilReview(assessment.nextReviewDate);
  const isDueForReview = isAssessmentDueForReview(assessment.nextReviewDate);

  return (
    <>
      <CRow>
        <CCol xs={12}>
          <CCard className="shadow-sm">
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div>
                <h4
                  className="mb-0"
                  style={{
                    color: 'var(--harmoni-charcoal)',
                    fontFamily: 'Poppins, sans-serif',
                  }}
                >
                  <FontAwesomeIcon
                    icon={faClipboardCheck}
                    size="lg"
                    className="me-2 text-primary"
                  />
                  Risk Assessment Details
                </h4>
                <small className="text-muted">ID: {assessment.id}</small>
              </div>
              <div className="d-flex gap-2">
                <CButton
                  color="light"
                  onClick={() => navigate('/risk-assessments')}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                  Back
                </CButton>
                <CButton
                  color="secondary"
                  variant="outline"
                  onClick={() => navigate(`/hazards/${assessment.hazardId}`)}
                >
                  <FontAwesomeIcon icon={faExternalLinkAlt} className="me-2" />
                  View Hazard
                </CButton>
                {isDueForReview && (
                  <CButton
                    color="warning"
                    onClick={() => navigate(`/risk-assessments/${assessment.id}/reassess`)}
                  >
                    <FontAwesomeIcon icon={faRefresh} className="me-2" />
                    Reassess
                  </CButton>
                )}
                <CButton
                  color="primary"
                  onClick={() => navigate(`/risk-assessments/${assessment.id}/edit`)}
                >
                  <FontAwesomeIcon icon={faEdit} className="me-2" />
                  Edit
                </CButton>
              </div>
            </CCardHeader>

            <CCardBody>
              {/* Review Status Alert */}
              {isDueForReview && (
                <CAlert color="warning" className="mb-4">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                  <strong>Review Required:</strong> This assessment is{' '}
                  {daysUntilReview < 0 ? `${Math.abs(daysUntilReview)} days overdue` : 'due'} for review.
                  <CButton
                    color="warning"
                    size="sm"
                    className="ms-3"
                    onClick={() => navigate(`/risk-assessments/${assessment.id}/reassess`)}
                  >
                    <FontAwesomeIcon icon={faRefresh} className="me-1" />
                    Start Reassessment
                  </CButton>
                </CAlert>
              )}

              <CRow>
                {/* Main Content */}
                <CCol md={8}>
                  {/* Hazard Information */}
                  <div className="mb-4">
                    <h5 className="mb-3">
                      <FontAwesomeIcon icon={faExclamationTriangle} className="me-2 text-warning" />
                      Related Hazard
                    </h5>
                    <CCard className="bg-light border-0">
                      <CCardBody className="p-3">
                        <div className="d-flex justify-content-between align-items-start">
                          <div>
                            <h6 className="mb-2 text-primary">{assessment.hazardTitle}</h6>
                            {assessment.hazard && (
                              <div className="text-muted small">
                                <div className="mb-1">
                                  <strong>Category:</strong> {assessment.hazard.category}
                                </div>
                                <div className="mb-1">
                                  <strong>Type:</strong> {assessment.hazard.type}
                                </div>
                                <div className="mb-1">
                                  <strong>Location:</strong> {assessment.hazard.location}
                                </div>
                                <div>
                                  <strong>Reported:</strong> {formatDateShort(assessment.hazard.identifiedDate)}
                                </div>
                              </div>
                            )}
                          </div>
                          <CButton
                            color="primary"
                            variant="outline"
                            size="sm"
                            onClick={() => navigate(`/hazards/${assessment.hazardId}`)}
                          >
                            <FontAwesomeIcon icon={faEye} className="me-1" />
                            View Details
                          </CButton>
                        </div>
                      </CCardBody>
                    </CCard>
                  </div>

                  {/* Assessment Overview */}
                  <div className="mb-4">
                    <h5 className="mb-3">Assessment Overview</h5>
                    <CRow className="mb-3">
                      <CCol sm={3}>
                        <h6 className="text-muted">Assessment Type</h6>
                        <div className="mb-2">{getAssessmentTypeBadge(assessment.type)}</div>
                        <small className="text-muted">{getAssessmentTypeDescription(assessment.type)}</small>
                      </CCol>
                      <CCol sm={3}>
                        <h6 className="text-muted">Risk Level</h6>
                        <div className="mb-2">{getRiskLevelBadge(assessment.riskLevel)}</div>
                        <small className="text-muted">{getRiskLevelDescription(assessment.riskLevel)}</small>
                      </CCol>
                      <CCol sm={3}>
                        <h6 className="text-muted">Risk Score</h6>
                        <div className="mb-2">{getRiskScoreBadge(assessment.riskScore)}</div>
                        <small className="text-muted">
                          {assessment.probabilityScore} × {assessment.severityScore}
                        </small>
                      </CCol>
                      <CCol sm={3}>
                        <h6 className="text-muted">Status</h6>
                        <div className="mb-2">{getApprovalStatusBadge(assessment.isApproved)}</div>
                        <small className="text-muted">
                          {assessment.isApproved ? 'Approved' : 'Pending approval'}
                        </small>
                      </CCol>
                    </CRow>
                  </div>

                  {/* Risk Matrix Visualization */}
                  <div className="mb-4">
                    <h6 className="text-muted mb-3">Risk Matrix (5×5)</h6>
                    <CTable bordered className="risk-matrix-table" size="sm">
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell className="text-center bg-light" style={{ width: '120px' }}>
                            Probability →<br />Severity ↓
                          </CTableHeaderCell>
                          <CTableHeaderCell className="text-center bg-light">Very Low (1)</CTableHeaderCell>
                          <CTableHeaderCell className="text-center bg-light">Low (2)</CTableHeaderCell>
                          <CTableHeaderCell className="text-center bg-light">Medium (3)</CTableHeaderCell>
                          <CTableHeaderCell className="text-center bg-light">High (4)</CTableHeaderCell>
                          <CTableHeaderCell className="text-center bg-light">Very High (5)</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {[5, 4, 3, 2, 1].map((severity) => (
                          <CTableRow key={severity}>
                            <CTableHeaderCell className="text-center bg-light">
                              {severity === 5 ? 'Very High (5)' :
                               severity === 4 ? 'High (4)' :
                               severity === 3 ? 'Medium (3)' :
                               severity === 2 ? 'Low (2)' : 'Very Low (1)'}
                            </CTableHeaderCell>
                            {[1, 2, 3, 4, 5].map((probability) => {
                              const cellScore = probability * severity;
                              const isCurrentAssessment = 
                                assessment.probabilityScore === probability && 
                                assessment.severityScore === severity;
                              
                              let cellColor = '';
                              if (cellScore >= 17) cellColor = 'table-danger';
                              else if (cellScore >= 10) cellColor = 'table-warning';
                              else if (cellScore >= 5) cellColor = 'table-info';
                              else cellColor = 'table-success';

                              return (
                                <CTableDataCell 
                                  key={probability}
                                  className={`text-center ${cellColor} ${isCurrentAssessment ? 'fw-bold border-dark' : ''}`}
                                  style={{
                                    position: 'relative',
                                    ...(isCurrentAssessment && {
                                      borderWidth: '3px',
                                      boxShadow: '0 0 10px rgba(0,123,255,0.5)'
                                    })
                                  }}
                                >
                                  {cellScore}
                                  {isCurrentAssessment && (
                                    <div className="position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center">
                                      <FontAwesomeIcon 
                                        icon={faExclamationCircle} 
                                        className="text-primary" 
                                        style={{ fontSize: '1.2rem' }}
                                      />
                                    </div>
                                  )}
                                </CTableDataCell>
                              );
                            })}
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                    <small className="text-muted">
                      Current assessment: Probability {assessment.probabilityScore} × Severity {assessment.severityScore} = Risk Score {assessment.riskScore}
                    </small>
                  </div>

                  {/* Assessment Details */}
                  <div className="mb-4">
                    <h5 className="mb-3">Assessment Details</h5>
                    
                    {assessment.potentialConsequences && (
                      <div className="mb-3">
                        <h6 className="text-muted">Potential Consequences</h6>
                        <p>{assessment.potentialConsequences}</p>
                      </div>
                    )}

                    {assessment.existingControls && (
                      <div className="mb-3">
                        <h6 className="text-muted">Existing Controls</h6>
                        <p>{assessment.existingControls}</p>
                      </div>
                    )}

                    {assessment.recommendedActions && (
                      <div className="mb-3">
                        <h6 className="text-muted">Recommended Actions</h6>
                        <p>{assessment.recommendedActions}</p>
                      </div>
                    )}

                    {assessment.additionalNotes && (
                      <div className="mb-3">
                        <h6 className="text-muted">Additional Notes</h6>
                        <p>{assessment.additionalNotes}</p>
                      </div>
                    )}
                  </div>
                </CCol>

                {/* Sidebar */}
                <CCol md={4}>
                  <div className="border-start ps-4">
                    {/* Assessment Information */}
                    <h6 className="text-muted mb-3">Assessment Information</h6>
                    <div className="mb-3">
                      <FontAwesomeIcon icon={faUser} className="me-2 text-muted" />
                      <strong>Assessor:</strong> {assessment.assessorName}
                    </div>
                    <div className="mb-3">
                      <FontAwesomeIcon icon={faCalendarAlt} className="me-2 text-muted" />
                      <strong>Assessment Date:</strong> {formatDate(assessment.assessmentDate)}
                    </div>
                    <div className="mb-4">
                      <FontAwesomeIcon icon={faClock} className="me-2 text-muted" />
                      <strong>Next Review:</strong> {formatDate(assessment.nextReviewDate)}
                      <div className="mt-1">
                        {getReviewStatusBadge(assessment.nextReviewDate)}
                      </div>
                    </div>

                    {/* Approval Information */}
                    {assessment.isApproved && assessment.approvedBy && (
                      <div className="mb-4">
                        <h6 className="text-muted mb-3">Approval Information</h6>
                        <div className="mb-2">
                          <FontAwesomeIcon icon={faCheckCircle} className="me-2 text-success" />
                          <strong>Approved by:</strong> {assessment.approvedBy.name}
                        </div>
                        {assessment.approvedAt && (
                          <div className="mb-2">
                            <FontAwesomeIcon icon={faCalendarAlt} className="me-2 text-muted" />
                            <strong>Approved on:</strong> {formatDate(assessment.approvedAt)}
                          </div>
                        )}
                        {assessment.approvalNotes && (
                          <div className="mb-2">
                            <h6 className="text-muted small">Approval Notes:</h6>
                            <p className="small">{assessment.approvalNotes}</p>
                          </div>
                        )}
                      </div>
                    )}

                    {/* Quick Actions */}
                    <h6 className="text-muted mb-3">Quick Actions</h6>
                    <CListGroup className="mb-4">
                      <CListGroupItem
                        className="d-flex justify-content-between align-items-center cursor-pointer"
                        style={{ cursor: 'pointer' }}
                        onClick={() => navigate(`/risk-assessments/${assessment.id}/edit`)}
                      >
                        <div>
                          <FontAwesomeIcon icon={faEdit} className="me-2 text-primary" />
                          Edit Assessment
                        </div>
                      </CListGroupItem>
                      
                      {isDueForReview && (
                        <CListGroupItem
                          className="d-flex justify-content-between align-items-center cursor-pointer"
                          style={{ cursor: 'pointer' }}
                          onClick={() => navigate(`/risk-assessments/${assessment.id}/reassess`)}
                        >
                          <div>
                            <FontAwesomeIcon icon={faRefresh} className="me-2 text-warning" />
                            Reassess Risk
                          </div>
                          <CBadge color="warning">Due</CBadge>
                        </CListGroupItem>
                      )}

                      <CListGroupItem
                        className="d-flex justify-content-between align-items-center cursor-pointer"
                        style={{ cursor: 'pointer' }}
                        onClick={() => navigate(`/hazards/${assessment.hazardId}`)}
                      >
                        <div>
                          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2 text-warning" />
                          View Related Hazard
                        </div>
                      </CListGroupItem>

                      <CListGroupItem
                        className="d-flex justify-content-between align-items-center cursor-pointer"
                        style={{ cursor: 'pointer' }}
                        onClick={() => window.print()}
                      >
                        <div>
                          <FontAwesomeIcon icon={faDownload} className="me-2 text-info" />
                          Print/Export Report
                        </div>
                      </CListGroupItem>
                    </CListGroup>

                    {/* Audit Information */}
                    <h6 className="text-muted mb-3">Audit Information</h6>
                    <div className="small text-muted">
                      <p className="mb-1">
                        <strong>Created:</strong> {formatDate(assessment.createdAt)}
                      </p>
                      {assessment.createdBy && (
                        <p className="mb-1">
                          <strong>Created By:</strong> {assessment.createdBy}
                        </p>
                      )}
                      {assessment.lastModifiedAt && (
                        <>
                          <p className="mb-1">
                            <strong>Modified:</strong> {formatDate(assessment.lastModifiedAt)}
                          </p>
                          {assessment.lastModifiedBy && (
                            <p className="mb-1">
                              <strong>Modified By:</strong> {assessment.lastModifiedBy}
                            </p>
                          )}
                        </>
                      )}
                    </div>
                  </div>
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default RiskAssessmentDetail;