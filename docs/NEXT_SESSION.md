# Next Session Brief - Day 21: React Project Setup

**Date Prepared:** 28/11/2025  
**Session:** Day 21 of Phase 3C (Week 5)  
**Estimated Duration:** 4-5 hours  
**Priority:** High - Begin Phase 3C (Web UI)

---

## 🎉 Previous Session Success (Day 20)

### ✅ Day 20 COMPLETE - All Tests Passing!

**What Was Accomplished:**
- ✅ CodeQualityAnalyzerService fully tested (15 tests)
- ✅ AnalyzeQualityCommand fully implemented (30 tests)
- ✅ HandleAsync method implemented and working
- ✅ Integration tests complete
- ✅ Build successful: 0 errors
- ✅ **715+ tests passing** 
- ✅ Code coverage: 85%+
- ✅ **Phase 3B (AI Integration) - 100% COMPLETE** 🎉

**Phase 3B Final Status:**
```
✅ AI Service Infrastructure - Complete
✅ Schema Analysis - Complete
✅ Suggestion Engine - Complete
✅ Interactive Chat - Complete
✅ Security Scanner - Complete
✅ Code Quality Analyzer - Complete
✅ All CLI commands operational
✅ All tests passing (715+)

Phase 3B: 100% Complete! 🚀
```

---

## 📋 Current Status

### Project Overview:
- **Phase 1:** ✅ Core Engine - Complete
- **Phase 1.5:** ✅ MVP Generators - Complete
- **Phase 2:** ✅ Modern Architecture - Complete
- **Phase 3A:** ✅ CLI Core - Complete (Days 1-10)
- **Phase 3B:** ✅ AI Integration - Complete (Days 11-20)
- **Phase 3C:** 🆕 Local Web UI - Starting (Days 21-35)
- **Phase 3D:** ☐ Migration & Polish - Planned (Days 36-45)

### Overall Progress:
- **Days Completed:** 20/45 (44%)
- **Tests Passing:** 715+
- **Code Coverage:** 85%+
- **Current Phase:** Phase 3C - Local Web UI

---

## 🎯 Session Objective: Day 21-22 Part 1

**Begin Phase 3C: Local Web UI Foundation**

Create a React + TypeScript web application that will serve as a local UI wrapper around the TargCC CLI core. This will provide a visual interface for developers who prefer GUI over command-line.

### What We're Building:

```
┌────────────────────────────────────────────┐
│  TargCC Web UI (localhost:5000)           │
├────────────────────────────────────────────┤
│                                            │
│  📊 Dashboard                              │
│  ┌──────────────────────────────────────┐ │
│  │  Tables: 12   Generated: 8           │ │
│  │  Last Run: 2 min ago                 │ │
│  └──────────────────────────────────────┘ │
│                                            │
│  🛠️ Quick Actions                         │
│  [Generate All] [Analyze] [AI Chat]      │
│                                            │
│  📁 Recent Activity                       │
│  - Customer entity generated              │
│  - Security scan completed                │
│                                            │
└────────────────────────────────────────────┘

Backend: ASP.NET Core API → Wraps CLI Commands
Frontend: React + TypeScript + Material-UI
```

---

## 📅 Day 21 Tasks (Part 1 of 2)

**Goal:** Setup React project structure and development environment

### Task 1: Create React + TypeScript Project

**Location:** `C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI\`

**Steps:**

1. **Create React App with TypeScript**
```bash
cd C:\Disk1\TargCC-Core-V2\src

# Create React app with TypeScript template
npx create-react-app TargCC.WebUI --template typescript

cd TargCC.WebUI
```

2. **Install Core Dependencies**
```bash
# Material-UI (MUI) for components
npm install @mui/material @emotion/react @emotion/styled

# Material-UI icons
npm install @mui/icons-material

# React Router for navigation
npm install react-router-dom
npm install --save-dev @types/react-router-dom

# React Query for data fetching
npm install @tanstack/react-query

# Axios for API calls
npm install axios
```

3. **Install Development Dependencies**
```bash
# Testing libraries (should be included, verify)
npm install --save-dev @testing-library/react @testing-library/jest-dom @testing-library/user-event

