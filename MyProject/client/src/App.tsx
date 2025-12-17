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
import { IndexFragmentationList } from './components/IndexFragmentation/IndexFragmentationList';
import { IndexFragmentationDetail } from './components/IndexFragmentation/IndexFragmentationDetail';
import { IndexFragmentationDataList } from './components/IndexFragmentationData/IndexFragmentationDataList';
import { IndexFragmentationDataDetail } from './components/IndexFragmentationData/IndexFragmentationDataDetail';
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
import { TableSizeList } from './components/TableSize/TableSizeList';
import { TableSizeDetail } from './components/TableSize/TableSizeDetail';
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
import { CcvwComboListActiveRestrictionsList } from './components/CcvwComboListActiveRestrictions/CcvwComboListActiveRestrictionsList';
import { CcvwComboListActiveRestrictionsDetail } from './components/CcvwComboListActiveRestrictions/CcvwComboListActiveRestrictionsDetail';
import { CcvwComboListCAlertMessageList } from './components/CcvwComboListCAlertMessage/CcvwComboListCAlertMessageList';
import { CcvwComboListCAlertMessageDetail } from './components/CcvwComboListCAlertMessage/CcvwComboListCAlertMessageDetail';
import { CcvwComboListCEnumerationList } from './components/CcvwComboListCEnumeration/CcvwComboListCEnumerationList';
import { CcvwComboListCEnumerationDetail } from './components/CcvwComboListCEnumeration/CcvwComboListCEnumerationDetail';
import { CcvwComboListCJobList } from './components/CcvwComboListCJob/CcvwComboListCJobList';
import { CcvwComboListCJobDetail } from './components/CcvwComboListCJob/CcvwComboListCJobDetail';
import { CcvwComboListCLanguageList } from './components/CcvwComboListCLanguage/CcvwComboListCLanguageList';
import { CcvwComboListCLanguageDetail } from './components/CcvwComboListCLanguage/CcvwComboListCLanguageDetail';
import { CcvwComboListCLoggedAlertList } from './components/CcvwComboListCLoggedAlert/CcvwComboListCLoggedAlertList';
import { CcvwComboListCLoggedAlertDetail } from './components/CcvwComboListCLoggedAlert/CcvwComboListCLoggedAlertDetail';
import { CcvwComboListCLoggedAlertForAffectedUserList } from './components/CcvwComboListCLoggedAlertForAffectedUser/CcvwComboListCLoggedAlertForAffectedUserList';
import { CcvwComboListCLoggedAlertForAffectedUserDetail } from './components/CcvwComboListCLoggedAlertForAffectedUser/CcvwComboListCLoggedAlertForAffectedUserDetail';
import { CcvwComboListCLoggedLoginList } from './components/CcvwComboListCLoggedLogin/CcvwComboListCLoggedLoginList';
import { CcvwComboListCLoggedLoginDetail } from './components/CcvwComboListCLoggedLogin/CcvwComboListCLoggedLoginDetail';
import { CcvwComboListCLookupList } from './components/CcvwComboListCLookup/CcvwComboListCLookupList';
import { CcvwComboListCLookupDetail } from './components/CcvwComboListCLookup/CcvwComboListCLookupDetail';
import { CcvwComboListCMfaList } from './components/CcvwComboListCMfa/CcvwComboListCMfaList';
import { CcvwComboListCMfaDetail } from './components/CcvwComboListCMfa/CcvwComboListCMfaDetail';
import { CcvwComboListCObjectToTranslateList } from './components/CcvwComboListCObjectToTranslate/CcvwComboListCObjectToTranslateList';
import { CcvwComboListCObjectToTranslateDetail } from './components/CcvwComboListCObjectToTranslate/CcvwComboListCObjectToTranslateDetail';
import { CcvwComboListCProcessList } from './components/CcvwComboListCProcess/CcvwComboListCProcessList';
import { CcvwComboListCProcessDetail } from './components/CcvwComboListCProcess/CcvwComboListCProcessDetail';
import { CcvwComboListCRoleList } from './components/CcvwComboListCRole/CcvwComboListCRoleList';
import { CcvwComboListCRoleDetail } from './components/CcvwComboListCRole/CcvwComboListCRoleDetail';
import { CcvwComboListCSystemDefaultList } from './components/CcvwComboListCSystemDefault/CcvwComboListCSystemDefaultList';
import { CcvwComboListCSystemDefaultDetail } from './components/CcvwComboListCSystemDefault/CcvwComboListCSystemDefaultDetail';
import { CcvwComboListCTableList } from './components/CcvwComboListCTable/CcvwComboListCTableList';
import { CcvwComboListCTableDetail } from './components/CcvwComboListCTable/CcvwComboListCTableDetail';
import { CcvwComboListCUserList } from './components/CcvwComboListCUser/CcvwComboListCUserList';
import { CcvwComboListCUserDetail } from './components/CcvwComboListCUser/CcvwComboListCUserDetail';
import { CcvwComboListCardList } from './components/CcvwComboListCard/CcvwComboListCardList';
import { CcvwComboListCardDetail } from './components/CcvwComboListCard/CcvwComboListCardDetail';
import { CcvwComboListCardActivationList } from './components/CcvwComboListCardActivation/CcvwComboListCardActivationList';
import { CcvwComboListCardActivationDetail } from './components/CcvwComboListCardActivation/CcvwComboListCardActivationDetail';
import { CcvwComboListCardActivationForCardList } from './components/CcvwComboListCardActivationForCard/CcvwComboListCardActivationForCardList';
import { CcvwComboListCardActivationForCardDetail } from './components/CcvwComboListCardActivationForCard/CcvwComboListCardActivationForCardDetail';
import { CcvwComboListCardActivationForCustomerList } from './components/CcvwComboListCardActivationForCustomer/CcvwComboListCardActivationForCustomerList';
import { CcvwComboListCardActivationForCustomerDetail } from './components/CcvwComboListCardActivationForCustomer/CcvwComboListCardActivationForCustomerDetail';
import { CcvwComboListCardActivationForDistributorList } from './components/CcvwComboListCardActivationForDistributor/CcvwComboListCardActivationForDistributorList';
import { CcvwComboListCardActivationForDistributorDetail } from './components/CcvwComboListCardActivationForDistributor/CcvwComboListCardActivationForDistributorDetail';
import { CcvwComboListCardClubList } from './components/CcvwComboListCardClub/CcvwComboListCardClubList';
import { CcvwComboListCardClubDetail } from './components/CcvwComboListCardClub/CcvwComboListCardClubDetail';
import { CcvwComboListCardClubForAssignedCompanyCustomerList } from './components/CcvwComboListCardClubForAssignedCompanyCustomer/CcvwComboListCardClubForAssignedCompanyCustomerList';
import { CcvwComboListCardClubForAssignedCompanyCustomerDetail } from './components/CcvwComboListCardClubForAssignedCompanyCustomer/CcvwComboListCardClubForAssignedCompanyCustomerDetail';
import { CcvwComboListCardForCustomerList } from './components/CcvwComboListCardForCustomer/CcvwComboListCardForCustomerList';
import { CcvwComboListCardForCustomerDetail } from './components/CcvwComboListCardForCustomer/CcvwComboListCardForCustomerDetail';
import { CcvwComboListCardForOwningDistributorList } from './components/CcvwComboListCardForOwningDistributor/CcvwComboListCardForOwningDistributorList';
import { CcvwComboListCardForOwningDistributorDetail } from './components/CcvwComboListCardForOwningDistributor/CcvwComboListCardForOwningDistributorDetail';
import { CcvwComboListCardForPreviousDistributorList } from './components/CcvwComboListCardForPreviousDistributor/CcvwComboListCardForPreviousDistributorList';
import { CcvwComboListCardForPreviousDistributorDetail } from './components/CcvwComboListCardForPreviousDistributor/CcvwComboListCardForPreviousDistributorDetail';
import { CcvwComboListCardOrderByDistributorList } from './components/CcvwComboListCardOrderByDistributor/CcvwComboListCardOrderByDistributorList';
import { CcvwComboListCardOrderByDistributorDetail } from './components/CcvwComboListCardOrderByDistributor/CcvwComboListCardOrderByDistributorDetail';
import { CcvwComboListCardOrderByDistributorForDistributorList } from './components/CcvwComboListCardOrderByDistributorForDistributor/CcvwComboListCardOrderByDistributorForDistributorList';
import { CcvwComboListCardOrderByDistributorForDistributorDetail } from './components/CcvwComboListCardOrderByDistributorForDistributor/CcvwComboListCardOrderByDistributorForDistributorDetail';
import { CcvwComboListCardOrderByDistributorForOrderedByUserList } from './components/CcvwComboListCardOrderByDistributorForOrderedByUser/CcvwComboListCardOrderByDistributorForOrderedByUserList';
import { CcvwComboListCardOrderByDistributorForOrderedByUserDetail } from './components/CcvwComboListCardOrderByDistributorForOrderedByUser/CcvwComboListCardOrderByDistributorForOrderedByUserDetail';
import { CcvwComboListCardOrderFromIssuerList } from './components/CcvwComboListCardOrderFromIssuer/CcvwComboListCardOrderFromIssuerList';
import { CcvwComboListCardOrderFromIssuerDetail } from './components/CcvwComboListCardOrderFromIssuer/CcvwComboListCardOrderFromIssuerDetail';
import { CcvwComboListCardOrderFromIssuerForOrderedByUserList } from './components/CcvwComboListCardOrderFromIssuerForOrderedByUser/CcvwComboListCardOrderFromIssuerForOrderedByUserList';
import { CcvwComboListCardOrderFromIssuerForOrderedByUserDetail } from './components/CcvwComboListCardOrderFromIssuerForOrderedByUser/CcvwComboListCardOrderFromIssuerForOrderedByUserDetail';
import { CcvwComboListCountryList } from './components/CcvwComboListCountry/CcvwComboListCountryList';
import { CcvwComboListCountryDetail } from './components/CcvwComboListCountry/CcvwComboListCountryDetail';
import { CcvwComboListCustomerList } from './components/CcvwComboListCustomer/CcvwComboListCustomerList';
import { CcvwComboListCustomerDetail } from './components/CcvwComboListCustomer/CcvwComboListCustomerDetail';
import { CcvwComboListCustomerCardTransactionList } from './components/CcvwComboListCustomerCardTransaction/CcvwComboListCustomerCardTransactionList';
import { CcvwComboListCustomerCardTransactionDetail } from './components/CcvwComboListCustomerCardTransaction/CcvwComboListCustomerCardTransactionDetail';
import { CcvwComboListCustomerCardTransactionForCardList } from './components/CcvwComboListCustomerCardTransactionForCard/CcvwComboListCustomerCardTransactionForCardList';
import { CcvwComboListCustomerCardTransactionForCardDetail } from './components/CcvwComboListCustomerCardTransactionForCard/CcvwComboListCustomerCardTransactionForCardDetail';
import { CcvwComboListCustomerCardTransactionForCustomerList } from './components/CcvwComboListCustomerCardTransactionForCustomer/CcvwComboListCustomerCardTransactionForCustomerList';
import { CcvwComboListCustomerCardTransactionForCustomerDetail } from './components/CcvwComboListCustomerCardTransactionForCustomer/CcvwComboListCustomerCardTransactionForCustomerDetail';
import { CcvwComboListCustomerCardTransactionForlgOwningDistributorList } from './components/CcvwComboListCustomerCardTransactionForlgOwningDistributor/CcvwComboListCustomerCardTransactionForlgOwningDistributorList';
import { CcvwComboListCustomerCardTransactionForlgOwningDistributorDetail } from './components/CcvwComboListCustomerCardTransactionForlgOwningDistributor/CcvwComboListCustomerCardTransactionForlgOwningDistributorDetail';
import { CcvwComboListCustomerClubList } from './components/CcvwComboListCustomerClub/CcvwComboListCustomerClubList';
import { CcvwComboListCustomerClubDetail } from './components/CcvwComboListCustomerClub/CcvwComboListCustomerClubDetail';
import { CcvwComboListCustomerCreditCardList } from './components/CcvwComboListCustomerCreditCard/CcvwComboListCustomerCreditCardList';
import { CcvwComboListCustomerCreditCardDetail } from './components/CcvwComboListCustomerCreditCard/CcvwComboListCustomerCreditCardDetail';
import { CcvwComboListCustomerCreditCardForCustomerList } from './components/CcvwComboListCustomerCreditCardForCustomer/CcvwComboListCustomerCreditCardForCustomerList';
import { CcvwComboListCustomerCreditCardForCustomerDetail } from './components/CcvwComboListCustomerCreditCardForCustomer/CcvwComboListCustomerCreditCardForCustomerDetail';
import { CcvwComboListCustomerForAssignedDistributorList } from './components/CcvwComboListCustomerForAssignedDistributor/CcvwComboListCustomerForAssignedDistributorList';
import { CcvwComboListCustomerForAssignedDistributorDetail } from './components/CcvwComboListCustomerForAssignedDistributor/CcvwComboListCustomerForAssignedDistributorDetail';
import { CcvwComboListCustomerPartnershipList } from './components/CcvwComboListCustomerPartnership/CcvwComboListCustomerPartnershipList';
import { CcvwComboListCustomerPartnershipDetail } from './components/CcvwComboListCustomerPartnership/CcvwComboListCustomerPartnershipDetail';
import { CcvwComboListCustomerPartnershipForCustomerList } from './components/CcvwComboListCustomerPartnershipForCustomer/CcvwComboListCustomerPartnershipForCustomerList';
import { CcvwComboListCustomerPartnershipForCustomerDetail } from './components/CcvwComboListCustomerPartnershipForCustomer/CcvwComboListCustomerPartnershipForCustomerDetail';
import { CcvwComboListCustomerWalletList } from './components/CcvwComboListCustomerWallet/CcvwComboListCustomerWalletList';
import { CcvwComboListCustomerWalletDetail } from './components/CcvwComboListCustomerWallet/CcvwComboListCustomerWalletDetail';
import { CcvwComboListCustomerWalletForCustomerList } from './components/CcvwComboListCustomerWalletForCustomer/CcvwComboListCustomerWalletForCustomerList';
import { CcvwComboListCustomerWalletForCustomerDetail } from './components/CcvwComboListCustomerWalletForCustomer/CcvwComboListCustomerWalletForCustomerDetail';
import { CcvwComboListCustomerWalletForWhoInitiatedList } from './components/CcvwComboListCustomerWalletForWhoInitiated/CcvwComboListCustomerWalletForWhoInitiatedList';
import { CcvwComboListCustomerWalletForWhoInitiatedDetail } from './components/CcvwComboListCustomerWalletForWhoInitiated/CcvwComboListCustomerWalletForWhoInitiatedDetail';
import { CcvwComboListDistributorList } from './components/CcvwComboListDistributor/CcvwComboListDistributorList';
import { CcvwComboListDistributorDetail } from './components/CcvwComboListDistributor/CcvwComboListDistributorDetail';
import { CcvwComboListEntityRiskColorList } from './components/CcvwComboListEntityRiskColor/CcvwComboListEntityRiskColorList';
import { CcvwComboListEntityRiskColorDetail } from './components/CcvwComboListEntityRiskColor/CcvwComboListEntityRiskColorDetail';
import { CcvwComboListFeeList } from './components/CcvwComboListFee/CcvwComboListFeeList';
import { CcvwComboListFeeDetail } from './components/CcvwComboListFee/CcvwComboListFeeDetail';
import { CcvwComboListGovtReport00List } from './components/CcvwComboListGovtReport00/CcvwComboListGovtReport00List';
import { CcvwComboListGovtReport00Detail } from './components/CcvwComboListGovtReport00/CcvwComboListGovtReport00Detail';
import { CcvwComboListImportTransactionLoadList } from './components/CcvwComboListImportTransactionLoad/CcvwComboListImportTransactionLoadList';
import { CcvwComboListImportTransactionLoadDetail } from './components/CcvwComboListImportTransactionLoad/CcvwComboListImportTransactionLoadDetail';
import { CcvwComboListImportTransactionLoadForDistributorList } from './components/CcvwComboListImportTransactionLoadForDistributor/CcvwComboListImportTransactionLoadForDistributorList';
import { CcvwComboListImportTransactionLoadForDistributorDetail } from './components/CcvwComboListImportTransactionLoadForDistributor/CcvwComboListImportTransactionLoadForDistributorDetail';
import { CcvwComboListImportTransactionLoadForInitiatedByUserList } from './components/CcvwComboListImportTransactionLoadForInitiatedByUser/CcvwComboListImportTransactionLoadForInitiatedByUserList';
import { CcvwComboListImportTransactionLoadForInitiatedByUserDetail } from './components/CcvwComboListImportTransactionLoadForInitiatedByUser/CcvwComboListImportTransactionLoadForInitiatedByUserDetail';
import { CcvwComboListLimitationList } from './components/CcvwComboListLimitation/CcvwComboListLimitationList';
import { CcvwComboListLimitationDetail } from './components/CcvwComboListLimitation/CcvwComboListLimitationDetail';
import { CcvwComboListMCCBReceiverList } from './components/CcvwComboListMCCBReceiver/CcvwComboListMCCBReceiverList';
import { CcvwComboListMCCBReceiverDetail } from './components/CcvwComboListMCCBReceiver/CcvwComboListMCCBReceiverDetail';
import { CcvwComboListMCCBReceiverAccountList } from './components/CcvwComboListMCCBReceiverAccount/CcvwComboListMCCBReceiverAccountList';
import { CcvwComboListMCCBReceiverAccountDetail } from './components/CcvwComboListMCCBReceiverAccount/CcvwComboListMCCBReceiverAccountDetail';
import { CcvwComboListMCCBReceiverForCustomerList } from './components/CcvwComboListMCCBReceiverForCustomer/CcvwComboListMCCBReceiverForCustomerList';
import { CcvwComboListMCCBReceiverForCustomerDetail } from './components/CcvwComboListMCCBReceiverForCustomer/CcvwComboListMCCBReceiverForCustomerDetail';
import { CcvwComboListMCCBTransactionList } from './components/CcvwComboListMCCBTransaction/CcvwComboListMCCBTransactionList';
import { CcvwComboListMCCBTransactionDetail } from './components/CcvwComboListMCCBTransaction/CcvwComboListMCCBTransactionDetail';
import { CcvwComboListMCCBTransactionForCustomerList } from './components/CcvwComboListMCCBTransactionForCustomer/CcvwComboListMCCBTransactionForCustomerList';
import { CcvwComboListMCCBTransactionForCustomerDetail } from './components/CcvwComboListMCCBTransactionForCustomer/CcvwComboListMCCBTransactionForCustomerDetail';
import { CcvwComboListNewCardRequestList } from './components/CcvwComboListNewCardRequest/CcvwComboListNewCardRequestList';
import { CcvwComboListNewCardRequestDetail } from './components/CcvwComboListNewCardRequest/CcvwComboListNewCardRequestDetail';
import { CcvwComboListNewCardRequestAPIList } from './components/CcvwComboListNewCardRequestAPI/CcvwComboListNewCardRequestAPIList';
import { CcvwComboListNewCardRequestAPIDetail } from './components/CcvwComboListNewCardRequestAPI/CcvwComboListNewCardRequestAPIDetail';
import { CcvwComboListNewCardRequestAPIForDistributorList } from './components/CcvwComboListNewCardRequestAPIForDistributor/CcvwComboListNewCardRequestAPIForDistributorList';
import { CcvwComboListNewCardRequestAPIForDistributorDetail } from './components/CcvwComboListNewCardRequestAPIForDistributor/CcvwComboListNewCardRequestAPIForDistributorDetail';
import { CcvwComboListNewCardRequestForCustomerList } from './components/CcvwComboListNewCardRequestForCustomer/CcvwComboListNewCardRequestForCustomerList';
import { CcvwComboListNewCardRequestForCustomerDetail } from './components/CcvwComboListNewCardRequestForCustomer/CcvwComboListNewCardRequestForCustomerDetail';
import { CcvwComboListNewCardRequestForDistributorSentToList } from './components/CcvwComboListNewCardRequestForDistributorSentTo/CcvwComboListNewCardRequestForDistributorSentToList';
import { CcvwComboListNewCardRequestForDistributorSentToDetail } from './components/CcvwComboListNewCardRequestForDistributorSentTo/CcvwComboListNewCardRequestForDistributorSentToDetail';
import { CcvwComboListNonCustomerList } from './components/CcvwComboListNonCustomer/CcvwComboListNonCustomerList';
import { CcvwComboListNonCustomerDetail } from './components/CcvwComboListNonCustomer/CcvwComboListNonCustomerDetail';
import { CcvwComboListRequestedTransactionList } from './components/CcvwComboListRequestedTransaction/CcvwComboListRequestedTransactionList';
import { CcvwComboListRequestedTransactionDetail } from './components/CcvwComboListRequestedTransaction/CcvwComboListRequestedTransactionDetail';
import { CcvwComboListRequestedTransactionForDistributorList } from './components/CcvwComboListRequestedTransactionForDistributor/CcvwComboListRequestedTransactionForDistributorList';
import { CcvwComboListRequestedTransactionForDistributorDetail } from './components/CcvwComboListRequestedTransactionForDistributor/CcvwComboListRequestedTransactionForDistributorDetail';
import { CcvwComboListRequestedTransactionForInitiatingCustomerList } from './components/CcvwComboListRequestedTransactionForInitiatingCustomer/CcvwComboListRequestedTransactionForInitiatingCustomerList';
import { CcvwComboListRequestedTransactionForInitiatingCustomerDetail } from './components/CcvwComboListRequestedTransactionForInitiatingCustomer/CcvwComboListRequestedTransactionForInitiatingCustomerDetail';
import { CcvwComboListRequestedTransactionForPayingCustomerList } from './components/CcvwComboListRequestedTransactionForPayingCustomer/CcvwComboListRequestedTransactionForPayingCustomerList';
import { CcvwComboListRequestedTransactionForPayingCustomerDetail } from './components/CcvwComboListRequestedTransactionForPayingCustomer/CcvwComboListRequestedTransactionForPayingCustomerDetail';
import { CcvwComboListRequestedTransactionForPrepaidCardList } from './components/CcvwComboListRequestedTransactionForPrepaidCard/CcvwComboListRequestedTransactionForPrepaidCardList';
import { CcvwComboListRequestedTransactionForPrepaidCardDetail } from './components/CcvwComboListRequestedTransactionForPrepaidCard/CcvwComboListRequestedTransactionForPrepaidCardDetail';
import { CcvwComboListRequestedTransactionForReceivingCustomerList } from './components/CcvwComboListRequestedTransactionForReceivingCustomer/CcvwComboListRequestedTransactionForReceivingCustomerList';
import { CcvwComboListRequestedTransactionForReceivingCustomerDetail } from './components/CcvwComboListRequestedTransactionForReceivingCustomer/CcvwComboListRequestedTransactionForReceivingCustomerDetail';
import { CcvwComboListRequestedTransactionForTargetCustomerList } from './components/CcvwComboListRequestedTransactionForTargetCustomer/CcvwComboListRequestedTransactionForTargetCustomerList';
import { CcvwComboListRequestedTransactionForTargetCustomerDetail } from './components/CcvwComboListRequestedTransactionForTargetCustomer/CcvwComboListRequestedTransactionForTargetCustomerDetail';
import { CcvwComboListRiskEventList } from './components/CcvwComboListRiskEvent/CcvwComboListRiskEventList';
import { CcvwComboListRiskEventDetail } from './components/CcvwComboListRiskEvent/CcvwComboListRiskEventDetail';
import { CcvwComboListRiskRuleList } from './components/CcvwComboListRiskRule/CcvwComboListRiskRuleList';
import { CcvwComboListRiskRuleDetail } from './components/CcvwComboListRiskRule/CcvwComboListRiskRuleDetail';
import { CcvwComboListRiskRuleLogList } from './components/CcvwComboListRiskRuleLog/CcvwComboListRiskRuleLogList';
import { CcvwComboListRiskRuleLogDetail } from './components/CcvwComboListRiskRuleLog/CcvwComboListRiskRuleLogDetail';
import { CcvwComboListRiskRuleOverrideList } from './components/CcvwComboListRiskRuleOverride/CcvwComboListRiskRuleOverrideList';
import { CcvwComboListRiskRuleOverrideDetail } from './components/CcvwComboListRiskRuleOverride/CcvwComboListRiskRuleOverrideDetail';
import { CcvwComboListTransactionBitList } from './components/CcvwComboListTransactionBit/CcvwComboListTransactionBitList';
import { CcvwComboListTransactionBitDetail } from './components/CcvwComboListTransactionBit/CcvwComboListTransactionBitDetail';
import { CcvwComboListTransactionBitForCustomerList } from './components/CcvwComboListTransactionBitForCustomer/CcvwComboListTransactionBitForCustomerList';
import { CcvwComboListTransactionBitForCustomerDetail } from './components/CcvwComboListTransactionBitForCustomer/CcvwComboListTransactionBitForCustomerDetail';
import { CcvwComboListTransactionCreditCardList } from './components/CcvwComboListTransactionCreditCard/CcvwComboListTransactionCreditCardList';
import { CcvwComboListTransactionCreditCardDetail } from './components/CcvwComboListTransactionCreditCard/CcvwComboListTransactionCreditCardDetail';
import { CcvwComboListTransactionCreditCardForCustomerList } from './components/CcvwComboListTransactionCreditCardForCustomer/CcvwComboListTransactionCreditCardForCustomerList';
import { CcvwComboListTransactionCreditCardForCustomerDetail } from './components/CcvwComboListTransactionCreditCardForCustomer/CcvwComboListTransactionCreditCardForCustomerDetail';
import { CcvwComboListTransactionLoadList } from './components/CcvwComboListTransactionLoad/CcvwComboListTransactionLoadList';
import { CcvwComboListTransactionLoadDetail } from './components/CcvwComboListTransactionLoad/CcvwComboListTransactionLoadDetail';
import { CcvwComboListTransactionLoadAccountList } from './components/CcvwComboListTransactionLoadAccount/CcvwComboListTransactionLoadAccountList';
import { CcvwComboListTransactionLoadAccountDetail } from './components/CcvwComboListTransactionLoadAccount/CcvwComboListTransactionLoadAccountDetail';
import { CcvwComboListTransactionLoadForCardList } from './components/CcvwComboListTransactionLoadForCard/CcvwComboListTransactionLoadForCardList';
import { CcvwComboListTransactionLoadForCardDetail } from './components/CcvwComboListTransactionLoadForCard/CcvwComboListTransactionLoadForCardDetail';
import { CcvwComboListTransactionLoadForCustomerList } from './components/CcvwComboListTransactionLoadForCustomer/CcvwComboListTransactionLoadForCustomerList';
import { CcvwComboListTransactionLoadForCustomerDetail } from './components/CcvwComboListTransactionLoadForCustomer/CcvwComboListTransactionLoadForCustomerDetail';
import { CcvwComboListTransactionLoadForDistributorList } from './components/CcvwComboListTransactionLoadForDistributor/CcvwComboListTransactionLoadForDistributorList';
import { CcvwComboListTransactionLoadForDistributorDetail } from './components/CcvwComboListTransactionLoadForDistributor/CcvwComboListTransactionLoadForDistributorDetail';
import { CcvwComboListTransactionLoadForUserList } from './components/CcvwComboListTransactionLoadForUser/CcvwComboListTransactionLoadForUserList';
import { CcvwComboListTransactionLoadForUserDetail } from './components/CcvwComboListTransactionLoadForUser/CcvwComboListTransactionLoadForUserDetail';
import { CcvwComboListUITextList } from './components/CcvwComboListUIText/CcvwComboListUITextList';
import { CcvwComboListUITextDetail } from './components/CcvwComboListUIText/CcvwComboListUITextDetail';
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
import { MnccvwComboListCardForCompanyCustomerList } from './components/MnccvwComboListCardForCompanyCustomer/MnccvwComboListCardForCompanyCustomerList';
import { MnccvwComboListCardForCompanyCustomerDetail } from './components/MnccvwComboListCardForCompanyCustomer/MnccvwComboListCardForCompanyCustomerDetail';
import { MnccvwComboListCardForCompanyCustomerDistributorList } from './components/MnccvwComboListCardForCompanyCustomerDistributor/MnccvwComboListCardForCompanyCustomerDistributorList';
import { MnccvwComboListCardForCompanyCustomerDistributorDetail } from './components/MnccvwComboListCardForCompanyCustomerDistributor/MnccvwComboListCardForCompanyCustomerDistributorDetail';
import { MnvwComboListCustomerWithPhoneAndIdentifierList } from './components/MnvwComboListCustomerWithPhoneAndIdentifier/MnvwComboListCustomerWithPhoneAndIdentifierList';
import { MnvwComboListCustomerWithPhoneAndIdentifierDetail } from './components/MnvwComboListCustomerWithPhoneAndIdentifier/MnvwComboListCustomerWithPhoneAndIdentifierDetail';
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
import { vwCustomerCardStatusList } from './components/vwCustomerCardStatus/vwCustomerCardStatusList';
import { vwCustomerCardStatusDetail } from './components/vwCustomerCardStatus/vwCustomerCardStatusDetail';
import { vwCustomerCardStatusDataList } from './components/vwCustomerCardStatusData/vwCustomerCardStatusDataList';
import { vwCustomerCardStatusDataDetail } from './components/vwCustomerCardStatusData/vwCustomerCardStatusDataDetail';
import { vwCustomerSecurityStatusCheckList } from './components/vwCustomerSecurityStatusCheck/vwCustomerSecurityStatusCheckList';
import { vwCustomerSecurityStatusCheckDetail } from './components/vwCustomerSecurityStatusCheck/vwCustomerSecurityStatusCheckDetail';
import { vwCustomerSecurityStatusCheckDataList } from './components/vwCustomerSecurityStatusCheckData/vwCustomerSecurityStatusCheckDataList';
import { vwCustomerSecurityStatusCheckDataDetail } from './components/vwCustomerSecurityStatusCheckData/vwCustomerSecurityStatusCheckDataDetail';
import { vwOnlineCardTransactionList } from './components/vwOnlineCardTransaction/vwOnlineCardTransactionList';
import { vwOnlineCardTransactionDetail } from './components/vwOnlineCardTransaction/vwOnlineCardTransactionDetail';
import { vwReportCardActivityList } from './components/vwReportCardActivity/vwReportCardActivityList';
import { vwReportCardActivityDetail } from './components/vwReportCardActivity/vwReportCardActivityDetail';
import { vwReportCardActivityDataList } from './components/vwReportCardActivityData/vwReportCardActivityDataList';
import { vwReportCardActivityDataDetail } from './components/vwReportCardActivityData/vwReportCardActivityDataDetail';
import { vwReportCustomerBankTransferList } from './components/vwReportCustomerBankTransfer/vwReportCustomerBankTransferList';
import { vwReportCustomerBankTransferDetail } from './components/vwReportCustomerBankTransfer/vwReportCustomerBankTransferDetail';
import { vwReportCustomerBankTransferDataList } from './components/vwReportCustomerBankTransferData/vwReportCustomerBankTransferDataList';
import { vwReportCustomerBankTransferDataDetail } from './components/vwReportCustomerBankTransferData/vwReportCustomerBankTransferDataDetail';
import { vwReportCustomerCreditCardTransactionsList } from './components/vwReportCustomerCreditCardTransactions/vwReportCustomerCreditCardTransactionsList';
import { vwReportCustomerCreditCardTransactionsDetail } from './components/vwReportCustomerCreditCardTransactions/vwReportCustomerCreditCardTransactionsDetail';
import { vwReportCustomerCreditCardTransactionsDataList } from './components/vwReportCustomerCreditCardTransactionsData/vwReportCustomerCreditCardTransactionsDataList';
import { vwReportCustomerCreditCardTransactionsDataDetail } from './components/vwReportCustomerCreditCardTransactionsData/vwReportCustomerCreditCardTransactionsDataDetail';
import { vwReportCustomerWalletBalanceList } from './components/vwReportCustomerWalletBalance/vwReportCustomerWalletBalanceList';
import { vwReportCustomerWalletBalanceDetail } from './components/vwReportCustomerWalletBalance/vwReportCustomerWalletBalanceDetail';
import { vwReportCustomerWalletBalanceDataList } from './components/vwReportCustomerWalletBalanceData/vwReportCustomerWalletBalanceDataList';
import { vwReportCustomerWalletBalanceDataDetail } from './components/vwReportCustomerWalletBalanceData/vwReportCustomerWalletBalanceDataDetail';
import { vwReportCustomerWalletBalanceForLawyerList } from './components/vwReportCustomerWalletBalanceForLawyer/vwReportCustomerWalletBalanceForLawyerList';
import { vwReportCustomerWalletBalanceForLawyerDetail } from './components/vwReportCustomerWalletBalanceForLawyer/vwReportCustomerWalletBalanceForLawyerDetail';
import { vwReportCustomerWalletBalanceForLawyerDataList } from './components/vwReportCustomerWalletBalanceForLawyerData/vwReportCustomerWalletBalanceForLawyerDataList';
import { vwReportCustomerWalletBalanceForLawyerDataDetail } from './components/vwReportCustomerWalletBalanceForLawyerData/vwReportCustomerWalletBalanceForLawyerDataDetail';
import { vwReportDistributorObligoBalanceList } from './components/vwReportDistributorObligoBalance/vwReportDistributorObligoBalanceList';
import { vwReportDistributorObligoBalanceDetail } from './components/vwReportDistributorObligoBalance/vwReportDistributorObligoBalanceDetail';
import { vwReportDistributorObligoBalanceDataList } from './components/vwReportDistributorObligoBalanceData/vwReportDistributorObligoBalanceDataList';
import { vwReportDistributorObligoBalanceDataDetail } from './components/vwReportDistributorObligoBalanceData/vwReportDistributorObligoBalanceDataDetail';
import { vwReportFromChangerList } from './components/vwReportFromChanger/vwReportFromChangerList';
import { vwReportFromChangerDetail } from './components/vwReportFromChanger/vwReportFromChangerDetail';
import { vwReportFromChangerDataList } from './components/vwReportFromChangerData/vwReportFromChangerDataList';
import { vwReportFromChangerDataDetail } from './components/vwReportFromChangerData/vwReportFromChangerDataDetail';
import { vwReportLoadTransferToDistributorList } from './components/vwReportLoadTransferToDistributor/vwReportLoadTransferToDistributorList';
import { vwReportLoadTransferToDistributorDetail } from './components/vwReportLoadTransferToDistributor/vwReportLoadTransferToDistributorDetail';
import { vwReportLoadTransferToDistributorDataList } from './components/vwReportLoadTransferToDistributorData/vwReportLoadTransferToDistributorDataList';
import { vwReportLoadTransferToDistributorDataDetail } from './components/vwReportLoadTransferToDistributorData/vwReportLoadTransferToDistributorDataDetail';
import { vwReportMonthlyCardFeeList } from './components/vwReportMonthlyCardFee/vwReportMonthlyCardFeeList';
import { vwReportMonthlyCardFeeDetail } from './components/vwReportMonthlyCardFee/vwReportMonthlyCardFeeDetail';
import { vwReportMonthlyCardFeeDataList } from './components/vwReportMonthlyCardFeeData/vwReportMonthlyCardFeeDataList';
import { vwReportMonthlyCardFeeDataDetail } from './components/vwReportMonthlyCardFeeData/vwReportMonthlyCardFeeDataDetail';
import { vwReportToChangerList } from './components/vwReportToChanger/vwReportToChangerList';
import { vwReportToChangerDetail } from './components/vwReportToChanger/vwReportToChangerDetail';
import { vwReportToChangerDataList } from './components/vwReportToChangerData/vwReportToChangerDataList';
import { vwReportToChangerDataDetail } from './components/vwReportToChangerData/vwReportToChangerDataDetail';
import { vwReportWalletTransferList } from './components/vwReportWalletTransfer/vwReportWalletTransferList';
import { vwReportWalletTransferDetail } from './components/vwReportWalletTransfer/vwReportWalletTransferDetail';
import { vwReportWalletTransferDataList } from './components/vwReportWalletTransferData/vwReportWalletTransferDataList';
import { vwReportWalletTransferDataDetail } from './components/vwReportWalletTransferData/vwReportWalletTransferDataDetail';

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
              <ListItemButton component={Link} to="/indexFragmentations">
                <ListItemText primary="IndexFragmentations Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/indexFragmentationDatas">
                <ListItemText primary="IndexFragmentationDatas Report" />
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
              <ListItemButton component={Link} to="/tableSizes">
                <ListItemText primary="TableSizes Report" />
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
              <ListItemButton component={Link} to="/ccvwComboListActiveRestrictionss">
                <ListItemText primary="CcvwComboListActiveRestrictionss Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCAlertMessages">
                <ListItemText primary="CcvwComboListCAlertMessages Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCEnumerations">
                <ListItemText primary="CcvwComboListCEnumerations Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCJobs">
                <ListItemText primary="CcvwComboListCJobs Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCLanguages">
                <ListItemText primary="CcvwComboListCLanguages Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCLoggedAlerts">
                <ListItemText primary="CcvwComboListCLoggedAlerts Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCLoggedAlertForAffectedUsers">
                <ListItemText primary="CcvwComboListCLoggedAlertForAffectedUsers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCLoggedLogins">
                <ListItemText primary="CcvwComboListCLoggedLogins Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCLookups">
                <ListItemText primary="CcvwComboListCLookups Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCMfas">
                <ListItemText primary="CcvwComboListCMfas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCObjectToTranslates">
                <ListItemText primary="CcvwComboListCObjectToTranslates Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCProcesss">
                <ListItemText primary="CcvwComboListCProcesss Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCRoles">
                <ListItemText primary="CcvwComboListCRoles Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCSystemDefaults">
                <ListItemText primary="CcvwComboListCSystemDefaults Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCTables">
                <ListItemText primary="CcvwComboListCTables Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCUsers">
                <ListItemText primary="CcvwComboListCUsers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCards">
                <ListItemText primary="CcvwComboListCards Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardActivations">
                <ListItemText primary="CcvwComboListCardActivations Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardActivationForCards">
                <ListItemText primary="CcvwComboListCardActivationForCards Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardActivationForCustomers">
                <ListItemText primary="CcvwComboListCardActivationForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardActivationForDistributors">
                <ListItemText primary="CcvwComboListCardActivationForDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardClubs">
                <ListItemText primary="CcvwComboListCardClubs Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardClubForAssignedCompanyCustomers">
                <ListItemText primary="CcvwComboListCardClubForAssignedCompanyCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardForCustomers">
                <ListItemText primary="CcvwComboListCardForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardForOwningDistributors">
                <ListItemText primary="CcvwComboListCardForOwningDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardForPreviousDistributors">
                <ListItemText primary="CcvwComboListCardForPreviousDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardOrderByDistributors">
                <ListItemText primary="CcvwComboListCardOrderByDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardOrderByDistributorForDistributors">
                <ListItemText primary="CcvwComboListCardOrderByDistributorForDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardOrderByDistributorForOrderedByUsers">
                <ListItemText primary="CcvwComboListCardOrderByDistributorForOrderedByUsers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardOrderFromIssuers">
                <ListItemText primary="CcvwComboListCardOrderFromIssuers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCardOrderFromIssuerForOrderedByUsers">
                <ListItemText primary="CcvwComboListCardOrderFromIssuerForOrderedByUsers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCountrys">
                <ListItemText primary="CcvwComboListCountrys Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomers">
                <ListItemText primary="CcvwComboListCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerCardTransactions">
                <ListItemText primary="CcvwComboListCustomerCardTransactions Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerCardTransactionForCards">
                <ListItemText primary="CcvwComboListCustomerCardTransactionForCards Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerCardTransactionForCustomers">
                <ListItemText primary="CcvwComboListCustomerCardTransactionForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerCardTransactionForlgOwningDistributors">
                <ListItemText primary="CcvwComboListCustomerCardTransactionForlgOwningDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerClubs">
                <ListItemText primary="CcvwComboListCustomerClubs Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerCreditCards">
                <ListItemText primary="CcvwComboListCustomerCreditCards Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerCreditCardForCustomers">
                <ListItemText primary="CcvwComboListCustomerCreditCardForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerForAssignedDistributors">
                <ListItemText primary="CcvwComboListCustomerForAssignedDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerPartnerships">
                <ListItemText primary="CcvwComboListCustomerPartnerships Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerPartnershipForCustomers">
                <ListItemText primary="CcvwComboListCustomerPartnershipForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerWallets">
                <ListItemText primary="CcvwComboListCustomerWallets Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerWalletForCustomers">
                <ListItemText primary="CcvwComboListCustomerWalletForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListCustomerWalletForWhoInitiateds">
                <ListItemText primary="CcvwComboListCustomerWalletForWhoInitiateds Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListDistributors">
                <ListItemText primary="CcvwComboListDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListEntityRiskColors">
                <ListItemText primary="CcvwComboListEntityRiskColors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListFees">
                <ListItemText primary="CcvwComboListFees Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListGovtReport00s">
                <ListItemText primary="CcvwComboListGovtReport00s Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListImportTransactionLoads">
                <ListItemText primary="CcvwComboListImportTransactionLoads Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListImportTransactionLoadForDistributors">
                <ListItemText primary="CcvwComboListImportTransactionLoadForDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListImportTransactionLoadForInitiatedByUsers">
                <ListItemText primary="CcvwComboListImportTransactionLoadForInitiatedByUsers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListLimitations">
                <ListItemText primary="CcvwComboListLimitations Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListMCCBReceivers">
                <ListItemText primary="CcvwComboListMCCBReceivers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListMCCBReceiverAccounts">
                <ListItemText primary="CcvwComboListMCCBReceiverAccounts Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListMCCBReceiverForCustomers">
                <ListItemText primary="CcvwComboListMCCBReceiverForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListMCCBTransactions">
                <ListItemText primary="CcvwComboListMCCBTransactions Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListMCCBTransactionForCustomers">
                <ListItemText primary="CcvwComboListMCCBTransactionForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListNewCardRequests">
                <ListItemText primary="CcvwComboListNewCardRequests Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListNewCardRequestAPIs">
                <ListItemText primary="CcvwComboListNewCardRequestAPIs Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListNewCardRequestAPIForDistributors">
                <ListItemText primary="CcvwComboListNewCardRequestAPIForDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListNewCardRequestForCustomers">
                <ListItemText primary="CcvwComboListNewCardRequestForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListNewCardRequestForDistributorSentTos">
                <ListItemText primary="CcvwComboListNewCardRequestForDistributorSentTos Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListNonCustomers">
                <ListItemText primary="CcvwComboListNonCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRequestedTransactions">
                <ListItemText primary="CcvwComboListRequestedTransactions Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRequestedTransactionForDistributors">
                <ListItemText primary="CcvwComboListRequestedTransactionForDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRequestedTransactionForInitiatingCustomers">
                <ListItemText primary="CcvwComboListRequestedTransactionForInitiatingCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRequestedTransactionForPayingCustomers">
                <ListItemText primary="CcvwComboListRequestedTransactionForPayingCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRequestedTransactionForPrepaidCards">
                <ListItemText primary="CcvwComboListRequestedTransactionForPrepaidCards Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRequestedTransactionForReceivingCustomers">
                <ListItemText primary="CcvwComboListRequestedTransactionForReceivingCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRequestedTransactionForTargetCustomers">
                <ListItemText primary="CcvwComboListRequestedTransactionForTargetCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRiskEvents">
                <ListItemText primary="CcvwComboListRiskEvents Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRiskRules">
                <ListItemText primary="CcvwComboListRiskRules Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRiskRuleLogs">
                <ListItemText primary="CcvwComboListRiskRuleLogs Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListRiskRuleOverrides">
                <ListItemText primary="CcvwComboListRiskRuleOverrides Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionBits">
                <ListItemText primary="CcvwComboListTransactionBits Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionBitForCustomers">
                <ListItemText primary="CcvwComboListTransactionBitForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionCreditCards">
                <ListItemText primary="CcvwComboListTransactionCreditCards Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionCreditCardForCustomers">
                <ListItemText primary="CcvwComboListTransactionCreditCardForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionLoads">
                <ListItemText primary="CcvwComboListTransactionLoads Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionLoadAccounts">
                <ListItemText primary="CcvwComboListTransactionLoadAccounts Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionLoadForCards">
                <ListItemText primary="CcvwComboListTransactionLoadForCards Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionLoadForCustomers">
                <ListItemText primary="CcvwComboListTransactionLoadForCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionLoadForDistributors">
                <ListItemText primary="CcvwComboListTransactionLoadForDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListTransactionLoadForUsers">
                <ListItemText primary="CcvwComboListTransactionLoadForUsers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/ccvwComboListUITexts">
                <ListItemText primary="CcvwComboListUITexts Report" />
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
              <ListItemButton component={Link} to="/mnccvwComboListCardForCompanyCustomers">
                <ListItemText primary="MnccvwComboListCardForCompanyCustomers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mnccvwComboListCardForCompanyCustomerDistributors">
                <ListItemText primary="MnccvwComboListCardForCompanyCustomerDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/mnvwComboListCustomerWithPhoneAndIdentifiers">
                <ListItemText primary="MnvwComboListCustomerWithPhoneAndIdentifiers Report" />
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
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwCustomerCardStatuss">
                <ListItemText primary="vwCustomerCardStatuss Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwCustomerCardStatusDatas">
                <ListItemText primary="vwCustomerCardStatusDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwCustomerSecurityStatusChecks">
                <ListItemText primary="vwCustomerSecurityStatusChecks Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwCustomerSecurityStatusCheckDatas">
                <ListItemText primary="vwCustomerSecurityStatusCheckDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwOnlineCardTransactions">
                <ListItemText primary="vwOnlineCardTransactions Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCardActivitys">
                <ListItemText primary="vwReportCardActivitys Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCardActivityDatas">
                <ListItemText primary="vwReportCardActivityDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCustomerBankTransfers">
                <ListItemText primary="vwReportCustomerBankTransfers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCustomerBankTransferDatas">
                <ListItemText primary="vwReportCustomerBankTransferDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCustomerCreditCardTransactionss">
                <ListItemText primary="vwReportCustomerCreditCardTransactionss Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCustomerCreditCardTransactionsDatas">
                <ListItemText primary="vwReportCustomerCreditCardTransactionsDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCustomerWalletBalances">
                <ListItemText primary="vwReportCustomerWalletBalances Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCustomerWalletBalanceDatas">
                <ListItemText primary="vwReportCustomerWalletBalanceDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCustomerWalletBalanceForLawyers">
                <ListItemText primary="vwReportCustomerWalletBalanceForLawyers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportCustomerWalletBalanceForLawyerDatas">
                <ListItemText primary="vwReportCustomerWalletBalanceForLawyerDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportDistributorObligoBalances">
                <ListItemText primary="vwReportDistributorObligoBalances Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportDistributorObligoBalanceDatas">
                <ListItemText primary="vwReportDistributorObligoBalanceDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportFromChangers">
                <ListItemText primary="vwReportFromChangers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportFromChangerDatas">
                <ListItemText primary="vwReportFromChangerDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportLoadTransferToDistributors">
                <ListItemText primary="vwReportLoadTransferToDistributors Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportLoadTransferToDistributorDatas">
                <ListItemText primary="vwReportLoadTransferToDistributorDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportMonthlyCardFees">
                <ListItemText primary="vwReportMonthlyCardFees Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportMonthlyCardFeeDatas">
                <ListItemText primary="vwReportMonthlyCardFeeDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportToChangers">
                <ListItemText primary="vwReportToChangers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportToChangerDatas">
                <ListItemText primary="vwReportToChangerDatas Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportWalletTransfers">
                <ListItemText primary="vwReportWalletTransfers Report" />
              </ListItemButton>
            </ListItem>
            <ListItem disablePadding>
              <ListItemButton component={Link} to="/vwReportWalletTransferDatas">
                <ListItemText primary="vwReportWalletTransferDatas Report" />
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
            <Route path="/indexFragmentations" element={<IndexFragmentationList />} />
            <Route path="/indexFragmentations/:id" element={<IndexFragmentationDetail />} />
            <Route path="/indexFragmentationDatas" element={<IndexFragmentationDataList />} />
            <Route path="/indexFragmentationDatas/:id" element={<IndexFragmentationDataDetail />} />
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
            <Route path="/tableSizes" element={<TableSizeList />} />
            <Route path="/tableSizes/:id" element={<TableSizeDetail />} />
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
            <Route path="/ccvwComboListActiveRestrictionss" element={<CcvwComboListActiveRestrictionsList />} />
            <Route path="/ccvwComboListActiveRestrictionss/:id" element={<CcvwComboListActiveRestrictionsDetail />} />
            <Route path="/ccvwComboListCAlertMessages" element={<CcvwComboListCAlertMessageList />} />
            <Route path="/ccvwComboListCAlertMessages/:id" element={<CcvwComboListCAlertMessageDetail />} />
            <Route path="/ccvwComboListCEnumerations" element={<CcvwComboListCEnumerationList />} />
            <Route path="/ccvwComboListCEnumerations/:id" element={<CcvwComboListCEnumerationDetail />} />
            <Route path="/ccvwComboListCJobs" element={<CcvwComboListCJobList />} />
            <Route path="/ccvwComboListCJobs/:id" element={<CcvwComboListCJobDetail />} />
            <Route path="/ccvwComboListCLanguages" element={<CcvwComboListCLanguageList />} />
            <Route path="/ccvwComboListCLanguages/:id" element={<CcvwComboListCLanguageDetail />} />
            <Route path="/ccvwComboListCLoggedAlerts" element={<CcvwComboListCLoggedAlertList />} />
            <Route path="/ccvwComboListCLoggedAlerts/:id" element={<CcvwComboListCLoggedAlertDetail />} />
            <Route path="/ccvwComboListCLoggedAlertForAffectedUsers" element={<CcvwComboListCLoggedAlertForAffectedUserList />} />
            <Route path="/ccvwComboListCLoggedAlertForAffectedUsers/:id" element={<CcvwComboListCLoggedAlertForAffectedUserDetail />} />
            <Route path="/ccvwComboListCLoggedLogins" element={<CcvwComboListCLoggedLoginList />} />
            <Route path="/ccvwComboListCLoggedLogins/:id" element={<CcvwComboListCLoggedLoginDetail />} />
            <Route path="/ccvwComboListCLookups" element={<CcvwComboListCLookupList />} />
            <Route path="/ccvwComboListCLookups/:id" element={<CcvwComboListCLookupDetail />} />
            <Route path="/ccvwComboListCMfas" element={<CcvwComboListCMfaList />} />
            <Route path="/ccvwComboListCMfas/:id" element={<CcvwComboListCMfaDetail />} />
            <Route path="/ccvwComboListCObjectToTranslates" element={<CcvwComboListCObjectToTranslateList />} />
            <Route path="/ccvwComboListCObjectToTranslates/:id" element={<CcvwComboListCObjectToTranslateDetail />} />
            <Route path="/ccvwComboListCProcesss" element={<CcvwComboListCProcessList />} />
            <Route path="/ccvwComboListCProcesss/:id" element={<CcvwComboListCProcessDetail />} />
            <Route path="/ccvwComboListCRoles" element={<CcvwComboListCRoleList />} />
            <Route path="/ccvwComboListCRoles/:id" element={<CcvwComboListCRoleDetail />} />
            <Route path="/ccvwComboListCSystemDefaults" element={<CcvwComboListCSystemDefaultList />} />
            <Route path="/ccvwComboListCSystemDefaults/:id" element={<CcvwComboListCSystemDefaultDetail />} />
            <Route path="/ccvwComboListCTables" element={<CcvwComboListCTableList />} />
            <Route path="/ccvwComboListCTables/:id" element={<CcvwComboListCTableDetail />} />
            <Route path="/ccvwComboListCUsers" element={<CcvwComboListCUserList />} />
            <Route path="/ccvwComboListCUsers/:id" element={<CcvwComboListCUserDetail />} />
            <Route path="/ccvwComboListCards" element={<CcvwComboListCardList />} />
            <Route path="/ccvwComboListCards/:id" element={<CcvwComboListCardDetail />} />
            <Route path="/ccvwComboListCardActivations" element={<CcvwComboListCardActivationList />} />
            <Route path="/ccvwComboListCardActivations/:id" element={<CcvwComboListCardActivationDetail />} />
            <Route path="/ccvwComboListCardActivationForCards" element={<CcvwComboListCardActivationForCardList />} />
            <Route path="/ccvwComboListCardActivationForCards/:id" element={<CcvwComboListCardActivationForCardDetail />} />
            <Route path="/ccvwComboListCardActivationForCustomers" element={<CcvwComboListCardActivationForCustomerList />} />
            <Route path="/ccvwComboListCardActivationForCustomers/:id" element={<CcvwComboListCardActivationForCustomerDetail />} />
            <Route path="/ccvwComboListCardActivationForDistributors" element={<CcvwComboListCardActivationForDistributorList />} />
            <Route path="/ccvwComboListCardActivationForDistributors/:id" element={<CcvwComboListCardActivationForDistributorDetail />} />
            <Route path="/ccvwComboListCardClubs" element={<CcvwComboListCardClubList />} />
            <Route path="/ccvwComboListCardClubs/:id" element={<CcvwComboListCardClubDetail />} />
            <Route path="/ccvwComboListCardClubForAssignedCompanyCustomers" element={<CcvwComboListCardClubForAssignedCompanyCustomerList />} />
            <Route path="/ccvwComboListCardClubForAssignedCompanyCustomers/:id" element={<CcvwComboListCardClubForAssignedCompanyCustomerDetail />} />
            <Route path="/ccvwComboListCardForCustomers" element={<CcvwComboListCardForCustomerList />} />
            <Route path="/ccvwComboListCardForCustomers/:id" element={<CcvwComboListCardForCustomerDetail />} />
            <Route path="/ccvwComboListCardForOwningDistributors" element={<CcvwComboListCardForOwningDistributorList />} />
            <Route path="/ccvwComboListCardForOwningDistributors/:id" element={<CcvwComboListCardForOwningDistributorDetail />} />
            <Route path="/ccvwComboListCardForPreviousDistributors" element={<CcvwComboListCardForPreviousDistributorList />} />
            <Route path="/ccvwComboListCardForPreviousDistributors/:id" element={<CcvwComboListCardForPreviousDistributorDetail />} />
            <Route path="/ccvwComboListCardOrderByDistributors" element={<CcvwComboListCardOrderByDistributorList />} />
            <Route path="/ccvwComboListCardOrderByDistributors/:id" element={<CcvwComboListCardOrderByDistributorDetail />} />
            <Route path="/ccvwComboListCardOrderByDistributorForDistributors" element={<CcvwComboListCardOrderByDistributorForDistributorList />} />
            <Route path="/ccvwComboListCardOrderByDistributorForDistributors/:id" element={<CcvwComboListCardOrderByDistributorForDistributorDetail />} />
            <Route path="/ccvwComboListCardOrderByDistributorForOrderedByUsers" element={<CcvwComboListCardOrderByDistributorForOrderedByUserList />} />
            <Route path="/ccvwComboListCardOrderByDistributorForOrderedByUsers/:id" element={<CcvwComboListCardOrderByDistributorForOrderedByUserDetail />} />
            <Route path="/ccvwComboListCardOrderFromIssuers" element={<CcvwComboListCardOrderFromIssuerList />} />
            <Route path="/ccvwComboListCardOrderFromIssuers/:id" element={<CcvwComboListCardOrderFromIssuerDetail />} />
            <Route path="/ccvwComboListCardOrderFromIssuerForOrderedByUsers" element={<CcvwComboListCardOrderFromIssuerForOrderedByUserList />} />
            <Route path="/ccvwComboListCardOrderFromIssuerForOrderedByUsers/:id" element={<CcvwComboListCardOrderFromIssuerForOrderedByUserDetail />} />
            <Route path="/ccvwComboListCountrys" element={<CcvwComboListCountryList />} />
            <Route path="/ccvwComboListCountrys/:id" element={<CcvwComboListCountryDetail />} />
            <Route path="/ccvwComboListCustomers" element={<CcvwComboListCustomerList />} />
            <Route path="/ccvwComboListCustomers/:id" element={<CcvwComboListCustomerDetail />} />
            <Route path="/ccvwComboListCustomerCardTransactions" element={<CcvwComboListCustomerCardTransactionList />} />
            <Route path="/ccvwComboListCustomerCardTransactions/:id" element={<CcvwComboListCustomerCardTransactionDetail />} />
            <Route path="/ccvwComboListCustomerCardTransactionForCards" element={<CcvwComboListCustomerCardTransactionForCardList />} />
            <Route path="/ccvwComboListCustomerCardTransactionForCards/:id" element={<CcvwComboListCustomerCardTransactionForCardDetail />} />
            <Route path="/ccvwComboListCustomerCardTransactionForCustomers" element={<CcvwComboListCustomerCardTransactionForCustomerList />} />
            <Route path="/ccvwComboListCustomerCardTransactionForCustomers/:id" element={<CcvwComboListCustomerCardTransactionForCustomerDetail />} />
            <Route path="/ccvwComboListCustomerCardTransactionForlgOwningDistributors" element={<CcvwComboListCustomerCardTransactionForlgOwningDistributorList />} />
            <Route path="/ccvwComboListCustomerCardTransactionForlgOwningDistributors/:id" element={<CcvwComboListCustomerCardTransactionForlgOwningDistributorDetail />} />
            <Route path="/ccvwComboListCustomerClubs" element={<CcvwComboListCustomerClubList />} />
            <Route path="/ccvwComboListCustomerClubs/:id" element={<CcvwComboListCustomerClubDetail />} />
            <Route path="/ccvwComboListCustomerCreditCards" element={<CcvwComboListCustomerCreditCardList />} />
            <Route path="/ccvwComboListCustomerCreditCards/:id" element={<CcvwComboListCustomerCreditCardDetail />} />
            <Route path="/ccvwComboListCustomerCreditCardForCustomers" element={<CcvwComboListCustomerCreditCardForCustomerList />} />
            <Route path="/ccvwComboListCustomerCreditCardForCustomers/:id" element={<CcvwComboListCustomerCreditCardForCustomerDetail />} />
            <Route path="/ccvwComboListCustomerForAssignedDistributors" element={<CcvwComboListCustomerForAssignedDistributorList />} />
            <Route path="/ccvwComboListCustomerForAssignedDistributors/:id" element={<CcvwComboListCustomerForAssignedDistributorDetail />} />
            <Route path="/ccvwComboListCustomerPartnerships" element={<CcvwComboListCustomerPartnershipList />} />
            <Route path="/ccvwComboListCustomerPartnerships/:id" element={<CcvwComboListCustomerPartnershipDetail />} />
            <Route path="/ccvwComboListCustomerPartnershipForCustomers" element={<CcvwComboListCustomerPartnershipForCustomerList />} />
            <Route path="/ccvwComboListCustomerPartnershipForCustomers/:id" element={<CcvwComboListCustomerPartnershipForCustomerDetail />} />
            <Route path="/ccvwComboListCustomerWallets" element={<CcvwComboListCustomerWalletList />} />
            <Route path="/ccvwComboListCustomerWallets/:id" element={<CcvwComboListCustomerWalletDetail />} />
            <Route path="/ccvwComboListCustomerWalletForCustomers" element={<CcvwComboListCustomerWalletForCustomerList />} />
            <Route path="/ccvwComboListCustomerWalletForCustomers/:id" element={<CcvwComboListCustomerWalletForCustomerDetail />} />
            <Route path="/ccvwComboListCustomerWalletForWhoInitiateds" element={<CcvwComboListCustomerWalletForWhoInitiatedList />} />
            <Route path="/ccvwComboListCustomerWalletForWhoInitiateds/:id" element={<CcvwComboListCustomerWalletForWhoInitiatedDetail />} />
            <Route path="/ccvwComboListDistributors" element={<CcvwComboListDistributorList />} />
            <Route path="/ccvwComboListDistributors/:id" element={<CcvwComboListDistributorDetail />} />
            <Route path="/ccvwComboListEntityRiskColors" element={<CcvwComboListEntityRiskColorList />} />
            <Route path="/ccvwComboListEntityRiskColors/:id" element={<CcvwComboListEntityRiskColorDetail />} />
            <Route path="/ccvwComboListFees" element={<CcvwComboListFeeList />} />
            <Route path="/ccvwComboListFees/:id" element={<CcvwComboListFeeDetail />} />
            <Route path="/ccvwComboListGovtReport00s" element={<CcvwComboListGovtReport00List />} />
            <Route path="/ccvwComboListGovtReport00s/:id" element={<CcvwComboListGovtReport00Detail />} />
            <Route path="/ccvwComboListImportTransactionLoads" element={<CcvwComboListImportTransactionLoadList />} />
            <Route path="/ccvwComboListImportTransactionLoads/:id" element={<CcvwComboListImportTransactionLoadDetail />} />
            <Route path="/ccvwComboListImportTransactionLoadForDistributors" element={<CcvwComboListImportTransactionLoadForDistributorList />} />
            <Route path="/ccvwComboListImportTransactionLoadForDistributors/:id" element={<CcvwComboListImportTransactionLoadForDistributorDetail />} />
            <Route path="/ccvwComboListImportTransactionLoadForInitiatedByUsers" element={<CcvwComboListImportTransactionLoadForInitiatedByUserList />} />
            <Route path="/ccvwComboListImportTransactionLoadForInitiatedByUsers/:id" element={<CcvwComboListImportTransactionLoadForInitiatedByUserDetail />} />
            <Route path="/ccvwComboListLimitations" element={<CcvwComboListLimitationList />} />
            <Route path="/ccvwComboListLimitations/:id" element={<CcvwComboListLimitationDetail />} />
            <Route path="/ccvwComboListMCCBReceivers" element={<CcvwComboListMCCBReceiverList />} />
            <Route path="/ccvwComboListMCCBReceivers/:id" element={<CcvwComboListMCCBReceiverDetail />} />
            <Route path="/ccvwComboListMCCBReceiverAccounts" element={<CcvwComboListMCCBReceiverAccountList />} />
            <Route path="/ccvwComboListMCCBReceiverAccounts/:id" element={<CcvwComboListMCCBReceiverAccountDetail />} />
            <Route path="/ccvwComboListMCCBReceiverForCustomers" element={<CcvwComboListMCCBReceiverForCustomerList />} />
            <Route path="/ccvwComboListMCCBReceiverForCustomers/:id" element={<CcvwComboListMCCBReceiverForCustomerDetail />} />
            <Route path="/ccvwComboListMCCBTransactions" element={<CcvwComboListMCCBTransactionList />} />
            <Route path="/ccvwComboListMCCBTransactions/:id" element={<CcvwComboListMCCBTransactionDetail />} />
            <Route path="/ccvwComboListMCCBTransactionForCustomers" element={<CcvwComboListMCCBTransactionForCustomerList />} />
            <Route path="/ccvwComboListMCCBTransactionForCustomers/:id" element={<CcvwComboListMCCBTransactionForCustomerDetail />} />
            <Route path="/ccvwComboListNewCardRequests" element={<CcvwComboListNewCardRequestList />} />
            <Route path="/ccvwComboListNewCardRequests/:id" element={<CcvwComboListNewCardRequestDetail />} />
            <Route path="/ccvwComboListNewCardRequestAPIs" element={<CcvwComboListNewCardRequestAPIList />} />
            <Route path="/ccvwComboListNewCardRequestAPIs/:id" element={<CcvwComboListNewCardRequestAPIDetail />} />
            <Route path="/ccvwComboListNewCardRequestAPIForDistributors" element={<CcvwComboListNewCardRequestAPIForDistributorList />} />
            <Route path="/ccvwComboListNewCardRequestAPIForDistributors/:id" element={<CcvwComboListNewCardRequestAPIForDistributorDetail />} />
            <Route path="/ccvwComboListNewCardRequestForCustomers" element={<CcvwComboListNewCardRequestForCustomerList />} />
            <Route path="/ccvwComboListNewCardRequestForCustomers/:id" element={<CcvwComboListNewCardRequestForCustomerDetail />} />
            <Route path="/ccvwComboListNewCardRequestForDistributorSentTos" element={<CcvwComboListNewCardRequestForDistributorSentToList />} />
            <Route path="/ccvwComboListNewCardRequestForDistributorSentTos/:id" element={<CcvwComboListNewCardRequestForDistributorSentToDetail />} />
            <Route path="/ccvwComboListNonCustomers" element={<CcvwComboListNonCustomerList />} />
            <Route path="/ccvwComboListNonCustomers/:id" element={<CcvwComboListNonCustomerDetail />} />
            <Route path="/ccvwComboListRequestedTransactions" element={<CcvwComboListRequestedTransactionList />} />
            <Route path="/ccvwComboListRequestedTransactions/:id" element={<CcvwComboListRequestedTransactionDetail />} />
            <Route path="/ccvwComboListRequestedTransactionForDistributors" element={<CcvwComboListRequestedTransactionForDistributorList />} />
            <Route path="/ccvwComboListRequestedTransactionForDistributors/:id" element={<CcvwComboListRequestedTransactionForDistributorDetail />} />
            <Route path="/ccvwComboListRequestedTransactionForInitiatingCustomers" element={<CcvwComboListRequestedTransactionForInitiatingCustomerList />} />
            <Route path="/ccvwComboListRequestedTransactionForInitiatingCustomers/:id" element={<CcvwComboListRequestedTransactionForInitiatingCustomerDetail />} />
            <Route path="/ccvwComboListRequestedTransactionForPayingCustomers" element={<CcvwComboListRequestedTransactionForPayingCustomerList />} />
            <Route path="/ccvwComboListRequestedTransactionForPayingCustomers/:id" element={<CcvwComboListRequestedTransactionForPayingCustomerDetail />} />
            <Route path="/ccvwComboListRequestedTransactionForPrepaidCards" element={<CcvwComboListRequestedTransactionForPrepaidCardList />} />
            <Route path="/ccvwComboListRequestedTransactionForPrepaidCards/:id" element={<CcvwComboListRequestedTransactionForPrepaidCardDetail />} />
            <Route path="/ccvwComboListRequestedTransactionForReceivingCustomers" element={<CcvwComboListRequestedTransactionForReceivingCustomerList />} />
            <Route path="/ccvwComboListRequestedTransactionForReceivingCustomers/:id" element={<CcvwComboListRequestedTransactionForReceivingCustomerDetail />} />
            <Route path="/ccvwComboListRequestedTransactionForTargetCustomers" element={<CcvwComboListRequestedTransactionForTargetCustomerList />} />
            <Route path="/ccvwComboListRequestedTransactionForTargetCustomers/:id" element={<CcvwComboListRequestedTransactionForTargetCustomerDetail />} />
            <Route path="/ccvwComboListRiskEvents" element={<CcvwComboListRiskEventList />} />
            <Route path="/ccvwComboListRiskEvents/:id" element={<CcvwComboListRiskEventDetail />} />
            <Route path="/ccvwComboListRiskRules" element={<CcvwComboListRiskRuleList />} />
            <Route path="/ccvwComboListRiskRules/:id" element={<CcvwComboListRiskRuleDetail />} />
            <Route path="/ccvwComboListRiskRuleLogs" element={<CcvwComboListRiskRuleLogList />} />
            <Route path="/ccvwComboListRiskRuleLogs/:id" element={<CcvwComboListRiskRuleLogDetail />} />
            <Route path="/ccvwComboListRiskRuleOverrides" element={<CcvwComboListRiskRuleOverrideList />} />
            <Route path="/ccvwComboListRiskRuleOverrides/:id" element={<CcvwComboListRiskRuleOverrideDetail />} />
            <Route path="/ccvwComboListTransactionBits" element={<CcvwComboListTransactionBitList />} />
            <Route path="/ccvwComboListTransactionBits/:id" element={<CcvwComboListTransactionBitDetail />} />
            <Route path="/ccvwComboListTransactionBitForCustomers" element={<CcvwComboListTransactionBitForCustomerList />} />
            <Route path="/ccvwComboListTransactionBitForCustomers/:id" element={<CcvwComboListTransactionBitForCustomerDetail />} />
            <Route path="/ccvwComboListTransactionCreditCards" element={<CcvwComboListTransactionCreditCardList />} />
            <Route path="/ccvwComboListTransactionCreditCards/:id" element={<CcvwComboListTransactionCreditCardDetail />} />
            <Route path="/ccvwComboListTransactionCreditCardForCustomers" element={<CcvwComboListTransactionCreditCardForCustomerList />} />
            <Route path="/ccvwComboListTransactionCreditCardForCustomers/:id" element={<CcvwComboListTransactionCreditCardForCustomerDetail />} />
            <Route path="/ccvwComboListTransactionLoads" element={<CcvwComboListTransactionLoadList />} />
            <Route path="/ccvwComboListTransactionLoads/:id" element={<CcvwComboListTransactionLoadDetail />} />
            <Route path="/ccvwComboListTransactionLoadAccounts" element={<CcvwComboListTransactionLoadAccountList />} />
            <Route path="/ccvwComboListTransactionLoadAccounts/:id" element={<CcvwComboListTransactionLoadAccountDetail />} />
            <Route path="/ccvwComboListTransactionLoadForCards" element={<CcvwComboListTransactionLoadForCardList />} />
            <Route path="/ccvwComboListTransactionLoadForCards/:id" element={<CcvwComboListTransactionLoadForCardDetail />} />
            <Route path="/ccvwComboListTransactionLoadForCustomers" element={<CcvwComboListTransactionLoadForCustomerList />} />
            <Route path="/ccvwComboListTransactionLoadForCustomers/:id" element={<CcvwComboListTransactionLoadForCustomerDetail />} />
            <Route path="/ccvwComboListTransactionLoadForDistributors" element={<CcvwComboListTransactionLoadForDistributorList />} />
            <Route path="/ccvwComboListTransactionLoadForDistributors/:id" element={<CcvwComboListTransactionLoadForDistributorDetail />} />
            <Route path="/ccvwComboListTransactionLoadForUsers" element={<CcvwComboListTransactionLoadForUserList />} />
            <Route path="/ccvwComboListTransactionLoadForUsers/:id" element={<CcvwComboListTransactionLoadForUserDetail />} />
            <Route path="/ccvwComboListUITexts" element={<CcvwComboListUITextList />} />
            <Route path="/ccvwComboListUITexts/:id" element={<CcvwComboListUITextDetail />} />
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
            <Route path="/mnccvwComboListCardForCompanyCustomers" element={<MnccvwComboListCardForCompanyCustomerList />} />
            <Route path="/mnccvwComboListCardForCompanyCustomers/:id" element={<MnccvwComboListCardForCompanyCustomerDetail />} />
            <Route path="/mnccvwComboListCardForCompanyCustomerDistributors" element={<MnccvwComboListCardForCompanyCustomerDistributorList />} />
            <Route path="/mnccvwComboListCardForCompanyCustomerDistributors/:id" element={<MnccvwComboListCardForCompanyCustomerDistributorDetail />} />
            <Route path="/mnvwComboListCustomerWithPhoneAndIdentifiers" element={<MnvwComboListCustomerWithPhoneAndIdentifierList />} />
            <Route path="/mnvwComboListCustomerWithPhoneAndIdentifiers/:id" element={<MnvwComboListCustomerWithPhoneAndIdentifierDetail />} />
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
            <Route path="/vwCustomerCardStatuss" element={<vwCustomerCardStatusList />} />
            <Route path="/vwCustomerCardStatuss/:id" element={<vwCustomerCardStatusDetail />} />
            <Route path="/vwCustomerCardStatusDatas" element={<vwCustomerCardStatusDataList />} />
            <Route path="/vwCustomerCardStatusDatas/:id" element={<vwCustomerCardStatusDataDetail />} />
            <Route path="/vwCustomerSecurityStatusChecks" element={<vwCustomerSecurityStatusCheckList />} />
            <Route path="/vwCustomerSecurityStatusChecks/:id" element={<vwCustomerSecurityStatusCheckDetail />} />
            <Route path="/vwCustomerSecurityStatusCheckDatas" element={<vwCustomerSecurityStatusCheckDataList />} />
            <Route path="/vwCustomerSecurityStatusCheckDatas/:id" element={<vwCustomerSecurityStatusCheckDataDetail />} />
            <Route path="/vwOnlineCardTransactions" element={<vwOnlineCardTransactionList />} />
            <Route path="/vwOnlineCardTransactions/:id" element={<vwOnlineCardTransactionDetail />} />
            <Route path="/vwReportCardActivitys" element={<vwReportCardActivityList />} />
            <Route path="/vwReportCardActivitys/:id" element={<vwReportCardActivityDetail />} />
            <Route path="/vwReportCardActivityDatas" element={<vwReportCardActivityDataList />} />
            <Route path="/vwReportCardActivityDatas/:id" element={<vwReportCardActivityDataDetail />} />
            <Route path="/vwReportCustomerBankTransfers" element={<vwReportCustomerBankTransferList />} />
            <Route path="/vwReportCustomerBankTransfers/:id" element={<vwReportCustomerBankTransferDetail />} />
            <Route path="/vwReportCustomerBankTransferDatas" element={<vwReportCustomerBankTransferDataList />} />
            <Route path="/vwReportCustomerBankTransferDatas/:id" element={<vwReportCustomerBankTransferDataDetail />} />
            <Route path="/vwReportCustomerCreditCardTransactionss" element={<vwReportCustomerCreditCardTransactionsList />} />
            <Route path="/vwReportCustomerCreditCardTransactionss/:id" element={<vwReportCustomerCreditCardTransactionsDetail />} />
            <Route path="/vwReportCustomerCreditCardTransactionsDatas" element={<vwReportCustomerCreditCardTransactionsDataList />} />
            <Route path="/vwReportCustomerCreditCardTransactionsDatas/:id" element={<vwReportCustomerCreditCardTransactionsDataDetail />} />
            <Route path="/vwReportCustomerWalletBalances" element={<vwReportCustomerWalletBalanceList />} />
            <Route path="/vwReportCustomerWalletBalances/:id" element={<vwReportCustomerWalletBalanceDetail />} />
            <Route path="/vwReportCustomerWalletBalanceDatas" element={<vwReportCustomerWalletBalanceDataList />} />
            <Route path="/vwReportCustomerWalletBalanceDatas/:id" element={<vwReportCustomerWalletBalanceDataDetail />} />
            <Route path="/vwReportCustomerWalletBalanceForLawyers" element={<vwReportCustomerWalletBalanceForLawyerList />} />
            <Route path="/vwReportCustomerWalletBalanceForLawyers/:id" element={<vwReportCustomerWalletBalanceForLawyerDetail />} />
            <Route path="/vwReportCustomerWalletBalanceForLawyerDatas" element={<vwReportCustomerWalletBalanceForLawyerDataList />} />
            <Route path="/vwReportCustomerWalletBalanceForLawyerDatas/:id" element={<vwReportCustomerWalletBalanceForLawyerDataDetail />} />
            <Route path="/vwReportDistributorObligoBalances" element={<vwReportDistributorObligoBalanceList />} />
            <Route path="/vwReportDistributorObligoBalances/:id" element={<vwReportDistributorObligoBalanceDetail />} />
            <Route path="/vwReportDistributorObligoBalanceDatas" element={<vwReportDistributorObligoBalanceDataList />} />
            <Route path="/vwReportDistributorObligoBalanceDatas/:id" element={<vwReportDistributorObligoBalanceDataDetail />} />
            <Route path="/vwReportFromChangers" element={<vwReportFromChangerList />} />
            <Route path="/vwReportFromChangers/:id" element={<vwReportFromChangerDetail />} />
            <Route path="/vwReportFromChangerDatas" element={<vwReportFromChangerDataList />} />
            <Route path="/vwReportFromChangerDatas/:id" element={<vwReportFromChangerDataDetail />} />
            <Route path="/vwReportLoadTransferToDistributors" element={<vwReportLoadTransferToDistributorList />} />
            <Route path="/vwReportLoadTransferToDistributors/:id" element={<vwReportLoadTransferToDistributorDetail />} />
            <Route path="/vwReportLoadTransferToDistributorDatas" element={<vwReportLoadTransferToDistributorDataList />} />
            <Route path="/vwReportLoadTransferToDistributorDatas/:id" element={<vwReportLoadTransferToDistributorDataDetail />} />
            <Route path="/vwReportMonthlyCardFees" element={<vwReportMonthlyCardFeeList />} />
            <Route path="/vwReportMonthlyCardFees/:id" element={<vwReportMonthlyCardFeeDetail />} />
            <Route path="/vwReportMonthlyCardFeeDatas" element={<vwReportMonthlyCardFeeDataList />} />
            <Route path="/vwReportMonthlyCardFeeDatas/:id" element={<vwReportMonthlyCardFeeDataDetail />} />
            <Route path="/vwReportToChangers" element={<vwReportToChangerList />} />
            <Route path="/vwReportToChangers/:id" element={<vwReportToChangerDetail />} />
            <Route path="/vwReportToChangerDatas" element={<vwReportToChangerDataList />} />
            <Route path="/vwReportToChangerDatas/:id" element={<vwReportToChangerDataDetail />} />
            <Route path="/vwReportWalletTransfers" element={<vwReportWalletTransferList />} />
            <Route path="/vwReportWalletTransfers/:id" element={<vwReportWalletTransferDetail />} />
            <Route path="/vwReportWalletTransferDatas" element={<vwReportWalletTransferDataList />} />
            <Route path="/vwReportWalletTransferDatas/:id" element={<vwReportWalletTransferDataDetail />} />
          </Routes>
        </Container>
      </Box>
    </Box>
  );
}

export default App;
