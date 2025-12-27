import { Routes, Route, Link } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Container, Box, Drawer, List, ListItem, ListItemButton, ListItemText } from '@mui/material';
import { Dashboard } from './components/Dashboard/Dashboard';
import { Report2List } from './components/Report2/Report2List';
import { Report2Detail } from './components/Report2/Report2Detail';
import { Report2Form } from './components/Report2/Report2Form';
import { ActiveRestrictionsList } from './components/ActiveRestrictions/ActiveRestrictionsList';
import { ActiveRestrictionsDetail } from './components/ActiveRestrictions/ActiveRestrictionsDetail';
import { ActiveRestrictionsForm } from './components/ActiveRestrictions/ActiveRestrictionsForm';
import { BackofficeTransactionList } from './components/BackofficeTransaction/BackofficeTransactionList';
import { BackofficeTransactionDetail } from './components/BackofficeTransaction/BackofficeTransactionDetail';
import { BackofficeTransactionForm } from './components/BackofficeTransaction/BackofficeTransactionForm';
import { AlertMessageList } from './components/AlertMessage/AlertMessageList';
import { AlertMessageDetail } from './components/AlertMessage/AlertMessageDetail';
import { AlertMessageForm } from './components/AlertMessage/AlertMessageForm';
import { AuditIndexedList } from './components/AuditIndexed/AuditIndexedList';
import { AuditIndexedDetail } from './components/AuditIndexed/AuditIndexedDetail';
import { AuditIndexedForm } from './components/AuditIndexed/AuditIndexedForm';
import { EnumerationList } from './components/Enumeration/EnumerationList';
import { EnumerationDetail } from './components/Enumeration/EnumerationDetail';
import { EnumerationForm } from './components/Enumeration/EnumerationForm';
import { JobList } from './components/Job/JobList';
import { JobDetail } from './components/Job/JobDetail';
import { JobForm } from './components/Job/JobForm';
import { JobAlertRecipientList } from './components/JobAlertRecipient/JobAlertRecipientList';
import { JobAlertRecipientDetail } from './components/JobAlertRecipient/JobAlertRecipientDetail';
import { JobAlertRecipientForm } from './components/JobAlertRecipient/JobAlertRecipientForm';
import { LanguageList } from './components/Language/LanguageList';
import { LanguageDetail } from './components/Language/LanguageDetail';
import { LanguageForm } from './components/Language/LanguageForm';
import { LoggedAlertList } from './components/LoggedAlert/LoggedAlertList';
import { LoggedAlertDetail } from './components/LoggedAlert/LoggedAlertDetail';
import { LoggedAlertForm } from './components/LoggedAlert/LoggedAlertForm';
import { LoggedJobList } from './components/LoggedJob/LoggedJobList';
import { LoggedJobDetail } from './components/LoggedJob/LoggedJobDetail';
import { LoggedJobForm } from './components/LoggedJob/LoggedJobForm';
import { LoggedLoginList } from './components/LoggedLogin/LoggedLoginList';
import { LoggedLoginDetail } from './components/LoggedLogin/LoggedLoginDetail';
import { LoggedLoginForm } from './components/LoggedLogin/LoggedLoginForm';
import { LoggedRequestList } from './components/LoggedRequest/LoggedRequestList';
import { LoggedRequestDetail } from './components/LoggedRequest/LoggedRequestDetail';
import { LoggedRequestForm } from './components/LoggedRequest/LoggedRequestForm';
import { LookupList } from './components/Lookup/LookupList';
import { LookupDetail } from './components/Lookup/LookupDetail';
import { LookupForm } from './components/Lookup/LookupForm';
import { MailList } from './components/Mail/MailList';
import { MailDetail } from './components/Mail/MailDetail';
import { MailForm } from './components/Mail/MailForm';
import { MfaList } from './components/Mfa/MfaList';
import { MfaDetail } from './components/Mfa/MfaDetail';
import { MfaForm } from './components/Mfa/MfaForm';
import { ObjectToTranslateList } from './components/ObjectToTranslate/ObjectToTranslateList';
import { ObjectToTranslateDetail } from './components/ObjectToTranslate/ObjectToTranslateDetail';
import { ObjectToTranslateForm } from './components/ObjectToTranslate/ObjectToTranslateForm';
import { ObjectTranslationList } from './components/ObjectTranslation/ObjectTranslationList';
import { ObjectTranslationDetail } from './components/ObjectTranslation/ObjectTranslationDetail';
import { ObjectTranslationForm } from './components/ObjectTranslation/ObjectTranslationForm';
import { PermissionList } from './components/Permission/PermissionList';
import { PermissionDetail } from './components/Permission/PermissionDetail';
import { PermissionForm } from './components/Permission/PermissionForm';
import { ProcessList } from './components/Process/ProcessList';
import { ProcessDetail } from './components/Process/ProcessDetail';
import { ProcessForm } from './components/Process/ProcessForm';
import { RoleList } from './components/Role/RoleList';
import { RoleDetail } from './components/Role/RoleDetail';
import { RoleForm } from './components/Role/RoleForm';
import { SystemAuditList } from './components/SystemAudit/SystemAuditList';
import { SystemAuditDetail } from './components/SystemAudit/SystemAuditDetail';
import { SystemAuditForm } from './components/SystemAudit/SystemAuditForm';
import { SystemDefaultList } from './components/SystemDefault/SystemDefaultList';
import { SystemDefaultDetail } from './components/SystemDefault/SystemDefaultDetail';
import { SystemDefaultForm } from './components/SystemDefault/SystemDefaultForm';
import { TableList } from './components/Table/TableList';
import { TableDetail } from './components/Table/TableDetail';
import { TableForm } from './components/Table/TableForm';
import { TableFieldsList } from './components/TableFields/TableFieldsList';
import { TableFieldsDetail } from './components/TableFields/TableFieldsDetail';
import { TableFieldsForm } from './components/TableFields/TableFieldsForm';
import { UserList } from './components/User/UserList';
import { UserDetail } from './components/User/UserDetail';
import { UserForm } from './components/User/UserForm';
import { UserLoginKeyList } from './components/UserLoginKey/UserLoginKeyList';
import { UserLoginKeyDetail } from './components/UserLoginKey/UserLoginKeyDetail';
import { UserLoginKeyForm } from './components/UserLoginKey/UserLoginKeyForm';
import { UserPermissionList } from './components/UserPermission/UserPermissionList';
import { UserPermissionDetail } from './components/UserPermission/UserPermissionDetail';
import { UserPermissionForm } from './components/UserPermission/UserPermissionForm';
import { UserStatusList } from './components/UserStatus/UserStatusList';
import { UserStatusDetail } from './components/UserStatus/UserStatusDetail';
import { UserStatusForm } from './components/UserStatus/UserStatusForm';
import { CardList } from './components/Card/CardList';
import { CardDetail } from './components/Card/CardDetail';
import { CardForm } from './components/Card/CardForm';
import { CardActivationList } from './components/CardActivation/CardActivationList';
import { CardActivationDetail } from './components/CardActivation/CardActivationDetail';
import { CardActivationForm } from './components/CardActivation/CardActivationForm';
import { CardClubList } from './components/CardClub/CardClubList';
import { CardClubDetail } from './components/CardClub/CardClubDetail';
import { CardClubForm } from './components/CardClub/CardClubForm';
import { CardIssuerList } from './components/CardIssuer/CardIssuerList';
import { CardIssuerDetail } from './components/CardIssuer/CardIssuerDetail';
import { CardIssuerForm } from './components/CardIssuer/CardIssuerForm';
import { CardOrderByDistributorList } from './components/CardOrderByDistributor/CardOrderByDistributorList';
import { CardOrderByDistributorDetail } from './components/CardOrderByDistributor/CardOrderByDistributorDetail';
import { CardOrderByDistributorForm } from './components/CardOrderByDistributor/CardOrderByDistributorForm';
import { CardOrderFromIssuerList } from './components/CardOrderFromIssuer/CardOrderFromIssuerList';
import { CardOrderFromIssuerDetail } from './components/CardOrderFromIssuer/CardOrderFromIssuerDetail';
import { CardOrderFromIssuerForm } from './components/CardOrderFromIssuer/CardOrderFromIssuerForm';
import { CountryList } from './components/Country/CountryList';
import { CountryDetail } from './components/Country/CountryDetail';
import { CountryForm } from './components/Country/CountryForm';
import { CreditGuardList } from './components/CreditGuard/CreditGuardList';
import { CreditGuardDetail } from './components/CreditGuard/CreditGuardDetail';
import { CreditGuardForm } from './components/CreditGuard/CreditGuardForm';
import { CustomerList } from './components/Customer/CustomerList';
import { CustomerDetail } from './components/Customer/CustomerDetail';
import { CustomerForm } from './components/Customer/CustomerForm';
import { CustomerCardTransactionList } from './components/CustomerCardTransaction/CustomerCardTransactionList';
import { CustomerCardTransactionDetail } from './components/CustomerCardTransaction/CustomerCardTransactionDetail';
import { CustomerCardTransactionForm } from './components/CustomerCardTransaction/CustomerCardTransactionForm';
import { CustomerCardTransactionDefinitionList } from './components/CustomerCardTransactionDefinition/CustomerCardTransactionDefinitionList';
import { CustomerCardTransactionDefinitionDetail } from './components/CustomerCardTransactionDefinition/CustomerCardTransactionDefinitionDetail';
import { CustomerCardTransactionDefinitionForm } from './components/CustomerCardTransactionDefinition/CustomerCardTransactionDefinitionForm';
import { CustomerClubList } from './components/CustomerClub/CustomerClubList';
import { CustomerClubDetail } from './components/CustomerClub/CustomerClubDetail';
import { CustomerClubForm } from './components/CustomerClub/CustomerClubForm';
import { CustomerCreditCardList } from './components/CustomerCreditCard/CustomerCreditCardList';
import { CustomerCreditCardDetail } from './components/CustomerCreditCard/CustomerCreditCardDetail';
import { CustomerCreditCardForm } from './components/CustomerCreditCard/CustomerCreditCardForm';
import { CustomerKYCList } from './components/CustomerKYC/CustomerKYCList';
import { CustomerKYCDetail } from './components/CustomerKYC/CustomerKYCDetail';
import { CustomerKYCForm } from './components/CustomerKYC/CustomerKYCForm';
import { CustomerPartnershipList } from './components/CustomerPartnership/CustomerPartnershipList';
import { CustomerPartnershipDetail } from './components/CustomerPartnership/CustomerPartnershipDetail';
import { CustomerPartnershipForm } from './components/CustomerPartnership/CustomerPartnershipForm';
import { CustomerPerformanceList } from './components/CustomerPerformance/CustomerPerformanceList';
import { CustomerPerformanceDetail } from './components/CustomerPerformance/CustomerPerformanceDetail';
import { CustomerPerformanceForm } from './components/CustomerPerformance/CustomerPerformanceForm';
import { CustomerWalletList } from './components/CustomerWallet/CustomerWalletList';
import { CustomerWalletDetail } from './components/CustomerWallet/CustomerWalletDetail';
import { CustomerWalletForm } from './components/CustomerWallet/CustomerWalletForm';
import { DistributorList } from './components/Distributor/DistributorList';
import { DistributorDetail } from './components/Distributor/DistributorDetail';
import { DistributorForm } from './components/Distributor/DistributorForm';
import { DistributorBranchList } from './components/DistributorBranch/DistributorBranchList';
import { DistributorBranchDetail } from './components/DistributorBranch/DistributorBranchDetail';
import { DistributorBranchForm } from './components/DistributorBranch/DistributorBranchForm';
import { DistributorObligoList } from './components/DistributorObligo/DistributorObligoList';
import { DistributorObligoDetail } from './components/DistributorObligo/DistributorObligoDetail';
import { DistributorObligoForm } from './components/DistributorObligo/DistributorObligoForm';
import { DrivebotPaymentList } from './components/DrivebotPayment/DrivebotPaymentList';
import { DrivebotPaymentDetail } from './components/DrivebotPayment/DrivebotPaymentDetail';
import { DrivebotPaymentForm } from './components/DrivebotPayment/DrivebotPaymentForm';
import { EntityRiskColorList } from './components/EntityRiskColor/EntityRiskColorList';
import { EntityRiskColorDetail } from './components/EntityRiskColor/EntityRiskColorDetail';
import { EntityRiskColorForm } from './components/EntityRiskColor/EntityRiskColorForm';
import { ExchangeRateList } from './components/ExchangeRate/ExchangeRateList';
import { ExchangeRateDetail } from './components/ExchangeRate/ExchangeRateDetail';
import { ExchangeRateForm } from './components/ExchangeRate/ExchangeRateForm';
import { ExternalCustomerList } from './components/ExternalCustomer/ExternalCustomerList';
import { ExternalCustomerDetail } from './components/ExternalCustomer/ExternalCustomerDetail';
import { ExternalCustomerForm } from './components/ExternalCustomer/ExternalCustomerForm';
import { ExternalTransactionList } from './components/ExternalTransaction/ExternalTransactionList';
import { ExternalTransactionDetail } from './components/ExternalTransaction/ExternalTransactionDetail';
import { ExternalTransactionForm } from './components/ExternalTransaction/ExternalTransactionForm';
import { FeeList } from './components/Fee/FeeList';
import { FeeDetail } from './components/Fee/FeeDetail';
import { FeeForm } from './components/Fee/FeeForm';
import { FeeOverrideList } from './components/FeeOverride/FeeOverrideList';
import { FeeOverrideDetail } from './components/FeeOverride/FeeOverrideDetail';
import { FeeOverrideForm } from './components/FeeOverride/FeeOverrideForm';
import { GovtReport00List } from './components/GovtReport00/GovtReport00List';
import { GovtReport00Detail } from './components/GovtReport00/GovtReport00Detail';
import { GovtReport00Form } from './components/GovtReport00/GovtReport00Form';
import { GovtReport10List } from './components/GovtReport10/GovtReport10List';
import { GovtReport10Detail } from './components/GovtReport10/GovtReport10Detail';
import { GovtReport10Form } from './components/GovtReport10/GovtReport10Form';
import { GovtReport30List } from './components/GovtReport30/GovtReport30List';
import { GovtReport30Detail } from './components/GovtReport30/GovtReport30Detail';
import { GovtReport30Form } from './components/GovtReport30/GovtReport30Form';
import { GovtReport40List } from './components/GovtReport40/GovtReport40List';
import { GovtReport40Detail } from './components/GovtReport40/GovtReport40Detail';
import { GovtReport40Form } from './components/GovtReport40/GovtReport40Form';
import { GovtReport99List } from './components/GovtReport99/GovtReport99List';
import { GovtReport99Detail } from './components/GovtReport99/GovtReport99Detail';
import { GovtReport99Form } from './components/GovtReport99/GovtReport99Form';
import { GovtReportDefinitionList } from './components/GovtReportDefinition/GovtReportDefinitionList';
import { GovtReportDefinitionDetail } from './components/GovtReportDefinition/GovtReportDefinitionDetail';
import { GovtReportDefinitionForm } from './components/GovtReportDefinition/GovtReportDefinitionForm';
import { GovtReportDistributorList } from './components/GovtReportDistributor/GovtReportDistributorList';
import { GovtReportDistributorDetail } from './components/GovtReportDistributor/GovtReportDistributorDetail';
import { GovtReportDistributorForm } from './components/GovtReportDistributor/GovtReportDistributorForm';
import { GovtReportPaymentList } from './components/GovtReportPayment/GovtReportPaymentList';
import { GovtReportPaymentDetail } from './components/GovtReportPayment/GovtReportPaymentDetail';
import { GovtReportPaymentForm } from './components/GovtReportPayment/GovtReportPaymentForm';
import { ImportTransactionLoadList } from './components/ImportTransactionLoad/ImportTransactionLoadList';
import { ImportTransactionLoadDetail } from './components/ImportTransactionLoad/ImportTransactionLoadDetail';
import { ImportTransactionLoadForm } from './components/ImportTransactionLoad/ImportTransactionLoadForm';
import { ImportTransactionLoadRowList } from './components/ImportTransactionLoadRow/ImportTransactionLoadRowList';
import { ImportTransactionLoadRowDetail } from './components/ImportTransactionLoadRow/ImportTransactionLoadRowDetail';
import { ImportTransactionLoadRowForm } from './components/ImportTransactionLoadRow/ImportTransactionLoadRowForm';
import { InvoiceList } from './components/Invoice/InvoiceList';
import { InvoiceDetail } from './components/Invoice/InvoiceDetail';
import { InvoiceForm } from './components/Invoice/InvoiceForm';
import { LeadList } from './components/Lead/LeadList';
import { LeadDetail } from './components/Lead/LeadDetail';
import { LeadForm } from './components/Lead/LeadForm';
import { LimitationList } from './components/Limitation/LimitationList';
import { LimitationDetail } from './components/Limitation/LimitationDetail';
import { LimitationForm } from './components/Limitation/LimitationForm';
import { LimitationOverrideList } from './components/LimitationOverride/LimitationOverrideList';
import { LimitationOverrideDetail } from './components/LimitationOverride/LimitationOverrideDetail';
import { LimitationOverrideForm } from './components/LimitationOverride/LimitationOverrideForm';
import { MCCBFPBankAccountList } from './components/MCCBFPBankAccount/MCCBFPBankAccountList';
import { MCCBFPBankAccountDetail } from './components/MCCBFPBankAccount/MCCBFPBankAccountDetail';
import { MCCBFPBankAccountForm } from './components/MCCBFPBankAccount/MCCBFPBankAccountForm';
import { MCCBFPCashOutList } from './components/MCCBFPCashOut/MCCBFPCashOutList';
import { MCCBFPCashOutDetail } from './components/MCCBFPCashOut/MCCBFPCashOutDetail';
import { MCCBFPCashOutForm } from './components/MCCBFPCashOut/MCCBFPCashOutForm';
import { MCCBFPMobileWalletList } from './components/MCCBFPMobileWallet/MCCBFPMobileWalletList';
import { MCCBFPMobileWalletDetail } from './components/MCCBFPMobileWallet/MCCBFPMobileWalletDetail';
import { MCCBFPMobileWalletForm } from './components/MCCBFPMobileWallet/MCCBFPMobileWalletForm';
import { MCCBFPPaymentCardList } from './components/MCCBFPPaymentCard/MCCBFPPaymentCardList';
import { MCCBFPPaymentCardDetail } from './components/MCCBFPPaymentCard/MCCBFPPaymentCardDetail';
import { MCCBFPPaymentCardForm } from './components/MCCBFPPaymentCard/MCCBFPPaymentCardForm';
import { MCCBParticipantList } from './components/MCCBParticipant/MCCBParticipantList';
import { MCCBParticipantDetail } from './components/MCCBParticipant/MCCBParticipantDetail';
import { MCCBParticipantForm } from './components/MCCBParticipant/MCCBParticipantForm';
import { MCCBReceiverList } from './components/MCCBReceiver/MCCBReceiverList';
import { MCCBReceiverDetail } from './components/MCCBReceiver/MCCBReceiverDetail';
import { MCCBReceiverForm } from './components/MCCBReceiver/MCCBReceiverForm';
import { MCCBReceiverAccountList } from './components/MCCBReceiverAccount/MCCBReceiverAccountList';
import { MCCBReceiverAccountDetail } from './components/MCCBReceiverAccount/MCCBReceiverAccountDetail';
import { MCCBReceiverAccountForm } from './components/MCCBReceiverAccount/MCCBReceiverAccountForm';
import { MCCBSecurityCheckList } from './components/MCCBSecurityCheck/MCCBSecurityCheckList';
import { MCCBSecurityCheckDetail } from './components/MCCBSecurityCheck/MCCBSecurityCheckDetail';
import { MCCBSecurityCheckForm } from './components/MCCBSecurityCheck/MCCBSecurityCheckForm';
import { MCCBTransactionList } from './components/MCCBTransaction/MCCBTransactionList';
import { MCCBTransactionDetail } from './components/MCCBTransaction/MCCBTransactionDetail';
import { MCCBTransactionForm } from './components/MCCBTransaction/MCCBTransactionForm';
import { NewCardRequestList } from './components/NewCardRequest/NewCardRequestList';
import { NewCardRequestDetail } from './components/NewCardRequest/NewCardRequestDetail';
import { NewCardRequestForm } from './components/NewCardRequest/NewCardRequestForm';
import { NewCardRequestAPIList } from './components/NewCardRequestAPI/NewCardRequestAPIList';
import { NewCardRequestAPIDetail } from './components/NewCardRequestAPI/NewCardRequestAPIDetail';
import { NewCardRequestAPIForm } from './components/NewCardRequestAPI/NewCardRequestAPIForm';
import { NonCustomerList } from './components/NonCustomer/NonCustomerList';
import { NonCustomerDetail } from './components/NonCustomer/NonCustomerDetail';
import { NonCustomerForm } from './components/NonCustomer/NonCustomerForm';
import { OneTimeLinkList } from './components/OneTimeLink/OneTimeLinkList';
import { OneTimeLinkDetail } from './components/OneTimeLink/OneTimeLinkDetail';
import { OneTimeLinkForm } from './components/OneTimeLink/OneTimeLinkForm';
import { OnlineApprovalList } from './components/OnlineApproval/OnlineApprovalList';
import { OnlineApprovalDetail } from './components/OnlineApproval/OnlineApprovalDetail';
import { OnlineApprovalForm } from './components/OnlineApproval/OnlineApprovalForm';
import { RequestedTransactionList } from './components/RequestedTransaction/RequestedTransactionList';
import { RequestedTransactionDetail } from './components/RequestedTransaction/RequestedTransactionDetail';
import { RequestedTransactionForm } from './components/RequestedTransaction/RequestedTransactionForm';
import { RequestedTransactionFeeList } from './components/RequestedTransactionFee/RequestedTransactionFeeList';
import { RequestedTransactionFeeDetail } from './components/RequestedTransactionFee/RequestedTransactionFeeDetail';
import { RequestedTransactionFeeForm } from './components/RequestedTransactionFee/RequestedTransactionFeeForm';
import { RiskEventList } from './components/RiskEvent/RiskEventList';
import { RiskEventDetail } from './components/RiskEvent/RiskEventDetail';
import { RiskEventForm } from './components/RiskEvent/RiskEventForm';
import { RiskRuleList } from './components/RiskRule/RiskRuleList';
import { RiskRuleDetail } from './components/RiskRule/RiskRuleDetail';
import { RiskRuleForm } from './components/RiskRule/RiskRuleForm';
import { RiskRuleLogList } from './components/RiskRuleLog/RiskRuleLogList';
import { RiskRuleLogDetail } from './components/RiskRuleLog/RiskRuleLogDetail';
import { RiskRuleLogForm } from './components/RiskRuleLog/RiskRuleLogForm';
import { RiskRuleOverrideList } from './components/RiskRuleOverride/RiskRuleOverrideList';
import { RiskRuleOverrideDetail } from './components/RiskRuleOverride/RiskRuleOverrideDetail';
import { RiskRuleOverrideForm } from './components/RiskRuleOverride/RiskRuleOverrideForm';
import { SapList } from './components/Sap/SapList';
import { SapDetail } from './components/Sap/SapDetail';
import { SapForm } from './components/Sap/SapForm';
import { SysdiagramsList } from './components/Sysdiagrams/SysdiagramsList';
import { SysdiagramsDetail } from './components/Sysdiagrams/SysdiagramsDetail';
import { SysdiagramsForm } from './components/Sysdiagrams/SysdiagramsForm';
import { TaskList } from './components/Task/TaskList';
import { TaskDetail } from './components/Task/TaskDetail';
import { TaskForm } from './components/Task/TaskForm';
import { TransactionBitList } from './components/TransactionBit/TransactionBitList';
import { TransactionBitDetail } from './components/TransactionBit/TransactionBitDetail';
import { TransactionBitForm } from './components/TransactionBit/TransactionBitForm';
import { TransactionCreditCardList } from './components/TransactionCreditCard/TransactionCreditCardList';
import { TransactionCreditCardDetail } from './components/TransactionCreditCard/TransactionCreditCardDetail';
import { TransactionCreditCardForm } from './components/TransactionCreditCard/TransactionCreditCardForm';
import { TransactionLoadList } from './components/TransactionLoad/TransactionLoadList';
import { TransactionLoadDetail } from './components/TransactionLoad/TransactionLoadDetail';
import { TransactionLoadForm } from './components/TransactionLoad/TransactionLoadForm';
import { TransactionLoadAccountList } from './components/TransactionLoadAccount/TransactionLoadAccountList';
import { TransactionLoadAccountDetail } from './components/TransactionLoadAccount/TransactionLoadAccountDetail';
import { TransactionLoadAccountForm } from './components/TransactionLoadAccount/TransactionLoadAccountForm';
import { UITextList } from './components/UIText/UITextList';
import { UITextDetail } from './components/UIText/UITextDetail';
import { UITextForm } from './components/UIText/UITextForm';
import { UserApprovalList } from './components/UserApproval/UserApprovalList';
import { UserApprovalDetail } from './components/UserApproval/UserApprovalDetail';
import { UserApprovalForm } from './components/UserApproval/UserApprovalForm';

