using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using MediatR;

namespace Harmoni360.Application.Features.Configuration.Commands;

public class CreateDepartmentCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HeadOfDepartment { get; set; }
    public string? Contact { get; set; }
    public string? Location { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = Department.Create(
            request.Name,
            request.Code,
            request.Description,
            request.HeadOfDepartment,
            request.Contact,
            request.Location,
            request.DisplayOrder,
            request.IsActive);

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        return department.Id;
    }
}