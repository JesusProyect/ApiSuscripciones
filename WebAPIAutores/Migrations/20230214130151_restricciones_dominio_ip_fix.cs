using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPIAutores.Migrations
{
    public partial class restricciones_dominio_ip_fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestriccionesDominio_LlavesApi_LlaveAPIId",
                table: "RestriccionesDominio");

            migrationBuilder.DropForeignKey(
                name: "FK_RestriccionesIP_LlavesApi_LlaveAPIId",
                table: "RestriccionesIP");

            migrationBuilder.DropIndex(
                name: "IX_RestriccionesIP_LlaveAPIId",
                table: "RestriccionesIP");

            migrationBuilder.DropIndex(
                name: "IX_RestriccionesDominio_LlaveAPIId",
                table: "RestriccionesDominio");

            migrationBuilder.DropColumn(
                name: "LlaveAPIId",
                table: "RestriccionesIP");

            migrationBuilder.DropColumn(
                name: "LlaveAPIId",
                table: "RestriccionesDominio");

            migrationBuilder.CreateIndex(
                name: "IX_RestriccionesIP_LlaveId",
                table: "RestriccionesIP",
                column: "LlaveId");

            migrationBuilder.CreateIndex(
                name: "IX_RestriccionesDominio_LlaveId",
                table: "RestriccionesDominio",
                column: "LlaveId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestriccionesDominio_LlavesApi_LlaveId",
                table: "RestriccionesDominio",
                column: "LlaveId",
                principalTable: "LlavesApi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RestriccionesIP_LlavesApi_LlaveId",
                table: "RestriccionesIP",
                column: "LlaveId",
                principalTable: "LlavesApi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestriccionesDominio_LlavesApi_LlaveId",
                table: "RestriccionesDominio");

            migrationBuilder.DropForeignKey(
                name: "FK_RestriccionesIP_LlavesApi_LlaveId",
                table: "RestriccionesIP");

            migrationBuilder.DropIndex(
                name: "IX_RestriccionesIP_LlaveId",
                table: "RestriccionesIP");

            migrationBuilder.DropIndex(
                name: "IX_RestriccionesDominio_LlaveId",
                table: "RestriccionesDominio");

            migrationBuilder.AddColumn<int>(
                name: "LlaveAPIId",
                table: "RestriccionesIP",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LlaveAPIId",
                table: "RestriccionesDominio",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestriccionesIP_LlaveAPIId",
                table: "RestriccionesIP",
                column: "LlaveAPIId");

            migrationBuilder.CreateIndex(
                name: "IX_RestriccionesDominio_LlaveAPIId",
                table: "RestriccionesDominio",
                column: "LlaveAPIId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestriccionesDominio_LlavesApi_LlaveAPIId",
                table: "RestriccionesDominio",
                column: "LlaveAPIId",
                principalTable: "LlavesApi",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestriccionesIP_LlavesApi_LlaveAPIId",
                table: "RestriccionesIP",
                column: "LlaveAPIId",
                principalTable: "LlavesApi",
                principalColumn: "Id");
        }
    }
}
