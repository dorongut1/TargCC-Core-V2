# 🎨 React UI Generator - אפיון מפורט

**תאריך יצירה:** 01/12/2025
**גרסה:** 1.0
**סטטוס:** מאושר לביצוע
**משך זמן משוער:** 4 שבועות (20 ימי עבודה)
**נקודת התחלה:** Phase 3E (המשך Phase 3C)

---

## 📋 תוכן עניינים

1. [רקע והקשר](#רקע-והקשר)
2. [מטרת הפרויקט](#מטרת-הפרויקט)
3. [מצב נוכחי](#מצב-נוכחי)
4. [ארכיטקטורה טכנית](#ארכיטקטורה-טכנית)
5. [רכיבים מפורטים](#רכיבים-מפורטים)
6. [תהליך יצירה](#תהליך-יצירה)
7. [פירוט משימות](#פירוט-משימות)
8. [תכנית ביצוע](#תכנית-ביצוע)
9. [קריטריוני הצלחה](#קריטריוני-הצלחה)

---

## 🎯 רקע והקשר

### TargCC Legacy - WinF Generator

ב-**TargCC המקורי** (VB.NET), הכלי יצר אוטומטית **ממשק משתמש מלא** עבור כל טבלה:

#### מה נוצר ב-WinF (Legacy):

```
לכל טבלה (Table) נוצרו 2 קונטרולים:

1️⃣ ctlXXX (Entity Control) - פורם לעריכת רשומה בודדת
   ├── TextBoxes לכל שדה
   ├── ComboBoxes אוטומטיים לכל Foreign Key
   ├── Buttons (Save, Delete, Cancel)
   ├── Validation אוטומטית
   ├── Panels עם קישורים לילדים והורים
   └── Event handlers (BeforeSave, AfterSave, etc.)

2️⃣ ctlcXXX (Collection Control) - גריד לרשימת רשומות
   ├── DataGridView עם כל הטורים
   ├── Auto sorting
   ├── Auto filtering
   ├── Double-click → opens entity form
   ├── Context menu (Add, Edit, Delete)
   └── Export to CSV/Excel

דוגמה: טבלת Customer
✅ ctlCustomer.vb        (800 שורות קוד!)
✅ ctlcCustomer.vb       (400 שורות קוד!)
✅ Menu entry אוטומטי
✅ ComboBoxes לכל Foreign Key
```

### TargCC V2 - המצב הנוכחי

ב-**TargCC V2** (C# + React), הכלי מייצר:

✅ **Backend:** Entity, Repository, CQRS, API Controller (עובד!)
✅ **Frontend Infrastructure:** Dashboard, Tables page, Wizard (עובד!)
❌ **Auto-Generated UI Components:** **חסר!**

**החסר:**
- אין Entity Forms אוטומטיים
- אין Collection Grids אוטומטיים
- אין TypeScript types אוטומטיים
- אין React hooks אוטומטיים
- צריך לכתוב את כל ה-UI ידנית!

---

## 🎯 מטרת הפרויקט

### ליצור React UI Generator שמייצר אוטומטית:

#### 1. **Entity Form Component** (כמו ctlXXX)
```tsx
// Generated: CustomerForm.tsx
<CustomerForm
  customerId={123}
  onSave={() => {}}
  onCancel={() => {}}
/>
```

**מה כולל:**
- Form עם כל השדות
- Validation (Formik + Yup)
- Material-UI components
- Auto ComboBoxes ל-Foreign Keys
- Handle prefixes (eno_, ent_, lkp_, enm_, etc.)
- Save/Cancel/Delete buttons
- Error handling

#### 2. **Collection Grid Component** (כמו ctlcXXX)
```tsx
// Generated: CustomerGrid.tsx
<CustomerGrid
  onRowClick={(customer) => {}}
  filters={{}}
/>
```

**מה כולל:**
- Material-UI DataGrid
- Sorting
- Filtering
- Pagination
- Actions (View, Edit, Delete)
- Export (CSV, Excel)
- Row selection

#### 3. **TypeScript Types**
```typescript
// Generated: Customer.types.ts
export interface Customer {
  id: number;
  name: string;
  email: string;
  statusId: number;
  // ... all fields
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {
  id: number;
}
```

#### 4. **React Hooks**
```typescript
// Generated: useCustomer.ts
export const useCustomer = (id: number) => {
  // React Query integration
  const { data, isLoading, error } = useQuery(['customer', id], ...)
  return { customer: data, isLoading, error, ... }
}

export const useCustomers = (filters?) => {
  // List hook
}

export const useCreateCustomer = () => {
  // Mutation hook
}
```

#### 5. **API Integration**
```typescript
// Generated: customerApi.ts
export const customerApi = {
  getById: (id: number) => api.get<Customer>(`/api/customers/${id}`),
  getAll: (params?) => api.get<Customer[]>('/api/customers', { params }),
  create: (data: CreateCustomerRequest) => api.post('/api/customers', data),
  update: (id, data) => api.put(`/api/customers/${id}`, data),
  delete: (id) => api.delete(`/api/customers/${id}`)
}
```

#### 6. **Page Component**
```tsx
// Generated: CustomersPage.tsx
export const CustomersPage = () => {
  const [selectedId, setSelectedId] = useState<number | null>(null);

  return (
    <>
      <CustomerGrid onRowClick={(c) => setSelectedId(c.id)} />
      {selectedId && (
        <CustomerFormDialog
          customerId={selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </>
  );
};
```

---

## 📊 מצב נוכחי

### מה קיים בפרויקט:

#### Backend Generators (✅ עובד):
```
src/TargCC.Core.Generators/
├── Entities/
│   ├── EntityGenerator.cs          ✅
│   ├── PropertyGenerator.cs        ✅
│   ├── MethodGenerator.cs          ✅
│   └── RelationshipGenerator.cs    ✅
├── Api/
│   └── ApiControllerGenerator.cs   ✅
├── CQRS/
│   ├── QueryGenerator.cs           ✅
│   ├── CommandGenerator.cs         ✅
│   └── DtoGenerator.cs             ✅
├── Repositories/
│   └── RepositoryGenerator.cs      ✅
└── Data/
    └── DbContextGenerator.cs       ✅
```

**סה"כ:** 44 קבצי C# של Generators

#### Frontend Infrastructure (✅ עובד):
```
src/TargCC.WebUI/src/
├── pages/
│   ├── Dashboard.tsx               ✅
│   ├── Tables.tsx                  ✅
│   ├── Connections.tsx             ✅
│   ├── Schema.tsx                  ✅
│   └── CodeDemo.tsx                ✅
├── components/
│   ├── wizard/                     ✅ (4 components)
│   ├── generation/                 ✅
│   ├── schema/                     ✅
│   ├── connections/                ✅
│   └── common/                     ✅
├── api/
│   ├── config.ts                   ✅
│   ├── schemaApi.ts                ✅
│   └── generationApi.ts            ✅
└── hooks/
    ├── useSchema.ts                ✅
    └── useGenerationHistory.ts     ✅
```

**סה"כ:** 109 קבצי TypeScript/TSX

#### Backend API (✅ עובד):
```
src/TargCC.WebAPI/
├── Controllers/
│   ├── HealthController.cs         ✅
│   ├── SchemaController.cs         ✅
│   └── GenerationController.cs     ✅
├── Services/
│   ├── SchemaService.cs            ✅
│   └── GenerationService.cs        ✅
└── Program.cs                      ✅
```

### ❌ מה חסר:

```
src/TargCC.Core.Generators/
└── UI/                             ❌ NOT EXISTS!
    ├── ReactEntityFormGenerator.cs
    ├── ReactCollectionGridGenerator.cs
    ├── TypeScriptTypeGenerator.cs
    ├── ReactHookGenerator.cs
    ├── ReactApiGenerator.cs
    └── ReactPageGenerator.cs
```

---

## 🏗️ ארכיטקטורה טכנית

### סקירה כללית

```
┌─────────────────────────────────────────────────────────┐
│                  TargCC V2 Architecture                 │
│                                                         │
│  ┌────────────────────────────────────────────────┐   │
│  │  CLI / WebUI                                   │   │
│  │  "targcc generate ui <Table>"                  │   │
│  └──────────────────┬─────────────────────────────┘   │
│                     ↓                                   │
│  ┌────────────────────────────────────────────────┐   │
│  │  DatabaseAnalyzer                              │   │
│  │  Reads schema → DatabaseSchema object          │   │
│  └──────────────────┬─────────────────────────────┘   │
│                     ↓                                   │
│  ┌────────────────────────────────────────────────┐   │
│  │  Backend Generators (Existing)                 │   │
│  │  • EntityGenerator                             │   │
│  │  • ApiControllerGenerator                      │   │
│  │  • RepositoryGenerator                         │   │
│  │  • CQRSGenerators                              │   │
│  └────────────────────────────────────────────────┘   │
│                     ↓                                   │
│  ┌────────────────────────────────────────────────┐   │
│  │  🆕 UI Generators (NEW!)                       │   │
│  │  • ReactEntityFormGenerator                    │   │
│  │  • ReactCollectionGridGenerator                │   │
│  │  • TypeScriptTypeGenerator                     │   │
│  │  • ReactHookGenerator                          │   │
│  │  • ReactApiGenerator                           │   │
│  │  • ReactPageGenerator                          │   │
│  └──────────────────┬─────────────────────────────┘   │
│                     ↓                                   │
│  ┌────────────────────────────────────────────────┐   │
│  │  File Writer                                   │   │
│  │  Writes to: <Project>/src/generated/          │   │
│  └────────────────────────────────────────────────┘   │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### מיקום בפרויקט:

```
Solution Structure:

TargCC.Core.sln
├── src/
│   ├── TargCC.Core.Generators/          ← Existing
│   │   ├── Entities/
│   │   ├── Api/
│   │   ├── CQRS/
│   │   └── UI/                          ← 🆕 NEW FOLDER!
│   │       ├── ReactEntityFormGenerator.cs
│   │       ├── ReactCollectionGridGenerator.cs
│   │       ├── TypeScriptTypeGenerator.cs
│   │       ├── ReactHookGenerator.cs
│   │       ├── ReactApiGenerator.cs
│   │       ├── ReactPageGenerator.cs
│   │       └── Templates/               ← Handlebars templates
│   │           ├── EntityForm.hbs
│   │           ├── CollectionGrid.hbs
│   │           ├── Types.hbs
│   │           ├── Hooks.hbs
│   │           ├── Api.hbs
│   │           └── Page.hbs
│   │
│   ├── TargCC.CLI/                      ← Existing
│   │   └── Commands/
│   │       └── GenerateCommand.cs       ← Update to add "ui" option
│   │
│   └── TargCC.WebUI/                    ← Existing
│       └── src/
│           └── generated/               ← 🆕 Generated code goes here!
│               ├── types/
│               │   └── Customer.types.ts
│               ├── api/
│               │   └── customerApi.ts
│               ├── hooks/
│               │   └── useCustomer.ts
│               ├── components/
│               │   ├── CustomerForm.tsx
│               │   └── CustomerGrid.tsx
│               └── pages/
│                   └── CustomersPage.tsx
```

### תהליך הגנרציה:

```
Input: Table "Customer"
         ↓
┌────────────────────────────────────────────────┐
│ 1. TypeScriptTypeGenerator                    │
│    → Customer.types.ts                         │
│    → CreateCustomerRequest, UpdateCustomer...  │
└────────────────────────────────────────────────┘
         ↓
┌────────────────────────────────────────────────┐
│ 2. ReactApiGenerator                           │
│    → customerApi.ts                            │
│    → getById, getAll, create, update, delete   │
└────────────────────────────────────────────────┘
         ↓
┌────────────────────────────────────────────────┐
│ 3. ReactHookGenerator                          │
│    → useCustomer.ts                            │
│    → useCustomer, useCustomers, useMutations   │
└────────────────────────────────────────────────┘
         ↓
┌────────────────────────────────────────────────┐
│ 4. ReactEntityFormGenerator                    │
│    → CustomerForm.tsx                          │
│    → Form with all fields + validation         │
└────────────────────────────────────────────────┘
         ↓
┌────────────────────────────────────────────────┐
│ 5. ReactCollectionGridGenerator                │
│    → CustomerGrid.tsx                          │
│    → DataGrid with sorting/filtering           │
└────────────────────────────────────────────────┘
         ↓
┌────────────────────────────────────────────────┐
│ 6. ReactPageGenerator                          │
│    → CustomersPage.tsx                         │
│    → Full page with Grid + Form dialog         │
└────────────────────────────────────────────────┘

Output: 6 files, ~2000 lines of code!
```

---

## 🧩 רכיבים מפורטים

### 1. TypeScriptTypeGenerator

**קובץ:** `src/TargCC.Core.Generators/UI/TypeScriptTypeGenerator.cs`

**מטרה:** יוצר TypeScript types מטבלאות

**Output:** `Customer.types.ts`

```typescript
// Generated by TargCC
// Table: Customer
// Generated: 2025-12-01 15:30:00

export interface Customer {
  id: number;
  name: string;
  email: string;
  passwordHashed?: string;          // eno_ prefix
  creditCard?: string;               // ent_ prefix (encrypted)
  statusCode: string;                // lkp_ prefix
  statusText: string;                // lkp_ prefix
  typeEnum: CustomerType;            // enm_ prefix
  orderCount: number;                // agg_ prefix
  createdAt: Date;
  updatedAt: Date;
}

export enum CustomerType {
  Undefined = 0,
  Retail = 1,
  Wholesale = 2,
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
  plainPassword: string;             // eno_ → plain password
  creditCard?: string;
  statusCode: string;
  typeEnum: CustomerType;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {
  id: number;
}

export interface CustomerFilters {
  name?: string;
  statusCode?: string;
  typeEnum?: CustomerType;
  createdFrom?: Date;
  createdTo?: Date;
}
```

**לוגיקה:**
- סורק את כל העמודות
- ממיר SQL types ל-TypeScript types
- מזהה prefixes (eno_, ent_, lkp_, enm_)
- יוצר interfaces מתאימים

**Prefixes Handling:**

| Prefix | SQL | TypeScript | הערה |
|--------|-----|------------|------|
| `eno_` | `VARCHAR(64)` | `passwordHashed?: string` | Hashed, optional ב-entity |
| `ent_` | `VARCHAR(MAX)` | `string` | Encrypted/Decrypted automatically |
| `lkp_` | `VARCHAR(10)` | `statusCode: string` + `statusText: string` | 2 properties! |
| `enm_` | `VARCHAR(20)` | `enum CustomerType` | Creates enum |
| `loc_` | `NVARCHAR(100)` | `name: string` + `nameLocalized: string` | Localized |
| `clc_` | `DECIMAL` | `readonly total: number` | Calculated, read-only |
| `blg_` | `DECIMAL` | `readonly discount: number` | Business logic only |
| `agg_` | `INT` | `readonly orderCount: number` | Aggregate, read-only |
| `spt_` | `NVARCHAR(MAX)` | `comments: string` | Separate update |

---

### 2. ReactApiGenerator

**קובץ:** `src/TargCC.Core.Generators/UI/ReactApiGenerator.cs`

**מטרה:** יוצר API client functions

**Output:** `customerApi.ts`

```typescript
// Generated by TargCC
// Table: Customer
// Base URL: /api/customers

import { api } from '../config';
import type {
  Customer,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  CustomerFilters,
} from '../types/Customer.types';

export const customerApi = {
  /**
   * Get customer by ID
   */
  getById: async (id: number): Promise<Customer> => {
    const response = await api.get<Customer>(`/api/customers/${id}`);
    return response.data;
  },

  /**
   * Get all customers with optional filters
   */
  getAll: async (filters?: CustomerFilters): Promise<Customer[]> => {
    const response = await api.get<Customer[]>('/api/customers', {
      params: filters,
    });
    return response.data;
  },

  /**
   * Get customers by status (from non-unique index)
   */
  getByStatus: async (statusCode: string): Promise<Customer[]> => {
    const response = await api.get<Customer[]>(
      `/api/customers/by-status/${statusCode}`
    );
    return response.data;
  },

  /**
   * Create new customer
   */
  create: async (data: CreateCustomerRequest): Promise<Customer> => {
    const response = await api.post<Customer>('/api/customers', data);
    return response.data;
  },

  /**
   * Update existing customer
   */
  update: async (
    id: number,
    data: UpdateCustomerRequest
  ): Promise<Customer> => {
    const response = await api.put<Customer>(`/api/customers/${id}`, data);
    return response.data;
  },

  /**
   * Update separate field (spt_ prefix)
   */
  updateComments: async (
    id: number,
    comments: string
  ): Promise<void> => {
    await api.patch(`/api/customers/${id}/comments`, { comments });
  },

  /**
   * Delete customer
   */
  delete: async (id: number): Promise<void> => {
    await api.delete(`/api/customers/${id}`);
  },

  /**
   * Get customer's orders (one-to-many relationship)
   */
  getOrders: async (id: number): Promise<Order[]> => {
    const response = await api.get<Order[]>(`/api/customers/${id}/orders`);
    return response.data;
  },
};
```

**לוגיקה:**
- יוצר function לכל Stored Procedure
- GetByID, GetAll, GetByXXX (מ-indexes)
- Create, Update, Delete
- UpdateSeparate (לשדות spt_)
- FillXXX (לילדים - one-to-many)

---

### 3. ReactHookGenerator

**קובץ:** `src/TargCC.Core.Generators/UI/ReactHookGenerator.cs`

**מטרה:** יוצר React hooks עם React Query

**Output:** `useCustomer.ts`

```typescript
// Generated by TargCC
// Table: Customer
// Uses: React Query

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customerApi } from '../api/customerApi';
import type {
  Customer,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  CustomerFilters,
} from '../types/Customer.types';

/**
 * Hook to fetch single customer by ID
 */
export const useCustomer = (id: number | null) => {
  return useQuery({
    queryKey: ['customer', id],
    queryFn: () => customerApi.getById(id!),
    enabled: id !== null,
  });
};

/**
 * Hook to fetch all customers with filters
 */
export const useCustomers = (filters?: CustomerFilters) => {
  return useQuery({
    queryKey: ['customers', filters],
    queryFn: () => customerApi.getAll(filters),
  });
};

/**
 * Hook to create customer
 */
export const useCreateCustomer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCustomerRequest) => customerApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
};

/**
 * Hook to update customer
 */
export const useUpdateCustomer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateCustomerRequest }) =>
      customerApi.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['customer', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
};

/**
 * Hook to delete customer
 */
export const useDeleteCustomer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => customerApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
    },
  });
};

/**
 * Hook to get customer's orders (relationship)
 */
export const useCustomerOrders = (customerId: number | null) => {
  return useQuery({
    queryKey: ['customer-orders', customerId],
    queryFn: () => customerApi.getOrders(customerId!),
    enabled: customerId !== null,
  });
};
```

**לוגיקה:**
- יוצר hook לכל API function
- Query hooks (useCustomer, useCustomers)
- Mutation hooks (useCreate, useUpdate, useDelete)
- Relationship hooks (useCustomerOrders)
- Auto cache invalidation

---

### 4. ReactEntityFormGenerator

**קובץ:** `src/TargCC.Core.Generators/UI/ReactEntityFormGenerator.cs`

**מטרה:** יוצר React form component

**Output:** `CustomerForm.tsx`

```tsx
// Generated by TargCC
// Table: Customer

import React from 'react';
import {
  Box,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Button,
  Grid,
  FormHelperText,
  IconButton,
  InputAdornment,
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import {
  useCustomer,
  useCreateCustomer,
  useUpdateCustomer,
} from '../hooks/useCustomer';
import type { CreateCustomerRequest } from '../types/Customer.types';

export interface CustomerFormProps {
  customerId?: number;
  onSave?: () => void;
  onCancel?: () => void;
}

const validationSchema = Yup.object({
  name: Yup.string()
    .required('Name is required')
    .max(100, 'Name must be at most 100 characters'),
  email: Yup.string()
    .required('Email is required')
    .email('Invalid email format')
    .max(255, 'Email must be at most 255 characters'),
  plainPassword: Yup.string()
    .required('Password is required')
    .min(8, 'Password must be at least 8 characters'),
  statusCode: Yup.string().required('Status is required'),
  typeEnum: Yup.number().required('Type is required'),
});

export const CustomerForm: React.FC<CustomerFormProps> = ({
  customerId,
  onSave,
  onCancel,
}) => {
  const [showPassword, setShowPassword] = React.useState(false);

  // Fetch customer if editing
  const { data: customer, isLoading } = useCustomer(customerId ?? null);

  // Mutations
  const createMutation = useCreateCustomer();
  const updateMutation = useUpdateCustomer();

  // Formik
  const formik = useFormik<CreateCustomerRequest>({
    initialValues: {
      name: customer?.name ?? '',
      email: customer?.email ?? '',
      plainPassword: '',
      statusCode: customer?.statusCode ?? '',
      typeEnum: customer?.typeEnum ?? 0,
    },
    validationSchema,
    enableReinitialize: true,
    onSubmit: async (values) => {
      try {
        if (customerId) {
          await updateMutation.mutateAsync({
            id: customerId,
            data: { ...values, id: customerId },
          });
        } else {
          await createMutation.mutateAsync(values);
        }
        onSave?.();
      } catch (error) {
        console.error('Failed to save customer:', error);
      }
    },
  });

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <Box component="form" onSubmit={formik.handleSubmit} sx={{ p: 2 }}>
      <Grid container spacing={2}>
        {/* Name Field */}
        <Grid item xs={12} sm={6}>
          <TextField
            fullWidth
            id="name"
            name="name"
            label="Name"
            value={formik.values.name}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={formik.touched.name && Boolean(formik.errors.name)}
            helperText={formik.touched.name && formik.errors.name}
            required
          />
        </Grid>

        {/* Email Field */}
        <Grid item xs={12} sm={6}>
          <TextField
            fullWidth
            id="email"
            name="email"
            label="Email"
            type="email"
            value={formik.values.email}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={formik.touched.email && Boolean(formik.errors.email)}
            helperText={formik.touched.email && formik.errors.email}
            required
          />
        </Grid>

        {/* Password Field (eno_ prefix) */}
        <Grid item xs={12} sm={6}>
          <TextField
            fullWidth
            id="plainPassword"
            name="plainPassword"
            label="Password"
            type={showPassword ? 'text' : 'password'}
            value={formik.values.plainPassword}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={
              formik.touched.plainPassword &&
              Boolean(formik.errors.plainPassword)
            }
            helperText={
              formik.touched.plainPassword && formik.errors.plainPassword
            }
            required
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    onClick={() => setShowPassword(!showPassword)}
                    edge="end"
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
        </Grid>

        {/* Status Field (lkp_ prefix → ComboBox) */}
        <Grid item xs={12} sm={6}>
          <FormControl
            fullWidth
            error={formik.touched.statusCode && Boolean(formik.errors.statusCode)}
          >
            <InputLabel id="statusCode-label">Status</InputLabel>
            <Select
              labelId="statusCode-label"
              id="statusCode"
              name="statusCode"
              value={formik.values.statusCode}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              label="Status"
              required
            >
              <MenuItem value="">
                <em>Choose</em>
              </MenuItem>
              <MenuItem value="ACT">Active</MenuItem>
              <MenuItem value="INA">Inactive</MenuItem>
              <MenuItem value="PEN">Pending</MenuItem>
            </Select>
            {formik.touched.statusCode && formik.errors.statusCode && (
              <FormHelperText>{formik.errors.statusCode}</FormHelperText>
            )}
          </FormControl>
        </Grid>

        {/* Type Field (enm_ prefix → Enum) */}
        <Grid item xs={12} sm={6}>
          <FormControl
            fullWidth
            error={formik.touched.typeEnum && Boolean(formik.errors.typeEnum)}
          >
            <InputLabel id="typeEnum-label">Type</InputLabel>
            <Select
              labelId="typeEnum-label"
              id="typeEnum"
              name="typeEnum"
              value={formik.values.typeEnum}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
              label="Type"
              required
            >
              <MenuItem value={0}>Undefined</MenuItem>
              <MenuItem value={1}>Retail</MenuItem>
              <MenuItem value={2}>Wholesale</MenuItem>
            </Select>
            {formik.touched.typeEnum && formik.errors.typeEnum && (
              <FormHelperText>{formik.errors.typeEnum}</FormHelperText>
            )}
          </FormControl>
        </Grid>

        {/* Buttons */}
        <Grid item xs={12}>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
            <Button variant="outlined" onClick={onCancel}>
              Cancel
            </Button>
            <Button
              type="submit"
              variant="contained"
              disabled={formik.isSubmitting}
            >
              {customerId ? 'Update' : 'Create'}
            </Button>
          </Box>
        </Grid>
      </Grid>
    </Box>
  );
};
```

**פיצ'רים:**
- ✅ Formik + Yup validation
- ✅ Material-UI components
- ✅ Handle all prefixes (eno_, lkp_, enm_, etc.)
- ✅ Auto ComboBoxes ל-Foreign Keys
- ✅ Password show/hide
- ✅ Error handling
- ✅ Loading state

---

### 5. ReactCollectionGridGenerator

**קובץ:** `src/TargCC.Core.Generators/UI/ReactCollectionGridGenerator.cs`

**מטרה:** יוצר DataGrid component

**Output:** `CustomerGrid.tsx`

```tsx
// Generated by TargCC
// Table: Customer

import React from 'react';
import {
  DataGrid,
  GridColDef,
  GridActionsCellItem,
  GridRowParams,
} from '@mui/x-data-grid';
import { Box, Chip, IconButton } from '@mui/material';
import { Edit, Delete, Visibility } from '@mui/icons-material';
import { useCustomers, useDeleteCustomer } from '../hooks/useCustomer';
import type { Customer, CustomerFilters } from '../types/Customer.types';

export interface CustomerGridProps {
  filters?: CustomerFilters;
  onRowClick?: (customer: Customer) => void;
  onEdit?: (customer: Customer) => void;
  onDelete?: (id: number) => void;
}

export const CustomerGrid: React.FC<CustomerGridProps> = ({
  filters,
  onRowClick,
  onEdit,
  onDelete,
}) => {
  const { data: customers = [], isLoading } = useCustomers(filters);
  const deleteMutation = useDeleteCustomer();

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this customer?')) {
      try {
        await deleteMutation.mutateAsync(id);
        onDelete?.(id);
      } catch (error) {
        console.error('Failed to delete customer:', error);
      }
    }
  };

  const columns: GridColDef[] = [
    {
      field: 'id',
      headerName: 'ID',
      width: 70,
      type: 'number',
    },
    {
      field: 'name',
      headerName: 'Name',
      width: 200,
      flex: 1,
    },
    {
      field: 'email',
      headerName: 'Email',
      width: 250,
      flex: 1,
    },
    {
      field: 'statusText',
      headerName: 'Status',
      width: 120,
      renderCell: (params) => (
        <Chip
          label={params.value}
          color={
            params.row.statusCode === 'ACT'
              ? 'success'
              : params.row.statusCode === 'INA'
              ? 'error'
              : 'default'
          }
          size="small"
        />
      ),
    },
    {
      field: 'orderCount',
      headerName: 'Orders',
      width: 100,
      type: 'number',
      align: 'center',
      headerAlign: 'center',
    },
    {
      field: 'createdAt',
      headerName: 'Created',
      width: 180,
      type: 'dateTime',
      valueGetter: (params) => new Date(params),
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'Actions',
      width: 120,
      getActions: (params: GridRowParams<Customer>) => [
        <GridActionsCellItem
          icon={<Visibility />}
          label="View"
          onClick={() => onRowClick?.(params.row)}
        />,
        <GridActionsCellItem
          icon={<Edit />}
          label="Edit"
          onClick={() => onEdit?.(params.row)}
        />,
        <GridActionsCellItem
          icon={<Delete />}
          label="Delete"
          onClick={() => handleDelete(params.row.id)}
        />,
      ],
    },
  ];

  return (
    <Box sx={{ height: 600, width: '100%' }}>
      <DataGrid
        rows={customers}
        columns={columns}
        loading={isLoading}
        pageSizeOptions={[10, 25, 50, 100]}
        initialState={{
          pagination: {
            paginationModel: { page: 0, pageSize: 25 },
          },
          sorting: {
            sortModel: [{ field: 'name', sort: 'asc' }],
          },
        }}
        checkboxSelection
        disableRowSelectionOnClick
        onRowClick={(params) => onRowClick?.(params.row)}
        sx={{
          '& .MuiDataGrid-row': {
            cursor: 'pointer',
          },
        }}
      />
    </Box>
  );
};
```

**פיצ'רים:**
- ✅ Material-UI DataGrid
- ✅ Sorting
- ✅ Pagination
- ✅ Filtering (via props)
- ✅ Actions (View, Edit, Delete)
- ✅ Checkbox selection
- ✅ Custom rendering (Chip for status)
- ✅ Loading state

---

### 6. ReactPageGenerator

**קובץ:** `src/TargCC.Core.Generators/UI/ReactPageGenerator.cs`

**מטרה:** יוצר Page component שמחבר הכל

**Output:** `CustomersPage.tsx`

```tsx
// Generated by TargCC
// Table: Customer

