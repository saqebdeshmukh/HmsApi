using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class appointidadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppointmentId",
                table: "HealthReports",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthReports_AppointmentId",
                table: "HealthReports",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthReports_Appointments_AppointmentId",
                table: "HealthReports",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthReports_Appointments_AppointmentId",
                table: "HealthReports");

            migrationBuilder.DropIndex(
                name: "IX_HealthReports_AppointmentId",
                table: "HealthReports");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "HealthReports");
        }
    }
}
