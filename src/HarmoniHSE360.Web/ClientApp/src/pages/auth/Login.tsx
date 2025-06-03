import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
  CButton,
  CCard,
  CCardBody,
  CCardGroup,
  CCol,
  CContainer,
  CForm,
  CFormInput,
  CInputGroup,
  CInputGroupText,
  CRow,
  CAlert,
  CSpinner,
  CFormCheck,
  CCallout,
  CAccordion,
  CAccordionBody,
  CAccordionHeader,
  CAccordionItem,
} from '@coreui/react';
import CIcon from '@coreui/icons-react';
import { cilLockLocked, cilUser, cilInfo } from '@coreui/icons';

import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { useLoginMutation, useGetDemoUsersQuery } from '../../features/auth/authApi';
import { loginStart, loginSuccess, loginFailure, selectAuth, clearError } from '../../features/auth/authSlice';
import type { LoginRequest } from '../../types/auth';

// Validation schema
const schema = yup.object({
  email: yup
    .string()
    .required('Email is required')
    .email('Please enter a valid email address'),
  password: yup
    .string()
    .required('Password is required')
    .min(6, 'Password must be at least 6 characters'),
});

const Login: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const auth = useAppSelector(selectAuth);
  
  const [login, { isLoading: isLoginLoading }] = useLoginMutation();
  const { data: demoUsers } = useGetDemoUsersQuery();
  const [showDemoUsers, setShowDemoUsers] = useState(false);

  const from = location.state?.from?.pathname || '/dashboard';

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<LoginRequest>({
    resolver: yupResolver(schema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
  });

  useEffect(() => {
    if (auth.isAuthenticated) {
      navigate(from, { replace: true });
    }
  }, [auth.isAuthenticated, navigate, from]);

  useEffect(() => {
    // Clear error when component mounts
    if (auth.error) {
      dispatch(clearError());
    }
  }, []);

  const onSubmit = async (data: LoginRequest) => {
    try {
      dispatch(loginStart());
      const result = await login(data).unwrap();
      dispatch(loginSuccess(result));
      navigate(from, { replace: true });
    } catch (error: any) {
      const message = error?.data?.message || 'Login failed. Please try again.';
      dispatch(loginFailure(message));
    }
  };

  const fillDemoCredentials = (email: string, password: string) => {
    setValue('email', email);
    setValue('password', password);
    setShowDemoUsers(false);
  };

  const handleClearError = () => {
    dispatch(clearError());
  };

  return (
    <div className="bg-light min-vh-100 d-flex flex-row align-items-center" style={{ backgroundColor: 'var(--harmoni-grey)' }}>
      <CContainer>
        <CRow className="justify-content-center">
          <CCol md={8} lg={6}>
            <CCardGroup>
              <CCard className="p-4 shadow-lg" style={{ borderRadius: '12px' }}>
                <CCardBody>
                  <CForm onSubmit={handleSubmit(onSubmit)}>
                    <div className="text-center mb-4">
                      <div className="mb-3">
                        <img 
                          src="/Harmoni_HSE_360_Logo.png" 
                          alt="HarmoniHSE360 Logo"
                          style={{ 
                            width: '120px', 
                            height: 'auto',
                            maxHeight: '80px'
                          }}
                        />
                      </div>
                      <p className="text-medium-emphasis" style={{ fontSize: '14px', marginTop: '16px' }}>
                        Complete Safety. Seamless Harmony.
                      </p>
                    </div>

                    <h3 className="mb-3" style={{ color: 'var(--harmoni-charcoal)', fontFamily: 'Poppins, sans-serif' }}>Welcome Back</h3>
                    <p className="text-medium-emphasis mb-4">Sign in to your account to continue</p>

                    {auth.error && (
                      <CAlert 
                        color="danger" 
                        dismissible 
                        onClose={handleClearError}
                        className="d-flex align-items-center"
                      >
                        <CIcon icon={cilInfo} className="flex-shrink-0 me-2" />
                        <div>{auth.error}</div>
                      </CAlert>
                    )}

                    <CInputGroup className="mb-3">
                      <CInputGroupText>
                        <CIcon icon={cilUser} />
                      </CInputGroupText>
                      <CFormInput
                        {...register('email')}
                        type="email"
                        placeholder="Email"
                        autoComplete="username"
                        invalid={!!errors.email}
                        disabled={isLoginLoading}
                      />
                    </CInputGroup>
                    {errors.email && (
                      <div className="text-danger small mb-2">{errors.email.message}</div>
                    )}

                    <CInputGroup className="mb-4">
                      <CInputGroupText>
                        <CIcon icon={cilLockLocked} />
                      </CInputGroupText>
                      <CFormInput
                        {...register('password')}
                        type="password"
                        placeholder="Password"
                        autoComplete="current-password"
                        invalid={!!errors.password}
                        disabled={isLoginLoading}
                      />
                    </CInputGroup>
                    {errors.password && (
                      <div className="text-danger small mb-3">{errors.password.message}</div>
                    )}

                    <CFormCheck
                      {...register('rememberMe')}
                      id="rememberMe"
                      label="Remember me"
                      className="mb-4"
                      disabled={isLoginLoading}
                    />

                    <CRow>
                      <CCol xs={6}>
                        <CButton
                          className="px-4 py-2"
                          type="submit"
                          disabled={isLoginLoading}
                          style={{
                            backgroundColor: 'var(--harmoni-teal)',
                            borderColor: 'var(--harmoni-teal)',
                            borderRadius: '8px',
                            fontWeight: '500',
                            fontSize: '16px',
                            color: 'white'
                          }}
                        >
                          {isLoginLoading ? (
                            <>
                              <CSpinner size="sm" className="me-2" />
                              Signing in...
                            </>
                          ) : (
                            'Sign In'
                          )}
                        </CButton>
                      </CCol>
                      <CCol xs={6} className="text-right">
                        <CButton 
                          className="px-0"
                          onClick={() => setShowDemoUsers(!showDemoUsers)}
                          style={{
                            color: 'var(--harmoni-teal)',
                            textDecoration: 'none',
                            background: 'none',
                            border: 'none',
                            fontSize: '14px'
                          }}
                        >
                          Demo Users
                        </CButton>
                      </CCol>
                    </CRow>
                  </CForm>
                </CCardBody>
              </CCard>

              <CCard 
                className="text-white py-5" 
                style={{ 
                  width: '44%', 
                  background: 'linear-gradient(135deg, var(--harmoni-teal) 0%, var(--harmoni-blue) 100%)',
                  borderRadius: '12px'
                }}
              >
                <CCardBody className="text-center">
                  <div>
                    <h2 style={{ fontFamily: 'Poppins, sans-serif', fontWeight: '600' }}>Welcome to HarmoniHSE360</h2>
                    <p className="mb-4" style={{ fontSize: '16px', opacity: '0.9' }}>
                      360° Coverage for a Safer School Environment
                    </p>
                    <div className="mt-4">
                      <div className="feature-highlight mb-4">
                        <div className="d-flex align-items-center justify-content-center mb-2">
                          <div 
                            className="rounded-circle d-flex align-items-center justify-content-center me-3"
                            style={{ width: '40px', height: '40px', backgroundColor: 'rgba(255,255,255,0.2)' }}
                          >
                            🛡️
                          </div>
                          <h5 className="mb-0" style={{ fontFamily: 'Poppins, sans-serif' }}>Safety First</h5>
                        </div>
                        <p className="small" style={{ opacity: '0.8' }}>Complete incident reporting and management</p>
                      </div>
                      <div className="feature-highlight mb-4">
                        <div className="d-flex align-items-center justify-content-center mb-2">
                          <div 
                            className="rounded-circle d-flex align-items-center justify-content-center me-3"
                            style={{ width: '40px', height: '40px', backgroundColor: 'rgba(255,255,255,0.2)' }}
                          >
                            📊
                          </div>
                          <h5 className="mb-0" style={{ fontFamily: 'Poppins, sans-serif' }}>Real-time Analytics</h5>
                        </div>
                        <p className="small" style={{ opacity: '0.8' }}>Live dashboards and reporting</p>
                      </div>
                      <div className="feature-highlight">
                        <div className="d-flex align-items-center justify-content-center mb-2">
                          <div 
                            className="rounded-circle d-flex align-items-center justify-content-center me-3"
                            style={{ width: '40px', height: '40px', backgroundColor: 'rgba(255,255,255,0.2)' }}
                          >
                            🌿
                          </div>
                          <h5 className="mb-0" style={{ fontFamily: 'Poppins, sans-serif' }}>Environmental Care</h5>
                        </div>
                        <p className="small" style={{ opacity: '0.8' }}>Sustainable practices tracking</p>
                      </div>
                    </div>
                  </div>
                </CCardBody>
              </CCard>
            </CCardGroup>

            {/* Demo Users Section */}
            {showDemoUsers && demoUsers && (
              <CRow className="mt-4">
                <CCol>
                  <CCallout color="info">
                    <h5>Demo User Accounts</h5>
                    <p>Click on any user below to auto-fill login credentials:</p>
                    
                    <CAccordion>
                      {demoUsers.users.map((user: any, index: number) => (
                        <CAccordionItem key={index} itemKey={index}>
                          <CAccordionHeader>
                            <strong>{user.name}</strong> - {user.role}
                          </CAccordionHeader>
                          <CAccordionBody>
                            <div className="d-flex justify-content-between align-items-center">
                              <div>
                                <p className="mb-1"><strong>Email:</strong> {user.email}</p>
                                <p className="mb-1"><strong>Password:</strong> {user.password}</p>
                                <p className="mb-0"><strong>Role:</strong> {user.role}</p>
                              </div>
                              <CButton
                                color="primary"
                                size="sm"
                                onClick={() => fillDemoCredentials(user.email, user.password)}
                              >
                                Use Credentials
                              </CButton>
                            </div>
                          </CAccordionBody>
                        </CAccordionItem>
                      ))}
                    </CAccordion>
                  </CCallout>
                </CCol>
              </CRow>
            )}
          </CCol>
        </CRow>
      </CContainer>
    </div>
  );
};

export default Login;