import React, { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Paper,
  Typography,
  Breadcrumbs,
  Link,
} from '@mui/material';
import { Add } from '@mui/icons-material';
import { CustomerGrid } from '../components/CustomerGrid';
import { CustomerForm } from '../components/CustomerForm';
import type { Customer } from '../types/Customer.types';

export const CustomersPage: React.FC = () => {
  const [selectedCustomerId, setSelectedCustomerId] = useState<number | null>(
    null
  );
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isCreating, setIsCreating] = useState(false);

  const handleRowClick = (customer: Customer) => {
    setSelectedCustomerId(customer.id);
    setIsFormOpen(true);
    setIsCreating(false);
  };

  const handleCreate = () => {
    setSelectedCustomerId(null);
    setIsFormOpen(true);
    setIsCreating(true);
  };

  const handleCloseForm = () => {
    setIsFormOpen(false);
    setSelectedCustomerId(null);
    setIsCreating(false);
  };

  const handleSave = () => {
    handleCloseForm();
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Breadcrumbs */}
      <Breadcrumbs sx={{ mb: 2 }}>
        <Link underline="hover" color="inherit" href="/">
          Home
        </Link>
        <Typography color="text.primary">Customers</Typography>
      </Breadcrumbs>

      {/* Header */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h4" component="h1">
          Customers
        </Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={handleCreate}
        >
          Add Customer
        </Button>
      </Box>

      {/* Grid */}
      <Paper sx={{ p: 2 }}>
        <CustomerGrid onRowClick={handleRowClick} />
      </Paper>

      {/* Form Dialog */}
      <Dialog
        open={isFormOpen}
        onClose={handleCloseForm}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          {isCreating ? 'Create Customer' : 'Edit Customer'}
        </DialogTitle>
        <DialogContent>
          <CustomerForm
            customerId={isCreating ? undefined : selectedCustomerId ?? undefined}
            onSave={handleSave}
            onCancel={handleCloseForm}
          />
        </DialogContent>
      </Dialog>
    </Box>
  );
};
```

**פיצ'רים:**
- ✅ Grid + Form Dialog
- ✅ Create/Edit modes
- ✅ Breadcrumbs
- ✅ Add button
- ✅ Full page layout

---

## 🔄 תהליך יצירה

### תרשים זרימה:

```
User runs:
$ targcc generate ui Customer

         ↓

