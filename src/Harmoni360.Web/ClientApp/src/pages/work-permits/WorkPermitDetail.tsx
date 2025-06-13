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
} from '@fortawesome/free-solid-svg-icons';
import {
  useGetWorkPermitByIdQuery,
  useDeleteWorkPermitMutation,
  useStartWorkMutation,
  useCompleteWorkMutation,
  useCancelWorkPermitMutation,
} from '../../features/work-permits/workPermitApi';
import WorkPermitAttachmentManager from '../../components/work-permits/WorkPermitAttachmentManager';
import {
  getWorkPermitStatusBadge,
  getWorkPermitTypeBadge,
  getRiskLevelBadge,
  formatDate,
  isWorkPermitExpired,
  isWorkPermitExpiringSoon,
} from '../../utils/workPermitUtils';

const WorkPermitDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('basic-info');
  const [showHazardsModal, setShowHazardsModal] = useState(false);
  const [showPrecautionsModal, setShowPrecautionsModal] = useState(false);

  const {
    data: permit,
    error,
    isLoading,
  } = useGetWorkPermitByIdQuery(id!);
  
  const [deleteWorkPermit, { isLoading: isDeleting }] = useDeleteWorkPermitMutation();
  const [startWork] = useStartWorkMutation();
  const [completeWork] = useCompleteWorkMutation();
  const [cancelWorkPermit] = useCancelWorkPermitMutation();

  const handleStartWork = async () => {
    if (window.confirm('Are you sure you want to start work on this permit?')) {
      try {
        await startWork(id!).unwrap();
      } catch (error) {
        console.error('Failed to start work:', error);
        alert('Failed to start work. Please try again.');
      }
    }
  };

  const handleCompleteWork = async () => {
    if (window.confirm('Are you sure you want to mark this work as completed?')) {
      try {
        await completeWork(id!).unwrap();
      } catch (error) {
        console.error('Failed to complete work:', error);
        alert('Failed to complete work. Please try again.');
      }
    }
  };

  const handleCancelPermit = async () => {
    if (window.confirm('Are you sure you want to cancel this work permit? This action cannot be undone.')) {
      try {
        await cancelWorkPermit(id!).unwrap();
      } catch (error) {
        console.error('Failed to cancel permit:', error);
        alert('Failed to cancel permit. Please try again.');
      }
    }
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this work permit? This action cannot be undone.')) {
      try {
        await deleteWorkPermit(id!).unwrap();
        navigate('/work-permits');
      } catch (error) {
        console.error('Failed to delete work permit:', error);
        alert('Failed to delete work permit. Please try again.');
      }
    }
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner className="text-primary" />
      </div>
    );
  }

  if (error || !permit) {
    return (
      <CAlert color="danger">
        <h4 className="alert-heading">Unable to load work permit</h4>
        <p>The work permit you're looking for could not be loaded. It may have been deleted or you may not have permission to view it.</p>
        <hr />
        <CButton color="danger" variant="outline" onClick={() => navigate('/work-permits')}>
          <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
          Back to Work Permits
        </CButton>
      </CAlert>
    );
  }

  const isExpired = permit.validUntil ? isWorkPermitExpired(permit.validUntil) : false;
  const isExpiringSoon = permit.validUntil ? isWorkPermitExpiringSoon(permit.validUntil) : false;

  return (
    <>
      {/* Breadcrumb */}
      <CBreadcrumb className="mb-4">
        <CBreadcrumbItem onClick={() => navigate('/')}>
          <FontAwesomeIcon icon={faHome} className="me-1" />
          Dashboard
        </CBreadcrumbItem>
        <CBreadcrumbItem onClick={() => navigate('/work-permits')}>
          Work Permits
        </CBreadcrumbItem>
        <CBreadcrumbItem active>
          Permit #{permit.id}
        </CBreadcrumbItem>
      </CBreadcrumb>

      {/* Expiry Warning */}
      {isExpired && (
        <CCallout color="danger" className="mb-4">
          <FontAwesomeIcon icon={faExclamationCircle} className="me-2" />
          <strong>EXPIRED:</strong> This work permit expired on {formatDate(permit.validUntil)}
        </CCallout>
      )}
      {!isExpired && isExpiringSoon && (
        <CCallout color="warning" className="mb-4">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
          <strong>EXPIRING SOON:</strong> This work permit expires on {formatDate(permit.validUntil)}
        </CCallout>
      )}

      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-start mb-4">
            <div>
              <h1 className="h3 mb-2">{permit.title}</h1>
              <div className="d-flex align-items-center gap-3 text-muted">
                <span>
                  <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                  {permit.startDate ? formatDate(permit.startDate) : 'Not specified'} - {permit.endDate ? formatDate(permit.endDate) : 'Not specified'}
                </span>
                <span>
                  <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                  {permit.workLocation}
                </span>
                <span>ID: #{permit.id}</span>
              </div>
            </div>
            
            <div className="d-flex gap-2">
              <CButtonGroup>
                <CButton
                  color="primary"
                  variant="outline"
                  onClick={() => navigate('/work-permits')}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                  Back
                </CButton>
                <CButton
                  color="primary"
                  onClick={() => navigate(`/work-permits/${id}/edit`)}
                  disabled={permit.status === 'Completed' || permit.status === 'Cancelled'}
                >
                  <FontAwesomeIcon icon={faEdit} className="me-2" />
                  Edit
                </CButton>
              </CButtonGroup>

              {/* Action Buttons */}
              {permit.status === 'Approved' && (
                <CButton
                  color="success"
                  onClick={handleStartWork}
                >
                  <FontAwesomeIcon icon={faPlay} className="me-2" />
                  Start Work
                </CButton>
              )}
              {permit.status === 'InProgress' && (
                <CButton
                  color="warning"
                  onClick={handleCompleteWork}
                >
                  <FontAwesomeIcon icon={faCheckCircle} className="me-2" />
                  Complete Work
                </CButton>
              )}
              {(permit.status === 'Draft' || permit.status === 'Submitted' || permit.status === 'Approved') && (
                <CButton
                  color="danger"
                  variant="outline"
                  onClick={handleCancelPermit}
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
                    Print Permit
                  </CDropdownItem>
                  <CDropdownItem>
                    <FontAwesomeIcon icon={faDownload} className="me-2" />
                    Export PDF
                  </CDropdownItem>
                  <CDropdownItem>
                    <FontAwesomeIcon icon={faShare} className="me-2" />
                    Share
                  </CDropdownItem>
                  <CDropdownDivider />
                  <CDropdownItem 
                    className="text-danger"
                    onClick={handleDelete}
                    disabled={isDeleting}
                  >
                    <FontAwesomeIcon icon={faTrash} className="me-2" />
                    {isDeleting ? 'Deleting...' : 'Delete Permit'}
                  </CDropdownItem>
                </CDropdownMenu>
              </CDropdown>
            </div>
          </div>

          {/* Status, Type and Risk Level Badges */}
          <div className="d-flex align-items-center gap-3 mb-4">
            <div className="d-flex align-items-center gap-2">
              <span className="text-muted">Status:</span>
              {getWorkPermitStatusBadge(permit.status)}
            </div>
            <div className="d-flex align-items-center gap-2">
              <span className="text-muted">Type:</span>
              {getWorkPermitTypeBadge(permit.type)}
            </div>
            {permit.riskLevel && (
              <div className="d-flex align-items-center gap-2">
                <span className="text-muted">Risk Level:</span>
                {getRiskLevelBadge(permit.riskLevel)}
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
                active={activeTab === 'basic-info'}
                onClick={() => setActiveTab('basic-info')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faInfoCircle} className="me-2" />
                Basic Information
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'work-details'}
                onClick={() => setActiveTab('work-details')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faWrench} className="me-2" />
                Work Details
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'safety-requirements'}
                onClick={() => setActiveTab('safety-requirements')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faShieldAlt} className="me-2" />
                Safety Requirements
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'k3-compliance'}
                onClick={() => setActiveTab('k3-compliance')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faCertificate} className="me-2" />
                K3 Compliance
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'risk-assessment'}
                onClick={() => setActiveTab('risk-assessment')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                Risk Assessment
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'attachments'}
                onClick={() => setActiveTab('attachments')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faPaperclip} className="me-2" />
                Attachments ({permit.attachments?.length || 0})
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
            {/* Basic Information Tab */}
            <CTabPane visible={activeTab === 'basic-info'}>
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
                            <small className="text-muted d-block">Title</small>
                            <span className="fw-semibold">{permit.title}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faHardHat} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Work Type</small>
                            <span>{getWorkPermitTypeBadge(permit.type)}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faMapMarkerAlt} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Work Location</small>
                            <span>{permit.workLocation}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faExclamationTriangle} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Priority</small>
                            <span>{permit.priority}</span>
                          </div>
                        </div>
                      </CCol>
                    </CRow>
                  </div>

                  {/* Description */}
                  <div className="mb-5">
                    <h5 className="mb-3">Description</h5>
                    <p className="text-body">{permit.description}</p>
                  </div>

                  {/* Timeline */}
                  <div className="mb-5">
                    <h5 className="mb-3">Timeline</h5>
                    <CRow className="g-4">
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faCalendarAlt} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Planned Start Date</small>
                            <span>{formatDate(permit.startDate)}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faCalendarAlt} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Planned End Date</small>
                            <span>{formatDate(permit.endDate)}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faClock} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Valid Until</small>
                            <span>{formatDate(permit.validUntil)}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faClock} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Estimated Duration</small>
                            <span>{permit.estimatedDuration || 'Not specified'} hours</span>
                          </div>
                        </div>
                      </CCol>
                    </CRow>
                  </div>
                </CCol>

                <CCol md={4}>
                  {/* Requestor Information */}
                  <CCard className="mb-4 border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">Requestor Information</h6>
                    </CCardHeader>
                    <CCardBody>
                      <div className="d-flex align-items-start mb-3">
                        <FontAwesomeIcon icon={faUser} className="text-muted me-3 mt-1" />
                        <div>
                          <small className="text-muted d-block">Name</small>
                          <span className="fw-semibold">{permit.requestorName}</span>
                        </div>
                      </div>
                      <div className="d-flex align-items-start mb-3">
                        <FontAwesomeIcon icon={faEnvelope} className="text-muted me-3 mt-1" />
                        <div>
                          <small className="text-muted d-block">Email</small>
                          <span>{permit.requestorEmail}</span>
                        </div>
                      </div>
                      <div className="d-flex align-items-start">
                        <FontAwesomeIcon icon={faBuilding} className="text-muted me-3 mt-1" />
                        <div>
                          <small className="text-muted d-block">Department</small>
                          <span>{permit.requestorDepartment}</span>
                        </div>
                      </div>
                    </CCardBody>
                  </CCard>

                  {/* Status & Current State */}
                  <CCard className="border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">Current Status</h6>
                    </CCardHeader>
                    <CCardBody>
                      <div className="mb-3">
                        <small className="text-muted d-block">Status</small>
                        {getWorkPermitStatusBadge(permit.status)}
                      </div>
                      {permit.riskLevel && (
                        <div className="mb-3">
                          <small className="text-muted d-block">Risk Level</small>
                          {getRiskLevelBadge(permit.riskLevel)}
                        </div>
                      )}
                      <div>
                        <small className="text-muted d-block">Permit Number</small>
                        <span className="fw-semibold">#{permit.permitNumber || permit.id}</span>
                      </div>
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>
            </CTabPane>

            {/* Work Details Tab */}
            <CTabPane visible={activeTab === 'work-details'}>
              <CRow>
                <CCol md={8}>
                  {/* Work Scope */}
                  <div className="mb-5">
                    <h5 className="mb-3">Work Scope & Activities</h5>
                    <p className="text-body">{permit.workScope || permit.description}</p>
                  </div>

                  {/* Equipment & Materials */}
                  <CRow className="mb-5">
                    <CCol md={6}>
                      <h6 className="mb-3">Equipment to be Used</h6>
                      <p className="text-body">{permit.equipmentToBeUsed || 'Not specified'}</p>
                    </CCol>
                    <CCol md={6}>
                      <h6 className="mb-3">Materials Involved</h6>
                      <p className="text-body">{permit.materialsInvolved || 'Not specified'}</p>
                    </CCol>
                  </CRow>

                  {/* Special Instructions */}
                  {permit.specialInstructions && (
                    <div className="mb-5">
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2 text-warning" />
                        Special Instructions
                      </h5>
                      <CCard className="border-warning bg-light">
                        <CCardBody>
                          <p className="mb-0">{permit.specialInstructions}</p>
                        </CCardBody>
                      </CCard>
                    </div>
                  )}
                </CCol>

                <CCol md={4}>
                  {/* Personnel Information */}
                  <CCard className="mb-4 border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">Personnel & Contacts</h6>
                    </CCardHeader>
                    <CCardBody>
                      <div className="mb-3">
                        <small className="text-muted d-block">Number of Workers</small>
                        <span className="fw-semibold">{permit.numberOfWorkers || 'Not specified'}</span>
                      </div>
                      <div className="mb-3">
                        <small className="text-muted d-block">Work Supervisor</small>
                        <span>{permit.workSupervisor || 'Not assigned'}</span>
                      </div>
                      <div className="mb-3">
                        <small className="text-muted d-block">Safety Officer</small>
                        <span>{permit.safetyOfficer || 'Not assigned'}</span>
                      </div>
                      <div className="mb-3">
                        <small className="text-muted d-block">Contact Phone</small>
                        <span>{permit.contactPhone || 'Not provided'}</span>
                      </div>
                      {permit.contractorCompany && (
                        <div>
                          <small className="text-muted d-block">Contractor Company</small>
                          <span>{permit.contractorCompany}</span>
                        </div>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>
            </CTabPane>

            {/* Safety Requirements Tab */}
            <CTabPane visible={activeTab === 'safety-requirements'}>
              <div className="mb-5">
                <h5 className="mb-3">Safety Requirements</h5>
                <CRow className="g-3">
                  <CCol md={6}>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon 
                        icon={permit.requiresHotWorkPermit ? faCheckCircle : faTimesCircle} 
                        className={`me-2 ${permit.requiresHotWorkPermit ? 'text-success' : 'text-muted'}`} 
                      />
                      <span>Hot Work Permit Required</span>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon 
                        icon={permit.requiresConfinedSpaceEntry ? faCheckCircle : faTimesCircle} 
                        className={`me-2 ${permit.requiresConfinedSpaceEntry ? 'text-success' : 'text-muted'}`} 
                      />
                      <span>Confined Space Entry</span>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon 
                        icon={permit.requiresElectricalIsolation ? faCheckCircle : faTimesCircle} 
                        className={`me-2 ${permit.requiresElectricalIsolation ? 'text-success' : 'text-muted'}`} 
                      />
                      <span>Electrical Isolation Required</span>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon 
                        icon={permit.requiresHeightWork ? faCheckCircle : faTimesCircle} 
                        className={`me-2 ${permit.requiresHeightWork ? 'text-success' : 'text-muted'}`} 
                      />
                      <span>Working at Height</span>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon 
                        icon={permit.requiresFireWatch ? faCheckCircle : faTimesCircle} 
                        className={`me-2 ${permit.requiresFireWatch ? 'text-success' : 'text-muted'}`} 
                      />
                      <span>Fire Watch Required</span>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon 
                        icon={permit.requiresGasMonitoring ? faCheckCircle : faTimesCircle} 
                        className={`me-2 ${permit.requiresGasMonitoring ? 'text-success' : 'text-muted'}`} 
                      />
                      <span>Gas Monitoring Required</span>
                    </div>
                  </CCol>
                </CRow>
              </div>

              {/* Emergency Procedures */}
              {permit.emergencyProcedures && (
                <div className="mb-5">
                  <h5 className="mb-3">
                    <FontAwesomeIcon icon={faExclamationTriangle} className="me-2 text-danger" />
                    Emergency Procedures
                  </h5>
                  <CCard className="border-danger bg-light">
                    <CCardBody>
                      <p className="mb-0">{permit.emergencyProcedures}</p>
                    </CCardBody>
                  </CCard>
                </div>
              )}

              {/* Hazards and Precautions Summary */}
              <CRow>
                <CCol md={6}>
                  <CCard className="border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">
                        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                        Identified Hazards
                      </h6>
                    </CCardHeader>
                    <CCardBody>
                      <div className="text-center">
                        <span className="h4 text-warning">{permit.hazards?.length || 0}</span>
                        <p className="text-muted mb-0">Total Hazards</p>
                      </div>
                    </CCardBody>
                  </CCard>
                </CCol>
                <CCol md={6}>
                  <CCard className="border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">
                        <FontAwesomeIcon icon={faShieldAlt} className="me-2" />
                        Safety Precautions
                      </h6>
                    </CCardHeader>
                    <CCardBody>
                      <div className="text-center">
                        <span className="h4 text-success">{permit.precautions?.length || 0}</span>
                        <p className="text-muted mb-0">Total Precautions</p>
                      </div>
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>
            </CTabPane>

            {/* K3 Compliance Tab */}
            <CTabPane visible={activeTab === 'k3-compliance'}>
              <div className="mb-5">
                <h5 className="mb-3">K3 Compliance Information</h5>
                <CRow className="g-4">
                  <CCol md={6}>
                    <div className="d-flex">
                      <FontAwesomeIcon icon={faCertificate} className="text-muted me-3 mt-1" />
                      <div>
                        <small className="text-muted d-block">K3 License Number</small>
                        <span className="fw-semibold">{permit.k3LicenseNumber || 'Not provided'}</span>
                      </div>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="d-flex">
                      <FontAwesomeIcon icon={faFileAlt} className="text-muted me-3 mt-1" />
                      <div>
                        <small className="text-muted d-block">Company Work Permit Number</small>
                        <span className="fw-semibold">{permit.companyWorkPermitNumber || 'Not provided'}</span>
                      </div>
                    </div>
                  </CCol>
                </CRow>
              </div>

              <div className="mb-5">
                <h5 className="mb-3">Compliance Status</h5>
                <CRow className="g-3">
                  <CCol md={6}>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon 
                        icon={permit.isJamsostekCompliant ? faCheckCircle : faTimesCircle} 
                        className={`me-2 ${permit.isJamsostekCompliant ? 'text-success' : 'text-danger'}`} 
                      />
                      <span>Jamsostek Compliant</span>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon 
                        icon={permit.hasSMK3Compliance ? faCheckCircle : faTimesCircle} 
                        className={`me-2 ${permit.hasSMK3Compliance ? 'text-success' : 'text-danger'}`} 
                      />
                      <span>SMK3 Compliance</span>
                    </div>
                  </CCol>
                </CRow>
              </div>

              {permit.environmentalPermitNumber && (
                <div className="mb-5">
                  <h5 className="mb-3">Environmental Permits</h5>
                  <div className="d-flex">
                    <FontAwesomeIcon icon={faFileAlt} className="text-muted me-3 mt-1" />
                    <div>
                      <small className="text-muted d-block">Environmental Permit Number</small>
                      <span className="fw-semibold">{permit.environmentalPermitNumber}</span>
                    </div>
                  </div>
                </div>
              )}
            </CTabPane>

            {/* Risk Assessment Tab */}
            <CTabPane visible={activeTab === 'risk-assessment'}>
              <div className="mb-5">
                <h5 className="mb-3">Risk Assessment Overview</h5>
                <CRow>
                  <CCol md={8}>
                    {permit.riskAssessmentSummary ? (
                      <div className="mb-4">
                        <h6 className="mb-3">Risk Assessment Summary</h6>
                        <p className="text-body">{permit.riskAssessmentSummary}</p>
                      </div>
                    ) : (
                      <CCard className="border-warning bg-light">
                        <CCardBody>
                          <p className="mb-0 text-warning">
                            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                            No risk assessment summary provided
                          </p>
                        </CCardBody>
                      </CCard>
                    )}
                  </CCol>
                  <CCol md={4}>
                    <CCard className="border">
                      <CCardHeader className="bg-light">
                        <h6 className="mb-0">Overall Risk Level</h6>
                      </CCardHeader>
                      <CCardBody className="text-center">
                        {permit.riskLevel ? getRiskLevelBadge(permit.riskLevel) : 'Not assessed'}
                      </CCardBody>
                    </CCard>
                  </CCol>
                </CRow>
              </div>

              {/* Hazards and Precautions Detail */}
              <CRow>
                <CCol md={6}>
                  <h6 className="mb-3">
                    <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                    Identified Hazards ({permit.hazards?.length || 0})
                  </h6>
                  {permit.hazards && permit.hazards.length > 0 ? (
                    <div className="text-center text-muted py-4">
                      <p>Hazards will be displayed here when backend integration is complete</p>
                    </div>
                  ) : (
                    <CCard className="border-dashed">
                      <CCardBody className="text-center text-muted">
                        <FontAwesomeIcon icon={faExclamationTriangle} size="2x" className="mb-2 opacity-50" />
                        <p className="mb-0">No hazards identified</p>
                      </CCardBody>
                    </CCard>
                  )}
                </CCol>
                <CCol md={6}>
                  <h6 className="mb-3">
                    <FontAwesomeIcon icon={faShieldAlt} className="me-2" />
                    Safety Precautions ({permit.precautions?.length || 0})
                  </h6>
                  {permit.precautions && permit.precautions.length > 0 ? (
                    <div className="text-center text-muted py-4">
                      <p>Precautions will be displayed here when backend integration is complete</p>
                    </div>
                  ) : (
                    <CCard className="border-dashed">
                      <CCardBody className="text-center text-muted">
                        <FontAwesomeIcon icon={faShieldAlt} size="2x" className="mb-2 opacity-50" />
                        <p className="mb-0">No precautions defined</p>
                      </CCardBody>
                    </CCard>
                  )}
                </CCol>
              </CRow>
            </CTabPane>

            {/* Attachments Tab */}
            <CTabPane visible={activeTab === 'attachments'}>
              <WorkPermitAttachmentManager
                workPermitId={id}
                attachments={permit.attachments || []}
                allowUpload={true}
                allowDelete={true}
              />
            </CTabPane>

            {/* Activity History Tab */}
            <CTabPane visible={activeTab === 'history'}>
              <CRow>
                <CCol md={8}>
                  <div className="text-center text-muted py-4">
                    <FontAwesomeIcon
                      icon={faHistory}
                      size="2x"
                      className="mb-2 opacity-50"
                    />
                    <p className="mb-0">Audit trail functionality coming soon</p>
                    <small>This will display the complete history of changes to this work permit</small>
                  </div>
                </CCol>
                <CCol md={4}>
                  {/* Quick Audit Info */}
                  <CCard className="border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">Audit Information</h6>
                    </CCardHeader>
                    <CCardBody className="small">
                      <div className="mb-3">
                        <FontAwesomeIcon icon={faClock} className="text-muted me-2" />
                        <span className="text-muted">Created:</span>
                        <div className="ms-4 mt-1">
                          <div>{formatDate(permit.createdAt)}</div>
                          {permit.createdBy && (
                            <div className="text-muted">by {permit.createdBy}</div>
                          )}
                        </div>
                      </div>
                      {permit.lastModifiedAt && (
                        <div>
                          <FontAwesomeIcon icon={faClock} className="text-muted me-2" />
                          <span className="text-muted">Last Modified:</span>
                          <div className="ms-4 mt-1">
                            <div>{formatDate(permit.lastModifiedAt)}</div>
                            {permit.lastModifiedBy && (
                              <div className="text-muted">by {permit.lastModifiedBy}</div>
                            )}
                          </div>
                        </div>
                      )}
                    </CCardBody>
                  </CCard>

                  {/* Approval History */}
                  {permit.approvals && permit.approvals.length > 0 && (
                    <CCard className="mt-4 border">
                      <CCardHeader className="bg-light">
                        <h6 className="mb-0">Approval History</h6>
                      </CCardHeader>
                      <CCardBody>
                        {permit.approvals.map((approval, index) => (
                          <div key={index} className="mb-3">
                            <div className="d-flex align-items-center justify-content-between mb-1">
                              <small className="text-muted">{approval.approverRole}</small>
                              <CBadge color={approval.status === 'Approved' ? 'success' : 'warning'}>
                                {approval.status}
                              </CBadge>
                            </div>
                            <div className="fw-semibold">{approval.approverName}</div>
                            {approval.approvedAt && (
                              <small className="text-muted">{formatDate(approval.approvedAt)}</small>
                            )}
                          </div>
                        ))}
                      </CCardBody>
                    </CCard>
                  )}
                </CCol>
              </CRow>
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>
    </>
  );
};

export default WorkPermitDetail;