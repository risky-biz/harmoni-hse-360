import React, { useState } from 'react';
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CButtonGroup,
  CFormInput,
  CFormSelect,
  CFormLabel,
  CInputGroup,
  CInputGroupText,
  CListGroup,
  CListGroupItem,
  CBadge,
  CProgress,
  CAlert,
  CCollapse,
  CAccordion,
  CAccordionItem,
  CAccordionHeader,
  CAccordionBody,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faMapMarkerAlt,
  faSearch,
  faFilter,
  faList,
  faChartBar,
  faExclamationTriangle,
  faEye,
  faRoute,
  faLayersAlt,
  faCalendarAlt,
  faBuilding,
  faUser,
} from '@fortawesome/free-solid-svg-icons';
import HazardMap from '../../components/hazards/HazardMap';
import { useGetHazardsQuery } from '../../features/hazards/hazardApi';
import type { HazardDto } from '../../types/hazard';

const HazardMapping: React.FC = () => {
  const [selectedHazard, setSelectedHazard] = useState<HazardDto | null>(null);
  const [showFilters, setShowFilters] = useState(false);
  const [showStats, setShowStats] = useState(true);
  const [viewMode, setViewMode] = useState<'map' | 'list' | 'both'>('both');
  
  // Filter parameters
  const [filters, setFilters] = useState({
    searchTerm: '',
    category: '',
    type: '',
    severity: '',
    status: '',
    riskLevel: '',
    location: '',
    department: '',
    dateFrom: '',
    dateTo: '',
  });

  const { data: hazardsData, isLoading, error } = useGetHazardsQuery({
    pageSize: 1000,
    includeRiskAssessments: true,
    ...filters,
  });

  const handleFilterChange = (key: string, value: string) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  };

  const clearFilters = () => {
    setFilters({
      searchTerm: '',
      category: '',
      type: '',
      severity: '',
      status: '',
      riskLevel: '',
      location: '',
      department: '',
      dateFrom: '',
      dateTo: '',
    });
  };

  const hazardsWithLocation = hazardsData?.hazards.filter(h => h.latitude && h.longitude) || [];
  const hazardsWithoutLocation = hazardsData?.hazards.filter(h => !h.latitude || !h.longitude) || [];

  // Calculate statistics
  const stats = {
    total: hazardsData?.hazards.length || 0,
    withLocation: hazardsWithLocation.length,
    withoutLocation: hazardsWithoutLocation.length,
    byRiskLevel: hazardsWithLocation.reduce((acc, hazard) => {
      const level = hazard.currentRiskLevel || 'Not Assessed';
      acc[level] = (acc[level] || 0) + 1;
      return acc;
    }, {} as Record<string, number>),
    byStatus: hazardsWithLocation.reduce((acc, hazard) => {
      acc[hazard.status] = (acc[hazard.status] || 0) + 1;
      return acc;
    }, {} as Record<string, number>),
  };

  const getRiskBadgeColor = (level: string) => {
    switch (level) {
      case 'Critical': return 'danger';
      case 'High': return 'warning';
      case 'Medium': return 'info';
      case 'Low': return 'success';
      case 'VeryLow': return 'secondary';
      default: return 'secondary';
    }
  };

  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case 'Identified': return 'warning';
      case 'UnderInvestigation': return 'info';
      case 'ActionTaken': return 'primary';
      case 'Resolved': return 'success';
      case 'Closed': return 'secondary';
      default: return 'secondary';
    }
  };

  return (
    <CRow>
      {/* Filters and Controls */}
      <CCol xs={12}>
        <CCard className="mb-3">
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <h5 className="mb-0">
              <FontAwesomeIcon icon={faMapMarkerAlt} className="me-2" />
              Hazard Location Mapping
            </h5>
            <div className="d-flex gap-2">
              <CButtonGroup size="sm">
                <CButton
                  color={viewMode === 'map' ? 'primary' : 'outline-primary'}
                  onClick={() => setViewMode('map')}
                >
                  Map Only
                </CButton>
                <CButton
                  color={viewMode === 'list' ? 'primary' : 'outline-primary'}
                  onClick={() => setViewMode('list')}
                >
                  List Only
                </CButton>
                <CButton
                  color={viewMode === 'both' ? 'primary' : 'outline-primary'}
                  onClick={() => setViewMode('both')}
                >
                  Both
                </CButton>
              </CButtonGroup>
              
              <CButton
                color="outline-primary"
                size="sm"
                onClick={() => setShowFilters(!showFilters)}
              >
                <FontAwesomeIcon icon={faFilter} className="me-1" />
                Filters
              </CButton>
              
              <CButton
                color="outline-info"
                size="sm"
                onClick={() => setShowStats(!showStats)}
              >
                <FontAwesomeIcon icon={faChartBar} className="me-1" />
                Statistics
              </CButton>
            </div>
          </CCardHeader>
          
          <CCollapse visible={showFilters}>
            <CCardBody className="border-top">
              <CRow>
                <CCol md={3}>
                  <CFormLabel>Search</CFormLabel>
                  <CInputGroup>
                    <CInputGroupText>
                      <FontAwesomeIcon icon={faSearch} />
                    </CInputGroupText>
                    <CFormInput
                      placeholder="Search hazards..."
                      value={filters.searchTerm}
                      onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                    />
                  </CInputGroup>
                </CCol>
                
                <CCol md={2}>
                  <CFormLabel>Category</CFormLabel>
                  <CFormSelect
                    value={filters.category}
                    onChange={(e) => handleFilterChange('category', e.target.value)}
                  >
                    <option value="">All Categories</option>
                    <option value="Physical">Physical</option>
                    <option value="Chemical">Chemical</option>
                    <option value="Biological">Biological</option>
                    <option value="Ergonomic">Ergonomic</option>
                    <option value="Psychosocial">Psychosocial</option>
                    <option value="Environmental">Environmental</option>
                  </CFormSelect>
                </CCol>
                
                <CCol md={2}>
                  <CFormLabel>Risk Level</CFormLabel>
                  <CFormSelect
                    value={filters.riskLevel}
                    onChange={(e) => handleFilterChange('riskLevel', e.target.value)}
                  >
                    <option value="">All Risk Levels</option>
                    <option value="Critical">Critical</option>
                    <option value="High">High</option>
                    <option value="Medium">Medium</option>
                    <option value="Low">Low</option>
                    <option value="VeryLow">Very Low</option>
                  </CFormSelect>
                </CCol>
                
                <CCol md={2}>
                  <CFormLabel>Status</CFormLabel>
                  <CFormSelect
                    value={filters.status}
                    onChange={(e) => handleFilterChange('status', e.target.value)}
                  >
                    <option value="">All Statuses</option>
                    <option value="Identified">Identified</option>
                    <option value="UnderInvestigation">Under Investigation</option>
                    <option value="ActionTaken">Action Taken</option>
                    <option value="Resolved">Resolved</option>
                    <option value="Closed">Closed</option>
                  </CFormSelect>
                </CCol>
                
                <CCol md={3}>
                  <CFormLabel>Location</CFormLabel>
                  <CFormInput
                    placeholder="Filter by location..."
                    value={filters.location}
                    onChange={(e) => handleFilterChange('location', e.target.value)}
                  />
                </CCol>
              </CRow>
              
              <CRow className="mt-3">
                <CCol md={3}>
                  <CFormLabel>Date From</CFormLabel>
                  <CFormInput
                    type="date"
                    value={filters.dateFrom}
                    onChange={(e) => handleFilterChange('dateFrom', e.target.value)}
                  />
                </CCol>
                
                <CCol md={3}>
                  <CFormLabel>Date To</CFormLabel>
                  <CFormInput
                    type="date"
                    value={filters.dateTo}
                    onChange={(e) => handleFilterChange('dateTo', e.target.value)}
                  />
                </CCol>
                
                <CCol md={3}>
                  <CFormLabel>Department</CFormLabel>
                  <CFormInput
                    placeholder="Filter by department..."
                    value={filters.department}
                    onChange={(e) => handleFilterChange('department', e.target.value)}
                  />
                </CCol>
                
                <CCol md={3} className="d-flex align-items-end">
                  <CButton color="secondary" onClick={clearFilters} className="w-100">
                    Clear Filters
                  </CButton>
                </CCol>
              </CRow>
            </CCardBody>
          </CCollapse>
        </CCard>
      </CCol>

      {/* Statistics Panel */}
      {showStats && (
        <CCol xs={12}>
          <CCard className="mb-3">
            <CCardHeader>
              <h6 className="mb-0">
                <FontAwesomeIcon icon={faChartBar} className="me-2" />
                Mapping Statistics
              </h6>
            </CCardHeader>
            <CCardBody>
              <CRow>
                <CCol md={3}>
                  <div className="border-start border-start-4 border-start-info py-1 px-3">
                    <div className="text-medium-emphasis small">Total Hazards</div>
                    <div className="fs-5 fw-semibold">{stats.total}</div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-start-4 border-start-success py-1 px-3">
                    <div className="text-medium-emphasis small">With Location</div>
                    <div className="fs-5 fw-semibold">{stats.withLocation}</div>
                    <div className="small">
                      {stats.total > 0 ? Math.round((stats.withLocation / stats.total) * 100) : 0}% coverage
                    </div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-start-4 border-start-warning py-1 px-3">
                    <div className="text-medium-emphasis small">Without Location</div>
                    <div className="fs-5 fw-semibold">{stats.withoutLocation}</div>
                  </div>
                </CCol>
                <CCol md={3}>
                  <div className="border-start border-start-4 border-start-primary py-1 px-3">
                    <div className="text-medium-emphasis small">Location Coverage</div>
                    <CProgress 
                      value={stats.total > 0 ? (stats.withLocation / stats.total) * 100 : 0} 
                      color="success" 
                      className="mt-1"
                    />
                  </div>
                </CCol>
              </CRow>
              
              <CRow className="mt-3">
                <CCol md={6}>
                  <h6>Risk Level Distribution</h6>
                  <div className="d-flex flex-wrap gap-2">
                    {Object.entries(stats.byRiskLevel).map(([level, count]) => (
                      <CBadge key={level} color={getRiskBadgeColor(level)} className="d-flex align-items-center gap-1">
                        {level}: {count}
                      </CBadge>
                    ))}
                  </div>
                </CCol>
                <CCol md={6}>
                  <h6>Status Distribution</h6>
                  <div className="d-flex flex-wrap gap-2">
                    {Object.entries(stats.byStatus).map(([status, count]) => (
                      <CBadge key={status} color={getStatusBadgeColor(status)} className="d-flex align-items-center gap-1">
                        {status}: {count}
                      </CBadge>
                    ))}
                  </div>
                </CCol>
              </CRow>
            </CCardBody>
          </CCard>
        </CCol>
      )}

      {/* Main Content */}
      {(viewMode === 'map' || viewMode === 'both') && (
        <CCol xs={viewMode === 'both' ? 8 : 12}>
          <HazardMap
            height="600px"
            showControls={true}
            selectedHazardId={selectedHazard?.id}
            onHazardSelect={setSelectedHazard}
            filterParams={filters}
          />
        </CCol>
      )}

      {(viewMode === 'list' || viewMode === 'both') && (
        <CCol xs={viewMode === 'both' ? 4 : 12}>
          <CCard style={{ height: '600px' }}>
            <CCardHeader>
              <h6 className="mb-0">
                <FontAwesomeIcon icon={faList} className="me-2" />
                Hazard List ({hazardsWithLocation.length})
              </h6>
            </CCardHeader>
            <CCardBody className="p-0" style={{ overflowY: 'auto' }}>
              {isLoading ? (
                <div className="text-center p-4">Loading hazards...</div>
              ) : error ? (
                <CAlert color="danger" className="m-3">
                  Error loading hazards. Please try again.
                </CAlert>
              ) : hazardsWithLocation.length === 0 ? (
                <div className="text-center p-4 text-muted">
                  No hazards with location data found
                </div>
              ) : (
                <CListGroup flush>
                  {hazardsWithLocation.map((hazard) => (
                    <CListGroupItem
                      key={hazard.id}
                      className={`cursor-pointer ${selectedHazard?.id === hazard.id ? 'bg-light' : ''}`}
                      onClick={() => setSelectedHazard(hazard)}
                    >
                      <div className="d-flex justify-content-between align-items-start">
                        <div className="flex-grow-1">
                          <h6 className="mb-1 fw-bold">{hazard.title}</h6>
                          <p className="mb-1 text-muted small">
                            {hazard.description.length > 80 
                              ? hazard.description.substring(0, 80) + '...' 
                              : hazard.description}
                          </p>
                          <div className="d-flex align-items-center gap-2 mb-1">
                            <CBadge color={getRiskBadgeColor(hazard.currentRiskLevel || 'Not Assessed')}>
                              {hazard.currentRiskLevel || 'Not Assessed'}
                            </CBadge>
                            <CBadge color="light" text="dark">
                              {hazard.category}
                            </CBadge>
                          </div>
                          <div className="small text-muted">
                            <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                            {hazard.location}
                          </div>
                          <div className="small text-muted">
                            <FontAwesomeIcon icon={faCalendarAlt} className="me-1" />
                            {new Date(hazard.identifiedDate).toLocaleDateString()}
                          </div>
                          <div className="small text-muted">
                            <FontAwesomeIcon icon={faUser} className="me-1" />
                            {hazard.reporterName}
                          </div>
                        </div>
                        <div className="d-flex flex-column gap-1">
                          <CButton
                            color="primary"
                            size="sm"
                            onClick={(e) => {
                              e.stopPropagation();
                              window.open(`/hazards/${hazard.id}`, '_blank');
                            }}
                          >
                            <FontAwesomeIcon icon={faEye} />
                          </CButton>
                        </div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              )}
            </CCardBody>
          </CCard>

          {/* Hazards without location */}
          {hazardsWithoutLocation.length > 0 && (
            <CCard className="mt-3">
              <CCardHeader>
                <h6 className="mb-0 text-warning">
                  <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                  Hazards Missing Location Data ({hazardsWithoutLocation.length})
                </h6>
              </CCardHeader>
              <CCardBody>
                <CAlert color="warning">
                  <p>The following hazards don't have location data and cannot be displayed on the map:</p>
                </CAlert>
                <CListGroup flush>
                  {hazardsWithoutLocation.slice(0, 5).map((hazard) => (
                    <CListGroupItem key={hazard.id} className="d-flex justify-content-between align-items-center">
                      <div>
                        <strong>{hazard.title}</strong>
                        <div className="small text-muted">{hazard.location}</div>
                      </div>
                      <CButton
                        color="primary"
                        size="sm"
                        onClick={() => window.open(`/hazards/${hazard.id}/edit`, '_blank')}
                      >
                        Add Location
                      </CButton>
                    </CListGroupItem>
                  ))}
                  {hazardsWithoutLocation.length > 5 && (
                    <CListGroupItem className="text-center">
                      <small className="text-muted">
                        ... and {hazardsWithoutLocation.length - 5} more
                      </small>
                    </CListGroupItem>
                  )}
                </CListGroup>
              </CCardBody>
            </CCard>
          )}
        </CCol>
      )}
    </CRow>
  );
};

export default HazardMapping;