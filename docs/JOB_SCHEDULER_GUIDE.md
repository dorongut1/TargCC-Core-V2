# TargCC Job Scheduler - Usage Guide

**Version:** 2.1.0
**Date:** 10/12/2025
**Status:** ✅ Implemented - Ready to Use

---

## 📖 Table of Contents

1. [Introduction](#introduction)
2. [Quick Start](#quick-start)
3. [Creating Jobs](#creating-jobs)
4. [Job Types](#job-types)
5. [CLI Reference](#cli-reference)
6. [Configuration](#configuration)
7. [Dashboard](#dashboard)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

---

## 🎯 Introduction

TargCC Job Scheduler is a modern, convention-based background job system powered by **Hangfire**. It allows you to:

- ✅ **Schedule recurring jobs** with CRON expressions
- ✅ **Trigger manual jobs** on-demand
- ✅ **Auto-discover jobs** using attributes (zero configuration!)
- ✅ **Monitor executions** via beautiful dashboard
- ✅ **Track history** in database (c_LoggedJob)
- ✅ **Send alerts** on failures (Email, Slack, Teams)

### Key Features

| Feature | Description |
|---------|-------------|
| **Convention-Based** | Write a class, add `[TargCCJob]`, done! |
| **Hangfire Dashboard** | Beautiful UI at `/hangfire` |
| **Database Logging** | All executions logged to `c_LoggedJob` |
| **CLI Integration** | Manage jobs from command line |
| **Dependency Injection** | Full DI support in jobs |
| **Automatic Retries** | Configurable retry logic |

---

## 🚀 Quick Start

### Step 1: Generate Job Infrastructure

When you create a new TargCC project, job infrastructure is automatically generated:

```bash
targcc init MyProject --template CleanArchitecture
```

This creates:
```
MyProject/
├── Application/
│   └── Jobs/
│       ├── ITargCCJob.cs                    ✅ Base interface
│       ├── JobResult.cs                     ✅ Result type
│       ├── TargCCJobAttribute.cs            ✅ Attributes
│       ├── SampleDailyJob.cs                ✅ Daily job example
│       └── SampleManualJob.cs               ✅ Manual job example
│
└── Infrastructure/
    └── Jobs/
        ├── HangfireSetup.cs                 ✅ DI setup
        ├── HangfireJobDiscoveryService.cs   ✅ Auto-discovery
        ├── JobExecutor.cs                   ✅ Execution wrapper
        ├── JobLogger.cs                     ✅ Database logging
        └── HangfireAuthorizationFilter.cs   ✅ Dashboard auth
```

### Step 2: Run Database Migration

Apply the job tables migration:

```bash
# Run SQL migration
sqlcmd -S localhost -d MyDatabase -i database/migrations/002_Create_Job_Tables.sql
```

This creates:
- `c_LoggedJob` - Execution history
- `c_JobAlert` - Alert configuration
- `SP_GetJobHistory` - Query history
- `SP_GetJobStatistics` - Get stats

### Step 3: Start Your Application

```bash
cd MyProject/API
dotnet run
```

**Jobs are automatically discovered and registered!**

### Step 4: View Dashboard

Open your browser:
```
http://localhost:5000/hangfire
```

You'll see:
- All discovered jobs
- Execution history
- Success/failure rates
- Real-time updates

---

## 📝 Creating Jobs

### Method 1: Using CLI (Recommended)

```bash
# Create a daily recurring job
targcc job generate DailyReportJob --cron "0 2 * * *"

# Create a manual job
targcc job generate MonthEndReportJob --manual

# Create with metadata
targcc job generate WeeklyCleanupJob \
  --cron "0 0 * * 0" \
  --display-name "Weekly Cleanup" \
  --description "Cleans up old logs and temp files"
```

### Method 2: Manual Creation

Create a new file in `Application/Jobs/`:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyProject.Application.Jobs;

namespace MyProject.Application.Jobs;

/// <summary>
/// Daily sales report job
/// </summary>
[TargCCJob("0 2 * * *")]  // 2 AM daily
[JobDisplayName("Daily Sales Report")]
[JobDescription("Generates daily sales report and emails it to managers")]
public class DailySalesReportJob : ITargCCJob
{
    private readonly IOrderRepository _orderRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<DailySalesReportJob> _logger;

    // Dependency Injection fully supported!
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
            var report = new
            {
                Date = yesterday,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.Total)
            };

            // Send email
            await _emailService.SendReportAsync("managers@company.com", report, cancellationToken);

            _logger.LogInformation(
                "Daily sales report sent. Orders: {Count}, Revenue: ${Revenue:N2}",
                report.TotalOrders,
                report.TotalRevenue);

            return JobResult.Success($"Processed {orders.Count} orders")
                .WithMetadata("Revenue", report.TotalRevenue)
                .WithMetadata("Orders", report.TotalOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily sales report");
            return JobResult.Failure($"Report generation failed: {ex.Message}", ex);
        }
    }
}
```

**That's it!** The job will be automatically discovered on next startup.

---

## 🎭 Job Types

### 1. Recurring Jobs (CRON)

Jobs that run on a schedule using CRON expressions.

```csharp
[TargCCJob("0 2 * * *")]  // Daily at 2 AM
[JobDisplayName("Daily Cleanup")]
public class DailyCleanupJob : ITargCCJob
{
    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        // Delete old logs
        await DeleteOldLogsAsync(cancellationToken);

        return JobResult.Success("Cleanup completed");
    }
}
```

**Common CRON patterns:**
```
"*/5 * * * *"     - Every 5 minutes
"0 * * * *"       - Every hour
"0 2 * * *"       - Daily at 2 AM
"0 0 * * 0"       - Weekly on Sunday at midnight
"0 0 1 * *"       - Monthly on 1st at midnight
"0 0 1 1 *"       - Yearly on January 1st at midnight
```

Use [crontab.guru](https://crontab.guru) to test expressions!

### 2. Manual Jobs

Jobs that can only be triggered via CLI or API.

```csharp
[TargCCJob(JobType.Manual)]
[JobDisplayName("Month-End Report")]
public class MonthEndReportJob : ITargCCJob
{
    public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        // Generate comprehensive month-end report
        // ...
        return JobResult.Success("Report generated");
    }
}
```

**Trigger manually:**
```bash
targcc job run MonthEndReportJob

# Or via API:
curl -X POST http://localhost:5000/api/jobs/MonthEndReportJob/trigger
```

### 3. Fire-and-Forget Jobs

Jobs that run once when enqueued (programmatically).

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
        // Send email...
        return JobResult.Success();
    }
}
```

**Enqueue from code:**
```csharp
// In your UserService.cs
public async Task<User> RegisterUserAsync(UserDto dto)
{
    var user = await _repository.AddAsync(new User { Email = dto.Email });

    // Enqueue welcome email job
    BackgroundJob.Enqueue<SendWelcomeEmailJob>(job =>
        job.ExecuteAsync(CancellationToken.None));

    return user;
}
```

---

## 💻 CLI Reference

### List Jobs

```bash
# List all jobs
targcc job list

# Output as JSON
targcc job list --format json

# Output as CSV
targcc job list --format csv
```

**Example output:**
```
📋 Discovered Jobs:

Name                      Type         Schedule            Status
----------------------------------------------------------------------
DailySalesReportJob       Recurring    0 2 * * *          Active
WeeklyCleanupJob          Recurring    0 0 * * 0          Active
MonthEndReportJob         Manual       Manual             Active

Total: 3 job(s) found

💡 Use 'targcc job info <name>' for details
```

### View Job Info

```bash
targcc job info DailySalesReportJob
```

**Output:**
```
📋 Job Information: DailySalesReportJob

Name:           DailySalesReportJob
Display Name:   Daily Sales Report
Type:           Recurring
CRON:           0 2 * * *  (Daily at 2 AM)
Queue:          default
Retry Attempts: 3
Status:         Active

Description:
Generates daily sales report and emails it to managers

Last Execution:
  Started:  2025-12-09 02:00:00
  Duration: 1,234 ms
  Status:   ✅ Success

💡 Use 'targcc job run DailySalesReportJob' to trigger manually
```

### Run Job

```bash
# Fire and forget
targcc job run DailySalesReportJob

# Wait for completion
targcc job run DailySalesReportJob --wait
```

### View History

```bash
# Last 10 executions
targcc job history DailySalesReportJob

# Last 50 executions
targcc job history DailySalesReportJob --limit 50

# Only failed executions
targcc job history DailySalesReportJob --failed
```

### View Statistics

```bash
# Last 7 days
targcc job stats DailySalesReportJob

# Last 30 days
targcc job stats DailySalesReportJob --days 30
```

**Output:**
```
📊 Job Statistics: DailySalesReportJob (Last 7 days)

Executions:
  Total:        7
  Successful:   7
  Failed:       0
  Success Rate: 100.00%

Duration:
  Average:      1,234 ms
  Minimum:      987 ms
  Maximum:      2,456 ms

Last Execution:
  Time:         2025-12-09 02:00:00
  Duration:     1,234 ms
  Status:       ✅ Success
```

### Generate Job

```bash
# Daily recurring job
targcc job generate DailyBackupJob --cron "0 3 * * *"

# Manual job
targcc job generate ProcessInvoicesJob --manual

# With metadata
targcc job generate WeeklySummaryJob \
  --cron "0 9 * * 1" \
  --display-name "Weekly Summary Email" \
  --description "Sends weekly summary to all users"
```

---

## ⚙️ Configuration

### appsettings.json

Add Hangfire configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=True;"
  },

  "Hangfire": {
    "ServerName": "MyApp-Server",
    "WorkerCount": 5,
    "Queues": [
      "default",
      "critical",
      "background"
    ],
    "Dashboard": {
      "Enabled": true,
      "Path": "/hangfire",
      "RequireAuthentication": true
    }
  },

  "TargCCJobs": {
    "AutoDiscovery": true,
    "LogToDatabase": true,
    "AlertsEnabled": true,
    "DefaultRetryAttempts": 3
  }
}
```

### Program.cs Setup

The setup is auto-generated, but here's what it looks like:

```csharp
using Hangfire;
using MyProject.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services
builder.Services.AddHangfireServices(builder.Configuration);

var app = builder.Build();

// Configure Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "MyProject - Job Scheduler"
});

