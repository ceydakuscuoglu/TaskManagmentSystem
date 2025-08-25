using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Approved_At",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Completed_At",
                table: "Tasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved_At",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Completed_At",
                table: "Tasks");
        }
    }
}
