using MediatR;
using Harmoni360.Application.Features.Authentication.DTOs;

namespace Harmoni360.Application.Features.Authentication.Commands;

public record LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; } = false;
}