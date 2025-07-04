import React, { useState, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CForm,
  CFormInput,
  CFormTextarea,
  CFormLabel,
  CButton,
  CRow,
  CCol,
  CAlert,
  CSpinner,
  CTabs,
  CTabList,
  CTab,
  CTabContent,
  CTabPanel,
  CInputGroup,
  CInputGroupText,
  CFormFeedback,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faBuilding,
  faGlobe,
  faPhone,
  faEnvelope,
  faMapMarkerAlt,
  faPalette,
  faIndustry,
  faCog,
  faSave,
  faEdit,
  faInfo,
} from '@fortawesome/free-solid-svg-icons';
import {
  useGetCompanyConfigurationQuery,
  useUpdateCompanyConfigurationMutation,
  useCreateCompanyConfigurationMutation,
  UpdateCompanyConfigurationRequest,
} from '../../services/companyConfigurationService';
import { useCompanyConfiguration } from '../../contexts/CompanyConfigurationContext';

const CompanyConfiguration: React.FC = () => {
  const { data: companyConfig, isLoading, error, refetch } = useGetCompanyConfigurationQuery();
  const { refreshConfig } = useCompanyConfiguration();
  const [updateCompanyConfig, { isLoading: isUpdating }] = useUpdateCompanyConfigurationMutation();
  const [createCompanyConfig, { isLoading: isCreating }] = useCreateCompanyConfigurationMutation();

  const [formData, setFormData] = useState<UpdateCompanyConfigurationRequest>({
    companyName: '',
    companyCode: '',
    companyDescription: '',
    websiteUrl: '',
    logoUrl: '',
    faviconUrl: '',
    primaryEmail: '',
    primaryPhone: '',
    emergencyContactNumber: '',
    address: '',
    city: '',
    state: '',
    postalCode: '',
    country: '',
    defaultLatitude: undefined,
    defaultLongitude: undefined,
    primaryColor: '',
    secondaryColor: '',
    accentColor: '',
    industryType: '',
    complianceStandards: '',
    regulatoryAuthority: '',
    timeZone: '',
    dateFormat: '',
    currency: '',
    language: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [successMessage, setSuccessMessage] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [activeTab, setActiveTab] = useState(1);

  // Initialize form data when companyConfig is loaded
  useEffect(() => {
    if (companyConfig) {
      setFormData({
        companyName: companyConfig.companyName || '',
        companyCode: companyConfig.companyCode || '',
        companyDescription: companyConfig.companyDescription || '',
        websiteUrl: companyConfig.websiteUrl || '',
        logoUrl: companyConfig.logoUrl || '',
        faviconUrl: companyConfig.faviconUrl || '',
        primaryEmail: companyConfig.primaryEmail || '',
        primaryPhone: companyConfig.primaryPhone || '',
        emergencyContactNumber: companyConfig.emergencyContactNumber || '',
        address: companyConfig.address || '',
        city: companyConfig.city || '',
        state: companyConfig.state || '',
        postalCode: companyConfig.postalCode || '',
        country: companyConfig.country || '',
        defaultLatitude: companyConfig.defaultLatitude,
        defaultLongitude: companyConfig.defaultLongitude,
        primaryColor: companyConfig.primaryColor || '',
        secondaryColor: companyConfig.secondaryColor || '',
        accentColor: companyConfig.accentColor || '',
        industryType: companyConfig.industryType || '',
        complianceStandards: companyConfig.complianceStandards || '',
        regulatoryAuthority: companyConfig.regulatoryAuthority || '',
        timeZone: companyConfig.timeZone || '',
        dateFormat: companyConfig.dateFormat || '',
        currency: companyConfig.currency || '',
        language: companyConfig.language || '',
      });
      setIsEditing(false);
    } else if (!isLoading && !companyConfig) {
      // No configuration exists, enable editing mode
      setIsEditing(true);
    }
  }, [companyConfig, isLoading]);

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.companyName.trim()) {
      newErrors.companyName = 'Company name is required';
    }

    if (!formData.companyCode.trim()) {
      newErrors.companyCode = 'Company code is required';
    } else if (formData.companyCode.length > 20) {
      newErrors.companyCode = 'Company code must be 20 characters or less';
    }

    if (formData.websiteUrl && !formData.websiteUrl.match(/^https?:\/\/.+/)) {
      newErrors.websiteUrl = 'Website URL must start with http:// or https://';
    }

    if (formData.primaryEmail && !formData.primaryEmail.match(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)) {
      newErrors.primaryEmail = 'Please enter a valid email address';
    }

    if (formData.defaultLatitude && (formData.defaultLatitude < -90 || formData.defaultLatitude > 90)) {
      newErrors.defaultLatitude = 'Latitude must be between -90 and 90';
    }

    if (formData.defaultLongitude && (formData.defaultLongitude < -180 || formData.defaultLongitude > 180)) {
      newErrors.defaultLongitude = 'Longitude must be between -180 and 180';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      if (companyConfig) {
        await updateCompanyConfig(formData).unwrap();
        setSuccessMessage('Company configuration updated successfully!');
      } else {
        await createCompanyConfig({
          companyName: formData.companyName,
          companyCode: formData.companyCode,
          companyDescription: formData.companyDescription,
          websiteUrl: formData.websiteUrl,
          primaryEmail: formData.primaryEmail,
        }).unwrap();
        setSuccessMessage('Company configuration created successfully!');
      }
      
      setIsEditing(false);
      await refetch();
      refreshConfig();
      
      // Clear success message after 5 seconds
      setTimeout(() => setSuccessMessage(''), 5000);
    } catch (error: any) {
      console.error('Failed to save company configuration:', error);
      setErrors({ general: 'Failed to save configuration. Please try again.' });
    }
  };

  const handleInputChange = (field: keyof UpdateCompanyConfigurationRequest, value: string | number | undefined) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error for this field when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  if (isLoading) {
    return (
      <CCard>
        <CCardBody className="text-center py-5">
          <CSpinner color="primary" />
          <div className="mt-2">Loading company configuration...</div>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <CCard>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <h5 className="mb-0">
          <FontAwesomeIcon icon={faBuilding} className="me-2" />
          Company Configuration
        </h5>
        {companyConfig && !isEditing && (
          <CButton
            color="primary"
            size="sm"
            onClick={() => setIsEditing(true)}
          >
            <FontAwesomeIcon icon={faEdit} className="me-1" />
            Edit
          </CButton>
        )}
      </CCardHeader>
      <CCardBody>
        {successMessage && (
          <CAlert color="success" className="mb-3">
            {successMessage}
          </CAlert>
        )}

        {errors.general && (
          <CAlert color="danger" className="mb-3">
            {errors.general}
          </CAlert>
        )}

        {error && (
          <CAlert color="warning" className="mb-3">
            Failed to load company configuration. Please refresh the page.
          </CAlert>
        )}

        {!companyConfig && !isLoading && (
          <CAlert color="info" className="mb-3">
            <FontAwesomeIcon icon={faInfo} className="me-2" />
            No company configuration found. Please create one to customize your organization's details.
          </CAlert>
        )}

        <CForm onSubmit={handleSubmit}>
          <CTabs activeItemKey={activeTab}>
            <CTabList variant="pills" className="mb-3">
              <CTab itemKey={1} onClick={() => setActiveTab(1)}>
                <FontAwesomeIcon icon={faBuilding} className="me-1" />
                Basic Information
              </CTab>
              <CTab itemKey={2} onClick={() => setActiveTab(2)}>
                <FontAwesomeIcon icon={faPhone} className="me-1" />
                Contact & Address
              </CTab>
              <CTab itemKey={3} onClick={() => setActiveTab(3)}>
                <FontAwesomeIcon icon={faPalette} className="me-1" />
                Branding & Theme
              </CTab>
              <CTab itemKey={4} onClick={() => setActiveTab(4)}>
                <FontAwesomeIcon icon={faIndustry} className="me-1" />
                Compliance & Settings
              </CTab>
            </CTabList>

            <CTabContent>
              {/* Basic Information Tab */}
              <CTabPanel itemKey={1}>
                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="companyName">Company Name *</CFormLabel>
                      <CFormInput
                        id="companyName"
                        value={formData.companyName}
                        onChange={(e) => handleInputChange('companyName', e.target.value)}
                        invalid={!!errors.companyName}
                        disabled={!isEditing}
                      />
                      <CFormFeedback invalid>{errors.companyName}</CFormFeedback>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="companyCode">Company Code *</CFormLabel>
                      <CFormInput
                        id="companyCode"
                        value={formData.companyCode}
                        onChange={(e) => handleInputChange('companyCode', e.target.value.toUpperCase())}
                        invalid={!!errors.companyCode}
                        disabled={!isEditing}
                        maxLength={20}
                      />
                      <CFormFeedback invalid>{errors.companyCode}</CFormFeedback>
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="companyDescription">Description</CFormLabel>
                  <CFormTextarea
                    id="companyDescription"
                    rows={3}
                    value={formData.companyDescription}
                    onChange={(e) => handleInputChange('companyDescription', e.target.value)}
                    disabled={!isEditing}
                    placeholder="Brief description of your organization..."
                  />
                </div>

                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="websiteUrl">Website URL</CFormLabel>
                      <CInputGroup>
                        <CInputGroupText>
                          <FontAwesomeIcon icon={faGlobe} />
                        </CInputGroupText>
                        <CFormInput
                          id="websiteUrl"
                          value={formData.websiteUrl}
                          onChange={(e) => handleInputChange('websiteUrl', e.target.value)}
                          invalid={!!errors.websiteUrl}
                          disabled={!isEditing}
                          placeholder="https://your-company.com"
                        />
                        <CFormFeedback invalid>{errors.websiteUrl}</CFormFeedback>
                      </CInputGroup>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="logoUrl">Logo URL</CFormLabel>
                      <CFormInput
                        id="logoUrl"
                        value={formData.logoUrl}
                        onChange={(e) => handleInputChange('logoUrl', e.target.value)}
                        disabled={!isEditing}
                        placeholder="https://your-company.com/logo.png"
                      />
                    </div>
                  </CCol>
                </CRow>
              </CTabPanel>

              {/* Contact & Address Tab */}
              <CTabPanel itemKey={2}>
                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="primaryEmail">Primary Email</CFormLabel>
                      <CInputGroup>
                        <CInputGroupText>
                          <FontAwesomeIcon icon={faEnvelope} />
                        </CInputGroupText>
                        <CFormInput
                          id="primaryEmail"
                          type="email"
                          value={formData.primaryEmail}
                          onChange={(e) => handleInputChange('primaryEmail', e.target.value)}
                          invalid={!!errors.primaryEmail}
                          disabled={!isEditing}
                        />
                        <CFormFeedback invalid>{errors.primaryEmail}</CFormFeedback>
                      </CInputGroup>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="primaryPhone">Primary Phone</CFormLabel>
                      <CInputGroup>
                        <CInputGroupText>
                          <FontAwesomeIcon icon={faPhone} />
                        </CInputGroupText>
                        <CFormInput
                          id="primaryPhone"
                          value={formData.primaryPhone}
                          onChange={(e) => handleInputChange('primaryPhone', e.target.value)}
                          disabled={!isEditing}
                        />
                      </CInputGroup>
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="emergencyContactNumber">Emergency Contact Number</CFormLabel>
                  <CFormInput
                    id="emergencyContactNumber"
                    value={formData.emergencyContactNumber}
                    onChange={(e) => handleInputChange('emergencyContactNumber', e.target.value)}
                    disabled={!isEditing}
                    placeholder="Emergency contact for critical situations"
                  />
                </div>

                <div className="mb-3">
                  <CFormLabel htmlFor="address">Address</CFormLabel>
                  <CFormInput
                    id="address"
                    value={formData.address}
                    onChange={(e) => handleInputChange('address', e.target.value)}
                    disabled={!isEditing}
                  />
                </div>

                <CRow>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="city">City</CFormLabel>
                      <CFormInput
                        id="city"
                        value={formData.city}
                        onChange={(e) => handleInputChange('city', e.target.value)}
                        disabled={!isEditing}
                      />
                    </div>
                  </CCol>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="state">State/Province</CFormLabel>
                      <CFormInput
                        id="state"
                        value={formData.state}
                        onChange={(e) => handleInputChange('state', e.target.value)}
                        disabled={!isEditing}
                      />
                    </div>
                  </CCol>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="country">Country</CFormLabel>
                      <CFormInput
                        id="country"
                        value={formData.country}
                        onChange={(e) => handleInputChange('country', e.target.value)}
                        disabled={!isEditing}
                      />
                    </div>
                  </CCol>
                </CRow>

                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="defaultLatitude">Default Latitude</CFormLabel>
                      <CInputGroup>
                        <CInputGroupText>
                          <FontAwesomeIcon icon={faMapMarkerAlt} />
                        </CInputGroupText>
                        <CFormInput
                          id="defaultLatitude"
                          type="number"
                          step="0.0001"
                          value={formData.defaultLatitude || ''}
                          onChange={(e) => handleInputChange('defaultLatitude', e.target.value ? parseFloat(e.target.value) : undefined)}
                          invalid={!!errors.defaultLatitude}
                          disabled={!isEditing}
                          placeholder="-6.1751"
                        />
                        <CFormFeedback invalid>{errors.defaultLatitude}</CFormFeedback>
                      </CInputGroup>
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="defaultLongitude">Default Longitude</CFormLabel>
                      <CFormInput
                        id="defaultLongitude"
                        type="number"
                        step="0.0001"
                        value={formData.defaultLongitude || ''}
                        onChange={(e) => handleInputChange('defaultLongitude', e.target.value ? parseFloat(e.target.value) : undefined)}
                        invalid={!!errors.defaultLongitude}
                        disabled={!isEditing}
                        placeholder="106.8650"
                      />
                      <CFormFeedback invalid>{errors.defaultLongitude}</CFormFeedback>
                    </div>
                  </CCol>
                </CRow>
              </CTabPanel>

              {/* Branding & Theme Tab */}
              <CTabPanel itemKey={3}>
                <CRow>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="primaryColor">Primary Color</CFormLabel>
                      <CFormInput
                        id="primaryColor"
                        type="color"
                        value={formData.primaryColor}
                        onChange={(e) => handleInputChange('primaryColor', e.target.value)}
                        disabled={!isEditing}
                      />
                    </div>
                  </CCol>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="secondaryColor">Secondary Color</CFormLabel>
                      <CFormInput
                        id="secondaryColor"
                        type="color"
                        value={formData.secondaryColor}
                        onChange={(e) => handleInputChange('secondaryColor', e.target.value)}
                        disabled={!isEditing}
                      />
                    </div>
                  </CCol>
                  <CCol md={4}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="accentColor">Accent Color</CFormLabel>
                      <CFormInput
                        id="accentColor"
                        type="color"
                        value={formData.accentColor}
                        onChange={(e) => handleInputChange('accentColor', e.target.value)}
                        disabled={!isEditing}
                      />
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="faviconUrl">Favicon URL</CFormLabel>
                  <CFormInput
                    id="faviconUrl"
                    value={formData.faviconUrl}
                    onChange={(e) => handleInputChange('faviconUrl', e.target.value)}
                    disabled={!isEditing}
                    placeholder="https://your-company.com/favicon.ico"
                  />
                </div>
              </CTabPanel>

              {/* Compliance & Settings Tab */}
              <CTabPanel itemKey={4}>
                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="industryType">Industry Type</CFormLabel>
                      <CFormInput
                        id="industryType"
                        value={formData.industryType}
                        onChange={(e) => handleInputChange('industryType', e.target.value)}
                        disabled={!isEditing}
                        placeholder="e.g., Education, Manufacturing, Healthcare"
                      />
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="regulatoryAuthority">Regulatory Authority</CFormLabel>
                      <CFormInput
                        id="regulatoryAuthority"
                        value={formData.regulatoryAuthority}
                        onChange={(e) => handleInputChange('regulatoryAuthority', e.target.value)}
                        disabled={!isEditing}
                        placeholder="e.g., Ministry of Education"
                      />
                    </div>
                  </CCol>
                </CRow>

                <div className="mb-3">
                  <CFormLabel htmlFor="complianceStandards">Compliance Standards</CFormLabel>
                  <CFormTextarea
                    id="complianceStandards"
                    rows={3}
                    value={formData.complianceStandards}
                    onChange={(e) => handleInputChange('complianceStandards', e.target.value)}
                    disabled={!isEditing}
                    placeholder="e.g., ISO 45001, ISO 14001, International School Standards"
                  />
                </div>

                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="timeZone">Time Zone</CFormLabel>
                      <CFormInput
                        id="timeZone"
                        value={formData.timeZone}
                        onChange={(e) => handleInputChange('timeZone', e.target.value)}
                        disabled={!isEditing}
                        placeholder="e.g., Asia/Jakarta"
                      />
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="currency">Currency</CFormLabel>
                      <CFormInput
                        id="currency"
                        value={formData.currency}
                        onChange={(e) => handleInputChange('currency', e.target.value)}
                        disabled={!isEditing}
                        placeholder="e.g., USD, IDR, EUR"
                      />
                    </div>
                  </CCol>
                </CRow>

                <CRow>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="dateFormat">Date Format</CFormLabel>
                      <CFormInput
                        id="dateFormat"
                        value={formData.dateFormat}
                        onChange={(e) => handleInputChange('dateFormat', e.target.value)}
                        disabled={!isEditing}
                        placeholder="e.g., DD/MM/YYYY, MM/DD/YYYY"
                      />
                    </div>
                  </CCol>
                  <CCol md={6}>
                    <div className="mb-3">
                      <CFormLabel htmlFor="language">Language</CFormLabel>
                      <CFormInput
                        id="language"
                        value={formData.language}
                        onChange={(e) => handleInputChange('language', e.target.value)}
                        disabled={!isEditing}
                        placeholder="e.g., en-US, id-ID"
                      />
                    </div>
                  </CCol>
                </CRow>
              </CTabPanel>
            </CTabContent>
          </CTabs>

          {isEditing && (
            <div className="d-flex justify-content-end gap-2 mt-4">
              <CButton
                color="secondary"
                onClick={() => {
                  setIsEditing(false);
                  setErrors({});
                  // Reset form data if we have existing config
                  if (companyConfig) {
                    setFormData({
                      companyName: companyConfig.companyName || '',
                      companyCode: companyConfig.companyCode || '',
                      companyDescription: companyConfig.companyDescription || '',
                      websiteUrl: companyConfig.websiteUrl || '',
                      logoUrl: companyConfig.logoUrl || '',
                      faviconUrl: companyConfig.faviconUrl || '',
                      primaryEmail: companyConfig.primaryEmail || '',
                      primaryPhone: companyConfig.primaryPhone || '',
                      emergencyContactNumber: companyConfig.emergencyContactNumber || '',
                      address: companyConfig.address || '',
                      city: companyConfig.city || '',
                      state: companyConfig.state || '',
                      postalCode: companyConfig.postalCode || '',
                      country: companyConfig.country || '',
                      defaultLatitude: companyConfig.defaultLatitude,
                      defaultLongitude: companyConfig.defaultLongitude,
                      primaryColor: companyConfig.primaryColor || '',
                      secondaryColor: companyConfig.secondaryColor || '',
                      accentColor: companyConfig.accentColor || '',
                      industryType: companyConfig.industryType || '',
                      complianceStandards: companyConfig.complianceStandards || '',
                      regulatoryAuthority: companyConfig.regulatoryAuthority || '',
                      timeZone: companyConfig.timeZone || '',
                      dateFormat: companyConfig.dateFormat || '',
                      currency: companyConfig.currency || '',
                      language: companyConfig.language || '',
                    });
                  }
                }}
                disabled={isUpdating || isCreating}
              >
                Cancel
              </CButton>
              <CButton
                type="submit"
                color="primary"
                disabled={isUpdating || isCreating}
              >
                {isUpdating || isCreating ? (
                  <>
                    <CSpinner size="sm" className="me-1" />
                    Saving...
                  </>
                ) : (
                  <>
                    <FontAwesomeIcon icon={faSave} className="me-1" />
                    Save Configuration
                  </>
                )}
              </CButton>
            </div>
          )}
        </CForm>
      </CCardBody>
    </CCard>
  );
};

export default CompanyConfiguration;
