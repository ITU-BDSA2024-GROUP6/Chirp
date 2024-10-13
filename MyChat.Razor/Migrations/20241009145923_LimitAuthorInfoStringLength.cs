using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyChat.Razor.Migrations
{
    /// <inheritdoc />
    public partial class LimitAuthorInfoStringLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Cheeps");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Cheeps",
                newName: "TimeStamp");

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Cheeps",
                type: "TEXT",
                maxLength: 160,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Text",
                table: "Cheeps");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "Cheeps",
                newName: "Timestamp");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Cheeps",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
