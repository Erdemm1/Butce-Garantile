using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Butce_Garantile.Migrations
{
    /// <inheritdoc />
    public partial class gelir2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GelirKaynağı",
                table: "Gelirler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDüzenli",
                table: "Gelirler",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GelirKaynağı",
                table: "Gelirler");

            migrationBuilder.DropColumn(
                name: "IsDüzenli",
                table: "Gelirler");
        }
    }
}
