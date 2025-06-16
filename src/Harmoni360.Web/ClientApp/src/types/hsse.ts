export interface HsseStatisticsDto {
  totalIncidents: number;
  totalHazards: number;
  totalSecurityIncidents: number;
  totalHealthIncidents: number;
  trir: number;
  ltifr: number;
  severityRate: number;
  complianceRate: number;
}

export interface HsseTrendPointDto {
  period: string;
  periodLabel: string;
  incidentCount: number;
  hazardCount: number;
  securityIncidentCount: number;
  healthIncidentCount: number;
}

export interface KpiInputs {
  hoursWorked: number;
  lostTimeInjuries: number;
  daysLost: number;
  compliantRecords: number;
  totalRecords: number;
}

export interface KpiTarget {
  trir: number;
  ltifr: number;
  severityRate: number;
  complianceRate: number;
}

export interface KpiMetric {
  value: number;
  target: number;
  title: string;
  description: string;
  benchmark?: string;
  isGoodDirectionLow: boolean;
}
