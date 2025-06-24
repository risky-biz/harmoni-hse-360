# Harmoni360 HSSE Dashboard Module Specification

Based on the dashboard image provided, this document provides comprehensive technical requirements for the Harmoni360 HSSE Dashboard module.

---

## 1. ✨ **Visual Analysis**

The image shows a sophisticated HSSE dashboard with multiple components:

### Top Section - Data Grid
- Spreadsheet-style grid showing "Numbers of Hazard Per Month"
- Columns for different hazard categories (Biological, Chemical, Physical, Mechanical, Ergonomic, Psychosocial)
- Monthly breakdown with year-over-year comparison
- Color-coded cells (green/red) indicating performance trends
- Total calculations and summary statistics

### Dashboard Components
1. **Type of Hazard Chart (2023-2024)** - Horizontal bar chart
2. **Non Conformance Criteria Chart** - Vertical bar chart with multiple categories
3. **Incident Triangle Chart** - Pyramid visualization showing incident hierarchy
4. **Hazard Case Status Pie Chart** - Shows open vs closed cases (90%/10%)
5. **IFR Study Related Activities** - Multi-line graph showing trends
6. **SR Study Related Activities** - Line graph with multiple data series
7. **Loss Time Injury Care Rate** - Bar chart comparing years
8. **KPI Summary Tables** - Multiple tables showing year-over-year metrics

A layout mockup or low-fidelity wireframe is recommended in the next phase to visually define how these components fit together.

---

## 2. ✅ **Functional Requirements**

### 2.1 Key Performance Indicators (KPIs)

```typescript
// Unified incident metrics model for consistency
interface UnifiedIncidentMetrics {
  rate: number;
  frequency: number;
  severity: SeverityLevel;
  category: 'recordable' | 'lostTime' | 'nearMiss' | 'firstAid';
  trend: TrendData;
}

interface HSSEMetrics {
  // Primary KPIs
  totalRecordableIncidentRate: number; // TRIR
  lostTimeInjuryFrequency: number; // LTIF
  nearMissReportingRate: number;
  hazardIdentificationRate: number;
  
  // Secondary KPIs
  incidentClosureRate: number;
  correctiveActionCompletionRate: number;
  safetyTrainingComplianceRate: number;
  environmentalComplianceScore: number;
  
  // Unified incident tracking
  incidentMetrics: UnifiedIncidentMetrics[];
}
```

### 2.2 Chart Components Required
- **GridView Component**: Excel-like data grid with sorting, filtering, and export
- **BarChart Component**: Both horizontal and vertical configurations
- **PieChart Component**: For status distributions
- **LineChart Component**: Multi-series trending over time
- **PyramidChart Component**: For incident triangle visualization
- **HeatMap Component**: For hazard distribution by location/time

**Chart Library**: Recharts (primary) for consistency and optimal React integration

### 2.3 Filter and Drill-down Capabilities

```typescript
interface DashboardFilters {
  dateRange: {
    startDate: Date;
    endDate: Date;
    preset: 'MTD' | 'QTD' | 'YTD' | 'Custom';
  };
  departments: string[];
  locations: string[];
  hazardTypes: HazardType[];
  incidentSeverity: SeverityLevel[];
  comparisonMode: 'YoY' | 'MoM' | 'QoQ';
}
```

### 2.4 Real-time vs Historical Data

```typescript
interface DataRefreshStrategy {
  realtime: {
    metrics: ['incidentCounts', 'activeHazards', 'environmentalSensors'];
    method: 'websocket';
    interval: 'immediate';
  };
  nearRealtime: {
    metrics: ['complianceScores', 'trainingStatus'];
    method: 'polling';
    interval: 300; // 5 minutes
  };
  historical: {
    metrics: ['trends', 'comparativeAnalysis', 'reports'];
    method: 'scheduled';
    interval: 86400; // daily
  };
}
```

### 2.5 User Customization & Personalization

