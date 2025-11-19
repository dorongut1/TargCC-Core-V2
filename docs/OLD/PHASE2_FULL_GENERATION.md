# Phase 2: Full Code Generation - תכנית מפורטת 🏗️

**תאריך:** 15/11/2025  
**זמן משוער:** 4-5 שבועות (20-25 ימי עבודה)  
**מטרה:** יצירת 8 פרויקטים מלאים מ-DB Schema!

---

## 🎯 מה זה Phase 2?

**Phase 2 = Full Code Generation**

```
Phase 1.5 (MVP) ✅
    ↓
    SQL + Entity מוכנים
    ↓
Phase 2 (Full Generation) ← כאן!
    ↓
    8 פרויקטים מלאים
    ↓
Phase 3 (Advanced)
```

### מה נוצר?
1. **DBController** - Business Logic layer (VB.NET)
2. **DBStdController** - .NET Standard wrapper
3. **TaskManager** - Background jobs (Console)
4. **WS** (Web Service) - ASMX endpoints
5. **WSController** - Client-side logic (VB.NET)
6. **WSStdController** - .NET Standard wrapper
7. **WinF** - Windows Forms UI
8. **Dependencies** - Shared assemblies

### מבנה פרויקט טיפוסי:
```
SolutionName/
├── DBController/          (VB.NET Class Library)
│   ├── CC/               (קוד אוטומטי)
│   │   ├── ccCustomer.vb
│   │   ├── ccCustomerCollection.vb
│   │   └── ...
│   ├── PartialFiles/     (קוד ידני - מוגן!)
│   │   ├── ccCustomer.prt.vb
│   │   └── ...
│   ├── TargCCConfig.config
│   └── TargCCController.vb
│
├── WS/                   (Web Service)
│   ├── CC/
│   │   └── CC.asmx
│   ├── ccAPI.aspx
│   └── Web.config
│
├── WSController/         (VB.NET Class Library)
│   ├── CC/
│   └── PartialFiles/
│
├── WinF/                 (Windows Forms)
│   ├── CC/
│   │   ├── ctlCustomer.vb
│   │   ├── frmCustomer.vb
│   │   └── ...
│   └── PartialFiles/
│
└── ... (4 additional projects)
```

---

## 📅 תכנית עבודה מפורטת

### 🗓️ Week 1-2: DBController (10 ימים)

#### Day 1-2: Controller Generator Setup (2 ימים)

**מבנה:**
```csharp
src/TargCC.Core.Generators/Controllers/
├── IControllerGenerator.cs
├── DBControllerGenerator.cs
├── EntityClassTemplate.vb.cs
├── CollectionClassTemplate.vb.cs
└── MethodGenerators/
    ├── GetByMethodGenerator.cs
    ├── UpdateMethodGenerator.cs
    ├── DeleteMethodGenerator.cs
    └── FillByMethodGenerator.cs
```

**יכולות בסיסיות:**
```vb
' ccCustomer.vb (generated)
Public Class ccCustomer
    ' Properties (from Column)
    Public Property ID As Integer
    Public Property Name As String
    Public Property Email As String
    
    ' GetByID
    Public Shared Function GetByID(
        pID As Integer, pRequester As clsRequester
    ) As clsFault
        ' Implementation
    End Function
    
    ' Update
    Public Function Update(pRequester As clsRequester) As clsFault
        ' Implementation
    End Function
    
    ' Delete
    Public Function Delete(pRequester As clsRequester) As clsFault
        ' Implementation
    End Function
End Class
```

**מה נעשה:**
- [ ] IControllerGenerator interface
- [ ] DBControllerGenerator class
- [ ] EntityClassTemplate
- [ ] Basic structure generation
- [ ] 5 tests

**זמן:** 2 ימים

---

#### Day 3-4: Method Generators - CRUD (2 ימים)