const drawerWidth = 240;

function App() {
  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
        <Toolbar>
          <Typography variant="h6" noWrap component="div">
            Report2 Admin
          </Typography>
        </Toolbar>
      </AppBar>
      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          '& .MuiDrawer-paper': { width: drawerWidth, boxSizing: 'border-box' },
        }}
      >
        <Toolbar />
        <Box sx={{ overflow: 'auto' }}>
          <List>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/">
                <ListItemText primary="Dashboard" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/report2s">
                <ListItemText primary="Report2s" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/activeRestrictionss">
                <ListItemText primary="ActiveRestrictionss" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/backofficeTransactions">
                <ListItemText primary="BackofficeTransactions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/alertMessages">
                <ListItemText primary="AlertMessages" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/auditIndexeds">
                <ListItemText primary="AuditIndexeds" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/enumerations">
                <ListItemText primary="Enumerations" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/jobs">
                <ListItemText primary="Jobs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/jobAlertRecipients">
                <ListItemText primary="JobAlertRecipients" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/languages">
                <ListItemText primary="Languages" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/loggedAlerts">
                <ListItemText primary="LoggedAlerts" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/loggedJobs">
                <ListItemText primary="LoggedJobs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/loggedLogins">
                <ListItemText primary="LoggedLogins" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/loggedRequests">
                <ListItemText primary="LoggedRequests" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/lookups">
                <ListItemText primary="Lookups" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mails">
                <ListItemText primary="Mails" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mfas">
                <ListItemText primary="Mfas" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/objectToTranslates">
                <ListItemText primary="ObjectToTranslates" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/objectTranslations">
                <ListItemText primary="ObjectTranslations" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/permissions">
                <ListItemText primary="Permissions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/processs">
                <ListItemText primary="Processs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/roles">
                <ListItemText primary="Roles" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/systemAudits">
                <ListItemText primary="SystemAudits" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/systemDefaults">
                <ListItemText primary="SystemDefaults" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/tables">
                <ListItemText primary="Tables" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/tableFieldss">
                <ListItemText primary="TableFieldss" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/users">
                <ListItemText primary="Users" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/userLoginKeys">
                <ListItemText primary="UserLoginKeys" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/userPermissions">
                <ListItemText primary="UserPermissions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/userStatuss">
                <ListItemText primary="UserStatuss" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/cards">
                <ListItemText primary="Cards" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/cardActivations">
                <ListItemText primary="CardActivations" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/cardClubs">
                <ListItemText primary="CardClubs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/cardIssuers">
                <ListItemText primary="CardIssuers" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/cardOrderByDistributors">
                <ListItemText primary="CardOrderByDistributors" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/cardOrderFromIssuers">
                <ListItemText primary="CardOrderFromIssuers" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/countrys">
                <ListItemText primary="Countrys" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/creditGuards">
                <ListItemText primary="CreditGuards" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customers">
                <ListItemText primary="Customers" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customerCardTransactions">
                <ListItemText primary="CustomerCardTransactions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customerCardTransactionDefinitions">
                <ListItemText primary="CustomerCardTransactionDefinitions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customerClubs">
                <ListItemText primary="CustomerClubs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customerCreditCards">
                <ListItemText primary="CustomerCreditCards" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customerKYCs">
                <ListItemText primary="CustomerKYCs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customerPartnerships">
                <ListItemText primary="CustomerPartnerships" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customerPerformances">
                <ListItemText primary="CustomerPerformances" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/customerWallets">
                <ListItemText primary="CustomerWallets" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/distributors">
                <ListItemText primary="Distributors" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/distributorBranchs">
                <ListItemText primary="DistributorBranchs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/distributorObligos">
                <ListItemText primary="DistributorObligos" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/drivebotPayments">
                <ListItemText primary="DrivebotPayments" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/entityRiskColors">
                <ListItemText primary="EntityRiskColors" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/exchangeRates">
                <ListItemText primary="ExchangeRates" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/externalCustomers">
                <ListItemText primary="ExternalCustomers" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/externalTransactions">
                <ListItemText primary="ExternalTransactions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/fees">
                <ListItemText primary="Fees" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/feeOverrides">
                <ListItemText primary="FeeOverrides" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/govtReport00s">
                <ListItemText primary="GovtReport00s" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/govtReport10s">
                <ListItemText primary="GovtReport10s" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/govtReport30s">
                <ListItemText primary="GovtReport30s" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/govtReport40s">
                <ListItemText primary="GovtReport40s" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/govtReport99s">
                <ListItemText primary="GovtReport99s" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/govtReportDefinitions">
                <ListItemText primary="GovtReportDefinitions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/govtReportDistributors">
                <ListItemText primary="GovtReportDistributors" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/govtReportPayments">
                <ListItemText primary="GovtReportPayments" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/importTransactionLoads">
                <ListItemText primary="ImportTransactionLoads" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/importTransactionLoadRows">
                <ListItemText primary="ImportTransactionLoadRows" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/invoices">
                <ListItemText primary="Invoices" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/leads">
                <ListItemText primary="Leads" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/limitations">
                <ListItemText primary="Limitations" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/limitationOverrides">
                <ListItemText primary="LimitationOverrides" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBFPBankAccounts">
                <ListItemText primary="MCCBFPBankAccounts" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBFPCashOuts">
                <ListItemText primary="MCCBFPCashOuts" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBFPMobileWallets">
                <ListItemText primary="MCCBFPMobileWallets" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBFPPaymentCards">
                <ListItemText primary="MCCBFPPaymentCards" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBParticipants">
                <ListItemText primary="MCCBParticipants" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBReceivers">
                <ListItemText primary="MCCBReceivers" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBReceiverAccounts">
                <ListItemText primary="MCCBReceiverAccounts" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBSecurityChecks">
                <ListItemText primary="MCCBSecurityChecks" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mCCBTransactions">
                <ListItemText primary="MCCBTransactions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/newCardRequests">
                <ListItemText primary="NewCardRequests" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/newCardRequestAPIs">
                <ListItemText primary="NewCardRequestAPIs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/nonCustomers">
                <ListItemText primary="NonCustomers" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/oneTimeLinks">
                <ListItemText primary="OneTimeLinks" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/onlineApprovals">
                <ListItemText primary="OnlineApprovals" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/requestedTransactions">
                <ListItemText primary="RequestedTransactions" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/requestedTransactionFees">
                <ListItemText primary="RequestedTransactionFees" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/riskEvents">
                <ListItemText primary="RiskEvents" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/riskRules">
                <ListItemText primary="RiskRules" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/riskRuleLogs">
                <ListItemText primary="RiskRuleLogs" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/riskRuleOverrides">
                <ListItemText primary="RiskRuleOverrides" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/saps">
                <ListItemText primary="Saps" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/sysdiagramss">
                <ListItemText primary="Sysdiagramss" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/tasks">
                <ListItemText primary="Tasks" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/transactionBits">
                <ListItemText primary="TransactionBits" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/transactionCreditCards">
                <ListItemText primary="TransactionCreditCards" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/transactionLoads">
                <ListItemText primary="TransactionLoads" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/transactionLoadAccounts">
                <ListItemText primary="TransactionLoadAccounts" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/uITexts">
                <ListItemText primary="UITexts" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/userApprovals">
                <ListItemText primary="UserApprovals" />
              </ListItemButton>
            </ListItem>
          </List>
        </Box>
      </Drawer>
      <Box component="main" sx={{ flexGrow: 1, p: 3 }}>
        <Toolbar />
        <Container maxWidth="xl">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/report2s" element={<Report2List />} />
            <Route path="/report2s/new" element={<Report2Form />} />
            <Route path="/report2s/:id" element={<Report2Detail />} />
            <Route path="/report2s/:id/edit" element={<Report2Form />} />
            <Route path="/activeRestrictionss" element={<ActiveRestrictionsList />} />
            <Route path="/activeRestrictionss/new" element={<ActiveRestrictionsForm />} />
            <Route path="/activeRestrictionss/:id" element={<ActiveRestrictionsDetail />} />
            <Route path="/activeRestrictionss/:id/edit" element={<ActiveRestrictionsForm />} />
            <Route path="/backofficeTransactions" element={<BackofficeTransactionList />} />
            <Route path="/backofficeTransactions/new" element={<BackofficeTransactionForm />} />
            <Route path="/backofficeTransactions/:id" element={<BackofficeTransactionDetail />} />
            <Route path="/backofficeTransactions/:id/edit" element={<BackofficeTransactionForm />} />
            <Route path="/alertMessages" element={<AlertMessageList />} />
            <Route path="/alertMessages/new" element={<AlertMessageForm />} />
            <Route path="/alertMessages/:id" element={<AlertMessageDetail />} />
            <Route path="/alertMessages/:id/edit" element={<AlertMessageForm />} />
            <Route path="/auditIndexeds" element={<AuditIndexedList />} />
            <Route path="/auditIndexeds/new" element={<AuditIndexedForm />} />
            <Route path="/auditIndexeds/:id" element={<AuditIndexedDetail />} />
            <Route path="/auditIndexeds/:id/edit" element={<AuditIndexedForm />} />
            <Route path="/enumerations" element={<EnumerationList />} />
            <Route path="/enumerations/new" element={<EnumerationForm />} />
            <Route path="/enumerations/:id" element={<EnumerationDetail />} />
            <Route path="/enumerations/:id/edit" element={<EnumerationForm />} />
            <Route path="/jobs" element={<JobList />} />
            <Route path="/jobs/new" element={<JobForm />} />
            <Route path="/jobs/:id" element={<JobDetail />} />
            <Route path="/jobs/:id/edit" element={<JobForm />} />
            <Route path="/jobAlertRecipients" element={<JobAlertRecipientList />} />
            <Route path="/jobAlertRecipients/new" element={<JobAlertRecipientForm />} />
            <Route path="/jobAlertRecipients/:id" element={<JobAlertRecipientDetail />} />
            <Route path="/jobAlertRecipients/:id/edit" element={<JobAlertRecipientForm />} />
            <Route path="/languages" element={<LanguageList />} />
            <Route path="/languages/new" element={<LanguageForm />} />
            <Route path="/languages/:id" element={<LanguageDetail />} />
            <Route path="/languages/:id/edit" element={<LanguageForm />} />
            <Route path="/loggedAlerts" element={<LoggedAlertList />} />
            <Route path="/loggedAlerts/new" element={<LoggedAlertForm />} />
            <Route path="/loggedAlerts/:id" element={<LoggedAlertDetail />} />
            <Route path="/loggedAlerts/:id/edit" element={<LoggedAlertForm />} />
            <Route path="/loggedJobs" element={<LoggedJobList />} />
            <Route path="/loggedJobs/new" element={<LoggedJobForm />} />
            <Route path="/loggedJobs/:id" element={<LoggedJobDetail />} />
            <Route path="/loggedJobs/:id/edit" element={<LoggedJobForm />} />
            <Route path="/loggedLogins" element={<LoggedLoginList />} />
            <Route path="/loggedLogins/new" element={<LoggedLoginForm />} />
            <Route path="/loggedLogins/:id" element={<LoggedLoginDetail />} />
            <Route path="/loggedLogins/:id/edit" element={<LoggedLoginForm />} />
            <Route path="/loggedRequests" element={<LoggedRequestList />} />
            <Route path="/loggedRequests/new" element={<LoggedRequestForm />} />
            <Route path="/loggedRequests/:id" element={<LoggedRequestDetail />} />
            <Route path="/loggedRequests/:id/edit" element={<LoggedRequestForm />} />
            <Route path="/lookups" element={<LookupList />} />
            <Route path="/lookups/new" element={<LookupForm />} />
            <Route path="/lookups/:id" element={<LookupDetail />} />
            <Route path="/lookups/:id/edit" element={<LookupForm />} />
            <Route path="/mails" element={<MailList />} />
            <Route path="/mails/new" element={<MailForm />} />
            <Route path="/mails/:id" element={<MailDetail />} />
            <Route path="/mails/:id/edit" element={<MailForm />} />
            <Route path="/mfas" element={<MfaList />} />
            <Route path="/mfas/new" element={<MfaForm />} />
            <Route path="/mfas/:id" element={<MfaDetail />} />
            <Route path="/mfas/:id/edit" element={<MfaForm />} />
            <Route path="/objectToTranslates" element={<ObjectToTranslateList />} />
            <Route path="/objectToTranslates/new" element={<ObjectToTranslateForm />} />
            <Route path="/objectToTranslates/:id" element={<ObjectToTranslateDetail />} />
            <Route path="/objectToTranslates/:id/edit" element={<ObjectToTranslateForm />} />
            <Route path="/objectTranslations" element={<ObjectTranslationList />} />
            <Route path="/objectTranslations/new" element={<ObjectTranslationForm />} />
            <Route path="/objectTranslations/:id" element={<ObjectTranslationDetail />} />
            <Route path="/objectTranslations/:id/edit" element={<ObjectTranslationForm />} />
            <Route path="/permissions" element={<PermissionList />} />
            <Route path="/permissions/new" element={<PermissionForm />} />
            <Route path="/permissions/:id" element={<PermissionDetail />} />
            <Route path="/permissions/:id/edit" element={<PermissionForm />} />
            <Route path="/processs" element={<ProcessList />} />
            <Route path="/processs/new" element={<ProcessForm />} />
            <Route path="/processs/:id" element={<ProcessDetail />} />
            <Route path="/processs/:id/edit" element={<ProcessForm />} />
            <Route path="/roles" element={<RoleList />} />
            <Route path="/roles/new" element={<RoleForm />} />
            <Route path="/roles/:id" element={<RoleDetail />} />
            <Route path="/roles/:id/edit" element={<RoleForm />} />
            <Route path="/systemAudits" element={<SystemAuditList />} />
            <Route path="/systemAudits/new" element={<SystemAuditForm />} />
            <Route path="/systemAudits/:id" element={<SystemAuditDetail />} />
            <Route path="/systemAudits/:id/edit" element={<SystemAuditForm />} />
            <Route path="/systemDefaults" element={<SystemDefaultList />} />
            <Route path="/systemDefaults/new" element={<SystemDefaultForm />} />
            <Route path="/systemDefaults/:id" element={<SystemDefaultDetail />} />
            <Route path="/systemDefaults/:id/edit" element={<SystemDefaultForm />} />
            <Route path="/tables" element={<TableList />} />
            <Route path="/tables/new" element={<TableForm />} />
            <Route path="/tables/:id" element={<TableDetail />} />
            <Route path="/tables/:id/edit" element={<TableForm />} />
            <Route path="/tableFieldss" element={<TableFieldsList />} />
            <Route path="/tableFieldss/new" element={<TableFieldsForm />} />
            <Route path="/tableFieldss/:id" element={<TableFieldsDetail />} />
            <Route path="/tableFieldss/:id/edit" element={<TableFieldsForm />} />
            <Route path="/users" element={<UserList />} />
            <Route path="/users/new" element={<UserForm />} />
            <Route path="/users/:id" element={<UserDetail />} />
            <Route path="/users/:id/edit" element={<UserForm />} />
            <Route path="/userLoginKeys" element={<UserLoginKeyList />} />
            <Route path="/userLoginKeys/new" element={<UserLoginKeyForm />} />
            <Route path="/userLoginKeys/:id" element={<UserLoginKeyDetail />} />
            <Route path="/userLoginKeys/:id/edit" element={<UserLoginKeyForm />} />
            <Route path="/userPermissions" element={<UserPermissionList />} />
            <Route path="/userPermissions/new" element={<UserPermissionForm />} />
            <Route path="/userPermissions/:id" element={<UserPermissionDetail />} />
            <Route path="/userPermissions/:id/edit" element={<UserPermissionForm />} />
            <Route path="/userStatuss" element={<UserStatusList />} />
            <Route path="/userStatuss/new" element={<UserStatusForm />} />
            <Route path="/userStatuss/:id" element={<UserStatusDetail />} />
            <Route path="/userStatuss/:id/edit" element={<UserStatusForm />} />
            <Route path="/cards" element={<CardList />} />
            <Route path="/cards/new" element={<CardForm />} />
            <Route path="/cards/:id" element={<CardDetail />} />
            <Route path="/cards/:id/edit" element={<CardForm />} />
            <Route path="/cardActivations" element={<CardActivationList />} />
            <Route path="/cardActivations/new" element={<CardActivationForm />} />
            <Route path="/cardActivations/:id" element={<CardActivationDetail />} />
            <Route path="/cardActivations/:id/edit" element={<CardActivationForm />} />
            <Route path="/cardClubs" element={<CardClubList />} />
            <Route path="/cardClubs/new" element={<CardClubForm />} />
            <Route path="/cardClubs/:id" element={<CardClubDetail />} />
            <Route path="/cardClubs/:id/edit" element={<CardClubForm />} />
            <Route path="/cardIssuers" element={<CardIssuerList />} />
            <Route path="/cardIssuers/new" element={<CardIssuerForm />} />
            <Route path="/cardIssuers/:id" element={<CardIssuerDetail />} />
            <Route path="/cardIssuers/:id/edit" element={<CardIssuerForm />} />
            <Route path="/cardOrderByDistributors" element={<CardOrderByDistributorList />} />
            <Route path="/cardOrderByDistributors/new" element={<CardOrderByDistributorForm />} />
            <Route path="/cardOrderByDistributors/:id" element={<CardOrderByDistributorDetail />} />
            <Route path="/cardOrderByDistributors/:id/edit" element={<CardOrderByDistributorForm />} />
            <Route path="/cardOrderFromIssuers" element={<CardOrderFromIssuerList />} />
            <Route path="/cardOrderFromIssuers/new" element={<CardOrderFromIssuerForm />} />
            <Route path="/cardOrderFromIssuers/:id" element={<CardOrderFromIssuerDetail />} />
            <Route path="/cardOrderFromIssuers/:id/edit" element={<CardOrderFromIssuerForm />} />
            <Route path="/countrys" element={<CountryList />} />
            <Route path="/countrys/new" element={<CountryForm />} />
            <Route path="/countrys/:id" element={<CountryDetail />} />
            <Route path="/countrys/:id/edit" element={<CountryForm />} />
            <Route path="/creditGuards" element={<CreditGuardList />} />
            <Route path="/creditGuards/new" element={<CreditGuardForm />} />
            <Route path="/creditGuards/:id" element={<CreditGuardDetail />} />
            <Route path="/creditGuards/:id/edit" element={<CreditGuardForm />} />
            <Route path="/customers" element={<CustomerList />} />
            <Route path="/customers/new" element={<CustomerForm />} />
            <Route path="/customers/:id" element={<CustomerDetail />} />
            <Route path="/customers/:id/edit" element={<CustomerForm />} />
            <Route path="/customerCardTransactions" element={<CustomerCardTransactionList />} />
            <Route path="/customerCardTransactions/new" element={<CustomerCardTransactionForm />} />
            <Route path="/customerCardTransactions/:id" element={<CustomerCardTransactionDetail />} />
            <Route path="/customerCardTransactions/:id/edit" element={<CustomerCardTransactionForm />} />
            <Route path="/customerCardTransactionDefinitions" element={<CustomerCardTransactionDefinitionList />} />
            <Route path="/customerCardTransactionDefinitions/new" element={<CustomerCardTransactionDefinitionForm />} />
            <Route path="/customerCardTransactionDefinitions/:id" element={<CustomerCardTransactionDefinitionDetail />} />
            <Route path="/customerCardTransactionDefinitions/:id/edit" element={<CustomerCardTransactionDefinitionForm />} />
            <Route path="/customerClubs" element={<CustomerClubList />} />
            <Route path="/customerClubs/new" element={<CustomerClubForm />} />
            <Route path="/customerClubs/:id" element={<CustomerClubDetail />} />
            <Route path="/customerClubs/:id/edit" element={<CustomerClubForm />} />
            <Route path="/customerCreditCards" element={<CustomerCreditCardList />} />
            <Route path="/customerCreditCards/new" element={<CustomerCreditCardForm />} />
            <Route path="/customerCreditCards/:id" element={<CustomerCreditCardDetail />} />
            <Route path="/customerCreditCards/:id/edit" element={<CustomerCreditCardForm />} />
            <Route path="/customerKYCs" element={<CustomerKYCList />} />
            <Route path="/customerKYCs/new" element={<CustomerKYCForm />} />
            <Route path="/customerKYCs/:id" element={<CustomerKYCDetail />} />
            <Route path="/customerKYCs/:id/edit" element={<CustomerKYCForm />} />
            <Route path="/customerPartnerships" element={<CustomerPartnershipList />} />
            <Route path="/customerPartnerships/new" element={<CustomerPartnershipForm />} />
            <Route path="/customerPartnerships/:id" element={<CustomerPartnershipDetail />} />
            <Route path="/customerPartnerships/:id/edit" element={<CustomerPartnershipForm />} />
            <Route path="/customerPerformances" element={<CustomerPerformanceList />} />
            <Route path="/customerPerformances/new" element={<CustomerPerformanceForm />} />
            <Route path="/customerPerformances/:id" element={<CustomerPerformanceDetail />} />
            <Route path="/customerPerformances/:id/edit" element={<CustomerPerformanceForm />} />
            <Route path="/customerWallets" element={<CustomerWalletList />} />
            <Route path="/customerWallets/new" element={<CustomerWalletForm />} />
            <Route path="/customerWallets/:id" element={<CustomerWalletDetail />} />
            <Route path="/customerWallets/:id/edit" element={<CustomerWalletForm />} />
            <Route path="/distributors" element={<DistributorList />} />
            <Route path="/distributors/new" element={<DistributorForm />} />
            <Route path="/distributors/:id" element={<DistributorDetail />} />
            <Route path="/distributors/:id/edit" element={<DistributorForm />} />
            <Route path="/distributorBranchs" element={<DistributorBranchList />} />
            <Route path="/distributorBranchs/new" element={<DistributorBranchForm />} />
            <Route path="/distributorBranchs/:id" element={<DistributorBranchDetail />} />
            <Route path="/distributorBranchs/:id/edit" element={<DistributorBranchForm />} />
            <Route path="/distributorObligos" element={<DistributorObligoList />} />
            <Route path="/distributorObligos/new" element={<DistributorObligoForm />} />
            <Route path="/distributorObligos/:id" element={<DistributorObligoDetail />} />
            <Route path="/distributorObligos/:id/edit" element={<DistributorObligoForm />} />
            <Route path="/drivebotPayments" element={<DrivebotPaymentList />} />
            <Route path="/drivebotPayments/new" element={<DrivebotPaymentForm />} />
            <Route path="/drivebotPayments/:id" element={<DrivebotPaymentDetail />} />
            <Route path="/drivebotPayments/:id/edit" element={<DrivebotPaymentForm />} />
            <Route path="/entityRiskColors" element={<EntityRiskColorList />} />
            <Route path="/entityRiskColors/new" element={<EntityRiskColorForm />} />
            <Route path="/entityRiskColors/:id" element={<EntityRiskColorDetail />} />
            <Route path="/entityRiskColors/:id/edit" element={<EntityRiskColorForm />} />
            <Route path="/exchangeRates" element={<ExchangeRateList />} />
            <Route path="/exchangeRates/new" element={<ExchangeRateForm />} />
            <Route path="/exchangeRates/:id" element={<ExchangeRateDetail />} />
            <Route path="/exchangeRates/:id/edit" element={<ExchangeRateForm />} />
            <Route path="/externalCustomers" element={<ExternalCustomerList />} />
            <Route path="/externalCustomers/new" element={<ExternalCustomerForm />} />
            <Route path="/externalCustomers/:id" element={<ExternalCustomerDetail />} />
            <Route path="/externalCustomers/:id/edit" element={<ExternalCustomerForm />} />
            <Route path="/externalTransactions" element={<ExternalTransactionList />} />
            <Route path="/externalTransactions/new" element={<ExternalTransactionForm />} />
            <Route path="/externalTransactions/:id" element={<ExternalTransactionDetail />} />
            <Route path="/externalTransactions/:id/edit" element={<ExternalTransactionForm />} />
            <Route path="/fees" element={<FeeList />} />
            <Route path="/fees/new" element={<FeeForm />} />
            <Route path="/fees/:id" element={<FeeDetail />} />
            <Route path="/fees/:id/edit" element={<FeeForm />} />
            <Route path="/feeOverrides" element={<FeeOverrideList />} />
            <Route path="/feeOverrides/new" element={<FeeOverrideForm />} />
            <Route path="/feeOverrides/:id" element={<FeeOverrideDetail />} />
            <Route path="/feeOverrides/:id/edit" element={<FeeOverrideForm />} />
            <Route path="/govtReport00s" element={<GovtReport00List />} />
            <Route path="/govtReport00s/new" element={<GovtReport00Form />} />
            <Route path="/govtReport00s/:id" element={<GovtReport00Detail />} />
            <Route path="/govtReport00s/:id/edit" element={<GovtReport00Form />} />
            <Route path="/govtReport10s" element={<GovtReport10List />} />
            <Route path="/govtReport10s/new" element={<GovtReport10Form />} />
            <Route path="/govtReport10s/:id" element={<GovtReport10Detail />} />
            <Route path="/govtReport10s/:id/edit" element={<GovtReport10Form />} />
            <Route path="/govtReport30s" element={<GovtReport30List />} />
            <Route path="/govtReport30s/new" element={<GovtReport30Form />} />
            <Route path="/govtReport30s/:id" element={<GovtReport30Detail />} />
            <Route path="/govtReport30s/:id/edit" element={<GovtReport30Form />} />
            <Route path="/govtReport40s" element={<GovtReport40List />} />
            <Route path="/govtReport40s/new" element={<GovtReport40Form />} />
            <Route path="/govtReport40s/:id" element={<GovtReport40Detail />} />
            <Route path="/govtReport40s/:id/edit" element={<GovtReport40Form />} />
            <Route path="/govtReport99s" element={<GovtReport99List />} />
            <Route path="/govtReport99s/new" element={<GovtReport99Form />} />
            <Route path="/govtReport99s/:id" element={<GovtReport99Detail />} />
            <Route path="/govtReport99s/:id/edit" element={<GovtReport99Form />} />
            <Route path="/govtReportDefinitions" element={<GovtReportDefinitionList />} />
            <Route path="/govtReportDefinitions/new" element={<GovtReportDefinitionForm />} />
            <Route path="/govtReportDefinitions/:id" element={<GovtReportDefinitionDetail />} />
            <Route path="/govtReportDefinitions/:id/edit" element={<GovtReportDefinitionForm />} />
            <Route path="/govtReportDistributors" element={<GovtReportDistributorList />} />
            <Route path="/govtReportDistributors/new" element={<GovtReportDistributorForm />} />
            <Route path="/govtReportDistributors/:id" element={<GovtReportDistributorDetail />} />
            <Route path="/govtReportDistributors/:id/edit" element={<GovtReportDistributorForm />} />
            <Route path="/govtReportPayments" element={<GovtReportPaymentList />} />
            <Route path="/govtReportPayments/new" element={<GovtReportPaymentForm />} />
            <Route path="/govtReportPayments/:id" element={<GovtReportPaymentDetail />} />
            <Route path="/govtReportPayments/:id/edit" element={<GovtReportPaymentForm />} />
            <Route path="/importTransactionLoads" element={<ImportTransactionLoadList />} />
            <Route path="/importTransactionLoads/new" element={<ImportTransactionLoadForm />} />
            <Route path="/importTransactionLoads/:id" element={<ImportTransactionLoadDetail />} />
            <Route path="/importTransactionLoads/:id/edit" element={<ImportTransactionLoadForm />} />
            <Route path="/importTransactionLoadRows" element={<ImportTransactionLoadRowList />} />
            <Route path="/importTransactionLoadRows/new" element={<ImportTransactionLoadRowForm />} />
            <Route path="/importTransactionLoadRows/:id" element={<ImportTransactionLoadRowDetail />} />
            <Route path="/importTransactionLoadRows/:id/edit" element={<ImportTransactionLoadRowForm />} />
            <Route path="/invoices" element={<InvoiceList />} />
            <Route path="/invoices/new" element={<InvoiceForm />} />
            <Route path="/invoices/:id" element={<InvoiceDetail />} />
            <Route path="/invoices/:id/edit" element={<InvoiceForm />} />
            <Route path="/leads" element={<LeadList />} />
            <Route path="/leads/new" element={<LeadForm />} />
            <Route path="/leads/:id" element={<LeadDetail />} />
            <Route path="/leads/:id/edit" element={<LeadForm />} />
            <Route path="/limitations" element={<LimitationList />} />
            <Route path="/limitations/new" element={<LimitationForm />} />
            <Route path="/limitations/:id" element={<LimitationDetail />} />
            <Route path="/limitations/:id/edit" element={<LimitationForm />} />
            <Route path="/limitationOverrides" element={<LimitationOverrideList />} />
            <Route path="/limitationOverrides/new" element={<LimitationOverrideForm />} />
            <Route path="/limitationOverrides/:id" element={<LimitationOverrideDetail />} />
            <Route path="/limitationOverrides/:id/edit" element={<LimitationOverrideForm />} />
            <Route path="/mCCBFPBankAccounts" element={<MCCBFPBankAccountList />} />
            <Route path="/mCCBFPBankAccounts/new" element={<MCCBFPBankAccountForm />} />
            <Route path="/mCCBFPBankAccounts/:id" element={<MCCBFPBankAccountDetail />} />
            <Route path="/mCCBFPBankAccounts/:id/edit" element={<MCCBFPBankAccountForm />} />
            <Route path="/mCCBFPCashOuts" element={<MCCBFPCashOutList />} />
            <Route path="/mCCBFPCashOuts/new" element={<MCCBFPCashOutForm />} />
            <Route path="/mCCBFPCashOuts/:id" element={<MCCBFPCashOutDetail />} />
            <Route path="/mCCBFPCashOuts/:id/edit" element={<MCCBFPCashOutForm />} />
            <Route path="/mCCBFPMobileWallets" element={<MCCBFPMobileWalletList />} />
            <Route path="/mCCBFPMobileWallets/new" element={<MCCBFPMobileWalletForm />} />
            <Route path="/mCCBFPMobileWallets/:id" element={<MCCBFPMobileWalletDetail />} />
            <Route path="/mCCBFPMobileWallets/:id/edit" element={<MCCBFPMobileWalletForm />} />
            <Route path="/mCCBFPPaymentCards" element={<MCCBFPPaymentCardList />} />
            <Route path="/mCCBFPPaymentCards/new" element={<MCCBFPPaymentCardForm />} />
            <Route path="/mCCBFPPaymentCards/:id" element={<MCCBFPPaymentCardDetail />} />
            <Route path="/mCCBFPPaymentCards/:id/edit" element={<MCCBFPPaymentCardForm />} />
            <Route path="/mCCBParticipants" element={<MCCBParticipantList />} />
            <Route path="/mCCBParticipants/new" element={<MCCBParticipantForm />} />
            <Route path="/mCCBParticipants/:id" element={<MCCBParticipantDetail />} />
            <Route path="/mCCBParticipants/:id/edit" element={<MCCBParticipantForm />} />
            <Route path="/mCCBReceivers" element={<MCCBReceiverList />} />
            <Route path="/mCCBReceivers/new" element={<MCCBReceiverForm />} />
            <Route path="/mCCBReceivers/:id" element={<MCCBReceiverDetail />} />
            <Route path="/mCCBReceivers/:id/edit" element={<MCCBReceiverForm />} />
            <Route path="/mCCBReceiverAccounts" element={<MCCBReceiverAccountList />} />
            <Route path="/mCCBReceiverAccounts/new" element={<MCCBReceiverAccountForm />} />
            <Route path="/mCCBReceiverAccounts/:id" element={<MCCBReceiverAccountDetail />} />
            <Route path="/mCCBReceiverAccounts/:id/edit" element={<MCCBReceiverAccountForm />} />
            <Route path="/mCCBSecurityChecks" element={<MCCBSecurityCheckList />} />
            <Route path="/mCCBSecurityChecks/new" element={<MCCBSecurityCheckForm />} />
            <Route path="/mCCBSecurityChecks/:id" element={<MCCBSecurityCheckDetail />} />
            <Route path="/mCCBSecurityChecks/:id/edit" element={<MCCBSecurityCheckForm />} />
            <Route path="/mCCBTransactions" element={<MCCBTransactionList />} />
            <Route path="/mCCBTransactions/new" element={<MCCBTransactionForm />} />
            <Route path="/mCCBTransactions/:id" element={<MCCBTransactionDetail />} />
            <Route path="/mCCBTransactions/:id/edit" element={<MCCBTransactionForm />} />
            <Route path="/newCardRequests" element={<NewCardRequestList />} />
            <Route path="/newCardRequests/new" element={<NewCardRequestForm />} />
            <Route path="/newCardRequests/:id" element={<NewCardRequestDetail />} />
            <Route path="/newCardRequests/:id/edit" element={<NewCardRequestForm />} />
            <Route path="/newCardRequestAPIs" element={<NewCardRequestAPIList />} />
            <Route path="/newCardRequestAPIs/new" element={<NewCardRequestAPIForm />} />
            <Route path="/newCardRequestAPIs/:id" element={<NewCardRequestAPIDetail />} />
            <Route path="/newCardRequestAPIs/:id/edit" element={<NewCardRequestAPIForm />} />
            <Route path="/nonCustomers" element={<NonCustomerList />} />
            <Route path="/nonCustomers/new" element={<NonCustomerForm />} />
            <Route path="/nonCustomers/:id" element={<NonCustomerDetail />} />
            <Route path="/nonCustomers/:id/edit" element={<NonCustomerForm />} />
            <Route path="/oneTimeLinks" element={<OneTimeLinkList />} />
            <Route path="/oneTimeLinks/new" element={<OneTimeLinkForm />} />
            <Route path="/oneTimeLinks/:id" element={<OneTimeLinkDetail />} />
            <Route path="/oneTimeLinks/:id/edit" element={<OneTimeLinkForm />} />
            <Route path="/onlineApprovals" element={<OnlineApprovalList />} />
            <Route path="/onlineApprovals/new" element={<OnlineApprovalForm />} />
            <Route path="/onlineApprovals/:id" element={<OnlineApprovalDetail />} />
            <Route path="/onlineApprovals/:id/edit" element={<OnlineApprovalForm />} />
            <Route path="/requestedTransactions" element={<RequestedTransactionList />} />
            <Route path="/requestedTransactions/new" element={<RequestedTransactionForm />} />
            <Route path="/requestedTransactions/:id" element={<RequestedTransactionDetail />} />
            <Route path="/requestedTransactions/:id/edit" element={<RequestedTransactionForm />} />
            <Route path="/requestedTransactionFees" element={<RequestedTransactionFeeList />} />
            <Route path="/requestedTransactionFees/new" element={<RequestedTransactionFeeForm />} />
            <Route path="/requestedTransactionFees/:id" element={<RequestedTransactionFeeDetail />} />
            <Route path="/requestedTransactionFees/:id/edit" element={<RequestedTransactionFeeForm />} />
            <Route path="/riskEvents" element={<RiskEventList />} />
            <Route path="/riskEvents/new" element={<RiskEventForm />} />
            <Route path="/riskEvents/:id" element={<RiskEventDetail />} />
            <Route path="/riskEvents/:id/edit" element={<RiskEventForm />} />
            <Route path="/riskRules" element={<RiskRuleList />} />
            <Route path="/riskRules/new" element={<RiskRuleForm />} />
            <Route path="/riskRules/:id" element={<RiskRuleDetail />} />
            <Route path="/riskRules/:id/edit" element={<RiskRuleForm />} />
            <Route path="/riskRuleLogs" element={<RiskRuleLogList />} />
            <Route path="/riskRuleLogs/new" element={<RiskRuleLogForm />} />
            <Route path="/riskRuleLogs/:id" element={<RiskRuleLogDetail />} />
            <Route path="/riskRuleLogs/:id/edit" element={<RiskRuleLogForm />} />
            <Route path="/riskRuleOverrides" element={<RiskRuleOverrideList />} />
            <Route path="/riskRuleOverrides/new" element={<RiskRuleOverrideForm />} />
            <Route path="/riskRuleOverrides/:id" element={<RiskRuleOverrideDetail />} />
            <Route path="/riskRuleOverrides/:id/edit" element={<RiskRuleOverrideForm />} />
            <Route path="/saps" element={<SapList />} />
            <Route path="/saps/new" element={<SapForm />} />
            <Route path="/saps/:id" element={<SapDetail />} />
            <Route path="/saps/:id/edit" element={<SapForm />} />
            <Route path="/sysdiagramss" element={<SysdiagramsList />} />
            <Route path="/sysdiagramss/new" element={<SysdiagramsForm />} />
            <Route path="/sysdiagramss/:id" element={<SysdiagramsDetail />} />
            <Route path="/sysdiagramss/:id/edit" element={<SysdiagramsForm />} />
            <Route path="/tasks" element={<TaskList />} />
            <Route path="/tasks/new" element={<TaskForm />} />
            <Route path="/tasks/:id" element={<TaskDetail />} />
            <Route path="/tasks/:id/edit" element={<TaskForm />} />
            <Route path="/transactionBits" element={<TransactionBitList />} />
            <Route path="/transactionBits/new" element={<TransactionBitForm />} />
            <Route path="/transactionBits/:id" element={<TransactionBitDetail />} />
            <Route path="/transactionBits/:id/edit" element={<TransactionBitForm />} />
            <Route path="/transactionCreditCards" element={<TransactionCreditCardList />} />
            <Route path="/transactionCreditCards/new" element={<TransactionCreditCardForm />} />
            <Route path="/transactionCreditCards/:id" element={<TransactionCreditCardDetail />} />
            <Route path="/transactionCreditCards/:id/edit" element={<TransactionCreditCardForm />} />
            <Route path="/transactionLoads" element={<TransactionLoadList />} />
            <Route path="/transactionLoads/new" element={<TransactionLoadForm />} />
            <Route path="/transactionLoads/:id" element={<TransactionLoadDetail />} />
            <Route path="/transactionLoads/:id/edit" element={<TransactionLoadForm />} />
            <Route path="/transactionLoadAccounts" element={<TransactionLoadAccountList />} />
            <Route path="/transactionLoadAccounts/new" element={<TransactionLoadAccountForm />} />
            <Route path="/transactionLoadAccounts/:id" element={<TransactionLoadAccountDetail />} />
            <Route path="/transactionLoadAccounts/:id/edit" element={<TransactionLoadAccountForm />} />
            <Route path="/uITexts" element={<UITextList />} />
            <Route path="/uITexts/new" element={<UITextForm />} />
            <Route path="/uITexts/:id" element={<UITextDetail />} />
            <Route path="/uITexts/:id/edit" element={<UITextForm />} />
            <Route path="/userApprovals" element={<UserApprovalList />} />
            <Route path="/userApprovals/new" element={<UserApprovalForm />} />
            <Route path="/userApprovals/:id" element={<UserApprovalDetail />} />
            <Route path="/userApprovals/:id/edit" element={<UserApprovalForm />} />
          </Routes>
        </Container>
      </Box>
    </Box>
  );
}

export default App;
