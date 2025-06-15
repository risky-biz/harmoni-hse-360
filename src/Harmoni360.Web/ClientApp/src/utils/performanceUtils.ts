import { useMemo, useCallback } from 'react';

// Memoization helper for complex calculations
export const useMemoizedCalculation = <T>(
  calculation: () => T,
  dependencies: any[]
): T => {
  return useMemo(calculation, dependencies);
};

// Debounced callback helper
export const useDebouncedCallback = <T extends (...args: any[]) => void>(
  callback: T,
  delay: number
): T => {
  return useCallback((...args: Parameters<T>) => {
    const timeoutId = setTimeout(() => callback(...args), delay);
    return () => clearTimeout(timeoutId);
  }, [callback, delay]) as T;
};

// Throttle function for API calls
export const throttle = <T extends (...args: any[]) => void>(
  func: T,
  limit: number
): T => {
  let inThrottle: boolean;
  return ((...args: Parameters<T>) => {
    if (!inThrottle) {
      func(...args);
      inThrottle = true;
      setTimeout(() => (inThrottle = false), limit);
    }
  }) as T;
};

// Virtual scrolling parameters
export interface VirtualScrollOptions {
  itemHeight: number;
  containerHeight: number;
  overscan?: number;
}

export const calculateVirtualItems = (
  totalItems: number,
  scrollTop: number,
  options: VirtualScrollOptions
) => {
  const { itemHeight, containerHeight, overscan = 5 } = options;
  
  const visibleStart = Math.floor(scrollTop / itemHeight);
  const visibleEnd = Math.min(
    visibleStart + Math.ceil(containerHeight / itemHeight),
    totalItems - 1
  );
  
  const startIndex = Math.max(0, visibleStart - overscan);
  const endIndex = Math.min(totalItems - 1, visibleEnd + overscan);
  
  return {
    startIndex,
    endIndex,
    offsetY: startIndex * itemHeight,
    visibleItems: endIndex - startIndex + 1
  };
};

// Cache management for API responses
class CacheManager {
  private cache = new Map<string, { data: any; timestamp: number; ttl: number }>();
  
  set(key: string, data: any, ttl: number = 5 * 60 * 1000) { // 5 minutes default TTL
    this.cache.set(key, {
      data,
      timestamp: Date.now(),
      ttl
    });
  }
  
  get(key: string): any | null {
    const item = this.cache.get(key);
    if (!item) return null;
    
    const isExpired = Date.now() - item.timestamp > item.ttl;
    if (isExpired) {
      this.cache.delete(key);
      return null;
    }
    
    return item.data;
  }
  
  clear() {
    this.cache.clear();
  }
  
  delete(key: string) {
    this.cache.delete(key);
  }
  
  has(key: string): boolean {
    const item = this.cache.get(key);
    if (!item) return false;
    
    const isExpired = Date.now() - item.timestamp > item.ttl;
    if (isExpired) {
      this.cache.delete(key);
      return false;
    }
    
    return true;
  }
}

export const cacheManager = new CacheManager();

// Optimized data transformation utilities
export const optimizeDataTransformation = {
  // Batch process arrays efficiently
  batchProcess: <T, R>(
    items: T[],
    processor: (item: T) => R,
    batchSize: number = 100
  ): R[] => {
    const results: R[] = [];
    for (let i = 0; i < items.length; i += batchSize) {
      const batch = items.slice(i, i + batchSize);
      results.push(...batch.map(processor));
      
      // Allow other tasks to run between batches
      if (i + batchSize < items.length) {
        return new Promise<R[]>(resolve => {
          setTimeout(() => {
            resolve(results);
          }, 0);
        }) as any;
      }
    }
    return results;
  },
  
  // Efficient array filtering with early exit
  efficientFilter: <T>(
    items: T[],
    predicate: (item: T) => boolean,
    maxResults?: number
  ): T[] => {
    const results: T[] = [];
    for (const item of items) {
      if (predicate(item)) {
        results.push(item);
        if (maxResults && results.length >= maxResults) {
          break;
        }
      }
    }
    return results;
  },
  
  // Memoized sorting
  memoizedSort: <T>(
    items: T[],
    compareFn: (a: T, b: T) => number,
    cacheKey: string
  ): T[] => {
    const cached = cacheManager.get(cacheKey);
    if (cached) return cached;
    
    const sorted = [...items].sort(compareFn);
    cacheManager.set(cacheKey, sorted, 2 * 60 * 1000); // 2 minutes cache
    return sorted;
  }
};

// Performance monitoring utilities
export const performanceMonitor = {
  // Measure function execution time
  measureTime: <T>(
    fn: () => T,
    label?: string
  ): { result: T; duration: number } => {
    const start = performance.now();
    const result = fn();
    const duration = performance.now() - start;
    
    if (label && duration > 16) { // Log if over 16ms (1 frame at 60fps)
      console.warn(`Performance warning: ${label} took ${duration.toFixed(2)}ms`);
    }
    
    return { result, duration };
  },
  
  // Measure async function execution time
  measureTimeAsync: async <T>(
    fn: () => Promise<T>,
    label?: string
  ): Promise<{ result: T; duration: number }> => {
    const start = performance.now();
    const result = await fn();
    const duration = performance.now() - start;
    
    if (label && duration > 100) { // Log if over 100ms for async operations
      console.warn(`Performance warning: ${label} took ${duration.toFixed(2)}ms`);
    }
    
    return { result, duration };
  }
};

// Image optimization utilities
export const imageOptimization = {
  // Create optimized image blob
  optimizeImage: (
    file: File,
    maxWidth: number = 1920,
    maxHeight: number = 1080,
    quality: number = 0.8
  ): Promise<Blob> => {
    return new Promise((resolve, reject) => {
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');
      const img = new Image();
      
      img.onload = () => {
        // Calculate new dimensions
        let { width, height } = img;
        if (width > maxWidth) {
          height = (height * maxWidth) / width;
          width = maxWidth;
        }
        if (height > maxHeight) {
          width = (width * maxHeight) / height;
          height = maxHeight;
        }
        
        canvas.width = width;
        canvas.height = height;
        
        // Draw and compress
        ctx?.drawImage(img, 0, 0, width, height);
        canvas.toBlob(
          blob => blob ? resolve(blob) : reject(new Error('Failed to create blob')),
          'image/jpeg',
          quality
        );
      };
      
      img.onerror = reject;
      img.src = URL.createObjectURL(file);
    });
  },
  
  // Generate thumbnail
  generateThumbnail: (
    file: File,
    size: number = 150
  ): Promise<string> => {
    return new Promise((resolve, reject) => {
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');
      const img = new Image();
      
      img.onload = () => {
        canvas.width = size;
        canvas.height = size;
        
        // Calculate crop dimensions for square thumbnail
        const minDim = Math.min(img.width, img.height);
        const startX = (img.width - minDim) / 2;
        const startY = (img.height - minDim) / 2;
        
        ctx?.drawImage(
          img,
          startX, startY, minDim, minDim,
          0, 0, size, size
        );
        
        resolve(canvas.toDataURL('image/jpeg', 0.7));
      };
      
      img.onerror = reject;
      img.src = URL.createObjectURL(file);
    });
  }
};