**GetByID Method:**
```vb
Public Shared Function GetByID(
    pID As Integer, 
    pRequester As clsRequester
) As clsFault
    Dim pFault As New clsFault("GetByID")
    
    Try
        ' Permission check
        If Not ccSecurity.HasPermission("tbl_Customer", pRequester) Then
            Return pFault.Finish("No permission")
        End If
        
        ' Call SP
        Dim pSP As String = "SP_GetCustomerByID"
        Dim pParams As New Dictionary(Of String, Object) From {
            {"CustomerID", pID}
        }
        
        Dim dt As DataTable = ccSQL.GetDataTable(pSP, pParams)
        
        If dt.Rows.Count = 0 Then
            Return pFault.Finish("Record not found")
        End If
        
        ' Fill object
        Me.FillFromDataRow(dt.Rows(0))
        
        Return pFault.Finish()
    Catch ex As Exception
        Return pFault.Finish(ex)
    End Try
End Function
```

**Update Method:**
```vb
Public Function Update(pRequester As clsRequester) As clsFault
    Dim pFault As New clsFault("Update")
    
    Try
        ' Validation
        If String.IsNullOrEmpty(Me.Name) Then
            Return pFault.Finish("Name required")
        End If
        
        ' Hash eno_ fields
        If Not String.IsNullOrEmpty(Me.Password) Then
            If Me.Password.StartsWith("[PleaseHash]") Then
                Me.PasswordHashed = ccSecurity.Hash(Me.Password)
            End If
        End If
        
        ' Encrypt ent_ fields
        ' ... similar logic
        
        ' Call SP
        Dim pSP As String = "SP_UpdateCustomer"
        Dim pParams As New Dictionary(Of String, Object) From {
            {"CustomerID", Me.ID},
            {"Name", Me.Name},
            {"Email", Me.Email}
            ' ... all updateable fields
        }
        
        ccSQL.ExecuteNonQuery(pSP, pParams)
        
        Return pFault.Finish()
    Catch ex As Exception
        Return pFault.Finish(ex)
    End Try
End Function
```

**Delete Method:**
```vb
Public Function Delete(pRequester As clsRequester) As clsFault
    ' Similar structure to Update
End Function
```

**מה נעשה:**
- [ ] GetByMethodGenerator
- [ ] UpdateMethodGenerator
- [ ] DeleteMethodGenerator
- [ ] Parameter mapping
- [ ] Error handling
- [ ] 15+ tests

**זמן:** 2 ימים

---

#### Day 5-6: Collection Class (2 ימים)

**ccCustomerCollection.vb:**
```vb
Public Class ccCustomerCollection
    Inherits List(Of ccCustomer)
    
    ' FillByXXX (from non-unique indexes)
    Public Function FillByStatus(
        pStatus As String,
        pRequester As clsRequester
    ) As clsFault
        ' Call SP_GetCustomersByStatus
        ' Fill collection
    End Function
    
    ' CloneByXXX
    Public Function CloneByStatus(
        pStatus As String
    ) As ccCustomerCollection
        ' Return subset
    End Function
    
    ' FindByXXX (from unique indexes)
    Public Function FindByEmail(pEmail As String) As ccCustomer
        ' Return single item
    End Function
    
    ' ToCSV
    Public Function ToCSV() As String
        ' Export to CSV
    End Function
End Class
```

**מה נעשה:**
- [ ] CollectionClassTemplate
- [ ] FillBy methods (from indexes)
- [ ] CloneBy methods
- [ ] FindBy methods
- [ ] Helper methods (ToCSV, etc.)
- [ ] 10+ tests

**זמן:** 2 ימים

---

#### Day 7-8: Relationship Methods (2 ימים)

**LoadDependants:**
```vb
' In ccCustomer.vb
Public Function LoadDependants(
    pRequester As clsRequester
) As clsFault
    ' Load all Orders for this Customer
    Me.Orders = New ccOrderCollection()
    Return Me.Orders.FillByCustomerID(Me.ID, pRequester)
End Function
```

