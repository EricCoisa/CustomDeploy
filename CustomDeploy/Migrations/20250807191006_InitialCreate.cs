using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CustomDeploy.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcessoNiveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcessoNiveis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Senha = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deploys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ApplicationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Plataforma = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deploys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deploys_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioAcessos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcessoNivelId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioAcessos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioAcessos_AcessoNiveis_AcessoNivelId",
                        column: x => x.AcessoNivelId,
                        principalTable: "AcessoNiveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioAcessos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeployComandos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeployId = table.Column<int>(type: "INTEGER", nullable: false),
                    Comando = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeployComandos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeployComandos_Deploys_DeployId",
                        column: x => x.DeployId,
                        principalTable: "Deploys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeployHistoricos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeployId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeployHistoricos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeployHistoricos_Deploys_DeployId",
                        column: x => x.DeployId,
                        principalTable: "Deploys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AcessoNiveis",
                columns: new[] { "Id", "AtualizadoEm", "CriadoEm", "Nome" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 8, 7, 19, 10, 6, 398, DateTimeKind.Utc).AddTicks(994), "Administrador" },
                    { 2, null, new DateTime(2025, 8, 7, 19, 10, 6, 398, DateTimeKind.Utc).AddTicks(996), "Operador" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcessoNivel_Nome",
                table: "AcessoNiveis",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeployComando_DeployId",
                table: "DeployComandos",
                column: "DeployId");

            migrationBuilder.CreateIndex(
                name: "IX_DeployComando_DeployId_Ordem",
                table: "DeployComandos",
                columns: new[] { "DeployId", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeployHistorico_Data",
                table: "DeployHistoricos",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_DeployHistorico_DeployId",
                table: "DeployHistoricos",
                column: "DeployId");

            migrationBuilder.CreateIndex(
                name: "IX_DeployHistorico_Status",
                table: "DeployHistoricos",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Deploy_Data",
                table: "Deploys",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Deploy_SiteName",
                table: "Deploys",
                column: "SiteName");

            migrationBuilder.CreateIndex(
                name: "IX_Deploy_Status",
                table: "Deploys",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Deploy_UsuarioId",
                table: "Deploys",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioAcesso_AcessoNivelId",
                table: "UsuarioAcessos",
                column: "AcessoNivelId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioAcesso_UsuarioId",
                table: "UsuarioAcessos",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeployComandos");

            migrationBuilder.DropTable(
                name: "DeployHistoricos");

            migrationBuilder.DropTable(
                name: "UsuarioAcessos");

            migrationBuilder.DropTable(
                name: "Deploys");

            migrationBuilder.DropTable(
                name: "AcessoNiveis");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
