using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace timrlink.net.CLI.Migrations
{
    public partial class AddNewColumnsToProjectTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "StartTime",
                table: "ProjectTimes",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastModifiedTime",
                table: "ProjectTimes",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EndTime",
                table: "ProjectTimes",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "TaskExternalId",
                table: "ProjectTimes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaskUUID",
                table: "ProjectTimes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmployeeNr",
                table: "ProjectTimes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserExternalId",
                table: "ProjectTimes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserUUID",
                table: "ProjectTimes",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskExternalId",
                table: "ProjectTimes");

            migrationBuilder.DropColumn(
                name: "TaskUUID",
                table: "ProjectTimes");

            migrationBuilder.DropColumn(
                name: "UserEmployeeNr",
                table: "ProjectTimes");

            migrationBuilder.DropColumn(
                name: "UserExternalId",
                table: "ProjectTimes");

            migrationBuilder.DropColumn(
                name: "UserUUID",
                table: "ProjectTimes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "ProjectTimes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedTime",
                table: "ProjectTimes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "ProjectTimes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
        }
    }
}
