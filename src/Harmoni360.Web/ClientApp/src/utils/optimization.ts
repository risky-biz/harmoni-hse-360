// Production optimization utilities for Harmoni360

/**
 * Code splitting utilities
 */
export const lazy = {
  // Health management pages
  HealthDashboard: () => import('../pages/health/HealthDashboard'),
  HealthList: () => import('../pages/health/HealthList'),
  HealthDetail: () => import('../pages/health/HealthDetail'),
  CreateHealthRecord: () => import('../pages/health/CreateHealthRecord'),
  EditHealthRecord: () => import('../pages/health/EditHealthRecord'),
  VaccinationManagement: () => import('../pages/health/VaccinationManagement'),
  HealthCompliance: () => import('../pages/health/HealthCompliance'),

  // Incident management pages
  IncidentDashboard: () => import('../pages/incidents/IncidentDashboard'),
  IncidentList: () => import('../pages/incidents/IncidentList'),
  IncidentDetail: () => import('../pages/incidents/IncidentDetail'),
  CreateIncident: () => import('../pages/incidents/CreateIncident'),
  EditIncident: () => import('../pages/incidents/EditIncident'),

  // PPE management pages
  PPEDashboard: () => import('../pages/ppe/PPEDashboard'),
  PPEList: () => import('../pages/ppe/PPEList'),
  PPEDetail: () => import('../pages/ppe/PPEDetail'),
  PPEManagement: () => import('../pages/ppe/PPEManagement'),

  // Hazard management pages
  HazardDashboard: () => import('../pages/hazards/HazardDashboard'),
  HazardList: () => import('../pages/hazards/HazardList'),
  HazardDetail: () => import('../pages/hazards/HazardDetail'),
  CreateHazard: () => import('../pages/hazards/CreateHazard'),

  // HSSE dashboard
  HsseDashboard: () => import('../pages/hsse/HsseDashboard'),

  // Heavy components
  Charts: () => import('../components/dashboard/ChartCard'),
  FileUploader: () => import('../components/common/AttachmentManager'),
  DataTable: () => import('../components/common/VirtualizedList'),
};

/**
 * Image optimization utilities
 */
export const imageOptimization = {
  // Generate responsive image URLs
  generateResponsiveUrl: (baseUrl: string, width: number, quality: number = 85): string => {
    if (!baseUrl) return '';
    
    // Add image optimization parameters
    const separator = baseUrl.includes('?') ? '&' : '?';
    return `${baseUrl}${separator}w=${width}&q=${quality}&f=webp`;
  },

  // Generate srcSet for responsive images
  generateSrcSet: (baseUrl: string, sizes: number[] = [320, 640, 768, 1024, 1280]): string => {
    return sizes
      .map(size => `${imageOptimization.generateResponsiveUrl(baseUrl, size)} ${size}w`)
      .join(', ');
  },

  // Preload critical images
  preloadImage: (src: string): Promise<void> => {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve();
      img.onerror = reject;
      img.src = src;
    });
  },

  // Batch preload images
  preloadImages: async (urls: string[]): Promise<void> => {
    try {
      await Promise.all(urls.map(url => imageOptimization.preloadImage(url)));
    } catch (error) {
      console.warn('Failed to preload some images:', error);
    }
  }
};

/**
 * Bundle optimization utilities
 */
export const bundleOptimization = {
  // Dynamic import with error handling
  dynamicImport: async <T>(importFn: () => Promise<{ default: T }>): Promise<T> => {
    try {
      const module = await importFn();
      return module.default;
    } catch (error) {
      console.error('Failed to load module:', error);
      throw error;
    }
  },

  // Preload route modules
  preloadRoute: (routeImport: () => Promise<any>): void => {
    // Use requestIdleCallback if available, otherwise use setTimeout
    if ('requestIdleCallback' in window) {
      requestIdleCallback(() => {
        routeImport().catch(console.error);
      });
    } else {
      setTimeout(() => {
        routeImport().catch(console.error);
      }, 1000);
    }
  },

  // Prefetch critical routes
  prefetchCriticalRoutes: (): void => {
    const criticalRoutes = [
      lazy.HealthDashboard,
      lazy.IncidentDashboard,
      lazy.PPEDashboard,
      lazy.HsseDashboard,
    ];

    criticalRoutes.forEach(route => {
      bundleOptimization.preloadRoute(route);
    });
  }
};

/**
 * API optimization utilities
 */
