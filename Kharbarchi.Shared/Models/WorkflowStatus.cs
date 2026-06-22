namespace Kharbarchi.Shared.Models;

public enum WorkflowStatus
{
    Draft = 0,
    Submitted = 10,
    ManagerApproved = 20,
    SuperAdminApproved = 30,
    QueuedForSync = 40,
    Synced = 50,
    Rejected = 90,
    Canceled = 95
}

public enum InventoryAdjustmentKind
{
    SetAbsoluteStock = 1,
    IncreaseStock = 2,
    DecreaseStock = 3,
    Shortage = 4,
    Excess = 5
}

public enum OutboxStatus
{
    Pending = 10,
    Locked = 20,
    Sent = 30,
    Failed = 90,
    Canceled = 95
}