**LoadOneToOneChildren:**
```vb
' If CustomerDetail exists
Public Function LoadOneToOneChildren(
    pRequester As clsRequester
) As clsFault
    ' Load CustomerDetail
    Me.Detail = New ccCustomerDetail()
    Return Me.Detail.GetByCustomerID(Me.ID, pRequester)
End Function
```

**מה נעשה:**
- [ ] Relationship detection (from Analyzers)
- [ ] LoadDependants method
- [ ] LoadOneToOneChildren method
- [ ] FillXXX methods (parent → children)
- [ ] 10+ tests

**זמן:** 2 ימים

---

#### Day 9-10: Prefix Special Handling (2 ימים)

**UpdateSeparate methods (spt_):**
```vb
' For column: spt_Comments
Public Function UpdateComments(
    pComments As String,
    pRequester As clsRequester
) As clsFault
    ' Call SP_UpdateCustomerComments
End Function
```

**UpdateAggregates (agg_):**
```vb
' For column: agg_OrderCount
Public Function UpdateAggregates(
    pOrderCountDelta As Integer,
    pRequester As clsRequester
) As clsFault
    ' Call SP_UpdateCustomerAggregates
End Function
```

**UpdateFriend (blg_):**
```vb
' Business Logic fields
Public Function UpdateFriend(
    pRequester As clsRequester
) As clsFault
    ' Update ALL fields including blg_
End Function
```

**מה נעשה:**
- [ ] UpdateSeparate generator
- [ ] UpdateAggregates generator
- [ ] UpdateFriend generator
- [ ] Prefix-specific logic
- [ ] 15+ tests

**זמן:** 2 ימים

---

### ✅ Checkpoint Week 1-2:
- ✅ DBController structure מוכן
- ✅ Entity + Collection classes
- ✅ CRUD methods
- ✅ Relationship methods
- ✅ Prefix handling
- ✅ 50+ tests
- ✅ Code Quality: Grade A

---

### 🗓️ Week 3: Web Service & WSController (5 ימים)

#### Day 1-2: Web Service Generator (2 ימים)

**CC.asmx:**
```vb
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
Public Class CC
    Inherits System.Web.Services.WebService
    
    <WebMethod()>
    Public Function GetCustomerByID(
        pID As Integer,
        pRequester As clsRequester
    ) As clsFault
        Dim pCustomer As New ccCustomer()
        Return pCustomer.GetByID(pID, pRequester)
    End Function
    
    <WebMethod()>
    Public Function UpdateCustomer(
        pCustomer As ccCustomer,
        pRequester As clsRequester
    ) As clsFault
        Return pCustomer.Update(pRequester)
    End Function
    
    ' ... all methods
End Class
```

**מה נעשה:**
- [ ] WebServiceGenerator
- [ ] ASMX template
- [ ] Method exposure
- [ ] Web.config generation
- [ ] 10+ tests

**זמן:** 2 ימים

---

#### Day 3-4: WSController Generator (2 ימים)

**WSController structure:**
```
WSController/
├── CC/
│   ├── ccCustomer.vb        (same API as DBController)
│   └── ccCustomerCollection.vb
└── PartialFiles/
    └── ccCustomer.prt.vb
```

**ccCustomer.vb (WSController):**
```vb
Public Class ccCustomer
    ' Same properties as DBController
    
    ' GetByID - calls Web Service
    Public Shared Function GetByID(
        pID As Integer,
        pRequester As clsRequester
    ) As clsFault
        ' Call WS
        Dim ws As New CC()
        Return ws.GetCustomerByID(pID, pRequester)
    End Function
    
    ' Same API, different implementation!
End Class
```

**מה נעשה:**
- [ ] WSControllerGenerator
- [ ] Same API as DBController
- [ ] Web Service calls
- [ ] Serialization handling
- [ ] 10+ tests

**זמן:** 2 ימים

---

#### Day 5: Support Projects - Std Controllers (1 יום)

