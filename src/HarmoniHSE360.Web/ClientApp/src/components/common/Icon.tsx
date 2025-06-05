import React from 'react';
import CIcon from '@coreui/icons-react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';

interface IconProps {
  icon: any;
  size?: 'sm' | 'lg' | '1x' | '2x' | '3x' | '4x' | '5x';
  className?: string;
}

// Helper component that handles both CoreUI and FontAwesome icons
export const Icon: React.FC<IconProps> = ({
  icon,
  size = 'sm',
  className = '',
}) => {
  // Check if it's a FontAwesome icon (has iconName property)
  if (icon && typeof icon === 'object' && 'iconName' in icon) {
    const faSize = size === 'sm' ? '1x' : size === 'lg' ? '2x' : size;
    return (
      <FontAwesomeIcon
        icon={icon as IconDefinition}
        size={faSize as any}
        className={className}
      />
    );
  }

  // Otherwise use CoreUI icon
  return <CIcon icon={icon} size={size} className={className} />;
};
