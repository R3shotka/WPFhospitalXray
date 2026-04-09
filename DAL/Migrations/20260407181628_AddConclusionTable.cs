using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddConclusionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Examinations_AspNetUsers_RadiologistId",
                table: "Examinations");

            migrationBuilder.DropForeignKey(
                name: "FK_Examinations_AspNetUsers_SurgeonId",
                table: "Examinations");

            migrationBuilder.DropIndex(
                name: "IX_Examinations_RadiologistId",
                table: "Examinations");

            migrationBuilder.DropIndex(
                name: "IX_Examinations_SurgeonId",
                table: "Examinations");

            migrationBuilder.DropColumn(
                name: "RadiologistConclusion",
                table: "Examinations");

            migrationBuilder.DropColumn(
                name: "RadiologistId",
                table: "Examinations");

            migrationBuilder.DropColumn(
                name: "SurgeonConclusion",
                table: "Examinations");

            migrationBuilder.DropColumn(
                name: "SurgeonId",
                table: "Examinations");

            migrationBuilder.CreateTable(
                name: "Conclusions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminationId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConclusionText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conclusions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conclusions_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conclusions_Examinations_ExaminationId",
                        column: x => x.ExaminationId,
                        principalTable: "Examinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conclusions_DoctorId",
                table: "Conclusions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conclusions_ExaminationId",
                table: "Conclusions",
                column: "ExaminationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conclusions");

            migrationBuilder.AddColumn<string>(
                name: "RadiologistConclusion",
                table: "Examinations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RadiologistId",
                table: "Examinations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurgeonConclusion",
                table: "Examinations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurgeonId",
                table: "Examinations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Examinations_RadiologistId",
                table: "Examinations",
                column: "RadiologistId");

            migrationBuilder.CreateIndex(
                name: "IX_Examinations_SurgeonId",
                table: "Examinations",
                column: "SurgeonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Examinations_AspNetUsers_RadiologistId",
                table: "Examinations",
                column: "RadiologistId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Examinations_AspNetUsers_SurgeonId",
                table: "Examinations",
                column: "SurgeonId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
