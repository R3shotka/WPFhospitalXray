using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class SplitDoctorIntoRadiologistAndSurgeon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Examinations_AspNetUsers_DoctorId",
                table: "Examinations");

            migrationBuilder.DropIndex(
                name: "IX_Examinations_DoctorId",
                table: "Examinations");

            migrationBuilder.DropColumn(
                name: "DoctorConclusion",
                table: "Examinations");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Examinations");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "DoctorConclusion",
                table: "Examinations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DoctorId",
                table: "Examinations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Examinations_DoctorId",
                table: "Examinations",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Examinations_AspNetUsers_DoctorId",
                table: "Examinations",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
