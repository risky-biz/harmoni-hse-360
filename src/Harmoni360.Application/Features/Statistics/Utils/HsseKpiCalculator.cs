using System;

namespace Harmoni360.Application.Features.Statistics.Utils;

public static class HsseKpiCalculator
{
    public static double CalculateTrir(int recordableIncidents, int hoursWorked)
    {
        if (hoursWorked <= 0) return 0;
        return recordableIncidents * 200_000d / hoursWorked;
    }

    public static double CalculateLtifr(int lostTimeInjuries, int hoursWorked)
    {
        if (hoursWorked <= 0) return 0;
        return lostTimeInjuries * 1_000_000d / hoursWorked;
    }

    public static double CalculateSeverityRate(int daysLost, int hoursWorked)
    {
        if (hoursWorked <= 0) return 0;
        return daysLost * 200_000d / hoursWorked;
    }

    public static double CalculateComplianceRate(int compliantRecords, int totalRecords)
    {
        if (totalRecords <= 0) return 0;
        return compliantRecords * 100d / totalRecords;
    }
}
