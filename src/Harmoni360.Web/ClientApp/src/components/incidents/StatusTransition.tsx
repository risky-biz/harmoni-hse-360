import React, { useState } from 'react';
import {
  CButton,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CFormTextarea,
  CFormLabel,
  CAlert,
  CSpinner,
  CBadge,
  CCard,
  CCardBody,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faCheckCircle, 
  faArrowRight, 
  faExclamationTriangle,
  faInfoCircle,
  faClock,
  faSearch
} from '@fortawesome/free-solid-svg-icons';

interface StatusTransitionProps {
  currentStatus: string;
  incidentId: number;
  onStatusChange: (newStatus: string, comment: string) => Promise<void>;
  disabled?: boolean;
}

interface StatusOption {
  value: string;
  label: string;
  description: string;
  icon: any;
  color: string;
  requiresComment: boolean;
}

const STATUS_TRANSITIONS: Record<string, StatusOption[]> = {
  'Reported': [
    {
      value: 'UnderInvestigation',
      label: 'Start Investigation',
      description: 'Begin formal investigation of the incident',
      icon: faSearch,
      color: 'warning',
      requiresComment: true,
    },
    {
      value: 'Resolved',
      label: 'Mark as Resolved',
      description: 'Close as resolved without investigation',
      icon: faCheckCircle,
      color: 'success',
      requiresComment: true,
    },
  ],
  'UnderInvestigation': [
    {
      value: 'AwaitingAction',
      label: 'Awaiting Action',
      description: 'Investigation complete, awaiting corrective actions',
      icon: faClock,
      color: 'info',
      requiresComment: true,
    },
    {
      value: 'Resolved',
      label: 'Mark as Resolved',
      description: 'Investigation complete, incident resolved',
      icon: faCheckCircle,
      color: 'success',
      requiresComment: true,
    },
    {
      value: 'Reported',
      label: 'Return to Reported',
      description: 'Return to reported status for re-evaluation',
      icon: faExclamationTriangle,
      color: 'secondary',
      requiresComment: true,
    },
  ],
  'AwaitingAction': [
    {
      value: 'Resolved',
      label: 'Mark as Resolved',
      description: 'All actions completed, incident resolved',
      icon: faCheckCircle,
      color: 'success',
      requiresComment: true,
    },
    {
      value: 'UnderInvestigation',
      label: 'Return to Investigation',
      description: 'Additional investigation required',
      icon: faSearch,
      color: 'warning',
      requiresComment: true,
    },
  ],
  'Resolved': [
    {
      value: 'Closed',
      label: 'Close Incident',
      description: 'Permanently close the incident',
      icon: faCheckCircle,
      color: 'secondary',
      requiresComment: false,
    },
    {
      value: 'UnderInvestigation',
      label: 'Reopen for Investigation',
      description: 'Reopen incident for further investigation',
      icon: faSearch,
      color: 'warning',
      requiresComment: true,
    },
  ],
  'Closed': [
    {
      value: 'UnderInvestigation',
      label: 'Reopen Incident',
      description: 'Reopen closed incident for investigation',
      icon: faSearch,
      color: 'warning',
      requiresComment: true,
    },
  ],
};

