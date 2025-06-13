import React, { useMemo, useCallback, useState, useEffect } from 'react';
import { FixedSizeList as List } from 'react-window';
import { CSpinner, CAlert } from '@coreui/react';

interface VirtualizedListProps<T> {
  items: T[];
  itemHeight: number;
  containerHeight: number;
  renderItem: (props: { index: number; style: React.CSSProperties; data: T[] }) => React.ReactElement;
  className?: string;
  loading?: boolean;
  error?: string;
  onItemsRendered?: (props: {
    overscanStartIndex: number;
    overscanStopIndex: number;
    visibleStartIndex: number;
    visibleStopIndex: number;
  }) => void;
  emptyMessage?: string;
}

function VirtualizedList<T>({
  items,
  itemHeight,
  containerHeight,
  renderItem,
  className = '',
  loading = false,
  error,
  onItemsRendered,
  emptyMessage = 'No items to display'
}: VirtualizedListProps<T>) {
  const [listRef, setListRef] = useState<List | null>(null);

  // Memoize the item data to prevent unnecessary re-renders
  const itemData = useMemo(() => items, [items]);

  // Memoized render function
  const memoizedRenderItem = useCallback(
    (props: { index: number; style: React.CSSProperties }) => {
      return renderItem({ ...props, data: itemData });
    },
    [renderItem, itemData]
  );

  // Handle scroll to item
  const scrollToItem = useCallback((index: number, align: 'auto' | 'start' | 'center' | 'end' = 'auto') => {
    listRef?.scrollToItem(index, align);
  }, [listRef]);

  // Handle items rendered callback
  const handleItemsRendered = useCallback((props: any) => {
    onItemsRendered?.(props);
  }, [onItemsRendered]);

  // Auto-scroll to top when items change
  useEffect(() => {
    if (listRef && items.length > 0) {
      listRef.scrollToItem(0);
    }
  }, [listRef, items.length]);

  if (loading) {
    return (
      <div className={`virtualized-list-loading d-flex align-items-center justify-content-center ${className}`} 
           style={{ height: containerHeight }}>
        <CSpinner size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className={`virtualized-list-error ${className}`} style={{ height: containerHeight }}>
        <CAlert color="danger">{error}</CAlert>
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className={`virtualized-list-empty d-flex align-items-center justify-content-center ${className}`} 
           style={{ height: containerHeight }}>
        <div className="text-muted">{emptyMessage}</div>
      </div>
    );
  }

  return (
    <div className={`virtualized-list ${className}`}>
      <List
        ref={setListRef}
        height={containerHeight}
        itemCount={items.length}
        itemSize={itemHeight}
        itemData={itemData}
        onItemsRendered={handleItemsRendered}
        overscanCount={5} // Render 5 extra items above and below viewport
      >
        {memoizedRenderItem}
      </List>
    </div>
  );
}

// Memoize the component to prevent unnecessary re-renders
export default React.memo(VirtualizedList) as <T>(props: VirtualizedListProps<T>) => React.ReactElement;