using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomDeploy.Migrations
{
    /// <inheritdoc />
    public partial class AddRepoUrlAndBranchToDeploy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Deploys",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildOutput",
                table: "Deploys",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepoUrl",
                table: "Deploys",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AcessoNiveis",
                keyColumn: "Id",
                keyValue: 1,
                column: "CriadoEm",
                value: new DateTime(2025, 8, 12, 4, 40, 21, 500, DateTimeKind.Utc).AddTicks(945));

            migrationBuilder.UpdateData(
                table: "AcessoNiveis",
                keyColumn: "Id",
                keyValue: 2,
                column: "CriadoEm",
                value: new DateTime(2025, 8, 12, 4, 40, 21, 500, DateTimeKind.Utc).AddTicks(947));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Deploys");

            migrationBuilder.DropColumn(
                name: "BuildOutput",
                table: "Deploys");

            migrationBuilder.DropColumn(
                name: "RepoUrl",
                table: "Deploys");

            migrationBuilder.UpdateData(
                table: "AcessoNiveis",
                keyColumn: "Id",
                keyValue: 1,
                column: "CriadoEm",
                value: new DateTime(2025, 8, 7, 21, 53, 27, 547, DateTimeKind.Utc).AddTicks(1724));

            migrationBuilder.UpdateData(
                table: "AcessoNiveis",
                keyColumn: "Id",
                keyValue: 2,
                column: "CriadoEm",
                value: new DateTime(2025, 8, 7, 21, 53, 27, 547, DateTimeKind.Utc).AddTicks(1726));
        }
    }
}
