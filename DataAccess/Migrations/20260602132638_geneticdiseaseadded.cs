using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class geneticdiseaseadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diabetic",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Hypertension",
                table: "Patients");

            migrationBuilder.CreateTable(
                name: "GeneticDisorders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiseaseName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneticDisorders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientDiseases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GeneticDiseaseId = table.Column<int>(type: "int", nullable: true),
                    Medicines = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDiseases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientDiseases_GeneticDisorders_GeneticDiseaseId",
                        column: x => x.GeneticDiseaseId,
                        principalTable: "GeneticDisorders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PatientDiseases_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientDiseases_GeneticDiseaseId",
                table: "PatientDiseases",
                column: "GeneticDiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDiseases_PatientId",
                table: "PatientDiseases",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientDiseases");

            migrationBuilder.DropTable(
                name: "GeneticDisorders");

            migrationBuilder.AddColumn<string>(
                name: "Diabetic",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hypertension",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
