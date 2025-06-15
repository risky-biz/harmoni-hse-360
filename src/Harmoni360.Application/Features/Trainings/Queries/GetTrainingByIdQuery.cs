using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;

namespace Harmoni360.Application.Features.Trainings.Queries;

public class GetTrainingByIdQuery : IRequest<TrainingDto?>
{
    public int Id { get; set; }
    public bool IncludeParticipants { get; set; } = true;
    public bool IncludeRequirements { get; set; } = true;
    public bool IncludeAttachments { get; set; } = true;
    public bool IncludeComments { get; set; } = true;
    public bool IncludeCertifications { get; set; } = true;

    public GetTrainingByIdQuery() { }

    public GetTrainingByIdQuery(int id)
    {
        Id = id;
    }
}