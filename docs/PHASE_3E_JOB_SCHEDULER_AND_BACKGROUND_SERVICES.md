# Phase 3E: Job Scheduler & Background Services

**Document Version:** 1.0
**Date:** 10/12/2025
**Status:** 📋 Specification Ready
**Priority:** 🔥 High - Enterprise Feature

---

## 📖 Table of Contents

1. [Executive Summary](#executive-summary)
2. [Background & Context](#background--context)
3. [Problem Statement](#problem-statement)
4. [Solution Overview](#solution-overview)
5. [Architecture](#architecture)
6. [Technical Specifications](#technical-specifications)
7. [Implementation Plan](#implementation-plan)
8. [Testing Strategy](#testing-strategy)
9. [Rollout Plan](#rollout-plan)

---

## 📋 Executive Summary

### What Is This?

Phase 3E adds **modern job scheduling** and **background services** to TargCC Core V2, replacing the legacy c_Job + TaskManager system with a convention-based, auto-discovery approach powered by Hangfire.

### Key Objectives

1. ✅ **Convention-Based Jobs** - Write a class, TargCC discovers it automatically
2. ✅ **Hangfire Integration** - Beautiful dashboard, CRON expressions, retries
3. ✅ **Auto-Discovery** - Zero configuration, attribute-based
4. ✅ **Legacy Migration** - Import c_Job definitions from old TARGCC
5. ✅ **TargCC Generation** - Generate job templates, registration code
6. ✅ **Enterprise Ready** - Distributed execution, monitoring, alerts

### Business Value

| Metric | Legacy (c_Job) | Modern (Phase 3E) |
|--------|----------------|-------------------|
| **Configuration** | Manual INSERT INTO c_Job | `[TargCCJob]` attribute |
| **Execution Logic** | Stored Procedures only | C# code + DI |
| **Dashboard** | ❌ None | ✅ Hangfire UI |
| **Monitoring** | Manual queries | Built-in dashboard |
| **Retries** | Manual | Automatic |
| **Distributed** | Single server | Multi-server |
| **Developer Experience** | SQL scripts | C# classes |

---

## 🏛️ Background & Context

### Legacy TARGCC (c_Job + TaskManager)

**What It Was:**
- **c_Job** - Table storing job definitions
- **c_JobAlertRecipient** - Email alerts on failure
- **c_LoggedJob** - Execution history
- **TaskManager.exe** - Console app polling c_Job every minute

```sql
-- Legacy c_Job structure
CREATE TABLE c_Job (
    ID INT IDENTITY(1,1),
    JobName VARCHAR(100),
    CronExpression VARCHAR(50),        -- "0 2 * * *"
    StoredProcName VARCHAR(100),       -- SP to execute
    IsActive BIT,
    LastRun DATETIME,
    NextRun DATETIME,
    LastResult NVARCHAR(MAX),
    FailureCount INT,
    EmailOnFailure BIT
)

-- Example job
INSERT INTO c_Job (JobName, CronExpression, StoredProcName, IsActive)
VALUES ('Daily Sales Report', '0 2 * * *', 'SP_GenerateSalesReport', 1)
```

**How TaskManager Worked:**

```vb
' TaskManager.exe (VB.NET Console)
While True
    ' Get due jobs
    Dim jobs = GetDueJobs()

    For Each job In jobs
        Try
            ' Execute stored procedure
            ExecuteSP(job.StoredProcName)

            ' Update success
            UpdateJobStatus(job.ID, success:=True)
        Catch ex As Exception
            ' Log failure
            UpdateJobStatus(job.ID, success:=False, error:=ex.Message)

            ' Send alert email
            If job.EmailOnFailure Then
                SendAlertEmail(job, ex)
            End If
        End Try
    Next

    ' Wait 1 minute
    Thread.Sleep(60000)
End While
```

**What Worked Well:**
- ✅ Simple concept (table + console app)
- ✅ CRON expressions for scheduling
- ✅ Email alerts on failure
- ✅ Execution history (c_LoggedJob)
- ✅ Easy to see what's running (query c_Job)

**The Problems:**
- 🔴 **Stored Procedures only** - no C# business logic
- 🔴 **No dashboard** - manual SQL queries to check status
- 🔴 **No retry logic** - fails once, done
- 🔴 **Single server** - doesn't scale
- 🔴 **Manual configuration** - INSERT INTO c_Job...
- 🔴 **Poor monitoring** - hard to debug
- 🔴 **Polling overhead** - checks every minute even if no jobs
- 🔴 **Tight coupling** - TaskManager.exe separate from main app

**Common Use Cases:**
1. **Scheduled Reports** - Daily sales report at 2 AM
2. **Data Cleanup** - Delete old logs weekly
3. **Batch Processing** - Process invoices at end of day
4. **Integration Sync** - Sync with external system hourly
5. **System Maintenance** - Backup database nightly
6. **Alerts** - Check for critical conditions and email

---

## 🎯 Problem Statement

### Primary Problem

**"We need modern job scheduling that integrates with C# code, has a UI, and doesn't require manual SQL configuration."**

### User Stories

#### Story 1: Developer Creating a Scheduled Job

```
As a developer,
When I need to create a daily report job,
I want to write a C# class with business logic,
So that I can use DI, async/await, and modern patterns.

Legacy way:
1. Write SP_GenerateSalesReport stored procedure
2. INSERT INTO c_Job (JobName, CronExpression, StoredProcName)
3. Deploy TaskManager.exe
4. Wait and hope it works

Modern way:
1. Write DailyReportJob.cs class with [TargCCJob] attribute
2. Build project
3. TargCC auto-discovers and registers it
4. View in Hangfire dashboard at /hangfire
```

#### Story 2: Operations Team Monitoring Jobs

```
As an operations engineer,
When I need to check if jobs are running,
I want a visual dashboard showing status, history, and failures,
So that I don't have to write SQL queries.

Legacy way:
SELECT * FROM c_Job WHERE LastRun > DATEADD(hour, -1, GETDATE())
SELECT * FROM c_LoggedJob WHERE Success = 0 ORDER BY RunDate DESC

Modern way:
Open http://localhost:5000/hangfire
See all jobs, success/failure, execution times, logs
```

#### Story 3: Business User Triggering Manual Job

```
As a business user,
When month-end closes early,
I want to trigger the monthly report manually,
So that I don't wait for the scheduled time.

Legacy way:
Call IT → IT runs: EXEC SP_GenerateMonthlyReport

Modern way:
Open dashboard → Click "Trigger Now" → Done
```

#### Story 4: Developer Debugging Failed Job

```
As a developer,
When a job fails,
I want to see the full exception stack trace and retry it,
So that I can debug and fix quickly.

Legacy way:
SELECT LastResult FROM c_Job WHERE JobName = 'X'
-- See truncated error message
-- Manually re-run SP in SSMS

Modern way:
Open dashboard → Click failed job → See full stack trace
Click "Retry" → Fixed
```

---

## 💡 Solution Overview

### Architecture Decision: Hangfire + Convention-Based Discovery

**Why Hangfire?**

| Feature | Hangfire | Quartz.NET | BackgroundService | Azure Functions |
|---------|----------|------------|-------------------|-----------------|
| **Dashboard** | ✅ Beautiful | ❌ None | ❌ None | ✅ Azure Portal |
| **CRON Support** | ✅ Yes | ✅ Yes | ❌ Manual | ✅ Yes |
| **Distributed** | ✅ Multi-server | ✅ Yes | ❌ Single | ✅ Cloud |
| **Retries** | ✅ Auto | ⚠️ Manual | ❌ Manual | ✅ Auto |
| **SQL Storage** | ✅ Yes | ✅ Yes | ❌ In-memory | ❌ Cloud |
| **.NET Integration** | ✅ Native | ✅ Native | ✅ Built-in | ⚠️ Serverless |
| **On-Premise** | ✅ Yes | ✅ Yes | ✅ Yes | ❌ Cloud only |
| **NuGet Package** | ✅ Yes | ✅ Yes | ✅ Built-in | ❌ Cloud SDK |
| **License** | Free (Pro: $749) | Free (OSS) | Free | Pay-per-use |

**Decision: Hangfire (Community Edition)**
- ✅ Best dashboard in the industry
- ✅ Zero configuration for basic use
- ✅ Perfect for on-premise + cloud
- ✅ Free for single-server (upgrade to Pro for multi-server)

---

### Convention-Based Job Discovery

**Core Concept:** Write a class, TargCC discovers it automatically.

#### Example 1: Daily Scheduled Job

```csharp
using TargCC.Application.Jobs;

[TargCCJob("0 2 * * *")]  // 2 AM daily
[JobDisplayName("Daily Sales Report")]
[JobDescription("Generates daily sales report and emails it to managers")]
public class DailySalesReportJob : ITargCCJob
{
    private readonly IOrderRepository _orderRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<DailySalesReportJob> _logger;

    // Dependency Injection supported!
    public DailySalesReportJob(
        IOrderRepository orderRepo,
        IEmailService emailService,
        ILogger<DailySalesReportJob> logger)
    {
        _orderRepo = orderRepo;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting daily sales report generation");

        try
        {
            // Get yesterday's orders
            var yesterday = DateTime.Today.AddDays(-1);
            var orders = await _orderRepo.GetOrdersByDateAsync(yesterday, cancellationToken);

            // Generate report
            var report = new SalesReport
            {
                Date = yesterday,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.Total)
            };

            // Send email
            await _emailService.SendReportAsync("managers@company.com", report, cancellationToken);

            _logger.LogInformation("Daily sales report sent successfully. Orders: {Count}, Revenue: {Revenue}",
                report.TotalOrders, report.TotalRevenue);

            return JobResult.Success($"Processed {orders.Count} orders, ${report.TotalRevenue:N2} revenue");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily sales report");
            return JobResult.Failure(ex.Message);
        }
    }
}
```

#### Example 2: Fire-and-Forget Job

```csharp
[TargCCJob(JobType.FireAndForget)]
[JobDisplayName("Send Welcome Email")]
public class SendWelcomeEmailJob : ITargCCJob
{
    private readonly IEmailService _emailService;

    public SendWelcomeEmailJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        // This job is not scheduled, it's triggered manually
        // Example: After user registration
        // BackgroundJob.Enqueue<SendWelcomeEmailJob>(job => job.ExecuteAsync(CancellationToken.None));

        await _emailService.SendAsync("user@example.com", "Welcome!", "...");

        return JobResult.Success();
    }
}
```

#### Example 3: Manual Trigger Only

```csharp
[TargCCJob(JobType.Manual)]
[JobDisplayName("Generate Month-End Report")]
[JobDescription("Manual trigger only - generates comprehensive month-end financial report")]
public class MonthEndReportJob : ITargCCJob
{
    private readonly IReportService _reportService;

    public MonthEndReportJob(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        // Long-running job
        var report = await _reportService.GenerateMonthEndReportAsync(cancellationToken);

        return JobResult.Success($"Generated report with {report.TotalTransactions} transactions");
    }
}
```

---

### TargCC Auto-Generation

**What TargCC Generates:**

```
Application/
├── Jobs/
│   ├── ITargCCJob.cs                 (GENERATED - interface)
│   ├── JobResult.cs                  (GENERATED - result type)
│   ├── TargCCJobAttribute.cs         (GENERATED - attribute)
│   ├── JobTypeEnum.cs                (GENERATED - enum)
│   ├── DailySalesReportJob.cs        (TEMPLATE - developer customizes)
│   └── SendWelcomeEmailJob.cs        (TEMPLATE - developer customizes)
│
Infrastructure/
├── Jobs/
│   ├── HangfireJobDiscoveryService.cs  (GENERATED - auto-discovery)
│   ├── HangfireConfiguration.cs        (GENERATED - Hangfire setup)
│   ├── JobExecutor.cs                  (GENERATED - wrapper)
│   └── JobLogger.cs                    (GENERATED - logging)
│
API/
├── Program.cs                          (UPDATED - adds Hangfire)
├── appsettings.json                    (UPDATED - Hangfire config)
└── Controllers/
    └── JobsController.cs               (GENERATED - manual triggers)
```

**Program.cs (Auto-Generated Setup):**

```csharp
// Auto-generated by TargCC
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// === TargCC Generated: Hangfire Setup ===
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer();

// Auto-discover and register jobs
builder.Services.AddSingleton<IJobDiscoveryService, HangfireJobDiscoveryService>();
// === End TargCC Generated ===

var app = builder.Build();

// === TargCC Generated: Hangfire Dashboard ===
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Auto-register discovered jobs
var jobDiscovery = app.Services.GetRequiredService<IJobDiscoveryService>();
jobDiscovery.RegisterAllJobs();
// === End TargCC Generated ===

app.Run();
```

---

### Three Operation Modes

#### Mode 1: Pure Auto-Discovery (Recommended)

```csharp
// Developer writes jobs
[TargCCJob("0 2 * * *")]
public class DailySalesReportJob : ITargCCJob { ... }

// TargCC discovers on startup
// Zero configuration needed!
```

**Use case:** Greenfield projects, modern approach

---

#### Mode 2: Hybrid (Legacy + New)

```csharp
// Option A: Keep c_Job table, import definitions
public class LegacyJobImporter
{
    public void ImportFromDatabase()
    {
        var legacyJobs = _db.Query<c_Job>("SELECT * FROM c_Job WHERE IsActive = 1");

        foreach (var job in legacyJobs)
        {
            RecurringJob.AddOrUpdate(
                job.JobName,
                () => ExecuteLegacyStoredProcedure(job.StoredProcName),
                job.CronExpression);
        }
    }
}
```

**Use case:** Gradual migration from legacy TARGCC

---

#### Mode 3: Manual Registration (Advanced)

```csharp
// For edge cases, manual registration
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHangfire(...);

        // Manual registration
        RecurringJob.AddOrUpdate(
            "custom-job",
            () => MyCustomMethod(),
            "0 */6 * * *");  // Every 6 hours
    }
}
```

**Use case:** Advanced scenarios, integration with external systems

---

## 🏗️ Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────────┐
│                     TargCC Generated Application                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Application Layer                                        │  │
│  │                                                          │  │
│  │  ┌────────────────┐  ┌────────────────┐  ┌───────────┐ │  │
│  │  │ Daily Report   │  │ Cleanup Job    │  │ Sync Job  │ │  │
│  │  │ Job            │  │                │  │           │ │  │
│  │  │ [TargCCJob]    │  │ [TargCCJob]    │  │ [Manual]  │ │  │
│  │  └────────────────┘  └────────────────┘  └───────────┘ │  │
│  │                                                          │  │
│  │  ITargCCJob ← interface (all jobs implement)            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                            ▼                                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Infrastructure Layer                                     │  │
│  │                                                          │  │
│  │  ┌──────────────────────────────────────────────────┐   │  │
│  │  │ JobDiscoveryService                              │   │  │
│  │  │  - Scans assemblies for [TargCCJob]             │   │  │
│  │  │  - Registers with Hangfire                       │   │  │
│  │  │  - Maps CRON expressions                         │   │  │
│  │  └──────────────────────────────────────────────────┘   │  │
│  │                                                          │  │
│  │  ┌──────────────────────────────────────────────────┐   │  │
│  │  │ JobExecutor                                      │   │  │
│  │  │  - Wraps job execution                           │   │  │
│  │  │  - Logs start/end                                │   │  │
│  │  │  - Handles exceptions                            │   │  │
│  │  │  - Returns JobResult                             │   │  │
│  │  └──────────────────────────────────────────────────┘   │  │
│  └──────────────────────────────────────────────────────────┘  │
│                            ▼                                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Hangfire (NuGet Package)                                │  │
│  │  - Job queue management                                  │  │
│  │  - CRON scheduling                                       │  │
│  │  - Retry logic                                           │  │
│  │  - Dashboard UI                                          │  │
│  └──────────────────────────────────────────────────────────┘  │
│                            ▼                                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ SQL Server                                               │  │
│  │  - Hangfire.Job                                          │  │
│  │  - Hangfire.State                                        │  │
│  │  - Hangfire.JobQueue                                     │  │
│  │  - c_LoggedJob (custom - TargCC generated)              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

### Database Schema

#### Hangfire Tables (Auto-Created)

```sql
-- Hangfire creates these automatically
Hangfire.Job             -- Job definitions
Hangfire.State           -- Job state history
Hangfire.JobQueue        -- Job queue
Hangfire.JobParameter    -- Job parameters
Hangfire.Server          -- Server registration
Hangfire.Set             -- Recurring jobs
Hangfire.List            -- Background jobs
Hangfire.Hash            -- State data
Hangfire.Counter         -- Counters
```

#### TargCC Custom Tables

```sql
-- ============================================
-- c_LoggedJob - Enhanced execution history
-- ============================================
CREATE TABLE [dbo].[c_LoggedJob] (
    [ID] BIGINT IDENTITY(1,1) NOT NULL,

    -- Job Identity
    [JobName] NVARCHAR(100) NOT NULL,
    [JobId] NVARCHAR(100) NULL,              -- Hangfire Job ID
    [JobType] NVARCHAR(50) NOT NULL,         -- Recurring, FireAndForget, Delayed, Manual

    -- Execution
    [StartedAt] DATETIME2 NOT NULL,
    [CompletedAt] DATETIME2 NULL,
    [DurationMs] INT NULL,

    -- Result
    [Success] BIT NOT NULL,
    [ResultMessage] NVARCHAR(MAX) NULL,
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [StackTrace] NVARCHAR(MAX) NULL,

    -- Context
    [ServerName] NVARCHAR(100) NULL,
    [TriggeredBy] NVARCHAR(100) NULL,        -- System, Manual, API

    -- Performance
    [MemoryUsedMB] INT NULL,
    [CpuTimeMs] INT NULL,

    -- Audit
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [PK_c_LoggedJob] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

CREATE NONCLUSTERED INDEX [IX_c_LoggedJob_JobName_StartedAt]
    ON [dbo].[c_LoggedJob]([JobName], [StartedAt] DESC)
GO

CREATE NONCLUSTERED INDEX [IX_c_LoggedJob_Success]
    ON [dbo].[c_LoggedJob]([Success], [StartedAt] DESC)
    WHERE [Success] = 0
GO

-- ============================================
-- c_JobAlert (Optional - for custom alerts)
-- ============================================
CREATE TABLE [dbo].[c_JobAlert] (
    [ID] INT IDENTITY(1,1) NOT NULL,
    [JobName] NVARCHAR(100) NOT NULL,

    -- Alert Configuration
    [AlertOnFailure] BIT NOT NULL DEFAULT 1,
    [AlertOnSuccess] BIT NOT NULL DEFAULT 0,
    [AlertOnDurationExceeds] INT NULL,       -- Milliseconds

    -- Recipients
    [EmailRecipients] NVARCHAR(500) NULL,    -- Comma-separated
    [SlackWebhook] NVARCHAR(500) NULL,
    [TeamsWebhook] NVARCHAR(500) NULL,

    -- Throttling
    [MaxAlertsPerHour] INT NULL,
    [LastAlertSent] DATETIME2 NULL,

    CONSTRAINT [PK_c_JobAlert] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UQ_c_JobAlert_JobName] UNIQUE ([JobName])
)
GO
```

---

### Job Lifecycle

```
1. Startup
   ↓
2. JobDiscoveryService scans assemblies
   ↓
3. Find all classes with [TargCCJob] attribute
   ↓
4. Register with Hangfire:
   - Recurring jobs → RecurringJob.AddOrUpdate(...)
   - Manual jobs → Add to registry (trigger via API)
   ↓
5. Hangfire scheduler runs
   ↓
6. Job due → Hangfire enqueues job
   ↓
7. Worker picks up job
   ↓
8. JobExecutor.ExecuteAsync(job)
   ├── Log start → c_LoggedJob
   ├── Call job.ExecuteAsync()
   ├── Catch exceptions
   ├── Log completion → c_LoggedJob
   └── Check alerts → c_JobAlert
   ↓
9. Success or Retry (if failed)
```

---

## 🔧 Technical Specifications

### Core Interfaces

#### ITargCCJob

```csharp
namespace TargCC.Application.Jobs;

/// <summary>
/// Base interface for all TargCC jobs.
/// Jobs implementing this interface are auto-discovered at startup.
/// </summary>
public interface ITargCCJob
{
    /// <summary>
    /// Executes the job logic.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown</param>
    /// <returns>Job execution result</returns>
    Task<JobResult> ExecuteAsync(CancellationToken cancellationToken);
}
```

#### JobResult

```csharp
namespace TargCC.Application.Jobs;

/// <summary>
/// Result of job execution
/// </summary>
public class JobResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public static JobResult Success(string message = "Job completed successfully")
    {
        return new JobResult { Success = true, Message = message };
    }

    public static JobResult Failure(string message, Exception exception = null)
    {
        var result = new JobResult { Success = false, Message = message };

        if (exception != null)
        {
            result.Metadata["Exception"] = exception.GetType().Name;
            result.Metadata["StackTrace"] = exception.StackTrace;
        }

        return result;
    }

    public JobResult WithMetadata(string key, object value)
    {
        Metadata[key] = value;
        return this;
    }
}
```

#### TargCCJobAttribute

```csharp
namespace TargCC.Application.Jobs;

/// <summary>
/// Marks a class as a TargCC job for auto-discovery
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TargCCJobAttribute : Attribute
{
    /// <summary>
    /// Job type
    /// </summary>
    public JobType JobType { get; set; } = JobType.Recurring;

    /// <summary>
    /// CRON expression for recurring jobs (e.g., "0 2 * * *" for 2 AM daily)
    /// </summary>
    public string CronExpression { get; set; }

    /// <summary>
    /// Time zone for CRON expression (default: UTC)
    /// </summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// Queue name for job execution (default: "default")
    /// </summary>
    public string Queue { get; set; } = "default";

    /// <summary>
    /// Retry attempts on failure (default: 3)
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Creates a recurring job attribute
    /// </summary>
    public TargCCJobAttribute(string cronExpression)
    {
        JobType = JobType.Recurring;
        CronExpression = cronExpression;
    }

    /// <summary>
    /// Creates a manual or fire-and-forget job attribute
    /// </summary>
    public TargCCJobAttribute(JobType jobType)
    {
        JobType = jobType;
    }
}

public enum JobType
{
    /// <summary>
    /// Recurring job with CRON schedule
    /// </summary>
    Recurring,

    /// <summary>
    /// Fire-and-forget job (enqueued once)
    /// </summary>
    FireAndForget,

    /// <summary>
    /// Manual trigger only (no auto-scheduling)
    /// </summary>
    Manual
}
```

#### JobDisplayNameAttribute & JobDescriptionAttribute

```csharp
namespace TargCC.Application.Jobs;

[AttributeUsage(AttributeTargets.Class)]
public class JobDisplayNameAttribute : Attribute
{
    public string DisplayName { get; }

    public JobDisplayNameAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class JobDescriptionAttribute : Attribute
{
    public string Description { get; }

    public JobDescriptionAttribute(string description)
    {
        Description = description;
    }
}
```

---

### Job Discovery Service

```csharp
namespace TargCC.Infrastructure.Jobs;

public interface IJobDiscoveryService
{
    /// <summary>
    /// Discovers all jobs and registers them with Hangfire
    /// </summary>
    void RegisterAllJobs();

    /// <summary>
    /// Gets all discovered jobs
    /// </summary>
    IEnumerable<JobMetadata> GetAllJobs();
}

public class HangfireJobDiscoveryService : IJobDiscoveryService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HangfireJobDiscoveryService> _logger;
    private readonly List<JobMetadata> _discoveredJobs = new();

    public HangfireJobDiscoveryService(
        IServiceProvider serviceProvider,
        ILogger<HangfireJobDiscoveryService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void RegisterAllJobs()
    {
        _logger.LogInformation("Starting job discovery...");

        // Get all types implementing ITargCCJob
        var jobTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ITargCCJob).IsAssignableFrom(t)
                     && !t.IsInterface
                     && !t.IsAbstract
                     && t.GetCustomAttribute<TargCCJobAttribute>() != null)
            .ToList();

        _logger.LogInformation("Found {Count} jobs to register", jobTypes.Count);

        foreach (var jobType in jobTypes)
        {
            RegisterJob(jobType);
        }

        _logger.LogInformation("Job discovery complete. Registered {Count} jobs", _discoveredJobs.Count);
    }

    private void RegisterJob(Type jobType)
    {
        var attribute = jobType.GetCustomAttribute<TargCCJobAttribute>();
        var displayName = jobType.GetCustomAttribute<JobDisplayNameAttribute>()?.DisplayName
                          ?? jobType.Name;
        var description = jobType.GetCustomAttribute<JobDescriptionAttribute>()?.Description;

        var metadata = new JobMetadata
        {
            JobType = jobType,
            JobName = displayName,
            Description = description,
            Type = attribute.JobType,
            CronExpression = attribute.CronExpression,
            Queue = attribute.Queue,
            RetryAttempts = attribute.RetryAttempts
        };

        _discoveredJobs.Add(metadata);

        // Register with Hangfire based on job type
        switch (attribute.JobType)
        {
            case JobType.Recurring:
                RecurringJob.AddOrUpdate(
                    displayName,
                    () => ExecuteJobAsync(jobType, CancellationToken.None),
                    attribute.CronExpression,
                    TimeZoneInfo.FindSystemTimeZoneById(attribute.TimeZone));

                _logger.LogInformation("Registered recurring job: {JobName} ({Cron})",
                    displayName, attribute.CronExpression);
                break;

            case JobType.Manual:
                // Manual jobs are not auto-scheduled, just tracked
                _logger.LogInformation("Registered manual job: {JobName}", displayName);
                break;

            case JobType.FireAndForget:
                // Fire-and-forget jobs are not auto-scheduled
                _logger.LogInformation("Registered fire-and-forget job: {JobName}", displayName);
                break;
        }
    }

    public async Task<JobResult> ExecuteJobAsync(Type jobType, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var jobName = jobType.Name;

        try
        {
            _logger.LogInformation("Starting job: {JobName}", jobName);

            // Resolve job instance from DI
            var job = (ITargCCJob)_serviceProvider.GetRequiredService(jobType);

            // Execute
            var result = await job.ExecuteAsync(cancellationToken);

            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Log to c_LoggedJob
            await LogJobExecution(jobName, true, duration, result.Message);

            _logger.LogInformation("Job completed successfully: {JobName} in {Duration}ms",
                jobName, duration);

            return result;
        }
        catch (Exception ex)
        {
            var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogError(ex, "Job failed: {JobName}", jobName);

            // Log failure
            await LogJobExecution(jobName, false, duration, ex.Message, ex.StackTrace);

            // Check alerts
            await CheckAndSendAlerts(jobName, ex);

            throw; // Re-throw for Hangfire retry logic
        }
    }

    private async Task LogJobExecution(
        string jobName,
        bool success,
        int durationMs,
        string message,
        string stackTrace = null)
    {
        // Insert into c_LoggedJob
        // Implementation details...
    }

    private async Task CheckAndSendAlerts(string jobName, Exception exception)
    {
        // Check c_JobAlert table
        // Send alerts if configured
        // Implementation details...
    }

    public IEnumerable<JobMetadata> GetAllJobs()
    {
        return _discoveredJobs.AsReadOnly();
    }
}

public class JobMetadata
{
    public Type JobType { get; set; }
    public string JobName { get; set; }
    public string Description { get; set; }
    public JobType Type { get; set; }
    public string CronExpression { get; set; }
    public string Queue { get; set; }
    public int RetryAttempts { get; set; }
}
```

---

### CLI Commands

```bash
# Job management commands
targcc job list                        # List all discovered jobs
targcc job info <job-name>             # Show job details
targcc job run <job-name>              # Trigger job manually
targcc job history <job-name>          # Show execution history
targcc job disable <job-name>          # Disable recurring job
targcc job enable <job-name>           # Enable recurring job

# Job generation
targcc generate job <name> --cron "0 2 * * *"    # Generate job template
targcc generate job <name> --manual              # Generate manual job
targcc generate job <name> --fire-and-forget     # Generate fire-and-forget job

# Examples:
targcc generate job DailyReportJob --cron "0 2 * * *"
# Creates: Application/Jobs/DailyReportJob.cs

targcc generate job SendEmailJob --fire-and-forget
# Creates: Application/Jobs/SendEmailJob.cs

targcc job list
# Output:
# Discovered Jobs:
#   1. DailySalesReportJob (Recurring: 0 2 * * *)
#   2. WeeklyCleanupJob (Recurring: 0 0 * * 0)
#   3. MonthEndReportJob (Manual)
#   4. SendWelcomeEmailJob (FireAndForget)

targcc job run MonthEndReportJob
# Output:
# Triggering MonthEndReportJob...
# Job enqueued: Hangfire-12345
# View status: http://localhost:5000/hangfire/jobs/details/12345
```

---

### JobsController (Auto-Generated)

```csharp
namespace TargCC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobDiscoveryService _jobDiscovery;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobDiscoveryService jobDiscovery,
        ILogger<JobsController> logger)
    {
        _jobDiscovery = jobDiscovery;
        _logger = logger;
    }

    /// <summary>
    /// Get all discovered jobs
    /// </summary>
    [HttpGet]
    public IActionResult GetAll()
    {
        var jobs = _jobDiscovery.GetAllJobs();
        return Ok(jobs);
    }

    /// <summary>
    /// Trigger a job manually
    /// </summary>
    [HttpPost("{jobName}/trigger")]
    public IActionResult TriggerJob(string jobName)
    {
        var job = _jobDiscovery.GetAllJobs()
            .FirstOrDefault(j => j.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase));

        if (job == null)
            return NotFound($"Job '{jobName}' not found");

        // Enqueue job
        var jobId = BackgroundJob.Enqueue(() =>
            _jobDiscovery.ExecuteJobAsync(job.JobType, CancellationToken.None));

        _logger.LogInformation("Job {JobName} triggered manually. JobId: {JobId}", jobName, jobId);

        return Ok(new { JobId = jobId, Message = $"Job '{jobName}' triggered successfully" });
    }

    /// <summary>
    /// Get job execution history
    /// </summary>
    [HttpGet("{jobName}/history")]
    public async Task<IActionResult> GetHistory(string jobName, [FromQuery] int limit = 50)
    {
        // Query c_LoggedJob
        var history = await _db.Query<c_LoggedJob>(
            "SELECT TOP (@Limit) * FROM c_LoggedJob WHERE JobName = @JobName ORDER BY StartedAt DESC",
            new { JobName = jobName, Limit = limit });

        return Ok(history);
    }
}
```

---

### appsettings.json Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MyApp;Trusted_Connection=True;",
    "HangfireConnection": "Server=.;Database=MyApp;Trusted_Connection=True;"
  },

  "Hangfire": {
    "ServerName": "MyApp-Server1",
    "WorkerCount": 5,
    "Queues": ["default", "critical", "background"],
    "Dashboard": {
      "Enabled": true,
      "Path": "/hangfire",
      "RequireAuthentication": true
    },
    "Storage": {
      "CommandBatchMaxTimeout": "00:05:00",
      "SlidingInvisibilityTimeout": "00:05:00",
      "QueuePollInterval": "00:00:00"
    }
  },

  "TargCCJobs": {
    "AutoDiscovery": true,
    "LogToDatabase": true,
    "AlertsEnabled": true,
    "DefaultRetryAttempts": 3,
    "DefaultQueue": "default"
  }
}
```

---

## 📅 Implementation Plan

### Timeline: 4-6 Days

#### Day 1: Core Infrastructure
```
✅ Install Hangfire NuGet packages
✅ Create database migration for c_LoggedJob, c_JobAlert
✅ Implement ITargCCJob interface
✅ Implement JobResult class
✅ Implement TargCCJobAttribute
✅ Create base job template
✅ Unit tests for JobResult
```

#### Day 2: Job Discovery
```
✅ Implement HangfireJobDiscoveryService
✅ Assembly scanning logic
✅ Job registration logic
✅ Integration with Hangfire
✅ Logging setup
✅ Unit tests for discovery service
```

#### Day 3: Job Execution & Logging
```
✅ Implement JobExecutor wrapper
✅ c_LoggedJob database logging
✅ Exception handling
✅ Retry logic integration
✅ Performance metrics (duration, memory)
✅ Integration tests for execution
```

#### Day 4: CLI & Generation
```
✅ CLI commands: job list, info, run, history
✅ Job template generator (targcc generate job)
✅ Program.cs auto-update (add Hangfire setup)
✅ appsettings.json template
✅ End-to-end tests
```

#### Day 5: Dashboard & API
```
✅ JobsController implementation
✅ Hangfire dashboard configuration
✅ Authentication for dashboard
✅ API endpoints for manual triggers
✅ Swagger documentation
✅ API integration tests
```

#### Day 6: Alerts & Migration (Optional)
```
✅ c_JobAlert implementation
✅ Email alert service
✅ Slack/Teams webhook support
✅ Legacy c_Job importer
✅ Migration guide documentation
✅ Performance tests
```

---

## 🧪 Testing Strategy

### Unit Tests

```csharp
// JobResultTests.cs
[TestClass]
public class JobResultTests
{
    [TestMethod]
    public void Success_ReturnsSuccessResult()
    {
        var result = JobResult.Success("Test message");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("Test message", result.Message);
    }

    [TestMethod]
    public void Failure_ReturnsFailureResult()
    {
        var exception = new InvalidOperationException("Test error");
        var result = JobResult.Failure("Failed", exception);

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Failed", result.Message);
        Assert.IsTrue(result.Metadata.ContainsKey("Exception"));
    }

    [TestMethod]
    public void WithMetadata_AddsMetadata()
    {
        var result = JobResult.Success()
            .WithMetadata("Count", 100)
            .WithMetadata("Duration", 1234);

        Assert.AreEqual(100, result.Metadata["Count"]);
        Assert.AreEqual(1234, result.Metadata["Duration"]);
    }
}

// JobDiscoveryServiceTests.cs
[TestClass]
public class JobDiscoveryServiceTests
{
    [TestMethod]
    public void RegisterAllJobs_DiscoversJobsWithAttribute()
    {
        var service = new HangfireJobDiscoveryService(/* dependencies */);

        service.RegisterAllJobs();

        var jobs = service.GetAllJobs();

        Assert.IsTrue(jobs.Any(j => j.JobName == "TestJob"));
    }

    [TestMethod]
    public void RegisterJob_RecurringJob_RegistersWithHangfire()
    {
        // Test that recurring jobs are registered with correct CRON
    }
}

// Sample test job
[TargCCJob("0 0 * * *")]
[JobDisplayName("TestJob")]
public class TestJob : ITargCCJob
{
    public Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(JobResult.Success());
    }
}
```

### Integration Tests

```csharp
[TestClass]
public class JobExecutionIntegrationTests
{
    [TestMethod]
    public async Task ExecuteJob_Success_LogsToDatabase()
    {
        // Arrange
        var job = new SampleSuccessJob();

        // Act
        var result = await job.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Success);

        // Verify logged to c_LoggedJob
        var log = await GetLastLoggedJob("SampleSuccessJob");
        Assert.IsNotNull(log);
        Assert.IsTrue(log.Success);
    }

    [TestMethod]
    public async Task ExecuteJob_Failure_LogsException()
    {
        // Arrange
        var job = new SampleFailingJob();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            job.ExecuteAsync(CancellationToken.None));

        // Verify logged to c_LoggedJob
        var log = await GetLastLoggedJob("SampleFailingJob");
        Assert.IsNotNull(log);
        Assert.IsFalse(log.Success);
        Assert.IsNotNull(log.ErrorMessage);
    }
}
```

### Performance Tests

```csharp
[TestClass]
public class JobPerformanceTests
{
    [TestMethod]
    public async Task JobDiscovery_100Jobs_CompletesUnder1Second()
    {
        var stopwatch = Stopwatch.StartNew();

        var service = new HangfireJobDiscoveryService(/* dependencies */);
        service.RegisterAllJobs();

        stopwatch.Stop();

        Assert.IsTrue(stopwatch.Elapsed.TotalSeconds < 1,
            $"Discovery took {stopwatch.Elapsed.TotalSeconds}s (expected < 1s)");
    }
}
```

---

## 🚀 Rollout Plan

### Phase 1: Internal Testing (Day 1-2)
```
✅ Deploy to development environment
✅ Test job discovery
✅ Test job execution
✅ Test Hangfire dashboard
✅ Verify logging to c_LoggedJob
```

### Phase 2: Documentation (Day 3-4)
```
✅ Update README.md with job examples
✅ Create JOB_DEVELOPMENT_GUIDE.md
✅ Update CLI help text
✅ Create video walkthrough (optional)
```

### Phase 3: Pilot (Day 5-7)
```
✅ Test with 2-3 real use cases
✅ Gather feedback
✅ Fix issues
✅ Optimize performance
```

### Phase 4: Production Release (Day 8+)
```
✅ Tag version: v2.5.0
✅ Release notes
✅ Announce to users
✅ Provide support for migration from legacy
```

---

## 📊 Success Metrics

### Performance Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Job discovery (100 jobs)** | < 1 second | Startup timing |
| **Job execution overhead** | < 100ms | Wrapper overhead |
| **Dashboard load time** | < 2 seconds | Browser timing |
| **Database logging** | < 50ms | Insert timing |

### Quality Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Unit test coverage** | > 80% | Code coverage tool |
| **Integration tests** | All scenarios | Test results |
| **Successful job discovery** | 100% | Discovery tests |
| **Zero manual configuration** | 100% | Convention-based |

### User Experience Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Developer onboarding** | < 5 minutes | Time to first job |
| **Dashboard clarity** | Intuitive | User feedback |
| **Error messages** | Helpful | User feedback |
| **CLI usability** | Easy | User feedback |

---

## 🎯 Summary

### What Phase 3E Delivers

1. ✅ **Convention-Based Jobs** - Write `[TargCCJob]` class, auto-discovered
2. ✅ **Hangfire Integration** - Beautiful dashboard at `/hangfire`
3. ✅ **Zero Configuration** - No manual registration needed
4. ✅ **CRON Support** - Standard CRON expressions
5. ✅ **Automatic Retries** - Built-in retry logic
6. ✅ **Distributed Execution** - Multi-server support (Hangfire Pro)
7. ✅ **CLI Commands** - `targcc job list/run/history`
8. ✅ **API Endpoints** - Manual trigger via REST API
9. ✅ **Comprehensive Logging** - c_LoggedJob database table
10. ✅ **Alert System** - Email/Slack/Teams notifications (optional)
11. ✅ **TargCC Generation** - Auto-generate job templates
12. ✅ **Legacy Migration** - Import from c_Job (optional)

### Comparison: Legacy vs Modern

| Feature | Legacy (c_Job) | Modern (Phase 3E) | Improvement |
|---------|----------------|-------------------|-------------|
| **Configuration** | SQL INSERT | `[TargCCJob]` attribute | 90% less code |
| **Logic** | Stored Procedures | C# classes + DI | 100% flexibility |
| **Dashboard** | ❌ None | ✅ Hangfire UI | Priceless |
| **Retry** | ❌ Manual | ✅ Automatic | 100% reliability |
| **Distributed** | ❌ Single server | ✅ Multi-server | Unlimited scale |
| **Monitoring** | SQL queries | UI dashboard | 95% faster |
| **Debugging** | Logs only | Full stack traces | 90% faster |

### Next Steps After Phase 3E

**Phase 3F: Advanced UI Generation**
- Form validation
- Grid sorting/filtering
- Custom themes

**Phase 4: Deployment & DevOps**
- Docker support
- CI/CD integration
- NuGet package

**Phase 5: Enterprise Features**
- Multi-tenant support
- Advanced security
- Performance optimization

---

**End of Phase 3E Specification**
**Ready for Implementation** 🚀
