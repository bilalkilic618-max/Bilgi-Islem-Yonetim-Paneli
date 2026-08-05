using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YÖNETİM_PANELİ.Migrations
{
    /// <inheritdoc />
    public partial class UyelikAlanlariEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "Teknisyenler");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Teknisyenler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResetsToken",
                table: "Teknisyenler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sifre",
                table: "Teknisyenler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Teknisyenler");

            migrationBuilder.DropColumn(
                name: "ResetsToken",
                table: "Teknisyenler");

            migrationBuilder.DropColumn(
                name: "Sifre",
                table: "Teknisyenler");

            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "Teknisyenler",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
