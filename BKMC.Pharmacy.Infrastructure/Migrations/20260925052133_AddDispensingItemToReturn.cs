using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BKMC.Pharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDispensingItemToReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_Batches_BatchId",
                table: "ReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_Medicines_MedicineId",
                table: "ReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Dispensings_DispensingId",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Patients_PatientId",
                table: "Returns");

            migrationBuilder.AddColumn<int>(
                name: "DispensingItemId",
                table: "ReturnItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnItems_DispensingItemId",
                table: "ReturnItems",
                column: "DispensingItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_Batches_BatchId",
                table: "ReturnItems",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_DispensingItems_DispensingItemId",
                table: "ReturnItems",
                column: "DispensingItemId",
                principalTable: "DispensingItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_Medicines_MedicineId",
                table: "ReturnItems",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Dispensings_DispensingId",
                table: "Returns",
                column: "DispensingId",
                principalTable: "Dispensings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Patients_PatientId",
                table: "Returns",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_Batches_BatchId",
                table: "ReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_DispensingItems_DispensingItemId",
                table: "ReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_Medicines_MedicineId",
                table: "ReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Dispensings_DispensingId",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Patients_PatientId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_ReturnItems_DispensingItemId",
                table: "ReturnItems");

            migrationBuilder.DropColumn(
                name: "DispensingItemId",
                table: "ReturnItems");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_Batches_BatchId",
                table: "ReturnItems",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_Medicines_MedicineId",
                table: "ReturnItems",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Dispensings_DispensingId",
                table: "Returns",
                column: "DispensingId",
                principalTable: "Dispensings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Patients_PatientId",
                table: "Returns",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
