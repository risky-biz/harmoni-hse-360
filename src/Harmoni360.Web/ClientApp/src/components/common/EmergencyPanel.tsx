import React, { useState } from 'react';
import { CCard, CCardBody, CButton, CModal, CModalHeader, CModalTitle, CModalBody, CModalFooter } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faExclamationTriangle, 
  faPhone, 
  faMapMarkerAlt, 
  faFirstAid,
  faTimes,
  faPhoneAlt
} from '@fortawesome/free-solid-svg-icons';

// Emergency contact interface
interface EmergencyContact {
  id: string;
  name: string;
  number: string;
  description: string;
  priority: 'primary' | 'secondary';
}

// Emergency procedure step interface
interface EmergencyStep {
  id: string;
  title: string;
  description: string;
  icon?: typeof faFirstAid;
}

// Emergency procedure interface
interface EmergencyProcedure {
  id: string;
  title: string;
  description: string;
  steps: EmergencyStep[];
  contacts: string[]; // Contact IDs
}

// Props for the main emergency panel
interface EmergencyPanelProps {
  procedures: EmergencyProcedure[];
  contacts: EmergencyContact[];
  className?: string;
  showQuickAccess?: boolean;
}

// Props for individual procedure display
interface EmergencyProcedureCardProps {
  procedure: EmergencyProcedure;
  contacts: EmergencyContact[];
  onContactCall: (contact: EmergencyContact) => void;
}

// Emergency contact quick access component
const EmergencyContactQuickAccess: React.FC<{
  contacts: EmergencyContact[];
  onCall: (contact: EmergencyContact) => void;
}> = ({ contacts, onCall }) => {
  const primaryContacts = contacts.filter(c => c.priority === 'primary');

  return (
    <div className="emergency-contacts">
      <h6 className="mb-3 text-center">
        <FontAwesomeIcon icon={faPhoneAlt} className="me-2" />
        Emergency Contacts
      </h6>
      {primaryContacts.map(contact => (
        <div key={contact.id} className="emergency-contact">
          <div>
            <div className="contact-name">{contact.name}</div>
            <div className="text-sm opacity-75">{contact.description}</div>
          </div>
          <div className="d-flex align-items-center gap-2">
            <span className="contact-number">{contact.number}</span>
            <button
              className="call-button"
              onClick={() => onCall(contact)}
              aria-label={`Call ${contact.name}`}
            >
              <FontAwesomeIcon icon={faPhone} className="me-1" />
              Call
            </button>
          </div>
        </div>
      ))}
    </div>
  );
};

// Emergency procedure card component
const EmergencyProcedureCard: React.FC<EmergencyProcedureCardProps> = ({
  procedure,
  contacts,
  onContactCall
}) => {
  const [showSteps, setShowSteps] = useState(false);
  const procedureContacts = contacts.filter(c => procedure.contacts.includes(c.id));

  return (
    <CCard className="emergency-procedures mb-3">
      <CCardBody>
        <div className="emergency-header">
          <FontAwesomeIcon icon={faExclamationTriangle} className="emergency-icon" />
          <h5 className="emergency-title mb-0">{procedure.title}</h5>
        </div>
        
        <div className="emergency-content">
          <p className="mb-3">{procedure.description}</p>
          
          <CButton
            color="danger"
            variant="outline"
            size="sm"
            onClick={() => setShowSteps(!showSteps)}
            className="mb-3"
          >
            {showSteps ? 'Hide' : 'Show'} Emergency Steps
          </CButton>

          {showSteps && (
            <ol className="emergency-steps" style={{ counterReset: 'step' }}>
              {procedure.steps.map(step => (
                <li key={step.id} className="emergency-step">
                  <strong>{step.title}</strong>
                  {step.description && <div className="mt-1">{step.description}</div>}
                </li>
              ))}
            </ol>
          )}

          {procedureContacts.length > 0 && (
            <div className="mt-3">
              <h6>Related Emergency Contacts:</h6>
              <div className="d-flex flex-wrap gap-2">
                {procedureContacts.map(contact => (
                  <CButton
                    key={contact.id}
                    color="danger"
                    size="sm"
                    onClick={() => onContactCall(contact)}
                  >
                    <FontAwesomeIcon icon={faPhone} className="me-1" />
                    {contact.name}: {contact.number}
                  </CButton>
                ))}
              </div>
            </div>
          )}
        </div>
      </CCardBody>
    </CCard>
  );
};

