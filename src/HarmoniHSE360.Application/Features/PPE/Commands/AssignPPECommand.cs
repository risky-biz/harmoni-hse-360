using HarmoniHSE360.Application.Features.PPE.DTOs;
using MediatR;

namespace HarmoniHSE360.Application.Features.PPE.Commands;

public record AssignPPECommand : IRequest<PPEAssignmentDto>
{
    public int PPEItemId { get; init; }
    public int AssignedToId { get; init; }
    public string? Purpose { get; init; }
}

public record ReturnPPECommand : IRequest<Unit>
{
    public int PPEItemId { get; init; }
    public string? NewCondition { get; init; }
    public string? ReturnNotes { get; init; }
}

public record MarkPPEAsLostCommand : IRequest<Unit>
{
    public int PPEItemId { get; init; }
    public string? Notes { get; init; }
}

public record UpdatePPEItemCommand : IRequest<PPEItemDto>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Manufacturer { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public string? Color { get; init; }
    public string Location { get; init; } = string.Empty;
    public DateTime? ExpiryDate { get; init; }
    public string? Notes { get; init; }
}

public record DeletePPEItemCommand : IRequest<Unit>
{
    public int Id { get; init; }
}