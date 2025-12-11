// ============================================================================
// TargCC Generated: Hangfire Job Scheduler Setup
// This code should be added to your Program.cs
// ============================================================================

using Hangfire;
using UpayCard.RiskManagement.Infrastructure.Jobs;

// === Add Hangfire Services ===
builder.Services.AddHangfireServices(builder.Configuration);

// After app.Build():

// === Configure Hangfire Dashboard ===
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "UpayCard_RiskManagement_CCV2 - Job Scheduler",
    StatsPollingInterval = 2000
});

// === Discover and Register Jobs ===
using (var scope = app.Services.CreateScope())
{
    var jobDiscovery = scope.ServiceProvider.GetRequiredService<IJobDiscoveryService>();
    jobDiscovery.RegisterAllJobs();
}

// ============================================================================
