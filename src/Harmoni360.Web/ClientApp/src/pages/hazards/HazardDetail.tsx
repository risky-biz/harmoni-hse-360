import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CRow,
  CCol,
  CButton,
  CBadge,
  CSpinner,
  CAlert,
  CListGroup,
  CListGroupItem,
  CCallout,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import { Icon } from '../../components/common/Icon';
import { useGetHazardQuery } from '../../features/hazards/hazardApi';
import HazardAuditTrail from '../../components/hazards/HazardAuditTrail';
import HazardAttachmentManager from '../../components/hazards/HazardAttachmentManager';
import MitigationActionsManager from '../../components/hazards/MitigationActionsManager';
import { formatDate } from '../../utils/dateUtils';

const HazardDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const hazardId = parseInt(id || '0');
  
  const { data: hazard, isLoading, error, refetch } = useGetHazardQuery({
    id: hazardId,
    includeAttachments: true,
    includeRiskAssessments: true,
    includeMitigationActions: true,
    includeReassessments: true,
  });

  const [showAttachmentsModal, setShowAttachmentsModal] = useState(false);
  const [showMitigationActionsModal, setShowMitigationActionsModal] = useState(false);
  const [showRiskAssessmentsModal, setShowRiskAssessmentsModal] = useState(false);

  const getSeverityColor = (severity: string) => {
    const colors: Record<string, string> = {
      'Negligible': 'success',
      'Minor': 'info',
      'Moderate': 'warning',
      'Major': 'danger',
      'Catastrophic': 'dark'
    };
    return colors[severity] || 'secondary';
  };

  const getStatusColor = (status: string) => {
    const colors: Record<string, string> = {
      'Reported': 'info',
      'UnderAssessment': 'warning',
      'ActionRequired': 'danger',
      'Mitigating': 'primary',
      'Monitoring': 'warning',
      'Resolved': 'success',
      'Closed': 'secondary'
    };
    return colors[status] || 'secondary';
  };

  const getRiskLevelColor = (level: string) => {
    const colors: Record<string, string> = {
      'Low': 'success',
      'Medium': 'warning',
      'High': 'danger',
      'Critical': 'dark'
    };
    return colors[level] || 'secondary';
  };

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading hazard details...</span>
      </div>
    );
  }

  if (error || !hazard) {
    return (
      <CAlert color="danger">
        Failed to load hazard details. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/hazards')}>
            <Icon icon={ACTION_ICONS.back} className="me-2" />
            Back to List
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <>
      <CRow>
        <CCol xs={12}>
          <CCard className="shadow-sm">
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div>
                <h4 className="mb-0">Hazard Details</h4>
                <small className="text-muted">ID: {hazard.id}</small>
              </div>
              <div>
                <CButton
                  color="light"
                  className="me-2"
                  onClick={() => navigate('/hazards')}
                >
                  <Icon icon={ACTION_ICONS.back} size="sm" className="me-2" />
                  Back
                </CButton>
                <CButton
                  color="primary"
                  className="me-2"
                  onClick={() => navigate(`/hazards/${id}/edit`)}
                >
                  <Icon icon={ACTION_ICONS.edit} size="sm" className="me-2" />
                  Edit
                </CButton>
              </div>
            </CCardHeader>

            <CCardBody>
              <CRow>
                <CCol md={8}>
                  <h5 className="mb-3">{hazard.title}</h5>

                  <div className="mb-4">
                    <h6 className="text-muted">Description</h6>
                    <p>{hazard.description}</p>
                  </div>

                  <CRow className="mb-4">
                    <CCol sm={6}>
                      <h6 className="text-muted">Status</h6>
                      <p>
                        <CBadge color={getStatusColor(hazard.status)}>
                          {hazard.status}
                        </CBadge>
                      </p>
                    </CCol>
                    <CCol sm={6}>
                      <h6 className="text-muted">Severity</h6>
                      <p>
                        <CBadge color={getSeverityColor(hazard.severity)}>
                          {hazard.severity}
                        </CBadge>
                      </p>
                    </CCol>
                  </CRow>

                  <CRow className="mb-4">
                    <CCol sm={6}>
                      <h6 className="text-muted">
                        <Icon
                          icon={CONTEXT_ICONS.category}
                          size="sm"
                          className="me-1"
                        />
                        Category
                      </h6>
                      <p>{hazard.category}</p>
                    </CCol>
                    <CCol sm={6}>
                      <h6 className="text-muted">
                        <Icon
                          icon={CONTEXT_ICONS.type}
                          size="sm"
                          className="me-1"
                        />
                        Type
                      </h6>
                      <p>{hazard.type}</p>
                    </CCol>
                  </CRow>

                  <CRow className="mb-4">
                    <CCol sm={6}>
                      <h6 className="text-muted">
                        <Icon
                          icon={CONTEXT_ICONS.date}
                          size="sm"
                          className="me-1"
                        />
                        Identified Date
                      </h6>
                      <p>{formatDate(hazard.identifiedDate)}</p>
                    </CCol>
                    <CCol sm={6}>
                      <h6 className="text-muted">
                        <Icon
                          icon={CONTEXT_ICONS.location}
                          size="sm"
                          className="me-1"
                        />
                        Location
                      </h6>
                      <p>{hazard.location}</p>
                    </CCol>
                  </CRow>

                  {hazard.expectedResolutionDate && (
                    <div className="mb-4">
                      <h6 className="text-muted">Expected Resolution</h6>
                      <p>{formatDate(hazard.expectedResolutionDate)}</p>
                    </div>
                  )}

                  {hazard.currentRiskLevel && (
                    <div className="mb-4">
                      <h6 className="text-muted">Current Risk Assessment</h6>
                      <div>
                        <CBadge 
                          color={getRiskLevelColor(hazard.currentRiskLevel)}
                          className="me-2"
                        >
                          {hazard.currentRiskLevel} Risk
                        </CBadge>
                        {hazard.currentRiskScore && (
                          <span className="text-muted">
                            Score: {hazard.currentRiskScore}
                          </span>
                        )}
                      </div>
                      {hazard.lastAssessmentDate && (
                        <small className="text-muted d-block mt-1">
                          Last assessed: {formatDate(hazard.lastAssessmentDate)}
                        </small>
                      )}
                    </div>
                  )}
                </CCol>

                <CCol md={4}>
                  <div className="border-start ps-4">
                    <h6 className="text-muted mb-3">Reporter Information</h6>
                    <div className="mb-3">
                      <FontAwesomeIcon
                        icon={CONTEXT_ICONS.reporter}
                        size="sm"
                        className="me-2"
                      />
                      <strong>{hazard.reporterName}</strong>
                    </div>
                    {hazard.reporterEmail && (
                      <div className="mb-3">
                        <small className="text-muted">
                          Email: {hazard.reporterEmail}
                        </small>
                      </div>
                    )}
                    <div className="mb-4">
                      <small className="text-muted">
                        Department: {hazard.reporterDepartment}
                      </small>
                    </div>

                    <h6 className="text-muted mb-3">Related Information</h6>
                    <CListGroup className="mb-4 related-info-mobile">
                      <CListGroupItem
                        className="d-flex justify-content-between cursor-pointer position-relative"
                        onClick={() => setShowRiskAssessmentsModal(true)}
                        style={{ cursor: 'pointer' }}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            setShowRiskAssessmentsModal(true);
                          }
                        }}
                      >
                        <div>
                          <span>Risk Assessments</span>
                          <small className="d-block text-muted">
                            Assessment history
                          </small>
                        </div>
                        <CBadge color="info">
                          {hazard.riskAssessmentsCount || 0}
                        </CBadge>
                      </CListGroupItem>
                      <CListGroupItem
                        className="d-flex justify-content-between cursor-pointer position-relative"
                        onClick={() => setShowMitigationActionsModal(true)}
                        style={{ cursor: 'pointer' }}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            setShowMitigationActionsModal(true);
                          }
                        }}
                      >
                        <div>
                          <span>Mitigation Actions</span>
                          <small className="d-block text-muted">
                            Assigned tasks
                          </small>
                        </div>
                        <CBadge color="info">
                          {hazard.mitigationActionsCount || 0}
                        </CBadge>
                      </CListGroupItem>
                      <CListGroupItem
                        className="d-flex justify-content-between cursor-pointer position-relative"
                        onClick={() => setShowAttachmentsModal(true)}
                        style={{ cursor: 'pointer' }}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            setShowAttachmentsModal(true);
                          }
                        }}
                      >
                        <div>
                          <span>Attachments</span>
                          <small className="d-block text-muted">
                            Photos, videos, documents
                          </small>
                        </div>
                        <CBadge color="info">
                          {hazard.attachmentsCount || 0}
                        </CBadge>
                      </CListGroupItem>
                    </CListGroup>

                    {hazard.attachmentsCount === 0 &&
                      hazard.mitigationActionsCount === 0 &&
                      hazard.riskAssessmentsCount === 0 && (
                        <CCallout color="info" className="small mb-4">
                          <strong>Note:</strong> Risk assessments, mitigation actions,
                          and file attachments can be added during the hazard
                          management process.
                          <div className="mt-2">
                            <CButton
                              color="primary"
                              size="sm"
                              variant="outline"
                              onClick={() => navigate(`/risk-assessments/create/${hazard.id}`)}
                            >
                              Create Risk Assessment
                            </CButton>
                          </div>
                        </CCallout>
                      )}

                    <h6 className="text-muted mb-3">Audit Information</h6>
                    <div className="small text-muted">
                      <p className="mb-1">
                        <strong>Created:</strong>{' '}
                        {formatDate(hazard.createdAt)}
                      </p>
                      {hazard.createdBy && (
                        <p className="mb-1">
                          <strong>Created By:</strong> {hazard.createdBy}
                        </p>
                      )}
                      {hazard.lastModifiedAt && (
                        <>
                          <p className="mb-1">
                            <strong>Modified:</strong>{' '}
                            {formatDate(hazard.lastModifiedAt)}
                          </p>
                          {hazard.lastModifiedBy && (
                            <p className="mb-1">
                              <strong>Modified By:</strong>{' '}
                              {hazard.lastModifiedBy}
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

      {/* Mitigation Actions (always visible) */}
      {hazard && (
        <CRow>
          <CCol xs={12}>
            <MitigationActionsManager
              hazardId={hazard.id}
              mitigationActions={hazard.mitigationActions || []}
              allowEdit={true}
              onRefresh={refetch}
            />
          </CCol>
        </CRow>
      )}

      {/* Hazard Audit Trail */}
      {hazard && (
        <CRow>
          <CCol xs={12}>
            <HazardAuditTrail hazardId={hazard.id} />
          </CCol>
        </CRow>
      )}

      {/* Attachments Modal */}
      <CModal
        visible={showAttachmentsModal}
        onClose={() => setShowAttachmentsModal(false)}
        size="lg"
      >
        <CModalHeader>
          <CModalTitle>Manage Attachments</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {hazard && (
            <HazardAttachmentManager
              hazardId={hazard.id}
              allowUpload={true}
              allowDelete={true}
            />
          )}
        </CModalBody>
      </CModal>

      {/* Mitigation Actions Modal */}
      <CModal
        visible={showMitigationActionsModal}
        onClose={() => setShowMitigationActionsModal(false)}
        size="xl"
      >
        <CModalHeader>
          <CModalTitle>Manage Mitigation Actions</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {hazard && (
            <MitigationActionsManager
              hazardId={hazard.id}
              mitigationActions={hazard.mitigationActions || []}
              allowEdit={true}
              onRefresh={refetch}
            />
          )}
        </CModalBody>
      </CModal>

      {/* Risk Assessments Modal */}
      <CModal
        visible={showRiskAssessmentsModal}
        onClose={() => setShowRiskAssessmentsModal(false)}
        size="xl"
      >
        <CModalHeader>
          <CModalTitle>Risk Assessments</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {hazard && hazard.riskAssessments && hazard.riskAssessments.length > 0 ? (
            <div>
              <div className="d-flex justify-content-between align-items-center mb-3">
                <h6 className="mb-0">Risk Assessment History</h6>
                <CButton
                  color="primary"
                  onClick={() => navigate(`/risk-assessments/create/${hazard.id}`)}
                >
                  <Icon icon={ACTION_ICONS.add} className="me-2" />
                  New Assessment
                </CButton>
              </div>
              <table className="table table-striped">
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>Assessor</th>
                    <th>Risk Level</th>
                    <th>Score</th>
                    <th>Date</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {hazard.riskAssessments.map(assessment => (
                    <tr key={assessment.id}>
                      <td>{assessment.type}</td>
                      <td>{assessment.assessorName}</td>
                      <td>
                        <CBadge color={getRiskLevelColor(assessment.riskLevel)}>
                          {assessment.riskLevel}
                        </CBadge>
                      </td>
                      <td>{assessment.riskScore}</td>
                      <td>{formatDate(assessment.assessmentDate)}</td>
                      <td>
                        <CBadge color={assessment.isActive ? 'success' : 'secondary'}>
                          {assessment.isActive ? 'Active' : 'Inactive'}
                        </CBadge>
                      </td>
                      <td>
                        <CButton
                          color="primary"
                          variant="outline"
                          size="sm"
                          onClick={() => navigate(`/risk-assessments/${assessment.id}`)}
                        >
                          View
                        </CButton>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="text-center py-4">
              <FontAwesomeIcon 
                icon={ACTION_ICONS.assessment} 
                size="3x" 
                className="text-muted mb-3 d-block mx-auto" 
              />
              <h6>No Risk Assessments</h6>
              <p className="text-muted mb-3">
                No risk assessments have been completed for this hazard.
              </p>
              <CButton
                color="primary"
                onClick={() => navigate(`/risk-assessments/create/${hazard.id}`)}
              >
                <Icon icon={ACTION_ICONS.add} className="me-2" />
                Create First Assessment
              </CButton>
            </div>
          )}
        </CModalBody>
      </CModal>
    </>
  );
};

export default HazardDetail;