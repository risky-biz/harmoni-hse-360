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
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import { Icon } from '../../components/common/Icon';
import {
  useGetIncidentDetailQuery,
  useDeleteIncidentMutation,
  useAddInvolvedPersonMutation,
  useUpdateInvolvedPersonMutation,
  useRemoveInvolvedPersonMutation,
} from '../../features/incidents/incidentApi';
import { InvolvedPersonsModal } from '../../features/incidents/components/InvolvedPersonsModal';
import CorrectiveActionsManager from '../../components/common/CorrectiveActionsManager';
import AttachmentManager from '../../components/common/AttachmentManager';
import IncidentAuditTrail from '../../components/common/IncidentAuditTrail';
import {
  getSeverityBadge,
  getStatusBadge,
  formatDate,
} from '../../utils/incidentUtils';

const IncidentDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showInvolvedPersonsModal, setShowInvolvedPersonsModal] =
    useState(false);
  const [showAttachmentsModal, setShowAttachmentsModal] = useState(false);
  const [showCorrectiveActionsModal, setShowCorrectiveActionsModal] =
    useState(false);

  const {
    data: incident,
    error,
    isLoading,
  } = useGetIncidentDetailQuery(Number(id));
  const [deleteIncident, { isLoading: isDeleting }] =
    useDeleteIncidentMutation();
  const [addInvolvedPerson] = useAddInvolvedPersonMutation();
  const [updateInvolvedPerson] = useUpdateInvolvedPersonMutation();
  const [removeInvolvedPerson] = useRemoveInvolvedPersonMutation();

  // Helper functions (removed - now imported from incidentUtils)

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading incident details...</span>
      </div>
    );
  }

  if (error || !incident) {
    return (
      <CAlert color="danger">
        Failed to load incident details. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/incidents')}>
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
                <h4 className="mb-0">Incident Details</h4>
                <small className="text-muted">ID: {incident.id}</small>
              </div>
              <div>
                <CButton
                  color="light"
                  className="me-2"
                  onClick={() => navigate('/incidents')}
                >
                  <Icon icon={ACTION_ICONS.back} size="sm" className="me-2" />
                  Back
                </CButton>
                <CButton
                  color="primary"
                  className="me-2"
                  onClick={() => navigate(`/incidents/${id}/edit`)}
                >
                  <Icon icon={ACTION_ICONS.edit} size="sm" className="me-2" />
                  Edit
                </CButton>
                <CButton
                  color="danger"
                  disabled={isDeleting}
                  onClick={async () => {
                    if (
                      window.confirm(
                        'Are you sure you want to delete this incident? This action cannot be undone.'
                      )
                    ) {
                      try {
                        await deleteIncident(Number(id)).unwrap();
                        navigate('/incidents');
                      } catch (error) {
                        console.error('Failed to delete incident:', error);
                        alert('Failed to delete incident. Please try again.');
                      }
                    }
                  }}
                >
                  {isDeleting ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Deleting...
                    </>
                  ) : (
                    <>
                      <Icon
                        icon={ACTION_ICONS.delete}
                        size="sm"
                        className="me-2"
                      />
                      Delete
                    </>
                  )}
                </CButton>
              </div>
            </CCardHeader>

            <CCardBody>
              <CRow>
                <CCol md={8}>
                  <h5 className="mb-3">{incident.title}</h5>

                  <div className="mb-4">
                    <h6 className="text-muted">Description</h6>
                    <p>{incident.description}</p>
                  </div>

                  <CRow className="mb-4">
                    <CCol sm={6}>
                      <h6 className="text-muted">Status</h6>
                      <p>{getStatusBadge(incident.status)}</p>
                    </CCol>
                    <CCol sm={6}>
                      <h6 className="text-muted">Severity</h6>
                      <p>{getSeverityBadge(incident.severity)}</p>
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
                        Incident Date
                      </h6>
                      <p>{formatDate(incident.incidentDate)}</p>
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
                      <p>{incident.location}</p>
                    </CCol>
                  </CRow>

                  {incident.injuryType && (
                    <div className="mb-4">
                      <h6 className="text-muted">Injury Details</h6>
                      <CListGroup>
                        <CListGroupItem>
                          <strong>Injury Type:</strong> {incident.injuryType}
                        </CListGroupItem>
                        <CListGroupItem>
                          <strong>Medical Treatment Provided:</strong>{' '}
                          {incident.medicalTreatmentProvided ? 'Yes' : 'No'}
                        </CListGroupItem>
                        <CListGroupItem>
                          <strong>Emergency Services Contacted:</strong>{' '}
                          {incident.emergencyServicesContacted ? 'Yes' : 'No'}
                        </CListGroupItem>
                      </CListGroup>
                    </div>
                  )}

                  {incident.witnessNames && (
                    <div className="mb-4">
                      <h6 className="text-muted">Witness Information</h6>
                      <p>{incident.witnessNames}</p>
                    </div>
                  )}

                  {incident.immediateActionsTaken && (
                    <div className="mb-4">
                      <h6 className="text-muted">Immediate Actions Taken</h6>
                      <p>{incident.immediateActionsTaken}</p>
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
                      <strong>{incident.reporterName}</strong>
                    </div>
                    <div className="mb-3">
                      <small className="text-muted">
                        Email: {incident.reporterEmail}
                      </small>
                    </div>
                    <div className="mb-4">
                      <small className="text-muted">
                        Department: {incident.reporterDepartment}
                      </small>
                    </div>

                    <h6 className="text-muted mb-3">Related Information</h6>
                    <CListGroup className="mb-4 related-info-mobile">
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
                          {incident.attachmentsCount || 0}
                        </CBadge>
                      </CListGroupItem>
                      <CListGroupItem
                        className="d-flex justify-content-between cursor-pointer position-relative"
                        onClick={() => setShowInvolvedPersonsModal(true)}
                        style={{ cursor: 'pointer' }}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            setShowInvolvedPersonsModal(true);
                          }
                        }}
                      >
                        <div>
                          <span>Involved Persons</span>
                          <small className="d-block text-muted">
                            Formally linked individuals
                          </small>
                        </div>
                        <CBadge color="info">
                          {incident.involvedPersons?.length || 0}
                        </CBadge>
                      </CListGroupItem>
                      <CListGroupItem
                        className="d-flex justify-content-between cursor-pointer position-relative"
                        onClick={() => setShowCorrectiveActionsModal(true)}
                        style={{ cursor: 'pointer' }}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            setShowCorrectiveActionsModal(true);
                          }
                        }}
                      >
                        <div>
                          <span>Corrective Actions</span>
                          <small className="d-block text-muted">
                            Assigned CAPA tasks
                          </small>
                        </div>
                        <CBadge color="info">
                          {incident.correctiveActionsCount || 0}
                        </CBadge>
                      </CListGroupItem>
                    </CListGroup>

                    {incident.attachmentsCount === 0 &&
                      incident.involvedPersons?.length === 0 &&
                      incident.correctiveActionsCount === 0 && (
                        <CCallout color="info" className="small mb-4">
                          <strong>Note:</strong> File uploads, formal person
                          assignments, and corrective actions are typically
                          added during the investigation phase.
                          <div className="mt-2">
                            <CButton
                              color="primary"
                              size="sm"
                              variant="outline"
                              onClick={() => setShowInvolvedPersonsModal(true)}
                            >
                              Add Involved Persons
                            </CButton>
                          </div>
                        </CCallout>
                      )}

                    <h6 className="text-muted mb-3">Audit Information</h6>
                    <div className="small text-muted">
                      <p className="mb-1">
                        <strong>Created:</strong>{' '}
                        {formatDate(incident.createdAt)}
                      </p>
                      {incident.createdBy && (
                        <p className="mb-1">
                          <strong>Created By:</strong> {incident.createdBy}
                        </p>
                      )}
                      {incident.lastModifiedAt && (
                        <>
                          <p className="mb-1">
                            <strong>Modified:</strong>{' '}
                            {formatDate(incident.lastModifiedAt)}
                          </p>
                          {incident.lastModifiedBy && (
                            <p className="mb-1">
                              <strong>Modified By:</strong>{' '}
                              {incident.lastModifiedBy}
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

      {/* Corrective Actions Manager */}
      {incident && (
        <CRow>
          <CCol xs={12}>
            <CorrectiveActionsManager
              incidentId={incident.id}
              allowEdit={true}
            />
          </CCol>
        </CRow>
      )}

      {/* Incident Audit Trail */}
      {incident && (
        <CRow>
          <CCol xs={12}>
            <IncidentAuditTrail incidentId={incident.id} />
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
          {incident && (
            <AttachmentManager
              incidentId={incident.id}
              allowUpload={true}
              allowDelete={true}
            />
          )}
        </CModalBody>
      </CModal>

      {/* Corrective Actions Modal */}
      <CModal
        visible={showCorrectiveActionsModal}
        onClose={() => setShowCorrectiveActionsModal(false)}
        size="xl"
      >
        <CModalHeader>
          <CModalTitle>Manage Corrective Actions</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {incident && (
            <CorrectiveActionsManager
              incidentId={incident.id}
              allowEdit={true}
            />
          )}
        </CModalBody>
      </CModal>

      {/* Involved Persons Modal */}
      {incident && (
        <InvolvedPersonsModal
          visible={showInvolvedPersonsModal}
          onClose={() => setShowInvolvedPersonsModal(false)}
          incidentId={incident.id}
          involvedPersons={incident.involvedPersons || []}
          onAdd={async (personId, involvementType, injuryDescription) => {
            await addInvolvedPerson({
              incidentId: incident.id,
              data: { personId: Number(personId), involvementType, injuryDescription },
            }).unwrap();
          }}
          onUpdate={async (personId, involvementType, injuryDescription) => {
            await updateInvolvedPerson({
              incidentId: incident.id,
              personId: Number(personId),
              data: { involvementType, injuryDescription },
            }).unwrap();
          }}
          onRemove={async (personId) => {
            await removeInvolvedPerson({
              incidentId: incident.id,
              personId: Number(personId),
            }).unwrap();
          }}
        />
      )}
    </>
  );
};

export default IncidentDetail;
