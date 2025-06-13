import React, { Component, ErrorInfo, ReactNode } from 'react';
import {
  CContainer,
  CRow,
  CCol,
  CCard,
  CCardBody,
  CButton,
  CAlert,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faExclamationCircle,
  faRedo,
  faHome,
} from '@fortawesome/free-solid-svg-icons';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
  isAuthError: boolean;
}

class AuthErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
    errorInfo: null,
    isAuthError: false,
  };

  public static getDerivedStateFromError(error: Error): State {
    // Check if this is an authentication or authorization error
    const isAuthError = error.message.includes('401') ||
                       error.message.includes('403') ||
                       error.message.includes('Unauthorized') ||
                       error.message.includes('Access denied') ||
                       error.message.includes('Permission denied');

    return {
      hasError: true,
      error,
      errorInfo: null,
      isAuthError,
    };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Auth Error Boundary caught an error:', error, errorInfo);
    
    this.setState({
      error,
      errorInfo,
    });

    // Log authentication/authorization errors differently
    if (this.state.isAuthError) {
      console.warn('Authentication/Authorization error caught:', {
        error: error.message,
        stack: error.stack,
        componentStack: errorInfo.componentStack,
        user: localStorage.getItem('user'),
        timestamp: new Date().toISOString(),
      });
    }
  }

  private handleRetry = () => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
      isAuthError: false,
    });
  };

  private handleGoHome = () => {
    window.location.href = '/dashboard';
  };

  private handleReload = () => {
    window.location.reload();
  };

  public render() {
    if (this.state.hasError) {
      // If a custom fallback is provided, use it
      if (this.props.fallback) {
        return this.props.fallback;
      }

      // Handle authentication/authorization errors specially
      if (this.state.isAuthError) {
        return (
          <CContainer className="d-flex align-items-center min-vh-100">
            <CRow className="justify-content-center w-100">
              <CCol md={8} lg={6} xl={5}>
                <CCard className="mx-4">
                  <CCardBody className="p-4 text-center">
                    <div className="mb-4">
                      <FontAwesomeIcon
                        icon={faExclamationCircle}
                        size="4x"
                        className="text-danger"
                      />
                    </div>
                    
                    <h1 className="h2 mb-3">Authentication Error</h1>
                    
                    <p className="text-medium-emphasis mb-4">
                      There was an issue with your authentication or permissions. 
                      This might be due to an expired session or insufficient access rights.
                    </p>

                    <CAlert color="danger" className="text-start mb-4">
                      <h6 className="alert-heading">Error Details:</h6>
                      <div className="small">
                        {this.state.error?.message || 'Unknown authentication error'}
                      </div>
                    </CAlert>

                    <div className="d-flex flex-column flex-sm-row gap-2 justify-content-center">
                      <CButton
                        color="primary"
                        onClick={this.handleReload}
                        className="me-sm-2"
                      >
                        <FontAwesomeIcon icon={faRedo} className="me-2" />
                        Reload Page
                      </CButton>
                      
                      <CButton
                        color="secondary"
                        variant="outline"
                        onClick={this.handleGoHome}
                      >
                        <FontAwesomeIcon icon={faHome} className="me-2" />
                        Go to Dashboard
                      </CButton>
                    </div>

                    <div className="mt-4 text-medium-emphasis small">
                      <p className="mb-1">
                        If this problem persists, please try logging out and logging back in.
                      </p>
                      <p className="mb-0">
                        Contact IT support if the issue continues.
                      </p>
                    </div>
                  </CCardBody>
                </CCard>
              </CCol>
            </CRow>
          </CContainer>
        );
      }

      // Handle general errors
      return (
        <CContainer className="d-flex align-items-center min-vh-100">
          <CRow className="justify-content-center w-100">
            <CCol md={8} lg={6} xl={5}>
              <CCard className="mx-4">
                <CCardBody className="p-4 text-center">
                  <div className="mb-4">
                    <FontAwesomeIcon
                      icon={faExclamationCircle}
                      size="4x"
                      className="text-warning"
                    />
                  </div>
                  
                  <h1 className="h2 mb-3">Something went wrong</h1>
                  
                  <p className="text-medium-emphasis mb-4">
                    An unexpected error occurred while loading this page.
                  </p>

                  {process.env.NODE_ENV === 'development' && this.state.error && (
                    <CAlert color="warning" className="text-start mb-4">
                      <h6 className="alert-heading">Error Details (Development):</h6>
                      <div className="small">
                        <strong>Error:</strong> {this.state.error.message}
                      </div>
                      {this.state.errorInfo && (
                        <details className="mt-2">
                          <summary>Component Stack</summary>
                          <pre className="small mt-2">
                            {this.state.errorInfo.componentStack}
                          </pre>
                        </details>
                      )}
                    </CAlert>
                  )}

                  <div className="d-flex flex-column flex-sm-row gap-2 justify-content-center">
                    <CButton
                      color="primary"
                      onClick={this.handleRetry}
                      className="me-sm-2"
                    >
                      <FontAwesomeIcon icon={faRedo} className="me-2" />
                      Try Again
                    </CButton>
                    
                    <CButton
                      color="secondary"
                      variant="outline"
                      onClick={this.handleGoHome}
                    >
                      <FontAwesomeIcon icon={faHome} className="me-2" />
                      Go to Dashboard
                    </CButton>
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>
        </CContainer>
      );
    }

    return this.props.children;
  }
}

export default AuthErrorBoundary;