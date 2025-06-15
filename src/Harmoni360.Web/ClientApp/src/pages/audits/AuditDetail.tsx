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
  CCallout,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
  CBreadcrumb,
  CBreadcrumbItem,
  CButtonGroup,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CDropdownDivider,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CProgress,
  CListGroup,
  CListGroupItem,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faHome,
  faArrowLeft,
  faEdit,
  faTrash,
  faEllipsisV,
  faPaperclip,
  faUsers,
  faTasks,
  faHistory,
  faExclamationTriangle,
  faCalendarAlt,
  faMapMarkerAlt,
  faUser,
  faEnvelope,
  faBuilding,
  faClock,
  faCheckCircle,
  faTimesCircle,
  faShieldAlt,
  faFileAlt,
  faPrint,
  faDownload,
  faShare,
  faPlay,
  faStop,
  faClipboardCheck,
  faHardHat,
  faExclamationCircle,
  faInfoCircle,
  faCheck,
  faWrench,
  faCertificate,
  faChartBar,
  faListAlt,
  faCamera,
  faTag,
  faFlag,
  faBullseye,
  faAward,
} from '@fortawesome/free-solid-svg-icons';
import {
  useGetAuditByIdQuery,
  useDeleteAuditMutation,
  useStartAuditMutation,
  useCompleteAuditMutation,
  useCancelAuditMutation,
} from '../../features/audits/auditApi';
import {
  getAuditStatusBadge,
  getAuditTypeBadge,
  getAuditPriorityBadge,
  getAuditScoreBadge,
  getFindingTypeBadge,
  getFindingSeverityBadge,
  getFindingStatusBadge,
  formatDate,
  formatDateTime,
  formatRelativeTime,
  isAuditOverdue,
  isAuditExpiringSoon,
  getAuditProgress,
  calculateAuditScore,
  getComplianceRate,
  getFindingsSummary,
  canEditAudit,
  canStartAudit,
  canCompleteAudit,
  canCancelAudit,
  canDeleteAudit,
} from '../../utils/auditUtils';

const AuditDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('overview');
  const [showFindingsModal, setShowFindingsModal] = useState(false);
  const [showItemsModal, setShowItemsModal] = useState(false);

  const {
    data: audit,
    error,
    isLoading,
  } = useGetAuditByIdQuery(id!);
  
  const [deleteAudit, { isLoading: isDeleting }] = useDeleteAuditMutation();
  const [startAudit] = useStartAuditMutation();
  const [completeAudit] = useCompleteAuditMutation();
  const [cancelAudit] = useCancelAuditMutation();

  const handleStartAudit = async () => {
    if (window.confirm('Are you sure you want to start this audit?')) {
      try {
        await startAudit(id!).unwrap();
      } catch (error) {
        console.error('Failed to start audit:', error);
        alert('Failed to start audit. Please try again.');
      }
    }
  };

  const handleCompleteAudit = async () => {
    if (window.confirm('Are you sure you want to mark this audit as completed?')) {
      try {
        await completeAudit(id!).unwrap();
      } catch (error) {
        console.error('Failed to complete audit:', error);
        alert('Failed to complete audit. Please try again.');
      }
    }
  };

  const handleCancelAudit = async () => {
    if (window.confirm('Are you sure you want to cancel this audit? This action cannot be undone.')) {
      try {
        await cancelAudit(id!).unwrap();
      } catch (error) {
        console.error('Failed to cancel audit:', error);
        alert('Failed to cancel audit. Please try again.');
      }
    }
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this audit? This action cannot be undone.')) {
      try {
        await deleteAudit(id!).unwrap();
        navigate('/audits');
      } catch (error) {
        console.error('Failed to delete audit:', error);
        alert('Failed to delete audit. Please try again.');
      }
    }
  };

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
        <h4>Error Loading Audit</h4>
        <p>Unable to load audit details. Please try again later.</p>
        <hr />
        <div className="d-flex">
          <CButton
            color="danger"
            variant="outline"
            onClick={() => navigate('/audits')}
          >
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            Back to Audits
          </CButton>
        </div>
      </CAlert>
    );
  }

  if (!audit) {
    return (
      <CAlert color="warning">
        <h4>Audit Not Found</h4>
        <p>The requested audit could not be found.</p>
        <hr />
        <div className="d-flex">
          <CButton
            color="warning"
            variant="outline"
            onClick={() => navigate('/audits')}
          >
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            Back to Audits
          </CButton>
        </div>
      </CAlert>
    );
  }

  const progress = getAuditProgress(audit);
  const auditScore = calculateAuditScore(audit);
  const complianceRate = getComplianceRate(audit);
  const findingsSummary = getFindingsSummary(audit.findings || []);

  return (
    <div className="container-fluid">
      {/* Breadcrumb */}
      <CBreadcrumb className="mb-4">
        <CBreadcrumbItem>
          <FontAwesomeIcon icon={faHome} className="me-1" />
          Dashboard
        </CBreadcrumbItem>
        <CBreadcrumbItem href="/audits">Audits</CBreadcrumbItem>
        <CBreadcrumbItem active>Audit Details</CBreadcrumbItem>
      </CBreadcrumb>

      {/* Status Alerts */}
      {isAuditOverdue(audit) && (
        <CCallout color="danger" className="mb-4">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
          <strong>Overdue:</strong> This audit was scheduled for {formatDate(audit.scheduledDate)} and is now overdue.
        </CCallout>
      )}

      {isAuditExpiringSoon(audit) && !isAuditOverdue(audit) && (
        <CCallout color="warning" className="mb-4">
          <FontAwesomeIcon icon={faClock} className="me-2" />
          <strong>Coming Up:</strong> This audit is scheduled for {formatDate(audit.scheduledDate)}.
        </CCallout>
      )}

      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-start mb-4">
            <div>
              <h1 className="h3 mb-2">{audit.title}</h1>
              <div className="d-flex align-items-center gap-3 text-muted">
                <span>
                  <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                  Scheduled: {audit.scheduledDate ? formatDate(audit.scheduledDate) : 'Not scheduled'}
                </span>
                {audit.location && (
                  <span>
                    <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                    {audit.location}
                  </span>
                )}
                <span>ID: #{audit.auditNumber}</span>
              </div>
            </div>
            
            <div className="d-flex gap-2">
              <CButtonGroup>
                <CButton
                  color="primary"
                  variant="outline"
                  onClick={() => navigate('/audits')}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                  Back
                </CButton>
                {canEditAudit(audit) && (
                  <CButton
                    color="primary"
                    onClick={() => navigate(`/audits/${id}/edit`)}
                  >
                    <FontAwesomeIcon icon={faEdit} className="me-2" />
                    Edit
                  </CButton>
                )}
              </CButtonGroup>

              {/* Action Buttons */}
              {canStartAudit(audit) && (
                <CButton
                  color="success"
                  onClick={handleStartAudit}
                >
                  <FontAwesomeIcon icon={faPlay} className="me-2" />
                  Start Audit
                </CButton>
              )}
              {canCompleteAudit(audit) && (
                <CButton
                  color="warning"
                  onClick={handleCompleteAudit}
                >
                  <FontAwesomeIcon icon={faCheckCircle} className="me-2" />
                  Complete Audit
                </CButton>
              )}
              {canCancelAudit(audit) && (
                <CButton
                  color="danger"
                  variant="outline"
                  onClick={handleCancelAudit}
                >
                  <FontAwesomeIcon icon={faTimesCircle} className="me-2" />
                  Cancel
                </CButton>
              )}

              <CDropdown>
                <CDropdownToggle color="secondary" variant="outline">
                  <FontAwesomeIcon icon={faEllipsisV} />
                </CDropdownToggle>
                <CDropdownMenu>
                  <CDropdownItem>
                    <FontAwesomeIcon icon={faPrint} className="me-2" />
                    Print Report
                  </CDropdownItem>
                  <CDropdownItem>
                    <FontAwesomeIcon icon={faDownload} className="me-2" />
                    Export PDF
                  </CDropdownItem>
                  <CDropdownItem>
                    <FontAwesomeIcon icon={faShare} className="me-2" />
                    Share
                  </CDropdownItem>
                  {canDeleteAudit(audit) && (
                    <>
                      <CDropdownDivider />
                      <CDropdownItem 
                        className="text-danger"
                        onClick={handleDelete}
                        disabled={isDeleting}
                      >
                        <FontAwesomeIcon icon={faTrash} className="me-2" />
                        {isDeleting ? 'Deleting...' : 'Delete Audit'}
                      </CDropdownItem>
                    </>
                  )}
                </CDropdownMenu>
              </CDropdown>
            </div>
          </div>

          {/* Status, Type and Priority Badges */}
          <div className="d-flex align-items-center gap-3 mb-4">
            <div className="d-flex align-items-center gap-2">
              <span className="text-muted">Status:</span>
              {getAuditStatusBadge(audit.status)}
            </div>
            <div className="d-flex align-items-center gap-2">
              <span className="text-muted">Type:</span>
              {getAuditTypeBadge(audit.type)}
            </div>
            <div className="d-flex align-items-center gap-2">
              <span className="text-muted">Priority:</span>
              {getAuditPriorityBadge(audit.priority)}
            </div>
            {audit.overallScore && (
              <div className="d-flex align-items-center gap-2">
                <span className="text-muted">Score:</span>
                {getAuditScoreBadge(audit.overallScore)}
              </div>
            )}
          </div>
        </CCol>
      </CRow>

      {/* Navigation Tabs */}
      <CCard className="mb-4">
        <CCardHeader className="bg-light">
          <CNav variant="tabs" className="card-header-tabs">
            <CNavItem>
              <CNavLink
                active={activeTab === 'overview'}
                onClick={() => setActiveTab('overview')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                Overview
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'audit-details'}
                onClick={() => setActiveTab('audit-details')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                Audit Details
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'checklist-items'}
                onClick={() => setActiveTab('checklist-items')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faTasks} className="me-2" />
                Checklist Items ({audit.items?.length || 0})
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'findings'}
                onClick={() => setActiveTab('findings')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                Findings ({audit.findings?.length || 0})
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'evidence'}
                onClick={() => setActiveTab('evidence')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faPaperclip} className="me-2" />
                Evidence ({audit.attachments?.length || 0})
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'history'}
                onClick={() => setActiveTab('history')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faHistory} className="me-2" />
                Activity History
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
                  {/* Basic Details */}
                  <div className="mb-5">
                    <h5 className="mb-3">Basic Information</h5>
                    <CRow className="g-4">
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faFileAlt} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Audit Number</small>
                            <span className="fw-semibold">{audit.auditNumber}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faClipboardCheck} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Audit Type</small>
                            <span>{getAuditTypeBadge(audit.type)}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faUser} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Lead Auditor</small>
                            <span>{audit.auditorName || 'Not assigned'}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faBuilding} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Department</small>
                            <span>{audit.departmentName || 'Not specified'}</span>
                          </div>
                        </div>
                      </CCol>
                    </CRow>
                  </div>

                  {/* Description */}
                  <div className="mb-5">
                    <h5 className="mb-3">Description</h5>
                    <p className="text-muted">{audit.description || 'No description provided.'}</p>
                  </div>

                  {/* Timeline */}
                  <div className="mb-5">
                    <h5 className="mb-3">Timeline</h5>
                    <CRow className="g-4">
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faCalendarAlt} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Scheduled Date</small>
                            <span>{audit.scheduledDate ? formatDate(audit.scheduledDate) : 'Not scheduled'}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faClock} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Estimated Duration</small>
                            <span>{audit.estimatedDurationMinutes ? `${audit.estimatedDurationMinutes} minutes` : 'Not specified'}</span>
                          </div>
                        </div>
                      </CCol>
                      {audit.startedDate && (
                        <CCol sm={6}>
                          <div className="d-flex">
                            <FontAwesomeIcon icon={faPlay} className="text-muted me-3 mt-1" />
                            <div>
                              <small className="text-muted d-block">Started</small>
                              <span>{formatDateTime(audit.startedDate)}</span>
                            </div>
                          </div>
                        </CCol>
                      )}
                      {audit.completedDate && (
                        <CCol sm={6}>
                          <div className="d-flex">
                            <FontAwesomeIcon icon={faCheckCircle} className="text-muted me-3 mt-1" />
                            <div>
                              <small className="text-muted d-block">Completed</small>
                              <span>{formatDateTime(audit.completedDate)}</span>
                            </div>
                          </div>
                        </CCol>
                      )}
                    </CRow>
                  </div>
                </CCol>

                <CCol md={4}>
                  {/* Progress Card */}
                  <CCard className="mb-4">
                    <CCardHeader>
                      <FontAwesomeIcon icon={faChartBar} className="me-2" />
                      Progress Overview
                    </CCardHeader>
                    <CCardBody>
                      <div className="mb-3">
                        <div className="d-flex justify-content-between align-items-center mb-2">
                          <span className="fw-semibold">Completion</span>
                          <span className="text-muted">{progress}%</span>
                        </div>
                        <CProgress value={progress} color={progress === 100 ? 'success' : progress > 50 ? 'info' : 'warning'} />
                      </div>

                      {auditScore !== null && (
                        <div className="mb-3">
                          <div className="d-flex justify-content-between align-items-center mb-2">
                            <span className="fw-semibold">Score</span>
                            <span className="text-muted">{auditScore}%</span>
                          </div>
                          <CProgress value={auditScore} color={auditScore >= 80 ? 'success' : auditScore >= 60 ? 'warning' : 'danger'} />
                        </div>
                      )}

                      {complianceRate !== null && (
                        <div className="mb-3">
                          <div className="d-flex justify-content-between align-items-center mb-2">
                            <span className="fw-semibold">Compliance</span>
                            <span className="text-muted">{complianceRate}%</span>
                          </div>
                          <CProgress value={complianceRate} color={complianceRate >= 90 ? 'success' : complianceRate >= 70 ? 'warning' : 'danger'} />
                        </div>
                      )}

                      <hr />

                      <div className="mb-2">
                        <small className="text-muted d-block">Items</small>
                        <span className="fw-semibold">{audit.items?.length || 0} total</span>
                      </div>

                      <div className="mb-2">
                        <small className="text-muted d-block">Findings</small>
                        <span className="fw-semibold">{findingsSummary.total} total</span>
                        {findingsSummary.open > 0 && (
                          <span className="text-danger ms-2">({findingsSummary.open} open)</span>
                        )}
                      </div>

                      <div className="mb-2">
                        <small className="text-muted d-block">Evidence</small>
                        <span className="fw-semibold">{audit.attachments?.length || 0} files</span>
                      </div>
                    </CCardBody>
                  </CCard>

                  {/* Critical Findings Alert */}
                  {findingsSummary.critical > 0 && (
                    <CAlert color="danger">
                      <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                      <strong>Critical Findings!</strong>
                      <br />
                      This audit has {findingsSummary.critical} critical finding{findingsSummary.critical > 1 ? 's' : ''} that require immediate attention.
                    </CAlert>
                  )}
                </CCol>
              </CRow>
            </CTabPane>

            {/* Audit Details Tab */}
            <CTabPane visible={activeTab === 'audit-details'}>
              <CRow>
                <CCol md={8}>
                  <div className="mb-4">
                    <h5 className="mb-3">Audit Scope & Standards</h5>
                    
                    {audit.standardsApplied && (
                      <div className="mb-3">
                        <strong>Standards Applied:</strong>
                        <p className="text-muted mt-1">{audit.standardsApplied}</p>
                      </div>
                    )}

                    {audit.isRegulatory && audit.regulatoryReference && (
                      <div className="mb-3">
                        <strong>Regulatory Reference:</strong>
                        <p className="text-muted mt-1">{audit.regulatoryReference}</p>
                      </div>
                    )}

                    <div className="mb-3">
                      <strong>Regulatory Audit:</strong>
                      <span className="ms-2">
                        {audit.isRegulatory ? (
                          <CBadge color="warning">Yes</CBadge>
                        ) : (
                          <CBadge color="secondary">No</CBadge>
                        )}
                      </span>
                    </div>
                  </div>

                  {audit.summary && (
                    <div className="mb-4">
                      <h5 className="mb-3">Summary</h5>
                      <p className="text-muted">{audit.summary}</p>
                    </div>
                  )}

                  {audit.recommendations && (
                    <div className="mb-4">
                      <h5 className="mb-3">Recommendations</h5>
                      <p className="text-muted">{audit.recommendations}</p>
                    </div>
                  )}
                </CCol>

                <CCol md={4}>
                  <CCard>
                    <CCardHeader>
                      <FontAwesomeIcon icon={faAward} className="me-2" />
                      Audit Metrics
                    </CCardHeader>
                    <CCardBody>
                      <div className="mb-3">
                        <small className="text-muted d-block">Risk Level</small>
                        <span className="fw-semibold">{audit.riskLevel || 'Not assessed'}</span>
                      </div>

                      <div className="mb-3">
                        <small className="text-muted d-block">Category</small>
                        <span className="fw-semibold">{audit.category || 'Not specified'}</span>
                      </div>

                      {audit.actualDurationMinutes && (
                        <div className="mb-3">
                          <small className="text-muted d-block">Actual Duration</small>
                          <span className="fw-semibold">{audit.actualDurationMinutes} minutes</span>
                        </div>
                      )}

                      <div className="mb-3">
                        <small className="text-muted d-block">Created</small>
                        <span className="fw-semibold">{formatDateTime(audit.createdAt)}</span>
                        <br />
                        <small className="text-muted">by {audit.createdBy}</small>
                      </div>

                      {audit.lastModifiedAt && (
                        <div className="mb-3">
                          <small className="text-muted d-block">Last Modified</small>
                          <span className="fw-semibold">{formatRelativeTime(audit.lastModifiedAt)}</span>
                          {audit.lastModifiedBy && (
                            <>
                              <br />
                              <small className="text-muted">by {audit.lastModifiedBy}</small>
                            </>
                          )}
                        </div>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>
            </CTabPane>

            {/* Checklist Items Tab */}
            <CTabPane visible={activeTab === 'checklist-items'}>
              {audit.items && audit.items.length > 0 ? (
                <CTable responsive striped hover>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell>Item #</CTableHeaderCell>
                      <CTableHeaderCell>Description</CTableHeaderCell>
                      <CTableHeaderCell>Category</CTableHeaderCell>
                      <CTableHeaderCell>Status</CTableHeaderCell>
                      <CTableHeaderCell>Compliance</CTableHeaderCell>
                      <CTableHeaderCell>Score</CTableHeaderCell>
                      <CTableHeaderCell>Assessed By</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {audit.items.map((item: any) => (
                      <CTableRow key={item.id}>
                        <CTableDataCell>
                          <span className="fw-semibold">{item.itemNumber}</span>
                          {item.isRequired && (
                            <CBadge color="danger" size="sm" className="ms-1">Required</CBadge>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>{item.description}</CTableDataCell>
                        <CTableDataCell>
                          {item.category && (
                            <CBadge color="info" variant="outline">{item.category}</CBadge>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={
                            item.status === 'Completed' ? 'success' :
                            item.status === 'InProgress' ? 'warning' :
                            item.status === 'NonCompliant' ? 'danger' :
                            item.status === 'NotApplicable' ? 'secondary' :
                            'light'
                          }>
                            {item.status || 'Not Started'}
                          </CBadge>
                        </CTableDataCell>
                        <CTableDataCell>
                          {item.isCompliant !== null ? (
                            <CBadge color={item.isCompliant ? 'success' : 'danger'}>
                              {item.isCompliant ? 'Compliant' : 'Non-Compliant'}
                            </CBadge>
                          ) : (
                            <span className="text-muted">-</span>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>
                          {item.actualPoints !== null && item.maxPoints !== null ? (
                            <span>{item.actualPoints}/{item.maxPoints}</span>
                          ) : (
                            <span className="text-muted">-</span>
                          )}
                        </CTableDataCell>
                        <CTableDataCell>
                          {item.assessedBy ? (
                            <div>
                              <span>{item.assessedBy}</span>
                              {item.assessedAt && (
                                <br />
                                <small className="text-muted">{formatRelativeTime(item.assessedAt)}</small>
                              )}
                            </div>
                          ) : (
                            <span className="text-muted">Not assessed</span>
                          )}
                        </CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>
              ) : (
                <CAlert color="info">
                  <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                  No checklist items have been added to this audit yet.
                </CAlert>
              )}
            </CTabPane>

            {/* Findings Tab */}
            <CTabPane visible={activeTab === 'findings'}>
              {audit.findings && audit.findings.length > 0 ? (
                <div>
                  {/* Findings Summary */}
                  <CRow className="mb-4">
                    <CCol>
                      <div className="d-flex gap-3 mb-3">
                        <CBadge color="danger">Critical: {findingsSummary.critical}</CBadge>
                        <CBadge color="warning">Major: {findingsSummary.major}</CBadge>
                        <CBadge color="info">Moderate: {findingsSummary.moderate}</CBadge>
                        <CBadge color="success">Minor: {findingsSummary.minor}</CBadge>
                        <CBadge color="secondary">Open: {findingsSummary.open}</CBadge>
                      </div>
                    </CCol>
                  </CRow>

                  {/* Findings List */}
                  <CRow>
                    {audit.findings.map((finding: any) => (
                      <CCol md={6} key={finding.id} className="mb-4">
                        <CCard className="h-100">
                          <CCardHeader className="d-flex justify-content-between align-items-start">
                            <div>
                              <h6 className="mb-1">{finding.findingNumber}</h6>
                              <div className="d-flex gap-2">
                                {getFindingTypeBadge(finding.type)}
                                {getFindingSeverityBadge(finding.severity)}
                              </div>
                            </div>
                            {getFindingStatusBadge(finding.status)}
                          </CCardHeader>
                          <CCardBody>
                            <p className="text-muted mb-3">{finding.description}</p>
                            
                            {finding.location && (
                              <div className="mb-2">
                                <FontAwesomeIcon icon={faMapMarkerAlt} className="text-muted me-2" />
                                <small className="text-muted">{finding.location}</small>
                              </div>
                            )}

                            {finding.immediateAction && (
                              <div className="mb-2">
                                <strong>Immediate Action:</strong>
                                <p className="text-muted small mt-1">{finding.immediateAction}</p>
                              </div>
                            )}

                            {finding.correctiveAction && (
                              <div className="mb-2">
                                <strong>Corrective Action:</strong>
                                <p className="text-muted small mt-1">{finding.correctiveAction}</p>
                              </div>
                            )}

                            {finding.dueDate && (
                              <div className="mt-3">
                                <FontAwesomeIcon icon={faCalendarAlt} className="text-muted me-2" />
                                <small className="text-muted">
                                  Due: {formatDate(finding.dueDate)}
                                  {finding.isOverdue && (
                                    <span className="text-danger ms-2">(Overdue)</span>
                                  )}
                                </small>
                              </div>
                            )}
                          </CCardBody>
                        </CCard>
                      </CCol>
                    ))}
                  </CRow>
                </div>
              ) : (
                <CAlert color="success">
                  <FontAwesomeIcon icon={faCheckCircle} className="me-2" />
                  No findings recorded for this audit. This indicates good compliance!
                </CAlert>
              )}
            </CTabPane>

            {/* Evidence Tab */}
            <CTabPane visible={activeTab === 'evidence'}>
              {audit.attachments && audit.attachments.length > 0 ? (
                <CRow>
                  {audit.attachments.map((attachment: any) => (
                    <CCol md={4} key={attachment.id} className="mb-3">
                      <CCard>
                        <CCardBody>
                          <div className="d-flex align-items-center mb-2">
                            <FontAwesomeIcon icon={faCamera} className="text-muted me-2" />
                            <span className="fw-semibold text-truncate">{attachment.originalFileName}</span>
                          </div>
                          
                          <div className="mb-2">
                            <small className="text-muted">
                              Size: {Math.round(attachment.fileSize / 1024)} KB
                            </small>
                          </div>

                          {attachment.description && (
                            <p className="text-muted small mb-3">{attachment.description}</p>
                          )}

                          {attachment.category && (
                            <CBadge color="info" variant="outline" size="sm" className="mb-2">
                              {attachment.category}
                            </CBadge>
                          )}

                          <div className="d-flex gap-2">
                            <CButton size="sm" color="primary" variant="outline">
                              <FontAwesomeIcon icon={faDownload} className="me-1" />
                              Download
                            </CButton>
                          </div>

                          <div className="mt-2">
                            <small className="text-muted">
                              Uploaded {formatRelativeTime(attachment.uploadedAt)}
                              <br />
                              by {attachment.uploadedBy}
                            </small>
                          </div>
                        </CCardBody>
                      </CCard>
                    </CCol>
                  ))}
                </CRow>
              ) : (
                <CAlert color="info">
                  <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                  No evidence files have been uploaded for this audit yet.
                </CAlert>
              )}
            </CTabPane>

            {/* Activity History Tab */}
            <CTabPane visible={activeTab === 'history'}>
              <CListGroup>
                <CListGroupItem className="d-flex justify-content-between align-items-start">
                  <div className="ms-2 me-auto">
                    <div className="fw-bold">Audit Created</div>
                    <small className="text-muted">
                      Created by {audit.createdBy} on {formatDateTime(audit.createdAt)}
                    </small>
                  </div>
                  <CBadge color="primary" pill>
                    <FontAwesomeIcon icon={faFileAlt} />
                  </CBadge>
                </CListGroupItem>

                {audit.startedDate && (
                  <CListGroupItem className="d-flex justify-content-between align-items-start">
                    <div className="ms-2 me-auto">
                      <div className="fw-bold">Audit Started</div>
                      <small className="text-muted">
                        Started on {formatDateTime(audit.startedDate)}
                      </small>
                    </div>
                    <CBadge color="success" pill>
                      <FontAwesomeIcon icon={faPlay} />
                    </CBadge>
                  </CListGroupItem>
                )}

                {audit.completedDate && (
                  <CListGroupItem className="d-flex justify-content-between align-items-start">
                    <div className="ms-2 me-auto">
                      <div className="fw-bold">Audit Completed</div>
                      <small className="text-muted">
                        Completed on {formatDateTime(audit.completedDate)}
                      </small>
                    </div>
                    <CBadge color="success" pill>
                      <FontAwesomeIcon icon={faCheckCircle} />
                    </CBadge>
                  </CListGroupItem>
                )}

                {audit.lastModifiedAt && audit.lastModifiedAt !== audit.createdAt && (
                  <CListGroupItem className="d-flex justify-content-between align-items-start">
                    <div className="ms-2 me-auto">
                      <div className="fw-bold">Audit Modified</div>
                      <small className="text-muted">
                        Last modified by {audit.lastModifiedBy} on {formatDateTime(audit.lastModifiedAt)}
                      </small>
                    </div>
                    <CBadge color="info" pill>
                      <FontAwesomeIcon icon={faEdit} />
                    </CBadge>
                  </CListGroupItem>
                )}
              </CListGroup>
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>
    </div>
  );
};

export default AuditDetail;