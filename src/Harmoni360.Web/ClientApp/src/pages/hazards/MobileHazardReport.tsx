import React, { useState, useRef, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CForm,
  CFormLabel,
  CFormInput,
  CFormTextarea,
  CFormSelect,
  CButton,
  CAlert,
  CSpinner,
  CProgress,
  CRow,
  CCol,
  CBadge,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CListGroup,
  CListGroupItem,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faCamera,
  faMapMarkerAlt,
  faCheck,
  faTimes,
  faExclamationTriangle,
  faSpinner,
  faLocationArrow,
  faImage,
  faTrashAlt,
  faUpload,
  faMobile,
} from '@fortawesome/free-solid-svg-icons';
import { useCreateHazardMutation } from '../../features/hazards/hazardApi';
import { useAuth } from '../../hooks/useAuth';
import type { CreateHazardRequest } from '../../types/hazard';

interface LocationData {
  latitude: number;
  longitude: number;
  accuracy: number;
  address?: string;
}

const MobileHazardReport: React.FC = () => {
  const { user } = useAuth();
  const [createHazard, { isLoading }] = useCreateHazardMutation();
  
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState<Partial<CreateHazardRequest>>({
    title: '',
    description: '',
    category: 'Physical',
    type: 'SafetyHazard',
    location: '',
    severity: 'Minor',
    reporterId: user?.id || 0,
    reporterDepartment: user?.department || '',
    attachments: [],
  });
  
  const [locationData, setLocationData] = useState<LocationData | null>(null);
  const [locationLoading, setLocationLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [previewImages, setPreviewImages] = useState<string[]>([]);
  const [showCamera, setShowCamera] = useState(false);
  
  const fileInputRef = useRef<HTMLInputElement>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [stream, setStream] = useState<MediaStream | null>(null);

  const totalSteps = 4;
  const progress = (step / totalSteps) * 100;

  // Get current location
  const getCurrentLocation = () => {
    setLocationLoading(true);
    setError(null);

    if (!navigator.geolocation) {
      setError('Geolocation is not supported by this device');
      setLocationLoading(false);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const locationData: LocationData = {
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          accuracy: position.coords.accuracy,
        };

        // Try to get address from coordinates
        try {
          const response = await fetch(
            `https://api.geocoding.com/v1/reverse?lat=${locationData.latitude}&lng=${locationData.longitude}&api_key=${import.meta.env.VITE_GEOCODING_API_KEY}`
          );
          
          if (response.ok) {
            const data = await response.json();
            locationData.address = data.display_name || `${locationData.latitude}, ${locationData.longitude}`;
          } else {
            locationData.address = `${locationData.latitude.toFixed(6)}, ${locationData.longitude.toFixed(6)}`;
          }
        } catch (error) {
          console.warn('Failed to get address:', error);
          locationData.address = `${locationData.latitude.toFixed(6)}, ${locationData.longitude.toFixed(6)}`;
        }

        setLocationData(locationData);
        setFormData(prev => ({
          ...prev,
          latitude: locationData.latitude,
          longitude: locationData.longitude,
          location: locationData.address || prev.location,
        }));
        setLocationLoading(false);
      },
      (error) => {
        setError(`Location error: ${error.message}`);
        setLocationLoading(false);
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 300000,
      }
    );
  };

  // Camera functions
  const startCamera = async () => {
    try {
      const mediaStream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }, // Use back camera
        audio: false,
      });
      
      setStream(mediaStream);
      setShowCamera(true);
      
      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream;
      }
    } catch (error) {
      setError('Unable to access camera. Please try uploading from gallery instead.');
      console.error('Camera error:', error);
    }
  };

  const stopCamera = () => {
    if (stream) {
      stream.getTracks().forEach(track => track.stop());
      setStream(null);
    }
    setShowCamera(false);
  };

  const capturePhoto = () => {
    if (videoRef.current && canvasRef.current) {
      const canvas = canvasRef.current;
      const video = videoRef.current;
      
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;
      
      const ctx = canvas.getContext('2d');
      if (ctx) {
        ctx.drawImage(video, 0, 0);
        
        canvas.toBlob((blob) => {
          if (blob) {
            const file = new File([blob], `hazard-photo-${Date.now()}.jpg`, {
              type: 'image/jpeg',
            });
            
            addImageFile(file);
          }
        }, 'image/jpeg', 0.8);
      }
      
      stopCamera();
    }
  };

  const addImageFile = (file: File) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      if (e.target?.result) {
        setPreviewImages(prev => [...prev, e.target!.result as string]);
      }
    };
    reader.readAsDataURL(file);

    setFormData(prev => ({
      ...prev,
      attachments: [...(prev.attachments || []), file],
    }));
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    files.forEach(addImageFile);
  };

  const removeImage = (index: number) => {
    setPreviewImages(prev => prev.filter((_, i) => i !== index));
    setFormData(prev => ({
      ...prev,
      attachments: prev.attachments?.filter((_, i) => i !== index) || [],
    }));
  };

  const handleSubmit = async () => {
    try {
      setError(null);
      
      const result = await createHazard({
        ...formData,
        reporterId: user?.id || 0,
        reporterDepartment: user?.department || '',
      } as CreateHazardRequest).unwrap();

      setSuccess(true);
      
      // Reset form after 3 seconds
      setTimeout(() => {
        setFormData({
          title: '',
          description: '',
          category: 'Physical',
          type: 'SafetyHazard',
          location: '',
          severity: 'Minor',
          reporterId: user?.id || 0,
          reporterDepartment: user?.department || '',
          attachments: [],
        });
        setLocationData(null);
        setPreviewImages([]);
        setStep(1);
        setSuccess(false);
      }, 3000);
    } catch (err: any) {
      setError(err.data?.message || 'Failed to submit hazard report');
    }
  };

  const nextStep = () => {
    if (step < totalSteps) {
      setStep(step + 1);
    }
  };

  const prevStep = () => {
    if (step > 1) {
      setStep(step - 1);
    }
  };

  useEffect(() => {
    // Auto-detect if user is on mobile and get location
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
      getCurrentLocation();
    }
  }, []);

  if (success) {
    return (
      <CCard className="m-3">
        <CCardBody className="text-center py-5">
          <div className="mb-4">
            <FontAwesomeIcon icon={faCheck} size="3x" className="text-success" />
          </div>
          <h4 className="text-success">Report Submitted Successfully!</h4>
          <p className="text-muted">Your hazard report has been submitted and relevant personnel have been notified.</p>
          <CBadge color="success" className="p-2">
            <FontAwesomeIcon icon={faMobile} className="me-2" />
            Mobile Report Submitted
          </CBadge>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <>
      <CCard className="m-3">
        <CCardHeader className="bg-primary text-white">
          <div className="d-flex justify-content-between align-items-center">
            <h5 className="mb-0">
              <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
              Report Hazard
            </h5>
            <CBadge color="light" textColor="dark">
              Step {step} of {totalSteps}
            </CBadge>
          </div>
          <CProgress value={progress} className="mt-2" height={4} />
        </CCardHeader>

        <CCardBody>
          {error && (
            <CAlert color="danger" dismissible onClose={() => setError(null)}>
              {error}
            </CAlert>
          )}

          <CForm>
            {/* Step 1: Basic Information */}
            {step === 1 && (
              <div>
                <h6 className="mb-3">Basic Information</h6>
                
                <div className="mb-3">
                  <CFormLabel htmlFor="title">Hazard Title *</CFormLabel>
                  <CFormInput
                    id="title"
                    value={formData.title}
                    onChange={(e) => setFormData(prev => ({ ...prev, title: e.target.value }))}
                    placeholder="Brief description of the hazard"
                    required
                  />
                </div>

                <div className="mb-3">
                  <CFormLabel htmlFor="category">Category *</CFormLabel>
                  <CFormSelect
                    id="category"
                    value={formData.category}
                    onChange={(e) => setFormData(prev => ({ ...prev, category: e.target.value }))}
                    required
                  >
                    <option value="Physical">Physical Hazard</option>
                    <option value="Chemical">Chemical Hazard</option>
                    <option value="Biological">Biological Hazard</option>
                    <option value="Ergonomic">Ergonomic Hazard</option>
                    <option value="Psychosocial">Psychosocial Hazard</option>
                    <option value="Environmental">Environmental Hazard</option>
                  </CFormSelect>
                </div>

                <div className="mb-3">
                  <CFormLabel htmlFor="severity">Severity Level *</CFormLabel>
                  <CFormSelect
                    id="severity"
                    value={formData.severity}
                    onChange={(e) => setFormData(prev => ({ ...prev, severity: e.target.value }))}
                    required
                  >
                    <option value="Negligible">Negligible</option>
                    <option value="Minor">Minor</option>
                    <option value="Moderate">Moderate</option>
                    <option value="Major">Major</option>
                    <option value="Catastrophic">Catastrophic</option>
                  </CFormSelect>
                </div>

                <div className="d-grid">
                  <CButton
                    color="primary"
                    onClick={nextStep}
                    disabled={!formData.title}
                  >
                    Next: Description
                  </CButton>
                </div>
              </div>
            )}

            {/* Step 2: Detailed Description */}
            {step === 2 && (
              <div>
                <h6 className="mb-3">Detailed Description</h6>
                
                <div className="mb-3">
                  <CFormLabel htmlFor="description">Description *</CFormLabel>
                  <CFormTextarea
                    id="description"
                    rows={5}
                    value={formData.description}
                    onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                    placeholder="Describe the hazard in detail. Include what happened, when, who was involved, and any immediate actions taken."
                    required
                  />
                  <small className="text-muted">
                    Be as specific as possible. This helps safety personnel understand and address the hazard quickly.
                  </small>
                </div>

                <div className="mb-3">
                  <CFormLabel htmlFor="type">Hazard Type</CFormLabel>
                  <CFormSelect
                    id="type"
                    value={formData.type}
                    onChange={(e) => setFormData(prev => ({ ...prev, type: e.target.value }))}
                  >
                    <option value="SafetyHazard">Safety Hazard</option>
                    <option value="HealthHazard">Health Hazard</option>
                    <option value="EnvironmentalHazard">Environmental Hazard</option>
                    <option value="SecurityHazard">Security Hazard</option>
                    <option value="OperationalHazard">Operational Hazard</option>
                  </CFormSelect>
                </div>

                <div className="d-grid gap-2">
                  <CButton color="secondary" onClick={prevStep}>
                    Back
                  </CButton>
                  <CButton
                    color="primary"
                    onClick={nextStep}
                    disabled={!formData.description}
                  >
                    Next: Location
                  </CButton>
                </div>
              </div>
            )}

            {/* Step 3: Location */}
            {step === 3 && (
              <div>
                <h6 className="mb-3">Location Information</h6>
                
                <div className="mb-3">
                  <CFormLabel htmlFor="location">Location Description *</CFormLabel>
                  <CFormInput
                    id="location"
                    value={formData.location}
                    onChange={(e) => setFormData(prev => ({ ...prev, location: e.target.value }))}
                    placeholder="Building, room, area, or specific location"
                    required
                  />
                </div>

                <div className="mb-3">
                  <CButton
                    color="outline-primary"
                    onClick={getCurrentLocation}
                    disabled={locationLoading}
                    className="w-100"
                  >
                    {locationLoading ? (
                      <>
                        <CSpinner size="sm" className="me-2" />
                        Getting Location...
                      </>
                    ) : (
                      <>
                        <FontAwesomeIcon icon={faLocationArrow} className="me-2" />
                        Get Current Location
                      </>
                    )}
                  </CButton>
                  
                  {locationData && (
                    <div className="mt-2 p-2 bg-light rounded">
                      <small>
                        <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1 text-success" />
                        Location captured (±{Math.round(locationData.accuracy)}m accuracy)
                      </small>
                      {locationData.address && (
                        <div className="small text-muted">{locationData.address}</div>
                      )}
                    </div>
                  )}
                </div>

                <div className="d-grid gap-2">
                  <CButton color="secondary" onClick={prevStep}>
                    Back
                  </CButton>
                  <CButton
                    color="primary"
                    onClick={nextStep}
                    disabled={!formData.location}
                  >
                    Next: Photos
                  </CButton>
                </div>
              </div>
            )}

            {/* Step 4: Photos and Submit */}
            {step === 4 && (
              <div>
                <h6 className="mb-3">Add Photos (Optional)</h6>
                
                <div className="mb-3">
                  <div className="d-grid gap-2">
                    <CButton
                      color="outline-primary"
                      onClick={startCamera}
                    >
                      <FontAwesomeIcon icon={faCamera} className="me-2" />
                      Take Photo
                    </CButton>
                    
                    <CButton
                      color="outline-secondary"
                      onClick={() => fileInputRef.current?.click()}
                    >
                      <FontAwesomeIcon icon={faImage} className="me-2" />
                      Choose from Gallery
                    </CButton>
                  </div>
                  
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*"
                    multiple
                    onChange={handleFileSelect}
                    style={{ display: 'none' }}
                  />
                </div>

                {/* Image Previews */}
                {previewImages.length > 0 && (
                  <div className="mb-3">
                    <h6>Attached Photos ({previewImages.length})</h6>
                    <CRow>
                      {previewImages.map((image, index) => (
                        <CCol xs={6} key={index} className="mb-2">
                          <div className="position-relative">
                            <img
                              src={image}
                              alt={`Preview ${index + 1}`}
                              className="img-fluid rounded"
                              style={{ aspectRatio: '1:1', objectFit: 'cover' }}
                            />
                            <CButton
                              color="danger"
                              size="sm"
                              className="position-absolute top-0 end-0 m-1"
                              onClick={() => removeImage(index)}
                            >
                              <FontAwesomeIcon icon={faTimes} />
                            </CButton>
                          </div>
                        </CCol>
                      ))}
                    </CRow>
                  </div>
                )}

                <div className="d-grid gap-2">
                  <CButton color="secondary" onClick={prevStep}>
                    Back
                  </CButton>
                  <CButton
                    color="success"
                    onClick={handleSubmit}
                    disabled={isLoading}
                  >
                    {isLoading ? (
                      <>
                        <CSpinner size="sm" className="me-2" />
                        Submitting...
                      </>
                    ) : (
                      <>
                        <FontAwesomeIcon icon={faUpload} className="me-2" />
                        Submit Report
                      </>
                    )}
                  </CButton>
                </div>
              </div>
            )}
          </CForm>
        </CCardBody>
      </CCard>

      {/* Camera Modal */}
      <CModal visible={showCamera} onClose={stopCamera} size="lg">
        <CModalHeader>
          <CModalTitle>Take Photo</CModalTitle>
        </CModalHeader>
        <CModalBody className="text-center">
          <video
            ref={videoRef}
            autoPlay
            playsInline
            className="w-100 rounded"
            style={{ maxHeight: '400px' }}
          />
          <canvas ref={canvasRef} style={{ display: 'none' }} />
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={stopCamera}>
            Cancel
          </CButton>
          <CButton color="primary" onClick={capturePhoto}>
            <FontAwesomeIcon icon={faCamera} className="me-2" />
            Capture
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};

export default MobileHazardReport;