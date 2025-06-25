# HSSE Enhanced Seeding Configuration Guide

## Overview

The Harmoni360 HSSE system now includes comprehensive enhanced seed data capabilities for demonstration purposes. This document outlines how to configure and use the enhanced seeding features.

## Configuration Settings

### Required Configuration in `appsettings.json` or `appsettings.Development.json`

```json
{
  "DataSeeding": {
    "ForceReseed": "false",
    "Categories": {
      "Essential": "true",
      "UserAccounts": "true", 
      "SampleData": "true"
    },
    "EnhancedHSSE": "true",
    "EnableHistoricalData": "true",
    "EnableCrossModuleData": "true",
    "EnableKPICalculation": "true"
  }
}
```

### Configuration Options Explained

#### Core Seeding Configuration
- **`ForceReseed`**: Completely clears and rebuilds all database data
- **`Categories.Essential`**: Seeds roles, permissions, module configurations, and admin users
- **`Categories.UserAccounts`**: Seeds sample user accounts for all departments
- **`Categories.SampleData`**: Seeds basic sample data for all HSSE modules

#### Enhanced HSSE Configuration
- **`EnhancedHSSE`**: Master switch for enhanced HSSE demonstration features
- **`EnableHistoricalData`**: Generates 3-5 years of historical HSSE data across all modules
- **`EnableCrossModuleData`**: Creates relationships and dependencies between modules
- **`EnableKPICalculation`**: Calculates KPI baselines, targets, and industry benchmarks

## Seed Data Components

### 1. Historical Data Seeder (`HSSEHistoricalDataSeeder`)

**Purpose**: Generates comprehensive historical data spanning 3-5 years for realistic dashboard demonstrations.

**Data Generated**:
- **Hazards**: 8-25 per month with seasonal variations, 85% resolution rate
- **Incidents**: 2-8 per month with severity-based resolution timelines
- **Inspections**: 15-30 per month with findings and corrective actions
- **Training Records**: 20-50 assignments per month with 85% completion rate
- **Waste Reports**: 10-25 per month with compliance tracking
- **Security Incidents**: 1-5 per month with resolution tracking
- **Health Monitoring**: 3-12 health incidents per month
- **Audits**: 2-6 audits per month with findings and corrective actions
- **PPE Assignments**: 20-40 assignments per month
- **Work Permits**: 15-35 permits per month with safety compliance

**Key Features**:
- Realistic data distribution with seasonal patterns
- Proper resolution timelines based on severity
- Cross-referenced data with proper relationships
- Historical progression showing improvement trends

### 2. Cross-Module Data Builder (`HSSECrossModuleDataBuilder`)

**Purpose**: Creates realistic relationships and dependencies between different HSSE modules.

**Relationships Built**:
- **Hazard-to-Incident**: Links incidents to previously reported hazards in same location
- **Inspection-to-Hazard**: Creates hazards from high-severity inspection findings
- **Training-to-Incident**: Identifies training gaps that contributed to incidents
- **Audit-to-Compliance**: Links audit findings to compliance tracking
- **Work Permit-to-Hazard**: Associates hazards with work permit activities
- **Waste-to-Environmental**: Links waste reports to environmental incidents
- **Security-to-Safety**: Connects security breaches to safety incidents
- **Health-to-Incident**: Associates health monitoring with safety incidents
- **PPE-to-Hazard**: Links PPE deficiencies to hazard mitigation
- **Departmental Cascading**: Shows how one department's issues affect others

### 3. KPI Baseline Calculator (`HSSEKPIBaselineCalculator`)

**Purpose**: Calculates realistic KPI baselines, targets, and industry benchmarks for dashboard metrics.

**KPIs Calculated**:

#### Safety KPIs
- **TRIR** (Total Recordable Incident Rate): per 200,000 hours
- **LTIFR** (Lost Time Injury Frequency Rate): per million hours  
- **NMRR** (Near Miss Reporting Rate): per million hours
- **SICR** (Safety Inspection Compliance Rate): percentage
- **HRWPC** (High-Risk Work Permit Compliance): percentage

#### Health KPIs
- **OHIR** (Occupational Health Incident Rate): per 1000 employees
- **HSCR** (Health Surveillance Compliance Rate): percentage
- **MERT** (Medical Emergency Response Time): minutes

#### Security KPIs
- **SIF** (Security Incident Frequency): incidents per month
- **CSIR** (Critical Security Incident Rate): percentage
- **SIRT** (Security Incident Resolution Time): hours

#### Environmental KPIs
- **WGR** (Waste Generation Rate): kg per 1000 hours
- **HWP** (Hazardous Waste Percentage): percentage
- **ECR** (Environmental Compliance Rate): percentage
- **EIR** (Environmental Incident Rate): per million hours

#### Training KPIs
- **TCR** (Training Completion Rate): percentage
- **STC** (Safety Training Coverage): percentage
- **ATS** (Average Training Score): percentage

#### Compliance KPIs
- **ACR** (Audit Compliance Rate): percentage
- **RCS** (Regulatory Compliance Score): percentage
- **CACR** (Corrective Action Completion Rate): percentage

#### Operational KPIs
- **PCR** (PPE Compliance Rate): percentage
- **WPPT** (Work Permit Processing Time): hours

**Benchmark Levels**:
- **World Class**: Top 10% industry performance
- **Industry Average**: Median industry performance
- **Minimum Acceptable**: Company minimum standards

## Dashboard Integration

### Materialized Views
The enhanced seed data works with the existing materialized views:
- `mv_hazard_statistics_monthly`
- `mv_incident_frequency_rates`
- `mv_safety_performance_monthly`
- `mv_monthly_hazard_trends`
- `mv_ppe_compliance`
- `mv_training_safety`
- `mv_inspection_safety`
- `mv_work_permit_safety`
- `mv_waste_environmental`
- `mv_security_incidents`
- `mv_health_monitoring`
- `mv_audit_findings`

