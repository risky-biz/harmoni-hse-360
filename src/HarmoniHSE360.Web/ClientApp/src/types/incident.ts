export interface UserDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  fullName: string;
}

export interface InvolvedPersonDto {
  id: number;
  person: UserDto;
  involvementType: string;
  injuryDescription?: string;
}

export interface IncidentDetailDto {
  id: number;
  title: string;
  description: string;
  severity: 'Minor' | 'Moderate' | 'Serious' | 'Critical';
  status:
    | 'Reported'
    | 'UnderInvestigation'
    | 'AwaitingAction'
    | 'Resolved'
    | 'Closed';
  incidentDate: string;
  location: string;
  reporterId?: number;
  reporterName: string;
  reporterEmail?: string;
  reporterDepartment?: string;
  investigatorId?: number;
  investigatorName?: string;
  createdAt: string;
  lastModifiedAt?: string;
  latitude?: number;
  longitude?: number;
  injuryType?: string;
  medicalTreatmentProvided?: boolean;
  emergencyServicesContacted?: boolean;
  witnessNames?: string;
  immediateActionsTaken?: string;
  involvedPersons: InvolvedPersonDto[];
  attachments: any[]; // TODO: Define attachment type
  correctiveActions: any[]; // TODO: Define corrective action type
}
