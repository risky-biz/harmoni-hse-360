import React, { useState, useCallback, useRef, useEffect } from 'react';
import { CSpinner } from '@coreui/react';

interface LazyImageProps {
  src: string;
  alt: string;
  className?: string;
  placeholder?: string;
  onLoad?: () => void;
  onError?: () => void;
  threshold?: number;
}

const LazyImage: React.FC<LazyImageProps> = React.memo(({
  src,
  alt,
  className = '',
  placeholder = '/placeholder.jpg',
  onLoad,
  onError,
  threshold = 0.1
}) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const [isInView, setIsInView] = useState(false);
  const [hasError, setHasError] = useState(false);
  const imgRef = useRef<HTMLImageElement>(null);
  const observerRef = useRef<IntersectionObserver | null>(null);

  const handleLoad = useCallback(() => {
    setIsLoaded(true);
    onLoad?.();
  }, [onLoad]);

  const handleError = useCallback(() => {
    setHasError(true);
    onError?.();
  }, [onError]);

  useEffect(() => {
    const currentRef = imgRef.current;
    
    if (!currentRef) return;

    // Set up intersection observer for lazy loading
    observerRef.current = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setIsInView(true);
            observerRef.current?.disconnect();
          }
        });
      },
      { threshold }
    );

    observerRef.current.observe(currentRef);

    return () => {
      observerRef.current?.disconnect();
    };
  }, [threshold]);

  // Only start loading image when it's in view
  const imageSrc = isInView ? src : undefined;

  return (
    <div className={`lazy-image-container ${className}`} ref={imgRef}>
      {!isLoaded && !hasError && (
        <div className="lazy-image-placeholder d-flex align-items-center justify-content-center">
          {isInView ? (
            <CSpinner size="sm" />
          ) : (
            <div className="placeholder-text">Loading...</div>
          )}
        </div>
      )}
      
      {hasError ? (
        <div className="lazy-image-error d-flex align-items-center justify-content-center">
          <span>Failed to load image</span>
        </div>
      ) : (
        imageSrc && (
          <img
            src={imageSrc}
            alt={alt}
            className={`lazy-image ${isLoaded ? 'loaded' : 'loading'}`}
            onLoad={handleLoad}
            onError={handleError}
            loading="lazy"
            decoding="async"
          />
        )
      )}
    </div>
  );
});

LazyImage.displayName = 'LazyImage';

export default LazyImage;