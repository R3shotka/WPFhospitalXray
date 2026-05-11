using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalImageLinksToAnalysisAndRetraining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MedicalImageId",
                table: "RetrainingRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicalImageId",
                table: "AnalysisResults",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetrainingRequests_MedicalImageId",
                table: "RetrainingRequests",
                column: "MedicalImageId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_MedicalImageId",
                table: "AnalysisResults",
                column: "MedicalImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalysisResults_MedicalImages_MedicalImageId",
                table: "AnalysisResults",
                column: "MedicalImageId",
                principalTable: "MedicalImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RetrainingRequests_MedicalImages_MedicalImageId",
                table: "RetrainingRequests",
                column: "MedicalImageId",
                principalTable: "MedicalImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalysisResults_MedicalImages_MedicalImageId",
                table: "AnalysisResults");

            migrationBuilder.DropForeignKey(
                name: "FK_RetrainingRequests_MedicalImages_MedicalImageId",
                table: "RetrainingRequests");

            migrationBuilder.DropIndex(
                name: "IX_RetrainingRequests_MedicalImageId",
                table: "RetrainingRequests");

            migrationBuilder.DropIndex(
                name: "IX_AnalysisResults_MedicalImageId",
                table: "AnalysisResults");

            migrationBuilder.DropColumn(
                name: "MedicalImageId",
                table: "RetrainingRequests");

            migrationBuilder.DropColumn(
                name: "MedicalImageId",
                table: "AnalysisResults");
        }
    }
}
