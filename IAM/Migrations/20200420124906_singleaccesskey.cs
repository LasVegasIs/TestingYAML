using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAM.Migrations
{
    public partial class singleaccesskey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionTokens_Token",
                table: "SessionTokens");

            migrationBuilder.CreateTable(
                name: "SingleAccessKeys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(nullable: false),
                    Issued = table.Column<DateTimeOffset>(nullable: false),
                    Used = table.Column<DateTimeOffset>(nullable: true),
                    Key = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleAccessKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionTokens_Token",
                table: "SessionTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SingleAccessKeys_AccountId",
                table: "SingleAccessKeys",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleAccessKeys_Key",
                table: "SingleAccessKeys",
                column: "Key",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SingleAccessKeys");

            migrationBuilder.DropIndex(
                name: "IX_SessionTokens_Token",
                table: "SessionTokens");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTokens_Token",
                table: "SessionTokens",
                column: "Token");
        }
    }
}
