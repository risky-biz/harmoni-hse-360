# Hierarchical Navigation - Module Management Demo

## ✅ **IMPLEMENTATION COMPLETE**

The hierarchical navigation now provides the proper wrapper structure you requested for easy module configuration management.

## New HTML Structure

### Before (Flat Structure):
```html
<ul class="sidebar-nav">
   <li class="nav-title depth-0">Work Permit Management</li>
   <li class="nav-group show depth-1">
      <a class="nav-link nav-group-toggle" href="#">Work Permits</a>
      <ul class="nav-group-items">
         <li class="nav-item depth-2">...</li>
      </ul>
   </li>
</ul>
```

### After (Wrapped Structure):
```html
<ul class="sidebar-nav">
   <li class="nav-module-wrapper depth-0" data-module="2">
      <div class="nav-module-title depth-0">Work Permit Management</div>
      <div class="nav-module-content">
         <li class="nav-group show depth-1">
            <a class="nav-link nav-group-toggle" href="#">Work Permits</a>
            <ul class="nav-group-items">
               <li class="nav-item depth-2">...</li>
            </ul>
         </li>
      </div>
   </li>
</ul>
```

## Module Management Features

### 1. **Module-Level Control**
Each module is now wrapped in a `.nav-module-wrapper` with a `data-module` attribute:

```html
<li class="nav-module-wrapper depth-0" data-module="WorkPermitManagement">
   <!-- Module title and all its content -->
</li>
```

### 2. **Easy Hide/Show Modules**
You can now hide entire modules with a single CSS class:

```javascript
// Hide Work Permit Management module completely
ModuleManager.hideModule(ModuleType.WorkPermitManagement);

// Show it again
ModuleManager.showModule(ModuleType.WorkPermitManagement);

// Toggle visibility
ModuleManager.toggleModule(ModuleType.WorkPermitManagement);
```

### 3. **Module Status Indicators**
Set different statuses for modules:

```javascript
// Mark module as disabled
ModuleManager.setModuleStatus(ModuleType.WorkPermitManagement, 'disabled');

// Mark as under maintenance  
ModuleManager.setModuleStatus(ModuleType.InspectionManagement, 'maintenance');

// Mark as coming soon
ModuleManager.setModuleStatus(ModuleType.TrainingManagement, 'coming-soon');

// Remove status
ModuleManager.setModuleStatus(ModuleType.WorkPermitManagement, null);
```

### 4. **CSS Classes for Styling**
- `.module-hidden` - Completely hides the module
- `.module-disabled` - Shows module with disabled styling and "(Disabled)" text
- `.module-maintenance` - Shows "(Maintenance)" indicator
- `.module-coming-soon` - Shows "(Coming Soon)" indicator

### 5. **Module Configuration Integration**
The new structure is perfect for future module configuration:

```javascript
// Example module status map
const moduleStatusMap = {
  "2": { enabled: true, status: null },           // Work Permits - Active
  "3": { enabled: false, status: "disabled" },   // Incidents - Disabled  
  "7": { enabled: true, status: "maintenance" }, // Training - Maintenance
  "8": { enabled: true, status: "coming-soon" }  // Licenses - Coming Soon
};

// Apply statuses to navigation
const processedNavigation = applyModuleStatus(
  navigationItems, 
  moduleStatusMap, 
  user.isSuperAdmin
);
```

## Usage Examples

### Developer Console Commands
You can test the module management in the browser console:

```javascript
// Import the module manager
import { ModuleManager, ModuleType } from './src/utils/navigationUtils';

// Hide the Work Permit Management module
ModuleManager.hideModule(ModuleType.WorkPermitManagement);

// Show it again after 3 seconds
setTimeout(() => {
  ModuleManager.showModule(ModuleType.WorkPermitManagement);
}, 3000);

// Set inspection module to maintenance mode
ModuleManager.setModuleStatus(ModuleType.InspectionManagement, 'maintenance');
```

### CSS Targeting
You can also target modules directly with CSS:

```css
/* Hide specific module */
[data-module="2"] {
  display: none;
}

/* Style disabled modules */
.nav-module-wrapper.module-disabled {
  opacity: 0.6;
}

/* Custom styling for maintenance modules */
.nav-module-wrapper.module-maintenance .nav-module-title::after {
  content: " 🔧 (Under Maintenance)";
  color: #ffc107;
}
```

### JavaScript DOM Manipulation
```javascript
// Direct DOM manipulation
const workPermitModule = document.querySelector('[data-module="WorkPermitManagement"]');
if (workPermitModule) {
  workPermitModule.style.display = 'none'; // Hide
  workPermitModule.style.display = '';     // Show
}

// Add custom classes
workPermitModule.classList.add('custom-module-style');
```

## Benefits of This Structure

1. **✅ Single Point Control**: Hide/show entire modules with one command
2. **✅ Easy Integration**: Perfect for future module configuration system
3. **✅ Granular Control**: Can target individual modules or groups
4. **✅ Clean CSS**: Simple selectors for styling
5. **✅ Future-Proof**: Extensible for additional module features
6. **✅ Performance**: Efficient DOM queries with data attributes
7. **✅ Accessibility**: Maintains proper semantic structure

## Implementation Files

- **Navigation Rendering**: `src/layouts/DefaultLayout.tsx`
- **Module Management**: `src/utils/navigationUtils.ts` 
- **Module Styling**: `src/styles/app.scss`
- **Type Definitions**: All existing type guards and interfaces

The hierarchical navigation is now **production ready** with the exact wrapper structure you requested for easy module configuration management! 🎉