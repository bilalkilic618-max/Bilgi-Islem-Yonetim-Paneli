using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YÖNETİM_PANELİ.Migrations
{
    /// <inheritdoc />
    public partial class KullaniciYonetimiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "KayitTarihi",
                table: "Teknisyenler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnayTarihi",
                table: "Teknisyenler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnaylayanAdmin",
                table: "Teknisyenler",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Teknisyenler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Teknisyen");

            // Mevcut kayıtların rolü, Login'deki eski Unvan mantığıyla doldurulur:
            // Unvan'ında "Depo" geçen → "Depo Sorumlusu", diğerleri → "Teknisyen".
            migrationBuilder.Sql(
                "UPDATE Teknisyenler SET Rol = CASE WHEN Unvan LIKE N'%Depo%' THEN N'Depo Sorumlusu' ELSE N'Teknisyen' END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KayitTarihi",
                table: "Teknisyenler");

            migrationBuilder.DropColumn(
                name: "OnayTarihi",
                table: "Teknisyenler");

            migrationBuilder.DropColumn(
                name: "OnaylayanAdmin",
                table: "Teknisyenler");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Teknisyenler");
        }
    }
}