export const apiOptimization = {
  // Request deduplication
  pendingRequests: new Map<string, Promise<any>>(),

  // Deduplicate identical API requests
  deduplicateRequest: <T>(key: string, requestFn: () => Promise<T>): Promise<T> => {
    if (apiOptimization.pendingRequests.has(key)) {
      return apiOptimization.pendingRequests.get(key)!;
    }

    const promise = requestFn()
      .finally(() => {
        apiOptimization.pendingRequests.delete(key);
      });

    apiOptimization.pendingRequests.set(key, promise);
    return promise;
  },

  // Batch API requests
  batchRequests: <T>(requests: Array<() => Promise<T>>, batchSize: number = 5): Promise<T[]> => {
    const batches: Array<Array<() => Promise<T>>> = [];
    
    for (let i = 0; i < requests.length; i += batchSize) {
      batches.push(requests.slice(i, i + batchSize));
    }

    return batches.reduce(async (acc, batch) => {
      const results = await acc;
      const batchResults = await Promise.all(batch.map(request => request()));
      return [...results, ...batchResults];
    }, Promise.resolve([] as T[]));
  },

  // Request priority queue
  createPriorityQueue: () => {
    const highPriorityQueue: Array<() => Promise<any>> = [];
    const normalPriorityQueue: Array<() => Promise<any>> = [];
    const lowPriorityQueue: Array<() => Promise<any>> = [];
    let processing = false;

    const processQueue = async () => {
      if (processing) return;
      processing = true;

      try {
        // Process high priority first
        while (highPriorityQueue.length > 0) {
          const request = highPriorityQueue.shift()!;
          await request();
        }

        // Then normal priority
        while (normalPriorityQueue.length > 0) {
          const request = normalPriorityQueue.shift()!;
          await request();
        }

        // Finally low priority
        while (lowPriorityQueue.length > 0) {
          const request = lowPriorityQueue.shift()!;
          await request();
        }
      } finally {
        processing = false;
      }
    };

    return {
      addRequest: (request: () => Promise<any>, priority: 'high' | 'normal' | 'low' = 'normal') => {
        switch (priority) {
          case 'high':
            highPriorityQueue.push(request);
            break;
          case 'low':
            lowPriorityQueue.push(request);
            break;
          default:
            normalPriorityQueue.push(request);
        }
        processQueue();
      }
    };
  }
};

/**
 * Memory optimization utilities
 */
export const memoryOptimization = {
  // Cleanup function for component unmount
  createCleanup: () => {
    const cleanupFunctions: Array<() => void> = [];
    
    return {
      add: (cleanup: () => void) => {
        cleanupFunctions.push(cleanup);
      },
      execute: () => {
        cleanupFunctions.forEach(cleanup => {
          try {
            cleanup();
          } catch (error) {
            console.error('Cleanup function failed:', error);
          }
        });
        cleanupFunctions.length = 0;
      }
    };
  },

  // Weak reference cache for avoiding memory leaks
  createWeakCache: <K extends object, V>() => {
    const cache = new WeakMap<K, V>();
    
    return {
      get: (key: K): V | undefined => cache.get(key),
      set: (key: K, value: V): void => { cache.set(key, value); },
      has: (key: K): boolean => cache.has(key),
      delete: (key: K): boolean => cache.delete(key)
    };
  },

  // Monitor memory usage in development
  monitorMemory: () => {
    if (process.env.NODE_ENV === 'development' && 'memory' in performance) {
      const memInfo = (performance as any).memory;
      console.group('🧠 Memory Usage');
      console.log(`Used: ${(memInfo.usedJSHeapSize / 1024 / 1024).toFixed(2)} MB`);
      console.log(`Total: ${(memInfo.totalJSHeapSize / 1024 / 1024).toFixed(2)} MB`);
      console.log(`Limit: ${(memInfo.jsHeapSizeLimit / 1024 / 1024).toFixed(2)} MB`);
      console.groupEnd();
    }
  }
};

/**
 * Performance optimization initializer
 */
export const initializeOptimizations = () => {
  // Prefetch critical routes on app start
  bundleOptimization.prefetchCriticalRoutes();
  
  // Monitor memory usage in development
  if (process.env.NODE_ENV === 'development') {
    setInterval(() => {
      memoryOptimization.monitorMemory();
    }, 60000); // Every minute
  }
  
  // Set up performance observers
  if ('PerformanceObserver' in window) {
    const observer = new PerformanceObserver((list) => {
      list.getEntries().forEach((entry) => {
        if (entry.entryType === 'measure' && entry.duration > 100) {
          console.warn(`⚠️ Slow operation: ${entry.name} took ${entry.duration.toFixed(2)}ms`);
        }
      });
    });
    
    observer.observe({ entryTypes: ['measure'] });
  }
};

/**
 * Resource hints for critical resources
 */
export const addResourceHints = () => {
  const head = document.head;
  
  // DNS prefetch for external resources
  const dnsPrefetch = [
    '//fonts.googleapis.com',
    '//fonts.gstatic.com',
    '//api.harmoni360.com'
  ];
  
  dnsPrefetch.forEach(domain => {
    const link = document.createElement('link');
    link.rel = 'dns-prefetch';
    link.href = domain;
    head.appendChild(link);
  });
  
  // Preconnect to critical origins
  const preconnect = [
    '//fonts.gstatic.com'
  ];
  
  preconnect.forEach(origin => {
    const link = document.createElement('link');
    link.rel = 'preconnect';
    link.href = origin;
    link.crossOrigin = 'anonymous';
    head.appendChild(link);
  });
};

export default {
  lazy,
  imageOptimization,
  bundleOptimization,
  apiOptimization,
  memoryOptimization,
  initializeOptimizations,
  addResourceHints
};