// Discover and register jobs
using (var scope = app.Services.CreateScope())
{
    var jobDiscovery = scope.ServiceProvider.GetRequiredService<IJobDiscoveryService>();
    jobDiscovery.RegisterAllJobs();
}

app.Run();
```

---

## 📊 Dashboard

Access the Hangfire dashboard:

```
http://localhost:5000/hangfire
```

### Features

1. **Jobs** - View all recurring jobs and their schedules
2. **Recurring Jobs** - Manage CRON schedules
3. **Processing** - See currently running jobs
4. **Succeeded** - View successful executions
5. **Failed** - View failures with stack traces
6. **Deleted** - View deleted jobs
7. **Retries** - View retry attempts

### Actions

- ✅ Trigger job manually
- ✅ Pause/Resume recurring jobs
- ✅ Retry failed jobs
- ✅ Delete jobs from queue
- ✅ View detailed execution logs

---

## 🎯 Best Practices

### 1. Job Design

**DO:**
- ✅ Keep jobs focused (single responsibility)
- ✅ Use async/await throughout
- ✅ Handle cancellation tokens
- ✅ Log important events
- ✅ Return descriptive JobResults

**DON'T:**
- ❌ Make jobs too large or complex
- ❌ Ignore cancellation tokens
- ❌ Throw unhandled exceptions without logging
- ❌ Access HttpContext (jobs run in background)

### 2. Error Handling

```csharp
public async Task<JobResult> ExecuteAsync(CancellationToken cancellationToken)
{
    try
    {
        // Job logic
        return JobResult.Success("Completed");
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Job was cancelled");
        return JobResult.Failure("Job was cancelled");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Job failed");
        return JobResult.Failure($"Failed: {ex.Message}", ex);
    }
}
```

### 3. Performance

- ✅ Use pagination for large datasets
- ✅ Process in batches
- ✅ Use cancellation tokens
- ✅ Set appropriate timeouts

### 4. Logging

```csharp
public class MyJob : ITargCCJob
{
    private readonly ILogger<MyJob> _logger;

