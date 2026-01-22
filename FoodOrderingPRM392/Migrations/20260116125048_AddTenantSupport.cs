using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrderingPRM392.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create Tenants table
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            // Step 2: Insert default tenant
            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreateTime", "Name", "UpdateTime" },
                values: new object[] { 1, new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Default Tenant", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc) });

            // Step 3: Add TenantId column with default value 1
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Step 4: Update existing stores to have TenantId = 1
            migrationBuilder.Sql("UPDATE Stores SET TenantId = 1 WHERE TenantId = 0");

            // Step 5: Create index
            migrationBuilder.CreateIndex(
                name: "IX_Stores_TenantId",
                table: "Stores",
                column: "TenantId");

            // Step 6: Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Stores_Tenants_TenantId",
                table: "Stores",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_Tenants_TenantId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Stores_TenantId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Stores");
        }
    }
}
