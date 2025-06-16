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
  CModalFooter,
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
  CTableBody,
  CTableRow,
  CTableHeaderCell,
  CTableDataCell,
  CProgress,
  CProgressBar,
  CFormTextarea,
  CFormLabel,
  CFormInput,
  CFormSelect,
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
  faExclamationCircle,
  faInfoCircle,
  faCheck,
  faWrench,
  faCertificate,
  faSearchPlus,
  faPlus,
  faComment,
  faImage,
  faFilePdf,
  faFileWord,
  faFileExcel,
  faChevronRight,
  faFlag,
  faIndustry,
  faThermometerHalf,
  faRuler,
  faEye
} from '@fortawesome/free-solid-svg-icons';
import {
  useGetInspectionByIdQuery,
  useStartInspectionMutation,
  useCompleteInspectionMutation,
  useAddCommentMutation,
  useAddFindingMutation,
  useUpdateFindingMutation,
  useCloseFindingMutation
} from '../../features/inspections/inspectionApi';
import { InspectionDetailDto, InspectionStatus, InspectionPriority, InspectionType, FindingStatus, FindingSeverity, InspectionAttachmentDto } from '../../types/inspection';
import DemoModeWrapper from '../../components/common/DemoModeWrapper';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { format, formatDistanceToNow } from 'date-fns';
import { toast } from 'react-toastify';
import InspectionAttachmentManager from '../../components/inspections/AttachmentManager';

