using MediatR;
using HarmoniHSE360.Application.Features.Authentication.DTOs;

namespace HarmoniHSE360.Application.Features.Authentication.Commands;

public record LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; } = false;
}