┌────────────────────────────────────────────────┐
│ CLI: GenerateCommand.cs                        │
│ - Parse arguments                              │
│ - Load DatabaseSchema                          │
│ - Call UIGeneratorOrchestrator                 │
└────────────────┬───────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────┐
│ UIGeneratorOrchestrator                        │
│ - Orchestrates all 6 generators                │
│ - Ensures correct order                        │
└────────────────┬───────────────────────────────┘
                 ↓
         ┌───────┴───────┐
         ↓               ↓
┌────────────────┐  ┌────────────────┐
│ 1. Types       │  │ 2. API         │
│ Generator      │  │ Generator      │
└────────┬───────┘  └────────┬───────┘
         ↓                   ↓
         └───────┬───────────┘
                 ↓
         ┌───────┴───────┐
         ↓               ↓
┌────────────────┐  ┌────────────────┐
│ 3. Hooks       │  │ 4. Form        │
│ Generator      │  │ Generator      │
└────────┬───────┘  └────────┬───────┘
         ↓                   ↓
         └───────┬───────────┘
                 ↓
         ┌───────┴───────┐
         ↓               ↓
┌────────────────┐  ┌────────────────┐
│ 5. Grid        │  │ 6. Page        │
│ Generator      │  │ Generator      │
└────────┬───────┘  └────────┬───────┘
         ↓                   ↓
         └───────┬───────────┘
                 ↓
