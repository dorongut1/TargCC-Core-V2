# Next Session: Day 33 - Backend Integration

**Date:** Next Session  
**Phase:** 3C - Local Web UI  
**Day:** 33 of 45  
**Duration:** ~4-5 hours  
**Status:** Ready to Start

---

## 🎯 Day 33 Objectives

### Primary Goal
Connect the Schema page to the WebAPI backend for real database schema loading, replacing mock data with live information.

### Specific Deliverables

1. **Schema API Client** (90 min)
   - Create API client module
   - Implement schema fetching
   - Handle authentication/headers
   - Request/response types

2. **React Hooks for Data** (60 min)
   - useSchema hook
   - useGeneration hook
   - Loading/error states
   - Automatic refresh

3. **Live Integration** (60 min)
   - Replace mockSchema usage
   - Connect all components
   - Real-time updates
   - Status indicators

4. **Enhanced Features** (45 min)
   - Database selector
   - Manual refresh button
   - Last updated display
   - Connection status

5. **Testing & Polish** (45 min)
   - API integration tests
   - Error handling tests
   - Loading state tests
   - UI polish

---

## 📋 Detailed Implementation Plan

### Part 1: API Client Module (90 minutes)

#### 1.1 Create API Configuration

```typescript
// src/api/config.ts

export const API_CONFIG = {
  baseUrl: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
};

export const API_ENDPOINTS = {
  schemas: '/api/schema',
  schemaDetail: (name: string) => `/api/schema/${name}`,
  generate: '/api/generate',
  generationStatus: (id: string) => `/api/generate/${id}`,
};
```

#### 1.2 Create Schema API Client

```typescript
// src/api/schemaApi.ts

import { API_CONFIG, API_ENDPOINTS } from './config';
import type { DatabaseSchema } from '../types/schema';

export interface SchemaListItem {
  name: string;
  displayName: string;
  tableCount: number;
  lastUpdated?: string;
}

export interface SchemaApiResponse {
  success: boolean;
  data?: DatabaseSchema;
  error?: string;
}

/**
 * Fetch list of available schemas
 */
export async function fetchSchemas(): Promise<SchemaListItem[]> {
  const response = await fetch(`${API_CONFIG.baseUrl}${API_ENDPOINTS.schemas}`, {
    method: 'GET',
    headers: API_CONFIG.headers,
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch schemas: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Fetch detailed schema information
 */
export async function fetchSchemaDetails(schemaName: string): Promise<DatabaseSchema> {
  const response = await fetch(
    `${API_CONFIG.baseUrl}${API_ENDPOINTS.schemaDetail(schemaName)}`,
    {
      method: 'GET',
      headers: API_CONFIG.headers,
    }
  );

  if (!response.ok) {
    throw new Error(`Failed to fetch schema details: ${response.statusText}`);
  }

  const result: SchemaApiResponse = await response.json();
  
  if (!result.success || !result.data) {
    throw new Error(result.error || 'Unknown error fetching schema');
  }

  return result.data;
}

/**
 * Refresh schema from database
 */
export async function refreshSchema(schemaName: string): Promise<DatabaseSchema> {
  const response = await fetch(
    `${API_CONFIG.baseUrl}${API_ENDPOINTS.schemaDetail(schemaName)}/refresh`,
    {
      method: 'POST',
      headers: API_CONFIG.headers,
    }
  );

  if (!response.ok) {
    throw new Error(`Failed to refresh schema: ${response.statusText}`);
  }

  const result: SchemaApiResponse = await response.json();
  
  if (!result.success || !result.data) {
    throw new Error(result.error || 'Unknown error refreshing schema');
  }

  return result.data;
}
```

---

### Part 2: React Hooks (60 minutes)

#### 2.1 useSchema Hook

```typescript
// src/hooks/useSchema.ts

import { useState, useEffect, useCallback } from 'react';
import { fetchSchemaDetails, refreshSchema } from '../api/schemaApi';
import type { DatabaseSchema } from '../types/schema';

interface UseSchemaResult {
  schema: DatabaseSchema | null;
  loading: boolean;
  error: string | null;
  refresh: () => Promise<void>;
  lastUpdated: Date | null;
}

export function useSchema(schemaName: string | null): UseSchemaResult {
  const [schema, setSchema] = useState<DatabaseSchema | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  const loadSchema = useCallback(async (name: string) => {
    setLoading(true);
    setError(null);

    try {
      const data = await fetchSchemaDetails(name);
      setSchema(data);
      setLastUpdated(new Date());
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load schema');
    } finally {
      setLoading(false);
    }
  }, []);

  const refresh = useCallback(async () => {
    if (!schemaName) return;

    setLoading(true);
    setError(null);

    try {
      const data = await refreshSchema(schemaName);
      setSchema(data);
      setLastUpdated(new Date());
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to refresh schema');
    } finally {
      setLoading(false);
    }
  }, [schemaName]);

  useEffect(() => {
    if (schemaName) {
      loadSchema(schemaName);
    }
  }, [schemaName, loadSchema]);

  return {
    schema,
    loading,
    error,
    refresh,
    lastUpdated,
  };
}
```

---

### Part 3: Update Schema Page (60 minutes)

#### 3.1 Integrate API with Schema Page

