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
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { useGetAvailableUsersQuery } from '../incidentApi';
import { InvolvedPersonDto } from '../../../types/incident';
import {
  faNotesMedical,
  faEdit,
  faTrash,
  faSearch,
  faUserPlus,
} from '@fortawesome/free-solid-svg-icons';

interface InvolvedPersonsModalProps {
  visible: boolean;
  onClose: () => void;
  incidentId: number;
  involvedPersons: InvolvedPersonDto[];
  onAdd: (
    personId: number | string,
    involvementType: string,
    injuryDescription?: string,
    manualPersonData?: { fullName: string; email?: string }
  ) => Promise<void>;
  onUpdate: (
    personId: number | string,
    involvementType: string,
    injuryDescription?: string
  ) => Promise<void>;
  onRemove: (personId: number | string) => Promise<void>;
  embedded?: boolean;
}

export const InvolvedPersonsModal: React.FC<InvolvedPersonsModalProps> = ({
  visible,
  onClose,
  involvedPersons = [],
  onAdd,
  onUpdate,
  onRemove,
  embedded = false,
}) => {
  const [selectedPersonId, setSelectedPersonId] = useState<number | string | null>(null);
  const [involvementType, setInvolvementType] = useState<string>('Witness');
  const [injuryDescription, setInjuryDescription] = useState<string>('');
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [inputMode, setInputMode] = useState<'dropdown' | 'manual'>('dropdown');
  const [manualPersonName, setManualPersonName] = useState<string>('');
  const [manualPersonEmail, setManualPersonEmail] = useState<string>('');

  const { data: availableUsers, isLoading: usersLoading } =
    useGetAvailableUsersQuery(searchTerm);

  // Filter out already involved persons
  const safeInvolvedPersons = Array.isArray(involvedPersons) ? involvedPersons : [];
  const filteredUsers =
    availableUsers?.filter(
      (user) => !safeInvolvedPersons.some((ip) => {
        // Handle both cases: when person exists and when it's a manual entry
        try {
          return ip && ip.person && ip.person.id === user.id;
        } catch (error) {
          console.warn('Error processing involved person:', ip, error);
          return false;
        }
      })
    ) || [];

  const handleAdd = async () => {
    if (inputMode === 'dropdown' && !selectedPersonId) return;
    if (inputMode === 'manual' && !manualPersonName.trim()) return;

    setLoading(true);
    setError(null);
    try {
      if (inputMode === 'manual') {
        // For manual entry, pass 0 as personId and include manual data
        await onAdd(
          0,
          involvementType,
          injuryDescription || undefined,
          {
            fullName: manualPersonName.trim(),
            email: manualPersonEmail.trim() || undefined,
          }
        );
      } else {
        await onAdd(
          selectedPersonId as number,
          involvementType,
          injuryDescription || undefined
        );
      }
      resetForm();
    } catch (err: any) {
      console.error('Error adding involved person:', err);
      // Only show error if it's an actual error, not just a resolved promise
      if (err && (err.status || err.data || err.message)) {
        const errorMessage = err?.data?.message || err?.message || 'Failed to add involved person. Please try again.';
        setError(errorMessage);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleUpdate = async () => {
    if (!selectedPersonId) return;

    setLoading(true);
    setError(null);
    try {
      await onUpdate(
        selectedPersonId,
        involvementType,
        injuryDescription || undefined
      );
      resetForm();
    } catch (err: any) {
      console.error('Error updating involved person:', err);
      // Only show error if it's an actual error
      if (err && (err.status || err.data || err.message)) {
        const errorMessage = err?.data?.message || err?.message || 'Failed to update involved person. Please try again.';
        setError(errorMessage);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleRemove = async (personId: number | string) => {
    if (!confirm('Are you sure you want to remove this person?')) return;

    setLoading(true);
    setError(null);
    try {
      const numericPersonId = typeof personId === 'string' ? parseInt(personId, 10) : personId;
      await onRemove(numericPersonId);
    } catch (err: any) {
      console.error('Error removing involved person:', err);
      // Only show error if it's an actual error
      if (err && (err.status || err.data || err.message)) {
        const errorMessage = err?.data?.message || err?.message || 'Failed to remove involved person. Please try again.';
        setError(errorMessage);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (person: InvolvedPersonDto) => {
    setSelectedPersonId(person.person?.id || person.id);
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
    setInputMode('dropdown');
    setManualPersonName('');
    setManualPersonEmail('');
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

  const content = (
    <>
      {!embedded && (
        <CModalHeader>
          <CModalTitle>Manage Involved Persons</CModalTitle>
        </CModalHeader>
      )}
      <div className={embedded ? '' : 'modal-body'}>
        {error && (
          <CAlert color="danger" dismissible onClose={() => setError(null)}>
            {error}
          </CAlert>
        )}

        {/* Current Involved Persons */}
        <div className="mb-4">
          <h6 className="mb-3">
            Current Involved Persons ({safeInvolvedPersons.length})
          </h6>
          <CListGroup>
            {safeInvolvedPersons.length === 0 ? (
              <CListGroupItem>
                <em className="text-muted">
                  No persons involved in this incident yet.
                </em>
              </CListGroupItem>
            ) : (
              safeInvolvedPersons.map((person) => (
                <CListGroupItem
                  key={person.id}
                  className="d-flex justify-content-between align-items-start"
                >
                  <div>
                    <div className="fw-bold">
                      {person.person ? person.person.fullName : person.manualPersonName}
                    </div>
                    <div className="text-muted small">
                      {person.person ? person.person.email : person.manualPersonEmail || 'No email provided'}
                    </div>
                    {person.injuryDescription && (
                      <div className="mt-1">
                        <small className="text-danger">
                          <Icon icon={faNotesMedical} />{' '}
                          {person.injuryDescription}
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
                      onClick={() => {
                        // For manual entries without person.id, use the record id
                        const idToRemove = person.person?.id || person.id;
                        handleRemove(idToRemove);
                      }}
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
          <h6 className="mb-3">
            {isEditing ? 'Edit Involved Person' : 'Add Involved Person'}
          </h6>
          <CForm>
            <CRow>
              <CCol md={6}>
                <div className="mb-3">
                  <CFormLabel>Person</CFormLabel>
                  {!isEditing ? (
                    <>
                      {inputMode === 'dropdown' && (
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
                      )}
                      <CInputGroup>
                        {inputMode === 'dropdown' ? (
                          <CFormSelect
                            value={selectedPersonId || ''}
                            onChange={(e) =>
                              setSelectedPersonId(Number(e.target.value))
                            }
                            disabled={usersLoading}
                          >
                            <option value="">Select a person...</option>
                            {filteredUsers.map((user) => (
                              <option key={user.id} value={user.id}>
                                {user.fullName} ({user.email})
                              </option>
                            ))}
                          </CFormSelect>
                        ) : (
                          <>
                            <CFormInput
                              type="text"
                              placeholder="Enter person's full name..."
                              value={manualPersonName}
                              onChange={(e) => setManualPersonName(e.target.value)}
                              required
                            />
                          </>
                        )}
                        <CButton
                          type="button"
                          color="primary"
                          variant="outline"
                          onClick={() => {
                            setInputMode(inputMode === 'dropdown' ? 'manual' : 'dropdown');
                            setSelectedPersonId(null);
                            setManualPersonName('');
                            setManualPersonEmail('');
                          }}
                          title={inputMode === 'dropdown' ? 'Enter person manually' : 'Select from existing users'}
                        >
                          {inputMode === 'dropdown' ? (
                            <FontAwesomeIcon icon={faUserPlus} />
                          ) : (
                            '↩'
                          )}
                        </CButton>
                      </CInputGroup>
                      {inputMode === 'manual' && (
                        <CFormInput
                          type="email"
                          placeholder="Email address (optional)"
                          value={manualPersonEmail}
                          onChange={(e) => setManualPersonEmail(e.target.value)}
                          className="mt-2"
                        />
                      )}
                      <small className="text-muted">
                        {inputMode === 'dropdown'
                          ? 'Select from existing users or click the button to enter manually'
                          : 'Enter person details manually. Click ↩ to select from existing users.'}
                      </small>
                    </>
                  ) : (
                    <CFormInput
                      value={
                        safeInvolvedPersons.find(
                          (p) => (p.person?.id || p.id) === selectedPersonId
                        )?.person?.fullName || 
                        safeInvolvedPersons.find(
                          (p) => (p.person?.id || p.id) === selectedPersonId
                        )?.manualPersonName || ''
                      }
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
                  <CButton
                    color="secondary"
                    onClick={resetForm}
                    disabled={loading}
                  >
                    Cancel
                  </CButton>
                </>
              ) : (
                <CButton
                  color="primary"
                  onClick={handleAdd}
                  disabled={loading || (inputMode === 'dropdown' ? !selectedPersonId : !manualPersonName.trim())}
                >
                  {loading ? <CSpinner size="sm" /> : 'Add Person'}
                </CButton>
              )}
            </div>
          </CForm>
        </div>
      </div>
      {!embedded && (
        <CModalFooter>
          <CButton color="secondary" onClick={onClose}>
            Close
          </CButton>
        </CModalFooter>
      )}
    </>
  );

  if (embedded) {
    return <>{visible && content}</>;
  }

  return (
    <CModal visible={visible} onClose={onClose} size="lg">
      {content}
    </CModal>
  );
};
