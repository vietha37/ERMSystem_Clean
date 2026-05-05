using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERMSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokensToAppUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiresAt",
                table: "AppUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenRevokedAt",
                table: "AppUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiresAt",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenRevokedAt",
                table: "AppUsers");
        }
    }
}
