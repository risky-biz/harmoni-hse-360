import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
  CButton,
  CCard,
  CCardBody,
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
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CBadge,
  CListGroup,
  CListGroupItem,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ACTION_ICONS, CONTEXT_ICONS } from '../../utils/iconMappings';

import { useAppDispatch, useAppSelector } from '../../store/hooks';
import {
  useLoginMutation,
  useGetDemoUsersQuery,
} from '../../features/auth/authApi';
import {
  loginStart,
  loginSuccess,
  loginFailure,
  selectAuth,
  clearError,
} from '../../features/auth/authSlice';
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
  const [showDemoModal, setShowDemoModal] = useState(false);

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
    setShowDemoModal(false);
  };

  const getRoleBadgeColor = (role: string): string => {
    const adminRoles = ['Super Admin', 'Developer', 'Admin'];
    const hseRoles = ['Incident Manager', 'Risk Manager', 'PPE Manager', 'Health Monitor', 'HSE Manager'];
    const securityRoles = ['Security Manager', 'Security Officer', 'Compliance Officer'];
    const specialistRoles = ['Safety Officer', 'Department Head', 'Hot Work Specialist', 'Confined Space Specialist', 'Electrical Supervisor', 'Special Work Specialist'];
    
    if (adminRoles.includes(role)) return 'danger';
    if (hseRoles.includes(role)) return 'warning';
    if (securityRoles.includes(role)) return 'info';
    if (specialistRoles.includes(role)) return 'primary';
    return 'secondary';
  };

  const handleClearError = () => {
    dispatch(clearError());
  };

  return (
    <div className="harmoni-login-container min-vh-100 d-flex align-items-center">
      <CContainer>
        <CRow className="justify-content-center">
          <CCol xs={12} sm={10} md={8} lg={6} xl={5}>
            <CCard className="harmoni-login-card shadow-lg border-0">
              <CCardBody className="harmoni-login-body">
                <CForm onSubmit={handleSubmit(onSubmit)}>
                  {/* Logo and Brand Section */}
                  <div className="text-center mb-4 mb-md-5">
                    <div className="mb-3">
                      <img
                        src="/Harmoni_360_Logo.png"
                        alt="Harmoni360 Logo"
                        className="harmoni-login-logo"
                      />
                    </div>
                    <h1 className="harmoni-login-title">
                      Welcome Back
                    </h1>
                    <p className="harmoni-login-subtitle">
                      Complete Safety. Seamless Harmony.
                    </p>
                  </div>

                  {/* Error Alert */}
                  {auth.error && (
                    <CAlert
                      color="danger"
                      dismissible
                      onClose={handleClearError}
                      className="harmoni-alert alert-danger d-flex align-items-center"
                    >
                      <FontAwesomeIcon 
                        icon={ACTION_ICONS.info} 
                        className="flex-shrink-0 me-2" 
                        aria-hidden="true"
                      />
                      <div>{auth.error}</div>
                    </CAlert>
                  )}

                  {/* Email Input */}
                  <div className="harmoni-form-group">
                    <label htmlFor="email" className="form-label visually-hidden">
                      Email Address
                    </label>
                    <CInputGroup className="harmoni-input-group mb-2">
                      <CInputGroupText>
                        <FontAwesomeIcon 
                          icon={CONTEXT_ICONS.user} 
                          aria-hidden="true"
                        />
                      </CInputGroupText>
                      <CFormInput
                        {...register('email')}
                        id="email"
                        type="email"
                        placeholder="Enter your email address"
                        autoComplete="username"
                        invalid={!!errors.email}
                        disabled={isLoginLoading}
                        aria-describedby={errors.email ? 'email-error' : undefined}
                      />
                    </CInputGroup>
                    {errors.email && (
                      <div 
                        id="email-error" 
                        className="harmoni-error-text"
                        role="alert"
                      >
                        {errors.email.message}
                      </div>
                    )}
                  </div>

                  {/* Password Input */}
                  <div className="harmoni-form-group">
                    <label htmlFor="password" className="form-label visually-hidden">
                      Password
                    </label>
                    <CInputGroup className="harmoni-input-group mb-2">
                      <CInputGroupText>
                        <FontAwesomeIcon 
                          icon={CONTEXT_ICONS.security} 
                          aria-hidden="true"
                        />
                      </CInputGroupText>
                      <CFormInput
                        {...register('password')}
                        id="password"
                        type="password"
                        placeholder="Enter your password"
                        autoComplete="current-password"
                        invalid={!!errors.password}
                        disabled={isLoginLoading}
                        aria-describedby={errors.password ? 'password-error' : undefined}
                      />
                    </CInputGroup>
                    {errors.password && (
                      <div 
                        id="password-error" 
                        className="harmoni-error-text"
                        role="alert"
                      >
                        {errors.password.message}
                      </div>
                    )}
                  </div>

                  {/* Remember Me Checkbox */}
                  <div className="harmoni-remember-me mb-4">
                    <CFormCheck
                      {...register('rememberMe')}
                      id="rememberMe"
                      label="Remember me for 30 days"
                      disabled={isLoginLoading}
                    />
                  </div>

                  {/* Action Buttons */}
                  <div className="d-grid gap-3">
                    <CButton
                      type="submit"
                      disabled={isLoginLoading}
                      className="harmoni-btn-primary"
                    >
                      {isLoginLoading ? (
                        <>
                          <CSpinner size="sm" className="me-2" aria-hidden="true" />
                          Signing in...
                        </>
                      ) : (
                        <>
                          <FontAwesomeIcon icon={CONTEXT_ICONS.security} className="me-2" aria-hidden="true" />
                          Sign In to Harmoni360
                        </>
                      )}
                    </CButton>
                    
                    <CButton
                      variant="outline"
                      onClick={() => setShowDemoModal(true)}
                      disabled={isLoginLoading}
                      className="harmoni-btn-outline"
                    >
                      <FontAwesomeIcon icon={CONTEXT_ICONS.user} className="me-2" aria-hidden="true" />
                      Try Demo Accounts
                    </CButton>
                  </div>
                </CForm>
              </CCardBody>
            </CCard>

            {/* Demo Users Modal */}
            <CModal
              visible={showDemoModal}
              onClose={() => setShowDemoModal(false)}
              size="lg"
              scrollable
              backdrop="static"
              className="harmoni-demo-modal"
            >
              <CModalHeader>
                <CModalTitle>
                  <FontAwesomeIcon icon={CONTEXT_ICONS.user} className="me-2" aria-hidden="true" />
                  Demo User Accounts
                </CModalTitle>
              </CModalHeader>
              <CModalBody>
                <p className="mb-4">
                  Select any demo account below to automatically fill in the login credentials.
                  These accounts demonstrate different user roles and permissions within Harmoni360.
                </p>
                
                {demoUsers && (
                  <CListGroup>
                    {/* Group users by category */}
                    {[
                      {
                        title: 'System Administration',
                        users: demoUsers.users.filter((user: any) => 
                          ['Super Admin', 'Developer', 'Admin'].includes(user.role)
                        )
                      },
                      {
                        title: 'HSE Management',
                        users: demoUsers.users.filter((user: any) => 
                          ['Incident Manager', 'Risk Manager', 'PPE Manager', 'Health Monitor', 'HSE Manager'].includes(user.role)
                        )
                      },
                      {
                        title: 'Security & Compliance',
                        users: demoUsers.users.filter((user: any) => 
                          ['Security Manager', 'Security Officer', 'Compliance Officer'].includes(user.role)
                        )
                      },
                      {
                        title: 'Work Permit Specialists',
                        users: demoUsers.users.filter((user: any) => 
                          ['Safety Officer', 'Department Head', 'Hot Work Specialist', 'Confined Space Specialist', 'Electrical Supervisor', 'Special Work Specialist'].includes(user.role)
                        )
                      },
                      {
                        title: 'General Users',
                        users: demoUsers.users.filter((user: any) => 
                          ['Reporter', 'Viewer'].includes(user.role) || user.email.includes('@bsj.sch.id')
                        )
                      }
                    ].map((category, categoryIndex) => (
                      category.users.length > 0 && (
                        <div key={categoryIndex} className="harmoni-demo-category">
                          <h6 className="category-title">
                            {category.title}
                          </h6>
                          {category.users.map((user: any, index: number) => (
                            <CListGroupItem
                              key={`${categoryIndex}-${index}`}
                              className="harmoni-demo-user p-0"
                            >
                              <button
                                type="button"
                                onClick={() => fillDemoCredentials(user.email, user.password)}
                                className="btn btn-link w-100 h-100 text-start p-3 text-decoration-none"
                                style={{
                                  backgroundColor: 'transparent',
                                  border: 'none',
                                  borderRadius: '0',
                                  color: 'inherit',
                                }}
                              >
                                <div className="d-flex justify-content-between align-items-center">
                                  <div className="user-info text-start">
                                    <div className="user-name">
                                      {user.name}
                                    </div>
                                    <div className="user-email">
                                      {user.email}
                                    </div>
                                  </div>
                                  <CBadge
                                    color={getRoleBadgeColor(user.role)}
                                    className="user-badge"
                                  >
                                    {user.role}
                                  </CBadge>
                                </div>
                              </button>
                            </CListGroupItem>
                          ))}
                        </div>
                      )
                    ))}
                  </CListGroup>
                )}
              </CModalBody>
              <CModalFooter>
                <CButton
                  color="secondary"
                  onClick={() => setShowDemoModal(false)}
                >
                  Close
                </CButton>
              </CModalFooter>
            </CModal>
          </CCol>
        </CRow>
      </CContainer>
    </div>
  );
};

export default Login;