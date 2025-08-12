using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomDeploy.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusMensagemExecutadoEmToDeployComando : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExecutadoEm",
                table: "DeployComandos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mensagem",
                table: "DeployComandos",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DeployComandos",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutadoEm",
                table: "DeployComandos");

            migrationBuilder.DropColumn(
                name: "Mensagem",
                table: "DeployComandos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DeployComandos");

            migrationBuilder.UpdateData(
                table: "AcessoNiveis",
                keyColumn: "Id",
                keyValue: 1,
                column: "CriadoEm",
                value: new DateTime(2025, 8, 7, 19, 10, 6, 398, DateTimeKind.Utc).AddTicks(994));

            migrationBuilder.UpdateData(
                table: "AcessoNiveis",
                keyColumn: "Id",
                keyValue: 2,
                column: "CriadoEm",
                value: new DateTime(2025, 8, 7, 19, 10, 6, 398, DateTimeKind.Utc).AddTicks(996));
        }
    }
}
