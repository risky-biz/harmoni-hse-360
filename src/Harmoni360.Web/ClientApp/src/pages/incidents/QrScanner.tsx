import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CAlert,
  CSpinner,
  CBadge,
} from '@coreui/react';
import { Icon } from '../../components/common/Icon';
import {
  faQrcode,
  faCamera,
  faArrowLeft,
  faLocationArrow,
} from '@fortawesome/free-solid-svg-icons';

const QrScanner: React.FC = () => {
  const navigate = useNavigate();
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [isScanning, setIsScanning] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [stream, setStream] = useState<MediaStream | null>(null);
  const [detectedCode, setDetectedCode] = useState<string | null>(null);

  useEffect(() => {
    return () => {
      // Cleanup camera stream when component unmounts
      if (stream) {
        stream.getTracks().forEach((track) => track.stop());
      }
    };
  }, [stream]);

  const startCamera = async () => {
    try {
      setError(null);
      setIsScanning(true);

      const mediaStream = await navigator.mediaDevices.getUserMedia({
        video: {
          facingMode: 'environment', // Use back camera
          width: { ideal: 1280 },
          height: { ideal: 720 },
        },
      });

      setStream(mediaStream);

      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream;
        videoRef.current.play();

        // Start scanning for QR codes
        videoRef.current.onloadedmetadata = () => {
          startQrDetection();
        };
      }
    } catch (err) {
      setError(
        'Failed to access camera. Please ensure you have granted camera permissions.'
      );
      setIsScanning(false);
      console.error('Camera error:', err);
    }
  };

  const stopCamera = () => {
    if (stream) {
      stream.getTracks().forEach((track) => track.stop());
      setStream(null);
    }
    setIsScanning(false);
    setDetectedCode(null);
  };

  const startQrDetection = () => {
    const detectQr = () => {
      if (!videoRef.current || !canvasRef.current || !isScanning) return;

      const video = videoRef.current;
      const canvas = canvasRef.current;
      const context = canvas.getContext('2d');

      if (!context || video.readyState !== video.HAVE_ENOUGH_DATA) {
        requestAnimationFrame(detectQr);
        return;
      }

      // Set canvas size to match video
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;

      // Draw current video frame to canvas
      context.drawImage(video, 0, 0, canvas.width, canvas.height);

      // Get image data for QR code detection
      const imageData = context.getImageData(0, 0, canvas.width, canvas.height);

      // In a real implementation, you would use a QR code detection library here
      // For this demo, we'll simulate QR code detection
      simulateQrDetection(imageData);

      if (isScanning) {
        requestAnimationFrame(detectQr);
      }
    };

    detectQr();
  };

  const simulateQrDetection = (_imageData: ImageData) => {
    // This is a placeholder for actual QR code detection
    // In production, you would use a library like qr-scanner or jsQR
    // _imageData would be processed here to detect QR codes

    // Simulate finding a QR code after some time
    const mockDetection = Math.random() > 0.98; // 2% chance per frame

    if (mockDetection && !detectedCode) {
      const mockQrData =
        'https://harmoni360.app/report/qr/loc_building_a_floor_2';
      handleQrDetected(mockQrData);
    }
  };

  const handleQrDetected = (qrData: string) => {
    setDetectedCode(qrData);

    try {
      // Parse QR code URL to extract location information
      const url = new URL(qrData);
      const pathParts = url.pathname.split('/');
      const qrCodeId = pathParts[pathParts.length - 1];

      if (qrCodeId && qrCodeId.startsWith('loc_')) {
        setSuccess(`QR Code detected! Redirecting to incident report...`);

        // Stop camera
        stopCamera();

        // Navigate to quick report with QR code data
        setTimeout(() => {
          navigate(
            `/incidents/quick-report?qr=${qrCodeId}&location=${extractLocationFromQrId(qrCodeId)}`
          );
        }, 1500);
      } else {
        setError(
          'Invalid QR code. Please scan a valid incident reporting QR code.'
        );
        setDetectedCode(null);
      }
    } catch (err) {
      setError('Unable to read QR code data. Please try again.');
      setDetectedCode(null);
    }
  };

  const extractLocationFromQrId = (qrId: string): string => {
    // Convert QR ID like "loc_building_a_floor_2" to readable location
    if (!qrId || typeof qrId !== 'string') {
      return 'Unknown Location';
    }
    const parts = qrId.replace('loc_', '').split('_');
    return parts
      .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
      .join(' ');
  };

  const handleManualEntry = () => {
    navigate('/incidents/quick-report');
  };

  return (
    <CRow className="justify-content-center">
      <CCol xs={12} md={8} lg={6}>
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4 className="mb-0">QR Code Scanner</h4>
              <small className="text-muted">
                Scan location QR codes for quick reporting
              </small>
            </div>
            <CButton
              color="light"
              variant="ghost"
              onClick={() => navigate('/incidents')}
            >
              <Icon icon={faArrowLeft} />
            </CButton>
          </CCardHeader>

          <CCardBody>
            {error && (
              <CAlert color="danger" dismissible onClose={() => setError(null)}>
                <strong>Error!</strong> {error}
              </CAlert>
            )}

            {success && (
              <CAlert color="success">
                <strong>Success!</strong> {success}
              </CAlert>
            )}

            {/* Camera View */}
            <div className="text-center mb-4">
              {!isScanning ? (
                <div className="py-5">
                  <Icon icon={faQrcode} size="4x" className="text-muted mb-3" />
                  <h5>Ready to Scan</h5>
                  <p className="text-muted">
                    Position your camera over a QR code to automatically start
                    incident reporting for that location.
                  </p>
                </div>
              ) : (
                <div className="position-relative">
                  <video
                    ref={videoRef}
                    className="w-100 rounded"
                    style={{ maxHeight: '400px', objectFit: 'cover' }}
                    muted
                    playsInline
                  />
                  <canvas ref={canvasRef} style={{ display: 'none' }} />

                  {/* Scanning overlay */}
                  <div className="position-absolute top-50 start-50 translate-middle">
                    <div
                      className="border border-primary border-3 rounded"
                      style={{
                        width: '200px',
                        height: '200px',
                        background: 'rgba(255,255,255,0.1)',
                        animation: 'pulse 2s infinite',
                      }}
                    >
                      <div className="d-flex align-items-center justify-content-center h-100">
                        <Icon
                          icon={faQrcode}
                          size="2x"
                          className="text-primary"
                        />
                      </div>
                    </div>
                  </div>

                  {detectedCode && (
                    <div className="position-absolute bottom-0 start-0 end-0 p-3">
                      <CBadge color="success" className="px-3 py-2">
                        <Icon icon={faLocationArrow} className="me-2" />
                        QR Code Detected!
                      </CBadge>
                    </div>
                  )}
                </div>
              )}
            </div>

            {/* Controls */}
            <div className="d-grid gap-2">
              {!isScanning ? (
                <>
                  <CButton color="primary" size="lg" onClick={startCamera}>
                    <Icon icon={faCamera} className="me-2" />
                    Start Camera
                  </CButton>

                  <CButton
                    color="secondary"
                    variant="outline"
                    onClick={handleManualEntry}
                  >
                    Report Without QR Code
                  </CButton>
                </>
              ) : (
                <CButton
                  color="danger"
                  variant="outline"
                  onClick={stopCamera}
                  disabled={!!detectedCode}
                >
                  {detectedCode ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      Processing...
                    </>
                  ) : (
                    'Stop Camera'
                  )}
                </CButton>
              )}
            </div>

            {/* Instructions */}
            <div className="mt-4 text-center">
              <small className="text-muted">
                <strong>How to use:</strong>
                <br />
                1. Click "Start Camera" to activate your device's camera
                <br />
                2. Point your camera at a QR code posted at incident locations
                <br />
                3. The app will automatically detect the code and pre-fill
                location details
                <br />
                4. Complete your incident report with location information
                already filled in
              </small>
            </div>
          </CCardBody>
        </CCard>
      </CCol>

      <style>{`
        @keyframes pulse {
          0% { opacity: 0.6; }
          50% { opacity: 1; }
          100% { opacity: 0.6; }
        }
      `}</style>
    </CRow>
  );
};

export default QrScanner;
