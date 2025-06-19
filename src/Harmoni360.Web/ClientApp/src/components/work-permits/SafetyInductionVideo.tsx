import React, { useState, useRef, useEffect } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CAlert,
  CProgress,
  CButton,
  CSpinner,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faVideo,
  faCheckCircle,
  faExclamationTriangle,
  faPlay,
  faPause,
  faRedo,
  faLock,
} from '@fortawesome/free-solid-svg-icons';
import { useAppSelector } from '../../store/hooks';
import { selectAuth } from '../../features/auth/authSlice';
import { WorkPermitSafetyVideo } from '../../types/workPermitSettings';
import { formatDuration } from '../../utils/formatters';

interface SafetyInductionVideoProps {
  videos: WorkPermitSafetyVideo[];
  requiredCompletionPercentage: number;
  onComplete: () => void;
  onProgressUpdate?: (videoId: number, progress: number) => void;
}

interface VideoProgress {
  videoId: number;
  watchedPercentage: number;
  completed: boolean;
  lastPosition: number;
}

export const SafetyInductionVideo: React.FC<SafetyInductionVideoProps> = ({
  videos,
  requiredCompletionPercentage,
  onComplete,
  onProgressUpdate,
}) => {
  const auth = useAppSelector(selectAuth);
  const videoRef = useRef<HTMLVideoElement>(null);
  const [currentVideoIndex, setCurrentVideoIndex] = useState(0);
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [progress, setProgress] = useState<Record<number, VideoProgress>>({});
  const [allVideosCompleted, setAllVideosCompleted] = useState(false);
  const [showControls, setShowControls] = useState(false);

  const currentVideo = videos[currentVideoIndex];
  const currentProgress = progress[currentVideo?.id] || {
    videoId: currentVideo?.id,
    watchedPercentage: 0,
    completed: false,
    lastPosition: 0,
  };

  // Generate authenticated video URL
  const getVideoStreamUrl = (videoId: number): string => {
    const baseUrl = `/api/work-permits/settings/videos/${videoId}/stream`;
    if (auth.token) {
      return `${baseUrl}?token=${encodeURIComponent(auth.token)}`;
    }
    return baseUrl;
  };

  useEffect(() => {
    // Initialize progress for all videos
    const initialProgress: Record<number, VideoProgress> = {};
    videos.forEach(video => {
      initialProgress[video.id] = {
        videoId: video.id,
        watchedPercentage: 0,
        completed: false,
        lastPosition: 0,
      };
    });
    setProgress(initialProgress);
  }, [videos]);

  useEffect(() => {
    // Check if all videos are completed
    const allCompleted = videos.every(video => {
      const videoProgress = progress[video.id];
      return videoProgress && videoProgress.watchedPercentage >= requiredCompletionPercentage;
    });
    
    if (allCompleted && videos.length > 0) {
      setAllVideosCompleted(true);
      onComplete();
    }
  }, [progress, videos, requiredCompletionPercentage, onComplete]);

  const handleTimeUpdate = () => {
    if (!videoRef.current || !currentVideo) return;

    const video = videoRef.current;
    const watchedPercentage = (video.currentTime / video.duration) * 100;
    
    setCurrentTime(video.currentTime);
    
    // Update progress for current video
    const updatedProgress = {
      ...currentProgress,
      watchedPercentage: Math.max(currentProgress.watchedPercentage, watchedPercentage),
      lastPosition: video.currentTime,
      completed: watchedPercentage >= requiredCompletionPercentage,
    };

    setProgress(prev => ({
      ...prev,
      [currentVideo.id]: updatedProgress,
    }));

    if (onProgressUpdate) {
      onProgressUpdate(currentVideo.id, updatedProgress.watchedPercentage);
    }
  };

  const handleVideoEnd = () => {
    setIsPlaying(false);
    
    // Mark current video as completed
    const updatedProgress = {
      ...currentProgress,
      watchedPercentage: 100,
      completed: true,
    };

    setProgress(prev => ({
      ...prev,
      [currentVideo.id]: updatedProgress,
    }));

    // Auto-advance to next video if available
    if (currentVideoIndex < videos.length - 1) {
      setTimeout(() => {
        setCurrentVideoIndex(currentVideoIndex + 1);
      }, 1000);
    }
  };

  const handlePlayPause = async () => {
    if (!videoRef.current) return;

    try {
      if (isPlaying) {
        videoRef.current.pause();
        // onPause event will handle setIsPlaying(false)
      } else {
        await videoRef.current.play();
        // onPlay event will handle setIsPlaying(true)
      }
    } catch (error) {
      console.error('Video play/pause error:', error);
      // Reset playing state on error
      setIsPlaying(false);
    }
  };

  const handleVideoSelect = (index: number) => {
    // Only allow selecting completed videos or the next video in sequence
    const canSelect = index === 0 || 
      (index > 0 && progress[videos[index - 1]?.id]?.completed) ||
      progress[videos[index]?.id]?.completed;
    
    if (canSelect) {
      // Pause current video before switching
      if (videoRef.current && !videoRef.current.paused) {
        videoRef.current.pause();
      }
      setCurrentVideoIndex(index);
      setIsPlaying(false);
    }
  };

  const handleRestart = async () => {
    if (!videoRef.current) return;
    
    try {
      videoRef.current.currentTime = 0;
      await videoRef.current.play();
      // onPlay event will handle setIsPlaying(true)
    } catch (error) {
      console.error('Video restart error:', error);
      setIsPlaying(false);
    }
  };

  const handleLoadedMetadata = () => {
    if (!videoRef.current) return;
    setDuration(videoRef.current.duration);
    
    // Resume from last position if available
    if (currentProgress.lastPosition > 0) {
      videoRef.current.currentTime = currentProgress.lastPosition;
    }
  };

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  if (videos.length === 0) {
    return (
      <CAlert color="info">
        No safety induction videos are configured for this work permit type.
      </CAlert>
    );
  }

  return (
    <CCard className="safety-induction-video">
      <CCardHeader className="p-2 p-md-3">
        <h6 className="mb-0 fs-6 fs-md-5">
          <FontAwesomeIcon icon={faVideo} className="me-2" />
          <span className="d-none d-sm-inline">Safety Induction Video</span>
          <span className="d-sm-none">Safety Video</span>
        </h6>
      </CCardHeader>
      <CCardBody className="p-2 p-md-3">
        {!allVideosCompleted && (
          <CAlert color="warning" className="small">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
            You must watch all safety videos ({requiredCompletionPercentage}% completion) before submitting this work permit.
          </CAlert>
        )}

        {allVideosCompleted && (
          <CAlert color="success" className="small">
            <FontAwesomeIcon icon={faCheckCircle} className="me-2" />
            All safety videos completed! You may now submit your work permit.
          </CAlert>
        )}

        <div className="video-playlist mb-3">
          <h6 className="small">Videos ({videos.length})</h6>
          {videos.map((video, index) => {
            const videoProgress = progress[video.id];
            const isLocked = index > 0 && !progress[videos[index - 1]?.id]?.completed;
            const isActive = index === currentVideoIndex;
            
            return (
              <div
                key={video.id}
                className={`video-item p-2 mb-2 rounded ${isActive ? 'bg-primary text-white' : 'bg-light'} ${!isLocked ? 'cursor-pointer' : 'opacity-50'}`}
                onClick={() => !isLocked && handleVideoSelect(index)}
                style={{ touchAction: 'manipulation' }}
              >
                <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start align-sm-center">
                  <div className="d-flex align-items-center mb-1 mb-sm-0">
                    <span className="me-2">
                      {videoProgress?.completed ? (
                        <FontAwesomeIcon icon={faCheckCircle} className="text-success" />
                      ) : isLocked ? (
                        <FontAwesomeIcon icon={faLock} />
                      ) : (
                        <FontAwesomeIcon icon={faPlay} />
                      )}
                    </span>
                    <span className="small fw-bold text-truncate">{video.fileName}</span>
                  </div>
                  <div className="small text-nowrap">
                    {video.durationFormatted} • {Math.round(videoProgress?.watchedPercentage || 0)}%
                  </div>
                </div>
                {videoProgress && videoProgress.watchedPercentage > 0 && (
                  <CProgress
                    value={videoProgress.watchedPercentage}
                    color={videoProgress.completed ? 'success' : 'primary'}
                    height={3}
                    className="mt-1"
                  />
                )}
              </div>
            );
          })}
        </div>

        {currentVideo && (
          <>
            <div className="video-player-container bg-black position-relative" style={{ paddingBottom: '56.25%' }}>
              <video
                ref={videoRef}
                src={getVideoStreamUrl(currentVideo.id)}
                className="position-absolute top-0 start-0 w-100 h-100"
                onTimeUpdate={handleTimeUpdate}
                onEnded={handleVideoEnd}
                onPlay={() => setIsPlaying(true)}
                onPause={() => setIsPlaying(false)}
                onLoadedMetadata={handleLoadedMetadata}
                onError={(e) => {
                  console.error('Video failed to load:', e);
                  console.error('Video ID:', currentVideo.id);
                  console.error('Stream URL:', getVideoStreamUrl(currentVideo.id));
                }}
                controlsList="nodownload"
                disablePictureInPicture
                onContextMenu={(e) => e.preventDefault()}
                playsInline
                preload="metadata"
              />
              
              {/* Custom controls overlay */}
              <div 
                className="position-absolute bottom-0 start-0 w-100 p-2 p-md-3 bg-gradient"
                style={{ background: 'linear-gradient(transparent, rgba(0,0,0,0.7))' }}
              >
                <CProgress
                  value={(currentTime / duration) * 100}
                  height={4}
                  className="mb-2"
                  color="danger"
                />
                
                <div className="d-flex justify-content-between align-items-center text-white">
                  <div className="d-flex gap-1 gap-md-2">
                    <CButton
                      color="light"
                      variant="ghost"
                      size="sm"
                      onClick={handlePlayPause}
                      style={{ minHeight: '44px', minWidth: '44px', touchAction: 'manipulation' }}
                    >
                      <FontAwesomeIcon icon={isPlaying ? faPause : faPlay} />
                    </CButton>
                    {currentProgress.completed && (
                      <CButton
                        color="light"
                        variant="ghost"
                        size="sm"
                        onClick={handleRestart}
                        style={{ minHeight: '44px', minWidth: '44px', touchAction: 'manipulation' }}
                      >
                        <FontAwesomeIcon icon={faRedo} />
                      </CButton>
                    )}
                  </div>
                  
                  <div className="small text-nowrap">
                    {formatTime(currentTime)} / {formatTime(duration)}
                  </div>
                </div>
              </div>
            </div>

            <div className="mt-3">
              <h6 className="small fw-bold">{currentVideo.title || currentVideo.fileName}</h6>
              {currentVideo.description && (
                <p className="text-muted small">{currentVideo.description}</p>
              )}
            </div>
          </>
        )}
      </CCardBody>
    </CCard>
  );
};