using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodingCell.YARPad.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationVersionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastModifiedByInstanceID",
                table: "YARPadConfigurations",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "YARPadConfigurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModifiedByInstanceID",
                table: "YARPadConfigurations");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "YARPadConfigurations");
        }
    }
}
