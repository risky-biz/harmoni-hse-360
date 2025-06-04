import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CSpinner,
  CAlert,
  CListGroup,
  CListGroupItem,
  CBadge,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormLabel,
  CFormInput,
  CFormTextarea,
  CFormSelect,
  CRow,
  CCol,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';
import { formatDateTime } from '../../utils/dateUtils';
import {
  useGetCorrectiveActionsQuery,
  useCreateCorrectiveActionMutation,
  useUpdateCorrectiveActionMutation,
  useDeleteCorrectiveActionMutation,
  useGetAvailableUsersQuery,
  CorrectiveActionDto,
  CreateCorrectiveActionRequest,
  UpdateCorrectiveActionRequest,
} from '../../features/incidents/incidentApi';

interface CorrectiveActionsManagerProps {
  incidentId: number;
  allowEdit?: boolean;
}

interface CorrectiveActionFormData {
  description: string;
  assignedToDepartment: string;
  assignedToId?: number;
  dueDate: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  status: 'Pending' | 'InProgress' | 'Completed' | 'Overdue';
  completionNotes?: string;
}

const PRIORITY_CONFIG = {
  Low: { color: 'secondary', label: 'Low' },
  Medium: { color: 'info', label: 'Medium' },
  High: { color: 'warning', label: 'High' },
  Critical: { color: 'danger', label: 'Critical' },
} as const;

const STATUS_CONFIG = {
  Pending: { color: 'secondary', label: 'Pending' },
  InProgress: { color: 'info', label: 'In Progress' },
  Completed: { color: 'success', label: 'Completed' },
  Overdue: { color: 'danger', label: 'Overdue' },
} as const;

