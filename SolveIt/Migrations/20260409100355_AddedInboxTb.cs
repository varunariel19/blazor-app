using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolveIt.Migrations
{
    /// <inheritdoc />
    public partial class AddedInboxTb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inbox",
                columns: table => new
                {
                    InboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    User1Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    User2Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecentText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SendedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbox", x => x.InboxId);
                    table.ForeignKey(
                        name: "FK_Inbox_AspNetUsers_User1Id",
                        column: x => x.User1Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inbox_AspNetUsers_User2Id",
                        column: x => x.User2Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Converstions",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SendedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Seen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Converstions", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_Converstions_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Converstions_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Converstions_Inbox_InboxId",
                        column: x => x.InboxId,
                        principalTable: "Inbox",
                        principalColumn: "InboxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Converstions_InboxId_SendedAt",
                table: "Converstions",
                columns: new[] { "InboxId", "SendedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Converstions_ReceiverId",
                table: "Converstions",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Converstions_SenderId",
                table: "Converstions",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_User1Id",
                table: "Inbox",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_User1Id_User2Id_SendedAt",
                table: "Inbox",
                columns: new[] { "User1Id", "User2Id", "SendedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_User2Id",
                table: "Inbox",
                column: "User2Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Converstions");

            migrationBuilder.DropTable(
                name: "Inbox");
        }
    }
}
