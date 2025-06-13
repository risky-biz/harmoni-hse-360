using Harmoni360.Application.Features.WorkPermits.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Commands
{
    public class CompleteWorkCommand : IRequest<WorkPermitDto>
    {
        public int Id { get; set; }
        public string CompletionNotes { get; set; } = string.Empty;
        public bool IsCompletedSafely { get; set; }
        public string LessonsLearned { get; set; } = string.Empty;
    }
}