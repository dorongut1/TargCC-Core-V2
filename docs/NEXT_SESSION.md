# Next Session: Day 24 - Advanced Features

**Date:** 30/11/2025  
**Phase:** 3C - Local Web UI  
**Day:** 24 of 45  
**Duration:** ~3 hours  
**Status:** Ready to Start

---

## 🎯 Day 24 Objectives

### Primary Goal
Enhance the React UI with advanced features: better loading states, error handling, auto-refresh, and smooth animations.

### Specific Deliverables

1. **Loading Skeletons** (MUI Skeleton)
   - Dashboard skeleton
   - Table skeleton
   - Widget skeletons

2. **Error Boundary Component**
   - Catch rendering errors
   - Fallback UI
   - Error logging
   - Reset functionality

3. **Auto-Refresh System**
   - Toggle auto-refresh
   - Configurable interval
   - Refresh indicators
   - Last updated time

4. **Enhanced Animations**
   - Smooth transitions
   - Fade effects
   - Loading animations

5. **Testing**
   - 10-15 new tests
   - Error boundary tests
   - Loading state tests

---

## 📋 Detailed Implementation Plan

### Part 1: Loading Skeletons (45 minutes)

#### 1.1 Dashboard Skeleton
```typescript
// DashboardSkeleton.tsx
import { Box, Grid, Skeleton, Card, CardContent } from '@mui/material';

const DashboardSkeleton = () => {
  return (
    <Box>
      <Skeleton variant="text" width={200} height={40} sx={{ mb: 3 }} />
      
      {/* QuickStats Skeletons */}
      <Grid container spacing={2} mb={4}>
        {[1, 2, 3, 4].map((i) => (
          <Grid item xs={12} sm={6} md={3} key={i}>
            <Card>
              <CardContent>
                <Skeleton variant="text" width="60%" />
                <Skeleton variant="text" width="40%" height={40} />
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
      
      {/* Widget Skeletons */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={8}>
          <Skeleton variant="rectangular" height={300} />
        </Grid>
        <Grid item xs={12} md={4}>
          <Skeleton variant="rectangular" height={300} />
        </Grid>
      </Grid>
    </Box>
  );
};
```

#### 1.2 Table Skeleton
```typescript
// TableSkeleton.tsx
import { TableRow, TableCell, Skeleton } from '@mui/material';

const TableSkeleton = ({ rows = 10, columns = 6 }) => {
  return (
    <>
      {Array.from({ length: rows }).map((_, i) => (
        <TableRow key={i}>
          {Array.from({ length: columns }).map((_, j) => (
            <TableCell key={j}>
              <Skeleton variant="text" />
            </TableCell>
          ))}
        </TableRow>
      ))}
    </>
  );
};
```

---

### Part 2: Error Boundary (45 minutes)

#### 2.1 Error Boundary Component
```typescript
// ErrorBoundary.tsx
import React, { Component, ErrorInfo, ReactNode } from 'react';
import { Box, Button, Typography, Paper } from '@mui/material';
import { Error as ErrorIcon } from '@mui/icons-material';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      return (
        <Box
          display="flex"
          justifyContent="center"
          alignItems="center"
          minHeight="400px"
        >
          <Paper sx={{ p: 4, maxWidth: 500, textAlign: 'center' }}>
            <ErrorIcon color="error" sx={{ fontSize: 60, mb: 2 }} />
            <Typography variant="h5" gutterBottom>
              Something went wrong
            </Typography>
            <Typography color="text.secondary" paragraph>
              {this.state.error?.message}
            </Typography>
            <Button variant="contained" onClick={this.handleReset}>
              Try Again
            </Button>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
```

---

### Part 3: Auto-Refresh System (45 minutes)

#### 3.1 Auto-Refresh Hook
```typescript
// useAutoRefresh.ts
import { useEffect, useRef, useState } from 'react';

interface UseAutoRefreshOptions {
  enabled?: boolean;
  interval?: number; // milliseconds
  onRefresh: () => void | Promise<void>;
}

export const useAutoRefresh = ({
  enabled = false,
  interval = 30000, // 30 seconds default
  onRefresh
}: UseAutoRefreshOptions) => {
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date());
  const intervalRef = useRef<NodeJS.Timeout>();

  useEffect(() => {
    if (enabled) {
      intervalRef.current = setInterval(async () => {
        await onRefresh();
        setLastRefresh(new Date());
      }, interval);
    }

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [enabled, interval, onRefresh]);

  return { lastRefresh };
};
```

#### 3.2 Auto-Refresh UI Component
```typescript
// AutoRefreshControl.tsx
import { Box, Switch, FormControlLabel, Chip } from '@mui/material';
import { Refresh as RefreshIcon } from '@mui/icons-material';

interface AutoRefreshControlProps {
  enabled: boolean;
  onToggle: (enabled: boolean) => void;
  lastRefresh: Date;
}

const AutoRefreshControl = ({
  enabled,
  onToggle,
  lastRefresh
}: AutoRefreshControlProps) => {
  const formatTimeAgo = (date: Date) => {
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
    if (seconds < 60) return `${seconds}s ago`;
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    const hours = Math.floor(minutes / 60);
    return `${hours}h ago`;
  };

  return (
    <Box display="flex" alignItems="center" gap={2}>
      <FormControlLabel
        control={
          <Switch
            checked={enabled}
            onChange={(e) => onToggle(e.target.checked)}
          />
        }
        label="Auto-refresh"
      />
      {enabled && (
        <Chip
          icon={<RefreshIcon />}
          label={`Updated ${formatTimeAgo(lastRefresh)}`}
          size="small"
          variant="outlined"
        />
      )}
    </Box>
  );
};
```

