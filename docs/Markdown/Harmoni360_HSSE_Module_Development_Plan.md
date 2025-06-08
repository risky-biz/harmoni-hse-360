
**Harmoni360 HSSE Module Development Plan Extension**

**Objective:**
To expand the current Harmoni360 HSE platform into a full-featured HSSE (Health, Safety, Security, and Environment) platform by adding essential security and shared modules that address real-world incident patterns and operational risks observed in the provided dashboards.

---

### Summary View: Prioritized HSSE Module Additions

| # | Module Name                        | Type      | Priority     |
|---|------------------------------------|-----------|--------------|
| 1 | Security Risk Assessment & Threat Modeling | Security  | 🔴 High |
| 2 | Security Incident Management       | Security  | 🔴 High |
| 3 | CCTV & Surveillance Integration    | Security  | 🔴 High |
| 4 | Visitor & Access Control           | Security  | 🔴 High |
| 5 | Emergency Response System          | Shared    | 🔴 High |
| 6 | CAPA & Investigation               | Shared    | 🟠 Med-High |
| 7 | Guard Tour Management              | Security  | 🟠 Medium |
| 8 | Lab/Equipment Safety Compliance    | Safety    | 🟠 Medium |

---

## Module Descriptions and Implementation Guidance

### 1. Security Risk Assessment & Threat Modeling (Security)
**Purpose:** Proactively identify, evaluate, and mitigate physical and operational threats to facilities, assets, and people.
- Threat identification (intrusion, sabotage, insider threat)
- Risk matrix (likelihood × impact)
- Control measure recommendations
- Scenario simulations and threat modeling
- Integration with Security Incident module for reactive + proactive coverage

### 2. Security Incident Management (Security)
**Purpose:** Report, track, and resolve security-related incidents (trespassing, assault, vandalism, etc.)
- Categorization: Minor / Moderate / Major
- Attach evidence (CCTV footage, images, witness reports)
- Escalation flows with tracking
- Role-based access for Security Team

### 3. CCTV & Surveillance Integration (Security)
**Purpose:** Link surveillance footage to incidents
- Tagging capability (time, location, user)
- View or attach relevant video/image files
- Access control for sensitive footage
- Optional future: AI-based anomaly detection

### 4. Visitor & Access Control (Security)
**Purpose:** Track visitor and contractor entry/exit activity
- Visitor pre-registration + QR codes
- Entry/exit logging (manual or RFID scan)
- Approval workflow for restricted areas
- Integration with student/staff directory

### 5. Emergency Response System (Shared)
**Purpose:** Unified response for HSE + Security incidents
- Emergency plan documentation
- Broadcast alerts (email/SMS/WhatsApp)
- Evacuation tracking
- Post-event analysis and report generation
- Drill scheduling & scoring

### 6. CAPA & Investigation (Shared)
**Purpose:** Conduct root cause analysis and assign corrective/preventive actions
- Link to incident or hazard ID
- Templates: 5 Whys, Fishbone, Timeline
- Assignment to departments/personnel
- Follow-up tracking with deadlines
- Integration with training if retraining is required

### 7. Guard Tour Management (Security)
**Purpose:** Digitize and monitor guard patrol rounds
- Route definition with checkpoints (QR/NFC)
- Schedule tracking and missed patrol alerts
- Time-stamped logs
- Supervisor review interface

### 8. Lab/Equipment Safety Compliance (Safety)
**Purpose:** Address frequent incidents related to lab equipment and technical areas
- Equipment inspection checklist
- Maintenance logging
- Flag issues for resolution
- Incident linkage for traceability

---

## Development Sequence (Recommended Sprint Order)
**Sprint 1–2:**
- Security Risk Assessment & Threat Modeling
- Security Incident Management

**Sprint 3–4:**
- CCTV Integration (backend only, attach footage)
- Visitor & Access Control

**Sprint 5–6:**
- Emergency Response System
- CAPA & Investigation Module

**Sprint 7:**
- Guard Tour Management
- Lab/Equipment Safety Compliance

---

## Notes for Developers
- Use existing modular monolith structure
- All modules must include:
  - CRUD with audit trail
  - Role-based authorization
  - API endpoints + Swagger docs
  - SignalR real-time updates if applicable
- Store file uploads (e.g., CCTV images) in secure storage (S3/local + encrypted)
- Ensure separation of concerns between Security and HSE incidents, but allow unified dashboard view
