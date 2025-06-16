import { useState, useEffect } from 'react';

// Breakpoint constants
export const BREAKPOINTS = {
  mobile: 576,
  tablet: 768,
  desktop: 992,
  large: 1200,
  xl: 1400,
} as const;

// Device type detection
export type DeviceType = 'mobile' | 'tablet' | 'desktop';
export type ScreenSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'xxl';

interface ResponsiveState {
  width: number;
  height: number;
  isMobile: boolean;
  isTablet: boolean;
  isDesktop: boolean;
  deviceType: DeviceType;
  screenSize: ScreenSize;
  orientation: 'portrait' | 'landscape';
  isTouch: boolean;
  reducedMotion: boolean;
  highContrast: boolean;
}

/**
 * Custom hook for responsive design and device detection
 * Provides real-time updates when screen size changes
 */
export const useResponsive = (): ResponsiveState => {
  const [state, setState] = useState<ResponsiveState>(() => {
    // Initial state (SSR-safe)
    if (typeof window === 'undefined') {
      return {
        width: 1024,
        height: 768,
        isMobile: false,
        isTablet: false,
        isDesktop: true,
        deviceType: 'desktop',
        screenSize: 'lg',
        orientation: 'landscape',
        isTouch: false,
        reducedMotion: false,
        highContrast: false,
      };
    }

    const width = window.innerWidth;
    const height = window.innerHeight;
    
    return {
      width,
      height,
      isMobile: width < BREAKPOINTS.tablet,
      isTablet: width >= BREAKPOINTS.tablet && width < BREAKPOINTS.desktop,
      isDesktop: width >= BREAKPOINTS.desktop,
      deviceType: getDeviceType(width),
      screenSize: getScreenSize(width),
      orientation: width > height ? 'landscape' : 'portrait',
      isTouch: isTouchDevice(),
      reducedMotion: prefersReducedMotion(),
      highContrast: prefersHighContrast(),
    };
  });

  useEffect(() => {
    if (typeof window === 'undefined') return;

    const updateState = () => {
      const width = window.innerWidth;
      const height = window.innerHeight;
      
      setState({
        width,
        height,
        isMobile: width < BREAKPOINTS.tablet,
        isTablet: width >= BREAKPOINTS.tablet && width < BREAKPOINTS.desktop,
        isDesktop: width >= BREAKPOINTS.desktop,
        deviceType: getDeviceType(width),
        screenSize: getScreenSize(width),
        orientation: width > height ? 'landscape' : 'portrait',
        isTouch: isTouchDevice(),
        reducedMotion: prefersReducedMotion(),
        highContrast: prefersHighContrast(),
      });
    };

    // Debounced resize handler
    let timeoutId: NodeJS.Timeout;
    const handleResize = () => {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(updateState, 100);
    };

    // Media query listeners for accessibility preferences
    const motionMediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    const contrastMediaQuery = window.matchMedia('(prefers-contrast: high)');
    
    const handleMotionChange = () => updateState();
    const handleContrastChange = () => updateState();

    // Add event listeners
    window.addEventListener('resize', handleResize);
    window.addEventListener('orientationchange', updateState);
    motionMediaQuery.addEventListener('change', handleMotionChange);
    contrastMediaQuery.addEventListener('change', handleContrastChange);

    // Cleanup
    return () => {
      clearTimeout(timeoutId);
      window.removeEventListener('resize', handleResize);
      window.removeEventListener('orientationchange', updateState);
      motionMediaQuery.removeEventListener('change', handleMotionChange);
      contrastMediaQuery.removeEventListener('change', handleContrastChange);
    };
  }, []);

  return state;
};

/**
 * Hook for matching specific breakpoints
 */
export const useBreakpoint = (breakpoint: keyof typeof BREAKPOINTS): boolean => {
  const { width } = useResponsive();
  return width >= BREAKPOINTS[breakpoint];
};

/**
 * Hook for responsive values based on current screen size
 */
export const useResponsiveValue = <T>(values: {
  mobile?: T;
  tablet?: T;
  desktop?: T;
  default: T;
}): T => {
  const { isMobile, isTablet, isDesktop } = useResponsive();
  
  if (isMobile && values.mobile !== undefined) return values.mobile;
  if (isTablet && values.tablet !== undefined) return values.tablet;
  if (isDesktop && values.desktop !== undefined) return values.desktop;
  
  return values.default;
};

