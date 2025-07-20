import React, { useEffect, useState } from 'react';
import ElsaStudioGuard from '../../components/auth/ElsaStudioGuard';
import { useAuth } from '../../hooks/useAuth';

const WorkflowManagement: React.FC = () => {
  const { token } = useAuth();
  const [isRedirecting, setIsRedirecting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const redirectToElsaStudio = async () => {
      if (!token) return;

      setIsRedirecting(true);
      setError(null);

      try {
        // Set JWT token in a cookie for Elsa Studio
        // Only use secure flag if running on HTTPS
        const isHttps = window.location.protocol === 'https:';
        const secureFlag = isHttps ? '; secure' : '';
        document.cookie = `harmoni360_token=${token}; path=/${secureFlag}; samesite=strict; max-age=28800`; // 8 hours

        // Redirect to Elsa Studio - the middleware will validate the JWT token from the cookie
        const elsaStudioUrl = `${window.location.origin}/elsa-studio/`;
        window.location.href = elsaStudioUrl;
      } catch (err) {
        setError(`Error redirecting to Elsa Studio: ${err instanceof Error ? err.message : 'Unknown error'}`);
        setIsRedirecting(false);
      }
    };

    redirectToElsaStudio();
  }, [token]);

  return (
    <ElsaStudioGuard>
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <div className="text-center">
          {error ? (
            <div className="alert alert-danger" role="alert">
              <h5 className="alert-heading">Authentication Error</h5>
              <p>{error}</p>
              <button 
                className="btn btn-primary"
                onClick={() => window.location.href = '/dashboard'}
              >
                Return to Dashboard
              </button>
            </div>
          ) : (
            <>
              <div className="spinner-border text-primary mb-3" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
              <p>
                {isRedirecting 
                  ? 'Redirecting to Elsa Studio...' 
                  : 'Preparing Elsa Studio access...'}
              </p>
            </>
          )}
        </div>
      </div>
    </ElsaStudioGuard>
  );
};

export default WorkflowManagement;