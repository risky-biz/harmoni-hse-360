import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CSpinner,
  CAlert,
  CForm,
  CFormInput,
  CFormSelect,
  CFormTextarea,
  CFormLabel,
  CProgress,
  CProgressBar,
  CListGroup,
  CListGroupItem,
  CBadge,
  CCallout,
  CTable,
  CTableHead,
  CTableBody,
  CTableHeaderCell,
  CTableDataCell,
  CTableRow,
  CButtonGroup,
  CFormCheck,
  CInputGroup,
  CInputGroupText,
  CTooltip,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faClipboardCheck,
  faArrowLeft,
  faArrowRight,
  faExclamationTriangle,
  faExclamationCircle,
  faInfo,
  faCheckCircle,
  faTimesCircle,
  faQuestionCircle,
  faShieldAlt,
  faCalculator,
  faFileAlt,
  faSave,
  faEye,
  faLightbulb,
  faUsers,
  faCalendarAlt,
} from '@fortawesome/free-solid-svg-icons';
import { useGetHazardQuery } from '../../features/hazards/hazardApi';
import { 
  useCreateRiskAssessmentMutation,
  CreateRiskAssessmentRequest,
} from '../../features/risk-assessments/riskAssessmentApi';
import { 
  RISK_LEVEL_CONFIG,
  ASSESSMENT_TYPE_CONFIG,
  getRiskLevelBadge,
  getAssessmentTypeBadge,
  getRiskScoreBadge,
} from '../../utils/riskAssessmentUtils';

interface CreateRiskAssessmentProps {
  isReassessment?: boolean;
  existingAssessmentId?: number;
}

interface RiskAssessmentFormData {
  hazardId: number;
  type: string;
  assessorName: string;
  assessmentDate: string;
  probabilityScore: number;
  severityScore: number;
  potentialConsequences: string;
  existingControls: string;
  recommendedActions: string;
  additionalNotes: string;
  nextReviewDate: string;
}

