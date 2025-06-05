# HarmoniHSE360 Dashboard Pattern Guide

## Overview

This guide documents the standardized dashboard architecture and patterns used in HarmoniHSE360. The Incident Management Dashboard serves as the reference implementation for all module-specific dashboards.

## Dashboard Architecture

### Component Structure

```
/pages/[module]/[Module]Dashboard.tsx    # Main dashboard page
/components/dashboard/                     # Reusable dashboard components
  ├── StatsCard.tsx                       # KPI display cards
  ├── ChartCard.tsx                       # Chart container
  ├── DonutChart.tsx                      # Donut/pie charts
  ├── BarChart.tsx                        # Bar/column charts
  ├── LineChart.tsx                       # Line/area charts
  ├── ProgressCard.tsx                    # Progress indicators
  ├── RecentItemsList.tsx                 # Recent items display
  └── index.ts                            # Component exports
```

### Data Flow

1. **API Layer**: RTK Query for data fetching with caching
2. **Real-time Updates**: SignalR integration for live data
3. **State Management**: Redux for global state
4. **Auto-refresh**: Configurable intervals for data updates

## Standard Dashboard Sections

### 1. Header Section
```tsx
<div className="dashboard-header">
  <h1>Module Dashboard</h1>
  <p>Dashboard description</p>
  <CButton>Primary Action</CButton>
</div>
```

### 2. Filter Controls
```tsx
<CCard className="dashboard-filters">
  <CCardBody>
    {/* Time range filters */}
    {/* Department/category filters */}
    {/* Refresh controls */}
    {/* Last update timestamp */}
  </CCardBody>
</CCard>
```

### 3. Key Metrics Row
```tsx
<CRow className="stats-row">
  <CCol lg={3} md={6}>
    <StatsCard
      title="Metric Title"
      value={metricValue}
      icon={iconName}
      color="primary"
      trend={trendData}
      onClick={navigationHandler}
    />
  </CCol>
  {/* 3-4 additional metrics */}
</CRow>
```

### 4. Charts Section
```tsx
<CRow>
  <CCol lg={4}>
    <ChartCard title="Distribution">
      <DonutChart data={chartData} />
    </ChartCard>
  </CCol>
  <CCol lg={4}>
    <ChartCard title="Analysis">
      <BarChart data={chartData} />
    </ChartCard>
  </CCol>
  <CCol lg={4}>
    <ChartCard title="Trends">
      <LineChart data={chartData} />
    </ChartCard>
  </CCol>
</CRow>
```

### 5. Performance Metrics
```tsx
<CRow>
  <CCol lg={3} md={6}>
    <ProgressCard
      title="SLA Compliance"
      value={compliantCount}
      total={totalCount}
      percentage={percentage}
      color="success"
    />
  </CCol>
  {/* Additional progress metrics */}
</CRow>
```

### 6. Detailed Analytics
```tsx
<CRow>
  <CCol lg={6}>
    <ChartCard title="Category Breakdown">
      <BarChart data={categoryData} />
    </ChartCard>
  </CCol>
  <CCol lg={6}>
    <ChartCard title="Department Performance">
      {/* Custom department visualization */}
    </ChartCard>
  </CCol>
</CRow>
```

### 7. Recent Items & Quick Actions
```tsx
<CRow>
  <CCol lg={8}>
    <RecentItemsList
      title="Recent Items"
      items={recentItems}
      maxItems={8}
      showAllLink={viewAllLink}
    />
  </CCol>
  <CCol lg={4}>
    <CCard>
      <CCardHeader>Quick Actions</CCardHeader>
      <CCardBody>
        {/* Action buttons */}
        {/* Summary stats */}
      </CCardBody>
    </CCard>
  </CCol>
</CRow>
```

## API Integration Pattern

### 1. Dashboard Query
```typescript
// incidentApi.ts
getIncidentDashboard: builder.query<DashboardDto, DashboardParams>({
  query: (params) => ({
    url: `/dashboard`,
    params,
  }),
  providesTags: ['IncidentDashboard', 'IncidentStatistics'],
})
```

### 2. Dashboard DTO Structure
```csharp
public class DashboardDto
{
    public OverallStatsDto OverallStats { get; set; }
    public List<StatusStatsDto> StatusStats { get; set; }
    public List<TrendDataDto> TrendData { get; set; }
    public List<CategoryStatsDto> CategoryStats { get; set; }
    public List<RecentItemDto> RecentItems { get; set; }
    public PerformanceMetricsDto PerformanceMetrics { get; set; }
}
```

### 3. Query Handler Pattern
```csharp
public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    // Aggregate data from multiple sources
    // Calculate statistics and metrics
    // Apply filters and date ranges
    // Return comprehensive dashboard data
}
```

## Real-time Updates

### 1. SignalR Integration
```typescript
// signalrService.ts
hubConnection.on('DashboardUpdate', () => {
  store.dispatch(
    moduleApi.util.invalidateTags(['Dashboard', 'Statistics'])
  );
});
```

### 2. Backend Notifications
```csharp
// Controller actions
await _hub.Clients.All.SendAsync("DashboardUpdate");
```