```typescript
interface DashboardPersonalization {
  savedLayouts: UserLayout[];
  favoriteKPIs: string[];
  customAlerts: AlertConfiguration[];
  defaultFilters: DashboardFilters;
  widgetPreferences: {
    [widgetId: string]: WidgetSettings;
  };
}
```

### 2.6 Mobile Support Features

```typescript
interface MobileDashboardFeatures {
  swipeNavigation: boolean;
  offlineMode: {
    enabled: boolean;
    cachedMetrics: string[];
    syncOnReconnect: boolean;
  };
  compactLayouts: {
    phone: DashboardLayout;
    tablet: DashboardLayout;
  };
  gestureControls: {
    pinchToZoom: boolean;
    pullToRefresh: boolean;
  };
}
```

---

## 3. ⚙️ **Technical Specifications**

### 3.1 Frontend Components (React + TypeScript)

```typescript
// Enhanced dashboard layout with error boundaries
interface DashboardLayoutProps {
  modules: DashboardModule[];
  layout: 'grid' | 'flow' | 'custom';
  refreshInterval?: number;
  errorBoundary?: DashboardErrorBoundary;
}

interface DashboardErrorBoundary {
  onChartLoadError: (error: Error) => ReactNode;
  onDataFetchError: (error: Error) => void;
  fallbackData?: CachedDashboardData;
  retryStrategy: {
    maxRetries: number;
    backoffMs: number;
  };
}

interface ChartComponentProps<T> {
  data: T[];
  config: ChartConfiguration;
  onDrillDown?: (dataPoint: T) => void;
  refreshInterval?: number;
  loading?: boolean;
  cacheStrategy?: MetricCacheStrategy;
}

interface KPICardProps {
  title: string;
  value: number | string;
  unit?: string;
  trend?: TrendIndicator;
  sparkline?: number[];
  onClick?: () => void;
  threshold?: KPIThreshold;
}
```

### 3.2 Backend API Endpoints

```csharp
[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : BaseController
{
    [HttpGet("overview")]
    public Task<IActionResult> GetDashboardOverview(
        [FromQuery] DashboardFilterDto filters);

    [HttpGet("hazards/monthly")]
    public Task<IActionResult> GetMonthlyHazardData(
        [FromQuery] DateRangeDto dateRange);

    [HttpGet("kpis")]
    public Task<IActionResult> GetKPIMetrics(
        [FromQuery] KPIFilterDto filters);

    [HttpGet("charts/{chartType}")]
    public Task<IActionResult> GetChartData(
        string chartType, 
        [FromQuery] ChartDataRequestDto request);
        
    [HttpPost("export")]
    public Task<IActionResult> ExportDashboard(
        [FromBody] ExportConfigurationDto config);
        
    [HttpPost("personalization")]
    public Task<IActionResult> SavePersonalization(
        [FromBody] DashboardPersonalizationDto personalization);
}
```

### 3.3 Database Schema & Performance Optimization

```sql
-- Dashboard Configuration Table
CREATE TABLE dashboard_configurations (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    layout_json JSONB NOT NULL,
    widgets JSONB NOT NULL,
    filters JSONB NOT NULL,
    personalization JSONB NOT NULL,
    created_at TIMESTAMP,
    updated_at TIMESTAMP
);

-- Dashboard Cache Table with TTL
CREATE TABLE dashboard_cache (
    cache_key VARCHAR(255) PRIMARY KEY,
    data_json JSONB NOT NULL,
    metric_type VARCHAR(50) NOT NULL,
    ttl_seconds INTEGER NOT NULL,
    expiry_time TIMESTAMP NOT NULL,
    created_at TIMESTAMP
);

-- Materialized Views with Aggregation Windows
CREATE MATERIALIZED VIEW mv_hazard_aggregates AS
WITH aggregation_windows AS (
    SELECT 
        '1_minute' as window, 
        DATE_TRUNC('minute', reported_date) as period
    FROM hazards
    WHERE reported_date >= NOW() - INTERVAL '7 days'
    UNION ALL
    SELECT 
        '1_hour' as window, 
        DATE_TRUNC('hour', reported_date) as period
    FROM hazards
    WHERE reported_date >= NOW() - INTERVAL '30 days'
    UNION ALL
    SELECT 
        '1_day' as window, 
        DATE_TRUNC('day', reported_date) as period
    FROM hazards
    WHERE reported_date >= NOW() - INTERVAL '1 year'
)
SELECT 
    window,
    period,
    hazard_type,
    location_id,
    COUNT(*) as hazard_count,
    SUM(CASE WHEN status = 'Open' THEN 1 ELSE 0 END) as open_count
FROM aggregation_windows
GROUP BY window, period, hazard_type, location_id;
```

