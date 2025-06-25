import React, { useState, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CFormInput,
  CFormLabel,
  CFormSelect,
  CFormTextarea,
  CFormCheck,
  CRow,
  CCol,
  CAlert,
  CSpinner,
  CButton,
  CAccordion,
  CAccordionBody,
  CAccordionHeader,
  CAccordionItem,
  CBadge,
  CListGroup,
  CListGroupItem,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter
} from '@coreui/react';
import { useGetHealthRecordsQuery, HealthRecordDto } from '../../features/health/healthApi';
import { HealthAlert, MedicalConditionBadge, EmergencyContactQuickAccess } from '../health';
import { HealthRecordDetailDto } from '../../features/health/healthApi';
import { PersonType } from '../../types/health';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faSearch
} from '@fortawesome/free-solid-svg-icons';

interface HealthAwareIncidentFormProps {
  category: string;
  involvedPersons: string;
  onHealthContextChange: (healthContext: HealthIncidentContext) => void;
}

interface HealthIncidentContext {
  isHealthRelated: boolean;
  selectedPersonId?: string;
  healthRecord?: HealthRecordDetailDto;
  injuryType?: string;
  bodyPartAffected?: string;
  symptomsObserved?: string;
  treatmentProvided?: string;
  treatmentLocation?: string;
  emergencyServicesContacted: boolean;
  parentNotificationRequired: boolean;
  followUpRequired: boolean;
  followUpNotes?: string;
}

const HEALTH_RELATED_CATEGORIES = [
  'Medical Emergency',
  'Student Injury',
  'Staff Injury',
  'Allergic Reaction',
  'Illness Outbreak',
  'First Aid',
  'Health & Safety'
];

const INJURY_TYPES = [
  'Cut/Laceration',
  'Bruise/Contusion',
  'Sprain/Strain',
  'Fracture',
  'Burn',
  'Head Injury',
  'Eye Injury',
  'Allergic Reaction',
  'Respiratory Issue',
  'Cardiac Event',
  'Seizure',
  'Fainting/Collapse',
  'Other'
];

const BODY_PARTS = [
  'Head',
  'Face',
  'Eyes',
  'Neck',
  'Chest',
  'Back',
  'Arms',
  'Hands',
  'Abdomen',
  'Legs',
  'Feet',
  'Multiple Areas'
];

const TREATMENT_LOCATIONS = [
  'School Nurse Office',
  'Classroom',
  'First Aid Station',
  'Hospital',
  'Clinic',
  'Ambulance',
  'Other'
];