**DBStdController & WSStdController:**
```
- Same code as VB.NET versions
- Compiled for .NET Standard
- Enables use in .NET Core projects
```

**מה נעשה:**
- [ ] Copy generation logic
- [ ] .NET Standard templates
- [ ] 5 tests

**זמן:** 1 יום

---

### 🗓️ Week 4: UI & TaskManager (5 ימים)

#### Day 1-3: WinForms Generator (3 ימים)

**Control structure:**
```
WinF/
├── CC/
│   ├── ctlCustomer.vb           (entity control)
│   ├── ctlCustomer.Designer.vb  (UI designer - not replaced!)
│   ├── ctlCustomerCollection.vb (grid control)
│   ├── frmCustomer.vb          (form)
│   └── pnlCustomer.vb          (panel with links)
└── PartialFiles/
    └── ctlCustomer.prt.vb
```

**ctlCustomer.vb:**
```vb
Public Class ctlCustomer
    ' TextBoxes for each property
    Friend WithEvents txtName As TextBox
    Friend WithEvents txtEmail As TextBox
    
    ' ComboBoxes for FKs
    Friend WithEvents cboStatus As ComboBox
    
    ' Load entity
    Public Sub LoadEntity(pCustomer As ccCustomer)
        txtName.Text = pCustomer.Name
        txtEmail.Text = pCustomer.Email
        ' ...
    End Sub
    
    ' Save entity
    Public Function SaveEntity(
        ByRef pCustomer As ccCustomer
    ) As Boolean
        pCustomer.Name = txtName.Text
        pCustomer.Email = txtEmail.Text
        ' ...
    End Function
End Class
```

**ctlCustomerCollection.vb:**
```vb
Public Class ctlCustomerCollection
    ' DataGridView
    Friend WithEvents dgvCustomers As DataGridView
    
    ' Load collection
    Public Sub LoadCollection(pColl As ccCustomerCollection)
        dgvCustomers.DataSource = pColl
    End Sub
    
    ' Double-click event
    Private Sub dgvCustomers_DoubleClick() Handles dgvCustomers.DoubleClick
        ' Open entity form
    End Sub
End Class
```

**מה נעשה:**
- [ ] WinFormsGenerator
- [ ] Entity control template
- [ ] Collection control template
- [ ] Designer file protection (not replaced!)
- [ ] Lookup ComboBox auto-fill
- [ ] 15+ tests

**זמן:** 3 ימים

---

#### Day 4: TaskManager Generator (1 יום)

**Module1.vb:**
```vb
Module Module1
    Sub Main(args As String())
        ' Run scheduled tasks
        Dim taskRunner As New TaskRunner()
        taskRunner.Run()
    End Sub
End Module

Public Class TaskRunner
    Public Sub Run()
        ' Task: Clean old logs
        CleanLogs()
        
        ' Task: Process audit
        ProcessAudit()
        
        ' Task: Send reports
        SendReports()
    End Sub
End Class
```

**מה נעשה:**
- [ ] TaskManagerGenerator
- [ ] Basic structure
- [ ] Sample tasks
- [ ] App.config
- [ ] 5 tests

**זמן:** 1 יום

---

#### Day 5: Dependencies Project (1 יום)

**\_Dependencies:**
```
_Dependencies/
├── TargetSMS.dll
├── OtherSharedDlls.dll
└── README.txt
```

**מה נעשה:**
- [ ] Create project structure
- [ ] Copy required DLLs
- [ ] README instructions
- [ ] 2 tests

**זמן:** 1 יום

---

### 🗓️ Week 5: Integration & Polish (5 ימים)

#### Day 1-3: Integration Tests (3 ימים)

