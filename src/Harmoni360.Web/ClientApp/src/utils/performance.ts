// Performance monitoring utilities for Harmoni360

export interface PerformanceMetrics {
  pageLoadTime: number;
  firstContentfulPaint: number;
  timeToInteractive: number;
  apiResponseTime: number;
  componentRenderTime: number;
}

export class PerformanceMonitor {
  private static instance: PerformanceMonitor;
  private metrics: Map<string, number> = new Map();

  static getInstance(): PerformanceMonitor {
    if (!PerformanceMonitor.instance) {
      PerformanceMonitor.instance = new PerformanceMonitor();
    }
    return PerformanceMonitor.instance;
  }

  // Measure page load performance
  measurePageLoad(): void {
    if (typeof window !== 'undefined' && 'performance' in window) {
      window.addEventListener('load', () => {
        const perfData = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
        
        this.metrics.set('pageLoadTime', perfData.loadEventEnd - perfData.loadEventStart);
        this.metrics.set('domContentLoaded', perfData.domContentLoadedEventEnd - perfData.domContentLoadedEventStart);
        this.metrics.set('timeToInteractive', perfData.loadEventEnd - perfData.fetchStart);
        
        // Report to console in development
        if (process.env.NODE_ENV === 'development') {
          console.group('🚀 Performance Metrics');
          console.log(`Page Load Time: ${this.metrics.get('pageLoadTime')}ms`);
          console.log(`DOM Content Loaded: ${this.metrics.get('domContentLoaded')}ms`);
          console.log(`Time to Interactive: ${this.metrics.get('timeToInteractive')}ms`);
          console.groupEnd();
        }
      });
    }
  }

  // Measure API response times
  measureApiCall(endpoint: string, startTime: number): void {
    const responseTime = performance.now() - startTime;
    this.metrics.set(`api_${endpoint}`, responseTime);
    
    if (process.env.NODE_ENV === 'development') {
      console.log(`📡 API ${endpoint}: ${responseTime.toFixed(2)}ms`);
      
      // Warn on slow API calls
      if (responseTime > 2000) {
        console.warn(`⚠️ Slow API call: ${endpoint} took ${responseTime.toFixed(2)}ms`);
      }
    }
  }

  // Measure component render times
  measureComponentRender(componentName: string, renderTime: number): void {
    this.metrics.set(`component_${componentName}`, renderTime);
    
    if (process.env.NODE_ENV === 'development' && renderTime > 100) {
      console.warn(`⚠️ Slow component render: ${componentName} took ${renderTime.toFixed(2)}ms`);
    }
  }

  // Get all metrics
  getMetrics(): Map<string, number> {
    return new Map(this.metrics);
  }

  // Reset metrics
  reset(): void {
    this.metrics.clear();
  }

  // Report performance issues
  reportPerformanceIssues(): void {
    const issues: string[] = [];
    
    this.metrics.forEach((time, key) => {
      if (key.startsWith('api_') && time > 3000) {
        issues.push(`Slow API: ${key} (${time.toFixed(2)}ms)`);
      }
      if (key.startsWith('component_') && time > 200) {
        issues.push(`Slow Component: ${key} (${time.toFixed(2)}ms)`);
      }
    });
    
    if (issues.length > 0) {
      console.group('🐌 Performance Issues Detected');
      issues.forEach(issue => console.warn(issue));
      console.groupEnd();
    }
  }
}

// Performance optimization hooks
export const usePerformanceMonitor = () => {
  const monitor = PerformanceMonitor.getInstance();
  
  const measureApiCall = (endpoint: string) => {
    const startTime = performance.now();
    return () => monitor.measureApiCall(endpoint, startTime);
  };
  
  const measureComponentRender = (componentName: string) => {
    const startTime = performance.now();
    return () => monitor.measureComponentRender(componentName, performance.now() - startTime);
  };
  
  return {
    measureApiCall,
    measureComponentRender,
    getMetrics: () => monitor.getMetrics(),
    reportIssues: () => monitor.reportPerformanceIssues()
  };
};

// Web Vitals measurement
export const measureWebVitals = () => {
  if (typeof window !== 'undefined') {
    // Measure First Contentful Paint
    const observer = new PerformanceObserver((list) => {
      list.getEntries().forEach((entry) => {
        if (entry.name === 'first-contentful-paint') {
          console.log('🎨 First Contentful Paint:', entry.startTime);
        }
        if (entry.name === 'largest-contentful-paint') {
          console.log('🖼️ Largest Contentful Paint:', entry.startTime);
        }
      });
    });
    
    observer.observe({ entryTypes: ['paint', 'largest-contentful-paint'] });
    
    // Measure Cumulative Layout Shift
    const clsObserver = new PerformanceObserver((list) => {
      let cls = 0;
      list.getEntries().forEach((entry: any) => {
        if (!entry.hadRecentInput) {
          cls += entry.value;
        }
      });
      if (cls > 0) {
        console.log('📏 Cumulative Layout Shift:', cls);
      }
    });
    
    clsObserver.observe({ entryTypes: ['layout-shift'] });
  }
};

// Bundle size analyzer helper
export const analyzeBundleSize = () => {
  if (process.env.NODE_ENV === 'development') {
    console.group('📦 Bundle Analysis Tips');
    console.log('Run "npm run build" to generate production bundle');
    console.log('Use "npm run analyze" to analyze bundle size');
    console.log('Consider code splitting for large components');
    console.log('Use dynamic imports for non-critical features');
    console.groupEnd();
  }
};

// Initialize performance monitoring
export const initializePerformanceMonitoring = () => {
  const monitor = PerformanceMonitor.getInstance();
  monitor.measurePageLoad();
  measureWebVitals();
  
  // Schedule performance report every 30 seconds in development
  if (process.env.NODE_ENV === 'development') {
    setInterval(() => {
      monitor.reportPerformanceIssues();
    }, 30000);
  }
};