/**
 * Hook for responsive chart dimensions
 */
export const useChartDimensions = (baseHeight: number = 300) => {
  const { isMobile, isTablet, width } = useResponsive();
  
  return {
    height: isMobile ? baseHeight * 0.7 : isTablet ? baseHeight * 0.85 : baseHeight,
    width: Math.min(width - 40, 1200), // Max width with padding
    aspectRatio: isMobile ? '16/10' : isTablet ? '16/9' : '2/1',
  };
};

/**
 * Hook for responsive font sizes
 */
export const useResponsiveFontSizes = (size: 'sm' | 'md' | 'lg' = 'md') => {
  return useResponsiveValue({
    mobile: {
      sm: { title: '0.8rem', value: '1.25rem', subtitle: '0.75rem' },
      md: { title: '0.85rem', value: '1.5rem', subtitle: '0.75rem' },
      lg: { title: '0.9rem', value: '1.75rem', subtitle: '0.75rem' },
    }[size],
    tablet: {
      sm: { title: '0.85rem', value: '1.5rem', subtitle: '0.8rem' },
      md: { title: '0.9rem', value: '1.75rem', subtitle: '0.8rem' },
      lg: { title: '1rem', value: '2rem', subtitle: '0.8rem' },
    }[size],
    desktop: {
      sm: { title: '0.9rem', value: '1.75rem', subtitle: '0.875rem' },
      md: { title: '1rem', value: '2rem', subtitle: '0.875rem' },
      lg: { title: '1.1rem', value: '2.5rem', subtitle: '0.875rem' },
    }[size],
    default: {
      title: '1rem',
      value: '2rem',
      subtitle: '0.875rem',
    },
  });
};

// Utility functions
function getDeviceType(width: number): DeviceType {
  if (width < BREAKPOINTS.tablet) return 'mobile';
  if (width < BREAKPOINTS.desktop) return 'tablet';
  return 'desktop';
}

function getScreenSize(width: number): ScreenSize {
  if (width < BREAKPOINTS.mobile) return 'xs';
  if (width < BREAKPOINTS.tablet) return 'sm';
  if (width < BREAKPOINTS.desktop) return 'md';
  if (width < BREAKPOINTS.large) return 'lg';
  if (width < BREAKPOINTS.xl) return 'xl';
  return 'xxl';
}

function isTouchDevice(): boolean {
  if (typeof window === 'undefined') return false;
  
  return (
    'ontouchstart' in window ||
    navigator.maxTouchPoints > 0 ||
    // @ts-ignore
    navigator.msMaxTouchPoints > 0
  );
}

function prefersReducedMotion(): boolean {
  if (typeof window === 'undefined') return false;
  
  return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
}

function prefersHighContrast(): boolean {
  if (typeof window === 'undefined') return false;
  
  return window.matchMedia('(prefers-contrast: high)').matches;
}

/**
 * Utility function to get responsive class names
 */
export const getResponsiveClassNames = (
  baseClass: string,
  responsive: { mobile?: string; tablet?: string; desktop?: string } = {}
): string => {
  const { isMobile, isTablet, isDesktop } = useResponsive();
  
  let classes = baseClass;
  
  if (isMobile && responsive.mobile) {
    classes += ` ${responsive.mobile}`;
  } else if (isTablet && responsive.tablet) {
    classes += ` ${responsive.tablet}`;
  } else if (isDesktop && responsive.desktop) {
    classes += ` ${responsive.desktop}`;
  }
  
  return classes;
};

/**
 * Utility function for responsive grid columns
 */
export const getResponsiveGridColumns = (
  columns: { mobile?: number; tablet?: number; desktop?: number; default: number }
): number => {
  const { isMobile, isTablet, isDesktop } = useResponsive();
  
  if (isMobile && columns.mobile !== undefined) return columns.mobile;
  if (isTablet && columns.tablet !== undefined) return columns.tablet;
  if (isDesktop && columns.desktop !== undefined) return columns.desktop;
  
  return columns.default;
};