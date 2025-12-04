# 🧪 TargCC V2 - הוראות Test מלאות

**תאריך:** 04/12/2025
**מטרה:** לבדוק את TargCC V2 end-to-end
**זמן משוער:** 30-45 דקות

---

## 📋 תוכן עניינים

1. [דרישות מקדימות](#דרישות-מקדימות)
2. [אופציה 1: Test אוטומטי (מומלץ)](#אופציה-1-test-אוטומטי-מומלץ)
3. [אופציה 2: Test ידני צעד-אחר-צעד](#אופציה-2-test-ידני-צעד-אחר-צעד)
4. [אופציה 3: Test מהיר (ללא DB)](#אופציה-3-test-מהיר-ללא-db)
5. [פתרון בעיות](#פתרון-בעיות)

---

## 🔧 דרישות מקדימות

### חובה:
- ✅ **.NET 9 SDK** - [הורד כאן](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ **Visual Studio 2022** או **VS Code**
- ✅ **Git**

### אופציונלי (ל-full test):
- ⚪ **SQL Server 2019+** (או Express)
- ⚪ **sqlcmd** CLI tool

### בדיקת מוכנות:

```bash
# בדוק .NET
dotnet --version
# Expected: 9.0.x

# בדוק SQL Server (אופציונלי)
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

---

## 🚀 אופציה 1: Test אוטומטי (מומלץ!)

### צעד 1: הרץ את הסקריפט

```bash
cd /home/user/TargCC-Core-V2
chmod +x test_targcc_v2.sh
./test_targcc_v2.sh
```

### מה הסקריפט עושה?

1. ✅ בודק prerequisites (dotnet, sqlcmd)
2. ✅ Build את TargCC V2
3. ✅ מריץ unit tests
4. ✅ יוצר test database עם 4 טבלאות
5. ✅ מריץ `targcc generate project`
6. ✅ Build את הפרויקט שנוצר
7. ✅ מציג סיכום
8. ✅ שואל אם לנקות

### תוצאה צפויה:

```
========================================
Step 1: Building TargCC V2
========================================

✓ Build completed successfully!

========================================
Step 2: Running Unit Tests
========================================

✓ Unit tests passed!

========================================
Step 3: Creating Test Database
========================================

✓ Database created: TargCCTest_20251204_143022
✓ Test tables created with sample data

========================================
Step 7: Generating Complete Project
========================================

✓ Project generation completed!

========================================
Test Complete!
========================================
```

### אם הסקריפט עבר בהצלחה:

🎉 **TargCC V2 עובד!**

---

## 📝 אופציה 2: Test ידני צעד-אחר-צעד

אם אתה רוצה שליטה מלאה או שהסקריפט לא עובד.

---

### שלב 1: Build TargCC (5 דק)

```bash
cd /home/user/TargCC-Core-V2

# Restore packages
dotnet restore

# Build
dotnet build --configuration Release
```

**תוצאה צפויה:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**אם יש שגיאות:**
- בדוק שיש .NET 9 SDK
- נסה `dotnet clean` ואז `dotnet build` שוב

---

### שלב 2: Run Tests (10 דק)

```bash
# Run all unit tests
dotnet test --configuration Release

# או רק unit tests
dotnet test --filter "Category=Unit" --configuration Release
```

**תוצאה צפויה:**
```
Passed!  - Failed:     0, Passed:   727, Skipped:     0, Total:   727
```

**אם tests נכשלים:**
- בדוק את ה-error messages
- רוב ה-tests אמורים לעבור
- כמה skipped tests זה בסדר

---

### שלב 3: יצירת Test Database (5 דק)

#### אופציה 3.1: עם sqlcmd

```bash
# Create database
sqlcmd -S localhost -Q "CREATE DATABASE TargCCTest"

# Create tables
sqlcmd -S localhost -d TargCCTest -i /home/user/TargCC-Core-V2/test_database_schema.sql
```

#### אופציה 3.2: עם SQL Server Management Studio (SSMS)

1. פתח SSMS
2. התחבר ל-localhost
3. New Query
4. העתק את התוכן של `test_database_schema.sql`
5. Execute (F5)

**תוצאה צפויה:**
```
Database Created Successfully!

Table Statistics:
  Customer  - 5 rows
  Order     - 5 rows
  Product   - 6 rows
  OrderItem - 9 rows
```

---

### שלב 4: יצירת פרויקט test (2 דק)

```bash
# Create test directory
mkdir -p /tmp/TargCCTest
cd /tmp/TargCCTest
```

---

### שלב 5: אתחול TargCC (1 דק)

```bash
# Path to TargCC CLI
TARGCC=/home/user/TargCC-Core-V2/src/TargCC.CLI/bin/Release/net9.0/TargCC.CLI

# Initialize
$TARGCC init
```

**תוצאה צפויה:**
```
Initializing TargCC

✓ Configuration created successfully
Config file: /tmp/TargCCTest/targcc.json

Would you like to configure database connection now? (Y/n) y
```

הכנס:
```
Connection string: Server=localhost;Database=TargCCTest;Trusted_Connection=true;
Output directory: . (Press Enter)
Default namespace: TestApp
```

---

### שלב 6: ניתוח DB (1 דק)

```bash
$TARGCC analyze schema
```

**תוצאה צפויה:**
```
Analyzing Database Schema

✓ Connected to database: TargCCTest
✓ Found 4 tables:
  - Customer (7 columns, 2 indexes, 0 relationships)
  - Order (5 columns, 3 indexes, 1 relationship)
  - Product (7 columns, 3 indexes, 0 relationships)
  - OrderItem (5 columns, 2 indexes, 2 relationships)

✓ Schema analysis completed!
```

---

### שלב 7: יצירת פרויקט שלם! (2-3 דק)

```bash
$TARGCC generate project --database TargCCTest --output . --namespace TestApp
```

**תוצאה צפויה:**
```
Generating Clean Architecture Project

Step 1: Analyzing database schema...
  ✓ Found 4 tables

Step 2: Creating solution structure...
  ✓ Solution structure created!

Step 3: Generating from 4 tables...
  Processing: Customer
  Processing: Order
  Processing: Product
  Processing: OrderItem
  ✓ Generated 80+ files from 4 tables!

Step 4: Generating support files...
  ✓ Support files generated!

✓ Complete project generated successfully!
  Project: TestApp
  Tables: 4
  Location: /tmp/TargCCTest
```

---

### שלב 8: בדיקת מה נוצר (1 דק)

```bash
# Show solution structure
ls -la

# Should show:
# TestApp.sln
# targcc.json
# src/
#   TestApp.Domain/
#   TestApp.Application/
#   TestApp.Infrastructure/
#   TestApp.API/
# tests/ (if included)

# Count files
find . -name "*.cs" | wc -l
# Should be ~80-100 files
```

---

### שלב 9: Build הפרויקט שנוצר (2 דק)

```bash
# Restore packages
dotnet restore

# Build
dotnet build --configuration Release
```

**תוצאה צפויה:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.23
```

**אם יש compile errors:**
- זה יכול להיות תקין! (אם יש *.prt.cs files עם קוד ישן)
- בדוק איזה שגיאות - אם זה רק ב-*.prt.cs זה בסדר
- הקבצים הנוצרים צריכים להיות ללא שגיאות

---

### שלב 10: הרצת ה-API (1 דק)

```bash
cd src/TestApp.API
dotnet run --configuration Release
```

**תוצאה צפויה:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

### שלב 11: בדיקת API (1 דק)

פתח דפדפן חדש:

```
https://localhost:5001/swagger
```

או מטרמינל אחר:

```bash
# Test Swagger UI
curl https://localhost:5001/swagger/index.html -k

# Test API endpoint
curl https://localhost:5001/api/customers -k

# Should return:
# [{"id":1,"name":"John Doe","email":"john.doe@example.com",...},...]
```

---

## ⚡ אופציה 3: Test מהיר (ללא DB)

אם אין לך SQL Server או רוצה רק לבדוק שה-build עובד:

```bash
cd /home/user/TargCC-Core-V2

# 1. Build
dotnet build --configuration Release

# 2. Run tests
dotnet test --filter "Category=Unit" --no-build

# 3. Check CLI
./src/TargCC.CLI/bin/Release/net9.0/TargCC.CLI --help
```

**אם כל 3 עברו בהצלחה → TargCC V2 עובד!** ✅

---

## 🔧 פתרון בעיות

### בעיה: "dotnet: command not found"

**פתרון:**
```bash
# Install .NET 9 SDK
# Windows: Download from https://dotnet.microsoft.com/download/dotnet/9.0
# Linux: sudo apt install dotnet-sdk-9.0
# Mac: brew install --cask dotnet-sdk
```

---

### בעיה: "Build failed with errors"

**פתרון:**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build --configuration Release -v detailed
```

בדוק את השגיאות הספציפיות ב-output.

---

### בעיה: "Cannot connect to SQL Server"

**פתרון:**
```bash
# Check SQL Server is running
# Windows: services.msc → SQL Server (MSSQLSERVER)
# Linux: sudo systemctl status mssql-server

# Test connection
sqlcmd -S localhost -Q "SELECT @@VERSION"

# If using Windows Authentication, make sure you have permissions
sqlcmd -S localhost -E -Q "SELECT SUSER_NAME()"
```

---

### בעיה: "targcc: command not found"

**פתרון:**
```bash
# Use full path
TARGCC=/home/user/TargCC-Core-V2/src/TargCC.CLI/bin/Release/net9.0/TargCC.CLI
$TARGCC --help

# Or add to PATH
export PATH=$PATH:/home/user/TargCC-Core-V2/src/TargCC.CLI/bin/Release/net9.0
```

---

### בעיה: "Project generation completed but build has errors"

**זה יכול להיות תקין!**

אם השגיאות רק ב-`*.prt.cs` files:
- זה מכוון! (Partial classes למשתמש)
- הקבצים הנוצרים אמורים להיות ללא שגיאות
- מחק את ה-`*.prt.cs` files או תקן אותם

אם השגיאות בקבצים שנוצרו:
- זה באג! דווח עליו
- שלח את ה-error log

---

### בעיה: "Some tests failed"

**זה יכול להיות בסדר!**

- בדוק כמה tests נכשלו
- אם פחות מ-5% → סביר
- אם יותר → יש בעיה

```bash
# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

---

## ✅ Checklist - מה אמור לעבוד?

לאחר Test מלא, סמן ✅:

- [ ] `dotnet build` עובר בהצלחה
- [ ] רוב ה-tests עוברים (>90%)
- [ ] `targcc init` עובד
- [ ] `targcc analyze schema` עובד
- [ ] `targcc generate project` עובד
- [ ] הפרויקט שנוצר מתקמפל
- [ ] ה-API מתניע
- [ ] Swagger UI נטען
- [ ] API מחזיר תשובות

**אם כל הסעיפים ✅ → TargCC V2 עובד 100%!** 🎉

---

## 📊 מה הלאה?

### אם הכל עבד:

1. **נסה DB אמיתי שלך:**
   ```bash
   targcc generate project --database YourDatabase --output ./MyProject
   ```

2. **התאם אישית:**
   - ערוך `targcc.json`
   - הוסף קוד ל-`*.prt.cs` files
   - הוסף business logic

3. **Deploy:**
   ```bash
   dotnet publish -c Release
   ```

### אם משהו לא עבד:

1. **דווח על הבעיה:**
   - GitHub Issues: https://github.com/dorongut1/TargCC-Core-V2/issues
   - שלח error logs
   - תאר מה ניסית

2. **בדוק Documentation:**
   - `docs/current/QUICKSTART.md`
   - `docs/current/CLI-REFERENCE.md`
   - `docs/V2_READINESS_STATUS.md`

3. **שאל שאלות:**
   - GitHub Discussions
   - פתח issue

---

## 📞 עזרה נוספת

- **Documentation:** `/home/user/TargCC-Core-V2/docs/`
- **Examples:** `/home/user/TargCC-Core-V2/examples/` (אם יש)
- **Tests:** `/home/user/TargCC-Core-V2/src/tests/`

---

**בהצלחה!** 🚀

אם משהו לא עובד - זה לא אתה, זה הקוד. דווח ונתקן! 😊

---

**תאריך:** 04/12/2025
**גרסה:** 1.0
**מחבר:** Claude
