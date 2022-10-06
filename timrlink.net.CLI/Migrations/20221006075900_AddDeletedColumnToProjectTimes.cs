using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace timrlink.net.CLI.Migrations
{
    public partial class AddDeletedColumnToProjectTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deleted",
                table: "ProjectTimes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "ProjectTimes");
        }
    }
}
