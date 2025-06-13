import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CForm,
  CFormInput,
  CFormLabel,
  CFormTextarea,
  CFormSelect,
  CAlert,
  CSpinner,
  CBadge,
  CCallout,
} from '@coreui/react';
import { Icon } from '../../components/common/Icon';
import {
  faQrcode,
  faMapMarkerAlt,
  faPaperPlane,
} from '@fortawesome/free-solid-svg-icons';

interface QuickReportData {
  title: string;
  description: string;
  severity: string;
  location: string;
  incidentDate: string;
  reporterName: string;
  reporterEmail: string;
  isAnonymous: boolean;
  qrCodeId?: string;
  latitude?: number;
  longitude?: number;
  photos: string[];
}

const QuickReport: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [formData, setFormData] = useState<QuickReportData>({
    title: '',
    description: '',
    severity: 'Minor',
    location: '',
    incidentDate: new Date().toISOString().slice(0, 16),
    reporterName: '',
    reporterEmail: '',
    isAnonymous: false,
    photos: [],
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitResult, setSubmitResult] = useState<{
    type: 'success' | 'error';
    message: string;
    referenceNumber?: string;
  } | null>(null);
  const [useLocation, setUseLocation] = useState(false);

  useEffect(() => {
    // Check for QR code ID from URL params
    const qrId = searchParams.get('qr');
    const location = searchParams.get('location');
    const lat = searchParams.get('lat');
    const lng = searchParams.get('lng');

    if (qrId) {
      setFormData((prev) => ({
        ...prev,
        qrCodeId: qrId,
        location: location || prev.location,
        latitude: lat ? parseFloat(lat) : undefined,
        longitude: lng ? parseFloat(lng) : undefined,
      }));
    }

    // Get user's location if they allow it
    if (navigator.geolocation && useLocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          setFormData((prev) => ({
            ...prev,
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
          }));
        },
        (error) => {
          console.warn('Location access denied:', error);
        }
      );
    }
  }, [searchParams, useLocation]);

  const handleInputChange = (
    field: keyof QuickReportData,
    value: string | boolean
  ) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handlePhotoCapture = async (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const files = event.target.files;
    if (!files) return;

    const newPhotos: string[] = [];
    for (let i = 0; i < Math.min(files.length, 5); i++) {
      const file = files[i];
      if (file.type.startsWith('image/')) {
        try {
          const base64 = await convertToBase64(file);
          newPhotos.push(base64);
        } catch (error) {
          console.error('Error converting photo:', error);
        }
      }
    }

    setFormData((prev) => ({
      ...prev,
      photos: [...prev.photos, ...newPhotos].slice(0, 5), // Max 5 photos
    }));
  };

  const convertToBase64 = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        const result = reader.result as string;
        // Remove data:image/jpeg;base64, prefix
        const base64 = result.split(',')[1];
        resolve(base64);
      };
      reader.onerror = reject;
    });
  };

  const removePhoto = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      photos: prev.photos.filter((_, i) => i !== index),
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setSubmitResult(null);

    try {
      const payload = {
        title: formData.title,
        description: formData.description,
        severity: formData.severity,
        location: formData.location,
        incidentDate: new Date(formData.incidentDate).toISOString(),
        reporterName: formData.isAnonymous
          ? 'Anonymous Reporter'
          : formData.reporterName,
        reporterEmail: formData.isAnonymous ? '' : formData.reporterEmail,
        isAnonymous: formData.isAnonymous,
        qrCodeId: formData.qrCodeId,
        latitude: formData.latitude,
        longitude: formData.longitude,
        reportingChannel: formData.qrCodeId ? 'QR' : 'QuickWeb',
        deviceInfo: navigator.userAgent,
        photoBase64: formData.photos,
      };

      const endpoint = formData.isAnonymous
        ? '/api/multichannel-reporting/anonymous'
        : '/api/multichannel-reporting/quick-report';

      const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (response.ok) {
        const result = await response.json();
        setSubmitResult({
          type: 'success',
          message: result.message || 'Report submitted successfully!',
          referenceNumber: result.referenceNumber,
        });

        // Clear form after successful submission
        setFormData({
          title: '',
          description: '',
          severity: 'Minor',
          location: formData.location, // Keep location if from QR code
          incidentDate: new Date().toISOString().slice(0, 16),
          reporterName: '',
          reporterEmail: '',
          isAnonymous: false,
          photos: [],
        });
      } else {
        const error = await response.json();
        setSubmitResult({
          type: 'error',
          message:
            error.message || 'Failed to submit report. Please try again.',
        });
      }
    } catch (error) {
      setSubmitResult({
        type: 'error',
        message: 'Network error. Please check your connection and try again.',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'Critical':
        return 'danger';
      case 'Serious':
        return 'warning';
      case 'Moderate':
        return 'info';
      default:
        return 'success';
    }
  };

  return (
    <CRow className="justify-content-center">
      <CCol xs={12} md={8} lg={6}>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0">Quick Incident Report</h4>
              <small className="text-muted">
                Fast and easy incident reporting
              </small>
            </div>
            {formData.qrCodeId && (
              <CBadge color="info">
                <Icon icon={faQrcode} className="me-1" />
                QR Code Location
              </CBadge>
            )}
          </CCardHeader>

          <CCardBody>
            {submitResult && (
              <CAlert
                color={submitResult.type === 'success' ? 'success' : 'danger'}
                dismissible
                onClose={() => setSubmitResult(null)}
              >
                <strong>
                  {submitResult.type === 'success' ? 'Success!' : 'Error!'}
                </strong>{' '}
                {submitResult.message}
                {submitResult.referenceNumber && (
                  <div className="mt-2">
                    <strong>Reference Number:</strong>{' '}
                    {submitResult.referenceNumber}
                  </div>
                )}
              </CAlert>
            )}

            {formData.qrCodeId && (
              <CCallout color="info" className="mb-4">
                <strong>Location detected from QR code</strong>
                <br />
                This form has been pre-filled with location information from the
                QR code you scanned.
              </CCallout>
            )}

            <CForm onSubmit={handleSubmit}>
              {/* Anonymous Toggle */}
              <div className="mb-3">
                <div className="form-check form-switch">
                  <input
                    className="form-check-input"
                    type="checkbox"
                    id="anonymousToggle"
                    checked={formData.isAnonymous}
                    onChange={(e) =>
                      handleInputChange('isAnonymous', e.target.checked)
                    }
                  />
                  <label className="form-check-label" htmlFor="anonymousToggle">
                    Submit as anonymous report
                  </label>
                </div>
                <small className="text-muted">
                  Anonymous reports are investigated but you won't receive
                  status updates
                </small>
              </div>

              {/* Reporter Information */}
              {!formData.isAnonymous && (
                <CRow className="mb-3">
                  <CCol md={6}>
                    <CFormLabel htmlFor="reporterName">Your Name *</CFormLabel>
                    <CFormInput
                      type="text"
                      id="reporterName"
                      value={formData.reporterName}
                      onChange={(e) =>
                        handleInputChange('reporterName', e.target.value)
                      }
                      required={!formData.isAnonymous}
                      placeholder="Enter your full name"
                    />
                  </CCol>
                  <CCol md={6}>
                    <CFormLabel htmlFor="reporterEmail">
                      Your Email *
                    </CFormLabel>
                    <CFormInput
                      type="email"
                      id="reporterEmail"
                      value={formData.reporterEmail}
                      onChange={(e) =>
                        handleInputChange('reporterEmail', e.target.value)
                      }
                      required={!formData.isAnonymous}
                      placeholder="your.email@example.com"
                    />
                  </CCol>
                </CRow>
              )}

              {/* Incident Details */}
              <div className="mb-3">
                <CFormLabel htmlFor="title">Incident Title *</CFormLabel>
                <CFormInput
                  type="text"
                  id="title"
                  value={formData.title}
                  onChange={(e) => handleInputChange('title', e.target.value)}
                  required
                  placeholder="Brief description of what happened"
                />
              </div>

              <div className="mb-3">
                <CFormLabel htmlFor="description">Description *</CFormLabel>
                <CFormTextarea
                  id="description"
                  rows={4}
                  value={formData.description}
                  onChange={(e) =>
                    handleInputChange('description', e.target.value)
                  }
                  required
                  placeholder="Provide detailed information about the incident..."
                />
              </div>

              <CRow className="mb-3">
                <CCol md={6}>
                  <CFormLabel htmlFor="severity">Severity</CFormLabel>
                  <CFormSelect
                    id="severity"
                    value={formData.severity}
                    onChange={(e) =>
                      handleInputChange('severity', e.target.value)
                    }
                  >
                    <option value="Minor">Minor</option>
                    <option value="Moderate">Moderate</option>
                    <option value="Serious">Serious</option>
                    <option value="Critical">Critical</option>
                  </CFormSelect>
                  <CBadge
                    color={getSeverityColor(formData.severity)}
                    className="mt-1"
                  >
                    {formData.severity}
                  </CBadge>
                </CCol>
                <CCol md={6}>
                  <CFormLabel htmlFor="incidentDate">
                    When did this occur?
                  </CFormLabel>
                  <CFormInput
                    type="datetime-local"
                    id="incidentDate"
                    value={formData.incidentDate}
                    onChange={(e) =>
                      handleInputChange('incidentDate', e.target.value)
                    }
                  />
                </CCol>
              </CRow>

              {/* Location */}
              <div className="mb-3">
                <CFormLabel htmlFor="location">
                  Location *
                  {formData.latitude && formData.longitude && (
                    <CBadge color="success" className="ms-2">
                      <Icon icon={faMapMarkerAlt} className="me-1" />
                      GPS Coordinates
                    </CBadge>
                  )}
                </CFormLabel>
                <CFormInput
                  type="text"
                  id="location"
                  value={formData.location}
                  onChange={(e) =>
                    handleInputChange('location', e.target.value)
                  }
                  required
                  placeholder="Building, room, or area where incident occurred"
                />
                {!formData.latitude && (
                  <div className="mt-2">
                    <CButton
                      color="secondary"
                      variant="outline"
                      size="sm"
                      onClick={() => setUseLocation(true)}
                      disabled={useLocation}
                    >
                      <Icon icon={faMapMarkerAlt} className="me-1" />
                      Use My Location
                    </CButton>
                  </div>
                )}
              </div>

              {/* Photo Capture */}
              <div className="mb-3">
                <CFormLabel htmlFor="photos">
                  Photos ({formData.photos.length}/5)
                  <small className="text-muted ms-2">
                    Optional - Add visual evidence
                  </small>
                </CFormLabel>

                {formData.photos.length < 5 && (
                  <div className="mb-2">
                    <input
                      type="file"
                      id="photos"
                      accept="image/*"
                      multiple
                      capture="environment"
                      onChange={handlePhotoCapture}
                      className="form-control"
                    />
                  </div>
                )}

                {formData.photos.length > 0 && (
                  <div className="d-flex flex-wrap gap-2">
                    {formData.photos.map((photo, index) => (
                      <div key={index} className="position-relative">
                        <img
                          src={`data:image/jpeg;base64,${photo}`}
                          alt={`Incident photo ${index + 1}`}
                          style={{
                            width: '80px',
                            height: '80px',
                            objectFit: 'cover',
                          }}
                          className="rounded border"
                        />
                        <CButton
                          color="danger"
                          size="sm"
                          className="position-absolute top-0 end-0 rounded-circle"
                          style={{
                            width: '24px',
                            height: '24px',
                            fontSize: '12px',
                          }}
                          onClick={() => removePhoto(index)}
                        >
                          ×
                        </CButton>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {/* Submit Button */}
              <div className="d-grid gap-2">
                <CButton
                  color="primary"
                  type="submit"
                  disabled={isSubmitting}
                  size="lg"
                >
                  {isSubmitting ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Submitting Report...
                    </>
                  ) : (
                    <>
                      <Icon icon={faPaperPlane} className="me-2" />
                      Submit Report
                    </>
                  )}
                </CButton>

                <CButton
                  color="secondary"
                  variant="outline"
                  onClick={() => navigate('/incidents')}
                  disabled={isSubmitting}
                >
                  Cancel
                </CButton>
              </div>
            </CForm>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default QuickReport;