**Full Stack Test:**
```csharp
[Fact]
public async Task FullStack_CRUDOperations_Success()
{
    // 1. Generate from DB
    var schema = await analyzer.AnalyzeAsync(connString);
    
    // 2. Generate all projects
    await generator.GenerateDBController(schema);
    await generator.GenerateWSController(schema);
    await generator.GenerateWebService(schema);
    await generator.GenerateWinForms(schema);
    // ... all 8 projects
    
    // 3. Build solution
    var buildResult = await builder.BuildSolutionAsync();
    Assert.True(buildResult.Success);
    
    // 4. Test WinF → WSController → WS → DBController → DB
    // Create Customer
    var customer = new Customer { Name = "Test", Email = "test@test.com" };
    var fault = customer.Update(requester);
    Assert.True(fault.isOK);
    
    // Read Customer
    var customer2 = new Customer();
    fault = customer2.GetByID(customer.ID, requester);
    Assert.True(fault.isOK);
    Assert.Equal("Test", customer2.Name);
    
    // Update Customer
    customer2.Name = "Updated";
    fault = customer2.Update(requester);
    Assert.True(fault.isOK);
    
    // Delete Customer
    fault = customer2.Delete(requester);
    Assert.True(fault.isOK);
}
```

**מה נעשה:**
- [ ] End-to-End tests
- [ ] Build verification
- [ ] CRUD operations
- [ ] Performance benchmarks
- [ ] 10+ scenarios

**זמן:** 3 ימים

---

#### Day 4-5: Documentation & Samples (2 ימים)

**FULL_GENERATION.md:**
```markdown
# Full Code Generation Guide

## Overview
From Database Schema → 8 Complete Projects

## Quick Start
1. Prepare your database
2. Run analyzer
3. Generate solution
4. Build & run

## Project Structure
- DBController: Business Logic
- WSController: Client Logic
- WS: Web Service
- WinF: Windows Forms
- ... (all 8)

## Customization
- *.prt files: Your manual code
- Events: Hook into generation
- Templates: Customize output
```

**Sample Project:**
```
Examples/TargCCOrders/
├── Database/
│   └── Orders.sql
├── Generated/
│   ├── DBController/
│   ├── WSController/
│   └── ... (all 8)
└── README.md
```

**מה נעשה:**
- [ ] FULL_GENERATION.md
- [ ] Sample project
- [ ] Video tutorial (optional)
- [ ] Migration guide

**זמן:** 2 ימים

---

## ✅ Phase 2 Deliverables

| רכיב | פלט | זמן | Tests |
|------|-----|-----|-------|
| **DBController** | VB.NET Class Library | 10 days | 50+ |
| **WSController** | VB.NET Class Library | 2 days | 10+ |
| **Web Service** | ASMX | 2 days | 10+ |
| **WinForms** | Windows Forms | 3 days | 15+ |
| **Support** | 4 projects | 3 days | 10+ |
| **Integration** | Tests + Docs | 5 days | 10+ |
| **סה"כ** | **8 projects** | **25 days** | **105+** |

---

## 🎯 Success Criteria

### Functional:
- ✅ יוצר 8 פרויקטים מלאים
- ✅ Build בלי שגיאות
- ✅ CRUD operations עובדות
- ✅ Relationships עובדות
- ✅ Prefixes מטופלים נכון
- ✅ *.prt files מוגנים
- ✅ End-to-End test עובר

### Quality:
- ✅ Code Coverage: 75%+
- ✅ SonarQube Grade: A
- ✅ All tests passing
- ✅ Documentation complete

### Performance:
- ✅ Small DB (<20 tables): < 2 min
- ✅ Medium DB (50 tables): < 5 min
- ✅ Large DB (200 tables): < 15 min

---

## 💡 Next Steps

**After Phase 2:**
→ **Phase 3: Advanced Features**

What we'll add:
- Modern UI (React Web)
- Visual Schema Designer
- Smart Error Guide
- AI Assistant
- Version Control
- Performance optimization

**Time:** 6-8 weeks  
**Goal:** Enterprise-ready system!

---

**Created:** 15/11/2025  
**Status:** Planning  
**Next:** Phase 1.5 first! 🚀
