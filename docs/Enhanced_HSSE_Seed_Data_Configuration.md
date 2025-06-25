# Enhanced HSSE Seed Data Configuration

## Overview

This document outlines the comprehensive seed data enhancement implemented for the Harmoni360 HSSE application. The enhanced seeding system provides realistic, interconnected demonstration data suitable for dashboard presentation and system evaluation.

## New Seeding Components

### 1. HSSEHistoricalDataSeeder
**Purpose**: Generates 3-5 years of comprehensive historical HSSE data across all modules.

**Features**:
- **Time Range**: 5 years of historical data (2019-2024)
- **Volume**: 15,000+ interconnected records across all HSSE modules
- **Seasonal Patterns**: Realistic seasonal variations (higher incidents during monsoon season)
- **Department Distribution**: Balanced across 10 operational departments
- **Location Coverage**: 12 facility locations with realistic geographical distribution
- **Status Progression**: Realistic lifecycle progression (85% resolved incidents, 90% completed trainings)

**Data Generated**:
- Hazards: 8-25 per month with seasonal variation
- Incidents: 2-8 per month with appropriate severity distribution
- Inspections: 15-30 per month across all locations
- Training Records: 20-50 assignments per month
- Waste Reports: 10-25 per month with compliance tracking
- Security Incidents: 1-5 per month (lower frequency)
- Health Monitoring: 3-12 incidents per month
- Audits: 2-6 per month with findings
- PPE Assignments: 20-40 per month
- Work Permits: 15-35 per month

### 2. HSSECrossModuleDataBuilder
**Purpose**: Creates realistic relationships and dependencies between different HSSE modules.

**Relationships Built**:
- **Hazard → Incident**: Links incidents to prior hazards in same location
- **Inspection → Hazard**: Creates hazards from critical inspection findings
- **Training → Incident**: Identifies training gaps that contributed to incidents
- **Audit → Compliance**: Links audit findings to regulatory compliance
- **Work Permit → Hazard**: Associates hazards with permit activities
- **Waste → Environmental**: Links waste non-compliance to environmental incidents
- **Security → Safety**: Connects security breaches to safety incidents
- **Health → Incident**: Links health issues to workplace incidents
- **PPE → Hazard**: Identifies PPE deficiencies for hazard mitigation
- **Departmental Cascading**: Shows how one department's issues affect others

### 3. HSSEKPIBaselineCalculator
**Purpose**: Establishes realistic KPI baselines, targets, and industry benchmarks.

**KPI Categories**:
- **Safety KPIs**: LTIFR, TRIFR, Hazard Resolution Rate, PPE Compliance
- **Health KPIs**: Occupational Illness Rate, Emergency Response Time
- **Security KPIs**: Security Incident Rate, Compliance Rate
- **Environmental KPIs**: Waste Compliance, Hazardous Waste Reduction
- **Compliance KPIs**: Audit Finding Resolution, Schedule Adherence
- **Training KPIs**: Completion Rate, Effectiveness Score
- **Inspection KPIs**: Completion Rate, Critical Finding Resolution
- **Risk Management KPIs**: Risk Assessment Coverage, High Risk Mitigation

**Benchmark Features**:
- Industry standard targets for Indonesian manufacturing
- 3-year improvement roadmap
- Threshold levels (Critical, Warning, Excellent)
- Monthly trend analysis with realistic variation
- Departmental performance comparison

## Configuration Options

### appsettings.json Configuration

```json
{
  "DataSeeding": {
    "ForceReseed": false,
    "Categories": {
      "Essential": true,
      "UserAccounts": true,
      "SampleData": true
    },
    "EnhancedHSSE": true,
    "EnableHistoricalData": true,
    "EnableCrossModuleData": true,
    "EnableKPIBaselines": true
  }
}
```

### Configuration Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `EnhancedHSSE` | false | Enable comprehensive historical seeding |
| `EnableHistoricalData` | false | Generate 3-5 years of historical data |
| `EnableCrossModuleData` | false | Build cross-module relationships |
| `EnableKPIBaselines` | false | Calculate KPI baselines and benchmarks |
| `ForceReseed` | false | Clear all data and regenerate |

## Data Volume and Performance

### Expected Record Counts (5-year dataset)
- **Hazards**: ~8,000 records
- **Incidents**: ~3,000 records
- **Inspections**: ~9,000 records
- **Training Assignments**: ~15,000 records
- **Waste Reports**: ~6,000 records
- **Security Incidents**: ~1,200 records
- **Health Incidents**: ~4,000 records
- **Audits**: ~1,800 records
- **PPE Assignments**: ~12,000 records
- **Work Permits**: ~10,000 records

