using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERMSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientEmergencyContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelationship",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationship",
                table: "Patients");

        }
    }
}
