using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodOrderingPRM392.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodSizeToFoodStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FoodStores_StoreId_FoodId",
                table: "FoodStores");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "FoodStores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SizeId",
                table: "FoodStores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasSize",
                table: "Foods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FoodSizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodSizes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FoodSizes",
                columns: new[] { "Id", "Description", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Nhỏ", "S", 1 },
                    { 2, "Vừa", "M", 2 },
                    { 3, "Lớn", "L", 3 },
                    { 4, "Siêu lớn", "XL", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodStores_SizeId",
                table: "FoodStores",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodStores_StoreId_FoodId_SizeId",
                table: "FoodStores",
                columns: new[] { "StoreId", "FoodId", "SizeId" },
                unique: true,
                filter: "[SizeId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_FoodStores_FoodSizes_SizeId",
                table: "FoodStores",
                column: "SizeId",
                principalTable: "FoodSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoodStores_FoodSizes_SizeId",
                table: "FoodStores");

            migrationBuilder.DropTable(
                name: "FoodSizes");

            migrationBuilder.DropIndex(
                name: "IX_FoodStores_SizeId",
                table: "FoodStores");

            migrationBuilder.DropIndex(
                name: "IX_FoodStores_StoreId_FoodId_SizeId",
                table: "FoodStores");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "FoodStores");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "FoodStores");

            migrationBuilder.DropColumn(
                name: "HasSize",
                table: "Foods");

            migrationBuilder.CreateIndex(
                name: "IX_FoodStores_StoreId_FoodId",
                table: "FoodStores",
                columns: new[] { "StoreId", "FoodId" },
                unique: true);
        }
    }
}
