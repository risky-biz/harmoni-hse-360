import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CForm,
  CFormInput,
  CFormLabel,
  CFormTextarea,
  CFormSelect,
  CRow,
  CCol,
  CButton,
  CAlert,
  CSpinner,
  CAccordion,
  CAccordionBody,
  CAccordionHeader,
  CAccordionItem,
  CBadge,
  CCallout,
  CInputGroup,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faMapPin } from '@fortawesome/free-solid-svg-icons';
import { useNavigate } from 'react-router-dom';
import { useCreateHazardMutation } from '../../features/hazards/hazardApi';
import { useAuth } from '../../hooks/useAuth';
import { 
  HAZARD_SEVERITIES,
  CreateHazardRequest 
} from '../../types/hazard';
import { 
  useGetHazardCategoriesQuery,
  useGetHazardTypesQuery 
} from '../../api/hazardConfigurationApi';
import { ACTION_ICONS, CONTEXT_ICONS, HAZARD_ICONS } from '../../utils/iconMappings';
import { PermissionGuard } from '../../components/auth/PermissionGuard';
import { ModuleType, PermissionType } from '../../types/permissions';
import { shouldUseMobileInterface } from '../../utils/deviceUtils';

const CreateHazard: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [createHazard, { isLoading, error }] = useCreateHazardMutation();
  
  // Fetch hazard categories and types from API
  const { data: categories = [], isLoading: isCategoriesLoading } = useGetHazardCategoriesQuery();
  const { data: types = [], isLoading: isTypesLoading } = useGetHazardTypesQuery();
  
  const [formData, setFormData] = useState<Partial<CreateHazardRequest>>({
    title: '',
    description: '',
    category: '',
    type: '',
    location: '',
    severity: '',
    reporterId: user?.id || 0,
    reporterDepartment: user?.department || '',
    expectedResolutionDate: '',
    latitude: undefined,
    longitude: undefined,
  });

  const [attachments, setAttachments] = useState<File[]>([]);
  const [locationLoading, setLocationLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const hazardData: CreateHazardRequest = {
        ...formData as CreateHazardRequest,
        attachments: attachments.length > 0 ? attachments : undefined,
      };
      
      const result = await createHazard(hazardData).unwrap();
      navigate(`/hazards/${result.id}`);
    } catch (err) {
      console.error('Failed to create hazard:', err);
    }
  };

  const handleInputChange = (field: keyof CreateHazardRequest, value: string | number) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setAttachments(Array.from(e.target.files));
    }
  };

  // Get current location using GPS
  const getCurrentLocation = () => {
    setLocationLoading(true);

    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const lat = position.coords.latitude;
          const lng = position.coords.longitude;
          
          // Update form data with coordinates
          setFormData(prev => ({
            ...prev,
            latitude: lat,
            longitude: lng,
            location: `${lat.toFixed(6)}, ${lng.toFixed(6)}`
          }));
          
          setLocationLoading(false);
        },
        (error) => {
          console.error('Geolocation error:', error);
          setLocationLoading(false);
          alert(
            'Unable to get your location. Please enter the location manually.'
          );
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 60000
        }
      );
    } else {
      setLocationLoading(false);
      alert('Geolocation is not supported by this browser.');
    }
  };

  return (
    <PermissionGuard 
      module={ModuleType.RiskManagement} 
      permission={PermissionType.Create}
      fallback={
        <div className="text-center p-4">
          <h3>Access Denied</h3>
          <p>You don't have permission to create hazard reports.</p>
        </div>
      }
    >
      <CRow>
        <CCol xs={12}>
          <CCard className="shadow-sm">
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <div>
                <h4
                  className="mb-0"
                  style={{
                    color: 'var(--harmoni-charcoal)',
                    fontFamily: 'Poppins, sans-serif',
                  }}
                >
                  <FontAwesomeIcon
                    icon={HAZARD_ICONS.reporting}
                    size="lg"
                    className="me-2 text-warning"
                  />
                  Report New Hazard
                </h4>
                <small className="text-muted">
                  Fill out all required information to report a hazard
                </small>
              </div>
              <div className="d-flex align-items-center gap-2">
                <CButton
                  color="secondary"
                  variant="outline"
                  onClick={() => navigate('/hazards')}
                >
                  <FontAwesomeIcon icon={ACTION_ICONS.back} size="sm" className="me-1" />
                  Back to List
                </CButton>
              </div>
            </CCardHeader>
          <CCardBody>
            {shouldUseMobileInterface() && (
              <CCallout color="info" className="mb-4">
                <div className="d-flex justify-content-between align-items-center">
                  <div>
                    <strong>📱 Mobile-Optimized Option Available</strong>
                    <p className="mb-0 mt-1">
                      For a better mobile experience with step-by-step guidance and camera integration, try our mobile-optimized interface.
                    </p>
                  </div>
                  <CButton
                    color="primary"
                    variant="outline"
                    size="sm"
                    onClick={() => navigate('/hazards/mobile-report')}
                    className="ms-3 flex-shrink-0"
                  >
                    Switch to Mobile
                  </CButton>
                </div>
              </CCallout>
            )}
            
            <CForm onSubmit={handleSubmit}>
              {error && (
                <CAlert
                  color="danger"
                  dismissible
                  className="mb-4"
                >
                  Failed to create hazard. Please try again.
                </CAlert>
              )}

              <CCallout color="warning" className="mb-4">
                <FontAwesomeIcon icon={HAZARD_ICONS.warning} className="me-2" />
                <strong>Important:</strong> Report hazards immediately to prevent potential incidents. 
                Provide as much detail as possible to ensure proper risk assessment and mitigation.
              </CCallout>

              {/* Basic Information Section */}
              <CAccordion>
                <CAccordionItem itemKey={1}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={CONTEXT_ICONS.basicInformation} className="me-2 text-primary" />
                      <strong>Basic Information</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={12}>
                        <CFormLabel htmlFor="title">
                          Hazard Title *
                        </CFormLabel>
                        <CFormInput
                          id="title"
                          value={formData.title}
                          onChange={(e) => handleInputChange('title', e.target.value)}
                          placeholder="Brief description of the hazard"
                          required
                        />
                      </CCol>
                    </CRow>

                    <CRow className="mb-3">
                      <CCol md={4}>
                        <CFormLabel htmlFor="severity">
                          Severity Level *
                        </CFormLabel>
                        <CFormSelect
                          id="severity"
                          value={formData.severity}
                          onChange={(e) => handleInputChange('severity', e.target.value)}
                          required
                        >
                          <option value="">Select Severity</option>
                          {HAZARD_SEVERITIES.map(severity => (
                            <option key={severity} value={severity}>{severity}</option>
                          ))}
                        </CFormSelect>
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="category">
                          Hazard Category *
                        </CFormLabel>
                        <CFormSelect
                          id="category"
                          value={formData.category}
                          onChange={(e) => handleInputChange('category', e.target.value)}
                          required
                          disabled={isCategoriesLoading}
                        >
                          <option value="">
                            {isCategoriesLoading ? 'Loading categories...' : 'Select Category'}
                          </option>
                          {categories.filter(cat => cat.isActive).map(category => (
                            <option key={category.id} value={category.name}>
                              {category.name}
                            </option>
                          ))}
                        </CFormSelect>
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="type">
                          Hazard Type *
                        </CFormLabel>
                        <CFormSelect
                          id="type"
                          value={formData.type}
                          onChange={(e) => handleInputChange('type', e.target.value)}
                          required
                          disabled={isTypesLoading}
                        >
                          <option value="">
                            {isTypesLoading ? 'Loading types...' : 'Select Type'}
                          </option>
                          {types.filter(type => type.isActive).map(type => (
                            <option key={type.id} value={type.name}>
                              {type.name}
                            </option>
                          ))}
                        </CFormSelect>
                      </CCol>
                    </CRow>

                    <div className="mb-3">
                      <CFormLabel htmlFor="description">
                        Detailed Description *
                      </CFormLabel>
                      <CFormTextarea
                        id="description"
                        rows={4}
                        value={formData.description}
                        onChange={(e) => handleInputChange('description', e.target.value)}
                        placeholder="Provide detailed information about the hazard, including conditions, potential risks, and any relevant circumstances"
                        required
                      />
                    </div>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Location and Time Section */}
                <CAccordionItem itemKey={2}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={CONTEXT_ICONS.locationTime} className="me-2 text-success" />
                      <strong>Location Information</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <CRow className="mb-3">
                      <CCol md={8}>
                        <CFormLabel htmlFor="location">
                          Hazard Location *
                        </CFormLabel>
                        <CInputGroup>
                          <CFormInput
                            id="location"
                            value={formData.location}
                            onChange={(e) => handleInputChange('location', e.target.value)}
                            placeholder="Enter location or click GPS button for coordinates"
                            required
                            readOnly={locationLoading}
                          />
                          <CButton
                            type="button"
                            color="primary"
                            variant="outline"
                            onClick={getCurrentLocation}
                            disabled={locationLoading}
                            title="Get current GPS coordinates"
                          >
                            {locationLoading ? (
                              <CSpinner size="sm" />
                            ) : (
                              <FontAwesomeIcon icon={faMapPin} />
                            )}
                          </CButton>
                        </CInputGroup>
                        <small className="text-muted">
                          {formData.latitude && formData.longitude 
                            ? `GPS coordinates: ${formData.latitude.toFixed(6)}, ${formData.longitude.toFixed(6)}`
                            : 'Click GPS button to automatically capture current location coordinates'
                          }
                        </small>
                      </CCol>
                      <CCol md={4}>
                        <CFormLabel htmlFor="expectedResolutionDate">
                          Expected Resolution Date
                        </CFormLabel>
                        <CFormInput
                          type="date"
                          id="expectedResolutionDate"
                          value={formData.expectedResolutionDate}
                          onChange={(e) => handleInputChange('expectedResolutionDate', e.target.value)}
                        />
                      </CCol>
                    </CRow>
                  </CAccordionBody>
                </CAccordionItem>

                {/* Evidence Upload Section */}
                <CAccordionItem itemKey={3}>
                  <CAccordionHeader>
                    <div className="d-flex align-items-center">
                      <FontAwesomeIcon icon={CONTEXT_ICONS.evidence} className="me-2 text-info" />
                      <strong>Evidence & Documentation</strong>
                    </div>
                  </CAccordionHeader>
                  <CAccordionBody>
                    <div className="mb-3">
                      <CFormLabel htmlFor="attachments">
                        Upload Photos or Documents
                      </CFormLabel>
                      <CFormInput
                        type="file"
                        id="attachments"
                        multiple
                        accept="image/*,.pdf,.doc,.docx"
                        onChange={handleFileChange}
                      />
                      <small className="text-muted">
                        Supported formats: Images (JPG, PNG), PDF, Word documents. 
                        Multiple files can be selected.
                      </small>
                    </div>

                    {attachments.length > 0 && (
                      <div className="mt-3">
                        <h6>Selected Files:</h6>
                        <ul className="list-unstyled">
                          {attachments.map((file, index) => (
                            <li key={index} className="d-flex align-items-center mb-2">
                              <FontAwesomeIcon icon={ACTION_ICONS.attach} className="me-2 text-muted" />
                              <span className="small">{file.name}</span>
                              <CBadge color="secondary" className="ms-2">
                                {(file.size / 1024 / 1024).toFixed(2)} MB
                              </CBadge>
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}
                  </CAccordionBody>
                </CAccordionItem>
              </CAccordion>

              {/* Submit Section */}
              <div className="mt-4 d-flex gap-2">
                <CButton 
                  type="submit" 
                  color="warning" 
                  disabled={isLoading}
                  className="px-4"
                >
                  {isLoading && <CSpinner size="sm" className="me-2" />}
                  <FontAwesomeIcon icon={HAZARD_ICONS.reporting} className="me-2" />
                  Submit Hazard Report
                </CButton>
                <CButton 
                  type="button" 
                  color="secondary" 
                  variant="outline"
                  onClick={() => navigate('/hazards')}
                >
                  <FontAwesomeIcon icon={ACTION_ICONS.cancel} className="me-1" />
                  Cancel
                </CButton>
              </div>
            </CForm>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
    </PermissionGuard>
  );
};

export default CreateHazard;