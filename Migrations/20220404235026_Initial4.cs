using Microsoft.EntityFrameworkCore.Migrations;

namespace Intex313.Migrations
{
    public partial class Initial4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Route",
                table: "Accidents",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Route",
                table: "Accidents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
