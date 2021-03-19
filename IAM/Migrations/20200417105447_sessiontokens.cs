using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAM.Migrations
{
    public partial class sessiontokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(nullable: false),
                    Credential = table.Column<int>(nullable: false),
                    Token = table.Column<string>(nullable: false),
                    UserAgent = table.Column<string>(nullable: false),
                    Ip = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: false),
                    Issued = table.Column<DateTimeOffset>(nullable: false),
                    LastRefreshed = table.Column<DateTimeOffset>(nullable: true),
                    Revoked = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionTokens_AccountId",
                table: "SessionTokens",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTokens_Token",
                table: "SessionTokens",
                column: "Token");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionTokens");
        }
    }
}
