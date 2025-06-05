import React from 'react';
import { CCard, CCardBody, CCardHeader, CListGroup, CListGroupItem, CBadge, CSpinner } from '@coreui/react';
import { formatDistanceToNow } from 'date-fns';

interface RecentItem {
  id: number;
  title: string;
  subtitle?: string;
  status: string;
  statusColor: string;
  timestamp: string;
  isOverdue?: boolean;
  onClick?: () => void;
}

interface RecentItemsListProps {
  title: string;
  items: RecentItem[];
  isLoading?: boolean;
  emptyMessage?: string;
  maxItems?: number;
  showAllLink?: {
    text: string;
    onClick: () => void;
  };
  className?: string;
}

const RecentItemsList: React.FC<RecentItemsListProps> = ({
  title,
  items,
  isLoading = false,
  emptyMessage = 'No items found',
  maxItems = 5,
  showAllLink,
  className = ''
}) => {
  const displayItems = items.slice(0, maxItems);

  return (
    <CCard className={`h-100 ${className}`}>
      <CCardHeader className="d-flex justify-content-between align-items-center">
        <h5 className="card-title mb-0">{title}</h5>
        {showAllLink && (
          <button 
            className="btn btn-link btn-sm p-0"
            onClick={showAllLink.onClick}
          >
            {showAllLink.text}
          </button>
        )}
      </CCardHeader>
      <CCardBody className="p-0">
        {isLoading ? (
          <div className="d-flex justify-content-center py-4">
            <CSpinner color="primary" />
          </div>
        ) : displayItems.length === 0 ? (
          <div className="text-center text-medium-emphasis py-4">
            {emptyMessage}
          </div>
        ) : (
          <CListGroup flush>
            {displayItems.map((item) => (
              <CListGroupItem
                key={item.id}
                className={`d-flex justify-content-between align-items-start border-start-0 border-end-0 ${
                  item.onClick ? 'list-group-item-action' : ''
                }`}
                style={item.onClick ? { cursor: 'pointer' } : undefined}
                onClick={item.onClick}
              >
                <div className="me-auto">
                  <div className="fw-semibold">
                    {item.title}
                    {item.isOverdue && (
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
                  <div className="text-muted small mt-1">
                    {formatDistanceToNow(new Date(item.timestamp), { addSuffix: true })}
                  </div>
                </div>
                <CBadge color={item.statusColor} className="ms-2">
                  {item.status}
                </CBadge>
              </CListGroupItem>
            ))}
          </CListGroup>
        )}
      </CCardBody>
    </CCard>
  );
};

export default RecentItemsList;