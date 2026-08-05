using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YÖNETİM_PANELİ.Migrations
{
    /// <inheritdoc />
    public partial class ArizaGuncelKurumsalSutunlar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Baslik",
                table: "Arizalar",
                newName: "PersonelNo");

            migrationBuilder.RenameColumn(
                name: "Aciklama",
                table: "Arizalar",
                newName: "HataAciklaması");

            migrationBuilder.AddColumn<string>(
                name: "AciliyetDurumu",
                table: "Arizalar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdSoyad",
                table: "Arizalar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArizayiAlan",
                table: "Arizalar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CihazTuru",
                table: "Arizalar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Departman",
                table: "Arizalar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AciliyetDurumu",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "AdSoyad",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "ArizayiAlan",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "CihazTuru",
                table: "Arizalar");

            migrationBuilder.DropColumn(
                name: "Departman",
                table: "Arizalar");

            migrationBuilder.RenameColumn(
                name: "PersonelNo",
                table: "Arizalar",
                newName: "Baslik");

            migrationBuilder.RenameColumn(
                name: "HataAciklaması",
                table: "Arizalar",
                newName: "Aciklama");
        }
    }
}