┌────────────────────────────────────────────────┐
│ FileWriter                                     │
│ - Write all 6 files                            │
│ - Update index files                           │
│ - Update routes                                │
└────────────────┬───────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────┐
│ Output:                                        │
│ ✅ Customer.types.ts                           │
│ ✅ customerApi.ts                              │
│ ✅ useCustomer.ts                              │
│ ✅ CustomerForm.tsx                            │
│ ✅ CustomerGrid.tsx                            │
│ ✅ CustomersPage.tsx                           │
└────────────────────────────────────────────────┘
```

### דוגמה מפורטת:

```bash
# User runs:
$ targcc generate ui Customer

# Step 1: Load schema
Loading database schema...
Found table: Customer (8 columns, 3 indexes, 1 FK)

# Step 2: Generate Types
Generating TypeScript types...
✅ Customer.types.ts (142 lines)
   - Customer interface
   - CustomerType enum
   - CreateCustomerRequest
   - UpdateCustomerRequest
   - CustomerFilters

# Step 3: Generate API
Generating API client...
✅ customerApi.ts (98 lines)
   - getById
   - getAll
   - getByStatus (from index)
   - create
   - update
   - delete

# Step 4: Generate Hooks
Generating React hooks...
✅ useCustomer.ts (121 lines)
   - useCustomer
   - useCustomers
   - useCreateCustomer
   - useUpdateCustomer
   - useDeleteCustomer

