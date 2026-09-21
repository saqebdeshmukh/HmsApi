using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class medrecidadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicalRecordId",
                table: "HealthReports",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthReports_MedicalRecordId",
                table: "HealthReports",
                column: "MedicalRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthReports_MedicalRecords_MedicalRecordId",
                table: "HealthReports",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthReports_MedicalRecords_MedicalRecordId",
                table: "HealthReports");

            migrationBuilder.DropIndex(
                name: "IX_HealthReports_MedicalRecordId",
                table: "HealthReports");

            migrationBuilder.DropColumn(
                name: "MedicalRecordId",
                table: "HealthReports");
        }
    }
}
