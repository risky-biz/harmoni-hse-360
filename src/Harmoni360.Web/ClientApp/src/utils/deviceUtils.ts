/**
 * Device detection utilities for responsive design and user experience optimization
 */

export const isMobileDevice = (): boolean => {
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
    navigator.userAgent
  );
};

export const isTouchDevice = (): boolean => {
  return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
};

export const isSmallScreen = (): boolean => {
  return window.innerWidth <= 768;
};

export const shouldUseMobileInterface = (): boolean => {
  return isMobileDevice() || (isTouchDevice() && isSmallScreen());
};

export const getDeviceInfo = () => {
  return {
    isMobile: isMobileDevice(),
    isTouch: isTouchDevice(),
    isSmallScreen: isSmallScreen(),
    shouldUseMobileInterface: shouldUseMobileInterface(),
    screenWidth: window.innerWidth,
    screenHeight: window.innerHeight,
    userAgent: navigator.userAgent,
  };
};