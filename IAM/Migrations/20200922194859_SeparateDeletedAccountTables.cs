using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAM.Migrations
{
    public partial class SeparateDeletedAccountTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeletedUserAccounts");

            migrationBuilder.CreateTable(
                name: "HardDeletedUserAccounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HardDeletedUserAccounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "SoftDeletedUserAccounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftDeletedUserAccounts", x => x.AccountId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HardDeletedUserAccounts");

            migrationBuilder.DropTable(
                name: "SoftDeletedUserAccounts");

            migrationBuilder.CreateTable(
                name: "DeletedUserAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    HardDeletionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoftDeletionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedUserAccounts", x => x.Id);
                });
        }
    }
}
