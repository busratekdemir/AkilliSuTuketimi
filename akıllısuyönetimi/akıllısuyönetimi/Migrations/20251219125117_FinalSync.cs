using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace akıllısuyönetimi.Migrations
{
    /// <inheritdoc />
    public partial class FinalSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "Meters",
                newName: "Meters",
                newSchema: "dbo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Meters",
                schema: "dbo",
                newName: "Meters");
        }
    }
}
