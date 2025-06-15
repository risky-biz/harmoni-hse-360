# HSSE Statistics Dashboard Implementation Plan

## Executive Summary

This document outlines the implementation strategy for a comprehensive HSSE (Health, Safety, Security, Environment) Statistics Dashboard in the Harmoni360 platform. The dashboard will consolidate statistical reporting across all HSSE modules, providing real‑time insights, predictive analytics, and exportable reports. The design follows existing Clean Architecture patterns (.NET 8 backend, React + TypeScript frontend) and aligns with UI/UX standards established in other modules.

## System Overview

### Core Objectives
- **Unified Interface**: Single dashboard with module selector for Health, Safety, Security, Environment, or **All Modules**.
- **Real‑time Updates**: Automatic data refresh when modules or filters change, leveraging SignalR where available.
- **Accessible Visualization**: Charts and metrics optimized for clarity and WCAG 2.1 compliance across desktop, tablet, and mobile.
- **Export & Reporting**: PDF export and print‑friendly layouts with company branding and metadata.

### Key Features
1. **Module Selection Dropdown**
2. **Dynamic KPIs**: Incident rates, severity/frequency rates, compliance percentages, and risk trends.
3. **Interactive Charts**: Line graphs for trends, bar charts for comparisons, and gauge cards for goal tracking.
4. **Custom Date Ranges & Filters**
5. **Audit Trail & Data Validation**: Log report generation and data changes for transparency.

## Research Summary

- **HSSE Reporting Standards**: References to ISO 45001, OSHA guidelines, and NIST security metrics informed KPI selection.
- **KPIs & Formulas**:
  - **Total Recordable Incident Rate (TRIR)** = `(Recordable Incidents × 200,000) / Employee Hours Worked`.
  - **Lost Time Injury Frequency Rate (LTIFR)** = `(Lost Time Injuries × 1,000,000) / Hours Worked`.
  - **Severity Rate (SR)** = `(Days Lost × 200,000) / Hours Worked`.
  - **Compliance Rate** = `Compliant Records / Total Records × 100%`.
- **UI/UX Patterns**: Utilized card‑based layouts, responsive grid systems, and colorblind‑friendly palettes based on Material Design research.

## Technical Architecture

### Backend
- **CQRS with MediatR** for queries/commands like `GetHsseStatisticsQuery`.
- **Entity Framework Core** using existing HSSE schema with aggregate queries.
- **SignalR Hub** for broadcasting dashboard updates.

### Frontend
- **React 18 + TypeScript** using reusable chart components (e.g., LineChart, BarChart, PieChart) built with Chart.js.
- **RTK Query** API slices for fetching dashboard data with caching and loading states.
- **Responsive Layout** via CSS grid and Bootstrap utility classes.

## Database Schema

No new tables are required. Existing incident, inspection, training, and audit tables will be queried through database views or materialized views to optimize aggregation performance. Indexes on date and module fields will support fast filtering.

## API Endpoints

- `GET /api/statistics` – Retrieves dashboard data with query parameters for module and date range.
- `GET /api/statistics/export` – Returns a PDF export of the current dashboard view.

## Implementation Phases

### Phase 1: Foundation (Weeks 1–2)
- Create backend queries and handlers for statistics aggregation.
- Build database views for optimized reporting.
- Scaffold React pages with module selector and placeholder charts.

### Phase 2: Core Features (Weeks 3–4)
- Implement chart components and real‑time SignalR updates.
- Add filter controls and loading/error states.
- Integrate audit logging for data access and report exports.

### Phase 3: Export & Polish (Weeks 5–6)
- Implement PDF export and print styles.
- Validate KPI calculations with sample data.
- Conduct accessibility testing across devices.

## Testing Strategy
- **Unit Tests** for calculation utilities and query handlers.
- **Integration Tests** validating API endpoints and data aggregation logic.
- **UI Tests** with React Testing Library covering module selection and chart rendering.

## Risk Management
- **Data Accuracy**: Cross‑check formulas against industry references and include unit tests for edge cases.
- **Performance**: Use pagination or data virtualization for large datasets and consider background jobs for heavy reports.
- **User Adoption**: Provide clear user documentation and in‑app tooltips explaining each metric.

## Success Metrics
- 90% of users can generate reports without assistance.
- Dashboard load time under 2 seconds for typical datasets.
- Accurate KPI calculations verified by HSSE officers.

## Conclusion

The HSSE Statistics Dashboard will provide a centralized view of safety, security, health, and environmental metrics for Harmoni360. By following the outlined phases and leveraging existing architecture, the dashboard will deliver actionable insights while maintaining consistency with other modules. This implementation plan serves as the roadmap for development and future enhancements.

## Progress Notes

- Phase 1 foundation completed with query handlers and basic dashboard scaffolding.
- Phase 2 features implemented: line chart trends, date filtering, and SignalR-based refresh.

- Phase 3 export and KPI validation completed: PDF export implemented with QuestPDF and KPI calculator unit tests added.
- User guide published outlining dashboard usage and export steps.
