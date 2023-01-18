using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeeMind.Services.Data.Migrations
{
    /// <inheritdoc />
    public partial class Version2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsFavorite",
                table: "Items",
                newName: "IsMasked");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsMasked",
                table: "Items",
                newName: "IsFavorite");
        }
    }
}
