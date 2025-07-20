using Harmoni360.Application.Common.Attributes;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WorkflowManagement;

/// <summary>
/// Workflow Management Module Definition - Elsa workflow engine integration
/// </summary>
[Module(
    ModuleType.WorkflowManagement,
    "Workflow Management",
    "Process automation and workflow management using Elsa workflow engine",
    "fas fa-project-diagram",
    displayOrder: 21,
    isEnabledByDefault: false, // Optional module - can be enabled as needed
    canBeDisabled: true
)]
public class WorkflowManagementModule
{
    // Module for workflow management and process automation
}