# ESLint and Prettier for code quality
npm install --save-dev eslint-config-prettier eslint-plugin-prettier prettier
```

4. **Project Structure Setup**

Create the following folder structure:
```
src/TargCC.WebUI/
├── public/
│   ├── index.html
│   └── favicon.ico
├── src/
│   ├── components/
│   │   ├── Dashboard/
│   │   │   └── Dashboard.tsx
│   │   ├── Layout/
│   │   │   ├── Layout.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── Header.tsx
│   │   └── common/
│   │       └── LoadingSpinner.tsx
│   ├── services/
│   │   └── api.ts
│   ├── hooks/
│   │   └── useTargccApi.ts
│   ├── types/
│   │   └── models.ts
│   ├── App.tsx
│   ├── App.css
│   ├── index.tsx
│   └── index.css
├── package.json
├── tsconfig.json
└── README.md
```

---

### Task 2: Configure TypeScript and ESLint

**File:** `tsconfig.json`

Update or verify configuration:
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "jsx": "react-jsx",
    "module": "ESNext",
    "moduleResolution": "node",
    "resolveJsonModule": true,
    "allowJs": true,
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noImplicitReturns": true,
    "baseUrl": "src"
  },
  "include": ["src"],
  "exclude": ["node_modules"]
}
```

**File:** `.eslintrc.json` (create new)

```json
{
  "extends": [
    "react-app",
    "react-app/jest",
    "prettier"
  ],
  "plugins": ["prettier"],
  "rules": {
    "prettier/prettier": "error",
    "no-console": "warn",
    "@typescript-eslint/no-unused-vars": "error"
  }
}
```

**File:** `.prettierrc` (create new)

```json
{
  "semi": true,
  "trailingComma": "es5",
  "singleQuote": true,
  "printWidth": 80,
  "tabWidth": 2,
  "useTabs": false
}
```

---

### Task 3: Create Type Definitions

**File:** `src/types/models.ts`

```typescript
// Database models
export interface Table {
  name: string;
  schema: string;
  rowCount: number;
  columns: Column[];
  hasGenerated: boolean;
  lastGenerated?: Date;
}

export interface Column {
  name: string;
  dataType: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  maxLength?: number;
}

// Generation models
export interface GenerationRequest {
  tableName: string;
  generateEntity: boolean;
  generateSql: boolean;
  generateRepository: boolean;
  generateCqrs: boolean;
  generateApi: boolean;
}

export interface GenerationResult {
  success: boolean;
  filesGenerated: string[];
  errors: string[];
  duration: number;
}

// Analysis models
export interface SchemaAnalysis {
  tables: Table[];
  totalTables: number;
  analyzedAt: Date;
}

export interface SecurityIssue {
  tableName: string;
  columnName: string;
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  description: string;
  recommendation: string;
}

export interface QualityReport {
  score: number;
  grade: string;
  namingIssues: QualityIssue[];
  bestPracticeViolations: QualityIssue[];
  relationshipIssues: QualityIssue[];
}

export interface QualityIssue {
  elementName: string;
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  description: string;
  recommendation?: string;
}

// AI models
export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
}

export interface Suggestion {
  category: 'Security' | 'Performance' | 'BestPractices';
  priority: 'High' | 'Medium' | 'Low';
  description: string;
  tableName?: string;
}

// API response wrapper
export interface ApiResponse<T> {
  data?: T;
  error?: string;
  success: boolean;
}
```

---

### Task 4: Create API Service

**File:** `src/services/api.ts`

```typescript
import axios, { AxiosInstance } from 'axios';
import {
  Table,
  GenerationRequest,
  GenerationResult,
  SchemaAnalysis,
  SecurityIssue,
  QualityReport,
  ChatMessage,
  Suggestion,
  ApiResponse,
} from '../types/models';

class TargccApiService {
  private api: AxiosInstance;

  constructor(baseURL: string = 'http://localhost:5000/api') {
    this.api = axios.create({
      baseURL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Response interceptor for error handling
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        console.error('API Error:', error);
        return Promise.reject(error);
      }
    );
  }

  // Schema endpoints
  async getTables(): Promise<ApiResponse<Table[]>> {
    try {
      const response = await this.api.get<Table[]>('/schema/tables');
      return { data: response.data, success: true };
    } catch (error) {
      return { error: 'Failed to fetch tables', success: false };
    }
  }

  async analyzeSchema(): Promise<ApiResponse<SchemaAnalysis>> {
    try {
      const response = await this.api.get<SchemaAnalysis>('/analyze/schema');
      return { data: response.data, success: true };
    } catch (error) {
      return { error: 'Failed to analyze schema', success: false };
    }
  }

  // Generation endpoints
  async generateCode(
    request: GenerationRequest
  ): Promise<ApiResponse<GenerationResult>> {
    try {
      const response = await this.api.post<GenerationResult>(
        '/generate',
        request
      );
      return { data: response.data, success: true };
    } catch (error) {
      return { error: 'Failed to generate code', success: false };
    }
  }

  // Analysis endpoints
  async analyzeSecurity(): Promise<ApiResponse<SecurityIssue[]>> {
    try {
      const response = await this.api.get<SecurityIssue[]>(
        '/analyze/security'
      );
      return { data: response.data, success: true };
    } catch (error) {
      return { error: 'Failed to analyze security', success: false };
    }
  }

  async analyzeQuality(): Promise<ApiResponse<QualityReport>> {
    try {
      const response = await this.api.get<QualityReport>('/analyze/quality');
      return { data: response.data, success: true };
    } catch (error) {
      return { error: 'Failed to analyze quality', success: false };
    }
  }

  // AI endpoints
  async getSuggestions(tableName?: string): Promise<ApiResponse<Suggestion[]>> {
    try {
      const response = await this.api.get<Suggestion[]>('/ai/suggestions', {
        params: { tableName },
      });
      return { data: response.data, success: true };
    } catch (error) {
      return { error: 'Failed to get suggestions', success: false };
    }
  }

  async chat(message: string): Promise<ApiResponse<ChatMessage>> {
    try {
      const response = await this.api.post<ChatMessage>('/ai/chat', {
        message,
      });
      return { data: response.data, success: true };
    } catch (error) {
      return { error: 'Failed to send message', success: false };
    }
  }
}

// Export singleton instance
export const targccApi = new TargccApiService();
export default targccApi;
```

