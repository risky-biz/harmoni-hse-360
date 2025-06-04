/**
 * Centralized date formatting utilities
 * Standard format: 'dd MMM yyyy hh:mm tt' (e.g., "15 Jan 2025 02:30 PM")
 */

export const formatDate = (date: string | Date, includeTime: boolean = true): string => {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  
  if (isNaN(dateObj.getTime())) {
    return 'Invalid Date';
  }

  // Get date components
  const day = dateObj.getDate().toString().padStart(2, '0');
  const month = dateObj.toLocaleDateString('en-US', { month: 'short' });
  const year = dateObj.getFullYear();
  
  if (!includeTime) {
    return `${day} ${month} ${year}`;
  }
  
  // Get time components
  const hours12 = dateObj.getHours() % 12 || 12;
  const minutes = dateObj.getMinutes().toString().padStart(2, '0');
  const ampm = dateObj.getHours() >= 12 ? 'PM' : 'AM';
  const hours12Padded = hours12.toString().padStart(2, '0');
  
  return `${day} ${month} ${year} ${hours12Padded}:${minutes} ${ampm}`;
};

export const formatDateOnly = (date: string | Date): string => {
  return formatDate(date, false);
};

export const formatDateTime = (date: string | Date): string => {
  return formatDate(date, true);
};

export const formatRelativeTime = (date: string | Date): string => {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  const now = new Date();
  const diffInSeconds = Math.floor((now.getTime() - dateObj.getTime()) / 1000);

  if (diffInSeconds < 60) {
    return 'Just now';
  } else if (diffInSeconds < 3600) {
    const minutes = Math.floor(diffInSeconds / 60);
    return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
  } else if (diffInSeconds < 86400) {
    const hours = Math.floor(diffInSeconds / 3600);
    return `${hours} hour${hours > 1 ? 's' : ''} ago`;
  } else if (diffInSeconds < 2592000) {
    const days = Math.floor(diffInSeconds / 86400);
    return `${days} day${days > 1 ? 's' : ''} ago`;
  } else {
    return formatDate(date, false);
  }
};

export const isOverdue = (dueDate: string | Date): boolean => {
  const dateObj = typeof dueDate === 'string' ? new Date(dueDate) : dueDate;
  return dateObj < new Date();
};