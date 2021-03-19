using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IAM.Migrations
{
    public partial class RemoveObsoleteTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeletedUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeletedUsers",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HardDeletionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoftDeletionTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedUsers", x => x.AccountId);
                });
        }
    }
}
