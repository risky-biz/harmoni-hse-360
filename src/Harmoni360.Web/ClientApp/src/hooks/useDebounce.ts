import { useState, useEffect, useRef, useCallback } from 'react';

/**
 * Debounces a value, updating it only after the specified delay
 */
export function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

/**
 * Debounces a callback function
 */
export function useDebouncedCallback<Args extends any[]>(
  callback: (...args: Args) => void,
  delay: number
): (...args: Args) => void {
  const timeoutRef = useRef<NodeJS.Timeout>();
  const callbackRef = useRef(callback);

  // Update callback ref when callback changes
  useEffect(() => {
    callbackRef.current = callback;
  }, [callback]);

  const debouncedCallback = useCallback(
    (...args: Args) => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }

      timeoutRef.current = setTimeout(() => {
        callbackRef.current(...args);
      }, delay);
    },
    [delay]
  );

  // Clean up timeout on unmount
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  return debouncedCallback;
}

/**
 * Advanced debounced search hook with loading state
 */
export function useDebouncedSearch<T>(
  searchFunction: (query: string) => Promise<T>,
  delay: number = 300
) {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const debouncedQuery = useDebounce(query, delay);
  const abortControllerRef = useRef<AbortController>();

  useEffect(() => {
    if (!debouncedQuery.trim()) {
      setResults(null);
      setIsLoading(false);
      setError(null);
      return;
    }

    const performSearch = async () => {
      // Cancel previous request
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }

      // Create new abort controller
      abortControllerRef.current = new AbortController();

      setIsLoading(true);
      setError(null);

      try {
        const searchResults = await searchFunction(debouncedQuery);
        setResults(searchResults);
      } catch (err) {
        if (err instanceof Error && err.name !== 'AbortError') {
          setError(err.message);
        }
      } finally {
        setIsLoading(false);
      }
    };

    performSearch();

    // Cleanup function
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [debouncedQuery, searchFunction]);

  // Clear search results
  const clearSearch = useCallback(() => {
    setQuery('');
    setResults(null);
    setError(null);
    setIsLoading(false);
  }, []);

  return {
    query,
    setQuery,
    results,
    isLoading,
    error,
    clearSearch
  };
}

/**
 * Throttle hook for limiting function execution frequency
 */
export function useThrottle<T>(value: T, limit: number): T {
  const [throttledValue, setThrottledValue] = useState<T>(value);
  const lastRun = useRef(Date.now());

  useEffect(() => {
    const handler = setTimeout(() => {
      if (Date.now() - lastRun.current >= limit) {
        setThrottledValue(value);
        lastRun.current = Date.now();
      }
    }, limit - (Date.now() - lastRun.current));

    return () => clearTimeout(handler);
  }, [value, limit]);

  return throttledValue;
}

/**
 * Optimized scroll event handler with throttling
 */
export function useScrollHandler(
  callback: (scrollY: number) => void,
  throttleMs: number = 16
) {
  const callbackRef = useRef(callback);
  const throttledCallback = useRef<(scrollY: number) => void>();

  useEffect(() => {
    callbackRef.current = callback;
  }, [callback]);

  useEffect(() => {
    let timeoutId: NodeJS.Timeout;
    let lastExecuted = 0;

    throttledCallback.current = (scrollY: number) => {
      const now = Date.now();
      
      if (now - lastExecuted >= throttleMs) {
        callbackRef.current(scrollY);
        lastExecuted = now;
      } else {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => {
          callbackRef.current(scrollY);
          lastExecuted = Date.now();
        }, throttleMs - (now - lastExecuted));
      }
    };

    const handleScroll = () => {
      throttledCallback.current?.(window.scrollY);
    };

    window.addEventListener('scroll', handleScroll, { passive: true });

    return () => {
      window.removeEventListener('scroll', handleScroll);
      clearTimeout(timeoutId);
    };
  }, [throttleMs]);

  return throttledCallback.current;
}