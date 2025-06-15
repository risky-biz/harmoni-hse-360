using MediatR;

namespace Harmoni360.Application.Features.WorkPermits.Queries
{
    public class GetWorkPermitStatisticsQuery : IRequest<WorkPermitStatisticsDto>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class WorkPermitStatisticsDto
    {
        public int TotalPermits { get; set; }
        public int CompletedPermits { get; set; }
        public int SafelyCompletedPermits { get; set; }
        public int OverduePermits { get; set; }
        public int HighRiskPermits { get; set; }
        public double AverageCompletionTime { get; set; }
        public double CompletionRate { get; set; }
        public double SafetyRate { get; set; }
        
        public Dictionary<string, int> PermitsByType { get; set; } = new();
        public Dictionary<string, int> PermitsByStatus { get; set; } = new();
        public Dictionary<string, int> PermitsByDepartment { get; set; } = new();
        public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
    }

    public class MonthlyTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public int TotalPermits { get; set; }
        public int CompletedPermits { get; set; }
        public int SafelyCompletedPermits { get; set; }
    }
}