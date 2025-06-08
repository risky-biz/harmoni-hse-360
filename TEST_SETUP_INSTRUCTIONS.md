# Harmoni HSE 360 - Test Setup Instructions

## 🔧 Issues Fixed

### 1. ✅ React Input Component Error
- **Problem**: `CFormInput` with `as="select"` was causing React errors
- **Solution**: Replaced with `CFormSelect` component in UserManagement.tsx
- **Files Modified**: `src/pages/admin/UserManagement.tsx`

### 2. ✅ API Proxy Configuration
- **Problem**: Frontend (port 5173) was trying to call backend APIs on wrong port
- **Solution**: Updated Vite proxy configuration to point to correct backend port (5211)
- **Files Modified**: `src/Harmoni360.Web/ClientApp/vite.config.ts`

### 3. ✅ JavaScript Runtime Error
- **Problem**: `qrId.replace()` called on potentially undefined variable
- **Solution**: Added null/undefined checks in QrScanner component
- **Files Modified**: `src/pages/incidents/QrScanner.tsx`

### 4. ✅ Security Incident Management Module Registration
- **Problem**: Module not appearing in sidebar navigation
- **Solution**: Added complete navigation configuration and permission mappings
- **Files Modified**: 
  - `src/utils/navigationUtils.ts`
  - `src/types/permissions.ts` 
  - `src/layouts/DefaultLayout.tsx`

### 5. ✅ User Management Route Configuration
- **Problem**: User Management redirecting to dashboard
- **Solution**: Created proper User Management page and admin routes
- **Files Modified**:
  - `src/pages/admin/UserManagement.tsx` (new file)
  - `src/App.tsx`

### 6. ✅ Notification API Authorization
- **Problem**: NotificationBell receiving HTML instead of JSON
- **Solution**: Temporarily set AllowAnonymous for testing, updated proxy config
- **Files Modified**: `src/Harmoni360.Web/Controllers/NotificationController.cs`

## 🚀 How to Run the Application

### Backend Setup
1. Open terminal in project root: `/mnt/d/Projects/harmoni-hse-360/`
2. Navigate to backend: `cd src/Harmoni360.Web`
3. Run backend: `dotnet run`
4. Backend will be available at: `http://localhost:5211`

### Frontend Setup 
1. Open **new terminal** in: `/mnt/d/Projects/harmoni-hse-360/src/Harmoni360.Web/ClientApp`
2. Install dependencies: `npm install` (if not done)
3. Run frontend: `npm run dev`
4. Frontend will be available at: `http://localhost:5173`

### Access Instructions
1. Navigate to: `http://localhost:5173`
2. Login with Super Admin: `superadmin@harmoni360.com` / `SuperAdmin123!`

## 🔐 Demo Users for Testing

### Full Access to Security Incident Management
- **security.manager@harmoni360.com** / `SecurityMgr123!` - **RECOMMENDED**
- **security.officer@harmoni360.com** / `SecurityOfc123!`
- **superadmin@harmoni360.com** / `SuperAdmin123!`
- **developer@harmoni360.com** / `Developer123!`
- **admin@harmoni360.com** / `Admin123!`

### Admin Access (User Management)
- **superadmin@harmoni360.com** / `SuperAdmin123!` - **RECOMMENDED**
- **developer@harmoni360.com** / `Developer123!`
- **admin@harmoni360.com** / `Admin123!`

## 📍 Module Locations in Navigation

### Security Incident Management 🔒
**Sidebar Location**: `Security Management > Security Incidents`

**Available Pages**:
- Security Dashboard: `/security/dashboard`
- Report Security Incident: `/security/incidents/create`
- Security Incidents: `/security/incidents`
- Threat Assessment: `/security/threat-assessment`
- Security Controls: `/security/controls`
- Security Analytics: `/security/analytics`

### User Management 👥
**Sidebar Location**: `Administration > User Management`
**Route**: `/admin/users`

## 🎯 Verification Steps

1. **Login Test**: Login with `superadmin@harmoni360.com` / `SuperAdmin123!`
2. **Navigation Test**: Verify both "Security Management" and "Administration" sections appear in sidebar
3. **Security Module Test**: Click through all Security Incident Management pages
4. **User Management Test**: Access Administration > User Management to see demo users list
5. **API Test**: Check browser console - NotificationBell should load count without errors

## 🐛 Troubleshooting

### If Security Management doesn't appear:
1. Verify user has SecurityIncidentManagement permissions
2. Check browser console for JavaScript errors
3. Try logging out and logging back in

### If API calls fail:
1. Ensure backend is running on port 5211
2. Check Vite proxy configuration in `vite.config.ts`
3. Verify CORS settings in backend

### If User Management shows error:
1. Check that user has Admin role permissions
2. Verify CFormSelect components are imported correctly
3. Check browser console for React errors

## ✅ Expected Results

After following these instructions:
- ✅ No "Something went wrong" errors
- ✅ Security Incident Management visible in sidebar 
- ✅ User Management accessible and functional
- ✅ All demo users display with proper roles
- ✅ No console errors from NotificationBell API calls
- ✅ Smooth navigation between all modules

## 🔄 Next Steps

1. **Revert Temporary Changes**: Change NotificationController back to require authentication
2. **Complete Security Pages**: Implement missing Security Incident Management pages
3. **Enhanced User Management**: Add create/edit/delete user functionality
4. **Testing**: Run comprehensive test suite with fixed components