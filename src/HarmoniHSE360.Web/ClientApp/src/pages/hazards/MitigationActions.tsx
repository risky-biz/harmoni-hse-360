import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CTable,
  CTableHead,
  CTableRow,
  CTableHeaderCell,
  CTableBody,
  CTableDataCell,
  CBadge,
  CProgress,
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
  CAlert,
  CSpinner,
  CTooltip,
  CButtonGroup,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faEdit,
  faCheck,
  faExclamationTriangle,
  faClock,
  faUser,
  faCalendarAlt,
  faDollarSign,
  faClipboardCheck,
  faChartLine,
  faEllipsisV,
  faCheckCircle,
  faTimesCircle,
  faStar,
  faArrowLeft,
} from '@fortawesome/free-solid-svg-icons';
import { useGetHazardQuery, useCreateMitigationActionMutation, useGetAvailableUsersQuery } from '../../features/hazards/hazardApi';
import type { HazardMitigationActionDto, CreateMitigationActionRequest } from '../../types/hazard';

const MitigationActions: React.FC = () => {
  const { hazardId } = useParams<{ hazardId: string }>();
  const navigate = useNavigate();
  const [showModal, setShowModal] = useState(false);
  const [selectedAction, setSelectedAction] = useState<HazardMitigationActionDto | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  const { data: hazard, isLoading, error } = useGetHazardQuery({ 
    id: parseInt(hazardId || '0'),
    includeMitigationActions: true 
  });

  const { data: availableUsers } = useGetAvailableUsersQuery(searchTerm);
  const [createMitigationAction, { isLoading: isCreating }] = useCreateMitigationActionMutation();

  const [formData, setFormData] = useState<Partial<CreateMitigationActionRequest>>({
    hazardId: parseInt(hazardId || '0'),
    actionDescription: '',
    type: 'Elimination',
    priority: 'Medium',
    assignedToId: 0,
    targetDate: '',
    estimatedCost: 0,
    requiresVerification: false,
  });

  const [error2, setError2] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError2(null);

    try {
      await createMitigationAction({
        ...formData,
        hazardId: parseInt(hazardId || '0'),
      } as CreateMitigationActionRequest).unwrap();

      setShowModal(false);
      resetForm();
    } catch (err: any) {
      setError2(err.data?.message || 'Failed to create mitigation action');
    }
  };

  const resetForm = () => {
    setFormData({
      hazardId: parseInt(hazardId || '0'),
      actionDescription: '',
      type: 'Elimination',
      priority: 'Medium',
      assignedToId: 0,
      targetDate: '',
      estimatedCost: 0,
      requiresVerification: false,
    });
    setSelectedAction(null);
    setSearchTerm('');
  };

  const getStatusBadge = (status: string) => {
    const statusConfig: Record<string, { color: string; icon: any }> = {
      'Pending': { color: 'warning', icon: faClock },
      'InProgress': { color: 'info', icon: faExclamationTriangle },
      'Completed': { color: 'success', icon: faCheck },
      'Overdue': { color: 'danger', icon: faTimesCircle },
      'Verified': { color: 'success', icon: faCheckCircle },
    };

    const config = statusConfig[status] || { color: 'secondary', icon: faClock };

    return (
      <CBadge color={config.color} className="d-flex align-items-center gap-1">
        <FontAwesomeIcon icon={config.icon} size="sm" />
        {status}
      </CBadge>
    );
  };

  const getPriorityBadge = (priority: string) => {
    const priorityConfig: Record<string, string> = {
      'Critical': 'danger',
      'High': 'warning',
      'Medium': 'info',
      'Low': 'secondary',
    };

    return <CBadge color={priorityConfig[priority] || 'secondary'}>{priority}</CBadge>;
  };

  const getTypeBadge = (type: string) => {
    const typeConfig: Record<string, string> = {
      'Elimination': 'danger',
      'Substitution': 'warning',
      'Engineering': 'info',
      'Administrative': 'primary',
      'PPE': 'success',
    };

    return <CBadge color={typeConfig[type] || 'secondary'}>{type}</CBadge>;
  };

  const calculateProgress = (action: HazardMitigationActionDto) => {
    if (action.status === 'Completed') return 100;
    if (action.status === 'Pending') return 0;
    
    const targetDate = new Date(action.targetDate);
    const createdDate = new Date(hazard?.createdAt || new Date());
    const today = new Date();
    
    const totalDays = Math.max(1, (targetDate.getTime() - createdDate.getTime()) / (1000 * 60 * 60 * 24));
    const elapsedDays = Math.max(0, (today.getTime() - createdDate.getTime()) / (1000 * 60 * 60 * 24));
    
    return Math.min(100, Math.round((elapsedDays / totalDays) * 100));
  };

  if (isLoading) {
    return (
      <div className="text-center p-5">
        <CSpinner color="primary" />
        <p className="mt-2">Loading mitigation actions...</p>
      </div>
    );
  }

  if (error || !hazard) {
    return (
      <CAlert color="danger">
        <h4>Error Loading Data</h4>
        <p>Unable to load hazard details. Please try again later.</p>
        <CButton color="primary" onClick={() => navigate('/hazards')}>
          Back to Hazards
        </CButton>
      </CAlert>
    );
  }

  const mitigationActions = hazard.mitigationActions || [];
  const completedCount = mitigationActions.filter(a => a.status === 'Completed').length;
  const overallProgress = mitigationActions.length > 0 
    ? Math.round((completedCount / mitigationActions.length) * 100) 
    : 0;

  return (
    <>
      <CRow>
        <CCol xs={12}>
          <CCard className="mb-4">
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div>
                <h4 className="mb-0">Mitigation Actions</h4>
                <small className="text-muted">Hazard: {hazard.title}</small>
              </div>
              <div className="d-flex gap-2">
                <CButton
                  color="secondary"
                  size="sm"
                  onClick={() => navigate(`/hazards/${hazardId}`)}
                >
                  <FontAwesomeIcon icon={faArrowLeft} className="me-1" />
                  Back to Hazard
                </CButton>
                <CButton
                  color="primary"
                  size="sm"
                  onClick={() => setShowModal(true)}
                >
                  <FontAwesomeIcon icon={faPlus} className="me-1" />
                  Add Action
                </CButton>
              </div>
            </CCardHeader>
            <CCardBody>
              {/* Summary Cards */}
              <CRow className="mb-4">
                <CCol sm={3}>
                  <div className="border-start border-start-4 border-start-info py-1 px-3">
                    <div className="text-medium-emphasis small">Total Actions</div>
                    <div className="fs-5 fw-semibold">{mitigationActions.length}</div>
                  </div>
                </CCol>
                <CCol sm={3}>
                  <div className="border-start border-start-4 border-start-success py-1 px-3">
                    <div className="text-medium-emphasis small">Completed</div>
                    <div className="fs-5 fw-semibold">{completedCount}</div>
                  </div>
                </CCol>
                <CCol sm={3}>
                  <div className="border-start border-start-4 border-start-warning py-1 px-3">
                    <div className="text-medium-emphasis small">In Progress</div>
                    <div className="fs-5 fw-semibold">
                      {mitigationActions.filter(a => a.status === 'InProgress').length}
                    </div>
                  </div>
                </CCol>
                <CCol sm={3}>
                  <div className="border-start border-start-4 border-start-danger py-1 px-3">
                    <div className="text-medium-emphasis small">Overdue</div>
                    <div className="fs-5 fw-semibold">
                      {mitigationActions.filter(a => 
                        a.status !== 'Completed' && new Date(a.targetDate) < new Date()
                      ).length}
                    </div>
                  </div>
                </CCol>
              </CRow>

              {/* Overall Progress */}
              <div className="mb-4">
                <div className="d-flex justify-content-between mb-1">
                  <span>Overall Progress</span>
                  <span>{overallProgress}%</span>
                </div>
                <CProgress value={overallProgress} color="success" height={20} />
              </div>

              {/* Actions Table */}
              <CTable hover responsive>
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>Action</CTableHeaderCell>
                    <CTableHeaderCell>Type</CTableHeaderCell>
                    <CTableHeaderCell>Priority</CTableHeaderCell>
                    <CTableHeaderCell>Assigned To</CTableHeaderCell>
                    <CTableHeaderCell>Target Date</CTableHeaderCell>
                    <CTableHeaderCell>Status</CTableHeaderCell>
                    <CTableHeaderCell>Progress</CTableHeaderCell>
                    <CTableHeaderCell>Actions</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {mitigationActions.length === 0 ? (
                    <CTableRow>
                      <CTableDataCell colSpan={8} className="text-center py-4">
                        <p className="mb-0 text-muted">No mitigation actions yet</p>
                        <CButton
                          color="primary"
                          variant="outline"
                          size="sm"
                          className="mt-2"
                          onClick={() => setShowModal(true)}
                        >
                          <FontAwesomeIcon icon={faPlus} className="me-1" />
                          Add First Action
                        </CButton>
                      </CTableDataCell>
                    </CTableRow>
                  ) : (
                    mitigationActions.map((action) => (
                      <CTableRow key={action.id}>
                        <CTableDataCell>
                          <div>
                            <strong>{action.actionDescription}</strong>
                            {action.estimatedCost > 0 && (
                              <div className="small text-muted">
                                <FontAwesomeIcon icon={faDollarSign} className="me-1" />
                                Est. Cost: ${action.estimatedCost.toLocaleString()}
                              </div>
                            )}
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>{getTypeBadge(action.type)}</CTableDataCell>
                        <CTableDataCell>{getPriorityBadge(action.priority)}</CTableDataCell>
                        <CTableDataCell>
                          <div className="d-flex align-items-center">
                            <FontAwesomeIcon icon={faUser} className="me-2 text-muted" />
                            {action.assignedToName || 'Unassigned'}
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <div className="d-flex align-items-center">
                            <FontAwesomeIcon icon={faCalendarAlt} className="me-2 text-muted" />
                            {new Date(action.targetDate).toLocaleDateString()}
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>{getStatusBadge(action.status)}</CTableDataCell>
                        <CTableDataCell>
                          <div style={{ width: '100px' }}>
                            <CProgress
                              value={calculateProgress(action)}
                              color={action.status === 'Completed' ? 'success' : 'primary'}
                              height={10}
                            />
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CDropdown>
                            <CDropdownToggle color="secondary" size="sm" caret={false}>
                              <FontAwesomeIcon icon={faEllipsisV} />
                            </CDropdownToggle>
                            <CDropdownMenu>
                              <CDropdownItem onClick={() => {
                                setSelectedAction(action);
                                setShowModal(true);
                              }}>
                                <FontAwesomeIcon icon={faEdit} className="me-2" />
                                Edit
                              </CDropdownItem>
                              {action.status !== 'Completed' && (
                                <CDropdownItem>
                                  <FontAwesomeIcon icon={faCheckCircle} className="me-2" />
                                  Mark Complete
                                </CDropdownItem>
                              )}
                              {action.requiresVerification && action.status === 'Completed' && !action.verifiedAt && (
                                <CDropdownItem>
                                  <FontAwesomeIcon icon={faClipboardCheck} className="me-2" />
                                  Verify
                                </CDropdownItem>
                              )}
                              <CDropdownItem>
                                <FontAwesomeIcon icon={faChartLine} className="me-2" />
                                View Details
                              </CDropdownItem>
                            </CDropdownMenu>
                          </CDropdown>
                        </CTableDataCell>
                      </CTableRow>
                    ))
                  )}
                </CTableBody>
              </CTable>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* Create/Edit Modal */}
      <CModal
        visible={showModal}
        onClose={() => {
          setShowModal(false);
          resetForm();
        }}
        size="lg"
      >
        <CForm onSubmit={handleSubmit}>
          <CModalHeader>
            <CModalTitle>
              {selectedAction ? 'Edit Mitigation Action' : 'Add New Mitigation Action'}
            </CModalTitle>
          </CModalHeader>
          <CModalBody>
            {error2 && (
              <CAlert color="danger" dismissible onClose={() => setError2(null)}>
                {error2}
              </CAlert>
            )}

            <CRow className="mb-3">
              <CCol md={12}>
                <CFormLabel htmlFor="actionDescription">Action Description *</CFormLabel>
                <CFormTextarea
                  id="actionDescription"
                  rows={3}
                  value={formData.actionDescription}
                  onChange={(e) => setFormData({ ...formData, actionDescription: e.target.value })}
                  placeholder="Describe the mitigation action to be taken..."
                  required
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="type">Control Type *</CFormLabel>
                <CFormSelect
                  id="type"
                  value={formData.type}
                  onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                  required
                >
                  <option value="Elimination">Elimination</option>
                  <option value="Substitution">Substitution</option>
                  <option value="Engineering">Engineering Controls</option>
                  <option value="Administrative">Administrative Controls</option>
                  <option value="PPE">Personal Protective Equipment</option>
                </CFormSelect>
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="priority">Priority *</CFormLabel>
                <CFormSelect
                  id="priority"
                  value={formData.priority}
                  onChange={(e) => setFormData({ ...formData, priority: e.target.value })}
                  required
                >
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </CFormSelect>
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="assignedTo">Assign To *</CFormLabel>
                <CFormSelect
                  id="assignedTo"
                  value={formData.assignedToId}
                  onChange={(e) => setFormData({ ...formData, assignedToId: parseInt(e.target.value) })}
                  required
                >
                  <option value="">Select user...</option>
                  {availableUsers?.map((user) => (
                    <option key={user.id} value={user.id}>
                      {user.name} - {user.department}
                    </option>
                  ))}
                </CFormSelect>
                <CFormInput
                  type="text"
                  placeholder="Search users..."
                  className="mt-2"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="targetDate">Target Date *</CFormLabel>
                <CFormInput
                  type="date"
                  id="targetDate"
                  value={formData.targetDate}
                  onChange={(e) => setFormData({ ...formData, targetDate: e.target.value })}
                  min={new Date().toISOString().split('T')[0]}
                  required
                />
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="estimatedCost">Estimated Cost ($)</CFormLabel>
                <CFormInput
                  type="number"
                  id="estimatedCost"
                  value={formData.estimatedCost}
                  onChange={(e) => setFormData({ ...formData, estimatedCost: parseFloat(e.target.value) || 0 })}
                  min="0"
                  step="0.01"
                  placeholder="0.00"
                />
              </CCol>
              <CCol md={6}>
                <div className="mt-4">
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      id="requiresVerification"
                      checked={formData.requiresVerification}
                      onChange={(e) => setFormData({ ...formData, requiresVerification: e.target.checked })}
                    />
                    <label className="form-check-label" htmlFor="requiresVerification">
                      Requires verification after completion
                    </label>
                  </div>
                </div>
              </CCol>
            </CRow>
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
            <CButton color="primary" type="submit" disabled={isCreating}>
              {isCreating ? (
                <>
                  <CSpinner size="sm" className="me-2" />
                  Creating...
                </>
              ) : (
                <>
                  <FontAwesomeIcon icon={faCheck} className="me-2" />
                  {selectedAction ? 'Update' : 'Create'} Action
                </>
              )}
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>
    </>
  );
};

export default MitigationActions;