using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Intex313.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accidents",
                columns: table => new
                {
                    Crash_ID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Crash_Date_Time = table.Column<DateTime>(nullable: false),
                    Route = table.Column<int>(nullable: false),
                    MilePoint = table.Column<decimal>(nullable: false),
                    Lat_Utm_Y = table.Column<int>(nullable: false),
                    Long_Utm_Y = table.Column<int>(nullable: false),
                    Main_Road_Name = table.Column<string>(maxLength: 50, nullable: true),
                    City = table.Column<string>(nullable: true),
                    County_Name = table.Column<string>(nullable: true),
                    Crash_Severity_ID = table.Column<int>(nullable: false),
                    Work_Zone_Related = table.Column<bool>(nullable: false),
                    Pedestrian_Involved = table.Column<bool>(nullable: false),
                    Bicyclist_Involved = table.Column<bool>(nullable: false),
                    Motorcycle_Involved = table.Column<bool>(nullable: false),
                    Improper_Restraint = table.Column<bool>(nullable: false),
                    Unrestrained = table.Column<bool>(nullable: false),
                    DUI = table.Column<bool>(nullable: false),
                    Intersection_Related = table.Column<bool>(nullable: false),
                    Wild_Animal_Related = table.Column<bool>(nullable: false),
                    Domestic_Animal_Related = table.Column<bool>(nullable: false),
                    Overturn_Rollover = table.Column<bool>(nullable: false),
                    Commercial_Motor_Veh_Involved = table.Column<bool>(nullable: false),
                    Teenage_Driver_Involved = table.Column<bool>(nullable: false),
                    Older_Driver_Involved = table.Column<bool>(nullable: false),
                    Night_Dark_Condition = table.Column<bool>(nullable: false),
                    Single_Vehicle = table.Column<bool>(nullable: false),
                    Distracted_Driving = table.Column<bool>(nullable: false),
                    Drowsy_Driving = table.Column<bool>(nullable: false),
                    Roadway_Departure = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accidents", x => x.Crash_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accidents");
        }
    }
}
