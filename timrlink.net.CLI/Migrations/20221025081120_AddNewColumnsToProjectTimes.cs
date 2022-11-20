using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace timrlink.net.CLI.Migrations
{
    public partial class AddNewColumnsToProjectTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndTimeOffset",
                table: "ProjectTimes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedOffset",
                table: "ProjectTimes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartTimeOffset",
                table: "ProjectTimes",
                type: "datetimeoffset",
                nullable: true);

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
                name: "EndTimeOffset",
                table: "ProjectTimes");

            migrationBuilder.DropColumn(
                name: "LastModifiedOffset",
                table: "ProjectTimes");

            migrationBuilder.DropColumn(
                name: "StartTimeOffset",
                table: "ProjectTimes");

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
        }
    }
}
