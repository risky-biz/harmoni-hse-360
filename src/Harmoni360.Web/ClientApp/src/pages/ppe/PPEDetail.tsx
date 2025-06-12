import React, { useState } from 'react';
import { useParams, useNavigate, useLocation, Navigate } from 'react-router-dom';
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
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CDropdownDivider,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faShieldAlt,
  faArrowLeft,
  faEdit,
  faTrash,
  faCertificate,
  faTools,
  faUserPlus,
  faUndo,
  faExclamationTriangle,
  faEllipsisV,
} from '@fortawesome/free-solid-svg-icons';
import {
  useGetPPEItemQuery,
  useDeletePPEItemMutation,
  useAssignPPEMutation,
  useReturnPPEMutation,
  useMarkPPEAsLostMutation,
} from '../../features/ppe/ppeApi';
import {
  getPPEStatusBadge,
  getPPEConditionBadge,
  formatDate,
  formatDateTime,
  formatCurrency,
} from '../../utils/ppeUtils';
import PPEAssignmentModal from '../../components/ppe/PPEAssignmentModal';

const PPEDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [showAssignmentModal, setShowAssignmentModal] = useState(false);

  // Validate the ID parameter
  const itemId = id ? parseInt(id, 10) : null;
  
  // Redirect to PPE list if ID is invalid
  if (!id || !itemId || isNaN(itemId)) {
    return <Navigate to="/ppe" replace />;
  }

  const {
    data: ppeItem,
    error,
    isLoading,
    refetch,
  } = useGetPPEItemQuery(itemId);
  
  const [deletePPEItem, { isLoading: isDeleting }] = useDeletePPEItemMutation();
  const [assignPPE, { isLoading: isAssigning }] = useAssignPPEMutation();
  const [returnPPE, { isLoading: isReturning }] = useReturnPPEMutation();
  const [markAsLost, { isLoading: isMarkingLost }] = useMarkPPEAsLostMutation();

  // Handle success message from navigation state
  React.useEffect(() => {
    if (location.state?.message) {
      setSuccessMessage(location.state.message);
      // Clear the navigation state
      window.history.replaceState({}, document.title);
      // Auto-hide message after 5 seconds
      const timer = setTimeout(() => setSuccessMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [location.state]);

  const handleAction = async (action: () => Promise<any>, actionName: string) => {
    setActionError(null);
    try {
      await action();
      setSuccessMessage(`PPE item ${actionName} successfully!`);
      refetch();
    } catch (error: any) {
      const errorMessage = error.data?.message || `Failed to ${actionName} PPE item. Please try again.`;
      setActionError(errorMessage);
      console.error(`${actionName} error:`, error);
    }
  };

  const handleAssign = () => {
    setShowAssignmentModal(true);
  };

  const handleAssignSubmit = async (userId: number, purpose: string, notes?: string) => {
    return handleAction(
      () => assignPPE({ 
        ppeItemId: Number(id), 
        assignedToId: userId,
        purpose: purpose,
        notes: notes
      }).unwrap(),
      'assigned'
    );
  };

  const handleReturn = () => {
    const notes = prompt('Return notes (optional):') || undefined;
    handleAction(
      () => returnPPE({ 
        ppeItemId: Number(id),
        returnNotes: notes
      }).unwrap(),
      'returned'
    );
  };

  const handleMarkAsLost = () => {
    const notes = prompt('Please provide details about how the item was lost:');
    if (notes) {
      handleAction(
        () => markAsLost({ 
          ppeItemId: Number(id),
          notes
        }).unwrap(),
        'marked as lost'
      );
    }
  };

  const handleDelete = async () => {
    if (
      window.confirm(
        `Are you sure you want to delete "${ppeItem?.itemCode} - ${ppeItem?.name}"? This action cannot be undone.`
      )
    ) {
      try {
        await deletePPEItem(Number(id)).unwrap();
        navigate('/ppe', {
          state: {
            message: 'PPE item deleted successfully!',
            type: 'success',
          },
        });
      } catch (error: any) {
        setActionError(error.data?.message || 'Failed to delete PPE item. Please try again.');
        console.error('Delete error:', error);
      }
    }
  };

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: '400px' }}
      >
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading PPE item details...</span>
      </div>
    );
  }

  if (error || !ppeItem) {
    return (
      <CAlert color="danger">
        Failed to load PPE item details. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/ppe')}>
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            Back to List
          </CButton>
        </div>
      </CAlert>
    );
  }

  const getWarningBadges = () => {
    const badges = [];
    
    if (ppeItem.isExpired) {
      badges.push(
        <CBadge key="expired" color="danger" className="me-1 mb-1">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
          Expired
        </CBadge>
      );
    } else if (ppeItem.isExpiringSoon) {
      badges.push(
        <CBadge key="expiring" color="warning" className="me-1 mb-1">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
          Expiring Soon
        </CBadge>
      );
    }
    
    if (ppeItem.isMaintenanceDue) {
      badges.push(
        <CBadge key="maintenance" color="warning" className="me-1 mb-1">
          <FontAwesomeIcon icon={faTools} className="me-1" />
          Maintenance Due
        </CBadge>
      );
    }
    
    if (ppeItem.isInspectionDue) {
      badges.push(
        <CBadge key="inspection" color="info" className="me-1 mb-1">
          Inspection Due
        </CBadge>
      );
    }
    
    return badges;
  };

  return (
    <>
      <CRow>
        <CCol xs={12}>
          <CCard className="shadow-sm">
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div>
                <h4
                  className="mb-0"
                  style={{
                    color: 'var(--harmoni-charcoal)',
                    fontFamily: 'Poppins, sans-serif',
                  }}
                >
                  <FontAwesomeIcon
                    icon={faShieldAlt}
                    className="me-2 text-primary"
                  />
                  PPE Item Details
                </h4>
                <small className="text-muted">
                  {ppeItem.itemCode} - {ppeItem.name}
                </small>
              </div>
              <div className="d-flex gap-2">
                <CButton
                  color="light"
                  onClick={() => navigate('/ppe')}
                >
                  <FontAwesomeIcon icon={faArrowLeft} size="sm" className="me-2" />
                  Back
                </CButton>
                <CButton
                  color="primary"
                  onClick={() => navigate(`/ppe/${id}/edit`)}
                >
                  <FontAwesomeIcon icon={faEdit} size="sm" className="me-2" />
                  Edit
                </CButton>
                <CDropdown>
                  <CDropdownToggle
                    color="secondary"
                    variant="outline"
                    caret={false}
                  >
                    <FontAwesomeIcon icon={faEllipsisV} />
                  </CDropdownToggle>
                  <CDropdownMenu>
                    {ppeItem.status === 'Available' && (
                      <CDropdownItem
                        onClick={handleAssign}
                        disabled={isAssigning}
                      >
                        <FontAwesomeIcon icon={faUserPlus} size="sm" className="me-2" />
                        Assign
                      </CDropdownItem>
                    )}
                    {ppeItem.status === 'Assigned' && (
                      <CDropdownItem
                        onClick={handleReturn}
                        disabled={isReturning}
                      >
                        <FontAwesomeIcon icon={faUndo} size="sm" className="me-2" />
                        Return
                      </CDropdownItem>
                    )}
                    <CDropdownItem
                      onClick={handleMarkAsLost}
                      disabled={isMarkingLost}
                    >
                      <FontAwesomeIcon icon={faExclamationTriangle} size="sm" className="me-2" />
                      Mark as Lost
                    </CDropdownItem>
                    <CDropdownDivider />
                    <CDropdownItem
                      className="text-danger"
                      onClick={handleDelete}
                      disabled={isDeleting}
                    >
                      <FontAwesomeIcon icon={faTrash} size="sm" className="me-2" />
                      Delete
                    </CDropdownItem>
                  </CDropdownMenu>
                </CDropdown>
              </div>
            </CCardHeader>

            <CCardBody>
              {successMessage && (
                <CAlert
                  color="success"
                  dismissible
                  onClose={() => setSuccessMessage(null)}
                >
                  {successMessage}
                </CAlert>
              )}

              {actionError && (
                <CAlert
                  color="danger"
                  dismissible
                  onClose={() => setActionError(null)}
                >
                  {actionError}
                </CAlert>
              )}

              <CRow>
                <CCol md={8}>
                  <div className="mb-4">
                    <h5 className="mb-3">{ppeItem.name}</h5>
                    <div className="mb-2">
                      {getWarningBadges()}
                    </div>
                  </div>

                  <div className="mb-4">
                    <h6 className="text-muted">Description</h6>
                    <p>{ppeItem.description}</p>
                  </div>

                  <CRow className="mb-4">
                    <CCol sm={6}>
                      <h6 className="text-muted">Status</h6>
                      <p>{getPPEStatusBadge(ppeItem.status)}</p>
                    </CCol>
                    <CCol sm={6}>
                      <h6 className="text-muted">Condition</h6>
                      <p>{getPPEConditionBadge(ppeItem.condition)}</p>
                    </CCol>
                  </CRow>

                  <CRow className="mb-4">
                    <CCol sm={6}>
                      <h6 className="text-muted">Category</h6>
                      <p>{ppeItem.categoryName}</p>
                    </CCol>
                    <CCol sm={6}>
                      <h6 className="text-muted">Location</h6>
                      <p>{ppeItem.location}</p>
                    </CCol>
                  </CRow>

                  <CRow className="mb-4">
                    <CCol sm={6}>
                      <h6 className="text-muted">Manufacturer</h6>
                      <p>{ppeItem.manufacturer}</p>
                    </CCol>
                    <CCol sm={6}>
                      <h6 className="text-muted">Model</h6>
                      <p>{ppeItem.model}</p>
                    </CCol>
                  </CRow>

                  <CRow className="mb-4">
                    <CCol sm={4}>
                      <h6 className="text-muted">Size</h6>
                      <p>{ppeItem.size}</p>
                    </CCol>
                    <CCol sm={4}>
                      <h6 className="text-muted">Color</h6>
                      <p>{ppeItem.color || 'Not specified'}</p>
                    </CCol>
                    <CCol sm={4}>
                      <h6 className="text-muted">Cost</h6>
                      <p>{formatCurrency(ppeItem.cost)}</p>
                    </CCol>
                  </CRow>

                  <CRow className="mb-4">
                    <CCol sm={6}>
                      <h6 className="text-muted">Purchase Date</h6>
                      <p>{formatDate(ppeItem.purchaseDate)}</p>
                    </CCol>
                    <CCol sm={6}>
                      <h6 className="text-muted">Expiry Date</h6>
                      <p>{ppeItem.expiryDate ? formatDate(ppeItem.expiryDate) : 'No expiry'}</p>
                    </CCol>
                  </CRow>

                  {ppeItem.assignedToName && (
                    <div className="mb-4">
                      <h6 className="text-muted">Assignment Information</h6>
                      <CListGroup>
                        <CListGroupItem>
                          <strong>Assigned To:</strong> {ppeItem.assignedToName}
                        </CListGroupItem>
                        {ppeItem.assignedDate && (
                          <CListGroupItem>
                            <strong>Assigned Date:</strong> {formatDate(ppeItem.assignedDate)}
                          </CListGroupItem>
                        )}
                      </CListGroup>
                    </div>
                  )}

                  {/* Certification Information */}
                  {(ppeItem.certificationNumber || ppeItem.certifyingBody || ppeItem.certificationStandard) && (
                    <div className="mb-4">
                      <h6 className="text-muted">
                        <FontAwesomeIcon icon={faCertificate} className="me-2 text-warning" />
                        Certification Information
                      </h6>
                      <CListGroup>
                        {ppeItem.certificationNumber && (
                          <CListGroupItem>
                            <strong>Certification Number:</strong> {ppeItem.certificationNumber}
                          </CListGroupItem>
                        )}
                        {ppeItem.certifyingBody && (
                          <CListGroupItem>
                            <strong>Certifying Body:</strong> {ppeItem.certifyingBody}
                          </CListGroupItem>
                        )}
                        {ppeItem.certificationStandard && (
                          <CListGroupItem>
                            <strong>Standard:</strong> {ppeItem.certificationStandard}
                          </CListGroupItem>
                        )}
                        {ppeItem.certificationDate && (
                          <CListGroupItem>
                            <strong>Certification Date:</strong> {formatDate(ppeItem.certificationDate)}
                          </CListGroupItem>
                        )}
                        {ppeItem.certificationExpiryDate && (
                          <CListGroupItem>
                            <strong>Certification Expiry:</strong> {formatDate(ppeItem.certificationExpiryDate)}
                          </CListGroupItem>
                        )}
                      </CListGroup>
                    </div>
                  )}

                  {/* Maintenance Information */}
                  {(ppeItem.maintenanceIntervalDays || ppeItem.lastMaintenanceDate || ppeItem.nextMaintenanceDate) && (
                    <div className="mb-4">
                      <h6 className="text-muted">
                        <FontAwesomeIcon icon={faTools} className="me-2 text-info" />
                        Maintenance Information
                      </h6>
                      <CListGroup>
                        {ppeItem.maintenanceIntervalDays && (
                          <CListGroupItem>
                            <strong>Maintenance Interval:</strong> {ppeItem.maintenanceIntervalDays} days
                          </CListGroupItem>
                        )}
                        {ppeItem.lastMaintenanceDate && (
                          <CListGroupItem>
                            <strong>Last Maintenance:</strong> {formatDate(ppeItem.lastMaintenanceDate)}
                          </CListGroupItem>
                        )}
                        {ppeItem.nextMaintenanceDate && (
                          <CListGroupItem>
                            <strong>Next Maintenance:</strong> {formatDate(ppeItem.nextMaintenanceDate)}
                          </CListGroupItem>
                        )}
                        {ppeItem.maintenanceInstructions && (
                          <CListGroupItem>
                            <strong>Instructions:</strong> {ppeItem.maintenanceInstructions}
                          </CListGroupItem>
                        )}
                      </CListGroup>
                    </div>
                  )}

                  {ppeItem.notes && (
                    <div className="mb-4">
                      <h6 className="text-muted">Notes</h6>
                      <p>{ppeItem.notes}</p>
                    </div>
                  )}
                </CCol>

                <CCol md={4}>
                  <div className="border-start ps-4">
                    <h6 className="text-muted mb-3">Item Information</h6>
                    <div className="mb-2">
                      <strong>Item Code:</strong> {ppeItem.itemCode}
                    </div>
                    <div className="mb-2">
                      <strong>ID:</strong> {ppeItem.id}
                    </div>
                    <div className="mb-4">
                      <strong>Category Type:</strong> {ppeItem.categoryType}
                    </div>

                    <h6 className="text-muted mb-3">Status Information</h6>
                    <div className="mb-2">
                      <strong>Current Status:</strong> {ppeItem.status}
                    </div>
                    <div className="mb-2">
                      <strong>Condition:</strong> {ppeItem.condition}
                    </div>
                    {ppeItem.isExpired && (
                      <div className="mb-2 text-danger">
                        <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                        <strong>Item has expired</strong>
                      </div>
                    )}
                    {ppeItem.isExpiringSoon && !ppeItem.isExpired && (
                      <div className="mb-2 text-warning">
                        <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                        <strong>Item expiring soon</strong>
                      </div>
                    )}
                    {ppeItem.isMaintenanceDue && (
                      <div className="mb-2 text-warning">
                        <FontAwesomeIcon icon={faTools} className="me-1" />
                        <strong>Maintenance due</strong>
                      </div>
                    )}
                    {ppeItem.isInspectionDue && (
                      <div className="mb-4 text-info">
                        <FontAwesomeIcon icon={faShieldAlt} className="me-1" />
                        <strong>Inspection due</strong>
                      </div>
                    )}

                    <h6 className="text-muted mb-3">Audit Information</h6>
                    <div className="small text-muted">
                      <p className="mb-1">
                        <strong>Created:</strong>{' '}
                        {formatDateTime(ppeItem.createdAt)}
                      </p>
                      {ppeItem.createdBy && (
                        <p className="mb-1">
                          <strong>Created By:</strong> {ppeItem.createdBy}
                        </p>
                      )}
                      {ppeItem.lastModifiedAt && (
                        <>
                          <p className="mb-1">
                            <strong>Modified:</strong>{' '}
                            {formatDateTime(ppeItem.lastModifiedAt)}
                          </p>
                          {ppeItem.lastModifiedBy && (
                            <p className="mb-1">
                              <strong>Modified By:</strong>{' '}
                              {ppeItem.lastModifiedBy}
                            </p>
                          )}
                        </>
                      )}
                    </div>

                    {/* Quick Actions */}
                    <div className="mt-4">
                      <h6 className="text-muted mb-3">Quick Actions</h6>
                      <div className="d-grid gap-2">
                        {ppeItem.status === 'Available' && (
                          <CButton
                            color="success"
                            size="sm"
                            onClick={handleAssign}
                            disabled={isAssigning}
                          >
                            {isAssigning ? (
                              <>
                                <CSpinner size="sm" className="me-2" />
                                Assigning...
                              </>
                            ) : (
                              <>
                                <FontAwesomeIcon icon={faUserPlus} className="me-2" />
                                Assign to User
                              </>
                            )}
                          </CButton>
                        )}
                        {ppeItem.status === 'Assigned' && (
                          <CButton
                            color="warning"
                            size="sm"
                            onClick={handleReturn}
                            disabled={isReturning}
                          >
                            {isReturning ? (
                              <>
                                <CSpinner size="sm" className="me-2" />
                                Processing...
                              </>
                            ) : (
                              <>
                                <FontAwesomeIcon icon={faUndo} className="me-2" />
                                Return Item
                              </>
                            )}
                          </CButton>
                        )}
                        <CButton
                          color="secondary"
                          size="sm"
                          variant="outline"
                          onClick={() => navigate(`/ppe/${id}/edit`)}
                        >
                          <FontAwesomeIcon icon={faEdit} className="me-2" />
                          Edit Details
                        </CButton>
                      </div>
                    </div>
                  </div>
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>

      {/* PPE Assignment Modal */}
      {ppeItem && (
        <PPEAssignmentModal
          visible={showAssignmentModal}
          onClose={() => setShowAssignmentModal(false)}
          ppeItemId={ppeItem.id}
          ppeItemName={ppeItem.name}
          ppeItemCode={ppeItem.itemCode}
          onAssign={handleAssignSubmit}
        />
      )}
    </>
  );
};

export default PPEDetail;