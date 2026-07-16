using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class renamegenetic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientDiseases_GeneticDisorders_GeneticDiseaseId",
                table: "PatientDiseases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeneticDisorders",
                table: "GeneticDisorders");

            migrationBuilder.RenameTable(
                name: "GeneticDisorders",
                newName: "GeneticDisease");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeneticDisease",
                table: "GeneticDisease",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientDiseases_GeneticDisease_GeneticDiseaseId",
                table: "PatientDiseases",
                column: "GeneticDiseaseId",
                principalTable: "GeneticDisease",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientDiseases_GeneticDisease_GeneticDiseaseId",
                table: "PatientDiseases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GeneticDisease",
                table: "GeneticDisease");

            migrationBuilder.RenameTable(
                name: "GeneticDisease",
                newName: "GeneticDisorders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GeneticDisorders",
                table: "GeneticDisorders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientDiseases_GeneticDisorders_GeneticDiseaseId",
                table: "PatientDiseases",
                column: "GeneticDiseaseId",
                principalTable: "GeneticDisorders",
                principalColumn: "Id");
        }
    }
}