### 3.4 Caching Strategy

```typescript
interface MetricCacheStrategy {
  metricType: 'realtime' | 'aggregate' | 'historical';
  ttl: number; // seconds
  refreshStrategy: 'push' | 'pull' | 'hybrid';
  invalidationTriggers: string[];
}

interface AggregationConfig {
  raw: { retention: '7d', granularity: '1m' };
  hourly: { retention: '30d', granularity: '1h' };
  daily: { retention: '1y', granularity: '1d' };
  monthly: { retention: '5y', granularity: '1M' };
}
```

### 3.5 Real-Time Updates with SignalR

```typescript
// Enhanced event types with data governance
type DashboardUpdateEvent =
  | { type: 'HAZARD_ADDED', payload: Hazard }
  | { type: 'KPI_UPDATED', payload: KPIMetric }
  | { type: 'CASE_STATUS_CHANGED', payload: IncidentCase }
  | { type: 'THRESHOLD_EXCEEDED', payload: ThresholdAlert };

interface SignalRConfiguration {
  hubUrl: string;
  reconnectInterval: number;
  maxReconnectAttempts: number;
  messageBufferSize: number;
}
```

### 3.6 Performance Requirements

```yaml
Performance Targets:
  First Contentful Paint: < 0.8s
  Time to Interactive: < 1.5s
  Initial Load: < 2s
  Chart Render: < 1s
  Export Generation: < 5s (10k records)
  API Response (P95): < 500ms
  
Resource Limits:
  Memory Usage: < 100MB
  WebSocket Connections: 1 per user
  Cache Hit Ratio: > 85%
  
Scalability:
  Concurrent Users: 100+
  Real-time Viewers: 50+
  Hazard Records: 1M+
  Incident Records: 100k+ annually
```

### 3.7 Integration Architecture

```csharp
// Clear integration service with existing modules
public interface IDashboardIntegrationService
{
    Task<DashboardData> AggregateFromModules(
        IIncidentModule incidents,
        IHazardModule hazards,
        IComplianceModule compliance,
        ITrainingModule training,
        INotificationModule notifications,
        DashboardFilters filters
    );
    
    Task<ExportResult> GenerateExport(
        DashboardData data,
        ExportConfiguration config
    );
}
```

---

## 4. 🚒 **HSSE-Specific Considerations**

### 4.1 Domain-Specific Metrics

```typescript
interface HSSEDashboardRequirements {
  healthMetrics: {
    occupationalIllnessRate: number;
    medicalTreatmentCases: number;
    healthSurveillanceCompliance: number;
    exposureMonitoring: ExposureData[];
  };
  
  safetyMetrics: {
    trir: number;
    ltif: number;
    nearMissRatio: number;
    unsafeActsObserved: number;
    safetyObservationCards: number;
    behaviorBasedSafetyScore: number;
  };
  
  securityMetrics: {
    securityIncidents: number;
    accessViolations: number;
    emergencyResponseTime: number;
    drillCompletionRate: number;
  };
  
  environmentMetrics: {
    environmentalIncidents: number;
    wasteReductionPercentage: number;
    energyConsumption: number;
    carbonFootprint: number;
    waterUsageEfficiency: number;
  };
}
```