const CreateRiskAssessment: React.FC<CreateRiskAssessmentProps> = ({
  isReassessment = false,
  existingAssessmentId,
}) => {
  const { hazardId } = useParams<{ hazardId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const [currentStep, setCurrentStep] = useState(1);
  const [formData, setFormData] = useState<RiskAssessmentFormData>({
    hazardId: Number(hazardId) || 0,
    type: 'General',
    assessorName: '',
    assessmentDate: new Date().toISOString().split('T')[0],
    probabilityScore: 1,
    severityScore: 1,
    potentialConsequences: '',
    existingControls: '',
    recommendedActions: '',
    additionalNotes: '',
    nextReviewDate: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Get hazard data for context
  const {
    data: hazard,
    isLoading: isLoadingHazard,
    error: hazardError,
  } = useGetHazardQuery({
    id: Number(hazardId),
    includeRiskAssessments: true,
  });

  // Risk assessment creation mutation
  const [createRiskAssessment, { isLoading: isCreating }] = useCreateRiskAssessmentMutation();

  // Initialize next review date when assessment date changes
  useEffect(() => {
    if (formData.assessmentDate) {
      const assessmentDate = new Date(formData.assessmentDate);
      const nextReviewDate = new Date(assessmentDate);
      
      // Default review period based on risk level
      const riskScore = formData.probabilityScore * formData.severityScore;
      if (riskScore >= 17) {
        nextReviewDate.setMonth(nextReviewDate.getMonth() + 3); // Critical: 3 months
      } else if (riskScore >= 10) {
        nextReviewDate.setMonth(nextReviewDate.getMonth() + 6); // High: 6 months
      } else if (riskScore >= 5) {
        nextReviewDate.setFullYear(nextReviewDate.getFullYear() + 1); // Medium: 1 year
      } else {
        nextReviewDate.setFullYear(nextReviewDate.getFullYear() + 2); // Low/Very Low: 2 years
      }
      
      setFormData(prev => ({
        ...prev,
        nextReviewDate: nextReviewDate.toISOString().split('T')[0],
      }));
    }
  }, [formData.assessmentDate, formData.probabilityScore, formData.severityScore]);

  const handleInputChange = (field: keyof RiskAssessmentFormData, value: string | number) => {
    setFormData(prev => ({
      ...prev,
      [field]: value,
    }));
    
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const validateStep = (step: number): boolean => {
    const newErrors: Record<string, string> = {};

    switch (step) {
      case 1: // Assessment Setup
        if (!formData.type) newErrors.type = 'Assessment type is required';
        if (!formData.assessorName.trim()) newErrors.assessorName = 'Assessor name is required';
        if (!formData.assessmentDate) {
          newErrors.assessmentDate = 'Assessment date is required';
        } else {
          const today = new Date().toISOString().split('T')[0];
          if (formData.assessmentDate < today) {
            newErrors.assessmentDate = 'Assessment date cannot be in the past';
          }
        }
        break;

      case 2: // Risk Analysis
        if (!formData.potentialConsequences.trim()) {
          newErrors.potentialConsequences = 'Potential consequences must be described';
        }
        break;

      case 3: // Risk Evaluation
        if (formData.probabilityScore < 1 || formData.probabilityScore > 5) {
          newErrors.probabilityScore = 'Probability score must be between 1 and 5';
        }
        if (formData.severityScore < 1 || formData.severityScore > 5) {
          newErrors.severityScore = 'Severity score must be between 1 and 5';
        }
        break;

      case 4: // Controls & Actions
        if (!formData.existingControls.trim()) {
          newErrors.existingControls = 'Existing controls must be documented';
        }
        if (formData.nextReviewDate) {
          const today = new Date().toISOString().split('T')[0];
          if (formData.nextReviewDate < today) {
            newErrors.nextReviewDate = 'Next review date cannot be in the past';
          }
        }
        break;
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleNext = () => {
    if (validateStep(currentStep)) {
      setCurrentStep(prev => Math.min(prev + 1, 5));
    }
  };

  const handlePrevious = () => {
    setCurrentStep(prev => Math.max(prev - 1, 1));
  };

  const handleSubmit = async () => {
    if (!validateStep(4)) return;

    setIsSubmitting(true);
    try {
      const createRequest: CreateRiskAssessmentRequest = {
        hazardId: formData.hazardId,
        type: formData.type,
        assessorName: formData.assessorName,
        assessmentDate: formData.assessmentDate + 'T00:00:00Z', // Ensure UTC format
        probabilityScore: formData.probabilityScore,
        severityScore: formData.severityScore,
        potentialConsequences: formData.potentialConsequences,
        existingControls: formData.existingControls,
        recommendedActions: formData.recommendedActions,
        additionalNotes: formData.additionalNotes,
        nextReviewDate: formData.nextReviewDate + 'T00:00:00Z', // Ensure UTC format
      };

      console.log('Submitting risk assessment:', createRequest);
      
      const result = await createRiskAssessment(createRequest).unwrap();
      
      navigate('/risk-assessments', {
        state: {
          message: `Risk assessment ${isReassessment ? 'updated' : 'created'} successfully!`
        }
      });
    } catch (error: any) {
      console.error('Failed to submit risk assessment:', error);
      
      // Handle specific API errors
      let errorMessage = 'Failed to save risk assessment. Please try again.';
      if (error?.data?.message) {
        errorMessage = error.data.message;
      } else if (error?.message) {
        errorMessage = error.message;
      }
      
      setErrors({ submit: errorMessage });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Calculate risk level based on scores
  const riskScore = formData.probabilityScore * formData.severityScore;
  const getRiskLevel = (score: number): string => {
    if (score >= 17) return 'Critical';
    if (score >= 10) return 'High';
    if (score >= 5) return 'Medium';
    if (score >= 2) return 'Low';
    return 'VeryLow';
  };

  const riskLevel = getRiskLevel(riskScore);
  const progressPercentage = (currentStep / 5) * 100;

  if (isLoadingHazard) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <CSpinner size="sm" className="text-primary" />
        <span className="ms-2">Loading hazard information...</span>
      </div>
    );
  }

  if (hazardError || !hazard) {
    return (
      <CAlert color="danger">
        Unable to load hazard information. Please try again.
        <div className="mt-3">
          <CButton color="primary" onClick={() => navigate('/hazards')}>
            <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
            Back to Hazards
          </CButton>
        </div>
      </CAlert>
    );
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="shadow-sm">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <div>
              <h4
                className="mb-0"
                style={{
                  color: 'var(--harmoni-charcoal)',
                  fontFamily: 'Poppins, sans-serif',
                }}
              >
                <FontAwesomeIcon
                  icon={faClipboardCheck}
                  size="lg"
                  className="me-2 text-primary"
                />
                {isReassessment ? 'Reassess Risk' : 'New Risk Assessment'}
              </h4>
              <small className="text-muted">
                Hazard: {hazard.title} (ID: {hazard.id})
              </small>
            </div>
            <div>
              <CButton
                color="light"
                onClick={() => navigate(`/hazards/${hazard.id}`)}
                className="me-2"
              >
                <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                Back to Hazard
              </CButton>
              <CButton
                color="secondary"
                variant="outline"
                onClick={() => navigate('/risk-assessments')}
              >
                <FontAwesomeIcon icon={faEye} className="me-2" />
                View All Assessments
              </CButton>
            </div>
          </CCardHeader>

          <CCardBody>
            {/* Progress Indicator */}
            <div className="mb-4">
              <div className="d-flex justify-content-between align-items-center mb-2">
                <span className="fw-semibold">Assessment Progress</span>
                <small className="text-muted">Step {currentStep} of 5</small>
              </div>
              <CProgress>
                <CProgressBar value={progressPercentage} color="primary" />
              </CProgress>
              <div className="d-flex justify-content-between mt-2 small text-muted">
                <span className={currentStep >= 1 ? 'text-primary fw-semibold' : ''}>Setup</span>
                <span className={currentStep >= 2 ? 'text-primary fw-semibold' : ''}>Analysis</span>
                <span className={currentStep >= 3 ? 'text-primary fw-semibold' : ''}>Evaluation</span>
                <span className={currentStep >= 4 ? 'text-primary fw-semibold' : ''}>Controls</span>
                <span className={currentStep >= 5 ? 'text-primary fw-semibold' : ''}>Review</span>
              </div>
            </div>

            {/* Error Alert */}
            {errors.submit && (
              <CAlert color="danger" className="mb-4">
                {errors.submit}
              </CAlert>
            )}

            <CRow>
              {/* Left Column - Form Steps */}
              <CCol lg={8}>
                <CForm>
                  {/* Step 1: Assessment Setup */}
                  {currentStep === 1 && (
                    <div>
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faInfo} className="me-2 text-info" />
                        Assessment Setup
                      </h5>
                      
                      {/* Hazard Context */}
                      <CCallout color="info" className="mb-4">
                        <h6>Related Hazard Information</h6>
                        <div className="row">
                          <div className="col-sm-6">
                            <strong>Title:</strong> {hazard.title}<br />
                            <strong>Category:</strong> {hazard.category}<br />
                            <strong>Type:</strong> {hazard.type}
                          </div>
                          <div className="col-sm-6">
                            <strong>Location:</strong> {hazard.location}<br />
                            <strong>Status:</strong> {hazard.status}<br />
                            <strong>Severity:</strong> {hazard.severity}
                          </div>
                        </div>
                        <div className="mt-2">
                          <strong>Description:</strong> {hazard.description}
                        </div>
                      </CCallout>

                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="assessmentType">
                              Assessment Type <span className="text-danger">*</span>
                            </CFormLabel>
                            <CFormSelect
                              id="assessmentType"
                              value={formData.type}
                              onChange={(e) => handleInputChange('type', e.target.value)}
                              invalid={!!errors.type}
                            >
                              {Object.entries(ASSESSMENT_TYPE_CONFIG).map(([key, config]) => (
                                <option key={key} value={key}>
                                  {config.label} - {config.description}
                                </option>
                              ))}
                            </CFormSelect>
                            {errors.type && <div className="text-danger small">{errors.type}</div>}
                          </div>
                        </CCol>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="assessorName">
                              Assessor Name <span className="text-danger">*</span>
                            </CFormLabel>
                            <CFormInput
                              id="assessorName"
                              type="text"
                              value={formData.assessorName}
                              onChange={(e) => handleInputChange('assessorName', e.target.value)}
                              placeholder="Enter your full name"
                              invalid={!!errors.assessorName}
                            />
                            {errors.assessorName && <div className="text-danger small">{errors.assessorName}</div>}
                          </div>
                        </CCol>
                      </CRow>

                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel htmlFor="assessmentDate">
                              Assessment Date <span className="text-danger">*</span>
                            </CFormLabel>
                            <CFormInput
                              id="assessmentDate"
                              type="date"
                              value={formData.assessmentDate}
                              onChange={(e) => handleInputChange('assessmentDate', e.target.value)}
                              min={new Date().toISOString().split('T')[0]}
                              invalid={!!errors.assessmentDate}
                            />
                            {errors.assessmentDate && <div className="text-danger small">{errors.assessmentDate}</div>}
                            <div className="form-text">Only today and future dates are allowed</div>
                          </div>
                        </CCol>
                      </CRow>
                    </div>
                  )}

                  {/* Step 2: Risk Analysis */}
                  {currentStep === 2 && (
                    <div>
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2 text-warning" />
                        Risk Analysis
                      </h5>

                      <CCallout color="warning" className="mb-4">
                        <h6>Risk Analysis Guidelines</h6>
                        <p className="mb-2">
                          Consider all potential consequences that could arise from this hazard. Think about:
                        </p>
                        <ul className="mb-0">
                          <li>Health and safety impacts on personnel</li>
                          <li>Environmental consequences</li>
                          <li>Operational disruptions</li>
                          <li>Financial and reputational impacts</li>
                          <li>Regulatory and compliance issues</li>
                        </ul>
                      </CCallout>

                      <div className="mb-4">
                        <CFormLabel htmlFor="potentialConsequences">
                          Potential Consequences <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormTextarea
                          id="potentialConsequences"
                          rows={6}
                          value={formData.potentialConsequences}
                          onChange={(e) => handleInputChange('potentialConsequences', e.target.value)}
                          placeholder="Describe in detail what could happen if this hazard materializes. Include immediate and long-term consequences..."
                          invalid={!!errors.potentialConsequences}
                        />
                        {errors.potentialConsequences && (
                          <div className="text-danger small">{errors.potentialConsequences}</div>
                        )}
                        <div className="form-text">
                          Be specific and comprehensive. This analysis forms the foundation of your risk assessment.
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Step 3: Risk Evaluation */}
                  {currentStep === 3 && (
                    <div>
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faCalculator} className="me-2 text-primary" />
                        Risk Evaluation (5×5 Matrix)
                      </h5>

                      <CCallout color="info" className="mb-4">
                        <h6>Risk Matrix Methodology</h6>
                        <p>
                          Rate both the <strong>Probability</strong> (likelihood of occurrence) and 
                          <strong> Severity</strong> (magnitude of consequences) on a scale of 1-5.
                          The risk score is calculated as: <strong>Probability × Severity = Risk Score</strong>
                        </p>
                      </CCallout>

                      <CRow>
                        <CCol md={6}>
                          <div className="mb-4">
                            <CFormLabel htmlFor="probabilityScore">
                              Probability Score <span className="text-danger">*</span>
                            </CFormLabel>
                            <CFormSelect
                              id="probabilityScore"
                              value={formData.probabilityScore}
                              onChange={(e) => handleInputChange('probabilityScore', Number(e.target.value))}
                              invalid={!!errors.probabilityScore}
                            >
                              <option value={1}>1 - Very Low (Rare)</option>
                              <option value={2}>2 - Low (Unlikely)</option>
                              <option value={3}>3 - Medium (Possible)</option>
                              <option value={4}>4 - High (Likely)</option>
                              <option value={5}>5 - Very High (Almost Certain)</option>
                            </CFormSelect>
                            {errors.probabilityScore && (
                              <div className="text-danger small">{errors.probabilityScore}</div>
                            )}
                          </div>
                        </CCol>
                        <CCol md={6}>
                          <div className="mb-4">
                            <CFormLabel htmlFor="severityScore">
                              Severity Score <span className="text-danger">*</span>
                            </CFormLabel>
                            <CFormSelect
                              id="severityScore"
                              value={formData.severityScore}
                              onChange={(e) => handleInputChange('severityScore', Number(e.target.value))}
                              invalid={!!errors.severityScore}
                            >
                              <option value={1}>1 - Very Low (Negligible)</option>
                              <option value={2}>2 - Low (Minor)</option>
                              <option value={3}>3 - Medium (Moderate)</option>
                              <option value={4}>4 - High (Major)</option>
                              <option value={5}>5 - Very High (Catastrophic)</option>
                            </CFormSelect>
                            {errors.severityScore && (
                              <div className="text-danger small">{errors.severityScore}</div>
                            )}
                          </div>
                        </CCol>
                      </CRow>

                      {/* Risk Score Visualization */}
                      <div className="mb-4">
                        <h6>Calculated Risk Assessment</h6>
                        <div className="d-flex align-items-center gap-3 mb-3">
                          <div className="text-center">
                            <div className="fw-bold">Probability</div>
                            <CBadge color="info" className="fs-6">{formData.probabilityScore}</CBadge>
                          </div>
                          <div className="text-center">
                            <FontAwesomeIcon icon={faTimesCircle} className="text-muted" />
                          </div>
                          <div className="text-center">
                            <div className="fw-bold">Severity</div>
                            <CBadge color="warning" className="fs-6">{formData.severityScore}</CBadge>
                          </div>
                          <div className="text-center">
                            <div className="fw-bold">=</div>
                          </div>
                          <div className="text-center">
                            <div className="fw-bold">Risk Score</div>
                            {getRiskScoreBadge(riskScore)}
                          </div>
                          <div className="text-center">
                            <div className="fw-bold">Risk Level</div>
                            {getRiskLevelBadge(riskLevel)}
                          </div>
                        </div>

                        {/* Risk Level Description */}
                        <CCallout color={RISK_LEVEL_CONFIG[riskLevel as keyof typeof RISK_LEVEL_CONFIG]?.color || 'secondary'}>
                          <strong>{RISK_LEVEL_CONFIG[riskLevel as keyof typeof RISK_LEVEL_CONFIG]?.description}</strong>
                          {riskLevel === 'Critical' && (
                            <div className="mt-2">
                              <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                              Immediate action required. Review period: 3 months.
                            </div>
                          )}
                          {riskLevel === 'High' && (
                            <div className="mt-2">
                              <FontAwesomeIcon icon={faExclamationCircle} className="me-2" />
                              Priority attention needed. Review period: 6 months.
                            </div>
                          )}
                        </CCallout>
                      </div>
                    </div>
                  )}

                  {/* Step 4: Controls & Actions */}
                  {currentStep === 4 && (
                    <div>
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faShieldAlt} className="me-2 text-success" />
                        Existing Controls & Recommended Actions
                      </h5>

                      <div className="mb-4">
                        <CFormLabel htmlFor="existingControls">
                          Existing Controls <span className="text-danger">*</span>
                        </CFormLabel>
                        <CFormTextarea
                          id="existingControls"
                          rows={4}
                          value={formData.existingControls}
                          onChange={(e) => handleInputChange('existingControls', e.target.value)}
                          placeholder="List and describe all current control measures in place to manage this hazard..."
                          invalid={!!errors.existingControls}
                        />
                        {errors.existingControls && (
                          <div className="text-danger small">{errors.existingControls}</div>
                        )}
                      </div>

                      <div className="mb-4">
                        <CFormLabel htmlFor="recommendedActions">
                          Recommended Actions
                        </CFormLabel>
                        <CFormTextarea
                          id="recommendedActions"
                          rows={4}
                          value={formData.recommendedActions}
                          onChange={(e) => handleInputChange('recommendedActions', e.target.value)}
                          placeholder="Recommend additional control measures, improvements, or actions to reduce the risk level..."
                        />
                        <div className="form-text">
                          Consider the hierarchy of controls: Elimination, Substitution, Engineering Controls, 
                          Administrative Controls, Personal Protective Equipment.
                        </div>
                      </div>

                      <div className="mb-4">
                        <CFormLabel htmlFor="additionalNotes">
                          Additional Notes
                        </CFormLabel>
                        <CFormTextarea
                          id="additionalNotes"
                          rows={3}
                          value={formData.additionalNotes}
                          onChange={(e) => handleInputChange('additionalNotes', e.target.value)}
                          placeholder="Any additional observations, assumptions, or important information..."
                        />
                      </div>

                      <div className="mb-4">
                        <CFormLabel htmlFor="nextReviewDate">
                          Next Review Date
                        </CFormLabel>
                        <CFormInput
                          id="nextReviewDate"
                          type="date"
                          value={formData.nextReviewDate}
                          onChange={(e) => handleInputChange('nextReviewDate', e.target.value)}
                          min={new Date().toISOString().split('T')[0]}
                          invalid={!!errors.nextReviewDate}
                        />
                        {errors.nextReviewDate && <div className="text-danger small">{errors.nextReviewDate}</div>}
                        <div className="form-text">
                          Review date automatically calculated based on risk level. Adjust if needed. Only today and future dates are allowed.
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Step 5: Review & Submit */}
                  {currentStep === 5 && (
                    <div>
                      <h5 className="mb-3">
                        <FontAwesomeIcon icon={faCheckCircle} className="me-2 text-success" />
                        Review & Submit
                      </h5>

                      <CCallout color="success" className="mb-4">
                        <h6>Assessment Summary</h6>
                        <p>Please review your risk assessment before submitting. Once submitted, 
                        this assessment will be saved and can be used for hazard management decisions.</p>
                      </CCallout>

                      {/* Assessment Summary */}
                      <CCard className="border-0 bg-light mb-4">
                        <CCardBody>
                          <h6 className="mb-3">Risk Assessment Summary</h6>
                          <CRow>
                            <CCol sm={6}>
                              <table className="table table-borderless table-sm">
                                <tbody>
                                  <tr>
                                    <td className="fw-semibold">Hazard:</td>
                                    <td>{hazard.title}</td>
                                  </tr>
                                  <tr>
                                    <td className="fw-semibold">Assessment Type:</td>
                                    <td>{getAssessmentTypeBadge(formData.type)}</td>
                                  </tr>
                                  <tr>
                                    <td className="fw-semibold">Assessor:</td>
                                    <td>{formData.assessorName}</td>
                                  </tr>
                                  <tr>
                                    <td className="fw-semibold">Assessment Date:</td>
                                    <td>{new Date(formData.assessmentDate).toLocaleDateString()}</td>
                                  </tr>
                                </tbody>
                              </table>
                            </CCol>
                            <CCol sm={6}>
                              <table className="table table-borderless table-sm">
                                <tbody>
                                  <tr>
                                    <td className="fw-semibold">Risk Score:</td>
                                    <td>{getRiskScoreBadge(riskScore)} ({formData.probabilityScore} × {formData.severityScore})</td>
                                  </tr>
                                  <tr>
                                    <td className="fw-semibold">Risk Level:</td>
                                    <td>{getRiskLevelBadge(riskLevel)}</td>
                                  </tr>
                                  <tr>
                                    <td className="fw-semibold">Next Review:</td>
                                    <td>{new Date(formData.nextReviewDate).toLocaleDateString()}</td>
                                  </tr>
                                </tbody>
                              </table>
                            </CCol>
                          </CRow>
                        </CCardBody>
                      </CCard>

                      {/* Key Information Validation */}
                      <div className="mb-4">
                        <h6>Validation Checklist</h6>
                        <CListGroup>
                          <CListGroupItem className="d-flex justify-content-between align-items-center">
                            <span>Potential consequences documented</span>
                            <FontAwesomeIcon 
                              icon={formData.potentialConsequences.trim().length > 20 ? faCheckCircle : faTimesCircle}
                              color={formData.potentialConsequences.trim().length > 20 ? 'green' : 'red'}
                            />
                          </CListGroupItem>
                          <CListGroupItem className="d-flex justify-content-between align-items-center">
                            <span>Existing controls documented</span>
                            <FontAwesomeIcon 
                              icon={formData.existingControls.trim().length > 0 ? faCheckCircle : faTimesCircle}
                              color={formData.existingControls.trim().length > 0 ? 'green' : 'red'}
                            />
                          </CListGroupItem>
                          <CListGroupItem className="d-flex justify-content-between align-items-center">
                            <span>Risk scores assigned</span>
                            <FontAwesomeIcon 
                              icon={faCheckCircle}
                              color="green"
                            />
                          </CListGroupItem>
                          <CListGroupItem className="d-flex justify-content-between align-items-center">
                            <span>Review date set</span>
                            <FontAwesomeIcon 
                              icon={formData.nextReviewDate ? faCheckCircle : faTimesCircle}
                              color={formData.nextReviewDate ? 'green' : 'red'}
                            />
                          </CListGroupItem>
                        </CListGroup>
                      </div>
                    </div>
                  )}
                </CForm>
              </CCol>

              {/* Right Column - Navigation & Context */}
              <CCol lg={4}>
                <div className="border-start ps-4">
                  {/* Current Step Info */}
                  <div className="mb-4">
                    <h6 className="text-muted">Current Step</h6>
                    <div className="mb-2">
                      {currentStep === 1 && 'Assessment Setup'}
                      {currentStep === 2 && 'Risk Analysis'}
                      {currentStep === 3 && 'Risk Evaluation'}
                      {currentStep === 4 && 'Controls & Actions'}
                      {currentStep === 5 && 'Review & Submit'}
                    </div>
                    <small className="text-muted">
                      {currentStep === 1 && 'Configure assessment parameters and verify hazard information.'}
                      {currentStep === 2 && 'Analyze potential consequences and impacts of the hazard.'}
                      {currentStep === 3 && 'Evaluate probability and severity using the 5×5 risk matrix.'}
                      {currentStep === 4 && 'Document existing controls and recommend additional actions.'}
                      {currentStep === 5 && 'Review your assessment and submit for approval.'}
                    </small>
                  </div>

                  {/* Quick Risk Reference */}
                  {currentStep === 3 && (
                    <div className="mb-4">
                      <h6 className="text-muted">Quick Reference</h6>
                      <div className="small">
                        <div className="mb-2">
                          <strong>Probability Guidelines:</strong>
                          <div>1 = Rare (&lt;1%)</div>
                          <div>2 = Unlikely (1-10%)</div>
                          <div>3 = Possible (10-50%)</div>
                          <div>4 = Likely (50-90%)</div>
                          <div>5 = Almost Certain (&gt;90%)</div>
                        </div>
                        <div>
                          <strong>Severity Guidelines:</strong>
                          <div>1 = Negligible</div>
                          <div>2 = Minor</div>
                          <div>3 = Moderate</div>
                          <div>4 = Major</div>
                          <div>5 = Catastrophic</div>
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Current Risk Level Display */}
                  {(currentStep >= 3) && (
                    <div className="mb-4">
                      <h6 className="text-muted">Current Assessment</h6>
                      <div className="text-center p-3 bg-light rounded">
                        <div className="mb-2">{getRiskScoreBadge(riskScore)}</div>
                        <div className="mb-2">{getRiskLevelBadge(riskLevel)}</div>
                        <small className="text-muted">
                          {formData.probabilityScore} × {formData.severityScore} = {riskScore}
                        </small>
                      </div>
                    </div>
                  )}

                  {/* Navigation Buttons */}
                  <div className="d-grid gap-2">
                    {currentStep > 1 && (
                      <CButton
                        color="secondary"
                        variant="outline"
                        onClick={handlePrevious}
                      >
                        <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                        Previous
                      </CButton>
                    )}
                    
                    {currentStep < 5 && (
                      <CButton
                        color="primary"
                        onClick={handleNext}
                      >
                        Next
                        <FontAwesomeIcon icon={faArrowRight} className="ms-2" />
                      </CButton>
                    )}
                    
                    {currentStep === 5 && (
                      <CButton
                        color="success"
                        onClick={handleSubmit}
                        disabled={isSubmitting || isCreating}
                      >
                        {(isSubmitting || isCreating) ? (
                          <>
                            <CSpinner size="sm" className="me-2" />
                            Submitting...
                          </>
                        ) : (
                          <>
                            <FontAwesomeIcon icon={faSave} className="me-2" />
                            Submit Assessment
                          </>
                        )}
                      </CButton>
                    )}
                  </div>
                </div>
              </CCol>
            </CRow>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  );
};

export default CreateRiskAssessment;