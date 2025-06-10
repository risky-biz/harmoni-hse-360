import React from 'react';
import { CListGroup, CListGroupItem, CBadge } from '@coreui/react';
import { formatDistanceToNow } from 'date-fns';

interface RecentItem {
  id: string;
  title: string;
  subtitle?: string;
  metadata?: Record<string, any>;
  clickAction?: () => void;
}

interface RecentItemsListProps {
  items: RecentItem[];
  maxItems?: number;
  showTimestamp?: boolean;
  emptyMessage?: string;
  className?: string;
}

const RecentItemsList: React.FC<RecentItemsListProps> = ({
  items,
  maxItems = 5,
  showTimestamp = true,
  emptyMessage = 'No recent items',
  className = ''
}) => {
  const displayItems = items.slice(0, maxItems);

  if (displayItems.length === 0) {
    return (
      <div className="text-center text-medium-emphasis py-4">
        {emptyMessage}
      </div>
    );
  }

  return (
    <CListGroup flush className={className}>
      {displayItems.map((item) => (
        <CListGroupItem
          key={item.id}
          className={`d-flex justify-content-between align-items-start border-start-0 border-end-0 ${
            item.clickAction ? 'list-group-item-action' : ''
          }`}
          style={item.clickAction ? { cursor: 'pointer' } : undefined}
          onClick={item.clickAction}
        >
          <div className="me-auto">
            <div className="fw-semibold">
              {item.title}
              {item.metadata?.isOverdue && (
                <CBadge color="danger" className="ms-2">
                  Overdue
                </CBadge>
              )}
            </div>
            {item.subtitle && (
              <div className="text-medium-emphasis small mt-1">
                {item.subtitle}
              </div>
            )}
            {showTimestamp && item.metadata?.timestamp && (
              <div className="text-muted small mt-1">
                {(() => {
                  try {
                    const date = new Date(item.metadata.timestamp);
                    return isNaN(date.getTime()) 
                      ? 'Unknown date' 
                      : formatDistanceToNow(date, { addSuffix: true });
                  } catch {
                    return 'Unknown date';
                  }
                })()}
              </div>
            )}
          </div>
          {item.metadata?.status && (
            <CBadge color={item.metadata.statusColor || 'primary'} className="ms-2">
              {item.metadata.status}
            </CBadge>
          )}
        </CListGroupItem>
      ))}
    </CListGroup>
  );
};

export default RecentItemsList;