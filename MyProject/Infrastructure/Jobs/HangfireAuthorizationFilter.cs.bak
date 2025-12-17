using Hangfire.Dashboard;

namespace UpayCard.RiskManagement.Infrastructure.Jobs;

/// <summary>
/// Authorization filter for Hangfire dashboard
/// IMPORTANT: Replace this with proper authentication in production!
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // TODO: Add proper authentication
        // For development, allow all access
        // In production, check user claims/roles

        // Example production code:
        // var httpContext = context.GetHttpContext();
        // return httpContext.User.Identity?.IsAuthenticated ?? false;

        // Development only:
        return true;
    }
}
