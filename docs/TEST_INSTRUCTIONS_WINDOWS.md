# 🪟 TargCC V2 - הוראות Test ל-Windows

**תאריך:** 04/12/2025
**מערכת:** Windows 10/11 + PowerShell
**זמן משוער:** 30-45 דקות

---

## 📋 תוכן עניינים

1. [דרישות מקדימות](#דרישות-מקדימות)
2. [אופציה 1: PowerShell Script (מומלץ!)](#אופציה-1-powershell-script-מומלץ)
3. [אופציה 2: Test ידני צעד-אחר-צעד](#אופציה-2-test-ידני-צעד-אחר-צעד)
4. [אופציה 3: WSL (Linux on Windows)](#אופציה-3-wsl-linux-on-windows)
5. [פתרון בעיות Windows](#פתרון-בעיות-windows)

---

## 🔧 דרישות מקדימות

### חובה:
- ✅ **Windows 10/11**
- ✅ **.NET 9 SDK** - [הורד כאן](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ **PowerShell 5.1+** (מגיע עם Windows)
- ✅ **Git for Windows** - [הורד כאן](https://git-scm.com/download/win)

### אופציונלי:
- ⚪ **SQL Server 2019+** או **SQL Server Express** - [הורד כאן](https://www.microsoft.com/sql-server/sql-server-downloads)
- ⚪ **Visual Studio 2022** או **VS Code**

### בדיקת מוכנות:

```powershell
# בדוק .NET
dotnet --version
# Expected: 9.0.x

# בדוק PowerShell
$PSVersionTable.PSVersion
# Expected: 5.1.x or 7.x

# בדוק SQL Server (אופציונלי)
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

---

## 🚀 אופציה 1: PowerShell Script (מומלץ!)

### צעד 1: פתח PowerShell כ-Administrator

```powershell
# לחץ לחיצה ימנית על PowerShell
# בחר "Run as Administrator"
```

### צעד 2: אפשר הרצת סקריפטים (פעם אחת!)

```powershell
# אפשר הרצת סקריפטים מקומיים
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# אישור: הקלד Y
```

**⚠️ זה בטוח!** זה מאפשר רק סקריפטים שיצרת בעצמך.

### צעד 3: נווט לתיקיית TargCC

```powershell
cd C:\path\to\TargCC-Core-V2

# דוגמה:
cd C:\Users\YourName\source\repos\TargCC-Core-V2
```

### צעד 4: הרץ את הסקריפט

```powershell
.\test_targcc_v2.ps1
```

**זהו!** הסקריפט יריץ הכל אוטומטית.

### פרמטרים אופציונליים:

```powershell
# דלג על tests (מהיר יותר)
.\test_targcc_v2.ps1 -SkipTests

# דלג על יצירת database (אם אין SQL Server)
.\test_targcc_v2.ps1 -SkipDatabase

# שנה SQL Server instance
.\test_targcc_v2.ps1 -SqlServer "localhost\SQLEXPRESS"

# שילוב
.\test_targcc_v2.ps1 -SkipTests -SqlServer "localhost\SQLEXPRESS"
```

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
Step 7: Generating Complete Project
========================================

✓ Project generation completed!

========================================
Test Complete!
========================================
```

---

## 📝 אופציה 2: Test ידני צעד-אחר-צעד

### שלב 1: פתח PowerShell

```powershell
# פתח PowerShell (לא צריך Administrator)
# Windows + X → Windows PowerShell
```

### שלב 2: Build TargCC

```powershell
cd C:\path\to\TargCC-Core-V2

# Restore
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

### שלב 3: Run Tests

```powershell
dotnet test --configuration Release
```

**תוצאה צפויה:**
```
Passed!  - Failed:     0, Passed:   727, Skipped:     0
```

### שלב 4: יצירת Test Database

#### אם יש לך SQL Server:

```powershell
# Create database
sqlcmd -S localhost -Q "CREATE DATABASE TargCCTest"

# Create tables
sqlcmd -S localhost -d TargCCTest -i test_database_schema.sql
```

#### אם אין SQL Server:

דלג לשלב 5 (אפשר לבדוק ללא DB).

### שלב 5: יצירת תיקייה לפרויקט

```powershell
$TestDir = "$env:TEMP\TargCCTest"
New-Item -ItemType Directory -Path $TestDir -Force
Set-Location $TestDir
```

### שלב 6: הגדר Path ל-TargCC CLI

```powershell
$TargCC = "C:\path\to\TargCC-Core-V2\src\TargCC.CLI\bin\Release\net9.0\TargCC.CLI.exe"

# בדוק שקיים
Test-Path $TargCC
# Should return: True
```

### שלב 7: אתחול TargCC

```powershell
& $TargCC init
```

**מלא את הפרטים:**
```
Connection string: Server=localhost;Database=TargCCTest;Trusted_Connection=true;
Output directory: . (Enter)
Default namespace: TestApp
```

### שלב 8: ניתוח DB

```powershell
& $TargCC analyze schema
```

**תוצאה צפויה:**
```
✓ Found 4 tables:
  - Customer
  - Order
  - Product
  - OrderItem
```

### שלב 9: יצירת פרויקט שלם!

```powershell
& $TargCC generate project --database TargCCTest --output . --namespace TestApp
```

**זה לוקח 2-3 דקות...**

**תוצאה צפויה:**
```
Step 1: Analyzing database schema...
  ✓ Found 4 tables

Step 2: Creating solution structure...
  ✓ Solution structure created!

Step 3: Generating from 4 tables...
  ✓ Generated 80+ files!

✓ Complete project generated successfully!
```

### שלב 10: Build הפרויקט שנוצר

```powershell
dotnet restore
dotnet build --configuration Release
```

**תוצאה צפויה:**
```
Build succeeded.
```

### שלב 11: Run API

```powershell
cd src\TestApp.API
dotnet run --configuration Release
```

**פתח בדפדפן:**
```
https://localhost:5001/swagger
```

---

## 🐧 אופציה 3: WSL (Linux on Windows)

אם אתה מעדיף bash script מהלינוקס:

### התקנת WSL:

```powershell
# PowerShell כ-Administrator
wsl --install

# Restart המחשב
```

### הרצה ב-WSL:

```bash
# פתח WSL
wsl

# Navigate to TargCC
cd /mnt/c/path/to/TargCC-Core-V2

# Run bash script
chmod +x test_targcc_v2.sh
./test_targcc_v2.sh
```

---

## 🔧 פתרון בעיות Windows

### ❌ "Running scripts is disabled"

**שגיאה:**
```
test_targcc_v2.ps1 cannot be loaded because running scripts is disabled
```

**פתרון:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

### ❌ "dotnet: command not found"

**פתרון:**
1. הורד .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0
2. התקן (Run as Administrator)
3. פתח PowerShell **חדש** (חשוב!)
4. בדוק: `dotnet --version`

---

### ❌ "sqlcmd is not recognized"

**פתרון 1: התקן SQL Server Tools**
1. הורד: https://aka.ms/ssmsfullsetup
2. או: https://go.microsoft.com/fwlink/?linkid=2230791 (רק command line tools)

**פתרון 2: דלג על DB**
```powershell
.\test_targcc_v2.ps1 -SkipDatabase
```

---

### ❌ "Cannot connect to SQL Server"

**בדוק אם SQL Server רץ:**

```powershell
# בדוק services
Get-Service -Name "*SQL*" | Where-Object {$_.Status -eq "Running"}

# Should show:
# MSSQLSERVER or MSSQL$SQLEXPRESS
```

**אם לא רץ:**

```powershell
# Start SQL Server
Start-Service MSSQLSERVER

# או עם instance name
Start-Service MSSQL$SQLEXPRESS
```

**שנה את instance name:**

```powershell
.\test_targcc_v2.ps1 -SqlServer "localhost\SQLEXPRESS"
```

---

### ❌ "Access Denied" / "Permission Denied"

**פתרון:**
1. פתח PowerShell כ-**Administrator**
2. לחיצה ימנית → "Run as Administrator"

---

### ❌ Build נכשל עם שגיאות

**פתרון 1: Clean & Rebuild**

```powershell
dotnet clean
dotnet restore
dotnet build --configuration Release
```

**פתרון 2: בדוק .NET version**

```powershell
dotnet --version
# צריך להיות 9.0.x
```

אם יש גרסה ישנה:
1. הסר את .NET הישן (Control Panel → Programs)
2. התקן .NET 9 SDK
3. פתח PowerShell חדש

---

### ❌ הסקריפט "נסגר מיד"

**זה קורה אם יש שגיאה בתחילת הסקריפט.**

**פתרון:**

```powershell
# הרץ עם verbose output
.\test_targcc_v2.ps1 -Verbose

# או הצג שגיאות
$ErrorActionPreference = "Stop"
.\test_targcc_v2.ps1
```

---

### ❌ "Path too long"

**Windows יש הגבלה של 260 תווים בנתיב.**

**פתרון:**

```powershell
# העבר את הפרויקט לנתיב קצר יותר
cd C:\T\TargCC

# במקום:
cd C:\Users\VeryLongUserName\Documents\My Projects\TargCC-Core-V2
```

---

## ✅ Checklist - מה צריך לעבוד?

- [ ] `dotnet --version` מחזיר 9.0.x
- [ ] `dotnet build` עובר
- [ ] רוב ה-tests עוברים
- [ ] `.\test_targcc_v2.ps1` רץ בלי שגיאות
- [ ] הפרויקט נוצר ב-`$env:TEMP\TargCCTest`
- [ ] `dotnet build` על הפרויקט שנוצר עובר
- [ ] ה-API מתניע
- [ ] Swagger נטען

**אם כל הסעיפים ✅ → TargCC V2 עובד!** 🎉

---

## 📊 סיכום פקודות

### Test מהיר (5 דק):

```powershell
cd C:\path\to\TargCC-Core-V2
dotnet build --configuration Release
dotnet test --filter "Category=Unit"
```

### Test מלא עם סקריפט (30 דק):

```powershell
cd C:\path\to\TargCC-Core-V2
.\test_targcc_v2.ps1
```

### Test מלא ידני (45 דק):

עקוב אחרי "אופציה 2" למעלה.

---

## 💡 טיפים ל-Windows

### 1. השתמש ב-Windows Terminal (מומלץ!)

**הורד:** Microsoft Store → "Windows Terminal"

**יתרונות:**
- צבעים
- Tabs
- מהיר יותר

### 2. Path קצרים

```powershell
# צור נתיב קצר
New-Item -ItemType Junction -Path "C:\T" -Target "C:\long\path\to\TargCC-Core-V2"

# עכשיו:
cd C:\T
```

### 3. שמור aliases

```powershell
# הוסף ל-PowerShell profile
notepad $PROFILE

# הוסף:
$env:TARGCC = "C:\path\to\TargCC-Core-V2\src\TargCC.CLI\bin\Release\net9.0\TargCC.CLI.exe"
function targcc { & $env:TARGCC $args }

# עכשיו אפשר להשתמש:
targcc --help
```

---

## 🎯 מה הלאה?

אם הכל עבד:
1. נסה DB אמיתי שלך
2. התאם ל-needs שלך
3. Deploy

אם משהו לא עבד:
1. קרא "פתרון בעיות" למעלה
2. דווח ב-GitHub Issues
3. שלח screenshots של השגיאות

---

## 📞 עזרה נוספת

**Windows-specific issues:**
- בדוק Windows Event Viewer
- בדוק Windows Firewall
- בדוק Antivirus (לפעמים חוסם)

**כללי:**
- `docs\TEST_INSTRUCTIONS.md` - הוראות Linux
- `docs\V2_READINESS_STATUS.md` - מצב מוכנות
- GitHub Issues

---

**בהצלחה!** 🚀

**הסקריפט PowerShell אמור לעבוד מצוין ב-Windows!**

---

**תאריך:** 04/12/2025
**גרסה:** 1.0 (Windows Edition)
**מחבר:** Claude