### Performance Characteristics
- **Seeding Time**: 10-15 minutes for complete dataset
- **Database Size**: ~500MB with indexes
- **Memory Usage**: Peak 2GB during seeding
- **CPU Usage**: Moderate (single-threaded generation)

## Dashboard Integration

### Supported Dashboard Views
1. **HSSE Statistics Dashboard**: Comprehensive metrics with 5-year trends
2. **Safety Performance Dashboard**: Incident rates, hazard analysis
3. **Compliance Dashboard**: Audit findings, regulatory compliance
4. **Training Dashboard**: Completion rates, certification tracking
5. **Environmental Dashboard**: Waste management, compliance trends
6. **Departmental Comparison**: Cross-department performance analysis

### KPI Visualization
- Monthly trend charts with 60 data points
- Year-over-year comparison
- Department benchmarking
- Industry standard comparison
- Target vs. actual performance

## Referential Integrity

### Primary Relationships
- All incidents reference valid users, locations, and departments
- Hazards link to appropriate risk assessments and mitigation actions
- Training assignments reference existing training programs and users
- Audit findings link to specific audits and corrective actions
- Work permits reference valid approvers and departments

### Cross-Module Dependencies
- Incidents can reference related hazards, training gaps, and PPE deficiencies
- Inspections generate hazards which can lead to incidents
- Training completion affects incident probability
- Audit findings drive compliance requirements
- Waste management impacts environmental incident rates

## Data Quality Features

### Realistic Data Patterns
- **Seasonal Variation**: Higher incidents during monsoon season (Oct-Mar)
- **Departmental Risk Profiles**: Production/Maintenance higher risk than Admin/HR
- **Severity Distribution**: 5% critical, 15% high, 40% medium, 40% low
- **Resolution Timelines**: Faster resolution for higher severity issues
- **Compliance Rates**: 95% waste compliance, 90% training completion

### Audit Trail
- All records include proper CreatedAt timestamps
- Status transitions follow realistic timelines
- User assignments match department and role appropriateness
- Approval workflows follow organizational hierarchy

## Testing and Validation

### Pre-Deployment Checks
1. **Data Consistency**: Verify all foreign key relationships
2. **Timeline Validity**: Ensure chronological order of events
3. **Volume Validation**: Confirm expected record counts
4. **Dashboard Loading**: Test all dashboard views load correctly
5. **Performance**: Verify acceptable query response times

### Quality Metrics
- Zero orphaned records
- 100% referential integrity
- Realistic data distribution
- Dashboard compatibility
- Performance within acceptable limits

## Maintenance and Updates

### Regular Maintenance
- Monitor seeding performance
- Update industry benchmarks annually
- Adjust seasonal patterns based on actual data
- Refresh KPI targets based on organization goals

### Extension Points
- Additional modules can be integrated
- New KPIs can be added to baseline calculator
- Cross-module relationships can be extended
- Historical data range can be adjusted

## Usage Examples

### Full Enhanced Seeding
```bash
# Enable all enhanced features
dotnet run --project Harmoni360.Web -- --seed-data --enhanced-hsse

# Or via configuration
# Set DataSeeding:EnhancedHSSE = true in appsettings.json
dotnet run --project Harmoni360.Web
```

### Selective Seeding
```json
{
  "DataSeeding": {
    "EnhancedHSSE": true,
    "EnableHistoricalData": true,
    "EnableCrossModuleData": false,
    "EnableKPIBaselines": false
  }
}
```

### Force Regeneration
```json
{
  "DataSeeding": {
    "ForceReseed": true,
    "EnhancedHSSE": true
  }
}
```

## Troubleshooting

### Common Issues
1. **Memory Exhaustion**: Reduce historical data range or increase VM memory
2. **Slow Performance**: Disable indexes during seeding, rebuild after
3. **Referential Integrity Errors**: Ensure essential data is seeded first
4. **Dashboard Loading Issues**: Verify materialized views are created

### Debugging
- Enable detailed logging: `"Logging": {"Default": "Debug"}`
- Monitor database connections and memory usage
- Check foreign key constraint violations
- Validate data generation parameters

## Conclusion

The enhanced HSSE seed data system provides a comprehensive, realistic dataset that demonstrates the full capabilities of the Harmoni360 HSSE application. With proper configuration, it generates years of interconnected data suitable for dashboard presentation, system evaluation, and user training.