using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YÖNETİM_PANELİ.Migrations
{
    /// <inheritdoc />
    public partial class EnvanterFotografEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotografYolu",
                table: "Envanterler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotografYolu",
                table: "Envanterler");
        }
    }
}
