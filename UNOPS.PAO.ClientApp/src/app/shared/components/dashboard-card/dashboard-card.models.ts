/**
 * Dashboard Card Models and Interfaces
 * Common data structures for dashboard card components
 */

export interface DashboardCardFilter {
  id: string;
  label: string;
  count?: number;
  active?: boolean;
}

export type DashboardCardSize = 'auto' | 'fixed' | 'compact' | 'tall';

export interface DashboardCardConfig {
  icon: string;
  iconColor: string;
  title: string;
  subtitle: string;
  height?: string; // Optional custom height, defaults to responsive
  size?: DashboardCardSize; // Predefined size options
  showFilters?: boolean;
  showViewAll?: boolean;
  viewAllText?: string;
  emptyStateIcon?: string;
  emptyStateTitle?: string;
  emptyStateMessage?: string;
  emptyStateActionLabel?: string;
}

export interface DashboardCardItem {
  id: string | number;
  title: string;
  subtitle?: string;
  description?: string;
  date?: Date | string;
  status?: string;
  type?: string;
  [key: string]: any; // Allow additional properties
}

export interface DashboardCardData {
  items: DashboardCardItem[];
  totalCount: number;
  displayCount: number;
  hasMore: boolean;
}
