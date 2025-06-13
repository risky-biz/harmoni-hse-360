import React, { useState, useMemo } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CButton,
  CSpinner,
  CAlert,
  CBadge,
  CInputGroup,
  CFormInput,
  CFormSelect,
  CPagination,
  CPaginationItem,
  CDropdown,
  CDropdownToggle,
  CDropdownMenu,
  CDropdownItem,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CForm,
  CFormLabel,
  CFormTextarea,
  CProgress
} from '@coreui/react';
import { 
  useGetVaccinationComplianceQuery,
  useRecordVaccinationMutation,
  useUpdateVaccinationMutation,
  useSetVaccinationExemptionMutation 
} from '../../features/health/healthApi';
import { 
  VaccinationStatus, 
  PersonType,
  RecordVaccinationRequest,
  SetVaccinationExemptionRequest 
} from '../../types/health';
import { formatDate, isOverdue } from '../../utils/dateUtils';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlus,
  faEdit,
  faShieldAlt,
  faExclamationTriangle,
  faCalendarAlt,
  faSearch,
  faEllipsisV,
  faCheckCircle,
  faTimes,
  faClock
} from '@fortawesome/free-solid-svg-icons';

interface VaccinationFilters {
  searchTerm: string;
  personType: PersonType | '';
  status: VaccinationStatus | '';
  vaccineName: string;
}

