import React, { useState, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CSpinner,
  CAlert,
  CRow,
  CCol,
  CBadge,
  CListGroup,
  CListGroupItem,
  CFormInput,
  CInputGroup
} from '@coreui/react';
import { useGetHealthRecordQuery } from '../../features/health/healthApi';
import { HealthRecordDto, MedicalConditionSeverity } from '../../types/health';
import { formatDate } from '../../utils/dateUtils';
import HealthAlert from './HealthAlert';
import EmergencyContactQuickAccess from './EmergencyContactQuickAccess';
import MedicalConditionBadge from './MedicalConditionBadge';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSearch,
  faQrcode,
  faUser,
  faHeartbeat,
  // faPhone - unused
  faExclamationTriangle,
  faHeart,
  faShieldAlt
} from '@fortawesome/free-solid-svg-icons';

interface EmergencyHealthAccessProps {
  personId?: string;
  qrCode?: string;
  emergencyMode?: boolean;
}

const EmergencyHealthAccess: React.FC<EmergencyHealthAccessProps> = ({
  personId,
  qrCode,
  emergencyMode = false
}) => {
  const [searchPersonId, setSearchPersonId] = useState(personId || '');
  const [activePersonId, setActivePersonId] = useState<string | null>(personId || null);
  const [emergencyAccessCode, setEmergencyAccessCode] = useState('');
  const [isEmergencyAccess, setIsEmergencyAccess] = useState(emergencyMode);

  const {
    data: healthRecord,
    isLoading,
    error,
    refetch
  } = useGetHealthRecordQuery({ id: parseInt(activePersonId!) }, {
    skip: !activePersonId
  });

  useEffect(() => {
    if (qrCode) {
      // Parse QR code for person ID
      try {
        const qrData = JSON.parse(qrCode);
        if (qrData.personId) {
          setActivePersonId(qrData.personId);
          setSearchPersonId(qrData.personId);
        }
      } catch {
        // If not JSON, treat as direct person ID
        setActivePersonId(qrCode);
        setSearchPersonId(qrCode);
      }
    }
  }, [qrCode]);

  const handleSearch = () => {
    if (searchPersonId.trim()) {
      setActivePersonId(searchPersonId.trim());
    }
  };

  const handleEmergencyAccess = () => {
    // In a real implementation, this would verify the emergency access code
    if (emergencyAccessCode === 'EMERGENCY123') {
      setIsEmergencyAccess(true);
      // Grant temporary access to search any health record
    }
  };

  const getCriticalConditions = (record: HealthRecordDto) => {
    return record.medicalConditions.filter(
      condition => condition.requiresEmergencyAction ||
      condition.severity === MedicalConditionSeverity.Critical
    );
  };

  const getRecentVaccinations = (record: HealthRecordDto) => {
    return record.vaccinations
      .sort((a, b) => new Date(b.dateAdministered).getTime() - new Date(a.dateAdministered).getTime())
      .slice(0, 3);
  };

  if (!activePersonId) {
    return (
      <CCard className={`${emergencyMode ? 'border-danger' : ''}`}>
        <CCardHeader className={`${emergencyMode ? 'bg-danger-subtle' : ''}`}>
          <div className="d-flex align-items-center">
            <FontAwesomeIcon icon={emergencyMode ? faExclamationTriangle : faQrcode} className="me-2" />
            <strong>
              {emergencyMode ? 'Emergency Health Access' : 'Quick Health Information Access'}
            </strong>
          </div>
        </CCardHeader>
        <CCardBody>
          {emergencyMode && (
            <CAlert color="danger" className="mb-3">
              <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
              Emergency access mode. Enter person ID or scan QR code for immediate health information.
            </CAlert>
          )}
          
          <CRow>
            <CCol md={8}>
              <CInputGroup className="mb-3">
                <CFormInput
                  placeholder="Enter person ID (student/staff ID)"
                  value={searchPersonId}
                  onChange={(e) => setSearchPersonId(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                />
                <CButton 
                  color={emergencyMode ? 'danger' : 'primary'} 
                  onClick={handleSearch}
                  disabled={!searchPersonId.trim()}
                >
                  <FontAwesomeIcon icon={faSearch} className="me-1" />
                  Access Health Info
                </CButton>
              </CInputGroup>
            </CCol>
          </CRow>

          {!isEmergencyAccess && (
            <div className="border-top pt-3">
              <h6>Emergency Staff Access</h6>
              <CRow>
                <CCol md={6}>
                  <CInputGroup>
                    <CFormInput
                      type="password"
                      placeholder="Emergency access code"
                      value={emergencyAccessCode}
                      onChange={(e) => setEmergencyAccessCode(e.target.value)}
                      onKeyPress={(e) => e.key === 'Enter' && handleEmergencyAccess()}
                    />
                    <CButton 
                      color="warning" 
                      onClick={handleEmergencyAccess}
                      disabled={!emergencyAccessCode.trim()}
                    >
                      Emergency Access
                    </CButton>
                  </CInputGroup>
                  <div className="small text-muted mt-1">
                    For authorized emergency responders only
                  </div>
                </CCol>
              </CRow>
            </div>
          )}
        </CCardBody>
      </CCard>
    );
  }

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '200px' }}>
        <CSpinner color="primary" size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger" className="d-flex align-items-center">
        <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="lg" />
        <div>
          Failed to load health information for person ID: {activePersonId}
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
        No health record found for person ID: {activePersonId}
      </CAlert>
    );
  }

  const criticalConditions = getCriticalConditions(healthRecord);
  const recentVaccinations = getRecentVaccinations(healthRecord);

  return (
    <div>
      {/* Emergency Alert Banner */}
      {criticalConditions.length > 0 && (
        <HealthAlert alert={{ medicalConditions: criticalConditions }} />
      )}

      {/* Person Information */}
      <CCard className="mb-3">
        <CCardBody>
          <CRow>
            <CCol md={8}>
              <div className="d-flex align-items-center mb-2">
                <FontAwesomeIcon icon={faUser} className="me-2" size="lg" />
                <h4 className="mb-0">{healthRecord.personName}</h4>
                <CBadge color="info" className="ms-3">{healthRecord.personType}</CBadge>
                <CBadge color="success" className="ms-2">ID: {activePersonId}</CBadge>
              </div>
              <div className="mb-2">
                <strong>Date of Birth:</strong> {formatDate(healthRecord.dateOfBirth, false)}
              </div>
              <div className="mb-2">
                <strong>Blood Type:</strong> {healthRecord.bloodType || 'Not specified'}
              </div>
              {healthRecord.medicalNotes && (
                <div className="mb-2">
                  <strong>Medical Notes:</strong> {healthRecord.medicalNotes}
                </div>
              )}
            </CCol>
            <CCol md={4} className="text-md-end">
              <div className="small text-muted">
                Last Updated: {formatDate(healthRecord.lastModifiedAt || healthRecord.createdAt || '')}
              </div>
              {emergencyMode && (
                <CBadge color="danger" className="mt-2">
                  EMERGENCY ACCESS
                </CBadge>
              )}
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      <CRow>
        <CCol lg={6}>
          {/* Medical Conditions */}
          <CCard className="mb-3">
            <CCardHeader>
              <FontAwesomeIcon icon={faHeartbeat} className="me-1" />
              <strong>Medical Conditions ({healthRecord.medicalConditions.length})</strong>
            </CCardHeader>
            <CCardBody className="p-0">
              {healthRecord.medicalConditions.length > 0 ? (
                <CListGroup flush>
                  {healthRecord.medicalConditions.map((condition) => (
                    <CListGroupItem key={condition.id}>
                      <div className="d-flex justify-content-between align-items-start">
                        <div className="flex-grow-1">
                          <MedicalConditionBadge condition={condition} />
                          {condition.treatmentPlan && (
                            <div className="small text-info mt-1">
                              <strong>Treatment:</strong> {condition.treatmentPlan}
                            </div>
                          )}
                          {condition.emergencyInstructions && (
                            <div className="small text-danger mt-1">
                              <strong>Emergency Instructions:</strong> {condition.emergencyInstructions}
                            </div>
                          )}
                        </div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              ) : (
                <div className="p-3 text-muted text-center">
                  No medical conditions recorded
                </div>
              )}
            </CCardBody>
          </CCard>

          {/* Recent Vaccinations */}
          <CCard className="mb-3">
            <CCardHeader>
              <FontAwesomeIcon icon={faShieldAlt} className="me-1" />
              <strong>Recent Vaccinations</strong>
            </CCardHeader>
            <CCardBody className="p-0">
              {recentVaccinations.length > 0 ? (
                <CListGroup flush>
                  {recentVaccinations.map((vaccination) => (
                    <CListGroupItem key={vaccination.id}>
                      <div className="d-flex justify-content-between align-items-center">
                        <div>
                          <strong>{vaccination.vaccineName}</strong>
                          <div className="small text-muted">
                            {formatDate(vaccination.dateAdministered, false)}
                            {vaccination.administeredBy && ` by ${vaccination.administeredBy}`}
                          </div>
                        </div>
                        <CBadge color="success">{vaccination.status}</CBadge>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              ) : (
                <div className="p-3 text-muted text-center">
                  No vaccination records
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>

        <CCol lg={6}>
          {/* Medical Notes */}
          {healthRecord.medicalNotes && (
            <CCard className="mb-3">
              <CCardHeader>
                <FontAwesomeIcon icon={faHeart} className="me-1" />
                <strong>Medical Notes</strong>
              </CCardHeader>
              <CCardBody>
                <div className="small">
                  {healthRecord.medicalNotes}
                </div>
              </CCardBody>
            </CCard>
          )}

          {/* Emergency Contacts */}
          <div id="emergency-contacts">
            <EmergencyContactQuickAccess
              emergencyContacts={healthRecord.emergencyContacts || []}
              personName={healthRecord.personName}
              emergencyType="Medical Emergency"
            />
          </div>
        </CCol>
      </CRow>

      {/* Quick Actions */}
      <div className="d-flex gap-2 mt-3">
        <CButton 
          color="secondary" 
          onClick={() => {
            setActivePersonId(null);
            setSearchPersonId('');
          }}
        >
          Search Another Person
        </CButton>
        <CButton 
          color="info" 
          onClick={() => window.print()}
        >
          Print Emergency Info
        </CButton>
      </div>
    </div>
  );
};

export default EmergencyHealthAccess;