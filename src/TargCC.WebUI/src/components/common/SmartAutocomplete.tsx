/**
 * SmartAutocomplete - A high-performance autocomplete component with server-side search,
 * paging, and caching support.
 *
 * Features:
 * - Server-side search with debouncing
 * - Infinite scroll / paging
 * - Result caching
 * - RTL support
 * - Loading states
 * - Customizable rendering
 */

import React, { useState, useCallback, useRef, useEffect } from 'react';
import {
  Autocomplete,
  TextField,
  CircularProgress,
  Box,
  Typography,
} from '@mui/material';
import { debounce } from 'lodash';

/**
 * Lookup item returned from the API
 */
export interface LookupItem {
  id: number;
  text: string;
  textNS?: string;
  [key: string]: unknown;
}

/**
 * Props for SmartAutocomplete component
 */
export interface SmartAutocompleteProps {
  /** API endpoint for lookup data (e.g., "/api/lookup/Customer") */
  apiEndpoint: string;

  /** Entity type name (e.g., "Customer", "Product") */
  entityType: string;

  /** Parent ID for filtered lookups (e.g., Customer by Distributor) */
  parentId?: number;

  /** Current value (ID) */
  value?: number | null;

  /** Callback when value changes */
  onChange: (value: number | null, item?: LookupItem) => void;

  /** Placeholder text */
  placeholder?: string;

  /** Label text */
  label?: string;

  /** Minimum characters before search triggers */
  minChars?: number;

  /** Debounce delay in milliseconds */
  debounceMs?: number;

  /** Page size for results */
  pageSize?: number;

  /** Whether the field is required */
  required?: boolean;

  /** Whether the field is disabled */
  disabled?: boolean;

  /** Error state */
  error?: boolean;

  /** Helper text */
  helperText?: string;

  /** Whether to use RTL direction */
  rtl?: boolean;

  /** Custom render option function */
  renderOption?: (option: LookupItem) => React.ReactNode;
}

/**
 * Cache for storing lookup results
 */
const lookupCache = new Map<string, { data: LookupItem[]; timestamp: number }>();
const CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes

/**
 * SmartAutocomplete component for server-side search with caching
 */
