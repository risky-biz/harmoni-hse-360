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
  CTable,
  CTableBody,
  CTableRow,
  CTableDataCell,
  CTableHeaderCell,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import {
  useGetSecurityIncidentQuery,
  useDeleteSecurityIncidentMutation,
  useEscalateIncidentMutation,
  useAssignIncidentMutation,
  useCloseIncidentMutation,
} from '../../features/security/securityApi';
import {
  SecurityIncidentType,
  SecuritySeverity,
  SecurityIncidentStatus,
  ThreatLevel,
  SecurityImpact,
  SecurityIncidentDetail,
} from '../../types/security';
import { formatDate } from '../../utils/dateUtils';

const SecurityIncidentDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('overview');
  const [showEscalateModal, setShowEscalateModal] = useState(false);
  const [showAssignModal, setShowAssignModal] = useState(false);
  const [showCloseModal, setShowCloseModal] = useState(false);

  const {
    data: incident,
    error,
    isLoading,
    refetch,
  } = useGetSecurityIncidentQuery(Number(id));

  const [deleteSecurityIncident, { isLoading: isDeleting }] =
    useDeleteSecurityIncidentMutation();
  const [escalateIncident, { isLoading: isEscalating }] =
    useEscalateIncidentMutation();
  const [assignIncident, { isLoading: isAssigning }] =
    useAssignIncidentMutation();
  const [closeIncident, { isLoading: isClosing }] =
    useCloseIncidentMutation();

  const getSeverityBadge = (severity: SecuritySeverity) => {
    const colorMap = {
      [SecuritySeverity.Low]: 'success',
      [SecuritySeverity.Medium]: 'warning',
      [SecuritySeverity.High]: 'danger',
      [SecuritySeverity.Critical]: 'dark',
    };
    
    const textMap = {
      [SecuritySeverity.Low]: 'Low',
      [SecuritySeverity.Medium]: 'Medium',
      [SecuritySeverity.High]: 'High',
      [SecuritySeverity.Critical]: 'Critical',
    };

    return (
      <CBadge color={colorMap[severity]} shape="rounded-pill">
        {textMap[severity]}
      </CBadge>
    );
  };

  const getStatusBadge = (status: SecurityIncidentStatus) => {
    const colorMap = {
      [SecurityIncidentStatus.Open]: 'danger',
      [SecurityIncidentStatus.Assigned]: 'warning',
      [SecurityIncidentStatus.Investigating]: 'info',
      [SecurityIncidentStatus.Contained]: 'warning',
      [SecurityIncidentStatus.Eradicating]: 'info',
      [SecurityIncidentStatus.Recovering]: 'info',
      [SecurityIncidentStatus.Resolved]: 'success',
      [SecurityIncidentStatus.Closed]: 'secondary',
    };

    const textMap = {
      [SecurityIncidentStatus.Open]: 'Open',
      [SecurityIncidentStatus.Assigned]: 'Assigned',
      [SecurityIncidentStatus.Investigating]: 'Investigating',
      [SecurityIncidentStatus.Contained]: 'Contained',
      [SecurityIncidentStatus.Eradicating]: 'Eradicating',
      [SecurityIncidentStatus.Recovering]: 'Recovering',
      [SecurityIncidentStatus.Resolved]: 'Resolved',
      [SecurityIncidentStatus.Closed]: 'Closed',
    };

    return (
      <CBadge color={colorMap[status]} shape="rounded-pill">
        {textMap[status]}
      </CBadge>
    );
  };

  const getThreatLevelBadge = (threatLevel: ThreatLevel) => {
    const colorMap = {
      [ThreatLevel.Minimal]: 'success',
      [ThreatLevel.Low]: 'info',
      [ThreatLevel.Medium]: 'warning',
      [ThreatLevel.High]: 'danger',
      [ThreatLevel.Severe]: 'dark',
    };

    const textMap = {
      [ThreatLevel.Minimal]: 'Minimal',
      [ThreatLevel.Low]: 'Low',
      [ThreatLevel.Medium]: 'Medium',
      [ThreatLevel.High]: 'High',
      [ThreatLevel.Severe]: 'Severe',
    };

    return (
      <CBadge color={colorMap[threatLevel]} shape="rounded-pill">
        {textMap[threatLevel]}
      </CBadge>
    );
  };

  const getTypeText = (type: SecurityIncidentType) => {
    const typeMap = {
      [SecurityIncidentType.PhysicalSecurity]: 'Physical Security',
      [SecurityIncidentType.Cybersecurity]: 'Cybersecurity',
      [SecurityIncidentType.PersonnelSecurity]: 'Personnel Security',
      [SecurityIncidentType.InformationSecurity]: 'Information Security',
    };
    return typeMap[type];
  };

  const getImpactText = (impact: SecurityImpact) => {
    const impactMap = {
      [SecurityImpact.None]: 'None',
      [SecurityImpact.Minor]: 'Minor',
      [SecurityImpact.Moderate]: 'Moderate',
      [SecurityImpact.Major]: 'Major',
      [SecurityImpact.Severe]: 'Severe',
    };
    return impactMap[impact];
  };

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading security incident details...</span>
      </div>
    );
  }

  if (error || !incident) {
    return (
      <CAlert color="danger">
        Failed to load security incident details. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/security/incidents')}>
            <FontAwesomeIcon icon={ACTION_ICONS.back} className="me-2" />
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
                <h4 className="mb-0">Security Incident Details</h4>
                <small className="text-muted">
                  {incident.incidentNumber} | ID: {incident.id}
                </small>
              </div>
              <div className="d-flex gap-2">
                <CButton
                  color="secondary"
                  variant="outline"
                  onClick={() => navigate('/security/incidents')}
                >
                  <FontAwesomeIcon icon={ACTION_ICONS.back} className="me-2" />
                  Back to List
                </CButton>
                <CButton
                  color="primary"
                  variant="outline"
                  onClick={() => navigate(`/security/incidents/${incident.id}/edit`)}
                >
                  <FontAwesomeIcon icon={ACTION_ICONS.edit} className="me-2" />
                  Edit
                </CButton>
              </div>
            </CCardHeader>

            <CCardBody>
              {/* Status and Priority Indicators */}
              <CRow className="mb-4">
                <CCol md={6}>
                  <CCallout 
                    color={incident.isOverdue ? 'warning' : 'info'} 
                    className="mb-0"
                  >
                    <div className="d-flex justify-content-between align-items-center">
                      <div>
                        <strong>Current Status:</strong> {getStatusBadge(incident.status)}
                        <div className="mt-1">
                          <small className="text-muted">
                            Days Open: {incident.daysOpen}
                            {incident.isOverdue && ' (Overdue)'}
                          </small>
                        </div>
                      </div>
                      <div className="text-end">
                        <div><strong>Threat Level:</strong> {getThreatLevelBadge(incident.threatLevel)}</div>
                        <div className="mt-1"><strong>Severity:</strong> {getSeverityBadge(incident.severity)}</div>
                      </div>
                    </div>
                  </CCallout>
                </CCol>
                <CCol md={6}>
                  <div className="d-flex gap-2 flex-wrap">
                    {incident.status !== SecurityIncidentStatus.Closed && (
                      <>
                        <CButton
                          color="warning"
                          size="sm"
                          onClick={() => setShowEscalateModal(true)}
                          disabled={isEscalating}
                        >
                          <FontAwesomeIcon icon={ACTION_ICONS.escalate} className="me-1" />
                          Escalate
                        </CButton>
                        <CButton
                          color="info"
                          size="sm"
                          onClick={() => setShowAssignModal(true)}
                          disabled={isAssigning}
                        >
                          <FontAwesomeIcon icon={ACTION_ICONS.assign} className="me-1" />
                          Assign
                        </CButton>
                        <CButton
                          color="success"
                          size="sm"
                          onClick={() => setShowCloseModal(true)}
                          disabled={isClosing}
                        >
                          <FontAwesomeIcon icon={ACTION_ICONS.close} className="me-1" />
                          Close
                        </CButton>
                      </>
                    )}
                  </div>
                </CCol>
              </CRow>

              {/* Navigation Tabs */}
              <CNav variant="tabs" className="mb-4">
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'overview'}
                    onClick={() => setActiveTab('overview')}
                    style={{ cursor: 'pointer' }}
                  >
                    Overview
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'threat'}
                    onClick={() => setActiveTab('threat')}
                    style={{ cursor: 'pointer' }}
                  >
                    Threat Assessment
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'response'}
                    onClick={() => setActiveTab('response')}
                    style={{ cursor: 'pointer' }}
                  >
                    Response Actions
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'attachments'}
                    onClick={() => setActiveTab('attachments')}
                    style={{ cursor: 'pointer' }}
                  >
                    Attachments ({incident.attachmentCount})
                  </CNavLink>
                </CNavItem>
                <CNavItem>
                  <CNavLink
                    active={activeTab === 'controls'}
                    onClick={() => setActiveTab('controls')}
                    style={{ cursor: 'pointer' }}
                  >
                    Security Controls
                  </CNavLink>
                </CNavItem>
              </CNav>

              {/* Tab Content */}
              <CTabContent>
                {/* Overview Tab */}
                <CTabPane visible={activeTab === 'overview'}>
                  <CRow>
                    <CCol md={6}>
                      <CCard className="h-100">
                        <CCardHeader>
                          <h6 className="mb-0">Basic Information</h6>
                        </CCardHeader>
                        <CCardBody>
                          <CTable borderless className="mb-0">
                            <CTableBody>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Title:
                                </CTableHeaderCell>
                                <CTableDataCell>{incident.title}</CTableDataCell>
                              </CTableRow>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Type:
                                </CTableHeaderCell>
                                <CTableDataCell>
                                  <CBadge color="info" shape="rounded-pill">
                                    {getTypeText(incident.incidentType)}
                                  </CBadge>
                                </CTableDataCell>
                              </CTableRow>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Location:
                                </CTableHeaderCell>
                                <CTableDataCell>{incident.location}</CTableDataCell>
                              </CTableRow>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Incident Date:
                                </CTableHeaderCell>
                                <CTableDataCell>{formatDate(incident.incidentDateTime)}</CTableDataCell>
                              </CTableRow>
                              {incident.detectionDateTime && (
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Detection Date:
                                  </CTableHeaderCell>
                                  <CTableDataCell>{formatDate(incident.detectionDateTime)}</CTableDataCell>
                                </CTableRow>
                              )}
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Impact:
                                </CTableHeaderCell>
                                <CTableDataCell>{getImpactText(incident.impact)}</CTableDataCell>
                              </CTableRow>
                              {incident.affectedPersonsCount && (
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Affected Persons:
                                  </CTableHeaderCell>
                                  <CTableDataCell>{incident.affectedPersonsCount}</CTableDataCell>
                                </CTableRow>
                              )}
                              {incident.estimatedLoss && (
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Estimated Loss:
                                  </CTableHeaderCell>
                                  <CTableDataCell>${incident.estimatedLoss.toLocaleString()}</CTableDataCell>
                                </CTableRow>
                              )}
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Data Breach:
                                </CTableHeaderCell>
                                <CTableDataCell>
                                  <CBadge color={incident.dataBreachOccurred ? 'danger' : 'success'}>
                                    {incident.dataBreachOccurred ? 'Yes' : 'No'}
                                  </CBadge>
                                </CTableDataCell>
                              </CTableRow>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Internal Threat:
                                </CTableHeaderCell>
                                <CTableDataCell>
                                  <CBadge color={incident.isInternalThreat ? 'warning' : 'info'}>
                                    {incident.isInternalThreat ? 'Yes' : 'No'}
                                  </CBadge>
                                </CTableDataCell>
                              </CTableRow>
                            </CTableBody>
                          </CTable>
                        </CCardBody>
                      </CCard>
                    </CCol>
                    <CCol md={6}>
                      <CCard className="h-100">
                        <CCardHeader>
                          <h6 className="mb-0">Personnel & Timeline</h6>
                        </CCardHeader>
                        <CCardBody>
                          <CTable borderless className="mb-0">
                            <CTableBody>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Reporter:
                                </CTableHeaderCell>
                                <CTableDataCell>
                                  {incident.reporterName || 'Anonymous'}
                                  {incident.reporterEmail && (
                                    <div><small className="text-muted">{incident.reporterEmail}</small></div>
                                  )}
                                </CTableDataCell>
                              </CTableRow>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Assigned To:
                                </CTableHeaderCell>
                                <CTableDataCell>
                                  {incident.assignedToName || 'Not assigned'}
                                </CTableDataCell>
                              </CTableRow>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Investigator:
                                </CTableHeaderCell>
                                <CTableDataCell>
                                  {incident.investigatorName || 'Not assigned'}
                                </CTableDataCell>
                              </CTableRow>
                              <CTableRow>
                                <CTableHeaderCell scope="row" className="fw-semibold">
                                  Created:
                                </CTableHeaderCell>
                                <CTableDataCell>
                                  {formatDate(incident.createdAt)}
                                  {incident.createdBy && (
                                    <div><small className="text-muted">by {incident.createdBy}</small></div>
                                  )}
                                </CTableDataCell>
                              </CTableRow>
                              {incident.lastModifiedAt && (
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Last Modified:
                                  </CTableHeaderCell>
                                  <CTableDataCell>
                                    {formatDate(incident.lastModifiedAt)}
                                    {incident.lastModifiedBy && (
                                      <div><small className="text-muted">by {incident.lastModifiedBy}</small></div>
                                    )}
                                  </CTableDataCell>
                                </CTableRow>
                              )}
                              {incident.containmentDateTime && (
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Contained:
                                  </CTableHeaderCell>
                                  <CTableDataCell>{formatDate(incident.containmentDateTime)}</CTableDataCell>
                                </CTableRow>
                              )}
                              {incident.resolutionDateTime && (
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Resolved:
                                  </CTableHeaderCell>
                                  <CTableDataCell>{formatDate(incident.resolutionDateTime)}</CTableDataCell>
                                </CTableRow>
                              )}
                            </CTableBody>
                          </CTable>
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>

                  {/* Description */}
                  <CRow className="mt-4">
                    <CCol>
                      <CCard>
                        <CCardHeader>
                          <h6 className="mb-0">Description</h6>
                        </CCardHeader>
                        <CCardBody>
                          <p className="mb-0">{incident.description}</p>
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>

                  {/* Containment Actions and Root Cause */}
                  {(incident.containmentActions || incident.rootCause) && (
                    <CRow className="mt-4">
                      {incident.containmentActions && (
                        <CCol md={6}>
                          <CCard>
                            <CCardHeader>
                              <h6 className="mb-0">Containment Actions</h6>
                            </CCardHeader>
                            <CCardBody>
                              <p className="mb-0">{incident.containmentActions}</p>
                            </CCardBody>
                          </CCard>
                        </CCol>
                      )}
                      {incident.rootCause && (
                        <CCol md={6}>
                          <CCard>
                            <CCardHeader>
                              <h6 className="mb-0">Root Cause</h6>
                            </CCardHeader>
                            <CCardBody>
                              <p className="mb-0">{incident.rootCause}</p>
                            </CCardBody>
                          </CCard>
                        </CCol>
                      )}
                    </CRow>
                  )}
                </CTabPane>

                {/* Threat Assessment Tab */}
                <CTabPane visible={activeTab === 'threat'}>
                  <CRow>
                    <CCol>
                      <CCard>
                        <CCardHeader className="d-flex justify-content-between align-items-center">
                          <h6 className="mb-0">Current Threat Assessment</h6>
                          <CButton
                            color="primary"
                            size="sm"
                            onClick={() => navigate(`/security/incidents/${incident.id}/threat-assessment`)}
                          >
                            <FontAwesomeIcon icon={ACTION_ICONS.edit} className="me-1" />
                            Update Assessment
                          </CButton>
                        </CCardHeader>
                        <CCardBody>
                          {incident.currentThreatAssessment ? (
                            <CTable borderless>
                              <CTableBody>
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Current Threat Level:
                                  </CTableHeaderCell>
                                  <CTableDataCell>
                                    {getThreatLevelBadge(incident.currentThreatAssessment.currentThreatLevel)}
                                  </CTableDataCell>
                                </CTableRow>
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Risk Score:
                                  </CTableHeaderCell>
                                  <CTableDataCell>
                                    {incident.currentThreatAssessment.riskScore}/100
                                    <CBadge color="info" className="ms-2">
                                      {incident.currentThreatAssessment.riskLevel}
                                    </CBadge>
                                  </CTableDataCell>
                                </CTableRow>
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Assessment Date:
                                  </CTableHeaderCell>
                                  <CTableDataCell>
                                    {formatDate(incident.currentThreatAssessment.assessmentDateTime)}
                                    <div><small className="text-muted">by {incident.currentThreatAssessment.assessedByName}</small></div>
                                  </CTableDataCell>
                                </CTableRow>
                                <CTableRow>
                                  <CTableHeaderCell scope="row" className="fw-semibold">
                                    Rationale:
                                  </CTableHeaderCell>
                                  <CTableDataCell>
                                    {incident.currentThreatAssessment.assessmentRationale}
                                  </CTableDataCell>
                                </CTableRow>
                              </CTableBody>
                            </CTable>
                          ) : (
                            <div className="text-center py-4">
                              <FontAwesomeIcon
                                icon={CONTEXT_ICONS.hazard}
                                className="text-muted mb-3"
                                style={{ fontSize: '2rem' }}
                              />
                              <h6 className="text-muted">No threat assessment available</h6>
                              <p className="text-muted">Create a threat assessment to analyze the security risk level.</p>
                              <CButton
                                color="primary"
                                onClick={() => navigate(`/security/incidents/${incident.id}/threat-assessment`)}
                              >
                                Create Assessment
                              </CButton>
                            </div>
                          )}
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>
                </CTabPane>

                {/* Response Actions Tab */}
                <CTabPane visible={activeTab === 'response'}>
                  <CRow>
                    <CCol>
                      <CCard>
                        <CCardHeader className="d-flex justify-content-between align-items-center">
                          <h6 className="mb-0">Response Actions ({incident.responseCount})</h6>
                          <CButton color="primary" size="sm">
                            <FontAwesomeIcon icon={ACTION_ICONS.create} className="me-1" />
                            Add Response
                          </CButton>
                        </CCardHeader>
                        <CCardBody>
                          {incident.responses && incident.responses.length > 0 ? (
                            <CListGroup>
                              {incident.responses.map((response, index) => (
                                <CListGroupItem key={response.id}>
                                  <div className="d-flex justify-content-between align-items-start">
                                    <div>
                                      <h6 className="mb-1">{response.actionTaken}</h6>
                                      <p className="mb-1">{formatDate(response.actionDateTime)}</p>
                                      <small className="text-muted">by {response.responderName}</small>
                                    </div>
                                    <div>
                                      <CBadge color={response.wasSuccessful ? 'success' : 'danger'}>
                                        {response.wasSuccessful ? 'Successful' : 'Failed'}
                                      </CBadge>
                                      {response.followUpRequired && (
                                        <CBadge color="warning" className="ms-1">
                                          Follow-up Required
                                        </CBadge>
                                      )}
                                    </div>
                                  </div>
                                </CListGroupItem>
                              ))}
                            </CListGroup>
                          ) : (
                            <div className="text-center py-4">
                              <FontAwesomeIcon
                                icon={ACTION_ICONS.respond}
                                className="text-muted mb-3"
                                style={{ fontSize: '2rem' }}
                              />
                              <h6 className="text-muted">No response actions recorded</h6>
                              <p className="text-muted">Record response actions taken to address this security incident.</p>
                            </div>
                          )}
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>
                </CTabPane>

                {/* Attachments Tab */}
                <CTabPane visible={activeTab === 'attachments'}>
                  <CRow>
                    <CCol>
                      <CCard>
                        <CCardHeader className="d-flex justify-content-between align-items-center">
                          <h6 className="mb-0">Evidence & Attachments ({incident.attachmentCount})</h6>
                          <CButton color="primary" size="sm">
                            <FontAwesomeIcon icon={ACTION_ICONS.upload} className="me-1" />
                            Upload Evidence
                          </CButton>
                        </CCardHeader>
                        <CCardBody>
                          {incident.attachments && incident.attachments.length > 0 ? (
                            <CListGroup>
                              {incident.attachments.map((attachment, index) => (
                                <CListGroupItem key={attachment.id} className="d-flex justify-content-between align-items-center">
                                  <div>
                                    <h6 className="mb-1">{attachment.fileName}</h6>
                                    <small className="text-muted">
                                      {attachment.fileSizeFormatted} | {attachment.fileType}
                                      {attachment.isConfidential && ' | Confidential'}
                                    </small>
                                    {attachment.description && (
                                      <div><small className="text-muted">{attachment.description}</small></div>
                                    )}
                                  </div>
                                  <div>
                                    <CButton color="primary" variant="outline" size="sm">
                                      <FontAwesomeIcon icon={ACTION_ICONS.download} />
                                    </CButton>
                                  </div>
                                </CListGroupItem>
                              ))}
                            </CListGroup>
                          ) : (
                            <div className="text-center py-4">
                              <FontAwesomeIcon
                                icon={ACTION_ICONS.upload}
                                className="text-muted mb-3"
                                style={{ fontSize: '2rem' }}
                              />
                              <h6 className="text-muted">No attachments</h6>
                              <p className="text-muted">Upload evidence, screenshots, or related documents.</p>
                            </div>
                          )}
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>
                </CTabPane>

                {/* Security Controls Tab */}
                <CTabPane visible={activeTab === 'controls'}>
                  <CRow>
                    <CCol>
                      <CCard>
                        <CCardHeader className="d-flex justify-content-between align-items-center">
                          <h6 className="mb-0">Implemented Security Controls</h6>
                          <CButton color="primary" size="sm">
                            <FontAwesomeIcon icon={ACTION_ICONS.create} className="me-1" />
                            Add Control
                          </CButton>
                        </CCardHeader>
                        <CCardBody>
                          {incident.implementedControls && incident.implementedControls.length > 0 ? (
                            <CListGroup>
                              {incident.implementedControls.map((control, index) => (
                                <CListGroupItem key={control.id}>
                                  <div className="d-flex justify-content-between align-items-start">
                                    <div>
                                      <h6 className="mb-1">{control.controlName}</h6>
                                      <p className="mb-1">{control.controlDescription}</p>
                                      <small className="text-muted">
                                        Implemented: {formatDate(control.implementationDate)} by {control.implementedByName}
                                      </small>
                                    </div>
                                    <div>
                                      <CBadge color="info" className="me-1">
                                        {control.controlType === 1 ? 'Preventive' : 
                                         control.controlType === 2 ? 'Detective' : 'Corrective'}
                                      </CBadge>
                                      <CBadge color={control.isOverdue ? 'warning' : 'success'}>
                                        {control.isOverdue ? 'Review Overdue' : 'Active'}
                                      </CBadge>
                                    </div>
                                  </div>
                                </CListGroupItem>
                              ))}
                            </CListGroup>
                          ) : (
                            <div className="text-center py-4">
                              <FontAwesomeIcon
                                icon={CONTEXT_ICONS.shield}
                                className="text-muted mb-3"
                                style={{ fontSize: '2rem' }}
                              />
                              <h6 className="text-muted">No security controls implemented</h6>
                              <p className="text-muted">Implement security controls to prevent similar incidents.</p>
                            </div>
                          )}
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>
                </CTabPane>
              </CTabContent>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  );
};

export default SecurityIncidentDetail;