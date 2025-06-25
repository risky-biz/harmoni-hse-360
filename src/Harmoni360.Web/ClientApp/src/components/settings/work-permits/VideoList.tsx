import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CRow,
  CCol,
  CButton,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CAlert,
  CBadge,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faPlay,
  faTrash,
  faVideo,
  faCalendar,
  faClock,
  faUser,
  faFileVideo,
} from '@fortawesome/free-solid-svg-icons';
import { useAppSelector } from '../../../store/hooks';
import { selectAuth } from '../../../features/auth/authSlice';
import {
  WorkPermitSettingsResponse,
  WorkPermitSafetyVideo,
} from '../../../types/workPermitSettings';
import { useDeleteSafetyVideoMutation } from '../../../services/workPermitSettingsApi';
import { formatFileSize, formatDuration, formatDate } from '../../../utils/formatters';

interface VideoListProps {
  settingsId: string;
  settings: WorkPermitSettingsResponse;
}

export const VideoList: React.FC<VideoListProps> = ({ settingsId, settings }) => {
  const auth = useAppSelector(selectAuth);
  const [deleteVideo] = useDeleteSafetyVideoMutation();
  const [selectedVideo, setSelectedVideo] = useState<WorkPermitSafetyVideo | null>(null);
  const [showVideoModal, setShowVideoModal] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [videoToDelete, setVideoToDelete] = useState<WorkPermitSafetyVideo | null>(null);
  const [videoLoadError, setVideoLoadError] = useState<string | null>(null);
  const [isVideoLoading, setIsVideoLoading] = useState(false);

  // Generate authenticated video URL
  const getVideoStreamUrl = (videoId: number): string => {
    const baseUrl = `/api/work-permits/settings/videos/${videoId}/stream`;
    if (auth.token) {
      return `${baseUrl}?token=${encodeURIComponent(auth.token)}`;
    }
    return baseUrl;
  };

  const handlePlayVideo = (video: WorkPermitSafetyVideo) => {
    setSelectedVideo(video);
    setShowVideoModal(true);
    setVideoLoadError(null);
    setIsVideoLoading(true);
  };

  const handleDeleteClick = (video: WorkPermitSafetyVideo) => {
    setVideoToDelete(video);
    setShowDeleteConfirm(true);
  };

  const handleConfirmDelete = async () => {
    if (!videoToDelete) return;

    try {
      await deleteVideo({
        settingsId,
        videoId: videoToDelete.id.toString(),
      }).unwrap();
      setShowDeleteConfirm(false);
      setVideoToDelete(null);
    } catch (error) {
      console.error('Failed to delete video:', error);
    }
  };

  const videos = settings.safetyVideos || [];

  if (videos.length === 0) {
    return (
      <CCard>
        <CCardBody className="text-center py-5">
          <FontAwesomeIcon icon={faFileVideo} size="3x" className="text-muted mb-3" />
          <h5>No Videos Uploaded</h5>
          <p className="text-muted">
            Upload safety induction videos that users must watch before submitting work permits.
          </p>
        </CCardBody>
      </CCard>
    );
  }

  return (
    <>
      <div className="video-list">
        <h5 className="mb-3">Safety Induction Videos ({videos.length})</h5>
        
        {videos.map((video) => (
          <CCard key={video.id} className="mb-3">
            <CCardBody className="p-2 p-md-3">
              <CRow className="align-items-start">
                <CCol xs={12} md={6} className="mb-2 mb-md-0">
                  <div className="d-flex align-items-start">
                    <div className="video-icon me-2 me-md-3">
                      <FontAwesomeIcon icon={faVideo} size="lg" className="text-primary d-none d-md-inline" />
                      <FontAwesomeIcon icon={faVideo} className="text-primary d-md-none" />
                    </div>
                    <div className="flex-grow-1">
                      <h6 className="mb-1 fs-6">{video.fileName || 'Untitled Video'}</h6>
                      {video.description && (
                        <p 
                          className="text-muted small mb-2" 
                          style={{ 
                            display: '-webkit-box',
                            WebkitLineClamp: 2,
                            WebkitBoxOrient: 'vertical',
                            overflow: 'hidden',
                            wordBreak: 'break-word'
                          }}
                          title={video.description}
                        >
                          {video.description}
                        </p>
                      )}
                      <div className="video-metadata d-flex flex-wrap gap-1">
                        {video.durationFormatted && (
                          <CBadge color="info" className="small">
                            <FontAwesomeIcon icon={faClock} className="me-1" />
                            {video.durationFormatted}
                          </CBadge>
                        )}
                        {video.fileSize && (
                          <CBadge color="secondary" className="small">
                            {formatFileSize(video.fileSize)}
                          </CBadge>
                        )}
                        {video.isActive && (
                          <CBadge color="success" className="small">Active</CBadge>
                        )}
                      </div>
                    </div>
                  </div>
                </CCol>
                <CCol xs={12} md={4} className="mb-2 mb-md-0">
                  <div className="text-muted small">
                    <div className="mb-1">
                      <FontAwesomeIcon icon={faCalendar} className="me-1" />
                      Created: {formatDate(video.createdAt)}
                    </div>
                    <div>
                      <FontAwesomeIcon icon={faUser} className="me-1" />
                      By: {video.createdBy}
                    </div>
                  </div>
                </CCol>
                <CCol xs={12} md={2} className="d-flex justify-content-end gap-1">
                  <CButton
                    color="primary"
                    variant="ghost"
                    size="sm"
                    onClick={() => handlePlayVideo(video)}
                    title="Preview video"
                    className="flex-grow-1 flex-md-grow-0"
                    style={{ minHeight: '44px' }}
                  >
                    <FontAwesomeIcon icon={faPlay} className="me-1 d-md-none" />
                    <FontAwesomeIcon icon={faPlay} className="d-none d-md-inline" />
                    <span className="d-md-none">Preview</span>
                  </CButton>
                  <CButton
                    color="danger"
                    variant="ghost"
                    size="sm"
                    onClick={() => handleDeleteClick(video)}
                    title="Delete video"
                    className="flex-grow-1 flex-md-grow-0"
                    style={{ minHeight: '44px' }}
                  >
                    <FontAwesomeIcon icon={faTrash} className="me-1 d-md-none" />
                    <FontAwesomeIcon icon={faTrash} className="d-none d-md-inline" />
                    <span className="d-md-none">Delete</span>
                  </CButton>
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        ))}
      </div>

      {/* Video Preview Modal */}
      <CModal
        visible={showVideoModal}
        onClose={() => {
          setShowVideoModal(false);
          setSelectedVideo(null);
          setVideoLoadError(null);
          setIsVideoLoading(false);
        }}
        size="lg"
        className="video-preview-modal"
      >
        <CModalHeader className="pb-2">
          <CModalTitle className="fs-6">{selectedVideo?.fileName}</CModalTitle>
        </CModalHeader>
        <CModalBody className="p-2 p-md-3">
          {selectedVideo && (
            <div className="video-player-container">
              {videoLoadError ? (
                <CAlert color="danger">
                  <h6>Failed to Load Video</h6>
                  <p className="mb-2">{videoLoadError}</p>
                  <CButton 
                    size="sm" 
                    color="primary" 
                    onClick={() => {
                      setVideoLoadError(null);
                      setIsVideoLoading(true);
                      // Force reload by resetting the video source
                      const video = document.querySelector('video') as HTMLVideoElement;
                      if (video) {
                        video.load();
                      }
                    }}
                  >
                    Try Again
                  </CButton>
                </CAlert>
              ) : (
                <>
                  {isVideoLoading && (
                    <div className="text-center p-4">
                      <div className="spinner-border text-primary" role="status">
                        <span className="visually-hidden">Loading video...</span>
                      </div>
                      <p className="mt-2 text-muted">Loading video...</p>
                    </div>
                  )}
                  <video
                    src={getVideoStreamUrl(selectedVideo.id)}
                    controls
                    style={{ 
                      width: '100%', 
                      height: 'auto', 
                      borderRadius: '8px',
                      display: isVideoLoading ? 'none' : 'block'
                    }}
                    playsInline
                    preload="metadata"
                    onError={(e) => {
                      setIsVideoLoading(false);
                      setVideoLoadError(`Failed to load video: ${selectedVideo.fileName}. Please check if the file exists and you have permission to view it.`);
                      console.error('Video failed to load:', e);
                      console.error('Video ID:', selectedVideo.id);
                      console.error('Stream URL:', getVideoStreamUrl(selectedVideo.id));
                    }}
                    onLoadStart={() => {
                      setIsVideoLoading(true);
                    }}
                    onCanPlay={() => {
                      setIsVideoLoading(false);
                    }}
                    onLoadedData={() => {
                      setIsVideoLoading(false);
                    }}
                  >
                    <p className="text-muted">Your browser does not support the video tag.</p>
                  </video>
                </>
              )}
              {selectedVideo.description && !videoLoadError && (
                <div className="mt-3">
                  <h6>Description:</h6>
                  <p className="small text-muted">{selectedVideo.description}</p>
                </div>
              )}
            </div>
          )}
        </CModalBody>
        <CModalFooter className="pt-2">
          <CButton
            color="secondary"
            onClick={() => {
              setShowVideoModal(false);
              setSelectedVideo(null);
              setVideoLoadError(null);
              setIsVideoLoading(false);
            }}
            className="w-100 w-sm-auto"
          >
            Close
          </CButton>
        </CModalFooter>
      </CModal>

      {/* Delete Confirmation Modal */}
      <CModal
        visible={showDeleteConfirm}
        onClose={() => {
          setShowDeleteConfirm(false);
          setVideoToDelete(null);
        }}
      >
        <CModalHeader>
          <CModalTitle>Delete Video</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <p>Are you sure you want to delete this video?</p>
          <p className="mb-0">
            <strong>{videoToDelete?.fileName}</strong>
          </p>
        </CModalBody>
        <CModalFooter>
          <CButton
            color="secondary"
            onClick={() => {
              setShowDeleteConfirm(false);
              setVideoToDelete(null);
            }}
          >
            Cancel
          </CButton>
          <CButton color="danger" onClick={handleConfirmDelete}>
            Delete Video
          </CButton>
        </CModalFooter>
      </CModal>
    </>
  );
};