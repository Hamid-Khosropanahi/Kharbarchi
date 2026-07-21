using Kharbarchi.Server.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kharbarchi.Server.Tests;

[TestClass]
public sealed class ErpMigrationSafetyTests
{
    [TestMethod]
    public void ErpWorkflowMigration_Up_IsAdditiveOnly()
    {
        var operations = new InspectableErpMigration().BuildUp();

        Assert.IsFalse(operations.Any(operation => operation is DropTableOperation
            or DropColumnOperation
            or DeleteDataOperation
            or AlterColumnOperation
            or RenameTableOperation
            or RenameColumnOperation));
    }

    [TestMethod]
    public void ErpWorkflowMigration_AddsRequiredCustomerAndHistorySchema()
    {
        var operations = new InspectableErpMigration().BuildUp();

        Assert.IsTrue(operations.OfType<AddColumnOperation>().Any(x => x.Table == "cbi_customers" && x.Name == "CreatedAtUtc"));
        Assert.IsTrue(operations.OfType<AddColumnOperation>().Any(x => x.Table == "cbi_customers" && x.Name == "CreditLimit"));
        Assert.IsTrue(operations.OfType<AddColumnOperation>().Any(x => x.Table == "com_orders" && x.Name == "DeliveryAddressLine"));
        Assert.IsTrue(operations.OfType<CreateTableOperation>().Any(x => x.Name == "cbi_customercredithistory"));
        Assert.IsTrue(operations.OfType<CreateTableOperation>().Any(x => x.Name == "prc_productpricehistory"));
    }

    private sealed class InspectableErpMigration : AddErpCustomersCreditSalesWorkflow20260721
    {
        public IReadOnlyList<MigrationOperation> BuildUp()
        {
            var builder = new MigrationBuilder("MySQL");
            base.Up(builder);
            return builder.Operations;
        }
    }
}
