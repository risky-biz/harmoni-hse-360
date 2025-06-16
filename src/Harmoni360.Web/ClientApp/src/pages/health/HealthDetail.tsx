import React, { useState } from 'react';
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
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane
} from '@coreui/react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  useGetHealthRecordQuery,
  HealthRecordDetailDto,
  MedicalConditionDto,
  VaccinationRecordDto,
  HealthIncidentDto,
  EmergencyContactDto
} from '../../features/health/healthApi';
import { formatDate } from '../../utils/dateUtils';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faEdit,
  faExclamationTriangle,
  faShieldAlt,
  faMedkit,
  faPhone,
  faCalendarAlt,
  faUser,
  faHeartbeat,
  faUsers,
  faArrowLeft,
  faPlus,
  faEllipsisV
} from '@fortawesome/free-solid-svg-icons';

const HealthDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('overview');
  const [showMedicalModal, setShowMedicalModal] = useState(false);
  const [selectedMedicalCondition, setSelectedMedicalCondition] = useState<MedicalConditionDto | null>(null);

  const {
    data: healthRecord,
    isLoading,
    error,
    refetch
  } = useGetHealthRecordQuery({ id: parseInt(id!) });

  const handleEdit = () => {
    navigate(`/health/edit/${id}`);
  };

  const handleBack = () => {
    navigate('/health');
  };

  const getPersonTypeIcon = (personType: string) => {
    return personType === 'Student' ? faUser : faUsers;
  };

  const getPersonTypeBadge = (personType: string) => {
    return personType === 'Student' ? 'info' : 'warning';
  };

  const getSeverityBadge = (severity: string) => {
    switch (severity) {
      case 'Critical': return 'danger';
      case 'High': return 'warning';
      case 'Medium': return 'info';
      case 'Low': return 'success';
      default: return 'secondary';
    }
  };

  const getVaccinationStatusBadge = (status: string) => {
    switch (status) {
      case 'Administered': return 'success';
      case 'Due': return 'warning';
      case 'Overdue': return 'danger';
      case 'Exempted': return 'secondary';
      case 'Scheduled': return 'info';
      default: return 'secondary';
    }
  };

  const getIncidentSeverityBadge = (severity: string) => {
    switch (severity) {
      case 'Critical': return 'danger';
      case 'Severe': return 'warning';
      case 'Moderate': return 'info';
      case 'Minor': return 'success';
      default: return 'secondary';
    }
  };

  const handleMedicalConditionClick = (condition: MedicalConditionDto) => {
    setSelectedMedicalCondition(condition);
    setShowMedicalModal(true);
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '400px' }}>
        <CSpinner color="primary" size="sm" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger" className="d-flex align-items-center">
        <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="lg" />
        <div>
          Failed to load health record details. 
          <CButton color="link" onClick={() => refetch()} className="p-0 ms-2">
            Try again
          </CButton>
        </div>
      </CAlert>
    );
  }

  if (!healthRecord) {
    return (
      <CAlert color="warning">
        Health record not found.
      </CAlert>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div className="d-flex align-items-center">
          <CButton color="ghost" onClick={handleBack} className="me-2">
            <FontAwesomeIcon icon={faArrowLeft} />
          </CButton>
          <h2>Health Record Details</h2>
        </div>
        <CButton color="primary" onClick={handleEdit}>
          <FontAwesomeIcon icon={faEdit} className="me-1" />
          Edit Record
        </CButton>
      </div>

      {/* Person Information Card */}
      <CCard className="mb-4">
        <CCardBody>
          <CRow>
            <CCol md={8}>
              <div className="d-flex align-items-center mb-3">
                <FontAwesomeIcon 
                  icon={getPersonTypeIcon(healthRecord.personType)} 
                  className="me-3" 
                  size="2x"
                />
                <div>
                  <h4 className="mb-0">{healthRecord.personName}</h4>
                  <div className="text-muted">{healthRecord.personEmail}</div>
                </div>
                <CBadge color={getPersonTypeBadge(healthRecord.personType)} className="ms-3">
                  {healthRecord.personType}
                </CBadge>
                <CBadge color={healthRecord.isActive ? 'success' : 'secondary'} className="ms-2">
                  {healthRecord.isActive ? 'Active' : 'Inactive'}
                </CBadge>
              </div>
              <CRow>
                <CCol sm={6}>
                  <strong>Date of Birth:</strong> {formatDate(healthRecord.dateOfBirth)}
                </CCol>
                <CCol sm={6}>
                  <strong>Blood Type:</strong> {healthRecord.bloodType || 'Not specified'}
                </CCol>
              </CRow>
            </CCol>
            <CCol md={4}>
              <div className="text-md-end">
                <div className="small text-muted">Created: {formatDate(healthRecord.createdAt)}</div>
                {healthRecord.lastModifiedAt && (
                  <div className="small text-muted">Updated: {formatDate(healthRecord.lastModifiedAt)}</div>
                )}
              </div>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {/* Navigation Tabs */}
      <CNav variant="tabs" className="mb-3">
        <CNavItem>
          <CNavLink
            active={activeTab === 'overview'}
            onClick={() => setActiveTab('overview')}
            className="cursor-pointer"
          >
            <FontAwesomeIcon icon={faHeartbeat} className="me-1" />
            Overview
          </CNavLink>
        </CNavItem>
        <CNavItem>
          <CNavLink
            active={activeTab === 'medical'}
            onClick={() => setActiveTab('medical')}
            className="cursor-pointer"
          >
            <FontAwesomeIcon icon={faMedkit} className="me-1" />
            Medical Conditions ({healthRecord.medicalConditions.length})
          </CNavLink>
        </CNavItem>
        <CNavItem>
          <CNavLink
            active={activeTab === 'vaccinations'}
            onClick={() => setActiveTab('vaccinations')}
            className="cursor-pointer"
          >
            <FontAwesomeIcon icon={faShieldAlt} className="me-1" />
            Vaccinations ({healthRecord.vaccinations.length})
          </CNavLink>
        </CNavItem>
        <CNavItem>
          <CNavLink
            active={activeTab === 'incidents'}
            onClick={() => setActiveTab('incidents')}
            className="cursor-pointer"
          >
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
            Health Incidents ({healthRecord.healthIncidents.length})
          </CNavLink>
        </CNavItem>
        <CNavItem>
          <CNavLink
            active={activeTab === 'contacts'}
            onClick={() => setActiveTab('contacts')}
            className="cursor-pointer"
          >
            <FontAwesomeIcon icon={faPhone} className="me-1" />
            Emergency Contacts ({healthRecord.emergencyContacts.length})
          </CNavLink>
        </CNavItem>
      </CNav>

      {/* Tab Content */}
      <CTabContent>
        {/* Overview Tab */}
        <CTabPane visible={activeTab === 'overview'}>
          <CRow>
            <CCol md={6}>
              <CCard className="mb-4">
                <CCardHeader>
                  <strong>Medical Information</strong>
                </CCardHeader>
                <CCardBody>
                  <div className="mb-3">
                    <strong>Primary Doctor:</strong><br />
                    {(healthRecord as any).primaryDoctorName || 'Not specified'}
                    {(healthRecord as any).primaryDoctorContact && (
                      <div className="small text-muted">{(healthRecord as any).primaryDoctorContact}</div>
                    )}
                  </div>
                  <div className="mb-3">
                    <strong>Insurance:</strong><br />
                    {(healthRecord as any).insuranceProvider || 'Not specified'}
                    {(healthRecord as any).insurancePolicyNumber && (
                      <div className="small text-muted">Policy: {(healthRecord as any).insurancePolicyNumber}</div>
                    )}
                  </div>
                  <div className="mb-3">
                    <strong>Health Checks:</strong><br />
                    Last: {(healthRecord as any).lastHealthCheckDate ? formatDate((healthRecord as any).lastHealthCheckDate) : 'Not recorded'}
                    {(healthRecord as any).nextHealthCheckDate && (
                      <div className="small text-muted">Next: {formatDate((healthRecord as any).nextHealthCheckDate)}</div>
                    )}
                  </div>
                  {healthRecord.medicalNotes && (
                    <div>
                      <strong>Medical Notes:</strong><br />
                      <div className="mt-1">{healthRecord.medicalNotes}</div>
                    </div>
                  )}
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={6}>
              <CCard className="mb-4">
                <CCardHeader>
                  <strong>Summary</strong>
                </CCardHeader>
                <CCardBody>
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <span>Medical Conditions</span>
                    <CBadge color="info">{healthRecord.medicalConditions.length}</CBadge>
                  </div>
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <span>Critical Conditions</span>
                    <CBadge color="danger">
                      {healthRecord.medicalConditions?.filter(mc => mc.requiresEmergencyAction).length || 0}
                    </CBadge>
                  </div>
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <span>Vaccinations</span>
                    <CBadge color="success">{healthRecord.vaccinations?.length || 0}</CBadge>
                  </div>
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <span>Overdue Vaccinations</span>
                    <CBadge color="warning">
                      {healthRecord.vaccinations?.filter(v => v.status === 'Overdue').length || 0}
                    </CBadge>
                  </div>
                  <div className="d-flex justify-content-between align-items-center mb-2">
                    <span>Health Incidents</span>
                    <CBadge color="info">{healthRecord.healthIncidents.length}</CBadge>
                  </div>
                  <div className="d-flex justify-content-between align-items-center">
                    <span>Emergency Contacts</span>
                    <CBadge color={healthRecord.emergencyContacts.length > 0 ? 'success' : 'warning'}>
                      {healthRecord.emergencyContacts.length}
                    </CBadge>
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>
        </CTabPane>

        {/* Medical Conditions Tab */}
        <CTabPane visible={activeTab === 'medical'}>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <strong>Medical Conditions</strong>
              <CButton color="primary" size="sm">
                <FontAwesomeIcon icon={faPlus} className="me-1" />
                Add Condition
              </CButton>
            </CCardHeader>
            <CCardBody className="p-0">
              {healthRecord.medicalConditions?.length > 0 ? (
                <CListGroup flush>
                  {healthRecord.medicalConditions.map((condition) => (
                    <CListGroupItem 
                      key={condition.id}
                      className="cursor-pointer"
                      onClick={() => handleMedicalConditionClick(condition)}
                    >
                      <div className="d-flex justify-content-between align-items-start">
                        <div className="flex-grow-1">
                          <div className="d-flex align-items-center mb-1">
                            <strong className="me-2">{condition.name}</strong>
                            <CBadge color={getSeverityBadge(condition.severity)} className="me-2">
                              {condition.severity}
                            </CBadge>
                            {condition.requiresEmergencyAction && (
                              <CBadge color="danger">
                                <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" size="xs" />
                                Emergency Action Required
                              </CBadge>
                            )}
                          </div>
                          <div className="text-muted small mb-1">{condition.type}</div>
                          {condition.description && (
                            <div className="small">{condition.description}</div>
                          )}
                        </div>
                        <div className="text-muted small">
                          {condition.diagnosedDate ? formatDate(condition.diagnosedDate) : 'Date not specified'}
                        </div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              ) : (
                <div className="text-center p-4">
                  <FontAwesomeIcon icon={faMedkit} size="3x" className="text-muted mb-3" />
                  <h5 className="text-muted">No Medical Conditions</h5>
                  <p className="text-muted">No medical conditions have been recorded.</p>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CTabPane>

        {/* Vaccinations Tab */}
        <CTabPane visible={activeTab === 'vaccinations'}>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <strong>Vaccination Records</strong>
              <CButton color="primary" size="sm">
                <FontAwesomeIcon icon={faPlus} className="me-1" />
                Record Vaccination
              </CButton>
            </CCardHeader>
            <CCardBody className="p-0">
              {healthRecord.vaccinations?.length > 0 ? (
                <CListGroup flush>
                  {healthRecord.vaccinations.map((vaccination) => (
                    <CListGroupItem key={vaccination.id}>
                      <div className="d-flex justify-content-between align-items-start">
                        <div className="flex-grow-1">
                          <div className="d-flex align-items-center mb-1">
                            <strong className="me-2">{vaccination.vaccineName}</strong>
                            <CBadge color={getVaccinationStatusBadge(vaccination.status)}>
                              {vaccination.status}
                            </CBadge>
                          </div>
                          <div className="small text-muted">
                            Administered: {vaccination.dateAdministered ? formatDate(vaccination.dateAdministered) : 'Date not specified'}
                            {vaccination.administeredBy && ` by ${vaccination.administeredBy}`}
                          </div>
                          {vaccination.expiryDate && (
                            <div className="small text-muted">
                              Expires: {formatDate(vaccination.expiryDate)}
                            </div>
                          )}
                          {vaccination.exemptionReason && (
                            <div className="small text-warning">
                              Exemption: {vaccination.exemptionReason}
                            </div>
                          )}
                        </div>
                        <div className="text-muted small">
                          {(vaccination as any).doseNumber && (vaccination as any).totalDosesRequired && (
                            <div>Dose {(vaccination as any).doseNumber}/{(vaccination as any).totalDosesRequired}</div>
                          )}
                        </div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              ) : (
                <div className="text-center p-4">
                  <FontAwesomeIcon icon={faShieldAlt} size="3x" className="text-muted mb-3" />
                  <h5 className="text-muted">No Vaccination Records</h5>
                  <p className="text-muted">No vaccinations have been recorded.</p>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CTabPane>

        {/* Health Incidents Tab */}
        <CTabPane visible={activeTab === 'incidents'}>
          <CCard>
            <CCardHeader>
              <strong>Health Incidents</strong>
            </CCardHeader>
            <CCardBody className="p-0">
              {healthRecord.healthIncidents?.length > 0 ? (
                <CListGroup flush>
                  {healthRecord.healthIncidents.map((incident) => (
                    <CListGroupItem key={incident.id}>
                      <div className="d-flex justify-content-between align-items-start">
                        <div className="flex-grow-1">
                          <div className="d-flex align-items-center mb-1">
                            <strong className="me-2">{incident.type}</strong>
                            <CBadge color={getIncidentSeverityBadge(incident.severity)}>
                              {incident.severity}
                            </CBadge>
                          </div>
                          <div className="small text-muted mb-1">
                            {formatDate(incident.incidentDateTime)}
                            {incident.treatmentLocation && ` - ${incident.treatmentLocation}`}
                          </div>
                          {incident.symptoms && (
                            <div className="small mb-1">
                              <strong>Symptoms:</strong> {incident.symptoms}
                            </div>
                          )}
                          {incident.treatmentProvided && (
                            <div className="small mb-1">
                              <strong>Treatment:</strong> {incident.treatmentProvided}
                            </div>
                          )}
                        </div>
                        <div className="text-muted small">
                          {incident.followUpRequired && (
                            <CBadge color="warning">Follow-up Required</CBadge>
                          )}
                        </div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              ) : (
                <div className="text-center p-4">
                  <FontAwesomeIcon icon={faExclamationTriangle} size="3x" className="text-muted mb-3" />
                  <h5 className="text-muted">No Health Incidents</h5>
                  <p className="text-muted">No health incidents have been recorded.</p>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CTabPane>

        {/* Emergency Contacts Tab */}
        <CTabPane visible={activeTab === 'contacts'}>
          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <strong>Emergency Contacts</strong>
              <CButton color="primary" size="sm">
                <FontAwesomeIcon icon={faPlus} className="me-1" />
                Add Contact
              </CButton>
            </CCardHeader>
            <CCardBody className="p-0">
              {healthRecord.emergencyContacts?.length > 0 ? (
                <CListGroup flush>
                  {healthRecord.emergencyContacts
                    .sort((a, b) => a.contactOrder - b.contactOrder)
                    .map((contact) => (
                    <CListGroupItem key={contact.id}>
                      <div className="d-flex justify-content-between align-items-start">
                        <div className="flex-grow-1">
                          <div className="d-flex align-items-center mb-1">
                            <strong className="me-2">{contact.name}</strong>
                            {contact.isPrimaryContact && (
                              <CBadge color="primary">Primary</CBadge>
                            )}
                          </div>
                          <div className="small text-muted mb-1">
                            {contact.relationship} - {contact.primaryPhone}
                          </div>
                          {contact.email && (
                            <div className="small text-muted mb-1">{contact.email}</div>
                          )}
                          <div className="small">
                            {contact.authorizedForPickup && (
                              <CBadge color="info" className="me-1">Pickup Authorized</CBadge>
                            )}
                            {contact.authorizedForMedicalDecisions && (
                              <CBadge color="success">Medical Decisions</CBadge>
                            )}
                          </div>
                        </div>
                        <div className="text-muted small">
                          Order: {contact.contactOrder}
                        </div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              ) : (
                <div className="text-center p-4">
                  <FontAwesomeIcon icon={faPhone} size="3x" className="text-muted mb-3" />
                  <h5 className="text-muted">No Emergency Contacts</h5>
                  <p className="text-muted">No emergency contacts have been added.</p>
                </div>
              )}
            </CCardBody>
          </CCard>
        </CTabPane>
      </CTabContent>

      {/* Medical Condition Detail Modal */}
      <CModal visible={showMedicalModal} onClose={() => setShowMedicalModal(false)} size="lg">
        <CModalHeader>
          <CModalTitle>Medical Condition Details</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {selectedMedicalCondition && (
            <div>
              <CRow className="mb-3">
                <CCol sm={6}>
                  <strong>Name:</strong> {selectedMedicalCondition.name}
                </CCol>
                <CCol sm={6}>
                  <strong>Type:</strong> {selectedMedicalCondition.type}
                </CCol>
              </CRow>
              <CRow className="mb-3">
                <CCol sm={6}>
                  <strong>Severity:</strong>{' '}
                  <CBadge color={getSeverityBadge(selectedMedicalCondition.severity)}>
                    {selectedMedicalCondition.severity}
                  </CBadge>
                </CCol>
                <CCol sm={6}>
                  <strong>Diagnosed:</strong> {formatDate(selectedMedicalCondition.diagnosedDate)}
                </CCol>
              </CRow>
              {(selectedMedicalCondition as any).diagnosedBy && (
                <div className="mb-3">
                  <strong>Diagnosed By:</strong> {(selectedMedicalCondition as any).diagnosedBy}
                </div>
              )}
              {selectedMedicalCondition.description && (
                <div className="mb-3">
                  <strong>Description:</strong><br />
                  {selectedMedicalCondition.description}
                </div>
              )}
              {selectedMedicalCondition.treatmentPlan && (
                <div className="mb-3">
                  <strong>Treatment Plan:</strong><br />
                  {selectedMedicalCondition.treatmentPlan}
                </div>
              )}
              {(selectedMedicalCondition as any).medicationRequired && (
                <div className="mb-3">
                  <strong>Medication Required:</strong><br />
                  {(selectedMedicalCondition as any).medicationRequired}
                </div>
              )}
              {(selectedMedicalCondition as any).emergencyProtocol && (
                <div className="mb-3">
                  <strong>Emergency Protocol:</strong><br />
                  <div className="text-danger">{(selectedMedicalCondition as any).emergencyProtocol}</div>
                </div>
              )}
              {selectedMedicalCondition.requiresEmergencyAction && (
                <CAlert color="danger">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                  This condition requires emergency action protocols.
                </CAlert>
              )}
            </div>
          )}
        </CModalBody>
        <CModalFooter>
          <CButton color="primary" size="sm">
            <FontAwesomeIcon icon={faEdit} className="me-1" />
            Edit
          </CButton>
          <CButton color="secondary" onClick={() => setShowMedicalModal(false)}>
            Close
          </CButton>
        </CModalFooter>
      </CModal>
    </div>
  );
};

export default HealthDetail;