const StatusTransition: React.FC<StatusTransitionProps> = ({
  currentStatus,
  incidentId,
  onStatusChange,
  disabled = false,
}) => {
  const [showModal, setShowModal] = useState(false);
  const [selectedStatus, setSelectedStatus] = useState<StatusOption | null>(null);
  const [comment, setComment] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const availableTransitions = STATUS_TRANSITIONS[currentStatus] || [];

  const handleStatusSelect = (status: StatusOption) => {
    setSelectedStatus(status);
    setComment('');
    setError(null);
  };

  const handleSubmit = async () => {
    if (!selectedStatus) return;

    if (selectedStatus.requiresComment && !comment.trim()) {
      setError('Please provide a comment for this status change.');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      await onStatusChange(selectedStatus.value, comment);
      setShowModal(false);
      setSelectedStatus(null);
      setComment('');
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Failed to update status');
    } finally {
      setIsSubmitting(false);
    }
  };

  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'Reported': return 'primary';
      case 'UnderInvestigation': return 'warning';
      case 'AwaitingAction': return 'info';
      case 'Resolved': return 'success';
      case 'Closed': return 'secondary';
      default: return 'primary';
    }
  };

  if (availableTransitions.length === 0) {
    return null;
  }

  return (
    <>
      <CButton
        color="primary"
        variant="outline"
        size="sm"
        onClick={() => setShowModal(true)}
        disabled={disabled}
      >
        <FontAwesomeIcon icon={faArrowRight} className="me-2" />
        Change Status
      </CButton>

      <CModal
        visible={showModal}
        onClose={() => {
          setShowModal(false);
          setSelectedStatus(null);
          setComment('');
          setError(null);
        }}
        size="lg"
      >
        <CModalHeader>
          <CModalTitle>Change Incident Status</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <div className="mb-4">
            <div className="d-flex align-items-center mb-3">
              <span className="text-muted me-2">Current Status:</span>
              <CBadge color={getStatusColor(currentStatus)}>
                {currentStatus}
              </CBadge>
            </div>

            {error && (
              <CAlert color="danger" dismissible onClose={() => setError(null)}>
                {error}
              </CAlert>
            )}

            <h6 className="mb-3">Available Status Transitions:</h6>
            
            <div className="d-grid gap-2">
              {availableTransitions.map((option) => (
                <CCard
                  key={option.value}
                  className={`cursor-pointer transition-all ${
                    selectedStatus?.value === option.value 
                      ? 'border-primary shadow-sm' 
                      : 'border'
                  }`}
                  onClick={() => handleStatusSelect(option)}
                  style={{ cursor: 'pointer' }}
                >
                  <CCardBody className="py-3">
                    <div className="d-flex align-items-start">
                      <div className={`me-3 text-${option.color}`}>
                        <FontAwesomeIcon icon={option.icon} size="lg" />
                      </div>
                      <div className="flex-grow-1">
                        <div className="d-flex align-items-center justify-content-between">
                          <h6 className="mb-1">{option.label}</h6>
                          <CBadge color={option.color}>
                            {option.value}
                          </CBadge>
                        </div>
                        <p className="text-muted small mb-0">
                          {option.description}
                        </p>
                        {option.requiresComment && (
                          <div className="mt-1">
                            <FontAwesomeIcon 
                              icon={faInfoCircle} 
                              className="text-info me-1" 
                              size="sm"
                            />
                            <small className="text-info">Comment required</small>
                          </div>
                        )}
                      </div>
                    </div>
                  </CCardBody>
                </CCard>
              ))}
            </div>

            {selectedStatus && (
              <div className="mt-4">
                <CFormLabel htmlFor="statusComment">
                  {selectedStatus.requiresComment ? 'Comment (Required)' : 'Comment (Optional)'}
                </CFormLabel>
                <CFormTextarea
                  id="statusComment"
                  rows={3}
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  placeholder={`Provide details about why you're changing the status to "${selectedStatus.label}"...`}
                  className={selectedStatus.requiresComment && !comment.trim() ? 'is-invalid' : ''}
                />
                {selectedStatus.requiresComment && !comment.trim() && (
                  <div className="invalid-feedback">
                    Please provide a comment for this status change.
                  </div>
                )}
              </div>
            )}
          </div>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={() => {
              setShowModal(false);
              setSelectedStatus(null);
              setComment('');
              setError(null);
            }}
          >
            Cancel
          </CButton>
          <CButton
            color="primary"
            onClick={handleSubmit}
            disabled={!selectedStatus || isSubmitting || (selectedStatus.requiresComment && !comment.trim())}
          >
            {isSubmitting ? (
              <>
                <CSpinner size="sm" className="me-2" />
                Updating...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faCheckCircle} className="me-2" />
                Update Status
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default StatusTransition;