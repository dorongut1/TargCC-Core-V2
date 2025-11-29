import { useEffect, useRef, useState } from 'react';

interface UseAutoRefreshOptions {
  /** Whether auto-refresh is enabled */
  enabled?: boolean;
  /** Refresh interval in milliseconds */
  interval?: number;
  /** Function to call on each refresh */
  onRefresh: () => void | Promise<void>;
}

interface UseAutoRefreshReturn {
  /** Timestamp of last refresh */
  lastRefresh: Date;
  /** Manually trigger a refresh */
  refresh: () => Promise<void>;
}

/**
 * Custom hook for auto-refreshing data at regular intervals.
 * 
 * @example
 * const { lastRefresh } = useAutoRefresh({
 *   enabled: true,
 *   interval: 30000, // 30 seconds
 *   onRefresh: async () => {
 *     await fetchData();
 *   }
 * });
 */
export const useAutoRefresh = ({
  enabled = false,
  interval = 30000,
  onRefresh
}: UseAutoRefreshOptions): UseAutoRefreshReturn => {
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date());
  const intervalRef = useRef<NodeJS.Timeout>();

  const refresh = async (): Promise<void> => {
    await onRefresh();
    setLastRefresh(new Date());
  };

  useEffect(() => {
    if (enabled && interval > 0) {
      // Set up interval for auto-refresh
      intervalRef.current = setInterval(async () => {
        await refresh();
      }, interval);
    }

    // Cleanup interval on unmount or when dependencies change
    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [enabled, interval]); // onRefresh is intentionally not in deps to avoid recreation

  return { lastRefresh, refresh };
};