### Performance Metrics
All seeded data is designed to populate the HSSE Statistics Dashboard with:
- **Trending Charts**: Multi-year trend analysis
- **Comparative Analytics**: Department and location comparisons
- **Performance Indicators**: KPI tracking against targets
- **Correlation Analysis**: Cross-module relationship visualization
- **Compliance Tracking**: Regulatory and company standard compliance

## Usage Instructions

### 1. Development Environment Setup

```bash
# Configure enhanced seeding
cp appsettings.Development.json.template appsettings.Development.json

# Edit configuration to enable enhanced HSSE
# Set EnhancedHSSE: true, EnableHistoricalData: true, etc.

# Run with enhanced seeding
dotnet run --environment Development
```

### 2. Production Demonstration Setup

```bash
# For demonstration purposes only
# Configure with reduced data volume
{
  "DataSeeding": {
    "Categories": {
      "Essential": "true",
      "UserAccounts": "true", 
      "SampleData": "true"
    },
    "EnhancedHSSE": "true",
    "EnableHistoricalData": "true",
    "EnableCrossModuleData": "false",
    "EnableKPICalculation": "true"
  }
}
```

### 3. Full Demonstration Reset

```bash
# Complete database reset with full enhanced data
{
  "DataSeeding": {
    "ForceReseed": "true",
    "Categories": {
      "Essential": "true",
      "UserAccounts": "true", 
      "SampleData": "true"
    },
    "EnhancedHSSE": "true",
    "EnableHistoricalData": "true",
    "EnableCrossModuleData": "true",
    "EnableKPICalculation": "true"
  }
}
```

## Performance Considerations

### Data Volume
- **Historical Data**: ~50,000-100,000 records over 3-5 years
- **Cross-Module Relationships**: ~10,000-20,000 relationship records
- **KPI Records**: ~500-1,000 monthly KPI calculations

### Seeding Time
- **Basic Sample Data**: 30-60 seconds
- **Enhanced HSSE Data**: 2-5 minutes
- **Full Reset + Enhanced**: 5-10 minutes

### Memory Usage
- **Recommended RAM**: 8GB minimum for full enhanced seeding
- **Database Size**: 500MB-1GB with full enhanced data

## Verification and Testing

### Dashboard Verification Checklist

1. **HSSE Statistics Dashboard**
   - [ ] All KPI tiles show realistic values
   - [ ] Trend charts display multi-year data
   - [ ] Department comparisons work
   - [ ] All materialized views populated

2. **Module-Specific Dashboards**
   - [ ] Incident Management: Shows historical incidents with trends
   - [ ] Risk Management: Displays hazards with resolution tracking
   - [ ] Training Management: Shows completion rates and coverage
   - [ ] PPE Management: Displays compliance and assignment data
   - [ ] Work Permits: Shows processing times and compliance
   - [ ] Waste Management: Displays generation rates and compliance

3. **Cross-Module Integration**
   - [ ] Hazards linked to related incidents
   - [ ] Training gaps associated with incidents
   - [ ] Audit findings tracked through resolution
   - [ ] Work permits associated with location hazards

### Data Quality Checks

```sql
-- Verify historical data distribution
SELECT 
    DATE_TRUNC('year', "CreatedAt") as year,
    COUNT(*) as record_count
FROM "Incidents" 
GROUP BY DATE_TRUNC('year', "CreatedAt")
ORDER BY year;

-- Check cross-module relationships
SELECT 
    COUNT(CASE WHEN "RelatedHazardId" IS NOT NULL THEN 1 END) as incidents_with_hazards,
    COUNT(*) as total_incidents,
    ROUND(COUNT(CASE WHEN "RelatedHazardId" IS NOT NULL THEN 1 END) * 100.0 / COUNT(*), 2) as relationship_percentage
FROM "Incidents";

-- Verify KPI calculations
SELECT 
    "MetricCode",
    "KPIType", 
    COUNT(*) as record_count,
    MIN("Period") as earliest_period,
    MAX("Period") as latest_period
FROM "HSSEKPIRecords"
GROUP BY "MetricCode", "KPIType"
ORDER BY "KPIType", "MetricCode";
```

## Troubleshooting

### Common Issues

1. **Out of Memory During Seeding**
   - Reduce historical data period to 2-3 years
   - Disable cross-module data temporarily
   - Increase application memory allocation

2. **Long Seeding Times**
   - Check database connection performance
   - Ensure adequate system resources
   - Consider seeding in phases

3. **Missing Dashboard Data**
   - Verify materialized views are refreshed
   - Check that enhanced seeding completed successfully
   - Confirm dashboard queries target correct time periods

4. **Inconsistent KPI Values**
   - Verify input data quality
   - Check KPI calculation logic
   - Ensure proper time zone handling

### Logging and Diagnostics

Enhanced seeding includes detailed logging:
- Progress indicators for each seeding phase
- Record counts for verification
- Timing information for performance analysis
- Error details for troubleshooting

Check application logs for seeding progress and any issues.

## Maintenance and Updates

### Regular Maintenance
- Monitor database size growth with enhanced data
- Refresh materialized views on schedule
- Archive old KPI records as needed
- Update benchmark targets annually

### Data Refresh
- Re-run enhanced seeding quarterly for demos
- Update KPI targets based on performance
- Refresh cross-module relationships as needed
- Validate data quality regularly

This enhanced seeding system provides a comprehensive foundation for demonstrating the full capabilities of the Harmoni360 HSSE system with realistic, interconnected data across all modules.