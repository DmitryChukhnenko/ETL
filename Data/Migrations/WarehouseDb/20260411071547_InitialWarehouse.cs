using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations.WarehouseDb
{
    /// <inheritdoc />
    public partial class InitialWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DimEmployees",
                columns: table => new
                {
                    EmployeeKey = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginalSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnifiedFullName = table.Column<string>(type: "text", nullable: false),
                    Resume = table.Column<string>(type: "text", nullable: false),
                    TotalIncome = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimEmployees", x => x.EmployeeKey);
                });

            migrationBuilder.CreateTable(
                name: "FactProjectExperiences",
                columns: table => new
                {
                    FactKey = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeKey = table.Column<int>(type: "integer", nullable: false),
                    ProjectName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    TechStack = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactProjectExperiences", x => x.FactKey);
                    table.ForeignKey(
                        name: "FK_FactProjectExperiences_DimEmployees_EmployeeKey",
                        column: x => x.EmployeeKey,
                        principalTable: "DimEmployees",
                        principalColumn: "EmployeeKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FactProjectExperiences_EmployeeKey",
                table: "FactProjectExperiences",
                column: "EmployeeKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactProjectExperiences");

            migrationBuilder.DropTable(
                name: "DimEmployees");
        }
    }
}
