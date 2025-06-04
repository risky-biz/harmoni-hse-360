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
  CListGroup,
  CListGroupItem,
  CBadge,
  CSpinner,
  CAlert,
  CInputGroup,
  CFormInput,
  CInputGroupText,
  CRow,
  CCol,
} from '@coreui/react';
import { Icon } from '../../../components/common/Icon';
import { useGetAvailableUsersQuery } from '../incidentApi';
import { InvolvedPersonDto } from '../../../types/incident';
import { faNotesMedical, faEdit, faTrash, faSearch } from '@fortawesome/free-solid-svg-icons';

interface InvolvedPersonsModalProps {
  visible: boolean;
  onClose: () => void;
  incidentId: number;
  involvedPersons: InvolvedPersonDto[];
  onAdd: (personId: number, involvementType: string, injuryDescription?: string) => Promise<void>;
  onUpdate: (personId: number, involvementType: string, injuryDescription?: string) => Promise<void>;
  onRemove: (personId: number) => Promise<void>;
}

export const InvolvedPersonsModal: React.FC<InvolvedPersonsModalProps> = ({
  visible,
  onClose,
  involvedPersons,
  onAdd,
  onUpdate,
  onRemove,
}) => {
  const [selectedPersonId, setSelectedPersonId] = useState<number | null>(null);
  const [involvementType, setInvolvementType] = useState<string>('Witness');
  const [injuryDescription, setInjuryDescription] = useState<string>('');
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const { data: availableUsers, isLoading: usersLoading } = useGetAvailableUsersQuery(searchTerm);

  // Filter out already involved persons
  const filteredUsers = availableUsers?.filter(
    user => !involvedPersons.some(ip => ip.person.id === user.id)
  ) || [];

  const handleAdd = async () => {
    if (!selectedPersonId) return;

    setLoading(true);
    setError(null);
    try {
      await onAdd(selectedPersonId, involvementType, injuryDescription || undefined);
      resetForm();
    } catch (err) {
      setError('Failed to add involved person. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdate = async () => {
    if (!selectedPersonId) return;

    setLoading(true);
    setError(null);
    try {
      await onUpdate(selectedPersonId, involvementType, injuryDescription || undefined);
      resetForm();
    } catch (err) {
      setError('Failed to update involved person. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleRemove = async (personId: number) => {
    if (!confirm('Are you sure you want to remove this person?')) return;

    setLoading(true);
    setError(null);
    try {
      await onRemove(personId);
    } catch (err) {
      setError('Failed to remove involved person. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (person: InvolvedPersonDto) => {
    setSelectedPersonId(person.person.id);
    setInvolvementType(person.involvementType);
    setInjuryDescription(person.injuryDescription || '');
    setIsEditing(true);
  };

  const resetForm = () => {
    setSelectedPersonId(null);
    setInvolvementType('Witness');
    setInjuryDescription('');
    setIsEditing(false);
    setSearchTerm('');
  };

  const getInvolvementBadge = (type: string) => {
    const colorMap: Record<string, string> = {
      Witness: 'info',
      Victim: 'danger',
      FirstResponder: 'success',
    };
    return <CBadge color={colorMap[type] || 'secondary'}>{type}</CBadge>;
  };

  useEffect(() => {
    if (!visible) {
      resetForm();
      setError(null);
    }
  }, [visible]);

  return (
    <CModal visible={visible} onClose={onClose} size="lg">
      <CModalHeader>
        <CModalTitle>Manage Involved Persons</CModalTitle>
      </CModalHeader>
      <CModalBody>
        {error && (
          <CAlert color="danger" dismissible onClose={() => setError(null)}>
            {error}
          </CAlert>
        )}

        {/* Current Involved Persons */}
        <div className="mb-4">
          <h6 className="mb-3">Current Involved Persons ({involvedPersons.length})</h6>
          <CListGroup>
            {involvedPersons.length === 0 ? (
              <CListGroupItem>
                <em className="text-muted">No persons involved in this incident yet.</em>
              </CListGroupItem>
            ) : (
              involvedPersons.map((person) => (
                <CListGroupItem key={person.person.id} className="d-flex justify-content-between align-items-start">
                  <div>
                    <div className="fw-bold">{person.person.fullName}</div>
                    <div className="text-muted small">{person.person.email}</div>
                    {person.injuryDescription && (
                      <div className="mt-1">
                        <small className="text-danger">
                          <Icon icon={faNotesMedical} /> {person.injuryDescription}
                        </small>
                      </div>
                    )}
                  </div>
                  <div className="d-flex align-items-center gap-2">
                    {getInvolvementBadge(person.involvementType)}
                    <CButton
                      color="primary"
                      variant="ghost"
                      size="sm"
                      onClick={() => handleEdit(person)}
                      disabled={loading}
                    >
                      <Icon icon={faEdit} />
                    </CButton>
                    <CButton
                      color="danger"
                      variant="ghost"
                      size="sm"
                      onClick={() => handleRemove(person.person.id)}
                      disabled={loading}
                    >
                      <Icon icon={faTrash} />
                    </CButton>
                  </div>
                </CListGroupItem>
              ))
            )}
          </CListGroup>
        </div>

        {/* Add/Edit Form */}
        <div className="border-top pt-4">
          <h6 className="mb-3">{isEditing ? 'Edit Involved Person' : 'Add Involved Person'}</h6>
          <CForm>
            <CRow>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel>Person</CFormLabel>
                  {!isEditing ? (
                    <>
                      <CInputGroup className="mb-2">
                        <CInputGroupText>
                          <Icon icon={faSearch} />
                        </CInputGroupText>
                        <CFormInput
                          type="text"
                          placeholder="Search by name or email..."
                          value={searchTerm}
                          onChange={(e) => setSearchTerm(e.target.value)}
                        />
                      </CInputGroup>
                      <CFormSelect
                        value={selectedPersonId || ''}
                        onChange={(e) => setSelectedPersonId(Number(e.target.value))}
                        disabled={usersLoading}
                      >
                        <option value="">Select a person...</option>
                        {filteredUsers.map((user) => (
                          <option key={user.id} value={user.id}>
                            {user.fullName} ({user.email})
                          </option>
                        ))}
                      </CFormSelect>
                    </>
                  ) : (
                    <CFormInput
                      value={involvedPersons.find(p => p.person.id === selectedPersonId)?.person.fullName || ''}
                      disabled
                    />
                  )}
                </div>
              </CCol>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel>Involvement Type</CFormLabel>
                  <CFormSelect
                    value={involvementType}
                    onChange={(e) => setInvolvementType(e.target.value)}
                  >
                    <option value="Witness">Witness</option>
                    <option value="Victim">Victim</option>
                    <option value="FirstResponder">First Responder</option>
                  </CFormSelect>
                </div>
              </CCol>
            </CRow>

            {involvementType === 'Victim' && (
              <div className="mb-3">
                <CFormLabel>Injury Description</CFormLabel>
                <CFormTextarea
                  rows={3}
                  value={injuryDescription}
                  onChange={(e) => setInjuryDescription(e.target.value)}
                  placeholder="Describe any injuries sustained..."
                />
              </div>
            )}

            <div className="d-flex gap-2">
              {isEditing ? (
                <>
                  <CButton
                    color="primary"
                    onClick={handleUpdate}
                    disabled={loading || !selectedPersonId}
                  >
                    {loading ? <CSpinner size="sm" /> : 'Update Person'}
                  </CButton>
                  <CButton color="secondary" onClick={resetForm} disabled={loading}>
                    Cancel
                  </CButton>
                </>
              ) : (
                <CButton
                  color="primary"
                  onClick={handleAdd}
                  disabled={loading || !selectedPersonId}
                >
                  {loading ? <CSpinner size="sm" /> : 'Add Person'}
                </CButton>
              )}
            </div>
          </CForm>
        </div>
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" onClick={onClose}>
          Close
        </CButton>
      </CModalFooter>
    </CModal>
  );
};