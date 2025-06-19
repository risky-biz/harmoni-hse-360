using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class CreateWorkPermitSettingCommandHandler : IRequestHandler<CreateWorkPermitSettingCommand, WorkPermitSettingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateWorkPermitSettingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<WorkPermitSettingDto> Handle(CreateWorkPermitSettingCommand request, CancellationToken cancellationToken)
    {
        // If this setting is being set as active, deactivate any existing active setting
        if (request.IsActive)
        {
            var existingActiveSetting = await _context.WorkPermitSettings
                .Where(wps => wps.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingActiveSetting != null)
            {
                existingActiveSetting.Deactivate(_currentUserService.Email);
            }
        }

        // Create the new setting using the factory method
        var setting = Harmoni360.Domain.Entities.WorkPermitSettings.Create(
            requireSafetyInduction: request.RequireSafetyInduction,
            enableFormValidation: request.EnableFormValidation,
            allowAttachments: request.AllowAttachments,
            maxAttachmentSize: request.MaxAttachmentSizeMB,
            formInstructions: request.FormInstructions,
            createdBy: _currentUserService.Email
        );

        if (!request.IsActive)
        {
            setting.Deactivate(_currentUserService.Email);
        }

        _context.WorkPermitSettings.Add(setting);
        await _context.SaveChangesAsync(cancellationToken);

        return new WorkPermitSettingDto
        {
            Id = setting.Id,
            RequireSafetyInduction = setting.RequireSafetyInduction,
            EnableFormValidation = setting.EnableFormValidation,
            AllowAttachments = setting.AllowAttachments,
            MaxAttachmentSizeMB = setting.MaxAttachmentSizeMB,
            FormInstructions = setting.FormInstructions,
            IsActive = setting.IsActive,
            SafetyVideos = new List<WorkPermitSafetyVideoDto>(),
            CreatedAt = setting.CreatedAt,
            CreatedBy = setting.CreatedBy,
            LastModifiedAt = setting.LastModifiedAt,
            LastModifiedBy = setting.LastModifiedBy
        };
    }
}