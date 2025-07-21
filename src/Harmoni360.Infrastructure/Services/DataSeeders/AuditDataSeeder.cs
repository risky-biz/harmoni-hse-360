using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Audits;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Infrastructure.Services.DataSeeders;

public class AuditDataSeeder : IDataSeeder
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AuditDataSeeder> _logger;
    private readonly Random _random = new();

    // Audit locations
    private readonly string[] _locations = new[]
    {
        "Main Production Floor - Building A",
        "Warehouse Section B", 
        "Chemical Storage Area C-12",
        "Electrical Substation Room 101",
        "Laboratory Building D",
        "Loading Dock Area",
        "Maintenance Workshop",
        "Boiler Room - Basement",
        "HVAC Plant Room",
        "Emergency Exit Routes",
        "Fire Safety Equipment Areas",
        "Water Treatment Facility",
        "Compressor Station",
        "Office Building - Floor 3",
        "Cafeteria and Kitchen Area"
    };

    // Audit titles by type
    private readonly Dictionary<AuditType, string[]> _auditTitles = new()
    {
        [AuditType.Safety] = new[]
        {
            "Annual Safety Management System Audit",
            "Workplace Safety Compliance Review",
            "Emergency Preparedness Assessment",
            "Personal Protective Equipment Program Audit",
            "Incident Management System Review",
            "Safety Training Program Evaluation",
            "Contractor Safety Management Audit",
            "Machine Safety and Guarding Assessment",
            "Electrical Safety Standards Compliance",
            "Chemical Handling and Storage Audit"
        },
        [AuditType.Environmental] = new[]
        {
            "Environmental Management System Audit",
            "Waste Management Compliance Review",
            "Air Quality Monitoring Assessment",
            "Water Discharge Compliance Audit",
            "Chemical Storage Environmental Review",
            "Environmental Impact Assessment",
            "Regulatory Compliance Verification",
            "Sustainability Program Evaluation",
            "Environmental Risk Assessment",
            "Pollution Prevention Program Audit"
        },
        [AuditType.Equipment] = new[]
        {
            "Critical Equipment Safety Audit",
            "Pressure Vessel Inspection Review",
            "Lifting Equipment Certification Audit",
            "Electrical Equipment Safety Assessment",
            "Process Equipment Integrity Review",
            "Emergency Equipment Readiness Audit",
            "Calibration Program Compliance Review",
            "Equipment Maintenance Program Audit",
            "Safety System Testing Verification",
            "Equipment Modification Control Audit"
        },
        [AuditType.Process] = new[]
        {
            "Process Safety Management Audit",
            "Operating Procedure Compliance Review",
            "Change Management Process Audit",
            "Training and Competency Assessment",
            "Management of Change Evaluation",
            "Process Hazard Analysis Review",
            "Emergency Response Procedure Audit",
            "Quality Management System Review",
            "Document Control System Audit",
            "Continuous Improvement Process Review"
        },
        [AuditType.Compliance] = new[]
        {
            "Regulatory Compliance Audit",
            "ISO 45001 Compliance Assessment",
            "ISO 14001 Environmental Compliance",
            "OSHA Standards Compliance Review",
            "Industry Standards Compliance Audit",
            "Legal Requirements Verification",
            "Permit Compliance Assessment",
            "Certification Maintenance Review",
            "Regulatory Reporting Compliance",
            "Standards Implementation Audit"
        },
        [AuditType.Fire] = new[]
        {
            "Fire Safety System Audit",
            "Fire Prevention Program Review",
            "Emergency Evacuation Procedures Audit",
            "Fire Protection Equipment Inspection",
            "Hot Work Permit System Review",
            "Fire Detection System Testing",
            "Sprinkler System Compliance Audit",
            "Fire Emergency Response Training",
            "Combustible Materials Storage Audit",
            "Fire Risk Assessment Review"
        },
        [AuditType.Chemical] = new[]
        {
            "Chemical Management System Audit",
            "Chemical Storage Compliance Review",
            "Chemical Handling Procedures Audit",
            "Material Safety Data Sheet Review",
            "Chemical Inventory Management Audit",
            "Chemical Waste Disposal Audit",
            "Chemical Emergency Response Review",
            "Personal Protection for Chemical Handling",
            "Chemical Transport and Logistics Audit",
            "Chemical Risk Assessment Review"
        },
        [AuditType.Ergonomic] = new[]
        {
            "Workplace Ergonomics Assessment",
            "Manual Handling Risk Assessment",
            "Workstation Ergonomics Audit",
            "Repetitive Strain Injury Prevention",
            "Ergonomic Training Program Review",
            "Display Screen Equipment Assessment",
            "Lifting and Handling Procedures Audit",
            "Workplace Design Ergonomics Review",
            "Employee Ergonomics Health Monitoring",
            "Ergonomic Equipment Compliance Audit"
        },
        [AuditType.Emergency] = new[]
        {
            "Emergency Preparedness Audit",
            "Crisis Management System Review",
            "Emergency Response Procedures Audit",
            "Business Continuity Planning Review",
            "Emergency Communication Systems Audit",
            "Disaster Recovery Planning Assessment",
            "Emergency Equipment Readiness Review",
            "Emergency Training Program Audit",
            "Emergency Contact Systems Review",
            "Emergency Shelter and Evacuation Audit"
        },
        [AuditType.Management] = new[]
        {
            "Management System Effectiveness Audit",
            "Leadership and Governance Review",
            "Management Review Process Audit",
            "Organizational Structure Assessment",
            "Management Communication Systems Review",
            "Strategic Planning Process Audit",
            "Management Performance Review",
            "Resource Management Assessment",
            "Management Training and Development",
            "Management Decision Making Process Review"
        }
    };

    // Common audit checklist items by category
    private readonly Dictionary<AuditCategory, string[]> _checklistItems = new()
    {
        [AuditCategory.Routine] = new[]
        {
            "Review standard operating procedures currency",
            "Check emergency response procedures",
            "Verify work permit procedures compliance",
            "Review incident reporting procedures",
            "Check change management procedures",
            "Verify risk assessment procedures",
            "Review communication procedures",
            "Check quality control procedures"
        },
        [AuditCategory.Planned] = new[]
        {
            "Review employee induction training programs",
            "Check ongoing safety training compliance",
            "Verify competency assessments are current",
            "Review emergency response training records",
            "Check specialized equipment training",
            "Verify supervisor and manager training",
            "Review contractor training requirements",
            "Check refresher training schedules"
        },
        [AuditCategory.Unplanned] = new[]
        {
            "Investigate immediate safety concerns raised",
            "Review incident-triggered audit requirements",
            "Assess emergency response effectiveness",
            "Evaluate unscheduled maintenance impacts",
            "Check reactive safety measures implementation",
            "Review urgent compliance requirements",
            "Assess ad-hoc training needs",
            "Evaluate immediate corrective actions taken"
        },
        [AuditCategory.Regulatory] = new[]
        {
            "Verify compliance with current regulations",
            "Check regulatory permit requirements",
            "Review mandatory reporting compliance",
            "Assess regulatory inspection readiness",
            "Verify license and certification currency",
            "Check regulatory change management",
            "Review legal requirement documentation",
            "Assess regulatory risk management"
        },
        [AuditCategory.Internal] = new[]
        {
            "Verify current procedures are available and up-to-date",
            "Check document control and version management",
            "Review training records and competency assessments",
            "Validate record keeping and data integrity",
            "Confirm regulatory compliance documentation",
            "Review incident investigation reports",
            "Check management review meeting minutes",
            "Verify audit findings and corrective actions tracking"
        },
        [AuditCategory.External] = new[]
        {
            "Prepare for third-party auditor requirements",
            "Review external certification requirements",
            "Check client-specific audit criteria",
            "Verify customer compliance requirements",
            "Review external stakeholder expectations",
            "Assess public disclosure requirements",
            "Check external reporting obligations",
            "Verify third-party verification processes"
        },
        [AuditCategory.Incident] = new[]
        {
            "Investigate root cause of safety incident",
            "Review incident response effectiveness",
            "Assess corrective actions implementation",
            "Check incident reporting accuracy",
            "Review lessons learned integration",
            "Verify incident prevention measures",
            "Assess emergency response performance",
            "Review incident communication processes"
        },
        [AuditCategory.Maintenance] = new[]
        {
            "Inspect safety equipment condition and placement",
            "Check equipment maintenance schedules",
            "Verify calibration records are current",
            "Review equipment inspection checklists",
            "Check emergency equipment accessibility",
            "Verify equipment modification controls",
            "Review equipment retirement procedures",
            "Check spare parts inventory management"
        }
    };

    public AuditDataSeeder(IApplicationDbContext context, ILogger<AuditDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting audit data seeding...");

            // Skip if audits already exist
            if (await _context.Audits.AnyAsync())
            {
                _logger.LogInformation("Audit data already exists, skipping seeding");
                return;
            }

            // Get users for assignment
            var users = await _context.Users.ToListAsync();
            var departments = await _context.Departments.ToListAsync();

            if (!users.Any())
            {
                _logger.LogWarning("No users found for audit assignment");
                return;
            }

            var audits = new List<Audit>();

            // Create audits for the past 12 months and next 6 months
            var startDate = DateTime.UtcNow.AddMonths(-12);
            var endDate = DateTime.UtcNow.AddMonths(6);

            for (var month = 0; month < 18; month++)
            {
                var auditDate = startDate.AddMonths(month);
                var auditsInMonth = _random.Next(2, 6); // 2-5 audits per month

                for (var i = 0; i < auditsInMonth; i++)
                {
                    var auditType = GetRandomEnumValue<AuditType>();
                    var audit = await CreateAuditAsync(auditType, auditDate, users, departments);
                    audits.Add(audit);
                }
            }

            _context.Audits.AddRange(audits);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully seeded {audits.Count} audits");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding audit data");
            throw;
        }
    }

    private Task<Audit> CreateAuditAsync(AuditType type, DateTime baseDate, List<User> users, List<Department> departments)
    {
        var titles = _auditTitles[type];
        var title = titles[_random.Next(titles.Length)];
        var auditor = users[_random.Next(users.Count)];
        var department = departments.Any() ? departments[_random.Next(departments.Count)] : null;

        var scheduledDate = baseDate.AddDays(_random.Next(0, 28));
        var isCompleted = scheduledDate < DateTime.UtcNow.AddDays(-7);
        var isInProgress = !isCompleted && scheduledDate < DateTime.UtcNow && _random.NextDouble() > 0.3;

        var audit = Audit.Create(
            title: title,
            description: $"Comprehensive {type} audit covering critical areas and compliance requirements.",
            type: type,
            category: GetRandomEnumValue<AuditCategory>(),
            priority: GetRandomEnumValue<AuditPriority>(),
            scheduledDate: scheduledDate,
            auditorId: auditor.Id,
            locationId: null,
            departmentId: department?.Id,
            facilityId: null,
            estimatedDurationMinutes: _random.Next(120, 480) // 2-8 hours
        );

        // Set compliance info if applicable
        var isRegulatory = _random.NextDouble() > 0.6;
        if (isRegulatory)
        {
            var regulatoryRefs = new[] { "OSHA 1910", "ISO 45001", "ISO 14001", "EPA 40 CFR", "DOT 49 CFR" };
            var standards = "ISO 45001:2018, OSHA 29 CFR 1910";
            audit.SetComplianceInfo(standards, isRegulatory, regulatoryRefs[_random.Next(regulatoryRefs.Length)]);
        }

        // Add audit items BEFORE progressing the audit status
        AddAuditItems(audit, type);

        // Progress the audit based on scheduled date
        if (isInProgress)
        {
            audit.Schedule(scheduledDate);
            audit.StartAudit();
        }
        else if (isCompleted)
        {
            audit.Schedule(scheduledDate);
            audit.StartAudit();
            
            var summary = GenerateAuditSummary(type);
            var recommendations = GenerateRecommendations(type);
            audit.CompleteAudit(summary, recommendations);
        }

        // Add findings if audit is completed or in progress
        if (audit.Status == AuditStatus.Completed || audit.Status == AuditStatus.InProgress)
        {
            AddAuditFindings(audit, auditor);
        }

        // Add comments from various stakeholders
        if (audit.Status != AuditStatus.Scheduled)
        {
            AddAuditComments(audit, users);
        }

        return Task.FromResult(audit);
    }

    private void AddAuditItems(Audit audit, AuditType type)
    {
        var categories = Enum.GetValues<AuditCategory>().ToList();
        var itemsPerCategory = _random.Next(3, 8);

        foreach (var category in categories.Take(_random.Next(2, categories.Count)))
        {
            var items = _checklistItems[category];
            var selectedItems = items.OrderBy(x => _random.Next()).Take(itemsPerCategory);

            var itemNumber = 1;
            foreach (var itemText in selectedItems)
            {
                var item = AuditItem.Create(
                    auditId: audit.Id,
                    description: itemText,
                    type: AuditItemType.YesNo,
                    isRequired: _random.NextDouble() > 0.3,
                    category: category.ToString(),
                    sortOrder: itemNumber,
                    expectedResult: "Compliant",
                    maxPoints: 10
                );

                // Assess item if audit is in progress or completed
                if (audit.Status == AuditStatus.InProgress || audit.Status == AuditStatus.Completed)
                {
                    var result = GetRandomEnumValue<AuditResult>();
                    var isCompliant = result == AuditResult.Compliant || result == AuditResult.PartiallyCompliant;
                    var score = result switch
                    {
                        AuditResult.Compliant => _random.Next(8, 11),
                        AuditResult.PartiallyCompliant => _random.Next(5, 8),
                        AuditResult.NonCompliant => _random.Next(0, 4),
                        AuditResult.NotApplicable => 0,
                        _ => 0
                    };

                    if (result == AuditResult.NotApplicable)
                    {
                        item.MarkAsNotApplicable("Not applicable for this location", "System");
                    }
                    else
                    {
                        item.StartAssessment();
                        item.CompleteAssessment(
                            actualResult: result.ToString(),
                            isCompliant: isCompliant,
                            assessedBy: "System",
                            actualPoints: score,
                            comments: GenerateItemComment(result),
                            evidence: "Evidence documented and verified"
                        );
                    }
                }

                audit.AddItem(item);
                itemNumber++;
            }
        }
    }

    private void AddAuditFindings(Audit audit, User auditor)
    {
        var findingCount = _random.Next(0, 5); // 0-4 findings per audit

        for (var i = 0; i < findingCount; i++)
        {
            var severity = GetRandomEnumValue<FindingSeverity>();
            var type = GetRandomEnumValue<FindingType>();
            
            var finding = AuditFinding.Create(
                auditId: audit.Id,
                description: GenerateFindingDescription(type, severity),
                type: type,
                severity: severity,
                location: _locations[_random.Next(_locations.Length)],
                equipment: null,
                auditItemId: null
            );

            // Set immediate action
            finding.SetImmediateAction(GenerateImmediateAction(severity));
            
            // Set corrective action with responsible person
            finding.SetCorrectiveAction(
                GenerateCorrectiveAction(type),
                DateTime.UtcNow.AddDays(_random.Next(30, 120)),
                auditor.Id,
                auditor.Name
            );

            // Progress some findings through their lifecycle if audit is completed
            if (_random.NextDouble() > 0.6 && audit.Status == AuditStatus.Completed)
            {
                // Mark as resolved first
                finding.MarkAsResolved();
                
                // If finding requires verification (Critical/Major), verify it before closing
                if (finding.RequiresVerification)
                {
                    finding.MarkAsVerified(auditor.Name, "Management review and corrective action effectiveness verified");
                }
                
                // Now close the finding
                finding.Close("Corrective actions completed and verified", auditor.Name);
            }

            audit.AddFinding(finding);
        }
    }

    private void AddAuditComments(Audit audit, List<User> users)
    {
        var commentCount = _random.Next(2, 6); // 2-5 comments per audit

        for (var i = 0; i < commentCount; i++)
        {
            var commenter = users[_random.Next(users.Count)];
            var commentDate = audit.ScheduledDate.AddDays(_random.Next(-5, 15));
            
            var comment = AuditComment.Create(
                auditId: audit.Id,
                comment: GenerateAuditComment(audit.Type, audit.Status),
                commentedBy: commenter.Name,
                category: "General",
                isInternal: _random.NextDouble() < 0.2 // 20% internal comments
            );

            audit.AddComment(comment);
        }
    }

    private string GenerateAuditComment(AuditType auditType, AuditStatus status)
    {
        var comments = status switch
        {
            AuditStatus.Scheduled => new[]
            {
                "Audit scheduled and all preparation materials have been provided to the team.",
                "Looking forward to this audit as it will help identify improvement opportunities.",
                "All documentation has been prepared and is ready for review.",
                "The audit scope and objectives have been clearly communicated to all stakeholders."
            },
            AuditStatus.InProgress => new[]
            {
                "The audit is progressing well with good cooperation from all departments.",
                "Initial findings suggest the system is working effectively with minor improvements needed.",
                "Team is very responsive to questions and providing excellent documentation.",
                "Some interesting observations that may lead to positive improvements.",
                "The audit process is revealing both strengths and areas for enhancement."
            },
            AuditStatus.Completed => new[]
            {
                "Excellent audit process with valuable insights and actionable recommendations.",
                "The findings will help us improve our systems and processes significantly.",
                "Comprehensive review that highlighted both strengths and improvement opportunities.",
                "Thank you to the audit team for their thorough and professional approach.",
                "The recommendations are practical and will be implemented according to the timeline.",
                "This audit has provided valuable feedback for our continuous improvement efforts."
            },
            _ => new[]
            {
                "General comment regarding the audit process and outcomes.",
                "Feedback on the audit methodology and team performance.",
                "Observations about the overall audit experience and value."
            }
        };

        return comments[_random.Next(comments.Length)];
    }

    private string GenerateAuditSummary(AuditType type)
    {
        var summaries = new Dictionary<AuditType, string[]>
        {
            [AuditType.Safety] = new[]
            {
                "Overall safety management system demonstrates strong commitment to worker protection with minor areas for improvement.",
                "Safety procedures are well-established and generally followed, with opportunities to enhance training effectiveness.",
                "Good safety culture evident throughout the organization with some procedural gaps identified."
            },
            [AuditType.Environmental] = new[]
            {
                "Environmental management practices meet regulatory requirements with opportunities for sustainability improvements.",
                "Strong environmental compliance demonstrated with recommendations for enhanced monitoring procedures.",
                "Environmental controls are effective with minor adjustments needed for optimization."
            },
            [AuditType.Equipment] = new[]
            {
                "Equipment maintenance programs are robust with some opportunities for predictive maintenance integration.",
                "Safety-critical equipment is well-maintained with minor documentation improvements needed.",
                "Equipment inspection procedures are comprehensive with opportunities for digitization."
            },
            [AuditType.Process] = new[]
            {
                "Process management systems are well-documented with opportunities for continuous improvement integration.",
                "Operational procedures are consistently followed with minor gaps in documentation currency.",
                "Process controls are effective with recommendations for enhanced monitoring and measurement."
            },
            [AuditType.Compliance] = new[]
            {
                "Regulatory compliance framework is robust with strong adherence to applicable standards.",
                "Compliance monitoring systems are effective with opportunities for automated tracking improvements.",
                "Legal requirements are well-managed with minor documentation updates needed."
            },
            [AuditType.Fire] = new[]
            {
                "Fire safety systems are well-maintained with effective emergency response procedures in place.",
                "Fire prevention measures are comprehensive with opportunities for enhanced training programs.",
                "Fire protection equipment is properly serviced with minor improvements needed in inspection documentation."
            },
            [AuditType.Chemical] = new[]
            {
                "Chemical management systems demonstrate strong control over hazardous materials with minor procedural improvements.",
                "Chemical storage and handling procedures are well-implemented with opportunities for enhanced monitoring.",
                "Chemical inventory management is effective with recommendations for improved tracking systems."
            },
            [AuditType.Ergonomic] = new[]
            {
                "Ergonomic assessments show good workplace design with opportunities for employee training enhancement.",
                "Manual handling procedures are well-established with minor improvements needed in risk assessment documentation.",
                "Workplace ergonomics demonstrate strong commitment to employee wellbeing with some optimization opportunities."
            },
            [AuditType.Emergency] = new[]
            {
                "Emergency preparedness systems are comprehensive with effective response procedures and regular training.",
                "Emergency response capabilities are well-developed with opportunities for communication system improvements.",
                "Crisis management procedures are robust with minor updates needed for business continuity planning."
            },
            [AuditType.Management] = new[]
            {
                "Management systems demonstrate strong leadership commitment with effective governance structures in place.",
                "Management review processes are well-established with opportunities for enhanced performance monitoring.",
                "Organizational management is effective with recommendations for improved communication and decision-making processes."
            }
        };

        var typeSpecific = summaries.ContainsKey(type) ? summaries[type] : summaries[AuditType.Safety];
        return typeSpecific[_random.Next(typeSpecific.Length)];
    }

    private string GenerateRecommendations(AuditType type)
    {
        var recommendations = new[]
        {
            "Implement regular refresher training programs to maintain competency levels.",
            "Consider digitizing manual processes to improve efficiency and accuracy.",
            "Enhance communication procedures to ensure timely information flow.",
            "Develop key performance indicators to measure continuous improvement.",
            "Strengthen contractor management procedures and oversight.",
            "Implement regular management review processes for system effectiveness."
        };

        var selected = recommendations.OrderBy(x => _random.Next()).Take(_random.Next(2, 4));
        return string.Join(" ", selected);
    }

    private string GenerateFindingTitle(FindingType type, FindingSeverity severity)
    {
        var titles = new Dictionary<FindingType, string[]>
        {
            [FindingType.NonConformance] = new[] { "Procedure not followed", "Missing documentation", "Regulatory requirement not met" },
            [FindingType.OpportunityForImprovement] = new[] { "Process improvement opportunity", "Efficiency enhancement potential", "Best practice implementation" },
            [FindingType.Observation] = new[] { "Good practice noted", "Positive trend observed", "Effective control identified" },
            [FindingType.PositiveFinding] = new[] { "Exemplary performance", "Best practice implementation", "Outstanding compliance achievement" },
            [FindingType.CriticalNonConformance] = new[] { "Critical safety violation", "Immediate risk to personnel", "Major regulatory breach" }
        };

        var typeSpecific = titles.ContainsKey(type) ? titles[type] : titles[FindingType.NonConformance];
        return $"{severity} - {typeSpecific[_random.Next(typeSpecific.Length)]}";
    }

    private string GenerateFindingDescription(FindingType type, FindingSeverity severity)
    {
        return $"Detailed description of {type.ToString().ToLower()} finding with {severity.ToString().ToLower()} impact on operations and compliance.";
    }

    private string GenerateImmediateAction(FindingSeverity severity)
    {
        return severity switch
        {
            FindingSeverity.Critical => "Immediate shutdown and isolation required until corrective actions completed.",
            FindingSeverity.Major => "Restrict access and implement temporary controls while addressing root cause.",
            FindingSeverity.Moderate => "Implement interim controls and monitor closely during correction period.",
            FindingSeverity.Minor => "Continue operations with enhanced monitoring during correction.",
            _ => "No immediate action required, address during normal operations."
        };
    }

    private string GenerateCorrectiveAction(FindingType type)
    {
        var actions = new Dictionary<FindingType, string[]>
        {
            [FindingType.NonConformance] = new[] { "Update procedures", "Provide additional training", "Implement controls" },
            [FindingType.OpportunityForImprovement] = new[] { "Evaluate improvement options", "Conduct cost-benefit analysis", "Pilot enhancement" },
            [FindingType.Observation] = new[] { "Share best practice", "Document effective control", "Consider standardization" },
            [FindingType.PositiveFinding] = new[] { "Share best practice across organization", "Document success factors", "Recognize exemplary performance" },
            [FindingType.CriticalNonConformance] = new[] { "Immediate shutdown and isolation", "Emergency corrective action", "Critical control implementation" }
        };

        var typeSpecific = actions.ContainsKey(type) ? actions[type] : actions[FindingType.NonConformance];
        return $"{typeSpecific[_random.Next(typeSpecific.Length)]} and verify effectiveness through follow-up assessment.";
    }

    private string GenerateItemComment(AuditResult result)
    {
        return result switch
        {
            AuditResult.Compliant => "Requirements fully met with evidence of effective implementation.",
            AuditResult.PartiallyCompliant => "Most requirements met with minor gaps requiring attention.",
            AuditResult.NonCompliant => "Significant gaps identified requiring immediate corrective action.",
            AuditResult.NotApplicable => "This requirement does not apply to current operations.",
            _ => "Assessment completed according to established criteria."
        };
    }

    private AuditPriority ConvertSeverityToPriority(FindingSeverity severity)
    {
        return severity switch
        {
            FindingSeverity.Critical => AuditPriority.Critical,
            FindingSeverity.Major => AuditPriority.High,
            FindingSeverity.Moderate => AuditPriority.Medium,
            _ => AuditPriority.Low
        };
    }

    private RiskLevel ConvertSeverityToRiskLevel(FindingSeverity severity)
    {
        return severity switch
        {
            FindingSeverity.Critical => RiskLevel.Critical,
            FindingSeverity.Major => RiskLevel.High,
            FindingSeverity.Moderate => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    private T GetRandomEnumValue<T>() where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values[_random.Next(values.Length)];
    }
}