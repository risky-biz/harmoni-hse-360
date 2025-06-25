import React, { useState, useRef } from 'react';
import {
  CForm,
  CFormInput,
  CFormTextarea,
  CButton,
  CProgress,
  CAlert,
  CRow,
  CCol,
  CFormLabel,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faUpload,
  faTimes,
  faVideo,
  faCheckCircle,
  faExclamationTriangle,
} from '@fortawesome/free-solid-svg-icons';
import { useVideoUpload } from '../../../services/workPermitSettingsApi';
import { formatFileSize, formatDuration } from '../../../utils/formatters';

interface VideoUploadComponentProps {
  settingsId: string;
  onUploadComplete: () => void;
  onCancel: () => void;
}

const ALLOWED_VIDEO_TYPES = ['video/mp4', 'video/webm', 'video/ogg'];
const MAX_FILE_SIZE = 500 * 1024 * 1024; // 500MB

export const VideoUploadComponent: React.FC<VideoUploadComponentProps> = ({
  settingsId,
  onUploadComplete,
  onCancel,
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const videoPreviewRef = useRef<HTMLVideoElement>(null);
  
  const { upload, isLoading, progress, error } = useVideoUpload(onUploadComplete);
  
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);
  const [videoPreviewUrl, setVideoPreviewUrl] = useState<string | null>(null);
  const [videoDuration, setVideoDuration] = useState<number>(0);

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setValidationError(null);

    // Validate file type
    if (!ALLOWED_VIDEO_TYPES.includes(file.type)) {
      setValidationError('Please select a valid video file (MP4, WebM, or OGG)');
      return;
    }

    // Validate file size
    if (file.size > MAX_FILE_SIZE) {
      setValidationError(`File size must be less than ${formatFileSize(MAX_FILE_SIZE)}`);
      return;
    }

    setSelectedFile(file);
    
    // Create preview URL
    const url = URL.createObjectURL(file);
    setVideoPreviewUrl(url);

    // Extract video duration when metadata loads
    const video = document.createElement('video');
    video.src = url;
    video.onloadedmetadata = () => {
      setVideoDuration(Math.floor(video.duration));
    };
  };

  const handleUpload = async () => {
    if (!selectedFile || !title.trim()) {
      setValidationError('Please select a file and enter a title');
      return;
    }

    try {
      await upload(settingsId, selectedFile, description);
      // onUploadComplete will be called automatically by useVideoUpload on success
    } catch (error) {
      console.error('Upload failed:', error);
    }
  };

  const handleCancel = () => {
    if (videoPreviewUrl) {
      URL.revokeObjectURL(videoPreviewUrl);
    }
    onCancel();
  };

  return (
    <div className="video-upload-component">
      <CForm>
        <div className="mb-3">
          <CFormLabel>Video File *</CFormLabel>
          <div className="d-flex flex-column flex-md-row align-items-start align-md-center gap-2">
            <CButton
              color="secondary"
              onClick={() => fileInputRef.current?.click()}
              disabled={isLoading}
              className="w-100 w-md-auto"
            >
              <FontAwesomeIcon icon={faVideo} className="me-2" />
              Select Video
            </CButton>
            {selectedFile && (
              <div className="text-muted small text-break">
                <strong>{selectedFile.name}</strong><br className="d-md-none" />
                <span className="ms-md-2">({formatFileSize(selectedFile.size)})</span>
              </div>
            )}
          </div>
          <CFormInput
            ref={fileInputRef}
            type="file"
            accept="video/mp4,video/webm,video/ogg"
            onChange={handleFileSelect}
            style={{ display: 'none' }}
          />
          <div className="form-text small">
            Accepted formats: MP4, WebM, OGG. Maximum size: {formatFileSize(MAX_FILE_SIZE)}
          </div>
        </div>

        {videoPreviewUrl && (
          <div className="mb-3">
            <CFormLabel>Preview</CFormLabel>
            <div className="video-preview-container" style={{ maxWidth: '100%' }}>
              <video
                ref={videoPreviewRef}
                src={videoPreviewUrl}
                controls
                style={{ width: '100%', height: 'auto', maxWidth: '400px' }}
                playsInline
                preload="metadata"
              />
              {videoDuration > 0 && (
                <div className="text-muted mt-1 small">
                  Duration: {formatDuration(videoDuration)}
                </div>
              )}
            </div>
          </div>
        )}

        <CRow>
          <CCol md={12}>
            <div className="mb-3">
              <CFormLabel htmlFor="videoTitle">Title *</CFormLabel>
              <CFormInput
                id="videoTitle"
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Enter video title"
                disabled={isLoading}
              />
            </div>
          </CCol>
        </CRow>

        <div className="mb-3">
          <CFormLabel htmlFor="videoDescription">Description</CFormLabel>
          <CFormTextarea
            id="videoDescription"
            rows={3}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Enter video description (optional)"
            disabled={isLoading}
          />
        </div>

        {validationError && (
          <CAlert color="danger" className="mb-3">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
            {validationError}
          </CAlert>
        )}

        {error && (
          <CAlert color="danger" className="mb-3">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
            Upload failed. Please try again.
          </CAlert>
        )}

        {isLoading && progress && (
          <div className="mb-3">
            <div className="d-flex justify-content-between mb-1">
              <span>Uploading...</span>
              <span>{progress.percentage}%</span>
            </div>
            <CProgress value={progress.percentage} color="primary" />
            <div className="text-muted small mt-1">
              {formatFileSize(progress.loaded)} of {formatFileSize(progress.total)}
            </div>
          </div>
        )}

        <div className="d-flex flex-column flex-sm-row justify-content-end gap-2">
          <CButton
            color="secondary"
            onClick={handleCancel}
            disabled={isLoading}
            className="order-2 order-sm-1 w-100 w-sm-auto"
          >
            <FontAwesomeIcon icon={faTimes} className="me-2" />
            Cancel
          </CButton>
          <CButton
            color="primary"
            onClick={handleUpload}
            disabled={!selectedFile || !title.trim() || isLoading}
            className="order-1 order-sm-2 w-100 w-sm-auto"
          >
            {isLoading ? (
              <>Uploading...</>
            ) : (
              <>
                <FontAwesomeIcon icon={faUpload} className="me-2" />
                <span className="d-none d-sm-inline">Upload Video</span>
                <span className="d-sm-none">Upload</span>
              </>
            )}
          </CButton>
        </div>
      </CForm>
    </div>
  );
};