    public async Task<JobResult> ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Job started");

        // Work

        _logger.LogInformation("Processed {Count} items", count);

        return JobResult.Success()
            .WithMetadata("ItemsProcessed", count);
    }
}
```

### 5. Testing

```csharp
[TestClass]
public class DailySalesReportJobTests
{
    [TestMethod]
    public async Task ExecuteAsync_WithOrders_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetOrdersByDateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { new Order { Total = 100 } });

        var job = new DailySalesReportJob(mockRepo.Object, ...);

        // Act
        var result = await job.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Metadata.ContainsKey("Revenue"));
    }
}
```

---

## 🔧 Troubleshooting

### Jobs Not Discovered

**Problem:** Jobs don't appear in dashboard or `targcc job list`

**Solutions:**
1. Check job has `[TargCCJob]` attribute
2. Verify job implements `ITargCCJob`
3. Ensure job is in Application/Jobs folder
4. Rebuild project
5. Check logs for discovery errors

### Job Fails Silently

**Problem:** Job executes but doesn't log or appears successful when it failed

**Solutions:**
1. Ensure you're returning `JobResult.Failure()` on errors
2. Check c_LoggedJob table for entries
3. Review JobLogger implementation
4. Check database connection string

### Dashboard Not Accessible

**Problem:** `/hangfire` returns 404 or access denied

**Solutions:**
1. Verify `app.UseHangfireDashboard()` is called
2. Check `HangfireAuthorizationFilter` logic
3. Ensure Hangfire middleware is registered before `app.Run()`
4. Check firewall/port settings

### Database Connection Issues

**Problem:** Jobs fail with SQL errors

**Solutions:**
1. Verify connection string in appsettings.json
2. Run database migration (002_Create_Job_Tables.sql)
3. Check SQL Server is running
4. Verify database permissions

---

## 📚 Additional Resources

- [Hangfire Documentation](https://docs.hangfire.io)
- [CRON Expression Reference](https://crontab.guru)
- [Phase 3E Specification](/docs/PHASE_3E_JOB_SCHEDULER_AND_BACKGROUND_SERVICES.md)
- [TargCC Documentation](/docs/)

---

**Need Help?**
- 📧 Create an issue on GitHub
- 💬 Join our Discord community
- 📖 Check the Phase 3E specification document

---

**Version:** 2.1.0 | **Last Updated:** 10/12/2025 | **Status:** ✅ Production Ready
