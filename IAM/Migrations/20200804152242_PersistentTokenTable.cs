using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAM.Migrations
{
    public partial class PersistentTokenTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersistentTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(nullable: false),
                    Token = table.Column<string>(nullable: false),
                    Issued = table.Column<DateTimeOffset>(nullable: false),
                    Revoked = table.Column<DateTimeOffset>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersistentTokens", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersistentTokens");
        }
    }
}