# Step 5: Generate Form
Generating entity form...
✅ CustomerForm.tsx (287 lines)
   - Form with 5 fields
   - Formik + Yup validation
   - Material-UI components
   - Password field with show/hide
   - Status ComboBox
   - Type enum Select

# Step 6: Generate Grid
Generating collection grid...
✅ CustomerGrid.tsx (156 lines)
   - DataGrid with 7 columns
   - Actions (View, Edit, Delete)
   - Sorting, pagination
   - Chip for status

# Step 7: Generate Page
Generating page component...
✅ CustomersPage.tsx (97 lines)
   - Grid + Form dialog
   - Create/Edit modes
   - Breadcrumbs
   - Add button

# Step 8: Update indexes
Updating index files...
✅ generated/index.ts
✅ generated/types/index.ts
✅ generated/api/index.ts
✅ generated/hooks/index.ts
✅ generated/components/index.ts
✅ generated/pages/index.ts

# Step 9: Update routes
Updating App.tsx routes...
✅ Added route: /customers

──────────────────────────────────────────
✅ Success! Generated 6 files (901 lines)
──────────────────────────────────────────

Next steps:
1. Review generated code in src/generated/
2. Run: npm run dev
3. Navigate to: http://localhost:5179/customers
4. Enjoy! 🎉
```

---

## 📅 פירוט משימות

### **Week 1: Foundation (Days 1-5)**

#### **Day 1: Project Setup & Architecture**
- [ ] Create `TargCC.Core.Generators/UI/` folder
- [ ] Create `IUIGenerator` interface
- [ ] Create `UIGeneratorOrchestrator.cs`
- [ ] Create base classes
- [ ] Setup unit test project structure
- [ ] Document architecture

**Deliverables:**
- ✅ UI folder structure
- ✅ Base interfaces
- ✅ Orchestrator skeleton
- ✅ 10+ unit tests

**Time:** 1 day

---

#### **Day 2: TypeScriptTypeGenerator**
- [ ] Create `TypeScriptTypeGenerator.cs`
- [ ] Implement interface generation
- [ ] Implement enum generation
- [ ] Handle all prefixes (eno_, ent_, lkp_, enm_, loc_, clc_, blg_, agg_, spt_)
- [ ] Create Request/Response types
- [ ] Create Filters type
- [ ] Write 20+ unit tests

**Deliverables:**
- ✅ TypeScriptTypeGenerator.cs (300+ lines)
- ✅ Customer.types.ts example
- ✅ 20+ unit tests

**Time:** 1 day

---

#### **Day 3: ReactApiGenerator**
- [ ] Create `ReactApiGenerator.cs`
- [ ] Generate CRUD functions
- [ ] Generate GetByXXX from indexes
- [ ] Generate UpdateSeparate for spt_ fields
- [ ] Generate relationship functions (FillXXX)
- [ ] Write 15+ unit tests

**Deliverables:**
- ✅ ReactApiGenerator.cs (250+ lines)
- ✅ customerApi.ts example
- ✅ 15+ unit tests

**Time:** 1 day

---

#### **Day 4: ReactHookGenerator**
- [ ] Create `ReactHookGenerator.cs`
- [ ] Generate Query hooks (useQuery)
- [ ] Generate Mutation hooks (useMutation)
- [ ] Generate relationship hooks
- [ ] Add cache invalidation logic
- [ ] Write 15+ unit tests

**Deliverables:**
- ✅ ReactHookGenerator.cs (200+ lines)
- ✅ useCustomer.ts example
- ✅ 15+ unit tests

**Time:** 1 day

---

#### **Day 5: Template System**
- [ ] Create Handlebars templates
- [ ] EntityForm.hbs template
- [ ] CollectionGrid.hbs template
- [ ] Types.hbs template
- [ ] Hooks.hbs template
- [ ] API.hbs template
- [ ] Page.hbs template
- [ ] Test template rendering

**Deliverables:**
- ✅ 6 Handlebars templates
- ✅ Template engine integration
- ✅ 10+ template tests

**Time:** 1 day

---

### **Week 2: UI Components (Days 6-10)**

#### **Day 6: ReactEntityFormGenerator - Basic**
- [ ] Create `ReactEntityFormGenerator.cs`
- [ ] Generate basic form structure
- [ ] Generate TextField components
- [ ] Generate form validation (Yup schema)
- [ ] Generate Formik integration
- [ ] Write 20+ unit tests

**Deliverables:**
- ✅ ReactEntityFormGenerator.cs (400+ lines)
- ✅ Basic form generation working
- ✅ 20+ unit tests

**Time:** 1 day

---

#### **Day 7: ReactEntityFormGenerator - Advanced**
- [ ] Handle eno_ prefix (password with show/hide)
- [ ] Handle lkp_ prefix (ComboBox)
- [ ] Handle enm_ prefix (Select with enum)
- [ ] Handle ent_ prefix (encrypted field)
- [ ] Handle loc_ prefix (localized field)
- [ ] Handle clc_/blg_/agg_ (read-only fields)
- [ ] Write 25+ unit tests

**Deliverables:**
- ✅ All prefixes handled
- ✅ CustomerForm.tsx example
- ✅ 25+ unit tests

**Time:** 1 day

---

#### **Day 8: ReactCollectionGridGenerator**
- [ ] Create `ReactCollectionGridGenerator.cs`
- [ ] Generate DataGrid component
- [ ] Generate columns from table schema
- [ ] Generate Actions column
- [ ] Handle different data types (date, number, boolean)
- [ ] Generate custom cell renderers (Chip for status)
- [ ] Write 20+ unit tests

**Deliverables:**
- ✅ ReactCollectionGridGenerator.cs (350+ lines)
- ✅ CustomerGrid.tsx example
- ✅ 20+ unit tests

**Time:** 1 day

---

#### **Day 9: ReactPageGenerator**
- [ ] Create `ReactPageGenerator.cs`
- [ ] Generate page layout
- [ ] Integrate Grid + Form Dialog
- [ ] Generate Breadcrumbs
- [ ] Generate Add button
- [ ] Generate routing configuration
- [ ] Write 15+ unit tests

**Deliverables:**
- ✅ ReactPageGenerator.cs (200+ lines)
- ✅ CustomersPage.tsx example
- ✅ 15+ unit tests

**Time:** 1 day

---

#### **Day 10: Integration & File Writing**
- [ ] Create `UIFileWriter.cs`
- [ ] Write all generated files
- [ ] Update index files
- [ ] Update App.tsx routes
- [ ] Create directory structure
- [ ] Handle file conflicts (.prt.tsx protection)
- [ ] Write 10+ integration tests

**Deliverables:**
- ✅ UIFileWriter.cs
- ✅ Complete file writing system
- ✅ 10+ integration tests

**Time:** 1 day

---

### **Week 3: CLI Integration & Testing (Days 11-15)**

#### **Day 11: CLI Command**
- [ ] Update `GenerateCommand.cs`
- [ ] Add "ui" option
- [ ] Add "ui-form" option (form only)
- [ ] Add "ui-grid" option (grid only)
- [ ] Add "ui-all" option (all tables)
- [ ] Test CLI commands

**Deliverables:**
- ✅ Updated GenerateCommand.cs
- ✅ All CLI options working
- ✅ 10+ CLI tests

**Time:** 1 day

---

#### **Day 12: Foreign Key Resolution**
- [ ] Auto-detect Foreign Keys
- [ ] Generate ComboBox for FK fields
- [ ] Load FK data (useXXX hooks)
- [ ] Display FK text (not just ID)
- [ ] Handle multi-level FKs
- [ ] Write 15+ tests

**Deliverables:**
- ✅ FK resolution working
- ✅ Auto ComboBoxes
- ✅ 15+ tests

**Time:** 1 day

---

#### **Day 13: Relationship Panels**
- [ ] Generate child relationship panels (Orders for Customer)
- [ ] Generate parent relationship links
- [ ] Generate one-to-one relationship displays
- [ ] Create RelationshipPanel component template
- [ ] Write 10+ tests

**Deliverables:**
- ✅ Relationship panels
- ✅ Parent/child navigation
- ✅ 10+ tests

**Time:** 1 day

---

#### **Day 14: End-to-End Testing**
- [ ] Create test database
- [ ] Generate UI for test tables
- [ ] Test all prefixes
- [ ] Test all relationships
- [ ] Test CRUD operations
- [ ] Fix bugs
- [ ] Write 20+ E2E tests

**Deliverables:**
- ✅ Full E2E test suite
- ✅ All bugs fixed
- ✅ 20+ E2E tests

**Time:** 1 day

---

#### **Day 15: Documentation & Examples**
- [ ] Write generator documentation
- [ ] Create usage examples
- [ ] Create video tutorial
- [ ] Update README.md
- [ ] Create migration guide (from WinF)
- [ ] Create best practices guide

**Deliverables:**
- ✅ Complete documentation
- ✅ 5+ examples
- ✅ Video tutorial
- ✅ Migration guide

**Time:** 1 day

---

### **Week 4: Advanced Features & Polish (Days 16-20)**

#### **Day 16: Advanced Validation**
- [ ] Server-side validation errors display
- [ ] Custom validation rules
- [ ] Cross-field validation
- [ ] Async validation (check if email exists)
- [ ] Write 15+ tests

**Deliverables:**
- ✅ Advanced validation
- ✅ 15+ tests

**Time:** 1 day

---

#### **Day 17: Advanced Grid Features**
- [ ] Custom filters
- [ ] Column visibility toggle
- [ ] Export to CSV/Excel
- [ ] Bulk delete
- [ ] Column resizing
- [ ] Write 15+ tests

**Deliverables:**
- ✅ Advanced grid features
- ✅ 15+ tests

**Time:** 1 day

---

#### **Day 18: Localization Support**
- [ ] Generate i18n keys
- [ ] Generate translation files
- [ ] Integrate with react-i18next
- [ ] Support loc_ prefix
- [ ] Write 10+ tests

**Deliverables:**
- ✅ Localization support
- ✅ 10+ tests

**Time:** 1 day

---

#### **Day 19: Performance Optimization**
- [ ] Code splitting
- [ ] Lazy loading
- [ ] Memoization
- [ ] Virtual scrolling for large grids
- [ ] Bundle size optimization
- [ ] Write performance tests

**Deliverables:**
- ✅ Optimized code
- ✅ Performance benchmarks

**Time:** 1 day

---

#### **Day 20: Final Testing & Release**
- [ ] Full regression testing
- [ ] Bug fixes
- [ ] Code review
- [ ] Performance testing
- [ ] Security audit
- [ ] Release v1.0

**Deliverables:**
- ✅ All tests passing
- ✅ Zero critical bugs
- ✅ Release notes
- ✅ v1.0 deployed

**Time:** 1 day

---

## 📆 תכנית ביצוע

### סיכום לוח זמנים:

```
┌─────────────────────────────────────────────────┐
│  React UI Generator - 4 Weeks (20 Days)         │
├─────────────────────────────────────────────────┤
│                                                 │
│  Week 1 (Days 1-5): Foundation                  │
│  ├─ Day 1: Architecture & Setup                 │
│  ├─ Day 2: TypeScriptTypeGenerator              │
│  ├─ Day 3: ReactApiGenerator                    │
│  ├─ Day 4: ReactHookGenerator                   │
│  └─ Day 5: Template System                      │
│                                                 │
│  Week 2 (Days 6-10): UI Components              │
│  ├─ Day 6: EntityFormGenerator - Basic          │
│  ├─ Day 7: EntityFormGenerator - Advanced       │
│  ├─ Day 8: CollectionGridGenerator              │
│  ├─ Day 9: PageGenerator                        │
│  └─ Day 10: Integration & File Writing          │
│                                                 │
│  Week 3 (Days 11-15): CLI & Testing             │
│  ├─ Day 11: CLI Command                         │
│  ├─ Day 12: Foreign Key Resolution              │
│  ├─ Day 13: Relationship Panels                 │
│  ├─ Day 14: End-to-End Testing                  │
│  └─ Day 15: Documentation                       │
│                                                 │
│  Week 4 (Days 16-20): Advanced & Polish         │
│  ├─ Day 16: Advanced Validation                 │
│  ├─ Day 17: Advanced Grid Features              │
│  ├─ Day 18: Localization                        │
│  ├─ Day 19: Performance                         │
│  └─ Day 20: Final Release                       │
│                                                 │
└─────────────────────────────────────────────────┘
```

### Milestones:

| Milestone | Date | Deliverable |
|-----------|------|-------------|
| **M1: Foundation Complete** | End of Week 1 | All base generators working |
| **M2: UI Components Complete** | End of Week 2 | Form + Grid + Page working |
| **M3: CLI Integration Complete** | End of Week 3 | CLI commands + E2E tests |
| **M4: Production Ready** | End of Week 4 | v1.0 Release |

---

## ✅ קריטריוני הצלחה

### טכניים:

1. **Code Generation:**
   - ✅ יוצר 6 קבצים לכל טבלה
   - ✅ סה"כ ~900-1000 שורות קוד per table
   - ✅ אפס שגיאות TypeScript
   - ✅ עובד עם כל ה-12 prefixes

2. **Testing:**
   - ✅ 200+ unit tests (all passing)
   - ✅ 20+ E2E tests (all passing)
   - ✅ 90%+ code coverage
   - ✅ Zero flaky tests

3. **Performance:**
   - ✅ Generation time < 5 seconds per table
   - ✅ Bundle size < 500KB per table
   - ✅ Page load time < 1 second
   - ✅ Form submit < 500ms

4. **Quality:**
   - ✅ TypeScript strict mode
   - ✅ ESLint zero errors
   - ✅ Prettier formatted
   - ✅ Accessibility (WCAG 2.1 AA)

### תפקודיים:

1. **CRUD Operations:**
   - ✅ Create entity works
   - ✅ Read/View entity works
   - ✅ Update entity works
   - ✅ Delete entity works

2. **Validation:**
   - ✅ Client-side validation works
   - ✅ Server-side validation displayed
   - ✅ Required fields enforced
   - ✅ Custom validators work

3. **Relationships:**
   - ✅ Foreign Keys → ComboBoxes
   - ✅ One-to-many → Child panels
   - ✅ One-to-one → Embedded forms
   - ✅ Many-to-many → Multi-select

4. **User Experience:**
   - ✅ Responsive design (mobile + desktop)
   - ✅ Loading states
   - ✅ Error messages clear
   - ✅ Keyboard navigation works

### עסקיים:

1. **Development Speed:**
   - ✅ 90% reduction in UI code writing
   - ✅ From 2-4 hours → 5-10 minutes per table
   - ✅ Zero manual boilerplate

2. **Maintenance:**
   - ✅ Schema change → Regenerate → Done
   - ✅ No manual updates needed
   - ✅ Consistent code style

3. **Scalability:**
   - ✅ Works for 1 table
   - ✅ Works for 100 tables
   - ✅ Works for 1000 tables

---

## 🎯 סיכום

### מה נבנה:

**6 Generators חדשים:**
1. TypeScriptTypeGenerator
2. ReactApiGenerator
3. ReactHookGenerator
4. ReactEntityFormGenerator
5. ReactCollectionGridGenerator
6. ReactPageGenerator

**Output לכל טבלה:**
- ✅ Customer.types.ts (~150 lines)
- ✅ customerApi.ts (~100 lines)
- ✅ useCustomer.ts (~120 lines)
- ✅ CustomerForm.tsx (~300 lines)
- ✅ CustomerGrid.tsx (~160 lines)
- ✅ CustomersPage.tsx (~100 lines)

**סה"כ:** ~930 lines of production-ready code per table!

### השוואה ל-Legacy:

| Feature | Legacy (WinF) | V2 (React) |
|---------|---------------|------------|
| **Entity Form** | ctlXXX.vb (800 lines) | CustomerForm.tsx (300 lines) |
| **Collection Grid** | ctlcXXX.vb (400 lines) | CustomerGrid.tsx (160 lines) |
| **Types** | ❌ None (VB types) | Customer.types.ts (150 lines) |
| **API Client** | ❌ Built-in to WS | customerApi.ts (100 lines) |
| **Hooks** | ❌ N/A | useCustomer.ts (120 lines) |
| **Page** | ❌ Menu only | CustomersPage.tsx (100 lines) |
| **Total Lines** | ~1200 | ~930 |
| **Technology** | VB.NET + WinForms | TypeScript + React + Material-UI |
| **Responsive** | ❌ No | ✅ Yes |
| **Modern UI** | ❌ 1995 look | ✅ 2025 Material Design |

---

## 🚀 Getting Started

### דוגמה מהירה:

```bash
# Step 1: Generate UI for a table
$ targcc generate ui Customer

# Output:
✅ Customer.types.ts
✅ customerApi.ts
✅ useCustomer.ts
✅ CustomerForm.tsx
✅ CustomerGrid.tsx
✅ CustomersPage.tsx

# Step 2: Run the app
$ cd src/TargCC.WebUI
$ npm run dev

# Step 3: Navigate to
http://localhost:5179/customers

# Step 4: Enjoy! 🎉
```

### Generate All:

```bash
# Generate UI for all tables
$ targcc generate ui --all

# Output:
✅ Customer (6 files, 930 lines)
✅ Order (6 files, 892 lines)
✅ Product (6 files, 745 lines)
✅ Category (6 files, 612 lines)
... (20 more tables)

Total: 24 tables, 144 files, ~19,680 lines of code!
Time: 37 seconds

🎉 Done! Your entire UI is ready!
```

---

**תאריך:** 01/12/2025
**מחבר:** Claude
**גרסה:** 1.0
**סטטוס:** ✅ מאושר לביצוע

**Ready to start? Let's build! 🚀**