### 3. Auto-refresh Implementation
```typescript
const [autoRefreshInterval, setAutoRefreshInterval] = useState(0);

useEffect(() => {
  if (autoRefreshInterval > 0) {
    const interval = setInterval(() => {
      refetch();
    }, autoRefreshInterval * 1000);
    return () => clearInterval(interval);
  }
}, [autoRefreshInterval, refetch]);
```

## Mobile Responsiveness

### 1. Responsive Grid System
- Use CoreUI's responsive grid with proper breakpoints
- Stack elements vertically on mobile devices
- Adjust chart heights for smaller screens

### 2. Touch-Friendly Interactions
- Larger touch targets for mobile
- Simplified navigation on small screens
- Collapsible filters for more space

### 3. Responsive Text
- Truncate long text on mobile
- Use abbreviations for small screens
- Hide non-essential elements

### 4. SCSS Patterns
```scss
// Breakpoints
$mobile: 576px;
$tablet: 768px;
$desktop: 992px;

// Responsive utilities
.hide-mobile {
  @media (max-width: $mobile) {
    display: none !important;
  }
}

// Touch-friendly buttons
@media (hover: none) and (pointer: coarse) {
  .btn-sm {
    padding: 0.5rem 1rem;
  }
}
```

## Performance Considerations

### 1. Data Caching
- Use RTK Query caching with appropriate tags
- Invalidate specific cache entries on updates
- Implement optimistic updates where appropriate

### 2. Lazy Loading
- Load charts only when visible
- Paginate large data sets
- Use virtual scrolling for long lists

### 3. Bundle Optimization
- Import only required chart components
- Use dynamic imports for heavy components
- Minimize re-renders with proper memoization

## Testing Strategy

### 1. Unit Tests
```typescript
describe('IncidentDashboard', () => {
  it('displays loading state', () => {});
  it('renders statistics correctly', () => {});
  it('handles filter changes', () => {});
  it('updates on SignalR events', () => {});
});
```

### 2. Integration Tests
- Test API data flow
- Verify SignalR connectivity
- Test filter interactions

### 3. E2E Tests
- Complete user workflows
- Mobile responsiveness testing
- Performance benchmarks

## Reusable Components

### StatsCard Props
```typescript
interface StatsCardProps {
  title: string;
  value: number | string;
  icon: string;
  color: string;
  isLoading?: boolean;
  trend?: {
    value: number;
    isPositive: boolean;
    label: string;
  };
  onClick?: () => void;
}
```

### ChartCard Props
```typescript
interface ChartCardProps {
  title: string;
  subtitle?: string;
  isLoading?: boolean;
  height?: string;
  children: React.ReactNode;
}
```

### RecentItemsList Props
```typescript
interface RecentItemsListProps {
  title: string;
  items: Array<{
    id: number;
    title: string;
    subtitle: string;
    status: string;
    statusColor: string;
    timestamp: string;
    isOverdue?: boolean;
    onClick?: () => void;
  }>;
  isLoading?: boolean;
  maxItems?: number;
  showAllLink?: {
    text: string;
    onClick: () => void;
  };
}
```

## Implementation Checklist

When creating a new module dashboard:

- [ ] Create dashboard page component
- [ ] Define dashboard DTO structure
- [ ] Implement API query and handler
- [ ] Add SignalR notifications
- [ ] Configure auto-refresh
- [ ] Implement responsive design
- [ ] Add loading and error states
- [ ] Create filter controls
- [ ] Build chart visualizations
- [ ] Add recent items section
- [ ] Implement quick actions
- [ ] Write unit tests
- [ ] Document API endpoints
- [ ] Test mobile responsiveness
- [ ] Verify SignalR updates

## Best Practices

1. **Consistent Naming**: Use `[Module]Dashboard` naming convention
2. **Color Coding**: Use semantic colors (success, warning, danger)
3. **Loading States**: Show skeleton loaders during data fetch
4. **Error Handling**: Display user-friendly error messages
5. **Empty States**: Provide meaningful empty state messages
6. **Accessibility**: Ensure WCAG compliance
7. **Performance**: Limit initial data load to essential metrics
8. **Customization**: Allow users to configure refresh intervals

## Future Enhancements

1. **User Preferences**: Save dashboard layout preferences
2. **Export Options**: PDF, Excel, PowerPoint exports
3. **Drill-down**: Click-through to detailed views
4. **Widgets**: Draggable, resizable dashboard widgets
5. **Themes**: Dark mode support
6. **Notifications**: Push notifications for critical metrics
7. **AI Insights**: Predictive analytics and recommendations

## References

- [Incident Dashboard Implementation](/src/HarmoniHSE360.Web/ClientApp/src/pages/incidents/IncidentDashboard.tsx)
- [Dashboard Components](/src/HarmoniHSE360.Web/ClientApp/src/components/dashboard/)
- [Dashboard Styles](/src/HarmoniHSE360.Web/ClientApp/src/styles/dashboard.scss)
- [API Documentation](/docs/api/dashboard-endpoints.md)