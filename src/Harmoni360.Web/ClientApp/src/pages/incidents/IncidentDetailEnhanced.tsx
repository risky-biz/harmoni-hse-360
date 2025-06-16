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
  faAmbulance,
  faEye,
  faFileAlt,
  faPrint,
  faDownload,
  faShare,
} from '@fortawesome/free-solid-svg-icons';
import {
  useGetIncidentDetailQuery,
  useDeleteIncidentMutation,
  useAddInvolvedPersonMutation,
  useUpdateInvolvedPersonMutation,
  useRemoveInvolvedPersonMutation,
  useUpdateIncidentStatusMutation,
} from '../../features/incidents/incidentApi';
import { InvolvedPersonsModal } from '../../features/incidents/components/InvolvedPersonsModal';
import CorrectiveActionsManager from '../../components/common/CorrectiveActionsManager';
import AttachmentManager from '../../components/common/AttachmentManager';
import IncidentAuditTrail from '../../components/common/IncidentAuditTrail';
import StatusTransition from '../../components/incidents/StatusTransition';
import {
  getSeverityBadge,
  getStatusBadge,
  formatDate,
} from '../../utils/incidentUtils';

const IncidentDetailEnhanced: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('overview');
  const [showInvolvedPersonsModal, setShowInvolvedPersonsModal] = useState(false);
  const [showAttachmentsModal, setShowAttachmentsModal] = useState(false);
  const [showCorrectiveActionsModal, setShowCorrectiveActionsModal] = useState(false);

  const {
    data: incident,
    error,
    isLoading,
  } = useGetIncidentDetailQuery(Number(id));
  
  const [deleteIncident, { isLoading: isDeleting }] = useDeleteIncidentMutation();
  const [updateStatus] = useUpdateIncidentStatusMutation();
  const [addInvolvedPerson] = useAddInvolvedPersonMutation();
  const [updateInvolvedPerson] = useUpdateInvolvedPersonMutation();
  const [removeInvolvedPerson] = useRemoveInvolvedPersonMutation();

  const handleStatusChange = async (newStatus: string, comment: string) => {
    await updateStatus({
      id: Number(id),
      status: newStatus,
      comment,
    }).unwrap();
  };

  const handleDelete = async () => {
    if (window.confirm('Are you sure you want to delete this incident? This action cannot be undone.')) {
      try {
        await deleteIncident(Number(id)).unwrap();
        navigate('/incidents');
      } catch (error) {
        console.error('Failed to delete incident:', error);
        alert('Failed to delete incident. Please try again.');
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

  if (error || !incident) {
    return (
      <CAlert color="danger">
        <h4 className="alert-heading">Unable to load incident</h4>
        <p>The incident you're looking for could not be loaded. It may have been deleted or you may not have permission to view it.</p>
        <hr />
        <CButton color="danger" variant="outline" onClick={() => navigate('/incidents')}>
          <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
          Back to Incidents
        </CButton>
      </CAlert>
    );
  }

  return (
    <>
      {/* Breadcrumb */}
      <CBreadcrumb className="mb-4">
        <CBreadcrumbItem onClick={() => navigate('/')}>
          <FontAwesomeIcon icon={faHome} className="me-1" />
          Dashboard
        </CBreadcrumbItem>
        <CBreadcrumbItem onClick={() => navigate('/incidents')}>
          Incidents
        </CBreadcrumbItem>
        <CBreadcrumbItem active>
          Incident #{incident.id}
        </CBreadcrumbItem>
      </CBreadcrumb>

      {/* Header */}
      <CRow className="mb-4">
        <CCol>
          <div className="d-flex justify-content-between align-items-start mb-4">
            <div>
              <h1 className="h3 mb-2">{incident.title}</h1>
              <div className="d-flex align-items-center gap-3 text-muted">
                <span>
                  <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                  {formatDate(incident.incidentDate)}
                </span>
                <span>
                  <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                  {incident.location}
                </span>
                <span>ID: #{incident.id}</span>
              </div>
            </div>
            
            <div className="d-flex gap-2">
              <CButtonGroup>
                <CButton
                  color="primary"
                  variant="outline"
                  onClick={() => navigate('/incidents')}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                  Back
                </CButton>
                <CButton
                  color="primary"
                  onClick={() => navigate(`/incidents/${id}/edit`)}
                >
                  <FontAwesomeIcon icon={faEdit} className="me-2" />
                  Edit
                </CButton>
              </CButtonGroup>

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
                  <CDropdownDivider />
                  <CDropdownItem 
                    className="text-danger"
                    onClick={handleDelete}
                    disabled={isDeleting}
                  >
                    <FontAwesomeIcon icon={faTrash} className="me-2" />
                    {isDeleting ? 'Deleting...' : 'Delete Incident'}
                  </CDropdownItem>
                </CDropdownMenu>
              </CDropdown>
            </div>
          </div>

          {/* Status and Severity Badges */}
          <div className="d-flex align-items-center gap-3 mb-4">
            <div className="d-flex align-items-center gap-2">
              <span className="text-muted">Status:</span>
              {getStatusBadge(incident.status)}
              <StatusTransition
                currentStatus={incident.status}
                incidentId={incident.id}
                onStatusChange={handleStatusChange}
              />
            </div>
            <div className="d-flex align-items-center gap-2">
              <span className="text-muted">Severity:</span>
              {getSeverityBadge(incident.severity)}
            </div>
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
                <FontAwesomeIcon icon={faFileAlt} className="me-2" />
                Overview
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'people'}
                onClick={() => setActiveTab('people')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faUsers} className="me-2" />
                People ({incident.involvedPersons?.length || 0})
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'attachments'}
                onClick={() => setActiveTab('attachments')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faPaperclip} className="me-2" />
                Attachments ({incident.attachmentsCount || 0})
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'actions'}
                onClick={() => setActiveTab('actions')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faTasks} className="me-2" />
                Actions ({incident.correctiveActionsCount || 0})
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink
                active={activeTab === 'history'}
                onClick={() => setActiveTab('history')}
                style={{ cursor: 'pointer' }}
              >
                <FontAwesomeIcon icon={faHistory} className="me-2" />
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
                <CCol lg={8}>
                  {/* Description */}
                  <div className="mb-5">
                    <h5 className="mb-3">Description</h5>
                    <p className="text-body">{incident.description}</p>
                  </div>

                  {/* Incident Details Grid */}
                  <div className="mb-5">
                    <h5 className="mb-3">Incident Details</h5>
                    <CRow className="g-4">
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faExclamationTriangle} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Category</small>
                            <span>{incident.category || 'Not specified'}</span>
                          </div>
                        </div>
                      </CCol>
                      <CCol sm={6}>
                        <div className="d-flex">
                          <FontAwesomeIcon icon={faBuilding} className="text-muted me-3 mt-1" />
                          <div>
                            <small className="text-muted d-block">Department</small>
                            <span>{incident.department || 'Not specified'}</span>
                          </div>
                        </div>
                      </CCol>
                    </CRow>
                  </div>

                  {/* Medical Information */}
                  {(incident.injuryType || incident.medicalTreatmentProvided || incident.emergencyServicesContacted) && (
                    <div className="mb-5">
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faAmbulance} className="me-2 text-danger" />
                        Medical Information
                      </h5>
                      <CCard className="border">
                        <CCardBody>
                          <CRow className="g-4">
                            {incident.injuryType && (
                              <CCol sm={12}>
                                <div className="d-flex justify-content-between align-items-center pb-3 border-bottom">
                                  <span className="text-muted">Injury Type</span>
                                  <span className="fw-semibold">{incident.injuryType}</span>
                                </div>
                              </CCol>
                            )}
                            <CCol sm={6}>
                              <div className="d-flex justify-content-between align-items-center">
                                <span className="text-muted">Medical Treatment Provided</span>
                                {incident.medicalTreatmentProvided ? (
                                  <CBadge color="success">
                                    <FontAwesomeIcon icon={faCheckCircle} className="me-1" />
                                    Yes
                                  </CBadge>
                                ) : (
                                  <CBadge color="secondary">
                                    <FontAwesomeIcon icon={faTimesCircle} className="me-1" />
                                    No
                                  </CBadge>
                                )}
                              </div>
                            </CCol>
                            <CCol sm={6}>
                              <div className="d-flex justify-content-between align-items-center">
                                <span className="text-muted">Emergency Services</span>
                                {incident.emergencyServicesContacted ? (
                                  <CBadge color="danger">
                                    <FontAwesomeIcon icon={faAmbulance} className="me-1" />
                                    Contacted
                                  </CBadge>
                                ) : (
                                  <CBadge color="secondary">
                                    Not Required
                                  </CBadge>
                                )}
                              </div>
                            </CCol>
                          </CRow>
                        </CCardBody>
                      </CCard>
                    </div>
                  )}

                  {/* Witness Information */}
                  {incident.witnessNames && (
                    <div className="mb-5">
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faEye} className="me-2" />
                        Witness Information
                      </h5>
                      <CCard className="border bg-light">
                        <CCardBody>
                          <p className="mb-0">{incident.witnessNames}</p>
                        </CCardBody>
                      </CCard>
                    </div>
                  )}

                  {/* Immediate Actions */}
                  {incident.immediateActionsTaken && (
                    <div className="mb-5">
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faTasks} className="me-2" />
                        Immediate Actions Taken
                      </h5>
                      <CCard className="border bg-light">
                        <CCardBody>
                          <p className="mb-0">{incident.immediateActionsTaken}</p>
                        </CCardBody>
                      </CCard>
                    </div>
                  )}
                </CCol>

                <CCol lg={4}>
                  {/* Reporter Information */}
                  <CCard className="mb-4 border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">Reporter Information</h6>
                    </CCardHeader>
                    <CCardBody>
                      <div className="d-flex align-items-start mb-3">
                        <FontAwesomeIcon icon={faUser} className="text-muted me-3 mt-1" />
                        <div>
                          <small className="text-muted d-block">Name</small>
                          <span className="fw-semibold">{incident.reporterName}</span>
                        </div>
                      </div>
                      <div className="d-flex align-items-start mb-3">
                        <FontAwesomeIcon icon={faEnvelope} className="text-muted me-3 mt-1" />
                        <div>
                          <small className="text-muted d-block">Email</small>
                          <span>{incident.reporterEmail}</span>
                        </div>
                      </div>
                      <div className="d-flex align-items-start">
                        <FontAwesomeIcon icon={faBuilding} className="text-muted me-3 mt-1" />
                        <div>
                          <small className="text-muted d-block">Department</small>
                          <span>{incident.reporterDepartment}</span>
                        </div>
                      </div>
                    </CCardBody>
                  </CCard>

                  {/* Quick Actions */}
                  <CCard className="mb-4 border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">Quick Actions</h6>
                    </CCardHeader>
                    <CCardBody>
                      <div className="d-grid gap-2">
                        <CButton
                          color="primary"
                          variant="outline"
                          onClick={() => setShowInvolvedPersonsModal(true)}
                        >
                          <FontAwesomeIcon icon={faUsers} className="me-2" />
                          Manage Involved Persons
                        </CButton>
                        <CButton
                          color="primary"
                          variant="outline"
                          onClick={() => setShowAttachmentsModal(true)}
                        >
                          <FontAwesomeIcon icon={faPaperclip} className="me-2" />
                          Manage Attachments
                        </CButton>
                        <CButton
                          color="primary"
                          variant="outline"
                          onClick={() => setShowCorrectiveActionsModal(true)}
                        >
                          <FontAwesomeIcon icon={faTasks} className="me-2" />
                          Manage Corrective Actions
                        </CButton>
                      </div>
                    </CCardBody>
                  </CCard>

                  {/* Audit Information */}
                  <CCard className="border">
                    <CCardHeader className="bg-light">
                      <h6 className="mb-0">Audit Information</h6>
                    </CCardHeader>
                    <CCardBody className="small">
                      <div className="mb-3">
                        <FontAwesomeIcon icon={faClock} className="text-muted me-2" />
                        <span className="text-muted">Created:</span>
                        <div className="ms-4 mt-1">
                          <div>{formatDate(incident.createdAt)}</div>
                          {incident.createdBy && (
                            <div className="text-muted">by {incident.createdBy}</div>
                          )}
                        </div>
                      </div>
                      {incident.lastModifiedAt && (
                        <div>
                          <FontAwesomeIcon icon={faClock} className="text-muted me-2" />
                          <span className="text-muted">Last Modified:</span>
                          <div className="ms-4 mt-1">
                            <div>{formatDate(incident.lastModifiedAt)}</div>
                            {incident.lastModifiedBy && (
                              <div className="text-muted">by {incident.lastModifiedBy}</div>
                            )}
                          </div>
                        </div>
                      )}
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>
            </CTabPane>

            {/* People Tab */}
            <CTabPane visible={activeTab === 'people'}>
              <InvolvedPersonsModal
                visible={true}
                onClose={() => {}}
                incidentId={incident.id}
                involvedPersons={incident.involvedPersons || []}
                onAdd={async (personId, involvementType, injuryDescription, manualPersonData) => {
                  try {
                    if (manualPersonData) {
                      // For manual entries, send 0 as PersonId and include manual person data
                      await addInvolvedPerson({
                        incidentId: incident.id,
                        data: { 
                          personId: 0, 
                          involvementType, 
                          injuryDescription,
                        },
                      }).unwrap();
                    } else {
                      // For existing users, ensure personId is a number
                      const numericPersonId = typeof personId === 'string' ? parseInt(personId, 10) : (typeof personId === 'number' ? personId : 0);
                      if (numericPersonId <= 0) {
                        throw new Error('Invalid person ID for existing user selection');
                      }
                      await addInvolvedPerson({
                        incidentId: incident.id,
                        data: { personId: numericPersonId, involvementType, injuryDescription },
                      }).unwrap();
                    }
                  } catch (error) {
                    console.error('Error in onAdd handler:', error);
                    throw error; // Re-throw to let modal handle it
                  }
                }}
                onUpdate={async (personId, involvementType, injuryDescription) => {
                  const numericPersonId = typeof personId === 'string' ? parseInt(personId, 10) : (typeof personId === 'number' ? personId : 0);
                  if (numericPersonId <= 0) {
                    throw new Error('Invalid person ID for update operation');
                  }
                  await updateInvolvedPerson({
                    incidentId: incident.id,
                    personId: numericPersonId,
                    data: { involvementType, injuryDescription },
                  }).unwrap();
                }}
                onRemove={async (personId) => {
                  const numericPersonId = typeof personId === 'string' ? parseInt(personId, 10) : (typeof personId === 'number' ? personId : 0);
                  if (numericPersonId <= 0) {
                    throw new Error('Invalid person ID for update operation');
                  }
                  await removeInvolvedPerson({
                    incidentId: incident.id,
                    personId: numericPersonId,
                  }).unwrap();
                }}
                embedded={true}
              />
            </CTabPane>

            {/* Attachments Tab */}
            <CTabPane visible={activeTab === 'attachments'}>
              <AttachmentManager
                incidentId={incident.id}
                allowUpload={true}
                allowDelete={true}
              />
            </CTabPane>

            {/* Actions Tab */}
            <CTabPane visible={activeTab === 'actions'}>
              <CorrectiveActionsManager
                incidentId={incident.id}
                allowEdit={true}
              />
            </CTabPane>

            {/* History Tab */}
            <CTabPane visible={activeTab === 'history'}>
              <IncidentAuditTrail incidentId={incident.id} />
            </CTabPane>
          </CTabContent>
        </CCardBody>
      </CCard>

      {/* Modals */}
      <CModal
        visible={showAttachmentsModal}
        onClose={() => setShowAttachmentsModal(false)}
        size="lg"
      >
        <CModalHeader>
          <CModalTitle>Manage Attachments</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <AttachmentManager
            incidentId={incident.id}
            allowUpload={true}
            allowDelete={true}
          />
        </CModalBody>
      </CModal>

      <CModal
        visible={showCorrectiveActionsModal}
        onClose={() => setShowCorrectiveActionsModal(false)}
        size="xl"
      >
        <CModalHeader>
          <CModalTitle>Manage Corrective Actions</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CorrectiveActionsManager
            incidentId={incident.id}
            allowEdit={true}
          />
        </CModalBody>
      </CModal>

      {incident && activeTab !== 'people' && (
        <InvolvedPersonsModal
          visible={showInvolvedPersonsModal}
          onClose={() => setShowInvolvedPersonsModal(false)}
          incidentId={incident.id}
          involvedPersons={incident.involvedPersons || []}
          onAdd={async (personId, involvementType, injuryDescription, manualPersonData) => {
            try {
              if (manualPersonData) {
                // For manual entries, send 0 as PersonId and include manual person data
                await addInvolvedPerson({
                  incidentId: incident.id,
                  data: { 
                    personId: 0, 
                    involvementType, 
                    injuryDescription,
                  },
                }).unwrap();
              } else {
                // For existing users, ensure personId is a number
                const numericPersonId = typeof personId === 'string' ? parseInt(personId, 10) : (typeof personId === 'number' ? personId : 0);
                if (numericPersonId <= 0) {
                  throw new Error('Invalid person ID for existing user selection');
                }
                await addInvolvedPerson({
                  incidentId: incident.id,
                  data: { personId: numericPersonId, involvementType, injuryDescription },
                }).unwrap();
              }
            } catch (error) {
              console.error('Error in onAdd handler:', error);
              throw error; // Re-throw to let modal handle it
            }
          }}
          onUpdate={async (personId, involvementType, injuryDescription) => {
            const numericPersonId = typeof personId === 'string' ? parseInt(personId, 10) : personId;
            await updateInvolvedPerson({
              incidentId: incident.id,
              personId: numericPersonId,
              data: { involvementType, injuryDescription },
            }).unwrap();
          }}
          onRemove={async (personId) => {
            const numericPersonId = typeof personId === 'string' ? parseInt(personId, 10) : personId;
            await removeInvolvedPerson({
              incidentId: incident.id,
              personId: numericPersonId,
            }).unwrap();
          }}
        />
      )}
    </>
  );
};

export default IncidentDetailEnhanced;