export const InspectionDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('overview');
  const [showAddFindingModal, setShowAddFindingModal] = useState(false);
  const [showCompleteModal, setShowCompleteModal] = useState(false);
  const [completionSummary, setCompletionSummary] = useState('');
  const [completionRecommendations, setCompletionRecommendations] = useState('');

  // API calls
  const { data: inspection, isLoading, error, refetch } = useGetInspectionByIdQuery(Number(id));
  const [startInspection] = useStartInspectionMutation();
  const [completeInspection] = useCompleteInspectionMutation();
  const [addComment] = useAddCommentMutation();
  const [addFinding] = useAddFindingMutation();

  const handleStartInspection = async () => {
    if (window.confirm('Are you sure you want to start this inspection?')) {
      try {
        await startInspection(Number(id)).unwrap();
        toast.success('Inspection started successfully');
        refetch();
      } catch (error: any) {
        console.error('Failed to start inspection:', error);
        toast.error(error?.data?.message || 'Failed to start inspection');
      }
    }
  };

  const handleCompleteInspection = async () => {
    if (!completionSummary.trim()) {
      toast.error('Please provide a summary before completing the inspection');
      return;
    }

    try {
      await completeInspection({
        inspectionId: Number(id),
        summary: completionSummary,
        recommendations: completionRecommendations,
        findings: [] // Findings are already added through the interface
      }).unwrap();
      
      toast.success('Inspection completed successfully');
      setShowCompleteModal(false);
      setCompletionSummary('');
      setCompletionRecommendations('');
      refetch();
    } catch (error: any) {
      console.error('Failed to complete inspection:', error);
      toast.error(error?.data?.message || 'Failed to complete inspection');
    }
  };

  const getStatusBadge = (status: InspectionStatus) => {
    const statusConfig = {
      [InspectionStatus.Draft]: { color: 'secondary', text: 'Draft' },
      [InspectionStatus.Scheduled]: { color: 'info', text: 'Scheduled' },
      [InspectionStatus.InProgress]: { color: 'warning', text: 'In Progress' },
      [InspectionStatus.Completed]: { color: 'success', text: 'Completed' },
      [InspectionStatus.Cancelled]: { color: 'danger', text: 'Cancelled' }
    };
    const config = statusConfig[status] || { color: 'secondary', text: status };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  };

  const getPriorityBadge = (priority: InspectionPriority) => {
    const priorityConfig = {
      [InspectionPriority.Low]: { color: 'success', text: 'Low' },
      [InspectionPriority.Medium]: { color: 'warning', text: 'Medium' },
      [InspectionPriority.High]: { color: 'danger', text: 'High' },
      [InspectionPriority.Critical]: { color: 'dark', text: 'Critical' }
    };
    const config = priorityConfig[priority] || { color: 'secondary', text: priority };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  };

  const getSeverityBadge = (severity: FindingSeverity) => {
    const severityConfig = {
      [FindingSeverity.Minor]: { color: 'info', text: 'Minor' },
      [FindingSeverity.Major]: { color: 'warning', text: 'Major' },
      [FindingSeverity.Critical]: { color: 'danger', text: 'Critical' },
      [FindingSeverity.Catastrophic]: { color: 'dark', text: 'Catastrophic' }
    };
    const config = severityConfig[severity] || { color: 'secondary', text: severity };
    return <CBadge color={config.color}>{config.text}</CBadge>;
  };

  const getItemTypeIcon = (type: string) => {
    const iconMap: { [key: string]: any } = {
      'YesNo': faCheck,
      'Text': faFileAlt,
      'Number': faRuler,
      'MultipleChoice': faCheckCircle,
      'Measurement': faThermometerHalf,
      'Visual': faEye
    };
    return iconMap[type] || faInfoCircle;
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner className="text-primary" />
        <span className="ms-2">Loading inspection details...</span>
      </div>
    );
  }

  if (error || !inspection) {
    return (
      <CAlert color="danger">
        <h4 className="alert-heading">Unable to load inspection</h4>
        <p>The inspection you're looking for could not be loaded. It may have been deleted or you may not have permission to view it.</p>
        <CButton color="primary" onClick={() => navigate('/inspections')}>
          <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
          Back to Inspections
        </CButton>
      </CAlert>
    );
  }

  return (
    <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Read}>
      <DemoModeWrapper>
        {/* Breadcrumb */}
        <CBreadcrumb className="mb-4">
          <CBreadcrumbItem>
            <FontAwesomeIcon icon={faHome} className="me-1" />
            Home
          </CBreadcrumbItem>
          <CBreadcrumbItem onClick={() => navigate('/inspections')} style={{ cursor: 'pointer' }}>
            Inspections
          </CBreadcrumbItem>
          <CBreadcrumbItem active>
            {inspection.inspectionNumber}
          </CBreadcrumbItem>
        </CBreadcrumb>

        <CRow>
          <CCol>
            <CCard>
              <CCardHeader className="border-bottom-0">
                <div className="d-flex justify-content-between align-items-start">
                  <div className="flex-grow-1">
                    <div className="d-flex align-items-center mb-2">
                      <h4 className="mb-0 me-3">
                        <FontAwesomeIcon icon={faClipboardCheck} className="me-2 text-primary" />
                        {inspection.title}
                      </h4>
                      {getStatusBadge(inspection.status)}
                      {getPriorityBadge(inspection.priority)}
                      {inspection.isOverdue && (
                        <CBadge color="danger" className="ms-2">
                          <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                          Overdue
                        </CBadge>
                      )}
                    </div>
                    <div className="text-muted">
                      <small>#{inspection.inspectionNumber} • Created {formatDistanceToNow(new Date(inspection.createdAt))} ago</small>
                    </div>
                  </div>
                  
                  <div className="d-flex align-items-center">
                    <CButton
                      color="light"
                      variant="outline"
                      className="me-2"
                      onClick={() => navigate('/inspections')}
                    >
                      <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
                      Back to List
                    </CButton>

                    <CDropdown>
                      <CDropdownToggle color="primary" variant="outline">
                        <FontAwesomeIcon icon={faEllipsisV} />
                      </CDropdownToggle>
                      <CDropdownMenu>
                        {inspection.canEdit && (
                          <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                            <CDropdownItem onClick={() => navigate(`/inspections/${id}/edit`)}>
                              <FontAwesomeIcon icon={faEdit} className="me-2" />
                              Edit Inspection
                            </CDropdownItem>
                          </PermissionGuard>
                        )}
                        {inspection.canStart && (
                          <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                            <CDropdownItem onClick={handleStartInspection}>
                              <FontAwesomeIcon icon={faPlay} className="me-2" />
                              Start Inspection
                            </CDropdownItem>
                          </PermissionGuard>
                        )}
                        {inspection.canComplete && (
                          <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                            <CDropdownItem onClick={() => setShowCompleteModal(true)}>
                              <FontAwesomeIcon icon={faCheck} className="me-2" />
                              Complete Inspection
                            </CDropdownItem>
                          </PermissionGuard>
                        )}
                        <CDropdownDivider />
                        <CDropdownItem>
                          <FontAwesomeIcon icon={faPrint} className="me-2" />
                          Print Report
                        </CDropdownItem>
                        <CDropdownItem>
                          <FontAwesomeIcon icon={faDownload} className="me-2" />
                          Export PDF
                        </CDropdownItem>
                      </CDropdownMenu>
                    </CDropdown>
                  </div>
                </div>

                {/* Tab Navigation */}
                <CNav variant="tabs" className="card-header-tabs mt-3">
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
                      active={activeTab === 'checklist'}
                      onClick={() => setActiveTab('checklist')}
                      style={{ cursor: 'pointer' }}
                    >
                      <FontAwesomeIcon icon={faTasks} className="me-2" />
                      Checklist ({inspection.items?.length || 0})
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      active={activeTab === 'findings'}
                      onClick={() => setActiveTab('findings')}
                      style={{ cursor: 'pointer' }}
                    >
                      <FontAwesomeIcon icon={faSearchPlus} className="me-2" />
                      Findings ({inspection.findings?.length || 0})
                      {inspection.criticalFindingsCount > 0 && (
                        <CBadge color="danger" className="ms-1">{inspection.criticalFindingsCount}</CBadge>
                      )}
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      active={activeTab === 'attachments'}
                      onClick={() => setActiveTab('attachments')}
                      style={{ cursor: 'pointer' }}
                    >
                      <FontAwesomeIcon icon={faPaperclip} className="me-2" />
                      Attachments ({inspection.attachments?.length || 0})
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      active={activeTab === 'comments'}
                      onClick={() => setActiveTab('comments')}
                      style={{ cursor: 'pointer' }}
                    >
                      <FontAwesomeIcon icon={faComment} className="me-2" />
                      Comments ({inspection.comments?.length || 0})
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
                        {/* Basic Information */}
                        <div className="mb-5">
                          <h5 className="mb-3">Basic Information</h5>
                          <CRow className="g-4">
                            <CCol sm={6}>
                              <div className="d-flex">
                                <FontAwesomeIcon icon={faFileAlt} className="text-muted me-3 mt-1" />
                                <div>
                                  <small className="text-muted d-block">Title</small>
                                  <span className="fw-semibold">{inspection.title}</span>
                                </div>
                              </div>
                            </CCol>
                            <CCol sm={6}>
                              <div className="d-flex">
                                <FontAwesomeIcon icon={faCertificate} className="text-muted me-3 mt-1" />
                                <div>
                                  <small className="text-muted d-block">Type</small>
                                  <CBadge color="info">{inspection.typeName}</CBadge>
                                </div>
                              </div>
                            </CCol>
                            <CCol sm={6}>
                              <div className="d-flex">
                                <FontAwesomeIcon icon={faCalendarAlt} className="text-muted me-3 mt-1" />
                                <div>
                                  <small className="text-muted d-block">Scheduled Date</small>
                                  <span className="fw-semibold">
                                    {format(new Date(inspection.scheduledDate), 'MMM dd, yyyy HH:mm')}
                                  </span>
                                </div>
                              </div>
                            </CCol>
                            <CCol sm={6}>
                              <div className="d-flex">
                                <FontAwesomeIcon icon={faUser} className="text-muted me-3 mt-1" />
                                <div>
                                  <small className="text-muted d-block">Inspector</small>
                                  <span className="fw-semibold">{inspection.inspectorName}</span>
                                </div>
                              </div>
                            </CCol>
                            <CCol sm={6}>
                              <div className="d-flex">
                                <FontAwesomeIcon icon={faBuilding} className="text-muted me-3 mt-1" />
                                <div>
                                  <small className="text-muted d-block">Department</small>
                                  <span className="fw-semibold">{inspection.departmentName}</span>
                                </div>
                              </div>
                            </CCol>
                            <CCol sm={6}>
                              <div className="d-flex">
                                <FontAwesomeIcon icon={faFlag} className="text-muted me-3 mt-1" />
                                <div>
                                  <small className="text-muted d-block">Category</small>
                                  <span className="fw-semibold">{inspection.categoryName}</span>
                                </div>
                              </div>
                            </CCol>
                          </CRow>
                        </div>

                        {/* Description */}
                        <div className="mb-5">
                          <h5 className="mb-3">Description</h5>
                          <p className="text-muted">{inspection.description}</p>
                        </div>

                        {/* Progress Summary */}
                        <div className="mb-5">
                          <h5 className="mb-3">Progress Summary</h5>
                          <div className="mb-3">
                            <div className="d-flex justify-content-between align-items-center mb-2">
                              <span>Checklist Items Completed</span>
                              <span className="fw-semibold">{inspection.completedItemsCount}/{inspection.itemsCount}</span>
                            </div>
                            <CProgress className="mb-2" height={8}>
                              <CProgressBar 
                                value={(inspection.completedItemsCount / inspection.itemsCount) * 100}
                                color="primary"
                              />
                            </CProgress>
                          </div>
                          <CRow className="g-3">
                            <CCol sm={3}>
                              <div className="text-center border rounded p-3">
                                <div className="h4 mb-1 text-primary">{inspection.itemsCount}</div>
                                <small className="text-muted">Total Items</small>
                              </div>
                            </CCol>
                            <CCol sm={3}>
                              <div className="text-center border rounded p-3">
                                <div className="h4 mb-1 text-success">{inspection.completedItemsCount}</div>
                                <small className="text-muted">Completed</small>
                              </div>
                            </CCol>
                            <CCol sm={3}>
                              <div className="text-center border rounded p-3">
                                <div className="h4 mb-1 text-warning">{inspection.findingsCount}</div>
                                <small className="text-muted">Total Findings</small>
                              </div>
                            </CCol>
                            <CCol sm={3}>
                              <div className="text-center border rounded p-3">
                                <div className="h4 mb-1 text-danger">{inspection.criticalFindingsCount}</div>
                                <small className="text-muted">Critical</small>
                              </div>
                            </CCol>
                          </CRow>
                        </div>

                        {/* Summary and Recommendations (if completed) */}
                        {inspection.status === InspectionStatus.Completed && (
                          <div className="mb-5">
                            <h5 className="mb-3">Summary & Recommendations</h5>
                            {inspection.summary && (
                              <div className="mb-3">
                                <h6>Summary</h6>
                                <p className="text-muted">{inspection.summary}</p>
                              </div>
                            )}
                            {inspection.recommendations && (
                              <div>
                                <h6>Recommendations</h6>
                                <p className="text-muted">{inspection.recommendations}</p>
                              </div>
                            )}
                          </div>
                        )}
                      </CCol>

                      <CCol md={4}>
                        {/* Status Information */}
                        <CCallout color="info">
                          <h6>Status Information</h6>
                          <div className="mb-2">
                            <strong>Current Status:</strong> {getStatusBadge(inspection.status)}
                          </div>
                          <div className="mb-2">
                            <strong>Priority:</strong> {getPriorityBadge(inspection.priority)}
                          </div>
                          <div className="mb-2">
                            <strong>Risk Level:</strong> 
                            <CBadge color={inspection.riskLevel === 'Critical' ? 'danger' : inspection.riskLevel === 'High' ? 'warning' : 'info'} className="ms-2">
                              {inspection.riskLevelName}
                            </CBadge>
                          </div>
                          {inspection.estimatedDurationMinutes && (
                            <div className="mb-2">
                              <strong>Estimated Duration:</strong> {inspection.estimatedDurationMinutes} minutes
                            </div>
                          )}
                          {inspection.actualDurationMinutes && (
                            <div className="mb-2">
                              <strong>Actual Duration:</strong> {inspection.actualDurationMinutes} minutes
                            </div>
                          )}
                        </CCallout>

                        {/* Timeline */}
                        <CCallout color="light">
                          <h6>Timeline</h6>
                          <div className="timeline">
                            <div className="timeline-item">
                              <FontAwesomeIcon icon={faCalendarAlt} className="text-muted me-2" />
                              <strong>Created:</strong> {format(new Date(inspection.createdAt), 'MMM dd, yyyy HH:mm')}
                            </div>
                            {inspection.startedDate && (
                              <div className="timeline-item">
                                <FontAwesomeIcon icon={faPlay} className="text-warning me-2" />
                                <strong>Started:</strong> {format(new Date(inspection.startedDate), 'MMM dd, yyyy HH:mm')}
                              </div>
                            )}
                            {inspection.completedDate && (
                              <div className="timeline-item">
                                <FontAwesomeIcon icon={faCheckCircle} className="text-success me-2" />
                                <strong>Completed:</strong> {format(new Date(inspection.completedDate), 'MMM dd, yyyy HH:mm')}
                              </div>
                            )}
                          </div>
                        </CCallout>
                      </CCol>
                    </CRow>
                  </CTabPane>

                  {/* Checklist Tab */}
                  <CTabPane visible={activeTab === 'checklist'}>
                    <div className="d-flex justify-content-between align-items-center mb-4">
                      <h5 className="mb-0">Inspection Checklist</h5>
                      <div>
                        <span className="text-muted me-3">
                          {inspection.completedItemsCount} of {inspection.itemsCount} items completed
                        </span>
                        <CProgress className="d-inline-block" style={{ width: '100px' }} height={6}>
                          <CProgressBar 
                            value={(inspection.completedItemsCount / inspection.itemsCount) * 100}
                            color="primary"
                          />
                        </CProgress>
                      </div>
                    </div>

                    {inspection.items && inspection.items.length > 0 ? (
                      <div className="checklist-items">
                        {inspection.items.map((item, index) => (
                          <CCard key={item.id} className="mb-3">
                            <CCardBody>
                              <div className="d-flex justify-content-between align-items-start">
                                <div className="flex-grow-1">
                                  <div className="d-flex align-items-center mb-2">
                                    <FontAwesomeIcon 
                                      icon={getItemTypeIcon(item.type)} 
                                      className="text-muted me-2" 
                                    />
                                    <span className="fw-semibold">Item {index + 1}</span>
                                    <CBadge color="info" className="ms-2">{item.typeName}</CBadge>
                                    {item.isRequired && (
                                      <CBadge color="warning" className="ms-1">Required</CBadge>
                                    )}
                                  </div>
                                  <h6 className="mb-2">{item.question}</h6>
                                  {item.description && (
                                    <p className="text-muted small mb-2">{item.description}</p>
                                  )}
                                  {item.response && (
                                    <div className="mb-2">
                                      <strong>Response:</strong> 
                                      <span className="ms-2">{item.response}</span>
                                      {item.unit && <span className="text-muted"> {item.unit}</span>}
                                    </div>
                                  )}
                                  {item.notes && (
                                    <div className="mb-2">
                                      <strong>Notes:</strong> 
                                      <span className="ms-2 text-muted">{item.notes}</span>
                                    </div>
                                  )}
                                </div>
                                <div className="text-end">
                                  <CBadge 
                                    color={item.isCompleted ? 'success' : item.hasResponse ? 'warning' : 'secondary'}
                                  >
                                    {item.statusName}
                                  </CBadge>
                                  {item.isCompliant !== undefined && (
                                    <div className="mt-1">
                                      <CBadge color={item.isCompliant ? 'success' : 'danger'}>
                                        {item.isCompliant ? 'Compliant' : 'Non-Compliant'}
                                      </CBadge>
                                    </div>
                                  )}
                                </div>
                              </div>
                            </CCardBody>
                          </CCard>
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-4">
                        <FontAwesomeIcon icon={faTasks} size="3x" className="text-muted mb-3" />
                        <h5>No checklist items</h5>
                        <p className="text-muted">This inspection doesn't have any checklist items yet.</p>
                      </div>
                    )}
                  </CTabPane>

                  {/* Findings Tab */}
                  <CTabPane visible={activeTab === 'findings'}>
                    <div className="d-flex justify-content-between align-items-center mb-4">
                      <h5 className="mb-0">Inspection Findings</h5>
                      {inspection.status === InspectionStatus.InProgress && (
                        <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                          <CButton color="primary" onClick={() => setShowAddFindingModal(true)}>
                            <FontAwesomeIcon icon={faPlus} className="me-1" />
                            Add Finding
                          </CButton>
                        </PermissionGuard>
                      )}
                    </div>

                    {inspection.findings && inspection.findings.length > 0 ? (
                      <div className="findings-list">
                        {inspection.findings.map((finding) => (
                          <CCard key={finding.id} className="mb-3">
                            <CCardBody>
                              <div className="d-flex justify-content-between align-items-start mb-3">
                                <div>
                                  <h6 className="mb-1">Finding #{finding.findingNumber}</h6>
                                  <div className="mb-2">
                                    {getSeverityBadge(finding.severity)}
                                    <CBadge color="info" className="ms-2">{finding.typeName}</CBadge>
                                    <CBadge 
                                      color={finding.status === 'Open' ? 'warning' : finding.status === 'Closed' ? 'success' : 'info'} 
                                      className="ms-2"
                                    >
                                      {finding.statusName}
                                    </CBadge>
                                  </div>
                                </div>
                                <small className="text-muted">
                                  {formatDistanceToNow(new Date(finding.createdAt))} ago
                                </small>
                              </div>
                              <p className="mb-3">{finding.description}</p>
                              {finding.location && (
                                <div className="mb-2">
                                  <FontAwesomeIcon icon={faMapMarkerAlt} className="text-muted me-2" />
                                  <strong>Location:</strong> {finding.location}
                                </div>
                              )}
                              {finding.equipment && (
                                <div className="mb-2">
                                  <FontAwesomeIcon icon={faIndustry} className="text-muted me-2" />
                                  <strong>Equipment:</strong> {finding.equipment}
                                </div>
                              )}
                              {finding.rootCause && (
                                <div className="mb-2">
                                  <strong>Root Cause:</strong> {finding.rootCause}
                                </div>
                              )}
                              {finding.immediateAction && (
                                <div className="mb-2">
                                  <strong>Immediate Action:</strong> {finding.immediateAction}
                                </div>
                              )}
                              {finding.correctiveAction && (
                                <div className="mb-2">
                                  <strong>Corrective Action:</strong> {finding.correctiveAction}
                                  {finding.responsiblePersonName && (
                                    <span className="ms-2 text-muted">
                                      (Assigned to: {finding.responsiblePersonName})
                                    </span>
                                  )}
                                  {finding.dueDate && (
                                    <span className="ms-2 text-muted">
                                      (Due: {format(new Date(finding.dueDate), 'MMM dd, yyyy')})
                                    </span>
                                  )}
                                </div>
                              )}
                            </CCardBody>
                          </CCard>
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-4">
                        <FontAwesomeIcon icon={faSearchPlus} size="3x" className="text-muted mb-3" />
                        <h5>No findings recorded</h5>
                        <p className="text-muted">
                          {inspection.status === InspectionStatus.InProgress 
                            ? 'Start recording findings during your inspection.' 
                            : 'No findings were recorded for this inspection.'
                          }
                        </p>
                        {inspection.status === InspectionStatus.InProgress && (
                          <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                            <CButton color="primary" onClick={() => setShowAddFindingModal(true)}>
                              <FontAwesomeIcon icon={faPlus} className="me-1" />
                              Add First Finding
                            </CButton>
                          </PermissionGuard>
                        )}
                      </div>
                    )}
                  </CTabPane>

                  {/* Attachments Tab */}
                  <CTabPane visible={activeTab === 'attachments'}>
                    <InspectionAttachmentManager
                      inspectionId={inspection.id}
                      attachments={inspection.attachments || []}
                      allowUpload={inspection.canEdit}
                      allowDelete={inspection.canEdit}
                      allowView={true}
                    />
                  </CTabPane>

                  {/* Comments Tab */}
                  <CTabPane visible={activeTab === 'comments'}>
                    <div className="d-flex justify-content-between align-items-center mb-4">
                      <h5 className="mb-0">Comments & Notes</h5>
                      <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                        <CButton color="primary">
                          <FontAwesomeIcon icon={faPlus} className="me-1" />
                          Add Comment
                        </CButton>
                      </PermissionGuard>
                    </div>

                    {inspection.comments && inspection.comments.length > 0 ? (
                      <div className="comments-list">
                        {inspection.comments.map((comment) => (
                          <CCard key={comment.id} className="mb-3">
                            <CCardBody>
                              <div className="d-flex justify-content-between align-items-start mb-2">
                                <div className="d-flex align-items-center">
                                  <FontAwesomeIcon icon={faUser} className="text-muted me-2" />
                                  <span className="fw-semibold">{comment.userName}</span>
                                  {comment.isInternal && (
                                    <CBadge color="warning" className="ms-2">Internal</CBadge>
                                  )}
                                </div>
                                <small className="text-muted">
                                  {formatDistanceToNow(new Date(comment.createdAt))} ago
                                </small>
                              </div>
                              <p className="mb-0">{comment.comment}</p>
                              {comment.replies && comment.replies.length > 0 && (
                                <div className="mt-3 ps-3 border-start">
                                  {comment.replies.map((reply) => (
                                    <div key={reply.id} className="mb-2">
                                      <div className="d-flex justify-content-between align-items-start mb-1">
                                        <span className="fw-semibold small">{reply.userName}</span>
                                        <small className="text-muted">
                                          {formatDistanceToNow(new Date(reply.createdAt))} ago
                                        </small>
                                      </div>
                                      <p className="small mb-0">{reply.comment}</p>
                                    </div>
                                  ))}
                                </div>
                              )}
                            </CCardBody>
                          </CCard>
                        ))}
                      </div>
                    ) : (
                      <div className="text-center py-4">
                        <FontAwesomeIcon icon={faComment} size="3x" className="text-muted mb-3" />
                        <h5>No comments yet</h5>
                        <p className="text-muted">Start a discussion about this inspection.</p>
                        <PermissionGuard module={ModuleType.InspectionManagement} permission={PermissionType.Update}>
                          <CButton color="primary">
                            <FontAwesomeIcon icon={faPlus} className="me-1" />
                            Add First Comment
                          </CButton>
                        </PermissionGuard>
                      </div>
                    )}
                  </CTabPane>

                  {/* History Tab */}
                  <CTabPane visible={activeTab === 'history'}>
                    <h5 className="mb-4">Activity History</h5>
                    <div className="timeline">
                      <div className="timeline-item">
                        <div className="timeline-marker bg-primary"></div>
                        <div className="timeline-content">
                          <h6>Inspection Created</h6>
                          <p className="text-muted small mb-1">
                            Created by {inspection.createdBy} on {format(new Date(inspection.createdAt), 'MMM dd, yyyy HH:mm')}
                          </p>
                        </div>
                      </div>
                      {inspection.startedDate && (
                        <div className="timeline-item">
                          <div className="timeline-marker bg-warning"></div>
                          <div className="timeline-content">
                            <h6>Inspection Started</h6>
                            <p className="text-muted small mb-1">
                              Started on {format(new Date(inspection.startedDate), 'MMM dd, yyyy HH:mm')}
                            </p>
                          </div>
                        </div>
                      )}
                      {inspection.completedDate && (
                        <div className="timeline-item">
                          <div className="timeline-marker bg-success"></div>
                          <div className="timeline-content">
                            <h6>Inspection Completed</h6>
                            <p className="text-muted small mb-1">
                              Completed on {format(new Date(inspection.completedDate), 'MMM dd, yyyy HH:mm')}
                            </p>
                          </div>
                        </div>
                      )}
                      <div className="timeline-item">
                        <div className="timeline-marker bg-info"></div>
                        <div className="timeline-content">
                          <h6>Last Modified</h6>
                          <p className="text-muted small mb-1">
                            Modified by {inspection.lastModifiedBy} on {format(new Date(inspection.lastModifiedAt), 'MMM dd, yyyy HH:mm')}
                          </p>
                        </div>
                      </div>
                    </div>
                  </CTabPane>
                </CTabContent>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>

        {/* Complete Inspection Modal */}
        <CModal visible={showCompleteModal} onClose={() => setShowCompleteModal(false)} size="lg">
          <CModalHeader>
            <CModalTitle>Complete Inspection</CModalTitle>
          </CModalHeader>
          <CModalBody>
            <div className="mb-3">
              <CFormLabel htmlFor="completionSummary">Summary *</CFormLabel>
              <CFormTextarea
                id="completionSummary"
                rows={4}
                value={completionSummary}
                onChange={(e) => setCompletionSummary(e.target.value)}
                placeholder="Provide a summary of the inspection results..."
              />
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="completionRecommendations">Recommendations</CFormLabel>
              <CFormTextarea
                id="completionRecommendations"
                rows={3}
                value={completionRecommendations}
                onChange={(e) => setCompletionRecommendations(e.target.value)}
                placeholder="Any recommendations based on the inspection findings..."
              />
            </div>
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" onClick={() => setShowCompleteModal(false)}>
              Cancel
            </CButton>
            <CButton color="primary" onClick={handleCompleteInspection}>
              <FontAwesomeIcon icon={faCheck} className="me-1" />
              Complete Inspection
            </CButton>
          </CModalFooter>
        </CModal>
      </DemoModeWrapper>
    </PermissionGuard>
  );
};