### 4.2 Compliance & Regulatory Reporting

```csharp
public interface IComplianceDashboard
{
    Task<ComplianceSnapshot> GetRegulatoryComplianceStatus();
    Task<List<UpcomingRequirement>> GetUpcomingComplianceDeadlines();
    Task<AuditReadinessScore> CalculateAuditReadiness();
    Task<byte[]> GenerateRegulatoryReport(ReportType type, DateRange period);
    Task<ComplianceGapAnalysis> AnalyzeComplianceGaps();
}
```

### 4.3 Role-Based Access Control

```typescript
enum DashboardAccessLevel {
  EXECUTIVE = 'EXECUTIVE',     // Full organizational view
  MANAGER = 'MANAGER',         // Department/location view
  SUPERVISOR = 'SUPERVISOR',   // Team view
  EMPLOYEE = 'EMPLOYEE',       // Personal metrics only
  CONTRACTOR = 'CONTRACTOR',   // Limited project view
  AUDITOR = 'AUDITOR'         // Read-only compliance view
}

interface DashboardPermissions {
  accessLevel: DashboardAccessLevel;
  allowedMetrics: string[];
  dataScope: {
    departments?: string[];
    locations?: string[];
    teams?: string[];
    projects?: string[];
  };
  exportPermissions: {
    allowed: boolean;
    formats: ExportFormat[];
    dataClassification: string[];
  };
  drillDownDepth: number;
  sensitiveDataAccess: boolean;
}
```

### 4.4 Alerts & Thresholds

```typescript
interface KPIThreshold {
  metric: keyof HSSEMetrics;
  thresholdValue: number;
  operator: 'above' | 'below' | 'equals' | 'between';
  severity: 'info' | 'warning' | 'critical';
  notifyRoles: DashboardAccessLevel[];
  escalationChain: EscalationStep[];
}

interface DashboardAlertSystem {
  thresholdAlerts: KPIThreshold[];
  anomalyDetection: {
    enabled: boolean;
    sensitivity: 'low' | 'medium' | 'high';
    baselineWindow: number; // days
    mlModelType: 'statistical' | 'ml' | 'hybrid';
  };
  notificationChannels: {
    email: boolean;
    sms: boolean;
    inApp: boolean;
    teams: boolean;
  };
}
```

### 4.5 Data Governance & Audit Trail

```typescript
interface DashboardDataGovernance {
  dataClassification: 'public' | 'internal' | 'confidential' | 'restricted';
  retentionPolicy: {
    raw: RetentionRule;
    aggregated: RetentionRule;
    exported: RetentionRule;
  };
  anonymizationRules: AnonymizationConfig[];
  complianceFlags: string[]; // GDPR, HIPAA, ISO27001
  encryptionRequirements: EncryptionSpec;
}
```

```csharp
public class DashboardAuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserRole { get; set; }
    public string Action { get; set; } // View, Export, DrillDown, Modify
    public string Dashboard { get; set; }
    public string DataAccessed { get; set; }
    public string DataClassification { get; set; }
    public DateTime AccessTime { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public Dictionary<string, object> Filters { get; set; }
    public bool SensitiveDataAccessed { get; set; }
}
```

### 4.6 Export & Scheduling Configuration

```typescript
interface ExportConfiguration {
  formats: ('PDF' | 'Excel' | 'PowerBI' | 'CSV' | 'JSON')[];
  scheduling: {
    enabled: boolean;
    frequency: CronExpression;
    recipients: EmailRecipient[];
    filters: DashboardFilters;
    includeCommentary: boolean;
  };
  templates: {
    executive: ExportTemplate;
    operational: ExportTemplate;
    compliance: ExportTemplate;
    custom: Map<string, ExportTemplate>;
  };
  dataGovernance: {
    watermark: boolean;
    encryption: boolean;
    accessExpiry: number; // hours
  };
}
```

