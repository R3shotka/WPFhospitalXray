using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddModelVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OnnxPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PtPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrainingDatasetPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OldDatasetPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrainingRunPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExperimentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Precision = table.Column<double>(type: "float", nullable: true),
                    Recall = table.Column<double>(type: "float", nullable: true),
                    Map50 = table.Column<double>(type: "float", nullable: true),
                    Map5095 = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelVersions_Version",
                table: "ModelVersions",
                column: "Version",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelVersions");
        }
    }
}
