import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CTable,
  CTableHead,
  CTableBody,
  CTableHeaderCell,
  CTableDataCell,
  CTableRow,
  CSpinner,
  CAlert,
  CBadge,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormInput,
  CFormLabel,
  CFormTextarea,
  CFormSelect,
  CRow,
  CCol,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPlus, faCheck, faTasks } from '@fortawesome/free-solid-svg-icons';
import {
  useCreateMitigationActionMutation,
  HazardMitigationActionDto,
} from '../../features/hazards/hazardApi';
import { formatDate } from '../../utils/dateUtils';
import { useGetAvailableUsersQuery } from '../../features/hazards/hazardApi';

interface MitigationActionsManagerProps {
  hazardId: number;
  mitigationActions: HazardMitigationActionDto[];
  allowEdit?: boolean;
  onRefresh?: () => void;
}

const MitigationActionsManager: React.FC<MitigationActionsManagerProps> = ({
  hazardId,
  mitigationActions = [],
  allowEdit = true,
  onRefresh,
}) => {
  const [showModal, setShowModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [formData, setFormData] = useState({
    actionDescription: '',
    type: 'Preventive',
    priority: 'Medium',
    assignedToId: '',
    targetDate: '',
    estimatedCost: '',
    requiresVerification: false,
  });

  const [createMitigationAction, { isLoading: isCreating }] = useCreateMitigationActionMutation();
  const { data: availableUsers = [] } = useGetAvailableUsersQuery(searchTerm);

  const handleCreateAction = async () => {
    try {
      await createMitigationAction({
        hazardId,
        actionDescription: formData.actionDescription,
        type: formData.type,
        priority: formData.priority,
        assignedToId: parseInt(formData.assignedToId),
        targetDate: formData.targetDate,
        estimatedCost: formData.estimatedCost ? parseFloat(formData.estimatedCost) : undefined,
        requiresVerification: formData.requiresVerification,
      }).unwrap();

      setShowModal(false);
      resetForm();
      onRefresh?.();
    } catch (error) {
      console.error('Failed to create mitigation action:', error);
    }
  };

  const resetForm = () => {
    setFormData({
      actionDescription: '',
      type: 'Preventive',
      priority: 'Medium',
      assignedToId: '',
      targetDate: '',
      estimatedCost: '',
      requiresVerification: false,
    });
    setSearchTerm('');
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'Critical':
        return 'danger';
      case 'High':
        return 'warning';
      case 'Medium':
        return 'info';
      case 'Low':
        return 'success';
      default:
        return 'secondary';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed':
        return 'success';
      case 'In Progress':
        return 'primary';
      case 'Pending':
        return 'warning';
      case 'Overdue':
        return 'danger';
      case 'Cancelled':
        return 'secondary';
      default:
        return 'secondary';
    }
  };

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader className="d-flex justify-content-between align-items-center">
          <h6 className="mb-0">
            <FontAwesomeIcon icon={faTasks} className="me-2" />
            Mitigation Actions ({mitigationActions.length})
          </h6>
          {allowEdit && (
            <CButton
              color="primary"
              size="sm"
              onClick={() => setShowModal(true)}
            >
              <FontAwesomeIcon icon={faPlus} className="me-2" />
              Add Action
            </CButton>
          )}
        </CCardHeader>
        <CCardBody>
          {mitigationActions.length === 0 ? (
            <CAlert color="info">
              No mitigation actions have been created for this hazard.
              {allowEdit && (
                <div className="mt-3">
                  <CButton
                    color="primary"
                    onClick={() => setShowModal(true)}
                  >
                    <FontAwesomeIcon icon={faPlus} className="me-2" />
                    Create First Action
                  </CButton>
                </div>
              )}
            </CAlert>
          ) : (
            <CTable striped hover responsive>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Action</CTableHeaderCell>
                  <CTableHeaderCell>Type</CTableHeaderCell>
                  <CTableHeaderCell>Priority</CTableHeaderCell>
                  <CTableHeaderCell>Assigned To</CTableHeaderCell>
                  <CTableHeaderCell>Target Date</CTableHeaderCell>
                  <CTableHeaderCell>Status</CTableHeaderCell>
                  <CTableHeaderCell>Verification</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {mitigationActions.map((action) => (
                  <CTableRow key={action.id}>
                    <CTableDataCell>
                      <div>
                        <div>{action.actionDescription}</div>
                        {action.completionNotes && (
                          <small className="text-muted d-block">
                            Completion Notes: {action.completionNotes}
                          </small>
                        )}
                      </div>
                    </CTableDataCell>
                    <CTableDataCell>{action.type}</CTableDataCell>
                    <CTableDataCell>
                      <CBadge color={getPriorityColor(action.priority)}>
                        {action.priority}
                      </CBadge>
                    </CTableDataCell>
                    <CTableDataCell>{action.assignedToName}</CTableDataCell>
                    <CTableDataCell>
                      {formatDate(action.targetDate)}
                      {action.completedDate && (
                        <small className="text-muted d-block">
                          Completed: {formatDate(action.completedDate)}
                        </small>
                      )}
                    </CTableDataCell>
                    <CTableDataCell>
                      <CBadge color={getStatusColor(action.status)}>
                        {action.status}
                      </CBadge>
                    </CTableDataCell>
                    <CTableDataCell>
                      {action.requiresVerification ? (
                        action.verifiedAt ? (
                          <div>
                            <FontAwesomeIcon
                              icon={faCheck}
                              className="text-success me-1"
                            />
                            <small>
                              Verified by {action.verifiedByName}
                              <br />
                              {formatDate(action.verifiedAt)}
                            </small>
                          </div>
                        ) : (
                          <CBadge color="warning">Pending Verification</CBadge>
                        )
                      ) : (
                        <span className="text-muted">Not Required</span>
                      )}
                    </CTableDataCell>
                  </CTableRow>
                ))}
              </CTableBody>
            </CTable>
          )}
        </CCardBody>
      </CCard>

      {/* Create Mitigation Action Modal */}
      <CModal
        visible={showModal}
        onClose={() => {
          setShowModal(false);
          resetForm();
        }}
        size="lg"
      >
        <CModalHeader>
          <CModalTitle>Create Mitigation Action</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel htmlFor="actionDescription">
                Action Description *
              </CFormLabel>
              <CFormTextarea
                id="actionDescription"
                rows={3}
                value={formData.actionDescription}
                onChange={(e) =>
                  setFormData({ ...formData, actionDescription: e.target.value })
                }
                placeholder="Describe the action to be taken..."
                required
              />
            </div>

            <CRow>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel htmlFor="type">Action Type *</CFormLabel>
                  <CFormSelect
                    id="type"
                    value={formData.type}
                    onChange={(e) =>
                      setFormData({ ...formData, type: e.target.value })
                    }
                  >
                    <option value="Preventive">Preventive</option>
                    <option value="Corrective">Corrective</option>
                    <option value="Detective">Detective</option>
                    <option value="Emergency">Emergency</option>
                  </CFormSelect>
                </div>
              </CCol>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel htmlFor="priority">Priority *</CFormLabel>
                  <CFormSelect
                    id="priority"
                    value={formData.priority}
                    onChange={(e) =>
                      setFormData({ ...formData, priority: e.target.value })
                    }
                  >
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Critical">Critical</option>
                  </CFormSelect>
                </div>
              </CCol>
            </CRow>

            <CRow>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel htmlFor="assignedTo">Assign To *</CFormLabel>
                  <CFormInput
                    type="text"
                    id="userSearch"
                    placeholder="Search for users..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="mb-2"
                  />
                  <CFormSelect
                    id="assignedTo"
                    value={formData.assignedToId}
                    onChange={(e) =>
                      setFormData({ ...formData, assignedToId: e.target.value })
                    }
                    required
                  >
                    <option value="">Select a user</option>
                    {availableUsers.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.name} - {user.department}
                      </option>
                    ))}
                  </CFormSelect>
                </div>
              </CCol>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel htmlFor="targetDate">Target Date *</CFormLabel>
                  <CFormInput
                    type="date"
                    id="targetDate"
                    value={formData.targetDate}
                    onChange={(e) =>
                      setFormData({ ...formData, targetDate: e.target.value })
                    }
                    required
                  />
                </div>
              </CCol>
            </CRow>

            <CRow>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel htmlFor="estimatedCost">
                    Estimated Cost (optional)
                  </CFormLabel>
                  <CFormInput
                    type="number"
                    id="estimatedCost"
                    value={formData.estimatedCost}
                    onChange={(e) =>
                      setFormData({ ...formData, estimatedCost: e.target.value })
                    }
                    placeholder="0.00"
                    step="0.01"
                  />
                </div>
              </CCol>
              <CCol md={6}>
                <div className="mb-3 mt-4">
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      id="requiresVerification"
                      checked={formData.requiresVerification}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          requiresVerification: e.target.checked,
                        })
                      }
                    />
                    <label
                      className="form-check-label"
                      htmlFor="requiresVerification"
                    >
                      Requires verification after completion
                    </label>
                  </div>
                </div>
              </CCol>
            </CRow>
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={() => {
              setShowModal(false);
              resetForm();
            }}
          >
            Cancel
          </CButton>
          <CButton
            color="primary"
            onClick={handleCreateAction}
            disabled={
              isCreating ||
              !formData.actionDescription ||
              !formData.assignedToId ||
              !formData.targetDate
            }
          >
            {isCreating ? (
              <>
                <CSpinner size="sm" className="me-2" />
                Creating...
              </>
            ) : (
              'Create Action'
            )}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default MitigationActionsManager;