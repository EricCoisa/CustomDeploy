using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomDeploy.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminalIdToDeployComando : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TerminalId",
                table: "DeployComandos",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AcessoNiveis",
                keyColumn: "Id",
                keyValue: 1,
                column: "CriadoEm",
                value: new DateTime(2025, 8, 12, 6, 45, 4, 79, DateTimeKind.Utc).AddTicks(5022));

            migrationBuilder.UpdateData(
                table: "AcessoNiveis",
                keyColumn: "Id",
                keyValue: 2,
                column: "CriadoEm",
                value: new DateTime(2025, 8, 12, 6, 45, 4, 79, DateTimeKind.Utc).AddTicks(5026));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TerminalId",
                table: "DeployComandos");

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
    }
}