const CorrectiveActionsManager: React.FC<CorrectiveActionsManagerProps> = ({
  incidentId,
  allowEdit = true,
}) => {
  const [showModal, setShowModal] = useState(false);
  const [editingAction, setEditingAction] = useState<CorrectiveActionDto | null>(null);
  const [formData, setFormData] = useState<CorrectiveActionFormData>({
    description: '',
    assignedToDepartment: '',
    assignedToId: undefined,
    dueDate: '',
    priority: 'Medium',
    status: 'Pending',
    completionNotes: '',
  });
  const [searchTerm] = useState('');

  const { 
    data: correctiveActions = [], 
    isLoading, 
    error 
  } = useGetCorrectiveActionsQuery(incidentId);

  const { data: availableUsers = [] } = useGetAvailableUsersQuery(searchTerm);

  const [createAction, { isLoading: isCreating }] = useCreateCorrectiveActionMutation();
  const [updateAction, { isLoading: isUpdating }] = useUpdateCorrectiveActionMutation();
  const [deleteAction, { isLoading: isDeleting }] = useDeleteCorrectiveActionMutation();

  const handleOpenModal = (action?: CorrectiveActionDto) => {
    if (action) {
      setEditingAction(action);
      setFormData({
        description: action.description,
        assignedToDepartment: action.assignedToDepartment,
        assignedToId: action.assignedTo?.id,
        dueDate: action.dueDate.split('T')[0], // Format date for input
        priority: action.priority,
        status: action.status,
        completionNotes: action.completionNotes || '',
      });
    } else {
      setEditingAction(null);
      setFormData({
        description: '',
        assignedToDepartment: '',
        assignedToId: undefined,
        dueDate: '',
        priority: 'Medium',
        status: 'Pending',
        completionNotes: '',
      });
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingAction(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (editingAction) {
        const updateData: UpdateCorrectiveActionRequest = {
          description: formData.description,
          assignedToDepartment: formData.assignedToDepartment,
          assignedToId: formData.assignedToId,
          dueDate: formData.dueDate,
          priority: formData.priority,
          status: formData.status,
          completionNotes: formData.completionNotes,
        };
        
        await updateAction({
          incidentId,
          actionId: editingAction.id,
          data: updateData,
        }).unwrap();
      } else {
        const createData: CreateCorrectiveActionRequest = {
          description: formData.description,
          assignedToDepartment: formData.assignedToDepartment,
          assignedToId: formData.assignedToId,
          dueDate: formData.dueDate,
          priority: formData.priority,
        };
        
        await createAction({
          incidentId,
          data: createData,
        }).unwrap();
      }
      
      handleCloseModal();
    } catch (error) {
      console.error('Failed to save corrective action:', error);
    }
  };

  const handleDelete = async (actionId: number) => {
    if (!confirm('Are you sure you want to delete this corrective action?')) {
      return;
    }

    try {
      await deleteAction({ incidentId, actionId }).unwrap();
    } catch (error) {
      console.error('Failed to delete corrective action:', error);
    }
  };


  const isOverdue = (dueDate: string, status: string) => {
    return status !== 'Completed' && new Date(dueDate) < new Date();
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center p-4">
        <CSpinner size="sm" />
        <span className="ms-2">Loading corrective actions...</span>
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger">
        Failed to load corrective actions. Please try again.
      </CAlert>
    );
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader className="d-flex justify-content-between align-items-center">
          <h6 className="mb-0">
            <FontAwesomeIcon icon={CONTEXT_ICONS.incident} className="me-2" />
            Corrective Actions ({correctiveActions.length})
          </h6>
          {allowEdit && (
            <CButton
              color="primary"
              size="sm"
              onClick={() => handleOpenModal()}
            >
              <FontAwesomeIcon icon={ACTION_ICONS.create} className="me-2" />
              Add Action
            </CButton>
          )}
        </CCardHeader>
        
        <CCardBody>
          {correctiveActions.length === 0 ? (
            <div className="text-center text-muted py-3">
              <FontAwesomeIcon icon={CONTEXT_ICONS.incident} size="2x" className="mb-2 opacity-50" />
              <p className="mb-0">No corrective actions defined yet</p>
              {allowEdit && (
                <p className="small">Add corrective actions to track follow-up tasks</p>
              )}
            </div>
          ) : (
            <CListGroup flush>
              {correctiveActions.map((action) => (
                <CListGroupItem key={action.id} className="d-flex justify-content-between align-items-start">
                  <div className="flex-grow-1">
                    <div className="d-flex align-items-center mb-2">
                      <CBadge 
                        color={STATUS_CONFIG[action.status]?.color || 'secondary'} 
                        className="me-2"
                      >
                        {STATUS_CONFIG[action.status]?.label || action.status}
                      </CBadge>
                      <CBadge 
                        color={PRIORITY_CONFIG[action.priority]?.color || 'secondary'}
                        className="me-2"
                      >
                        {PRIORITY_CONFIG[action.priority]?.label || action.priority}
                      </CBadge>
                      {isOverdue(action.dueDate, action.status) && (
                        <CBadge color="danger">OVERDUE</CBadge>
                      )}
                    </div>
                    
                    <h6 className="mb-1">{action.description}</h6>
                    
                    <div className="text-muted small">
                      <div>Assigned to: {action.assignedTo?.fullName || action.assignedToDepartment}</div>
                      <div>Due: {formatDateTime(action.dueDate)}</div>
                      {action.completedDate && (
                        <div>Completed: {formatDateTime(action.completedDate)}</div>
                      )}
                      {action.completionNotes && (
                        <div className="mt-1">
                          <strong>Notes:</strong> {action.completionNotes}
                        </div>
                      )}
                    </div>
                  </div>
                  
                  {allowEdit && (
                    <div className="d-flex gap-2">
                      <CButton
                        color="primary"
                        variant="ghost"
                        size="sm"
                        onClick={() => handleOpenModal(action)}
                        disabled={isDeleting}
                      >
                        <FontAwesomeIcon icon={ACTION_ICONS.edit} />
                      </CButton>
                      <CButton
                        color="danger"
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDelete(action.id)}
                        disabled={isDeleting}
                      >
                        <FontAwesomeIcon icon={ACTION_ICONS.delete} />
                      </CButton>
                    </div>
                  )}
                </CListGroupItem>
              ))}
            </CListGroup>
          )}
        </CCardBody>
      </CCard>

      {/* Add/Edit Modal */}
      <CModal visible={showModal} onClose={handleCloseModal} size="lg">
        <CModalHeader>
          <CModalTitle>
            {editingAction ? 'Edit Corrective Action' : 'Add Corrective Action'}
          </CModalTitle>
        </CModalHeader>
        
        <CForm onSubmit={handleSubmit}>
          <CModalBody>
            <CRow>
              <CCol md={12}>
                <div className="mb-3">
                  <CFormLabel>Description *</CFormLabel>
                  <CFormTextarea
                    rows={3}
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    placeholder="Describe the corrective action required..."
                    required
                  />
                </div>
              </CCol>
            </CRow>

            <CRow>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel>Assigned Department *</CFormLabel>
                  <CFormInput
                    value={formData.assignedToDepartment}
                    onChange={(e) => setFormData({ ...formData, assignedToDepartment: e.target.value })}
                    placeholder="e.g., Safety Department"
                    required
                  />
                </div>
              </CCol>
              
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel>Assigned Person (Optional)</CFormLabel>
                  <CFormSelect
                    value={formData.assignedToId || ''}
                    onChange={(e) => setFormData({ 
                      ...formData, 
                      assignedToId: e.target.value ? Number(e.target.value) : undefined 
                    })}
                  >
                    <option value="">Select a person...</option>
                    {availableUsers.map((user) => (
                      <option key={user.id} value={user.id}>
                        {user.fullName} ({user.email})
                      </option>
                    ))}
                  </CFormSelect>
                </div>
              </CCol>
            </CRow>

            <CRow>
              <CCol md={4}>
                <div className="mb-3">
                  <CFormLabel>Due Date *</CFormLabel>
                  <CFormInput
                    type="date"
                    value={formData.dueDate}
                    onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
                    min={new Date().toISOString().split('T')[0]}
                    required
                  />
                </div>
              </CCol>
              
              <CCol md={4}>
                <div className="mb-3">
                  <CFormLabel>Priority *</CFormLabel>
                  <CFormSelect
                    value={formData.priority}
                    onChange={(e) => setFormData({ 
                      ...formData, 
                      priority: e.target.value as 'Low' | 'Medium' | 'High' | 'Critical'
                    })}
                    required
                  >
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Critical">Critical</option>
                  </CFormSelect>
                </div>
              </CCol>
              
              {editingAction && (
                <CCol md={4}>
                  <div className="mb-3">
                    <CFormLabel>Status</CFormLabel>
                    <CFormSelect
                      value={formData.status}
                      onChange={(e) => setFormData({ 
                        ...formData, 
                        status: e.target.value as 'Pending' | 'InProgress' | 'Completed' | 'Overdue'
                      })}
                    >
                      <option value="Pending">Pending</option>
                      <option value="InProgress">In Progress</option>
                      <option value="Completed">Completed</option>
                    </CFormSelect>
                  </div>
                </CCol>
              )}
            </CRow>

            {editingAction && formData.status === 'Completed' && (
              <CRow>
                <CCol md={12}>
                  <div className="mb-3">
                    <CFormLabel>Completion Notes</CFormLabel>
                    <CFormTextarea
                      rows={3}
                      value={formData.completionNotes}
                      onChange={(e) => setFormData({ ...formData, completionNotes: e.target.value })}
                      placeholder="Describe how the action was completed..."
                    />
                  </div>
                </CCol>
              </CRow>
            )}
          </CModalBody>
          
          <CModalFooter>
            <CButton color="secondary" onClick={handleCloseModal}>
              Cancel
            </CButton>
            <CButton 
              color="primary" 
              type="submit"
              disabled={isCreating || isUpdating}
            >
              {isCreating || isUpdating ? (
                <>
                  <CSpinner size="sm" className="me-2" />
                  {editingAction ? 'Updating...' : 'Creating...'}
                </>
              ) : (
                <>
                  <FontAwesomeIcon icon={ACTION_ICONS.save} className="me-2" />
                  {editingAction ? 'Update Action' : 'Create Action'}
                </>
              )}
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </>
  );
};

export default CorrectiveActionsManager;