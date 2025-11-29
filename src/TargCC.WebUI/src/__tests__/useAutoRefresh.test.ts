import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useAutoRefresh } from '../hooks/useAutoRefresh';

describe('useAutoRefresh', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  it('returns lastRefresh timestamp', () => {
    const onRefresh = vi.fn();
    const { result } = renderHook(() =>
      useAutoRefresh({ enabled: false, onRefresh })
    );

    expect(result.current.lastRefresh).toBeInstanceOf(Date);
  });

  it('returns refresh function', () => {
    const onRefresh = vi.fn();
    const { result } = renderHook(() =>
      useAutoRefresh({ enabled: false, onRefresh })
    );

    expect(result.current.refresh).toBeInstanceOf(Function);
  });

  it('does not call onRefresh when disabled', () => {
    const onRefresh = vi.fn();
    renderHook(() =>
      useAutoRefresh({ enabled: false, interval: 1000, onRefresh })
    );

    vi.advanceTimersByTime(5000);
    expect(onRefresh).not.toHaveBeenCalled();
  });

  it('calls onRefresh at specified interval when enabled', async () => {
    const onRefresh = vi.fn();
    renderHook(() =>
      useAutoRefresh({ enabled: true, interval: 1000, onRefresh })
    );

    // Should not be called immediately
    expect(onRefresh).not.toHaveBeenCalled();

    // After 1 second, should be called once
    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(1));

    // After 2 seconds total, should be called twice
    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(2));

    // After 3 seconds total, should be called three times
    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(3));
  });

  it('uses default interval of 30 seconds', async () => {
    const onRefresh = vi.fn();
    renderHook(() =>
      useAutoRefresh({ enabled: true, onRefresh })
    );

    // After 29 seconds, should not be called yet
    vi.advanceTimersByTime(29000);
    expect(onRefresh).not.toHaveBeenCalled();

    // After 30 seconds, should be called
    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(1));
  });

  it('clears interval when disabled', async () => {
    const onRefresh = vi.fn();
    const { rerender } = renderHook(
      ({ enabled }) => useAutoRefresh({ enabled, interval: 1000, onRefresh }),
      { initialProps: { enabled: true } }
    );

    // First interval tick
    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(1));

    // Disable auto-refresh
    rerender({ enabled: false });

    // Wait for more time
    vi.advanceTimersByTime(5000);
    
    // Should still be called only once (interval cleared)
    expect(onRefresh).toHaveBeenCalledTimes(1);
  });

  it('clears interval on unmount', async () => {
    const onRefresh = vi.fn();
    const { unmount } = renderHook(() =>
      useAutoRefresh({ enabled: true, interval: 1000, onRefresh })
    );

    // First tick
    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(1));

    // Unmount
    unmount();

    // Wait more time
    vi.advanceTimersByTime(5000);

    // Should not be called again after unmount
    expect(onRefresh).toHaveBeenCalledTimes(1);
  });

  it('handles async onRefresh function', async () => {
    const onRefresh = vi.fn().mockImplementation(
      () => new Promise(resolve => setTimeout(resolve, 100))
    );

    renderHook(() =>
      useAutoRefresh({ enabled: true, interval: 1000, onRefresh })
    );

    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(1));
  });

  it('manual refresh updates lastRefresh timestamp', async () => {
    const onRefresh = vi.fn();
    const { result } = renderHook(() =>
      useAutoRefresh({ enabled: false, onRefresh })
    );

    const initialTime = result.current.lastRefresh;

    // Wait a bit
    vi.advanceTimersByTime(100);

    // Manual refresh
    await result.current.refresh();

    // Timestamp should be updated
    expect(result.current.lastRefresh.getTime()).toBeGreaterThan(initialTime.getTime());
  });

  it('manual refresh calls onRefresh', async () => {
    const onRefresh = vi.fn();
    const { result } = renderHook(() =>
      useAutoRefresh({ enabled: false, onRefresh })
    );

    await result.current.refresh();

    expect(onRefresh).toHaveBeenCalledTimes(1);
  });

  it('updates lastRefresh after each auto-refresh', async () => {
    const onRefresh = vi.fn();
    const { result } = renderHook(() =>
      useAutoRefresh({ enabled: true, interval: 1000, onRefresh })
    );

    const initialTime = result.current.lastRefresh;

    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(1));

    // LastRefresh should be updated
    expect(result.current.lastRefresh.getTime()).toBeGreaterThan(initialTime.getTime());
  });

  it('handles interval change', async () => {
    const onRefresh = vi.fn();
    const { rerender } = renderHook(
      ({ interval }) => useAutoRefresh({ enabled: true, interval, onRefresh }),
      { initialProps: { interval: 1000 } }
    );

    // First interval
    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(1));

    // Change interval to 2000ms
    rerender({ interval: 2000 });

    // Old interval shouldn't trigger
    vi.advanceTimersByTime(1000);
    expect(onRefresh).toHaveBeenCalledTimes(1);

    // New interval should trigger
    vi.advanceTimersByTime(1000);
    await waitFor(() => expect(onRefresh).toHaveBeenCalledTimes(2));
  });
});
