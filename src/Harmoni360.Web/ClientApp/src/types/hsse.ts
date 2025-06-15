export interface HsseStatisticsDto {
  totalIncidents: number;
  totalHazards: number;
  totalSecurityIncidents: number;
  totalHealthIncidents: number;
}

export interface HsseTrendPointDto {
  period: string;
  periodLabel: string;
  incidentCount: number;
  hazardCount: number;
  securityIncidentCount: number;
  healthIncidentCount: number;
}