---

### Task 5: Create Basic Layout Components

**File:** `src/components/Layout/Layout.tsx`

```typescript
import React from 'react';
import { Box, CssBaseline, ThemeProvider, createTheme } from '@mui/material';
import Header from './Header';
import Sidebar from './Sidebar';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box sx={{ display: 'flex', minHeight: '100vh' }}>
        <Sidebar />
        <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
          <Header />
          <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
            {children}
          </Box>
        </Box>
      </Box>
    </ThemeProvider>
  );
};

export default Layout;
```

**File:** `src/components/Layout/Header.tsx`

```typescript
import React from 'react';
import { AppBar, Toolbar, Typography, IconButton } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';

const Header: React.FC = () => {
  return (
    <AppBar position="static">
      <Toolbar>
        <IconButton
          size="large"
          edge="start"
          color="inherit"
          aria-label="menu"
          sx={{ mr: 2 }}
        >
          <MenuIcon />
        </IconButton>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          TargCC Code Generator
        </Typography>
      </Toolbar>
    </AppBar>
  );
};

export default Header;
```

**File:** `src/components/Layout/Sidebar.tsx`

```typescript
import React from 'react';
import {
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import CodeIcon from '@mui/icons-material/Code';
import SecurityIcon from '@mui/icons-material/Security';
import ChatIcon from '@mui/icons-material/Chat';

const drawerWidth = 240;

const Sidebar: React.FC = () => {
  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
        },
      }}
    >
      <List>
        <ListItem button>
          <ListItemIcon>
            <DashboardIcon />
          </ListItemIcon>
          <ListItemText primary="Dashboard" />
        </ListItem>
        <ListItem button>
          <ListItemIcon>
            <CodeIcon />
          </ListItemIcon>
          <ListItemText primary="Generate" />
        </ListItem>
        <ListItem button>
          <ListItemIcon>
            <SecurityIcon />
          </ListItemIcon>
          <ListItemText primary="Analyze" />
        </ListItem>
        <Divider />
        <ListItem button>
          <ListItemIcon>
            <ChatIcon />
          </ListItemIcon>
          <ListItemText primary="AI Chat" />
        </ListItem>
      </List>
    </Drawer>
  );
};

export default Sidebar;
```

---

### Task 6: Create Simple Dashboard

**File:** `src/components/Dashboard/Dashboard.tsx`

```typescript
import React from 'react';
import {
  Container,
  Grid,
  Paper,
  Typography,
  Box,
} from '@mui/material';

const Dashboard: React.FC = () => {
  return (
    <Container maxWidth="lg">
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6">Tables</Typography>
            <Typography variant="h3">12</Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6">Generated</Typography>
            <Typography variant="h3">8</Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6">Tests</Typography>
            <Typography variant="h3" color="success.main">
              715
            </Typography>
          </Paper>
        </Grid>
      </Grid>
      <Box sx={{ mt: 3 }}>
        <Paper sx={{ p: 2 }}>
          <Typography variant="h6" gutterBottom>
            Quick Actions
          </Typography>
          <Typography>
            Welcome to TargCC Code Generator! Select an action from the sidebar
            to get started.
          </Typography>
        </Paper>
      </Box>
    </Container>
  );
};

export default Dashboard;
```

---

### Task 7: Update App.tsx

**File:** `src/App.tsx`

