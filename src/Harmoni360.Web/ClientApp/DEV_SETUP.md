# Harmoni360 Development Setup

## Running the Application

### Option 1: Separate Backend and Frontend (Recommended for Development)

1. **Start the Backend API** (from the project root):
   ```bash
   cd src/Harmoni360.Web
   dotnet run
   ```
   The API will start on `http://localhost:5000`

2. **Start the Frontend Dev Server** (in a new terminal):
   ```bash
   cd src/Harmoni360.Web/ClientApp
   npm run dev
   ```
   The frontend will start on `http://localhost:5173`

3. **Access the application** at `http://localhost:5173`

### Option 2: Running Both Together

If you're running the .NET application with the SPA proxy:
1. Make sure the backend is configured to serve the SPA
2. Access the application at `http://localhost:5000`

## Common Issues

### 401 Unauthorized Error
If you see 401 errors in the console:
1. Make sure you're logged in first - navigate to `/login`
2. Use the test credentials from `docs/Guides/Seeded_Users.md`
3. Check that the API is running on the correct port

### Port Conflicts
- Frontend dev server: Port 5173
- Backend API: Port 5000
- If you're accessing on port 8080, ensure your proxy is configured correctly

### API Proxy Configuration
The Vite dev server is configured to proxy API calls:
- `/api/*` requests → `http://localhost:5000/api/*`
- `/hubs/*` requests → `http://localhost:5000/hubs/*`

## Login Credentials
Default test users are available in `docs/Guides/Seeded_Users.md`

## Build for Production
```bash
cd src/Harmoni360.Web/ClientApp
npm run build
```
The built files will be in the `dist` directory.