---

## 5. 🚀 **Implementation Plan**

### Phase 1: Core Dashboard Infrastructure (Sprint 1-2)
```typescript
// Module structure
Follow Harmoni360 Module Structure pattern
```

### Phase 2: Data Integration & Caching (Sprint 3)
- Implement CQRS handlers for dashboard queries
- Set up Redis caching with TTL strategies
- Create materialized views for performance
- Implement data aggregation services

### Phase 3: Real-time Updates & Visualization (Sprint 4)
- SignalR hub implementation
- Chart components with Recharts
- Interactive drill-down features
- Mobile-responsive layouts

### Phase 4: Mobile Optimization & Offline Support (Sprint 5)
- PWA capabilities
- Offline data caching
- Mobile-specific UI components
- Gesture controls implementation

### Phase 5: User Customization & Saved Views (Sprint 6)
- Personalization settings
- Custom dashboard builder
- Saved filter sets
- Widget preferences

### Phase 6: Advanced Exports & Scheduling (Sprint 7)
- Export template engine
- Scheduled report generation
- Multi-format support
- Email integration

### Phase 7: Performance Optimization & Testing (Sprint 8)
- Performance profiling
- Load testing
- Cache optimization
- Bundle size reduction

### Testing Strategy
```typescript
interface DashboardTestPlan {
  unitTests: {
    components: ['KPICard', 'ChartWrapper', 'FilterPanel', 'ErrorBoundary'];
    services: ['DashboardService', 'AggregationService', 'CacheService'];
    coverage: '>= 80%';
  };
  integrationTests: {
    scenarios: ['DataRefresh', 'FilterApplication', 'DrillDown', 'Export'];
    dataSets: ['Empty', 'Normal', 'HighVolume', 'EdgeCases'];
  };
  performanceTests: {
    concurrent: [10, 50, 100, 500];
    dataVolume: ['1K', '10K', '100K', '1M'];
    metrics: ['TTFB', 'FCP', 'TTI', 'MemoryUsage'];
  };
  e2eTests: {
    userJourneys: ['ViewDashboard', 'ApplyFilters', 'ExportReport', 'DrillDown'];
    devices: ['Desktop', 'Tablet', 'Mobile'];
  };
}
```

### Future Enhancements (Phase 2)
```typescript
interface DeferredFeatures {
  predictiveAnalytics: {
    timeline: "Q3 2024";
    features: ['RiskForecasting', 'TrendPrediction', 'AnomalyDetection'];
  };
  iotIntegration: {
    timeline: "Q4 2024";
    features: ['SensorData', 'RealTimeAlerts', 'EnvironmentalMonitoring'];
  };
  advancedML: {
    timeline: "Q1 2025";
    features: ['PatternRecognition', 'RootCauseAnalysis', 'OptimizationSuggestions'];
  };
  customDashboardBuilder: {
    timeline: "Q2 2025";
    features: ['DragDropWidgets', 'CustomCharts', 'AdvancedFormulas'];
  };
}
```

---

## 📋 **Success Metrics**

### Technical Metrics
- Page Load Time: < 2 seconds
- Real-time Update Latency: < 500ms
- Dashboard Availability: 99.9%
- Cache Hit Rate: > 85%
- Mobile Performance Score: > 90

### Business Metrics
- User Adoption Rate: > 80%
- Dashboard Usage Frequency: Daily
- Export Utilization: > 60% of users
- Custom View Creation: > 40% of users
- Mobile Usage: > 30% of sessions

### HSSE Impact Metrics
- Incident Reporting Time: -50%
- Hazard Identification: +30%
- Compliance Score: > 95%
- Decision Making Speed: +40%
- Preventable Incidents: -25%

---

This comprehensive dashboard specification empowers Harmoni360 users with real-time HSSE performance insights while maintaining enterprise-grade security, performance, and scalability. The phased implementation approach ensures early value delivery while building towards a fully-featured analytics platform.