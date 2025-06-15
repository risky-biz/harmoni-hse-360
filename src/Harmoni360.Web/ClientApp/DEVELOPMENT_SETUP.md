# Development Setup Guide

## Backend API Setup

To use Harmoni360 with real data instead of mock data, you need to ensure the backend is running and the database is properly seeded.

### Prerequisites

1. **Backend Server Running**: The .NET backend API must be running on the expected port
2. **Database Connection**: Ensure PostgreSQL database is accessible
3. **Database Seeded**: The application automatically seeds demo data in development mode

### Database Seeding

The application automatically seeds comprehensive demo data when running in development mode, including:

- **Users**: Various roles (Admin, HSE Manager, HSE Officer, Employees)
- **Incidents**: Sample incident reports with different severities and statuses
- **Hazards**: Hazard reports with risk assessments and mitigation actions
- **PPE Data**: PPE items, categories, and assignments
- **Health Records**: Employee health records and vaccination data
- **Security Incidents**: Security-related incidents and threat assessments

### Configuration

Data seeding is controlled by configuration settings in `appsettings.Development.json`:

```json
{
  "DataSeeding": {
    "SeedIncidents": "true",
    "SeedHazards": "true", 
    "SeedHealthData": "true",
    "SeedPPEData": "true",
    "SeedPPEItems": "true",
    "SeedExtendedData": "true",
    "ReSeedIncidents": "false"
  }
}
```

### Troubleshooting

If you see console warnings about APIs being unavailable:

1. **Check Backend Status**: Ensure the .NET API is running
2. **Verify Database Connection**: Check if the database is accessible
3. **Check Seeding**: Look for seeding logs in the backend console
4. **Clear Browser Cache**: Refresh browser cache if needed

### Console Messages

The frontend will show helpful console messages when APIs are unavailable:

- `NotificationBell: API unavailable. Please ensure the backend is running and database is seeded.`
- `DashboardService: API unavailable. Please ensure the backend is running and database is seeded.`
- `IncidentAPI: API unavailable. Please ensure the backend is running and database is seeded.`

### Running with Real Data

1. Start the backend API server
2. The database will be automatically migrated and seeded on startup
3. Start the frontend development server
4. Navigate to the application - all data will be real seeded data

### Re-seeding Data

To re-seed the database with fresh data:

1. Set `"ReSeedIncidents": "true"` in configuration
2. Restart the backend application
3. The seeder will clear and regenerate all incident data

This ensures you always have realistic, comprehensive test data for development and demo purposes.