```typescript
import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import Dashboard from './components/Dashboard/Dashboard';

const App: React.FC = () => {
  return (
    <Router>
      <Layout>
        <Routes>
          <Route path="/" element={<Dashboard />} />
        </Routes>
      </Layout>
    </Router>
  );
};

export default App;
```

---

### Task 8: Create Basic Tests

**File:** `src/components/Dashboard/Dashboard.test.tsx`

```typescript
import React from 'react';
import { render, screen } from '@testing-library/react';
import Dashboard from './Dashboard';

describe('Dashboard Component', () => {
  it('renders dashboard title', () => {
    render(<Dashboard />);
    expect(screen.getByText('Dashboard')).toBeInTheDocument();
  });

  it('displays tables count', () => {
    render(<Dashboard />);
    expect(screen.getByText('Tables')).toBeInTheDocument();
    expect(screen.getByText('12')).toBeInTheDocument();
  });

  it('displays generated count', () => {
    render(<Dashboard />);
    expect(screen.getByText('Generated')).toBeInTheDocument();
    expect(screen.getByText('8')).toBeInTheDocument();
  });

  it('displays tests count', () => {
    render(<Dashboard />);
    expect(screen.getByText('Tests')).toBeInTheDocument();
    expect(screen.getByText('715')).toBeInTheDocument();
  });

  it('displays welcome message', () => {
    render(<Dashboard />);
    expect(screen.getByText(/Welcome to TargCC/i)).toBeInTheDocument();
  });
});
```

**File:** `src/App.test.tsx`

```typescript
import React from 'react';
import { render } from '@testing-library/react';
import App from './App';

describe('App Component', () => {
  it('renders without crashing', () => {
    render(<App />);
  });

  it('renders layout with dashboard', () => {
    const { container } = render(<App />);
    expect(container.querySelector('main')).toBeInTheDocument();
  });
});
```

---

### Task 9: Run and Test

**Commands:**

```bash
# Install all dependencies
npm install

# Run development server
npm start

# Should open browser at http://localhost:3000

# Run tests
npm test

# Build for production (verify it works)
npm run build
```

---

## 🎯 Day 21 Success Criteria

- [ ] React project created with TypeScript
- [ ] All dependencies installed (MUI, Router, Query, Axios)
- [ ] TypeScript and ESLint configured
- [ ] Project structure created
- [ ] Type definitions complete
- [ ] API service created
- [ ] Layout components working (Header, Sidebar, Layout)
- [ ] Dashboard component displays
- [ ] App runs on http://localhost:3000
- [ ] Tests pass (7+ tests)
- [ ] Build succeeds

---

## 📊 Expected Output

### When running `npm start`:

```
Compiled successfully!

You can now view targcc.webui in the browser.

  Local:            http://localhost:3000
  On Your Network:  http://192.168.1.x:3000

Note that the development build is not optimized.
To create a production build, use npm run build.
```

### Browser Display:
- Header with "TargCC Code Generator" title
- Sidebar with menu items (Dashboard, Generate, Analyze, AI Chat)
- Dashboard with 3 stat cards (Tables: 12, Generated: 8, Tests: 715)
- Welcome message in main area

---

## 📝 Notes for Next Session (Day 22)

**After Day 21 completes, Day 22 will:**
1. Add React Query integration
2. Create data fetching hooks
3. Connect to backend API (when ready)
4. Add error handling and loading states
5. Create more UI components

**Backend API Note:**
The Web UI will need a backend API wrapper. This will be created in Day 25 using ASP.NET Core Minimal API that wraps the CLI commands.

For now, the frontend will work with mock data or handle API errors gracefully.

---

## 🔗 Reference Resources

- **Material-UI Docs:** https://mui.com/
- **React Router:** https://reactrouter.com/
- **React Query:** https://tanstack.com/query/latest
- **TypeScript:** https://www.typescriptlang.org/
- **Create React App:** https://create-react-app.dev/

---

## 💾 Git Workflow

**After completing Day 21:**

```bash
git add src/TargCC.WebUI
git commit -m "feat(ui): Day 21 - React project setup with TypeScript

- Created React app with TypeScript
- Installed MUI, Router, Query, Axios
- Configured TypeScript and ESLint
- Created project structure
- Implemented type definitions
- Created API service
- Built Layout components (Header, Sidebar, Layout)
- Created Dashboard component
- Added 7 tests
- All tests passing

Phase 3C Day 21 complete"
```

---

**Document Created:** 28/11/2025  
**For Session:** Day 21 (Phase 3C Start)  
**Estimated Time:** 4-5 hours  
**Prerequisites:** Node.js 18+ installed  
**Status:** Ready to begin! 🚀
