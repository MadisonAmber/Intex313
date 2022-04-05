using Microsoft.EntityFrameworkCore.Migrations;

namespace Intex313.Migrations
{
    public partial class Initial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Long_Utm_Y",
                table: "Accidents",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Lat_Utm_Y",
                table: "Accidents",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Long_Utm_Y",
                table: "Accidents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<int>(
                name: "Lat_Utm_Y",
                table: "Accidents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal));
        }
    }
}