const HealthAwareIncidentForm: React.FC<HealthAwareIncidentFormProps> = ({
  category,
  onHealthContextChange
}) => {
  const [healthContext, setHealthContext] = useState<HealthIncidentContext>({
    isHealthRelated: false,
    emergencyServicesContacted: false,
    parentNotificationRequired: false,
    followUpRequired: false
  });

  const [showPersonSearch, setShowPersonSearch] = useState(false);
  const [personSearchTerm, setPersonSearchTerm] = useState('');

  const {
    data: healthRecords,
    isLoading: isLoadingRecords
  } = useGetHealthRecordsQuery({
    searchTerm: personSearchTerm
  }, {
    skip: !showPersonSearch || personSearchTerm.length < 2
  });

  // Determine if incident is health-related based on category
  useEffect(() => {
    const isHealthRelated = HEALTH_RELATED_CATEGORIES.includes(category);
    setHealthContext(prev => ({
      ...prev,
      isHealthRelated,
      parentNotificationRequired: isHealthRelated && category.includes('Student')
    }));
  }, [category]);

  // Update parent component when health context changes
  useEffect(() => {
    onHealthContextChange(healthContext);
  }, [healthContext, onHealthContextChange]);

  const handleInputChange = (field: keyof HealthIncidentContext, value: any) => {
    setHealthContext(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const selectHealthRecord = (record: HealthRecordDto) => {
    // Convert the summary record to the detailed format expected by components
    const detailedRecord: HealthRecordDetailDto = {
      ...record,
      medicalConditions: [], // Will be loaded separately if needed
      vaccinations: [],
      healthIncidents: [],
      emergencyContacts: []
    };
    
    setHealthContext(prev => ({
      ...prev,
      selectedPersonId: record.personId.toString(),
      healthRecord: detailedRecord,
      parentNotificationRequired: record.personType === PersonType.Student
    }));
    setShowPersonSearch(false);
    setPersonSearchTerm('');
  };

  if (!healthContext.isHealthRelated) {
    return null;
  }

  return (
    <CAccordionItem itemKey="health-context">
      <CAccordionHeader>
        <FontAwesomeIcon icon={faSearch} className="me-2" />
        Health & Medical Information
        {healthContext.healthRecord && (
          <CBadge color="info" className="ms-2">
            Health Record Found
          </CBadge>
        )}
      </CAccordionHeader>
      <CAccordionBody>
        {/* Person Selection */}
        <CCard className="mb-3">
          <CCardHeader>
            <strong>Affected Person</strong>
          </CCardHeader>
          <CCardBody>
            <CRow>
              <CCol md={8}>
                <CFormLabel>Search for person with health record</CFormLabel>
                <div className="d-flex gap-2">
                  <CFormInput
                    placeholder="Enter name or ID to search health records..."
                    value={personSearchTerm}
                    onChange={(e) => setPersonSearchTerm(e.target.value)}
                  />
                  <CButton
                    color="primary"
                    onClick={() => setShowPersonSearch(true)}
                    disabled={personSearchTerm.length < 2}
                  >
                    <FontAwesomeIcon icon={faSearch} />
                  </CButton>
                </div>
                {healthContext.healthRecord && (
                  <div className="mt-2">
                    <strong>Selected: </strong>
                    {healthContext.healthRecord.personName} ({healthContext.healthRecord.personType})
                  </div>
                )}
              </CCol>
            </CRow>
          </CCardBody>
        </CCard>

        {/* Health Alert Display */}
        {healthContext.healthRecord && healthContext.healthRecord.medicalConditions.length > 0 && (
          <HealthAlert
            alert={{ medicalConditions: healthContext.healthRecord.medicalConditions }}
          />
        )}

        {/* Medical Details */}
        <CCard className="mb-3">
          <CCardHeader>
            <strong>Medical Details</strong>
          </CCardHeader>
          <CCardBody>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="injuryType">Type of Injury/Illness</CFormLabel>
                <CFormSelect
                  id="injuryType"
                  value={healthContext.injuryType || ''}
                  onChange={(e) => handleInputChange('injuryType', e.target.value)}
                >
                  <option value="">Select injury/illness type</option>
                  {INJURY_TYPES.map(type => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={6}>
                <CFormLabel htmlFor="bodyPartAffected">Body Part Affected</CFormLabel>
                <CFormSelect
                  id="bodyPartAffected"
                  value={healthContext.bodyPartAffected || ''}
                  onChange={(e) => handleInputChange('bodyPartAffected', e.target.value)}
                >
                  <option value="">Select body part</option>
                  {BODY_PARTS.map(part => (
                    <option key={part} value={part}>{part}</option>
                  ))}
                </CFormSelect>
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={12}>
                <CFormLabel htmlFor="symptomsObserved">Symptoms Observed</CFormLabel>
                <CFormTextarea
                  id="symptomsObserved"
                  rows={3}
                  value={healthContext.symptomsObserved || ''}
                  onChange={(e) => handleInputChange('symptomsObserved', e.target.value)}
                  placeholder="Describe the symptoms observed at the time of the incident..."
                />
              </CCol>
            </CRow>
          </CCardBody>
        </CCard>

        {/* Treatment Information */}
        <CCard className="mb-3">
          <CCardHeader>
            <strong>Treatment & Response</strong>
          </CCardHeader>
          <CCardBody>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel htmlFor="treatmentLocation">Treatment Location</CFormLabel>
                <CFormSelect
                  id="treatmentLocation"
                  value={healthContext.treatmentLocation || ''}
                  onChange={(e) => handleInputChange('treatmentLocation', e.target.value)}
                >
                  <option value="">Select treatment location</option>
                  {TREATMENT_LOCATIONS.map(location => (
                    <option key={location} value={location}>{location}</option>
                  ))}
                </CFormSelect>
              </CCol>
              <CCol md={6}>
                <div className="mt-4">
                  <CFormCheck
                    id="emergencyServices"
                    label="Emergency services contacted"
                    checked={healthContext.emergencyServicesContacted}
                    onChange={(e) => handleInputChange('emergencyServicesContacted', e.target.checked)}
                  />
                  <CFormCheck
                    id="parentNotification"
                    label="Parent/Guardian notification required"
                    checked={healthContext.parentNotificationRequired}
                    onChange={(e) => handleInputChange('parentNotificationRequired', e.target.checked)}
                  />
                  <CFormCheck
                    id="followUpRequired"
                    label="Medical follow-up required"
                    checked={healthContext.followUpRequired}
                    onChange={(e) => handleInputChange('followUpRequired', e.target.checked)}
                  />
                </div>
              </CCol>
            </CRow>

            <CRow className="mb-3">
              <CCol md={12}>
                <CFormLabel htmlFor="treatmentProvided">Treatment Provided</CFormLabel>
                <CFormTextarea
                  id="treatmentProvided"
                  rows={3}
                  value={healthContext.treatmentProvided || ''}
                  onChange={(e) => handleInputChange('treatmentProvided', e.target.value)}
                  placeholder="Describe the first aid or medical treatment provided..."
                />
              </CCol>
            </CRow>

            {healthContext.followUpRequired && (
              <CRow className="mb-3">
                <CCol md={12}>
                  <CFormLabel htmlFor="followUpNotes">Follow-up Notes</CFormLabel>
                  <CFormTextarea
                    id="followUpNotes"
                    rows={3}
                    value={healthContext.followUpNotes || ''}
                    onChange={(e) => handleInputChange('followUpNotes', e.target.value)}
                    placeholder="Describe the required follow-up care or monitoring..."
                  />
                </CCol>
              </CRow>
            )}
          </CCardBody>
        </CCard>

        {/* Emergency Contacts */}
        {healthContext.healthRecord && healthContext.healthRecord.emergencyContacts.length > 0 && (
          <EmergencyContactQuickAccess
            emergencyContacts={healthContext.healthRecord.emergencyContacts || []}
            personName={healthContext.healthRecord.personName}
            emergencyType="Medical Incident"
            compact={true}
          />
        )}

        {/* Person Search Modal */}
        <CModal visible={showPersonSearch} onClose={() => setShowPersonSearch(false)} size="lg">
          <CModalHeader>
            <CModalTitle>Search Health Records</CModalTitle>
          </CModalHeader>
          <CModalBody>
            <CFormInput
              className="mb-3"
              placeholder="Search by name or ID..."
              value={personSearchTerm}
              onChange={(e) => setPersonSearchTerm(e.target.value)}
            />

            {isLoadingRecords ? (
              <div className="text-center p-3">
                <CSpinner color="primary" />
              </div>
            ) : healthRecords && healthRecords.records.length > 0 ? (
              <CListGroup>
                {healthRecords.records.map((record) => (
                  <CListGroupItem
                    key={record.id}
                    className="cursor-pointer"
                    onClick={() => selectHealthRecord(record)}
                  >
                    <div className="d-flex justify-content-between align-items-center">
                      <div>
                        <strong>{record.personName}</strong>
                        <div className="small text-muted">
                          {record.personType} • {record.personEmail}
                        </div>
                        {record.medicalConditionsCount > 0 && (
                          <div className="mt-1">
                            <span className="small text-info">
                              {record.medicalConditionsCount} medical condition{record.medicalConditionsCount > 1 ? 's' : ''}
                            </span>
                            {record.hasCriticalConditions && (
                              <span className="small text-danger ms-2">
                                ⚠️ Critical conditions
                              </span>
                            )}
                          </div>
                        )}
                      </div>
                      <CBadge color="info">{record.personType}</CBadge>
                    </div>
                  </CListGroupItem>
                ))}
              </CListGroup>
            ) : personSearchTerm.length >= 2 ? (
              <CAlert color="info">
                No health records found for "{personSearchTerm}"
              </CAlert>
            ) : (
              <CAlert color="info">
                Enter at least 2 characters to search for health records
              </CAlert>
            )}
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" onClick={() => setShowPersonSearch(false)}>
              Cancel
            </CButton>
          </CModalFooter>
        </CModal>
      </CAccordionBody>
    </CAccordionItem>
  );
};

export default HealthAwareIncidentForm;