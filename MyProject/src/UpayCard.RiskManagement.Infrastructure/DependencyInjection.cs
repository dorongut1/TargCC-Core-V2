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
        services.AddScoped<IIndexFragmentationRepository, IndexFragmentationRepository>();
        services.AddScoped<IIndexFragmentationDataRepository, IndexFragmentationDataRepository>();
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
        services.AddScoped<ITableSizeRepository, TableSizeRepository>();
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
        services.AddScoped<ICcvwComboListActiveRestrictionsRepository, CcvwComboListActiveRestrictionsRepository>();
        services.AddScoped<ICcvwComboListCAlertMessageRepository, CcvwComboListCAlertMessageRepository>();
        services.AddScoped<ICcvwComboListCEnumerationRepository, CcvwComboListCEnumerationRepository>();
        services.AddScoped<ICcvwComboListCJobRepository, CcvwComboListCJobRepository>();
        services.AddScoped<ICcvwComboListCLanguageRepository, CcvwComboListCLanguageRepository>();
        services.AddScoped<ICcvwComboListCLoggedAlertRepository, CcvwComboListCLoggedAlertRepository>();
        services.AddScoped<ICcvwComboListCLoggedAlertForAffectedUserRepository, CcvwComboListCLoggedAlertForAffectedUserRepository>();
        services.AddScoped<ICcvwComboListCLoggedLoginRepository, CcvwComboListCLoggedLoginRepository>();
        services.AddScoped<ICcvwComboListCLookupRepository, CcvwComboListCLookupRepository>();
        services.AddScoped<ICcvwComboListCMfaRepository, CcvwComboListCMfaRepository>();
        services.AddScoped<ICcvwComboListCObjectToTranslateRepository, CcvwComboListCObjectToTranslateRepository>();
        services.AddScoped<ICcvwComboListCProcessRepository, CcvwComboListCProcessRepository>();
        services.AddScoped<ICcvwComboListCRoleRepository, CcvwComboListCRoleRepository>();
        services.AddScoped<ICcvwComboListCSystemDefaultRepository, CcvwComboListCSystemDefaultRepository>();
        services.AddScoped<ICcvwComboListCTableRepository, CcvwComboListCTableRepository>();
        services.AddScoped<ICcvwComboListCUserRepository, CcvwComboListCUserRepository>();
        services.AddScoped<ICcvwComboListCardRepository, CcvwComboListCardRepository>();
        services.AddScoped<ICcvwComboListCardActivationRepository, CcvwComboListCardActivationRepository>();
        services.AddScoped<ICcvwComboListCardActivationForCardRepository, CcvwComboListCardActivationForCardRepository>();
        services.AddScoped<ICcvwComboListCardActivationForCustomerRepository, CcvwComboListCardActivationForCustomerRepository>();
        services.AddScoped<ICcvwComboListCardActivationForDistributorRepository, CcvwComboListCardActivationForDistributorRepository>();
        services.AddScoped<ICcvwComboListCardClubRepository, CcvwComboListCardClubRepository>();
        services.AddScoped<ICcvwComboListCardClubForAssignedCompanyCustomerRepository, CcvwComboListCardClubForAssignedCompanyCustomerRepository>();
        services.AddScoped<ICcvwComboListCardForCustomerRepository, CcvwComboListCardForCustomerRepository>();
        services.AddScoped<ICcvwComboListCardForOwningDistributorRepository, CcvwComboListCardForOwningDistributorRepository>();
        services.AddScoped<ICcvwComboListCardForPreviousDistributorRepository, CcvwComboListCardForPreviousDistributorRepository>();
        services.AddScoped<ICcvwComboListCardOrderByDistributorRepository, CcvwComboListCardOrderByDistributorRepository>();
        services.AddScoped<ICcvwComboListCardOrderByDistributorForDistributorRepository, CcvwComboListCardOrderByDistributorForDistributorRepository>();
        services.AddScoped<ICcvwComboListCardOrderByDistributorForOrderedByUserRepository, CcvwComboListCardOrderByDistributorForOrderedByUserRepository>();
        services.AddScoped<ICcvwComboListCardOrderFromIssuerRepository, CcvwComboListCardOrderFromIssuerRepository>();
        services.AddScoped<ICcvwComboListCardOrderFromIssuerForOrderedByUserRepository, CcvwComboListCardOrderFromIssuerForOrderedByUserRepository>();
        services.AddScoped<ICcvwComboListCountryRepository, CcvwComboListCountryRepository>();
        services.AddScoped<ICcvwComboListCustomerRepository, CcvwComboListCustomerRepository>();
        services.AddScoped<ICcvwComboListCustomerCardTransactionRepository, CcvwComboListCustomerCardTransactionRepository>();
        services.AddScoped<ICcvwComboListCustomerCardTransactionForCardRepository, CcvwComboListCustomerCardTransactionForCardRepository>();
        services.AddScoped<ICcvwComboListCustomerCardTransactionForCustomerRepository, CcvwComboListCustomerCardTransactionForCustomerRepository>();
        services.AddScoped<ICcvwComboListCustomerCardTransactionForlgOwningDistributorRepository, CcvwComboListCustomerCardTransactionForlgOwningDistributorRepository>();
        services.AddScoped<ICcvwComboListCustomerClubRepository, CcvwComboListCustomerClubRepository>();
        services.AddScoped<ICcvwComboListCustomerCreditCardRepository, CcvwComboListCustomerCreditCardRepository>();
        services.AddScoped<ICcvwComboListCustomerCreditCardForCustomerRepository, CcvwComboListCustomerCreditCardForCustomerRepository>();
        services.AddScoped<ICcvwComboListCustomerForAssignedDistributorRepository, CcvwComboListCustomerForAssignedDistributorRepository>();
        services.AddScoped<ICcvwComboListCustomerPartnershipRepository, CcvwComboListCustomerPartnershipRepository>();
        services.AddScoped<ICcvwComboListCustomerPartnershipForCustomerRepository, CcvwComboListCustomerPartnershipForCustomerRepository>();
        services.AddScoped<ICcvwComboListCustomerWalletRepository, CcvwComboListCustomerWalletRepository>();
        services.AddScoped<ICcvwComboListCustomerWalletForCustomerRepository, CcvwComboListCustomerWalletForCustomerRepository>();
        services.AddScoped<ICcvwComboListCustomerWalletForWhoInitiatedRepository, CcvwComboListCustomerWalletForWhoInitiatedRepository>();
        services.AddScoped<ICcvwComboListDistributorRepository, CcvwComboListDistributorRepository>();
        services.AddScoped<ICcvwComboListEntityRiskColorRepository, CcvwComboListEntityRiskColorRepository>();
        services.AddScoped<ICcvwComboListFeeRepository, CcvwComboListFeeRepository>();
        services.AddScoped<ICcvwComboListGovtReport00Repository, CcvwComboListGovtReport00Repository>();
        services.AddScoped<ICcvwComboListImportTransactionLoadRepository, CcvwComboListImportTransactionLoadRepository>();
        services.AddScoped<ICcvwComboListImportTransactionLoadForDistributorRepository, CcvwComboListImportTransactionLoadForDistributorRepository>();
        services.AddScoped<ICcvwComboListImportTransactionLoadForInitiatedByUserRepository, CcvwComboListImportTransactionLoadForInitiatedByUserRepository>();
        services.AddScoped<ICcvwComboListLimitationRepository, CcvwComboListLimitationRepository>();
        services.AddScoped<ICcvwComboListMCCBReceiverRepository, CcvwComboListMCCBReceiverRepository>();
        services.AddScoped<ICcvwComboListMCCBReceiverAccountRepository, CcvwComboListMCCBReceiverAccountRepository>();
        services.AddScoped<ICcvwComboListMCCBReceiverForCustomerRepository, CcvwComboListMCCBReceiverForCustomerRepository>();
        services.AddScoped<ICcvwComboListMCCBTransactionRepository, CcvwComboListMCCBTransactionRepository>();
        services.AddScoped<ICcvwComboListMCCBTransactionForCustomerRepository, CcvwComboListMCCBTransactionForCustomerRepository>();
        services.AddScoped<ICcvwComboListNewCardRequestRepository, CcvwComboListNewCardRequestRepository>();
        services.AddScoped<ICcvwComboListNewCardRequestAPIRepository, CcvwComboListNewCardRequestAPIRepository>();
        services.AddScoped<ICcvwComboListNewCardRequestAPIForDistributorRepository, CcvwComboListNewCardRequestAPIForDistributorRepository>();
        services.AddScoped<ICcvwComboListNewCardRequestForCustomerRepository, CcvwComboListNewCardRequestForCustomerRepository>();
        services.AddScoped<ICcvwComboListNewCardRequestForDistributorSentToRepository, CcvwComboListNewCardRequestForDistributorSentToRepository>();
        services.AddScoped<ICcvwComboListNonCustomerRepository, CcvwComboListNonCustomerRepository>();
        services.AddScoped<ICcvwComboListRequestedTransactionRepository, CcvwComboListRequestedTransactionRepository>();
        services.AddScoped<ICcvwComboListRequestedTransactionForDistributorRepository, CcvwComboListRequestedTransactionForDistributorRepository>();
        services.AddScoped<ICcvwComboListRequestedTransactionForInitiatingCustomerRepository, CcvwComboListRequestedTransactionForInitiatingCustomerRepository>();
        services.AddScoped<ICcvwComboListRequestedTransactionForPayingCustomerRepository, CcvwComboListRequestedTransactionForPayingCustomerRepository>();
        services.AddScoped<ICcvwComboListRequestedTransactionForPrepaidCardRepository, CcvwComboListRequestedTransactionForPrepaidCardRepository>();
        services.AddScoped<ICcvwComboListRequestedTransactionForReceivingCustomerRepository, CcvwComboListRequestedTransactionForReceivingCustomerRepository>();
        services.AddScoped<ICcvwComboListRequestedTransactionForTargetCustomerRepository, CcvwComboListRequestedTransactionForTargetCustomerRepository>();
        services.AddScoped<ICcvwComboListRiskEventRepository, CcvwComboListRiskEventRepository>();
        services.AddScoped<ICcvwComboListRiskRuleRepository, CcvwComboListRiskRuleRepository>();
        services.AddScoped<ICcvwComboListRiskRuleLogRepository, CcvwComboListRiskRuleLogRepository>();
        services.AddScoped<ICcvwComboListRiskRuleOverrideRepository, CcvwComboListRiskRuleOverrideRepository>();
        services.AddScoped<ICcvwComboListTransactionBitRepository, CcvwComboListTransactionBitRepository>();
        services.AddScoped<ICcvwComboListTransactionBitForCustomerRepository, CcvwComboListTransactionBitForCustomerRepository>();
        services.AddScoped<ICcvwComboListTransactionCreditCardRepository, CcvwComboListTransactionCreditCardRepository>();
        services.AddScoped<ICcvwComboListTransactionCreditCardForCustomerRepository, CcvwComboListTransactionCreditCardForCustomerRepository>();
        services.AddScoped<ICcvwComboListTransactionLoadRepository, CcvwComboListTransactionLoadRepository>();
        services.AddScoped<ICcvwComboListTransactionLoadAccountRepository, CcvwComboListTransactionLoadAccountRepository>();
        services.AddScoped<ICcvwComboListTransactionLoadForCardRepository, CcvwComboListTransactionLoadForCardRepository>();
        services.AddScoped<ICcvwComboListTransactionLoadForCustomerRepository, CcvwComboListTransactionLoadForCustomerRepository>();
        services.AddScoped<ICcvwComboListTransactionLoadForDistributorRepository, CcvwComboListTransactionLoadForDistributorRepository>();
        services.AddScoped<ICcvwComboListTransactionLoadForUserRepository, CcvwComboListTransactionLoadForUserRepository>();
        services.AddScoped<ICcvwComboListUITextRepository, CcvwComboListUITextRepository>();
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
        services.AddScoped<IMnccvwComboListCardForCompanyCustomerRepository, MnccvwComboListCardForCompanyCustomerRepository>();
        services.AddScoped<IMnccvwComboListCardForCompanyCustomerDistributorRepository, MnccvwComboListCardForCompanyCustomerDistributorRepository>();
        services.AddScoped<IMnvwComboListCustomerWithPhoneAndIdentifierRepository, MnvwComboListCustomerWithPhoneAndIdentifierRepository>();
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
        services.AddScoped<IvwCustomerCardStatusRepository, vwCustomerCardStatusRepository>();
        services.AddScoped<IvwCustomerCardStatusDataRepository, vwCustomerCardStatusDataRepository>();
        services.AddScoped<IvwCustomerSecurityStatusCheckRepository, vwCustomerSecurityStatusCheckRepository>();
        services.AddScoped<IvwCustomerSecurityStatusCheckDataRepository, vwCustomerSecurityStatusCheckDataRepository>();
        services.AddScoped<IvwOnlineCardTransactionRepository, vwOnlineCardTransactionRepository>();
        services.AddScoped<IvwReportCardActivityRepository, vwReportCardActivityRepository>();
        services.AddScoped<IvwReportCardActivityDataRepository, vwReportCardActivityDataRepository>();
        services.AddScoped<IvwReportCustomerBankTransferRepository, vwReportCustomerBankTransferRepository>();
        services.AddScoped<IvwReportCustomerBankTransferDataRepository, vwReportCustomerBankTransferDataRepository>();
        services.AddScoped<IvwReportCustomerCreditCardTransactionsRepository, vwReportCustomerCreditCardTransactionsRepository>();
        services.AddScoped<IvwReportCustomerCreditCardTransactionsDataRepository, vwReportCustomerCreditCardTransactionsDataRepository>();
        services.AddScoped<IvwReportCustomerWalletBalanceRepository, vwReportCustomerWalletBalanceRepository>();
        services.AddScoped<IvwReportCustomerWalletBalanceDataRepository, vwReportCustomerWalletBalanceDataRepository>();
        services.AddScoped<IvwReportCustomerWalletBalanceForLawyerRepository, vwReportCustomerWalletBalanceForLawyerRepository>();
        services.AddScoped<IvwReportCustomerWalletBalanceForLawyerDataRepository, vwReportCustomerWalletBalanceForLawyerDataRepository>();
        services.AddScoped<IvwReportDistributorObligoBalanceRepository, vwReportDistributorObligoBalanceRepository>();
        services.AddScoped<IvwReportDistributorObligoBalanceDataRepository, vwReportDistributorObligoBalanceDataRepository>();
        services.AddScoped<IvwReportFromChangerRepository, vwReportFromChangerRepository>();
        services.AddScoped<IvwReportFromChangerDataRepository, vwReportFromChangerDataRepository>();
        services.AddScoped<IvwReportLoadTransferToDistributorRepository, vwReportLoadTransferToDistributorRepository>();
        services.AddScoped<IvwReportLoadTransferToDistributorDataRepository, vwReportLoadTransferToDistributorDataRepository>();
        services.AddScoped<IvwReportMonthlyCardFeeRepository, vwReportMonthlyCardFeeRepository>();
        services.AddScoped<IvwReportMonthlyCardFeeDataRepository, vwReportMonthlyCardFeeDataRepository>();
        services.AddScoped<IvwReportToChangerRepository, vwReportToChangerRepository>();
        services.AddScoped<IvwReportToChangerDataRepository, vwReportToChangerDataRepository>();
        services.AddScoped<IvwReportWalletTransferRepository, vwReportWalletTransferRepository>();
        services.AddScoped<IvwReportWalletTransferDataRepository, vwReportWalletTransferDataRepository>();

        return services;
    }
}
