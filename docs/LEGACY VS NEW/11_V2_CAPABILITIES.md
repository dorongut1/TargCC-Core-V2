# TargCC V2 - יכולות מלאות
## Complete Capabilities Documentation

**תאריך עדכון:** 2025-12-27
**גרסה:** V2.0

---

## סקירה כללית

TargCC V2 הוא כלי לייצור קוד אוטומטי מתוך מבנה Database. הוא מייצר פרויקט Clean Architecture מלא הכולל:
- Backend ב-.NET 9 (C#)
- Frontend ב-React + TypeScript
- SQL Stored Procedures
- תיעוד אוטומטי

---

## 1. ניתוח Database

### 1.1 טבלאות וColumns
| יכולת | סטטוס | הערות |
|-------|--------|-------|
| זיהוי כל הטבלאות | ✅ | כולל Views |
| ניתוח columns עם types מדויקים | ✅ | SQL → C# → TypeScript mapping |
| זיהוי Primary Keys | ✅ | Single ו-Composite |
| זיהוי Foreign Keys | ✅ | מאכלס `IsForeignKey` ו-`ReferencedTable` |
| זיהוי Indexes | ✅ | Unique ו-Non-Unique |
| Extended Properties | ✅ | `ccUICreateMenu`, `ccUICreateEntity`, etc. |

### 1.2 יחסים (Relationships)
| יכולת | סטטוס | הערות |
|-------|--------|-------|
| One-to-Many | ✅ | Customer → Orders |
| Many-to-One | ✅ | Order → Customer |
| Self-referencing | ✅ | Employee → Manager |

### 1.3 זיהוי סוגי טבלאות
| סוג | Pattern | GenerateUI | GenerateSPs |
|-----|---------|------------|-------------|
| Regular Table | `*` | ✅ | ✅ |
| System Table | `c_*` | ✅ | ✅ |
| Auto ComboList View | `ccvwComboList_*` | ❌ | ❌ |
| Manual ComboList View | `mnccvwComboList_*` | ✅ (Read-only) | ❌ |
| Regular View | `vw*` | ✅ (Read-only) | ❌ |

---

## 2. יצירת Backend (.NET 9)

### 2.1 מבנה Solution
```
{ProjectName}/
├── {ProjectName}.sln
├── src/
│   ├── {ProjectName}.Domain/           # Entities
│   ├── {ProjectName}.Application/      # CQRS, DTOs, Interfaces
│   ├── {ProjectName}.Infrastructure/   # DbContext, Repositories
│   └── {ProjectName}.API/              # Controllers, Program.cs
└── tests/
    └── {ProjectName}.Tests/
```

### 2.2 Domain Layer
| Component | Generated | Details |
|-----------|-----------|---------|
| Entity Classes | ✅ | מ-Table columns |
| Base Entity | ✅ | `BaseEntity<TId>` |

### 2.3 Application Layer
| Component | Generated | Details |
|-----------|-----------|---------|
| IApplicationDbContext | ✅ | Interface עם DbSet לכל Entity |
| DTOs | ✅ | `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto` |
| Commands | ✅ | `Create{Entity}Command`, `Update{Entity}Command`, `Delete{Entity}Command` |
| Queries | ✅ | `Get{Entity}Query`, `GetAll{Entity}Query`, `GetFiltered{Entity}Query` |
| Handlers | ✅ | MediatR handlers לכל Command/Query |
| Validators | ✅ | FluentValidation |
| Repository Interfaces | ✅ | `I{Entity}Repository` |

### 2.4 Infrastructure Layer
| Component | Generated | Details |
|-----------|-----------|---------|
| ApplicationDbContext | ✅ | EF Core DbContext |
| Entity Configurations | ✅ | `IEntityTypeConfiguration<T>` |
| Repository Implementations | ✅ | `{Entity}Repository` |
| DI Registration | ✅ | `DependencyInjection.cs` |

### 2.5 API Layer
| Component | Generated | Details |
|-----------|-----------|---------|
| Controllers | ✅ | REST API לכל Entity |
| Program.cs | ✅ | עם Swagger, CORS, DI |
| appsettings.json | ✅ | Connection string placeholder |

### 2.6 API Endpoints
לכל Entity נוצרים:
```
GET    /api/{entity}           # GetAll with pagination
GET    /api/{entity}/{id}      # GetById
GET    /api/{entity}/filtered  # GetFiltered with parameters
POST   /api/{entity}           # Create
PUT    /api/{entity}/{id}      # Update
DELETE /api/{entity}/{id}      # Delete
```

---

## 3. יצירת SQL Stored Procedures

### 3.1 Standard SPs
| SP Name | Generated | Parameters |
|---------|-----------|------------|
| `SP_GetAll{Entity}s` | ✅ | `@Skip`, `@Take`, `@WithParentText` |
| `SP_GetFiltered{Entity}s` | ✅ | Indexed columns + `@Skip`, `@Take`, `@WithParentText` |
| `SP_Get{Entity}ByID` | ✅ | PK columns |
| `SP_Add{Entity}` | ✅ | All non-PK columns |
| `SP_Update{Entity}` | ✅ | PK + updatable columns |
| `SP_Delete{Entity}` | ✅ | PK columns |

### 3.2 @WithParentText Feature ✅
כאשר `@WithParentText = 1`:
- LEFT JOIN ל-`ccvwComboList_{ParentTable}` לכל FK column
- מחזיר `{FKColumn}_Text` עם שם ההורה

**דוגמה:**
```sql
CREATE OR ALTER PROCEDURE [dbo].[SP_GetAllCards]
    @Skip INT = NULL,
    @Take INT = NULL,
    @WithParentText BIT = 1
AS
BEGIN
    IF @WithParentText = 1
    BEGIN
        SELECT
            t.[ID],
            t.[CustomerID],
            p1.[Text] AS [CustomerID_Text]  -- שם הלקוח
        FROM [Card] t
        LEFT JOIN [ccvwComboList_Customer] p1 ON t.[CustomerID] = p1.[ID]
        ...
    END
    ELSE
    BEGIN
        SELECT * FROM [Card] ...
    END
END
```

### 3.3 Index-Based SPs
| SP Type | Generated When | Example |
|---------|---------------|---------|
| `SP_Get{Entity}By{IndexColumn}` | Unique Index | `SP_GetUserByEmail` |
| `SP_GetFiltered{Entity}By{Column}` | Non-Unique Index | `SP_GetFilteredOrdersByCustomerID` |

---

## 4. יצירת Frontend (React + TypeScript)

### 4.1 Project Structure
```
client/
├── src/
│   ├── components/
│   │   └── {Entity}/
│   │       ├── {Entity}List.tsx      # DataGrid with CRUD
│   │       ├── {Entity}Form.tsx      # Create/Edit form
│   │       └── {Entity}Detail.tsx    # View details
│   ├── hooks/
│   │   └── use{Entity}.ts            # Custom React hooks
│   ├── services/
│   │   └── {entity}Api.ts            # API client functions
│   ├── types/
│   │   └── {entity}.ts               # TypeScript interfaces
│   └── App.tsx
├── package.json
├── vite.config.ts
└── tsconfig.json
```

### 4.2 Generated Components
| Component | Features |
|-----------|----------|
| `{Entity}List.tsx` | MUI DataGrid, Server-side pagination/sorting, CRUD buttons, Export |
| `{Entity}Form.tsx` | React Hook Form, Validation, Edit/Create modes |
| `{Entity}Detail.tsx` | Read-only view of entity |

### 4.3 Technologies Used
- React 18
- TypeScript
- Vite (build tool)
- Material-UI (MUI) v5
- React Query (TanStack Query)
- React Hook Form
- Axios

### 4.4 RTL Support ✅
- Hebrew/RTL direction support
- Theme configuration for RTL

---

## 5. Extended Properties Support

### 5.1 Table-Level Properties
| Property | Values | Effect |
|----------|--------|--------|
| `ccUICreateMenu` | 0/1 | יצירת כניסה בתפריט |
| `ccUICreateEntity` | 0/1 | יצירת טופס עריכה |
| `ccUICreateCollection` | 0/1 | יצירת grid/רשימה |
| `ccAuditLevel` | 0/1/2 | 0=None, 1=Track, 2=Full Audit |

### 5.2 Column-Level Properties (Future)
- Planned: `ccDisplayName`, `ccDisplayOrder`, `ccInputMask`

---

## 6. CLI Commands

### 6.1 Generate Project
```bash
dotnet run -- generate project \
    --database "MyDatabase" \
    --connection-string "Server=localhost;Database=MyDB;..." \
    --output "C:\Output\Path" \
    --namespace "MyCompany.MyProject" \
    --force
```

### 6.2 Available Options
| Option | Description |
|--------|-------------|
| `--database` | Database name |
| `--connection-string` | Full SQL Server connection string |
| `--output` | Output directory |
| `--namespace` | Root namespace for generated code |
| `--force` | Overwrite existing files |
| `--include-tests` | Generate test project (default: true) |

---

## 7. מה עדיין בפיתוח

### 7.1 הושלם לאחרונה ✅
| Feature | Status | Details |
|---------|--------|---------|
| יצירת ccvwComboList Views | ✅ Complete | 73 Views נוצרים אוטומטית |
| @WithParentText | ✅ Complete | 292 LEFT JOINs, 120 IF blocks |
| FK Column Population | ✅ Complete | 146 FK references מאוכלסים |

### 7.2 בקרוב
| Feature | Status | Priority |
|---------|--------|----------|
| Audit Triggers (CLR) | 📋 Planned | Medium |
| System Tables generation | 📋 Planned | Medium |

### 7.2 לעתיד
- File upload support
- Alert system
- Report generation
- Multi-language UI
- Custom field types

---

## 8. Comparison: Legacy vs V2

| Feature | Legacy (VB.NET) | V2 (.NET 9) |
|---------|----------------|-------------|
| Language | VB.NET | C# |
| Architecture | 3-Tier | Clean Architecture |
| Frontend | VB.NET Forms | React + TypeScript |
| API | Custom | REST + Swagger |
| ORM | ADO.NET | Entity Framework Core |
| DI | None | Built-in .NET DI |
| Validation | Manual | FluentValidation |
| Testing | None | xUnit ready |

---

## 9. Files Generated Count (Example)

עבור database עם 99 טבלאות:
- C# Files: ~2,000+
- TypeScript/TSX Files: ~540
- SQL Files: 1 (consolidated)

---

## Contact & Resources

- **Repository:** TargCC-Core-V2
- **Documentation:** `docs/LEGACY VS NEW/`
- **Testing Checklist:** `09_TESTING_CHECKLIST.md`
- **Session Summary:** `10_SESSION_SUMMARY.md`
