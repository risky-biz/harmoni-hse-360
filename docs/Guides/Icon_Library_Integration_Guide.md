# Icon Library Integration Guide

This guide explains how to use both CoreUI icons and Font Awesome icons together in the Harmoni360 application.

## Overview

Since CoreUI icons have a limited set compared to Font Awesome, we can use both libraries together to get the best of both worlds:
- **CoreUI Icons**: Use for common UI elements and consistency with CoreUI components
- **Font Awesome Icons**: Use for specialized icons not available in CoreUI

## Installation

### 1. Install Font Awesome React Components

```bash
cd src/Harmoni360.Web/ClientApp
npm install --save @fortawesome/fontawesome-svg-core
npm install --save @fortawesome/free-solid-svg-icons
npm install --save @fortawesome/free-regular-svg-icons
npm install --save @fortawesome/free-brands-svg-icons
npm install --save @fortawesome/react-fontawesome
```

### 2. Create Icon Helper Component

Create a unified icon component that can use both libraries:

```typescript
// src/components/common/Icon.tsx
import React from 'react';
import CIcon from '@coreui/icons-react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';

interface IconProps {
  icon: any; // CoreUI icon or Font Awesome icon
  size?: 'sm' | 'lg' | 'xl' | string;
  className?: string;
  color?: string;
}

export const Icon: React.FC<IconProps> = ({ icon, size, className, color }) => {
  // Check if it's a Font Awesome icon
  if (icon && typeof icon === 'object' && 'iconName' in icon) {
    return (
      <FontAwesomeIcon 
        icon={icon as IconDefinition} 
        size={size as any}
        className={className}
        color={color}
      />
    );
  }
  
  // Otherwise, use CoreUI icon
  return (
    <CIcon 
      icon={icon} 
      size={size}
      className={className}
      style={{ color }}
    />
  );
};
```

## Usage Examples

### 1. Basic Usage

```typescript
import React from 'react';
import { Icon } from '../../components/common/Icon';

// CoreUI icons
import { cilUser, cilSettings } from '@coreui/icons';

// Font Awesome icons
import { faSave, faArrowLeft, faCalendarAlt, faMapMarkerAlt } from '@fortawesome/free-solid-svg-icons';
import { faTrashAlt } from '@fortawesome/free-regular-svg-icons';

const MyComponent = () => {
  return (
    <div>
      {/* CoreUI Icons */}
      <Icon icon={cilUser} size="lg" />
      <Icon icon={cilSettings} className="text-primary" />
      
      {/* Font Awesome Icons */}
      <Icon icon={faSave} size="lg" color="green" />
      <Icon icon={faArrowLeft} />
      <Icon icon={faCalendarAlt} className="me-2" />
      <Icon icon={faMapMarkerAlt} size="sm" />
      <Icon icon={faTrashAlt} className="text-danger" />
    </div>
  );
};
```

### 2. Fixing Missing Icons in Harmoni360

Here's how to fix the missing icons in our incident pages:

```typescript
// src/pages/incidents/EditIncident.tsx
import React from 'react';
import { Icon } from '../../components/common/Icon';
import { cilTask } from '@coreui/icons';
import { faSave, faArrowLeft } from '@fortawesome/free-solid-svg-icons';

// Replace missing CoreUI icons with Font Awesome
// Before: import { cilSave, cilArrowLeft } from '@coreui/icons';
// After: Use Font Awesome icons

const EditIncident = () => {
  return (
    <>
      <CButton color="secondary" onClick={() => navigate(-1)}>
        <Icon icon={faArrowLeft} className="me-2" />
        Back
      </CButton>
      
      <CButton color="primary" type="submit">
        <Icon icon={faSave} className="me-2" />
        Save Changes
      </CButton>
    </>
  );
};
```

