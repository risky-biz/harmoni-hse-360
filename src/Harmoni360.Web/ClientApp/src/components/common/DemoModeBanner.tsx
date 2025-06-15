import React, { useEffect, useRef } from 'react';
import { CAlert, CRow, CCol, CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faInfoCircle, faFlask, faCog, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';
import { useGetApplicationModeInfoQuery } from '../../api/applicationModeApi';

interface DemoModeBannerProps {
  sidebarShow?: boolean;
}

const DemoModeBanner: React.FC<DemoModeBannerProps> = ({ sidebarShow = true }) => {
  const { data: modeInfo, isLoading, error } = useGetApplicationModeInfoQuery();
  const bannerRef = useRef<HTMLDivElement>(null);

  // Set CSS variable for banner height - must be before early returns
  useEffect(() => {
    const updateBannerHeight = () => {
      if (bannerRef.current && modeInfo && (modeInfo.isDemoMode || modeInfo.bannerMessage)) {
        const height = bannerRef.current.offsetHeight;
        document.documentElement.style.setProperty('--demo-banner-height', `${height}px`);
      } else {
        document.documentElement.style.setProperty('--demo-banner-height', '0px');
      }
    };

    // Update height after render
    updateBannerHeight();
    
    // Update height on window resize
    window.addEventListener('resize', updateBannerHeight);
    
    return () => {
      document.documentElement.style.setProperty('--demo-banner-height', '0px');
      window.removeEventListener('resize', updateBannerHeight);
    };
  }, [modeInfo?.isDemoMode, modeInfo?.bannerMessage]);

  // Don't render if loading
  if (isLoading) {
    return null;
  }

  // If API error, log helpful message but don't show banner
  if (error || !modeInfo) {
    if (import.meta.env.DEV) {
      console.warn('DemoModeBanner: Application mode API unavailable. Please ensure the backend is running.');
    }
    return null;
  }

  // Don't render if not demo mode and no banner message
  if (!modeInfo.isDemoMode && !modeInfo.bannerMessage) {
    return null;
  }

  const getIcon = () => {
    switch (modeInfo.environment) {
      case 'Demo':
        return faFlask;
      case 'Development':
        return faCog;
      case 'Staging':
        return faExclamationTriangle;
      default:
        return faInfoCircle;
    }
  };

  const getBannerContent = () => {
    if (modeInfo.isDemoMode) {
      return (
        <CRow className="align-items-center">
          <CCol md="auto">
            <FontAwesomeIcon icon={getIcon()} className="me-2" />
            <strong>{modeInfo.bannerMessage}</strong>
          </CCol>
          <CCol md="auto">
            <CBadge color="dark" className="ms-2">
              {modeInfo.environmentDisplayName}
            </CBadge>
          </CCol>
          <CCol className="text-end">
            <small>
              {modeInfo.limitations.canCreateUsers ? '' : 'User creation disabled • '}
              {modeInfo.limitations.canDeleteData ? '' : 'Data deletion disabled • '}
              {modeInfo.limitations.canSendEmails ? '' : 'Email sending disabled • '}
              Sample data environment
            </small>
          </CCol>
        </CRow>
      );
    }

    return (
      <CRow className="align-items-center">
        <CCol>
          <FontAwesomeIcon icon={getIcon()} className="me-2" />
          <strong>{modeInfo.bannerMessage}</strong>
          <CBadge color="secondary" className="ms-2">
            {modeInfo.environmentDisplayName}
          </CBadge>
        </CCol>
      </CRow>
    );
  };

  return (
    <CAlert
      ref={bannerRef}
      color={modeInfo.bannerColor}
      className="mb-0 rounded-0 border-0 border-bottom demo-banner"
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 1040,
        fontSize: '0.875rem',
        padding: '0.5rem 0',
        transition: 'all 0.15s ease-in-out'
      }}
    >
      <div 
        className="demo-banner-content"
        style={{
          marginLeft: sidebarShow ? 'var(--cui-sidebar-width, 256px)' : '0',
          transition: 'margin-left 0.15s ease-in-out',
          paddingLeft: '1.5rem',
          paddingRight: '1.5rem'
        }}
      >
        {getBannerContent()}
      </div>
    </CAlert>
  );
};

export default DemoModeBanner;