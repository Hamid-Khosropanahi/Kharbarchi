using Kharbarchi.Shared.Security;

namespace Kharbarchi.Server.Security;

public static class AuthorizationPolicyNames
{
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string AdminOnly = "AdminOnly";

    public const string CatalogRead = "CatalogRead";
    public const string CatalogWrite = "CatalogWrite";
    public const string PriceRead = "PriceRead";
    public const string PurchasePriceRead = "PurchasePriceRead";
    public const string PriceProposalCreate = "PriceProposalCreate";
    public const string PriceProposalManagerApproval = "PriceProposalManagerApproval";
    public const string PriceProposalFinalApproval = "PriceProposalFinalApproval";
    public const string StockRead = "StockRead";
    public const string InventoryProposalCreate = "InventoryProposalCreate";
    public const string InventoryProposalManagerApproval = "InventoryProposalManagerApproval";
    public const string InventoryProposalFinalApproval = "InventoryProposalFinalApproval";

    public const string ProductImportRead = "ProductImportRead";
    public const string ProductImportWrite = KharbarchiPolicies.ProductImportWrite;
    public const string OrdersRead = "OrdersRead";
    public const string OrdersImportWrite = "OrdersImportWrite";
    public const string OrderPaymentWorkflow = "OrderPaymentWorkflow";
    public const string BarookPaymentOperator = "BarookPaymentOperator";
    public const string AccountingOrdersRead = "AccountingOrdersRead";
    public const string ManualReceiptCreate = "ManualReceiptCreate";

    public const string CentralSyncAgentOnly = "CentralSyncAgentOnly";
    public const string GatewayAdminOnly = "GatewayAdminOnly";
}