const VaccinationManagement: React.FC = () => {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [filters, setFilters] = useState<VaccinationFilters>({
    searchTerm: '',
    personType: '',
    status: '',
    vaccineName: ''
  });

  const [showRecordModal, setShowRecordModal] = useState(false);
  const [showExemptionModal, setShowExemptionModal] = useState(false);
  const [selectedVaccination, setSelectedVaccination] = useState<string | null>(null);

  const [recordVaccination, { isLoading: isRecording }] = useRecordVaccinationMutation();
  const [setVaccinationExemption, { isLoading: isSettingExemption }] = useSetVaccinationExemptionMutation();

  const {
    data: complianceData,
    isLoading,
    error,
    refetch
  } = useGetVaccinationComplianceQuery({
    includeDetails: true,
    page,
    pageSize,
    ...filters
  });

  const [recordForm, setRecordForm] = useState<RecordVaccinationRequest>({
    vaccineName: '',
    vaccineType: '',
    dateAdministered: new Date().toISOString().split('T')[0],
    administeredBy: '',
    batchNumber: '',
    manufacturer: '',
    doseNumber: 1,
    totalDosesRequired: 1,
    expiryDate: '',
    nextDueDate: '',
    sideEffects: ''
  });

  const [exemptionForm, setExemptionForm] = useState<SetVaccinationExemptionRequest>({
    exemptionReason: '',
    exemptionDocumentPath: ''
  });

  const getStatusBadge = (status: VaccinationStatus) => {
    switch (status) {
      case VaccinationStatus.Administered: return 'success';
      case VaccinationStatus.Due: return 'warning';
      case VaccinationStatus.Overdue: return 'danger';
      case VaccinationStatus.Exempted: return 'secondary';
      case VaccinationStatus.Scheduled: return 'info';
      default: return 'secondary';
    }
  };

  const getStatusIcon = (status: VaccinationStatus) => {
    switch (status) {
      case VaccinationStatus.Administered: return faCheckCircle;
      case VaccinationStatus.Due: return faClock;
      case VaccinationStatus.Overdue: return faExclamationTriangle;
      case VaccinationStatus.Exempted: return faTimes;
      case VaccinationStatus.Scheduled: return faCalendarAlt;
      default: return faClock;
    }
  };

  const handleFilterChange = (field: keyof VaccinationFilters, value: string) => {
    setFilters(prev => ({
      ...prev,
      [field]: value
    }));
    setPage(1); // Reset to first page when filtering
  };

  const handleRecordVaccination = async (healthRecordId: string) => {
    setSelectedVaccination(healthRecordId);
    setShowRecordModal(true);
  };

  const handleSetExemption = async (healthRecordId: string) => {
    setSelectedVaccination(healthRecordId);
    setShowExemptionModal(true);
  };

  const submitRecordVaccination = async () => {
    if (!selectedVaccination) return;

    try {
      await recordVaccination({
        healthRecordId: selectedVaccination,
        vaccination: recordForm
      }).unwrap();
      
      setShowRecordModal(false);
      setRecordForm({
        vaccineName: '',
        vaccineType: '',
        dateAdministered: new Date().toISOString().split('T')[0],
        administeredBy: '',
        batchNumber: '',
        manufacturer: '',
        doseNumber: 1,
        totalDosesRequired: 1,
        expiryDate: '',
        nextDueDate: '',
        sideEffects: ''
      });
      refetch();
    } catch (error) {
      console.error('Failed to record vaccination:', error);
    }
  };

  const submitVaccinationExemption = async () => {
    if (!selectedVaccination) return;

    try {
      await setVaccinationExemption({
        vaccinationId: selectedVaccination,
        exemption: exemptionForm
      }).unwrap();
      
      setShowExemptionModal(false);
      setExemptionForm({
        exemptionReason: '',
        exemptionDocumentPath: ''
      });
      refetch();
    } catch (error) {
      console.error('Failed to set vaccination exemption:', error);
    }
  };

  const complianceStats = useMemo(() => {
    if (!complianceData) return null;

    return {
      overall: complianceData.complianceRate,
      compliant: complianceData.totalCompliant,
      overdue: complianceData.totalOverdue,
      exempted: complianceData.totalExempted,
      total: complianceData.totalRequired
    };
  }, [complianceData]);

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: '400px' }}>
        <CSpinner color="primary" size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <CAlert color="danger" className="d-flex align-items-center">
        <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-2" size="lg" />
        <div>
          Failed to load vaccination data. 
          <CButton color="link" onClick={() => refetch()} className="p-0 ms-2">
            Try again
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Vaccination Management</h2>
        <CButton color="primary" onClick={() => setShowRecordModal(true)}>
          <FontAwesomeIcon icon={faPlus} className="me-1" />
          Record Vaccination
        </CButton>
      </div>

      {/* Compliance Overview */}
      {complianceStats && (
        <CRow className="mb-4">
          <CCol md={3}>
            <CCard className="text-center">
              <CCardBody>
                <div className="fs-4 fw-semibold text-success">{complianceStats.overall.toFixed(1)}%</div>
                <div className="text-muted">Overall Compliance</div>
                <CProgress 
                  value={complianceStats.overall} 
                  color="success" 
                  className="mt-2"
                />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol md={3}>
            <CCard className="text-center">
              <CCardBody>
                <div className="fs-4 fw-semibold text-success">{complianceStats.compliant}</div>
                <div className="text-muted">Compliant</div>
              </CCardBody>
            </CCard>
          </CCol>
          <CCol md={3}>
            <CCard className="text-center">
              <CCardBody>
                <div className="fs-4 fw-semibold text-danger">{complianceStats.overdue}</div>
                <div className="text-muted">Overdue</div>
              </CCardBody>
            </CCard>
          </CCol>
          <CCol md={3}>
            <CCard className="text-center">
              <CCardBody>
                <div className="fs-4 fw-semibold text-secondary">{complianceStats.exempted}</div>
                <div className="text-muted">Exempted</div>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      )}

      {/* Filters and Search */}
      <CCard className="mb-4">
        <CCardHeader>
          <strong>Vaccination Records</strong>
        </CCardHeader>
        <CCardBody>
          <CRow className="mb-3">
            <CCol md={3}>
              <CInputGroup>
                <CFormInput
                  placeholder="Search people..."
                  value={filters.searchTerm}
                  onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                />
                <CButton type="button" color="outline-secondary">
                  <FontAwesomeIcon icon={faSearch} />
                </CButton>
              </CInputGroup>
            </CCol>
            <CCol md={3}>
              <CFormSelect
                value={filters.personType}
                onChange={(e) => handleFilterChange('personType', e.target.value)}
              >
                <option value="">All Types</option>
                <option value={PersonType.Student}>Students</option>
                <option value={PersonType.Staff}>Staff</option>
              </CFormSelect>
            </CCol>
            <CCol md={3}>
              <CFormSelect
                value={filters.status}
                onChange={(e) => handleFilterChange('status', e.target.value)}
              >
                <option value="">All Status</option>
                <option value={VaccinationStatus.Administered}>Administered</option>
                <option value={VaccinationStatus.Due}>Due</option>
                <option value={VaccinationStatus.Overdue}>Overdue</option>
                <option value={VaccinationStatus.Exempted}>Exempted</option>
                <option value={VaccinationStatus.Scheduled}>Scheduled</option>
              </CFormSelect>
            </CCol>
            <CCol md={3}>
              <CFormInput
                placeholder="Vaccine name..."
                value={filters.vaccineName}
                onChange={(e) => handleFilterChange('vaccineName', e.target.value)}
              />
            </CCol>
          </CRow>

          {complianceData?.vaccineBreakdown && complianceData.vaccineBreakdown.length > 0 ? (
            <>
              <CTable hover responsive>
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>Vaccine</CTableHeaderCell>
                    <CTableHeaderCell>Required</CTableHeaderCell>
                    <CTableHeaderCell>Compliant</CTableHeaderCell>
                    <CTableHeaderCell>Overdue</CTableHeaderCell>
                    <CTableHeaderCell>Exempted</CTableHeaderCell>
                    <CTableHeaderCell>Compliance Rate</CTableHeaderCell>
                    <CTableHeaderCell>Actions</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {complianceData.vaccineBreakdown.map((vaccine) => (
                    <CTableRow key={vaccine.vaccineName}>
                      <CTableDataCell>
                        <div className="d-flex align-items-center">
                          <FontAwesomeIcon icon={faShieldAlt} className="me-2" />
                          <strong>{vaccine.vaccineName}</strong>
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>{vaccine.required}</CTableDataCell>
                      <CTableDataCell>
                        <CBadge color="success">{vaccine.compliant}</CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color="danger">{vaccine.overdue}</CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color="secondary">{vaccine.exempted}</CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <div className="d-flex align-items-center">
                          <span className="me-2">{vaccine.complianceRate.toFixed(1)}%</span>
                          <CProgress 
                            value={vaccine.complianceRate} 
                            color={vaccine.complianceRate >= 95 ? 'success' : 
                                   vaccine.complianceRate >= 85 ? 'warning' : 'danger'}
                            style={{ width: '60px', height: '8px' }}
                          />
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CDropdown>
                          <CDropdownToggle color="ghost" size="sm">
                            <FontAwesomeIcon icon={faEllipsisV} />
                          </CDropdownToggle>
                          <CDropdownMenu>
                            <CDropdownItem onClick={() => handleRecordVaccination('')}>
                              <FontAwesomeIcon icon={faPlus} className="me-1" />
                              Record Vaccination
                            </CDropdownItem>
                            <CDropdownItem onClick={() => handleSetExemption('')}>
                              <FontAwesomeIcon icon={faTimes} className="me-1" />
                              Set Exemption
                            </CDropdownItem>
                          </CDropdownMenu>
                        </CDropdown>
                      </CTableDataCell>
                    </CTableRow>
                  ))}
                </CTableBody>
              </CTable>

              {/* Pagination */}
              <div className="d-flex justify-content-center mt-3">
                <CPagination aria-label="Vaccination records pagination">
                  <CPaginationItem
                    disabled={page === 1}
                    onClick={() => setPage(page - 1)}
                  >
                    Previous
                  </CPaginationItem>
                  
                  {/* Simple pagination for now */}
                  <CPaginationItem active>{page}</CPaginationItem>
                  
                  <CPaginationItem onClick={() => setPage(page + 1)}>
                    Next
                  </CPaginationItem>
                </CPagination>
              </div>
            </>
          ) : (
            <div className="text-center p-4">
              <FontAwesomeIcon icon={faShieldAlt} size="3x" className="text-muted mb-3" />
              <h5 className="text-muted">No Vaccination Data Found</h5>
              <p className="text-muted">Start by recording vaccinations for students and staff.</p>
              <CButton color="primary" onClick={() => setShowRecordModal(true)}>
                <FontAwesomeIcon icon={faPlus} className="me-1" />
                Record First Vaccination
              </CButton>
            </div>
          )}
        </CCardBody>
      </CCard>

      {/* Record Vaccination Modal */}
      <CModal visible={showRecordModal} onClose={() => setShowRecordModal(false)} size="lg">
        <CModalHeader>
          <CModalTitle>Record Vaccination</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel>Vaccine Name *</CFormLabel>
                <CFormInput
                  value={recordForm.vaccineName}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, vaccineName: e.target.value }))}
                  placeholder="e.g., COVID-19, Influenza"
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel>Vaccine Type</CFormLabel>
                <CFormInput
                  value={recordForm.vaccineType || ''}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, vaccineType: e.target.value }))}
                  placeholder="e.g., mRNA, Inactivated"
                />
              </CCol>
            </CRow>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel>Date Administered *</CFormLabel>
                <CFormInput
                  type="date"
                  value={recordForm.dateAdministered}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, dateAdministered: e.target.value }))}
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel>Administered By</CFormLabel>
                <CFormInput
                  value={recordForm.administeredBy || ''}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, administeredBy: e.target.value }))}
                  placeholder="Doctor/Nurse name"
                />
              </CCol>
            </CRow>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel>Batch Number</CFormLabel>
                <CFormInput
                  value={recordForm.batchNumber || ''}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, batchNumber: e.target.value }))}
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel>Manufacturer</CFormLabel>
                <CFormInput
                  value={recordForm.manufacturer || ''}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, manufacturer: e.target.value }))}
                  placeholder="e.g., Pfizer, Moderna"
                />
              </CCol>
            </CRow>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel>Dose Number</CFormLabel>
                <CFormInput
                  type="number"
                  min="1"
                  value={recordForm.doseNumber || 1}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, doseNumber: parseInt(e.target.value) || 1 }))}
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel>Total Doses Required</CFormLabel>
                <CFormInput
                  type="number"
                  min="1"
                  value={recordForm.totalDosesRequired || 1}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, totalDosesRequired: parseInt(e.target.value) || 1 }))}
                />
              </CCol>
            </CRow>
            <CRow className="mb-3">
              <CCol md={6}>
                <CFormLabel>Expiry Date</CFormLabel>
                <CFormInput
                  type="date"
                  value={recordForm.expiryDate || ''}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, expiryDate: e.target.value }))}
                />
              </CCol>
              <CCol md={6}>
                <CFormLabel>Next Due Date</CFormLabel>
                <CFormInput
                  type="date"
                  value={recordForm.nextDueDate || ''}
                  onChange={(e) => setRecordForm(prev => ({ ...prev, nextDueDate: e.target.value }))}
                />
              </CCol>
            </CRow>
            <div className="mb-3">
              <CFormLabel>Side Effects</CFormLabel>
              <CFormTextarea
                rows={3}
                value={recordForm.sideEffects || ''}
                onChange={(e) => setRecordForm(prev => ({ ...prev, sideEffects: e.target.value }))}
                placeholder="Any observed side effects..."
              />
            </div>
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="primary"
            onClick={submitRecordVaccination}
            disabled={isRecording || !recordForm.vaccineName}
          >
            {isRecording ? <CSpinner size="sm" className="me-1" /> : <FontAwesomeIcon icon={faCheckCircle} className="me-1" />}
            Record Vaccination
          </CButton>
          <CButton color="secondary" onClick={() => setShowRecordModal(false)}>
            Cancel
          </CButton>
        </CModalFooter>
      </CModal>

      {/* Set Exemption Modal */}
      <CModal visible={showExemptionModal} onClose={() => setShowExemptionModal(false)}>
        <CModalHeader>
          <CModalTitle>Set Vaccination Exemption</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel>Exemption Reason *</CFormLabel>
              <CFormTextarea
                rows={3}
                value={exemptionForm.exemptionReason}
                onChange={(e) => setExemptionForm(prev => ({ ...prev, exemptionReason: e.target.value }))}
                placeholder="Medical, religious, or other reason for exemption..."
              />
            </div>
            <div className="mb-3">
              <CFormLabel>Supporting Document Path</CFormLabel>
              <CFormInput
                value={exemptionForm.exemptionDocumentPath || ''}
                onChange={(e) => setExemptionForm(prev => ({ ...prev, exemptionDocumentPath: e.target.value }))}
                placeholder="Path to supporting documentation"
              />
            </div>
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="warning"
            onClick={submitVaccinationExemption}
            disabled={isSettingExemption || !exemptionForm.exemptionReason}
          >
            {isSettingExemption ? <CSpinner size="sm" className="me-1" /> : <FontAwesomeIcon icon={faTimes} className="me-1" />}
            Set Exemption
          </CButton>
          <CButton color="secondary" onClick={() => setShowExemptionModal(false)}>
            Cancel
          </CButton>
        </CModalFooter>
      </CModal>
    </div>
  );
};

export default VaccinationManagement;