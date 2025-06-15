using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Configuration.Commands;

public class UpdateDepartmentCommand : IRequest<Unit>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HeadOfDepartment { get; set; }
    public string? Contact { get; set; }
    public string? Location { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department == null)
        {
            throw new ArgumentException($"Department with ID {request.Id} not found");
        }

        department.Update(
            request.Name,
            request.Code,
            request.Description,
            request.HeadOfDepartment,
            request.Contact,
            request.Location,
            request.DisplayOrder,
            request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}