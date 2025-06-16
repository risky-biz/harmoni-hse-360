import React, { useState } from 'react';
import { useEmergencyContact } from '../../contexts/CompanyConfigurationContext';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CListGroup,
  CListGroupItem,
  CBadge,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CSpinner,
  CAlert
} from '@coreui/react';
import { EmergencyContactDto, useTriggerEmergencyNotificationMutation } from '../../features/health/healthApi';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faUser, 
  faExclamationTriangle, 
  faBell, 
  faPhone, 
  faEnvelope
} from '@fortawesome/free-solid-svg-icons';

interface EmergencyContactQuickAccessProps {
  emergencyContacts: EmergencyContactDto[];
  personName: string;
  emergencyType?: string;
  compact?: boolean;
}

const EmergencyContactQuickAccess: React.FC<EmergencyContactQuickAccessProps> = ({
  emergencyContacts,
  personName,
  emergencyType = 'Medical Emergency',
  compact = false
}) => {
  const [showNotificationModal, setShowNotificationModal] = useState(false);
  const [selectedContact, setSelectedContact] = useState<EmergencyContactDto | null>(null);
  const [notificationMessage, setNotificationMessage] = useState('');
  const emergencyContactMessage = useEmergencyContact();

  const [triggerEmergencyNotification, { isLoading: isSendingNotification }] = 
    useTriggerEmergencyNotificationMutation();

  const sortedContacts = emergencyContacts
    .filter(contact => contact.authorizedForMedicalDecisions || contact.isPrimaryContact)
    .sort((a, b) => {
      if (a.isPrimaryContact && !b.isPrimaryContact) return -1;
      if (!a.isPrimaryContact && b.isPrimaryContact) return 1;
      return 0; // No contactOrder property available
    });

  const handleCallContact = (phoneNumber: string) => {
    window.open(`tel:${phoneNumber}`, '_self');
  };

  const handleEmailContact = (email: string) => {
    const subject = encodeURIComponent(`Emergency: ${personName} - ${emergencyType}`);
    const body = encodeURIComponent(
      `This is an emergency notification regarding ${personName}.\n\n` +
      `Emergency Type: ${emergencyType}\n` +
      `Time: ${new Date().toLocaleString()}\n\n` +
      `Please contact the school immediately for more information.`
    );
    window.open(`mailto:${email}?subject=${subject}&body=${body}`, '_self');
  };

  const handleNotifyContact = (contact: EmergencyContactDto) => {
    setSelectedContact(contact);
    setNotificationMessage(
      `Emergency notification for ${personName}.\n\n` +
      `Emergency Type: ${emergencyType}\n` +
      `Time: ${new Date().toLocaleString()}\n\n` +
      emergencyContactMessage
    );
    setShowNotificationModal(true);
  };

  const sendEmergencyNotification = async () => {
    if (!selectedContact) return;

    try {
      await triggerEmergencyNotification({
        personId: 0,
        personName: personName,
        location: 'Emergency',
        emergencyContactIds: [parseInt(selectedContact.id.toString())],
        message: notificationMessage,
        severity: 'High'
      }).unwrap();

      setShowNotificationModal(false);
      setSelectedContact(null);
    } catch (error) {
      console.error('Failed to send emergency notification:', error);
    }
  };

  if (sortedContacts.length === 0) {
    return (
      <CAlert color="warning" className="d-flex align-items-center">
        <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" />
        <div>
          No emergency contacts with medical decision authorization found for {personName}.
        </div>
      </CAlert>
    );
  }

  if (compact) {
    return (
      <div className="d-flex gap-2 flex-wrap">
        {sortedContacts.slice(0, 2).map((contact) => (
          <CButton
            key={contact.id}
            color="danger"
            size="sm"
            onClick={() => handleCallContact(contact.primaryPhone)}
          >
            <FontAwesomeIcon icon={faPhone} className="me-1" />
            {contact.name}
          </CButton>
        ))}
        {sortedContacts.length > 2 && (
          <CBadge color="info">+{sortedContacts.length - 2} more</CBadge>
        )}
      </div>
    );
  }

  return (
    <>
      <CCard className="border-warning">
        <CCardHeader className="bg-warning-subtle">
          <div className="d-flex align-items-center justify-content-between">
            <strong>
              <FontAwesomeIcon icon={faPhone} className="me-1" />
              Emergency Contacts - {personName}
            </strong>
            <CBadge color="warning">{emergencyType}</CBadge>
          </div>
        </CCardHeader>
        <CCardBody className="p-0">
          <CListGroup flush>
            {sortedContacts.map((contact) => (
              <CListGroupItem key={contact.id}>
                <div className="d-flex justify-content-between align-items-start">
                  <div className="flex-grow-1">
                    <div className="d-flex align-items-center mb-1">
                      <FontAwesomeIcon icon={faUser} className="me-1" size="sm" />
                      <strong className="me-2">{contact.name}</strong>
                      {contact.isPrimaryContact && (
                        <CBadge color="primary" className="me-1">Primary</CBadge>
                      )}
                      {contact.authorizedForMedicalDecisions && (
                        <CBadge color="success" className="me-1">Medical Auth</CBadge>
                      )}
                      {contact.authorizedForPickup && (
                        <CBadge color="info">Pickup Auth</CBadge>
                      )}
                    </div>
                    <div className="small text-muted mb-1">
                      <FontAwesomeIcon icon={faUser} className="me-1" size="sm" />
                      {contact.relationship}
                    </div>
                    <div className="small mb-1">
                      <FontAwesomeIcon icon={faPhone} className="me-1" size="sm" />
                      <strong>{contact.primaryPhone}</strong>
                      {contact.secondaryPhone && (
                        <span className="text-muted ms-2">• {contact.secondaryPhone}</span>
                      )}
                    </div>
                    {contact.email && (
                      <div className="small mb-1">
                        <FontAwesomeIcon icon={faEnvelope} className="me-1" size="sm" />
                        {contact.email}
                      </div>
                    )}
                  </div>
                  <div className="d-flex flex-column gap-1 ms-3">
                    <CButton
                      color="danger"
                      size="sm"
                      onClick={() => handleCallContact(contact.primaryPhone)}
                    >
                      <FontAwesomeIcon icon={faPhone} className="me-1" />
                      Call
                    </CButton>
                    {contact.secondaryPhone && (
                      <CButton
                        color="outline-danger"
                        size="sm"
                        onClick={() => handleCallContact(contact.secondaryPhone!)}
                      >
                        <FontAwesomeIcon icon={faPhone} className="me-1" />
                        Call Alt
                      </CButton>
                    )}
                    {contact.email && (
                      <CButton
                        color="warning"
                        size="sm"
                        onClick={() => handleEmailContact(contact.email!)}
                      >
                        <FontAwesomeIcon icon={faEnvelope} className="me-1" />
                        Email
                      </CButton>
                    )}
                    <CButton
                      color="info"
                      size="sm"
                      onClick={() => handleNotifyContact(contact)}
                    >
                      <FontAwesomeIcon icon={faBell} className="me-1" />
                      Notify
                    </CButton>
                  </div>
                </div>
              </CListGroupItem>
            ))}
          </CListGroup>
        </CCardBody>
      </CCard>

      {/* Emergency Notification Modal */}
      <CModal visible={showNotificationModal} onClose={() => setShowNotificationModal(false)}>
        <CModalHeader>
          <CModalTitle>Send Emergency Notification</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {selectedContact && (
            <div>
              <CAlert color="warning" className="d-flex align-items-center mb-3">
                <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" />
                <div>
                  <strong>Emergency notification will be sent to:</strong><br />
                  {selectedContact.name} ({selectedContact.relationship})
                </div>
              </CAlert>
              
              <div className="mb-3">
                <strong>Contact Methods:</strong>
                <ul className="mb-0 mt-1">
                  <li>Primary Phone: {selectedContact.primaryPhone}</li>
                  {selectedContact.secondaryPhone && (
                    <li>Secondary Phone: {selectedContact.secondaryPhone}</li>
                  )}
                  {selectedContact.email && (
                    <li>Email: {selectedContact.email}</li>
                  )}
                </ul>
              </div>

              <div className="mb-3">
                <strong>Notification Message:</strong>
                <div className="border p-2 mt-1 small bg-light">
                  {notificationMessage.split('\n').map((line, index) => (
                    <div key={index}>{line}</div>
                  ))}
                </div>
              </div>
            </div>
          )}
        </CModalBody>
        <CModalFooter>
          <CButton
            color="danger"
            onClick={sendEmergencyNotification}
            disabled={isSendingNotification}
          >
            {isSendingNotification ? (
              <>
                <CSpinner size="sm" className="me-1" />
                Sending...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faBell} className="me-1" />
                Send Emergency Notification
              </>
            )}
          </CButton>
          <CButton 
            color="secondary" 
            onClick={() => setShowNotificationModal(false)}
            disabled={isSendingNotification}
          >
            Cancel
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default EmergencyContactQuickAccess;