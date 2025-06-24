using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateComprehensiveHSSEMaterializedViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop any existing materialized views
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_hazard_statistics_monthly CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_incident_frequency_rates CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_safety_performance_monthly CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_monthly_hazard_trends CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_ppe_compliance CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_training_safety CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_inspection_safety CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_work_permit_safety CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_waste_environmental CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_security_incidents CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_health_monitoring CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_audit_findings CASCADE;");

            // Create core HSSE materialized views for dashboard metrics

            // 1. Hazard Statistics Monthly - aggregated hazard data by month, location, and category
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_hazard_statistics_monthly AS
                SELECT 
                    DATE_TRUNC('month', h.""IdentifiedDate"") as period_month,
                    h.""Location"",
                    COALESCE(hc.""Name"", 'Unknown') as category,
                    COUNT(*) as total_hazards,
                    COUNT(CASE WHEN h.""Severity"" IN ('Major', 'Catastrophic') THEN 1 END) as high_severity_count,
                    COUNT(CASE WHEN h.""Status"" IN ('Reported', 'UnderAssessment') THEN 1 END) as open_cases,
                    COUNT(CASE WHEN h.""Status"" IN ('Resolved', 'Closed') THEN 1 END) as closed_cases
                FROM ""Hazards"" h
                LEFT JOIN ""HazardCategories"" hc ON h.""CategoryId"" = hc.""Id""
                WHERE h.""IdentifiedDate"" >= CURRENT_DATE - INTERVAL '2 years'
                GROUP BY DATE_TRUNC('month', h.""IdentifiedDate""), h.""Location"", COALESCE(hc.""Name"", 'Unknown');
            ");

            // 2. Incident Frequency Rates - yearly incident statistics and severity metrics
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_incident_frequency_rates AS
                SELECT 
                    DATE_TRUNC('year', i.""IncidentDate"") as period_year,
                    i.""Location"",
                    COUNT(*) as total_incidents,
                    COUNT(CASE WHEN i.""Severity"" IN ('Major', 'Serious', 'Critical', 'Emergency') THEN 1 END) as high_severity_incidents,
                    COUNT(CASE WHEN i.""InjuryType"" IS NOT NULL THEN 1 END) as injury_incidents,
                    AVG(CASE 
                        WHEN i.""Severity"" = 'Emergency' THEN 6 
                        WHEN i.""Severity"" = 'Critical' THEN 5 
                        WHEN i.""Severity"" = 'Serious' THEN 4 
                        WHEN i.""Severity"" = 'Major' THEN 3 
                        WHEN i.""Severity"" = 'Moderate' THEN 2 
                        ELSE 1 END) as avg_severity_score
                FROM ""Incidents"" i
                WHERE i.""IncidentDate"" >= CURRENT_DATE - INTERVAL '5 years'
                GROUP BY DATE_TRUNC('year', i.""IncidentDate""), i.""Location"";
            ");

            // 3. Safety Performance Monthly - hazard and risk assessment metrics
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_safety_performance_monthly AS
                SELECT 
                    DATE_TRUNC('month', h.""IdentifiedDate"") as period_month,
                    h.""Location"",
                    COUNT(h.""Id"") as hazard_count,
                    COUNT(ra.""Id"") as risk_assessments_completed,
                    AVG(COALESCE(ra.""ProbabilityScore"" * ra.""SeverityScore"", 0)) as avg_risk_score,
                    COUNT(CASE WHEN ra.""ProbabilityScore"" * ra.""SeverityScore"" >= 20 THEN 1 END) as critical_risk_count,
                    COUNT(CASE WHEN ra.""ProbabilityScore"" * ra.""SeverityScore"" >= 15 THEN 1 END) as high_risk_count
                FROM ""Hazards"" h
                LEFT JOIN ""RiskAssessments"" ra ON h.""Id"" = ra.""HazardId""
                WHERE h.""IdentifiedDate"" >= CURRENT_DATE - INTERVAL '2 years'
                GROUP BY DATE_TRUNC('month', h.""IdentifiedDate""), h.""Location"";
            ");

            // 4. Monthly Hazard Trends - trend analysis and risk level categorization
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_monthly_hazard_trends AS
                SELECT 
                    DATE_TRUNC('month', h.""IdentifiedDate"") as period_month,
                    EXTRACT(YEAR FROM h.""IdentifiedDate"") as year,
                    EXTRACT(MONTH FROM h.""IdentifiedDate"") as month,
                    h.""Location"",
                    COUNT(*) as hazard_count,
                    COUNT(CASE WHEN h.""Severity"" IN ('Negligible', 'Minor') THEN 1 END) as low_severity_count,
                    COUNT(CASE WHEN h.""Severity"" IN ('Major', 'Catastrophic') THEN 1 END) as high_severity_count,
                    CASE 
                        WHEN COUNT(*) > 10 THEN 'High'
                        WHEN COUNT(*) > 5 THEN 'Medium'
                        ELSE 'Low'
                    END as risk_level
                FROM ""Hazards"" h
                WHERE h.""IdentifiedDate"" >= CURRENT_DATE - INTERVAL '2 years'
                GROUP BY DATE_TRUNC('month', h.""IdentifiedDate""), EXTRACT(YEAR FROM h.""IdentifiedDate""), 
                         EXTRACT(MONTH FROM h.""IdentifiedDate""), h.""Location"";
            ");

            // Create stub materialized views for extended modules (these will show default data until modules are populated)
            
            // 5. PPE Compliance - placeholder for PPE management data
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_ppe_compliance AS
                SELECT 
                    'Default' as ""Department"",
                    0 as total_assignments,
                    0 as active_assignments,
                    0 as returned_assignments,
                    'Default' as ppe_category;
            ");

            // 6. Training Safety - placeholder for training management data
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_training_safety AS
                SELECT 
                    'Default' as ""Department"",
                    0 as total_training_participants,
                    0 as completed_trainings,
                    0 as mandatory_trainings,
                    0 as mandatory_completed,
                    0 as passed_trainings;
            ");

            // 7. Inspection Safety - placeholder for inspection data
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_inspection_safety AS
                SELECT 
                    CURRENT_DATE::timestamp as period_month,
                    'Default' as location,
                    0 as total_inspections,
                    0 as safety_inspections,
                    0 as total_findings,
                    0 as critical_findings,
                    0 as high_priority_findings;
            ");

            // 8. Work Permit Safety - placeholder for work permit data
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_work_permit_safety AS
                SELECT 
                    CURRENT_DATE::timestamp as period_month,
                    'Default' as department,
                    0 as total_permits,
                    0 as approved_permits,
                    0 as completed_permits,
                    0 as high_risk_permits,
                    0 as hot_work_permits,
                    0 as confined_space_permits;
            ");

            // 9. Waste Environmental - placeholder for waste management data
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_waste_environmental AS
                SELECT 
                    CURRENT_DATE::timestamp as period_month,
                    'Default' as department,
                    0 as total_waste_reports,
                    0 as hazardous_waste_reports,
                    0 as disposed_reports,
                    0.0 as total_waste_quantity,
                    0.0 as disposal_rate;
            ");

            // 10. Security Incidents - placeholder for security data
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_security_incidents AS
                SELECT 
                    CURRENT_DATE::timestamp as period_month,
                    'Default' as ""Location"",
                    0 as total_security_incidents,
                    0 as high_impact_incidents,
                    0 as resolved_incidents,
                    0 as critical_incidents;
            ");

            // 11. Health Monitoring - placeholder for health incident data
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_health_monitoring AS
                SELECT 
                    CURRENT_DATE::timestamp as period_month,
                    'Default' as ""Department"",
                    0 as total_health_incidents,
                    0 as occupational_health_cases,
                    0 as medical_emergencies,
                    0 as resolved_cases;
            ");

            // 12. Audit Findings - placeholder for audit data
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_audit_findings AS
                SELECT 
                    CURRENT_DATE::timestamp as period_month,
                    'Default' as ""Department"",
                    0 as total_audits,
                    0 as safety_audits,
                    0 as total_findings,
                    0 as major_nonconformities,
                    0 as minor_nonconformities;
            ");

            // Create unique indexes for concurrent refresh capability
            // Core views with actual data
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_hazard_statistics_monthly_pk ON mv_hazard_statistics_monthly (period_month, \"Location\", category);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_incident_frequency_rates_pk ON mv_incident_frequency_rates (period_year, \"Location\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_safety_performance_monthly_pk ON mv_safety_performance_monthly (period_month, \"Location\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_monthly_hazard_trends_pk ON mv_monthly_hazard_trends (period_month, \"Location\");");

            // Stub views with default data - use column combinations that ensure uniqueness
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_ppe_compliance_pk ON mv_ppe_compliance (\"Department\", ppe_category);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_training_safety_pk ON mv_training_safety (\"Department\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_inspection_safety_pk ON mv_inspection_safety (period_month, location);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_work_permit_safety_pk ON mv_work_permit_safety (period_month, department);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_waste_environmental_pk ON mv_waste_environmental (period_month, department);");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_security_incidents_pk ON mv_security_incidents (period_month, \"Location\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_health_monitoring_pk ON mv_health_monitoring (period_month, \"Department\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX idx_mv_audit_findings_pk ON mv_audit_findings (period_month, \"Department\");");

            // Create additional indexes for better query performance
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_hazard_stats_period_location ON mv_hazard_statistics_monthly (period_month, \"Location\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_incident_rates_period_location ON mv_incident_frequency_rates (period_year, \"Location\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_safety_performance_period_location ON mv_safety_performance_monthly (period_month, \"Location\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_mv_hazard_trends_period_location ON mv_monthly_hazard_trends (period_month, \"Location\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all indexes first
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_hazard_statistics_monthly_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_incident_frequency_rates_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_safety_performance_monthly_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_monthly_hazard_trends_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_ppe_compliance_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_training_safety_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_inspection_safety_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_work_permit_safety_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_waste_environmental_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_security_incidents_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_health_monitoring_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_audit_findings_pk;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_hazard_stats_period_location;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_incident_rates_period_location;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_safety_performance_period_location;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_mv_hazard_trends_period_location;");

            // Drop all materialized views
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_hazard_statistics_monthly CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_incident_frequency_rates CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_safety_performance_monthly CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_monthly_hazard_trends CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_ppe_compliance CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_training_safety CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_inspection_safety CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_work_permit_safety CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_waste_environmental CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_security_incidents CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_health_monitoring CASCADE;");
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_audit_findings CASCADE;");
        }
    }
}
