using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestApp.Domain.Interfaces;
using TestApp.Infrastructure.Repositories;

namespace TestApp.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register database connection
        services.AddScoped<IDbConnection>(sp => 
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            // Auto-detect database type from connection string
            if (connectionString?.Contains(".db", StringComparison.OrdinalIgnoreCase) == true ||
                connectionString?.Contains(".sqlite", StringComparison.OrdinalIgnoreCase) == true)
            {
                // SQLite connection for file-based databases
                return new SqliteConnection(connectionString);
            }
            else
            {
                // SQL Server connection (default for production)
                return new SqlConnection(connectionString);
            }
        });

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IVwCustomerOrderSummaryRepository, VwCustomerOrderSummaryRepository>();
        services.AddScoped<IVwOrderDetailsRepository, VwOrderDetailsRepository>();
        services.AddScoped<IVwProductSalesRepository, VwProductSalesRepository>();

        return services;
    }
}
