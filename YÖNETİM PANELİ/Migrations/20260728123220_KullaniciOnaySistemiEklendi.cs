using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YÖNETİM_PANELİ.Migrations
{
    /// <inheritdoc />
    public partial class KullaniciOnaySistemiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailOnayKodu",
                table: "Teknisyenler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailOnayliMi",
                table: "Teknisyenler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "YoneticiOnayliMi",
                table: "Teknisyenler",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailOnayKodu",
                table: "Teknisyenler");

            migrationBuilder.DropColumn(
                name: "EmailOnayliMi",
                table: "Teknisyenler");

            migrationBuilder.DropColumn(
                name: "YoneticiOnayliMi",
                table: "Teknisyenler");
        }
    }
}