// Main emergency panel component
export const EmergencyPanel: React.FC<EmergencyPanelProps> = ({
  procedures,
  contacts,
  className = '',
  showQuickAccess = true
}) => {
  const [showContactModal, setShowContactModal] = useState(false);
  const [selectedContact, setSelectedContact] = useState<EmergencyContact | null>(null);

  const handleContactCall = (contact: EmergencyContact) => {
    setSelectedContact(contact);
    setShowContactModal(true);
  };

  const makeCall = () => {
    if (selectedContact) {
      // In a real app, this would integrate with phone system or show appropriate UI
      window.open(`tel:${selectedContact.number}`, '_self');
      setShowContactModal(false);
    }
  };

  return (
    <div className={`emergency-panel ${className}`}>
      {showQuickAccess && (
        <EmergencyContactQuickAccess
          contacts={contacts}
          onCall={handleContactCall}
        />
      )}

      <div className="emergency-procedures-list">
        {procedures.map(procedure => (
          <EmergencyProcedureCard
            key={procedure.id}
            procedure={procedure}
            contacts={contacts}
            onContactCall={handleContactCall}
          />
        ))}
      </div>

      {/* Contact confirmation modal */}
      <CModal visible={showContactModal} onClose={() => setShowContactModal(false)}>
        <CModalHeader>
          <CModalTitle>
            <FontAwesomeIcon icon={faPhone} className="me-2 text-danger" />
            Emergency Call
          </CModalTitle>
        </CModalHeader>
        <CModalBody>
          {selectedContact && (
            <div className="text-center">
              <div className="mb-3">
                <FontAwesomeIcon icon={faExclamationTriangle} className="text-danger" size="3x" />
              </div>
              <h5>Call Emergency Contact?</h5>
              <p className="mb-2">
                <strong>{selectedContact.name}</strong>
              </p>
              <p className="mb-2">{selectedContact.description}</p>
              <p className="h4 text-danger">
                <FontAwesomeIcon icon={faPhone} className="me-2" />
                {selectedContact.number}
              </p>
              <div className="alert alert-warning mt-3">
                <small>
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
                  Only call this number in genuine emergency situations. 
                  Misuse of emergency services may result in penalties.
                </small>
              </div>
            </div>
          )}
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowContactModal(false)}>
            <FontAwesomeIcon icon={faTimes} className="me-1" />
            Cancel
          </CButton>
          <CButton color="danger" onClick={makeCall}>
            <FontAwesomeIcon icon={faPhone} className="me-1" />
            Call Now
          </CButton>
        </CModalFooter>
      </CModal>
    </div>
  );
};

// Quick emergency button component for headers/toolbars
interface QuickEmergencyButtonProps {
  onEmergencyCall: () => void;
  className?: string;
}

export const QuickEmergencyButton: React.FC<QuickEmergencyButtonProps> = ({
  onEmergencyCall,
  className = ''
}) => {
  return (
    <CButton
      color="danger"
      size="sm"
      onClick={onEmergencyCall}
      className={`emergency-quick-button ${className}`}
      title="Emergency Services"
    >
      <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
      Emergency
    </CButton>
  );
};

// Status timeline component for incident tracking
interface StatusTimelineProps {
  items: Array<{
    id: string;
    title: string;
    description: string;
    timestamp: string;
    status: 'completed' | 'current' | 'pending';
    user?: string;
  }>;
  className?: string;
}

export const StatusTimeline: React.FC<StatusTimelineProps> = ({
  items,
  className = ''
}) => {
  return (
    <div className={`status-timeline ${className}`}>
      {items.map(item => (
        <div key={item.id} className={`timeline-item ${item.status}`}>
          <div className="timeline-content">
            <div className="timeline-title">{item.title}</div>
            <div className="timeline-description">{item.description}</div>
            <div className="timeline-meta">
              {item.timestamp}
              {item.user && ` • ${item.user}`}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

// Priority indicator component
interface PriorityIndicatorProps {
  priority: 'low' | 'medium' | 'high' | 'critical';
  showIcon?: boolean;
  showText?: boolean;
  className?: string;
}

export const PriorityIndicator: React.FC<PriorityIndicatorProps> = ({
  priority,
  showIcon = true,
  showText = true,
  className = ''
}) => {
  const iconMap = {
    low: faFirstAid,
    medium: faExclamationTriangle,
    high: faExclamationTriangle,
    critical: faExclamationTriangle
  };

  const textMap = {
    low: 'Low Priority',
    medium: 'Medium Priority',
    high: 'High Priority',
    critical: 'Critical Priority'
  };

  return (
    <span className={`priority-indicator ${priority} ${className}`}>
      {showIcon && (
        <FontAwesomeIcon icon={iconMap[priority]} />
      )}
      {showText && textMap[priority]}
    </span>
  );
};

// Default emergency data for common scenarios
export const defaultEmergencyData = {
  contacts: [
    {
      id: 'fire',
      name: 'Fire Department',
      number: '113',
      description: 'Fire, explosion, hazardous materials',
      priority: 'primary' as const
    },
    {
      id: 'medical',
      name: 'Medical Emergency',
      number: '118',
      description: 'Medical emergencies, ambulance',
      priority: 'primary' as const
    },
    {
      id: 'police',
      name: 'Police',
      number: '110',
      description: 'Security incidents, violence, theft',
      priority: 'primary' as const
    },
    {
      id: 'security',
      name: 'School Security',
      number: '+62-21-xxx-xxxx',
      description: 'Campus security, access control',
      priority: 'secondary' as const
    }
  ],
  procedures: [
    {
      id: 'fire',
      title: 'Fire Emergency',
      description: 'Steps to follow in case of fire or smoke detection',
      steps: [
        {
          id: '1',
          title: 'Alert',
          description: 'Sound alarm and alert others immediately'
        },
        {
          id: '2',
          title: 'Evacuate',
          description: 'Exit building using nearest safe exit route'
        },
        {
          id: '3',
          title: 'Assembly',
          description: 'Proceed to designated assembly point'
        },
        {
          id: '4',
          title: 'Report',
          description: 'Report to emergency coordinator'
        }
      ],
      contacts: ['fire', 'security']
    },
    {
      id: 'medical',
      title: 'Medical Emergency',
      description: 'Steps for medical emergencies and first aid situations',
      steps: [
        {
          id: '1',
          title: 'Assess',
          description: 'Check if person is conscious and breathing'
        },
        {
          id: '2',
          title: 'Call',
          description: 'Call emergency medical services immediately'
        },
        {
          id: '3',
          title: 'First Aid',
          description: 'Provide first aid if trained to do so'
        },
        {
          id: '4',
          title: 'Wait',
          description: 'Stay with person until help arrives'
        }
      ],
      contacts: ['medical', 'security']
    }
  ]
};

export default EmergencyPanel;