---

### Part 4: Smooth Animations (30 minutes)

#### 4.1 Fade Transition Component
```typescript
// FadeIn.tsx
import { Box, Fade } from '@mui/material';
import { ReactNode } from 'react';

interface FadeInProps {
  children: ReactNode;
  delay?: number;
}

const FadeIn = ({ children, delay = 0 }: FadeInProps) => {
  return (
    <Fade in timeout={500} style={{ transitionDelay: `${delay}ms` }}>
      <Box>{children}</Box>
    </Fade>
  );
};
```

#### 4.2 Add Animations to Dashboard
```typescript
// In Dashboard.tsx, wrap widgets:
<FadeIn delay={100}>
  <QuickStats {...} />
</FadeIn>

<FadeIn delay={200}>
  <RecentGenerations {...} />
</FadeIn>
```

---

## 🧪 Testing Strategy

### Total Tests Target: 10-15 new tests

#### Error Boundary Tests (5 tests)
```typescript
// ErrorBoundary.test.tsx
- Renders children when no error
- Catches and displays error
- Shows error message
- Reset button works
- Logs error to console
```

#### Loading Skeleton Tests (3 tests)
```typescript
// DashboardSkeleton.test.tsx
- Renders correct number of skeletons
- Renders all skeleton types
- Matches dashboard structure

// TableSkeleton.test.tsx
- Renders correct number of rows
- Renders correct number of columns
```

#### Auto-Refresh Tests (5 tests)
```typescript
// useAutoRefresh.test.ts
- Calls onRefresh at interval when enabled
- Does not refresh when disabled
- Cleans up interval on unmount
- Updates lastRefresh timestamp
- Handles async refresh

// AutoRefreshControl.test.tsx
- Toggle switch works
- Displays last refresh time
- Shows/hides chip based on enabled state
```

---

## 📁 Files to Create

### New Component Files
```
src/components/
├── ErrorBoundary.tsx           (80 lines)
├── DashboardSkeleton.tsx       (60 lines)
├── TableSkeleton.tsx           (40 lines)
├── AutoRefreshControl.tsx      (70 lines)
└── FadeIn.tsx                  (20 lines)

src/hooks/
└── useAutoRefresh.ts           (40 lines)
```

### New Test Files
```
src/__tests__/
├── ErrorBoundary.test.tsx      (70 lines)
├── DashboardSkeleton.test.tsx  (40 lines)
├── TableSkeleton.test.tsx      (30 lines)
├── useAutoRefresh.test.ts      (80 lines)
└── AutoRefreshControl.test.tsx (60 lines)
```

### Modified Files
```
src/pages/
├── Dashboard.tsx               (Add ErrorBoundary, Skeleton, FadeIn)
└── Tables.tsx                  (Add ErrorBoundary, Skeleton, AutoRefresh)

src/App.tsx                     (Wrap with ErrorBoundary)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Loading skeletons show on initial load
- [ ] Error boundary catches errors gracefully
- [ ] Auto-refresh toggles and works correctly
- [ ] Smooth fade animations on page load
- [ ] All interactions smooth and responsive

### Testing
- [ ] 10-15 new tests written
- [ ] All tests have correct logic
- [ ] Tests cover happy paths and edge cases
- [ ] Mock timers used for auto-refresh tests

### Code Quality
- [ ] TypeScript strict mode compliant
- [ ] No build errors or warnings
- [ ] Components under 100 lines each
- [ ] Proper prop types and interfaces
- [ ] Clean, readable code

### Documentation
- [ ] Updated Phase3_Checklist.md
- [ ] Updated STATUS.md
- [ ] Updated HANDOFF.md for Day 25
- [ ] Code comments where needed

---

## 🚀 Getting Started

### 1. Environment Setup (2 min)
```bash
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI

# Verify app runs
npm run dev
# Open http://localhost:5173
```

### 2. Create Directory Structure (2 min)
```bash
# Create hooks directory
mkdir src\hooks

# Verify structure
dir src
```

### 3. Implementation Order
1. Start with ErrorBoundary (simplest)
2. Then Loading Skeletons
3. Then Auto-Refresh hook + UI
4. Then Fade animations
5. Write tests after each component

---

## 💡 Tips for Success

### Development Workflow
1. **Component First:** Build UI component
2. **Test Immediately:** Write tests right after
3. **Visual Check:** Always verify in browser
4. **Iterate Fast:** Small changes, frequent checks

### Common Pitfalls to Avoid
- Don't skip error boundary - essential for production
- Use MUI Skeleton for consistency
- Test auto-refresh with mock timers
- Don't overdo animations - keep subtle

### Performance Considerations
- Use React.memo for heavy components
- Debounce auto-refresh if needed
- Lazy load skeletons if too many
- Keep animations under 500ms

---

## 📞 Quick Commands

```bash
# Development
npm run dev              # Start dev server
npm test                 # Run tests
npm run build            # Build for production

# Troubleshooting
Remove-Item -Recurse -Force node_modules\.vite
npm run dev              # Clear cache and restart
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 3 hours  
**Expected Output:** Enhanced UX with 5 new components and 10-15 tests  
**Next Day:** Day 25 - Backend API

---

**Created:** 29/11/2025  
**Status:** Ready for Day 24! 🚀
