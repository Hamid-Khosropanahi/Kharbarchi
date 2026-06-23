namespace Kharbarchi.Shared.Security;

public static class KharbarchiRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string PricingManager = "PricingManager";
    public const string PricingEmployee = "PricingEmployee";
    public const string WarehouseEmployee = "WarehouseEmployee";
    public const string SalesManager = "SalesManager";
    public const string ShippingOrderManager = "ShippingOrderManager";
    public const string Accountant = "Accountant";
    public const string CentralSyncAgent = "CentralSyncAgent";
    public const string GatewayAdmin = "GatewayAdmin";
    public const string Customer = "Customer";

    // Backward compatibility with older code. New code should prefer SuperAdmin.
    public const string LegacyAdmin = "Admin";

    public static readonly string[] AllSystemRoles =
    [
        SuperAdmin,
        PricingManager,
        PricingEmployee,
        WarehouseEmployee,
        SalesManager,
        ShippingOrderManager,
        Accountant,
        CentralSyncAgent,
        GatewayAdmin,
        Customer,
        LegacyAdmin
    ];

    public static readonly string[] ProductViewers =
    [
        SuperAdmin,
        LegacyAdmin,
        PricingManager,
        SalesManager
    ];

    public static readonly string[] OrderViewers =
    [
        SuperAdmin,
        LegacyAdmin,
        PricingManager,
        SalesManager,
        ShippingOrderManager,
        Accountant
    ];

    public static readonly string[] OrderPaymentOperators =
    [
        SuperAdmin,
        LegacyAdmin,
        ShippingOrderManager,
        Accountant
    ];

    public static readonly string[] BarookPaymentOperators =
    [
        SuperAdmin,
        LegacyAdmin,
        ShippingOrderManager
    ];

    public const string CatalogWriters = SuperAdmin + "," + PricingManager;
    public const string PriceProposers = SuperAdmin + "," + PricingManager + "," + PricingEmployee;
    public const string PriceApprovers = SuperAdmin + "," + PricingManager;
    public const string PurchasePriceReaders = SuperAdmin + "," + PricingManager;
    public const string StockReaders = SuperAdmin + "," + PricingManager + "," + WarehouseEmployee;
    public const string InventoryProposers = SuperAdmin + "," + PricingManager + "," + WarehouseEmployee;
}
