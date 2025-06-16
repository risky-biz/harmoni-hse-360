import React, { useState, useEffect } from 'react';
import {
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CButton,
  CForm,
  CFormLabel,
  CFormSelect,
  CFormTextarea,
  CFormInput,
  CSpinner,
  CAlert,
  CInputGroup,
  CInputGroupText,
  CRow,
  CCol,
  CBadge,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faUserPlus,
  faSearch,
  faUser,
  faEnvelope,
} from '@fortawesome/free-solid-svg-icons';
import { useGetAvailableUsersQuery } from '../../features/incidents/incidentApi';

interface PPEAssignmentModalProps {
  visible: boolean;
  onClose: () => void;
  ppeItemId: number;
  ppeItemName: string;
  ppeItemCode: string;
  onAssign: (userId: number, purpose: string, notes?: string) => Promise<void>;
}

export const PPEAssignmentModal: React.FC<PPEAssignmentModalProps> = ({
  visible,
  onClose,
  ppeItemId,
  ppeItemName,
  ppeItemCode,
  onAssign,
}) => {
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [purpose, setPurpose] = useState<string>('Standard Assignment');
  const [notes, setNotes] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const { data: availableUsers, isLoading: usersLoading } =
    useGetAvailableUsersQuery(searchTerm);

  const filteredUsers = availableUsers || [];

  // Reset form when modal opens
  useEffect(() => {
    if (visible) {
      setSelectedUserId(null);
      setPurpose('Standard Assignment');
      setNotes('');
      setSearchTerm('');
      setError(null);
    }
  }, [visible]);

  const handleAssign = async () => {
    if (!selectedUserId) {
      setError('Please select a user to assign the PPE item to.');
      return;
    }

    setLoading(true);
    setError(null);
    
    try {
      await onAssign(selectedUserId, purpose, notes || undefined);
      onClose();
    } catch (error: any) {
      const errorMessage = error.data?.message || 'Failed to assign PPE item. Please try again.';
      setError(errorMessage);
      console.error('Assignment error:', error);
    } finally {
      setLoading(false);
    }
  };

  const selectedUser = filteredUsers.find(user => user.id === selectedUserId);

  const assignmentPurposes = [
    'Standard Assignment',
    'Temporary Assignment',
    'Emergency Assignment',
    'Training Purpose',
    'Project Assignment',
    'Inspection Assignment',
    'Maintenance Assignment',
  ];

  return (
    <CModal
      visible={visible}
      onClose={onClose}
      backdrop="static"
      size="lg"
    >
      <CModalHeader>
        <CModalTitle>
          <FontAwesomeIcon icon={faUserPlus} className="me-2" />
          Assign PPE Item
        </CModalTitle>
      </CModalHeader>
      
      <CModalBody>
        {error && (
          <CAlert color="danger" dismissible onClose={() => setError(null)}>
            {error}
          </CAlert>
        )}

        {/* PPE Item Information */}
        <div className="mb-4 p-3 bg-light rounded">
          <h6 className="mb-2">PPE Item Details</h6>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <strong>{ppeItemCode}</strong> - {ppeItemName}
            </div>
            <CBadge color="primary">Available</CBadge>
          </div>
        </div>

        <CForm>
          {/* User Search and Selection */}
          <CRow className="mb-3">
            <CCol>
              <CFormLabel htmlFor="userSearch">
                <FontAwesomeIcon icon={faSearch} className="me-2" />
                Search and Select User
              </CFormLabel>
              <CInputGroup className="mb-2">
                <CInputGroupText>
                  <FontAwesomeIcon icon={faSearch} />
                </CInputGroupText>
                <CFormInput
                  id="userSearch"
                  placeholder="Search by name or email..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </CInputGroup>
              
              {usersLoading && (
                <div className="text-center py-2">
                  <CSpinner size="sm" /> Loading users...
                </div>
              )}

              <CFormSelect
                value={selectedUserId || ''}
                onChange={(e) => setSelectedUserId(e.target.value ? Number(e.target.value) : null)}
                size="sm"
                className="mb-2"
              >
                <option value="">Select a user...</option>
                {filteredUsers.map((user) => (
                  <option key={user.id} value={user.id}>
                    {user.fullName} ({user.email})
                  </option>
                ))}
              </CFormSelect>

              {selectedUser && (
                <div className="mt-2 p-2 border rounded bg-light">
                  <small className="text-muted">Selected User:</small>
                  <div className="d-flex align-items-center mt-1">
                    <FontAwesomeIcon icon={faUser} className="me-2 text-primary" />
                    <div>
                      <strong>{selectedUser.fullName}</strong>
                      <br />
                      <small className="text-muted">
                        <FontAwesomeIcon icon={faEnvelope} className="me-1" />
                        {selectedUser.email}
                      </small>
                    </div>
                  </div>
                </div>
              )}
            </CCol>
          </CRow>

          {/* Assignment Purpose */}
          <CRow className="mb-3">
            <CCol>
              <CFormLabel htmlFor="purpose">Assignment Purpose</CFormLabel>
              <CFormSelect
                id="purpose"
                value={purpose}
                onChange={(e) => setPurpose(e.target.value)}
              >
                {assignmentPurposes.map((purposeOption) => (
                  <option key={purposeOption} value={purposeOption}>
                    {purposeOption}
                  </option>
                ))}
              </CFormSelect>
            </CCol>
          </CRow>

          {/* Assignment Notes */}
          <CRow className="mb-3">
            <CCol>
              <CFormLabel htmlFor="notes">Assignment Notes (Optional)</CFormLabel>
              <CFormTextarea
                id="notes"
                rows={3}
                placeholder="Add any additional notes about this assignment..."
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
              />
              <small className="text-muted">
                {notes.length}/500 characters
              </small>
            </CCol>
          </CRow>
        </CForm>
      </CModalBody>

      <CModalFooter>
        <CButton
          color="secondary"
          onClick={onClose}
          disabled={loading}
        >
          Cancel
        </CButton>
        <CButton
          color="primary"
          onClick={handleAssign}
          disabled={loading || !selectedUserId}
        >
          {loading ? (
            <>
              <CSpinner size="sm" className="me-2" />
              Assigning...
            </>
          ) : (
            <>
              <FontAwesomeIcon icon={faUserPlus} className="me-2" />
              Assign PPE Item
            </>
          )}
        </CButton>
      </CModalFooter>
    </CModal>
  );
};

export default PPEAssignmentModal;