export const SmartAutocomplete: React.FC<SmartAutocompleteProps> = ({
  apiEndpoint,
  entityType,
  parentId,
  value,
  onChange,
  placeholder = 'Search...',
  label,
  minChars = 2,
  debounceMs = 300,
  pageSize = 20,
  required = false,
  disabled = false,
  error = false,
  helperText,
  rtl = true,
  renderOption,
}) => {
  const [options, setOptions] = useState<LookupItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [inputValue, setInputValue] = useState('');
  const [selectedItem, setSelectedItem] = useState<LookupItem | null>(null);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);

  const abortControllerRef = useRef<AbortController | null>(null);

  /**
   * Generate cache key for a search
   */
  const getCacheKey = useCallback(
    (searchText: string, pageNum: number): string => {
      return `${entityType}_${parentId ?? 'null'}_${searchText}_${pageNum}`;
    },
    [entityType, parentId]
  );

  /**
   * Check if cached data is still valid
   */
  const getCachedData = useCallback((key: string): LookupItem[] | null => {
    const cached = lookupCache.get(key);
    if (!cached) {
      return null;
    }

    if (Date.now() - cached.timestamp > CACHE_TTL_MS) {
      lookupCache.delete(key);
      return null;
    }

    return cached.data;
  }, []);

  /**
   * Cache search results
   */
  const setCachedData = useCallback((key: string, data: LookupItem[]): void => {
    lookupCache.set(key, { data, timestamp: Date.now() });
  }, []);

  /**
   * Fetch options from API
   */
  const fetchOptions = useCallback(
    async (searchText: string, pageNum: number = 1): Promise<void> => {
      if (searchText.length < minChars) {
        setOptions([]);
        return;
      }

      const cacheKey = getCacheKey(searchText, pageNum);
      const cachedData = getCachedData(cacheKey);

      if (cachedData) {
        if (pageNum === 1) {
          setOptions(cachedData);
        } else {
          setOptions((prev) => [...prev, ...cachedData]);
        }
        setHasMore(cachedData.length === pageSize);
        return;
      }

      // Cancel previous request
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }

      abortControllerRef.current = new AbortController();

      setLoading(true);

      try {
        const params = new URLSearchParams({
          search: searchText,
          page: pageNum.toString(),
          pageSize: pageSize.toString(),
        });

        if (parentId) {
          params.append('parentId', parentId.toString());
        }

        const response = await fetch(`${apiEndpoint}/${entityType}?${params}`, {
          signal: abortControllerRef.current.signal,
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data: LookupItem[] = await response.json();

        setCachedData(cacheKey, data);

        if (pageNum === 1) {
          setOptions(data);
        } else {
          setOptions((prev) => [...prev, ...data]);
        }

        setHasMore(data.length === pageSize);
      } catch (err) {
        if (err instanceof Error && err.name === 'AbortError') {
          // Request was cancelled, ignore
          return;
        }
        console.error('Error fetching lookup options:', err);
        setOptions([]);
      } finally {
        setLoading(false);
      }
    },
    [apiEndpoint, entityType, parentId, minChars, pageSize, getCacheKey, getCachedData, setCachedData]
  );

  /**
   * Debounced fetch function
   */
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const debouncedFetch = useCallback(
    debounce((searchText: string) => {
      setPage(1);
      fetchOptions(searchText, 1);
    }, debounceMs),
    [fetchOptions, debounceMs]
  );

  /**
   * Handle input change
   */
  const handleInputChange = useCallback(
    (_event: React.SyntheticEvent, newInputValue: string): void => {
      setInputValue(newInputValue);
      debouncedFetch(newInputValue);
    },
    [debouncedFetch]
  );

  /**
   * Handle selection change
   */
  const handleChange = useCallback(
    (_event: React.SyntheticEvent, newValue: LookupItem | null): void => {
      setSelectedItem(newValue);
      onChange(newValue?.id ?? null, newValue ?? undefined);
    },
    [onChange]
  );

  /**
   * Handle scroll for infinite loading
   */
  const handleScroll = useCallback(
    (event: React.SyntheticEvent): void => {
      const listboxNode = event.currentTarget;
      if (
        listboxNode.scrollTop + listboxNode.clientHeight >=
          listboxNode.scrollHeight - 50 &&
        hasMore &&
        !loading
      ) {
        const nextPage = page + 1;
        setPage(nextPage);
        fetchOptions(inputValue, nextPage);
      }
    },
    [hasMore, loading, page, inputValue, fetchOptions]
  );

  /**
   * Load initial value if provided
   */
  useEffect(() => {
    if (value && !selectedItem) {
      // Fetch the item by ID
      const loadInitialValue = async (): Promise<void> => {
        try {
          const response = await fetch(`${apiEndpoint}/${entityType}/${value}`);
          if (response.ok) {
            const item: LookupItem = await response.json();
            setSelectedItem(item);
            setInputValue(item.text);
          }
        } catch {
          console.error('Error loading initial value');
        }
      };

      loadInitialValue();
    }
  }, [value, selectedItem, apiEndpoint, entityType]);

  /**
   * Cleanup on unmount
   */
  useEffect(() => {
    return () => {
      debouncedFetch.cancel();
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [debouncedFetch]);

  return (
    <Autocomplete<LookupItem, false, false, false>
      value={selectedItem}
      onChange={handleChange}
      inputValue={inputValue}
      onInputChange={handleInputChange}
      options={options}
      loading={loading}
      disabled={disabled}
      getOptionLabel={(option) => option.text}
      isOptionEqualToValue={(option, val) => option.id === val.id}
      filterOptions={(x) => x} // Disable client-side filtering
      ListboxProps={{
        onScroll: handleScroll,
        style: { direction: rtl ? 'rtl' : 'ltr' },
      }}
      renderOption={(props, option) => (
        <Box
          component="li"
          {...props}
          key={option.id}
          sx={{ direction: rtl ? 'rtl' : 'ltr' }}
        >
          {renderOption ? (
            renderOption(option)
          ) : (
            <Typography>{option.text}</Typography>
          )}
        </Box>
      )}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          placeholder={placeholder}
          required={required}
          error={error}
          helperText={helperText}
          InputProps={{
            ...params.InputProps,
            style: { direction: rtl ? 'rtl' : 'ltr' },
            endAdornment: (
              <>
                {loading ? <CircularProgress color="inherit" size={20} /> : null}
                {params.InputProps.endAdornment}
              </>
            ),
          }}
        />
      )}
      noOptionsText={
        inputValue.length < minChars
          ? `Type at least ${minChars} characters...`
          : 'No results found'
      }
      loadingText="Loading..."
    />
  );
};

export default SmartAutocomplete;
