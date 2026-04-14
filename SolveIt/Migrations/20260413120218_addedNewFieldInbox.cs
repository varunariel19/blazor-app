using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolveIt.Migrations
{
    /// <inheritdoc />
    public partial class addedNewFieldInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastMsgUserId",
                table: "Inbox",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMsgUserId",
                table: "Inbox");
        }
    }
}