```typescript
// src/pages/Schema.tsx

import { useState } from 'react';
import { 
  Container, 
  Stack, 
  Paper, 
  Typography,
  Alert,
  CircularProgress,
  Box,
  IconButton,
  Tooltip
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import SchemaViewer from '../components/schema/SchemaViewer';
import SchemaStats from '../components/schema/SchemaStats';
import RelationshipGraph from '../components/schema/RelationshipGraph';
import ExportMenu from '../components/schema/ExportMenu';
import { useSchema } from '../hooks/useSchema';
import { mockSchema } from '../utils/mockSchema'; // Fallback

const Schema = () => {
  // For now, hardcode a schema name - later we'll add a selector
  const [selectedSchema] = useState<string>('dbo');
  
  const { schema, loading, error, refresh, lastUpdated } = useSchema(selectedSchema);

  // Use mock data as fallback during development
  const displaySchema = schema || mockSchema;

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Stack spacing={3}>
        {/* Page Header */}
        <Paper sx={{ p: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box>
            <Typography variant="h4">Database Schema</Typography>
            {lastUpdated && (
              <Typography variant="caption" color="text.secondary">
                Last updated: {lastUpdated.toLocaleString()}
              </Typography>
            )}
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Tooltip title="Refresh Schema">
              <IconButton onClick={refresh} disabled={loading}>
                <RefreshIcon />
              </IconButton>
            </Tooltip>
            <ExportMenu schema={displaySchema} />
          </Box>
        </Paper>

        {/* Loading State */}
        {loading && (
          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <CircularProgress />
            <Typography sx={{ mt: 2 }}>Loading schema...</Typography>
          </Paper>
        )}

        {/* Error State */}
        {error && (
          <Alert severity="error" onClose={() => {}}>
            {error}
          </Alert>
        )}

        {/* Content */}
        {!loading && (
          <>
            <SchemaStats schema={displaySchema} />
            <RelationshipGraph schema={displaySchema} />
            <SchemaViewer schema={displaySchema} />
          </>
        )}
      </Stack>
    </Container>
  );
};

export default Schema;
```

---

### Part 4: Environment Configuration (15 minutes)

#### 4.1 Add Environment Variables

```env
# .env
VITE_API_URL=http://localhost:5000
```

#### 4.2 Add Environment Types

```typescript
// src/vite-env.d.ts

/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
```

---

### Part 5: Testing (45 minutes)

```typescript
// src/__tests__/api/schemaApi.test.ts

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { fetchSchemas, fetchSchemaDetails, refreshSchema } from '../../api/schemaApi';

// Mock fetch
global.fetch = vi.fn();

describe('schemaApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('fetchSchemas', () => {
    it('returns list of schemas', async () => {
      const mockSchemas = [
        { name: 'dbo', displayName: 'Default', tableCount: 5 }
      ];

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockSchemas,
      });

      const result = await fetchSchemas();
      expect(result).toEqual(mockSchemas);
    });

    it('throws error on failed request', async () => {
      (global.fetch as any).mockResolvedValueOnce({
        ok: false,
        statusText: 'Internal Server Error',
      });

      await expect(fetchSchemas()).rejects.toThrow();
    });
  });

  describe('fetchSchemaDetails', () => {
    it('returns schema details', async () => {
      const mockSchema = {
        success: true,
        data: { tables: [], relationships: [] }
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockSchema,
      });

      const result = await fetchSchemaDetails('dbo');
      expect(result).toEqual(mockSchema.data);
    });
  });
});
```

---

## 📁 Files to Create/Modify

### New Files
```
src/api/
├── config.ts                 (30 lines)
├── schemaApi.ts             (120 lines)
└── types.ts                 (40 lines)

src/hooks/
├── useSchema.ts             (100 lines)
└── useGeneration.ts         (100 lines)

src/__tests__/api/
├── schemaApi.test.ts        (80 lines)
└── hooks/
    └── useSchema.test.ts     (70 lines)

.env                          (5 lines)
src/vite-env.d.ts            (10 lines)
```

### Modified Files
```
src/pages/Schema.tsx          (+60 lines)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Can load schema from API
- [ ] Refresh button works
- [ ] Loading states display correctly
- [ ] Errors handled gracefully
- [ ] Falls back to mock data if API unavailable
- [ ] All existing features still work

### Testing
- [ ] 8-12 new tests written
- [ ] API calls tested
- [ ] Hook logic tested
- [ ] Error scenarios covered

### Code Quality
- [ ] TypeScript compliant
- [ ] Proper error handling
- [ ] Clean code structure
- [ ] No console warnings

### Documentation
- [ ] STATUS.md updated
- [ ] HANDOFF.md for Day 34
- [ ] API docs updated
- [ ] PROGRESS.md updated

---

## 🚀 Getting Started

### 1. Verify WebAPI is Running
```bash
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebAPI
dotnet run
# Verify: http://localhost:5000/api/schema
```

### 2. Development Order
1. Create API configuration
2. Create schema API client
3. Create useSchema hook
4. Update Schema page
5. Add environment config
6. Test integration
7. Write tests
8. Update docs

---

## 💡 Tips for Success

### API Integration
- Start with simple GET requests
- Add error handling early
- Use proper TypeScript types
- Test with real backend
- Handle timeout cases

### State Management
- Keep hooks focused
- Handle loading states
- Clear error messages
- Implement retry logic
- Cache when appropriate

### User Experience
- Show loading indicators
- Display helpful errors
- Maintain mock fallback
- Quick response times
- Smooth transitions

---

## 📞 Quick Commands

```bash
# Start dev server
npm run dev

# Run API tests
npm test -- --run src/__tests__/api

# Type check
npx tsc --noEmit

# Start WebAPI
cd ../TargCC.WebAPI && dotnet run
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 4-5 hours  
**Expected Output:** Live backend integration  
**Next Day:** Day 34 - Additional Features

---

**Created:** 01/12/2025 22:00  
**Status:** Ready for Day 33! 🚀