```typescript
// src/pages/incidents/IncidentDetail.tsx
import React from 'react';
import { Icon } from '../../components/common/Icon';
import { cilWarning } from '@coreui/icons';
import { 
  faArrowLeft, 
  faPencilAlt, 
  faTrash,
  faMapMarkerAlt,
  faCalendarAlt 
} from '@fortawesome/free-solid-svg-icons';

const IncidentDetail = () => {
  return (
    <>
      {/* Navigation */}
      <Icon icon={faArrowLeft} /> Back
      
      {/* Actions */}
      <Icon icon={faPencilAlt} /> Edit
      <Icon icon={faTrash} /> Delete
      
      {/* Details */}
      <Icon icon={faMapMarkerAlt} /> {incident.location}
      <Icon icon={faCalendarAlt} /> {formatDate(incident.incidentDate)}
    </>
  );
};
```

## Icon Mapping Reference

| Purpose | CoreUI Icon | Font Awesome Alternative |
|---------|-------------|-------------------------|
| Save | `cilSave` ❌ | `faSave` ✅ |
| Back Arrow | `cilArrowLeft` ❌ | `faArrowLeft` ✅ |
| Edit/Pencil | `cilPencil` ❌ | `faPencilAlt` ✅ |
| Delete/Trash | `cilTrash` ❌ | `faTrash` or `faTrashAlt` ✅ |
| Location | `cilLocationPin` ❌ | `faMapMarkerAlt` ✅ |
| Calendar | `cilCalendar` ❌ | `faCalendarAlt` ✅ |
| User | `cilUser` ✅ | `faUser` ✅ |
| Settings | `cilSettings` ✅ | `faCog` ✅ |
| Warning | `cilWarning` ✅ | `faExclamationTriangle` ✅ |

## Best Practices

1. **Consistency First**: Use CoreUI icons when available for consistency with CoreUI components
2. **Font Awesome Fallback**: Use Font Awesome for specialized icons not in CoreUI
3. **Icon Size**: Keep icon sizes consistent across the app
4. **Icon Style**: Prefer solid icons for actions, regular for decorative
5. **Performance**: Only import the icons you need to minimize bundle size

## Available Icon Styles

### Font Awesome Free Styles
- **Solid** (`fas`): Filled icons, best for actions
- **Regular** (`far`): Outlined icons, good for secondary actions
- **Brands** (`fab`): Company/brand logos

### CoreUI Icons
- Single style, optimized for UI components

## Performance Considerations

### Tree Shaking
Both libraries support tree shaking, so only imported icons are included in the bundle:

```typescript
// Good - specific imports
import { faSave, faEdit } from '@fortawesome/free-solid-svg-icons';
import { cilUser, cilSettings } from '@coreui/icons';

// Bad - importing entire library
import * as Icons from '@fortawesome/free-solid-svg-icons';
```

### Icon Library Setup (Optional)
For frequently used icons, create an icon library:

```typescript
// src/utils/iconLibrary.ts
import { library } from '@fortawesome/fontawesome-svg-core';
import { 
  faSave, 
  faArrowLeft, 
  faPencilAlt,
  faTrash,
  faMapMarkerAlt,
  faCalendarAlt 
} from '@fortawesome/free-solid-svg-icons';

// Add icons to library
library.add(
  faSave,
  faArrowLeft,
  faPencilAlt,
  faTrash,
  faMapMarkerAlt,
  faCalendarAlt
);

// Then use string references
<FontAwesomeIcon icon="save" />
```

## License Information

- **CoreUI Icons**: MIT License
- **Font Awesome Free**: 
  - Icons: CC BY 4.0 License
  - Code: MIT License
  - Attribution not required but appreciated

## Resources

- [CoreUI Icons Documentation](https://coreui.io/react/docs/components/icon/)
- [Font Awesome Free Icons Gallery](https://fontawesome.com/icons?d=gallery&m=free)
- [Font Awesome React Documentation](https://fontawesome.com/docs/web/use-with/react/)

---

**Last Updated**: December 2024  
**Applies to**: Harmoni360 v1.0