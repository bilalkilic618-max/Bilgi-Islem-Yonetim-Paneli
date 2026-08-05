using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YÖNETİM_PANELİ.Migrations
{
    /// <inheritdoc />
    public partial class BarkodSistemiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BarkodNo",
                table: "Envanterler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BarkodNo",
                table: "DepoHareketleri",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarkodNo",
                table: "Envanterler");

            migrationBuilder.DropColumn(
                name: "BarkodNo",
                table: "DepoHareketleri");
        }
    }
}
