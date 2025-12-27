using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpayCard.RiskManagement.Application.Common.Interfaces;
using UpayCard.RiskManagement.Domain.Interfaces;
using UpayCard.RiskManagement.Infrastructure.Data;
using UpayCard.RiskManagement.Infrastructure.Repositories;

namespace UpayCard.RiskManagement.Infrastructure;

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

        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Auto-detect database type and register DbContext accordingly
        if (connectionString?.Contains(".db", StringComparison.OrdinalIgnoreCase) == true ||
            connectionString?.Contains(".sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            // SQLite
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            // SQL Server (default for production)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        // Register IApplicationDbContext interface
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Register repositories
        services.AddScoped<IReport2Repository, Report2Repository>();
        services.AddScoped<IActiveRestrictionsRepository, ActiveRestrictionsRepository>();
        services.AddScoped<IBackofficeTransactionRepository, BackofficeTransactionRepository>();
        services.AddScoped<IAlertMessageRepository, AlertMessageRepository>();
        services.AddScoped<IAuditIndexedRepository, AuditIndexedRepository>();
        services.AddScoped<IEnumerationRepository, EnumerationRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobAlertRecipientRepository, JobAlertRecipientRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<ILoggedAlertRepository, LoggedAlertRepository>();
        services.AddScoped<ILoggedJobRepository, LoggedJobRepository>();
        services.AddScoped<ILoggedLoginRepository, LoggedLoginRepository>();
        services.AddScoped<ILoggedRequestRepository, LoggedRequestRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<IMailRepository, MailRepository>();
        services.AddScoped<IMfaRepository, MfaRepository>();
        services.AddScoped<IObjectToTranslateRepository, ObjectToTranslateRepository>();
        services.AddScoped<IObjectTranslationRepository, ObjectTranslationRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IProcessRepository, ProcessRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISystemAuditRepository, SystemAuditRepository>();
        services.AddScoped<ISystemDefaultRepository, SystemDefaultRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<ITableFieldsRepository, TableFieldsRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserLoginKeyRepository, UserLoginKeyRepository>();
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        services.AddScoped<IUserStatusRepository, UserStatusRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<ICardActivationRepository, CardActivationRepository>();
        services.AddScoped<ICardClubRepository, CardClubRepository>();
        services.AddScoped<ICardIssuerRepository, CardIssuerRepository>();
        services.AddScoped<ICardOrderByDistributorRepository, CardOrderByDistributorRepository>();
        services.AddScoped<ICardOrderFromIssuerRepository, CardOrderFromIssuerRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<ICreditGuardRepository, CreditGuardRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerCardTransactionRepository, CustomerCardTransactionRepository>();
        services.AddScoped<ICustomerCardTransactionDefinitionRepository, CustomerCardTransactionDefinitionRepository>();
        services.AddScoped<ICustomerClubRepository, CustomerClubRepository>();
        services.AddScoped<ICustomerCreditCardRepository, CustomerCreditCardRepository>();
        services.AddScoped<ICustomerKYCRepository, CustomerKYCRepository>();
        services.AddScoped<ICustomerPartnershipRepository, CustomerPartnershipRepository>();
        services.AddScoped<ICustomerPerformanceRepository, CustomerPerformanceRepository>();
        services.AddScoped<ICustomerWalletRepository, CustomerWalletRepository>();
        services.AddScoped<IDistributorRepository, DistributorRepository>();
        services.AddScoped<IDistributorBranchRepository, DistributorBranchRepository>();
        services.AddScoped<IDistributorObligoRepository, DistributorObligoRepository>();
        services.AddScoped<IDrivebotPaymentRepository, DrivebotPaymentRepository>();
        services.AddScoped<IEntityRiskColorRepository, EntityRiskColorRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IExternalCustomerRepository, ExternalCustomerRepository>();
        services.AddScoped<IExternalTransactionRepository, ExternalTransactionRepository>();
        services.AddScoped<IFeeRepository, FeeRepository>();
        services.AddScoped<IFeeOverrideRepository, FeeOverrideRepository>();
        services.AddScoped<IGovtReport00Repository, GovtReport00Repository>();
        services.AddScoped<IGovtReport10Repository, GovtReport10Repository>();
        services.AddScoped<IGovtReport30Repository, GovtReport30Repository>();
        services.AddScoped<IGovtReport40Repository, GovtReport40Repository>();
        services.AddScoped<IGovtReport99Repository, GovtReport99Repository>();
        services.AddScoped<IGovtReportDefinitionRepository, GovtReportDefinitionRepository>();
        services.AddScoped<IGovtReportDistributorRepository, GovtReportDistributorRepository>();
        services.AddScoped<IGovtReportPaymentRepository, GovtReportPaymentRepository>();
        services.AddScoped<IImportTransactionLoadRepository, ImportTransactionLoadRepository>();
        services.AddScoped<IImportTransactionLoadRowRepository, ImportTransactionLoadRowRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<ILimitationRepository, LimitationRepository>();
        services.AddScoped<ILimitationOverrideRepository, LimitationOverrideRepository>();
        services.AddScoped<IMCCBFPBankAccountRepository, MCCBFPBankAccountRepository>();
        services.AddScoped<IMCCBFPCashOutRepository, MCCBFPCashOutRepository>();
        services.AddScoped<IMCCBFPMobileWalletRepository, MCCBFPMobileWalletRepository>();
        services.AddScoped<IMCCBFPPaymentCardRepository, MCCBFPPaymentCardRepository>();
        services.AddScoped<IMCCBParticipantRepository, MCCBParticipantRepository>();
        services.AddScoped<IMCCBReceiverRepository, MCCBReceiverRepository>();
        services.AddScoped<IMCCBReceiverAccountRepository, MCCBReceiverAccountRepository>();
        services.AddScoped<IMCCBSecurityCheckRepository, MCCBSecurityCheckRepository>();
        services.AddScoped<IMCCBTransactionRepository, MCCBTransactionRepository>();
        services.AddScoped<INewCardRequestRepository, NewCardRequestRepository>();
        services.AddScoped<INewCardRequestAPIRepository, NewCardRequestAPIRepository>();
        services.AddScoped<INonCustomerRepository, NonCustomerRepository>();
        services.AddScoped<IOneTimeLinkRepository, OneTimeLinkRepository>();
        services.AddScoped<IOnlineApprovalRepository, OnlineApprovalRepository>();
        services.AddScoped<IRequestedTransactionRepository, RequestedTransactionRepository>();
        services.AddScoped<IRequestedTransactionFeeRepository, RequestedTransactionFeeRepository>();
        services.AddScoped<IRiskEventRepository, RiskEventRepository>();
        services.AddScoped<IRiskRuleRepository, RiskRuleRepository>();
        services.AddScoped<IRiskRuleLogRepository, RiskRuleLogRepository>();
        services.AddScoped<IRiskRuleOverrideRepository, RiskRuleOverrideRepository>();
        services.AddScoped<ISapRepository, SapRepository>();
        services.AddScoped<ISysdiagramsRepository, SysdiagramsRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITransactionBitRepository, TransactionBitRepository>();
        services.AddScoped<ITransactionCreditCardRepository, TransactionCreditCardRepository>();
        services.AddScoped<ITransactionLoadRepository, TransactionLoadRepository>();
        services.AddScoped<ITransactionLoadAccountRepository, TransactionLoadAccountRepository>();
        services.AddScoped<IUITextRepository, UITextRepository>();
        services.AddScoped<IUserApprovalRepository, UserApprovalRepository>();

        return services;
    }
}
